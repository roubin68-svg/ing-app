// src/features/offers/api/offersApi.js
import apiClient from "../../../core/api/apiClient";

/**
 * Offers APIs
 * - Public: /api/v1/offers
 * - Supplier (My): /api/v1/offers/my
 * - Upload file: /api/v1/offers/my/upload-file
 */
const offersApi = {
    // ---------------------------
    // Supplier - My Offers
    // ---------------------------

    /**
     * GET /api/v1/offers/my
     * query: { page, pageSize, status?, productCategoryId?, productName?, sortBy?, sortDir? }
     */
    getMyOffers: async (params) => {
        const res = await apiClient.get("/offers/my", { params });
        return res.data;
    },

    /**
     * GET /api/v1/offers/my/available-products
     * خروجی: درخت دسته‌بندی + محصولات مجاز برای supplier
     */
    getAvailableProducts: async () => {
        const res = await apiClient.get("/offers/my/available-products");
        return res.data;
    },

    /**
     * POST /api/v1/offers/my
     * body: { productId }
     * خروجی: { offerId }
     */
    createDraft: async (payload) => {
        const res = await apiClient.post("/offers/my", payload);
        return res.data;
    },

    /**
     * PUT /api/v1/offers/my/{offerId}/product
     * body: { productId }
     * تغییر محصول روی Draft (reset کامل اطلاعات)
     */
    changeProduct: async (offerId, payload) => {
        const res = await apiClient.put(`/offers/my/${offerId}/product`, payload);
        return res.data;
    },

    /**
     * GET /api/v1/offers/my/{offerId}
     */
    getMyOfferDetail: async (offerId) => {
        const res = await apiClient.get(`/offers/my/${offerId}`);
        return res.data;
    },

    /**
     * PUT /api/v1/offers/my/{offerId}/header
     * body: UpdateOfferHeaderRequest
     */
    updateHeader: async (offerId, payload) => {
        const res = await apiClient.put(`/offers/my/${offerId}/header`, payload);
        return res.data;
    },

    /**
     * PUT /api/v1/offers/my/{offerId}/documents
     * body: { items: [{ attributeDefinitionId, value?, filePath? }] }
     */
    saveDocuments: async (offerId, payload) => {
        const res = await apiClient.put(`/offers/my/${offerId}/documents`, payload);
        return res.data;
    },

    /**
     * POST /api/v1/offers/my/{offerId}/submit
     */
    submit: async (offerId) => {
        const res = await apiClient.post(`/offers/my/${offerId}/submit`);
        return res.data;
    },

    /**
     * POST /api/v1/offers/my/{offerId}/cancel
     * body: string? (reason)
     */
    cancel: async (offerId, reason) => {
        const res = await apiClient.post(`/offers/my/${offerId}/cancel`, reason ?? null, {
            headers: { "Content-Type": "application/json" },
        });
        return res.data;
    },

    /**
     * POST /api/v1/offers/my/upload-file
     * multipart/form-data: offerId, file
     * خروجی: { filePath, originalFileName, size }
     */
    uploadMyOfferFile: async ({ offerId, file }) => {
        const formData = new FormData();
        formData.append("offerId", String(offerId));
        formData.append("file", file);

        const res = await apiClient.post("/offers/my/upload-file", formData, {
            headers: { "Content-Type": "multipart/form-data" },
        });
        return res.data;
    },

    /**
     * GET /api/v1/offers/my/upload-file/file
     * query: { offerId, filePath }
     * دانلود فایل آگهی
     */
    downloadOfferFile: async (offerId, filePath, originalFileName) => {
        try {
            const res = await apiClient.get("/offers/my/upload-file/file", {
                params: { offerId, filePath },
                responseType: "blob",
            });

            // ایجاد URL برای blob
            const url = window.URL.createObjectURL(new Blob([res.data]));
            const link = document.createElement("a");
            link.href = url;
            link.setAttribute("download", originalFileName || "file");
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
        } catch (error) {
            throw error;
        }
    },

    /**
     * DELETE /api/v1/offers/my/upload-file/file
     * query: { offerId, filePath }
     * حذف فایل آگهی (Soft Delete)
     */
    deleteOfferFile: async (offerId, filePath) => {
        const res = await apiClient.delete("/offers/my/upload-file/file", {
            params: { offerId, filePath },
        });
        return res.data;
    },

    // ---------------------------
    // Public Offers
    // ---------------------------

    /**
     * GET /api/v1/offers
     * query: PublicOfferSearchQuery
     */
    searchPublic: async (params) => {
        const res = await apiClient.get("/offers", { params });
        return res.data;
    },

    /**
     * GET /api/v1/offers/{offerId}
     */
    getPublicDetail: async (offerId) => {
        const res = await apiClient.get(`/offers/${offerId}`);
        return res.data;
    },

    /**
     * GET /api/v1/offers/{offerId}/files
     * query: { offerId, filePath }
     * دانلود فایل آگهی عمومی
     */
    downloadPublicOfferFile: async (offerId, filePath, originalFileName) => {
        try {
            const res = await apiClient.get(`/offers/${offerId}/files`, {
                params: { offerId, filePath },
                responseType: "blob",
            });

            // ایجاد URL برای blob
            const url = window.URL.createObjectURL(new Blob([res.data]));
            const link = document.createElement("a");
            link.href = url;
            link.setAttribute("download", originalFileName || "file");
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
        } catch (error) {
            throw error;
        }
    },

    /**
 * GET /api/v1/product-attribute-templates/by-product/{productId}
 */
    getProductAttributeTemplates: async (productId) => {
        const res = await apiClient.get(
            `/product-attribute-templates/${productId}`
        );
        return res.data;
    },

    // ---------------------------
    // Supplier Contact & Click Tracking
    // ---------------------------

    /**
     * POST /api/v1/offers/{offerId}/contact-click
     * ثبت کلیک روی اطلاعات تماس
     */
    logContactClick: async (offerId) => {
        const res = await apiClient.post(`/offers/${offerId}/contact-click`);
        return res.data;
    },

    /**
     * GET /api/v1/offers/{offerId}/supplier-contact
     * دریافت اطلاعات تماس supplier
     */
    getSupplierContact: async (offerId) => {
        const res = await apiClient.get(`/offers/${offerId}/supplier-contact`);
        return res.data;
    },

    // ============================================
    // Admin APIs
    // ============================================
    getAdminOffers: async (params) => {
        const res = await apiClient.get("/admin/offers", { params });
        return res.data;
    },

    rejectOffer: async (offerId, reason) => {
        const res = await apiClient.put(`/admin/offers/${offerId}/reject`, { reason });
        return res.data;
    },

    getAdminOfferDetail: async (offerId) => {
        const res = await apiClient.get(`/admin/offers/${offerId}`);
        return res.data;
    },

    getOfferStatusHistory: async (offerId) => {
        const res = await apiClient.get(`/admin/offers/${offerId}/status-history`);
        return res.data;
    },
};

export default offersApi;
