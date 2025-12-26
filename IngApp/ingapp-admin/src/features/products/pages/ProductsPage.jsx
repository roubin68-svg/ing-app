// src/features/products/pages/ProductsPage.jsx
import React, { useCallback, useEffect, useState } from "react";
import {
    App,
    Card,
    Table,
    Switch,
    Button,
    Space,
    Modal,
    Form,
    Input,
    Select,
    Row,
    Col,
} from "antd";
import { PlusOutlined, EditOutlined } from "@ant-design/icons";

import productsApi from "../api/productsApi";
import CategoryTreeSelect from "../components/CategoryTreeSelect";

const { Option } = Select;

const ProductsPage = () => {
    const { message: msgApi, modal } = App.useApp();

    // ========================
    // States
    // ========================
    const [loading, setLoading] = useState(false);

    const [data, setData] = useState([]);
    const [total, setTotal] = useState(0);

    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [filters, setFilters] = useState({
        categoryId: null,
        isActive: null,
    });

    const [sortBy, setSortBy] = useState(null);
    const [sortDesc, setSortDesc] = useState(false);

    // Product Modal
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingProduct, setEditingProduct] = useState(null);
    const [form] = Form.useForm();

    // ========================
    // Load Products (Paging + Filter)
    // ========================
    const loadProducts = useCallback(
        async (targetPage = page) => {
            try {
                setLoading(true);

                const params = {
                    page: targetPage,
                    pageSize,
                    categoryId: filters.categoryId,
                    isActive: filters.isActive,
                    search: filters.search || null,
                    sortBy,
                    sortDesc,
                };

                const res = await productsApi.getPaged(params);

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
                msgApi.error("خطا در دریافت لیست محصولات");
            } finally {
                setLoading(false);
            }
        },
        [page, pageSize, filters, sortBy, sortDesc, msgApi]
    );

    useEffect(() => {
        loadProducts(1);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [pageSize, filters, sortBy, sortDesc]);

    // ========================
    // Table change (paging)
    // ========================
    const handleTableChange = (pagination, _filters, sorter) => {
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

        loadProducts(newPage);
    };


    // ========================
    // Activate / Deactivate
    // ========================
    const handleChangeStatus = async (record, value) => {
        try {
            if (value) {
                await productsApi.activate(record.id);
                msgApi.success("محصول فعال شد");
            } else {
                await productsApi.deactivate(record.id);
                msgApi.success("محصول غیرفعال شد");
            }
            loadProducts();
        } catch (e) {
            console.error(e);
            msgApi.error(e.message || "خطا در تغییر وضعیت محصول");
        }
    };

    // ========================
    // Filters
    // ========================
    const handleSearchClick = () => {
        setPage(1);
        loadProducts(1);
    };

    const handleClearFilters = () => {
        setFilters({
            categoryId: null,
            isActive: null,
            search: "",
        });

        setSortBy(null);
        setSortDesc(false);

        setPage(1);
        loadProducts(1);
    };


    // ========================
    // Modal (Create / Edit)
    // ========================
    const openCreateModal = () => {
        setEditingProduct(null);
        form.resetFields();
        setIsModalOpen(true);
    };

    const openEditModal = async (record) => {
        try {
            setEditingProduct(record);
            const dto = await productsApi.getById(record.id);

            form.setFieldsValue({
                name: dto.name,
                categoryId: dto.categoryId,
                unit: dto.unit,
            });

            setIsModalOpen(true);
        } catch (e) {
            console.error(e);
            msgApi.error("خطا در دریافت اطلاعات محصول");
        }
    };

    const handleModalCancel = () => {
        setIsModalOpen(false);
        setEditingProduct(null);
        form.resetFields();
    };

    const handleFormFinish = async (values) => {
        try {
            const payload = {
                name: values.name.trim(),
                categoryId: values.categoryId,
                unit: values.unit?.trim() || null,
            };

            if (editingProduct) {
                await productsApi.update(editingProduct.id, payload);
                msgApi.success("محصول با موفقیت ویرایش شد");
            } else {
                await productsApi.create(payload);
                msgApi.success("محصول جدید با موفقیت ایجاد شد");
            }

            handleModalCancel();
            loadProducts();
        } catch (err) {
            console.error(err);
            const msg =
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در ذخیره اطلاعات محصول";
            msgApi.error(msg);
        }
    };

    // ========================
    // Columns
    // ========================
    const columns = [
        {
            title: "نام محصول",
            dataIndex: "name",
            width: "30%",
            sorter: true,
        },
        {
            title: "دسته‌بندی",
            dataIndex: "categoryName",
            width: "25%",
            sorter: true,
        },
        {
            title: "واحد",
            dataIndex: "unit",
            width: "15%",
            render: (v) => v || <span style={{ color: "#999" }}>—</span>,
        },
        {
            title: "فعال",
            dataIndex: "isActive",
            width: "10%",
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
            width: 200,
            render: (_, record) => (
                <Space size="small">
                    <Button
                        size="small"
                        icon={<EditOutlined />}
                        onClick={() => openEditModal(record)}
                    >
                        ویرایش
                    </Button>
                </Space>
            ),
        },
    ];

    return (
        <>
            <Card
                title="مدیریت محصولات"
                bordered={false}
                extra={
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={openCreateModal}
                    >
                        افزودن محصول جدید
                    </Button>
                }
            >
                {/* ---------------- Filters ---------------- */}
                <Row gutter={12} style={{ marginBottom: 20 }}>
                    <Col span={8}>
                        <CategoryTreeSelect
                            value={filters.categoryId}
                            onChange={(v) =>
                                setFilters({
                                    ...filters,
                                    categoryId: v ?? null,
                                })
                            }
                        />
                    </Col>
                    <Col span={8}>
                        <Input
                            placeholder="جستجو بر اساس نام محصول"
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
                        pageSize,
                        total,
                        showSizeChanger: true,
                    }}
                    onChange={handleTableChange}
                    bordered={false}
                />
            </Card>

            {/* ---------------- Modal (Create / Edit) ---------------- */}
            <Modal
                open={isModalOpen}
                title={editingProduct ? "ویرایش محصول" : "افزودن محصول جدید"}
                onCancel={handleModalCancel}
                onOk={() => form.submit()}
                okText="ذخیره"
                cancelText="انصراف"
                destroyOnClose
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleFormFinish}
                >
                    <Form.Item
                        label="نام محصول"
                        name="name"
                        rules={[
                            { required: true, message: "نام محصول الزامی است" },
                        ]}
                    >
                        <Input />
                    </Form.Item>

                    <Form.Item
                        label="دسته‌بندی"
                        name="categoryId"
                        rules={[
                            {
                                required: true,
                                message: "دسته‌بندی محصول الزامی است",
                            },
                        ]}
                    >
                        <CategoryTreeSelect />
                    </Form.Item>

                    <Form.Item
                        label="واحد"
                        name="unit"
                        rules={[{ required: true, message: "واحد محصول الزامی است" }]}
                    >
                        <Input placeholder="مثال: کیلوگرم، عدد، متر" />
                    </Form.Item>
                </Form>
            </Modal>
        </>
    );
};

export default ProductsPage;
