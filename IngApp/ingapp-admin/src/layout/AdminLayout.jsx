// src/layout/AdminLayout.jsx
import React, { useEffect, useState } from "react";
import { Layout, Button, Avatar, Dropdown, Space, Badge, Spin, Typography, List, Drawer } from "antd";
import {
    MenuFoldOutlined,
    MenuUnfoldOutlined,
    BellOutlined,
    UserOutlined,
    LogoutOutlined,
    SolutionOutlined,
} from "@ant-design/icons";
import { Outlet, useNavigate, useLocation } from "react-router-dom";

import Sidebar from "./Sidebar";
import { useAuth } from "../core/auth/useAuth";
import { getMeApi } from "../features/auth/api/authApi";

import suppliersApi from "../features/suppliers/api/suppliersApi";
import supplierOnboardingApi from "../features/suppliers/api/supplierOnboardingApi";

// Hook برای تشخیص موبایل
const useIsMobile = () => {
    const [isMobile, setIsMobile] = useState(false);

    useEffect(() => {
        const checkIsMobile = () => {
            setIsMobile(window.innerWidth < 992); // lg breakpoint
        };

        checkIsMobile();
        window.addEventListener('resize', checkIsMobile);
        return () => window.removeEventListener('resize', checkIsMobile);
    }, []);

    return isMobile;
};

const { Header, Sider, Content } = Layout;

