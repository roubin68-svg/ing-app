// src/features/commissions/api/commissionsApi.js
import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/commissions";

const commissionsApi = {
    // GET /api/v1/commissions/my
    // دریافت لیست پورسانت‌های بازاریاب
    getMyCommissions: async (params) => {
        const res = await apiClient.get(`${BASE_URL}/my`, { params });
        return res.data;
    },

    // GET /api/v1/commissions/my/total
    // دریافت مجموع پورسانت‌های بازاریاب
    getMyTotalCommission: async () => {
        const res = await apiClient.get(`${BASE_URL}/my/total`);
        return res.data;
    },
};

export default commissionsApi;











