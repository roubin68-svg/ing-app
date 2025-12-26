// src/features/supplierTypes/pages/SupplierTypesPage.jsx
import React, { useCallback, useEffect, useState } from "react";
import {
    App,
    Card,
    Table,
    Switch,
    Input,
    Select,
    Row,
    Col,
    Button,
    Space,
    Modal,
    Form,
} from "antd";

import supplierTypesApi from "../api/supplierTypesApi";


const { Option } = Select;

const SupplierTypesPage = () => {
    const { message: msgApi, modal: modalApi } = App.useApp();

    // ========================
    // State اصلی لیست
    // ========================
    const [data, setData] = useState([]);
    const [total, setTotal] = useState(0);
    const [loading, setLoading] = useState(false);

    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [sortBy, setSortBy] = useState(null);
    const [sortDesc, setSortDesc] = useState(false);

    const [filters, setFilters] = useState({
        search: "",
        isActive: null, // true / false / null
    });

    // ========================
    // Modal ایجاد / ویرایش
    // ========================
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingItem, setEditingItem] = useState(null);
    const [form] = Form.useForm();

    // ========================
    // لود لیست
    // ========================
    const loadSupplierTypes = useCallback(
        async (targetPage = 1) => {
            setLoading(true);
            try {
                const params = {
                    page: targetPage,
                    pageSize,
                    sortBy,
                    sortDesc,
                };

                if (filters.search && filters.search.trim().length > 0) {
                    params.search = filters.search.trim();
                }

                if (filters.isActive !== null && filters.isActive !== undefined) {
                    params.isActive = filters.isActive;
                }

                const res = await supplierTypesApi.getPaged(params);

                const items = Array.isArray(res.items) ? res.items : [];
                setData(
                    items.map((item) => ({
                        key: item.id,
                        ...item,
                    }))
                );

                setTotal(res.totalCount || 0);
                setPage(res.page || targetPage);
                setPageSize(res.pageSize || pageSize);
            } catch (err) {
                console.error(err);
                msgApi.error(err.message || "خطا در دریافت لیست نوع تأمین‌کننده‌ها.");
            } finally {
                setLoading(false);
            }
        },
        [pageSize, sortBy, sortDesc, filters, msgApi]
    );

    // لود اولیه
    useEffect(() => {
        loadSupplierTypes(1);
    }, [loadSupplierTypes]);

    // ========================
    // مدیریت فیلترها
    // ========================
    const handleFilterChange = (field, value) => {
        setFilters((prev) => ({
            ...prev,
            [field]: value,
        }));
    };

    const handleSearchClick = () => {
        setPage(1);
        loadSupplierTypes(1);
    };

    const handleClearFilters = () => {
        setFilters({
            search: "",
            isActive: null,
        });
        setPage(1);
        loadSupplierTypes(1);
    };

    // ========================
    // تغییر صفحه / سورت جدول
    // ========================
    const handleTableChange = (pagination, _tableFilters, sorter) => {
        const newPage = pagination.current || 1;
        const newPageSize = pagination.pageSize || 10;

        setPage(newPage);
        setPageSize(newPageSize);

        if (sorter && sorter.field) {
            setSortBy(sorter.field);
            setSortDesc(sorter.order === "descend");
        } else {
            setSortBy(null);
            setSortDesc(false);
        }

        loadSupplierTypes(newPage);
    };

    // ========================
    // Modal ایجاد / ویرایش
    // ========================
    const openCreateModal = () => {
        setEditingItem(null);
        form.resetFields();
        form.setFieldsValue({
            isActive: true,
        });
        setIsModalOpen(true);
    };

    const openEditModal = (record) => {
        setEditingItem(record);
        form.setFieldsValue({
            name: record.name,
            description: record.description,
            isActive: record.isActive,
        });
        setIsModalOpen(true);
    };

    const handleModalCancel = () => {
        setIsModalOpen(false);
        setEditingItem(null);
    };

    const handleFormFinish = async (values) => {
        try {
            const payload = {
                name: values.name?.trim(),
                description: values.description || "",
                isActive: values.isActive ?? true,
            };

            if (!payload.name) {
                msgApi.error("نام نوع تأمین‌کننده الزامی است.");
                return;
            }

            if (editingItem) {
                await supplierTypesApi.update(editingItem.id, payload);
                msgApi.success("نوع تأمین‌کننده با موفقیت ویرایش شد.");
            } else {
                await supplierTypesApi.create(payload);
                msgApi.success("نوع تأمین‌کننده جدید با موفقیت ثبت شد.");
            }

            setIsModalOpen(false);
            setEditingItem(null);
            // پس از ذخیره، صفحه فعلی را دوباره لود می‌کنیم
            loadSupplierTypes(page);
        } catch (err) {
            console.error(err);
            msgApi.error(err.message || "خطا در ذخیره نوع تأمین‌کننده.");
        }
    };

    // ========================
    // Active / Inactive با API جدا
    // ========================
    const handleChangeStatus = async (record, value) => {
        try {
            if (value) {
                await supplierTypesApi.activate(record.id);
                msgApi.success("نوع تأمین‌کننده فعال شد.");
            } else {
                await supplierTypesApi.deactivate(record.id);
                msgApi.success("نوع تأمین‌کننده غیرفعال شد.");
            }

            // فقط همان صفحه فعلی را رفرش می‌کنیم
            loadSupplierTypes(page);
        } catch (err) {
            console.error(err);
            msgApi.error(err.message || "خطا در تغییر وضعیت نوع تأمین‌کننده.");
        }
    };

    // ========================
    // تعریف ستون‌های جدول
    // ========================
    const columns = [
        {
            title: "شناسه",
            dataIndex: "id",
            key: "id",
            width: 90,
            sorter: true,
        },
        {
            title: "نام نوع تأمین‌کننده",
            dataIndex: "name",
            key: "name",
            sorter: true,
        },
        {
            title: "توضیحات",
            dataIndex: "description",
            key: "description",
            ellipsis: true,
            render: (value) => value || "—",
        },
        {
            title: "فعال",
            dataIndex: "isActive",
            key: "isActive",
            width: 120,
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
            width: 140,
            render: (_, record) => (
                <Space>
                    <Button size="small" onClick={() => openEditModal(record)}>
                        ویرایش
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
                title="مدیریت نوع تأمین‌کننده‌ها"
                extra={
                    <Button type="primary" onClick={openCreateModal}>
                        افزودن نوع تأمین‌کننده
                    </Button>
                }
            >
                {/* فیلترها */}
                <Row gutter={12} style={{ marginBottom: 16 }}>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <Input
                            placeholder="جستجو بر اساس نام"
                            value={filters.search}
                            allowClear
                            onChange={(e) =>
                                handleFilterChange("search", e.target.value)
                            }
                        />
                    </Col>

                    <Col xs={24} sm={12} md={8} lg={6}>
                        <Select
                            allowClear
                            placeholder="وضعیت"
                            value={
                                filters.isActive === null
                                    ? undefined
                                    : filters.isActive
                            }
                            onChange={(value) =>
                                handleFilterChange(
                                    "isActive",
                                    value === undefined ? null : value
                                )
                            }
                            style={{ width: "100%" }}
                        >
                            <Option value={true}>فقط فعال‌ها</Option>
                            <Option value={false}>فقط غیرفعال‌ها</Option>
                        </Select>
                    </Col>

                    <Col
                        xs={24}
                        sm={24}
                        md={8}
                        lg={12}
                        style={{ marginTop: 8, textAlign: "left" }}
                    >
                        <Space>
                            <Button type="primary" onClick={handleSearchClick}>
                                جستجو
                            </Button>
                            <Button onClick={handleClearFilters}>
                                پاکسازی
                            </Button>
                        </Space>
                    </Col>
                </Row>

                {/* جدول */}
                <Table
                    loading={loading}
                    dataSource={data}
                    columns={columns}
                    pagination={{
                        current: page,
                        pageSize,
                        total,
                        showSizeChanger: true,
                    }}
                    onChange={handleTableChange}
                    bordered={false}
                    scroll={{ x: 800 }}
                    onRow={() => ({
                        style: { height: 56 },
                    })}
                />
            </Card>

            {/* Modal ایجاد / ویرایش */}
            <Modal
                open={isModalOpen}
                title={
                    editingItem
                        ? "ویرایش نوع تأمین‌کننده"
                        : "افزودن نوع تأمین‌کننده جدید"
                }
                okText={editingItem ? "ذخیره تغییرات" : "ثبت نوع جدید"}
                cancelText="انصراف"
                onCancel={handleModalCancel}
                onOk={() => form.submit()}
                destroyOnClose
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleFormFinish}
                    preserve={false}
                >
                    <Form.Item
                        name="name"
                        label="نام نوع تأمین‌کننده"
                        rules={[
                            {
                                required: true,
                                message: "لطفاً نام نوع تأمین‌کننده را وارد کنید.",
                            },
                        ]}
                    >
                        <Input maxLength={200} />
                    </Form.Item>

                    <Form.Item name="description" label="توضیحات">
                        <Input.TextArea rows={3} maxLength={1000} />
                    </Form.Item>

                    <Form.Item
                        name="isActive"
                        label="وضعیت"
                        valuePropName="checked"
                    >
                        <Switch checkedChildren="فعال" unCheckedChildren="غیرفعال" />
                    </Form.Item>
                </Form>
            </Modal>
        </>
    );
};

export default SupplierTypesPage;
