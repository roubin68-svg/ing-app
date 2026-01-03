import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Card, Divider, Spin, message, Space, TreeSelect, Button, Alert, App, Form, InputNumber, Switch, Input, Upload, Progress, Typography } from "antd";
import { DownloadOutlined, FileOutlined, FilePdfOutlined, FileWordOutlined, DeleteOutlined, CheckCircleOutlined } from "@ant-design/icons";
import offersApi from "../api/offersApi";
import { DatePicker as JalaliDatePicker } from "antd-jalali";
import dayjs from "dayjs";
import jalaali from "jalaali-js";

const TAX_RATE = 0.10; // فعلاً ۱۰٪ - بعداً می‌بریم config/global

const todayShamsi = () => {
    const now = new Date();

    const j = jalaali.toJalaali(
        now.getFullYear(),
        now.getMonth() + 1,
        now.getDate()
    );

    // 👈 dayjs شمسی واقعی
    return dayjs(`${j.jy}/${j.jm}/${j.jd}`, "YYYY/M/D");
};


const toShamsi = (gregorian) => {
    if (!gregorian) return null;

    // بدون timezone
    const [y, m, d] = gregorian.split("T")[0].split("-").map(Number);
    const j = jalaali.toJalaali(y, m, d);

    return dayjs(`${j.jy}/${j.jm}/${j.jd}`, "YYYY/M/D");
};



const toGregorianString = (shamsiDayjs) => {
    if (!shamsiDayjs) return null;

    const [jy, jm, jd] = shamsiDayjs.format("YYYY/M/D").split("/").map(Number);
    const g = jalaali.toGregorian(jy, jm, jd);

    // ⛔ Date نساز
    // ✅ string ISO بساز (backend-friendly)
    return `${g.gy}-${String(g.gm).padStart(2, "0")}-${String(g.gd).padStart(2, "0")}T00:00:00`;
};

// تبدیل date به dayjs شمسی (برای JalaliDatePicker onChange)
// وقتی روی "امروز" کلیک می‌شود، JalaliDatePicker ممکن است date میلادی برگرداند
const ensureShamsiDayjs = (date) => {
    if (!date) return null;
    
    // اگر dayjs object است
    if (dayjs.isDayjs(date)) {
        try {
            const year = date.year();
            
            // اگر سال بزرگتر از 2000 است، میلادی است (سال‌های شمسی 1300-1500 هستند)
            if (year > 2000) {
                // میلادی است - به شمسی تبدیل می‌کنیم
                const dateStr = date.format("YYYY-MM-DD");
                return toShamsi(dateStr);
            }
            
            // سال بین 1300-2000 است - احتمالاً شمسی است
            return date;
        } catch {
            // در صورت خطا، همان را برمی‌گردانیم
            return date;
        }
    }
    
    // اگر Date object است (همیشه میلادی)
    if (date instanceof Date) {
        const j = jalaali.toJalaali(
            date.getFullYear(),
            date.getMonth() + 1,
            date.getDate()
        );
        return dayjs(`${j.jy}/${j.jm}/${j.jd}`, "YYYY/M/D");
    }
    
    // اگر string است
    if (typeof date === "string") {
        return toShamsi(date) || todayShamsi();
    }
    
    return todayShamsi();
};



