// src/features/buyerManagement/api/buyerManagementApi.js
import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/buyer-management";

const buyerManagementApi = {
    // GET /api/v1/buyer-management
    // دریافت لیست صفحه‌بندی‌شده خریداران
    getPaged: async (params) => {
        const res = await apiClient.get(BASE_URL, { params });
        return res.data;
    },

    // GET /api/v1/buyer-management/{id}
    // دریافت خریدار بر اساس Id
    getById: async (buyerProfileId) => {
        const res = await apiClient.get(`${BASE_URL}/${buyerProfileId}`);
        return res.data;
    },

    // POST /api/v1/buyer-management
    // ایجاد خریدار جدید
    create: async (payload) => {
        const res = await apiClient.post(BASE_URL, payload);
        return res.data;
    },

    // PUT /api/v1/buyer-management/{id}
    // به‌روزرسانی اطلاعات خریدار
    update: async (buyerProfileId, payload) => {
        const res = await apiClient.put(`${BASE_URL}/${buyerProfileId}`, payload);
        return res.data;
    },

    // PUT /api/v1/buyer-management/{id}/referral
    // تنظیم یا تغییر بازاریاب برای خریدار
    setReferral: async (buyerProfileId, payload) => {
        const res = await apiClient.put(`${BASE_URL}/${buyerProfileId}/referral`, payload);
        return res.data;
    },

    // DELETE /api/v1/buyer-management/{id}/referral
    // حذف بازاریاب از خریدار
    removeReferral: async (buyerProfileId) => {
        const res = await apiClient.delete(`${BASE_URL}/${buyerProfileId}/referral`);
        return res.data;
    },

    // DELETE /api/v1/buyer-management/{id}
    // حذف خریدار
    delete: async (buyerProfileId) => {
        const res = await apiClient.delete(`${BASE_URL}/${buyerProfileId}`);
        return res.data;
    },
};

export default buyerManagementApi;












