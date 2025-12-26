// src/features/productCategories/api/productCategoryApi.js
import apiClient from "../../../core/api/apiClient";

/**
 * API های مربوط به Product Category
 */
const productCategoryApi = {
    // دریافت لیست کامل دسته‌بندی‌ها (برای Tree)
    getAll: () =>
        apiClient.get("/product-categories").then((res) => res.data),

    // ایجاد دسته‌بندی جدید
    create: (dto) =>
        apiClient.post("/product-categories", dto),

    // ویرایش دسته‌بندی
    update: (id, dto) =>
        apiClient.put(`/product-categories/${id}`, dto),

    // فعال‌سازی دسته‌بندی
    activate: (id) =>
        apiClient.put(`/product-categories/${id}/activate`),

    // غیرفعال‌سازی دسته‌بندی
    deactivate: (id) =>
        apiClient.put(`/product-categories/${id}/deactivate`),
};

export default productCategoryApi;
