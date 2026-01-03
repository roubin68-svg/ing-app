// src/core/api/apiClient.js
import axios from "axios";

// در صورت نیاز می‌تونی از env بخونی
const apiClient = axios.create({
    baseURL: "http://localhost:5273/api/v1",
    timeout: 60000, // 60 ثانیه timeout برای درخواست‌های طولانی (مثل migration)
});

// ---------------- Request: اضافه کردن JWT ----------------
apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem("token");
        if (token && !config.headers.Authorization) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// ---------------- Response: هماهنگ با ApiResult -----------
apiClient.interceptors.response.use(
    (response) => {
        // اینجا فقط برای statusهای 2xx صدا زده می‌شود
        const payload = response.data;

        // اگر شکل پاسخ از نوع ApiResult بود، بازش می‌کنیم
        if (
            payload &&
            typeof payload === "object" &&
            Object.prototype.hasOwnProperty.call(payload, "success")
        ) {
            if (payload.success) {
                // ✅ در حالت موفق:
                // res.data در بقیه کدها == payload.data
                response.data = payload.data;
                return response;
            } else {
                // اگر به هر دلیلی ApiResult با success=false و status 2xx بیاد
                const msg =
                    payload.message ||
                    "خطا در انجام عملیات. لطفاً دوباره تلاش کنید.";
                const error = new Error(msg);
                error.isApiResult = true;
                error.apiResult = payload;
                // Extract Request ID if available
                error.requestId = payload.requestId || payload.request_id || payload.requestID || null;
                return Promise.reject(error);
            }
        }

        // اگر ApiResult نبود (مثلاً endpoint قدیمی) همون رو پاس می‌دیم
        return response;
    },
    (error) => {
        // اینجا خطاهای status غیر 2xx یا خطای شبکه می‌آید

        // 🔴 فقط این بخش جدید اضافه شده: هندل کردن 401 و توکن منقضی
        try {
            const status = error?.response?.status;

            if (status === 401) {
                // توکن نامعتبر / منقضی → پاک کردن و هدایت به صفحه ورود
                try {
                    localStorage.removeItem("token");
                } catch {
                    // اگر localStorage در دسترس نبود، نادیده می‌گیریم
                }

                if (window.location.pathname !== "/login") {
                    window.location.href = "/login";
                }
            }
        } catch {
            // اگر به هر دلیلی این بخش خطا داد، نمی‌گذاریم کل interceptor بترکد
        }

        try {
            // 🔴 تشخیص خطاهای شبکه و timeout
            const isNetworkError = !error.response;
            const isTimeoutError = error.code === "ECONNABORTED" || error.code === "ETIMEDOUT" || error.message?.includes("timeout");
            const isConnectionError = error.code === "ERR_NETWORK" || error.code === "ECONNREFUSED" || error.message?.includes("Network Error");

            if (isNetworkError || isTimeoutError || isConnectionError) {
                // اگر timeout بود (مثلاً هنگام migration)
                if (isTimeoutError) {
                    error.message = "زمان درخواست به پایان رسید. ممکن است سرور در حال انجام عملیات طولانی (مثل migration) باشد. لطفاً چند لحظه صبر کنید و دوباره تلاش کنید.";
                } 
                // اگر خطای اتصال شبکه بود
                else if (isConnectionError) {
                    error.message = "خطا در اتصال به سرور. لطفاً اتصال اینترنت خود را بررسی کنید.";
                } 
                // سایر خطاهای شبکه
                else {
                    error.message = "خطا در ارتباط با سرور. لطفاً اتصال اینترنت و وضعیت سرور را بررسی کنید.";
                }
            } else if (error.response && error.response.data) {
                const data = error.response.data;

                // اگر بدنه‌اش شبیه ApiResult.Fail بود
                if (
                    typeof data === "object" &&
                    Object.prototype.hasOwnProperty.call(data, "success") &&
                    Object.prototype.hasOwnProperty.call(data, "message")
                ) {
                    error.message = data.message || "خطا در انجام عملیات.";
                    // Extract Request ID if available
                    error.requestId = data.requestId || data.request_id || data.requestID || null;
                } else if (typeof data.message === "string") {
                    error.message = data.message;
                    // Try to extract Request ID from nested object
                    if (typeof data === "object") {
                        error.requestId = data.requestId || data.request_id || data.requestID || null;
                    }
                } else if (typeof data.error === "string") {
                    error.message = data.error;
                    // Try to extract Request ID from nested object
                    if (typeof data === "object") {
                        error.requestId = data.requestId || data.request_id || data.requestID || null;
                    }
                }
            } else if (!error.message) {
                error.message = "خطا در ارتباط با سرور.";
            }
        } catch {
            // اگر parsing خراب شد، همون error اصلی می‌ره
        }

        return Promise.reject(error);
    }
);

export default apiClient;
