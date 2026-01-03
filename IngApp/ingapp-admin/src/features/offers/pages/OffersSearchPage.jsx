// src/features/offers/pages/OffersSearchPage.jsx
import React, { useEffect, useState, useCallback } from "react";
import {
    App,
    Button,
    Card,
    Col,
    Form,
    Input,
    InputNumber,
    Row,
    Select,
    Space,
    Spin,
    Tag,
    Empty,
    Pagination,
} from "antd";
import {
    SearchOutlined,
    ReloadOutlined,
    EyeOutlined,
} from "@ant-design/icons";
import { useSearchParams } from "react-router-dom";
import dayjs from "dayjs";
import jalaali from "jalaali-js";
import offersApi from "../api/offersApi";
import CategoryTreeSelect from "../../products/components/CategoryTreeSelect";
import OfferDetailDrawer from "../components/OfferDetailDrawer";

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

const SORT_OPTIONS = [
    { value: "newest", label: "جدیدترین" },
    { value: "oldest", label: "قدیمی‌ترین" },
    { value: "priceAsc", label: "قیمت: کم به زیاد" },
    { value: "priceDesc", label: "قیمت: زیاد به کم" },
    { value: "quantityAsc", label: "مقدار: کم به زیاد" },
    { value: "quantityDesc", label: "مقدار: زیاد به کم" },
];

const formatPrice = (v) =>
    v != null
        ? v.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
        : "-";

