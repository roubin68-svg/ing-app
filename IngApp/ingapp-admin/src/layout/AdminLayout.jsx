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
    WalletOutlined,
    CrownOutlined,
} from "@ant-design/icons";
import { Outlet, useNavigate, useLocation } from "react-router-dom";

import Sidebar from "./Sidebar";
import { useAuth } from "../core/auth/useAuth";
import { getMeApi } from "../features/auth/api/authApi";

import suppliersApi from "../features/suppliers/api/suppliersApi";
import supplierOnboardingApi from "../features/suppliers/api/supplierOnboardingApi";
import walletApi from "../features/wallet/api/walletApi";

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
    const [walletBalance, setWalletBalance] = useState(null);
    const [loadingWallet, setLoadingWallet] = useState(false);

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

    // ---------- بارگذاری موجودی کیف پول ----------
    const loadWalletBalance = async () => {
        try {
            setLoadingWallet(true);
            const result = await walletApi.getBalance();
            setWalletBalance(result);
        } catch (error) {
            // Silent fail - اگر خطا داشت، موجودی null می‌ماند
            console.error("Error loading wallet balance:", error);
        } finally {
            setLoadingWallet(false);
        }
    };

    useEffect(() => {
        loadWalletBalance();

        // گوش دادن به event برای به‌روزرسانی موجودی بعد از تراکنش‌های مالی
        const handleWalletBalanceChanged = () => {
            loadWalletBalance();
        };

        window.addEventListener('walletBalanceChanged', handleWalletBalanceChanged);

        return () => {
            window.removeEventListener('walletBalanceChanged', handleWalletBalanceChanged);
        };
    }, []);

    // ---------- فرمت موجودی ----------
    const formatPrice = (rial) => {
        if (rial == null) return "۰ تومان";
        const toman = rial / 10;
        return `${toman.toLocaleString("fa-IR")} تومان`;
    };

    // ---------- منوی آواتار بالا ----------
    const userMenu = {
        items: [
            // موجودی کیف پول (Header)
            {
                key: "wallet-balance",
                label: (
                    <div style={{ 
                        padding: "10px 12px", 
                        background: "#f0f7ff",
                        borderRadius: "4px",
                        margin: "0 -4px"
                    }}>
                        <div style={{ 
                            display: "flex", 
                            alignItems: "center", 
                            gap: "6px", 
                            marginBottom: "4px" 
                        }}>
                            <WalletOutlined style={{ color: "#1890ff", fontSize: "14px" }} />
                            <span style={{ fontSize: "12px", color: "#666" }}>موجودی کیف پول</span>
                        </div>
                        {loadingWallet ? (
                            <Spin size="small" />
                        ) : (
                            <div style={{ 
                                fontSize: "16px", 
                                fontWeight: "bold", 
                                color: "#1890ff",
                                marginTop: "2px"
                            }}>
                                {formatPrice(walletBalance?.balanceRial)}
                            </div>
                        )}
                    </div>
                ),
                disabled: true,
            },
            {
                type: "divider",
                style: { margin: "4px 0" },
            },
            {
                key: "profile",
                label: "پروفایل",
                icon: <UserOutlined />,
            },
            {
                key: "wallet",
                label: "کیف پول",
                icon: <WalletOutlined />,
            },
            {
                key: "subscriptions",
                label: "اشتراک‌ها",
                icon: <CrownOutlined />,
            },           
           
            {
                type: "divider",
                style: { margin: "4px 0" },
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
            } else if (key === "wallet") {
                navigate("/wallet");
            } else if (key === "subscriptions") {
                navigate("/subscriptions");
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
