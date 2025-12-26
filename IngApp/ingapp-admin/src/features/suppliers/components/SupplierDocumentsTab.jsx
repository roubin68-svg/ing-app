import React, { useEffect, useState } from "react";
import {
    Table,
    Tag,
    Button,
    Space,
    Modal,
    Input,
    message,
} from "antd";
import {
    CheckOutlined,
    CloseOutlined,
    EyeOutlined,
} from "@ant-design/icons";
import suppliersApi from "../api/suppliersApi";
import apiClient from "../../../core/api/apiClient";

const { TextArea } = Input;

const SupplierDocumentsTab = ({ supplierUserId }) => {
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState([]);

    const [rejectModalOpen, setRejectModalOpen] = useState(false);
    const [rejectNote, setRejectNote] = useState("");
    const [selectedDocId, setSelectedDocId] = useState(null);

    // ----------------------------
    // Load documents
    // ----------------------------
    const loadDocuments = async () => {
        if (!supplierUserId) return;

        setLoading(true);
        try {
            const res = await suppliersApi.getDocuments({
                userId: supplierUserId,
                page: 1,
                pageSize: 50,
            });

            setData(res.items || []);
        } catch (err) {
            message.error(
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در دریافت مدارک"
            );
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadDocuments();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [supplierUserId]);

    // ----------------------------
    // Actions
    // ----------------------------
    const approveDocument = async (docId) => {
        try {
            await suppliersApi.reviewDocument(docId, {
                status: 1,
            });
            message.success("مدرک تأیید شد");
            loadDocuments();
        } catch (err) {
            message.error(
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در تأیید مدرک"
            );
        }
    };

    const openRejectModal = (docId) => {
        setSelectedDocId(docId);
        setRejectNote("");
        setRejectModalOpen(true);
    };

    const submitReject = async () => {
        if (!rejectNote.trim()) {
            message.warning("وارد کردن دلیل رد الزامی است");
            return;
        }

        try {
            await suppliersApi.reviewDocument(selectedDocId, {
                status: 2,
                adminNote: rejectNote,
            });
            message.success("مدرک رد شد");
            setRejectModalOpen(false);
            loadDocuments();
        } catch (err) {
            message.error(
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در رد مدرک"
            );
        }
    };

    // ----------------------------
    // Helpers
    // ----------------------------
    const renderStatus = (status) => {
        switch (status) {
            case "Approved":
            case 1:
                return <Tag color="green">تأیید شده</Tag>;
            case "Rejected":
            case 2:
                return <Tag color="red">رد شده</Tag>;
            default:
                return <Tag color="orange">در حال بررسی</Tag>;
        }
    };



    const downloadDocumentFile = async (documentId, filename) => {
        try {
            const res = await apiClient.get(
                `/kyc/documents/${documentId}/file`,
                {
                    responseType: "blob",
                }
            );

            const blob = new Blob([res.data]);
            const url = window.URL.createObjectURL(blob);

            const a = document.createElement("a");
            a.href = url;
            a.download = filename || "document";
            a.click();

            window.URL.revokeObjectURL(url);
        } catch (e) {
            message.error("خطا در دانلود فایل");
        }
    };



    // ----------------------------
    // Columns
    // ----------------------------
    const columns = [
        {
            title: "عنوان مدرک",
            dataIndex: "attributeDisplayName",
        },
        {
            title: "محتوا",
            render: (_, record) => {
                if (record.filePath) {
                    return (
                        <Button
                            icon={<EyeOutlined />}
                            onClick={() =>
                                downloadDocumentFile(
                                    record.id,
                                    record.value || "document"
                                )
                            }
                        >
                            دانلود فایل
                        </Button>
                    );
                }
                return record.value || "-";
            },
        },
        {
            title: "وضعیت",
            dataIndex: "status",
            render: renderStatus,
        },
        {
            title: "یادداشت ادمین",
            dataIndex: "adminNote",
            render: (note) => note || "-",
        },
        {
            title: "عملیات",
            render: (_, record) => (
                <Space>
                    <Button
                        icon={<CheckOutlined />}
                        onClick={() => approveDocument(record.id)}
                        disabled={record.status === 1}
                    >
                        تأیید
                    </Button>
                    <Button
                        danger
                        icon={<CloseOutlined />}
                        onClick={() => openRejectModal(record.id)}
                        disabled={record.status === 2}
                    >
                        رد
                    </Button>
                </Space>
            ),
        },
    ];

    return (
        <>
            <Table
                rowKey="id"
                loading={loading}
                dataSource={data}
                columns={columns}
                pagination={false}
                style={{ marginBottom: 24 }}
            />

            {/* Reject Modal */}
            <Modal
                title="رد مدرک"
                open={rejectModalOpen}
                onOk={submitReject}
                onCancel={() => setRejectModalOpen(false)}
                okText="ثبت"
                cancelText="انصراف"
            >
                <TextArea
                    rows={4}
                    placeholder="دلیل رد مدرک را وارد کنید"
                    value={rejectNote}
                    onChange={(e) => setRejectNote(e.target.value)}
                />
            </Modal>
        </>
    );
};

export default SupplierDocumentsTab;
