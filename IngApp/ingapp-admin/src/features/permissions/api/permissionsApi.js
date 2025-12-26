// src/features/permissions/api/permissionsApi.js
import apiClient from "../../../core/api/apiClient";

const permissionsApi = {
    // GET /api/v1/permissions/paged
    getPaged: async (params) => {
        const res = await apiClient.get("/permissions/paged", { params });
        return res.data; // => { items, page, pageSize, totalCount }
    },

    getById: async (id) => {
        const res = await apiClient.get(`/permissions/${id}`);
        return res.data;
    },

    create: async (payload) => {
        const res = await apiClient.post("/permissions", payload);
        return res.data;
    },

    update: async (id, payload) => {
        const res = await apiClient.put(`/permissions/${id}`, payload);
        return res.data;
    },

    delete: async (id) => {
        const res = await apiClient.delete(`/permissions/${id}`);
        return res.data;
    },
    getRoles: async (id) => {
        const res = await apiClient.get(`/permissions/${id}/roles`);
        return res.data;
    },
    getAll: async () => {
        const res = await apiClient.get(`/permissions/all`);
        return res.data;
    },

};

export default permissionsApi;
