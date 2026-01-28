// src/features/users/pages/UsersPage.jsx
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
    Checkbox,
} from "antd";
import {
    PlusOutlined,
    EditOutlined,
    TeamOutlined,
    LockOutlined,
    WalletOutlined,
} from "@ant-design/icons";
import { useNavigate } from "react-router-dom";

import userApi from "../api/userApi";
import roleApi from "../../roles/api/rolesApi";
import walletManagementApi from "../../walletManagement/api/walletManagementApi";


const { Option } = Select;

// =======================
// ثابت‌ها برای enum ها
// (اعداد دقیقاً مطابق backend)
// =======================

const USER_TYPE_OPTIONS = [
    { value: 1, label: "خریدار", code: "Buyer" },
    { value: 2, label: "تأمین‌کننده", code: "Supplier" },
    { value: 3, label: "مدیر سیستم", code: "Admin" },
    { value: 4, label: "بازاریاب", code: "Visitor" },
];

// تبدیل userType (عدد) به UserTypeCode (string)
const getUserTypeCode = (userTypeId) => {
    const option = USER_TYPE_OPTIONS.find(opt => opt.value === userTypeId);
    return option?.code || null;
};

// تبدیل UserTypeCode (string) به userType (عدد)
const getUserTypeId = (userTypeCode) => {
    const option = USER_TYPE_OPTIONS.find(opt => opt.code === userTypeCode);
    return option?.value || null;
};

const SUBSCRIPTION_LEVEL_OPTIONS = [
    { value: 0, label: "بدون اشتراک" },
    { value: 1, label: "برنزی" },
    { value: 2, label: "نقره‌ای" },
    { value: 3, label: "طلایی" },
];

const VERIFICATION_STATUS_OPTIONS = [
    { value: 0, label: "ارسال نشده" },
    { value: 1, label: "در انتظار بررسی" },
    { value: 2, label: "تأیید شده" },
    { value: 3, label: "رد شده" },
];

