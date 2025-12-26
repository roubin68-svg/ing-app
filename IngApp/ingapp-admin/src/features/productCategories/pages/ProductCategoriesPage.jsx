// src/features/productCategories/pages/ProductCategoriesPage.jsx
import React, { useEffect, useState } from "react";
import {
    Card,
    Button,
    Modal,
    Form,
    Input,
    Switch,
    App,
    Tree,
    Spin,
    Dropdown,
    Popconfirm,
} from "antd";
import {
    PlusOutlined,
    EditOutlined,
    MoreOutlined,
    CheckCircleOutlined,
    StopOutlined,
} from "@ant-design/icons";
import productCategoryApi from "../api/productCategoryApi";

const ProductCategoriesPage = () => {
    const { message } = App.useApp();

    // ================================
    // States
    // ================================
    const [reloadFlag, setReloadFlag] = useState(false);

    const [treeData, setTreeData] = useState([]);
    const [loadingTree, setLoadingTree] = useState(false);

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editingItem, setEditingItem] = useState(null);
    const [parentId, setParentId] = useState(null);
    const [form] = Form.useForm();
    const [formKey, setFormKey] = useState(0);

    // ================================
    // Load Categories
    // ================================
    const loadCategories = async () => {
        try {
            setLoadingTree(true);
            const data = await productCategoryApi.getAll();
            setTreeData(convertToTree(data));
        } catch (e) {
            console.error(e);
            message.error(e.message || "خطا در دریافت دسته‌بندی‌ها");
        } finally {
            setLoadingTree(false);
        }
    };

    useEffect(() => {
        loadCategories();
    }, [reloadFlag]);

    // ================================
    // Convert Flat List → Tree
    // ================================
    const convertToTree = (items) => {
        if (!items || items.length === 0) return [];

        const map = {};
        items.forEach((i) => {
            map[i.id] = {
                key: i.id,
                raw: i,
                title: renderTitle(i),
                children: [],
            };
        });

        const roots = [];
        items.forEach((i) => {
            if (i.parentId) {
                map[i.parentId]?.children.push(map[i.id]);
            } else {
                roots.push(map[i.id]);
            }
        });

        return roots;
    };

    // ================================
    // Tree Node Title
    // ================================
    const renderTitle = (item) => (
        <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
            <span>{item.name}</span>
            {!item.isActive && (
                <span style={{ color: "red", fontSize: 12 }}>غیرفعال</span>
            )}

            <Dropdown
                trigger={["click"]}
                menu={{
                    items: [
                        {
                            key: "edit",
                            icon: <EditOutlined />,
                            label: "ویرایش",
                            onClick: () => openEdit(item),
                        },
                        {
                            key: "add",
                            icon: <PlusOutlined />,
                            label: "افزودن زیر‌دسته",
                            onClick: () => openCreateChild(item.id),
                        },
                        {
                            key: "status",
                            label: item.isActive ? (
                                <Popconfirm
                                    title="غیرفعال‌سازی دسته‌بندی؟"
                                    okText="بله"
                                    cancelText="خیر"
                                    onConfirm={() => deactivate(item.id)}
                                >
                                    <span style={{ color: "red" }}>
                                        <StopOutlined /> غیرفعال
                                    </span>
                                </Popconfirm>
                            ) : (
                                <Popconfirm
                                    title="فعال‌سازی دسته‌بندی؟"
                                    okText="بله"
                                    cancelText="خیر"
                                    onConfirm={() => activate(item.id)}
                                >
                                    <span style={{ color: "green" }}>
                                        <CheckCircleOutlined /> فعال
                                    </span>
                                </Popconfirm>
                            ),
                        },
                    ],
                }}
            >
                <Button type="text" icon={<MoreOutlined />} />
            </Dropdown>
        </div>
    );

    // ================================
    // Actions
    // ================================
    const activate = async (id) => {
        try {
            await productCategoryApi.activate(id);
            message.success("دسته‌بندی فعال شد");
            setReloadFlag((f) => !f);
        } catch (e) {
            console.error(e);
            message.error(e.message || "خطا در فعال‌سازی");
        }
    };

    const deactivate = async (id) => {
        try {
            await productCategoryApi.deactivate(id);
            message.success("دسته‌بندی غیرفعال شد");
            setReloadFlag((f) => !f);
        } catch (e) {
            console.error(e);
            message.error(e.message || "خطا در غیرفعال‌سازی");
        }
    };

    // ================================
    // Form Actions
    // ================================
    const openCreateRoot = () => {
        setEditingItem(null);
        setParentId(null);
        form.resetFields();
        setFormKey(Date.now());
        setIsFormOpen(true);
    };

    const openCreateChild = (pid) => {
        setEditingItem(null);
        setParentId(pid);
        form.resetFields();
        setFormKey(Date.now());
        setIsFormOpen(true);
    };

    const openEdit = (item) => {
        setEditingItem(item);
        setParentId(item.parentId ?? null);

        form.setFieldsValue({
            name: item.name,
            description: item.description,
        });

        setFormKey(item.id);
        setIsFormOpen(true);
    };

    const handleSubmit = async () => {
        try {
            const values = await form.validateFields();
            const payload = {
                ...values,
                parentId: parentId ?? null,
            };

            if (editingItem) {
                await productCategoryApi.update(editingItem.id, payload);
                message.success("دسته‌بندی ویرایش شد");
            } else {
                await productCategoryApi.create(payload);
                message.success("دسته‌بندی ایجاد شد");
            }

            setIsFormOpen(false);
            setEditingItem(null);
            setParentId(null);
            setReloadFlag((f) => !f);
        } catch (e) {
            if (e?.errorFields) return;
            console.error(e);
            message.error(e.message || "خطا در ذخیره‌سازی");
        }
    };

    // ================================
    // Render
    // ================================
    return (
        <Card
            title="مدیریت دسته‌بندی محصولات"
            bordered={false}
            extra={
                <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={openCreateRoot}
                >
                    افزودن دسته‌بندی
                </Button>
            }
        >
            {loadingTree ? (
                <Spin />
            ) : (
                <Tree
                    treeData={treeData}
                    defaultExpandAll
                    showLine
                    blockNode
                />
            )}

            <Modal
                key={formKey}
                title={editingItem ? "ویرایش دسته‌بندی" : "افزودن دسته‌بندی"}
                open={isFormOpen}
                onOk={handleSubmit}
                onCancel={() => setIsFormOpen(false)}
                okText="ذخیره"
                cancelText="انصراف"
                destroyOnClose
            >
                <Form layout="vertical" form={form}>
                    <Form.Item
                        label="نام دسته‌بندی"
                        name="name"
                        rules={[
                            { required: true, message: "نام الزامی است" },
                        ]}
                    >
                        <Input />
                    </Form.Item>

                    <Form.Item label="توضیحات" name="description">
                        <Input.TextArea rows={3} />
                    </Form.Item>
                </Form>
            </Modal>
        </Card>
    );
};

export default ProductCategoriesPage;
