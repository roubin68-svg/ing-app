// src/features/walletManagement/pages/WalletManagementPage.jsx
import React, { useEffect, useState } from "react";
import {
    Card,
    Descriptions,
    Table,
    Tag,
    Space,
    Button,
    Row,
    Col,
    InputNumber,
    Input,
    Form,
    message,
    Typography,
    Checkbox,
    Tooltip,
} from "antd";
import {
    WalletOutlined,
    ArrowDownOutlined,
    ArrowUpOutlined,
    ArrowLeftOutlined,
    QuestionCircleOutlined,
} from "@ant-design/icons";
import { useNavigate, useParams } from "react-router-dom";
import walletManagementApi from "../api/walletManagementApi";
import userApi from "../../users/api/userApi";
import { toShamsiDateTimeString } from "../../../core/utils/dateUtils";

const { Title, Text } = Typography;

const WalletManagementPage = () => {
    const navigate = useNavigate();
    const { userId } = useParams();

    const [loadingBalance, setLoadingBalance] = useState(false);
    const [loadingTx, setLoadingTx] = useState(false);
    const [balance, setBalance] = useState(null);
    const [user, setUser] = useState(null);
    const [transactions, setTransactions] = useState([]);
    const [pagination, setPagination] = useState({
        current: 1,
        pageSize: 20,
        total: 0,
    });

    const [depositForm] = Form.useForm();
    const [withdrawForm] = Form.useForm();
    const [submittingDeposit, setSubmittingDeposit] = useState(false);
    const [submittingWithdraw, setSubmittingWithdraw] = useState(false);

    useEffect(() => {
        if (!userId) return;
        loadUser();
        loadBalance();
        loadTransactions();
    }, [userId]);

    const loadUser = async () => {
        try {
            const res = await userApi.getById(userId);
            setUser(res);
        } catch (error) {
            console.error(error);
            message.error("خطا در دریافت اطلاعات کاربر");
        }
    };

    const loadBalance = async () => {
        try {
            setLoadingBalance(true);
            const res = await walletManagementApi.getUserBalance(userId);
            // ApiResult unwrap شده، انتظار: { balanceRial, walletId, ... }
            setBalance(res);
        } catch (error) {
            console.error(error);
            message.error("خطا در دریافت موجودی کیف پول کاربر");
        } finally {
            setLoadingBalance(false);
        }
    };

    const loadTransactions = async (page = 1, pageSize = 20) => {
        try {
            setLoadingTx(true);
            const res = await walletManagementApi.getUserTransactions(userId, {
                page,
                pageSize,
            });
            // انتظار: { items, page, pageSize, totalCount }
            setTransactions(res?.items || []);
            setPagination({
                current: res?.page || page,
                pageSize: res?.pageSize || pageSize,
                total: res?.totalCount || 0,
            });
        } catch (error) {
            console.error(error);
            message.error("خطا در دریافت تراکنش‌های کیف پول کاربر");
        } finally {
            setLoadingTx(false);
        }
    };

    // نمایش مبلغ به تومان در UI (ورودی به ریال است)
    const formatPrice = (rial) => {
        if (rial == null) return "0";
        const toman = rial / 10;
        return toman.toLocaleString("fa-IR");
    };

    const txColumns = [
        {
            title: "تاریخ",
            dataIndex: "createdAt",
            key: "createdAt",
            width: 150,
            render: (value) => {
                const { date, time } = toShamsiDateTimeString(value);
                return (
                    <div>
                        <div>{date}</div>
                        <div style={{ fontSize: "12px", color: "#999" }}>
                            {time}
                        </div>
                    </div>
                );
            },
        },
        {
            title: "جهت",
            dataIndex: "directionCode",
            key: "directionCode",
            width: 120,
            render: (code) => {
                if (code === "Credit") {
                    return (
                        <Tag color="green" icon={<ArrowDownOutlined />}>
                            واریز
                        </Tag>
                    );
                } else if (code === "Debit") {
                    return (
                        <Tag color="red" icon={<ArrowUpOutlined />}>
                            برداشت
                        </Tag>
                    );
                }
                return code;
            },
        },
        {
            title: "نوع عملیات",
            dataIndex: "operationTypeTitle",
            key: "operationTypeTitle",
        },
        {
            title: "وضعیت",
            dataIndex: "statusCode",
            key: "statusCode",
            width: 120,
            render: (code, record) => {
                const colorMap = {
                    Committed: "green",
                    Pending: "orange",
                    Failed: "red",
                    Reversed: "default",
                };
                return (
                    <Tag color={colorMap[code] || "default"}>
                        {record.statusTitle}
                    </Tag>
                );
            },
        },
        {
            title: "منبع",
            dataIndex: "sourceCategory",
            key: "sourceCategory",
            render: (value) => {
                switch (value) {
                    case "Bank":
                        return <Tag color="green">بانکی</Tag>;
                    case "Commission":
                        return <Tag color="blue">پورسانت</Tag>;
                    case "Manual":
                        return <Tag color="orange">عملیات دستی</Tag>;
                    default:
                        return <Tag color="default">سایر</Tag>;
                }
            },
        },
        {
            title: "مبلغ (تومان)",
            dataIndex: "amountRial",
            key: "amountRial",
            align: "right",
            width: 150,
            render: (value, record) => {
                const formatted = formatPrice(value);
                const color = record.directionCode === "Credit" ? "#52c41a" : "#f5222d";
                return (
                    <span style={{ color, fontWeight: "bold", direction: "ltr", display: "block" }}>
                        {formatted}
                    </span>
                );
            },
        },
        {
            title: "توضیحات",
            dataIndex: "description",
            key: "description",
            render: (text) => text || "-",
        },
    ];

    const handleDeposit = async (values) => {
        try {
            setSubmittingDeposit(true);
            await walletManagementApi.manualDeposit(userId, {
                // مبلغ در فرم به «تومان» وارد می‌شود؛ برای API به ریال تبدیل می‌کنیم
                amountRial: values.amountToman * 10,
                description: values.description || "واریز دستی توسط مدیر",
                isBankSettlement: values.isBankSettlement || false,
            });
            message.success("واریز دستی با موفقیت انجام شد");
            depositForm.resetFields();
            await Promise.all([loadBalance(), loadTransactions(pagination.current, pagination.pageSize)]);
        } catch (error) {
            console.error(error);
            const msg =
                error?.response?.data?.message ||
                error?.response?.data?.errors?.[0] ||
                error?.message ||
                "خطا در واریز دستی";
            message.error(msg);
        } finally {
            setSubmittingDeposit(false);
        }
    };

    const handleWithdraw = async (values) => {
        try {
            setSubmittingWithdraw(true);
            await walletManagementApi.manualWithdrawal(userId, {
                // مبلغ در فرم به «تومان» وارد می‌شود؛ برای API به ریال تبدیل می‌کنیم
                amountRial: values.amountToman * 10,
                description: values.description || "برداشت دستی توسط مدیر",
                isBankSettlement: values.isBankSettlement || false,
            });
            message.success("برداشت دستی با موفقیت انجام شد");
            withdrawForm.resetFields();
            await Promise.all([loadBalance(), loadTransactions(pagination.current, pagination.pageSize)]);
        } catch (error) {
            console.error(error);
            const msg =
                error?.response?.data?.message ||
                error?.response?.data?.errors?.[0] ||
                error?.message ||
                "خطا در برداشت دستی";
            message.error(msg);
        } finally {
            setSubmittingWithdraw(false);
        }
    };

    return (
        <div>
            <Button
                icon={<ArrowLeftOutlined />}
                style={{ marginBottom: 16 }}
                onClick={() => navigate(-1)}
            >
                بازگشت
            </Button>

            <Card
                title={
                    <Space>
                        <WalletOutlined />
                        <span>مدیریت کیف پول کاربر</span>
                    </Space>
                }
            >
                <Row gutter={24}>
                    <Col xs={24} md={10}>
                        <div style={{ marginBottom: 24 }}>
                            <Title level={5} style={{ marginTop: 0 }}>موجودی کیف پول</Title>
                            <Card
                                loading={loadingBalance}
                                style={{ background: "#f6ffed", borderColor: "#b7eb8f" }}
                            >
                                <Space direction="vertical">
                                    <Text type="secondary">موجودی فعلی</Text>
                                    <Title level={3} style={{ margin: 0, color: "#52c41a" }}>
                                        {balance ? formatPrice(balance.balanceRial) : "-"}
                                    </Title>
                                </Space>
                            </Card>
                        </div>

                        <Title level={5}>اطلاعات کاربر</Title>
                        <Descriptions bordered size="small" column={1}>
                            <Descriptions.Item label="شماره موبایل">
                                {user?.phoneNumber || "-"}
                            </Descriptions.Item>
                            <Descriptions.Item label="نام">
                                {user?.displayName || "-"}
                            </Descriptions.Item>
                            <Descriptions.Item label="نوع کاربر">
                                {user?.userTypeName || "-"}
                            </Descriptions.Item>
                        </Descriptions>
                    </Col>

                    <Col xs={24} md={14}>
                        <Row gutter={16}>
                            <Col span={12}>
                                <Card
                                    title={
                                        <Space>
                                            <ArrowDownOutlined style={{ color: "#52c41a" }} />
                                            <span>واریز دستی (افزایش موجودی)</span>
                                            <Tooltip
                                                placement="top"
                                                title={
                                                    <div>
                                                        <p style={{ marginBottom: 4 }}>
                                                            از این فرم برای <strong>افزایش موجودی کیف پول کاربر</strong> استفاده کنید.
                                                        </p>
                                                        <p style={{ marginBottom: 4 }}>
                                                            اگر کاربر به هر دلیلی باید اعتباری داخل سیستم بگیرد (مثلاً پاداش، اصلاح اشتباه قبلی، یا
                                                            ثبت شارژی که از قبل انجام شده)، مبلغ را به <strong>تومان</strong> وارد کنید.
                                                        </p>
                                                        <p style={{ marginBottom: 0 }}>
                                                            اگر این اعتبار به خاطر یک <strong>تراکنش بانکی واقعی</strong> است (مثلاً کارت‌به‌کارت
                                                            به حساب شما)، حتماً تیک «این واریز یک تراکنش بانکی است» را فعال کنید تا در
                                                            گزارش‌های مالی به‌درستی تفکیک شود.
                                                        </p>
                                                    </div>
                                                }
                                            >
                                                <QuestionCircleOutlined style={{ color: "#999" }} />
                                            </Tooltip>
                                        </Space>
                                    }
                                >
                                    <Form
                                        form={depositForm}
                                        layout="vertical"
                                        onFinish={handleDeposit}
                                    >
                                        <Form.Item
                                            label="مبلغ (تومان)"
                                            name="amountToman"
                                            rules={[
                                                {
                                                    required: true,
                                                    message: "لطفاً مبلغ واریز را وارد کنید",
                                                },
                                                {
                                                    type: "number",
                                                    min: 1,
                                                    message: "مبلغ باید بزرگ‌تر از صفر باشد",
                                                },
                                            ]}
                                        >
                                            <InputNumber
                                                style={{ width: "100%" }}
                                                min={1}
                                                step={1000}
                                                formatter={(value) =>
                                                    value
                                                        ? `${value}`.replace(
                                                              /\B(?=(\d{3})+(?!\d))/g,
                                                              ","
                                                          )
                                                        : ""
                                                }
                                                parser={(value) =>
                                                    value ? value.replace(/[^\d]/g, "") : ""
                                                }
                                            />
                                        </Form.Item>
                                        <Form.Item label="توضیحات" name="description">
                                            <Input.TextArea
                                                rows={2}
                                                placeholder="مثال: واریز پاداش ویژه / اصلاح موجودی"
                                            />
                                        </Form.Item>
                                        <Form.Item
                                            name="isBankSettlement"
                                            valuePropName="checked"
                                            tooltip="اگر این گزینه را فعال کنید یعنی این واریز واقعاً بابت دریافت پول از کاربر (مثلاً کارت‌به‌کارت یا واریز بانکی به حساب ما) انجام شده است."
                                        >
                                            <Checkbox>
                                                این <strong>واریز</strong> یک <strong>تراکنش بانکی</strong> است
                                            </Checkbox>
                                        </Form.Item>
                                        <Form.Item>
                                            <Button
                                                type="primary"
                                                htmlType="submit"
                                                loading={submittingDeposit}
                                                block
                                            >
                                                ثبت واریز
                                            </Button>
                                        </Form.Item>
                                    </Form>
                                </Card>
                            </Col>

                            <Col span={12}>
                                <Card
                                    title={
                                        <Space>
                                            <ArrowUpOutlined style={{ color: "#f5222d" }} />
                                            <span>برداشت دستی (کاهش موجودی)</span>
                                            <Tooltip
                                                placement="top"
                                                title={
                                                    <div>
                                                        <p style={{ marginBottom: 4 }}>
                                                            از این فرم برای <strong>کاهش موجودی کیف پول کاربر</strong> استفاده کنید.
                                                        </p>
                                                        <p style={{ marginBottom: 4 }}>
                                                            اگر کاربر باید پولی از اعتبارش برگردانده شود (مثلاً اصلاح اشتباه، لغو خدمت، یا تسویه)،
                                                            مبلغ را به <strong>تومان</strong> وارد کنید.
                                                        </p>
                                                        <p style={{ marginBottom: 0 }}>
                                                            اگر واقعاً پول را به حساب بانکی کاربر واریز کرده‌اید (مثلاً کارت‌به‌کارت)، حتماً تیک
                                                            «این برداشت یک تراکنش بانکی است» را فعال کنید تا این برداشت در گزارش‌ها به‌عنوان
                                                            <strong>پرداخت واقعی به کاربر</strong> مشخص شود.
                                                        </p>
                                                    </div>
                                                }
                                            >
                                                <QuestionCircleOutlined style={{ color: "#999" }} />
                                            </Tooltip>
                                        </Space>
                                    }
                                >
                                    <Form
                                        form={withdrawForm}
                                        layout="vertical"
                                        onFinish={handleWithdraw}
                                    >
                                        <Form.Item
                                            label="مبلغ (تومان)"
                                            name="amountToman"
                                            rules={[
                                                {
                                                    required: true,
                                                    message: "لطفاً مبلغ برداشت را وارد کنید",
                                                },
                                                {
                                                    type: "number",
                                                    min: 1,
                                                    message: "مبلغ باید بزرگ‌تر از صفر باشد",
                                                },
                                            ]}
                                        >
                                            <InputNumber
                                                style={{ width: "100%" }}
                                                min={1}
                                                step={1000}
                                                formatter={(value) =>
                                                    value
                                                        ? `${value}`.replace(
                                                              /\B(?=(\d{3})+(?!\d))/g,
                                                              ","
                                                          )
                                                        : ""
                                                }
                                                parser={(value) =>
                                                    value ? value.replace(/[^\d]/g, "") : ""
                                                }
                                            />
                                        </Form.Item>
                                        <Form.Item label="توضیحات" name="description">
                                            <Input.TextArea
                                                rows={2}
                                                placeholder="مثال: تسویه حساب / اصلاح مانده"
                                            />
                                        </Form.Item>
                                        <Form.Item
                                            name="isBankSettlement"
                                            valuePropName="checked"
                                            tooltip="اگر این گزینه را فعال کنید یعنی این برداشت واقعاً بابت پرداخت پول به حساب کاربر (مثلاً کارت‌به‌کارت یا واریز بانکی) انجام شده است."
                                        >
                                            <Checkbox>
                                                این <strong>برداشت</strong> یک <strong>تراکنش بانکی</strong> است
                                            </Checkbox>
                                        </Form.Item>
                                        <Form.Item>
                                            <Button
                                                type="primary"
                                                danger
                                                htmlType="submit"
                                                loading={submittingWithdraw}
                                                block
                                            >
                                                ثبت برداشت
                                            </Button>
                                        </Form.Item>
                                    </Form>
                                </Card>
                            </Col>
                        </Row>
                    </Col>
                </Row>
            </Card>

            <Card
                title="تاریخچه تراکنش‌های کیف پول"
                style={{ marginTop: 24 }}
            >
                <Table
                    columns={txColumns}
                    dataSource={transactions}
                    loading={loadingTx}
                    rowKey="id"
                    scroll={{ x: 1200 }}
                    pagination={{
                        current: pagination.current,
                        pageSize: pagination.pageSize,
                        total: pagination.total,
                        showSizeChanger: true,
                        showTotal: (total) =>
                            `مجموع: ${total.toLocaleString("fa-IR")} تراکنش`,
                        pageSizeOptions: ["10", "20", "50", "100"],
                        onChange: (page, pageSize) => {
                            loadTransactions(page, pageSize);
                        },
                    }}
                />
            </Card>
        </div>
    );
};

export default WalletManagementPage;


