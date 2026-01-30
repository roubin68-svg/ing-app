// src/features/walletManagement/pages/WalletUsersListPage.jsx
import React, { useEffect, useState, useCallback } from "react";
import { Card, Table, Form, Input, Select, Button, Space, Tag, message, Checkbox } from "antd";
import { WalletOutlined } from "@ant-design/icons";
import { useNavigate, useSearchParams } from "react-router-dom";
import walletManagementApi from "../api/walletManagementApi";

const { Option } = Select;

// ثابت نوع کاربر (هم‌راستا با UsersPage)
const USER_TYPE_OPTIONS = [
    { value: 1, label: "خریدار", code: "Buyer" },
    { value: 2, label: "تأمین‌کننده", code: "Supplier" },
    { value: 3, label: "مدیر سیستم", code: "Admin" },
    { value: 4, label: "بازاریاب", code: "Visitor" },
];

const WalletUsersListPage = () => {
    const [form] = Form.useForm();
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();

    const [loading, setLoading] = useState(false);
    const [data, setData] = useState([]);
    const [pagination, setPagination] = useState({ current: 1, pageSize: 20, total: 0 });
    
    // خواندن فیلتر hasTransactions از URL
    const initialHasTransactions = searchParams.get("hasTransactions") === "true";
    
    const [filters, setFilters] = useState({
        phoneNumber: "",
        displayName: "",
        userTypeId: null,
        hasTransactions: initialHasTransactions,
    });

    const loadData = useCallback(
        async (page = 1, pageSize = 20, currentFilters = filters) => {
            setLoading(true);
            try {
                const params = {
                    page,
                    pageSize,
                    phoneNumber: currentFilters.phoneNumber || undefined,
                    displayName: currentFilters.displayName || undefined,
                    userTypeId: currentFilters.userTypeId || undefined,
                    hasTransactions: currentFilters.hasTransactions || undefined,
                };

                // apiClient interceptor ApiResult را باز می‌کند، بنابراین
                // اینجا مستقیماً PagedResult<WalletUserSummaryDto> را می‌گیریم
                const paged = await walletManagementApi.getWalletUsers(params);

                setData(paged.items || []);
                setPagination({
                    current: paged.page,
                    pageSize: paged.pageSize,
                    total: paged.totalCount,
                });
            } catch (err) {
                console.error("Error loading wallet users", err);
                const msg =
                    err?.message ||
                    err?.response?.data?.message ||
                    "خطا در دریافت لیست کاربران مالی";
                message.error(msg);
            } finally {
                setLoading(false);
            }
        },
        [filters]
    );

    useEffect(() => {
        // تنظیم فرم با فیلترهای URL
        if (initialHasTransactions) {
            form.setFieldsValue({ hasTransactions: true });
        }
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
            phoneNumber: values.phoneNumber || "",
            displayName: values.displayName || "",
            userTypeId: values.userTypeId || null,
            hasTransactions: values.hasTransactions || false,
        };
        setFilters(newFilters);
        
        // به‌روزرسانی URL
        const newSearchParams = new URLSearchParams();
        if (newFilters.hasTransactions) {
            newSearchParams.set("hasTransactions", "true");
        }
        setSearchParams(newSearchParams);
        
        loadData(1, pagination.pageSize, newFilters);
    };

    const handleReset = () => {
        form.resetFields();
        const newFilters = {
            phoneNumber: "",
            displayName: "",
            userTypeId: null,
            hasTransactions: false,
        };
        setFilters(newFilters);
        setSearchParams(new URLSearchParams());
        loadData(1, pagination.pageSize, newFilters);
    };

    const columns = [
        {
            title: "شماره موبایل",
            dataIndex: "phoneNumber",
            key: "phoneNumber",
        },
        {
            title: "نام",
            dataIndex: "displayName",
            key: "displayName",
            render: (text) => text || "-",
        },
        {
            title: "نوع کاربر",
            dataIndex: "userTypeTitle",
            key: "userTypeTitle",
            render: (text) => (text ? <Tag color="blue">{text}</Tag> : "-"),
        },
        {
            title: "موجودی کیف پول (تومان)",
            dataIndex: "balanceRial",
            key: "balanceRial",
            render: (value) => {
                if (typeof value !== "number") return "0";
                const toman = value / 10;
                return toman.toLocaleString("fa-IR");
            },
        },
        {
            title: "عملیات",
            key: "actions",
            render: (_, record) => (
                <Space>
                    <Button
                        size="small"
                        icon={<WalletOutlined />}
                        onClick={() => navigate(`/wallet/admin/${record.userId}`)}
                    >
                        مدیریت کیف پول
                    </Button>
                </Space>
            ),
        },
    ];

    return (
        <Card title="مدیریت کیف پول کاربران">
            <Form
                layout="inline"
                form={form}
                onFinish={handleSearch}
                style={{ marginBottom: 16 }}
            >
                <Form.Item name="phoneNumber" label="شماره موبایل">
                    <Input placeholder="مثلاً 0912..." allowClear />
                </Form.Item>
                <Form.Item name="displayName" label="نام">
                    <Input placeholder="نام کاربر" allowClear />
                </Form.Item>
                <Form.Item name="userTypeId" label="نوع کاربر">
                    <Select
                        allowClear
                        placeholder="انتخاب نوع کاربر"
                        style={{ minWidth: 160 }}
                    >
                        {USER_TYPE_OPTIONS.map((opt) => (
                            <Option key={opt.value} value={opt.value}>
                                {opt.label}
                            </Option>
                        ))}
                    </Select>
                </Form.Item>
                <Form.Item name="hasTransactions" valuePropName="checked">
                    <Checkbox>فقط کاربران دارای گردش حساب</Checkbox>
                </Form.Item>
                <Form.Item>
                    <Space>
                        <Button type="primary" htmlType="submit">
                            جستجو
                        </Button>
                        <Button onClick={handleReset}>پاکسازی</Button>
                    </Space>
                </Form.Item>
            </Form>

            <Table
                rowKey="userId"
                columns={columns}
                dataSource={data}
                loading={loading}
                pagination={{
                    current: pagination.current,
                    pageSize: pagination.pageSize,
                    total: pagination.total,
                    showSizeChanger: true,
                    showTotal: (total) => `${total.toLocaleString("fa-IR")} کاربر`,
                }}
                onChange={handleTableChange}
            />
        </Card>
    );
};

export default WalletUsersListPage;