const UsersPage = () => {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);

    const [data, setData] = useState([]);
    const [total, setTotal] = useState(0);

    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [sortBy, setSortBy] = useState(null);
    const [sortDesc, setSortDesc] = useState(false);

    // ----- Filters -----
    const [filters, setFilters] = useState({
        phoneNumber: "",
        displayName: "",
        userType: null,
        subscriptionLevel: null,
        verificationStatus: null,
    });

    // ----- Modal ساخت/ویرایش کاربر -----
    const [isUserModalOpen, setIsUserModalOpen] = useState(false);
    const [editingUser, setEditingUser] = useState(null);
    const [userForm] = Form.useForm();

    // ----- Modal مدیریت نقش‌ها -----
    const [isRolesModalOpen, setIsRolesModalOpen] = useState(false);
    const [rolesLoading, setRolesLoading] = useState(false);
    const [allRoles, setAllRoles] = useState([]); // از API /roles
    const [selectedUser, setSelectedUser] = useState(null);
    const [selectedUserRoleNames, setSelectedUserRoleNames] = useState([]); // نقش‌ها بر اساس Name

    // ----- Modal تنظیم رمز عبور -----
    const [isPasswordModalOpen, setIsPasswordModalOpen] = useState(false);
    const [passwordForm] = Form.useForm();
    const [settingPassword, setSettingPassword] = useState(false);

    // ========================
    // Load Users (Paging + Filter + Sort)
    // ========================
    const loadUsers = useCallback(
        async (targetPage = page) => {
            try {
                setLoading(true);

                const params = {
                    page: targetPage,
                    pageSize,
                    sortBy,
                    sortDesc,
                    phoneNumber: filters.phoneNumber || null,
                    displayName: filters.displayName || null,
                    userTypeCode: filters.userType ? getUserTypeCode(filters.userType) : null,
                    subscriptionLevel: filters.subscriptionLevel ?? null,
                    verificationStatus: filters.verificationStatus ?? null,
                    // در صورت نیاز بعداً roleId هم اضافه می‌کنیم
                };

                const res = await userApi.getPaged(params);

                setData(
                    (res.items || []).map((u) => ({
                        key: u.id,
                        ...u,
                    }))
                );
                setTotal(res.totalCount || 0);
                setPage(res.page || targetPage);
            } catch (err) {
                console.error(err);
                message.error("خطا در دریافت لیست کاربران");
            } finally {
                setLoading(false);
            }
        },
        [page, pageSize, sortBy, sortDesc, filters]
    );

    useEffect(() => {
        loadUsers(1);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [pageSize, sortBy, sortDesc, filters]);

    // ========================
    // Table change (paging / sort)
    // ========================
    const handleTableChange = (pagination, _, sorter) => {
        const newPage = pagination.current;
        const newPageSize = pagination.pageSize;

        setPage(newPage);
        setPageSize(newPageSize);

        if (sorter && sorter.order) {
            setSortBy(sorter.field);
            setSortDesc(sorter.order === "descend");
        } else {
            setSortBy(null);
            setSortDesc(false);
        }

        loadUsers(newPage);
    };

    // ========================
    // Active / Deactive switch
    // ========================
    const handleChangeStatus = async (user, value) => {
        try {
            await userApi.changeStatus(user.id, { isActive: value });
            message.success("وضعیت کاربر به‌روزرسانی شد");
            loadUsers();
        } catch (e) {
            console.error(e);
            message.error("خطا در تغییر وضعیت کاربر");
        }
    };

    const handleSearch = () => {
        setPage(1);
        loadUsers(1);
    };

    const handleClearFilters = () => {
        setFilters({
            phoneNumber: "",
            displayName: "",
            userType: null,
            subscriptionLevel: null,
            verificationStatus: null,
        });
        setPage(1);
        loadUsers(1);
    };

    // ========================
    // User Modal (Create / Edit)
    // ========================
    const openCreateModal = () => {
        setEditingUser(null);
        userForm.resetFields();
        userForm.setFieldsValue({
            subscriptionLevel: 0,
            verificationStatus: 0,
        });
        setIsUserModalOpen(true);
    };

    const openEditModal = (user) => {
        setEditingUser(user);
        userForm.setFieldsValue({
            phoneNumber: user.phoneNumber,
            displayName: user.displayName,
            userType: getUserTypeId(user.userTypeCode || user.userTypeName),
            subscriptionLevel: user.subscriptionLevel,
            verificationStatus: user.verificationStatus,
        });
        setIsUserModalOpen(true);
    };

    const handleUserModalCancel = () => {
        setIsUserModalOpen(false);
        setEditingUser(null);
        userForm.resetFields();
    };

    const handleUserFormFinish = async (values) => {
        const payload = {
            phoneNumber: values.phoneNumber.trim(),
            displayName: values.displayName?.trim() || null,
            userTypeCode: getUserTypeCode(values.userType),
            subscriptionLevel: values.subscriptionLevel,
            verificationStatus: values.verificationStatus,
        };

        try {
            if (editingUser) {
                await userApi.update(editingUser.id, payload);
                message.success("کاربر با موفقیت ویرایش شد");
            } else {
                await userApi.create(payload);
                message.success("کاربر جدید با موفقیت ایجاد شد");
            }

            handleUserModalCancel();
            loadUsers();
        } catch (err) {
            console.error(err);
            message.error("خطا در ذخیره اطلاعات کاربر");
        }
    };

    // ========================
    // Roles Modal (Manage Roles)
    // ========================
    const ensureRolesLoaded = async () => {
        if (allRoles.length > 0) return;
        try {
            setRolesLoading(true);
            const roles = await roleApi.getAll();
            setAllRoles(roles || []);
        } catch (e) {
            console.error(e);
            message.error("خطا در دریافت لیست نقش‌ها");
        } finally {
            setRolesLoading(false);
        }
    };

    const openRolesModal = async (user) => {
        await ensureRolesLoaded();
        setSelectedUser(user);
        setSelectedUserRoleNames(user.roles || []);
        setIsRolesModalOpen(true);
    };

    const handleRolesModalCancel = () => {
        setIsRolesModalOpen(false);
        setSelectedUser(null);
        setSelectedUserRoleNames([]);
    };

    const handleSaveRoles = async () => {
        if (!selectedUser) return;

        try {
            setRolesLoading(true);

            const beforeNames = selectedUser.roles || [];
            const afterNames = selectedUserRoleNames || [];

            const rolesToAdd = allRoles.filter(
                (r) => afterNames.includes(r.name) && !beforeNames.includes(r.name)
            );
            const rolesToRemove = allRoles.filter(
                (r) => !afterNames.includes(r.name) && beforeNames.includes(r.name)
            );

            // اضافه کردن
            for (const r of rolesToAdd) {
                await userApi.assignRole(selectedUser.id, r.id);
            }

            // حذف کردن
            for (const r of rolesToRemove) {
                await userApi.removeRole(selectedUser.id, r.id);
            }

            message.success("نقش‌های کاربر با موفقیت به‌روزرسانی شد");
            handleRolesModalCancel();
            loadUsers();
        } catch (e) {
            console.error(e);
            message.error("خطا در به‌روزرسانی نقش‌های کاربر");
        } finally {
            setRolesLoading(false);
        }
    };

    // ========================
    // Password Modal
    // ========================
    const openPasswordModal = (user) => {
        setSelectedUser(user);
        passwordForm.resetFields();
        setIsPasswordModalOpen(true);
    };

    const handlePasswordModalCancel = () => {
        setIsPasswordModalOpen(false);
        setSelectedUser(null);
        passwordForm.resetFields();
    };

    const handleSetPassword = async (values) => {
        if (!selectedUser) return;

        try {
            setSettingPassword(true);
            await userApi.setPassword(selectedUser.id, values.password);
            message.success("رمز عبور با موفقیت تنظیم شد");
            handlePasswordModalCancel();
        } catch (err) {
            console.error(err);
            message.error(err.response?.data?.message || "خطا در تنظیم رمز عبور");
        } finally {
            setSettingPassword(false);
        }
    };

    // ========================
    // Table Columns
    // ========================
    const columns = [
        {
            title: "موبایل",
            dataIndex: "phoneNumber",
            sorter: true,
            width: "12%",
        },
        {
            title: "نام",
            dataIndex: "displayName",
            sorter: true,
            width: "15%",
        },
        {
            title: "نوع کاربر",
            dataIndex: "userTypeName",
            sorter: true,
            width: "12%",
            render: (v) => <Tag>{v}</Tag>,
        },
        {
            title: "اشتراک",
            dataIndex: "subscriptionLevelName",
            sorter: true,
            width: "12%",
            render: (v) => <Tag color="blue">{v}</Tag>,
        },
        {
            title: "وضعیت تأیید",
            dataIndex: "verificationStatusName",
            sorter: true,
            width: "12%",
            render: (v) => <Tag color="purple">{v}</Tag>,
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
            title: "نقش‌ها",
            dataIndex: "roles",
            width: "20%",
            render: (roles) =>
                roles && roles.length > 0 ? (
                    roles.map((r) => (
                        <Tag key={r} color="geekblue">
                            {r}
                        </Tag>
                    ))
                ) : (
                    <span style={{ color: "#999" }}>بدون نقش</span>
                ),
        },
        {
            title: "عملیات",
            key: "actions",
            width: "26%",
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
                        onClick={() => openRolesModal(record)}
                    >
                        نقش‌ها
                    </Button>
                    <Button
                        size="small"
                        icon={<LockOutlined />}
                        onClick={() => openPasswordModal(record)}
                    >
                        رمز عبور
                    </Button>
                    <Button
                        size="small"
                        icon={<WalletOutlined />}
                        onClick={() => navigate(`/wallet/admin/${record.id}`)}
                    >
                        کیف پول
                    </Button>
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
                title="مدیریت کاربران"
                bordered={false}
                extra={
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={openCreateModal}
                    >
                        افزودن کاربر جدید
                    </Button>
                }
            >
                {/* ---------------- Filters ---------------- */}
                <Row gutter={12} style={{ marginBottom: 20 }}>
                    <Col span={6}>
                        <Input
                            placeholder="شماره موبایل"
                            value={filters.phoneNumber}
                            onChange={(e) =>
                                setFilters({ ...filters, phoneNumber: e.target.value })
                            }
                        />
                    </Col>

                    <Col span={6}>
                        <Input
                            placeholder="نام"
                            value={filters.displayName}
                            onChange={(e) =>
                                setFilters({ ...filters, displayName: e.target.value })
                            }
                        />
                    </Col>

                    <Col span={4}>
                        <Select
                            placeholder="نوع کاربر"
                            allowClear
                            style={{ width: "100%" }}
                            value={filters.userType}
                            onChange={(v) => setFilters({ ...filters, userType: v ?? null })}
                        >
                            {USER_TYPE_OPTIONS.map((opt) => (
                                <Option key={opt.value} value={opt.value}>
                                    {opt.label}
                                </Option>
                            ))}
                        </Select>
                    </Col>

                    <Col span={4}>
                        <Select
                            placeholder="سطح اشتراک"
                            allowClear
                            style={{ width: "100%" }}
                            value={filters.subscriptionLevel}
                            onChange={(v) =>
                                setFilters({ ...filters, subscriptionLevel: v ?? null })
                            }
                        >
                            {SUBSCRIPTION_LEVEL_OPTIONS.map((opt) => (
                                <Option key={opt.value} value={opt.value}>
                                    {opt.label}
                                </Option>
                            ))}
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

                {/* ---------------- Table ---------------- */}
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
                />
            </Card>

            {/* ============ User Modal (Create / Edit) ============ */}
            <Modal
                open={isUserModalOpen}
                title={editingUser ? "ویرایش کاربر" : "افزودن کاربر جدید"}
                onCancel={handleUserModalCancel}
                onOk={() => userForm.submit()}
                okText="ذخیره"
                cancelText="انصراف"
                destroyOnClose
            >
                <Form
                    form={userForm}
                    layout="vertical"
                    onFinish={handleUserFormFinish}
                >
                    <Form.Item
                        label="شماره موبایل"
                        name="phoneNumber"
                        rules={[
                            { required: true, message: "شماره موبایل الزامی است" },
                            {
                                pattern: /^09\d{9}$/,
                                message: "شماره موبایل وارد شده معتبر نیست",
                            },
                        ]}
                    >
                        <Input />
                    </Form.Item>

                    <Form.Item
                        label="نام نمایشی"
                        name="displayName"
                        rules={[{ required: true, message: "نام الزامی است" }]}
                    >
                        <Input />
                    </Form.Item>

                    <Form.Item
                        label="نوع کاربر"
                        name="userType"
                        rules={[{ required: true, message: "نوع کاربر الزامی است" }]}
                    >
                        <Select placeholder="انتخاب نوع کاربر">
                            {USER_TYPE_OPTIONS.map((opt) => (
                                <Option key={opt.value} value={opt.value}>
                                    {opt.label}
                                </Option>
                            ))}
                        </Select>
                    </Form.Item>

                    <Form.Item
                        label="سطح اشتراک"
                        name="subscriptionLevel"
                        rules={[{ required: true, message: "سطح اشتراک الزامی است" }]}
                    >
                        <Select placeholder="انتخاب سطح اشتراک">
                            {SUBSCRIPTION_LEVEL_OPTIONS.map((opt) => (
                                <Option key={opt.value} value={opt.value}>
                                    {opt.label}
                                </Option>
                            ))}
                        </Select>
                    </Form.Item>

                    <Form.Item
                        label="وضعیت تأیید"
                        name="verificationStatus"
                        rules={[{ required: true, message: "وضعیت تأیید الزامی است" }]}
                    >
                        <Select placeholder="انتخاب وضعیت">
                            {VERIFICATION_STATUS_OPTIONS.map((opt) => (
                                <Option key={opt.value} value={opt.value}>
                                    {opt.label}
                                </Option>
                            ))}
                        </Select>
                    </Form.Item>
                </Form>
            </Modal>

            {/* ============ Roles Modal ============ */}
            <Modal
                open={isRolesModalOpen}
                title={
                    selectedUser
                        ? `مدیریت نقش‌های کاربر: ${selectedUser.displayName || selectedUser.phoneNumber}`
                        : "مدیریت نقش‌ها"
                }
                onCancel={handleRolesModalCancel}
                onOk={handleSaveRoles}
                okText="ذخیره نقش‌ها"
                cancelText="انصراف"
                confirmLoading={rolesLoading}
                destroyOnClose
            >
                <Checkbox.Group
                    style={{ width: "100%" }}
                    value={selectedUserRoleNames}
                    onChange={(vals) => setSelectedUserRoleNames(vals)}
                >
                    <Row>
                        {allRoles.map((r) => (
                            <Col span={24} key={r.id} style={{ marginBottom: 8 }}>
                                <Checkbox value={r.name}>{r.displayName || r.name}</Checkbox>
                            </Col>
                        ))}
                    </Row>
                </Checkbox.Group>
            </Modal>

            {/* ============ Password Modal ============ */}
            <Modal
                open={isPasswordModalOpen}
                title={
                    selectedUser
                        ? `تنظیم رمز عبور برای ${selectedUser.displayName || selectedUser.phoneNumber}`
                        : "تنظیم رمز عبور"
                }
                onCancel={handlePasswordModalCancel}
                footer={null}
                destroyOnClose
            >
                <Form
                    form={passwordForm}
                    layout="vertical"
                    onFinish={handleSetPassword}
                >
                    <Form.Item
                        label="رمز عبور جدید"
                        name="password"
                        rules={[
                            { required: true, message: "رمز عبور را وارد کنید" },
                            { min: 6, message: "رمز عبور باید حداقل 6 کاراکتر باشد" },
                        ]}
                    >
                        <Input.Password placeholder="رمز عبور جدید (حداقل 6 کاراکتر)" />
                    </Form.Item>

                    <Form.Item
                        label="تأیید رمز عبور"
                        name="confirmPassword"
                        dependencies={["password"]}
                        rules={[
                            { required: true, message: "تأیید رمز عبور را وارد کنید" },
                            ({ getFieldValue }) => ({
                                validator(_, value) {
                                    if (!value || getFieldValue("password") === value) {
                                        return Promise.resolve();
                                    }
                                    return Promise.reject(
                                        new Error("رمز عبور و تأیید آن مطابقت ندارند")
                                    );
                                },
                            }),
                        ]}
                    >
                        <Input.Password placeholder="تأیید رمز عبور" />
                    </Form.Item>

                    <Form.Item>
                        <Space>
                            <Button
                                type="primary"
                                htmlType="submit"
                                loading={settingPassword}
                            >
                                تنظیم رمز عبور
                            </Button>
                            <Button onClick={handlePasswordModalCancel}>انصراف</Button>
                        </Space>
                    </Form.Item>
                </Form>
            </Modal>
        </>
    );
};

export default UsersPage;
