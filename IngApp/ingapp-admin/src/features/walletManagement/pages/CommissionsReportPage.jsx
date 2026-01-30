// src/features/walletManagement/pages/CommissionsReportPage.jsx
import React, { useCallback, useEffect, useState } from "react";
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
    DollarOutlined,
    SearchOutlined,
    ReloadOutlined,
} from "@ant-design/icons";
import { DatePicker as JalaliDatePicker } from "antd-jalali";
import { toGregorianISO, ensureShamsiDayjs, todayShamsi, toShamsiDateTimeString } from "../../../core/utils/dateUtils";
import walletManagementApi from "../api/walletManagementApi";

const { Option } = Select;
const { Title } = Typography;

// نمایش مبلغ به تومان در UI (ورودی به ریال است)
const formatPrice = (rial) => {
    if (rial == null) return "0";
    const toman = rial / 10;
    return toman.toLocaleString("fa-IR");
};

const commissionTypeOptions = [
    { value: undefined, label: "همه انواع" },
    { value: "UnlockContactCommission", label: "پورسانت باز کردن تماس" },
    { value: "SubscriptionCommission", label: "پورسانت خرید اشتراک" },
];

const CommissionsReportPage = () => {
    const [form] = Form.useForm();

    const [loading, setLoading] = useState(false);
    const [data, setData] = useState([]);
    const [pagination, setPagination] = useState({
        current: 1,
        pageSize: 20,
        total: 0,
    });
    const [summary, setSummary] = useState({
        totalCommissionRial: 0,
        totalCount: 0,
    });
    const [filters, setFilters] = useState({
        visitorPhoneNumber: "",
        visitorDisplayName: "",
        commissionType: undefined,
        fromDate: null,
        toDate: null,
    });

    const loadData = useCallback(
        async (page = 1, pageSize = 20, currentFilters = filters) => {
            setLoading(true);
            try {
                const params = {
                    page,
                    pageSize,
                    visitorPhoneNumber: currentFilters.visitorPhoneNumber || undefined,
                    visitorDisplayName: currentFilters.visitorDisplayName || undefined,
                    commissionType: currentFilters.commissionType || undefined,
                    fromDate: currentFilters.fromDate
                        ? toGregorianISO(currentFilters.fromDate)
                        : undefined,
                    toDate: currentFilters.toDate
                        ? toGregorianISO(currentFilters.toDate)
                        : undefined,
                };

                const report = await walletManagementApi.getCommissionsReport(params);

                const paged = report.commissions;

                setData(paged.items || []);
                setPagination({
                    current: paged.page,
                    pageSize: paged.pageSize,
                    total: paged.totalCount,
                });

                setSummary({
                    totalCommissionRial: report.totalCommissionRial || 0,
                    totalCount: report.totalCount || 0,
                });
            } catch (error) {
                console.error("Error loading commissions report", error);
                const msg =
                    error?.message ||
                    error?.response?.data?.message ||
                    "خطا در دریافت گزارش پورسانت‌ها";
                message.error(msg);
            } finally {
                setLoading(false);
            }
        },
        [filters]
    );

    useEffect(() => {
        // بارگذاری اولیه بدون فیلتر تاریخ
        loadData(1, pagination.pageSize, filters);
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
            visitorPhoneNumber: values.visitorPhoneNumber || "",
            visitorDisplayName: values.visitorDisplayName || "",
            commissionType: values.commissionType || undefined,
            fromDate: values.fromDate || null,
            toDate: values.toDate || null,
        };
        setFilters(newFilters);
        loadData(1, pagination.pageSize, newFilters);
    };

    const handleReset = () => {
        form.resetFields();
        const newFilters = {
            visitorPhoneNumber: "",
            visitorDisplayName: "",
            commissionType: undefined,
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
            title: "بازاریاب",
            dataIndex: "visitorPhoneNumber",
            key: "visitorPhoneNumber",
            width: 150,
            render: (phone, record) => (
                <div>
                    <div>{phone}</div>
                    {record.visitorDisplayName && (
                        <div style={{ fontSize: "12px", color: "#999" }}>
                            {record.visitorDisplayName}
                        </div>
                    )}
                </div>
            ),
        },
        {
            title: "خریدار",
            dataIndex: "buyerPhoneNumber",
            key: "buyerPhoneNumber",
            width: 150,
            render: (phone, record) => (
                <div>
                    <div>{phone}</div>
                    {record.buyerDisplayName && (
                        <div style={{ fontSize: "12px", color: "#999" }}>
                            {record.buyerDisplayName}
                        </div>
                    )}
                </div>
            ),
        },
        {
            title: "نوع پورسانت",
            dataIndex: "commissionTypeTitle",
            key: "commissionTypeTitle",
            width: 180,
            render: (title, record) => {
                const color = record.commissionType === "UnlockContactCommission" ? "blue" : "green";
                return <Tag color={color}>{title}</Tag>;
            },
        },
        {
            title: "مبلغ اصلی (تومان)",
            dataIndex: "originalAmountRial",
            key: "originalAmountRial",
            align: "right",
            width: 150,
            render: (value) => (
                <span style={{ direction: "ltr", display: "block" }}>
                    {formatPrice(value)}
                </span>
            ),
        },
        {
            title: "درصد پورسانت",
            dataIndex: "commissionPercentage",
            key: "commissionPercentage",
            align: "right",
            width: 120,
            render: (value) => `${value}%`,
        },
        {
            title: "مبلغ پورسانت (تومان)",
            dataIndex: "commissionAmountRial",
            key: "commissionAmountRial",
            align: "right",
            width: 150,
            render: (value) => (
                <span style={{ color: "#52c41a", fontWeight: "bold", direction: "ltr", display: "block" }}>
                    {formatPrice(value)}
                </span>
            ),
        },
        {
            title: "توضیحات",
            dataIndex: "description",
            key: "description",
            render: (text) => text || "-",
        },
    ];

    return (
        <div>
            {/* فیلترها */}
            <Card
                title="گزارش پورسانت‌ها"
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
                            <Form.Item name="visitorPhoneNumber" label="شماره موبایل بازاریاب">
                                <Input placeholder="مثلاً 0912..." allowClear />
                            </Form.Item>
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Form.Item name="visitorDisplayName" label="نام بازاریاب">
                                <Input placeholder="نام بازاریاب" allowClear />
                            </Form.Item>
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Form.Item name="commissionType" label="نوع پورسانت">
                                <Select allowClear placeholder="انتخاب نوع">
                                    {commissionTypeOptions.map((opt) =>
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
                                    مجموع پورسانت‌ها (تومان)
                                </span>
                            }
                            value={summary.totalCommissionRial}
                            valueStyle={{
                                color: "#52c41a",
                                fontSize: "24px",
                                fontWeight: "bold",
                                direction: "ltr",
                            }}
                            prefix={<DollarOutlined />}
                            formatter={(value) => `${formatPrice(value)} تومان`}
                        />
                    </Card>
                </Col>
                <Col xs={24} sm={12} md={12} lg={12}>
                    <Card>
                        <Statistic
                            title={
                                <span style={{ fontSize: "16px", fontWeight: "bold" }}>
                                    تعداد کل پورسانت‌ها
                                </span>
                            }
                            value={summary.totalCount}
                            valueStyle={{
                                color: "#1890ff",
                                fontSize: "24px",
                                fontWeight: "bold",
                            }}
                            formatter={(value) => `${value.toLocaleString("fa-IR")} مورد`}
                        />
                    </Card>
                </Col>
            </Row>

            {/* جدول پورسانت‌ها */}
            <Card>
                <Table
                    rowKey="id"
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
                            `مجموع: ${total.toLocaleString("fa-IR")} پورسانت`,
                        pageSizeOptions: ["10", "20", "50", "100"],
                    }}
                    onChange={handleTableChange}
                />
            </Card>
        </div>
    );
};

export default CommissionsReportPage;






