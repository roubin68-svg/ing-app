// src/features/menuSettings/api/menuApi.js
import apiClient from "../../../core/api/apiClient";

/**
 * API های مربوط به منو
 */

const menuApi = {
    // منوی کاربر فعلی (سمت چپ داشبورد)
    getMyMenu: () => apiClient.get("/menus/my").then((res) => res.data),

    // درخت کامل منو برای مدیریت (صفحه تنظیمات منو)
    getAdminTree: () => apiClient.get("/menus/admin").then((res) => res.data),

    // CRUD
    create: (dto) => apiClient.post("/menus", dto),
    update: (id, dto) => apiClient.put(`/menus/${id}`, dto),
    remove: (id) => apiClient.delete(`/menus/${id}`),

    // تغییر والد
    changeParent: (id, parentId) =>
        apiClient.put(`/menus/${id}/parent`, { parentId }),

    // تغییر ترتیب
    changeOrder: (id, newOrder) =>
        apiClient.put(`/menus/${id}/order`, { newOrder }),

    // تغییر وضعیت فعال / غیرفعال
    changeStatus: (id, isActive) =>
        apiClient.put(`/menus/${id}/status`, { isActive }),

    // تغییر Permission لازم برای منو
    changePermission: (id, permissionCode) =>
        apiClient.put(`/menus/${id}/permission`, { permissionCode }),
};

export default menuApi;
