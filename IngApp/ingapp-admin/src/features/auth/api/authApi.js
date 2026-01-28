// src/features/auth/api/authApi.js
import apiClient from "../../../core/api/apiClient";

// 🔹 ارسال OTP
export const sendOtpApi = (phoneNumber) => {
  return apiClient.post("/auth/send-otp", {
    phoneNumber: phoneNumber,
  });
};

// 🔹 تایید OTP و دریافت JWT
export const verifyOtpApi = (phoneNumber, code) => {
  return apiClient.post("/auth/verify-otp", {
    phoneNumber: phoneNumber,
    code: code, // ⚠️ طبق قرارداد API باید نام فیلد "code" باشد
  });
};

// 🔹 گرفتن اطلاعات کاربر لاگین‌شده (/api/v1/auth/me)
export const getMeApi = () => {
  // baseURL در apiClient از قبل شامل /api/v1 هست،
  // برای همین فقط "auth/me" رو صدا می‌زنیم
  return apiClient.get("/auth/me");
};

// 🔹 به‌روزرسانی پروفایل خود کاربر
export const updateMyProfileApi = (payload) => {
  return apiClient.put("/auth/me", payload);
};

// 🔹 ورود با رمز عبور
export const loginWithPasswordApi = (phoneNumber, password) => {
  return apiClient.post("/auth/login-with-password", {
    phoneNumber: phoneNumber,
    password: password,
  });
};

// 🔹 تنظیم یا تغییر رمز عبور
export const setPasswordApi = (payload) => {
  return apiClient.post("/auth/set-password", payload);
};
