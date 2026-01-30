// src/features/plans/pages/PlansManagementPage.jsx
import React, { useEffect, useState } from "react";
import {
    Card,
    Table,
    Button,
    Space,
    Tag,
    Modal,
    Form,
    Input,
    InputNumber,
    Switch,
    message,
    Popconfirm,
    Typography,
} from "antd";
import {
    PlusOutlined,
    EditOutlined,
    DeleteOutlined,
    CheckCircleOutlined,
    CloseCircleOutlined,
} from "@ant-design/icons";
import plansApi from "../api/plansApi";

const { Text } = Typography;

const PlansManagementPage = () => {
    const [loading, setLoading] = useState(true);
    const [plans, setPlans] = useState([]);
    const [pagination, setPagination] = useState({
        current: 1,
        pageSize: 20,
        total: 0,
    });
    const [modalVisible, setModalVisible] = useState(false);
    const [editingPlan, setEditingPlan] = useState(null);
    const [form] = Form.useForm();

    useEffect(() => {
        loadPlans();
    }, []);

    const loadPlans = async (page = 1, pageSize = 20) => {
        try {
            setLoading(true);
            const result = await plansApi.getPaged({ page, pageSize });
            // apiClient interceptor unwraps ApiResult
            setPlans(result?.items || []);
            setPagination({
                current: result?.page || page,
                pageSize: result?.pageSize || pageSize,
                total: result?.totalCount || 0,
            });
        } catch (error) {
            message.error("خطا در دریافت لیست پلن‌ها");
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const formatPrice = (rial) => {
        if (rial == null) return "-";
        const toman = rial / 10;
        return `${toman.toLocaleString("fa-IR")} تومان`;
    };

    const handleCreate = () => {
        setEditingPlan(null);
        form.resetFields();
        setModalVisible(true);
    };

    const handleEdit = (plan) => {
        setEditingPlan(plan);
        form.setFieldsValue({
            code: plan.code,
            title: plan.title,
            description: plan.description,
            durationMonths: plan.durationMonths,
            priceRial: plan.priceRial / 10, // تبدیل به تومان
            unlimitedContactViews: plan.unlimitedContactViews,
            isActive: plan.isActive,
            displayOrder: plan.displayOrder,
        });
        setModalVisible(true);
    };

    const handleDelete = async (id) => {
        try {
            await plansApi.delete(id);
            message.success("پلن با موفقیت حذف شد");
            loadPlans(pagination.current, pagination.pageSize);
        } catch (error) {
            const errorMsg =
                error?.response?.data?.message ||
                error?.message ||
                "خطا در حذف پلن";
            message.error(errorMsg);
            console.error(error);
        }
    };

    const handleToggleStatus = async (id, currentStatus) => {
        try {
            await plansApi.toggleStatus(id, !currentStatus);
            message.success(`پلن ${!currentStatus ? "فعال" : "غیرفعال"} شد`);
            loadPlans(pagination.current, pagination.pageSize);
        } catch (error) {
            message.error("خطا در تغییر وضعیت پلن");
            console.error(error);
        }
    };

    const handleSubmit = async (values) => {
        try {
            // تبدیل تومان به ریال
            const payload = {
                ...values,
                priceRial: values.priceRial * 10,
            };

            if (editingPlan) {
                await plansApi.update(editingPlan.id, payload);
                message.success("پلن با موفقیت به‌روزرسانی شد");
            } else {
                await plansApi.create(payload);
                message.success("پلن با موفقیت ایجاد شد");
            }

            setModalVisible(false);
            form.resetFields();
            loadPlans(pagination.current, pagination.pageSize);
        } catch (error) {
            const errorMsg =
                error?.response?.data?.message ||
                error?.message ||
                "خطا در ذخیره پلن";
            message.error(errorMsg);
            console.error(error);
        }
    };

    const columns = [
        {
            title: "کد",
            dataIndex: "code",
            key: "code",
            width: 150,
        },
        {
            title: "عنوان",
            dataIndex: "title",
            key: "title",
        },
        {
            title: "مدت (ماه)",
            dataIndex: "durationMonths",
            key: "durationMonths",
            align: "center",
            width: 100,
        },
        {
            title: "قیمت",
            dataIndex: "priceRial",
            key: "priceRial",
            render: (price) => formatPrice(price),
            align: "left",
        },
        {
            title: "دسترسی نامحدود",
            dataIndex: "unlimitedContactViews",
            key: "unlimitedContactViews",
            align: "center",
            width: 120,
            render: (value) =>
                value ? (
                    <Tag color="green" icon={<CheckCircleOutlined />}>
                        بله
                    </Tag>
                ) : (
                    <Tag color="red">خیر</Tag>
                ),
        },
        {
            title: "وضعیت",
            dataIndex: "isActive",
            key: "isActive",
            align: "center",
            width: 100,
            render: (isActive, record) => (
                <Tag
                    color={isActive ? "green" : "red"}
                    style={{ cursor: "pointer" }}
                    onClick={() => handleToggleStatus(record.id, isActive)}
                >
                    {isActive ? "فعال" : "غیرفعال"}
                </Tag>
            ),
        },
        {
            title: "ترتیب نمایش",
            dataIndex: "displayOrder",
            key: "displayOrder",
            align: "center",
            width: 100,
        },
        {
            title: "عملیات",
            key: "actions",
            width: 150,
            render: (_, record) => (
                <Space>
                    <Button
                        type="link"
                        icon={<EditOutlined />}
                        onClick={() => handleEdit(record)}
                    >
                        ویرایش
                    </Button>
                    <Popconfirm
                        title="حذف پلن"
                        description="آیا از حذف این پلن مطمئن هستید؟"
                        onConfirm={() => handleDelete(record.id)}
                        okText="بله"
                        cancelText="خیر"
                    >
                        <Button
                            type="link"
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
        <div>
            <Card
                title="مدیریت پلن‌های اشتراک"
                extra={
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={handleCreate}
                    >
                        ایجاد پلن جدید
                    </Button>
                }
            >
                <Table
                    columns={columns}
                    dataSource={plans}
                    loading={loading}
                    rowKey="id"
                    pagination={{
                        ...pagination,
                        onChange: (page, pageSize) => {
                            loadPlans(page, pageSize);
                        },
                    }}
                />
            </Card>

            {/* Modal ایجاد/ویرایش */}
            <Modal
                title={editingPlan ? "ویرایش پلن" : "ایجاد پلن جدید"}
                open={modalVisible}
                onOk={() => form.submit()}
                onCancel={() => {
                    setModalVisible(false);
                    form.resetFields();
                }}
                okText="ذخیره"
                cancelText="انصراف"
                width={600}
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSubmit}
                >
                    <Form.Item
                        label="کد پلن"
                        name="code"
                        rules={[
                            { required: true, message: "کد پلن الزامی است" },
                            { pattern: /^[A-Za-z0-9_]+$/, message: "کد پلن باید فقط شامل حروف انگلیسی، اعداد و _ باشد" },
                        ]}
                    >
                        <Input placeholder="مثال: Plan1Month" />
                    </Form.Item>

                    <Form.Item
                        label="عنوان"
                        name="title"
                        rules={[{ required: true, message: "عنوان الزامی است" }]}
                    >
                        <Input placeholder="مثال: پلن 1 ماهه" />
                    </Form.Item>

                    <Form.Item
                        label="توضیحات"
                        name="description"
                    >
                        <Input.TextArea rows={3} placeholder="توضیحات پلن (اختیاری)" />
                    </Form.Item>

                    <Form.Item
                        label="مدت اعتبار (ماه)"
                        name="durationMonths"
                        rules={[
                            { required: true, message: "مدت اعتبار الزامی است" },
                            { type: "number", min: 1, message: "مدت باید حداقل 1 ماه باشد" },
                        ]}
                    >
                        <InputNumber
                            style={{ width: "100%" }}
                            min={1}
                            placeholder="مثال: 1"
                        />
                    </Form.Item>

                    <Form.Item
                        label="قیمت (تومان)"
                        name="priceRial"
                        rules={[
                            { required: true, message: "قیمت الزامی است" },
                            { type: "number", min: 0, message: "قیمت نمی‌تواند منفی باشد" },
                        ]}
                    >
                        <InputNumber
                            style={{ width: "100%" }}
                            min={0}
                            formatter={(value) =>
                                `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                            }
                            parser={(value) => value.replace(/\$\s?|(,*)/g, "")}
                            placeholder="مثال: 100000"
                        />
                    </Form.Item>

                    <Form.Item
                        label="ترتیب نمایش"
                        name="displayOrder"
                        rules={[{ type: "number", min: 0 }]}
                    >
                        <InputNumber
                            style={{ width: "100%" }}
                            min={0}
                            placeholder="0"
                        />
                    </Form.Item>

                    <Form.Item
                        label="دسترسی نامحدود به اطلاعات تماس"
                        name="unlimitedContactViews"
                        valuePropName="checked"
                    >
                        <Switch />
                    </Form.Item>

                    <Form.Item
                        label="وضعیت"
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

export default PlansManagementPage;




















