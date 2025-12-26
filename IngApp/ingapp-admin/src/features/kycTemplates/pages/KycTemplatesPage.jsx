// src/features/kycTemplates/pages/KycTemplatesPage.jsx

import React, { useCallback, useEffect, useMemo, useState } from "react";
import {
    App,
    Card,
    Table,
    Switch,
    InputNumber,
    Select,
    Row,
    Col,
    Button,
    Space,
    Tag,
    Divider,
} from "antd";

import kycTemplatesApi from "../api/kycTemplatesApi";
import supplierTypesApi from "../../supplierTypes/api/supplierTypesApi";
import kycAttributeDefinitionsApi from "../../kycAttributeDefinitions/api/kycAttributeDefinitionsApi";

import { getKycDataTypeLabel } from "../../../core/constants/kycDataTypes";

const { Option } = Select;

const KycTemplatesPage = () => {
    const { message: msgApi } = App.useApp();

    const [loading, setLoading] = useState(false);

    const [supplierTypes, setSupplierTypes] = useState([]);
    const [selectedSupplierTypeId, setSelectedSupplierTypeId] = useState(null);

    const [attributeDefinitions, setAttributeDefinitions] = useState([]);
    const [templateItems, setTemplateItems] = useState([]);

    // ========================
    // Load SupplierTypes (getAll)
    // ========================
    const loadSupplierTypes = useCallback(async () => {
        try {
            const res = await supplierTypesApi.getAll();
            setSupplierTypes(res || []);
        } catch (err) {
            console.error(err);
            msgApi.error("خطا در دریافت نوع‌های تأمین‌کننده");
        }
    }, [msgApi]);

    // ========================
    // Load Attribute Definitions (getPaged)
    // ========================
    const loadAttributeDefinitions = useCallback(async () => {
        try {
            const res = await kycAttributeDefinitionsApi.getPaged({
                page: 1,
                pageSize: 1000,
            });

            setAttributeDefinitions(res?.items || []);
        } catch (err) {
            console.error(err);
            msgApi.error("خطا در دریافت فیلدهای KYC");
        }
    }, [msgApi]);

    // ========================
    // Load Template
    // ========================
    const loadTemplate = useCallback(
        async (supplierTypeId) => {
            if (!supplierTypeId) return;

            try {
                setLoading(true);
                const res = await kycTemplatesApi.getBySupplierType(supplierTypeId);

                const items = res?.data || [];

                setTemplateItems(
                    items.map((x) => ({
                        ...x,
                        key: x.attributeDefinitionId,
                    }))
                );
            } catch (err) {
                console.error(err);
                msgApi.error("خطا در دریافت Template");
            } finally {
                setLoading(false);
            }
        },
        [msgApi]
    );

    // ========================
    // Initial Load
    // ========================
    useEffect(() => {
        loadSupplierTypes();
        loadAttributeDefinitions();
    }, [loadSupplierTypes, loadAttributeDefinitions]);

    // ========================
    // SupplierType Change
    // ========================
    const handleSupplierTypeChange = (value) => {
        setSelectedSupplierTypeId(value);
        setTemplateItems([]);
        loadTemplate(value);
    };

    // ========================
    // Helpers
    // ========================
    const templateAttributeIds = useMemo(
        () => new Set(templateItems.map((x) => x.attributeDefinitionId)),
        [templateItems]
    );

    const getNextSortOrder = () => {
        if (templateItems.length === 0) return 1;
        return Math.max(...templateItems.map((x) => x.sortOrder)) + 1;
    };

    // ========================
    // Toggle Attribute
    // ========================
    const handleToggleAttribute = (attr, checked) => {
        if (checked) {
            setTemplateItems((prev) => [
                ...prev,
                {
                    key: attr.id,
                    attributeDefinitionId: attr.id,
                    displayName: attr.displayName,
                    dataType: attr.dataType,
                    isRequired: attr.defaultRequired || false,
                    sortOrder: getNextSortOrder(),
                },
            ]);
        } else {
            setTemplateItems((prev) =>
                prev.filter((x) => x.attributeDefinitionId !== attr.id)
            );
        }
    };

    const updateTemplateItem = (id, changes) => {
        setTemplateItems((prev) =>
            prev.map((x) =>
                x.attributeDefinitionId === id
                    ? { ...x, ...changes }
                    : x
            )
        );
    };

    // ========================
    // Save
    // ========================
    const handleSave = async () => {
        if (!selectedSupplierTypeId) return;

        try {
            setLoading(true);

            await kycTemplatesApi.upsert({
                supplierTypeId: selectedSupplierTypeId,
                requirements: templateItems.map((x) => ({
                    attributeDefinitionId: x.attributeDefinitionId,
                    isRequired: x.isRequired,
                    sortOrder: x.sortOrder,
                })),
            });

            msgApi.success("Template با موفقیت ذخیره شد");
            loadTemplate(selectedSupplierTypeId);
        } catch (err) {
            console.error(err);
            msgApi.error(
                err?.response?.data?.message ||
                "خطا در ذخیره Template"
            );
        } finally {
            setLoading(false);
        }
    };

    // ========================
    // Columns
    // ========================
    const columns = [
        { title: "عنوان", dataIndex: "displayName" },
        {
            title: "نوع داده",
            dataIndex: "dataType",
            width: 140,
            render: (v) => <Tag>{getKycDataTypeLabel(v)}</Tag>,
        },
        {
            title: "اجباری",
            width: 120,
            render: (_, r) => (
                <Switch
                    checked={r.isRequired}
                    onChange={(val) =>
                        updateTemplateItem(r.attributeDefinitionId, {
                            isRequired: val,
                        })
                    }
                />
            ),
        },
        {
            title: "ترتیب",
            width: 120,
            render: (_, r) => (
                <InputNumber
                    min={1}
                    value={r.sortOrder}
                    onChange={(val) =>
                        updateTemplateItem(r.attributeDefinitionId, {
                            sortOrder: val,
                        })
                    }
                />
            ),
        },
    ];

    return (
        <Card title="مدیریت KYC Template">
            <Row gutter={12} style={{ marginBottom: 16 }}>
                <Col span={8}>
                    <Select
                        placeholder="انتخاب نوع تأمین‌کننده"
                        value={selectedSupplierTypeId}
                        onChange={handleSupplierTypeChange}
                        allowClear
                        style={{ width: "100%" }}
                    >
                        {supplierTypes.map((s) => (
                            <Option key={s.id} value={s.id}>
                                {s.name}
                            </Option>
                        ))}
                    </Select>
                </Col>
            </Row>

            {!selectedSupplierTypeId && (
                <p style={{ color: "#999" }}>
                    لطفاً ابتدا نوع تأمین‌کننده را انتخاب کنید.
                </p>
            )}

            {selectedSupplierTypeId && (
                <>
                    <Table
                        loading={loading}
                        dataSource={templateItems}
                        columns={columns}
                        pagination={false}
                        rowKey="attributeDefinitionId"
                    />

                    <Divider />

                    <Row gutter={[16, 16]}>
                        {attributeDefinitions.map((attr) => (
                            <Col span={8} key={attr.id}>
                                <Space>
                                    <Switch
                                        checked={templateAttributeIds.has(
                                            attr.id
                                        )}
                                        onChange={(val) =>
                                            handleToggleAttribute(attr, val)
                                        }
                                    />
                                    <span>{attr.displayName}</span>
                                </Space>
                            </Col>
                        ))}
                    </Row>

                    <Divider />

                    <Button
                        type="primary"
                        onClick={handleSave}
                        loading={loading}
                    >
                        ذخیره Template
                    </Button>
                </>
            )}
        </Card>
    );
};

export default KycTemplatesPage;
