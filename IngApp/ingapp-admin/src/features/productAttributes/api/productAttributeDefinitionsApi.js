// src/features/productAttributes/api/productAttributeDefinitionsApi.js
import apiClient from "../../../core/api/apiClient";

/**
 * API های مربوط به Product Attribute Definition
 */
const productAttributeDefinitionsApi = {
    // GET /api/v1/product-attribute-definitions/paged
    getPaged: async (params) => {
        const res = await apiClient.get(
            "/product-attribute-definitions/paged",
            { params }
        );
        return res.data; // { items, page, pageSize, totalCount }
    },

    // GET /api/v1/product-attribute-definitions/{id}
    getById: async (id) => {
        const res = await apiClient.get(
            `/product-attribute-definitions/${id}`
        );
        return res.data;
    },

    // POST /api/v1/product-attribute-definitions
    create: async (payload) => {
        const res = await apiClient.post(
            "/product-attribute-definitions",
            payload
        );
        return res.data;
    },

    // PUT /api/v1/product-attribute-definitions/{id}
    update: async (id, payload) => {
        const res = await apiClient.put(
            `/product-attribute-definitions/${id}`,
            payload
        );
        return res.data;
    },

    // PUT /api/v1/product-attribute-definitions/{id}/activate
    activate: async (id) => {
        const res = await apiClient.put(
            `/product-attribute-definitions/${id}/activate`
        );
        return res.data;
    },

    // PUT /api/v1/product-attribute-definitions/{id}/deactivate
    deactivate: async (id) => {
        const res = await apiClient.put(
            `/product-attribute-definitions/${id}/deactivate`
        );
        return res.data;
    },
};

export default productAttributeDefinitionsApi;
