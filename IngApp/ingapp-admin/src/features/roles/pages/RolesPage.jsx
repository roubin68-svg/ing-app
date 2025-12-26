// src/features/roles/pages/RolesPage.jsx
import React, { useCallback, useEffect, useState } from "react";
import {
    App,
    Card,
    Table,
    Tag,
    Switch,
    Input,
    Select,
    Row,
    Col,
    Button,
    Space,
    Modal,
    Form,
    List,
    Checkbox,
    Spin,
} from "antd";
import {
    PlusOutlined,
    EditOutlined,
    KeyOutlined,
} from "@ant-design/icons";

import rolesApi from "../api/rolesApi";
import permissionsApi from "../../permissions/api/permissionsApi";

const { Option } = Select;

const RolesPage = () => {
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

    // Role Modal
    const [isRoleModalOpen, setIsRoleModalOpen] = useState(false);
    const [editingRole, setEditingRole] = useState(null);
    const [roleForm] = Form.useForm();

    // Permissions Modal (Manage Role Permissions)
    const [isPermissionsModalOpen, setIsPermissionsModalOpen] = useState(false);
    const [selectedRoleForPermissions, setSelectedRoleForPermissions] =
        useState(null);
    const [permissionsLoading, setPermissionsLoading] = useState(false);
    const [savingPermissions, setSavingPermissions] = useState(false);
    const [allPermissions, setAllPermissions] = useState([]);
    const [selectedPermissionCodes, setSelectedPermissionCodes] = useState([]);

    // ========================
    // Load Roles (Paging + Filter + Sort)
    // ========================
    const loadRoles = useCallback(
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

                const res = await rolesApi.getPaged(params);

                setData(
                    (res.items || []).map((r) => ({
                        key: r.id,
                        ...r,
                    }))
                );
                setTotal(res.totalCount || 0);
                setPage(res.page || targetPage);
            } catch (err) {
                console.error(err);
                msgApi.error("خطا در دریافت لیست نقش‌ها");
            } finally {
                setLoading(false);
            }
        },
        [page, pageSize, sortBy, sortDesc, filters, msgApi]
    );

    useEffect(() => {
        loadRoles(1);
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

        loadRoles(newPage);
    };

    // ========================
    // Active / Inactive (با Update)
    // ========================
    const handleChangeStatus = async (record, value) => {
        try {
            await rolesApi.update(record.id, {
                displayName: record.displayName,
                description: record.description,
                isActive: value,
            });
            msgApi.success("وضعیت نقش به‌روزرسانی شد");
            loadRoles();
        } catch (e) {
            console.error(e);
            msgApi.error("خطا در تغییر وضعیت نقش");
        }
    };

    const handleSearchClick = () => {
        setPage(1);
        loadRoles(1);
    };

    const handleClearFilters = () => {
        setFilters({
            search: "",
            isActive: null,
        });
        setPage(1);
        loadRoles(1);
    };

    // ========================
    // Role Modal (Create / Edit)
    // ========================
    const openCreateModal = () => {
        setEditingRole(null);
        roleForm.resetFields();
        setIsRoleModalOpen(true);
    };

    const openEditModal = (role) => {
        setEditingRole(role);
        roleForm.setFieldsValue({
            name: role.name,
            displayName: role.displayName,
            description: role.description,
            isActive: role.isActive,
        });
        setIsRoleModalOpen(true);
    };

    const handleRoleModalCancel = () => {
        setIsRoleModalOpen(false);
        setEditingRole(null);
        roleForm.resetFields();
    };

    const handleRoleFormFinish = async (values) => {
        try {
            if (editingRole) {
                const payload = {
                    displayName: values.displayName.trim(),
                    description: values.description?.trim() || null,
                    isActive: values.isActive,
                };
                await rolesApi.update(editingRole.id, payload);
                msgApi.success("نقش با موفقیت ویرایش شد");
            } else {
                const payload = {
                    name: values.name.trim(),
                    displayName: values.displayName.trim(),
                    description: values.description?.trim() || null,
                };
                await rolesApi.create(payload);
                msgApi.success("نقش جدید با موفقیت ایجاد شد");
            }

            handleRoleModalCancel();
            loadRoles();
        } catch (err) {
            console.error(err);
            const msg =
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در ذخیره اطلاعات نقش";
            msgApi.error(msg);
        }
    };

    // ========================
    // DELETE
    // ========================
    const handleDelete = (record) => {
        modal.confirm({
            title: "حذف نقش",
            content: (
                <>
                    آیا از حذف نقش <b>{record.displayName}</b> با کد{" "}
                    <b>{record.name}</b> مطمئن هستید؟
                </>
            ),
            okText: "حذف",
            okType: "danger",
            cancelText: "انصراف",
            onOk: async () => {
                try {
                    await rolesApi.delete(record.id);
                    msgApi.success("نقش با موفقیت حذف شد");
                    loadRoles();
                } catch (err) {
                    console.error(err);
                    const msg =
                        err?.response?.data?.message ||
                        err?.response?.data?.Error ||
                        "این نقش قابل حذف نیست.";
                    msgApi.error(msg);
                }
            },
        });
    };

    // ========================
    // Permissions Modal (Manage Role-Permissions)
    // ========================
    const openPermissionsModal = async (role) => {
        setSelectedRoleForPermissions(role);
        setIsPermissionsModalOpen(true);
        setPermissionsLoading(true);
        setAllPermissions([]);
        setSelectedPermissionCodes(role.permissions || []);

        try {
            const permissions = await permissionsApi.getAll();
            setAllPermissions(permissions || []);
        } catch (err) {
            console.error(err);
            msgApi.error("خطا در دریافت لیست دسترسی‌ها");
        } finally {
            setPermissionsLoading(false);
        }
    };

    const handlePermissionsModalCancel = () => {
        setIsPermissionsModalOpen(false);
        setSelectedRoleForPermissions(null);
        setAllPermissions([]);
        setSelectedPermissionCodes([]);
        setPermissionsLoading(false);
        setSavingPermissions(false);
    };

    const togglePermissionCode = (code, checked) => {
        setSelectedPermissionCodes((prev) => {
            if (checked) {
                if (prev.includes(code)) return prev;
                return [...prev, code];
            } else {
                return prev.filter((c) => c !== code);
            }
        });
    };

    const handleSavePermissions = async () => {
        try {
            setSavingPermissions(true);
            await rolesApi.assignPermissions(selectedRoleForPermissions.id, {
                permissionCodes: selectedPermissionCodes,
            });
            msgApi.success("دسترسی‌های نقش با موفقیت به‌روزرسانی شد");
            handlePermissionsModalCancel();
            loadRoles();
        } catch (err) {
            console.error(err);
            const msg =
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در ذخیره دسترسی‌های نقش";
            msgApi.error(msg);
            setSavingPermissions(false);
        }
    };

    // ========================
    // Columns
    // ========================
    const columns = [
        {
            title: "کد نقش",
            dataIndex: "name",
            sorter: true,
            width: "18%",
            render: (name) => (
                <Tag color="blue" style={{ fontFamily: "monospace" }}>
                    {name}
                </Tag>
            ),
        },
        {
            title: "نام نمایشی",
            dataIndex: "displayName",
            sorter: true,
            width: "22%",
        },
        {
            title: "توضیحات",
            dataIndex: "description",
            ellipsis: true,
            width: "30%",
            render: (v) => v || <span style={{ color: "#999" }}>—</span>,
        },
        {
            title: "تعداد دسترسی‌ها",
            dataIndex: "permissions",
            width: "12%",
            render: (_, record) =>
                record.permissions ? record.permissions.length : 0,
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
            width: 260,
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
                        icon={<KeyOutlined />}
                        onClick={() => openPermissionsModal(record)}
                    >
                        مجوزها
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
                title="مدیریت نقش‌ها (Roles)"
                bordered={false}
                extra={
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={openCreateModal}
                    >
                        افزودن نقش جدید
                    </Button>
                }
            >
                {/* ---------------- Filters ---------------- */}
                <Row gutter={12} style={{ marginBottom: 20 }}>
                    <Col span={8}>
                        <Input
                            placeholder="جستجو بر اساس کد یا نام نمایشی"
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
                    scroll={{ x: 1000 }}
                    // ارتفاع ردیف کمی بیشتر تا دکمه حذف کاملاً داخل ردیف دیده شود
                    onRow={() => ({
                        style: { height: 56 },
                    })}
                />
            </Card>

            {/* ---------------- Role Modal (Create / Edit) ---------------- */}
            <Modal
                open={isRoleModalOpen}
                title={editingRole ? "ویرایش نقش" : "افزودن نقش جدید"}
                onCancel={handleRoleModalCancel}
                onOk={() => roleForm.submit()}
                okText="ذخیره"
                cancelText="انصراف"
                destroyOnClose
            >
                <Form
                    form={roleForm}
                    layout="vertical"
                    onFinish={handleRoleFormFinish}
                >
                    {!editingRole && (
                        <Form.Item
                            label="کد نقش (Name)"
                            name="name"
                            rules={[
                                { required: true, message: "کد نقش الزامی است" },
                                { max: 100, message: "حداکثر ۱۰۰ کاراکتر مجاز است" },
                            ]}
                        >
                            <Input placeholder="مثال: Admin, Seller, Viewer" />
                        </Form.Item>
                    )}

                    {editingRole && (
                        <Form.Item label="کد نقش (Name)">
                            <Input value={editingRole.name} disabled />
                        </Form.Item>
                    )}

                    <Form.Item
                        label="نام نمایشی"
                        name="displayName"
                        rules={[
                            { required: true, message: "نام نمایشی الزامی است" },
                            { max: 200, message: "حداکثر ۲۰۰ کاراکتر مجاز است" },
                        ]}
                    >
                        <Input placeholder="مثال: مدیر سیستم" />
                    </Form.Item>

                    <Form.Item label="توضیحات" name="description">
                        <Input.TextArea rows={3} placeholder="توضیح اختیاری" />
                    </Form.Item>

                    {editingRole && (
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

            {/* ---------------- Permissions Modal (Manage Role Permissions) ---------------- */}
            <Modal
                open={isPermissionsModalOpen}
                title={
                    selectedRoleForPermissions
                        ? `مدیریت دسترسی‌های نقش: ${selectedRoleForPermissions.displayName} (${selectedRoleForPermissions.name})`
                        : "مدیریت دسترسی‌های نقش"
                }
                onCancel={handlePermissionsModalCancel}
                okText="ذخیره"
                cancelText="انصراف"
                onOk={handleSavePermissions}
                confirmLoading={savingPermissions}
                destroyOnClose
            >
                {permissionsLoading ? (
                    <div style={{ textAlign: "center", padding: 24 }}>
                        <Spin />
                    </div>
                ) : allPermissions.length === 0 ? (
                    <p style={{ textAlign: "center", color: "#999" }}>
                        هیچ دسترسی‌ای ثبت نشده است.
                    </p>
                ) : (
                    <List
                        dataSource={allPermissions}
                        style={{ maxHeight: 400, overflowY: "auto" }}
                        renderItem={(perm) => {
                            const checked = selectedPermissionCodes.includes(
                                perm.code
                            );
                            return (
                                <List.Item>
                                    <Checkbox
                                        checked={checked}
                                        onChange={(e) =>
                                            togglePermissionCode(
                                                perm.code,
                                                e.target.checked
                                            )
                                        }
                                    >
                                        <Space size="small">
                                            <Tag
                                                color="blue"
                                                style={{ fontFamily: "monospace" }}
                                            >
                                                {perm.code}
                                            </Tag>
                                            <span>{perm.displayName}</span>
                                        </Space>
                                    </Checkbox>
                                </List.Item>
                            );
                        }}
                    />
                )}
            </Modal>
        </>
    );
};

export default RolesPage;
