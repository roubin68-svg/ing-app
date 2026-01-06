// src/features/products/pages/ProductsPage.jsx
import React, { useCallback, useEffect, useState } from "react";
import {
    App,
    Card,
    Table,
    Switch,
    Button,
    Space,
    Modal,
    Form,
    Input,
    Select,
    Row,
    Col,
    Upload,
    Image,
    Spin,
} from "antd";
import { PlusOutlined, EditOutlined, UploadOutlined, DeleteOutlined, PictureOutlined } from "@ant-design/icons";

import productsApi from "../api/productsApi";
import CategoryTreeSelect from "../components/CategoryTreeSelect";

const { Option } = Select;

const ProductsPage = () => {
    const { message: msgApi, modal } = App.useApp();

    // ========================
    // States
    // ========================
    const [loading, setLoading] = useState(false);

    const [data, setData] = useState([]);
    const [total, setTotal] = useState(0);

    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [filters, setFilters] = useState({
        categoryId: null,
        isActive: null,
    });

    const [sortBy, setSortBy] = useState(null);
    const [sortDesc, setSortDesc] = useState(false);

    // Product Modal
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingProduct, setEditingProduct] = useState(null);
    const [form] = Form.useForm();
    
    // Image upload state
    const [imageFile, setImageFile] = useState(null);
    const [imagePreview, setImagePreview] = useState(null);
    const [existingImagePath, setExistingImagePath] = useState(null);
    const [uploadedImagePath, setUploadedImagePath] = useState(null); // مسیر فایل آپلود شده
    const [uploading, setUploading] = useState(false);
    const [imageBlobUrls, setImageBlobUrls] = useState({}); // { "productId_imagePath": blobUrl }

    // ========================
    // Load Products (Paging + Filter)
    // ========================
    const loadProducts = useCallback(
        async (targetPage = page) => {
            try {
                setLoading(true);

                const params = {
                    page: targetPage,
                    pageSize,
                    categoryId: filters.categoryId,
                    isActive: filters.isActive,
                    search: filters.search || null,
                    sortBy,
                    sortDesc,
                };

                const res = await productsApi.getPaged(params);

                const products = (res.items || []).map((p) => ({
                    key: p.id,
                    ...p,
                }));
                setData(products);
                setTotal(res.totalCount || 0);
                setPage(res.page || targetPage);

                // ساخت blob URLs برای تصاویر
                const blobUrlPromises = products
                    .filter(p => p.imagePath)
                    .map(async (p) => {
                        try {
                            const blobUrl = await productsApi.getProductImageBlobUrl(p.id, p.imagePath);
                            if (blobUrl) {
                                return { key: `${p.id}_${p.imagePath}`, blobUrl };
                            }
                        } catch (err) {
                            console.error(`Error loading image for product ${p.id}:`, err);
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
            } catch (err) {
                console.error(err);
                msgApi.error("خطا در دریافت لیست محصولات");
            } finally {
                setLoading(false);
            }
        },
        [page, pageSize, filters, sortBy, sortDesc, msgApi]
    );

    useEffect(() => {
        loadProducts(1);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [pageSize, filters, sortBy, sortDesc]);

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

    // ========================
    // Table change (paging)
    // ========================
    const handleTableChange = (pagination, _filters, sorter) => {
        const newPage = pagination.current;
        const newPageSize = pagination.pageSize;

        setPage(newPage);
        setPageSize(newPageSize);

        if (sorter && sorter.order) {
            setSortBy(sorter.field);
            setSortDesc(sorter.order === "descend");
        } else {
            setSortBy(null);
            setSortDesc(false);
        }

        loadProducts(newPage);
    };


    // ========================
    // Activate / Deactivate
    // ========================
    const handleChangeStatus = async (record, value) => {
        try {
            if (value) {
                await productsApi.activate(record.id);
                msgApi.success("محصول فعال شد");
            } else {
                await productsApi.deactivate(record.id);
                msgApi.success("محصول غیرفعال شد");
            }
            loadProducts();
        } catch (e) {
            console.error(e);
            msgApi.error(e.message || "خطا در تغییر وضعیت محصول");
        }
    };

    // ========================
    // Filters
    // ========================
    const handleSearchClick = () => {
        setPage(1);
        loadProducts(1);
    };

    const handleClearFilters = () => {
        setFilters({
            categoryId: null,
            isActive: null,
            search: "",
        });

        setSortBy(null);
        setSortDesc(false);

        setPage(1);
        loadProducts(1);
    };


    // ========================
    // Modal (Create / Edit)
    // ========================
    const openCreateModal = () => {
        setEditingProduct(null);
        form.resetFields();
        setImageFile(null);
        setImagePreview(null);
        setExistingImagePath(null);
        setUploadedImagePath(null);
        setIsModalOpen(true);
    };

    const openEditModal = async (record) => {
        try {
            setEditingProduct(record);
            const dto = await productsApi.getById(record.id);

            form.setFieldsValue({
                name: dto.name,
                categoryId: dto.categoryId,
                unit: dto.unit,
            });

            setExistingImagePath(dto.imagePath || null);
            setImageFile(null);
            setImagePreview(null);
            setUploadedImagePath(null);

            // اگر تصویر موجود است، blob URL بساز
            if (dto.imagePath) {
                try {
                    const blobUrl = await productsApi.getProductImageBlobUrl(record.id, dto.imagePath);
                    if (blobUrl) {
                        setImageBlobUrls(prev => ({
                            ...prev,
                            [`${record.id}_${dto.imagePath}`]: blobUrl
                        }));
                    }
                } catch (err) {
                    console.error("Error loading existing image:", err);
                }
            }

            setIsModalOpen(true);
        } catch (e) {
            console.error(e);
            msgApi.error("خطا در دریافت اطلاعات محصول");
        }
    };

    const handleModalCancel = () => {
        setIsModalOpen(false);
        setEditingProduct(null);
        form.resetFields();
        setImageFile(null);
        setImagePreview(null);
        setExistingImagePath(null);
        setUploadedImagePath(null);
    };

    const handleFormFinish = async (values) => {
        try {
            // اگر آپلود در حال انجام است، اجازه ذخیره نده
            if (uploading) {
                msgApi.warning("لطفاً صبر کنید تا آپلود تصویر کامل شود");
                return;
            }

            setUploading(true);
            
            let productId;
            let finalImagePath;

            if (editingProduct) {
                // Edit: اگر تصویر جدید آپلود شده، از آن استفاده کن، وگرنه از existing استفاده کن
                if (uploadedImagePath !== null && uploadedImagePath !== undefined) {
                    // تصویر جدید آپلود شده
                    finalImagePath = uploadedImagePath;
                } else {
                    // از تصویر موجود استفاده کن
                    finalImagePath = existingImagePath;
                }

                const payload = {
                    name: values.name.trim(),
                    categoryId: values.categoryId,
                    unit: values.unit?.trim() || null,
                    imagePath: finalImagePath || null,
                };
                await productsApi.update(editingProduct.id, payload);
                msgApi.success("محصول با موفقیت ویرایش شد");
            } else {
                // Create: ابتدا محصول را ایجاد می‌کنیم
                const payload = {
                    name: values.name.trim(),
                    categoryId: values.categoryId,
                    unit: values.unit?.trim() || null,
                    imagePath: null, // ابتدا بدون تصویر
                };
                const created = await productsApi.create(payload);
                productId = created.id;

                // سپس اگر تصویر انتخاب شده بود، آپلود می‌کنیم
                if (imageFile) {
                    try {
                        const result = await productsApi.uploadProductImage({
                            productId,
                            file: imageFile,
                        });
                        
                        // Handle both camelCase and PascalCase (filePath or FilePath)
                        const filePath = result?.filePath || result?.FilePath;
                        if (filePath) {
                            finalImagePath = filePath;
                            // به‌روزرسانی محصول با imagePath
                            await productsApi.update(productId, {
                                name: values.name.trim(),
                                categoryId: values.categoryId,
                                unit: values.unit?.trim() || null,
                                imagePath: finalImagePath,
                            });
                        }
                    } catch (imgErr) {
                        console.error(imgErr);
                        msgApi.warning("محصول ایجاد شد اما آپلود تصویر با خطا مواجه شد");
                    }
                }

                msgApi.success("محصول جدید با موفقیت ایجاد شد");
            }

            handleModalCancel();
            loadProducts();
        } catch (err) {
            console.error(err);
            const msg =
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در ذخیره اطلاعات محصول";
            msgApi.error(msg);
        } finally {
            setUploading(false);
        }
    };

    const handleImageChange = async (info) => {
        if (info.file.status === "removed") {
            setImageFile(null);
            setImagePreview(null);
            setUploadedImagePath(null);
            return;
        }

        const file = info.file.originFileObj || info.file;
        if (!file) return;

        // نمایش پیش‌نمایش
        setImageFile(file);
        const reader = new FileReader();
        reader.onload = (e) => {
            setImagePreview(e.target.result);
        };
        reader.readAsDataURL(file);

        // اگر در حالت edit هستیم، بلافاصله آپلود کنیم
        if (editingProduct) {
            try {
                setUploading(true);
                const result = await productsApi.uploadProductImage({
                    productId: editingProduct.id,
                    file: file,
                });
                
                // Handle both camelCase and PascalCase (filePath or FilePath)
                const filePath = result?.filePath || result?.FilePath;
                if (filePath) {
                    setUploadedImagePath(filePath);
                    msgApi.success("تصویر با موفقیت آپلود شد");
                }
            } catch (err) {
                console.error(err);
                msgApi.error("خطا در آپلود تصویر");
                setImageFile(null);
                setImagePreview(null);
            } finally {
                setUploading(false);
            }
        }
        // برای create، آپلود بعد از ایجاد محصول انجام می‌شود
    };

    const handleRemoveImage = () => {
        setImageFile(null);
        setImagePreview(null);
        setUploadedImagePath(null);
    };

    // ========================
    // Columns
    // ========================
    const columns = [
        {
            title: "تصویر",
            key: "image",
            width: 80,
            render: (_, record) => {
                const blobUrlKey = record.imagePath ? `${record.id}_${record.imagePath}` : null;
                const imageUrl = blobUrlKey ? imageBlobUrls[blobUrlKey] : null;
                
                if (imageUrl) {
                    return (
                        <Image
                            src={imageUrl}
                            alt={record.name}
                            width={50}
                            height={50}
                            style={{ objectFit: "cover", borderRadius: 4 }}
                            preview={false}
                        />
                    );
                }
                
                return (
                    <div
                        style={{
                            width: 50,
                            height: 50,
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                            background: "#f0f0f0",
                            borderRadius: 4,
                            border: "1px solid #d9d9d9",
                        }}
                    >
                        <PictureOutlined style={{ fontSize: 24, color: "#999" }} />
                    </div>
                );
            },
        },
        {
            title: "نام محصول",
            dataIndex: "name",
            width: "25%",
            sorter: true,
        },
        {
            title: "دسته‌بندی",
            dataIndex: "categoryName",
            width: "25%",
            sorter: true,
        },
        {
            title: "واحد",
            dataIndex: "unit",
            width: "15%",
            render: (v) => v || <span style={{ color: "#999" }}>—</span>,
        },
        {
            title: "فعال",
            dataIndex: "isActive",
            width: "10%",
            render: (_, record) => (
                <Switch
                    checked={record.isActive}
                    onChange={(val) => handleChangeStatus(record, val)}
                />
            ),
        },
        {
            title: "عملیات",
            key: "actions",
            width: 200,
            render: (_, record) => (
                <Space size="small">
                    <Button
                        size="small"
                        icon={<EditOutlined />}
                        onClick={() => openEditModal(record)}
                    >
                        ویرایش
                    </Button>
                </Space>
            ),
        },
    ];

    return (
        <>
            <Card
                title="مدیریت محصولات"
                bordered={false}
                extra={
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={openCreateModal}
                    >
                        افزودن محصول جدید
                    </Button>
                }
            >
                {/* ---------------- Filters ---------------- */}
                <Row gutter={12} style={{ marginBottom: 20 }}>
                    <Col span={8}>
                        <CategoryTreeSelect
                            value={filters.categoryId}
                            onChange={(v) =>
                                setFilters({
                                    ...filters,
                                    categoryId: v ?? null,
                                })
                            }
                        />
                    </Col>
                    <Col span={8}>
                        <Input
                            placeholder="جستجو بر اساس نام محصول"
                            value={filters.search}
                            onChange={(e) =>
                                setFilters({ ...filters, search: e.target.value })
                            }
                        />
                    </Col>
                    <Col span={4}>
                        <Select
                            placeholder="وضعیت"
                            allowClear
                            style={{ width: "100%" }}
                            value={filters.isActive}
                            onChange={(v) =>
                                setFilters({
                                    ...filters,
                                    isActive: v === undefined ? null : v,
                                })
                            }
                        >
                            <Option value={true}>فعال</Option>
                            <Option value={false}>غیرفعال</Option>
                        </Select>
                    </Col>

                    <Col span={4}>
                        <Space>
                            <Button type="primary" onClick={handleSearchClick}>
                                جستجو
                            </Button>
                            <Button onClick={handleClearFilters}>پاکسازی</Button>
                        </Space>
                    </Col>
                </Row>

                {/* ---------------- Table ---------------- */}
                <Table
                    loading={loading}
                    dataSource={data}
                    columns={columns}
                    pagination={{
                        current: page,
                        pageSize,
                        total,
                        showSizeChanger: true,
                    }}
                    onChange={handleTableChange}
                    bordered={false}
                />
            </Card>

            {/* ---------------- Modal (Create / Edit) ---------------- */}
            <Modal
                open={isModalOpen}
                title={editingProduct ? "ویرایش محصول" : "افزودن محصول جدید"}
                onCancel={handleModalCancel}
                onOk={() => form.submit()}
                okText="ذخیره"
                cancelText="انصراف"
                destroyOnClose
                confirmLoading={uploading}
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleFormFinish}
                >
                    <Form.Item
                        label="نام محصول"
                        name="name"
                        rules={[
                            { required: true, message: "نام محصول الزامی است" },
                        ]}
                    >
                        <Input />
                    </Form.Item>

                    <Form.Item
                        label="دسته‌بندی"
                        name="categoryId"
                        rules={[
                            {
                                required: true,
                                message: "دسته‌بندی محصول الزامی است",
                            },
                        ]}
                    >
                        <CategoryTreeSelect />
                    </Form.Item>

                    <Form.Item
                        label="واحد"
                        name="unit"
                        rules={[{ required: true, message: "واحد محصول الزامی است" }]}
                    >
                        <Input placeholder="مثال: کیلوگرم، عدد، متر" />
                    </Form.Item>

                    <Form.Item label="تصویر محصول">
                        <Space direction="vertical" style={{ width: "100%" }} size="middle">
                            {/* Existing image */}
                            {existingImagePath && !imageFile && (() => {
                                const blobUrlKey = editingProduct?.id && existingImagePath ? `${editingProduct.id}_${existingImagePath}` : null;
                                const existingImageUrl = blobUrlKey ? imageBlobUrls[blobUrlKey] : null;
                                return existingImageUrl ? (
                                    <div style={{ display: "flex", alignItems: "center", gap: 12, padding: 12, background: "#f6ffed", borderRadius: 6, border: "1px solid #b7eb8f" }}>
                                        <Image
                                            src={existingImageUrl}
                                            alt="تصویر محصول"
                                            width={80}
                                            height={80}
                                            style={{ objectFit: "cover", borderRadius: 4 }}
                                            preview={false}
                                        />
                                        <div style={{ flex: 1 }}>
                                            <div style={{ fontWeight: 500 }}>تصویر موجود</div>
                                            <div style={{ fontSize: 12, color: "#666" }}>برای تغییر تصویر، تصویر جدید انتخاب کنید</div>
                                        </div>
                                    </div>
                                ) : null;
                            })()}

                            {/* Uploading indicator */}
                            {uploading && imageFile && (
                                <div style={{ display: "flex", alignItems: "center", gap: 12, padding: 12, background: "#e6f7ff", borderRadius: 6, border: "1px solid #91d5ff" }}>
                                    <Spin size="small" />
                                    <div style={{ flex: 1 }}>
                                        <div style={{ fontWeight: 500 }}>در حال آپلود...</div>
                                        <div style={{ fontSize: 12, color: "#666" }}>لطفاً صبر کنید</div>
                                    </div>
                                </div>
                            )}

                            {/* New image preview (after upload success) */}
                            {imagePreview && !uploading && (
                                <div style={{ display: "flex", alignItems: "center", gap: 12, padding: 12, background: uploadedImagePath ? "#f6ffed" : "#fff7e6", borderRadius: 6, border: uploadedImagePath ? "1px solid #b7eb8f" : "1px solid #ffd591" }}>
                                    <Image
                                        src={imagePreview}
                                        alt="پیش‌نمایش"
                                        width={80}
                                        height={80}
                                        style={{ objectFit: "cover", borderRadius: 4 }}
                                        preview={false}
                                    />
                                    <div style={{ flex: 1 }}>
                                        <div style={{ fontWeight: 500 }}>{imageFile?.name || "تصویر جدید"}</div>
                                        <div style={{ fontSize: 12, color: "#666" }}>
                                            {uploadedImagePath ? "✅ تصویر با موفقیت آپلود شد" : editingProduct ? "در حال آپلود..." : "برای آپلود، محصول را ذخیره کنید"}
                                        </div>
                                    </div>
                                    <Button
                                        icon={<DeleteOutlined />}
                                        size="small"
                                        danger
                                        onClick={handleRemoveImage}
                                        disabled={uploading}
                                    >
                                        حذف
                                    </Button>
                                </div>
                            )}

                            {/* Upload button */}
                            {!imagePreview && !uploading && (
                                <Upload
                                    beforeUpload={() => false}
                                    onChange={handleImageChange}
                                    showUploadList={false}
                                    accept="image/*"
                                    disabled={uploading}
                                >
                                    <Button icon={<UploadOutlined />} block disabled={uploading}>
                                        {existingImagePath ? "تغییر تصویر" : "انتخاب تصویر"}
                                    </Button>
                                </Upload>
                            )}
                        </Space>
                    </Form.Item>
                </Form>
            </Modal>
        </>
    );
};

export default ProductsPage;
