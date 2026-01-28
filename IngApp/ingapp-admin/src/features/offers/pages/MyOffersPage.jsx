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
    Drawer,
    Descriptions,
    Tabs,
    Alert,
    Spin,
} from "antd";
import {
    PlusOutlined,
    SearchOutlined,
    ReloadOutlined,
    EyeOutlined,
    DeleteOutlined,
    HistoryOutlined,
    DownloadOutlined,
    FilePdfOutlined,
    FileWordOutlined,
    FileOutlined,
    CloseCircleOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";
import jalaali from "jalaali-js";
import { useNavigate } from "react-router-dom";
import offersApi from "../api/offersApi";
import productCategoryApi from "../../productCategories/api/productCategoryApi";
import CategoryTreeSelect from "../../products/components/CategoryTreeSelect";
import apiClient from "../../../core/api/apiClient";


const STATUS_OPTIONS = [
    { value: "Draft", label: "پیش‌نویس" },
    { value: "Published", label: "منتشر شده" },
    { value: "Cancel", label: "لغو شده" },
    { value: "Rejected", label: "رد شده" },
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

// تبدیل تاریخ میلادی به شمسی با ساعت و دقیقه
const toShamsiWithTime = (gregorian) => {
    if (!gregorian) return { date: "-", time: "" };
    
    let dateObj;
    if (typeof gregorian === "string") {
        dateObj = new Date(gregorian);
    } else if (gregorian instanceof Date) {
        dateObj = gregorian;
    } else {
        return { date: "-", time: "" };
    }
    
    const year = dateObj.getFullYear();
    const month = dateObj.getMonth() + 1;
    const day = dateObj.getDate();
    const hour = dateObj.getHours();
    const minute = dateObj.getMinutes();
    
    const j = jalaali.toJalaali(year, month, day);
    const shamsiDate = `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
    const time = `${String(hour).padStart(2, "0")}:${String(minute).padStart(2, "0")}`;
    
    return { date: shamsiDate, time };
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

    // Detail drawer (with tabs)
    const [detailDrawerVisible, setDetailDrawerVisible] = useState(false);
    const [selectedOfferDetail, setSelectedOfferDetail] = useState(null);
    const [loadingDetail, setLoadingDetail] = useState(false);
    const [historyData, setHistoryData] = useState([]);
    const [loadingHistory, setLoadingHistory] = useState(false);
    const [attributeTemplates, setAttributeTemplates] = useState([]);

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
                    await offersApi.cancel(offerId);
                    message.success("آگهی لغو شد");
                    loadOffers();
                } catch {
                    message.error("خطا در لغو آگهی");
                }
            },
        });
    };

    const handleViewOffer = async (offerId) => {
        setDetailDrawerVisible(true);
        setLoadingDetail(true);
        setLoadingHistory(true);
        
        try {
            // Load offer detail
            const detailRes = await offersApi.getMyOfferDetail(offerId);
            const detail = detailRes?.data || detailRes;
            setSelectedOfferDetail(detail);
            
            // Load attribute templates if productId exists
            if (detail?.header?.productId) {
                try {
                    const templatesRes = await offersApi.getProductAttributeTemplates(detail.header.productId);
                    const templates = templatesRes?.data ?? templatesRes ?? [];
                    setAttributeTemplates(templates);
                } catch (e) {
                    console.error("Error loading attribute templates:", e);
                    setAttributeTemplates([]);
                }
            }
            
            // Load history
            const historyRes = await offersApi.getOfferStatusHistory(offerId);
            const history = historyRes?.data || historyRes;
            setHistoryData(history || []);
        } catch (e) {
            console.error(e);
            message.error("خطا در دریافت اطلاعات آگهی");
        } finally {
            setLoadingDetail(false);
            setLoadingHistory(false);
        }
    };

    const handleCloseDetailDrawer = () => {
        setDetailDrawerVisible(false);
        setSelectedOfferDetail(null);
        setHistoryData([]);
        setAttributeTemplates([]);
    };

    const formatPrice = (v) =>
        v != null
            ? v.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
            : "-";

    const getStatusColor = (status) => {
        const statusStr = getStatusString(status);
        switch (statusStr) {
            case "Draft": return "gold";
            case "Pending": return "blue";
            case "Published": return "green";
            case "Cancel": return "red";
            case "Rejected": return "red";
            default: return "default";
        }
    };

    const getStatusLabel = (status) => {
        const statusStr = getStatusString(status);
        const option = STATUS_OPTIONS.find(opt => opt.value === statusStr);
        return option?.label || statusStr;
    };

    // -----------------------
    // Table Columns
    // -----------------------
    const columns = [
        {
            title: "شناسه",
            dataIndex: "id",

            sorter: true,
        },
        {
            title: "محصول",
            dataIndex: "productName",
            sorter: true,
            render: (productName, record) => (
                <div>
                    <div style={{ fontWeight: 500 }}>{productName || "-"}</div>
                    <div style={{ fontSize: 12, color: "#999", marginTop: 4 }}>
                        {record.productCategoryName || "-"}
                    </div>
                </div>
            ),
        },
        {
            title: "وضعیت",
            dataIndex: "status",

            render: (status) => {
                const statusStr = getStatusString(status);
                return <Tag color={getStatusColor(status)}>{getStatusLabel(status)}</Tag>;
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
            sorter: true,
            render: (count) => count ?? 0,
        },
        {
            title: "کلیک تماس",
            dataIndex: "contactClickCount",
            sorter: true,
            render: (count) => count ?? 0,
        },
        {
            title: "عملیات",
            render: (_, record) => {
                const statusStr = getStatusString(record.status);
                return (
                    <Space>
                        {statusStr === "Draft" ? (
                            <Button
                                icon={<EyeOutlined />}
                                onClick={() =>
                                    navigate(
                                        `/supplier/offers/manage/${record.id}`
                                    )
                                }
                            >
                                ادامه
                            </Button>
                        ) : (
                            <Button
                                icon={<EyeOutlined />}
                                onClick={() => handleViewOffer(record.id)}
                            >
                                مشاهده
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
                <Row gutter={[16, 16]}>
                    <Col xs={24} sm={12} md={8} lg={6}>
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

                    <Col xs={24} sm={12} md={8} lg={6}>
                        <Form.Item
                            label="نام محصول"
                            name="productName"
                        >
                            <Input placeholder="جستجو بر اساس محصول" />
                        </Form.Item>
                    </Col>

                    <Col xs={24} sm={12} md={8} lg={6}>
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

                    <Col xs={24} sm={12} md={8} lg={6}>
                        <Form.Item label="وضعیت" name="status">
                            <Select
                                allowClear
                                placeholder="انتخاب وضعیت"
                                options={STATUS_OPTIONS}
                            />
                        </Form.Item>
                    </Col>

                    <Col xs={24} sm={24} md={24} lg={24}>
                        <Form.Item label=" " colon={false}>
                            <Space wrap>
                            <Button
                                type="primary"
                                icon={<SearchOutlined />}
                                onClick={handleSearch}
                                    block={window.innerWidth < 768}
                            >
                                جستجو
                            </Button>
                            <Button
                                icon={<ReloadOutlined />}
                                onClick={handleClear}
                                    block={window.innerWidth < 768}
                            >
                                پاکسازی
                            </Button>
                        </Space>
                        </Form.Item>
                    </Col>
                </Row>
            </Form>

            {/* Table */}
            <Table
                rowKey="id"
                loading={loading}
                columns={columns}
                dataSource={data}
                scroll={{ x: 'max-content' }}
                pagination={{
                    current: page,
                    pageSize,
                    total,
                    showSizeChanger: true,
                    responsive: true,
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

            {/* Detail Drawer with Tabs */}
            <Drawer
                title="جزئیات آگهی"
                placement="right"
                width={800}
                open={detailDrawerVisible}
                onClose={handleCloseDetailDrawer}
                destroyOnClose
            >
                {loadingDetail ? (
                    <div style={{ textAlign: "center", padding: 48 }}>
                        <Spin size="large" />
                    </div>
                ) : !selectedOfferDetail ? (
                    <div>اطلاعاتی یافت نشد</div>
                ) : (
                    <Tabs
                        defaultActiveKey="details"
                        items={[
                            {
                                key: "details",
                                label: "جزئیات آگهی",
                                children: (
                                    <div>
                                        {/* Offer Details */}
                                        <Descriptions
                                            bordered
                                            column={2}
                                            size="small"
                                            style={{ marginBottom: 24 }}
                                        >
                                            <Descriptions.Item label="وضعیت" span={2}>
                                                <Tag color={getStatusColor(selectedOfferDetail.header?.status)}>
                                                    {getStatusLabel(selectedOfferDetail.header?.status)}
                                                </Tag>
                                            </Descriptions.Item>

                                            {selectedOfferDetail.header?.rejectedReason && (
                                                <Descriptions.Item label="دلیل رد" span={2}>
                                                    {selectedOfferDetail.header.rejectedReason}
                                                </Descriptions.Item>
                                            )}

                                            <Descriptions.Item label="دسته محصول">
                                                {selectedOfferDetail.header?.productCategoryName || "-"}
                                            </Descriptions.Item>
                                            <Descriptions.Item label="نام محصول">
                                                {selectedOfferDetail.header?.productName || "-"}
                                            </Descriptions.Item>

                                            <Descriptions.Item label="قیمت واحد">
                                                {formatPrice(selectedOfferDetail.header?.unitPrice)} تومان
                                            </Descriptions.Item>
                                            <Descriptions.Item label="مقدار">
                                                {selectedOfferDetail.header?.quantity ?? "-"} {selectedOfferDetail.header?.unit || ""}
                                            </Descriptions.Item>

                                            <Descriptions.Item label="قیمت کل">
                                                {formatPrice(selectedOfferDetail.header?.totalPrice)} تومان
                                            </Descriptions.Item>
                                            {selectedOfferDetail.header?.hasTax && (
                                                <Descriptions.Item label="مبلغ مالیات">
                                                    {formatPrice(selectedOfferDetail.header?.taxAmount)} تومان
                                                </Descriptions.Item>
                                            )}
                                            {selectedOfferDetail.header?.hasTax && (
                                                <Descriptions.Item label="قیمت کل + مالیات" span={2}>
                                                    <span style={{ fontWeight: 500 }}>
                                                        {formatPrice(
                                                            (selectedOfferDetail.header?.totalPrice || 0) + 
                                                            (selectedOfferDetail.header?.taxAmount || 0)
                                                        )} تومان
                                                    </span>
                                                </Descriptions.Item>
                                            )}

                                            <Descriptions.Item label="تاریخ ایجاد">
                                                {toShamsi(selectedOfferDetail.header?.createdAt) || "-"}
                                            </Descriptions.Item>
                                            {selectedOfferDetail.header?.publishedAt && (
                                                <Descriptions.Item label="تاریخ انتشار">
                                                    {toShamsi(selectedOfferDetail.header.publishedAt) || "-"}
                                                </Descriptions.Item>
                                            )}

                                            {selectedOfferDetail.header?.expireAtBySupplier && (
                                                <Descriptions.Item label="تاریخ انقضا" span={2}>
                                                    {toShamsi(selectedOfferDetail.header.expireAtBySupplier) || "-"}
                                                </Descriptions.Item>
                                            )}
                                        </Descriptions>

                                        {/* Documents - مشابه مرحله 4 */}
                                        {attributeTemplates.length > 0 && (
                                            <Card size="small" title="ویژگی‌ها و مدارک" style={{ marginBottom: 24 }}>
                                                <Space direction="vertical" style={{ width: "100%" }} size="middle">
                                                    {attributeTemplates.map(attr => {
                                                        const doc = selectedOfferDetail?.documents?.find(
                                                            d => d.attributeDefinitionId === attr.attributeDefinitionId
                                                        );

                                                        return (
                                                            <div
                                                                key={attr.attributeDefinitionId}
                                                                style={{
                                                                    display: "flex",
                                                                    justifyContent: "space-between",
                                                                    padding: "6px 0",
                                                                    borderBottom: "1px dashed #eee",
                                                                }}
                                                            >
                                                                <span>{attr.displayName}</span>

                                                                {attr.dataType === 5 ? (
                                                                    // File type: نمایش نام فایل + دکمه Download
                                                                    doc?.filePath ? (
                                                                        <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                                                                            <span>{doc?.value || "فایل"}</span>
                                                                            <Button 
                                                                                size="small"
                                                                                onClick={async () => {
                                                                                    try {
                                                                                        await offersApi.downloadOfferFile(
                                                                                            selectedOfferDetail.header.id,
                                                                                            doc.filePath,
                                                                                            doc.value
                                                                                        );
                                                                                    } catch (error) {
                                                                                        message.error("خطا در دانلود فایل");
                                                                                    }
                                                                                }}
                                                                            >
                                                                                دانلود فایل
                                                                            </Button>
                                                                        </div>
                                                                    ) : (
                                                                        <span>-</span>
                                                                    )
                                                                ) : attr.dataType === 4 ? (
                                                                    // Date type: تبدیل gregorian به شمسی
                                                                    doc?.value ? (
                                                                        <span>{toShamsi(doc.value)}</span>
                                                                    ) : (
                                                                        <span>-</span>
                                                                    )
                                                                ) : attr.dataType === 3 ? (
                                                                    // Boolean type: نمایش "بله" یا "خیر"
                                                                    doc?.value === "true" ? (
                                                                        <span>بله</span>
                                                                    ) : doc?.value === "false" ? (
                                                                        <span>خیر</span>
                                                                    ) : (
                                                                        <span>-</span>
                                                                    )
                                                                ) : (
                                                                    // Text, Number: نمایش مستقیم
                                                                    <span>{doc?.value ?? "-"}</span>
                                                                )}
                                                            </div>
                                                        );
                                                    })}
                                                </Space>
                                            </Card>
                                        )}

                                        {/* Cancel Button - فقط برای Published */}
                                        {selectedOfferDetail.header?.status === 3 && (
                                            <div style={{ textAlign: "left", marginTop: 16 }}>
                                                <Button
                                                    danger
                                                    icon={<CloseCircleOutlined />}
                                                    onClick={() => {
                                                        handleCloseDetailDrawer();
                                                        handleCancel(selectedOfferDetail.header.id);
                                                    }}
                                                >
                                                    لغو آگهی
                                                </Button>
                                            </div>
                                        )}
                                    </div>
                                ),
                            },
                            {
                                key: "history",
                                label: "تاریخچه",
                                children: (
                                    <Table
                                        rowKey="id"
                                        loading={loadingHistory}
                                        dataSource={historyData}
                                        columns={[
                                            {
                                                title: "از وضعیت",
                                                dataIndex: "oldStatus",
                                                render: (status) => (
                                                    <Tag color={getStatusColor(status)}>{getStatusLabel(status)}</Tag>
                                                ),
                                            },
                                            {
                                                title: "به وضعیت",
                                                dataIndex: "newStatus",
                                                render: (status) => (
                                                    <Tag color={getStatusColor(status)}>{getStatusLabel(status)}</Tag>
                                                ),
                                            },
                                            {
                                                title: "توسط",
                                                render: (_, r) => r.adminDisplayName || r.adminUserId || "-",
                                            },
                                            {
                                                title: "یادداشت",
                                                dataIndex: "note",
                                                render: (v) => v || "-",
                                            },
                                            {
                                                title: "تاریخ",
                                                dataIndex: "createdAt",
                                                render: (date) => {
                                                    const { date: shamsiDate, time } = toShamsiWithTime(date);
                                                    return (
                                                        <div style={{ display: "flex", flexDirection: "column" }}>
                                                            <span>{shamsiDate}</span>
                                                            {time && <span style={{ fontSize: "12px", color: "#999" }}>{time}</span>}
                                                        </div>
                                                    );
                                                },
                                            },
                                        ]}
                                        pagination={false}
                                    />
                                ),
                            },
                        ]}
                    />
                )}
            </Drawer>
        </Card>
    );
};

export default MyOffersPage;
