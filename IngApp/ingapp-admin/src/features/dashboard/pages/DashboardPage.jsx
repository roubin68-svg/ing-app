import React, { useEffect, useState } from "react";
import { Card, Row, Col, Tag, Button, Space, Spin, Empty, Alert, Image } from "antd";
import { useNavigate } from "react-router-dom";
import { EyeOutlined, ArrowLeftOutlined, InfoCircleOutlined, PictureOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import jalaali from "jalaali-js";

import supplierOnboardingApi from "../../suppliers/api/supplierOnboardingApi";
import offersApi from "../../offers/api/offersApi";
import productsApi from "../../products/api/productsApi";

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

const DashboardPage = () => {
    const navigate = useNavigate();

    const [supplierStatus, setSupplierStatus] = useState(null);
    const [loading, setLoading] = useState(true);
    const [recentOffers, setRecentOffers] = useState([]);
    const [loadingOffers, setLoadingOffers] = useState(false);
    const [showApprovedAlert, setShowApprovedAlert] = useState(false);
    const [imageBlobUrls, setImageBlobUrls] = useState({}); // { "productId_imagePath": blobUrl }

    // ----------------------------------
    // Load supplier onboarding status
    // ----------------------------------
    useEffect(() => {
        const loadStatus = async () => {
            try {
                const res = await supplierOnboardingApi.getMyProfile();

                if (!res) {
                    setSupplierStatus(null);
                    setShowApprovedAlert(false);
                } else {
                    const status = res.verificationStatus;
                    setSupplierStatus({
                        status: status, // NotSubmitted | Pending | Approved | Rejected
                        rejectionReason: res.rejectionReason,
                    });
                    
                    // بررسی اینکه آیا Alert تأیید قبلاً نمایش داده شده یا نه
                    if (status === "Approved") {
                        const alertShown = localStorage.getItem("supplierApprovedAlertShown");
                        if (!alertShown) {
                            setShowApprovedAlert(true);
                        }
                    } else {
                        setShowApprovedAlert(false);
                    }
                }
            } catch {
                setSupplierStatus(null);
                setShowApprovedAlert(false);
            } finally {
                setLoading(false);
            }
        };

        loadStatus();
    }, []);

    // ----------------------------------
    // Load Recent Offers
    // ----------------------------------
    useEffect(() => {
        const loadRecentOffers = async () => {
            try {
                setLoadingOffers(true);
                const result = await offersApi.searchPublic({
                    page: 1,
                    pageSize: 12,
                    sortBy: "newest",
                });
                const offers = result || [];
                setRecentOffers(offers);

                // ساخت blob URLs برای تصاویر محصولات
                const blobUrlPromises = offers
                    .filter(offer => offer.productImagePath && offer.productId)
                    .map(async (offer) => {
                        try {
                            const blobUrl = await productsApi.getProductImageBlobUrl(offer.productId, offer.productImagePath);
                            if (blobUrl) {
                                return { key: `${offer.productId}_${offer.productImagePath}`, blobUrl };
                            }
                        } catch (err) {
                            console.error(`Error loading image for product ${offer.productId}:`, err);
                        }
                        return null;
                    });

                const blobUrlResults = await Promise.all(blobUrlPromises);
                const newBlobUrls = {};
                blobUrlResults.forEach(result => {
                    if (result) {
                        newBlobUrls[result.key] = result.blobUrl;
                    }
                });

                // Revoke old URLs
                setImageBlobUrls(prev => {
                    Object.values(prev).forEach(url => {
                        if (url && typeof url === 'string') {
                            window.URL.revokeObjectURL(url);
                        }
                    });
                    return newBlobUrls;
                });
            } catch (error) {
                console.error("خطا در بارگذاری آگهی‌های اخیر:", error);
            } finally {
                setLoadingOffers(false);
            }
        };

        loadRecentOffers();
    }, []);

    // Cleanup: Revoke blob URLs when component unmounts
    useEffect(() => {
        return () => {
            Object.values(imageBlobUrls).forEach(url => {
                if (url && typeof url === 'string') {
                    window.URL.revokeObjectURL(url);
                }
            });
        };
    }, [imageBlobUrls]);

    // ----------------------------------
    // UI helpers
    // ----------------------------------


    // بررسی اینکه آیا باید Alert نمایش داده شود
    const shouldShowAlert = () => {
        if (!supplierStatus) return false;
        // فقط برای NotSubmitted, Pending, Rejected نمایش بده
        return ["NotSubmitted", "Pending", "Rejected"].includes(supplierStatus.status);
    };

    const getAlertType = () => {
        if (!supplierStatus) return "info";
        switch (supplierStatus.status) {
            case "NotSubmitted":
                return "info";
            case "Pending":
                return "warning";
            case "Rejected":
                return "error";
            default:
                return "info";
        }
    };

    const getAlertMessage = () => {
        if (!supplierStatus) return "";
        switch (supplierStatus.status) {
            case "NotSubmitted":
                return "هنوز مدارکی ارسال نکرده‌اید. برای تبدیل شدن به تأمین‌کننده، لطفاً فرآیند ثبت‌نام را تکمیل کنید.";
            case "Pending":
                return "مدارک شما ارسال شده و در انتظار بررسی است. پس از تأیید، می‌توانید آگهی ثبت کنید.";
            case "Rejected":
                return supplierStatus.rejectionReason
                    ? `درخواست شما رد شده است: ${supplierStatus.rejectionReason}. لطفاً اطلاعات را اصلاح و مجدداً ارسال کنید.`
                    : "درخواست شما رد شده است. لطفاً اطلاعات را اصلاح و مجدداً ارسال کنید.";
            default:
                return "";
        }
    };

    return (
        <Card title="داشبورد">
            {/* Alert برای تأیید شدن (فقط یک بار) */}
            {showApprovedAlert && (
                <Alert
                    message="درخواست همکاری شما تأیید شده است"
                    description="تبریک! شما اکنون می‌توانید آگهی ثبت کنید."
                    type="success"
                    showIcon
                    closable
                    onClose={() => {
                        setShowApprovedAlert(false);
                        localStorage.setItem("supplierApprovedAlertShown", "true");
                    }}
                    style={{ marginBottom: 16 }}
                />
            )}

            {/* Alert برای درخواست همکاری (فقط در صورت نیاز) */}
            {shouldShowAlert() && (
                <Alert
                    message="درخواست همکاری به عنوان تأمین‌کننده"
                    description={
                        <Space direction="vertical" size="small">
                            <div>{getAlertMessage()}</div>
                            <Button
                                type="default"
                                size="small"
                                onClick={() => navigate("/supplier-onboarding")}
                            >
                                مشاهده و مدیریت درخواست
                            </Button>
                        </Space>
                    }
                    type={getAlertType()}
                    icon={<InfoCircleOutlined />}
                    showIcon
                    closable
                    style={{ marginBottom: 16 }}
                />
            )}

            {/* 10 آگهی آخر */}
            <Row gutter={[16, 16]}>
                <Col xs={24}>
                    <Card
                        title="آگهی‌های اخیر"
                        extra={
                            <Button
                                type="link"
                                icon={<ArrowLeftOutlined />}
                                onClick={() => navigate("/offers-search")}
                            >
                                مشاهده تمام آگهی‌ها
                            </Button>
                        }
                    >
                        {loadingOffers ? (
                            <div style={{ textAlign: "center", padding: "48px 0" }}>
                                <Spin size="large" />
                                    </div>
                        ) : recentOffers.length === 0 ? (
                            <Empty description="آگهی‌ای یافت نشد" />
                        ) : (
                            <Row gutter={[16, 16]}>
                                {recentOffers.map((offer) => (
                                    <Col xs={24} sm={12} md={8} lg={6} key={offer.id}>
                                        <Card
                                            hoverable
                                            style={{ height: "100%", cursor: "pointer", display: "flex", flexDirection: "column" }}
                                            bodyStyle={{ flex: 1, display: "flex", flexDirection: "column" }}
                                            onClick={() => navigate(`/offers-search?offerId=${offer.id}`)}
                                            actions={[
                                                <Button
                                                    type="link"
                                                    icon={<EyeOutlined />}
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        navigate(`/offers-search?offerId=${offer.id}`);
                                                    }}
                                                    key="view"
                                                >
                                                    مشاهده جزئیات
                                                </Button>
                                            ]}
                                        >
                                            <div style={{ flex: 1, display: "flex", flexDirection: "column" }}>
                                                <div style={{ display: "flex", alignItems: "flex-start", gap: 12, marginBottom: 15 }}>
                                                    <div style={{ flex: 1 }}>
                                                        <Tag color="blue" style={{ marginBottom: 8 }}>
                                                            #{offer.id}
                                                        </Tag>
                                                        <div style={{ fontSize: 16, fontWeight: "bold", marginBottom: 8 }}>
                                                            {offer.productName}
                                                        </div>
                                                        <Tag>{offer.productCategoryName}</Tag>
                                                    </div>
                                                    {(() => {
                                                        const blobUrlKey = offer.productImagePath && offer.productId 
                                                            ? `${offer.productId}_${offer.productImagePath}` 
                                                            : null;
                                                        const imageUrl = blobUrlKey ? imageBlobUrls[blobUrlKey] : null;
                                                        
                                                        return imageUrl ? (
                                                            <Image
                                                                src={imageUrl}
                                                                alt={offer.productName}
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
                                                                    border: "1px solid #d9d9d9",
                                                                    flexShrink: 0,
                                                                }}
                                                            >
                                                                <PictureOutlined style={{ fontSize: 28, color: "#999" }} />
                                                            </div>
                                                        );
                                                    })()}
                                                </div>
                                                <div style={{ fontSize: 13, color: "#666", flex: 1, display: "flex", flexDirection: "column" }}>
                                                    <div style={{ marginBottom: 6 }}>قیمت واحد: {formatPrice(offer.unitPrice)} تومان</div>
                                                    <div style={{ marginBottom: 6 }}>مقدار: {formatPrice(offer.quantity)} {offer.unit}</div>
                                                    <div style={{ marginBottom: 6, fontWeight: 500 }}>قیمت کل: {formatPrice(offer.totalPrice)} تومان</div>
                                                    {offer.hasTax && offer.taxAmount ? (
                                                        <>
                                                            <div style={{ marginBottom: 6 }}>مبلغ مالیات: {formatPrice(offer.taxAmount)} تومان</div>
                                                            <div style={{ marginBottom: 6, fontWeight: 500 }}>
                                                                قیمت کل + مالیات: {formatPrice((offer.totalPrice || 0) + (offer.taxAmount || 0))} تومان
                                                            </div>
                                                        </>
                                                    ) : (
                                                        <div style={{ marginBottom: 6, color: "#52c41a", fontSize: 12 }}>
                                                            این کالا مالیات ندارد
                                                        </div>
                                                    )}
                                                    <div style={{ marginTop: "auto", fontSize: 12 }}>
                                                        تاریخ انتشار: {toShamsi(offer.publishedAt) || "-"}
                                                    </div>
                                                </div>
                                            </div>
                                        </Card>
                                    </Col>
                                ))}
                            </Row>
                        )}
                    </Card>
                </Col>
            </Row>
        </Card>
    );
};

export default DashboardPage;
