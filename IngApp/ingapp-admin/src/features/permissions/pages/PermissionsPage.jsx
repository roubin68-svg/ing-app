// src/features/permissions/pages/PermissionsPage.jsx
import React, { useCallback, useEffect, useState } from "react";
import {
    App,
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
    Spin,
    List,
} from "antd";
import {
    PlusOutlined,
    EditOutlined,
    TeamOutlined,
    UserOutlined,
} from "@ant-design/icons";

import permissionsApi from "../api/permissionsApi";

const { Option } = Select;

const PermissionsPage = () => {
    const { modal, message: msgApi } = App.useApp();

    const [loading, setLoading] = useState(false);

    const [data, setData] = useState([]);
    const [total, setTotal] = useState(0);

    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [sortBy, setSortBy] = useState(null);
    const [sortDesc, setSortDesc] = useState(false);

    const [filters, setFilters] = useState({
        search: "",
        isActive: null,
    });

    // Permission Modal
    const [isPermissionModalOpen, setIsPermissionModalOpen] = useState(false);
    const [editingPermission, setEditingPermission] = useState(null);
    const [permissionForm] = Form.useForm();

    // Roles Modal
    const [isRolesModalOpen, setIsRolesModalOpen] = useState(false);
    const [selectedPermissionForRoles, setSelectedPermissionForRoles] = useState(null);
    const [rolesLoading, setRolesLoading] = useState(false);
    const [roles, setRoles] = useState([]);

    // ========================
    // Load Permissions
    // ========================
    const loadPermissions = useCallback(
        async (targetPage = page) => {
            try {
                setLoading(true);

                const params = {
                    page: targetPage,
                    pageSize,
                    sortBy,
                    sortDesc,
                    search: filters.search || null,
                    isActive: filters.isActive,
                };

                const res = await permissionsApi.getPaged(params);

                setData(
                    (res.items || []).map((p) => ({
                        key: p.id,
                        ...p,
                    }))
                );
                setTotal(res.totalCount || 0);
                setPage(res.page || targetPage);
            } catch (err) {
                console.error(err);
                msgApi.error("خطا در دریافت لیست دسترسی‌ها");
            } finally {
                setLoading(false);
            }
        },
        [page, pageSize, sortBy, sortDesc, filters]
    );

    useEffect(() => {
        loadPermissions(1);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [pageSize, sortBy, sortDesc, filters]);

    // ========================
    // Table change
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

        loadPermissions(newPage);
    };

    // ========================
    // Active Switch
    // ========================
    const handleChangeStatus = async (record, value) => {
        try {
            await permissionsApi.update(record.id, {
                displayName: record.displayName,
                description: record.description,
                isActive: value,
            });
            msgApi.success("وضعیت مجوز به‌روزرسانی شد");
            loadPermissions();
        } catch (e) {
            console.error(e);
            msgApi.error("خطا در تغییر وضعیت مجوز");
        }
    };

    const handleSearchClick = () => {
        setPage(1);
        loadPermissions(1);
    };

    const handleClearFilters = () => {
        setFilters({
            search: "",
            isActive: null,
        });
        setPage(1);
        loadPermissions(1);
    };

    // ========================
    // Permission Modal
    // ========================
    const openCreateModal = () => {
        setEditingPermission(null);
        permissionForm.resetFields();
        setIsPermissionModalOpen(true);
    };

    const openEditModal = (permission) => {
        setEditingPermission(permission);
        permissionForm.setFieldsValue({
            code: permission.code,
            displayName: permission.displayName,
            description: permission.description,
            isActive: permission.isActive,
        });
        setIsPermissionModalOpen(true);
    };

    const handlePermissionModalCancel = () => {
        setIsPermissionModalOpen(false);
        setEditingPermission(null);
        permissionForm.resetFields();
    };

    const handlePermissionFormFinish = async (values) => {
        try {
            if (editingPermission) {
                const payload = {
                    displayName: values.displayName.trim(),
                    description: values.description?.trim() || null,
                    isActive: values.isActive,
                };
                await permissionsApi.update(editingPermission.id, payload);
                msgApi.success("مجوز با موفقیت ویرایش شد");
            } else {
                const payload = {
                    code: values.code.trim(),
                    displayName: values.displayName.trim(),
                    description: values.description?.trim() || null,
                };
                await permissionsApi.create(payload);
                msgApi.success("مجوز جدید با موفقیت ایجاد شد");
            }

            handlePermissionModalCancel();
            loadPermissions();
        } catch (err) {
            console.error(err);
            const msg =
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در ذخیره اطلاعات مجوز";
            msgApi.error(msg);
        }
    };

    // ========================
    // DELETE
    // ========================
    const handleDelete = (record) => {
        modal.confirm({
            title: "حذف مجوز",
            content: (
                <>
                    آیا از حذف مجوز <b>{record.displayName}</b> با کد{" "}
                    <b>{record.code}</b> مطمئن هستید؟
                </>
            ),
            okText: "حذف",
            okType: "danger",
            cancelText: "انصراف",
            onOk: async () => {
                try {
                    await permissionsApi.delete(record.id);
                    msgApi.success("مجوز با موفقیت حذف شد");
                    loadPermissions();
                } catch (err) {
                    console.error(err);
                    const msg =
                        err?.response?.data?.message ||
                        err?.response?.data?.Error ||
                        "این مجوز قابل حذف نیست";
                    msgApi.error(msg);
                }
            },
        });
    };

    // ========================
    // ROLES MODAL (Option C)
    // ========================
    const openRolesModal = async (permission) => {
        setSelectedPermissionForRoles(permission);
        setIsRolesModalOpen(true);

        setRoles([]);
        setRolesLoading(true);

        try {
            const res = await permissionsApi.getRoles(permission.id);
            setRoles(res);
        } catch (err) {
            console.error(err);
            msgApi.error("خطا در دریافت نقش‌های این دسترسی");
        } finally {
            setRolesLoading(false);
        }
    };

    const handleRolesModalCancel = () => {
        setIsRolesModalOpen(false);
        setSelectedPermissionForRoles(null);
    };

    // ========================
    // Columns
    // ========================
    const columns = [
        {
            title: "کد مجوز",
            dataIndex: "code",
            sorter: true,
            width: "20%",
            render: (code) => (
                <Tag color="blue" style={{ fontFamily: "monospace" }}>
                    {code}
                </Tag>
            ),
        },
        {
            title: "عنوان",
            dataIndex: "displayName",
            sorter: true,
            width: "20%",
        },
        {
            title: "توضیحات",
            dataIndex: "description",
            ellipsis: true,
            width: "30%",
            render: (v) => v || <span style={{ color: "#999" }}>—</span>,
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
            fixed: "right",
            width: 220,
            render: (_, record) => (
                <Space size="small">
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
                        danger
                        onClick={() => handleDelete(record)}
                    >
                        حذف
                    </Button>
                </Space>
            ),
        },
    ];

    return (
        <>
            <Card
                title="مدیریت دسترسی‌ها (Permissions)"
                bordered={false}
                extra={
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={openCreateModal}
                    >
                        افزودن مجوز جدید
                    </Button>
                }
            >
                {/* ---------------- Filters ---------------- */}
                <Row gutter={12} style={{ marginBottom: 20 }}>
                    <Col span={8}>
                        <Input
                            placeholder="جستجو بر اساس کد یا عنوان"
                            value={filters.search}
                            onChange={(e) =>
                                setFilters({ ...filters, search: e.target.value })
                            }
                        />
                    </Col>

                    <Col span={4}>
                        <Select
                            placeholder="وضعیت"
                            allowClear
                            style={{ width: "100%" }}
                            value={filters.isActive}
                            onChange={(v) =>
                                setFilters({
                                    ...filters,
                                    isActive: v === undefined ? null : v,
                                })
                            }
                        >
                            <Option value={true}>فعال</Option>
                            <Option value={false}>غیرفعال</Option>
                        </Select>
                    </Col>

                    <Col span={4}>
                        <Space>
                            <Button type="primary" onClick={handleSearchClick}>
                                جستجو
                            </Button>
                            <Button onClick={handleClearFilters}>پاکسازی</Button>
                        </Space>
                    </Col>
                </Row>

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
                    scroll={{ x: 1000 }}
                />
            </Card>

            {/* ---------------- Permission Modal ---------------- */}
            <Modal
                open={isPermissionModalOpen}
                title={editingPermission ? "ویرایش مجوز" : "افزودن مجوز جدید"}
                onCancel={handlePermissionModalCancel}
                onOk={() => permissionForm.submit()}
                okText="ذخیره"
                cancelText="انصراف"
                destroyOnClose
            >
                <Form
                    form={permissionForm}
                    layout="vertical"
                    onFinish={handlePermissionFormFinish}
                >
                    {!editingPermission && (
                        <Form.Item
                            label="کد مجوز (Code)"
                            name="code"
                            rules={[
                                { required: true, message: "کد مجوز الزامی است" },
                                { max: 200, message: "حداکثر ۲۰۰ کاراکتر مجاز است" },
                            ]}
                        >
                            <Input placeholder="مثال: Users.Manage" />
                        </Form.Item>
                    )}

                    {editingPermission && (
                        <Form.Item label="کد مجوز (Code)">
                            <Input value={editingPermission.code} disabled />
                        </Form.Item>
                    )}

                    <Form.Item
                        label="عنوان"
                        name="displayName"
                        rules={[
                            { required: true, message: "عنوان الزامی است" },
                            { max: 200, message: "حداکثر ۲۰۰ کاراکتر مجاز است" },
                        ]}
                    >
                        <Input placeholder="مثال: مدیریت کاربران" />
                    </Form.Item>

                    <Form.Item label="توضیحات" name="description">
                        <Input.TextArea rows={3} placeholder="توضیح اختیاری" />
                    </Form.Item>

                    {editingPermission && (
                        <Form.Item
                            label="فعال است؟"
                            name="isActive"
                            valuePropName="checked"
                        >
                            <Switch />
                        </Form.Item>
                    )}
                </Form>
            </Modal>

            {/* ---------------- Roles Modal (Option C) ---------------- */}
            <Modal
                open={isRolesModalOpen}
                title={
                    selectedPermissionForRoles
                        ? `نقش‌های دارای این دسترسی: ${selectedPermissionForRoles.code}`
                        : "نقش‌ها"
                }
                onCancel={handleRolesModalCancel}
                footer={[
                    <Button key="close" onClick={handleRolesModalCancel}>
                        بستن
                    </Button>,
                ]}
                destroyOnClose
            >
                {rolesLoading ? (
                    <div style={{ textAlign: "center", padding: 20 }}>
                        <Spin />
                    </div>
                ) : roles.length === 0 ? (
                    <p style={{ textAlign: "center", color: "#999" }}>
                        هیچ نقشی این دسترسی را ندارد.
                    </p>
                ) : (
                    <List
                        dataSource={roles}
                        renderItem={(role) => (
                            <List.Item>
                                <Space size="small">
                                    <UserOutlined style={{ color: "#1677ff" }} />
                                    <Tag color="blue">{role.name}</Tag>
                                    <span>{role.displayName}</span>
                                </Space>
                            </List.Item>
                        )}
                    />
                )}
            </Modal>
        </>
    );
};

export default PermissionsPage;
