// src/features/walletManagement/pages/IncomeExpenseReportPage.jsx
import React, { useCallback, useEffect, useState } from "react";
import {
    Card,
    Form,
    Button,
    Space,
    Row,
    Col,
    message,
    Statistic,
    Typography,
    Table,
    Tag,
} from "antd";
import {
    DollarOutlined,
    ArrowUpOutlined,
    ArrowDownOutlined,
    SearchOutlined,
    ReloadOutlined,
    RiseOutlined,
    FallOutlined,
} from "@ant-design/icons";
import { DatePicker as JalaliDatePicker } from "antd-jalali";
import { toGregorianISO, ensureShamsiDayjs, todayShamsi } from "../../../core/utils/dateUtils";
import walletManagementApi from "../api/walletManagementApi";

const { Title } = Typography;

// نمایش مبلغ به تومان در UI (ورودی به ریال است)
const formatPrice = (rial) => {
    if (rial == null) return "0";
    const toman = rial / 10;
    return toman.toLocaleString("fa-IR");
};

const IncomeExpenseReportPage = () => {
    const [form] = Form.useForm();

    const [loading, setLoading] = useState(false);
    const [report, setReport] = useState({
        totalIncomeRial: 0,
        totalExpenseRial: 0,
        netProfitRial: 0,
        incomeTransactionCount: 0,
        expenseTransactionCount: 0,
        incomeCategories: [],
        expenseCategories: [],
    });
    const [filters, setFilters] = useState({
        fromDate: null,
        toDate: null,
    });

    const loadData = useCallback(
        async (currentFilters = filters) => {
            setLoading(true);
            try {
                const params = {
                    fromDate: currentFilters.fromDate
                        ? toGregorianISO(currentFilters.fromDate)
                        : undefined,
                    toDate: currentFilters.toDate
                        ? toGregorianISO(currentFilters.toDate)
                        : undefined,
                };

                const data = await walletManagementApi.getIncomeExpenseReport(params);
                setReport(data);
            } catch (error) {
                console.error("Error loading income/expense report", error);
                const msg =
                    error?.message ||
                    error?.response?.data?.message ||
                    "خطا در دریافت گزارش درآمد/هزینه";
                message.error(msg);
            } finally {
                setLoading(false);
            }
        },
        [filters]
    );

    useEffect(() => {
        // بارگذاری اولیه بدون فیلتر تاریخ
        loadData(filters);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const handleSearch = () => {
        const values = form.getFieldsValue();
        const newFilters = {
            fromDate: values.fromDate || null,
            toDate: values.toDate || null,
        };
        setFilters(newFilters);
        loadData(newFilters);
    };

    const handleReset = () => {
        form.resetFields();
        const newFilters = {
            fromDate: null,
            toDate: null,
        };
        setFilters(newFilters);
        loadData(newFilters);
    };

    const incomeColumns = [
        {
            title: "دسته",
            dataIndex: "categoryName",
            key: "categoryName",
        },
        {
            title: "تعداد تراکنش",
            dataIndex: "transactionCount",
            key: "transactionCount",
            align: "center",
            render: (value) => value.toLocaleString("fa-IR"),
        },
        {
            title: "مبلغ (تومان)",
            dataIndex: "totalAmountRial",
            key: "totalAmountRial",
            align: "right",
            render: (value) => (
                <span style={{ color: "#52c41a", fontWeight: "bold", direction: "ltr", display: "block" }}>
                    {formatPrice(value)}
                </span>
            ),
        },
    ];

    const expenseColumns = [
        {
            title: "دسته",
            dataIndex: "categoryName",
            key: "categoryName",
        },
        {
            title: "تعداد تراکنش",
            dataIndex: "transactionCount",
            key: "transactionCount",
            align: "center",
            render: (value) => value.toLocaleString("fa-IR"),
        },
        {
            title: "مبلغ (تومان)",
            dataIndex: "totalAmountRial",
            key: "totalAmountRial",
            align: "right",
            render: (value) => (
                <span style={{ color: "#f5222d", fontWeight: "bold", direction: "ltr", display: "block" }}>
                    {formatPrice(value)}
                </span>
            ),
        },
    ];

    return (
        <div>
            {/* فیلترها */}
            <Card
                title={
                    <div>
                        <div>گزارش درآمد/هزینه</div>
                        <div style={{ fontSize: "12px", color: "#999", fontWeight: "normal", marginTop: 4 }}>
                            این گزارش فقط تراکنش‌های واقعی بانکی را نمایش می‌دهد.
                            تراکنش‌های داخلی (خرید اشتراک، باز کردن تماس) که از موجودی کیف پول کاربر انجام می‌شوند،
                            در این گزارش محاسبه نمی‌شوند.
                        </div>
                    </div>
                }
                style={{ marginBottom: 16 }}
                extra={
                    <Space>
                        <Button
                            type="primary"
                            icon={<SearchOutlined />}
                            onClick={handleSearch}
                        >
                            جستجو
                        </Button>
                        <Button
                            icon={<ReloadOutlined />}
                            onClick={handleReset}
                        >
                            پاکسازی
                        </Button>
                    </Space>
                }
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSearch}
                >
                    <Row gutter={16}>
                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Form.Item
                                name="fromDate"
                                label="از تاریخ"
                                getValueFromEvent={(date) => ensureShamsiDayjs(date)}
                            >
                                <JalaliDatePicker
                                    style={{ width: "100%" }}
                                    format="YYYY/MM/DD"
                                    placeholder="انتخاب تاریخ"
                                    defaultPickerValue={todayShamsi()}
                                />
                            </Form.Item>
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Form.Item
                                name="toDate"
                                label="تا تاریخ"
                                getValueFromEvent={(date) => ensureShamsiDayjs(date)}
                            >
                                <JalaliDatePicker
                                    style={{ width: "100%" }}
                                    format="YYYY/MM/DD"
                                    placeholder="انتخاب تاریخ"
                                    defaultPickerValue={todayShamsi()}
                                />
                            </Form.Item>
                        </Col>
                    </Row>
                </Form>
            </Card>

            {/* خلاصه آمار */}
            <Row gutter={16} style={{ marginBottom: 16 }}>
                <Col xs={24} sm={12} md={8} lg={8}>
                    <Card>
                        <Statistic
                            title={
                                <span style={{ fontSize: "16px", fontWeight: "bold" }}>
                                    مجموع درآمدها (تومان)
                                </span>
                            }
                            value={report.totalIncomeRial}
                            valueStyle={{
                                color: "#52c41a",
                                fontSize: "24px",
                                fontWeight: "bold",
                                direction: "ltr",
                            }}
                            prefix={<ArrowDownOutlined />}
                            formatter={(value) => `${formatPrice(value)} تومان`}
                        />
                        <div style={{ marginTop: 8, fontSize: "12px", color: "#999" }}>
                            {report.incomeTransactionCount.toLocaleString("fa-IR")} تراکنش
                        </div>
                    </Card>
                </Col>
                <Col xs={24} sm={12} md={8} lg={8}>
                    <Card>
                        <Statistic
                            title={
                                <span style={{ fontSize: "16px", fontWeight: "bold" }}>
                                    مجموع هزینه‌ها (تومان)
                                </span>
                            }
                            value={report.totalExpenseRial}
                            valueStyle={{
                                color: "#f5222d",
                                fontSize: "24px",
                                fontWeight: "bold",
                                direction: "ltr",
                            }}
                            prefix={<ArrowUpOutlined />}
                            formatter={(value) => `${formatPrice(value)} تومان`}
                        />
                        <div style={{ marginTop: 8, fontSize: "12px", color: "#999" }}>
                            {report.expenseTransactionCount.toLocaleString("fa-IR")} تراکنش
                        </div>
                    </Card>
                </Col>
                <Col xs={24} sm={12} md={8} lg={8}>
                    <Card>
                        <Statistic
                            title={
                                <span style={{ fontSize: "16px", fontWeight: "bold" }}>
                                    سود/زیان خالص (تومان)
                                </span>
                            }
                            value={report.netProfitRial}
                            valueStyle={{
                                color: report.netProfitRial >= 0 ? "#52c41a" : "#f5222d",
                                fontSize: "24px",
                                fontWeight: "bold",
                                direction: "ltr",
                            }}
                            prefix={report.netProfitRial >= 0 ? <RiseOutlined /> : <FallOutlined />}
                            formatter={(value) => `${formatPrice(value)} تومان`}
                        />
                        <div style={{ marginTop: 8 }}>
                            {report.netProfitRial >= 0 ? (
                                <Tag color="green">سود</Tag>
                            ) : (
                                <Tag color="red">زیان</Tag>
                            )}
                        </div>
                    </Card>
                </Col>
            </Row>

            {/* جزئیات درآمدها */}
            <Card
                title="جزئیات درآمدها"
                style={{ marginBottom: 16 }}
                loading={loading}
            >
                <Table
                    rowKey="categoryName"
                    columns={incomeColumns}
                    dataSource={report.incomeCategories}
                    pagination={false}
                    summary={(pageData) => {
                        const total = pageData.reduce((sum, item) => sum + item.totalAmountRial, 0);
                        const totalCount = pageData.reduce((sum, item) => sum + item.transactionCount, 0);
                        return (
                            <Table.Summary fixed>
                                <Table.Summary.Row>
                                    <Table.Summary.Cell index={0}>
                                        <strong>جمع کل</strong>
                                    </Table.Summary.Cell>
                                    <Table.Summary.Cell index={1} align="center">
                                        <strong>{totalCount.toLocaleString("fa-IR")}</strong>
                                    </Table.Summary.Cell>
                                    <Table.Summary.Cell index={2} align="right">
                                        <strong style={{ color: "#52c41a", direction: "ltr", display: "block" }}>
                                            {formatPrice(total)}
                                        </strong>
                                    </Table.Summary.Cell>
                                </Table.Summary.Row>
                            </Table.Summary>
                        );
                    }}
                />
            </Card>

            {/* جزئیات هزینه‌ها */}
            <Card
                title="جزئیات هزینه‌ها"
                loading={loading}
            >
                <Table
                    rowKey="categoryName"
                    columns={expenseColumns}
                    dataSource={report.expenseCategories}
                    pagination={false}
                    summary={(pageData) => {
                        const total = pageData.reduce((sum, item) => sum + item.totalAmountRial, 0);
                        const totalCount = pageData.reduce((sum, item) => sum + item.transactionCount, 0);
                        return (
                            <Table.Summary fixed>
                                <Table.Summary.Row>
                                    <Table.Summary.Cell index={0}>
                                        <strong>جمع کل</strong>
                                    </Table.Summary.Cell>
                                    <Table.Summary.Cell index={1} align="center">
                                        <strong>{totalCount.toLocaleString("fa-IR")}</strong>
                                    </Table.Summary.Cell>
                                    <Table.Summary.Cell index={2} align="right">
                                        <strong style={{ color: "#f5222d", direction: "ltr", display: "block" }}>
                                            {formatPrice(total)}
                                        </strong>
                                    </Table.Summary.Cell>
                                </Table.Summary.Row>
                            </Table.Summary>
                        );
                    }}
                />
            </Card>
        </div>
    );
};

export default IncomeExpenseReportPage;

