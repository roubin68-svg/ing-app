// src/features/wallet/api/walletApi.js
import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/wallet";

const walletApi = {
    // GET /api/v1/wallet/balance
    // دریافت موجودی کیف پول
    getBalance: async () => {
        const res = await apiClient.get(`${BASE_URL}/balance`);
        return res.data;
    },

    // GET /api/v1/wallet/transactions
    // دریافت لیست تراکنش‌های کیف پول
    getTransactions: async (params) => {
        const res = await apiClient.get(`${BASE_URL}/transactions`, { params });
        return res.data;
    },
};

export default walletApi;




















