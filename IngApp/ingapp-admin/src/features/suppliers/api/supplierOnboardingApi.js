// src/features/suppliers/api/supplierOnboardingApi.js

import apiClient from "../../../core/api/apiClient";

const BASE_URL = "/supplier-profiles";

const supplierOnboardingApi = {
    // GET /api/v1/supplier-profiles/my
    getMyProfile: async () => {
        const res = await apiClient.get(`${BASE_URL}/my`);
        return res.data;
    },

    // PUT /api/v1/supplier-profiles/my
    upsertMyProfile: async (payload) => {
        const res = await apiClient.put(`${BASE_URL}/my`, payload);
        return res.data;
    },

    // POST /api/v1/supplier-profiles/my/submit
    submit: async () => {
        const res = await apiClient.post(`${BASE_URL}/my/submit`);
        return res.data;
    },

    // GET /api/v1/supplier-profiles/my/has-paid-onboarding
    hasPaidOnboarding: async () => {
        const res = await apiClient.get(`${BASE_URL}/my/has-paid-onboarding`);
        return res.data;
    },
};

export default supplierOnboardingApi;
