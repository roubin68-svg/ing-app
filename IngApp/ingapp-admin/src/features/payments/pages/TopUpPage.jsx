// src/features/payments/pages/TopUpPage.jsx
import React, { useEffect, useState } from "react";
import {
    Card,
    Form,
    InputNumber,
    Button,
    Select,
    message,
    Spin,
    Descriptions,
    Space,
    Steps,
} from "antd";
import { WalletOutlined, CheckCircleOutlined } from "@ant-design/icons";
import paymentsApi from "../api/paymentsApi";
import { useNavigate } from "react-router-dom";

const { Step } = Steps;

const TopUpPage = () => {
    const navigate = useNavigate();
    const [form] = Form.useForm();
    const [loading, setLoading] = useState(false);
    const [gateways, setGateways] = useState([]);
    const [currentStep, setCurrentStep] = useState(0);
    const [paymentId, setPaymentId] = useState(null);
    const [paymentStatus, setPaymentStatus] = useState(null);
    const [checkingStatus, setCheckingStatus] = useState(false);

    useEffect(() => {
        loadGateways();
    }, []);

    const loadGateways = async () => {
        try {
            const result = await paymentsApi.getActiveGateways();
            // apiClient interceptor unwraps ApiResult, so result is: Array<PaymentGatewayDto>
            const gatewaysList = Array.isArray(result) ? result : [];
            setGateways(gatewaysList);
            
            // اگر Mock Gateway وجود دارد، به صورت پیش‌فرض انتخاب می‌کنیم
            const mockGateway = gatewaysList.find(g => 
                g.code === "Mock" || 
                g.Code === "Mock" || 
                g.title?.toLowerCase().includes("mock") ||
                g.Title?.toLowerCase().includes("mock")
            );
            if (mockGateway) {
                form.setFieldsValue({ gatewayId: mockGateway.id || mockGateway.Id });
            }
        } catch (error) {
            message.error("خطا در دریافت درگاه‌های پرداخت");
            console.error(error);
        }
    };

    const handleSubmit = async (values) => {
        try {
            setLoading(true);
            // تبدیل تومان به ریال
            const amountRial = (values.amountToman || 0) * 10;
            const result = await paymentsApi.createTopUpRequest({
                amountRial: amountRial,
                gatewayId: values.gatewayId,
            });

            const paymentId = result?.paymentId || result?.PaymentId;
            const gatewayCode = result?.gatewayCode || result?.GatewayCode;
            const paymentToken = result?.paymentToken || result?.PaymentToken;
            const redirectUrl = result?.redirectUrl || result?.RedirectUrl;

            if (paymentId) {
                setPaymentId(paymentId);
                setCurrentStep(1);

                // برای Mock Gateway، مستقیماً تایید می‌کنیم (بدون timeout)
                if (gatewayCode === "Mock") {
                    // مستقیماً verify می‌کنیم (انگار که به درگاه رفته و پرداخت موفق شده)
                    await verifyMockPayment(paymentId, paymentToken);
                } else {
                    // برای درگاه‌های واقعی، به صفحه پرداخت هدایت می‌شود
                    if (redirectUrl) {
                        window.location.href = redirectUrl;
                    } else {
                        // اگر redirectUrl نبود، برای تست Mock می‌کنیم
                        message.warning("درگاه واقعی در دسترس نیست. در حال استفاده از Mock Gateway...");
                        await verifyMockPayment(paymentId, paymentToken);
                    }
                }
            } else {
                message.error("خطا در ایجاد درخواست پرداخت");
            }
        } catch (error) {
            message.error(
                error?.response?.data?.message || "خطا در ایجاد درخواست پرداخت"
            );
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const verifyMockPayment = async (paymentId, paymentToken) => {
        try {
            setCheckingStatus(true);
            const result = await paymentsApi.verifyPayment(paymentId, {
                gatewayTransactionId: `MOCK-${paymentToken}`,
            });

            if (result?.success) {
                setPaymentStatus(result);
                setCurrentStep(2);
                const amountToman = form.getFieldValue("amountToman") || 0;
                message.success(
                    `پرداخت با موفقیت انجام شد! مبلغ: ${amountToman.toLocaleString("fa-IR")} تومان - موجودی جدید: ${result.newBalanceToman?.toLocaleString("fa-IR")} تومان`
                );
                
                // به‌روزرسانی موجودی کیف پول در header
                window.dispatchEvent(new CustomEvent('walletBalanceChanged'));
            } else {
                message.error(result?.errorMessage || "خطا در تایید پرداخت");
            }
        } catch (error) {
            message.error("خطا در تایید پرداخت");
            console.error(error);
        } finally {
            setCheckingStatus(false);
        }
    };

    const formatPrice = (rial) => {
        if (rial == null) return "-";
        const toman = rial / 10;
        return `${toman.toLocaleString("fa-IR")} تومان`;
    };

    return (
        <div style={{ padding: "24px", maxWidth: "800px", margin: "0 auto" }}>
            <Card>
                <Space direction="vertical" size="large" style={{ width: "100%" }}>
                    <h2 style={{ margin: 0 }}>
                        <WalletOutlined /> شارژ کیف پول
                    </h2>

                    <Steps current={currentStep}>
                        <Step title="اطلاعات پرداخت" />
                        <Step title="در حال پردازش" />
                        <Step title="تکمیل شده" />
                    </Steps>

                    {currentStep === 0 && (
                        <Form
                            form={form}
                            layout="vertical"
                            onFinish={handleSubmit}
                        >
                            <Form.Item
                                label="مبلغ شارژ (تومان)"
                                name="amountToman"
                                rules={[
                                    { required: true, message: "لطفاً مبلغ را وارد کنید" },
                                    {
                                        type: "number",
                                        min: 10000,
                                        message: "حداقل مبلغ شارژ 10,000 تومان است",
                                    },
                                ]}
                            >
                                <InputNumber
                                    style={{ width: "100%" }}
                                    placeholder="مبلغ را به تومان وارد کنید"
                                    formatter={(value) =>
                                        `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                                    }
                                    parser={(value) => value.replace(/\$\s?|(,*)/g, "")}
                                />
                            </Form.Item>

                            <Form.Item
                                label="درگاه پرداخت"
                                name="gatewayId"
                                rules={[{ required: true, message: "لطفاً درگاه پرداخت را انتخاب کنید" }]}
                            >
                                <Select placeholder="درگاه پرداخت را انتخاب کنید">
                                    {gateways.map((gateway) => {
                                        const gatewayId = gateway.id || gateway.Id;
                                        const gatewayCode = gateway.code || gateway.Code;
                                        const gatewayTitle = gateway.title || gateway.Title;
                                        return (
                                            <Select.Option key={gatewayId} value={gatewayId}>
                                                {gatewayTitle} {gatewayCode === "Mock" ? "(تست)" : ""}
                                            </Select.Option>
                                        );
                                    })}
                                </Select>
                            </Form.Item>
                            
                            {/* توضیح برای Mock Gateway */}
                            {gateways.some(g => (g.code || g.Code) === "Mock") && (
                                <div style={{ 
                                    padding: "12px", 
                                    background: "#e6f7ff", 
                                    borderRadius: "4px",
                                    border: "1px solid #91d5ff",
                                    marginBottom: "16px"
                                }}>
                                    <span style={{ fontSize: "12px", color: "#1890ff" }}>
                                        💡 <strong>نکته:</strong> درگاه Mock برای تست استفاده می‌شود و پرداخت به صورت خودکار انجام می‌شود.
                                    </span>
                                </div>
                            )}

                            <Form.Item>
                                <Button type="primary" htmlType="submit" loading={loading} block>
                                    ادامه پرداخت
                                </Button>
                            </Form.Item>
                        </Form>
                    )}

                    {currentStep === 1 && (
                        <div style={{ textAlign: "center", padding: "40px 0" }}>
                            <Spin size="large" />
                            <p style={{ marginTop: "16px" }}>
                                {checkingStatus
                                    ? "در حال تایید پرداخت..."
                                    : "در حال پردازش پرداخت..."}
                            </p>
                        </div>
                    )}

                    {currentStep === 2 && paymentStatus && (
                        <div>
                            <Space direction="vertical" size="large" style={{ width: "100%" }}>
                                <div style={{ textAlign: "center" }}>
                                    <CheckCircleOutlined
                                        style={{ fontSize: "64px", color: "#52c41a" }}
                                    />
                                    <h3 style={{ marginTop: "16px" }}>پرداخت با موفقیت انجام شد</h3>
                                </div>
                                <Descriptions bordered column={1}>
                                    <Descriptions.Item label="مبلغ پرداخت شده">
                                        {(() => {
                                            const amountToman = form.getFieldValue("amountToman") || 0;
                                            return formatPrice(amountToman * 10);
                                        })()}
                                    </Descriptions.Item>
                                    <Descriptions.Item label="موجودی جدید">
                                        <span style={{ fontSize: "18px", fontWeight: "bold", color: "#1890ff" }}>
                                            {formatPrice(paymentStatus.newBalanceRial)}
                                        </span>
                                    </Descriptions.Item>
                                </Descriptions>
                                <Button
                                    type="primary"
                                    block
                                    onClick={() => {
                                        navigate("/wallet");
                                        window.location.reload();
                                    }}
                                >
                                    بازگشت به کیف پول
                                </Button>
                            </Space>
                        </div>
                    )}
                </Space>
            </Card>
        </div>
    );
};

export default TopUpPage;

