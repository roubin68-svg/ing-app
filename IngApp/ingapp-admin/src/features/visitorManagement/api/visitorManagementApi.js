// src/features/visitorManagement/api/visitorManagementApi.js
import apiClient from "../../../core/api/apiClient";

const visitorManagementApi = {
    // لیست صفحه‌بندی‌شده Visitor ها
    getPaged: (params) =>
        apiClient
            .get("/visitor-management", { params })
            .then((res) => res.data),

    // دریافت Visitor بر اساس ID
    getById: (visitorProfileId) =>
        apiClient
            .get(`/visitor-management/${visitorProfileId}`)
            .then((res) => res.data),

    // ایجاد Visitor جدید
    create: (payload) =>
        apiClient
            .post("/visitor-management", payload)
            .then((res) => res.data),

    // ویرایش Visitor
    update: (visitorProfileId, payload) =>
        apiClient
            .put(`/visitor-management/${visitorProfileId}`, payload)
            .then((res) => res.data),

    // تغییر وضعیت فعال/غیرفعال
    changeStatus: (visitorProfileId, isActive) =>
        apiClient
            .put(`/visitor-management/${visitorProfileId}/status`, isActive)
            .then((res) => res.data),

    // حذف Visitor
    delete: (visitorProfileId) =>
        apiClient
            .delete(`/visitor-management/${visitorProfileId}`)
            .then((res) => res.data),

    // دریافت لیست Buyer های یک Visitor
    getBuyers: (visitorProfileId) =>
        apiClient
            .get(`/visitor-management/${visitorProfileId}/buyers`)
            .then((res) => res.data),

    // اضافه کردن Buyer به Visitor
    addBuyer: (visitorProfileId, payload) =>
        apiClient
            .post(`/visitor-management/${visitorProfileId}/buyers`, payload)
            .then((res) => res.data),

    // حذف Buyer از Visitor
    removeBuyer: (visitorProfileId, buyerProfileId) =>
        apiClient
            .delete(`/visitor-management/${visitorProfileId}/buyers/${buyerProfileId}`)
            .then((res) => res.data),

    // دریافت Commission Rules یک Visitor
    getCommissionRules: (visitorProfileId) =>
        apiClient
            .get(`/visitor-management/${visitorProfileId}/commission-rules`)
            .then((res) => res.data),

    // تنظیم Commission Rule برای Visitor
    setCommissionRule: (visitorProfileId, payload) =>
        apiClient
            .post(`/visitor-management/${visitorProfileId}/commission-rules`, payload)
            .then((res) => res.data),

    // حذف Commission Rule از Visitor
    removeCommissionRule: (visitorProfileId, commissionRuleCode) =>
        apiClient
            .delete(`/visitor-management/${visitorProfileId}/commission-rules/${commissionRuleCode}`)
            .then((res) => res.data),
};

export default visitorManagementApi;


