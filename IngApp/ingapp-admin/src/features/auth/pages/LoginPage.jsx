// src/features/auth/pages/LoginPage.jsx
import React, { useState, useEffect, useRef } from "react";
import { Card, Form, Input, Button, Typography, Space } from "antd";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../../core/auth/useAuth";
import "../../../theme/font.css";
import "../../../theme/login.css";

const { Title, Text } = Typography;

const LoginPage = () => {
    const { sendOtp, loginWithOtp, isLoading, isAuthenticated } = useAuth();

    const [step, setStep] = useState("phone");
    const [phoneNumber, setPhoneNumber] = useState("");
    const [lastPhoneNumber, setLastPhoneNumber] = useState("");
    const [resendSeconds, setResendSeconds] = useState(0);

    const [otpDigits, setOtpDigits] = useState(["", "", "", "", "", ""]);
    const [otpError, setOtpError] = useState("");
    const [phoneError, setPhoneError] = useState("");

    const inputsRef = useRef([]);
    const [phoneForm] = Form.useForm();
    const navigate = useNavigate();

    useEffect(() => {
        if (isAuthenticated) navigate("/", { replace: true });
    }, [isAuthenticated, navigate]);

    useEffect(() => {
        if (resendSeconds <= 0) return;
        const timer = setInterval(() => {
            setResendSeconds((prev) => (prev > 0 ? prev - 1 : 0));
        }, 1000);
        return () => clearInterval(timer);
    }, [resendSeconds]);

    const validatePhone = () => {
        if (!phoneNumber.trim()) {
            setPhoneError("شماره موبایل را وارد کنید");
            return false;
        }
        if (!/^09\d{9}$/.test(phoneNumber.trim())) {
            setPhoneError("شماره موبایل باید 11 رقم و با 09 شروع شود");
            return false;
        }
        setPhoneError("");
        return true;
    };

    // ---------------------------------
    // ارسال OTP
    // ---------------------------------
    const handleSendOtp = async () => {
        if (!validatePhone()) return;

        if (resendSeconds > 0 && phoneNumber === lastPhoneNumber) return;

        const result = await sendOtp(phoneNumber.trim());

        // ❗ اگر success=false → پیام سرور نمایش داده شود + مرحله بعد نرویم
        if (!result.ok) {
            setPhoneError(result.message); // پیام واقعی سرور
            return;
        }

        // موفق → مرحله OTP
        setPhoneError("");
        setStep("otp");
        setLastPhoneNumber(phoneNumber.trim());
        setResendSeconds(120);
        setOtpDigits(["", "", "", "", "", ""]);
        setOtpError("");

        setTimeout(() => {
            if (inputsRef.current[0]) inputsRef.current[0].focus();
        }, 0);
    };

    // ---------------------------------
    // ارسال مجدد کد
    // ---------------------------------
    const handleResendCode = async () => {
        if (!phoneNumber || resendSeconds > 0) return;

        const result = await sendOtp(phoneNumber.trim());

        if (!result.ok) {
            setPhoneError(result.message);
            return;
        }

        setResendSeconds(120);
        setOtpDigits(["", "", "", "", "", ""]);
        setOtpError("");

        setTimeout(() => {
            if (inputsRef.current[0]) inputsRef.current[0].focus();
        }, 0);
    };

    // ---------------------------------
    // وارد کردن کد OTP
    // ---------------------------------
    const handleOtpChange = async (index, value) => {
        if (!/^\d?$/.test(value)) return;

        const updated = [...otpDigits];
        updated[index] = value;
        setOtpDigits(updated);

        setOtpError("");

        if (value && index < 5) {
            const next = inputsRef.current[index + 1];
            if (next) next.focus();
        }

        if (index === 5 && value) {
            const code = [...updated].join("");

            if (code.length === 6) {
                const result = await loginWithOtp(phoneNumber.trim(), code);

                // ❗ اگر success=false → پیام سرور نمایش داده شود + مرحله OTP بماند
                if (!result.ok) {
                    setOtpError(result.message); // پیام واقعی سرور
                    setOtpDigits(["", "", "", "", "", ""]);
                    setTimeout(() => {
                        if (inputsRef.current[0]) inputsRef.current[0].focus();
                    }, 0);
                    return;
                }

                // موفق → ورود + ریدایرکت
                navigate("/", { replace: true });
            }
        }
    };

    const handleOtpKeyDown = (index, e) => {
        if (e.key === "Backspace") {
            if (otpDigits[index] !== "") {
                const updated = [...otpDigits];
                updated[index] = "";
                setOtpDigits(updated);
            } else if (index > 0) {
                const prev = inputsRef.current[index - 1];
                if (prev) prev.focus();

                const updated = [...otpDigits];
                updated[index - 1] = "";
                setOtpDigits(updated);
            }
        }

        if (e.key === "ArrowLeft" && index > 0) {
            const prev = inputsRef.current[index - 1];
            if (prev) prev.focus();
        }

        if (e.key === "ArrowRight" && index < 5) {
            const next = inputsRef.current[index + 1];
            if (next) next.focus();
        }
    };

    const isSameNumberBlocked =
        resendSeconds > 0 && phoneNumber === lastPhoneNumber;

    return (
        <div className="login-wrapper">
            <Card className="login-card" bordered={false}>
                <Space direction="vertical" style={{ width: "100%" }} size={12}>
                    <div className="login-header">
                        <Title level={3} className="login-title">سامانه معاملات نگین گوهر</Title>
                        <Text className="login-subtitle">لطفاً شماره موبایل و کد یک‌بارمصرف را وارد کنید.</Text>
                    </div>

                    {/* PHONE STEP */}
                    {step === "phone" && (
                        <>
                            <Form layout="vertical" autoComplete="off">
                                <Form.Item label="شماره موبایل">
                                    <Input
                                        className="login-input"
                                        value={phoneNumber}
                                        onChange={(e) => {
                                            setPhoneNumber(e.target.value);
                                            if (phoneError) validatePhone();
                                        }}
                                        placeholder="09xxxxxxxxx"
                                        onKeyDown={(e) => {
                                            if (e.key === "Enter") {
                                                e.preventDefault();
                                                handleSendOtp();
                                            }
                                        }}
                                    />
                                </Form.Item>

                                {phoneError && (
                                    <div className="login-error-box">{phoneError}</div>
                                )}

                                <Button
                                    type="primary"
                                    block
                                    size="large"
                                    loading={isLoading}
                                    disabled={isLoading || isSameNumberBlocked}
                                    className="login-btn"
                                    onClick={handleSendOtp}
                                >
                                    {isSameNumberBlocked
                                        ? `ارسال مجدد (${resendSeconds})`
                                        : "ارسال کد ورود"}
                                </Button>
                            </Form>
                        </>
                    )}

                    {/* OTP STEP */}
                    {step === "otp" && (
                        <Form layout="vertical">
                            <Form.Item label="کد ارسال‌شده">
                                <div className="otp-input-group" dir="ltr">
                                    {otpDigits.map((digit, index) => (
                                        <input
                                            key={index}
                                            type="text"
                                            inputMode="numeric"
                                            maxLength={1}
                                            className="otp-input"
                                            value={digit}
                                            onChange={(e) => handleOtpChange(index, e.target.value)}
                                            onKeyDown={(e) => handleOtpKeyDown(index, e)}
                                            ref={(el) => (inputsRef.current[index] = el)}
                                        />
                                    ))}
                                </div>
                            </Form.Item>

                            {otpError && (
                                <div className="login-error-box">{otpError}</div>
                            )}

                            <Button
                                type="primary"
                                block
                                size="large"
                                loading={isLoading}
                                className="login-btn"
                                onClick={() => handleOtpChange(5, otpDigits[5] || "")}
                            >
                                ورود
                            </Button>

                            <div className="login-links">
                                <Button
                                    type="link"
                                    onClick={() => {
                                        setStep("phone");
                                        setOtpDigits(["", "", "", "", "", ""]);
                                        setOtpError("");
                                    }}
                                >
                                    اصلاح شماره موبایل
                                </Button>

                                <Button
                                    type="link"
                                    disabled={isLoading || resendSeconds > 0}
                                    onClick={handleResendCode}
                                >
                                    {resendSeconds > 0
                                        ? `ارسال مجدد (${resendSeconds})`
                                        : "ارسال مجدد کد"}
                                </Button>
                            </div>
                        </Form>
                    )}
                </Space>
            </Card>
        </div>
    );
};

export default LoginPage;
