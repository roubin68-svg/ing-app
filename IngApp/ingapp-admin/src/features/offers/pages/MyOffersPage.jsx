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
    EyeOutlined,
    DeleteOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";
import jalaali from "jalaali-js";
import { useNavigate } from "react-router-dom";
import offersApi from "../api/offersApi";
import productCategoryApi from "../../productCategories/api/productCategoryApi";
import CategoryTreeSelect from "../../products/components/CategoryTreeSelect";


const STATUS_OPTIONS = [
    { value: "Draft", label: "پیش‌نویس" },
    { value: "Published", label: "منتشر شده" },
    { value: "Cancel", label: "لغو شده" },
];

// تبدیل تاریخ میلادی به شمسی
const toShamsi = (gregorian) => {
    if (!gregorian) return null;
    
    // اگر string است
    if (typeof gregorian === "string") {
        const [y, m, d] = gregorian.split("T")[0].split("-").map(Number);
        const j = jalaali.toJalaali(y, m, d);
        return `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
    }
    
    // اگر Date object است
    if (gregorian instanceof Date) {
        const j = jalaali.toJalaali(
            gregorian.getFullYear(),
            gregorian.getMonth() + 1,
            gregorian.getDate()
        );
        return `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
    }
    
    return null;
};

// تبدیل Status enum به string
const getStatusString = (status) => {
    // Status می‌تواند عدد (enum) یا string باشد
    if (typeof status === "number") {
        switch (status) {
            case 0: return "Draft";
            case 1: return "Pending";
            case 3: return "Published";
            case 4: return "Cancel";
            case 5: return "Rejected";
            default: return "Draft";
        }
    }
    return status;
};

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
                offerId: filters.offerId ? Number(filters.offerId) : undefined,
                status: filters.status || undefined,
                productName: filters.productName || undefined,
                productCategoryId: filters.productCategoryId || undefined,
                sortBy: sorter.field || sortBy || undefined,
                sortDirection:
                    sorter.order === "ascend"
                        ? "asc"
                        : sorter.order === "descend"
                            ? "desc"
                            : sortDirection || undefined,
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
                const statusStr = getStatusString(status);
                if (statusStr === "Draft")
                    return <Tag color="gold">پیش‌نویس</Tag>;
                if (statusStr === "Published")
                    return <Tag color="green">منتشر شده</Tag>;
                if (statusStr === "Cancel")
                    return <Tag color="red">لغو شده</Tag>;
                if (statusStr === "Pending")
                    return <Tag color="blue">در انتظار</Tag>;
                if (statusStr === "Rejected")
                    return <Tag color="red">رد شده</Tag>;
                return statusStr || status;
            },
        },
        {
            title: "تاریخ ایجاد",
            dataIndex: "createdAt",
            sorter: true,
            render: (date) => {
                const shamsiDate = toShamsi(date);
                return shamsiDate || "-";
            },
        },
        {
            title: "تعداد بازدید",
            dataIndex: "viewCount",
            width: 120,
            sorter: true,
            render: (count) => count ?? 0,
        },
        {
            title: "کلیک تماس",
            dataIndex: "contactClickCount",
            width: 120,
            sorter: true,
            render: (count) => count ?? 0,
        },
        {
            title: "عملیات",
            width: 220,
            render: (_, record) => {
                const statusStr = getStatusString(record.status);
                return (
                    <Space>
                        <Button
                            icon={<EyeOutlined />}
                            onClick={() =>
                                navigate(
                                    `/supplier/offers/manage/${record.id}`
                                )
                            }
                        >
                            {statusStr === "Draft"
                                ? "ادامه"
                                : "مشاهده"}
                        </Button>

                        {statusStr !== "Cancel" && (
                            <Button
                                danger
                                icon={<DeleteOutlined />}
                                onClick={() =>
                                    handleCancel(record.id)
                                }
                            >
                                لغو
                            </Button>
                        )}
                    </Space>
                );
            },
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
                            label="کد آگهی"
                            name="offerId"
                        >
                            <Input 
                                placeholder="جستجو بر اساس کد آگهی" 
                                type="number"
                            />
                        </Form.Item>
                    </Col>

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
                        // تبدیل نام فیلد به format مورد نیاز backend
                        let sortField = sorter.field;
                        if (sorter.field === "viewCount") {
                            sortField = "viewCount";
                        } else if (sorter.field === "contactClickCount") {
                            sortField = "contactClickCount";
                        } else if (sorter.field === "productName") {
                            sortField = "productName";
                        } else if (sorter.field === "createdAt") {
                            sortField = "createdAt";
                        } else if (sorter.field === "id") {
                            sortField = "createdAt"; // شناسه را بر اساس تاریخ ایجاد sort می‌کنیم
                        }
                        
                        setSortBy(sortField);
                        setSortDirection(
                            sorter.order === "ascend"
                                ? "asc"
                                : "desc"
                        );
                    } else {
                        setSortBy(null);
                        setSortDirection(null);
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
