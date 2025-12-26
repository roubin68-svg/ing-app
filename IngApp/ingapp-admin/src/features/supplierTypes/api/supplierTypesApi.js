// src/features/supplierTypes/api/supplierTypesApi.js
import apiClient from "../../../core/api/apiClient";

const supplierTypesApi = {
    // GET /api/v1/supplier-types  (paged)
    getPaged: async (params) => {
        const res = await apiClient.get("/supplier-types", { params });
        // expected: { items, page, pageSize, totalCount }
        return res.data;
    },

    // GET /api/v1/supplier-types/all
    getAll: async () => {
        const res = await apiClient.get("/supplier-types/all");
        return res.data;
    },

    // GET /api/v1/supplier-types/{id}
    getById: async (id) => {
        const res = await apiClient.get(`/supplier-types/${id}`);
        return res.data;
    },

    // POST /api/v1/supplier-types
    create: async (payload) => {
        // payload: { name, description, isActive }
        const res = await apiClient.post("/supplier-types", payload);
        return res.data;
    },

    // PUT /api/v1/supplier-types/{id}
    update: async (id, payload) => {
        // payload: { name, description, isActive }
        const res = await apiClient.put(`/supplier-types/${id}`, payload);
        return res.data;
    },

    // PUT /api/v1/supplier-types/{id}/activate
    activate: async (id) => {
        const res = await apiClient.put(`/supplier-types/${id}/activate`);
        return res.data;
    },

    // PUT /api/v1/supplier-types/{id}/deactivate
    deactivate: async (id) => {
        const res = await apiClient.put(`/supplier-types/${id}/deactivate`);
        return res.data;
    },
};

export default supplierTypesApi;
