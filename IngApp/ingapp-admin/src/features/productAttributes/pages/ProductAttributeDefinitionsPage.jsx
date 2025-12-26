// src/features/productAttributes/pages/ProductAttributeDefinitionsPage.jsx
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

import productAttributeDefinitionsApi from "../api/productAttributeDefinitionsApi";
import {
    getProductAttributeDataTypeLabel,
    getProductAttributeDataTypeOptions,
} from "../../../core/constants/productAttributeDataTypes";

const { Option } = Select;

const ProductAttributeDefinitionsPage = () => {
    const { message: msgApi } = App.useApp();

    // ========================
    // States
    // ========================
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState([]);
    const [total, setTotal] = useState(0);

    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [filters, setFilters] = useState({
        search: "",
        dataType: null,
        isActive: null,
    });

    const [sortBy, setSortBy] = useState(null);
    const [sortDesc, setSortDesc] = useState(false);

    // Modal
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingItem, setEditingItem] = useState(null);
    const [form] = Form.useForm();

    // ========================
    // Load Data
    // ========================
    const loadData = useCallback(
        async (targetPage = page) => {
            try {
                setLoading(true);

                const params = {
                    page: targetPage,
                    pageSize,
                    displayName: filters.search || null,
                    dataType: filters.dataType,
                    isActive: filters.isActive,
                    sortBy,
                    sortDesc,
                };

                const res =
                    await productAttributeDefinitionsApi.getPaged(params);

                setData(
                    (res.items || []).map((x) => ({
                        key: x.id,
                        ...x,
                    }))
                );
                setTotal(res.totalCount || 0);
                setPage(res.page || targetPage);
            } catch (e) {
                console.error(e);
                msgApi.error("خطا در دریافت لیست ویژگی‌ها");
            } finally {
                setLoading(false);
            }
        },
        [page, pageSize, filters, sortBy, sortDesc, msgApi]
    );

    useEffect(() => {
        loadData(1);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [pageSize, filters, sortBy, sortDesc]);

    // ========================
    // Table Change
    // ========================
    const handleTableChange = (pagination, _f, sorter) => {
        const newPage = pagination.current;
        const newPageSize = pagination.pageSize;

        setPage(newPage);
        setPageSize(newPageSize);

        if (sorter?.order) {
            setSortBy(sorter.field);
            setSortDesc(sorter.order === "descend");
        } else {
            setSortBy(null);
            setSortDesc(false);
        }

        loadData(newPage);
    };

    // ========================
    // Activate / Deactivate
    // ========================
    const handleChangeStatus = async (record, value) => {
        try {
            if (value) {
                await productAttributeDefinitionsApi.activate(record.id);
                msgApi.success("ویژگی فعال شد");
            } else {
                await productAttributeDefinitionsApi.deactivate(record.id);
                msgApi.success("ویژگی غیرفعال شد");
            }
            loadData();
        } catch (e) {
            console.error(e);
            msgApi.error("خطا در تغییر وضعیت");
        }
    };

    // ========================
    // Filters
    // ========================
    const handleSearch = () => {
        setPage(1);
        loadData(1);
    };

    const handleClearFilters = () => {
        setFilters({
            search: "",
            dataType: null,
            isActive: null,
        });
        setSortBy(null);
        setSortDesc(false);
        setPage(1);
        loadData(1);
    };

    // ========================
    // Modal
    // ========================
    const openCreateModal = () => {
        setEditingItem(null);
        form.resetFields();
        setIsModalOpen(true);
    };

    const openEditModal = async (record) => {
        try {
            setEditingItem(record);
            const dto =
                await productAttributeDefinitionsApi.getById(record.id);

            form.setFieldsValue({
                displayName: dto.displayName,
                dataType: dto.dataType,
                unit: dto.unit,
            });

            setIsModalOpen(true);
        } catch (e) {
            console.error(e);
            msgApi.error("خطا در دریافت اطلاعات");
        }
    };

    const closeModal = () => {
        setIsModalOpen(false);
        setEditingItem(null);
        form.resetFields();
    };

    const handleFormFinish = async (values) => {
        try {
            const payload = {
                displayName: values.displayName.trim(),
                dataType: values.dataType,
                unit: values.unit?.trim() || null,
            };

            if (editingItem) {
                await productAttributeDefinitionsApi.update(
                    editingItem.id,
                    payload
                );
                msgApi.success("ویژگی ویرایش شد");
            } else {
                await productAttributeDefinitionsApi.create(payload);
                msgApi.success("ویژگی جدید ایجاد شد");
            }

            closeModal();
            loadData();
        } catch (e) {
            console.error(e);
            msgApi.error("خطا در ذخیره اطلاعات");
        }
    };

    // ========================
    // Columns
    // ========================
    const columns = [
        {
            title: "عنوان ویژگی",
            dataIndex: "displayName",
            sorter: true,
        },
        {
            title: "نوع داده",
            dataIndex: "dataType",
            render: (v) => getProductAttributeDataTypeLabel(v),
            width: 160,
        },
        {
            title: "واحد",
            dataIndex: "unit",
            width: 120,
            render: (v) => v || <span style={{ color: "#999" }}>—</span>,
        },
        {
            title: "فعال",
            dataIndex: "isActive",
            width: 90,
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
            width: 150,
            render: (_, record) => (
                <Button
                    size="small"
                    icon={<EditOutlined />}
                    onClick={() => openEditModal(record)}
                >
                    ویرایش
                </Button>
            ),
        },
    ];

    return (
        <>
            <Card
                title="مدیریت ویژگی‌های محصول"
                bordered={false}
                extra={
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={openCreateModal}
                    >
                        افزودن ویژگی
                    </Button>
                }
            >
                {/* Filters */}
                <Row gutter={12} style={{ marginBottom: 16 }}>
                    <Col span={6}>
                        <Input
                            placeholder="جستجو عنوان ویژگی"
                            value={filters.search}
                            onChange={(e) =>
                                setFilters({
                                    ...filters,
                                    search: e.target.value,
                                })
                            }
                        />
                    </Col>

                    <Col span={5}>
                        <Select
                            placeholder="نوع داده"
                            allowClear
                            style={{ width: "100%" }}
                            value={filters.dataType}
                            onChange={(v) =>
                                setFilters({
                                    ...filters,
                                    dataType: v ?? null,
                                })
                            }
                            options={getProductAttributeDataTypeOptions()}
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
                                    isActive: v ?? null,
                                })
                            }
                        >
                            <Option value={true}>فعال</Option>
                            <Option value={false}>غیرفعال</Option>
                        </Select>
                    </Col>

                    <Col span={6}>
                        <Space>
                            <Button type="primary" onClick={handleSearch}>
                                جستجو
                            </Button>
                            <Button onClick={handleClearFilters}>
                                پاکسازی
                            </Button>
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
                        pageSize,
                        total,
                        showSizeChanger: true,
                    }}
                    onChange={handleTableChange}
                />
            </Card>

            {/* Modal */}
            <Modal
                open={isModalOpen}
                title={
                    editingItem ? "ویرایش ویژگی" : "افزودن ویژگی جدید"
                }
                onCancel={closeModal}
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
                        label="عنوان ویژگی"
                        name="displayName"
                        rules={[
                            {
                                required: true,
                                message: "عنوان ویژگی الزامی است",
                            },
                        ]}
                    >
                        <Input />
                    </Form.Item>

                    <Form.Item
                        label="نوع داده"
                        name="dataType"
                        rules={[
                            {
                                required: true,
                                message: "نوع داده الزامی است",
                            },
                        ]}
                    >
                        <Select
                            options={getProductAttributeDataTypeOptions()}
                        />
                    </Form.Item>

                    <Form.Item label="واحد" name="unit">
                        <Input />
                    </Form.Item>
                </Form>
            </Modal>
        </>
    );
};

export default ProductAttributeDefinitionsPage;
