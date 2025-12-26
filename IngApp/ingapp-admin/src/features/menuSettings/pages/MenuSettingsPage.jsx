// src/features/menuSettings/pages/MenuSettingsPage.jsx

import React, { useState, useEffect } from "react";
import {
    Card,
    Button,
    Modal,
    Form,
    Input,
    Switch,
    InputNumber,
    App,
    Tree,
    Spin,
    Dropdown,
    Popconfirm,
} from "antd";
import {
    PlusOutlined,
    EditOutlined,
    DeleteOutlined,
    MoreOutlined,
} from "@ant-design/icons";
import menuApi from "../api/menuApi";

// =====================================================
//  MAIN PAGE
// =====================================================
const MenuSettingsPage = () => {
    const { message } = App.useApp();
    const [reloadFlag, setReloadFlag] = useState(false);

    // فرم و مودال
    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editingItem, setEditingItem] = useState(null);
    const [parentId, setParentId] = useState(null);
    const [form] = Form.useForm();
    const [formKey, setFormKey] = useState(0);

    // درخت
    const [treeData, setTreeData] = useState([]);
    const [loadingTree, setLoadingTree] = useState(false);

    // ================================
    // Load Menu Tree
    // ================================
    const loadTree = async () => {
        try {
            setLoadingTree(true);
            const data = await menuApi.getAdminTree();
            setTreeData(convertToAntTree(data));
        } catch (e) {
            console.error(e);
            message.error(e.message || "خطا در دریافت منوها");
        } finally {
            setLoadingTree(false);
        }
    };

    useEffect(() => {
        loadTree();
    }, [reloadFlag]);

    // ================================
    // TREE → ANT FORMAT
    // ================================
    const convertToAntTree = (items) =>
        (items || []).map((m) => ({
            key: m.id,
            id: m.id,
            parentId: m.parentId,
            order: m.order,
            raw: m,
            title: (
                <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                    <span>{m.title}</span>
                    {!m.isActive && (
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
                                    onClick: () => openEdit(m),
                                },
                                {
                                    key: "add",
                                    icon: <PlusOutlined />,
                                    label: "افزودن زیرمنو",
                                    onClick: () => openCreateChild(m.id),
                                },
                                {
                                    key: "delete",
                                    label: (
                                        <Popconfirm
                                            title="حذف منو؟"
                                            okText="بله"
                                            cancelText="خیر"
                                            onConfirm={() => deleteItem(m.id)}
                                        >
                                            <span style={{ color: "red" }}>
                                                <DeleteOutlined /> حذف
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
            ),
            children:
                m.children && m.children.length > 0
                    ? convertToAntTree(m.children)
                    : [],
        }));

    // ================================
    // DELETE
    // ================================
    const deleteItem = async (id) => {
        try {
            await menuApi.remove(id);
            message.success("منو حذف شد");
            setReloadFlag((f) => !f);
        } catch (e) {
            console.error(e);
            message.error(e.message || "خطا در حذف منو");
        }
    };

    // ================================
    // DRAG & DROP
    // ================================
    const handleDrop = async (info) => {
        const dragNode = info.dragNode.raw;
        const dropNode = info.node.raw;

        if (!dragNode || !dropNode) return;
        if (dragNode.id === dropNode.id) return;

        let newParentId = dragNode.parentId ?? null;
        let newOrder = dragNode.order || 1;

        const posArr = info.node.pos.split("-");
        const dropIndex = Number(posArr[posArr.length - 1]);
        const relativePos = info.dropPosition - dropIndex;

        if (info.dropToGap === false) {
            newParentId = dropNode.id;
            newOrder = 1_000_000;
        } else {
            newParentId = dropNode.parentId ?? null;
            if (relativePos < 0) {
                newOrder = dropNode.order || 1;
            } else {
                newOrder = (dropNode.order || 1) + 1;
            }
        }

        try {
            if (newParentId !== dragNode.parentId) {
                await menuApi.changeParent(dragNode.id, newParentId);
            }
            await menuApi.changeOrder(dragNode.id, newOrder);

            message.success("ساختار منو بروزرسانی شد");
            setReloadFlag((f) => !f);
        } catch (e) {
            console.error(e);
            message.error(e.message || "خطا در جابجایی منو");
        }
    };

    // ===================================================
    //  FORM ACTIONS
    // ===================================================
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
            title: item.title,
            key: item.key,
            route: item.route,
            icon: item.icon,
            order: item.order,
            requiredPermissionCode: item.requiredPermissionCode,
            isActive: item.isActive,
        });

        setFormKey(item.id);
        setIsFormOpen(true);
    };

    const handleSubmit = async () => {
        try {
            const values = await form.validateFields();
            const payload = { ...values, parentId: parentId ?? null };

            if (editingItem) {
                await menuApi.update(editingItem.id, payload);
                message.success("منو با موفقیت ویرایش شد");
            } else {
                await menuApi.create(payload);
                message.success("منو با موفقیت ایجاد شد");
            }

            setIsFormOpen(false);
            setEditingItem(null);
            setParentId(null);
            setReloadFlag((f) => !f);
        } catch (e) {
            if (e?.errorFields) return;
            console.error(e);
            message.error(e.message || "خطا در ذخیره‌سازی منو");
        }
    };

    // ===================================================
    //  PAGE RENDER
    // ===================================================
    return (
        <Card
            title="مدیریت منو"
            bordered={false}
            extra={
                <Button type="primary" icon={<PlusOutlined />} onClick={openCreateRoot}>
                    افزودن منوی جدید
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
                    draggable
                    blockNode
                    onDrop={handleDrop}
                />
            )}

            {/* FORM MODAL */}
            <Modal
                key={formKey}
                title={editingItem ? "ویرایش منو" : "افزودن منو"}
                open={isFormOpen}
                onOk={handleSubmit}
                onCancel={() => setIsFormOpen(false)}
                okText="ذخیره"
                cancelText="انصراف"
                destroyOnClose
            >
                <Form layout="vertical" form={form}>
                    <Form.Item
                        label="عنوان"
                        name="title"
                        rules={[{ required: true, message: "عنوان الزامی است" }]}
                    >
                        <Input placeholder="مثال: تنظیمات" />
                    </Form.Item>

                    <Form.Item
                        label="Key"
                        name="key"
                        rules={[{ required: true, message: "Key الزامی است" }]}
                    >
                        <Input placeholder="menu-settings" />
                    </Form.Item>

                    <Form.Item label="Route" name="route">
                        <Input placeholder="/menu-settings" />
                    </Form.Item>

                    <Form.Item label="آیکن" name="icon">
                        <Input placeholder="SettingOutlined" />
                    </Form.Item>

                    <Form.Item label="ترتیب نمایش" name="order">
                        <InputNumber style={{ width: "100%" }} />
                    </Form.Item>

                    <Form.Item label="Permission لازم" name="requiredPermissionCode">
                        <Input placeholder="مثال: Menu.Manage" />
                    </Form.Item>

                    <Form.Item
                        label="فعال"
                        name="isActive"
                        valuePropName="checked"
                    >
                        <Switch />
                    </Form.Item>
                </Form>
            </Modal>
        </Card>
    );
};

export default MenuSettingsPage;
