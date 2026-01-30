// src/features/visitorProfiles/pages/MyBuyersPage.jsx
import React, { useEffect, useState } from "react";
import {
    Card,
    Table,
    Button,
    Space,
    Input,
    Modal,
    Form,
    message,
    Spin,
    Tag,
    Row,
    Col,
} from "antd";
import { PlusOutlined, UserOutlined, SearchOutlined } from "@ant-design/icons";
import visitorProfilesApi from "../api/visitorProfilesApi";
import jalaali from "jalaali-js";

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

const MyBuyersPage = () => {
    const [form] = Form.useForm();
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState([]);
    const [total, setTotal] = useState(0);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [searchText, setSearchText] = useState("");
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [adding, setAdding] = useState(false);

    useEffect(() => {
        loadBuyers();
    }, [page, pageSize, searchText]);

    const loadBuyers = async () => {
        try {
            setLoading(true);
            const buyers = await visitorProfilesApi.getMyBuyers();
            
            // Client-side filtering and pagination
            let filtered = buyers || [];
            
            // Apply search filter
            if (searchText.trim()) {
                const search = searchText.trim().toLowerCase();
                filtered = filtered.filter((buyer) =>
                    buyer.userPhoneNumber?.toLowerCase().includes(search) ||
                    buyer.userDisplayName?.toLowerCase().includes(search) ||
                    buyer.businessName?.toLowerCase().includes(search)
                );
            }
            
            setTotal(filtered.length);
            
            // Apply pagination
            const start = (page - 1) * pageSize;
            const end = start + pageSize;
            const paginated = filtered.slice(start, end);
            
            setData(
                paginated.map((buyer, index) => ({
                    key: buyer.buyerProfileId,
                    rowNumber: start + index + 1,
                    ...buyer,
                }))
            );
        } catch (error) {
            console.error("Error loading buyers:", error);
            message.error("خطا در دریافت لیست خریداران");
        } finally {
            setLoading(false);
        }
    };

    const handleAddBuyer = async (values) => {
        try {
            setAdding(true);
            await visitorProfilesApi.addMyBuyer({
                mobile: values.mobile.trim(),
                buyerName: values.buyerName?.trim() || null,
            });
            message.success("خریدار با موفقیت اضافه شد");
            form.resetFields();
            setIsModalOpen(false);
            loadBuyers();
        } catch (error) {
            const errorMsg =
                error?.response?.data?.message ||
                error?.response?.data?.Error ||
                error?.message ||
                "خطا در اضافه کردن خریدار";
            message.error(errorMsg);
            console.error(error);
        } finally {
            setAdding(false);
        }
    };

    const handleModalCancel = () => {
        setIsModalOpen(false);
        form.resetFields();
    };

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
            key: "userPhoneNumber",
            render: (text) => text || "-",
        },
        {
            title: "نام نمایشی",
            dataIndex: "userDisplayName",
            key: "userDisplayName",
            render: (text) => text || "-",
        },
        {
            title: "نام کسب‌وکار",
            dataIndex: "businessName",
            key: "businessName",
            render: (text) => text || "-",
        },
        {
            title: "تاریخ معرفی",
            dataIndex: "referredAt",
            key: "referredAt",
            render: (date) => toShamsi(date),
        },
    ];

    return (
        <Card
            title={
                <Space>
                    <UserOutlined />
                    <span>خریداران من</span>
                </Space>
            }
            extra={
                <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={() => setIsModalOpen(true)}
                >
                    افزودن خریدار جدید
                </Button>
            }
        >
            {/* فیلتر جستجو */}
            <Row gutter={12} style={{ marginBottom: 16 }}>
                <Col xs={24} sm={12} md={8} lg={6}>
                    <Input
                        placeholder="جستجو بر اساس شماره موبایل، نام یا کسب‌وکار"
                        prefix={<SearchOutlined />}
                        value={searchText}
                        allowClear
                        onChange={(e) => {
                            setSearchText(e.target.value);
                            setPage(1); // Reset to first page when searching
                        }}
                    />
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
                    onChange: (newPage, newPageSize) => {
                        setPage(newPage);
                        setPageSize(newPageSize);
                    },
                }}
                scroll={{ x: "max-content" }}
            />

            {/* Modal افزودن خریدار */}
            <Modal
                title="افزودن خریدار جدید"
                open={isModalOpen}
                onCancel={handleModalCancel}
                footer={null}
                destroyOnClose
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleAddBuyer}
                >
                    <Form.Item
                        label="شماره موبایل"
                        name="mobile"
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
                        label="نام خریدار (اختیاری)"
                        name="buyerName"
                        rules={[
                            { max: 100, message: "نام نمی‌تواند بیشتر از 100 کاراکتر باشد" },
                        ]}
                    >
                        <Input placeholder="در صورت عدم وجود در سیستم، نام خریدار را وارد کنید" />
                    </Form.Item>

                    <Form.Item>
                        <Space>
                            <Button
                                type="primary"
                                htmlType="submit"
                                loading={adding}
                            >
                                افزودن
                            </Button>
                            <Button onClick={handleModalCancel}>انصراف</Button>
                        </Space>
                    </Form.Item>
                </Form>
            </Modal>
        </Card>
    );
};

export default MyBuyersPage;












