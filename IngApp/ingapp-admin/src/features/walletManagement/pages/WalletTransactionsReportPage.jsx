// src/features/walletManagement/pages/WalletTransactionsReportPage.jsx
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

const directionOptions = [
    { value: undefined, label: "همه" },
    { value: "Credit", label: "واریز به کیف پول" },
    { value: "Debit", label: "برداشت از کیف پول" },
];

const statusOptions = [
    { value: undefined, label: "همه" },
    { value: "Pending", label: "در انتظار" },
    { value: "Committed", label: "نهایی شده" },
    { value: "Failed", label: "ناموفق" },
    { value: "Reversed", label: "برگشت خورده" },
];

const sourceCategoryOptions = [
    { value: undefined, label: "همه منابع" },
    { value: "Bank", label: "تراکنش‌های بانکی (TopUp/Payment)" },
    { value: "Commission", label: "پورسانت‌ها (CommissionEarned)" },
    { value: "Manual", label: "عملیات دستی مدیر" },
    { value: "Other", label: "سایر" },
];

const operationTypeOptions = [
    { value: undefined, label: "همه انواع" },
    { value: "TopUp", label: "شارژ کیف پول" },
    { value: "ManualDeposit", label: "واریز دستی" },
    { value: "ManualWithdrawal", label: "برداشت دستی" },
    { value: "CommissionEarned", label: "پورسانت" },
    { value: "SubscriptionPurchase", label: "خرید اشتراک" },
    { value: "UnlockContactFee", label: "باز کردن تماس" },
];

const WalletTransactionsReportPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
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
    });

    // خواندن فیلترها از URL در بارگذاری اولیه
    const getInitialFilters = () => {
        const sourceCategory = searchParams.get("sourceCategory") || undefined;
        const directionCode = searchParams.get("directionCode") || undefined;
        const operationType = searchParams.get("operationTypeCode") || undefined;
        
        return {
            phoneNumber: "",
            displayName: "",
            directionCode: directionCode,
            statusCode: undefined,
            sourceCategory: sourceCategory,
            operationTypeCode: operationType,
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
                    statusCode: currentFilters.statusCode || undefined,
                    sourceCategory: currentFilters.sourceCategory || undefined,
                    operationTypeCode: currentFilters.operationTypeCode || undefined,
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

                setSummary({
                    totalCreditRial: report.totalCreditRial || 0,
                    totalDebitRial: report.totalDebitRial || 0,
                });
            } catch (error) {
                console.error("Error loading wallet transactions report", error);
                const msg =
                    error?.message ||
                    error?.response?.data?.message ||
                    "خطا در دریافت گزارش تراکنش‌های مالی";
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
        if (initialFilters.sourceCategory) {
            form.setFieldsValue({ sourceCategory: initialFilters.sourceCategory });
        }
        if (initialFilters.directionCode) {
            form.setFieldsValue({ directionCode: initialFilters.directionCode });
        }
        if (initialFilters.operationTypeCode) {
            form.setFieldsValue({ operationTypeCode: initialFilters.operationTypeCode });
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
            statusCode: values.statusCode || undefined,
            sourceCategory: values.sourceCategory || undefined,
            operationTypeCode: values.operationTypeCode || undefined,
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
            statusCode: undefined,
            sourceCategory: undefined,
            operationTypeCode: undefined,
            fromDate: null,
            toDate: null,
        };
        setFilters(newFilters);
        loadData(1, pagination.pageSize, newFilters);
    };

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
        },
        {
            title: "نام کاربر",
            dataIndex: "displayName",
            key: "displayName",
            render: (text) => text || "-",
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
        },
    ];

    return (
        <div>

            {/* فیلترها */}
            <Card
                title="گزارش تراکنش‌های مالی (دفتر کل)"
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
                            <Form.Item name="directionCode" label="جهت تراکنش">
                                <Select allowClear placeholder="انتخاب جهت">
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
                            <Form.Item name="statusCode" label="وضعیت تراکنش">
                                <Select allowClear placeholder="انتخاب وضعیت">
                                    {statusOptions.map((opt) =>
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
                            <Form.Item name="sourceCategory" label="منبع تراکنش">
                                <Select allowClear placeholder="انتخاب منبع">
                                    {sourceCategoryOptions.map((opt) =>
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
                            <Form.Item name="operationTypeCode" label="نوع عملیات">
                                <Select allowClear placeholder="انتخاب نوع">
                                    {operationTypeOptions.map((opt) =>
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

            {/* خلاصه آمار */}
            <Row gutter={16} style={{ marginBottom: 16 }}>
                <Col xs={24} sm={12} md={12} lg={12}>
                    <Card>
                        <Statistic
                            title={
                                <span style={{ fontSize: "16px", fontWeight: "bold" }}>
                                    جمع واریزها (تومان)
                                </span>
                            }
                            value={summary.totalCreditRial}
                            valueStyle={{
                                color: "#52c41a",
                                fontSize: "24px",
                                fontWeight: "bold",
                                direction: "ltr",
                            }}
                            prefix={<ArrowDownOutlined />}
                            formatter={(value) => `${formatPrice(value)} تومان`}
                        />
                    </Card>
                </Col>
                <Col xs={24} sm={12} md={12} lg={12}>
                    <Card>
                        <Statistic
                            title={
                                <span style={{ fontSize: "16px", fontWeight: "bold" }}>
                                    جمع برداشت‌ها (تومان)
                                </span>
                            }
                            value={summary.totalDebitRial}
                            valueStyle={{
                                color: "#f5222d",
                                fontSize: "24px",
                                fontWeight: "bold",
                                direction: "ltr",
                            }}
                            prefix={<ArrowUpOutlined />}
                            formatter={(value) => `${formatPrice(value)} تومان`}
                        />
                    </Card>
                </Col>
            </Row>

            {/* جدول تراکنش‌ها */}
            <Card>
                <Table
                    rowKey="transactionId"
                    columns={columns}
                    dataSource={data}
                    loading={loading}
                    scroll={{ x: 1200 }}
                    pagination={{
                        current: pagination.current,
                        pageSize: pagination.pageSize,
                        total: pagination.total,
                        showSizeChanger: true,
                        showTotal: (total) =>
                            `مجموع: ${total.toLocaleString("fa-IR")} تراکنش`,
                        pageSizeOptions: ["10", "20", "50", "100"],
                    }}
                    onChange={handleTableChange}
                />
            </Card>
        </div>
    );
};

export default WalletTransactionsReportPage;


