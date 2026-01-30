// src/features/subscriptions/pages/SubscriptionsPage.jsx
import React, { useEffect, useState } from "react";
import {
    Card,
    Spin,
    Row,
    Col,
    Button,
    Descriptions,
    Tag,
    Table,
    Modal,
    message,
    Space,
    Popconfirm,
} from "antd";
import {
    CrownOutlined,
    CheckCircleOutlined,
    ClockCircleOutlined,
    CloseCircleOutlined,
} from "@ant-design/icons";
import { Typography } from "antd";
import subscriptionsApi from "../api/subscriptionsApi";
import dayjs from "dayjs";
import jalaali from "jalaali-js";

const { Text } = Typography;

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

const SubscriptionsPage = () => {
    const [loading, setLoading] = useState(true);
    const [plans, setPlans] = useState([]);
    const [activeSubscription, setActiveSubscription] = useState(null);
    const [history, setHistory] = useState([]);
    const [purchaseModalVisible, setPurchaseModalVisible] = useState(false);
    const [selectedPlan, setSelectedPlan] = useState(null);
    const [purchasing, setPurchasing] = useState(false);
    const [historyTablePagination, setHistoryTablePagination] = useState({
        current: 1,
        pageSize: 10,
    });
    
    // پیدا کردن subscription با آخرین EndDate از تاریخچه (برای Alert)
    // این subscription می‌تواند در حال حاضر فعال باشد یا هنوز شروع نشده باشد
    const getLatestActiveSubscription = () => {
        if (!history || history.length === 0) {
            return activeSubscription || null;
        }
        
        const now = new Date();
        // پیدا کردن subscription‌های Active که EndDate در آینده است
        const activeSubscriptions = history.filter(sub => {
            const endDate = new Date(sub.endDate || sub.EndDate);
            const statusCode = sub.statusCode || sub.StatusCode;
            return statusCode === "Active" && endDate >= now;
        });
        
        if (activeSubscriptions.length === 0) {
            return activeSubscription || null;
        }
        
        // پیدا کردن subscription با آخرین EndDate
        const latest = activeSubscriptions.reduce((latest, current) => {
            const latestEndDate = new Date(latest.endDate || latest.EndDate);
            const currentEndDate = new Date(current.endDate || current.EndDate);
            return currentEndDate > latestEndDate ? current : latest;
        });
        
        return latest || activeSubscription || null;
    };

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            setLoading(true);
            const [plansRes, activeRes, historyRes] = await Promise.all([
                subscriptionsApi.getActivePlans(),
                subscriptionsApi.getMyActiveSubscription(),
                subscriptionsApi.getMySubscriptionHistory(),
            ]);
            setPlans(plansRes || []);
            setActiveSubscription(activeRes || null);
            setHistory(historyRes || []);
        } catch (error) {
            message.error("خطا در دریافت اطلاعات اشتراک");
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const formatPrice = (rial) => {
        if (rial == null) return "-";
        const toman = rial / 10;
        return `${toman.toLocaleString("fa-IR")} تومان`;
    };

    const handlePurchase = (plan) => {
        setSelectedPlan(plan);
        setPurchaseModalVisible(true);
    };

    const handleCancelSubscription = async (subscriptionId) => {
        try {
            const result = await subscriptionsApi.cancelSubscription(subscriptionId);
            
            if (result?.success) {
                const refundToman = result.refundAmountRial ? (result.refundAmountRial / 10).toLocaleString("fa-IR") : "0";
                
                let successMessage = "اشتراک با موفقیت لغو شد.";
                if (result.refundAmountRial > 0) {
                    successMessage += ` مبلغ ${refundToman} تومان به کیف پول شما برگشت داده شد.`;
                } else {
                    successMessage += " به دلیل استفاده از اشتراک و کارمزد خدمات، مبلغی به کیف پول شما برگشت داده نشد.";
                }
                
                message.success(successMessage);
                
                // نمایش جزئیات محاسبه در یک Modal
                Modal.info({
                    title: "جزئیات محاسبه برگشت مبلغ",
                    width: 600,
                    content: (
                        <div style={{ whiteSpace: "pre-line", direction: "rtl", textAlign: "right" }}>
                            {result.calculationDescription || "جزئیات محاسبه در دسترس نیست."}
                        </div>
                    ),
                });
                
                loadData();
                
                // به‌روزرسانی موجودی کیف پول در header
                if (result.refundAmountRial > 0) {
                    window.dispatchEvent(new CustomEvent('walletBalanceChanged'));
                }
            } else {
                message.error(result?.errorMessage || "خطا در لغو اشتراک");
            }
        } catch (error) {
            const errorMsg =
                error?.response?.data?.message ||
                error?.message ||
                "خطا در لغو اشتراک";
            message.error(errorMsg);
            console.error(error);
        }
    };

    const confirmPurchase = async () => {
        if (!selectedPlan) return;

        try {
            setPurchasing(true);
            const result = await subscriptionsApi.purchaseSubscription({
                planId: selectedPlan.id,
            });

            if (result?.success) {
                // بررسی اینکه آیا subscription جدید بعد از subscription فعلی شروع می‌شود
                const willStartAfterActive = result?.willStartAfterActive || result?.WillStartAfterActive;
                const activeSubscriptionEndDate = result?.activeSubscriptionEndDate || result?.ActiveSubscriptionEndDate;
                
                let successMessage = `اشتراک با موفقیت خریداری شد!`;
                
                if (result.chargedAmountToman || result?.ChargedAmountToman) {
                    const amountToman = result.chargedAmountToman || result.ChargedAmountToman;
                    successMessage += ` مبلغ: ${amountToman.toLocaleString("fa-IR")} تومان`;
                }
                
                if (willStartAfterActive && activeSubscriptionEndDate) {
                    // اگر subscription جدید بعد از subscription فعلی شروع می‌شود
                    const endDateShamsi = toShamsi(new Date(activeSubscriptionEndDate));
                    successMessage += `. اشتراک جدید شما بعد از پایان اشتراک فعلی (${endDateShamsi}) شروع می‌شود.`;
                    
                    // نمایش پیام اضافی به صورت info
                    message.info({
                        content: `شما قبلاً اشتراک فعال دارید. اشتراک جدید بعد از پایان اشتراک فعلی (${endDateShamsi}) فعال خواهد شد.`,
                        duration: 8,
                    });
                }
                
                message.success(successMessage);
                setPurchaseModalVisible(false);
                setSelectedPlan(null);
                loadData();
                
                // به‌روزرسانی موجودی کیف پول در header
                if (result.charged || result?.Charged) {
                    window.dispatchEvent(new CustomEvent('walletBalanceChanged'));
                }
            } else {
                message.error(result?.errorMessage || "خطا در خرید اشتراک");
            }
        } catch (error) {
            const errorMsg =
                error?.response?.data?.message ||
                error?.message ||
                "خطا در خرید اشتراک";
            message.error(errorMsg);
            console.error(error);
        } finally {
            setPurchasing(false);
        }
    };

    const getStatusColor = (statusCode) => {
        if (statusCode === "Active") return "green";
        if (statusCode === "Expired") return "red";
        if (statusCode === "Cancelled") return "orange";
        return "default";
    };

    const historyColumns = [
        {
            title: "ردیف",
            key: "rowNumber",
            width: 60,
            align: "center",
            render: (_, __, index) => {
                // محاسبه شماره ردیف با در نظر گیری pagination
                const current = historyTablePagination.current || 1;
                const pageSize = historyTablePagination.pageSize || 10;
                return (current - 1) * pageSize + index + 1;
            },
        },
        {
            title: "پلن",
            dataIndex: "planTitle",
            key: "planTitle",
        },
        {
            title: "مدت",
            dataIndex: "durationMonths",
            key: "durationMonths",
            render: (months) => `${months} ماه`,
        },
        {
            title: "وضعیت",
            dataIndex: "statusCode",
            key: "statusCode",
            render: (code) => (
                <Tag color={getStatusColor(code)}>
                    {code === "Active" ? "فعال" : code === "Expired" ? "منقضی شده" : "لغو شده"}
                </Tag>
            ),
        },
        {
            title: "تاریخ خرید",
            dataIndex: "purchasedAt",
            key: "purchasedAt",
            render: (date) => toShamsi(date) || "-",
        },
        {
            title: "تاریخ شروع",
            dataIndex: "startDate",
            key: "startDate",
            render: (date) => toShamsi(date) || "-",
        },
        {
            title: "تاریخ پایان",
            dataIndex: "endDate",
            key: "endDate",
            render: (date) => toShamsi(date) || "-",
        },
        {
            title: "عملیات",
            key: "actions",
            align: "center",
            width: 120,
            render: (_, record) => {
                const now = new Date();
                const startDate = new Date(record.startDate);
                const endDate = new Date(record.endDate);
                const isCancellable = 
                    record.statusCode === "Active" && 
                    endDate > now; // هنوز تمام نشده

                if (!isCancellable) {
                    return null;
                }

                return (
                    <Popconfirm
                        title="لغو اشتراک"
                        description="آیا از لغو این اشتراک مطمئن هستید؟ مبلغ باقیمانده (منهای کارمزد خدمات) به کیف پول شما برگشت داده می‌شود."
                        onConfirm={() => handleCancelSubscription(record.id)}
                        okText="بله، لغو کن"
                        cancelText="انصراف"
                        okButtonProps={{ danger: true }}
                    >
                        <Button
                            size="small"
                            danger
                            icon={<CloseCircleOutlined />}
                        >
                            لغو
                        </Button>
                    </Popconfirm>
                );
            },
        },
    ];

    if (loading) {
        return (
            <div style={{ padding: "24px", textAlign: "center" }}>
                <Spin size="large" />
            </div>
        );
    }

    return (
        
            <Space direction="vertical" size="large" style={{ width: "100%" }}>
                {/* اشتراک فعال */}
                {activeSubscription && (
                    <Card>
                        <Space direction="vertical" size="middle" style={{ width: "100%" }}>
                            <h2 style={{ margin: 0 }}>
                                <CrownOutlined /> اشتراک فعال
                            </h2>
                            <Descriptions bordered column={2}>
                                <Descriptions.Item label="پلن">
                                    {activeSubscription.planTitle}
                                </Descriptions.Item>
                                <Descriptions.Item label="مدت">
                                    {activeSubscription.durationMonths} ماه
                                </Descriptions.Item>
                                <Descriptions.Item label="تاریخ شروع">
                                    {toShamsi(activeSubscription.startDate) || "-"}
                                </Descriptions.Item>
                                <Descriptions.Item label="تاریخ پایان">
                                    {toShamsi(activeSubscription.endDate) || "-"}
                                </Descriptions.Item>
                                <Descriptions.Item label="دسترسی نامحدود به اطلاعات تماس">
                                    {activeSubscription.unlimitedContactViews ? (
                                        <Tag color="green" icon={<CheckCircleOutlined />}>
                                            فعال
                                        </Tag>
                                    ) : (
                                        <Tag color="red">غیرفعال</Tag>
                                    )}
                                </Descriptions.Item>
                            </Descriptions>
                        </Space>
                    </Card>
                )}

                {/* پلن‌های موجود */}
                <Card 
                    title={
                        <Space>
                            <CrownOutlined />
                            <span>پلن‌های اشتراک</span>
                        </Space>
                    }
                >
                    <div style={{ marginBottom: "16px", padding: "12px", background: "#e6f7ff", borderRadius: "4px", border: "1px solid #91d5ff" }}>
                        <Text type="secondary" style={{ fontSize: "13px" }}>
                            💡 <strong>نکته:</strong> با خرید اشتراک، شما دسترسی نامحدود به اطلاعات تماس تمام آگهی‌ها خواهید داشت و دیگر نیازی به پرداخت برای هر آگهی نیست.
                        </Text>
                    </div>
                    <Row gutter={[16, 16]}>
                        {plans.map((plan) => (
                            <Col xs={24} sm={12} md={8} lg={6} key={plan.id}>
                                <Card
                                    hoverable
                                    style={{
                                        height: "100%",
                                        textAlign: "center",
                                        border: "2px solid #f0f0f0",
                                    }}
                                    bodyStyle={{ padding: "20px" }}
                                >
                                    <div style={{ marginBottom: "16px" }}>
                                        <CrownOutlined style={{ fontSize: "32px", color: "#faad14", marginBottom: "8px" }} />
                                        <h3 style={{ margin: "8px 0" }}>{plan.title}</h3>
                                    </div>
                                    
                                    <div style={{ 
                                        fontSize: "28px", 
                                        fontWeight: "bold", 
                                        color: "#1890ff",
                                        margin: "16px 0" 
                                    }}>
                                        {formatPrice(plan.priceRial)}
                                    </div>
                                    
                                    <div style={{ 
                                        margin: "12px 0",
                                        fontSize: "14px",
                                        color: "#666"
                                    }}>
                                        مدت اعتبار: <strong>{plan.durationMonths} ماه</strong>
                                    </div>
                                    
                                    {plan.unlimitedContactViews && (
                                        <div style={{ 
                                            margin: "12px 0",
                                            padding: "8px",
                                            background: "#f6ffed",
                                            borderRadius: "4px",
                                            border: "1px solid #b7eb8f"
                                        }}>
                                            <Tag color="green" icon={<CheckCircleOutlined />} style={{ margin: 0 }}>
                                                دسترسی نامحدود به اطلاعات تماس
                                            </Tag>
                                        </div>
                                    )}
                                    
                                    <div style={{ 
                                        marginTop: "16px",
                                        fontSize: "12px",
                                        color: "#999",
                                        textAlign: "right"
                                    }}>
                                        <div>✓ بدون محدودیت در تعداد آگهی‌ها</div>
                                        <div>✓ بدون نیاز به پرداخت برای هر آگهی</div>
                                        <div>✓ صرفه‌جویی در هزینه‌ها</div>
                                    </div>
                                    
                                    <Button
                                        type="primary"
                                        block
                                        size="large"
                                        icon={<CrownOutlined />}
                                        style={{ marginTop: "20px" }}
                                        onClick={() => handlePurchase(plan)}
                                    >
                                        خرید اشتراک
                                    </Button>
                                </Card>
                            </Col>
                        ))}
                    </Row>
                </Card>

                {/* تاریخچه اشتراک‌ها */}
                {history.length > 0 && (
                    <Card title="تاریخچه اشتراک‌ها">
                        <Table
                            columns={historyColumns}
                            dataSource={history}
                            rowKey="id"
                            pagination={{
                                current: historyTablePagination.current,
                                pageSize: historyTablePagination.pageSize,
                                showSizeChanger: true,
                                showTotal: (total, range) => 
                                    `${range[0]}-${range[1]} از ${total} مورد`,
                                pageSizeOptions: ['10', '20', '50', '100'],
                                onChange: (page, pageSize) => {
                                    setHistoryTablePagination({
                                        current: page,
                                        pageSize: pageSize,
                                    });
                                },
                                onShowSizeChange: (current, size) => {
                                    setHistoryTablePagination({
                                        current: 1,
                                        pageSize: size,
                                    });
                                },
                            }}
                        />
                    </Card>
                )}

                {/* Modal خرید اشتراک */}
                <Modal
                    title={
                        <Space>
                            <CrownOutlined style={{ color: "#faad14" }} />
                            <span>تأیید خرید اشتراک</span>
                        </Space>
                    }
                    open={purchaseModalVisible}
                    onOk={confirmPurchase}
                    onCancel={() => {
                        setPurchaseModalVisible(false);
                        setSelectedPlan(null);
                    }}
                    confirmLoading={purchasing}
                    okText="تأیید و خرید"
                    cancelText="انصراف"
                    width={500}
                >
                    {selectedPlan && (() => {
                        const latestSubscription = getLatestActiveSubscription();
                        return (
                            <Space direction="vertical" size="large" style={{ width: "100%" }}>
                                {/* Alert اگر subscription فعال وجود دارد (یا subscription با آخرین EndDate) */}
                                {latestSubscription && (
                                    <div style={{ 
                                        padding: "12px 16px", 
                                        background: "#fff1f0", 
                                        borderRadius: "4px",
                                        border: "1px solid #ffccc7",
                                        marginBottom: "8px"
                                    }}>
                                        <Text style={{ fontSize: "13px", color: "#cf1322" }}>
                                            <strong>⚠️ توجه:</strong> شما اشتراک فعال تا تاریخ <strong>{toShamsi(latestSubscription.endDate || latestSubscription.EndDate)}</strong> دارید. در صورت تأیید، اشتراک جدید شما بعد از پایان این اشتراک شروع می‌شود.
                                        </Text>
                                    </div>
                                )}
                            
                            <div>
                                <Text>
                                    آیا از خرید اشتراک <strong>{selectedPlan.title}</strong> مطمئن هستید؟
                                </Text>
                            </div>
                            
                            <Descriptions bordered column={1}>
                                <Descriptions.Item label="پلن">
                                    {selectedPlan.title}
                                </Descriptions.Item>
                                <Descriptions.Item label="مدت اعتبار">
                                    <strong>{selectedPlan.durationMonths} ماه</strong>
                                </Descriptions.Item>
                                <Descriptions.Item label="مبلغ پرداختی">
                                    <Text strong style={{ fontSize: "18px", color: "#1890ff" }}>
                                        {formatPrice(selectedPlan.priceRial)}
                                    </Text>
                                </Descriptions.Item>
                                {selectedPlan.unlimitedContactViews && (
                                    <Descriptions.Item label="امکانات">
                                        <Tag color="green" icon={<CheckCircleOutlined />}>
                                            دسترسی نامحدود به اطلاعات تماس تمام آگهی‌ها
                                        </Tag>
                                    </Descriptions.Item>
                                )}
                            </Descriptions>
                            
                            <div style={{ 
                                padding: "12px", 
                                background: "#fff7e6", 
                                borderRadius: "4px",
                                border: "1px solid #ffd591"
                            }}>
                                <Text type="secondary" style={{ fontSize: "12px" }}>
                                    💡 <strong>توجه:</strong> مبلغ از موجودی کیف پول شما کسر خواهد شد. در صورت موجودی ناکافی، ابتدا کیف پول خود را شارژ کنید.
                                </Text>
                            </div>
                        </Space>
                        );
                    })()}
                </Modal>
            </Space>
    );
};

export default SubscriptionsPage;

