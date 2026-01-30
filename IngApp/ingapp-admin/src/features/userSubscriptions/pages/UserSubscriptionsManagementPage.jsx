// src/features/userSubscriptions/pages/UserSubscriptionsManagementPage.jsx
import React, { useEffect, useState } from "react";
import {
    Card,
    Table,
    Space,
    Tag,
    Input,
    Button,
    message,
    Typography,
    Modal,
    Form,
    DatePicker as JalaliDatePicker,
} from "antd";
import {
    ReloadOutlined,
    SearchOutlined,
    EditOutlined,
    DownOutlined,
    UpOutlined,
} from "@ant-design/icons";
import userSubscriptionsApi from "../api/userSubscriptionsApi";
import jalaali from "jalaali-js";
import { toGregorianISO, ensureShamsiDayjs, todayShamsi } from "../../../core/utils/dateUtils";
import dayjs from "dayjs";

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
    const [users, setUsers] = useState([]);
    const [expandedRowKeys, setExpandedRowKeys] = useState([]);
    const [pagination, setPagination] = useState({
        current: 1,
        pageSize: 20,
        total: 0,
    });
    const [filters, setFilters] = useState({
        userPhoneNumber: "",
        userDisplayName: "",
    });

    // Modal ویرایش
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [editingSubscription, setEditingSubscription] = useState(null);
    const [editForm] = Form.useForm();
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        loadUsers();
    }, []);

    const loadUsers = async (page = 1, pageSize = 20) => {
        try {
            setLoading(true);
            const params = {
                page,
                pageSize,
                ...(filters.userPhoneNumber && { userPhoneNumber: filters.userPhoneNumber }),
                ...(filters.userDisplayName && { userDisplayName: filters.userDisplayName }),
            };
            const result = await userSubscriptionsApi.getUsersWithSubscriptionsSummary(params);
            // apiClient interceptor unwraps ApiResult
            setUsers(result?.items || []);
            setPagination({
                current: result?.page || page,
                pageSize: result?.pageSize || pageSize,
                total: result?.totalCount || 0,
            });
        } catch (error) {
            message.error("خطا در دریافت لیست کاربران");
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
        loadUsers(1, pagination.pageSize);
    };

    const handleReset = () => {
        setFilters({
            userPhoneNumber: "",
            userDisplayName: "",
        });
        setPagination((prev) => ({ ...prev, current: 1 }));
        setTimeout(() => {
            loadUsers(1, pagination.pageSize);
        }, 100);
    };

    const handleEdit = (subscription) => {
        setEditingSubscription(subscription);
        editForm.setFieldsValue({
            startDate: subscription.startDate ? ensureShamsiDayjs(dayjs(subscription.startDate)) : null,
            endDate: subscription.endDate ? ensureShamsiDayjs(dayjs(subscription.endDate)) : null,
        });
        setIsEditModalOpen(true);
    };

    const handleCancelSubscription = () => {
        Modal.confirm({
            title: "لغو اشتراک",
            content: "آیا از لغو این اشتراک اطمینان دارید؟",
            okText: "بله، لغو کن",
            cancelText: "خیر",
            okType: "danger",
            onOk: async () => {
                try {
                    setSubmitting(true);
                    const payload = {
                        statusCode: "Cancelled",
                        cancelledAt: new Date().toISOString(),
                    };

                    await userSubscriptionsApi.update(editingSubscription.id, payload);
                    message.success("اشتراک با موفقیت لغو شد");
                    handleEditModalCancel();
                    loadUsers(pagination.current, pagination.pageSize);
                } catch (error) {
                    console.error(error);
                    const msg = error?.response?.data?.message || "خطا در لغو اشتراک";
                    message.error(msg);
                } finally {
                    setSubmitting(false);
                }
            },
        });
    };

    const handleEditModalCancel = () => {
        setIsEditModalOpen(false);
        setEditingSubscription(null);
        editForm.resetFields();
    };

    const handleEditFormFinish = async (values) => {
        try {
            setSubmitting(true);
            const payload = {
                startDate: values.startDate ? toGregorianISO(values.startDate) : null,
                endDate: values.endDate ? toGregorianISO(values.endDate) : null,
            };

            await userSubscriptionsApi.update(editingSubscription.id, payload);
            message.success("اشتراک با موفقیت ویرایش شد");
            handleEditModalCancel();
            loadUsers(pagination.current, pagination.pageSize);
        } catch (error) {
            console.error(error);
            const msg = error?.response?.data?.message || "خطا در ویرایش اشتراک";
            message.error(msg);
        } finally {
            setSubmitting(false);
        }
    };

    const handleExpand = (expanded, record) => {
        if (expanded) {
            setExpandedRowKeys([...expandedRowKeys, record.userId]);
        } else {
            setExpandedRowKeys(expandedRowKeys.filter(key => key !== record.userId));
        }
    };

    // ستون‌های جدول اصلی
    const columns = [
        {
            title: "کاربر",
            key: "user",
            width: 200,
            render: (_, record) => (
                <div>
                    <div style={{ fontWeight: 500 }}>{record.userDisplayName || "بدون نام"}</div>
                    <div style={{ fontSize: "12px", color: "#999" }}>
                        {record.userPhoneNumber}
                    </div>
                </div>
            ),
        },
        {
            title: "تعداد کل اشتراک‌ها",
            dataIndex: "totalSubscriptionsCount",
            key: "totalSubscriptionsCount",
            align: "center",
            width: 120,
            render: (count) => (
                <Tag color="blue">{count.toLocaleString("fa-IR")}</Tag>
            ),
        },
        {
            title: "اشتراک‌های فعال",
            dataIndex: "activeSubscriptionsCount",
            key: "activeSubscriptionsCount",
            align: "center",
            width: 120,
            render: (count) => (
                <Tag color="green">{count.toLocaleString("fa-IR")}</Tag>
            ),
        },
        {
            title: "اشتراک‌های تمام شده",
            dataIndex: "expiredSubscriptionsCount",
            key: "expiredSubscriptionsCount",
            align: "center",
            width: 120,
            render: (count) => (
                <Tag color="red">{count.toLocaleString("fa-IR")}</Tag>
            ),
        },
        {
            title: "عملیات",
            key: "expand",
            align: "center",
            width: 100,
            render: (_, record) => (
                <Button
                    type="text"
                    size="small"
                    icon={expandedRowKeys.includes(record.userId) ? <UpOutlined /> : <DownOutlined />}
                    onClick={() => handleExpand(!expandedRowKeys.includes(record.userId), record)}
                >
                    {expandedRowKeys.includes(record.userId) ? "بستن" : "جزئیات"}
                </Button>
            ),
        },
    ];

    // ستون‌های جدول اشتراک‌ها (در expandedRowRender)
    const subscriptionColumns = [
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
        {
            title: "عملیات",
            key: "actions",
            align: "center",
            width: 120,
            fixed: "right",
            render: (_, subscription) => (
                <Button
                    size="small"
                    icon={<EditOutlined />}
                    onClick={() => handleEdit(subscription)}
                >
                    ویرایش
                </Button>
            ),
        },
    ];

    return (
        <>
            <Card
                title="مدیریت اشتراک‌های خریداری شده"
                extra={
                    <Button
                        icon={<ReloadOutlined />}
                        onClick={() => loadUsers(pagination.current, pagination.pageSize)}
                    >
                        به‌روزرسانی
                    </Button>
                }
            >
                {/* فیلترها */}
                <Space direction="vertical" size="middle" style={{ width: "100%", marginBottom: "16px" }}>
                    <Space wrap>
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
                    dataSource={users}
                    loading={loading}
                    rowKey="userId"
                    pagination={{
                        ...pagination,
                        onChange: (page, pageSize) => {
                            loadUsers(page, pageSize);
                        },
                    }}
                    expandable={{
                        expandedRowKeys,
                        onExpand: handleExpand,
                        expandedRowRender: (record) => (
                            <div style={{ margin: 0, background: "#fafafa" }}>
                                <Table
                                    columns={subscriptionColumns}
                                    dataSource={record.subscriptions || []}
                                    rowKey="id"
                                    pagination={false}
                                    size="small"
                                    bordered
                                />
                            </div>
                        ),
                        expandIcon: () => null, // دکمه expand را مخفی می‌کنیم چون در ستون آخر است
                    }}
                />
            </Card>

            {/* Modal ویرایش اشتراک */}
            <Modal
                open={isEditModalOpen}
                title="ویرایش اشتراک"
                onCancel={handleEditModalCancel}
                footer={[
                    <Button
                        key="save"
                        type="primary"
                        onClick={() => editForm.submit()}
                        loading={submitting}
                    >
                        ذخیره
                    </Button>,
                    <Button
                        key="cancel-subscription"
                        danger
                        onClick={handleCancelSubscription}
                        loading={submitting}
                        disabled={editingSubscription?.statusCode === "Cancelled"}
                    >
                        لغو اشتراک
                    </Button>,
                    <Button key="cancel" onClick={handleEditModalCancel}>
                        انصراف
                    </Button>,
                ]}
                destroyOnClose
            >
                <Form
                    form={editForm}
                    layout="vertical"
                    onFinish={handleEditFormFinish}
                >
                    <Form.Item
                        name="startDate"
                        label="تاریخ شروع"
                        getValueFromEvent={(date) => ensureShamsiDayjs(date)}
                    >
                        <JalaliDatePicker
                            style={{ width: "100%" }}
                            format="YYYY/MM/DD"
                            placeholder="انتخاب تاریخ"
                        />
                    </Form.Item>

                    <Form.Item
                        name="endDate"
                        label="تاریخ پایان"
                        getValueFromEvent={(date) => ensureShamsiDayjs(date)}
                    >
                        <JalaliDatePicker
                            style={{ width: "100%" }}
                            format="YYYY/MM/DD"
                            placeholder="انتخاب تاریخ"
                        />
                    </Form.Item>
                </Form>
            </Modal>
        </>
    );
};

export default UserSubscriptionsManagementPage;
