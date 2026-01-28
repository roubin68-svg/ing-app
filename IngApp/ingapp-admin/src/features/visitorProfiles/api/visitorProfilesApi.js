// src/features/visitorProfiles/api/visitorProfilesApi.js
import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/visitor-profiles";

const visitorProfilesApi = {
    // GET /api/v1/visitor-profiles/my
    // دریافت پروفایل Visitor کاربر فعلی
    getMyProfile: async () => {
        const res = await apiClient.get(`${BASE_URL}/my`);
        return res.data;
    },

    // PUT /api/v1/visitor-profiles/my
    // ایجاد یا به‌روزرسانی پروفایل Visitor
    upsertMyProfile: async (payload) => {
        const res = await apiClient.put(`${BASE_URL}/my`, payload);
        return res.data;
    },

    // GET /api/v1/visitor-profiles/by-code/{referralCode}
    // دریافت پروفایل Visitor بر اساس ReferralCode
    getByReferralCode: async (referralCode) => {
        const res = await apiClient.get(`${BASE_URL}/by-code/${referralCode}`);
        return res.data;
    },

    // GET /api/v1/visitor-profiles/my/buyers
    // دریافت لیست Buyer های Visitor فعلی
    getMyBuyers: async () => {
        const res = await apiClient.get(`${BASE_URL}/my/buyers`);
        return res.data;
    },

    // POST /api/v1/visitor-profiles/my/buyers
    // اضافه کردن Buyer به Visitor فعلی
    addMyBuyer: async (payload) => {
        const res = await apiClient.post(`${BASE_URL}/my/buyers`, payload);
        return res.data;
    },
};

export default visitorProfilesApi;









