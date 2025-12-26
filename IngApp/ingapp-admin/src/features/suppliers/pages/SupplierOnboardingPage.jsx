// src/features/suppliers/pages/SupplierOnboardingPage.jsx

import React, { useCallback, useEffect, useMemo, useState } from "react";
import {
    App,
    Card,
    Form,
    Input,
    Button,
    Row,
    Col,
    Space,
    Spin,
    Radio,
    Divider,
    Upload,
    Typography,
    Alert,
    Tag,
    Progress,
    Modal,
    Image,
    Select,
} from "antd";
import { UploadOutlined, DownloadOutlined, EyeOutlined } from "@ant-design/icons";

import supplierOnboardingApi from "../api/supplierOnboardingApi";
import supplierTypesApi from "../../supplierTypes/api/supplierTypesApi";
import { getProvinces, getCitiesByProvince } from "../../../core/location/iranProvinces";

const { Text } = Typography;
const { Option } = Select;

/**
 * DataType mapping (Backend enum):
 * 1 = File
 * 2 = Text
 * 3 = Number
 * 4 = Boolean
 * 5 = Enum
 */

const SupplierOnboardingPage = () => {
    const { message, modal } = App.useApp();

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    // type | profile | kyc | status
    const [stage, setStage] = useState("type");

    const [supplierTypes, setSupplierTypes] = useState([]);
    const [profile, setProfile] = useState(null);
    const [selectedSupplierTypeId, setSelectedSupplierTypeId] = useState(null);

    const [kycLoading, setKycLoading] = useState(false);
    const [submittingKyc, setSubmittingKyc] = useState(false);

    const [kycRequirements, setKycRequirements] = useState([]);
    const [kycDocuments, setKycDocuments] = useState([]);

    /**
     * Local draft values (NOT persisted until Submit):
     * key: attributeDefinitionId => {
     *   valueDraft: string|null,
     *   fileDraft: { filePath, fileName, mimeType, size } | null,
     *   uploading: boolean,
     *   uploadProgress: number,
     *   localPreviewUrl: string|null,
     * }
     */
    const [draftByAttrId, setDraftByAttrId] = useState({});

    // Preview Modal state
    const [preview, setPreview] = useState({
        open: false,
        title: "",
        url: "",
    });

    const [form] = Form.useForm();

    // province/city select state (UI-only)
    const [selectedProvince, setSelectedProvince] = useState(null);

    // --------------------------------------------------
    // API Base (Dev/Prod)
    // --------------------------------------------------
    const apiBaseUrl = useMemo(() => {
        try {
            if (typeof import.meta !== "undefined" && import.meta.env) {
                return (
                    import.meta.env.VITE_API_BASE_URL ||
                    import.meta.env.VITE_API_URL ||
                    ""
                );
            }
        } catch {
            // ignore
        }

        if (typeof process !== "undefined" && process.env) {
            return (
                process.env.REACT_APP_API_BASE_URL ||
                process.env.REACT_APP_API_URL ||
                ""
            );
        }

        return "https://localhost:7145";
    }, []);

    const getAccessToken = () => {
        return (
            localStorage.getItem("accessToken") ||
            localStorage.getItem("token") ||
            localStorage.getItem("jwt") ||
            ""
        );
    };

    const unwrapApiResult = (payload) => {
        if (!payload) return null;
        if (typeof payload === "object" && "success" in payload && "data" in payload) {
            return payload.data;
        }
        return payload;
    };

    const apiFetchJson = async (path, options = {}) => {
        const token = getAccessToken();

        const res = await fetch(`${apiBaseUrl}${path}`, {
            ...options,
            headers: {
                ...(options.headers || {}),
                "Content-Type": "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        });

        const text = await res.text();
        let json = null;
        try {
            json = text ? JSON.parse(text) : null;
        } catch {
            json = null;
        }

        if (!res.ok) {
            const msg =
                json?.message ||
                json?.title ||
                (Array.isArray(json?.errors) ? json.errors.join("، ") : null) ||
                "خطای سرور";
            throw new Error(msg);
        }

        // حتی اگر ok بود ولی success=false
        if (json && typeof json === "object" && "success" in json && json.success === false) {
            throw new Error(json.message || "عملیات ناموفق بود.");
        }

        return json;
    };

    // --------------------------------------------------
    // Helpers
    // --------------------------------------------------
    const getDataTypeNumber = (dt) => {
        if (typeof dt === "number") return dt;
        const map = { File: 1, Text: 2, Number: 3, Boolean: 4, Enum: 5 };
        return map[dt] || 0;
    };

    const statusLabel = (s) => {
        if (s === null || s === undefined) return null;

        // numeric enum from backend
        if (typeof s === "number") {
            if (s === 0) return "در انتظار بررسی";
            if (s === 1) return "تأیید شده";
            if (s === 2) return "رد شده";
        }

        // string enum (fallback)
        const map = {
            Pending: "در انتظار بررسی",
            Approved: "تأیید شده",
            Rejected: "رد شده",
        };

        return map[s] || String(s);
    };


    const getDocumentByAttributeId = useCallback(
        (attributeId) => {
            return (kycDocuments || []).find((d) => d.attributeDefinitionId === attributeId) || null;
        },
        [kycDocuments]
    );

    // Stage2 required fields (business rule)
    const isProfileBasicComplete = useCallback((p) => {
        if (!p) return false;
        const businessName = (p.businessName ?? "").trim();
        const province = (p.province ?? "").trim();
        const city = (p.city ?? "").trim();
        const contactPhone = (p.contactPhone ?? "").trim();

        return Boolean(businessName && province && city && contactPhone);
    }, []);

    /**
     * Derive stage from backend data (Single Source of Truth):
     * - no supplierTypeId => type
     * - supplierTypeId exists:
     *    - if basic info incomplete => profile
     *    - else if verificationStatus != 0 => status
     *    - else => kyc
     */
    const deriveStageFromProfile = useCallback(
        (p) => {
            if (!p?.supplierTypeId) return "type";
            if (!isProfileBasicComplete(p)) return "profile";

            // verificationStatus: 0=NotSubmitted , 1=Pending , 2=Approved , 3=Rejected
            if (
                p?.verificationStatus === "Pending" ||
                p?.verificationStatus === "Approved" ||
                p?.verificationStatus === "Rejected"
            ) {
                return "status";
            }


            return "kyc";
        },
        [isProfileBasicComplete]
    );

    const isAnyUploading = useMemo(() => {
        return Object.values(draftByAttrId || {}).some((x) => x?.uploading);
    }, [draftByAttrId]);

    // --------------------------------------------------
    // Province / City options (MUST be before any return)
    // --------------------------------------------------
    const provinces = useMemo(() => getProvinces(), []);
    const cities = useMemo(
        () => getCitiesByProvince(selectedProvince),
        [selectedProvince]
    );

    // --------------------------------------------------
    // Load Supplier Types
    // --------------------------------------------------
    const loadSupplierTypes = useCallback(async () => {
        const res = await supplierTypesApi.getAll();
        setSupplierTypes(res || []);
    }, []);

    // --------------------------------------------------
    // Load My Profile
    // --------------------------------------------------
    const loadMyProfile = useCallback(async () => {
        const raw = await supplierOnboardingApi.getMyProfile();
        const res = unwrapApiResult(raw);

        setProfile(res);

        if (res?.supplierTypeId && selectedSupplierTypeId === null) {
            setSelectedSupplierTypeId(res.supplierTypeId);

            // فرم را پر می‌کنیم که اگر برگشت Stage 2 لازم شد، آماده باشد
            form.setFieldsValue({
                businessName: res.businessName,
                nationalId: res.nationalId,
                licenseNumber: res.licenseNumber,
                province: res.province,
                city: res.city,
                address: res.address,
                contactName: res.contactName,
                contactPhone: res.contactPhone,
            });

            setSelectedProvince(res.province || null);
        }

        return res;
    }, [form]);

    // --------------------------------------------------
    // Load KYC (requirements + documents)
    // --------------------------------------------------
    const loadKyc = useCallback(
        async (supplierTypeId, shouldUpdateStage = true) => {
            if (!supplierTypeId) return { requirements: [], documents: [] };


            setKycLoading(true);
            try {
                const reqRes = await apiFetchJson("/api/v1/kyc/my/requirements", { method: "GET" });
                const requirements = unwrapApiResult(reqRes) || [];
                setKycRequirements(requirements);

                const docRes = await apiFetchJson("/api/v1/kyc/my/documents", { method: "GET" });
                const documents = unwrapApiResult(docRes) || [];
                setKycDocuments(documents);

                // init draft map if not exists, BUT do not overwrite existing draft
                setDraftByAttrId((prev) => {
                    const next = { ...(prev || {}) };
                    (requirements || []).forEach((r) => {
                        const attrId = r.attributeDefinitionId;
                        if (!next[attrId]) {
                            next[attrId] = {
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

                if (shouldUpdateStage) {
                    // Stage is derived from profile, not from docs.
                    const nextStage = deriveStageFromProfile(profile);
                    if (nextStage) setStage(nextStage);
                }

                return { requirements, documents };
            } catch (e) {
                message.error(e?.message || "خطا در دریافت اطلاعات KYC");
                return { requirements: [], documents: [] };
            } finally {
                setKycLoading(false);
            }
        },
        [message, deriveStageFromProfile]
    );

    const clearKycDraftLocal = useCallback(() => {
        setDraftByAttrId((prev) => {
            const next = { ...(prev || {}) };
            Object.keys(next).forEach((k) => {
                next[k] = {
                    valueDraft: null,
                    fileDraft: null,
                    uploading: false,
                    uploadProgress: 0,
                    localPreviewUrl: null,
                };
            });
            return next;
        });
    }, []);

    // --------------------------------------------------
    // Init
    // --------------------------------------------------
    useEffect(() => {
        (async () => {
            try {
                await loadSupplierTypes();

                const prof = await loadMyProfile();

                const nextStage = deriveStageFromProfile(prof);
                setStage(nextStage);

                // فقط وقتی وارد KYC/Status هستیم، KYC را لود کن
                if (nextStage === "kyc" || nextStage === "status") {
                    await loadKyc(prof?.supplierTypeId, false);
                }

                setLoading(false);
            } catch {
                message.error("خطا در بارگذاری اطلاعات");
                setStage("type");
                setLoading(false);
            }
        })();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // --------------------------------------------------
    // Save Profile (Draft) => next stage from backend
    // --------------------------------------------------
    const handleSaveProfile = async (values) => {
        try {
            setSaving(true);

            const payload = {
                ...values,
                supplierTypeId: selectedSupplierTypeId,
            };

            await supplierOnboardingApi.upsertMyProfile(payload);

            // 👈 این خط کلید حل باگه
            const prof = await loadMyProfile();
            message.success("اطلاعات ذخیره شد");

            // derive stage after save (do NOT force)
            const nextStage = deriveStageFromProfile(prof);
            setStage(nextStage);

            // اگر رفتیم به KYC یا Status، لیست‌ها رو تازه کن
            if (nextStage === "kyc" || nextStage === "status") {
                await loadKyc(prof?.supplierTypeId, false);
            }
        } catch (e) {
            message.error(e?.message || "خطا در ذخیره اطلاعات");
        } finally {
            setSaving(false);
        }
    };

    // --------------------------------------------------
    // Download persisted file with token
    // --------------------------------------------------
    const downloadPersistedFile = async (documentId, filename) => {
        try {
            const token = getAccessToken();

            const res = await fetch(`${apiBaseUrl}/api/v1/kyc/documents/${documentId}/file`, {
                method: "GET",
                headers: {
                    ...(token ? { Authorization: `Bearer ${token}` } : {}),
                },
            });

            if (!res.ok) {
                throw new Error("دانلود فایل ممکن نیست.");
            }

            const blob = await res.blob();
            const url = window.URL.createObjectURL(blob);

            const a = document.createElement("a");
            a.href = url;
            a.download = filename || "document";
            document.body.appendChild(a);
            a.click();
            a.remove();

            window.URL.revokeObjectURL(url);
        } catch (e) {
            message.error(e?.message || "خطا در دانلود فایل");
        }
    };

    // --------------------------------------------------
    // Upload KYC file (XHR progress)
    // --------------------------------------------------
    const uploadKycFileWithProgress = async (attributeId, file) => {
        const token = getAccessToken();

        const isImage = file?.type?.startsWith("image/");
        const localPreviewUrl = isImage ? URL.createObjectURL(file) : null;

        // set uploading state
        setDraftByAttrId((prev) => ({
            ...(prev || {}),
            [attributeId]: {
                ...(prev?.[attributeId] || {}),
                uploading: true,
                uploadProgress: 0,
                // draft file info BEFORE upload
                fileDraft: {
                    filePath: null,
                    fileName: file.name,
                    mimeType: file.type,
                    size: file.size,
                },
                localPreviewUrl,
            },
        }));

        return new Promise((resolve) => {
            const xhr = new XMLHttpRequest();
            xhr.open("POST", `${apiBaseUrl}/api/v1/kyc/my/upload-file`, true);
            if (token) xhr.setRequestHeader("Authorization", `Bearer ${token}`);

            xhr.upload.onprogress = (evt) => {
                if (!evt.lengthComputable) return;
                const percent = Math.round((evt.loaded / evt.total) * 100);
                setDraftByAttrId((prev) => ({
                    ...(prev || {}),
                    [attributeId]: {
                        ...(prev?.[attributeId] || {}),
                        uploadProgress: percent,
                    },
                }));
            };

            xhr.onload = () => {
                try {
                    const json = xhr.responseText ? JSON.parse(xhr.responseText) : null;

                    if (xhr.status < 200 || xhr.status >= 300) {
                        const msg = json?.message || "خطا در آپلود فایل";
                        throw new Error(msg);
                    }

                    if (json && typeof json === "object" && "success" in json && json.success === false) {
                        throw new Error(json.message || "آپلود ناموفق بود.");
                    }

                    const data = unwrapApiResult(json);
                    const filePath = data?.FilePath || data?.filePath;

                    if (!filePath) throw new Error("مسیر فایل از سرور دریافت نشد.");

                    setDraftByAttrId((prev) => ({
                        ...(prev || {}),
                        [attributeId]: {
                            ...(prev?.[attributeId] || {}),
                            uploading: false,
                            uploadProgress: 100,
                            fileDraft: {
                                ...(prev?.[attributeId]?.fileDraft || {}),
                                filePath,
                                // تصمیم شما: filename را در Value نگه می‌داریم
                                fileName: prev?.[attributeId]?.fileDraft?.fileName || file.name,
                                mimeType: file.type,
                                size: file.size,
                            },
                        },
                    }));

                    message.success("فایل آپلود شد (در انتظار ارسال)");
                    resolve(false);
                } catch (e) {
                    setDraftByAttrId((prev) => ({
                        ...(prev || {}),
                        [attributeId]: {
                            ...(prev?.[attributeId] || {}),
                            uploading: false,
                        },
                    }));
                    message.error(e?.message || "خطا در آپلود فایل");
                    resolve(false);
                }
            };

            xhr.onerror = () => {
                setDraftByAttrId((prev) => ({
                    ...(prev || {}),
                    [attributeId]: {
                        ...(prev?.[attributeId] || {}),
                        uploading: false,
                    },
                }));
                message.error("خطا در آپلود فایل");
                resolve(false);
            };

            const fd = new FormData();
            fd.append("file", file);
            xhr.send(fd);
        });
    };

    // --------------------------------------------------
    // UI Validation before submit
    // --------------------------------------------------
    const validateKycBeforeSubmit = () => {
        const errors = [];

        (kycRequirements || []).forEach((r) => {
            if (!r.isRequired) return;

            const attrId = r.attributeDefinitionId;
            const dt = getDataTypeNumber(r.dataType);
            const doc = getDocumentByAttributeId(attrId);
            const draft = draftByAttrId?.[attrId] || {};

            if (dt === 1) {
                const hasPersisted = Boolean(doc?.filePath);
                const hasDraft = Boolean(draft?.fileDraft?.filePath);
                if (!hasPersisted && !hasDraft) {
                    errors.push(`آپلود فایل برای «${r.attributeDisplayName}» اجباری است.`);
                }
            } else {
                const persistedVal = doc?.value ?? null;
                const draftVal = draft?.valueDraft ?? null;

                const hasPersisted =
                    persistedVal !== null &&
                    persistedVal !== undefined &&
                    String(persistedVal).trim() !== "";

                const hasDraft =
                    draftVal !== null &&
                    draftVal !== undefined &&
                    String(draftVal).trim() !== "";

                if (!hasPersisted && !hasDraft) {
                    errors.push(`وارد کردن مقدار برای «${r.attributeDisplayName}» اجباری است.`);
                }
            }
        });

        return errors;
    };

    // --------------------------------------------------
    // Submit KYC
    // --------------------------------------------------
    const handleSubmitKyc = async () => {
        if (isAnyUploading) {
            message.warning("لطفاً منتظر بمانید تا آپلود فایل‌ها کامل شود.");
            return;
        }

        const errors = validateKycBeforeSubmit();
        if (errors.length) {
            message.error(errors[0]);
            return;
        }

        try {
            setSubmittingKyc(true);

            const payload = (kycRequirements || []).map((r) => {
                const attrId = r.attributeDefinitionId;
                const dt = getDataTypeNumber(r.dataType);

                const doc = getDocumentByAttributeId(attrId);
                const draft = draftByAttrId?.[attrId] || {};

                if (dt === 1) {
                    const filePath = draft?.fileDraft?.filePath || doc?.filePath || null;
                    const fileName = draft?.fileDraft?.fileName || doc?.value || null;

                    return {
                        attributeDefinitionId: attrId,
                        dataType: r.dataType,
                        value: fileName,   // filename in Value
                        filePath,
                    };
                }

                const value = (draft?.valueDraft ?? doc?.value ?? null);

                return {
                    attributeDefinitionId: attrId,
                    dataType: r.dataType,
                    value: value,
                    filePath: null,
                };
            });

            await apiFetchJson("/api/v1/kyc/my/submit", {
                method: "POST",
                body: JSON.stringify(payload),
            });

            message.success("مدارک با موفقیت ارسال شد");

            // پس از submit: پروفایل را مجدد بگیر تا verificationStatus آپدیت شود
            const prof = await loadMyProfile();

            // ⬅️ اگر بک‌اند هنوز status جدید را نداد، خودمان Stage را جلو می‌بریم
            setStage(deriveStageFromProfile(prof));

            // refresh KYC lists (بدون overwrite stage)
            await loadKyc(prof?.supplierTypeId, false);


            // پاکسازی draft (UX روشن‌تر)
            clearKycDraftLocal();
        } catch (e) {
            message.error(e?.message || "خطا در ارسال مدارک");
        } finally {
            setSubmittingKyc(false);
        }
    };

    // --------------------------------------------------
    // Stage Header
    // --------------------------------------------------
    const stageInfo = useMemo(() => {
        if (stage === "type")
            return { step: 1, title: "انتخاب نوع تأمین‌کننده" };
        if (stage === "profile")
            return { step: 2, title: "تکمیل اطلاعات پایه کسب‌وکار" };
        if (stage === "kyc")
            return { step: 3, title: "ارسال مدارک احراز هویت" };
        return { step: 4, title: "وضعیت بررسی مدارک" };
    }, [stage]);

    const overallDocStatus = useMemo(() => {
        const docs = kycDocuments || [];
        if (!docs.length) return "NotSubmitted";
        if (docs.some((d) => d.status === "Rejected")) return "Rejected";
        if (docs.some((d) => d.status === "Pending")) return "Pending";
        if (docs.every((d) => d.status === "Approved")) return "Approved";
        return "NotSubmitted";
    }, [kycDocuments]);

    const renderOverallKycAlert = () => {
        if (overallDocStatus === "Approved") {
            return <Alert type="success" message="مدارک شما تأیید شده است." showIcon />;
        }
        if (overallDocStatus === "Pending") {
            return <Alert type="info" message="مدارک شما ارسال شده و در انتظار بررسی است." showIcon />;
        }
        if (overallDocStatus === "Rejected") {
            return (
                <Alert
                    type="warning"
                    message="برخی مدارک رد شده است. موارد را اصلاح و مجدداً ارسال کنید."
                    showIcon
                />
            );
        }
        return <Alert type="info" message="لطفاً مدارک احراز هویت را تکمیل و ارسال کنید." showIcon />;
    };

    const renderDocStatusTag = (status) => {
        if (status === null || status === undefined) return null;

        let color = "default";

        if (status === 0 || status === "Pending") color = "orange";
        if (status === 1 || status === "Approved") color = "green";
        if (status === 2 || status === "Rejected") color = "red";

        return <Tag color={color}>{statusLabel(status)}</Tag>;
    };


    // --------------------------------------------------
    // SupplierType change with confirm
    // --------------------------------------------------
    const handleSupplierTypeChange = async (nextTypeId) => {
        const prevTypeId = selectedSupplierTypeId;
        
        // first selection => no confirm
        if (!prevTypeId || prevTypeId === nextTypeId) {
            setSelectedSupplierTypeId(nextTypeId);
            return;
        }

        // 👈 برای اینکه UI قفل نباشه، انتخاب جدید رو موقتاً نشون می‌دیم
        setSelectedSupplierTypeId(nextTypeId);

        modal.confirm({
            title: "تغییر نوع تأمین‌کننده",
            content:
                "با تغییر نوع تأمین‌کننده، مدارک قبلی شما حذف می‌شود و باید مجدداً ارسال شوند. آیا مطمئن هستید؟",
            okText: "بله، تغییر بده",
            cancelText: "انصراف",
            onOk: async () => {
                try {
                    setSaving(true);

                    // فقط تغییر لوکال
                    setSelectedSupplierTypeId(nextTypeId);

                    // ریست KYC در UI
                    setKycDocuments([]);
                    setKycRequirements([]);
                    clearKycDraftLocal();

                    message.success("نوع تأمین‌کننده تغییر کرد. لطفاً اطلاعات را ذخیره کنید.");

                    // برگرد به مرحله پروفایل
                    setStage("profile");
                } finally {
                    setSaving(false);
                }
            },
            onCancel: () => {
                setSelectedSupplierTypeId(prevTypeId);
            },
        });

    };


    // --------------------------------------------------
    // Render KYC Field UI (File vs Value) - unchanged except province/city not here
    // --------------------------------------------------
    const renderKycField = (r) => {
        const attrId = r.attributeDefinitionId;
        const dt = getDataTypeNumber(r.dataType);

        const doc = getDocumentByAttributeId(attrId);
        const draft = draftByAttrId?.[attrId] || {};

        const persistedFileName = doc?.value || null;      // filename in Value
        const persistedFilePath = doc?.filePath || null;
        const persistedValue = doc?.value || null;         // for non-file too

        const hasDraftFile = Boolean(draft?.fileDraft?.filePath);
        const hasDraftValue =
            draft?.valueDraft !== null &&
            draft?.valueDraft !== undefined &&
            String(draft?.valueDraft).trim() !== "";

        const isUploading = Boolean(draft?.uploading);
        const isImageDraft = Boolean(draft?.localPreviewUrl);

        const hasPersistedFile = Boolean(persistedFilePath);
        const hasPersistedValue =
            persistedValue !== null &&
            persistedValue !== undefined &&
            String(persistedValue).trim() !== "";

        const showDraftBadge = (dt === 1 ? hasDraftFile : hasDraftValue);

        const requiredTag = r.isRequired ? <Tag color="red">اجباری</Tag> : null;

        if (dt === 1) {
            return (
                <Space direction="vertical" style={{ width: "100%" }}>
                    {hasPersistedFile && (
                        <div style={{ display: "flex", justifyContent: "space-between", gap: 12 }}>
                            <div style={{ minWidth: 0 }}>
                                <Text type="secondary">فایل ثبت‌شده در سیستم:</Text>
                                <div style={{ whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>
                                    <Text>{persistedFileName || "فایل"}</Text>
                                </div>
                            </div>

                            <Space>
                                <Button
                                    icon={<DownloadOutlined />}
                                    onClick={() => downloadPersistedFile(doc.id, persistedFileName || "document")}
                                >
                                    دانلود
                                </Button>
                            </Space>
                        </div>
                    )}

                    <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "center" }}>
                        <Space direction="vertical" style={{ flex: 1, minWidth: 0 }}>
                            <Space>
                                <Text strong>{r.attributeDisplayName}</Text>
                                {requiredTag}
                                {renderDocStatusTag(doc?.status)}
                                {showDraftBadge && <Tag color="gold">در انتظار ارسال</Tag>}
                            </Space>

                            {r.description && <Text type="secondary">{r.description}</Text>}

                            {draft?.fileDraft?.fileName && (
                                <Text type="secondary" style={{ whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>
                                    فایل انتخاب‌شده: {draft.fileDraft.fileName}
                                </Text>
                            )}

                            {isUploading && (
                                <Progress percent={draft?.uploadProgress || 0} />
                            )}
                        </Space>

                        <Space direction="vertical" align="end">
                            <Upload
                                beforeUpload={(file) => uploadKycFileWithProgress(attrId, file)}
                                showUploadList={false}
                            >
                                <Button icon={<UploadOutlined />} disabled={isUploading}>
                                    انتخاب فایل
                                </Button>
                            </Upload>

                            {isImageDraft && (
                                <Button
                                    icon={<EyeOutlined />}
                                    onClick={() =>
                                        setPreview({
                                            open: true,
                                            title: draft?.fileDraft?.fileName || r.attributeDisplayName,
                                            url: draft.localPreviewUrl,
                                        })
                                    }
                                >
                                    پیش‌نمایش
                                </Button>
                            )}
                        </Space>
                    </div>

                    {doc?.adminNote && (
                        <Alert type="warning" message={doc.adminNote} showIcon />
                    )}
                </Space>
            );
        }

        const showPersistedLine = hasPersistedValue && !showDraftBadge;

        if (dt === 2) {
            return (
                <Space direction="vertical" style={{ width: "100%" }}>
                    <Space>
                        <Text strong>{r.attributeDisplayName}</Text>
                        {requiredTag}
                        {renderDocStatusTag(doc?.status)}
                        {showDraftBadge && <Tag color="gold">در انتظار ارسال</Tag>}
                    </Space>

                    {r.description && <Text type="secondary">{r.description}</Text>}

                    {showPersistedLine && (
                        <Text type="secondary">مقدار ثبت‌شده: {String(persistedValue)}</Text>
                    )}

                    <Input
                        value={draft?.valueDraft ?? ""}
                        onChange={(e) =>
                            setDraftByAttrId((prev) => ({
                                ...(prev || {}),
                                [attrId]: {
                                    ...(prev?.[attrId] || {}),
                                    valueDraft: e.target.value,
                                },
                            }))
                        }
                    />

                    {doc?.adminNote && (
                        <Alert type="warning" message={doc.adminNote} showIcon />
                    )}
                </Space>
            );
        }

        if (dt === 3) {
            return (
                <Space direction="vertical" style={{ width: "100%" }}>
                    <Space>
                        <Text strong>{r.attributeDisplayName}</Text>
                        {requiredTag}
                        {renderDocStatusTag(doc?.status)}
                        {showDraftBadge && <Tag color="gold">در انتظار ارسال</Tag>}
                    </Space>

                    {r.description && <Text type="secondary">{r.description}</Text>}

                    {showPersistedLine && (
                        <Text type="secondary">مقدار ثبت‌شده: {String(persistedValue)}</Text>
                    )}

                    <Input
                        type="number"
                        value={draft?.valueDraft ?? ""}
                        onChange={(e) =>
                            setDraftByAttrId((prev) => ({
                                ...(prev || {}),
                                [attrId]: {
                                    ...(prev?.[attrId] || {}),
                                    valueDraft: e.target.value,
                                },
                            }))
                        }
                    />

                    {doc?.adminNote && (
                        <Alert type="warning" message={doc.adminNote} showIcon />
                    )}
                </Space>
            );
        }

        if (dt === 4) {
            return (
                <Space direction="vertical" style={{ width: "100%" }}>
                    <Space>
                        <Text strong>{r.attributeDisplayName}</Text>
                        {requiredTag}
                        {renderDocStatusTag(doc?.status)}
                        {showDraftBadge && <Tag color="gold">در انتظار ارسال</Tag>}
                    </Space>

                    {r.description && <Text type="secondary">{r.description}</Text>}

                    {showPersistedLine && (
                        <Text type="secondary">مقدار ثبت‌شده: {String(persistedValue)}</Text>
                    )}

                    <Radio.Group
                        value={draft?.valueDraft ?? null}
                        onChange={(e) =>
                            setDraftByAttrId((prev) => ({
                                ...(prev || {}),
                                [attrId]: {
                                    ...(prev?.[attrId] || {}),
                                    valueDraft: e.target.value,
                                },
                            }))
                        }
                    >
                        <Radio value="true">بله</Radio>
                        <Radio value="false">خیر</Radio>
                    </Radio.Group>

                    {doc?.adminNote && (
                        <Alert type="warning" message={doc.adminNote} showIcon />
                    )}
                </Space>
            );
        }

        if (dt === 5) {
            return (
                <Space direction="vertical" style={{ width: "100%" }}>
                    <Space>
                        <Text strong>{r.attributeDisplayName}</Text>
                        {requiredTag}
                        {renderDocStatusTag(doc?.status)}
                        {showDraftBadge && <Tag color="gold">در انتظار ارسال</Tag>}
                    </Space>

                    {r.description && <Text type="secondary">{r.description}</Text>}

                    {showPersistedLine && (
                        <Text type="secondary">مقدار ثبت‌شده: {String(persistedValue)}</Text>
                    )}

                    <Input
                        value={draft?.valueDraft ?? ""}
                        onChange={(e) =>
                            setDraftByAttrId((prev) => ({
                                ...(prev || {}),
                                [attrId]: {
                                    ...(prev?.[attrId] || {}),
                                    valueDraft: e.target.value,
                                },
                            }))
                        }
                        placeholder="مقدار را وارد کنید"
                    />

                    {doc?.adminNote && (
                        <Alert type="warning" message={doc.adminNote} showIcon />
                    )}
                </Space>
            );
        }

        return (
            <Space direction="vertical" style={{ width: "100%" }}>
                <Space>
                    <Text strong>{r.attributeDisplayName}</Text>
                    {requiredTag}
                </Space>
                <Text type="secondary">نوع داده پشتیبانی نمی‌شود</Text>
            </Space>
        );
    };

    // --------------------------------------------------
    // Guards
    // --------------------------------------------------
    //const canGoToProfile = Boolean(selectedSupplierTypeId);
    const canGoToKyc = Boolean(profile?.supplierTypeId || selectedSupplierTypeId);

    if (loading) {
        return (
            <div style={{ textAlign: "center", padding: 48 }}>
                <Spin />
            </div>
        );
    }


    return (
        <Card title="درخواست همکاری به عنوان تأمین‌کننده">
            {/* Preview Modal */}
            <Modal
                open={preview.open}
                title={preview.title}
                footer={null}
                onCancel={() => setPreview({ open: false, title: "", url: "" })}
            >
                <Image alt={preview.title} src={preview.url} style={{ width: "100%" }} />
            </Modal>

            {/* Stage Header */}
            <div style={{ marginBottom: 24 }}>
                <strong>مرحله {stageInfo.step} از 4</strong>
                <div style={{ color: "#888" }}>{stageInfo.title}</div>
            </div>

            <Divider />

            {/* =============================
                Stage 1: Supplier Type
            ============================== */}
            {stage === "type" && (
                <Space direction="vertical" size="large" style={{ width: "100%" }}>
                    <p>لطفاً نوع تأمین‌کننده خود را انتخاب کنید:</p>

                    <Radio.Group
                        value={selectedSupplierTypeId}
                        onChange={(e) => handleSupplierTypeChange(e.target.value)}
                    >
                        <Space direction="vertical">
                            {supplierTypes.map((t) => (
                                <Radio key={t.id} value={t.id}>
                                    <strong>{t.name}</strong>
                                    <div style={{ color: "#888" }}>{t.description}</div>
                                </Radio>
                            ))}
                        </Space>
                    </Radio.Group>

                    <Divider />

                    <Button
                        type="primary"
                        disabled={!selectedSupplierTypeId}
                        onClick={() => setStage("profile")}
                    >
                        ادامه
                    </Button>
                </Space>
            )}

            {/* =============================
                Stage 2: Profile (Draft)
            ============================== */}
            {stage === "profile" && (
                <Form form={form} layout="vertical" onFinish={handleSaveProfile}>
                    <Row gutter={16}>
                        <Col span={12}>
                            <Form.Item
                                label="نام کسب‌وکار"
                                name="businessName"
                                rules={[{ required: true, message: "نام کسب‌وکار الزامی است" }]}
                            >
                                <Input />
                            </Form.Item>
                        </Col>

                        <Col span={12}>
                            <Form.Item label="کد ملی" name="nationalId">
                                <Input />
                            </Form.Item>
                        </Col>

                        <Col span={12}>
                            <Form.Item label="شماره مجوز" name="licenseNumber">
                                <Input />
                            </Form.Item>
                        </Col>

                        <Col span={12}>
                            <Form.Item
                                label="استان"
                                name="province"
                                rules={[{ required: true, message: "استان الزامی است" }]}
                            >
                                <Select
                                    placeholder="انتخاب استان"
                                    onChange={(val) => {
                                        setSelectedProvince(val);
                                        form.setFieldsValue({ city: null });
                                    }}
                                    allowClear
                                >
                                    {provinces.map((p) => (
                                        <Option key={p} value={p}>
                                            {p}
                                        </Option>
                                    ))}
                                </Select>
                            </Form.Item>
                        </Col>

                        <Col span={12}>
                            <Form.Item
                                label="شهر"
                                name="city"
                                rules={[{ required: true, message: "شهر الزامی است" }]}
                            >
                                <Select
                                    placeholder="انتخاب شهر"
                                    disabled={!selectedProvince}
                                    allowClear
                                >
                                    {cities.map((c) => (
                                        <Option key={c} value={c}>
                                            {c}
                                        </Option>
                                    ))}
                                </Select>
                            </Form.Item>
                        </Col>

                        <Col span={24}>
                            <Form.Item label="آدرس" name="address">
                                <Input.TextArea rows={3} />
                            </Form.Item>
                        </Col>

                        <Col span={12}>
                            <Form.Item label="نام رابط" name="contactName">
                                <Input />
                            </Form.Item>
                        </Col>

                        <Col span={12}>
                            <Form.Item
                                label="شماره تماس"
                                name="contactPhone"
                                rules={[{ required: true, message: "شماره تماس الزامی است" }]}
                            >
                                <Input />
                            </Form.Item>
                        </Col>
                    </Row>

                    <Divider />

                    <Space>
                        <Button onClick={() => setStage("type")}>بازگشت</Button>

                        <Button type="primary" htmlType="submit" loading={saving}>
                            ذخیره و ادامه
                        </Button>
                    </Space>
                </Form>
            )}

            {/* =============================
                Stage 3: KYC
            ============================== */}
            {stage === "kyc" && (
                <Space direction="vertical" size="large" style={{ width: "100%" }}>
                    {!canGoToKyc && (
                        <Alert
                            type="warning"
                            message="برای ارسال مدارک ابتدا باید پروفایل تأمین‌کننده را تکمیل و ذخیره کنید."
                            showIcon
                        />
                    )}

                    {kycLoading ? (
                        <div style={{ textAlign: "center", padding: 24 }}>
                            <Spin />
                        </div>
                    ) : (
                        <>
                            {renderOverallKycAlert()}

                            {kycRequirements.length === 0 && (
                                <Alert
                                    type="info"
                                    message="برای نوع تأمین‌کننده شما مدرکی تعریف نشده است."
                                    showIcon
                                />
                            )}

                            {(kycRequirements || []).map((r) => (
                                <Card key={r.attributeDefinitionId} size="small">
                                    {renderKycField(r)}
                                </Card>
                            ))}

                            <Divider />

                            <Space>
                                <Button
                                    onClick={() => {
                                        setStage("profile");
                                    }}
                                >
                                    بازگشت
                                </Button>

                                <Button
                                    type="primary"
                                    loading={submittingKyc}
                                    onClick={handleSubmitKyc}
                                    disabled={
                                        kycRequirements.length === 0 ||
                                        !canGoToKyc ||
                                        isAnyUploading
                                    }
                                >
                                    ارسال مدارک
                                </Button>
                            </Space>
                        </>
                    )}
                </Space>
            )}

            {/* =============================
                Stage 4: Status
            ============================== */}
            {stage === "status" && (
                <Space direction="vertical" size="large" style={{ width: "100%" }}>
                    {kycLoading ? (
                        <div style={{ textAlign: "center", padding: 24 }}>
                            <Spin />
                        </div>
                    ) : (
                        <>

                            


                            {/* در Stage 4 دیگر پیام "مدارک را ارسال کنید" نداریم؛ فقط وضعیت */}
                            {profile?.verificationStatus === "Pending" && (
                                <Alert type="warning" message="مدارک شما ارسال شده و در انتظار بررسی است." showIcon />
                            )}

                            {profile?.verificationStatus === "Approved" && (
                                <Alert type="success" message="تبریک! حساب تأمین‌کننده شما تأیید شد." showIcon />
                            )}

                            {profile?.verificationStatus === "Rejected" && (
                                <Alert
                                    type="error"
                                    message="مدارک شما رد شده است."
                                    description={profile?.rejectionReason || "لطفاً مدارک را اصلاح و مجدداً ارسال کنید."}
                                    showIcon
                                />
                            )}

                                <Card title="خلاصه اطلاعات تأمین‌کننده" style={{ marginBottom: 24 }}>
                                    <Row gutter={[16, 16]}>
                                        <Col span={12}>
                                            <Text type="secondary">نوع تأمین‌کننده</Text>
                                            <div>
                                                <Text strong>
                                                    {
                                                        supplierTypes.find(
                                                            t => t.id === profile?.supplierTypeId
                                                        )?.name || "-"
                                                    }
                                                </Text>
                                            </div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">نام کسب‌وکار</Text>
                                            <div>
                                                <Text strong>{profile?.businessName || "-"}</Text>
                                            </div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">استان</Text>
                                            <div>
                                                <Text strong>{profile?.province || "-"}</Text>
                                            </div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">شهر</Text>
                                            <div>
                                                <Text strong>{profile?.city || "-"}</Text>
                                            </div>
                                        </Col>

                                        <Col span={12}>
                                            <Text type="secondary">شماره تماس</Text>
                                            <div>
                                                <Text strong>{profile?.contactPhone || "-"}</Text>
                                            </div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">کد ملی</Text>
                                            <div><Text strong>{profile?.nationalId || "-"}</Text></div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">شماره مجوز</Text>
                                            <div><Text strong>{profile?.licenseNumber || "-"}</Text></div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">نام رابط</Text>
                                            <div><Text strong>{profile?.contactName || "-"}</Text></div>
                                        </Col>
                                        <Col span={24}>
                                            <Text type="secondary">آدرس</Text>
                                            <div><Text strong>{profile?.address || "-"}</Text></div>
                                        </Col>
                                    </Row>
                                </Card>

                            {(kycDocuments || []).length === 0 ? (
                                <Alert type="info" message="هنوز مدرکی ارسال نشده است." showIcon />
                            ) : (
                                (kycDocuments || []).map((d) => (
                                    <Card key={d.id} size="small">
                                        <Space direction="vertical" style={{ width: "100%" }}>
                                            <Space>
                                                <strong>{d.attributeDisplayName}</strong>
                                                {renderDocStatusTag(d.status)}
                                            </Space>

                                            {d.adminNote && (
                                                <Alert type="warning" message={d.adminNote} showIcon />
                                            )}

                                            {d.filePath ? (
                                                <Space>
                                                    <Text type="secondary">
                                                        فایل: {d.value || "فایل"}
                                                    </Text>
                                                    <Button
                                                        icon={<DownloadOutlined />}
                                                        onClick={() => downloadPersistedFile(d.id, d.value || "document")}
                                                    >
                                                        دانلود
                                                    </Button>
                                                </Space>
                                            ) : (
                                                <Text type="secondary">
                                                    مقدار: {d.value ?? "-"}
                                                </Text>
                                            )}
                                        </Space>
                                    </Card>
                                ))
                            )}

                            <Divider />

                            <Space>
                                {/* ویرایش فقط وقتی کلی رد شده */}
                                {profile?.verificationStatus === "Rejected" && (
                                    <Button type="primary" onClick={() => setStage("profile")}>
                                        ویرایش و ارسال مجدد
                                    </Button>
                                )}
                            </Space>
                        </>
                    )}
                </Space>
            )}
        </Card>
    );
};

export default SupplierOnboardingPage;
