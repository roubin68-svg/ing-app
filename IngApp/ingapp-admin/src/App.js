// src/App.js
import React from "react";

// 🌐 Theme & Global Styles
import "./theme/App.css";
import "./theme/font.css";
import "./theme/layout.css";

// 🧩 Ant Design RTL + Locale
import { ConfigProvider } from "antd";
import faIR from "antd/locale/fa_IR";

// 🔧 App Providers (Axios, AuthContext, etc.)
import AppProviders from "./app/AppProviders";

// 🔀 Router for all pages
import AppRouter from "./app/AppRouter";

function App() {
  return (
    <ConfigProvider
      direction="rtl"
      locale={faIR}
      theme={{
        token: {
          fontFamily: "Vazirmatn, sans-serif",
        },
      }}
    >
      <AppProviders>
        <AppRouter />
      </AppProviders>
    </ConfigProvider>
  );
}

export default App;
