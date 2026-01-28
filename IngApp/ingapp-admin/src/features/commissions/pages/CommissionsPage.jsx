// src/features/commissions/pages/CommissionsPage.jsx
import React, { useEffect, useState } from "react";
import { Card, Spin, Table, Descriptions, Tag, Space, message } from "antd";
import { DollarOutlined, ReloadOutlined } from "@ant-design/icons";
import commissionsApi from "../api/commissionsApi";
import dayjs from "dayjs";

const CommissionsPage = () => {
    const [loading, setLoading] = useState(true);
    const [commissions, setCommissions] = useState([]);
    const [totalCommission, setTotalCommission] = useState(null);
    const [pagination, setPagination] = useState({
        current: 1,
        pageSize: 20,
        total: 0,
    });

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            setLoading(true);
            const [commissionsRes, totalRes] = await Promise.all([
                commissionsApi.getMyCommissions({
                    page: pagination.current,
                    pageSize: pagination.pageSize,
                }),
                commissionsApi.getMyTotalCommission(),
            ]);
            setCommissions(commissionsRes || []);
            setTotalCommission(totalRes);
        } catch (error) {
            message.error("خطا در دریافت اطلاعات پورسانت");
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const formatPrice = (rial) => {
        if (rial == null) return "-";
        const toman = rial / 10;
        return `${toman.toLocaleString("fa-IR")} تومان`;
    };

    const getCommissionTypeText = (type) => {
        if (type === "UnlockContactCommission") return "باز کردن اطلاعات تماس";
        if (type === "SubscriptionCommission") return "خرید اشتراک";
        return type;
    };

    const columns = [
        {
            title: "تاریخ",
            dataIndex: "createdAt",
            key: "createdAt",
            render: (date) => dayjs(date).format("YYYY/MM/DD HH:mm"),
        },
        {
            title: "نوع پورسانت",
            dataIndex: "commissionType",
            key: "commissionType",
            render: (type) => getCommissionTypeText(type),
        },
        {
            title: "خریدار",
            dataIndex: "buyerDisplayName",
            key: "buyerDisplayName",
            render: (name) => name || "-",
        },
        {
            title: "مبلغ اصلی",
            dataIndex: "originalAmountRial",
            key: "originalAmountRial",
            render: (amount) => formatPrice(amount),
            align: "left",
        },
        {
            title: "درصد پورسانت",
            dataIndex: "commissionPercentage",
            key: "commissionPercentage",
            render: (percent) => `${percent}%`,
        },
        {
            title: "مبلغ پورسانت",
            dataIndex: "commissionAmountRial",
            key: "commissionAmountRial",
            render: (amount) => (
                <span style={{ fontWeight: "bold", color: "#52c41a" }}>
                    {formatPrice(amount)}
                </span>
            ),
            align: "left",
        },
        {
            title: "توضیحات",
            dataIndex: "description",
            key: "description",
            ellipsis: true,
        },
    ];

    return (
        <div style={{ padding: "24px" }}>
            <Space direction="vertical" size="large" style={{ width: "100%" }}>
                {/* مجموع پورسانت */}
                <Card>
                    <Space direction="vertical" size="middle" style={{ width: "100%" }}>
                        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                            <h2 style={{ margin: 0 }}>
                                <DollarOutlined /> مجموع پورسانت‌ها
                            </h2>
                            <Space>
                                <button
                                    onClick={loadData}
                                    style={{
                                        border: "none",
                                        background: "none",
                                        cursor: "pointer",
                                        padding: "4px 8px",
                                    }}
                                >
                                    <ReloadOutlined />
                                </button>
                            </Space>
                        </div>
                        {totalCommission ? (
                            <Descriptions bordered column={2}>
                                <Descriptions.Item label="مجموع پورسانت‌ها">
                                    <span style={{ fontSize: "24px", fontWeight: "bold", color: "#52c41a" }}>
                                        {formatPrice(totalCommission.totalAmountRial)}
                                    </span>
                                </Descriptions.Item>
                                <Descriptions.Item label="معادل تومان">
                                    {totalCommission.totalAmountToman?.toLocaleString("fa-IR")} تومان
                                </Descriptions.Item>
                            </Descriptions>
                        ) : (
                            <Spin />
                        )}
                    </Space>
                </Card>

                {/* لیست پورسانت‌ها */}
                <Card title="تاریخچه پورسانت‌ها">
                    <Table
                        columns={columns}
                        dataSource={commissions}
                        loading={loading}
                        rowKey="id"
                        pagination={{
                            ...pagination,
                            onChange: (page, pageSize) => {
                                setPagination({ ...pagination, current: page, pageSize });
                                loadData();
                            },
                        }}
                    />
                </Card>
            </Space>
        </div>
    );
};

export default CommissionsPage;










