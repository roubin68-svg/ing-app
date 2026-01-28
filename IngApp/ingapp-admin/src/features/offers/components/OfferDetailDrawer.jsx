// src/features/offers/components/OfferDetailDrawer.jsx
import React, { useEffect, useState, useRef } from "react";
import {
    Drawer,
    Spin,
    Card,
    Descriptions,
    Space,
    Button,
    message,
    Divider,
    Image,
    Tag,
} from "antd";
import { DownloadOutlined, FileOutlined, FilePdfOutlined, FileWordOutlined, PhoneOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import jalaali from "jalaali-js";
import offersApi from "../api/offersApi";
import apiClient from "../../../core/api/apiClient";
import productsApi from "../../products/api/productsApi";
import { PictureOutlined } from "@ant-design/icons";

// Hook برای تشخیص اندازه صفحه
const useWindowSize = () => {
    const [windowSize, setWindowSize] = useState({
        width: typeof window !== 'undefined' ? window.innerWidth : 1024,
        height: typeof window !== 'undefined' ? window.innerHeight : 768,
    });

    useEffect(() => {
        const handleResize = () => {
            setWindowSize({
                width: window.innerWidth,
                height: window.innerHeight,
            });
        };

        window.addEventListener('resize', handleResize);
        return () => window.removeEventListener('resize', handleResize);
    }, []);

    return windowSize;
};

// تبدیل تاریخ میلادی به شمسی
const toShamsi = (gregorian) => {
    if (!gregorian) return null;
    
    if (typeof gregorian === "string") {
        const [y, m, d] = gregorian.split("T")[0].split("-").map(Number);
        const j = jalaali.toJalaali(y, m, d);
        return `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
    }
    
    if (gregorian instanceof Date) {
        const j = jalaali.toJalaali(
            gregorian.getFullYear(),
            gregorian.getMonth() + 1,
            gregorian.getDate()
        );
        return `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
    }
    
    return null;
};

const formatPrice = (v) =>
    v != null
        ? v.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
        : "-";

const OfferDetailDrawer = ({ offerId, visible, onClose }) => {
    const { width } = useWindowSize();
    const [loading, setLoading] = useState(false);
    const [offerDetail, setOfferDetail] = useState(null);
    const [contactInfo, setContactInfo] = useState(null);
    const [loadingContact, setLoadingContact] = useState(false);
    const [showContactInfo, setShowContactInfo] = useState(false);
    const [attributeTemplates, setAttributeTemplates] = useState([]);
    const [productImageBlobUrl, setProductImageBlobUrl] = useState(null);

    // محاسبه عرض Drawer بر اساس اندازه صفحه
    const drawerWidth = width < 768 ? "100%" : width < 1024 ? "90%" : 600;

    // -----------------------
    // Helper: Transform contact info from PascalCase to camelCase
    // -----------------------
    const transformContactInfo = (rawInfo) => {
        if (!rawInfo) return null;
        return {
            businessName: rawInfo?.BusinessName || rawInfo?.businessName,
            supplierTypeName: rawInfo?.SupplierTypeName || rawInfo?.supplierTypeName,
            contactPhone: rawInfo?.ContactPhone || rawInfo?.contactPhone,
            mobile: rawInfo?.Mobile || rawInfo?.mobile,
            address: rawInfo?.Address || rawInfo?.address,
            province: rawInfo?.Province || rawInfo?.province,
            city: rawInfo?.City || rawInfo?.city,
        };
    };

    // -----------------------
    // Handle Contact Click
    // -----------------------
    const handleShowContact = async () => {
        try {
            setLoadingContact(true);
            
            // اگر قبلاً اطلاعات تماس load شده، فقط نمایش بده
            if (contactInfo && showContactInfo) {
                return;
            }

            // 1. ابتدا Unlock Contact را انجام بده (با پرداخت از کیف پول)
            try {
                const unlockResult = await offersApi.unlockContact(offerId);
                // apiClient interceptor unwraps ApiResult, so unlockResult is: UnlockContactResultDto (PascalCase)
                
                // بررسی نتیجه Unlock (PascalCase از Backend)
                const isUnlocked = unlockResult?.IsUnlocked || unlockResult?.isUnlocked;
                const charged = unlockResult?.Charged || unlockResult?.charged;
                const chargedAmountToman = unlockResult?.ChargedAmountToman || unlockResult?.chargedAmountToman;
                const errorMessage = unlockResult?.ErrorMessage || unlockResult?.errorMessage;
                
                if (isUnlocked) {
                    if (charged) {
                        message.success(
                            `اطلاعات تماس با موفقیت باز شد. ${chargedAmountToman ? `مبلغ: ${chargedAmountToman.toLocaleString("fa-IR")} تومان` : ''}`
                        );
                        // به‌روزرسانی موجودی کیف پول در header
                        window.dispatchEvent(new CustomEvent('walletBalanceChanged'));
                    } else {
                        message.success("اطلاعات تماس باز شد (رایگان)");
                    }
                } else if (errorMessage) {
                    message.error(errorMessage);
                    return;
                } else {
                    message.error("خطا در باز کردن اطلاعات تماس");
                    return;
                }
            } catch (unlockError) {
                // اگر خطا در Unlock بود، بررسی کن که آیا به خاطر موجودی ناکافی است
                const errorMsg = unlockError?.response?.data?.message || unlockError?.message || "خطا در باز کردن اطلاعات تماس";
                
                if (errorMsg.includes("موجودی") || errorMsg.includes("کافی نیست")) {
                    message.error("موجودی کیف پول کافی نیست. لطفاً ابتدا کیف پول خود را شارژ کنید.");
                } else {
                    message.error(errorMsg);
                }
                console.error("Unlock Contact Error:", unlockError);
                return;
            }

            // 2. ثبت کلیک
            try {
                await offersApi.logContactClick(offerId);
            } catch (e) {
                console.error("Error logging contact click:", e);
            }

            // 3. دریافت اطلاعات تماس
            const result = await offersApi.getSupplierContact(offerId);
            // apiClient interceptor unwraps ApiResult, so result is: { BusinessName, SupplierTypeName, ... } (PascalCase)
            const info = transformContactInfo(result);
            setContactInfo(info);
            setShowContactInfo(true);
        } catch (error) {
            message.error("خطا در دریافت اطلاعات تماس");
            console.error(error);
        } finally {
            setLoadingContact(false);
        }
    };

    // -----------------------
    // Load Offer Detail
    // -----------------------
    useEffect(() => {
        if (!visible || !offerId) {
            setOfferDetail(null);
            setContactInfo(null);
            setShowContactInfo(false);
            return;
        }

        let isMounted = true;

        const load = async () => {
            try {
                setLoading(true);
                
                // 1. Load offer detail
                const result = await offersApi.getPublicDetail(offerId);
                if (!isMounted) return;
                setOfferDetail(result);
                
                // 2. Load attribute templates if productId exists
                if (result?.header?.productId) {
                    try {
                        const templatesRes = await offersApi.getProductAttributeTemplates(result.header.productId);
                        if (!isMounted) return;
                        const templates = templatesRes?.data ?? templatesRes ?? [];
                        setAttributeTemplates(templates);
                    } catch (e) {
                        console.error("Error loading attribute templates:", e);
                        if (isMounted) setAttributeTemplates([]);
                    }
                }

                // 3. Load product image if exists
                if (result?.header?.productImagePath && result?.header?.productId) {
                    try {
                        const url = await productsApi.getProductImageBlobUrl(
                            result.header.productId,
                            result.header.productImagePath
                        );
                        if (!isMounted) return;
                        setProductImageBlobUrl(url);
                    } catch (e) {
                        console.error("Error loading product image:", e);
                        if (isMounted) setProductImageBlobUrl(null);
                    }
                } else {
                    if (isMounted) setProductImageBlobUrl(null);
                }

                // 4. Check if user has viewed contact info (from backend) - ALWAYS check from backend
                try {
                    const hasViewedResult = await offersApi.hasViewedContact(offerId);
                    if (!isMounted) return;
                    
                    // apiClient interceptor unwraps ApiResult, so response is: { hasViewed: true/false }
                    const hasViewed = hasViewedResult?.hasViewed === true;
                    
                    if (hasViewed) {
                        // اگر قبلاً دیده باشد، اطلاعات تماس را مستقیماً load کن
                        try {
                            setLoadingContact(true);
                            const contactResult = await offersApi.getSupplierContact(offerId);
                            if (!isMounted) return;
                            
                            // apiClient interceptor unwraps ApiResult, so contactResult is: { BusinessName, SupplierTypeName, ... } (PascalCase)
                            const info = transformContactInfo(contactResult);
                            
                            if (isMounted) {
                                setContactInfo(info);
                                setShowContactInfo(true);
                            }
                        } catch (e) {
                            console.error("Error loading contact info:", e);
                            if (isMounted) {
                                setContactInfo(null);
                                setShowContactInfo(false);
                            }
                        } finally {
                            if (isMounted) setLoadingContact(false);
                        }
                    } else {
                        // اگر قبلاً ندیده باشد، اطلاعات تماس را reset کن
                        if (isMounted) {
                            setContactInfo(null);
                            setShowContactInfo(false);
                        }
                    }
                } catch (e) {
                    console.error("Error checking viewed contact:", e);
                    if (isMounted) {
                        setContactInfo(null);
                        setShowContactInfo(false);
                    }
                }
            } catch (error) {
                if (isMounted) {
                    message.error("خطا در بارگذاری جزئیات آگهی");
                    console.error(error);
                }
            } finally {
                if (isMounted) setLoading(false);
            }
        };

        load();

        return () => {
            isMounted = false;
        };
    }, [visible, offerId]);

    // Cleanup blob URL
    useEffect(() => {
        return () => {
            if (productImageBlobUrl) {
                window.URL.revokeObjectURL(productImageBlobUrl);
            }
        };
    }, [productImageBlobUrl]);

    // -----------------------
    // Render Document Value
    // -----------------------
    const renderDocumentValue = (doc) => {
        switch (doc.dataType) {
            case 1: // Text
                return <span>{doc.value || "-"}</span>;

            case 2: // Number
                return <span>{doc.value || "-"}</span>;

            case 3: // Boolean
                return (
                    <span>{doc.value === "true" ? "بله" : doc.value === "false" ? "خیر" : "-"}</span>
                );

            case 4: // Date
                return <span>{doc.value ? toShamsi(doc.value) : "-"}</span>;

            case 5: // File
                if (!doc.filePath) return <span>-</span>;
                
                const isImage = doc.filePath.match(/\.(jpg|jpeg|png|gif|webp|bmp)$/i);
                const isPdf = doc.filePath.match(/\.pdf$/i);
                const isWord = doc.filePath.match(/\.(doc|docx)$/i);
                const fileName = doc.value || "فایل پیوست";
                
                // ساخت URL کامل برای فایل (با استفاده از baseURL از apiClient)
                // باید /api/v1 را حذف کنیم و فقط base را بگیریم
                const baseURL = apiClient.defaults.baseURL?.replace("/api/v1", "") || "";
                const fileUrl = `${baseURL}/api/v1/offers/${offerId}/files?offerId=${offerId}&filePath=${encodeURIComponent(doc.filePath)}`;
                
                return (
                    <div style={{ 
                        background: "#fafafa", 
                        padding: "12px", 
                        borderRadius: "6px", 
                        border: "1px solid #e8e8e8"
                    }}>
                        {isImage ? (
                            <div style={{ marginBottom: 12, textAlign: "center" }}>
                                <Image
                                    src={fileUrl}
                                    alt={fileName}
                                    style={{ maxWidth: "100%", maxHeight: 300, borderRadius: "4px" }}
                                    preview={{
                                        src: fileUrl
                                    }}
                                />
                            </div>
                        ) : (
                            <div style={{ 
                                marginBottom: 12, 
                                textAlign: "center",
                                padding: "24px",
                                background: "#fff",
                                borderRadius: "4px",
                                border: "1px dashed #d9d9d9"
                            }}>
                                {isPdf ? (
                                    <FilePdfOutlined style={{ fontSize: 48, color: "#ff4d4f" }} />
                                ) : isWord ? (
                                    <FileWordOutlined style={{ fontSize: 48, color: "#1890ff" }} />
                                ) : (
                                    <FileOutlined style={{ fontSize: 48, color: "#666" }} />
                                )}
                                <div style={{ marginTop: 8, fontSize: 12, color: "#666" }}>
                                    {isPdf ? "فایل PDF" : isWord ? "فایل Word" : "فایل پیوست"}
                                </div>
                            </div>
                        )}
                        <div style={{ 
                            display: "flex", 
                            alignItems: "center", 
                            justifyContent: "space-between",
                            gap: 12
                        }}>
                            <span style={{ 
                                fontSize: 14, 
                                color: "#333",
                                flex: 1,
                                overflow: "hidden",
                                textOverflow: "ellipsis",
                                whiteSpace: "nowrap"
                            }}>
                                {fileName}
                            </span>
                            <Button
                                type="primary"
                                size="small"
                                icon={<DownloadOutlined />}
                                onClick={async () => {
                                    try {
                                        await offersApi.downloadPublicOfferFile(offerId, doc.filePath, doc.value);
                                    } catch (error) {
                                        message.error("خطا در دانلود فایل");
                                    }
                                }}
                            >
                                دانلود
                            </Button>
                        </div>
                    </div>
                );

            default:
                return <span>{doc.value || "-"}</span>;
        }
    };

    if (!visible) return null;

    return (
        <Drawer
            title="جزئیات آگهی"
            placement="right"
            width={drawerWidth}
            onClose={onClose}
            open={visible}
        >
            {loading ? (
                <div style={{ textAlign: "center", padding: 48 }}>
                    <Spin size="large" />
                </div>
            ) : offerDetail ? (
                <Space direction="vertical" size="large" style={{ width: "100%" }}>
                    {/* نمایش محصول با تصویر و دسته - مشابه کارت‌های آگهی */}
                    <div style={{ display: "flex", alignItems: "flex-start", gap: 12     }}>
                        <div style={{ flex: 1 }}>
                            <Tag color="blue" style={{ marginBottom: 8 }}>
                                #{offerDetail.header?.id}
                            </Tag>
                            <div style={{ fontSize: 16, fontWeight: "bold", marginBottom: 8 }}>
                                {offerDetail.header?.productName || "-"}
                            </div>
                            <Tag>{offerDetail.header?.productCategoryName || "بدون دسته‌بندی"}</Tag>
                        </div>
                        {productImageBlobUrl ? (
                            <Image
                                src={productImageBlobUrl}
                                alt={offerDetail.header?.productName}
                                width={80}
                                height={80}
                                style={{ objectFit: "cover", borderRadius: 4, flexShrink: 0, border: "1px solid #f0f0f0" }}
                                preview={false}
                            />
                        ) : (
                            <div
                                style={{
                                    width: 80,
                                    height: 80,
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "center",
                                    background: "#f0f0f0",
                                    borderRadius: 4,
                                    border: "1px solid #f0f0f0",
                                    flexShrink: 0,
                                }}
                            >
                                <PictureOutlined style={{ fontSize: 28, color: "#999" }} />
                            </div>
                        )}
                    </div>

                    {/* اطلاعات اصلی */}
                    <Descriptions column={1} bordered size="small">
                        <Descriptions.Item label="قیمت واحد (تومان)">
                            {formatPrice(offerDetail.header?.unitPrice)}
                        </Descriptions.Item>
                        <Descriptions.Item label="مقدار">
                            {offerDetail.header?.quantity
                                ? `${formatPrice(offerDetail.header.quantity)} ${offerDetail.header.unit || ""}`
                                : "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label={<span style={{ fontWeight: 600 }}>قیمت کل (تومان)</span>}>
                            <span style={{ fontWeight: 600 }}>
                                {formatPrice(offerDetail.header?.totalPrice)}
                            </span>
                        </Descriptions.Item>
                        {offerDetail.header?.hasTax && offerDetail.header?.taxAmount ? (
                            <>
                                <Descriptions.Item label={<span style={{ fontWeight: 600 }}>مبلغ مالیات (تومان)</span>}>
                                    <span style={{ fontWeight: 600 }}>
                                        {formatPrice(offerDetail.header?.taxAmount)}
                                    </span>
                                </Descriptions.Item>
                                <Descriptions.Item label={<span style={{ fontWeight: 600 }}>قیمت کل + مالیات (تومان)</span>}>
                                    <span style={{ fontWeight: 600 }}>
                                        {formatPrice(
                                            (offerDetail.header?.totalPrice || 0) + 
                                            (offerDetail.header?.taxAmount || 0)
                                        )}
                                    </span>
                                </Descriptions.Item>
                            </>
                        ) : (
                            <Descriptions.Item label="مالیات">
                                <span style={{ color: "#52c41a", fontSize: 12 }}>
                                    این کالا مالیات ندارد
                                </span>
                            </Descriptions.Item>
                        )}
                        <Descriptions.Item label="تاریخ انتشار">
                            {offerDetail.header?.publishedAt
                                ? toShamsi(offerDetail.header.publishedAt)
                                : "-"}
                        </Descriptions.Item>
                        {offerDetail.header?.expireAtBySupplier && (
                            <Descriptions.Item label="تاریخ انقضا">
                                {toShamsi(offerDetail.header.expireAtBySupplier)}
                            </Descriptions.Item>
                        )}
                    </Descriptions>

                    {/* ویژگی‌ها و مدارک - مشابه مرحله 4 */}
                    {attributeTemplates.length > 0 && (
                        <Card title="ویژگی‌ها و مدارک" size="small">
                            <Space direction="vertical" style={{ width: "100%" }} size="middle">
                                {attributeTemplates.map(attr => {
                                    const doc = offerDetail?.documents?.find(
                                        d => d.attributeDefinitionId === attr.attributeDefinitionId
                                    );

                                    return (
                                        <div
                                            key={attr.attributeDefinitionId}
                                            style={{
                                                display: "flex",
                                                justifyContent: "space-between",
                                                padding: "6px 0",
                                                borderBottom: "1px dashed #eee",
                                            }}
                                        >
                                            <span>{attr.displayName}</span>

                                            {attr.dataType === 5 ? (
                                                // File type: نمایش thumbnail/icon + دکمه Download
                                                doc?.filePath ? (
                                                    <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                                                        {(() => {
                                                            const fileName = doc.filePath.toLowerCase();
                                                            const isImage = /\.(jpg|jpeg|png|gif|webp|bmp)$/i.test(fileName);
                                                            const isPdf = fileName.endsWith(".pdf");
                                                            const isWord = /\.(doc|docx)$/i.test(fileName);
                                                            
                                                            // ساخت URL برای فایل
                                                            const baseURL = apiClient.defaults.baseURL?.replace("/api/v1", "") || "";
                                                            const fileUrl = `${baseURL}/api/v1/offers/${offerId}/files?offerId=${offerId}&filePath=${encodeURIComponent(doc.filePath)}`;
                                                            
                                                            if (isImage) {
                                                                return (
                                                                    <Image
                                                                        src={fileUrl}
                                                                        alt={doc.value || "تصویر"}
                                                                        width={32}
                                                                        height={32}
                                                                        style={{ 
                                                                            objectFit: "cover", 
                                                                            borderRadius: 4,
                                                                            border: "1px solid #d9d9d9",
                                                                            cursor: "pointer"
                                                                        }}
                                                                        preview={{
                                                                            src: fileUrl
                                                                        }}
                                                                    />
                                                                );
                                                            } else {
                                                                return (
                                                                    <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                                                                        {isPdf ? (
                                                                            <FilePdfOutlined style={{ fontSize: 20, color: "#ff4d4f" }} />
                                                                        ) : isWord ? (
                                                                            <FileWordOutlined style={{ fontSize: 20, color: "#1890ff" }} />
                                                                        ) : (
                                                                            <FileOutlined style={{ fontSize: 20, color: "#666" }} />
                                                                        )}
                                                                        <Button 
                                                                            size="small"
                                                                            icon={<DownloadOutlined />}
                                                                            onClick={async () => {
                                                                                try {
                                                                                    await offersApi.downloadPublicOfferFile(
                                                                                        offerId,
                                                                                        doc.filePath,
                                                                                        doc.value
                                                                                    );
                                                                                } catch (error) {
                                                                                    message.error("خطا در دانلود فایل");
                                                                                }
                                                                            }}
                                                                        >
                                                                            دانلود فایل
                                                                        </Button>
                                                                    </div>
                                                                );
                                                            }
                                                        })()}
                                                    </div>
                                                ) : (
                                                    <span>-</span>
                                                )
                                            ) : attr.dataType === 4 ? (
                                                // Date type: تبدیل gregorian به شمسی
                                                doc?.value ? (
                                                    <span>{toShamsi(doc.value)}</span>
                                                ) : (
                                                    <span>-</span>
                                                )
                                            ) : attr.dataType === 3 ? (
                                                // Boolean type: نمایش "بله" یا "خیر"
                                                doc?.value === "true" ? (
                                                    <span>بله</span>
                                                ) : doc?.value === "false" ? (
                                                    <span>خیر</span>
                                                ) : (
                                                    <span>-</span>
                                                )
                                            ) : (
                                                // Text, Number: نمایش مستقیم
                                                <span>{doc?.value ?? "-"}</span>
                                            )}
                                        </div>
                                    );
                                })}
                            </Space>
                        </Card>
                    )}

                    {/* دکمه نمایش اطلاعات تماس - فقط اگر اطلاعات تماس نمایش داده نشده باشد */}
                    {!showContactInfo && (
                        <Card size="small">
                            <Button
                                type="primary"
                                icon={<PhoneOutlined />}
                                loading={loadingContact}
                                onClick={handleShowContact}
                                block
                            >
                                نمایش اطلاعات تماس
                            </Button>
                        </Card>
                    )}

                    {/* نمایش اطلاعات تماس - زیر ویژگی‌ها و مدارک */}
                    {showContactInfo && contactInfo && (
                        <Card title="اطلاعات تماس تأمین‌کننده" size="small">
                            <Space direction="vertical" style={{ width: "100%" }} size="middle">
                                <div
                                    style={{
                                        display: "flex",
                                        justifyContent: "space-between",
                                        padding: "6px 0",
                                        borderBottom: "1px dashed #eee",
                                    }}
                                >
                                    <span>نام کسب‌وکار</span>
                                    <span>{contactInfo.businessName || "-"}</span>
                                </div>
                                <div
                                    style={{
                                        display: "flex",
                                        justifyContent: "space-between",
                                        padding: "6px 0",
                                        borderBottom: "1px dashed #eee",
                                    }}
                                >
                                    <span>نوع تأمین‌کننده</span>
                                    <span>{contactInfo.supplierTypeName || "-"}</span>
                                </div>
                                <div
                                    style={{
                                        display: "flex",
                                        justifyContent: "space-between",
                                        padding: "6px 0",
                                        borderBottom: "1px dashed #eee",
                                    }}
                                >
                                    <span>موبایل</span>
                                    <span>{contactInfo.mobile || "-"}</span>
                                </div>
                                {contactInfo.contactPhone && (
                                    <div
                                        style={{
                                            display: "flex",
                                            justifyContent: "space-between",
                                            padding: "6px 0",
                                            borderBottom: "1px dashed #eee",
                                        }}
                                    >
                                        <span>شماره تماس دفتر</span>
                                        <span>{contactInfo.contactPhone}</span>
                                    </div>
                                )}
                                {contactInfo.province && (
                                    <div
                                        style={{
                                            display: "flex",
                                            justifyContent: "space-between",
                                            padding: "6px 0",
                                            borderBottom: "1px dashed #eee",
                                        }}
                                    >
                                        <span>استان</span>
                                        <span>{contactInfo.province}</span>
                                    </div>
                                )}
                                {contactInfo.city && (
                                    <div
                                        style={{
                                            display: "flex",
                                            justifyContent: "space-between",
                                            padding: "6px 0",
                                            borderBottom: "1px dashed #eee",
                                        }}
                                    >
                                        <span>شهر</span>
                                        <span>{contactInfo.city}</span>
                                    </div>
                                )}
                                {contactInfo.address && (
                                    <div
                                        style={{
                                            display: "flex",
                                            justifyContent: "space-between",
                                            padding: "6px 0",
                                            borderBottom: "1px dashed #eee",
                                        }}
                                    >
                                        <span>آدرس</span>
                                        <span>{contactInfo.address}</span>
                                    </div>
                                )}
                            </Space>
                        </Card>
                    )}
                </Space>
            ) : (
                <div style={{ textAlign: "center", padding: 48 }}>
                    <p>آگهی یافت نشد</p>
                </div>
            )}
        </Drawer>
    );
};

export default OfferDetailDrawer;

