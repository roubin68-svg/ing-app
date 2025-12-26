// src/features/roles/api/rolesApi.js
import apiClient from "../../../core/api/apiClient";

const rolesApi = {
    // GET /api/v1/Roles/paged
    getPaged: async (params) => {
        const res = await apiClient.get("/roles/paged", { params });
        return res.data; // { items, page, pageSize, totalCount }
    },

    // GET /api/v1/Roles
    getAll: async () => {
        const res = await apiClient.get("/roles");
        return res.data;
    },

    // GET /api/v1/Roles/{id}
    getById: async (id) => {
        const res = await apiClient.get(`/roles/${id}`);
        return res.data;
    },

    // POST /api/v1/Roles
    create: async (payload) => {
        const res = await apiClient.post("/roles", payload);
        return res.data;
    },

    // PUT /api/v1/Roles/{id}
    update: async (id, payload) => {
        const res = await apiClient.put(`/roles/${id}`, payload);
        return res.data;
    },

    // DELETE /api/v1/Roles/{id}
    delete: async (id) => {
        const res = await apiClient.delete(`/roles/${id}`);
        return res.data;
    },

    // POST /api/v1/Roles/{id}/permissions
    assignPermissions: async (id, payload) => {
        // payload: { permissionCodes: string[] }
        const res = await apiClient.post(`/roles/${id}/permissions`, payload);
        return res.data;
    },
};

export default rolesApi;
