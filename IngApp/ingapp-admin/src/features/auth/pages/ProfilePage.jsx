// src/features/auth/pages/ProfilePage.jsx
import React, { useEffect, useState } from "react";
import { Card, Descriptions, Form, Input, Button, Space, message, Modal, Tag, Spin, Alert, Tabs, Table, Image, App, Row, Col, Typography, Select } from "antd";
import { useNavigate } from "react-router-dom";
import { EditOutlined, DownloadOutlined, FilePdfOutlined, FileWordOutlined, FileOutlined } from "@ant-design/icons";
import { getMeApi, updateMyProfileApi, setPasswordApi } from "../api/authApi";
import { useAuth } from "../../../core/auth/useAuth";
import supplierOnboardingApi from "../../suppliers/api/supplierOnboardingApi";
import buyerProfilesApi from "../../buyerProfiles/api/buyerProfilesApi";
import visitorProfilesApi from "../../visitorProfiles/api/visitorProfilesApi";
import apiClient from "../../../core/api/apiClient";
import jalaali from "jalaali-js";
import { getProvinces, getCitiesByProvince } from "../../../core/location/iranProvinces";

const { Text } = Typography;

// تبدیل تاریخ میلادی به شمسی
const toShamsi = (gregorian) => {
    if (!gregorian) return "-";
    
    try {
        let year, month, day;
        
        if (typeof gregorian === "string") {
            const parts = gregorian.split("T")[0].split("-");
            if (parts.length !== 3) return "-";
            year = parseInt(parts[0], 10);
            month = parseInt(parts[1], 10);
            day = parseInt(parts[2], 10);
            
            if (isNaN(year) || isNaN(month) || isNaN(day)) return "-";
            if (year < 1900 || year > 2100) return "-";
        } else if (gregorian instanceof Date) {
            if (isNaN(gregorian.getTime())) return "-";
            year = gregorian.getFullYear();
            month = gregorian.getMonth() + 1;
            day = gregorian.getDate();
        } else {
            return "-";
        }
        
        const j = jalaali.toJalaali(year, month, day);
        if (!j || !j.jy || !j.jm || !j.jd) return "-";
        
        return `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
    } catch (error) {
        console.error("Error converting date to Shamsi:", error);
        return "-";
    }
};

// Helper functions for BusinessType enum
const getBusinessTypeLabel = (value) => {
    if (value === 1 || value === "Natural" || value === "1") return "حقیقی";
    if (value === 2 || value === "Legal" || value === "2") return "حقوقی";
    return value || "-";
};

// Helper functions for ContactPosition enum
const getContactPositionLabel = (value) => {
    if (value === 1 || value === "PurchaseManager" || value === "1") return "مسئول خرید";
    if (value === 2 || value === "CEO" || value === "2") return "مدیر عامل";
    return value || "-";
};

const getUserTypeLabel = (userType) => {
    switch (userType) {
        case "Buyer": return "خریدار";
        case "Supplier": return "تأمین‌کننده";
        case "Admin": return "مدیر سیستم";
        case "Visitor": return "بازاریاب";
        default: return userType;
    }
};

const getSubscriptionLevelLabel = (level) => {
    switch (level) {
        case 0: return "بدون اشتراک";
        case 1: return "برنزی";
        case 2: return "نقره‌ای";
        case 3: return "طلایی";
        default: return "نامشخص";
    }
};

const getVerificationStatusLabel = (status) => {
    switch (status) {
        case 0: return "ارسال نشده";
        case 1: return "در انتظار بررسی";
        case 2: return "تأیید شده";
        case 3: return "رد شده";
        default: return "نامشخص";
    }
};

const getVerificationStatusColor = (status) => {
    switch (status) {
        case 0: return "default";
        case 1: return "processing";
        case 2: return "success";
        case 3: return "error";
        default: return "default";
    }
};

// تبدیل DocumentStatus enum به label و color
// DocumentStatus: Pending = 0, Approved = 1, Rejected = 2
const getDocumentStatusInfo = (status) => {
    let statusNum = status;
    if (typeof status === "string") {
        switch (status) {
            case "Pending": statusNum = 0; break;
            case "Approved": statusNum = 1; break;
            case "Rejected": statusNum = 2; break;
            default: statusNum = 0;
        }
    }
    
    // DocumentStatus: 0=Pending, 1=Approved, 2=Rejected
    const labels = {
        0: "در انتظار بررسی",
        1: "تأیید شده",
        2: "رد شده"
    };
    
    const colors = {
        0: "processing",
        1: "success",
        2: "error"
    };
    
    return {
        label: labels[statusNum] || "نامشخص",
        color: colors[statusNum] || "default"
    };
};

const ProfilePage = () => {
    const navigate = useNavigate();
    const { logout } = useAuth();
    const { modal } = App.useApp();
    const [form] = Form.useForm();
    
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [userInfo, setUserInfo] = useState(null);
    const [supplierProfile, setSupplierProfile] = useState(null);
    const [buyerProfile, setBuyerProfile] = useState(null);
    const [visitorProfile, setVisitorProfile] = useState(null);
    const [kycDocuments, setKycDocuments] = useState([]);
    const [editing, setEditing] = useState(false);
    const [activeTab, setActiveTab] = useState("user");
    const [fileBlobUrls, setFileBlobUrls] = useState({}); // { documentId: blobUrl }
    const [isPasswordModalOpen, setIsPasswordModalOpen] = useState(false);
    const [passwordForm] = Form.useForm();
    const [changingPassword, setChangingPassword] = useState(false);
    
    // Buyer Profile form state
    const [buyerForm] = Form.useForm();
    const [editingBuyer, setEditingBuyer] = useState(false);
    const [savingBuyer, setSavingBuyer] = useState(false);
    const [selectedProvince, setSelectedProvince] = useState(null);
    const [validatingCode, setValidatingCode] = useState(false);
    const provinces = React.useMemo(() => getProvinces(), []);
    const cities = React.useMemo(
        () => getCitiesByProvince(selectedProvince),
        [selectedProvince]
    );

    useEffect(() => {
        loadProfile();
    }, []);

    // وقتی editing فعال می‌شود، form values را از userInfo set کن
    useEffect(() => {
        if (editing && userInfo) {
            form.setFieldsValue({
                displayName: userInfo.displayName || "",
                phoneNumber: userInfo.phoneNumber || "",
            });
        }
    }, [editing, userInfo, form]);

    // Cleanup: Revoke blob URLs when component unmounts
    useEffect(() => {
        return () => {
            Object.values(fileBlobUrls).forEach(url => {
                if (url && typeof url === 'string') {
                    window.URL.revokeObjectURL(url);
                }
            });
        };
    }, [fileBlobUrls]);

    const loadProfile = async () => {
        try {
            setLoading(true);
            const res = await getMeApi();
            setUserInfo(res.data);

            form.setFieldsValue({
                displayName: res.data.displayName || "",
                phoneNumber: res.data.phoneNumber || "",
            });

            // بارگذاری اطلاعات تأمین‌کننده (اگر وجود دارد)
            try {
                const supplierRes = await supplierOnboardingApi.getMyProfile();
                setSupplierProfile(supplierRes);

                // بارگذاری مدارک KYC
                try {
                    const kycRes = await apiClient.get("/kyc/my/documents");
                    const documents = kycRes.data || [];
                    setKycDocuments(documents);

                    // ساخت blob URLs برای فایل‌های تصویری
                    const token = localStorage.getItem("token");
                    const blobUrlPromises = documents
                        .filter(d => d.filePath && d.value)
                        .map(async (d) => {
                            const fileName = (d.value || "").toLowerCase();
                            const isImage = /\.(jpg|jpeg|png|gif|webp|bmp)$/i.test(fileName);
                            if (!isImage) return null;

                            try {
                                const fileUrl = `${apiClient.defaults.baseURL}/kyc/documents/${d.id}/file`;
                                const res = await fetch(fileUrl, {
                                    method: "GET",
                                    headers: {
                                        ...(token ? { Authorization: `Bearer ${token}` } : {}),
                                    },
                                });
                                if (!res.ok) return null;
                                const blob = await res.blob();
                                const blobUrl = window.URL.createObjectURL(blob);
                                return { documentId: d.id, blobUrl };
                            } catch {
                                return null;
                            }
                        });

                    const blobUrlResults = await Promise.all(blobUrlPromises);
                    const newBlobUrls = {};
                    blobUrlResults.forEach(result => {
                        if (result) {
                            newBlobUrls[result.documentId] = result.blobUrl;
                        }
                    });
                    setFileBlobUrls(prev => {
                        // Revoke old URLs
                        Object.values(prev).forEach(url => {
                            if (url && typeof url === 'string') {
                                window.URL.revokeObjectURL(url);
                            }
                        });
                        return newBlobUrls;
                    });
                } catch {
                    setKycDocuments([]);
                }
            } catch {
                setSupplierProfile(null);
                setKycDocuments([]);
            }

            // بارگذاری اطلاعات Buyer (اگر وجود دارد)
            try {
                const buyerRes = await buyerProfilesApi.getMyProfile();
                console.log("[ProfilePage] Buyer profile response:", buyerRes);
                
                // API ممکن است null برگرداند اگر پروفایل وجود نداشته باشد
                // یا یک object برگرداند اگر پروفایل وجود داشته باشد
                if (buyerRes && typeof buyerRes === 'object' && (buyerRes.id || buyerRes.userId)) {
                    // پروفایل وجود دارد
                    console.log("[ProfilePage] Buyer profile found, setting state");
                    setBuyerProfile(buyerRes);
                    setSelectedProvince(buyerRes.province || null);
                    buyerForm.setFieldsValue({
                        businessName: buyerRes.businessName,
                        contactMobile: buyerRes.contactMobile,
                        province: buyerRes.province,
                        city: buyerRes.city,
                        address: buyerRes.address,
                        description: buyerRes.description,
                        referrerVisitorCode: buyerRes.referredByVisitorCode || null,
                    });
                } else {
                    // پروفایل وجود ندارد
                    console.log("[ProfilePage] Buyer profile not found (null or invalid response)");
                    setBuyerProfile(null);
                }
            } catch (error) {
                // اگر خطا رخ داد، لاگ می‌کنیم و پروفایل را null تنظیم می‌کنیم
                console.error("[ProfilePage] Error loading buyer profile:", error);
                console.error("[ProfilePage] Error details:", {
                    status: error?.response?.status,
                    data: error?.response?.data,
                    message: error?.message
                });
                
                // اگر خطای 500 باشد، پیام خطا را نمایش می‌دهیم
                if (error?.response?.status === 500) {
                    const errorMessage = error?.response?.data?.message || error?.message || "خطا در بارگذاری پروفایل خریدار";
                    message.error(errorMessage);
                }
                
                setBuyerProfile(null);
            }

            // بارگذاری اطلاعات Visitor (اگر وجود دارد)
            try {
                const visitorRes = await visitorProfilesApi.getMyProfile();
                setVisitorProfile(visitorRes);
            } catch {
                setVisitorProfile(null);
            }
        } catch (error) {
            message.error("خطا در بارگذاری اطلاعات پروفایل");
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    // Buyer Profile handlers
    const handleBuyerSave = async () => {
        try {
            const values = await buyerForm.validateFields();
            
            // بررسی کد معرف قبل از ارسال (اگر وارد شده باشد)
            if (values.referrerVisitorCode && values.referrerVisitorCode.trim()) {
                try {
                    const visitorProfile = await visitorProfilesApi.getByReferralCode(values.referrerVisitorCode.trim().toUpperCase());
                    
                    // بررسی اینکه آیا کد معرف متعلق به خود کاربر است
                    if (userInfo && visitorProfile && visitorProfile.userId === userInfo.id) {
                        message.error("شما نمی‌توانید خودتان را به عنوان بازاریاب انتخاب کنید. لطفاً کد معرف را پاک کنید یا کد معرف دیگری وارد کنید.");
                        buyerForm.setFieldsValue({ referrerVisitorCode: "" });
                        return;
                    }
                    
                    // بررسی اینکه آیا بازاریاب فعال است
                    if (visitorProfile && !visitorProfile.isActive) {
                        message.error("این کد معرف غیرفعال است. لطفاً کد معرف دیگری وارد کنید.");
                        buyerForm.setFieldsValue({ referrerVisitorCode: "" });
                        return;
                    }
                } catch (error) {
                    if (error?.response?.status === 404) {
                        message.error("کد معرف یافت نشد. لطفاً کد معرف را بررسی کنید.");
                        buyerForm.setFieldsValue({ referrerVisitorCode: "" });
                        return;
                    }
                    // اگر خطای دیگری بود، ادامه می‌دهیم و backend آن را handle می‌کند
                }
            }
            
            setSavingBuyer(true);
            const result = await buyerProfilesApi.upsertMyProfile(values);
            setBuyerProfile(result);
            setEditingBuyer(false);
            message.success("پروفایل خریدار با موفقیت ذخیره شد");
            
            // بارگذاری مجدد اطلاعات برای به‌روزرسانی فرم
            await loadProfile();
        } catch (error) {
            console.error("Error in handleBuyerSave:", error);
            
            // استخراج پیام خطا از response
            let errorMsg = "خطا در ذخیره پروفایل";
            
            if (error?.response?.data) {
                // اگر ApiResult باشد
                if (error.response.data.message) {
                    errorMsg = error.response.data.message;
                } else if (error.response.data.error) {
                    errorMsg = error.response.data.error;
                } else if (typeof error.response.data === 'string') {
                    errorMsg = error.response.data;
                }
            } else if (error?.message) {
                errorMsg = error.message;
            }
            
            message.error(errorMsg);
        } finally {
            setSavingBuyer(false);
        }
    };

    const handleValidateReferralCode = async () => {
        const code = buyerForm.getFieldValue("referrerVisitorCode");
        if (!code || code.trim() === "") {
            message.warning("لطفاً کد معرف را وارد کنید");
            return;
        }

        try {
            setValidatingCode(true);
            const visitorProfile = await visitorProfilesApi.getByReferralCode(code.trim().toUpperCase());
            
            if (visitorProfile) {
                // بررسی اینکه آیا کد معرف متعلق به خود کاربر است
                if (userInfo && visitorProfile.userId === userInfo.id) {
                    message.error("شما نمی‌توانید خودتان را به عنوان بازاریاب انتخاب کنید. لطفاً کد معرف دیگری وارد کنید.");
                    buyerForm.setFieldsValue({ referrerVisitorCode: "" });
                    return;
                }
                
                // بررسی اینکه آیا بازاریاب فعال است
                if (!visitorProfile.isActive) {
                    message.warning("این کد معرف غیرفعال است. لطفاً کد معرف دیگری وارد کنید.");
                    buyerForm.setFieldsValue({ referrerVisitorCode: "" });
                    return;
                }
                
                message.success(`کد معرف معتبر است. معرف: ${visitorProfile.businessName || visitorProfile.userDisplayName || "نامشخص"}`);
            } else {
                message.warning("کد معرف یافت نشد. لطفاً کد معرف را بررسی کنید.");
            }
        } catch (error) {
            if (error?.response?.status === 404) {
                message.warning("کد معرف یافت نشد. لطفاً کد معرف را بررسی کنید.");
            } else {
                const errorMsg = error?.response?.data?.message || error?.message || "خطا در بررسی کد معرف";
                message.error(errorMsg);
            }
        } finally {
            setValidatingCode(false);
        }
    };

    const handleSave = async () => {
        try {
            const values = await form.validateFields();
            const originalPhoneNumber = userInfo?.phoneNumber;

            // اگر شماره موبایل تغییر کرده، هشدار بده
            if (values.phoneNumber !== originalPhoneNumber) {
                modal.confirm({
                    title: "تغییر شماره موبایل",
                    content: "با تغییر شماره موبایل، شما از سیستم خارج می‌شوید و باید با شماره موبایل جدید دوباره وارد شوید. آیا مطمئن هستید؟",
                    okText: "بله، تغییر می‌دهم",
                    cancelText: "انصراف",
                    onOk: async () => {
                        try {
                            await performUpdate(values);
                        } catch (error) {
                            // Error handling در performUpdate انجام می‌شود
                            // اینجا فقط throw نمی‌کنیم تا modal بسته نشود
                        }
                    },
                });
            } else {
                await performUpdate(values);
            }
        } catch (error) {
            if (error.errorFields) {
                return;
            }
            message.error(error?.response?.data?.message || "خطا در به‌روزرسانی پروفایل");
        }
    };

    const performUpdate = async (values) => {
        try {
            setSaving(true);
            const originalPhoneNumber = userInfo?.phoneNumber;
            const phoneChanged = values.phoneNumber !== originalPhoneNumber;

            await updateMyProfileApi({
                displayName: values.displayName?.trim() || "",
                phoneNumber: values.phoneNumber?.trim() || "",
            });

            message.success("پروفایل با موفقیت به‌روزرسانی شد");

            if (phoneChanged) {
                message.warning("شما از سیستم خارج می‌شوید. لطفاً با شماره موبایل جدید وارد شوید.");
                setTimeout(() => {
                    logout();
                    navigate("/login");
                }, 2000);
            } else {
                setEditing(false);
                await loadProfile();
            }
        } catch (error) {
            // استخراج پیام خطا از response
            let errorMessage = "خطا در به‌روزرسانی پروفایل";
            
            if (error?.response?.data) {
                // اگر ApiResult باشد
                if (error.response.data.message) {
                    errorMessage = error.response.data.message;
                } else if (error.response.data.Error) {
                    errorMessage = error.response.data.Error;
                } else if (typeof error.response.data === 'string') {
                    errorMessage = error.response.data;
                }
            } else if (error?.message) {
                errorMessage = error.message;
            }
            
            message.error(errorMessage);
            throw error; // برای اینکه در Modal.confirm catch شود
        } finally {
            setSaving(false);
        }
    };

    const handleChangePassword = async (values) => {
        try {
            setChangingPassword(true);
            await setPasswordApi({
                currentPassword: values.currentPassword || "",
                newPassword: values.newPassword,
                confirmPassword: values.confirmPassword,
            });
            message.success("رمز عبور با موفقیت تغییر کرد");
            setIsPasswordModalOpen(false);
            passwordForm.resetFields();
        } catch (error) {
            const errorMsg =
                error?.response?.data?.message || error?.message || "خطا در تغییر رمز عبور";
            message.error(errorMsg);
        } finally {
            setChangingPassword(false);
        }
    };

    if (loading) {
        return (
            <div style={{ textAlign: "center", padding: 48 }}>
                <Spin size="large" />
            </div>
        );
    }

    if (!userInfo) {
        return <Alert type="error" message="خطا در بارگذاری اطلاعات کاربر" />;
    }

    const userProfileTab = (
        <>
            {!editing ? (
                <>
                    <Descriptions column={1} bordered>
                        <Descriptions.Item label="نام">
                            {userInfo.displayName || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="شماره موبایل">
                            {userInfo.phoneNumber}
                        </Descriptions.Item>
                        <Descriptions.Item label="تاریخ عضویت">
                            {toShamsi(userInfo.createdAt)}
                        </Descriptions.Item>
                        <Descriptions.Item label="نوع کاربر">
                            <Tag>{getUserTypeLabel(userInfo.userType)}</Tag>
                        </Descriptions.Item>
                        <Descriptions.Item label="وضعیت تأیید">
                            <Tag color={getVerificationStatusColor(userInfo.verificationStatus)}>
                                {getVerificationStatusLabel(userInfo.verificationStatus)}
                            </Tag>
                        </Descriptions.Item>
                        <Descriptions.Item label="سطح اشتراک">
                            {getSubscriptionLevelLabel(userInfo.subscriptionLevel)}
                        </Descriptions.Item>
                    </Descriptions>
                    <Alert
                        type="info"
                        message="برای ویرایش نام و شماره موبایل روی دکمه رو به رو کلیک کنید."
                        action={
                            <Button
                                type="primary"
                                icon={<EditOutlined />}
                                onClick={() => setEditing(true)}
                            >
                                ویرایش نام و شماره موبایل
                            </Button>
                        }
                        style={{ marginTop: 24 }}
                    />
                    <Alert
                        message="برای تغییر رمز عبور روی دکمه رو به رو کلیک کنید."
                        action={
                            <Button
                                type="default"
                                onClick={() => setIsPasswordModalOpen(true)}
                            >
                                تغییر رمز عبور
                            </Button>
                        }
                        style={{ 
                            marginTop: 16,
                            backgroundColor: '#ffffff',
                            border: '1px solid #d9d9d9'
                        }}
                    />
                </>
            ) : (
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSave}
                >
                    <Form.Item
                        label="نام"
                        name="displayName"
                        rules={[
                            { max: 100, message: "نام نمی‌تواند بیشتر از 100 کاراکتر باشد" }
                        ]}
                    >
                        <Input placeholder="نام را وارد کنید" />
                    </Form.Item>

                    <Form.Item
                        label="شماره موبایل"
                        name="phoneNumber"
                        rules={[
                            { required: true, message: "شماره موبایل الزامی است" },
                            { pattern: /^09\d{9}$/, message: "شماره موبایل باید 11 رقم و با 09 شروع شود" }
                        ]}
                    >
                        <Input placeholder="09xxxxxxxxx" />
                    </Form.Item>

                    <Space>
                        <Button type="primary" htmlType="submit" loading={saving}>
                            ذخیره
                        </Button>
                        <Button onClick={() => {
                            setEditing(false);
                            // reset fields به مقادیر اصلی userInfo
                            if (userInfo) {
                                form.setFieldsValue({
                                    displayName: userInfo.displayName || "",
                                    phoneNumber: userInfo.phoneNumber || "",
                                });
                            }
                        }}>
                            انصراف
                        </Button>
                    </Space>
                </Form>
            )}
        </>
    );

    const supplierProfileTab = (
        <>
            {!supplierProfile ? (
                <Alert type="info" message="شما هنوز درخواست همکاری به عنوان تأمین‌کننده ارسال نکرده‌اید." />
            ) : (
                <>
                    <Descriptions 
                        bordered 
                        size="small"
                        column={2}
                        style={{ marginBottom: 24 }}
                    >
                        <Descriptions.Item label="وضعیت" span={1}>
                            <Tag color={getVerificationStatusColor(
                                supplierProfile.verificationStatus === "Approved" ? 2 :
                                supplierProfile.verificationStatus === "Pending" ? 1 :
                                supplierProfile.verificationStatus === "Rejected" ? 3 : 0
                            )}>
                                {supplierProfile.verificationStatus === "Approved" ? "تأیید شده" :
                                 supplierProfile.verificationStatus === "Pending" ? "در انتظار بررسی" :
                                 supplierProfile.verificationStatus === "Rejected" ? "رد شده" : "ارسال نشده"}
                            </Tag>
                        </Descriptions.Item>
                        <Descriptions.Item label="شماره موبایل (Login)" span={1}>
                            {supplierProfile.userPhoneNumber || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="تاریخ ایجاد" span={1}>
                            {supplierProfile.createdAt ? toShamsi(supplierProfile.createdAt) : "-"}
                        </Descriptions.Item>
                        {supplierProfile.updatedAt ? (
                            <Descriptions.Item label="آخرین به‌روزرسانی" span={1}>
                                {toShamsi(supplierProfile.updatedAt)}
                            </Descriptions.Item>
                        ) : (
                            <Descriptions.Item label="آخرین به‌روزرسانی" span={1}>
                                "-"
                            </Descriptions.Item>
                        )}
                        {supplierProfile.rejectionReason && (
                            <Descriptions.Item label="دلیل رد" span={2}>
                                {supplierProfile.rejectionReason}
                            </Descriptions.Item>
                        )}
                        <Descriptions.Item label="نوع تأمین‌کننده" span={2}>
                            {supplierProfile.supplierTypeName || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="نوع کسب‌وکار" span={1}>
                            {getBusinessTypeLabel(supplierProfile.businessType)}
                        </Descriptions.Item>
                        <Descriptions.Item label="نام کسب‌وکار" span={1}>
                            {supplierProfile.businessName || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="کد ملی / شماره ملی" span={1}>
                            {supplierProfile.nationalId || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="شماره ثبت" span={1}>
                            {supplierProfile.licenseNumber || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="استان" span={1}>
                            {supplierProfile.province || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="شهر" span={1}>
                            {supplierProfile.city || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="آدرس" span={2}>
                            {supplierProfile.address || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="نام رابط" span={1}>
                            {supplierProfile.contactName || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="سمت رابط" span={1}>
                            {getContactPositionLabel(supplierProfile.contactPosition)}
                        </Descriptions.Item>
                        <Descriptions.Item label="شماره موبایل رابط" span={1}>
                            {supplierProfile.contactMobile || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="شماره تماس کسب‌وکار" span={1}>
                            {supplierProfile.contactPhone || "-"}
                        </Descriptions.Item>
                    </Descriptions>

                    {kycDocuments && kycDocuments.length > 0 && (
                        <div style={{ marginTop: 24 }}>
                            <Table
                                dataSource={kycDocuments}
                                rowKey="id"
                                pagination={false}
                                scroll={{ x: 'max-content' }}
                                columns={[
                                    {
                                        title: "ردیف",
                                        key: "index",
                                        width: 60,
                                        render: (_, __, index) => index + 1,
                                    },
                                    {
                                        title: "نوع مدرک",
                                        dataIndex: "attributeDisplayName",
                                        key: "attributeDisplayName",
                                    },
                                    {
                                        title: "مقدار/فایل",
                                        key: "value",
                                        render: (_, doc) => {
                                            // اگر filePath دارد، یعنی فایل است
                                            if (doc.filePath) {
                                                const fileName = doc.value || doc.filePath || "فایل پیوست";
                                                const isImage = /\.(jpg|jpeg|png|gif|webp|bmp)$/i.test(fileName);
                                                const isPdf = /\.pdf$/i.test(fileName);
                                                const isWord = /\.(doc|docx)$/i.test(fileName);
                                                
                                                const fileUrl = `${apiClient.defaults.baseURL}/kyc/documents/${doc.id}/file`;
                                                const blobUrl = fileBlobUrls[doc.id];
                                                
                                                return (
                                                    <Space direction="vertical" size="small" style={{ width: "100%" }}>
                                                        <div style={{
                                                            display: "flex",
                                                            alignItems: "center",
                                                            gap: 8,
                                                            background: "#fafafa",
                                                            padding: "8px",
                                                            borderRadius: "4px",
                                                            border: "1px solid #e8e8e8"
                                                        }}>
                                                            {isImage ? (
                                                                blobUrl ? (
                                                                    <Image
                                                                        src={blobUrl}
                                                                        alt={fileName}
                                                                        style={{ maxWidth: 40, maxHeight: 40, objectFit: "cover", borderRadius: "4px" }}
                                                                        preview={{ src: blobUrl }}
                                                                    />
                                                                ) : (
                                                                    <div style={{
                                                                        width: 40,
                                                                        height: 40,
                                                                        display: "flex",
                                                                        alignItems: "center",
                                                                        justifyContent: "center",
                                                                        background: "#f0f0f0",
                                                                        borderRadius: 4,
                                                                        border: "1px solid #d9d9d9"
                                                                    }}>
                                                                        <Spin size="small" />
                                                                    </div>
                                                                )
                                                            ) : isPdf ? (
                                                                <FilePdfOutlined style={{ fontSize: 20, color: "#f5222d" }} />
                                                            ) : isWord ? (
                                                                <FileWordOutlined style={{ fontSize: 20, color: "#1890ff" }} />
                                                            ) : (
                                                                <FileOutlined style={{ fontSize: 20, color: "#666" }} />
                                                            )}
                                                            <span style={{ flex: 1, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                                                                {fileName}
                                                            </span>
                                                            <Button
                                                                type="link"
                                                                size="small"
                                                                icon={<DownloadOutlined />}
                                                                onClick={async () => {
                                                                    try {
                                                                        const res = await apiClient.get(
                                                                            `/kyc/documents/${doc.id}/file`,
                                                                            { responseType: "blob" }
                                                                        );
                                                                        const blob = new Blob([res.data]);
                                                                        const url = window.URL.createObjectURL(blob);
                                                                        const a = document.createElement("a");
                                                                        a.href = url;
                                                                        a.download = fileName;
                                                                        a.click();
                                                                        window.URL.revokeObjectURL(url);
                                                                        message.success("فایل با موفقیت دانلود شد");
                                                                    } catch (error) {
                                                                        message.error("خطا در دانلود فایل");
                                                                    }
                                                                }}
                                                            >
                                                                دانلود
                                                            </Button>
                                                        </div>
                                                        {doc.adminNote && (
                                                            <Alert type="warning" message={doc.adminNote} showIcon size="small" />
                                                        )}
                                                    </Space>
                                                );
                                            }
                                            
                                            // اگر filePath ندارد، مقدار متنی را نمایش بده
                                            return (
                                                <Space direction="vertical" size="small" style={{ width: "100%" }}>
                                                    <span>{doc.value || "-"}</span>
                                                    {doc.adminNote && (
                                                        <Alert type="warning" message={doc.adminNote} showIcon size="small" />
                                                    )}
                                                </Space>
                                            );
                                        },
                                    },
                                    {
                                        title: "وضعیت",
                                        dataIndex: "status",
                                        key: "status",
                                        render: (status) => {
                                            const statusInfo = getDocumentStatusInfo(status);
                                            return <Tag color={statusInfo.color}>{statusInfo.label}</Tag>;
                                        },
                                    },
                                    {
                                        title: "یادداشت مدیر",
                                        dataIndex: "adminNote",
                                        key: "adminNote",
                                        render: (note) => note ? <Alert type="warning" message={note} showIcon size="small" /> : "-",
                                    },
                                ]}
                            />
                        </div>
                    )}
                </>
            )}
        </>
    );

    return (
            <Card
                title="پروفایل کاربری"
            >
            <Tabs
                activeKey={activeTab}
                onChange={setActiveTab}
                items={[
                    {
                        key: "user",
                        label: "اطلاعات کاربری",
                        children: userProfileTab,
                    },
                    ...(supplierProfile
                        ? [
                              {
                                  key: "supplier",
                                  label: "پروفایل تأمین‌کننده",
                                  children: supplierProfileTab,
                              },
                          ]
                        : []),
                    ...(buyerProfile || true // همیشه Tab را نمایش بده (حتی اگر پروفایل وجود نداشته باشد)
                        ? [
                              {
                                  key: "buyer",
                                  label: "پروفایل خریدار",
                                  children: (
                                      <>
                                          {!buyerProfile ? (
                                              <Alert type="info" message="شما هنوز پروفایل خریدار ایجاد نکرده‌اید. می‌توانید آن را ایجاد کنید." />
                                          ) : !editingBuyer ? (
                                              <>
                                                  <Descriptions 
                                                      bordered 
                                                      size="small"
                                                      column={2}
                                                      style={{ marginBottom: 24 }}
                                                  >
                                                      <Descriptions.Item label="نام کسب‌وکار" span={1}>
                                                          {buyerProfile.businessName || "-"}
                                                      </Descriptions.Item>
                                                      <Descriptions.Item label="شماره تماس اضطراری" span={1}>
                                                          {buyerProfile.contactMobile || "-"}
                                                      </Descriptions.Item>
                                                      <Descriptions.Item label="استان" span={1}>
                                                          {buyerProfile.province || "-"}
                                                      </Descriptions.Item>
                                                      <Descriptions.Item label="شهر" span={1}>
                                                          {buyerProfile.city || "-"}
                                                      </Descriptions.Item>
                                                      <Descriptions.Item label="آدرس" span={2}>
                                                          {buyerProfile.address || "-"}
                                                      </Descriptions.Item>
                                                      {buyerProfile.description && (
                                                          <Descriptions.Item label="توضیحات" span={2}>
                                                              {buyerProfile.description}
                                                          </Descriptions.Item>
                                                      )}
                                                      <Descriptions.Item label="بازاریاب معرف" span={2}>
                                                          {buyerProfile.referredByVisitorName ? (
                                                              <Space>
                                                                  <Text strong>{buyerProfile.referredByVisitorName}</Text>
                                                                  {buyerProfile.referredByVisitorCode && (
                                                                      <Text type="secondary" code>{buyerProfile.referredByVisitorCode}</Text>
                                                                  )}
                                                              </Space>
                                                          ) : (
                                                              <Text type="secondary">تنظیم نشده</Text>
                                                          )}
                                                      </Descriptions.Item>
                                                      <Descriptions.Item label="تاریخ ایجاد" span={1}>
                                                          {buyerProfile.createdAt ? toShamsi(buyerProfile.createdAt) : "-"}
                                                      </Descriptions.Item>
                                                      {buyerProfile.updatedAt && (
                                                          <Descriptions.Item label="آخرین به‌روزرسانی" span={1}>
                                                              {toShamsi(buyerProfile.updatedAt)}
                                                          </Descriptions.Item>
                                                      )}
                                                  </Descriptions>
                                                  <Alert
                                                      type="info"
                                                      message="برای ثبت بازاریاب و ویرایش پروفایل خریدار روی دکمه رو به رو کلیک کنید."
                                                      action={
                                                          <Button
                                                              type="primary"
                                                              icon={<EditOutlined />}
                                                              onClick={() => setEditingBuyer(true)}
                                                          >
                                                              ویرایش پروفایل
                                                          </Button>
                                                      }
                                                      style={{ marginTop: 24 }}
                                                  />
                                              </>
                                          ) : (
                                              <Form
                                                  form={buyerForm}
                                                  layout="vertical"
                                                  onFinish={handleBuyerSave}
                                              >
                                                  <Form.Item label="نام کسب‌وکار" name="businessName">
                                                      <Input placeholder="نام کسب‌وکار یا نام نمایشی" />
                                                  </Form.Item>

                                                  <Form.Item 
                                                      label="شماره تماس اضطراری" 
                                                      name="contactMobile"
                                                      rules={[
                                                          {
                                                              pattern: /^09\d{9}$/,
                                                              message: "شماره تماس باید 11 رقم و با 09 شروع شود",
                                                          },
                                                      ]}
                                                  >
                                                      <Input placeholder="09xxxxxxxxx" />
                                                  </Form.Item>

                                                  <Row gutter={12}>
                                                      <Col span={12}>
                                                          <Form.Item label="استان" name="province">
                                                              <Select
                                                                  placeholder="انتخاب استان"
                                                                  allowClear
                                                                  onChange={(value) => {
                                                                      setSelectedProvince(value || null);
                                                                      buyerForm.setFieldsValue({ city: null });
                                                                  }}
                                                              >
                                                                  {provinces.map((p) => (
                                                                      <Select.Option key={p} value={p}>
                                                                          {p}
                                                                      </Select.Option>
                                                                  ))}
                                                              </Select>
                                                          </Form.Item>
                                                      </Col>
                                                      <Col span={12}>
                                                          <Form.Item label="شهر" name="city">
                                                              <Select
                                                                  placeholder="انتخاب شهر"
                                                                  allowClear
                                                                  disabled={!selectedProvince}
                                                              >
                                                                  {cities.map((c) => (
                                                                      <Select.Option key={c} value={c}>
                                                                          {c}
                                                                      </Select.Option>
                                                                  ))}
                                                              </Select>
                                                          </Form.Item>
                                                      </Col>
                                                  </Row>

                                                  <Form.Item label="آدرس" name="address">
                                                      <Input.TextArea rows={2} placeholder="آدرس" />
                                                  </Form.Item>

                                                  <Form.Item label="توضیحات" name="description">
                                                      <Input.TextArea rows={3} placeholder="توضیحات" />
                                                  </Form.Item>

                                                  <Form.Item
                                                      label="کد معرف بازاریاب (اختیاری)"
                                                      name="referrerVisitorCode"
                                                      help={buyerProfile.referredByVisitorCode 
                                                          ? "کد معرف قبلاً تنظیم شده و قابل تغییر نیست. برای حذف یا تغییر، با مدیر سیستم تماس بگیرید."
                                                          : "اگر از طریق یک بازاریاب معرفی شده‌اید، کد معرف او را وارد کنید. با این کار، پورسانت خریدهای شما به بازاریاب تعلق می‌گیرد."}
                                                  >
                                                      <Input
                                                          placeholder="کد معرف بازاریاب (4 کاراکتر)"
                                                          maxLength={4}
                                                          style={{ textTransform: "uppercase" }}
                                                          disabled={!!buyerProfile.referredByVisitorCode}
                                                          addonAfter={
                                                              !buyerProfile.referredByVisitorCode ? (
                                                                  <Button
                                                                      size="small"
                                                                      onClick={handleValidateReferralCode}
                                                                      loading={validatingCode}
                                                                  >
                                                                      بررسی
                                                                  </Button>
                                                              ) : null
                                                          }
                                                      />
                                                  </Form.Item>

                                                  <Space>
                                                      <Button type="primary" htmlType="submit" loading={savingBuyer}>
                                                          ذخیره
                                                      </Button>
                                                      <Button onClick={() => {
                                                          setEditingBuyer(false);
                                                          if (buyerProfile) {
                                                              buyerForm.setFieldsValue({
                                                                  businessName: buyerProfile.businessName,
                                                                  contactMobile: buyerProfile.contactMobile,
                                                                  province: buyerProfile.province,
                                                                  city: buyerProfile.city,
                                                                  address: buyerProfile.address,
                                                                  description: buyerProfile.description,
                                                                  referrerVisitorCode: buyerProfile.referredByVisitorCode || null,
                                                              });
                                                              setSelectedProvince(buyerProfile.province || null);
                                                          }
                                                      }}>
                                                          انصراف
                                                      </Button>
                                                  </Space>
                                              </Form>
                                          )}
                                      </>
                                  ),
                              },
                          ]
                        : []),
                    ...(visitorProfile
                        ? [
                              {
                                  key: "visitor",
                                  label: "پروفایل بازاریاب",
                                  children: (
                                      <>
                                          <Descriptions 
                                              bordered 
                                              size="small"
                                              column={2}
                                              style={{ marginBottom: 24 }}
                                          >
                                              <Descriptions.Item label="کد معرف" span={2}>
                                                  <Space>
                                                      <span style={{
                                                          fontSize: "16px",
                                                          fontWeight: "bold",
                                                          color: "#1890ff",
                                                          fontFamily: "monospace",
                                                      }}>
                                                          {visitorProfile.referralCode}
                                                      </span>
                                                      <Button
                                                          size="small"
                                                          icon={<EditOutlined />}
                                                          onClick={() => {
                                                              navigator.clipboard.writeText(visitorProfile.referralCode);
                                                              message.success("کد معرف کپی شد");
                                                          }}
                                                      >
                                                          کپی
                                                      </Button>
                                                  </Space>
                                              </Descriptions.Item>
                                              <Descriptions.Item label="وضعیت" span={1}>
                                                  <Tag color={visitorProfile.isActive ? "success" : "default"}>
                                                      {visitorProfile.isActive ? "فعال" : "غیرفعال"}
                                                  </Tag>
                                              </Descriptions.Item>
                                              <Descriptions.Item label="شماره موبایل (Login)" span={1}>
                                                  {visitorProfile.userPhoneNumber || "-"}
                                              </Descriptions.Item>
                                              <Descriptions.Item label="نام کسب‌وکار" span={1}>
                                                  {visitorProfile.businessName || "-"}
                                              </Descriptions.Item>
                                              <Descriptions.Item label="شماره تماس اضطراری" span={1}>
                                                  {visitorProfile.contactMobile || "-"}
                                              </Descriptions.Item>
                                              <Descriptions.Item label="استان" span={1}>
                                                  {visitorProfile.province || "-"}
                                              </Descriptions.Item>
                                              <Descriptions.Item label="شهر" span={1}>
                                                  {visitorProfile.city || "-"}
                                              </Descriptions.Item>
                                              <Descriptions.Item label="آدرس" span={2}>
                                                  {visitorProfile.address || "-"}
                                              </Descriptions.Item>
                                              {visitorProfile.description && (
                                                  <Descriptions.Item label="توضیحات" span={2}>
                                                      {visitorProfile.description}
                                                  </Descriptions.Item>
                                              )}
                                              <Descriptions.Item label="تاریخ ایجاد" span={1}>
                                                  {visitorProfile.createdAt ? toShamsi(visitorProfile.createdAt) : "-"}
                                              </Descriptions.Item>
                                              {visitorProfile.updatedAt ? (
                                                  <Descriptions.Item label="آخرین به‌روزرسانی" span={1}>
                                                      {toShamsi(visitorProfile.updatedAt)}
                                                  </Descriptions.Item>
                                              ) : (
                                                  <Descriptions.Item label="آخرین به‌روزرسانی" span={1}>
                                                      "-"
                                                  </Descriptions.Item>
                                              )}
                                          </Descriptions>
                                          <Alert
                                              type="info"
                                              message="برای مدیریت خریداران معرفی شده، به صفحه اختصاصی آن بروید."
                                              action={
                                                  <Button
                                                      type="primary"
                                                      onClick={() => navigate("/my-buyers")}
                                                  >
                                                      مدیریت خریداران
                                                  </Button>
                                              }
                                          />
                                      </>
                                  ),
                              },
                          ]
                        : []),
                ]}
            />

            {/* Password Change Modal */}
            <Modal
                title="تغییر رمز عبور"
                open={isPasswordModalOpen}
                onCancel={() => {
                    setIsPasswordModalOpen(false);
                    passwordForm.resetFields();
                }}
                footer={null}
                destroyOnClose
            >
                <Form
                    form={passwordForm}
                    layout="vertical"
                    onFinish={handleChangePassword}
                >
                    {userInfo?.passwordHash && (
                        <Form.Item
                            label="رمز عبور فعلی"
                            name="currentPassword"
                            rules={[
                                { required: true, message: "رمز عبور فعلی را وارد کنید" },
                            ]}
                        >
                            <Input.Password placeholder="رمز عبور فعلی" />
                        </Form.Item>
                    )}

                    <Form.Item
                        label="رمز عبور جدید"
                        name="newPassword"
                        rules={[
                            { required: true, message: "رمز عبور جدید را وارد کنید" },
                            { min: 6, message: "رمز عبور باید حداقل 6 کاراکتر باشد" },
                        ]}
                    >
                        <Input.Password placeholder="رمز عبور جدید (حداقل 6 کاراکتر)" />
                    </Form.Item>

                    <Form.Item
                        label="تأیید رمز عبور جدید"
                        name="confirmPassword"
                        dependencies={["newPassword"]}
                        rules={[
                            { required: true, message: "تأیید رمز عبور را وارد کنید" },
                            ({ getFieldValue }) => ({
                                validator(_, value) {
                                    if (!value || getFieldValue("newPassword") === value) {
                                        return Promise.resolve();
                                    }
                                    return Promise.reject(
                                        new Error("رمز عبور جدید و تأیید آن مطابقت ندارند")
                                    );
                                },
                            }),
                        ]}
                    >
                        <Input.Password placeholder="تأیید رمز عبور جدید" />
                    </Form.Item>

                    <Form.Item>
                        <Space>
                            <Button
                                type="primary"
                                htmlType="submit"
                                loading={changingPassword}
                            >
                                تغییر رمز عبور
                            </Button>
                            <Button
                                onClick={() => {
                                    setIsPasswordModalOpen(false);
                                    passwordForm.resetFields();
                                }}
                            >
                                انصراف
                            </Button>
                        </Space>
                    </Form.Item>
                </Form>
            </Modal>
        </Card>
    );
};

export default ProfilePage;