const AdminLayout = ({ children }) => {
    const isMobile = useIsMobile();
    const [collapsed, setCollapsed] = useState(false);
    const [drawerVisible, setDrawerVisible] = useState(false);

    const [userInfo, setUserInfo] = useState(null);
    const [loadingUser, setLoadingUser] = useState(true);

    const navigate = useNavigate();
    const location = useLocation();
    const { logout } = useAuth();

    const [pendingCount, setPendingCount] = useState(0);
    const [notifications, setNotifications] = useState([]);
    const [supplierStatus, setSupplierStatus] = useState(null);

    // در موبایل همیشه collapsed باشد
    useEffect(() => {
        if (isMobile) {
            setCollapsed(true);
        }
    }, [isMobile]);



    // ---------- گرفتن اطلاعات کاربر از /auth/me ----------
    useEffect(() => {
        let isMounted = true;

        const loadUserInfo = async () => {
            try {
                const response = await getMeApi();
                if (!isMounted) return;

                setUserInfo(response.data);
            } catch (err) {
                if (!isMounted) return;

                // اگر خطا داشتیم، حالت مهمان نمایش داده می‌شود
                setUserInfo({
                    displayName: "مهمان",
                    roles: [],
                });
            } finally {
                if (isMounted) {
                    setLoadingUser(false);
                }
            }
        };

        loadUserInfo();

        return () => {
            isMounted = false;
        };
    }, []);

    useEffect(() => {
        if (!userInfo?.permissions?.includes("Supplier.Manage")) {
            setNotifications([]);
            return;
        }

        let timer;

        const loadNotifications = async () => {
            try {
                const count = await suppliersApi.getPendingCount();

                if (count > 0) {
                    setNotifications([
                        {
                            key: "supplier-pending",
                            text: `${count} درخواست تأمین‌کننده در حال بررسی وجود دارد`,
                            onClick: () =>
                                navigate("/suppliers", {
                                    state: { defaultVerificationStatus: "Pending" },
                                }),
                        },
                    ]);
                } else {
                    setNotifications([]);
                }
            } catch (e) {
                // silent
            }
        };

        loadNotifications();
        timer = setInterval(loadNotifications, 30000);

        return () => clearInterval(timer);
    }, [userInfo, navigate]);

    // ---------- بررسی وضعیت درخواست همکاری ----------
    useEffect(() => {
        const loadSupplierStatus = async () => {
            try {
                const res = await supplierOnboardingApi.getMyProfile();
                if (res) {
                    setSupplierStatus({
                        status: res.verificationStatus, // NotSubmitted | Pending | Approved | Rejected
                    });
                } else {
                    setSupplierStatus(null);
                }
            } catch {
                setSupplierStatus(null);
            }
        };

        loadSupplierStatus();
    }, []);



    // ---------- لاگ‌اوت ----------
    const handleLogout = () => {
        logout(); // از AuthContext توکن و اطلاعات پاک می‌شود
        navigate("/login", { replace: true, state: { from: location.pathname } });
    };

    // ---------- متن نمایش نقش‌ها و نام ----------
    const userRolesText =
        userInfo?.roles && userInfo.roles.length > 0
            ? `(${userInfo.roles.join(" - ")})`
            : "(بدون نقش)";

    const userNameText = userInfo?.displayName || "مهمان";

    // ---------- منوی آواتار بالا ----------
    const userMenu = {
        items: [
            {
                key: "profile",
                label: "پروفایل",
            },
            {
                type: "divider",
            },
            {
                key: "logout",
                label: "خروج",
                icon: <LogoutOutlined />,
                danger: true,
            },
        ],
        onClick: ({ key }) => {
            if (key === "profile") {
                navigate("/profile");
            } else if (key === "logout") {
                handleLogout();
            }
        },
    };

    const notificationMenu = {
        items:
            notifications.length > 0
                ? notifications.map((n) => ({
                    key: n.key,
                    label: (
                        <span
                            onClick={n.onClick}
                            style={{ cursor: "pointer" }}
                        >
                            {n.text}
                        </span>
                    ),
                }))
                : [
                    {
                        key: "empty",
                        label: (
                            <Typography.Text type="secondary">
                                اعلانی برای نمایش وجود ندارد
                            </Typography.Text>
                        ),
                        disabled: true,
                    },
                ],
    };


    return (
        <Layout className="admin-layout">
            {/* ---------- SIDEBAR (Desktop) ---------- */}
            <Sider
                trigger={null}
                collapsible
                collapsed={collapsed}
                width={260}
                collapsedWidth={80}
                className="admin-sider"
                breakpoint="lg"
                onBreakpoint={(broken) => {
                    if (broken) {
                        setCollapsed(true);
                    }
                }}
                style={{
                    display: isMobile ? 'none' : 'block'
                }}
            >
                {/* ---------- LOGO AREA ---------- */}
                <div className={`admin-logo ${collapsed ? "admin-logo--collapsed" : ""}`}>
                    {collapsed ? (
                        <div className="admin-logo-circle">نگین</div>
                    ) : (
                        <span className="admin-logo-text">سامانه معاملات نگین گوهر</span>
                    )}
                </div>

                <Sidebar collapsed={collapsed} onItemClick={() => {}} />
            </Sider>

            {/* ---------- DRAWER (Mobile) ---------- */}
            <Drawer
                title={
                    <div className="admin-logo" style={{ padding: 0, margin: 0, borderBottom: '1px solid rgba(148, 163, 184, 0.25)' }}>
                        <span className="admin-logo-text">سامانه معاملات نگین گوهر</span>
                    </div>
                }
                placement="right"
                onClose={() => setDrawerVisible(false)}
                open={drawerVisible}
                width={280}
                headerStyle={{
                    background: '#020617',
                    borderBottom: '1px solid rgba(148, 163, 184, 0.25)',
                    padding: '0',
                }}
                bodyStyle={{
                    padding: 0,
                    background: '#020617',
                    color: '#e5e7eb',
                }}
                styles={{
                    body: {
                        background: '#020617',
                        color: '#e5e7eb',
                    }
                }}
            >
                <Sidebar 
                    collapsed={false} 
                    onItemClick={() => {
                        setDrawerVisible(false);
                    }} 
                />
            </Drawer>

            {/* ---------- MAIN AREA ---------- */}
            <Layout className="admin-main-layout">
                <Header className="admin-header">
                    {/* سمت راست (در RTL): دکمه منو */}
                    <div className="admin-header-left">
                        <Button
                            type="text"
                            onClick={() => {
                                if (isMobile) {
                                    setDrawerVisible(true);
                                } else {
                                    setCollapsed(!collapsed);
                                }
                            }}
                            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
                            className="admin-trigger"
                        />
                    </div>

                    {/* سمت چپ (در RTL): درخواست همکاری + نوتیف + اطلاعات کاربر + منوی پروفایل */}
                    <div className="admin-header-right">
                        {/* دکمه درخواست همکاری (فقط اگر هنوز درخواست نداده) */}
                        {(!supplierStatus || supplierStatus.status === "NotSubmitted") && (
                            <Button
                                type="default"
                                size="small"
                                icon={<SolutionOutlined />}
                                onClick={() => navigate("/supplier-onboarding")}
                                style={{ padding: "16px 16px" }}
                            >
                                درخواست همکاری به عنوان تأمین‌کننده
                            </Button>
                        )}

                        {userInfo?.permissions?.includes("Supplier.Manage") && (
                            <Dropdown
                                menu={notificationMenu}
                                trigger={["click"]}
                                placement="bottomLeft"
                            >
                                <Badge count={notifications.length} size="small">
                                    <BellOutlined
                                        style={{ fontSize: 20, cursor: "pointer" }}
                                    />
                                </Badge>
                            </Dropdown>
                        )}



                        <Dropdown menu={userMenu} trigger={["click"]} placement="bottomLeft">
                            <div className="admin-user-area admin-user-area--clickable">
                                <Avatar
                                    size={40}
                                    icon={<UserOutlined />}
                                    className="admin-user-avatar"
                                />
                                <div className="admin-user-meta">
                                    <div className="admin-user-name">
                                        {loadingUser ? <Spin size="small" /> : userNameText}
                                    </div>
                                    <div className="admin-user-roles">
                                        {loadingUser ? null : userRolesText}
                                    </div>
                                </div>
                            </div>
                        </Dropdown>
                    </div>
                </Header>

                <Content className="admin-content">
                    {children || <Outlet />}
                </Content>
            </Layout>
        </Layout>
    );
};

export default AdminLayout;
