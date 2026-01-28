// src/features/userSubscriptions/api/userSubscriptionsApi.js
import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/user-subscriptions";

const userSubscriptionsApi = {
    // GET /api/v1/user-subscriptions/paged
    // دریافت لیست اشتراک‌ها با Pagination و فیلتر
    getPaged: async (params) => {
        const res = await apiClient.get(`${BASE_URL}/paged`, { params });
        return res.data;
    },
};

export default userSubscriptionsApi;











