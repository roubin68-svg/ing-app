// src/features/offers/components/OfferDetailDrawer.jsx
import React, { useEffect, useState } from "react";
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
    Modal,
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
    const [contactModalVisible, setContactModalVisible] = useState(false);
    const [contactInfo, setContactInfo] = useState(null);
    const [loadingContact, setLoadingContact] = useState(false);
    const [attributeTemplates, setAttributeTemplates] = useState([]);
    const [productImageBlobUrl, setProductImageBlobUrl] = useState(null);

    // محاسبه عرض Drawer بر اساس اندازه صفحه
    const drawerWidth = width < 768 ? "100%" : width < 1024 ? "90%" : 600;

    // -----------------------
    // Handle Contact Click
    // -----------------------
    const handleShowContact = async () => {
        try {
            setLoadingContact(true);
            // ثبت کلیک
            await offersApi.logContactClick(offerId);
            // دریافت اطلاعات تماس
            const info = await offersApi.getSupplierContact(offerId);
            setContactInfo(info);
            setContactModalVisible(true);
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
            return;
        }

        const load = async () => {
            try {
                setLoading(true);
                const result = await offersApi.getPublicDetail(offerId);
                setOfferDetail(result);
                
                // Load attribute templates if productId exists
                if (result?.header?.productId) {
                    try {
                        const templatesRes = await offersApi.getProductAttributeTemplates(result.header.productId);
                        const templates = templatesRes?.data ?? templatesRes ?? [];
                        setAttributeTemplates(templates);
                    } catch (e) {
                        console.error("Error loading attribute templates:", e);
                        setAttributeTemplates([]);
                    }
                }

                // Load product image if exists
                if (result?.header?.productImagePath && result?.header?.productId) {
                    try {
                        const url = await productsApi.getProductImageBlobUrl(
                            result.header.productId,
                            result.header.productImagePath
                        );
                        setProductImageBlobUrl(url);
                    } catch (e) {
                        console.error("Error loading product image:", e);
                        setProductImageBlobUrl(null);
                    }
                } else {
                    setProductImageBlobUrl(null);
                }
            } catch (error) {
                message.error("خطا در بارگذاری جزئیات آگهی");
                console.error(error);
            } finally {
                setLoading(false);
            }
        };

        load();
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

                    {/* دکمه نمایش اطلاعات تماس */}
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
                </Space>
            ) : (
                <div style={{ textAlign: "center", padding: 48 }}>
                    <p>آگهی یافت نشد</p>
                </div>
            )}

            {/* Modal نمایش اطلاعات تماس */}
            <Modal
                title="اطلاعات تماس تأمین‌کننده"
                open={contactModalVisible}
                onCancel={() => {
                    setContactModalVisible(false);
                    setContactInfo(null);
                }}
                footer={[
                    <Button key="close" onClick={() => {
                        setContactModalVisible(false);
                        setContactInfo(null);
                    }}>
                        بستن
                    </Button>
                ]}
            >
                {contactInfo ? (
                    <Descriptions column={1} bordered size="small">
                        <Descriptions.Item label="نام کسب‌وکار">
                            {contactInfo.businessName || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="نوع تأمین‌کننده">
                            {contactInfo.supplierTypeName || "-"}
                        </Descriptions.Item>
                        <Descriptions.Item label="موبایل">
                            {contactInfo.mobile || "-"}
                        </Descriptions.Item>
                        {contactInfo.contactPhone && (
                            <Descriptions.Item label="شماره تماس دفتر">
                                {contactInfo.contactPhone}
                            </Descriptions.Item>
                        )}
                        {contactInfo.province && (
                            <Descriptions.Item label="استان">
                                {contactInfo.province}
                            </Descriptions.Item>
                        )}
                        {contactInfo.city && (
                            <Descriptions.Item label="شهر">
                                {contactInfo.city}
                            </Descriptions.Item>
                        )}
                        {contactInfo.address && (
                            <Descriptions.Item label="آدرس">
                                {contactInfo.address}
                            </Descriptions.Item>
                        )}
                    </Descriptions>
                ) : (
                    <Spin />
                )}
            </Modal>
        </Drawer>
    );
};

export default OfferDetailDrawer;

