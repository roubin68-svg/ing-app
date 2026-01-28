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
} from "antd";
import {
    WalletOutlined,
    ArrowDownOutlined,
    ArrowUpOutlined,
    ArrowLeftOutlined,
} from "@ant-design/icons";
import { useNavigate, useParams } from "react-router-dom";
import walletManagementApi from "../api/walletManagementApi";
import userApi from "../../users/api/userApi";
import jalaali from "jalaali-js";

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

    const formatPrice = (rial) => {
        if (rial == null) return "-";
        const toman = rial / 10;
        return `${toman.toLocaleString("fa-IR")} تومان`;
    };

    // تبدیل تاریخ میلادی به شمسی با ساعت
    const toShamsiWithTime = (gregorian) => {
        if (!gregorian) return { date: "-", time: "" };

        let d;
        if (typeof gregorian === "string") {
            d = new Date(gregorian);
        } else if (gregorian instanceof Date) {
            d = gregorian;
        } else {
            return { date: "-", time: "" };
        }

        const y = d.getFullYear();
        const m = d.getMonth() + 1;
        const day = d.getDate();
        const h = d.getHours();
        const mi = d.getMinutes();

        const j = jalaali.toJalaali(y, m, day);
        const dateStr = `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(
            j.jd
        ).padStart(2, "0")}`;
        const timeStr = `${String(h).padStart(2, "0")}:${String(mi).padStart(2, "0")}`;

        return { date: dateStr, time: timeStr };
    };

    const getDirectionColor = (code) => {
        if (code === "Credit" || code === "AdminCredit") return "green";
        if (code === "Debit" || code === "AdminDebit") return "red";
        return "default";
    };

    const getDirectionText = (code) => {
        if (code === "Credit") return "واریز";
        if (code === "Debit") return "برداشت";
        if (code === "AdminCredit") return "واریز دستی مدیر";
        if (code === "AdminDebit") return "برداشت دستی مدیر";
        return code;
    };

    const txColumns = [
        {
            title: "تاریخ",
            dataIndex: "createdAt",
            key: "createdAt",
            render: (date) => {
                const { date: d, time } = toShamsiWithTime(date);
                return (
                    <div style={{ display: "flex", flexDirection: "column" }}>
                        <span>{d}</span>
                        {time && <span style={{ fontSize: 12, color: "#888" }}>{time}</span>}
                    </div>
                );
            },
        },
        {
            title: "نوع تراکنش",
            dataIndex: "directionCode",
            key: "directionCode",
            render: (code) => (
                <Tag color={getDirectionColor(code)}>{getDirectionText(code)}</Tag>
            ),
        },
        {
            title: "مبلغ",
            dataIndex: "amountRial",
            key: "amountRial",
            render: (amount) => formatPrice(amount),
        },
        {
            title: "نوع عملیات",
            dataIndex: "operationTypeTitle",
            key: "operationTypeTitle",
        },
        {
            title: "مرجع",
            dataIndex: "referenceTypeTitle",
            key: "referenceTypeTitle",
            render: (text) => text || "-",
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
                amountRial: values.amountRial,
                description: values.description || "واریز دستی توسط مدیر",
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
                amountRial: values.amountRial,
                description: values.description || "برداشت دستی توسط مدیر",
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
                                        </Space>
                                    }
                                >
                                    <Form
                                        form={depositForm}
                                        layout="vertical"
                                        onFinish={handleDeposit}
                                    >
                                        <Form.Item
                                            label="مبلغ (ریال)"
                                            name="amountRial"
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
                                        </Space>
                                    }
                                >
                                    <Form
                                        form={withdrawForm}
                                        layout="vertical"
                                        onFinish={handleWithdraw}
                                    >
                                        <Form.Item
                                            label="مبلغ (ریال)"
                                            name="amountRial"
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
                    pagination={{
                        ...pagination,
                        showSizeAdjuster: true,
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


