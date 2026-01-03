// src/features/auth/pages/ProfilePage.jsx
import React, { useEffect, useState } from "react";
import { Card, Descriptions, Form, Input, Button, Space, message, Modal, Tag, Spin, Alert, Tabs, Table, Image, App } from "antd";
import { useNavigate } from "react-router-dom";
import { EditOutlined, DownloadOutlined, FilePdfOutlined, FileWordOutlined, FileOutlined } from "@ant-design/icons";
import { getMeApi, updateMyProfileApi } from "../api/authApi";
import { useAuth } from "../../../core/auth/useAuth";
import supplierOnboardingApi from "../../suppliers/api/supplierOnboardingApi";
import apiClient from "../../../core/api/apiClient";
import jalaali from "jalaali-js";

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

const getUserTypeLabel = (userType) => {
    switch (userType) {
        case "Buyer": return "خریدار";
        case "Supplier": return "تأمین‌کننده";
        case "Admin": return "مدیر سیستم";
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
    const [kycDocuments, setKycDocuments] = useState([]);
    const [editing, setEditing] = useState(false);
    const [activeTab, setActiveTab] = useState("user");
    const [fileBlobUrls, setFileBlobUrls] = useState({}); // { documentId: blobUrl }

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
        } catch (error) {
            message.error("خطا در بارگذاری اطلاعات پروفایل");
            console.error(error);
        } finally {
            setLoading(false);
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
                        <Descriptions.Item label="وضعیت" span={2}>
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
                        <Descriptions.Item label="نام کسب‌وکار" span={2}>
                            {supplierProfile.businessName || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="شماره موبایل (Login)" span={2}>
                            {supplierProfile.userPhoneNumber || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="شماره تماس">
                            {supplierProfile.contactPhone || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="کد ملی">
                            {supplierProfile.nationalId || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="شماره مجوز">
                            {supplierProfile.licenseNumber || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="نام رابط">
                            {supplierProfile.contactName || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="استان">
                            {supplierProfile.province || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="شهر">
                            {supplierProfile.city || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="آدرس" span={2}>
                            {supplierProfile.address || "-"}
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
            extra={
                activeTab === "user" && !editing && (
                    <Button
                        type="primary"
                        icon={<EditOutlined />}
                        onClick={() => setEditing(true)}
                    >
                        ویرایش نام و شماره موبایل
                    </Button>
                )
            }
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
                    {
                        key: "supplier",
                        label: "پروفایل تأمین‌کننده",
                        children: supplierProfileTab,
                    },
                ]}
            />
        </Card>
    );
};

export default ProfilePage;