const OffersSearchPage = () => {
    const { message: messageApi } = App.useApp();
    const [form] = Form.useForm();
    const [searchParams, setSearchParams] = useSearchParams();

    // -----------------------
    // State
    // -----------------------
    const [loading, setLoading] = useState(false);
    const [offers, setOffers] = useState([]);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(20);
    const [selectedOfferId, setSelectedOfferId] = useState(null);
    const [drawerVisible, setDrawerVisible] = useState(false);

    // چک کردن وجود offerId در URL برای باز کردن دراور در ابتدا
    useEffect(() => {
        const offerIdParam = searchParams.get("offerId");
        if (offerIdParam) {
            setSelectedOfferId(Number(offerIdParam));
            setDrawerVisible(true);
        }
    }, [searchParams]);

    // -----------------------
    // Load Offers
    // -----------------------
    const loadOffers = useCallback(async () => {
        try {
            setLoading(true);
            const values = form.getFieldsValue();
            
            const params = {
                page,
                pageSize,
                offerId: values.offerId ? Number(values.offerId) : undefined,
                categoryId: values.categoryId || undefined,
                productId: values.productId || undefined,
                productName: values.productName?.trim() || undefined,
                minPrice: values.minPrice || undefined,
                maxPrice: values.maxPrice || undefined,
                sortBy: values.sortBy || "newest",
            };

            // حذف undefined values
            Object.keys(params).forEach(key => params[key] === undefined && delete params[key]);

            const result = await offersApi.searchPublic(params);
            setOffers(result || []);
        } catch (error) {
            messageApi.error("خطا در بارگذاری آگهی‌ها");
            console.error(error);
        } finally {
            setLoading(false);
        }
    }, [form, page, pageSize, messageApi]);

    useEffect(() => {
        loadOffers();
    }, [loadOffers]);

    // -----------------------
    // Handlers
    // -----------------------
    const handleSearch = () => {
        setPage(1);
        loadOffers();
    };

    const handleReset = () => {
        form.resetFields();
        setPage(1);
        setTimeout(() => {
            loadOffers();
        }, 100);
    };

    const handleViewOffer = async (offerId) => {
        setSelectedOfferId(offerId);
        setDrawerVisible(true);
    };

    const handleCloseDrawer = () => {
        setDrawerVisible(false);
        setSelectedOfferId(null);
        // پاک کردن پارامتر از URL بدون ریلود صفحه
        if (searchParams.has("offerId")) {
            searchParams.delete("offerId");
            setSearchParams(searchParams);
        }
    };

    return (
        <Card title="جستجو و فیلتر آگهی‌ها">
            {/* Filters */}
            <Form
                form={form}
                layout="vertical"
                style={{ marginBottom: 16 }}
            >
                <Row gutter={16}>
                    <Col span={6}>
                        <Form.Item label="کد آگهی" name="offerId">
                            <Input 
                                placeholder="جستجو بر اساس کد آگهی" 
                                type="number"
                            />
                        </Form.Item>
                    </Col>
                    <Col span={6}>
                        <Form.Item label="دسته‌بندی" name="categoryId">
                            <CategoryTreeSelect placeholder="همه دسته‌بندی‌ها" />
                        </Form.Item>
                    </Col>
                    <Col span={6}>
                        <Form.Item label="نام محصول" name="productName">
                            <Input placeholder="جستجوی نام محصول" allowClear />
                        </Form.Item>
                    </Col>
                    <Col span={6}>
                        <Form.Item label="مرتب‌سازی" name="sortBy">
                            <Select
                                placeholder="مرتب‌سازی"
                                options={SORT_OPTIONS}
                                defaultValue="newest"
                            />
                        </Form.Item>
                    </Col>
                </Row>
                <Row gutter={16}>
                    <Col span={6}>
                        <Form.Item label="حداقل قیمت واحد (تومان)" name="minPrice">
                            <InputNumber
                                style={{ width: "100%" }}
                                min={0}
                                formatter={(value) =>
                                    value ? `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",") : ""
                                }
                                parser={(value) =>
                                    value ? value.replace(/[^\d]/g, "") : ""
                                }
                                placeholder="حداقل قیمت"
                            />
                        </Form.Item>
                    </Col>
                    <Col span={6}>
                        <Form.Item label="حداکثر قیمت واحد (تومان)" name="maxPrice">
                            <InputNumber
                                style={{ width: "100%" }}
                                min={0}
                                formatter={(value) =>
                                    value ? `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",") : ""
                                }
                                parser={(value) =>
                                    value ? value.replace(/[^\d]/g, "") : ""
                                }
                                placeholder="حداکثر قیمت"
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
                                onClick={handleReset}
                            >
                                پاکسازی
                            </Button>
                        </Space>
                    </Col>
                </Row>
            </Form>

            {/* Offers Cards */}
            {loading ? (
                <div style={{ textAlign: "center", padding: 48 }}>
                    <Spin size="large" />
                </div>
            ) : offers.length === 0 ? (
                <Empty description="آگهی‌ای یافت نشد" />
            ) : (
                <>
                    <Row gutter={[16, 16]}>
                        {offers.map((offer) => (
                            <Col xs={24} sm={12} md={8} lg={6} key={offer.id}>
                                <Card
                                    hoverable
                                    style={{ height: "100%", cursor: "pointer" }}
                                    onClick={() => handleViewOffer(offer.id)}
                                    actions={[
                                        <Button
                                            type="link"
                                            icon={<EyeOutlined />}
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                handleViewOffer(offer.id);
                                            }}
                                            key="view"
                                        >
                                            مشاهده جزئیات
                                        </Button>
                                    ]}
                                >
                                    <div style={{ marginBottom: 12 }}>
                                        <Tag color="blue" style={{ marginBottom: 8 }}>
                                            #{offer.id}
                                        </Tag>
                                    </div>
                                    <Card.Meta
                                        title={
                                            <div style={{ fontSize: 16, fontWeight: "bold", marginBottom: 12 }}>
                                                {offer.productName}
                                            </div>
                                        }
                                        description={
                                            <Space direction="vertical" size="middle" style={{ width: "100%" }}>
                                                <div>
                                                    <Tag>{offer.productCategoryName}</Tag>
                                                </div>
                                                <div style={{ fontSize: 13, color: "#666" }}>
                                                    <div style={{ marginBottom: 6 }}>قیمت واحد: {formatPrice(offer.unitPrice)} تومان</div>
                                                    <div style={{ marginBottom: 6 }}>قیمت کل: {formatPrice(offer.totalPrice)} تومان</div>
                                                    <div style={{ marginBottom: 6 }}>مقدار: {formatPrice(offer.quantity)} {offer.unit}</div>
                                                    <div style={{ marginTop: 8, fontSize: 12 }}>
                                                        تاریخ انتشار: {toShamsi(offer.publishedAt) || "-"}
                                                    </div>
                                                </div>
                                            </Space>
                                        }
                                    />
                                </Card>
                            </Col>
                        ))}
                    </Row>
                    <div style={{ marginTop: 24, textAlign: "center" }}>
                        <Pagination
                            current={page}
                            pageSize={pageSize}
                            total={offers.length}
                            showSizeChanger
                            showTotal={(total) => `مجموع ${total} آگهی`}
                            onChange={(newPage, newPageSize) => {
                                setPage(newPage);
                                setPageSize(newPageSize);
                            }}
                        />
                    </div>
                </>
            )}

            <OfferDetailDrawer
                offerId={selectedOfferId}
                visible={drawerVisible}
                onClose={handleCloseDrawer}
            />
        </Card>
    );
};

export default OffersSearchPage;
