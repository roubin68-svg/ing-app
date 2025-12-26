// src/features/suppliers/api/supplierCategoryAccessApi.js
import apiClient from "../../../core/api/apiClient";

const supplierCategoryAccessApi = {
    /**
     * دریافت Category های مجاز Supplier
     * GET /api/v1/suppliers/{userId}/categories
     */
    getBySupplier: async (userId) => {
        if (!userId) {
            throw new Error("userId is required");
        }

        const res = await apiClient.get(
            `/suppliers/${userId}/categories`
        );
        return res.data; // SupplierCategoryAccessDto[]
    },

    /**
     * Sync دسترسی Category ها
     * POST /api/v1/suppliers/{userId}/categories
     */
    sync: async (userId, payload) => {
        if (!userId) {
            throw new Error("userId is required");
        }

        const res = await apiClient.post(
            `/suppliers/${userId}/categories`,
            payload
        );
        return res.data;
    },
};

export default supplierCategoryAccessApi;
