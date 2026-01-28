// src/features/buyerManagement/pages/BuyerManagementPage.jsx
import React, { useCallback, useEffect, useState } from "react";
import {
    Card,
    Table,
    Tag,
    message,
    Input,
    Select,
    Row,
    Col,
    Button,
    Space,
    Modal,
    Form,
    Typography,
    Popconfirm,
    Descriptions,
    Divider,
} from "antd";
import {
    PlusOutlined,
    EditOutlined,
    DeleteOutlined,
    UserOutlined,
    LinkOutlined,
    DisconnectOutlined,
} from "@ant-design/icons";
import buyerManagementApi from "../api/buyerManagementApi";
import visitorManagementApi from "../../visitorManagement/api/visitorManagementApi";
import { getProvinces, getCitiesByProvince } from "../../../core/location/iranProvinces";
import jalaali from "jalaali-js";

const { Option } = Select;
const { Text } = Typography;

// تبدیل تاریخ میلادی به شمسی
const toShamsi = (gregorian) => {
    if (!gregorian) return "-";
    
    try {
        let year, month, day;
        
        if (typeof gregorian === "string") {
            const parts = gregorian.split("T")[0].split("-");
            if (parts.length !== 3) return "-";
            year = parseInt(parts[0], 10);
            month = parseInt(parts[1], 10);
            day = parseInt(parts[2], 10);
            
            if (isNaN(year) || isNaN(month) || isNaN(day)) return "-";
            if (year < 1900 || year > 2100) return "-";
        } else if (gregorian instanceof Date) {
            if (isNaN(gregorian.getTime())) return "-";
            year = gregorian.getFullYear();
            month = gregorian.getMonth() + 1;
            day = gregorian.getDate();
        } else {
            return "-";
        }
        
        const j = jalaali.toJalaali(year, month, day);
        if (!j || !j.jy || !j.jm || !j.jd) return "-";
        
        return `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
    } catch (error) {
        console.error("Error converting date to Shamsi:", error);
        return "-";
    }
};

const BuyerManagementPage = () => {
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState([]);
    const [total, setTotal] = useState(0);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [sortBy, setSortBy] = useState(null);
    const [sortDesc, setSortDesc] = useState(false);

    // Filters
    const [filters, setFilters] = useState({
        search: "",
    });

    // Buyer Modal
    const [isBuyerModalOpen, setIsBuyerModalOpen] = useState(false);
    const [editingBuyer, setEditingBuyer] = useState(null);
    const [buyerForm] = Form.useForm();
    
    // Province/City state
    const [selectedProvince, setSelectedProvince] = useState(null);
    const provinces = React.useMemo(() => getProvinces(), []);
    const cities = React.useMemo(
        () => getCitiesByProvince(selectedProvince),
        [selectedProvince]
    );

    // Referral Modal
    const [isReferralModalOpen, setIsReferralModalOpen] = useState(false);
    const [selectedBuyer, setSelectedBuyer] = useState(null);
    const [referralForm] = Form.useForm();
    const [visitors, setVisitors] = useState([]);
    const [loadingVisitors, setLoadingVisitors] = useState(false);
    const [referralMethod, setReferralMethod] = useState("code"); // "code" or "select"

    // ========================
    // Load Buyers
    // ========================
    const loadBuyers = useCallback(
        async (targetPage = page) => {
            try {
                setLoading(true);
                const params = {
                    page: targetPage,
                    pageSize,
                    sortBy,
                    sortDesc,
                    search: filters.search || null,
                };

                const res = await buyerManagementApi.getPaged(params);
                setData(
                    (res.items || []).map((b) => ({
                        key: b.id,
                        ...b,
                    }))
                );
                setTotal(res.totalCount || 0);
                setPage(res.page || targetPage);
            } catch (err) {
                console.error("Error in loadBuyers:", err);
                const errorMessage = 
                    err?.response?.data?.message || 
                    err?.response?.data?.error || 
                    err?.message || 
                    "خطا در دریافت لیست خریداران";
                message.error(errorMessage);
            } finally {
                setLoading(false);
            }
        },
        [page, pageSize, sortBy, sortDesc, filters]
    );

    useEffect(() => {
        loadBuyers(1);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [pageSize, sortBy, sortDesc, filters]);

    const handleSearch = () => {
        setPage(1);
        loadBuyers(1);
    };

    const handleTableChange = (pagination, _, sorter) => {
        setPage(pagination.current);
        setPageSize(pagination.pageSize);
        if (sorter.field) {
            setSortBy(sorter.field);
            setSortDesc(sorter.order === "descend");
        }
    };

    // ========================
    // Buyer Modal
    // ========================
    const openCreateModal = async () => {
        setEditingBuyer(null);
        setSelectedProvince(null);
        buyerForm.resetFields();
        await loadVisitorsForSelect();
        setIsBuyerModalOpen(true);
    };

    const openEditModal = (buyer) => {
        setEditingBuyer(buyer);
        setSelectedProvince(buyer.province || null);
        buyerForm.setFieldsValue({
            businessName: buyer.businessName,
            contactMobile: buyer.contactMobile,
            province: buyer.province,
            city: buyer.city,
            address: buyer.address,
            description: buyer.description,
        });
        setIsBuyerModalOpen(true);
    };

    const handleBuyerModalCancel = () => {
        setIsBuyerModalOpen(false);
        setEditingBuyer(null);
        setSelectedProvince(null);
        buyerForm.resetFields();
    };

    const handleBuyerFormFinish = async (values) => {
        try {
            if (editingBuyer) {
                await buyerManagementApi.update(editingBuyer.id, {
                    businessName: values.businessName?.trim() || null,
                    contactMobile: values.contactMobile?.trim() || null,
                    province: values.province || null,
                    city: values.city || null,
                    address: values.address?.trim() || null,
                    description: values.description?.trim() || null,
                });
                message.success("خریدار با موفقیت ویرایش شد");
            } else {
                await buyerManagementApi.create({
                    phoneNumber: values.phoneNumber.trim(),
                    displayName: values.displayName?.trim() || null,
                    businessName: values.businessName?.trim() || null,
                    contactMobile: values.contactMobile?.trim() || null,
                    province: values.province || null,
                    city: values.city || null,
                    address: values.address?.trim() || null,
                    description: values.description?.trim() || null,
                    referredByVisitorId: values.referredByVisitorId || null,
                    referralCode: values.referralCode?.trim() || null,
                });
                message.success("خریدار جدید با موفقیت ایجاد شد");
            }
            handleBuyerModalCancel();
            loadBuyers();
        } catch (err) {
            console.error("Error in handleBuyerFormFinish:", err);
            const errorMessage = 
                err?.response?.data?.message || 
                err?.response?.data?.error || 
                err?.message || 
                "خطا در ذخیره اطلاعات خریدار";
            message.error(errorMessage);
        }
    };

    const handleDelete = async (buyer) => {
        try {
            await buyerManagementApi.delete(buyer.id);
            message.success("خریدار با موفقیت حذف شد");
            loadBuyers();
        } catch (err) {
            console.error(err);
            message.error(err.response?.data?.message || "خطا در حذف خریدار");
        }
    };

    // ========================
    // Referral Modal
    // ========================
    const loadVisitorsForSelect = async () => {
        try {
            setLoadingVisitors(true);
            const res = await visitorManagementApi.getPaged({ 
                page: 1, 
                pageSize: 1000,
                isActive: true 
            });
            setVisitors(res.items || []);
        } catch (err) {
            console.error(err);
            message.error("خطا در دریافت لیست بازاریاب‌ها");
        } finally {
            setLoadingVisitors(false);
        }
    };

    const openReferralModal = async (buyer) => {
        setSelectedBuyer(buyer);
        await loadVisitorsForSelect();
        referralForm.setFieldsValue({
            referralMethod: buyer.referredByVisitorId ? "select" : "code",
            referredByVisitorId: buyer.referredByVisitorId || null,
            referralCode: buyer.referredByVisitorCode || null,
        });
        setReferralMethod(buyer.referredByVisitorId ? "select" : "code");
        setIsReferralModalOpen(true);
    };

    const handleReferralModalCancel = () => {
        setIsReferralModalOpen(false);
        setSelectedBuyer(null);
        referralForm.resetFields();
        setReferralMethod("code");
    };

    const handleReferralFormFinish = async (values) => {
        if (!selectedBuyer) return;
        
        try {
            if (values.referralMethod === "select" && values.referredByVisitorId) {
                await buyerManagementApi.setReferral(selectedBuyer.id, {
                    referredByVisitorId: values.referredByVisitorId,
                    referralCode: null,
                });
            } else if (values.referralMethod === "code" && values.referralCode) {
                await buyerManagementApi.setReferral(selectedBuyer.id, {
                    referredByVisitorId: null,
                    referralCode: values.referralCode.trim().toUpperCase(),
                });
            } else {
                message.warning("لطفاً بازاریاب را انتخاب کنید یا کد معرف را وارد کنید");
                return;
            }
            
            message.success("بازاریاب با موفقیت تنظیم شد");
            handleReferralModalCancel();
            loadBuyers();
        } catch (err) {
            console.error("Error in handleReferralFormFinish:", err);
            
            // استخراج پیام خطا از response
            let errorMessage = "خطا در تنظیم بازاریاب";
            
            if (err?.response?.data) {
                // اگر response.data یک object است و message دارد
                if (typeof err.response.data === 'object' && err.response.data.message) {
                    errorMessage = err.response.data.message;
                } 
                // اگر response.data خودش یک string است
                else if (typeof err.response.data === 'string') {
                    errorMessage = err.response.data;
                }
                // اگر response.data.error دارد
                else if (err.response.data.error) {
                    errorMessage = err.response.data.error;
                }
            } 
            // اگر error.message مستقیم وجود دارد
            else if (err?.message) {
                errorMessage = err.message;
            }
            
            message.error(errorMessage);
        }
    };

    const handleRemoveReferral = async (buyer) => {
        try {
            await buyerManagementApi.removeReferral(buyer.id);
            message.success("بازاریاب با موفقیت حذف شد");
            loadBuyers();
        } catch (err) {
            console.error(err);
            message.error(err.response?.data?.message || "خطا در حذف بازاریاب");
        }
    };

    // ========================
    // Table Columns
    // ========================
    const columns = [
        {
            title: "ردیف",
            key: "rowNumber",
            width: 80,
            align: "center",
            render: (_, __, index) => {
                const start = (page - 1) * pageSize;
                return start + index + 1;
            },
        },
        {
            title: "شماره موبایل",
            dataIndex: "userPhoneNumber",
            width: "12%",
        },
        {
            title: "نام",
            dataIndex: "userDisplayName",
            width: "12%",
            render: (text) => text || "-",
        },
        {
            title: "نام کسب‌وکار",
            dataIndex: "businessName",
            width: "15%",
            render: (text) => text || "-",
        },
        {
            title: "بازاریاب",
            key: "referral",
            width: "15%",
            render: (_, record) => {
                if (record.referredByVisitorCode) {
                    return (
                        <Space direction="vertical" size="small">
                            <Text strong>{record.referredByVisitorName || "-"}</Text>
                            <Text type="secondary" code>{record.referredByVisitorCode}</Text>
                        </Space>
                    );
                }
                return <Text type="secondary">-</Text>;
            },
        },
        {
            title: "استان",
            dataIndex: "province",
            width: "10%",
            render: (text) => text || "-",
        },
        {
            title: "شهر",
            dataIndex: "city",
            width: "10%",
            render: (text) => text || "-",
        },
        {
            title: "تاریخ ایجاد",
            dataIndex: "createdAt",
            width: "12%",
            render: (date) => toShamsi(date),
        },
        {
            title: "عملیات",
            key: "actions",
            width: "20%",
            render: (_, record) => (
                <Space>
                    <Button
                        size="small"
                        icon={<EditOutlined />}
                        onClick={() => openEditModal(record)}
                    >
                        ویرایش
                    </Button>
                    <Button
                        size="small"
                        icon={record.referredByVisitorId ? <LinkOutlined /> : <UserOutlined />}
                        onClick={() => openReferralModal(record)}
                    >
                        {record.referredByVisitorId ? "تغییر بازاریاب" : "افزودن بازاریاب"}
                    </Button>
                    {record.referredByVisitorId && (
                        <Popconfirm
                            title="آیا از حذف بازاریاب اطمینان دارید؟"
                            onConfirm={() => handleRemoveReferral(record)}
                            okText="بله"
                            cancelText="خیر"
                        >
                            <Button
                                size="small"
                                danger
                                icon={<DisconnectOutlined />}
                            >
                                حذف بازاریاب
                            </Button>
                        </Popconfirm>
                    )}
                    <Popconfirm
                        title="آیا از حذف این خریدار اطمینان دارید؟"
                        onConfirm={() => handleDelete(record)}
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

    // ========================
    // Render
    // ========================
    return (
        <>
            <Card
                title="مدیریت خریداران"
                bordered={false}
                extra={
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={openCreateModal}
                    >
                        افزودن خریدار جدید
                    </Button>
                }
            >
                {/* فیلترها */}
                <Row gutter={12} style={{ marginBottom: 16 }}>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <Input
                            placeholder="جستجو بر اساس شماره موبایل، نام یا کسب‌وکار"
                            value={filters.search}
                            allowClear
                            onChange={(e) =>
                                setFilters({ ...filters, search: e.target.value })
                            }
                            onPressEnter={handleSearch}
                        />
                    </Col>
                    <Col>
                        <Space>
                            <Button type="primary" onClick={handleSearch}>
                                جستجو
                            </Button>
                            <Button
                                onClick={() => {
                                    setFilters({ search: "" });
                                    setPage(1);
                                    loadBuyers(1);
                                }}
                            >
                                پاکسازی
                            </Button>
                        </Space>
                    </Col>
                </Row>

                {/* جدول */}
                <Table
                    columns={columns}
                    dataSource={data}
                    loading={loading}
                    pagination={{
                        current: page,
                        pageSize: pageSize,
                        total: total,
                        showSizeChanger: true,
                        showTotal: (total) => `مجموع ${total} خریدار`,
                    }}
                    onChange={handleTableChange}
                    scroll={{ x: 1200 }}
                />
            </Card>

            {/* Buyer Modal */}
            <Modal
                open={isBuyerModalOpen}
                title={editingBuyer ? "ویرایش خریدار" : "افزودن خریدار جدید"}
                onCancel={handleBuyerModalCancel}
                onOk={() => buyerForm.submit()}
                okText="ذخیره"
                cancelText="انصراف"
                destroyOnClose
                width={600}
            >
                <Form
                    form={buyerForm}
                    layout="vertical"
                    onFinish={handleBuyerFormFinish}
                >
                    {!editingBuyer && (
                        <>
                            <Form.Item
                                label="شماره موبایل"
                                name="phoneNumber"
                                rules={[
                                    { required: true, message: "شماره موبایل الزامی است" },
                                    {
                                        pattern: /^09\d{9}$/,
                                        message: "شماره موبایل باید 11 رقم و با 09 شروع شود",
                                    },
                                ]}
                            >
                                <Input placeholder="09xxxxxxxxx" />
                            </Form.Item>

                            <Form.Item
                                label="نام نمایشی (اختیاری)"
                                name="displayName"
                                rules={[
                                    { max: 100, message: "نام نمی‌تواند بیشتر از 100 کاراکتر باشد" },
                                ]}
                            >
                                <Input placeholder="نام نمایشی" />
                            </Form.Item>

                            <Divider>بازاریاب (اختیاری)</Divider>

                            <Form.Item
                                label="کد معرف بازاریاب (اختیاری)"
                                name="referralCode"
                            >
                                <Input placeholder="کد معرف بازاریاب" />
                            </Form.Item>

                            <Form.Item
                                label="یا انتخاب از لیست"
                                name="referredByVisitorId"
                            >
                                <Select
                                    placeholder="انتخاب بازاریاب"
                                    showSearch
                                    filterOption={(input, option) => {
                                        const text = option?.label || option?.children || "";
                                        const textStr = typeof text === 'string' ? text : String(text);
                                        return textStr.toLowerCase().includes(input.toLowerCase());
                                    }}
                                    optionFilterProp="label"
                                    allowClear
                                >
                                    {visitors.map((v) => {
                                        const label = `${v.referralCode} - ${v.userDisplayName || v.userPhoneNumber}`;
                                        return (
                                            <Option key={v.id} value={v.id} label={label}>
                                                {label}
                                            </Option>
                                        );
                                    })}
                                </Select>
                            </Form.Item>
                        </>
                    )}

                    {editingBuyer && (
                        <Descriptions column={1} bordered size="small" style={{ marginBottom: 16 }}>
                            <Descriptions.Item label="شماره موبایل">
                                {editingBuyer.userPhoneNumber}
                            </Descriptions.Item>
                            <Descriptions.Item label="نام">
                                {editingBuyer.userDisplayName || "-"}
                            </Descriptions.Item>
                            {editingBuyer.referredByVisitorCode && (
                                <Descriptions.Item label="بازاریاب">
                                    <Space>
                                        <Text>{editingBuyer.referredByVisitorName || "-"}</Text>
                                        <Text code>{editingBuyer.referredByVisitorCode}</Text>
                                    </Space>
                                </Descriptions.Item>
                            )}
                        </Descriptions>
                    )}

                    <Form.Item label="نام کسب‌وکار" name="businessName">
                        <Input placeholder="نام کسب‌وکار" />
                    </Form.Item>

                    <Form.Item 
                        label="شماره تماس اضطراری" 
                        name="contactMobile"
                        rules={[
                            {
                                pattern: /^09\d{9}$/,
                                message: "شماره تماس باید 11 رقم و با 09 شروع شود",
                            },
                        ]}
                    >
                        <Input placeholder="09xxxxxxxxx" />
                    </Form.Item>

                    <Row gutter={12}>
                        <Col span={12}>
                            <Form.Item label="استان" name="province">
                                <Select
                                    placeholder="انتخاب استان"
                                    allowClear
                                    onChange={(value) => {
                                        setSelectedProvince(value || null);
                                        buyerForm.setFieldsValue({ city: null });
                                    }}
                                >
                                    {provinces.map((p) => (
                                        <Option key={p} value={p}>
                                            {p}
                                        </Option>
                                    ))}
                                </Select>
                            </Form.Item>
                        </Col>
                        <Col span={12}>
                            <Form.Item label="شهر" name="city">
                                <Select
                                    placeholder="انتخاب شهر"
                                    allowClear
                                    disabled={!selectedProvince}
                                >
                                    {cities.map((c) => (
                                        <Option key={c} value={c}>
                                            {c}
                                        </Option>
                                    ))}
                                </Select>
                            </Form.Item>
                        </Col>
                    </Row>

                    <Form.Item label="آدرس" name="address">
                        <Input.TextArea rows={2} placeholder="آدرس" />
                    </Form.Item>

                    <Form.Item label="توضیحات" name="description">
                        <Input.TextArea rows={3} placeholder="توضیحات" />
                    </Form.Item>
                </Form>
            </Modal>

            {/* Referral Modal */}
            <Modal
                open={isReferralModalOpen}
                title="تنظیم بازاریاب"
                onCancel={handleReferralModalCancel}
                onOk={() => referralForm.submit()}
                okText="ذخیره"
                cancelText="انصراف"
                destroyOnClose
                width={500}
            >
                <Form
                    form={referralForm}
                    layout="vertical"
                    onFinish={handleReferralFormFinish}
                >
                    <Form.Item
                        label="روش انتخاب"
                        name="referralMethod"
                        rules={[{ required: true, message: "لطفاً روش انتخاب را مشخص کنید" }]}
                    >
                        <Select
                            onChange={(value) => {
                                setReferralMethod(value);
                                referralForm.setFieldsValue({
                                    referredByVisitorId: null,
                                    referralCode: null,
                                });
                            }}
                        >
                            <Option value="code">وارد کردن کد معرف</Option>
                            <Option value="select">انتخاب از لیست</Option>
                        </Select>
                    </Form.Item>

                    {referralMethod === "code" ? (
                        <Form.Item
                            label="کد معرف بازاریاب"
                            name="referralCode"
                            rules={[
                                { required: true, message: "کد معرف الزامی است" },
                                { min: 4, max: 4, message: "کد معرف باید 4 کاراکتر باشد" },
                            ]}
                        >
                            <Input placeholder="کد معرف (4 کاراکتر)" style={{ textTransform: "uppercase" }} />
                        </Form.Item>
                    ) : (
                        <Form.Item
                            label="انتخاب بازاریاب"
                            name="referredByVisitorId"
                            rules={[{ required: true, message: "لطفاً بازاریاب را انتخاب کنید" }]}
                        >
                            <Select
                                placeholder="انتخاب بازاریاب"
                                loading={loadingVisitors}
                                showSearch
                                filterOption={(input, option) => {
                                    const text = option?.label || option?.children || "";
                                    const textStr = typeof text === 'string' ? text : String(text);
                                    return textStr.toLowerCase().includes(input.toLowerCase());
                                }}
                                optionFilterProp="label"
                            >
                                {visitors
                                    .filter((v) => {
                                        // فیلتر کردن بازاریاب‌هایی که با Buyer یکی هستند
                                        if (!selectedBuyer) return true;
                                        return v.userId !== selectedBuyer.userId;
                                    })
                                    .map((v) => {
                                        const label = `${v.referralCode} - ${v.userDisplayName || v.userPhoneNumber}`;
                                        return (
                                            <Option key={v.id} value={v.id} label={label}>
                                                {label}
                                            </Option>
                                        );
                                    })}
                            </Select>
                        </Form.Item>
                    )}
                </Form>
            </Modal>
        </>
    );
};

export default BuyerManagementPage;

