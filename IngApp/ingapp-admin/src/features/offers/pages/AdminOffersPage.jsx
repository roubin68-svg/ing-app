import React, { useEffect, useState, useCallback } from "react";
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
    Modal,
    Input as AntInput,
    Descriptions,
    Drawer,
    Tabs,
    Alert,
    Spin,
    Image,
} from "antd";
import {
    SearchOutlined,
    ReloadOutlined,
    EyeOutlined,
    CloseCircleOutlined,
    HistoryOutlined,
    DownloadOutlined,
    FilePdfOutlined,
    FileWordOutlined,
    FileOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";
import jalaali from "jalaali-js";
import offersApi from "../api/offersApi";
import suppliersApi from "../../suppliers/api/suppliersApi";
import productCategoryApi from "../../productCategories/api/productCategoryApi";
import CategoryTreeSelect from "../../products/components/CategoryTreeSelect";
import apiClient from "../../../core/api/apiClient";

const { Option } = Select;
const { TextArea } = AntInput;

const STATUS_OPTIONS = [
    { value: "Draft", label: "پیش‌نویس" },
    { value: "Pending", label: "در انتظار" },
    { value: "Published", label: "منتشر شده" },
    { value: "Cancel", label: "لغو شده" },
    { value: "Rejected", label: "رد شده" },
];

// تبدیل تاریخ میلادی به شمسی
const toShamsi = (gregorian) => {
    if (!gregorian) return null;
    
    if (typeof gregorian === "string") {
        const [y, m, d] = gregorian.split("T")[0].split("-").map(Number);
        const j = jalaali.toJalaali(y, m, d);
        return `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
    }
    
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

// فرمت قیمت
const formatPrice = (v) =>
    v != null
        ? v.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
        : "-";

// تبدیل Status enum به string
const getStatusString = (status) => {
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

const getStatusLabel = (status) => {
    const statusStr = getStatusString(status);
    const option = STATUS_OPTIONS.find(opt => opt.value === statusStr);
    return option?.label || statusStr;
};

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

const AdminOffersPage = () => {
    const { message, modal } = App.useApp();
    const [form] = Form.useForm();
    const [rejectForm] = Form.useForm();

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

    const [suppliers, setSuppliers] = useState([]);
    const [categories, setCategories] = useState([]);

    // Modal states
    const [rejectModalVisible, setRejectModalVisible] = useState(false);
    const [selectedOfferId, setSelectedOfferId] = useState(null);
    const [detailDrawerVisible, setDetailDrawerVisible] = useState(false);
    const [selectedOfferDetail, setSelectedOfferDetail] = useState(null);
    const [loadingDetail, setLoadingDetail] = useState(false);
    const [historyData, setHistoryData] = useState([]);
    const [loadingHistory, setLoadingHistory] = useState(false);
    const [attributeTemplates, setAttributeTemplates] = useState([]);

    // -----------------------
    // Load Suppliers for filter
    // -----------------------
    const loadSuppliers = useCallback(async () => {
        try {
            const res = await suppliersApi.getPaged({ page: 1, pageSize: 1000 });
            setSuppliers(res.items || []);
        } catch {
            message.error("خطا در دریافت لیست تامین‌کنندگان");
        }
    }, [message]);

    // -----------------------
    // Load Categories
    // -----------------------
    const loadCategories = useCallback(async () => {
        try {
            const res = await productCategoryApi.getAll();
            setCategories(res || []);
        } catch {
            message.error("خطا در دریافت دسته‌بندی‌ها");
        }
    }, [message]);

    // -----------------------
    // Load Offers
    // -----------------------
    const loadOffers = useCallback(async (
        pageIndex = page,
        pageSizeValue = pageSize,
        sorter = {}
    ) => {
        try {
            setLoading(true);

            const filters = form.getFieldsValue();

            const params = {
                page: pageIndex,
                pageSize: pageSizeValue,
                offerId: filters.offerId ? Number(filters.offerId) : undefined,
                status: filters.status || undefined, // اگر خالی باشد، undefined بفرست (همه وضعیت‌ها)
                supplierUserId: filters.supplierUserId || undefined,
                productName: filters.productName || undefined,
                productCategoryId: filters.productCategoryId || undefined,
                sortBy: sorter.field || sortBy || undefined,
                sortDirection:
                    sorter.order === "ascend"
                        ? "asc"
                        : sorter.order === "descend"
                            ? "desc"
                            : sortDirection || undefined,
            };

            const res = await offersApi.getAdminOffers(params);

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
    }, [page, pageSize, sortBy, sortDirection, form, message]);

    useEffect(() => {
        loadSuppliers();
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

    const handleReject = (offerId) => {
        setSelectedOfferId(offerId);
        setRejectModalVisible(true);
        rejectForm.resetFields();
    };

    const handleRejectSubmit = async () => {
        try {
            const values = await rejectForm.validateFields();
            await offersApi.rejectOffer(selectedOfferId, values.reason);
            message.success("آگهی با موفقیت رد شد");
            setRejectModalVisible(false);
            rejectForm.resetFields();
            loadOffers();
            // Refresh detail if open
            if (detailDrawerVisible && selectedOfferDetail?.header?.id === selectedOfferId) {
                handleViewOffer(selectedOfferId);
            }
        } catch (e) {
            if (e.errorFields) {
                return; // Form validation error
            }
            console.error(e);
            message.error("خطا در رد کردن آگهی");
        }
    };

    const handleViewOffer = async (offerId) => {
        setDetailDrawerVisible(true);
        setLoadingDetail(true);
        setLoadingHistory(true);
        setSelectedOfferDetail(null);
        setHistoryData([]);
        setAttributeTemplates([]);
        try {
            const [detailRes, historyRes] = await Promise.all([
                offersApi.getAdminOfferDetail(offerId),
                offersApi.getOfferStatusHistory(offerId),
            ]);
            const detail = detailRes?.data || detailRes;
            const history = historyRes?.data || historyRes;
            setSelectedOfferDetail(detail || null);
            setHistoryData(history || []);
            
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
            title: "تامین‌کننده",
            dataIndex: "supplierBusinessName",
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
            sorter: true,
            render: (status) => {
                const statusStr = getStatusString(status);
                return <Tag color={getStatusColor(status)}>{getStatusLabel(status)}</Tag>;
            },
        },
        {
            title: "تاریخ انتشار",
            dataIndex: "publishedAt",
            sorter: true,
            render: (date) => {
                if (!date) return "-";
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
            fixed: "right",
            render: (_, record) => {
                return (
                        <Button
                            icon={<EyeOutlined />}
                            onClick={() => handleViewOffer(record.id)}
                        >
                            مشاهده
                        </Button>
                );
            },
        },
    ];

    // -----------------------
    // History Table Columns
    // -----------------------
    const historyColumns = [
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
    ];

    // -----------------------
    // Render
    // -----------------------
    return (
        <>
            <Card
                title="مدیریت آگهی‌ها"
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
                            <Form.Item
                                label="تامین‌کننده"
                                name="supplierUserId"
                            >
                                <Select
                                    allowClear
                                    placeholder="انتخاب تامین‌کننده"
                                    showSearch
                                    filterOption={(input, option) =>
                                        (option?.label ?? "").toLowerCase().includes(input.toLowerCase())
                                    }
                                    options={suppliers.map(s => ({
                                        value: s.userId,
                                        label: s.businessName || s.userPhoneNumber || "نامشخص",
                                    }))}
                                />
                            </Form.Item>
                        </Col>

                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Form.Item
                                label="وضعیت"
                                name="status"
                            >
                                <Select placeholder="همه وضعیت‌ها" allowClear>
                                    {STATUS_OPTIONS.map(opt => (
                                        <Option key={opt.value} value={opt.value}>
                                            {opt.label}
                                        </Option>
                                    ))}
                                </Select>
                            </Form.Item>
                        </Col>

                        <Col xs={24} sm={24} md={24} lg={24}>
                            <Space>
                                <Button
                                    type="primary"
                                    icon={<SearchOutlined />}
                                    onClick={handleSearch}
                                >
                                    جستجو
                                </Button>
                                <Button onClick={handleClear}>
                                    پاک کردن
                                </Button>
                            </Space>
                        </Col>
                    </Row>
                </Form>

                {/* Table */}
                <Table
                    columns={columns}
                    dataSource={data}
                    loading={loading}
                    rowKey="id"
                    scroll={{ x: 'max-content' }}
                    pagination={{
                        current: page,
                        pageSize: pageSize,
                        total: total,
                        showSizeChanger: true,
                        showTotal: (total) => `مجموع: ${total}`,
                        onChange: (newPage, newPageSize) => {
                            setPage(newPage);
                            setPageSize(newPageSize);
                            loadOffers(newPage, newPageSize);
                        },
                    }}
                    onChange={(pagination, filters, sorter) => {
                        if (sorter.field) {
                            setSortBy(sorter.field);
                            setSortDirection(
                                sorter.order === "ascend" ? "asc" : "desc"
                            );
                            loadOffers(page, pageSize, sorter);
                        }
                    }}
                />
            </Card>

            {/* Reject Modal */}
            <Modal
                title="رد کردن آگهی"
                open={rejectModalVisible}
                onOk={handleRejectSubmit}
                onCancel={() => {
                    setRejectModalVisible(false);
                    rejectForm.resetFields();
                }}
                okText="رد کردن"
                cancelText="انصراف"
                okButtonProps={{ danger: true }}
            >
                <Form form={rejectForm} layout="vertical">
                    <Form.Item
                        name="reason"
                        label="دلیل رد"
                        rules={[
                            { required: true, message: "لطفاً دلیل رد را وارد کنید" },
                            { min: 10, message: "دلیل رد باید حداقل 10 کاراکتر باشد" },
                        ]}
                    >
                        <TextArea
                            rows={4}
                            placeholder="لطفاً دلیل رد کردن این آگهی را به صورت کامل وارد کنید..."
                        />
                    </Form.Item>
                </Form>
            </Modal>

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
                                        {/* Rejection Alert */}
                                        {selectedOfferDetail.header?.rejectedReason && (
                                            <Alert
                                                message="دلیل رد آگهی"
                                                description={selectedOfferDetail.header.rejectedReason}
                                                type="error"
                                                showIcon
                                                style={{ marginBottom: 24 }}
                                            />
                                        )}

                                        {/* Offer Details - New Order */}
                                        <Descriptions
                                            bordered
                                            column={2}
                                            size="small"
                                            style={{ marginBottom: 24 }}
                                        >
                                            {/* وضعیت - سطر اول */}
                                            <Descriptions.Item label="وضعیت" span={2}>
                                                <Tag color={getStatusColor(selectedOfferDetail.header?.status)}>
                                                    {getStatusLabel(selectedOfferDetail.header?.status)}
                                                </Tag>
                                            </Descriptions.Item>

                                            {/* دلیل رد - اگر رد شده */}
                                            {selectedOfferDetail.header?.rejectedReason && (
                                                <Descriptions.Item label="دلیل رد" span={2}>
                                                    {selectedOfferDetail.header.rejectedReason}
                                                </Descriptions.Item>
                                            )}

                                            {/* نام تأمین‌کننده */}
                                            <Descriptions.Item label="نام تأمین‌کننده" span={2}>
                                                {selectedOfferDetail.header?.supplierBusinessName || "-"}
                                            </Descriptions.Item>

                                            {/* دسته محصول و نام محصول - یک سطر */}
                                            <Descriptions.Item label="دسته محصول">
                                                {selectedOfferDetail.header?.productCategoryName || "-"}
                                            </Descriptions.Item>
                                            <Descriptions.Item label="نام محصول">
                                                {selectedOfferDetail.header?.productName || "-"}
                                            </Descriptions.Item>

                                            {/* قیمت واحد و مقدار - یک سطر */}
                                            <Descriptions.Item label="قیمت واحد">
                                                {formatPrice(selectedOfferDetail.header?.unitPrice)} تومان
                                            </Descriptions.Item>
                                            <Descriptions.Item label="مقدار">
                                                {selectedOfferDetail.header?.quantity ?? "-"} {selectedOfferDetail.header?.unit || ""}
                                            </Descriptions.Item>

                                            {/* قیمت کل و مالیات - یک سطر */}
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

                                            {/* تاریخ ایجاد و تاریخ انتشار - یک سطر */}
                                            <Descriptions.Item label="تاریخ ایجاد">
                                                {toShamsi(selectedOfferDetail.header?.createdAt) || "-"}
                                            </Descriptions.Item>
                                            {selectedOfferDetail.header?.publishedAt && (
                                                <Descriptions.Item label="تاریخ انتشار">
                                                    {toShamsi(selectedOfferDetail.header.publishedAt) || "-"}
                                                </Descriptions.Item>
                                            )}

                                            {/* تاریخ انقضا - سطر آخر */}
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
                                                                                        await offersApi.downloadPublicOfferFile(
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

                                        {/* Reject Button - فقط برای Published */}
                                        {selectedOfferDetail.header?.status === 3 && (
                                            <div style={{ textAlign: "left", marginTop: 16 }}>
                                                <Button
                                                    danger
                                                    icon={<CloseCircleOutlined />}
                                                    onClick={() => {
                                                        handleCloseDetailDrawer();
                                                        handleReject(selectedOfferDetail.header.id);
                                                    }}
                                                >
                                                    رد کردن آگهی
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
                                        columns={historyColumns}
                                        pagination={false}
                                    />
                                ),
                            },
                        ]}
                    />
                )}
            </Drawer>
        </>
    );
};

export default AdminOffersPage;
