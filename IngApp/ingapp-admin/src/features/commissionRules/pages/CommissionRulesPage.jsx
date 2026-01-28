// src/features/commissionRules/pages/CommissionRulesPage.jsx
import React, { useEffect, useState } from "react";
import {
    Card,
    Table,
    Button,
    Space,
    message,
    Modal,
    Form,
    Input,
    InputNumber,
    Switch,
    Tag,
    Popconfirm,
} from "antd";
import { PlusOutlined, EditOutlined, DeleteOutlined } from "@ant-design/icons";
import { DatePicker as JalaliDatePicker } from "antd-jalali";
import commissionRulesApi from "../api/commissionRulesApi";
import { toShamsiDayjs, toGregorianISO, toShamsiString, ensureShamsiDayjs, todayShamsi } from "../../../core/utils/dateUtils";

const { TextArea } = Input;

const CommissionRulesPage = () => {
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState([]);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingRule, setEditingRule] = useState(null);
    const [form] = Form.useForm();

    useEffect(() => {
        loadRules();
    }, []);

    const loadRules = async () => {
        try {
            setLoading(true);
            const rules = await commissionRulesApi.getAll();
            setData(rules || []);
        } catch (error) {
            message.error("خطا در دریافت لیست قوانین پورسانت");
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const openCreateModal = () => {
        setEditingRule(null);
        form.resetFields();
        form.setFieldsValue({
            isActive: true,
        });
        setIsModalOpen(true);
    };

    const openEditModal = (rule) => {
        setEditingRule(rule);
        form.setFieldsValue({
            title: rule.title,
            description: rule.description,
            commissionPercentage: rule.commissionPercentage,
            isActive: rule.isActive,
            effectiveFrom: rule.effectiveFrom ? toShamsiDayjs(rule.effectiveFrom) : null,
            effectiveTo: rule.effectiveTo ? toShamsiDayjs(rule.effectiveTo) : null,
        });
        setIsModalOpen(true);
    };

    const handleModalCancel = () => {
        setIsModalOpen(false);
        setEditingRule(null);
        form.resetFields();
    };

    const handleFormFinish = async (values) => {
        try {
            if (editingRule) {
                await commissionRulesApi.update(editingRule.id, {
                    title: values.title,
                    description: values.description,
                    commissionPercentage: values.commissionPercentage,
                    isActive: values.isActive,
                    effectiveFrom: values.effectiveFrom ? toGregorianISO(values.effectiveFrom) : null,
                    effectiveTo: values.effectiveTo ? toGregorianISO(values.effectiveTo) : null,
                });
                message.success("قانون پورسانت با موفقیت به‌روزرسانی شد");
            } else {
                await commissionRulesApi.create({
                    code: values.code,
                    title: values.title,
                    description: values.description,
                    commissionPercentage: values.commissionPercentage,
                    isActive: values.isActive,
                    effectiveFrom: values.effectiveFrom ? toGregorianISO(values.effectiveFrom) : null,
                    effectiveTo: values.effectiveTo ? toGregorianISO(values.effectiveTo) : null,
                });
                message.success("قانون پورسانت با موفقیت ایجاد شد");
            }
            handleModalCancel();
            loadRules();
        } catch (error) {
            const errorMsg = error?.response?.data?.message || error?.message || "خطا در ذخیره قانون پورسانت";
            message.error(errorMsg);
        }
    };

    const handleDelete = async (id) => {
        try {
            await commissionRulesApi.delete(id);
            message.success("قانون پورسانت با موفقیت حذف شد");
            loadRules();
        } catch (error) {
            const errorMsg = error?.response?.data?.message || error?.message || "خطا در حذف قانون پورسانت";
            message.error(errorMsg);
        }
    };

    const columns = [
        {
            title: "کد",
            dataIndex: "code",
            key: "code",
            render: (code) => <Tag color="blue">{code}</Tag>,
        },
        {
            title: "عنوان",
            dataIndex: "title",
            key: "title",
        },
        {
            title: "درصد پورسانت",
            dataIndex: "commissionPercentage",
            key: "commissionPercentage",
            render: (percent) => `${percent}%`,
            align: "left",
        },
        {
            title: "وضعیت",
            dataIndex: "isActive",
            key: "isActive",
            render: (isActive) => (
                <Tag color={isActive ? "green" : "red"}>
                    {isActive ? "فعال" : "غیرفعال"}
                </Tag>
            ),
        },
        {
            title: "تاریخ شروع",
            dataIndex: "effectiveFrom",
            key: "effectiveFrom",
            render: (date) => toShamsiString(date),
        },
        {
            title: "تاریخ پایان",
            dataIndex: "effectiveTo",
            key: "effectiveTo",
            render: (date) => toShamsiString(date),
        },
        {
            title: "عملیات",
            key: "actions",
            fixed: "right",
            width: 180,
            render: (_, record) => (
                <Space size="small">
                    <Button
                        size="small"
                        icon={<EditOutlined />}
                        onClick={() => openEditModal(record)}
                    >
                        ویرایش
                    </Button>
                    <Popconfirm
                        title="آیا از حذف این قانون پورسانت مطمئن هستید؟"
                        onConfirm={() => handleDelete(record.id)}
                        okText="بله"
                        cancelText="خیر"
                    >
                        <Button
                            size="small"
                            danger
                            icon={<DeleteOutlined />}
                        >
                            حذف
                        </Button>
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <div style={{ padding: "24px" }}>
            <Card
                title="مدیریت قوانین پورسانت"
                extra={
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={openCreateModal}
                    >
                        افزودن قانون جدید
                    </Button>
                }
            >
                <Table
                    columns={columns}
                    dataSource={data}
                    loading={loading}
                    rowKey="id"
                    pagination={false}
                />
            </Card>

            <Modal
                title={editingRule ? "ویرایش قانون پورسانت" : "افزودن قانون پورسانت جدید"}
                open={isModalOpen}
                onCancel={handleModalCancel}
                footer={[
                    <Button key="cancel" onClick={handleModalCancel}>
                        انصراف
                    </Button>,
                    <Button key="submit" type="primary" onClick={() => form.submit()}>
                        {editingRule ? "ذخیره تغییرات" : "ایجاد"}
                    </Button>,
                ]}
                width={600}
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleFormFinish}
                >
                    {!editingRule && (
                        <Form.Item
                            label="کد قانون"
                            name="code"
                            rules={[
                                { required: true, message: "کد قانون الزامی است" },
                                { max: 50, message: "کد قانون نمی‌تواند بیشتر از 50 کاراکتر باشد" },
                            ]}
                        >
                            <Input placeholder="مثال: UnlockContactCommission" />
                        </Form.Item>
                    )}

                    <Form.Item
                        label="عنوان"
                        name="title"
                        rules={[
                            { required: true, message: "عنوان الزامی است" },
                            { max: 200, message: "عنوان نمی‌تواند بیشتر از 200 کاراکتر باشد" },
                        ]}
                    >
                        <Input placeholder="عنوان قانون پورسانت" />
                    </Form.Item>

                    <Form.Item
                        label="توضیحات"
                        name="description"
                        rules={[
                            { max: 1000, message: "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد" },
                        ]}
                    >
                        <TextArea rows={3} placeholder="توضیحات قانون پورسانت" />
                    </Form.Item>

                    <Form.Item
                        label="درصد پورسانت"
                        name="commissionPercentage"
                        rules={[
                            { required: true, message: "درصد پورسانت الزامی است" },
                            { type: "number", min: 0, max: 100, message: "درصد پورسانت باید بین 0 تا 100 باشد" },
                        ]}
                    >
                        <InputNumber
                            min={0}
                            max={100}
                            step={0.01}
                            style={{ width: "100%" }}
                            placeholder="مثال: 10 برای 10%"
                            formatter={(value) => `${value}%`}
                            parser={(value) => value.replace('%', '')}
                        />
                    </Form.Item>

                    <Form.Item
                        label="تاریخ شروع اعتبار"
                        name="effectiveFrom"
                        dependencies={["effectiveTo"]}
                        getValueFromEvent={(date) => {
                            // اطمینان از اینکه تاریخ شمسی است
                            const shamsiDate = ensureShamsiDayjs(date);
                            // بعد از تغییر، تاریخ پایان را هم validate کن
                            setTimeout(() => {
                                form.validateFields(["effectiveTo"]);
                            }, 0);
                            return shamsiDate;
                        }}
                        rules={[
                            {
                                validator: (_, value) => {
                                    if (!value) return Promise.resolve();
                                    const effectiveTo = form.getFieldValue("effectiveTo");
                                    if (effectiveTo && value.isAfter(effectiveTo)) {
                                        return Promise.reject(new Error("تاریخ شروع باید قبل از تاریخ پایان باشد"));
                                    }
                                    return Promise.resolve();
                                },
                            },
                        ]}
                    >
                        <JalaliDatePicker
                            style={{ width: "100%" }}
                            format="YYYY/MM/DD"
                            placeholder="انتخاب تاریخ"
                            defaultPickerValue={todayShamsi()}
                        />
                    </Form.Item>

                    <Form.Item
                        label="تاریخ پایان اعتبار"
                        name="effectiveTo"
                        dependencies={["effectiveFrom"]}
                        getValueFromEvent={(date) => {
                            // اطمینان از اینکه تاریخ شمسی است
                            const shamsiDate = ensureShamsiDayjs(date);
                            // بعد از تغییر، تاریخ شروع را هم validate کن
                            setTimeout(() => {
                                form.validateFields(["effectiveFrom"]);
                            }, 0);
                            return shamsiDate;
                        }}
                        rules={[
                            {
                                validator: (_, value) => {
                                    if (!value) return Promise.resolve();
                                    const effectiveFrom = form.getFieldValue("effectiveFrom");
                                    if (effectiveFrom && value.isBefore(effectiveFrom)) {
                                        return Promise.reject(new Error("تاریخ پایان باید بعد از تاریخ شروع باشد"));
                                    }
                                    return Promise.resolve();
                                },
                            },
                        ]}
                    >
                        <JalaliDatePicker
                            style={{ width: "100%" }}
                            format="YYYY/MM/DD"
                            placeholder="انتخاب تاریخ"
                            defaultPickerValue={todayShamsi()}
                        />
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
        </div>
    );
};

export default CommissionRulesPage;

