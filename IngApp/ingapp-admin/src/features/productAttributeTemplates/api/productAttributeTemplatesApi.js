// src/features/productAttributeTemplates/api/productAttributeTemplatesApi.js

import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/product-attribute-templates";

const productAttributeTemplatesApi = {
    /**
     * دریافت Template فعال بر اساس Product
     * GET /api/v1/product-attribute-templates/{productId}
     */
    getByProduct: async (productId) => {
        if (!productId) {
            throw new Error("productId is required");
        }

        const res = await apiClient.get(`${BASE_URL}/${productId}`);
        return res.data; // List<ProductAttributeTemplateItemDto>
    },

    /**
     * ذخیره Template (Upsert)
     * POST /api/v1/product-attribute-templates
     */
    upsert: async (payload) => {
        if (!payload?.productId) {
            throw new Error("productId is required");
        }

        const res = await apiClient.post(BASE_URL, payload);
        return res.data;
    },
};

export default productAttributeTemplatesApi;
