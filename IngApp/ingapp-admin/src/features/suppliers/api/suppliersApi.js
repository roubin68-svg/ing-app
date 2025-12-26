// src/features/suppliers/api/suppliersApi.js
import apiClient from "../../../core/api/apiClient";

const suppliersApi = {
    // GET /api/v1/supplier-profiles/paged
    getPaged: async (params) => {
        const res = await apiClient.get("/supplier-profiles/paged", { params });
        return res.data; // { items, page, pageSize, totalCount }
    },

    // GET /api/v1/supplier-profiles/{id}
    getById: async (id) => {
        const res = await apiClient.get(`/supplier-profiles/${id}`);
        return res.data;
    },

    // PUT /api/v1/supplier-profiles/{id}/verification-status
    updateVerificationStatus: async (id, payload) => {
        const res = await apiClient.put(
            `/supplier-profiles/${id}/verification-status`,
            payload
        );
        return res.data;
    },

    // GET verification history
    getVerificationHistory: async (id) => {
        const res = await apiClient.get(
            `/supplier-profiles/${id}/verification-history`
        );
        return res.data;
    },

    // GET activity logs
    getActivityLogs: async (id) => {
        const res = await apiClient.get(
            `/supplier-profiles/${id}/activity-logs`
        );
        return res.data;
    },

    // GET /api/v1/supplier-profiles/{id}
    getDetail: async (id) => {
        const res = await apiClient.get(`/supplier-profiles/${id}`);
        return res.data;
    },
    // -------------------- KYC Documents --------------------

    // GET /api/v1/kyc/documents
    getDocuments: async (params) => {
        const res = await apiClient.get("/kyc/documents", { params });
        return res.data; // { items, page, pageSize, totalCount }
    },

    // PUT /api/v1/kyc/documents/{id}/review
    reviewDocument: async (documentId, payload) => {
        const res = await apiClient.put(
            `/kyc/documents/${documentId}/review`,
            payload
        );
        return res.data;
    },

    // GET /api/v1/supplier-profiles/pending-count
    getPendingCount: async () => {
        const res = await apiClient.get("/supplier-profiles/pending-count");
        return res.data; // number
    },

};

export default suppliersApi;
