// src/app/AppProviders.jsx
import React from "react";
import { BrowserRouter } from "react-router-dom";
import { ConfigProvider, App as AntApp } from "antd";
import faIR from "antd/locale/fa_IR";
import { AuthProvider } from "../core/auth/AuthContext";

const AppProviders = ({ children }) => {
    return (
        <ConfigProvider direction="rtl" locale={faIR}>
            <AntApp>
                <AuthProvider>
                    <BrowserRouter>
                        {children}
                    </BrowserRouter>
                </AuthProvider>
            </AntApp>
        </ConfigProvider>
    );
};

export default AppProviders;
