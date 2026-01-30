// src/features/walletManagement/api/walletManagementApi.js
import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/wallet-management";

const walletManagementApi = {
    // GET /api/v1/wallet-management/users
    // دریافت لیست کاربران به همراه خلاصه کیف پول
    getWalletUsers: async (params) => {
        const res = await apiClient.get(`${BASE_URL}/users`, { params });
        return res.data;
    },

    // GET /api/v1/wallet-management/users/{userId}/balance
    // دریافت موجودی کیف پول یک کاربر
    getUserBalance: async (userId) => {
        const res = await apiClient.get(`${BASE_URL}/users/${userId}/balance`);
        return res.data;
    },

    // GET /api/v1/wallet-management/users/{userId}/transactions
    // دریافت لیست تراکنش‌های یک کاربر
    getUserTransactions: async (userId, params) => {
        const res = await apiClient.get(`${BASE_URL}/users/${userId}/transactions`, { params });
        return res.data;
    },

    // POST /api/v1/wallet-management/users/{userId}/deposit
    // واریز دستی به کیف پول کاربر
    manualDeposit: async (userId, payload) => {
        const res = await apiClient.post(`${BASE_URL}/users/${userId}/deposit`, payload);
        return res.data;
    },

    // POST /api/v1/wallet-management/users/{userId}/withdrawal
    // برداشت دستی از کیف پول کاربر
    manualWithdrawal: async (userId, payload) => {
        const res = await apiClient.post(`${BASE_URL}/users/${userId}/withdrawal`, payload);
        return res.data;
    },

    // GET /api/v1/wallet-management/transactions
    // گزارش دفتر کل تراکنش‌های مالی (همه کاربران)
    getAllTransactions: async (params) => {
        const res = await apiClient.get(`${BASE_URL}/transactions`, { params });
        return res.data;
    },

    // GET /api/v1/wallet-management/commissions-report
    // گزارش پورسانت‌ها
    getCommissionsReport: async (params) => {
        const res = await apiClient.get(`${BASE_URL}/commissions-report`, { params });
        return res.data;
    },

    // GET /api/v1/wallet-management/income-expense-report
    // گزارش درآمد/هزینه
    getIncomeExpenseReport: async (params) => {
        const res = await apiClient.get(`${BASE_URL}/income-expense-report`, { params });
        return res.data;
    },

    // GET /api/v1/wallet-management/dashboard
    // داشبورد مالی
    getFinancialDashboard: async () => {
        const res = await apiClient.get(`${BASE_URL}/dashboard`);
        return res.data;
    },
};

export default walletManagementApi;

