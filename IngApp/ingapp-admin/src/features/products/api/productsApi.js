// src/features/products/api/productsApi.js
import apiClient from "../../../core/api/apiClient";

/**
 * API های مربوط به Product
 */
const productsApi = {
    // GET /api/v1/products/paged
    getPaged: async (params) => {
        const res = await apiClient.get("/products/paged", { params });
        return res.data; // { items, page, pageSize, totalCount }
    },

    // GET /api/v1/products/{id}
    getById: async (id) => {
        const res = await apiClient.get(`/products/${id}`);
        return res.data;
    },

    // POST /api/v1/products
    create: async (payload) => {
        const res = await apiClient.post("/products", payload);
        return res.data;
    },

    // PUT /api/v1/products/{id}
    update: async (id, payload) => {
        const res = await apiClient.put(`/products/${id}`, payload);
        return res.data;
    },

    // PUT /api/v1/products/{id}/activate
    activate: async (id) => {
        const res = await apiClient.put(`/products/${id}/activate`);
        return res.data;
    },

    // PUT /api/v1/products/{id}/deactivate
    deactivate: async (id) => {
        const res = await apiClient.put(`/products/${id}/deactivate`);
        return res.data;
    },

    /**
     * POST /api/v1/products/upload-image
     * multipart/form-data: productId, file
     * خروجی: { filePath, originalFileName, size }
     */
    uploadProductImage: async ({ productId, file }) => {
        const formData = new FormData();
        formData.append("productId", String(productId));
        formData.append("file", file);

        const res = await apiClient.post("/products/upload-image", formData, {
            headers: { "Content-Type": "multipart/form-data" },
        });
        return res.data;
    },

    /**
     * GET /api/v1/products/upload-image/image
     * query: { productId, filePath }
     * دریافت تصویر محصول به صورت blob و برگرداندن blob URL
     */
    getProductImageBlobUrl: async (productId, filePath) => {
        if (!filePath) return null;
        try {
            const res = await apiClient.get("/products/upload-image/image", {
                params: { productId, filePath },
                responseType: "blob",
            });
            const blob = new Blob([res.data]);
            return window.URL.createObjectURL(blob);
        } catch (error) {
            console.error("Error loading product image:", error);
            return null;
        }
    },

    /**
     * GET /api/v1/products/upload-image/image
     * query: { productId, filePath }
     * دریافت URL تصویر محصول (deprecated - استفاده از getProductImageBlobUrl)
     */
    getProductImageUrl: (productId, filePath) => {
        if (!filePath) return null;
        return `${apiClient.defaults.baseURL}/products/upload-image/image?productId=${productId}&filePath=${encodeURIComponent(filePath)}`;
    },
};

export default productsApi;
