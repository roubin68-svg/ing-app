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
    Image,
} from "antd";
import {
    SearchOutlined,
    ReloadOutlined,
    EyeOutlined,
    PictureOutlined,
} from "@ant-design/icons";
import { useSearchParams } from "react-router-dom";
import dayjs from "dayjs";
import jalaali from "jalaali-js";
import offersApi from "../api/offersApi";
import CategoryTreeSelect from "../../products/components/CategoryTreeSelect";
import OfferDetailDrawer from "../components/OfferDetailDrawer";
import productsApi from "../../products/api/productsApi";

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
    const [imageBlobUrls, setImageBlobUrls] = useState({}); // { "productId_imagePath": blobUrl }

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
            const offersList = result || [];
            setOffers(offersList);

            // ساخت blob URLs برای تصاویر محصولات
            const blobUrlPromises = offersList
                .filter(offer => offer.productImagePath && offer.productId)
                .map(async (offer) => {
                    try {
                        const blobUrl = await productsApi.getProductImageBlobUrl(offer.productId, offer.productImagePath);
                        if (blobUrl) {
                            return { key: `${offer.productId}_${offer.productImagePath}`, blobUrl };
                        }
                    } catch (err) {
                        console.error(`Error loading image for product ${offer.productId}:`, err);
                    }
                    return null;
                });

            const blobUrlResults = await Promise.all(blobUrlPromises);
            const newBlobUrls = {};
            blobUrlResults.forEach(result => {
                if (result) {
                    newBlobUrls[result.key] = result.blobUrl;
                }
            });

            // Revoke old URLs
            setImageBlobUrls(prev => {
                Object.values(prev).forEach(url => {
                    if (url && typeof url === 'string') {
                        window.URL.revokeObjectURL(url);
                    }
                });
                return newBlobUrls;
            });
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

    // Cleanup: Revoke blob URLs when component unmounts
    useEffect(() => {
        return () => {
            Object.values(imageBlobUrls).forEach(url => {
                if (url && typeof url === 'string') {
                    window.URL.revokeObjectURL(url);
                }
            });
        };
    }, [imageBlobUrls]);

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
                <Row gutter={[16, 16]}>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <Form.Item label="کد آگهی" name="offerId">
                            <Input 
                                placeholder="جستجو بر اساس کد آگهی" 
                                type="number"
                            />
                        </Form.Item>
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <Form.Item label="دسته‌بندی" name="categoryId">
                            <CategoryTreeSelect placeholder="همه دسته‌بندی‌ها" />
                        </Form.Item>
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <Form.Item label="نام محصول" name="productName">
                            <Input placeholder="جستجوی نام محصول" allowClear />
                        </Form.Item>
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
                        <Form.Item label="مرتب‌سازی" name="sortBy">
                            <Select
                                placeholder="مرتب‌سازی"
                                options={SORT_OPTIONS}
                                defaultValue="newest"
                            />
                        </Form.Item>
                    </Col>
                    <Col xs={24} sm={12} md={8} lg={6}>
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
                    <Col xs={24} sm={12} md={8} lg={6}>
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
                    <Col xs={24} sm={24} md={24} lg={12}>
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
                                    onClick={handleReset}
                                    block={window.innerWidth < 768}
                                >
                                    پاکسازی
                                </Button>
                            </Space>
                        </Form.Item>
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
                                    style={{ height: "100%", cursor: "pointer", display: "flex", flexDirection: "column" }}
                                    bodyStyle={{ flex: 1, display: "flex", flexDirection: "column" }}
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
                                    <div style={{ flex: 1, display: "flex", flexDirection: "column" }}>
                                        <div style={{ display: "flex", alignItems: "flex-start", gap: 12, marginBottom: 15 }}>
                                            <div style={{ flex: 1 }}>
                                                <Tag color="blue" style={{ marginBottom: 8 }}>
                                                    #{offer.id}
                                                </Tag>
                                                <div style={{ fontSize: 16, fontWeight: "bold", marginBottom: 8 }}>
                                                    {offer.productName}
                                                </div>
                                                <Tag>{offer.productCategoryName}</Tag>
                                            </div>
                                            {(() => {
                                                const blobUrlKey = offer.productImagePath && offer.productId 
                                                    ? `${offer.productId}_${offer.productImagePath}` 
                                                    : null;
                                                const imageUrl = blobUrlKey ? imageBlobUrls[blobUrlKey] : null;
                                                
                                                return imageUrl ? (
                                                    <Image
                                                        src={imageUrl}
                                                        alt={offer.productName}
                                                        width={80}
                                                        height={80}
                                                        style={{ objectFit: "cover", borderRadius: 4, flexShrink: 0, border: "1px solid #f0f0f0" }}
                                                        preview={false}
                                                    />
                                                ) : (
                                                    <div
                                                        style={{
                                                            width: 80,
                                                            height: 80,
                                                            display: "flex",
                                                            alignItems: "center",
                                                            justifyContent: "center",
                                                            background: "#f0f0f0",
                                                            borderRadius: 4,
                                                            border: "1px solid #d9d9d9",
                                                            flexShrink: 0,
                                                        }}
                                                    >
                                                        <PictureOutlined style={{ fontSize: 28, color: "#999" }} />
                                                    </div>
                                                );
                                            })()}
                                        </div>
                                        <div style={{ fontSize: 13, color: "#666", flex: 1, display: "flex", flexDirection: "column" }}>
                                            <div style={{ marginBottom: 6 }}>قیمت واحد: {formatPrice(offer.unitPrice)} تومان</div>
                                            <div style={{ marginBottom: 6 }}>مقدار: {formatPrice(offer.quantity)} {offer.unit}</div>
                                            <div style={{ marginBottom: 6, fontWeight: 500 }}>قیمت کل: {formatPrice(offer.totalPrice)} تومان</div>
                                            {offer.hasTax && offer.taxAmount ? (
                                                <>
                                                    <div style={{ marginBottom: 6 }}>مبلغ مالیات: {formatPrice(offer.taxAmount)} تومان</div>
                                                    <div style={{ marginBottom: 6, fontWeight: 500 }}>
                                                        قیمت کل + مالیات: {formatPrice((offer.totalPrice || 0) + (offer.taxAmount || 0))} تومان
                                                    </div>
                                                </>
                                            ) : (
                                                <div style={{ marginBottom: 6, color: "#52c41a", fontSize: 12 }}>
                                                    این کالا مالیات ندارد
                                                </div>
                                            )}
                                            <div style={{ marginTop: "auto", fontSize: 12 }}>
                                                تاریخ انتشار: {toShamsi(offer.publishedAt) || "-"}
                                            </div>
                                        </div>
                                    </div>
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
