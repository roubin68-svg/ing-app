// src/features/subscriptions/api/subscriptionsApi.js
import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/subscriptions";

const subscriptionsApi = {
    // GET /api/v1/subscriptions/plans
    // دریافت لیست پلن‌های فعال
    getActivePlans: async () => {
        const res = await apiClient.get(`${BASE_URL}/plans`);
        return res.data;
    },

    // GET /api/v1/subscriptions/my/active
    // دریافت اشتراک فعال کاربر
    getMyActiveSubscription: async () => {
        const res = await apiClient.get(`${BASE_URL}/my/active`);
        return res.data;
    },

    // GET /api/v1/subscriptions/my/history
    // دریافت تاریخچه اشتراک‌های کاربر
    getMySubscriptionHistory: async () => {
        const res = await apiClient.get(`${BASE_URL}/my/history`);
        return res.data;
    },

    // POST /api/v1/subscriptions/purchase
    // خرید اشتراک
    purchaseSubscription: async (payload) => {
        const res = await apiClient.post(`${BASE_URL}/purchase`, payload);
        return res.data;
    },

    // POST /api/v1/subscriptions/{subscriptionId}/cancel
    // لغو اشتراک
    cancelSubscription: async (subscriptionId) => {
        const res = await apiClient.post(`${BASE_URL}/${subscriptionId}/cancel`);
        return res.data;
    },
};

export default subscriptionsApi;


















