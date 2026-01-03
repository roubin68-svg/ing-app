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
    const [loading, setLoading] = useState(false);
    const [offerDetail, setOfferDetail] = useState(null);
    const [contactModalVisible, setContactModalVisible] = useState(false);
    const [contactInfo, setContactInfo] = useState(null);
    const [loadingContact, setLoadingContact] = useState(false);

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
            } catch (error) {
                message.error("خطا در بارگذاری جزئیات آگهی");
                console.error(error);
            } finally {
                setLoading(false);
            }
        };

        load();
    }, [visible, offerId]);

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
                // apiClient.defaults.baseURL = "http://localhost:5273/api/v1"
                // پس باید /api/v1 را حذف کنیم و فقط base را بگیریم
                const baseURL = apiClient.defaults.baseURL?.replace("/api/v1", "") || "http://localhost:5273";
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
            title={
                <Space>
                    <span>جزئیات آگهی</span>
                    {offerDetail?.header?.id && (
                        <Tag color="blue">#{offerDetail.header.id}</Tag>
                    )}
                </Space>
            }
            placement="right"
            width={600}
            onClose={onClose}
            open={visible}
        >
            {loading ? (
                <div style={{ textAlign: "center", padding: 48 }}>
                    <Spin size="large" />
                </div>
            ) : offerDetail ? (
                <Space direction="vertical" size="large" style={{ width: "100%" }}>
                    {/* اطلاعات اصلی */}
                    <Card title="اطلاعات اصلی" size="small">
                        <Descriptions column={1} bordered size="small">
                            <Descriptions.Item label="محصول">
                                {offerDetail.header?.productName || "-"}
                            </Descriptions.Item>
                            <Descriptions.Item label="قیمت واحد (تومان)">
                                {formatPrice(offerDetail.header?.unitPrice)}
                            </Descriptions.Item>
                            <Descriptions.Item label="مقدار">
                                {offerDetail.header?.quantity
                                    ? `${formatPrice(offerDetail.header.quantity)} ${offerDetail.header.unit || ""}`
                                    : "-"}
                            </Descriptions.Item>
                            <Descriptions.Item label="قیمت کل (تومان)">
                                {formatPrice(offerDetail.header?.totalPrice)}
                            </Descriptions.Item>
                            {offerDetail.header?.hasTax && (
                                <Descriptions.Item label="مبلغ مالیات (تومان)">
                                    {formatPrice(offerDetail.header?.taxAmount)}
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
                    </Card>

                    {/* ویژگی‌ها و مدارک */}
                    {offerDetail.documents && offerDetail.documents.length > 0 && (
                        <Card title="ویژگی‌ها و مدارک" size="small">
                            <Space direction="vertical" size="middle" style={{ width: "100%" }}>
                                {offerDetail.documents.map((doc) => (
                                    <div key={doc.attributeDefinitionId}>
                                        <Divider orientation="left" style={{ margin: "8px 0" }}>
                                            {doc.displayName}
                                        </Divider>
                                        <div style={{ paddingRight: 16 }}>
                                            {renderDocumentValue(doc)}
                                        </div>
                                    </div>
                                ))}
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

