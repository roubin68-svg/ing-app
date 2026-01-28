// src/core/auth/AuthContext.jsx
import React, { createContext, useContext, useState } from "react";
import { message } from "antd";
import { sendOtpApi, verifyOtpApi, loginWithPasswordApi } from "../../features/auth/api/authApi";

const AuthContext = createContext();

export const useAuthContext = () => useContext(AuthContext);

export const AuthProvider = ({ children }) => {
    const [token, setToken] = useState(localStorage.getItem("token") || null);
    const [isLoading, setIsLoading] = useState(false);

    const extractMessage = (err) => {
        try {
            if (err?.message) return err.message;

            if (!err.response) return "خطای ناشناخته رخ داد!";
            const data = err.response.data;

            if (typeof data === "string") return data;
            if (data && typeof data === "object") {
                return (
                    data.message ||
                    data.error ||
                    "خطا در ارتباط با سرور. لطفاً دوباره تلاش کنید."
                );
            }

            return "خطای ناشناخته در ارتباط با سرور.";
        } catch {
            return "خطای نامشخص!";
        }
    };

    // ---------------------------------------------------
    // SEND OTP  — returns:  { ok: boolean, message: string }
    // ---------------------------------------------------
    const sendOtp = async (phoneNumber) => {
        setIsLoading(true);

        try {
            const res = await sendOtpApi(phoneNumber);
            // به‌خاطر interceptor، اینجا res.data == AuthResponse است
            const data = res?.data || {};
            const msg = data.message || "کد با موفقیت ارسال شد.";

            message.success(msg);
            return { ok: true, message: msg };
        } catch (err) {
            const msg = extractMessage(err);
            message.error(msg);
            return { ok: false, message: msg };
        } finally {
            setIsLoading(false);
        }
    };

    // ---------------------------------------------------
    // VERIFY OTP  — returns:  { ok: boolean, message: string }
    // ---------------------------------------------------
    const loginWithOtp = async (phoneNumber, otp) => {
        setIsLoading(true);

        try {
            const res = await verifyOtpApi(phoneNumber, otp);
            // اینجا هم res.data == AuthResponse است
            const data = res?.data || {};

            const jwt = data.token || data.jwt || null;
            const msg = data.message || "ورود با موفقیت انجام شد.";

            if (!jwt) {
                message.error(msg);
                return { ok: false, message: msg };
            }

            localStorage.setItem("token", jwt);
            setToken(jwt);

            message.success(msg);
            return { ok: true, message: msg };
        } catch (err) {
            const msg = extractMessage(err);
            message.error(msg);
            return { ok: false, message: msg };
        } finally {
            setIsLoading(false);
        }
    };

    // ---------------------------------------------------
    // LOGIN WITH PASSWORD  — returns:  { ok: boolean, message: string }
    // ---------------------------------------------------
    const loginWithPassword = async (phoneNumber, password) => {
        setIsLoading(true);

        try {
            const res = await loginWithPasswordApi(phoneNumber, password);
            const data = res?.data || {};

            const jwt = data.token || data.jwt || null;
            const msg = data.message || "ورود با موفقیت انجام شد.";

            if (!jwt) {
                message.error(msg);
                return { ok: false, message: msg };
            }

            localStorage.setItem("token", jwt);
            setToken(jwt);

            message.success(msg);
            return { ok: true, message: msg };
        } catch (err) {
            const msg = extractMessage(err);
            message.error(msg);
            return { ok: false, message: msg };
        } finally {
            setIsLoading(false);
        }
    };

    // ---------------------------------------------------
    // LOGOUT
    // ---------------------------------------------------
    const logout = () => {
        localStorage.removeItem("token");
        setToken(null);
    };

    return (
        <AuthContext.Provider
            value={{
                token,
                isLoading,
                sendOtp,
                loginWithOtp,
                loginWithPassword,
                logout,
                isAuthenticated: !!token,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
};

// فایل useAuth.js هم از قبل هست و فقط از useAuthContext استفاده می‌کند
// src/core/auth/useAuth.js
// export const useAuth = () => useAuthContext();
