// src/features/users/api/userApi.jsx
import apiClient from "../../../core/api/apiClient";


const userApi = {
    // لیست صفحه‌بندی‌شده کاربران
    getPaged: (params) =>
        apiClient
            .get("/users", { params })
            .then((res) => res.data),

    // دریافت اطلاعات یک کاربر بر اساس شناسه
    getById: (id) =>
        apiClient
            .get(`/users/${id}`)
            .then((res) => res.data),

    // ایجاد کاربر جدید
    create: (payload) =>
        apiClient
            .post("/users", payload)
            .then((res) => res.data),

    // ویرایش کاربر
    update: (id, payload) =>
        apiClient
            .put(`/users/${id}`, payload)
            .then((res) => res.data),

    // تغییر وضعیت فعال/غیرفعال
    changeStatus: (id, payload) =>
        apiClient
            .put(`/users/${id}/status`, payload)
            .then((res) => res.data),

    // افزودن نقش به کاربر
    assignRole: (userId, roleId) =>
        apiClient
            .post(`/users/${userId}/roles`, { roleId })
            .then((res) => res.data),

    // حذف نقش از کاربر
    removeRole: (userId, roleId) =>
        apiClient
            .delete(`/users/${userId}/roles/${roleId}`)
            .then((res) => res.data),

    // تنظیم رمز عبور (Admin)
    setPassword: (userId, password) =>
        apiClient
            .post(`/users/${userId}/set-password`, { password })
            .then((res) => res.data),
};

export default userApi;
