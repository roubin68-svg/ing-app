// src/features/walletManagement/pages/FinancialDashboardPage.jsx
import React, { useEffect, useState } from "react";
import {
    Card,
    Row,
    Col,
    Statistic,
    message,
    Spin,
    Typography,
    Space,
    Tag,
} from "antd";
import {
    DollarOutlined,
    ArrowDownOutlined,
    ArrowUpOutlined,
    WalletOutlined,
    GiftOutlined,
    ShoppingOutlined,
    PhoneOutlined,
    BankOutlined,
    RiseOutlined,
    FallOutlined,
} from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import walletManagementApi from "../api/walletManagementApi";

const { Title } = Typography;

// نمایش مبلغ به تومان در UI (ورودی به ریال است)
const formatPrice = (rial) => {
    if (rial == null) return "0";
    const toman = rial / 10;
    return toman.toLocaleString("fa-IR");
};

const FinancialDashboardPage = () => {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(true);
    const [dashboard, setDashboard] = useState({
        totalRealIncomeRial: 0,
        realIncomeTransactionCount: 0,
        totalRealExpenseRial: 0,
        realExpenseTransactionCount: 0,
        netRealProfitRial: 0,
        totalCommissionsRial: 0,
        commissionCount: 0,
        totalSubscriptionPurchasesRial: 0,
        subscriptionPurchaseCount: 0,
        totalUnlockContactFeesRial: 0,
        unlockContactCount: 0,
        totalWalletBalanceRial: 0,
        walletUserWithTransactionCount: 0,
        internalTransactionCount: 0,
    });

    useEffect(() => {
        loadDashboard();
    }, []);

    const loadDashboard = async () => {
        try {
            setLoading(true);
            const data = await walletManagementApi.getFinancialDashboard();
            setDashboard(data);
        } catch (error) {
            console.error("Error loading financial dashboard", error);
            const msg =
                error?.message ||
                error?.response?.data?.message ||
                "خطا در دریافت داشبورد مالی";
            message.error(msg);
        } finally {
            setLoading(false);
        }
    };

    // تابع برای رفتن به گزارش تراکنش‌ها با فیلتر
    const navigateToTransactions = (filters) => {
        const params = new URLSearchParams();
        if (filters.sourceCategory) params.append("sourceCategory", filters.sourceCategory);
        if (filters.operationType) params.append("operationTypeCode", filters.operationType);
        if (filters.directionCode) params.append("directionCode", filters.directionCode);
        navigate(`/wallet-transactions-report?${params.toString()}`);
    };

    return (
        <div>
            <Spin spinning={loading}>
                {/* تراکنش‌های واقعی بانکی */}
                <Card
                    title={
                        <Space>
                            <BankOutlined />
                            <span>تراکنش‌های واقعی بانکی</span>
                        </Space>
                    }
                    style={{ marginBottom: 16 }}
                >
                    <Row gutter={16}>
                        <Col xs={24} sm={12} md={8} lg={8}>
                            <Card
                                hoverable
                                style={{ cursor: "pointer", background: "#f6ffed", borderColor: "#b7eb8f" }}
                                onClick={() => navigate("/bank-transactions-report?directionCode=Credit")}
                            >
                                <Statistic
                                    title="درآمد واقعی (تومان)"
                                    value={dashboard.totalRealIncomeRial}
                                    valueStyle={{
                                        color: "#52c41a",
                                        fontSize: "20px",
                                        fontWeight: "bold",
                                        direction: "ltr",
                                    }}
                                    prefix={<ArrowDownOutlined />}
                                    formatter={(value) => `${formatPrice(value)} تومان`}
                                />
                                <div style={{ marginTop: 8, fontSize: "12px", color: "#999" }}>
                                    {(dashboard.realIncomeTransactionCount || 0).toLocaleString("fa-IR")} تراکنش بانکی واریز
                                </div>
                            </Card>
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={8}>
                            <Card
                                hoverable
                                style={{ cursor: "pointer", background: "#fff1f0", borderColor: "#ffccc7" }}
                                onClick={() => navigate("/bank-transactions-report?directionCode=Debit")}
                            >
                                <Statistic
                                    title="هزینه واقعی (تومان)"
                                    value={dashboard.totalRealExpenseRial}
                                    valueStyle={{
                                        color: "#f5222d",
                                        fontSize: "20px",
                                        fontWeight: "bold",
                                        direction: "ltr",
                                    }}
                                    prefix={<ArrowUpOutlined />}
                                    formatter={(value) => `${formatPrice(value)} تومان`}
                                />
                                <div style={{ marginTop: 8, fontSize: "12px", color: "#999" }}>
                                    {(dashboard.realExpenseTransactionCount || 0).toLocaleString("fa-IR")} تراکنش بانکی برداشت
                                </div>
                            </Card>
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={8}>
                            <Card
                                style={{
                                    background: dashboard.netRealProfitRial >= 0 ? "#f6ffed" : "#fff1f0",
                                    borderColor: dashboard.netRealProfitRial >= 0 ? "#b7eb8f" : "#ffccc7",
                                }}
                            >
                                <Statistic
                                    title="سود/زیان خالص (تومان)"
                                    value={dashboard.netRealProfitRial}
                                    valueStyle={{
                                        color: dashboard.netRealProfitRial >= 0 ? "#52c41a" : "#f5222d",
                                        fontSize: "20px",
                                        fontWeight: "bold",
                                        direction: "ltr",
                                    }}
                                    prefix={dashboard.netRealProfitRial >= 0 ? <RiseOutlined /> : <FallOutlined />}
                                    formatter={(value) => `${formatPrice(value)} تومان`}
                                />
                                <div style={{ marginTop: 8 }}>
                                    {dashboard.netRealProfitRial >= 0 ? (
                                        <Tag color="green">سود</Tag>
                                    ) : (
                                        <Tag color="red">زیان</Tag>
                                    )}
                                </div>
                            </Card>
                        </Col>
                    </Row>
                </Card>

                {/* تراکنش‌های داخلی */}
                <Card
                    title={
                        <Space>
                            <WalletOutlined />
                            <span>تراکنش‌های داخلی سیستم</span>
                        </Space>
                    }
                    style={{ marginBottom: 16 }}
                >
                    <Row gutter={16}>
                        <Col xs={24} sm={12} md={8} lg={8}>
                            <Card
                                hoverable
                                style={{ cursor: "pointer" }}
                                onClick={() => navigateToTransactions({ sourceCategory: "Commission" })}
                            >
                                <Statistic
                                    title="پورسانت‌ها (تومان)"
                                    value={dashboard.totalCommissionsRial}
                                    valueStyle={{
                                        color: "#1890ff",
                                        fontSize: "18px",
                                        fontWeight: "bold",
                                        direction: "ltr",
                                    }}
                                    prefix={<GiftOutlined />}
                                    formatter={(value) => `${formatPrice(value)} تومان`}
                                />
                                <div style={{ marginTop: 8, fontSize: "12px", color: "#999" }}>
                                    {dashboard.commissionCount.toLocaleString("fa-IR")} پورسانت پرداخت شده
                                </div>
                            </Card>
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={8}>
                            <Card
                                hoverable
                                style={{ cursor: "pointer" }}
                                onClick={() => navigateToTransactions({ operationType: "SubscriptionPurchase" })}
                            >
                                <Statistic
                                    title="خرید اشتراک (تومان)"
                                    value={dashboard.totalSubscriptionPurchasesRial}
                                    valueStyle={{
                                        color: "#722ed1",
                                        fontSize: "18px",
                                        fontWeight: "bold",
                                        direction: "ltr",
                                    }}
                                    prefix={<ShoppingOutlined />}
                                    formatter={(value) => `${formatPrice(value)} تومان`}
                                />
                                <div style={{ marginTop: 8, fontSize: "12px", color: "#999" }}>
                                    {dashboard.subscriptionPurchaseCount.toLocaleString("fa-IR")} خرید اشتراک
                                </div>
                            </Card>
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={8}>
                            <Card
                                hoverable
                                style={{ cursor: "pointer" }}
                                onClick={() => navigateToTransactions({ operationType: "UnlockContactFee" })}
                            >
                                <Statistic
                                    title="باز کردن تماس (تومان)"
                                    value={dashboard.totalUnlockContactFeesRial}
                                    valueStyle={{
                                        color: "#fa8c16",
                                        fontSize: "18px",
                                        fontWeight: "bold",
                                        direction: "ltr",
                                    }}
                                    prefix={<PhoneOutlined />}
                                    formatter={(value) => `${formatPrice(value)} تومان`}
                                />
                                <div style={{ marginTop: 8, fontSize: "12px", color: "#999" }}>
                                    {dashboard.unlockContactCount.toLocaleString("fa-IR")} باز کردن تماس
                                </div>
                            </Card>
                        </Col>
                    </Row>
                </Card>

                {/* خلاصه کیف پول‌ها */}
                <Card
                    title={
                        <Space>
                            <WalletOutlined />
                            <span>خلاصه کیف پول‌ها</span>
                        </Space>
                    }
                    style={{ marginBottom: 16 }}
                >
                    <Row gutter={16}>
                        <Col xs={24} sm={12} md={12} lg={12}>
                            <Card
                                hoverable
                                style={{ cursor: "pointer" }}
                                onClick={() => navigate("/wallet-management?hasTransactions=true")}
                            >
                                <Statistic
                                    title="مجموع موجودی کیف پول‌ها (تومان)"
                                    value={dashboard.totalWalletBalanceRial}
                                    valueStyle={{
                                        color: "#1890ff",
                                        fontSize: "20px",
                                        fontWeight: "bold",
                                        direction: "ltr",
                                    }}
                                    prefix={<WalletOutlined />}
                                    formatter={(value) => `${formatPrice(value)} تومان`}
                                />
                                <div style={{ marginTop: 8, fontSize: "12px", color: "#999" }}>
                                    {(dashboard.walletUserWithTransactionCount || 0).toLocaleString("fa-IR")} کاربر دارای گردش حساب
                                </div>
                            </Card>
                        </Col>
                        <Col xs={24} sm={12} md={12} lg={12}>
                            <Card>
                                <Statistic
                                    title="تعداد کل تراکنش‌ها"
                                    value={((dashboard.realIncomeTransactionCount || 0) + (dashboard.realExpenseTransactionCount || 0)) + (dashboard.internalTransactionCount || 0)}
                                    valueStyle={{
                                        color: "#722ed1",
                                        fontSize: "20px",
                                        fontWeight: "bold",
                                    }}
                                    formatter={(value) => `${value.toLocaleString("fa-IR")} تراکنش`}
                                />
                                <div style={{ marginTop: 8, fontSize: "12px", color: "#999" }}>
                                    {((dashboard.realIncomeTransactionCount || 0) + (dashboard.realExpenseTransactionCount || 0)).toLocaleString("fa-IR")} بانکی +{" "}
                                    {dashboard.internalTransactionCount.toLocaleString("fa-IR")} داخلی
                                </div>
                            </Card>
                        </Col>
                    </Row>
                </Card>

                {/* دکمه مشاهده گزارش کامل */}
                <Card>
                    <Row gutter={16}>
                        <Col xs={24} sm={8}>
                            <Card
                                hoverable
                                style={{ cursor: "pointer", textAlign: "center" }}
                                onClick={() => navigate("/wallet-transactions-report")}
                            >
                                <Title level={4} style={{ margin: 0 }}>
                                    مشاهده گزارش کامل تراکنش‌ها
                                </Title>
                                <div style={{ marginTop: 8, color: "#999" }}>
                                    کلیک کنید برای مشاهده جزئیات
                                </div>
                            </Card>
                        </Col>
                        <Col xs={24} sm={8}>
                            <Card
                                hoverable
                                style={{ cursor: "pointer", textAlign: "center" }}
                                onClick={() => navigate("/commissions-report")}
                            >
                                <Title level={4} style={{ margin: 0 }}>
                                    گزارش پورسانت‌ها
                                </Title>
                                <div style={{ marginTop: 8, color: "#999" }}>
                                    مشاهده جزئیات پورسانت‌ها
                                </div>
                            </Card>
                        </Col>
                        <Col xs={24} sm={8}>
                            <Card
                                hoverable
                                style={{ cursor: "pointer", textAlign: "center" }}
                                onClick={() => navigate("/income-expense-report")}
                            >
                                <Title level={4} style={{ margin: 0 }}>
                                    گزارش درآمد/هزینه
                                </Title>
                                <div style={{ marginTop: 8, color: "#999" }}>
                                    مشاهده گزارش درآمد/هزینه
                                </div>
                            </Card>
                        </Col>
                    </Row>
                </Card>
            </Spin>
        </div>
    );
};

export default FinancialDashboardPage;




