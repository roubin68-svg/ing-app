// src/features/userSubscriptions/pages/UserSubscriptionsManagementPage.jsx
import React, { useEffect, useState } from "react";
import {
    Card,
    Table,
    Space,
    Tag,
    Input,
    Select,
    Button,
    message,
    Typography,
} from "antd";
import {
    ReloadOutlined,
    SearchOutlined,
} from "@ant-design/icons";
import userSubscriptionsApi from "../api/userSubscriptionsApi";
import jalaali from "jalaali-js";

const { Text } = Typography;

// تبدیل تاریخ میلادی به شمسی
const toShamsi = (gregorian) => {
    if (!gregorian) return "-";
    
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
    
    return "-";
};

const UserSubscriptionsManagementPage = () => {
    const [loading, setLoading] = useState(true);
    const [subscriptions, setSubscriptions] = useState([]);
    const [pagination, setPagination] = useState({
        current: 1,
        pageSize: 20,
        total: 0,
    });
    const [filters, setFilters] = useState({
        statusCode: undefined,
        userPhoneNumber: "",
        userDisplayName: "",
    });

    useEffect(() => {
        loadSubscriptions();
    }, []);

    const loadSubscriptions = async (page = 1, pageSize = 20) => {
        try {
            setLoading(true);
            const params = {
                page,
                pageSize,
                ...(filters.statusCode && { statusCode: filters.statusCode }),
                ...(filters.userPhoneNumber && { userPhoneNumber: filters.userPhoneNumber }),
                ...(filters.userDisplayName && { userDisplayName: filters.userDisplayName }),
            };
            const result = await userSubscriptionsApi.getPaged(params);
            // apiClient interceptor unwraps ApiResult
            setSubscriptions(result?.items || []);
            setPagination({
                current: result?.page || page,
                pageSize: result?.pageSize || pageSize,
                total: result?.totalCount || 0,
            });
        } catch (error) {
            message.error("خطا در دریافت لیست اشتراک‌ها");
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

    const getStatusColor = (statusCode) => {
        if (statusCode === "Active") return "green";
        if (statusCode === "Expired") return "red";
        if (statusCode === "Cancelled") return "orange";
        if (statusCode === "Pending") return "blue";
        return "default";
    };

    const getStatusText = (statusCode) => {
        const statusMap = {
            Active: "فعال",
            Expired: "منقضی شده",
            Cancelled: "لغو شده",
            Pending: "در انتظار",
        };
        return statusMap[statusCode] || statusCode;
    };

    const handleFilterChange = (key, value) => {
        setFilters((prev) => ({
            ...prev,
            [key]: value,
        }));
    };

    const handleSearch = () => {
        setPagination((prev) => ({ ...prev, current: 1 }));
        loadSubscriptions(1, pagination.pageSize);
    };

    const handleReset = () => {
        setFilters({
            statusCode: undefined,
            userPhoneNumber: "",
            userDisplayName: "",
        });
        setPagination((prev) => ({ ...prev, current: 1 }));
        setTimeout(() => {
            loadSubscriptions(1, pagination.pageSize);
        }, 100);
    };

    const columns = [
        {
            title: "کاربر",
            key: "user",
            width: 200,
            render: (_, record) => (
                <div>
                    <div style={{ fontWeight: 500 }}>{record.userDisplayName}</div>
                    <div style={{ fontSize: "12px", color: "#999" }}>
                        {record.userPhoneNumber}
                    </div>
                </div>
            ),
        },
        {
            title: "پلن",
            dataIndex: "planTitle",
            key: "planTitle",
            width: 150,
        },
        {
            title: "مدت",
            dataIndex: "durationMonths",
            key: "durationMonths",
            align: "center",
            width: 80,
            render: (months) => `${months} ماه`,
        },
        {
            title: "قیمت",
            dataIndex: "planPriceRial",
            key: "planPriceRial",
            align: "left",
            width: 120,
            render: (price) => formatPrice(price),
        },
        {
            title: "وضعیت",
            dataIndex: "statusCode",
            key: "statusCode",
            align: "center",
            width: 120,
            render: (code) => (
                <Tag color={getStatusColor(code)}>{getStatusText(code)}</Tag>
            ),
        },
        {
            title: "تاریخ شروع",
            dataIndex: "startDate",
            key: "startDate",
            width: 120,
            render: (date) => toShamsi(date),
        },
        {
            title: "تاریخ پایان",
            dataIndex: "endDate",
            key: "endDate",
            width: 120,
            render: (date) => toShamsi(date),
        },
        {
            title: "تاریخ خرید",
            dataIndex: "purchasedAt",
            key: "purchasedAt",
            width: 120,
            render: (date) => toShamsi(date),
        },
        {
            title: "دسترسی نامحدود",
            dataIndex: "unlimitedContactViews",
            key: "unlimitedContactViews",
            align: "center",
            width: 120,
            render: (value) =>
                value ? (
                    <Tag color="green">بله</Tag>
                ) : (
                    <Tag color="red">خیر</Tag>
                ),
        },
    ];

    return (
        
            <Card
                title="مدیریت اشتراک‌های خریداری شده"
                extra={
                    <Button
                        icon={<ReloadOutlined />}
                        onClick={() => loadSubscriptions(pagination.current, pagination.pageSize)}
                    >
                        به‌روزرسانی
                    </Button>
                }
            >
                {/* فیلترها */}
                <Space direction="vertical" size="middle" style={{ width: "100%", marginBottom: "16px" }}>
                    <Space wrap>
                        <Select
                            placeholder="وضعیت"
                            allowClear
                            style={{ width: 150 }}
                            value={filters.statusCode}
                            onChange={(value) => handleFilterChange("statusCode", value)}
                        >
                            <Select.Option value="Active">فعال</Select.Option>
                            <Select.Option value="Expired">منقضی شده</Select.Option>
                            <Select.Option value="Cancelled">لغو شده</Select.Option>
                            <Select.Option value="Pending">در انتظار</Select.Option>
                        </Select>

                        <Input
                            placeholder="شماره موبایل کاربر"
                            style={{ width: 200 }}
                            value={filters.userPhoneNumber}
                            onChange={(e) => handleFilterChange("userPhoneNumber", e.target.value)}
                            onPressEnter={handleSearch}
                        />

                        <Input
                            placeholder="نام کاربر"
                            style={{ width: 200 }}
                            value={filters.userDisplayName}
                            onChange={(e) => handleFilterChange("userDisplayName", e.target.value)}
                            onPressEnter={handleSearch}
                        />

                        <Button
                            type="primary"
                            icon={<SearchOutlined />}
                            onClick={handleSearch}
                        >
                            جستجو
                        </Button>

                        <Button onClick={handleReset}>پاک کردن</Button>
                    </Space>
                </Space>

                <Table
                    columns={columns}
                    dataSource={subscriptions}
                    loading={loading}
                    rowKey="id"
                    pagination={{
                        ...pagination,
                        onChange: (page, pageSize) => {
                            loadSubscriptions(page, pageSize);
                        },
                    }}
                />
            </Card>
    );
};

export default UserSubscriptionsManagementPage;











