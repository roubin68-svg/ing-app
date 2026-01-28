// src/features/commissionRules/api/commissionRulesApi.js
import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/commission-rules";

const commissionRulesApi = {
    // GET /api/v1/commission-rules
    // دریافت لیست تمام قوانین پورسانت
    getAll: async () => {
        const res = await apiClient.get(BASE_URL);
        return res.data;
    },

    // GET /api/v1/commission-rules/{id}
    // دریافت یک قانون پورسانت بر اساس ID
    getById: async (id) => {
        const res = await apiClient.get(`${BASE_URL}/${id}`);
        return res.data;
    },

    // POST /api/v1/commission-rules
    // ایجاد قانون پورسانت جدید
    create: async (payload) => {
        const res = await apiClient.post(BASE_URL, payload);
        return res.data;
    },

    // PUT /api/v1/commission-rules/{id}
    // به‌روزرسانی قانون پورسانت
    update: async (id, payload) => {
        const res = await apiClient.put(`${BASE_URL}/${id}`, payload);
        return res.data;
    },

    // DELETE /api/v1/commission-rules/{id}
    // حذف قانون پورسانت
    delete: async (id) => {
        const res = await apiClient.delete(`${BASE_URL}/${id}`);
        return res.data;
    },
};

export default commissionRulesApi;


