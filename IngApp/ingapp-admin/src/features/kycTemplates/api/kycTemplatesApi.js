// src/features/kycTemplates/api/kycTemplatesApi.js

import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/kyc-templates";

const kycTemplatesApi = {
    /**
     * دریافت Template فعال بر اساس SupplierType
     * GET /api/v1/kyc-templates/{supplierTypeId}
     */
    getBySupplierType: async (supplierTypeId) => {
        if (!supplierTypeId) {
            throw new Error("supplierTypeId is required");
        }

        const res = await apiClient.get(`${BASE_URL}/${supplierTypeId}`);
        return res;
    },

    /**
     * ذخیره Template (Upsert)
     * POST /api/v1/kyc-templates
     */
    upsert: async (payload) => {
        if (!payload?.supplierTypeId) {
            throw new Error("supplierTypeId is required");
        }

        const res = await apiClient.post(BASE_URL, payload);
        return res;
    },
};

export default kycTemplatesApi;
