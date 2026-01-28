// src/features/payments/api/paymentsApi.js
import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/payments";

const paymentsApi = {
    // GET /api/v1/payments/gateways
    // دریافت لیست درگاه‌های پرداخت فعال
    getActiveGateways: async () => {
        const res = await apiClient.get(`${BASE_URL}/gateways`);
        return res.data;
    },

    // POST /api/v1/payments/topup
    // ایجاد درخواست TopUp (شارژ کیف پول)
    createTopUpRequest: async (payload) => {
        const res = await apiClient.post(`${BASE_URL}/topup`, payload);
        return res.data;
    },

    // POST /api/v1/payments/verify/{paymentId}
    // تایید پرداخت (Callback از درگاه)
    verifyPayment: async (paymentId, payload) => {
        const res = await apiClient.post(`${BASE_URL}/verify/${paymentId}`, payload);
        return res.data;
    },

    // GET /api/v1/payments/{paymentId}/status
    // دریافت وضعیت پرداخت
    getPaymentStatus: async (paymentId) => {
        const res = await apiClient.get(`${BASE_URL}/${paymentId}/status`);
        return res.data;
    },
};

export default paymentsApi;











