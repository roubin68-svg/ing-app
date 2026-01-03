import React, { useEffect, useState } from "react";
import { Card, Row, Col, Tag, Button, Space, Spin, Empty, Alert } from "antd";
import { useNavigate } from "react-router-dom";
import { EyeOutlined, ArrowLeftOutlined, InfoCircleOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import jalaali from "jalaali-js";

import supplierOnboardingApi from "../../suppliers/api/supplierOnboardingApi";
import offersApi from "../../offers/api/offersApi";

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
                    pageSize: 10,
                    sortBy: "newest",
                });
                setRecentOffers(result || []);
            } catch (error) {
                console.error("خطا در بارگذاری آگهی‌های اخیر:", error);
            } finally {
                setLoadingOffers(false);
            }
        };

        loadRecentOffers();
    }, []);

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
                                            style={{ height: "100%", cursor: "pointer" }}
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
                                            <div style={{ marginBottom: 12 }}>
                                                <Tag color="blue" style={{ marginBottom: 8 }}>
                                                    #{offer.id}
                                                </Tag>
                                            </div>
                                            <Card.Meta
                                                title={
                                                    <div style={{ fontSize: 16, fontWeight: "bold", marginBottom: 12 }}>
                                                        {offer.productName}
                                                    </div>
                                                }
                                                description={
                                                    <Space direction="vertical" size="middle" style={{ width: "100%" }}>
                                                        <div>
                                                            <Tag>{offer.productCategoryName}</Tag>
                                                        </div>
                                                        <div style={{ fontSize: 13, color: "#666" }}>
                                                            <div style={{ marginBottom: 6 }}>قیمت واحد: {formatPrice(offer.unitPrice)} تومان</div>
                                                            <div style={{ marginBottom: 6 }}>قیمت کل: {formatPrice(offer.totalPrice)} تومان</div>
                                                            <div style={{ marginBottom: 6 }}>مقدار: {formatPrice(offer.quantity)} {offer.unit}</div>
                                                            <div style={{ marginTop: 8, fontSize: 12 }}>
                                                                تاریخ انتشار: {toShamsi(offer.publishedAt) || "-"}
                                                            </div>
                                                        </div>
                                                    </Space>
                                                }
                                            />
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