export default function OfferManagementPage() {
    const { modal } = App.useApp();
    const navigate = useNavigate();
    const { id: offerId } = useParams();

    const [loading, setLoading] = useState(true);
    const [offerDetail, setOfferDetail] = useState(null);
    const [wizardStep, setWizardStep] = useState(1); // backend source of truth

    // Step 1 - Select Product
    const [productTree, setProductTree] = useState([]);
    const [loadingProducts, setLoadingProducts] = useState(false);
    const [selectedProductId, setSelectedProductId] = useState(null);
    const [confirmedProductId, setConfirmedProductId] = useState(null);
    const [savingStep1, setSavingStep1] = useState(false);

    // Step 2 - Main Info (Header)
    const [headerForm] = Form.useForm();
    const [savingStep2, setSavingStep2] = useState(false);

    // Step 3
    const [attributeTemplates, setAttributeTemplates] = useState([]);
    const [documentsDraft, setDocumentsDraft] = useState({});
    const [attrLoading, setAttrLoading] = useState(false);
    const [docDraftByAttrId, setDocDraftByAttrId] = useState({});
    const [savingStep3, setSavingStep3] = useState(false);


    // Step 4
    const [submitting, setSubmitting] = useState(false);

    const formatPrice = (v) =>
        v != null
            ? v.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
            : "-";

    // Helper function to extract error message and Request ID
    const getErrorMessage = (error, defaultMessage) => {
        try {
            let errorMsg = defaultMessage;
            let requestId = null;

            // Extract Request ID from error object (set by apiClient interceptor)
            requestId = error?.requestId || null;

            // Extract message from error
            if (error?.message) {
                errorMsg = error.message;
            } else if (error?.response?.data) {
                const data = error.response.data;
                if (typeof data === "object") {
                    errorMsg = data.message || data.error || defaultMessage;
                    // Also check response data for Request ID if not already found
                    if (!requestId) {
                        requestId = data.requestId || data.request_id || data.requestID || null;
                    }
                } else if (typeof data === "string") {
                    errorMsg = data;
                }
            }

            // Log error for debugging
            console.error("Error:", error);
            if (requestId) {
                console.error("Request ID:", requestId);
            }

            // Append Request ID to message if available
            if (requestId) {
                return `${errorMsg} (Request ID: ${requestId})`;
            }

            return errorMsg;
        } catch {
            return defaultMessage;
        }
    };


    // ---------------------------
    // Stage header info (UX)
    // ---------------------------
    const stageInfo = useMemo(() => {
        switch (wizardStep) {
            case 1:
                return { step: 1, title: "انتخاب محصول" };
            case 2:
                return { step: 2, title: "اطلاعات اصلی آگهی" };
            case 3:
                return { step: 3, title: "ویژگی‌ها و مدارک" };
            case 4:
                return { step: 4, title: "بازبینی و انتشار" };
            default:
                return { step: 1, title: "انتخاب محصول" };
        }
    }, [wizardStep]);

    const mapTreeToTreeSelectData = (nodes) =>
        (nodes || []).map((node) => ({
            title: node.name,
            value: `cat-${node.id}`,
            selectable: false,
            children: [
                ...(node.children || []).map((c) => ({
                    title: c.name,
                    value: `cat-${c.id}`,
                    selectable: false,
                    children: mapTreeToTreeSelectData([c])[0]?.children || [],
                })),
                ...(node.products || []).map((p) => ({
                    title: p.productName,
                    value: p.productId,
                    isLeaf: true,
                })),
            ],
        }));

    const ensureDayjs = (value) => {
        if (!value) return null;
        if (dayjs.isDayjs(value)) return value;
        
        // اگر string بود (از backend آمده - gregorian ISO format)
        if (typeof value === "string") {
            return toShamsi(value);
        }
        
        return dayjs(value);
    };

    // ---------------------------
    // Initial load
    // ---------------------------
    useEffect(() => {
        let mounted = true;

        const load = async () => {
            try {
                setLoading(true);

                // new offer
                if (!offerId) {
                    setWizardStep(1);
                    setOfferDetail(null);
                    return;
                }

                // edit offer
                const detail = await offersApi.getMyOfferDetail(offerId);
                if (!mounted) return;

                setOfferDetail(detail);
                setWizardStep(detail.header?.wizardStep ?? 1);
            } catch (error) {
                message.error(getErrorMessage(error, "خطا در بارگذاری آگهی"));
                navigate("/my-offers", { replace: true });
            } finally {
                if (mounted) setLoading(false);
            }
        };

        load();
        return () => {
            mounted = false;
        };
    }, [offerId, navigate]);

    useEffect(() => {
        if (wizardStep !== 1) return;

        let mounted = true;

        const loadProducts = async () => {
            try {
                setLoadingProducts(true);
                const tree = await offersApi.getAvailableProducts();
                if (!mounted) return;
                setProductTree(tree || []);
            } catch (error) {
                message.error(getErrorMessage(error, "خطا در بارگذاری محصولات"));
            } finally {
                if (mounted) setLoadingProducts(false);
            }
        };

        loadProducts();

        return () => {
            mounted = false;
        };
    }, [wizardStep]);

    useEffect(() => {
        if (!offerDetail?.header?.productId) return;

        const pid = offerDetail.header.productId;
        setSelectedProductId(pid);
        setConfirmedProductId(pid);
    }, [offerDetail]);


    useEffect(() => {
        if (wizardStep !== 2) return;
        
        // اگر آگهی جدید است (offerDetail null) یا header ندارد، فرم را خالی کن
        if (!offerDetail?.header) {
            headerForm.resetFields();
            headerForm.setFieldsValue({
                unitPrice: undefined,
                quantity: undefined,
                hasTax: false,
                expireAtShamsi: null,
            });
            return;
        }

        // اگر آگهی موجود است، مقادیر را پر کن
        const expireRaw = offerDetail.header.expireAtBySupplier;

        headerForm.setFieldsValue({
            unitPrice: offerDetail.header.unitPrice,
            quantity: offerDetail.header.quantity,
            hasTax: offerDetail.header.hasTax,
            unit: offerDetail.header.unit,
            expireAtShamsi: expireRaw
                ? toShamsi(expireRaw)
                : null,
        });
    }, [wizardStep, offerDetail, headerForm]);

    useEffect(() => {
        // برای Stage 3 و 4 باید templates را load کنیم
        if (wizardStep !== 3 && wizardStep !== 4) return;
        if (!offerDetail?.header?.productId) return;

        const loadTemplates = async () => {
            setAttrLoading(true);
            try {
                const res = await offersApi.getProductAttributeTemplates(
                    offerDetail.header.productId
                );

                const templates = res?.data ?? res ?? [];
                setAttributeTemplates(templates);

                // init draft from existing documents
                const draft = {};
                (offerDetail.documents || []).forEach(d => {
                    draft[d.attributeDefinitionId] = {
                        value: d.value ?? null,
                        filePath: d.filePath ?? null,
                    };
                });
                setDocumentsDraft(draft);

            } finally {
                setAttrLoading(false);
            }
        };

        loadTemplates();
    }, [wizardStep, offerDetail?.header?.productId]);

    useEffect(() => {
        if (wizardStep !== 3) return;
        if (!attributeTemplates?.length) return;

        setDocDraftByAttrId(prev => {
            const next = { ...prev };

            attributeTemplates.forEach(attr => {
                if (!next[attr.attributeDefinitionId]) {
                    next[attr.attributeDefinitionId] = {
                        valueDraft: null,
                        fileDraft: null,
                        uploading: false,
                        uploadProgress: 0,
                        localPreviewUrl: null,
                    };
                }
            });

            return next;
        });
    }, [wizardStep, attributeTemplates]);


    const renderAttributeInput = (attr) => {
        const current = documentsDraft[attr.attributeDefinitionId] || {};

        const updateValue = (value) => {
            setDocumentsDraft(prev => ({
                ...prev,
                [attr.attributeDefinitionId]: {
                    ...prev[attr.attributeDefinitionId],
                    value,
                },
            }));
        };

        const updateFile = (filePath) => {
            setDocumentsDraft(prev => ({
                ...prev,
                [attr.attributeDefinitionId]: {
                    ...prev[attr.attributeDefinitionId],
                    filePath,
                },
            }));
        };

        switch (attr.dataType) {
            case 1: // Text
                return (
                    <Input
                        value={current.value ?? ""}
                        onChange={e => updateValue(e.target.value)}
                    />
                );

            case 2: // Number
                return (
                    <Input
                        type="number"
                        value={current.value ?? ""}
                        onChange={e => updateValue(e.target.value)}
                    />
                );

            case 3: // Boolean
                return (
                    <Switch
                        checked={current.value === "true"}
                        onChange={v => updateValue(String(v))}
                    />
                );

            case 4: // Date (شمسی)
                const dateValue = ensureDayjs(current.value);
                return (
                    <JalaliDatePicker
                        value={dateValue || undefined}
                        onChange={date => {
                            // مطمئن می‌شویم که date همیشه dayjs شمسی است
                            const shamsiDate = ensureShamsiDayjs(date);
                            updateValue(shamsiDate);
                        }}
                        format="YYYY/MM/DD"
                        style={{ width: "100%" }}
                        defaultPickerValue={todayShamsi()}
                        placeholder="انتخاب تاریخ"
                    />
                );

            case 5: {
                const attrId = attr.attributeDefinitionId;

                const persisted = documentsDraft[attrId]?.filePath
                    ? documentsDraft[attrId]
                    : null;

                const draft = docDraftByAttrId[attrId];

                return (
                    <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>

                        {/* فایل قبلی (persisted) */}
                        {persisted && !draft?.fileDraft && (
                            <div
                                style={{
                                    display: "flex",
                                    justifyContent: "space-between",
                                    alignItems: "center",
                                    background: "#f6ffed",
                                    padding: "12px",
                                    borderRadius: 6,
                                    border: "1px solid #b7eb8f",
                                }}
                            >
                                <Space>
                                    {(() => {
                                        const fileName = persisted.value?.toLowerCase() || "";
                                        const isImage = /\.(jpg|jpeg|png|gif|webp|bmp)$/i.test(fileName);
                                        const isPdf = fileName.endsWith(".pdf");
                                        const isWord = /\.(doc|docx)$/i.test(fileName);
                                        
                                        if (isImage) {
                                            return (
                                                <img
                                                    src={`http://localhost:5273/api/v1/offers/my/upload-file/file?offerId=${offerId}&filePath=${encodeURIComponent(persisted.filePath)}`}
                                                    alt={persisted.value}
                                                    style={{
                                                        width: 40,
                                                        height: 40,
                                                        objectFit: "cover",
                                                        borderRadius: 4,
                                                        border: "1px solid #d9d9d9",
                                                    }}
                                                />
                                            );
                                        }
                                        if (isPdf) return <FilePdfOutlined style={{ fontSize: 24, color: "#ff4d4f" }} />;
                                        if (isWord) return <FileWordOutlined style={{ fontSize: 24, color: "#1890ff" }} />;
                                        return <FileOutlined style={{ fontSize: 24, color: "#666" }} />;
                                    })()}
                                    <div>
                                        <div style={{ fontWeight: 500, fontSize: 13 }}>{persisted.value || "فایل"}</div>
                                        <div style={{ fontSize: 11, color: "#666" }}>فایل ثبت‌شده</div>
                                    </div>
                                </Space>
                                <Space>
                                    <Button 
                                        size="small"
                                        icon={<DownloadOutlined />}
                                        onClick={async () => {
                                            try {
                                                await offersApi.downloadOfferFile(offerId, persisted.filePath, persisted.value);
                                            } catch (error) {
                                                message.error(getErrorMessage(error, "خطا در دانلود فایل"));
                                            }
                                        }}
                                    >
                                        دانلود
                                    </Button>
                                    <Button 
                                        size="small"
                                        danger
                                        icon={<DeleteOutlined />}
                                        onClick={async () => {
                                            try {
                                                await offersApi.deleteOfferFile(offerId, persisted.filePath);
                                                
                                                // حذف از state
                                                setDocumentsDraft(prev => {
                                                    const next = { ...prev };
                                                    delete next[attrId];
                                                    return next;
                                                });
                                                
                                                message.success("فایل با موفقیت حذف شد");
                                                
                                                // Reload offer detail برای به‌روزرسانی UI
                                                const detail = await offersApi.getMyOfferDetail(offerId);
                                                setOfferDetail(detail);
                                            } catch (error) {
                                                message.error(getErrorMessage(error, "خطا در حذف فایل"));
                                            }
                                        }}
                                    >
                                        حذف
                                    </Button>
                                </Space>
                            </div>
                        )}

                        {/* فایل جدید (draft) */}
                        {draft?.fileDraft && (
                            <div
                                style={{
                                    background: draft.uploading ? "#fff7e6" : "#f6ffed",
                                    padding: "12px",
                                    borderRadius: 6,
                                    border: `1px solid ${draft.uploading ? "#ffd591" : "#b7eb8f"}`,
                                }}
                            >
                                <Space direction="vertical" size="small" style={{ width: "100%" }}>
                                    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                                        <Space>
                                            {draft.localPreviewUrl ? (
                                                <img
                                                    src={draft.localPreviewUrl}
                                                    alt="preview"
                                                    style={{
                                                        width: 50,
                                                        height: 50,
                                                        objectFit: "cover",
                                                        borderRadius: 4,
                                                        border: "1px solid #d9d9d9",
                                                    }}
                                                />
                                            ) : (
                                                (() => {
                                                    const fileName = draft.fileDraft.fileName?.toLowerCase() || "";
                                                    const mimeType = draft.fileDraft.mimeType || "";
                                                    const isPdf = mimeType === "application/pdf" || fileName.endsWith(".pdf");
                                                    const isWord = mimeType.includes("word") || /\.(doc|docx)$/i.test(fileName);
                                                    
                                                    if (isPdf) return <FilePdfOutlined style={{ fontSize: 32, color: "#ff4d4f" }} />;
                                                    if (isWord) return <FileWordOutlined style={{ fontSize: 32, color: "#1890ff" }} />;
                                                    return <FileOutlined style={{ fontSize: 32, color: "#666" }} />;
                                                })()
                                            )}
                                            <div>
                                                <div style={{ fontWeight: 500, fontSize: 13 }}>
                                                    {draft.fileDraft.fileName}
                                                </div>
                                                <div style={{ fontSize: 11, color: "#666" }}>
                                                    {draft.fileDraft.size ? 
                                                        (draft.fileDraft.size < 1024 
                                                            ? `${draft.fileDraft.size} B`
                                                            : draft.fileDraft.size < 1024 * 1024
                                                            ? `${(draft.fileDraft.size / 1024).toFixed(2)} KB`
                                                            : `${(draft.fileDraft.size / (1024 * 1024)).toFixed(2)} MB`)
                                                        : ""}
                                                </div>
                                            </div>
                                        </Space>
                                        {!draft.uploading && draft.fileDraft.filePath && (
                                            <CheckCircleOutlined style={{ fontSize: 20, color: "#52c41a" }} />
                                        )}
                                    </div>
                                    
                                    {draft.uploading && (
                                        <Progress 
                                            percent={draft.uploadProgress || 0} 
                                            status="active"
                                            size="small"
                                        />
                                    )}
                                    
                                    {!draft.uploading && draft.fileDraft.filePath && (
                                        <Button
                                            size="small"
                                            danger
                                            icon={<DeleteOutlined />}
                                            onClick={async () => {
                                                // چک می‌کنیم که آیا فایل در دیتابیس ذخیره شده یا نه
                                                // اگر فایل در offerDetail.documents باشد، یعنی persisted است
                                                const isPersisted = offerDetail?.documents?.some(
                                                    doc => doc.attributeDefinitionId === attrId && 
                                                           doc.filePath === draft.fileDraft.filePath &&
                                                           !doc.isDeleted
                                                );
                                                
                                                if (isPersisted) {
                                                    // اگر در دیتابیس ذخیره شده، به backend می‌رویم
                                                    try {
                                                        setDocDraftByAttrId(prev => ({
                                                            ...prev,
                                                            [attrId]: {
                                                                ...prev[attrId],
                                                                deleting: true,
                                                            },
                                                        }));

                                                        await offersApi.deleteOfferFile(offerId, draft.fileDraft.filePath);

                                                        // حذف از state
                                                        setDocDraftByAttrId(prev => {
                                                            const next = { ...prev };
                                                            delete next[attrId];
                                                            return next;
                                                        });
                                                        setDocumentsDraft(prev => {
                                                            const next = { ...prev };
                                                            delete next[attrId];
                                                            return next;
                                                        });
                                                        updateValue(null);
                                                        updateFile(null);
                                                        
                                                        message.success("فایل با موفقیت حذف شد");
                                                        
                                                        // Reload offer detail
                                                        const detail = await offersApi.getMyOfferDetail(offerId);
                                                        setOfferDetail(detail);
                                                    } catch (error) {
                                                        message.error(getErrorMessage(error, "خطا در حذف فایل"));
                                                        setDocDraftByAttrId(prev => ({
                                                            ...prev,
                                                            [attrId]: {
                                                                ...prev[attrId],
                                                                deleting: false,
                                                            },
                                                        }));
                                                    }
                                                } else {
                                                    // اگر هنوز در دیتابیس ذخیره نشده، فقط state را پاک می‌کنیم
                                                    setDocDraftByAttrId(prev => ({
                                                        ...prev,
                                                        [attrId]: {
                                                            uploading: false,
                                                            localPreviewUrl: null,
                                                            fileDraft: null,
                                                        },
                                                    }));
                                                    updateValue(null);
                                                    updateFile(null);
                                                    
                                                    message.success("فایل حذف شد");
                                                }
                                            }}
                                        >
                                            حذف فایل
                                        </Button>
                                    )}
                                </Space>
                            </div>
                        )}

                        {/* آپلود فایل جدید */}
                        {(!persisted || (draft?.fileDraft && !draft.uploading && draft.fileDraft.filePath)) && (
                            <Upload
                                showUploadList={false}
                                beforeUpload={async (file) => {
                                    try {
                                        const isImage = file.type.startsWith("image/");
                                        const localPreviewUrl = isImage
                                            ? URL.createObjectURL(file)
                                            : null;

                                        setDocDraftByAttrId(prev => ({
                                            ...prev,
                                            [attrId]: {
                                                uploading: true,
                                                uploadProgress: 0,
                                                localPreviewUrl,
                                                fileDraft: {
                                                    fileName: file.name,
                                                    size: file.size,
                                                    mimeType: file.type,
                                                },
                                            },
                                        }));

                                        const res = await offersApi.uploadMyOfferFile({
                                            offerId,
                                            file,
                                        });

                                        setDocDraftByAttrId(prev => ({
                                            ...prev,
                                            [attrId]: {
                                                uploading: false,
                                                uploadProgress: 100,
                                                localPreviewUrl,
                                                fileDraft: {
                                                    ...prev[attrId].fileDraft,
                                                    filePath: res.filePath,
                                                },
                                            },
                                        }));

                                        // مهم: هم‌زمان documentsDraft رو هم آپدیت کن
                                        updateValue(file.name);
                                        updateFile(res.filePath);
                                        
                                        message.success("فایل با موفقیت آپلود شد");
                                        return false;
                                    } catch (error) {
                                        // Reset uploading state on error
                                        setDocDraftByAttrId(prev => ({
                                            ...prev,
                                            [attrId]: {
                                                uploading: false,
                                                uploadProgress: 0,
                                                localPreviewUrl: null,
                                                fileDraft: null,
                                            },
                                        }));
                                        message.error(getErrorMessage(error, "خطا در آپلود فایل"));
                                    }
                                    return false;
                                }}
                            >
                                <Button 
                                    loading={draft?.uploading}
                                    disabled={draft?.uploading}
                                >
                                    {persisted ? "تغییر فایل" : "انتخاب فایل"}
                                </Button>
                            </Upload>
                        )}
                    </div>
                );
            }


            default:
                return null;
        }
    };



    if (loading) {
        return (
            <div style={{ textAlign: "center", padding: 48 }}>
                <Spin />
            </div>
        );
    }

    return (
        <Card title="ایجاد / ویرایش آگهی">
            {/* Stage Header (مثل SupplierOnboarding) */}
            <div style={{ marginBottom: 24 }}>
                <strong>مرحله {stageInfo.step} از 4</strong>
                <div style={{ color: "#888" }}>{stageInfo.title}</div>
            </div>

            <Divider />

            {/* =============================
                Stage Content
            ============================== */}
            {wizardStep === 1 && (
                <div>
                    {productTree.length === 0 && !loadingProducts && (
                        <Alert
                            type="warning"
                            message="محصولی برای ثبت آگهی در دسترس نیست."
                            showIcon
                        />
                    )}

                    <div style={{ marginTop: 16, maxWidth: 420 }}>
                        <TreeSelect
                            style={{ width: "100%" }}
                            placeholder="انتخاب محصول"
                            loading={loadingProducts}
                            treeData={mapTreeToTreeSelectData(productTree)}
                            value={selectedProductId}
                            treeDefaultExpandAll
                            allowClear
                            onChange={(value) => {
                                if (!value || isNaN(Number(value))) {
                                    setSelectedProductId(null);
                                    return;
                                }
                                setSelectedProductId(Number(value));
                            }}
                            treeCheckable={false}
                            treeNodeFilterProp="title"
                            showSearch
                        />
                    </div>
                </div>
            )}


            {wizardStep === 2 && (
                <Form
                    form={headerForm}
                    layout="vertical"
                    style={{ maxWidth: 420 }}
                >
                    <Form.Item
                        label="قیمت واحد (تومان)"
                        name="unitPrice"
                        rules={[
                            { required: true, message: "قیمت واحد الزامی است" },
                            { 
                                validator: (_, value) => {
                                    if (value == null || value === "") {
                                        return Promise.resolve();
                                    }
                                    if (value <= 0) {
                                        return Promise.reject(new Error("قیمت واحد باید بیشتر از صفر باشد"));
                                    }
                                    return Promise.resolve();
                                }
                            }
                        ]}
                    >
                        <InputNumber
                            style={{ width: "100%" }}
                            placeholder="مثال: 100000"
                            formatter={(value) =>
                                value ? `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",") : ""
                            }
                            parser={(value) =>
                                value ? value.replace(/[^\d]/g, "") : ""
                            }
                            onChange={(value) => {
                                // اگر 0 یا کمتر از 0 بود، به null تبدیل کن (خالی بماند)
                                if (value != null && value <= 0) {
                                    headerForm.setFieldValue("unitPrice", null);
                                }
                            }}
                        />

                    </Form.Item>

                    <Form.Item
                        label="مقدار"
                        name="quantity"
                        rules={[
                            { required: true, message: "مقدار الزامی است" },
                            { 
                                validator: (_, value) => {
                                    if (value == null || value === "") {
                                        return Promise.resolve();
                                    }
                                    if (value <= 0) {
                                        return Promise.reject(new Error("مقدار باید بیشتر از صفر باشد"));
                                    }
                                    return Promise.resolve();
                                }
                            }
                        ]}
                    >
                        <InputNumber
                            style={{ width: "100%" }}
                            step={0.01}
                            placeholder="مثال: 100"
                            formatter={(value) =>
                                value ? `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",") : ""
                            }
                            parser={(value) =>
                                value ? value.replace(/[^\d.]/g, "") : ""
                            }
                            onChange={(value) => {
                                // اگر 0 یا کمتر از 0 بود، به null تبدیل کن (خالی بماند)
                                if (value != null && value <= 0) {
                                    headerForm.setFieldValue("quantity", null);
                                }
                            }}
                        />

                    </Form.Item>

                    {/* ✅ Unit لازم است برای API */}
                    <Form.Item name="unit" hidden>
                        <Input />
                    </Form.Item>

                    <Form.Item label="واحد">
                        <Input value={offerDetail?.header?.unit ?? ""} disabled />
                    </Form.Item>

                    {/* ✅ Switch به جای Checkbox */}
                    <Form.Item label="مشمول مالیات" name="hasTax" valuePropName="checked">
                        <Switch />
                    </Form.Item>

                    {/* ✅ نمایش قیمت کل و مالیات (محاسبه سمت UI) */}
                    <Form.Item shouldUpdate>
                        {() => {
                            const unitPrice = headerForm.getFieldValue("unitPrice");
                            const quantity = headerForm.getFieldValue("quantity");
                            const hasTax = Boolean(headerForm.getFieldValue("hasTax"));

                            // فقط اگر هر دو مقدار معتبر باشند محاسبه کن
                            const unitPriceNum = unitPrice != null && unitPrice > 0 ? Number(unitPrice) : null;
                            const quantityNum = quantity != null && quantity > 0 ? Number(quantity) : null;
                            
                            const totalPrice = (unitPriceNum != null && quantityNum != null) 
                                ? unitPriceNum * quantityNum 
                                : null;
                            const taxAmount = (hasTax && totalPrice != null) ? totalPrice * TAX_RATE : null;

                            return (
                                <>
                                    <Form.Item label="قیمت کل (تومان)">
                                        <Input
                                            value={totalPrice != null
                                                ? totalPrice.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                                                : "-"}
                                            disabled 
                                            placeholder="پس از وارد کردن قیمت واحد و مقدار محاسبه می‌شود"
                                        />
                                    </Form.Item>

                                    {hasTax && (
                                        <Form.Item label={`مبلغ مالیات (${TAX_RATE * 100}٪) (تومان)`}>
                                            <Input 
                                                value={taxAmount != null
                                                    ? taxAmount.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
                                                    : "-"}
                                                disabled 
                                                placeholder="پس از وارد کردن قیمت واحد و مقدار محاسبه می‌شود"
                                            />
                                        </Form.Item>
                                    )}
                                </>
                            );
                        }}
                    </Form.Item>

                    {/* ✅ تاریخ انقضا - فعلاً ورودی شمسی (YYYY/MM/DD) */}
                    <Form.Item
                        label="تاریخ اعتبار آگهی"
                        name="expireAtShamsi"
                    >
                        <JalaliDatePicker
                            style={{ width: "100%" }}
                            format="YYYY/MM/DD"
                            placeholder="انتخاب تاریخ"
                            defaultPickerValue={todayShamsi()}
                            onChange={(date) => {
                                // مطمئن می‌شویم که date همیشه dayjs شمسی است
                                const shamsiDate = ensureShamsiDayjs(date);
                                headerForm.setFieldValue("expireAtShamsi", shamsiDate);
                            }}
                        />


                    </Form.Item>
                </Form>
            )}


            {wizardStep === 3 && (
                <>
                    {attrLoading ? (
                        <Spin />
                    ) : (
                        <Form layout="vertical" style={{ maxWidth: 420 }}>
                            {attributeTemplates.map(attr => (
                                <Form.Item
                                    key={attr.attributeDefinitionId}
                                    label={attr.displayName}
                                    required={attr.isRequired}
                                >
                                    {renderAttributeInput(attr)}
                                </Form.Item>
                            ))}
                        </Form>
                    )}
                </>
            )}



            {wizardStep === 4 && (

                <Card>
                    <Divider orientation="right">
                        بازبینی نهایی آگهی
                    </Divider>

                    {/* خلاصه اطلاعات */}
                    <Space direction="vertical" size={16} style={{ width: "100%" }}>

                        {/* محصول */}
                        <Card size="small" title="محصول">
                            <div>{offerDetail?.header?.productName || "-"}</div>
                        </Card>

                        {/* اطلاعات اصلی */}
                        <Card size="small" title="اطلاعات آگهی">
                            <div>قیمت واحد: {formatPrice(offerDetail?.header?.unitPrice)} تومان</div>
                            <div>مقدار: {offerDetail?.header?.quantity ?? "-"}</div>

                            <div>
                                قیمت کل: {formatPrice(offerDetail?.header?.totalPrice)} تومان
                            </div>

                            {offerDetail?.header?.hasTax && (
                                <div>
                                    مبلغ مالیات: {formatPrice(offerDetail?.header?.taxAmount)} تومان
                                </div>
                            )}

                            <div>
                                تاریخ انقضا:{" "}
                                {offerDetail?.header?.expireAtBySupplier
                                    ? toShamsi(offerDetail.header.expireAtBySupplier).format("YYYY/MM/DD")
                                    : "-"}
                            </div>
                        </Card>


                        {/* ویژگی‌ها و مدارک */}
                        <Card size="small" title="ویژگی‌ها و مدارک">
                            {attributeTemplates.map(attr => {
                                const doc = offerDetail?.documents?.find(
                                    d => d.attributeDefinitionId === attr.attributeDefinitionId
                                );

                                return (
                                    <div
                                        key={attr.attributeDefinitionId}
                                        style={{
                                            display: "flex",
                                            justifyContent: "space-between",
                                            padding: "6px 0",
                                            borderBottom: "1px dashed #eee",
                                        }}
                                    >
                                        <span>{attr.displayName}</span>

                                        {attr.dataType === 5 ? (
                                            // File type: نمایش نام فایل + دکمه Download
                                            doc?.filePath ? (
                                                <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                                                    <span>{doc?.value || "فایل"}</span>
                                                    <Button 
                                                        size="small"
                                                        onClick={async () => {
                                                            try {
                                                                await offersApi.downloadOfferFile(offerId, doc.filePath, doc.value);
                                                            } catch (error) {
                                                                message.error(getErrorMessage(error, "خطا در دانلود فایل"));
                                                            }
                                                        }}
                                                    >
                                                        دانلود فایل
                                                    </Button>
                                                </div>
                                            ) : (
                                                <span>-</span>
                                            )
                                        ) : attr.dataType === 4 ? (
                                            // Date type: تبدیل gregorian به شمسی
                                            doc?.value ? (
                                                <span>{toShamsi(doc.value).format("YYYY/MM/DD")}</span>
                                            ) : (
                                                <span>-</span>
                                            )
                                        ) : attr.dataType === 3 ? (
                                            // Boolean type: نمایش "بله" یا "خیر"
                                            doc?.value === "true" ? (
                                                <span>بله</span>
                                            ) : doc?.value === "false" ? (
                                                <span>خیر</span>
                                            ) : (
                                                <span>-</span>
                                            )
                                        ) : (
                                            // Text, Number: نمایش مستقیم
                                            <span>{doc?.value ?? "-"}</span>
                                        )}
                                    </div>
                                );
                            })}
                        </Card>

                        {/* نمایش وضعیت آگهی */}
                        {offerDetail?.header?.status !== undefined && (
                            <Alert
                                type={
                                    offerDetail.header.status === 3 ? "success" : // Published
                                    offerDetail.header.status === 4 ? "error" : // Cancel
                                    "warning" // Draft
                                }
                                message={
                                    offerDetail.header.status === 3 ? "آگهی منتشر شده است" :
                                    offerDetail.header.status === 4 ? "آگهی شما لغو شده است" :
                                    "آگهی هنوز منتشر نشده و به کسی نمایش داده نمی‌شود"
                                }
                                showIcon
                            />
                        )}
                    </Space>
                </Card>
            )}


            <Divider />

            {/* =============================
                Action Buttons (per step)
            ============================== */}
            <Space>
                {wizardStep === 1 && (
                    <Button
                        type="primary"
                        disabled={!Number.isInteger(selectedProductId)}
                        loading={savingStep1}
                        onClick={async () => {
                            if (!selectedProductId) return;

                            const isNew = !offerDetail;
                            const prevProductId = Number(offerDetail?.header?.productId);

                            const proceed = async () => {
                                try {
                                    setSavingStep1(true);

                                    // create draft
                                    if (isNew) {
                                        const res = await offersApi.createDraft({
                                            productId: selectedProductId,
                                        });

                                        // 🔴 خیلی مهم: اول detail را load کن
                                        const detail = await offersApi.getMyOfferDetail(res.offerId);

                                        // state ها را ست کن
                                        setOfferDetail(detail);
                                        setWizardStep(detail.header.wizardStep);

                                        // بعد route را sync کن (بدون reload)
                                        navigate(`/supplier/offers/manage/${res.offerId}`, { replace: true });

                                        return;
                                    }


                                    // change product on existing draft
                                    if (prevProductId !== selectedProductId) {
                                        await offersApi.changeProduct(offerDetail.header.id, {
                                            productId: selectedProductId,
                                        });
                                    }

                                    // reload detail
                                    const detail = await offersApi.getMyOfferDetail(
                                        offerDetail.header.id
                                    );
                                    setOfferDetail(detail);
                                    // اگر محصول تغییر نکرده باشد، به مرحله 2 برو
                                    // در غیر این صورت wizardStep از backend می‌آید (که باید 2 باشد)
                                    setWizardStep(prevProductId === selectedProductId ? 2 : detail.header.wizardStep);
                                } catch (error) {
                                    message.error(getErrorMessage(error, "خطا در ذخیره محصول"));
                                } finally {
                                    setSavingStep1(false);
                                }
                            };

                            // confirm reset if product changed
                            if (!isNew && prevProductId !== selectedProductId) {
                                modal.confirm({
                                    title: "تغییر محصول",
                                    content: "در صورت تغییر محصول، تمام اطلاعات قبلی حذف می‌شود. ادامه می‌دهید؟",
                                    okText: "بله، ادامه",
                                    cancelText: "انصراف",
                                    onOk: async () => {
                                        await proceed();
                                        setConfirmedProductId(selectedProductId);
                                    },
                                    onCancel: () => {
                                        // برگشت به محصول قبلی
                                        setSelectedProductId(confirmedProductId);
                                    },
                                });

                            } else {
                                proceed();
                            }
                        }}
                    >
                        ذخیره و ادامه
                    </Button>
                )}
                {wizardStep === 2 && (
                    <>
                        <Button
                            onClick={() => setWizardStep(1)}
                        >
                            مرحله قبل
                        </Button>

                        <Button
                            type="primary"
                            loading={savingStep2}
                            onClick={async () => {
                                try {
                                    // Validation اضافی: چک کردن که قیمت و مقدار معتبر باشند
                                    const values = await headerForm.validateFields();
                                    
                                    if (!values.unitPrice || values.unitPrice <= 0) {
                                        message.error("لطفاً قیمت واحد را وارد کنید");
                                        return;
                                    }
                                    
                                    if (!values.quantity || values.quantity <= 0) {
                                        message.error("لطفاً مقدار را وارد کنید");
                                        return;
                                    }

                                    // محاسبه total و tax (بعد از validation)
                                    const unitPrice = Number(values.unitPrice);
                                    const quantity = Number(values.quantity);
                                    const totalPrice = unitPrice * quantity;

                                    const hasTax = Boolean(values.hasTax);
                                    const taxAmount = hasTax ? totalPrice * TAX_RATE : null;

                                    // تبدیل شمسی به میلادی (فعلاً ساده: اگر خالی بود null)
                                    // ⚠️ برای تبدیل دقیق شمسی → میلادی در قدم بعدی jalaali-js اضافه می‌کنیم
                                    const expireAtBySupplier = values.expireAtShamsi
                                        ? toGregorianString(values.expireAtShamsi)
                                        : null;


                                    // ✅ unit را حتماً بفرست (برای رفع 400)
                                    const payload = {
                                        unitPrice,
                                        quantity,
                                        unit: offerDetail?.header?.unit ?? values.unit, // مطمئن
                                        hasTax,
                                        taxAmount,
                                        expireAtBySupplier, // فعلاً placeholder تا تبدیل دقیق
                                    };

                                    setSavingStep2(true);

                                    await offersApi.updateHeader(offerDetail.header.id, payload);


                                    const detail = await offersApi.getMyOfferDetail(
                                        offerDetail.header.id
                                    );

                                    setOfferDetail(detail);
                                    setWizardStep(detail.header.wizardStep);
                                } catch (error) {
                                    message.error(getErrorMessage(error, "خطا در ذخیره اطلاعات اصلی"));
                                } finally {
                                    setSavingStep2(false);
                                }
                            }}
                        >
                            ذخیره و ادامه
                        </Button>
                    </>
                )}

                {wizardStep === 3 && (
                    <>
                        <Button
                            onClick={() => setWizardStep(2)}
                        >
                            مرحله قبل
                        </Button>

                        <Button
                            type="primary"
                            loading={savingStep3}
                            onClick={async () => {
                                try {
                                    setSavingStep3(true);

                                    // Validation: چک کردن فیلدهای required
                                    const errors = [];
                                    attributeTemplates.forEach(attr => {
                                        if (!attr.isRequired) return;
                                        
                                        const draft = documentsDraft[attr.attributeDefinitionId];
                                        
                                        if (attr.dataType === 5) {
                                            // File type
                                            if (!draft?.filePath) {
                                                errors.push(`فیلد «${attr.displayName}» الزامی است`);
                                            }
                                        } else {
                                            // Text, Number, Boolean, Date
                                            if (!draft?.value || (typeof draft.value === "string" && draft.value.trim() === "")) {
                                                errors.push(`فیلد «${attr.displayName}» الزامی است`);
                                            }
                                        }
                                    });

                                    if (errors.length > 0) {
                                        message.error(errors[0]);
                                        return;
                                    }

                                    const items = Object.entries(documentsDraft).map(
                                        ([attributeDefinitionId, v]) => ({
                                            attributeDefinitionId: Number(attributeDefinitionId),
                                            value:
                                                v.value && dayjs.isDayjs(v.value)
                                                    ? toGregorianString(v.value)   // ✅ تبدیل شمسی → میلادی
                                                    : v.value ?? null,
                                            filePath: v.filePath ?? null,
                                        })
                                    );

                                    await offersApi.saveDocuments(offerId, { items });

                                    // reload detail بعد از save
                                    const detail = await offersApi.getMyOfferDetail(offerId);
                                    setOfferDetail(detail);
                                    setWizardStep(detail.header.wizardStep);
                                } catch (error) {
                                    message.error(getErrorMessage(error, "خطا در ذخیره ویژگی‌ها و مدارک"));
                                } finally {
                                    setSavingStep3(false);
                                }
                            }}
                        >
                            ذخیره و ادامه
                        </Button>
                    </>
                )}

                {wizardStep === 4 && (
                    <>
                        {offerDetail?.header?.status === 3 ? (
                            // اگر منتشر شده: فقط دکمه لغو آگهی
                            <Button
                                danger
                                loading={submitting}
                                onClick={async () => {
                                    try {
                                        setSubmitting(true);
                                        await offersApi.cancel(offerId);
                                        message.success("آگهی لغو شد");
                                        
                                        // reload detail
                                        const detail = await offersApi.getMyOfferDetail(offerId);
                                        setOfferDetail(detail);
                                    } catch (error) {
                                        message.error(getErrorMessage(error, "خطا در لغو آگهی"));
                                    } finally {
                                        setSubmitting(false);
                                    }
                                }}
                            >
                                لغو آگهی
                            </Button>
                        ) : offerDetail?.header?.status === 4 ? (
                            // اگر لغو شده: هیچ دکمه‌ای نمایش نده
                            null
                        ) : (
                            // اگر Draft: دکمه‌های معمولی
                            <>
                                <Button onClick={() => setWizardStep(3)}>
                                    مرحله قبل
                                </Button>

                                <Button
                                    type="primary"
                                    loading={submitting}
                                    onClick={async () => {
                                        try {
                                            setSubmitting(true);
                                            await offersApi.submit(offerId);
                                            message.success("آگهی با موفقیت ارسال شد");
                                            
                                            // reload detail برای گرفتن status جدید
                                            const detail = await offersApi.getMyOfferDetail(offerId);
                                            setOfferDetail(detail);
                                            setWizardStep(detail.header.wizardStep);
                                        } catch (error) {
                                            message.error(getErrorMessage(error, "خطا در ارسال آگهی"));
                                        } finally {
                                            setSubmitting(false);
                                        }
                                    }}
                                >
                                    انتشار آگهی
                                </Button>
                            </>
                        )}
                    </>
                )}


            </Space>
        </Card>
    );
}
