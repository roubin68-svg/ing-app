// src/features/productAttributeTemplates/pages/ProductAttributeTemplatesPage.jsx
import React, { useEffect, useState } from "react";
import {
    App,
    Card,
    Select,
    Table,
    Switch,
    Button,
    Space,
    Spin,
    Divider,
    Typography,
} from "antd";

import productsApi from "../../products/api/productsApi";
import productAttributeDefinitionsApi from "../../productAttributes/api/productAttributeDefinitionsApi";
import productAttributeTemplatesApi from "../api/productAttributeTemplatesApi";
import {
    getProductAttributeDataTypeLabel,
} from "../../../core/constants/productAttributeDataTypes";

const { Text } = Typography;

const ProductAttributeTemplatesPage = () => {
    const { message } = App.useApp();

    // ========================
    // States
    // ========================
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);

    const [products, setProducts] = useState([]);
    const [selectedProductId, setSelectedProductId] = useState(null);

    const [attributeDefinitions, setAttributeDefinitions] = useState([]);
    const [templateItems, setTemplateItems] = useState([]);

    // ========================
    // Load Products
    // ========================
    useEffect(() => {
        const loadProducts = async () => {
            try {
                const res = await productsApi.getPaged({
                    page: 1,
                    pageSize: 1000,
                });
                setProducts(res.items || []);
            } catch (e) {
                console.error(e);
                message.error("خطا در دریافت لیست محصولات");
            }
        };

        loadProducts();
    }, [message]);

    // ========================
    // Load Attribute Definitions
    // ========================
    useEffect(() => {
        const loadAttributes = async () => {
            try {
                const res = await productAttributeDefinitionsApi.getPaged({
                    page: 1,
                    pageSize: 1000,
                    isActive: true,
                });
                setAttributeDefinitions(res.items || []);
            } catch (e) {
                console.error(e);
                message.error("خطا در دریافت ویژگی‌ها");
            }
        };

        loadAttributes();
    }, [message]);

    // ========================
    // Load Template by Product
    // ========================
    useEffect(() => {
        if (!selectedProductId) {
            setTemplateItems([]);
            return;
        }

        const loadTemplate = async () => {
            try {
                setLoading(true);
                const res =
                    await productAttributeTemplatesApi.getByProduct(
                        selectedProductId
                    );

                setTemplateItems(
                    (res || []).map((x) => ({
                        key: x.attributeDefinitionId,
                        ...x,
                    }))
                );
            } catch (e) {
                console.error(e);
                message.error("خطا در دریافت Template محصول");
            } finally {
                setLoading(false);
            }
        };

        loadTemplate();
    }, [selectedProductId, message]);

    // ========================
    // Helpers
    // ========================
    const isInTemplate = (attributeId) =>
        templateItems.some(
            (x) => x.attributeDefinitionId === attributeId
        );

    const toggleAttribute = (attribute, checked) => {
        if (checked) {
            setTemplateItems((prev) => [
                ...prev,
                {
                    attributeDefinitionId: attribute.id,
                    displayName: attribute.displayName,
                    dataType: attribute.dataType,
                    unit: attribute.unit,
                    isRequired: false,
                    key: attribute.id,
                },
            ]);
        } else {
            setTemplateItems((prev) =>
                prev.filter(
                    (x) =>
                        x.attributeDefinitionId !== attribute.id
                )
            );
        }
    };

    const updateRequired = (attributeId, value) => {
        setTemplateItems((prev) =>
            prev.map((x) =>
                x.attributeDefinitionId === attributeId
                    ? { ...x, isRequired: value }
                    : x
            )
        );
    };

    // ========================
    // Save Template
    // ========================
    const handleSave = async () => {
        if (!selectedProductId) {
            message.warning("ابتدا یک محصول انتخاب کنید");
            return;
        }

        try {
            setSaving(true);

            const payload = {
                productId: selectedProductId,
                requirements: templateItems.map((x) => ({
                    attributeDefinitionId:
                        x.attributeDefinitionId,
                    isRequired: x.isRequired,
                })),
            };

            await productAttributeTemplatesApi.upsert(payload);
            message.success("Template محصول ذخیره شد");
        } catch (e) {
            console.error(e);
            message.error("خطا در ذخیره Template");
        } finally {
            setSaving(false);
        }
    };

    // ========================
    // Columns
    // ========================
    const templateColumns = [
        {
            title: "عنوان ویژگی",
            dataIndex: "displayName",
        },
        {
            title: "نوع داده",
            dataIndex: "dataType",
            width: 160,
            render: (v) => getProductAttributeDataTypeLabel(v),
        },
        {
            title: "واحد",
            dataIndex: "unit",
            width: 120,
            render: (v) => v || <span style={{ color: "#999" }}>—</span>,
        },
        {
            title: "الزامی",
            width: 120,
            render: (_, record) => (
                <Switch
                    checked={record.isRequired}
                    onChange={(val) =>
                        updateRequired(
                            record.attributeDefinitionId,
                            val
                        )
                    }
                />
            ),
        },
    ];

    return (
        <Card title="Template ویژگی‌های محصول" bordered={false}>
            <Space
                direction="vertical"
                style={{ width: "100%" }}
                size="large"
            >
                {/* Product Select */}
                <Select
                    placeholder="انتخاب محصول"
                    style={{ width: 400 }}
                    value={selectedProductId}
                    onChange={(v) => setSelectedProductId(v)}
                    options={products.map((p) => ({
                        value: p.id,
                        label: p.name,
                    }))}
                    allowClear
                />

                {/* Template Table */}
                {loading ? (
                    <Spin />
                ) : (
                    <Table
                        dataSource={templateItems}
                        columns={templateColumns}
                        pagination={false}
                    />
                )}

                {/* Attribute Selector */}
                <Divider />
                <Text strong>افزودن / حذف ویژگی‌ها</Text>

                <Table
                    dataSource={attributeDefinitions}
                    rowKey="id"
                    pagination={false}
                    size="small"
                    columns={[
                        {
                            title: "عنوان ویژگی",
                            dataIndex: "displayName",
                        },
                        {
                            title: "نوع داده",
                            dataIndex: "dataType",
                            width: 140,
                            render: (v) =>
                                getProductAttributeDataTypeLabel(v),
                        },
                        {
                            title: "در Template",
                            width: 120,
                            render: (_, record) => (
                                <Switch
                                    checked={isInTemplate(
                                        record.id
                                    )}
                                    onChange={(val) =>
                                        toggleAttribute(
                                            record,
                                            val
                                        )
                                    }
                                />
                            ),
                        },
                    ]}
                />

                {/* Save */}
                <Button
                    type="primary"
                    loading={saving}
                    onClick={handleSave}
                    disabled={!selectedProductId}
                >
                    ذخیره Template
                </Button>
            </Space>
        </Card>
    );
};

export default ProductAttributeTemplatesPage;
