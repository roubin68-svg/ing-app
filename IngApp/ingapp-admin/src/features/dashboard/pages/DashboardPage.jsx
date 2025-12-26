import React, { useEffect, useState } from "react";
import { Card, Row, Col, Tag } from "antd";
import { useNavigate } from "react-router-dom";

import supplierOnboardingApi from "../../suppliers/api/supplierOnboardingApi";

const DashboardPage = () => {
    const navigate = useNavigate();

    const [supplierStatus, setSupplierStatus] = useState(null);
    const [loading, setLoading] = useState(true);

    // ----------------------------------
    // Load supplier onboarding status
    // ----------------------------------
    useEffect(() => {
        const loadStatus = async () => {
            try {
                const res = await supplierOnboardingApi.getMyProfile();

                if (!res) {
                    setSupplierStatus(null);
                } else {
                    setSupplierStatus({
                        status: res.verificationStatus, // NotSubmitted | Pending | Approved | Rejected
                        rejectionReason: res.rejectionReason,
                    });
                }
            } catch {
                setSupplierStatus(null);
            } finally {
                setLoading(false);
            }
        };

        loadStatus();
    }, []);

    // ----------------------------------
    // UI helpers
    // ----------------------------------
    const getCardBackground = () => {
        if (!supplierStatus) return {};

        switch (supplierStatus.status) {
            case "NotSubmitted":
                return { backgroundColor: "#f0f5ff" }; // blue light
            case "Pending":
                return { backgroundColor: "#fff7e6" }; // orange light
            case "Approved":
                return { backgroundColor: "#f6ffed" }; // green light
            case "Rejected":
                return { backgroundColor: "#fff1f0" }; // red light
            default:
                return {};
        }
    };
    const getCardBorderColore = () => {
        if (!supplierStatus) return {};

        switch (supplierStatus.status) {
            case "NotSubmitted":
                return { borderColor: "#2f54eb" }; // blue light
            case "Pending":
                return { borderColor: "#fa8c16" }; // orange light
            case "Approved":
                return { borderColor: "#52c41a" }; // green light
            case "Rejected":
                return { borderColor: "#ff4d4f" }; // red light
            default:
                return {};
        }
    };

    const renderStatusTag = () => {
        if (!supplierStatus) return null;

        switch (supplierStatus.status) {
            case "NotSubmitted":
                return <Tag color="blue">در انتظار ارسال مدارک</Tag>;
            case "Pending":
                return <Tag color="orange">در حال بررسی</Tag>;
            case "Approved":
                return <Tag color="green">تأیید شده</Tag>;
            case "Rejected":
                return <Tag color="red">رد شده</Tag>;
            default:
                return null;
        }
    };

    const renderStatusMessage = () => {
        if (!supplierStatus) return null;

        let borderColor = "#d9d9d9";
        let text = "";

        switch (supplierStatus.status) {
            case "NotSubmitted":
                borderColor = "#2f54eb";
                text = "هنوز مدارکی ارسال نکرده‌اید. لطفاً فرآیند ثبت‌نام را تکمیل کنید.";
                break;
            case "Pending":
                borderColor = "#fa8c16";
                text = "مدارک شما ارسال شده و در انتظار بررسی است.";
                break;
            case "Approved":
                borderColor = "#52c41a";
                text = "درخواست شما تأیید شده است.";
                break;
            case "Rejected":
                borderColor = "#ff4d4f";
                text = supplierStatus.rejectionReason
                    ? `درخواست شما رد شده است: ${supplierStatus.rejectionReason}`
                    : "درخواست شما رد شده است.";
                break;
            default:
                return null;
        }

        return (
            <div
                style={{
                    marginTop: 12,
                    padding: "10px 12px",
                    border: `1px solid ${borderColor}`,
                    borderRadius: 8,
                    background: "#fff",
                    fontSize: 13,
                }}
            >
                {text}
            </div>
        );
    };

    return (
        <div className="ingapp-page">
            <h1 className="ingapp-page-title">داشبورد</h1>

            <Row gutter={[16, 16]}>
                <Col xs={24} sm={12} md={12} lg={10}>
                    <Card
                        hoverable
                        loading={loading}
                        onClick={() => navigate("/supplier-onboarding")}
                        style={{
                            height: "100%",
                            ...getCardBackground(),
                            ...getCardBorderColore(),
                        }}
                    >
                        <Card.Meta
                            title="درخواست همکاری به عنوان تأمین‌کننده"
                            description={
                                <>
                                    <div>
                                        ثبت و مدیریت اطلاعات برای تبدیل شدن به
                                        تأمین‌کننده
                                    </div>

                                    {supplierStatus && (
                                        <>
                                            <div style={{ marginTop: 8 }}>
                                                {renderStatusTag()}
                                            </div>
                                            {renderStatusMessage()}
                                        </>
                                    )}
                                </>
                            }
                        />
                    </Card>
                </Col>
            </Row>
        </div>
    );
};

export default DashboardPage;
