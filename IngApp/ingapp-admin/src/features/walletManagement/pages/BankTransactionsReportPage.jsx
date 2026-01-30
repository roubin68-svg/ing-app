// src/features/walletManagement/pages/BankTransactionsReportPage.jsx
import React, { useCallback, useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import {
    Card,
    Table,
    Form,
    Input,
    Button,
    Space,
    Select,
    Statistic,
    Row,
    Col,
    message,
    Tag,
    Typography,
} from "antd";
import {
    ArrowUpOutlined,
    ArrowDownOutlined,
    SearchOutlined,
    ReloadOutlined,
    BankOutlined,
} from "@ant-design/icons";
import { DatePicker as JalaliDatePicker } from "antd-jalali";
import { toGregorianISO, toShamsiString, ensureShamsiDayjs, todayShamsi, toShamsiDateTimeString } from "../../../core/utils/dateUtils";
import walletManagementApi from "../api/walletManagementApi";

const { Option } = Select;
const { Title } = Typography;

// نمایش مبلغ به تومان در UI (ورودی به ریال است)
const formatPrice = (rial) => {
    if (rial == null) return "0";
    const toman = rial / 10;
    return toman.toLocaleString("fa-IR");
};

const BankTransactionsReportPage = () => {
    const [searchParams] = useSearchParams();
    const [form] = Form.useForm();

    const [loading, setLoading] = useState(false);
    const [data, setData] = useState([]);
    const [pagination, setPagination] = useState({
        current: 1,
        pageSize: 20,
        total: 0,
    });
    const [summary, setSummary] = useState({
        totalCreditRial: 0,
        totalDebitRial: 0,
        creditCount: 0,
        debitCount: 0,
    });

    // خواندن فیلترها از URL در بارگذاری اولیه
    const getInitialFilters = () => {
        const directionCode = searchParams.get("directionCode") || undefined;
        
        return {
            phoneNumber: "",
            displayName: "",
            directionCode: directionCode,
            fromDate: null,
            toDate: null,
        };
    };

    const [filters, setFilters] = useState(getInitialFilters());

    const loadData = useCallback(
        async (page = 1, pageSize = 20, currentFilters = filters) => {
            setLoading(true);
            try {
                const params = {
                    page,
                    pageSize,
                    phoneNumber: currentFilters.phoneNumber || undefined,
                    displayName: currentFilters.displayName || undefined,
                    directionCode: currentFilters.directionCode || undefined,
                    sourceCategory: "Bank", // فقط تراکنش‌های بانکی
                    fromDate: currentFilters.fromDate
                        ? toGregorianISO(currentFilters.fromDate)
                        : undefined,
                    toDate: currentFilters.toDate
                        ? toGregorianISO(currentFilters.toDate)
                        : undefined,
                };

                const report = await walletManagementApi.getAllTransactions(params);

                const paged = report.transactions;

                setData(paged.items || []);
                setPagination({
                    current: paged.page,
                    pageSize: paged.pageSize,
                    total: paged.totalCount,
                });

                // استفاده از خلاصه API برای کل تراکنش‌های بانکی (نه فقط صفحه فعلی)
                // برای محاسبه دقیق‌تر، باید از totalCreditRial و totalDebitRial استفاده کنیم
                // اما چون API فقط برای صفحه فعلی خلاصه می‌دهد، از items استفاده می‌کنیم
                const creditItems = (paged.items || []).filter(t => t.directionCode === "Credit");
                const debitItems = (paged.items || []).filter(t => t.directionCode === "Debit");
                
                // برای نمایش دقیق‌تر، می‌توانیم از report.totalCreditRial و report.totalDebitRial استفاده کنیم
                // اما این برای همه تراکنش‌هاست، نه فقط بانکی. پس فعلاً از items استفاده می‌کنیم
                setSummary({
                    totalCreditRial: creditItems.reduce((sum, t) => sum + (t.amountRial || 0), 0),
                    totalDebitRial: debitItems.reduce((sum, t) => sum + (t.amountRial || 0), 0),
                    creditCount: creditItems.length,
                    debitCount: debitItems.length,
                });
            } catch (error) {
                console.error("Error loading bank transactions report", error);
                const msg =
                    error?.message ||
                    error?.response?.data?.message ||
                    "خطا در دریافت گزارش تراکنش‌های بانکی";
                message.error(msg);
            } finally {
                setLoading(false);
            }
        },
        [filters]
    );

    useEffect(() => {
        // بارگذاری اولیه با فیلترهای URL
        const initialFilters = getInitialFilters();
        setFilters(initialFilters);
        
        // تنظیم فرم با فیلترهای URL
        if (initialFilters.directionCode) {
            form.setFieldsValue({ directionCode: initialFilters.directionCode });
        }
        
        loadData(1, pagination.pageSize, initialFilters);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const handleTableChange = (paginationConfig) => {
        const { current, pageSize } = paginationConfig;
        setPagination((prev) => ({ ...prev, current, pageSize }));
        loadData(current, pageSize, filters);
    };

    const handleSearch = () => {
        const values = form.getFieldsValue();
        const newFilters = {
            phoneNumber: values.phoneNumber || "",
            displayName: values.displayName || "",
            directionCode: values.directionCode || undefined,
            fromDate: values.fromDate || null,
            toDate: values.toDate || null,
        };
        setFilters(newFilters);
        loadData(1, pagination.pageSize, newFilters);
    };

    const handleReset = () => {
        form.resetFields();
        const newFilters = {
            phoneNumber: "",
            displayName: "",
            directionCode: undefined,
            fromDate: null,
            toDate: null,
        };
        setFilters(newFilters);
        loadData(1, pagination.pageSize, newFilters);
    };

    const directionOptions = [
        { value: undefined, label: "همه" },
        { value: "Credit", label: "واریز به حساب" },
        { value: "Debit", label: "برداشت از حساب" },
    ];

    const columns = [
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
            title: "شماره موبایل",
            dataIndex: "phoneNumber",
            key: "phoneNumber",
            width: 120,
        },
        {
            title: "نام کاربر",
            dataIndex: "displayName",
            key: "displayName",
            render: (text) => text || "-",
        },
        {
            title: "نوع",
            dataIndex: "directionCode",
            key: "directionCode",
            width: 120,
            render: (value) => {
                if (value === "Credit") {
                    return (
                        <Tag color="green" icon={<ArrowDownOutlined />}>
                            واریز
                        </Tag>
                    );
                } else if (value === "Debit") {
                    return (
                        <Tag color="red" icon={<ArrowUpOutlined />}>
                            برداشت
                        </Tag>
                    );
                }
                return "-";
            },
        },
        {
            title: "نوع عملیات",
            dataIndex: "operationTypeTitle",
            key: "operationTypeTitle",
            width: 150,
        },
        {
            title: "منبع",
            dataIndex: "sourceCategory",
            key: "sourceCategory",
            width: 120,
            render: (value) => {
                if (value === "Bank") {
                    return <Tag color="blue">بانکی</Tag>;
                }
                return value || "-";
            },
        },
        {
            title: "مبلغ (تومان)",
            dataIndex: "amountRial",
            key: "amountRial",
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
        },
    ];

    const netAmount = summary.totalCreditRial - summary.totalDebitRial;

    return (
        <div>

            {/* فیلترها */}
            <Card
                title={
                    <Space>
                        <span>گزارش تراکنش‌های بانکی</span>
                    </Space>
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
                            <Form.Item name="phoneNumber" label="شماره موبایل">
                                <Input placeholder="مثلاً 0912..." allowClear />
                            </Form.Item>
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Form.Item name="displayName" label="نام کاربر">
                                <Input placeholder="نام کاربر" allowClear />
                            </Form.Item>
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Form.Item name="directionCode" label="نوع تراکنش">
                                <Select allowClear placeholder="انتخاب نوع">
                                    {directionOptions.map((opt) =>
                                        opt.value ? (
                                            <Option key={opt.value} value={opt.value}>
                                                {opt.label}
                                            </Option>
                                        ) : null
                                    )}
                                </Select>
                            </Form.Item>
                        </Col>
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

            {/* خلاصه */}
            <Row gutter={16} style={{ marginBottom: 16 }}>
                <Col xs={24} sm={12} md={6}>
                    <Card>
                        <Statistic
                            title="مجموع واریزها (تومان)"
                            value={summary.totalCreditRial}
                            valueStyle={{ color: "#52c41a", direction: "ltr" }}
                            prefix={<ArrowDownOutlined />}
                            formatter={(value) => `${formatPrice(value)} تومان`}
                        />
                        <div style={{ marginTop: 8, fontSize: "12px", color: "#999" }}>
                            {summary.creditCount.toLocaleString("fa-IR")} تراکنش
                        </div>
                    </Card>
                </Col>
                <Col xs={24} sm={12} md={6}>
                    <Card>
                        <Statistic
                            title="مجموع برداشت‌ها (تومان)"
                            value={summary.totalDebitRial}
                            valueStyle={{ color: "#f5222d", direction: "ltr" }}
                            prefix={<ArrowUpOutlined />}
                            formatter={(value) => `${formatPrice(value)} تومان`}
                        />
                        <div style={{ marginTop: 8, fontSize: "12px", color: "#999" }}>
                            {summary.debitCount.toLocaleString("fa-IR")} تراکنش
                        </div>
                    </Card>
                </Col>
                <Col xs={24} sm={12} md={6}>
                    <Card>
                        <Statistic
                            title="سود/زیان خالص (تومان)"
                            value={netAmount}
                            valueStyle={{
                                color: netAmount >= 0 ? "#52c41a" : "#f5222d",
                                direction: "ltr",
                            }}
                            formatter={(value) => `${formatPrice(value)} تومان`}
                        />
                        <div style={{ marginTop: 8 }}>
                            {netAmount >= 0 ? (
                                <Tag color="green">سود</Tag>
                            ) : (
                                <Tag color="red">زیان</Tag>
                            )}
                        </div>
                    </Card>
                </Col>
                <Col xs={24} sm={12} md={6}>
                    <Card>
                        <Statistic
                            title="تعداد کل تراکنش‌ها"
                            value={pagination.total}
                            valueStyle={{ color: "#1890ff" }}
                            formatter={(value) => `${value.toLocaleString("fa-IR")} تراکنش`}
                        />
                    </Card>
                </Col>
            </Row>

            {/* جدول */}
            <Card title="لیست تراکنش‌های بانکی">
                <Table
                    rowKey="id"
                    columns={columns}
                    dataSource={data}
                    loading={loading}
                    pagination={{
                        current: pagination.current,
                        pageSize: pagination.pageSize,
                        total: pagination.total,
                        showSizeChanger: true,
                        showTotal: (total) => `${total.toLocaleString("fa-IR")} تراکنش`,
                    }}
                    onChange={handleTableChange}
                    scroll={{ x: 1200 }}
                />
            </Card>
        </div>
    );
};

export default BankTransactionsReportPage;

