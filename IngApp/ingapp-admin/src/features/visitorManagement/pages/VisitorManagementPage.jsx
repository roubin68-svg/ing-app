// src/features/visitorManagement/pages/VisitorManagementPage.jsx
import React, { useCallback, useEffect, useState } from "react";
import {
    Card,
    Table,
    Tag,
    Switch,
    message,
    Input,
    Select,
    Row,
    Col,
    Button,
    Space,
    Modal,
    Form,
    InputNumber,
    Typography,
    Popconfirm,
    Descriptions,
    Divider,
} from "antd";
import {
    PlusOutlined,
    EditOutlined,
    DeleteOutlined,
    UserAddOutlined,
    DollarOutlined,
    TeamOutlined,
    CopyOutlined,
} from "@ant-design/icons";
import visitorManagementApi from "../api/visitorManagementApi";
import userApi from "../../users/api/userApi";
import { getProvinces, getCitiesByProvince } from "../../../core/location/iranProvinces";
import { toShamsiString } from "../../../core/utils/dateUtils";

const { Option } = Select;
const { Text, Title } = Typography;

const VisitorManagementPage = () => {
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState([]);
    const [total, setTotal] = useState(0);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [sortBy, setSortBy] = useState(null);
    const [sortDesc, setSortDesc] = useState(false);

    // Filters
    const [filters, setFilters] = useState({
        search: "",
        isActive: null,
    });

    // Visitor Modal
    const [isVisitorModalOpen, setIsVisitorModalOpen] = useState(false);
    const [editingVisitor, setEditingVisitor] = useState(null);
    const [visitorForm] = Form.useForm();
    const [users, setUsers] = useState([]);
    const [loadingUsers, setLoadingUsers] = useState(false);
    
    // Province/City state
    const [selectedProvince, setSelectedProvince] = useState(null);
    const provinces = React.useMemo(() => getProvinces(), []);
    const cities = React.useMemo(
        () => getCitiesByProvince(selectedProvince),
        [selectedProvince]
    );

    // Buyers Modal
    const [isBuyersModalOpen, setIsBuyersModalOpen] = useState(false);
    const [selectedVisitor, setSelectedVisitor] = useState(null);
    const [buyers, setBuyers] = useState([]);
    const [loadingBuyers, setLoadingBuyers] = useState(false);
    const [buyerForm] = Form.useForm();

    // Modal مدیریت قوانین پورسانت بازاریاب
    const [isCommissionModalOpen, setIsCommissionModalOpen] = useState(false);
    const [commissionRules, setCommissionRules] = useState([]);
    const [loadingCommission, setLoadingCommission] = useState(false);
    const [commissionForm] = Form.useForm();

    // ========================
    // Load Visitors
    // ========================
    const loadVisitors = useCallback(
        async (targetPage = page) => {
            try {
                setLoading(true);
                const params = {
                    page: targetPage,
                    pageSize,
                    sortBy,
                    sortDesc,
                    search: filters.search || null,
                    isActive: filters.isActive ?? null,
                };

                const res = await visitorManagementApi.getPaged(params);
                setData(
                    (res.items || []).map((v) => ({
                        key: v.id,
                        ...v,
                    }))
                );
                setTotal(res.totalCount || 0);
                setPage(res.page || targetPage);
            } catch (err) {
                console.error("Error in loadVisitors:", err);
                const errorMessage = 
                    err?.response?.data?.message || 
                    err?.response?.data?.error || 
                    err?.message || 
                    "خطا در دریافت لیست بازاریاب‌ها";
                message.error(errorMessage);
            } finally {
                setLoading(false);
            }
        },
        [page, pageSize, sortBy, sortDesc, filters]
    );

    useEffect(() => {
        loadVisitors(1);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [pageSize, sortBy, sortDesc, filters]);

    // ========================
    // Table Change
    // ========================
    const handleTableChange = (pagination, _, sorter) => {
        setPage(pagination.current);
        setPageSize(pagination.pageSize);
        if (sorter.field) {
            setSortBy(sorter.field);
            setSortDesc(sorter.order === "descend");
        } else {
            setSortBy(null);
            setSortDesc(false);
        }
    };

    const handleSearch = () => {
        setPage(1);
        loadVisitors(1);
    };

    const handleClearFilters = () => {
        setFilters({ search: "", isActive: null });
        setPage(1);
    };

    // ========================
    // Visitor Modal
    // ========================
    const loadUsersForSelect = async () => {
        try {
            setLoadingUsers(true);
            // دریافت لیست کاربران
            const usersRes = await userApi.getPaged({ page: 1, pageSize: 1000 });
            const allUsers = usersRes.items || [];
            
            // دریافت لیست Visitor ها برای فیلتر کردن کاربرانی که قبلاً VisitorProfile دارند
            const visitorsRes = await visitorManagementApi.getPaged({ 
                page: 1, 
                pageSize: 1000 
            });
            const existingVisitorUserIds = new Set(
                (visitorsRes.items || []).map(v => v.userId)
            );
            
            // فیلتر کردن کاربرانی که VisitorProfile ندارند
            const availableUsers = allUsers.filter(
                user => !existingVisitorUserIds.has(user.id)
            );
            
            setUsers(availableUsers);
        } catch (err) {
            console.error(err);
            message.error("خطا در دریافت لیست کاربران");
        } finally {
            setLoadingUsers(false);
        }
    };

    const openCreateModal = async () => {
        await loadUsersForSelect();
        setEditingVisitor(null);
        setSelectedProvince(null);
        visitorForm.resetFields();
        setIsVisitorModalOpen(true);
    };

    const openEditModal = async (visitor) => {
        await loadUsersForSelect();
        setEditingVisitor(visitor);
        setSelectedProvince(visitor.province || null);
        visitorForm.setFieldsValue({
            userId: visitor.userId,
            businessName: visitor.businessName,
            contactMobile: visitor.contactMobile,
            province: visitor.province,
            city: visitor.city,
            address: visitor.address,
            description: visitor.description,
            isActive: visitor.isActive,
        });
        setIsVisitorModalOpen(true);
    };

    const handleVisitorModalCancel = () => {
        setIsVisitorModalOpen(false);
        setEditingVisitor(null);
        setSelectedProvince(null);
        visitorForm.resetFields();
    };

    const handleVisitorFormFinish = async (values) => {
        try {
            if (editingVisitor) {
                await visitorManagementApi.update(editingVisitor.id, {
                    businessName: values.businessName?.trim() || null,
                    contactMobile: values.contactMobile?.trim() || null,
                    province: values.province || null,
                    city: values.city || null,
                    address: values.address?.trim() || null,
                    description: values.description?.trim() || null,
                    isActive: values.isActive,
                });
                message.success("بازاریاب با موفقیت ویرایش شد");
            } else {
                if (!values.userId) {
                    message.error("لطفاً کاربر را انتخاب کنید");
                    return;
                }
                
                await visitorManagementApi.create({
                    userId: values.userId,
                    businessName: values.businessName?.trim() || null,
                    contactMobile: values.contactMobile?.trim() || null,
                    province: values.province || null,
                    city: values.city || null,
                    address: values.address?.trim() || null,
                    description: values.description?.trim() || null,
                    isActive: values.isActive ?? true,
                });
                message.success("بازاریاب جدید با موفقیت ایجاد شد");
            }
            handleVisitorModalCancel();
            loadVisitors();
        } catch (err) {
            console.error("Error in handleVisitorFormFinish:", err);
            
            // استخراج پیام خطا از response
            let errorMessage = "خطا در ذخیره اطلاعات بازاریاب";
            
            if (err?.response?.data) {
                // اگر ApiResult باشد
                if (err.response.data.message) {
                    errorMessage = err.response.data.message;
                } else if (err.response.data.error) {
                    errorMessage = err.response.data.error;
                } else if (Array.isArray(err.response.data.errors) && err.response.data.errors.length > 0) {
                    // اگر ValidationException باشد (errors array)
                    errorMessage = err.response.data.errors[0];
                } else if (typeof err.response.data === 'string') {
                    errorMessage = err.response.data;
                }
            } else if (err?.message) {
                errorMessage = err.message;
            }
            
            message.error(errorMessage);
        }
    };

    const handleChangeStatus = async (visitor, isActive) => {
        try {
            await visitorManagementApi.changeStatus(visitor.id, isActive);
            message.success(`بازاریاب ${isActive ? "فعال" : "غیرفعال"} شد`);
            loadVisitors();
        } catch (err) {
            console.error(err);
            message.error("خطا در تغییر وضعیت بازاریاب");
        }
    };

    const handleDelete = async (visitor) => {
        try {
            await visitorManagementApi.delete(visitor.id);
            message.success("بازاریاب با موفقیت حذف شد");
            loadVisitors();
        } catch (err) {
            console.error(err);
            message.error(err.response?.data?.message || "خطا در حذف بازاریاب");
        }
    };

    // ========================
    // Buyers Modal
    // ========================
    const openBuyersModal = async (visitor) => {
        setSelectedVisitor(visitor);
        setIsBuyersModalOpen(true);
        await loadBuyers(visitor.id);
    };

    const loadBuyers = async (visitorProfileId) => {
        try {
            setLoadingBuyers(true);
            const res = await visitorManagementApi.getBuyers(visitorProfileId);
            setBuyers(res || []);
        } catch (err) {
            console.error(err);
            message.error("خطا در دریافت لیست خریداران");
        } finally {
            setLoadingBuyers(false);
        }
    };

    const handleAddBuyer = async (values) => {
        if (!selectedVisitor) return;
        try {
            await visitorManagementApi.addBuyer(selectedVisitor.id, {
                mobile: values.mobile.trim(),
                buyerName: values.buyerName?.trim() || null,
            });
            message.success("خریدار با موفقیت اضافه شد");
            buyerForm.resetFields();
            await loadBuyers(selectedVisitor.id);
            loadVisitors(); // به‌روزرسانی تعداد Buyer ها
        } catch (err) {
            console.error(err);
            message.error(err.response?.data?.message || "خطا در اضافه کردن خریدار");
        }
    };

    const handleRemoveBuyer = async (buyerProfileId) => {
        if (!selectedVisitor) return;
        try {
            await visitorManagementApi.removeBuyer(selectedVisitor.id, buyerProfileId);
            message.success("خریدار با موفقیت حذف شد");
            await loadBuyers(selectedVisitor.id);
            loadVisitors();
        } catch (err) {
            console.error(err);
            message.error("خطا در حذف خریدار");
        }
    };

    // ========================
    // Modal مدیریت قوانین پورسانت بازاریاب
    // ========================
    const openCommissionModal = async (visitor) => {
        setSelectedVisitor(visitor);
        setIsCommissionModalOpen(true);
        await loadCommissionRules(visitor.id);
    };

    const loadCommissionRules = async (visitorProfileId) => {
        try {
            setLoadingCommission(true);
            const res = await visitorManagementApi.getCommissionRules(visitorProfileId);
            setCommissionRules(res || []);
        } catch (err) {
            console.error(err);
            const errorMsg =
                err?.response?.data?.message ||
                err?.response?.data?.errors?.[0] ||
                err?.message ||
                "خطا در دریافت لیست قوانین پورسانت بازاریاب";
            message.error(errorMsg);
        } finally {
            setLoadingCommission(false);
        }
    };

    const handleSaveCommissionRule = async (ruleCode, values) => {
        if (!selectedVisitor) return;
        try {
            await visitorManagementApi.setCommissionRule(selectedVisitor.id, {
                commissionRuleCode: ruleCode,
                commissionPercentage: values.commissionPercentage,
                isActive: values.isActive ?? true,
                // فعلاً بازه زمانی را خالی می‌گذاریم (از همین لحظه به بعد، بدون تاریخ پایان)
                effectiveFrom: null,
                effectiveTo: null,
            });
            message.success("قانون پورسانت بازاریاب با موفقیت ذخیره شد");
            await loadCommissionRules(selectedVisitor.id);
        } catch (err) {
            console.error(err);
            const errorMsg =
                err?.response?.data?.message ||
                err?.response?.data?.errors?.[0] ||
                err?.message ||
                "خطا در ذخیره قانون پورسانت بازاریاب";
            message.error(errorMsg);
        }
    };

    const handleRemoveCommissionRule = async (ruleCode) => {
        if (!selectedVisitor) return;
        try {
            await visitorManagementApi.removeCommissionRule(selectedVisitor.id, ruleCode);
            message.success("قانون پورسانت بازاریاب با موفقیت غیرفعال شد");
            await loadCommissionRules(selectedVisitor.id);
        } catch (err) {
            console.error(err);
            const errorMsg =
                err?.response?.data?.message ||
                err?.response?.data?.errors?.[0] ||
                err?.message ||
                "خطا در غیرفعال‌سازی قانون پورسانت بازاریاب";
            message.error(errorMsg);
        }
    };

    const copyReferralCode = (code) => {
        navigator.clipboard.writeText(code);
        message.success("کد معرف کپی شد");
    };

    // ========================
    // Table Columns
    // ========================
    const columns = [
        {
            title: "کد معرف",
            dataIndex: "referralCode",
            width: "12%",
            render: (code) => (
                <Space>
                    <Text code>{code}</Text>
                    <Button
                        type="text"
                        size="small"
                        icon={<CopyOutlined />}
                        onClick={() => copyReferralCode(code)}
                    />
                </Space>
            ),
        },
        {
            title: "موبایل",
            dataIndex: "userPhoneNumber",
            width: "12%",
        },
        {
            title: "شماره تماس اضطراری",
            dataIndex: "contactMobile",
            width: "12%",
            render: (text) => text || "-",
        },
        {
            title: "نام",
            dataIndex: "userDisplayName",
            width: "12%",
        },
        {
            title: "نام کسب‌وکار",
            dataIndex: "businessName",
            width: "15%",
        },
        {
            title: "تعداد خریدار",
            dataIndex: "buyerCount",
            width: "10%",
            align: "center",
            render: (count) => <Tag color="blue">{count}</Tag>,
        },
        {
            title: "مجموع پورسانت",
            dataIndex: "totalCommissionRial",
            width: "12%",
            align: "right",
            render: (amount) => (
                <Text>
                    {(amount / 10).toLocaleString("fa-IR")} <small>تومان</small>
                </Text>
            ),
        },
        {
            title: "فعال",
            dataIndex: "isActive",
            width: "8%",
            render: (_, record) => (
                <Switch
                    checked={record.isActive}
                    onChange={(val) => handleChangeStatus(record, val)}
                />
            ),
        },
        {
            title: "عملیات",
            key: "actions",
            width: "20%",
            render: (_, record) => (
                <Space>
                    <Button
                        size="small"
                        icon={<EditOutlined />}
                        onClick={() => openEditModal(record)}
                    >
                        ویرایش
                    </Button>
                    <Button
                        size="small"
                        icon={<TeamOutlined />}
                        onClick={() => openBuyersModal(record)}
                    >
                        خریداران
                    </Button>
                    <Button
                        size="small"
                        icon={<DollarOutlined />}
                        onClick={() => openCommissionModal(record)}
                    >
                        پورسانت
                    </Button>
                    <Popconfirm
                        title="آیا از حذف این بازاریاب اطمینان دارید؟"
                        onConfirm={() => handleDelete(record)}
                        okText="بله"
                        cancelText="خیر"
                    >
                        <Button
                            size="small"
                            danger
                            icon={<DeleteOutlined />}
                        >
                            حذف
                        </Button>
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    // ========================
    // Render
    // ========================
    return (
        <>
            <Card
                title="مدیریت بازاریاب‌ها"
                bordered={false}
                extra={
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={openCreateModal}
                    >
                        افزودن بازاریاب جدید
                    </Button>
                }
            >
                {/* Filters */}
                <Row gutter={12} style={{ marginBottom: 20 }}>
                    <Col span={8}>
                        <Input
                            placeholder="جستجو (موبایل، نام، کد معرف، نام کسب‌وکار)"
                            value={filters.search}
                            onChange={(e) =>
                                setFilters({ ...filters, search: e.target.value })
                            }
                            onPressEnter={handleSearch}
                        />
                    </Col>
                    <Col span={4}>
                        <Select
                            placeholder="وضعیت"
                            allowClear
                            style={{ width: "100%" }}
                            value={filters.isActive}
                            onChange={(v) =>
                                setFilters({ ...filters, isActive: v ?? null })
                            }
                        >
                            <Option value={true}>فعال</Option>
                            <Option value={false}>غیرفعال</Option>
                        </Select>
                    </Col>
                    <Col span={4}>
                        <Space>
                            <Button type="primary" onClick={handleSearch}>
                                جستجو
                            </Button>
                            <Button onClick={handleClearFilters}>پاکسازی</Button>
                        </Space>
                    </Col>
                </Row>

                {/* Table */}
                <Table
                    loading={loading}
                    dataSource={data}
                    columns={columns}
                    pagination={{
                        current: page,
                        pageSize: pageSize,
                        total,
                        showSizeChanger: true,
                    }}
                    onChange={handleTableChange}
                    bordered={false}
                    scroll={{ x: 1200 }}
                />
            </Card>

            {/* Visitor Modal */}
            <Modal
                open={isVisitorModalOpen}
                title={editingVisitor ? "ویرایش بازاریاب" : "افزودن بازاریاب جدید"}
                onCancel={handleVisitorModalCancel}
                onOk={() => visitorForm.submit()}
                okText="ذخیره"
                cancelText="انصراف"
                destroyOnClose
                width={600}
            >
                <Form
                    form={visitorForm}
                    layout="vertical"
                    onFinish={handleVisitorFormFinish}
                >
                    {!editingVisitor && (
                        <Form.Item
                            label="کاربر"
                            name="userId"
                            rules={[{ required: true, message: "لطفاً کاربر را انتخاب کنید" }]}
                        >
                            <Select
                                placeholder="انتخاب کاربر"
                                loading={loadingUsers}
                                showSearch
                                filterOption={(input, option) => {
                                    const text = option?.label || option?.children || "";
                                    const textStr = typeof text === 'string' ? text : String(text);
                                    return textStr.toLowerCase().includes(input.toLowerCase());
                                }}
                                optionFilterProp="label"
                                notFoundContent={loadingUsers ? "در حال بارگذاری..." : "کاربری یافت نشد"}
                            >
                                {users.map((u) => {
                                    const label = `${u.phoneNumber} - ${u.displayName || "بدون نام"}`;
                                    return (
                                        <Option key={u.id} value={u.id} label={label}>
                                            {label}
                                        </Option>
                                    );
                                })}
                            </Select>
                        </Form.Item>
                    )}

                    {editingVisitor && (
                        <Descriptions column={1} bordered size="small" style={{ marginBottom: 16 }}>
                            <Descriptions.Item label="کد معرف">
                                <Text code>{editingVisitor.referralCode}</Text>
                            </Descriptions.Item>
                            <Descriptions.Item label="موبایل">
                                {editingVisitor.userPhoneNumber}
                            </Descriptions.Item>
                            <Descriptions.Item label="نام">
                                {editingVisitor.userDisplayName || "-"}
                            </Descriptions.Item>
                        </Descriptions>
                    )}

                    <Form.Item label="نام کسب‌وکار" name="businessName">
                        <Input placeholder="نام کسب‌وکار" />
                    </Form.Item>

                    <Form.Item 
                        label="شماره تماس اضطراری" 
                        name="contactMobile"
                        rules={[
                            {
                                pattern: /^09\d{9}$/,
                                message: "شماره تماس باید 11 رقم و با 09 شروع شود",
                            },
                        ]}
                    >
                        <Input placeholder="09xxxxxxxxx" />
                    </Form.Item>

                    <Row gutter={12}>
                        <Col span={12}>
                            <Form.Item label="استان" name="province">
                                <Select
                                    placeholder="انتخاب استان"
                                    allowClear
                                    onChange={(value) => {
                                        setSelectedProvince(value || null);
                                        visitorForm.setFieldsValue({ city: null });
                                    }}
                                >
                                    {provinces.map((p) => (
                                        <Option key={p} value={p}>
                                            {p}
                                        </Option>
                                    ))}
                                </Select>
                            </Form.Item>
                        </Col>
                        <Col span={12}>
                            <Form.Item label="شهر" name="city">
                                <Select
                                    placeholder="انتخاب شهر"
                                    allowClear
                                    disabled={!selectedProvince}
                                >
                                    {cities.map((c) => (
                                        <Option key={c} value={c}>
                                            {c}
                                        </Option>
                                    ))}
                                </Select>
                            </Form.Item>
                        </Col>
                    </Row>

                    <Form.Item label="آدرس" name="address">
                        <Input.TextArea rows={2} placeholder="آدرس" />
                    </Form.Item>

                    <Form.Item label="توضیحات" name="description">
                        <Input.TextArea rows={3} placeholder="توضیحات" />
                    </Form.Item>

                    <Form.Item
                        label="وضعیت"
                        name="isActive"
                        initialValue={true}
                    >
                        <Switch checkedChildren="فعال" unCheckedChildren="غیرفعال" />
                    </Form.Item>
                </Form>
            </Modal>

            {/* Buyers Modal */}
            <Modal
                open={isBuyersModalOpen}
                title={
                    <Space>
                        <TeamOutlined />
                        <span>مدیریت خریداران بازاریاب</span>
                        {selectedVisitor && (
                            <Text type="secondary">
                                ({selectedVisitor.referralCode})
                            </Text>
                        )}
                    </Space>
                }
                onCancel={() => {
                    setIsBuyersModalOpen(false);
                    setSelectedVisitor(null);
                    buyerForm.resetFields();
                }}
                footer={null}
                width={800}
                destroyOnClose
            >
                <Form
                    form={buyerForm}
                    layout="inline"
                    onFinish={handleAddBuyer}
                    style={{ marginBottom: 16 }}
                >
                    <Form.Item
                        name="mobile"
                        rules={[
                            { required: true, message: "لطفاً شماره موبایل را وارد کنید" },
                        ]}
                    >
                        <Input placeholder="شماره موبایل خریدار" style={{ width: 200 }} />
                    </Form.Item>
                    <Form.Item name="buyerName">
                        <Input placeholder="نام خریدار (اختیاری)" style={{ width: 200 }} />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit" icon={<UserAddOutlined />}>
                            افزودن خریدار
                        </Button>
                    </Form.Item>
                </Form>

                <Divider />

                <Table
                    loading={loadingBuyers}
                    dataSource={buyers}
                    columns={[
                        {
                            title: "موبایل",
                            dataIndex: "userPhoneNumber",
                        },
                        {
                            title: "نام",
                            dataIndex: "userDisplayName",
                        },
                        {
                            title: "نام کسب‌وکار",
                            dataIndex: "businessName",
                        },
                        {
                            title: "تاریخ معرفی",
                            dataIndex: "referredAt",
                            render: (date) =>
                                date
                                    ? new Date(date).toLocaleDateString("fa-IR")
                                    : "-",
                        },
                        {
                            title: "عملیات",
                            key: "actions",
                            render: (_, record) => (
                                <Popconfirm
                                    title="آیا از حذف این خریدار اطمینان دارید؟"
                                    onConfirm={() => handleRemoveBuyer(record.buyerProfileId)}
                                    okText="بله"
                                    cancelText="خیر"
                                >
                                    <Button size="small" danger>
                                        حذف
                                    </Button>
                                </Popconfirm>
                            ),
                        },
                    ]}
                    pagination={false}
                    size="small"
                />
            </Modal>

            {/* Modal قوانین پورسانت بازاریاب */}
            <Modal
                open={isCommissionModalOpen}
                title={
                    <Space>
                        <DollarOutlined />
                        <span>مدیریت قوانین پورسانت بازاریاب</span>
                        {selectedVisitor && (
                            <Text type="secondary">
                                ({selectedVisitor.referralCode})
                            </Text>
                        )}
                    </Space>
                }
                onCancel={() => {
                    setIsCommissionModalOpen(false);
                    setSelectedVisitor(null);
                    commissionForm.resetFields();
                }}
                footer={null}
                width={700}
                destroyOnClose
            >
                {loadingCommission ? (
                    <div style={{ textAlign: "center", padding: 40 }}>
                        <Text>در حال بارگذاری...</Text>
                    </div>
                ) : (
                    <div>
                        {commissionRules.map((rule) => (
                            <Card
                                key={rule.commissionRuleCode}
                                size="small"
                                style={{ marginBottom: 16 }}
                                title={rule.commissionRuleTitle}
                                extra={
                                    rule.commissionPercentage != null && (
                                        <Popconfirm
                                            title="آیا از غیرفعال‌کردن این قانون اطمینان دارید؟"
                                            onConfirm={() =>
                                                handleRemoveCommissionRule(rule.commissionRuleCode)
                                            }
                                            okText="بله"
                                            cancelText="خیر"
                                        >
                                            <Button size="small" danger>
                                                غیرفعال‌سازی
                                            </Button>
                                        </Popconfirm>
                                    )
                                }
                            >
                                <Form
                                    layout="vertical"
                                    initialValues={{
                                        commissionPercentage:
                                            rule.commissionPercentage ?? rule.defaultCommissionPercentage,
                                        isActive: rule.isActive,
                                    }}
                                    onFinish={(values) =>
                                        handleSaveCommissionRule(rule.commissionRuleCode, values)
                                    }
                                >
                                    <Row gutter={16}>
                                        <Col span={12}>
                                            <Form.Item label="درصد پیش‌فرض">
                                                <InputNumber
                                                    value={rule.defaultCommissionPercentage}
                                                    disabled
                                                    style={{ width: "100%" }}
                                                    addonAfter="%"
                                                />
                                            </Form.Item>
                                        </Col>
                                        <Col span={12}>
                                            <Form.Item
                                                label="درصد اختصاصی"
                                                name="commissionPercentage"
                                                rules={[
                                                    {
                                                        required: true,
                                                        message: "لطفاً درصد را وارد کنید",
                                                    },
                                                    {
                                                        type: "number",
                                                        min: 0,
                                                        max: 100,
                                                        message: "درصد باید بین 0 تا 100 باشد",
                                                    },
                                                ]}
                                            >
                                                <InputNumber
                                                    style={{ width: "100%" }}
                                                    min={0}
                                                    max={100}
                                                    addonAfter="%"
                                                />
                                            </Form.Item>
                                        </Col>
                                    </Row>
                                    <Form.Item name="isActive" valuePropName="checked">
                                        <Switch checkedChildren="فعال" unCheckedChildren="غیرفعال" />
                                    </Form.Item>
                                    <Row>
                                        <Col span={24}>
                                            <Text type="secondary" style={{ fontSize: 12 }}>
                                                دوره اعتبار برای این بازاریاب:&nbsp;
                                                {rule.effectiveFrom || rule.effectiveTo ? (
                                                    <>
                                                        {rule.effectiveFrom
                                                            ? toShamsiString(rule.effectiveFrom)
                                                            : "بدون تاریخ شروع"}{" "}
                                                        تا{" "}
                                                        {rule.effectiveTo
                                                            ? toShamsiString(rule.effectiveTo)
                                                            : "بدون تاریخ پایان"}
                                                    </>
                                                ) : (
                                                    "بدون محدودیت (از ابتدا تا کنون)"
                                                )}
                                            </Text>
                                        </Col>
                                    </Row>
                                    <Form.Item>
                                        <Button type="primary" htmlType="submit">
                                            ذخیره
                                        </Button>
                                    </Form.Item>
                                </Form>
                            </Card>
                        ))}
                    </div>
                )}
            </Modal>
        </>
    );
};

export default VisitorManagementPage;

