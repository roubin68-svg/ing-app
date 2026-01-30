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

    // GET /api/v1/user-subscriptions/users-summary
    // دریافت لیست کاربران با خلاصه اشتراک‌ها
    getUsersWithSubscriptionsSummary: async (params) => {
        const res = await apiClient.get(`${BASE_URL}/users-summary`, { params });
        return res.data;
    },

    // PUT /api/v1/user-subscriptions/{subscriptionId}
    // ویرایش اشتراک کاربر
    update: async (subscriptionId, payload) => {
        const res = await apiClient.put(`${BASE_URL}/${subscriptionId}`, payload);
        return res.data;
    },
};

export default userSubscriptionsApi;


















