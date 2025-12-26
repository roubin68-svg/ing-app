import React, { useEffect, useState } from "react";
import { Table, Tag, message } from "antd";
import suppliersApi from "../api/suppliersApi";

const SupplierHistoryTab = ({ supplierId }) => {
    const [loading, setLoading] = useState(false);
    const [verificationHistory, setVerificationHistory] = useState([]);

    // ----------------------------
    // Load history
    // ----------------------------
    const loadHistory = async () => {
        if (!supplierId) return;

        setLoading(true);
        try {
            const [vh, al] = await Promise.all([
                suppliersApi.getVerificationHistory(supplierId),
                suppliersApi.getActivityLogs(supplierId),
            ]);

            setVerificationHistory(vh || []);
        } catch (err) {
            message.error(
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در دریافت تاریخچه"
            );
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadHistory();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [supplierId]);

    // ----------------------------
    // Helpers
    // ----------------------------
    const renderStatus = (status) => {
        switch (status) {
            case "Approved":
            case 2:
                return <Tag color="green">تأیید شده</Tag>;
            case "Rejected":
            case 3:
                return <Tag color="red">رد شده</Tag>;
            case "Pending":
            case 1:
                return <Tag color="orange">در حال بررسی</Tag>;
            default:
                return <Tag>-</Tag>;
        }
    };

    // ----------------------------
    // Columns
    // ----------------------------
    const verificationColumns = [
        {
            title: "از وضعیت",
            dataIndex: "oldStatus",
            render: renderStatus,
        },
        {
            title: "به وضعیت",
            dataIndex: "newStatus",
            render: renderStatus,
        },
        {
            title: "توسط",
            render: (_, r) => r.adminDisplayName || r.adminUserId || "-",
        },
        {
            title: "یادداشت",
            dataIndex: "note",
            render: (v) => v || "-",
        },
        {
            title: "تاریخ",
            dataIndex: "createdAt",
        },
    ];

   
    return (
        <>
            <h4>تاریخچه تغییر وضعیت</h4>
            <Table
                rowKey="id"
                loading={loading}
                dataSource={verificationHistory}
                columns={verificationColumns}
                pagination={false}
                style={{ marginBottom: 24 }}
            />           
        </>
    );
};

export default SupplierHistoryTab;
