// src/features/kycAttributeDefinitions/api/kycAttributeDefinitionsApi.js
import apiClient from "../../../core/api/apiClient";

const kycAttributeDefinitionsApi = {
    // GET /api/v1/kyc-attribute-definitions (paged)
    getPaged: async (params) => {
        const res = await apiClient.get("/kyc-attribute-definitions", { params });
        return res.data;
    },

    // GET /api/v1/kyc-attribute-definitions/{id}
    getById: async (id) => {
        const res = await apiClient.get(`/kyc-attribute-definitions/${id}`);
        return res.data;
    },

    // POST /api/v1/kyc-attribute-definitions
    create: async (payload) => {
        const res = await apiClient.post("/kyc-attribute-definitions", payload);
        return res.data;
    },

    // PUT /api/v1/kyc-attribute-definitions/{id}
    update: async (id, payload) => {
        const res = await apiClient.put(`/kyc-attribute-definitions/${id}`, payload);
        return res.data;
    },
};

export default kycAttributeDefinitionsApi;
