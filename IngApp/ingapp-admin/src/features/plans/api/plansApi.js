// src/features/plans/api/plansApi.js
import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/plans";

const plansApi = {
    // GET /api/v1/plans/paged
    // دریافت لیست Plan ها (با Pagination)
    getPaged: async (params) => {
        const res = await apiClient.get(`${BASE_URL}/paged`, { params });
        return res.data;
    },

    // GET /api/v1/plans
    // دریافت تمام Plan ها
    getAll: async () => {
        const res = await apiClient.get(BASE_URL);
        return res.data;
    },

    // GET /api/v1/plans/{id}
    // دریافت Plan بر اساس Id
    getById: async (id) => {
        const res = await apiClient.get(`${BASE_URL}/${id}`);
        return res.data;
    },

    // POST /api/v1/plans
    // ایجاد Plan جدید
    create: async (payload) => {
        const res = await apiClient.post(BASE_URL, payload);
        return res.data;
    },

    // PUT /api/v1/plans/{id}
    // به‌روزرسانی Plan
    update: async (id, payload) => {
        const res = await apiClient.put(`${BASE_URL}/${id}`, payload);
        return res.data;
    },

    // PUT /api/v1/plans/{id}/status
    // تغییر وضعیت فعال/غیرفعال
    toggleStatus: async (id, isActive) => {
        const res = await apiClient.put(`${BASE_URL}/${id}/status`, { isActive });
        return res.data;
    },

    // DELETE /api/v1/plans/{id}
    // حذف Plan
    delete: async (id) => {
        const res = await apiClient.delete(`${BASE_URL}/${id}`);
        return res.data;
    },
};

export default plansApi;











