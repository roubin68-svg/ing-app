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
};

export default productsApi;
