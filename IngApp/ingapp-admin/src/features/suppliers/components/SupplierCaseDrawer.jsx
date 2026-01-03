import React, { useEffect, useState } from "react";
import { Drawer, Tabs, Spin, Descriptions, Tag, message, Button, Space, Modal, Input } from "antd";
import suppliersApi from "../api/suppliersApi";
import SupplierDocumentsTab from "./SupplierDocumentsTab";
import SupplierHistoryTab from "./SupplierHistoryTab";
import {
    CheckOutlined,
    CloseOutlined,
    EyeOutlined,
} from "@ant-design/icons";
import jalaali from "jalaali-js";

// تبدیل تاریخ میلادی به شمسی
const toShamsi = (gregorian) => {
    if (!gregorian) return null;
    
    if (typeof gregorian === "string") {
        const [y, m, d] = gregorian.split("T")[0].split("-").map(Number);
        const j = jalaali.toJalaali(y, m, d);
        return `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
    }
    
    if (gregorian instanceof Date) {
        const j = jalaali.toJalaali(
            gregorian.getFullYear(),
            gregorian.getMonth() + 1,
            gregorian.getDate()
        );
        return `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
    }
    
    return null;
};

const SupplierCaseDrawer = ({ open, onClose, supplierId, onStatusChanged }) => {
    const [loading, setLoading] = useState(false);
    const [detail, setDetail] = useState(null);
    const [rejectModalOpen, setRejectModalOpen] = useState(false);
    const [rejectReason, setRejectReason] = useState("");

    const approveSupplier = async () => {
        try {
            await suppliersApi.updateVerificationStatus(supplierId, {
                status: 2, // Approved
                note: null,
            });
            await onStatusChanged?.();
            message.success("تأمین‌کننده تأیید شد");            
            onClose();
        } catch (err) {
            message.error(
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در تأیید تأمین‌کننده"
            );
        }
    };

    const submitRejectSupplier = async () => {
        if (!rejectReason.trim()) {
            message.warning("وارد کردن دلیل رد الزامی است");
            return;
        }

        try {
            await suppliersApi.updateVerificationStatus(supplierId, {
                status: 3, // Rejected
                note: rejectReason,
            });
            await onStatusChanged?.();
            message.success("تأمین‌کننده رد شد");
            setRejectModalOpen(false);
            
            onClose();
        } catch (err) {
            message.error(
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در رد تأمین‌کننده"
            );
        }
    };

    const getStatusLabel = (status) => {
        switch (status) {
            case "Approved":
            case 2:
                return { text: "تأیید شده", color: "green" };

            case "Rejected":
            case 3:
                return { text: "رد شده", color: "red" };

            case "Pending":
            case 1:
                return { text: "در حال بررسی", color: "orange" };

            default:
                return { text: "ثبت نشده", color: "default" };
        }
    };


    // ----------------------------
    // Load supplier detail
    // ----------------------------
    const loadDetail = async () => {
        if (!supplierId) return;

        setLoading(true);
        try {
            const res = await suppliersApi.getById(supplierId);
            setDetail(res);
        } catch (err) {
            message.error(
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در دریافت اطلاعات تأمین‌کننده"
            );
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (open) {
            loadDetail();
        } else {
            setDetail(null);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [open, supplierId]);

    return (
        <Drawer
            open={open}
            onClose={onClose}
            width={900}
            destroyOnClose
            title="پرونده تأمین‌کننده"
        >
            {loading ? (
                <Spin />
            ) : !detail ? null : (
                <Tabs
                    defaultActiveKey="profile"
                    items={[
                        {
                            key: "profile",
                            label: "پروفایل",
                            children: (() => {
                                const statusInfo = getStatusLabel(detail.verificationStatus);

                                return (
                                    <Descriptions
                                        bordered
                                        size="small"
                                        column={2}
                                        style={{ marginBottom: 24 }}
                                    >
                                        <Descriptions.Item label="وضعیت" span={2}>
                                            <Tag color={statusInfo.color}>
                                                {statusInfo.text}
                                            </Tag>
                                        </Descriptions.Item>
                                        <Descriptions.Item label="تاریخ ایجاد" span={1}>
                                            {detail.createdAt ? toShamsi(detail.createdAt) : "-"}
                                        </Descriptions.Item>
                                        {detail.updatedAt ? (
                                            <Descriptions.Item label="آخرین به‌روزرسانی" span={1}>
                                                {toShamsi(detail.updatedAt)}
                                            </Descriptions.Item>
                                        ) : (
                                            <Descriptions.Item label="آخرین به‌روزرسانی" span={1}>
                                                "-"
                                            </Descriptions.Item>
                                        )}
                                        {detail.rejectionReason && (
                                            <Descriptions.Item label="دلیل رد" span={2}>
                                                {detail.rejectionReason}
                                            </Descriptions.Item>
                                        )}

                                        <Descriptions.Item label="نوع تأمین‌کننده" span={2}>
                                            {detail.supplierTypeName}
                                        </Descriptions.Item>

                                        <Descriptions.Item label="نام کسب‌وکار" span={2}>
                                            {detail.businessName}
                                        </Descriptions.Item>

                                        <Descriptions.Item label="شماره موبایل (Login)" span={2}>
                                            {detail.userPhoneNumber}
                                        </Descriptions.Item>

                                        <Descriptions.Item label="شماره تماس">
                                            {detail.contactPhone}
                                        </Descriptions.Item>

                                        <Descriptions.Item label="کد ملی">
                                            {detail.nationalId || "-"}
                                        </Descriptions.Item>

                                        <Descriptions.Item label="شماره مجوز">
                                            {detail.licenseNumber || "-"}
                                        </Descriptions.Item>

                                        <Descriptions.Item label="نام رابط">
                                            {detail.contactName || "-"}
                                        </Descriptions.Item>

                                        <Descriptions.Item label="استان">
                                            {detail.province}
                                        </Descriptions.Item>

                                        <Descriptions.Item label="شهر">
                                            {detail.city}
                                        </Descriptions.Item>

                                        <Descriptions.Item label="آدرس" span={2}>
                                            {detail.address || "-"}
                                        </Descriptions.Item>
                                    </Descriptions>
                                );
                            })(),
                        },
                        {
                            key: "documents",
                            label: "مدارک",
                            children: (
                                <SupplierDocumentsTab
                                    supplierUserId={detail.userId}
                                />
                            ),
                        },
                        {
                            key: "history",
                            label: "تاریخچه",
                            children: (
                                <SupplierHistoryTab supplierId={supplierId} />
                            ),
                        },
                    ]}
                    />
            )}
            {/* Footer Actions */}
            <div
                style={{
                    position: "sticky",
                    bottom: "-24px",
                    background: "#fff",
                    padding: "12px 16px",
                    borderTop: "1px solid #f0f0f0",
                    textAlign: "left",
                    zIndex: 1,
                }}
            >
                <Space>
                    <Button
                        danger
                        icon={<CloseOutlined />}
                        onClick={() => {
                            setRejectReason("");
                            setRejectModalOpen(true);
                        }}
                    >
                        رد تأمین‌کننده
                    </Button>

                    <Button                        
                        icon={<CheckOutlined />}
                        onClick={approveSupplier}
                    >
                        تأیید تأمین‌کننده
                    </Button>
                </Space>
            </div>
            <Modal
                title="رد تأمین‌کننده"
                open={rejectModalOpen}
                onOk={submitRejectSupplier}
                onCancel={() => setRejectModalOpen(false)}
                okText="ثبت"
                cancelText="انصراف"
            >
                <Input.TextArea
                    rows={4}
                    placeholder="دلیل رد تأمین‌کننده را وارد کنید"
                    value={rejectReason}
                    onChange={(e) => setRejectReason(e.target.value)}
                />
            </Modal>
        </Drawer>


    );

};

export default SupplierCaseDrawer;
