// src/features/buyerProfiles/api/buyerProfilesApi.js
import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/buyer-profiles";

const buyerProfilesApi = {
    // GET /api/v1/buyer-profiles/my
    // دریافت پروفایل Buyer کاربر فعلی
    getMyProfile: async () => {
        const res = await apiClient.get(`${BASE_URL}/my`);
        return res.data;
    },

    // PUT /api/v1/buyer-profiles/my
    // ایجاد یا به‌روزرسانی پروفایل Buyer
    upsertMyProfile: async (payload) => {
        const res = await apiClient.put(`${BASE_URL}/my`, payload);
        return res.data;
    },
};

export default buyerProfilesApi;











