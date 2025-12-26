// src/features/kycAttributeDefinitions/pages/KycAttributeDefinitionsPage.jsx
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
    Tag,
} from "antd";
import kycAttributeDefinitionsApi from "../api/kycAttributeDefinitionsApi";

import {
    KYC_DATA_TYPES,
    getKycDataTypeLabel,
    getKycDataTypeOptions,
} from "../../../core/constants/kycDataTypes";


const { Option } = Select;



const KycAttributeDefinitionsPage = () => {
    const { message: msgApi } = App.useApp();

    // ========================
    // State لیست
    // ========================
    const [data, setData] = useState([]);
    const [total, setTotal] = useState(0);
    const [loading, setLoading] = useState(false);

    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [sortBy, setSortBy] = useState(null);
    const [sortDesc, setSortDesc] = useState(false);

    const [filters, setFilters] = useState({
        displayName: "",
        dataType: null,
        isActive: null,
    });

    // ========================
    // Modal Create / Edit
    // ========================
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingItem, setEditingItem] = useState(null);
    const [form] = Form.useForm();

    // ========================
    // Load Data
    // ========================
    const loadData = useCallback(
        async (targetPage = 1) => {
            setLoading(true);
            try {
                const params = {
                    page: targetPage,
                    pageSize,
                    sortBy,
                    sortDesc,
                };

                if (filters.displayName?.trim()) {
                    params.displayName = filters.displayName.trim();
                }

                if (filters.dataType !== null) {
                    params.dataType = filters.dataType;
                }

                if (filters.isActive !== null) {
                    params.isActive = filters.isActive;
                }

                const res = await kycAttributeDefinitionsApi.getPaged(params);

                setData(
                    (res.items || []).map((x) => ({
                        key: x.id,
                        ...x,
                    }))
                );
                setTotal(res.totalCount || 0);
                setPage(res.page || targetPage);
                setPageSize(res.pageSize || pageSize);
            } catch (err) {
                console.error(err);
                msgApi.error("خطا در دریافت لیست فیلدهای KYC");
            } finally {
                setLoading(false);
            }
        },
        [pageSize, sortBy, sortDesc, filters, msgApi]
    );

    useEffect(() => {
        loadData(1);
    }, [loadData]);

    // ========================
    // Filters
    // ========================
    const handleFilterChange = (field, value) => {
        setFilters((prev) => ({ ...prev, [field]: value }));
    };

    const handleSearch = () => {
        setPage(1);
        loadData(1);
    };

    const handleClear = () => {
        setFilters({
            displayName: "",
            dataType: null,
            isActive: null,
        });
        setPage(1);
        loadData(1);
    };

    // ========================
    // Table Change
    // ========================
    const handleTableChange = (pagination, _f, sorter) => {
        const newPage = pagination.current || 1;
        setPage(newPage);
        setPageSize(pagination.pageSize || 10);

        if (sorter?.field) {
            setSortBy(sorter.field);
            setSortDesc(sorter.order === "descend");
        } else {
            setSortBy(null);
            setSortDesc(false);
        }

        loadData(newPage);
    };

    // ========================
    // Modal handlers
    // ========================
    const openCreateModal = () => {
        setEditingItem(null);
        form.resetFields();
        form.setFieldsValue({
            isActive: true,
            defaultRequired: false,
        });
        setIsModalOpen(true);
    };

    const openEditModal = (record) => {
        setEditingItem(record);
        form.setFieldsValue(record);
        setIsModalOpen(true);
    };

    const handleSubmit = async (values) => {
        try {
            const payload = {
                displayName: values.displayName.trim(),
                description: values.description || "",
                dataType: values.dataType,
                defaultRequired: values.defaultRequired,
                isActive: values.isActive,
            };

            if (editingItem) {
                await kycAttributeDefinitionsApi.update(editingItem.id, payload);
                msgApi.success("فیلد KYC ویرایش شد.");
            } else {
                await kycAttributeDefinitionsApi.create(payload);
                msgApi.success("فیلد KYC جدید ثبت شد.");
            }

            setIsModalOpen(false);
            setEditingItem(null);
            loadData(page);
        } catch (err) {
            console.error(err);
            msgApi.error("خطا در ذخیره فیلد KYC");
        }
    };

    const handleChangeStatus = async (record, value) => {
        try {
            const payload = {
                displayName: record.displayName,
                description: record.description || "",
                dataType: record.dataType,
                defaultRequired: record.defaultRequired,
                isActive: value,
            };

            await kycAttributeDefinitionsApi.update(record.id, payload);
            msgApi.success("وضعیت فیلد KYC با موفقیت تغییر کرد.");
            loadData(page);
        } catch (err) {
            console.error(err);
            msgApi.error("خطا در تغییر وضعیت فیلد KYC");
        }
    };


    // ========================
    // Columns
    // ========================
    const columns = [
        {
            title: "شناسه",
            dataIndex: "id",
            width: 90,
            sorter: true,
        },
        {
            title: "عنوان",
            dataIndex: "displayName",
            sorter: true,
        },
        {
            title: "نوع داده",
            dataIndex: "dataType",
            width: 140,
            render: (val) => <Tag>{getKycDataTypeLabel(val)}</Tag>
        },
        {
            title: "پیش‌فرض اجباری",
            dataIndex: "defaultRequired",
            width: 150,
            render: (v) => (v ? "بله" : "خیر"),
        },
        {
            title: "فعال",
            dataIndex: "isActive",
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
            width: 120,
            render: (_, record) => (
                <Button size="small" onClick={() => openEditModal(record)}>
                    ویرایش
                </Button>
            ),
        },
    ];

    // ========================
    // Render
    // ========================
    return (
        <>
            <Card
                title="مدیریت فیلدهای KYC"
                extra={
                    <Button type="primary" onClick={openCreateModal}>
                        افزودن فیلد KYC
                    </Button>
                }
            >
                <Row gutter={12} style={{ marginBottom: 16 }}>
                    <Col xs={24} sm={12} md={6}>
                        <Input
                            placeholder="عنوان فیلد"
                            value={filters.displayName}
                            onChange={(e) =>
                                handleFilterChange(
                                    "displayName",
                                    e.target.value
                                )
                            }
                        />
                    </Col>

                    <Col xs={24} sm={12} md={6}>
                        <Select
                            allowClear
                            placeholder="نوع داده"
                            value={filters.dataType}
                            onChange={(v) =>
                                handleFilterChange("dataType", v ?? null)
                            }
                            style={{ width: "100%" }}
                        >
                            {getKycDataTypeOptions().map((t) => (
                                <Option key={t.value} value={t.value}>
                                    {t.label}
                                </Option>
                            ))}

                        </Select>
                    </Col>

                    <Col xs={24} sm={12} md={6}>
                        <Select
                            allowClear
                            placeholder="وضعیت"
                            value={filters.isActive}
                            onChange={(v) =>
                                handleFilterChange("isActive", v ?? null)
                            }
                            style={{ width: "100%" }}
                        >
                            <Option value={true}>فعال</Option>
                            <Option value={false}>غیرفعال</Option>
                        </Select>
                    </Col>

                    <Col xs={24} sm={24} md={6}>
                        <Space>
                            <Button type="primary" onClick={handleSearch}>
                                جستجو
                            </Button>
                            <Button onClick={handleClear}>پاکسازی</Button>
                        </Space>
                    </Col>
                </Row>

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
                    scroll={{ x: 900 }}
                />
            </Card>

            <Modal
                open={isModalOpen}
                title={
                    editingItem
                        ? "ویرایش فیلد KYC"
                        : "افزودن فیلد KYC جدید"
                }
                onCancel={() => setIsModalOpen(false)}
                onOk={() => form.submit()}
                destroyOnClose
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSubmit}
                >
                    <Form.Item
                        name="displayName"
                        label="عنوان"
                        rules={[{ required: true }]}
                    >
                        <Input />
                    </Form.Item>

                    <Form.Item name="description" label="توضیحات">
                        <Input.TextArea rows={3} />
                    </Form.Item>

                    <Form.Item
                        name="dataType"
                        label="نوع داده"
                        rules={[{ required: true }]}
                    >
                        <Select disabled={!!editingItem}>
                            {getKycDataTypeOptions().map((t) => (
                                <Option key={t.value} value={t.value}>
                                    {t.label}
                                </Option>
                            ))}
                        </Select>

                    </Form.Item>

                    <Form.Item
                        name="defaultRequired"
                        label="پیش‌فرض اجباری"
                        valuePropName="checked"
                    >
                        <Switch />
                    </Form.Item>

                    <Form.Item
                        name="isActive"
                        label="وضعیت"
                        valuePropName="checked"
                    >
                        <Switch />
                    </Form.Item>
                </Form>
            </Modal>
        </>
    );
};

export default KycAttributeDefinitionsPage;
