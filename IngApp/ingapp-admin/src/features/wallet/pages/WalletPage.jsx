// src/features/wallet/pages/WalletPage.jsx
import React, { useEffect, useState } from "react";
import { Card, Spin, Descriptions, Table, Tag, Space, Button, message, Typography } from "antd";
import { WalletOutlined, ReloadOutlined, PlusOutlined } from "@ant-design/icons";
import walletApi from "../api/walletApi";
import { useNavigate } from "react-router-dom";
import jalaali from "jalaali-js";

const { Text } = Typography;

const WalletPage = () => {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(true);
    const [loadingBalance, setLoadingBalance] = useState(true);
    const [balance, setBalance] = useState(null);
    const [transactions, setTransactions] = useState([]);
    const [pagination, setPagination] = useState({
        current: 1,
        pageSize: 20,
        total: 0,
    });

    const loadBalance = async () => {
        try {
            setLoadingBalance(true);
            const result = await walletApi.getBalance();
            // apiClient interceptor unwraps ApiResult, so result is: { walletId, balanceRial }
            setBalance(result);
        } catch (error) {
            message.error("خطا در دریافت موجودی کیف پول");
            console.error(error);
        } finally {
            setLoadingBalance(false);
        }
    };

    const loadTransactions = async (page = 1, pageSize = 20) => {
        try {
            setLoading(true);
            const result = await walletApi.getTransactions({ page, pageSize });
            // apiClient interceptor unwraps ApiResult, so result is: { items, page, pageSize, totalCount }
            setTransactions(result?.items || []);
            setPagination({
                current: result?.page || page,
                pageSize: result?.pageSize || pageSize,
                total: result?.totalCount || 0,
            });
        } catch (error) {
            message.error("خطا در دریافت تراکنش‌ها");
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadBalance();
        loadTransactions();
    }, []);

    const formatPrice = (rial) => {
        if (rial == null) return "-";
        const toman = rial / 10;
        return `${toman.toLocaleString("fa-IR")} تومان`;
    };

    const getDirectionColor = (code) => {
        if (code === "Credit") return "green";
        if (code === "Debit") return "red";
        return "default";
    };

    const getDirectionText = (code) => {
        if (code === "Credit") return "واریز";
        if (code === "Debit") return "برداشت";
        return code;
    };

    // تبدیل تاریخ میلادی به شمسی با ساعت و دقیقه
    const toShamsiWithTime = (gregorian) => {
        if (!gregorian) return { date: "-", time: "" };
        
        let dateObj;
        if (typeof gregorian === "string") {
            dateObj = new Date(gregorian);
        } else if (gregorian instanceof Date) {
            dateObj = gregorian;
        } else {
            return { date: "-", time: "" };
        }
        
        const year = dateObj.getFullYear();
        const month = dateObj.getMonth() + 1;
        const day = dateObj.getDate();
        const hour = dateObj.getHours();
        const minute = dateObj.getMinutes();
        
        const j = jalaali.toJalaali(year, month, day);
        const shamsiDate = `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
        const time = `${String(hour).padStart(2, "0")}:${String(minute).padStart(2, "0")}`;
        
        return { date: shamsiDate, time };
    };

    const columns = [
        {
            title: "تاریخ",
            dataIndex: "createdAt",
            key: "createdAt",
            render: (date) => {
                const { date: shamsiDate, time } = toShamsiWithTime(date);
                return (
                    <div style={{ display: "flex", flexDirection: "column" }}>
                        <span>{shamsiDate}</span>
                        {time && <span style={{ fontSize: "12px", color: "#999" }}>{time}</span>}
                    </div>
                );
            },
        },
        {
            title: "نوع",
            dataIndex: "directionCode",
            key: "directionCode",
            render: (code) => (
                <Tag color={getDirectionColor(code)}>{getDirectionText(code)}</Tag>
            ),
        },
        {
            title: "مبلغ",
            dataIndex: "amountRial",
            key: "amountRial",
            render: (amount) => formatPrice(amount),
        },
        {
            title: "نوع عملیات",
            dataIndex: "operationTypeTitle",
            key: "operationTypeTitle",
        },
        {
            title: "وضعیت",
            dataIndex: "statusTitle",
            key: "statusTitle",
            render: (status) => (
                <Tag color={status === "تایید شده" ? "green" : "orange"}>{status}</Tag>
            ),
        },
        {
            title: "توضیحات",
            dataIndex: "description",
            key: "description"
        },
    ];

    return (
        
            <Space direction="vertical" size="large" style={{ width: "100%" }}>
                {/* موجودی کیف پول */}
                <Card
                    title={
                        <Space>
                            <span>موجودی کیف پول</span>
                        </Space>
                    }
                    extra={
                        <Space>
                            <Button
                                icon={<ReloadOutlined />}
                                onClick={() => {
                                    loadBalance();
                                    loadTransactions();
                                }}
                            >
                                به‌روزرسانی
                            </Button>
                            <Button
                                type="primary"
                                icon={<PlusOutlined />}
                                onClick={() => navigate("/payments/topup")}
                            >
                                شارژ کیف پول
                            </Button>
                        </Space>
                    }
                >
                    {loadingBalance ? (
                        <div style={{ textAlign: "center", padding: "20px" }}>
                            <Spin />
                        </div>
                    ) : balance ? (
                        <Descriptions bordered column={2}>
                            <Descriptions.Item label="موجودی">
                                <Space>
                                    <Text strong style={{ fontSize: "18px", color: "#1890ff" }}>
                                        {formatPrice(balance.balanceRial)}
                                    </Text>
                                </Space>
                            </Descriptions.Item>
                            <Descriptions.Item label="شناسه کیف پول">
                                <Text type="secondary">{balance.walletId}</Text>
                            </Descriptions.Item>
                        </Descriptions>
                    ) : (
                        <div>خطا در دریافت موجودی</div>
                    )}
                </Card>

                {/* تراکنش‌ها */}
                <Card title="تراکنش‌های کیف پول">
                    <Table
                        columns={columns}
                        dataSource={transactions}
                        loading={loading}
                        rowKey="id"
                        pagination={{
                            ...pagination,
                            onChange: (page, pageSize) => {
                                loadTransactions(page, pageSize);
                            },
                        }}
                    />
                </Card>
            </Space>
        
    );
};

export default WalletPage;

