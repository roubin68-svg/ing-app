import React, { useEffect, useState } from "react";
import {
    App,
    Button,
    Card,
    Col,
    Form,
    Input,
    Row,
    Select,
    Space,
    Table,
    Tag,
} from "antd";
import {
    PlusOutlined,
    SearchOutlined,
    ReloadOutlined,
} from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import offersApi from "../api/offersApi";
import productCategoryApi from "../../productCategories/api/productCategoryApi";
import CategoryTreeSelect from "../../products/components/CategoryTreeSelect";


const STATUS_OPTIONS = [
    { value: "Draft", label: "پیش‌نویس" },
    { value: "Published", label: "منتشر شده" },
    { value: "Cancel", label: "لغو شده" },
];

const MyOffersPage = () => {
    const { message, modal } = App.useApp();
    const navigate = useNavigate();

    const [form] = Form.useForm();

    // -----------------------
    // State
    // -----------------------
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState([]);

    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [total, setTotal] = useState(0);

    const [sortBy, setSortBy] = useState(null);
    const [sortDirection, setSortDirection] = useState(null);

    const [categories, setCategories] = useState([]);

    // -----------------------
    // Load Categories
    // -----------------------
    const loadCategories = async () => {
        try {
            const res = await productCategoryApi.getAll();
            setCategories(res || []);
        } catch {
            message.error("خطا در دریافت دسته‌بندی‌ها");
        }
    };

    // -----------------------
    // Load Offers
    // -----------------------
    const loadOffers = async (
        pageIndex = page,
        pageSizeValue = pageSize,
        sorter = {}
    ) => {
        try {
            setLoading(true);

            const filters = form.getFieldsValue();

            const res = await offersApi.getMyOffers({
                page: pageIndex,
                pageSize: pageSizeValue,
                status: filters.status || undefined,
                productName: filters.productName || undefined,
                productCategoryId: filters.productCategoryId || undefined,
                sortBy: sorter.field || sortBy,
                sortDirection:
                    sorter.order === "ascend"
                        ? "asc"
                        : sorter.order === "descend"
                            ? "desc"
                            : sortDirection,
            });

            setData(res.items || []);
            setTotal(res.totalCount);
            setPage(res.page);
            setPageSize(res.pageSize);
        } catch (e) {
            console.error(e);
            message.error("خطا در دریافت آگهی‌ها");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadCategories();
        loadOffers(1);
    }, []);

    // -----------------------
    // Actions
    // -----------------------
    const handleSearch = () => {
        setPage(1);
        loadOffers(1);
    };

    const handleClear = () => {
        form.resetFields();

        setSortBy(null);
        setSortDirection(null);

        setPage(1);
        loadOffers(1, pageSize, {});
    };


    const handleCreate = () => {
        navigate("/supplier/offers/manage");
    };

    const handleCancel = (offerId) => {
        modal.confirm({
            title: "لغو آگهی",
            content: "آیا از لغو این آگهی مطمئن هستید؟",
            okText: "لغو",
            cancelText: "انصراف",
            onOk: async () => {
                try {
                    await offersApi.cancel(
                        offerId,
                        "Cancelled by supplier"
                    );
                    message.success("آگهی لغو شد");
                    loadOffers();
                } catch {
                    message.error("خطا در لغو آگهی");
                }
            },
        });
    };

    // -----------------------
    // Table Columns
    // -----------------------
    const columns = [
        {
            title: "شناسه",
            dataIndex: "id",
            width: 90,
            sorter: true,
        },
        {
            title: "محصول",
            dataIndex: "productName",
            sorter: true,
        },
        {
            title: "دسته‌بندی",
            dataIndex: "productCategoryName",
        },
        {
            title: "وضعیت",
            dataIndex: "status",
            width: 140,
            render: (status) => {
                if (status === "Draft")
                    return <Tag color="gold">پیش‌نویس</Tag>;
                if (status === "Published")
                    return <Tag color="green">منتشر شده</Tag>;
                if (status === "Cancel")
                    return <Tag color="red">لغو شده</Tag>;
                return status;
            },
        },
        {
            title: "تاریخ ایجاد",
            dataIndex: "createdAt",
            sorter: true,
        },
        {
            title: "عملیات",
            width: 220,
            render: (_, record) => (
                <Space>
                    <Button
                        type="link"
                        onClick={() =>
                            navigate(
                                `/supplier/offers/manage/${record.id}`
                            )
                        }
                    >
                        {record.status === "Draft"
                            ? "ادامه"
                            : "مشاهده"}
                    </Button>

                    {record.status !== "Cancel" && (
                        <Button
                            danger
                            type="link"
                            onClick={() =>
                                handleCancel(record.id)
                            }
                        >
                            لغو
                        </Button>
                    )}
                </Space>
            ),
        },
    ];

    // -----------------------
    // Render
    // -----------------------
    return (
        <Card
            title="مدیریت آگهی‌ها"
            extra={
                <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={handleCreate}
                >
                    ثبت آگهی جدید
                </Button>
            }
        >
            {/* Filters */}
            <Form
                form={form}
                layout="vertical"
                style={{ marginBottom: 16 }}
            >
                <Row gutter={16}>
                    <Col span={6}>
                        <Form.Item
                            label="نام محصول"
                            name="productName"
                        >
                            <Input placeholder="جستجو بر اساس محصول" />
                        </Form.Item>
                    </Col>

                    <Col span={6}>
                        <Form.Item
                            label="دسته‌بندی"
                            name="productCategoryId"
                        >
                            <CategoryTreeSelect
                                allowClear
                                placeholder="انتخاب دسته‌بندی"
                            />
                        </Form.Item>
                    </Col>

                    <Col span={6}>
                        <Form.Item label="وضعیت" name="status">
                            <Select
                                allowClear
                                placeholder="انتخاب وضعیت"
                                options={STATUS_OPTIONS}
                            />
                        </Form.Item>
                    </Col>

                    <Col span={6} style={{ marginTop: 30 }}>
                        <Space>
                            <Button
                                type="primary"
                                icon={<SearchOutlined />}
                                onClick={handleSearch}
                            >
                                جستجو
                            </Button>
                            <Button
                                icon={<ReloadOutlined />}
                                onClick={handleClear}
                            >
                                پاکسازی
                            </Button>
                        </Space>
                    </Col>
                </Row>
            </Form>

            {/* Table */}
            <Table
                rowKey="id"
                loading={loading}
                columns={columns}
                dataSource={data}
                pagination={{
                    current: page,
                    pageSize,
                    total,
                    showSizeChanger: true,
                }}
                onChange={(pagination, _, sorter) => {
                    setPage(pagination.current);
                    setPageSize(pagination.pageSize);

                    if (sorter?.order) {
                        setSortBy(sorter.field);
                        setSortDirection(
                            sorter.order === "ascend"
                                ? "asc"
                                : "desc"
                        );
                    }

                    loadOffers(
                        pagination.current,
                        pagination.pageSize,
                        sorter
                    );
                }}                
            />
        </Card>
    );
};

export default MyOffersPage;
