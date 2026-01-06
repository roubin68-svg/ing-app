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
import { UploadOutlined, DownloadOutlined, EyeOutlined, FileOutlined, FilePdfOutlined, FileWordOutlined, CheckCircleOutlined, DeleteOutlined } from "@ant-design/icons";

import supplierOnboardingApi from "../api/supplierOnboardingApi";
import supplierTypesApi from "../../supplierTypes/api/supplierTypesApi";
import { getProvinces, getCitiesByProvince } from "../../../core/location/iranProvinces";
import apiClient from "../../../core/api/apiClient";

const { Text } = Typography;
const { Option } = Select;

// Helper functions for BusinessType enum
const getBusinessTypeLabel = (value) => {
    if (value === 1 || value === "Natural" || value === "1") return "حقیقی";
    if (value === 2 || value === "Legal" || value === "2") return "حقوقی";
    return value || "-";
};

const getBusinessTypeValue = (label) => {
    if (label === "حقیقی") return 1;
    if (label === "حقوقی") return 2;
    return label;
};

// Helper functions for ContactPosition enum
const getContactPositionLabel = (value) => {
    if (value === 1 || value === "PurchaseManager" || value === "1") return "مسئول خرید";
    if (value === 2 || value === "CEO" || value === "2") return "مدیر عامل";
    return value || "-";
};

const getContactPositionValue = (label) => {
    if (label === "مسئول خرید") return 1;
    if (label === "مدیر عامل") return 2;
    return label;
};

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
    const [imageBlobUrls, setImageBlobUrls] = useState({}); // برای نگه‌داری blob URLs تصاویر persist شده: { documentId: blobUrl }

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
        // استفاده از apiClient.defaults.baseURL و حذف /api/v1 برای base URL
        const baseURL = apiClient.defaults.baseURL?.replace("/api/v1", "") || "";
        return baseURL;
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
        try {
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
                businessType: res.businessType, // enum value (1 or 2)
                contactName: res.contactName,
                contactPosition: res.contactPosition, // enum value (1 or 2)
                contactMobile: res.contactMobile,
                contactPhone: res.contactPhone,
            });

            setSelectedProvince(res.province || null);
        }

        return res;
        } catch (error) {
            // اگر 404 بود (پروفایل یافت نشد)، این طبیعی است و null برمی‌گردانیم
            if (error?.response?.status === 404 || error?.status === 404) {
                setProfile(null);
                return null;
            }
            // برای خطاهای دیگر، دوباره throw می‌کنیم
            throw error;
        }
    }, [form, selectedSupplierTypeId]);

    // --------------------------------------------------
    // Load KYC (requirements + documents)
    // --------------------------------------------------
    const loadKyc = useCallback(
        async (supplierTypeId, shouldUpdateStage = true) => {
            if (!supplierTypeId) return { requirements: [], documents: [] };


            setKycLoading(true);
            try {
                const reqRes = await apiClient.get("/kyc/my/requirements");
                const requirements = reqRes.data || [];
                setKycRequirements(requirements);

                const docRes = await apiClient.get("/kyc/my/documents");
                const documents = docRes.data || [];
                setKycDocuments(documents);

                // ساخت blob URLs برای تصاویر persist شده
                const token = getAccessToken();
                const blobUrlPromises = documents
                    .filter(d => d.filePath && d.value)
                    .map(async (d) => {
                        const fileName = (d.value || "").toLowerCase();
                        const isImage = /\.(jpg|jpeg|png|gif|webp|bmp)$/i.test(fileName);
                        if (!isImage) return null;

                        try {
                            const res = await fetch(`${apiClient.defaults.baseURL}/kyc/documents/${d.id}/file`, {
                                method: "GET",
                                headers: {
                                    ...(token ? { Authorization: `Bearer ${token}` } : {}),
                                },
                            });
                            if (!res.ok) return null;
                            const blob = await res.blob();
                            const blobUrl = window.URL.createObjectURL(blob);
                            return { documentId: d.id, blobUrl };
                        } catch {
                            return null;
                        }
                    });

                const blobUrlResults = await Promise.all(blobUrlPromises);
                const newBlobUrls = {};
                blobUrlResults.forEach(result => {
                    if (result) {
                        newBlobUrls[result.documentId] = result.blobUrl;
                    }
                });
                setImageBlobUrls(prev => {
                    // Revoke old URLs
                    Object.values(prev).forEach(url => {
                        if (url && typeof url === 'string') {
                            window.URL.revokeObjectURL(url);
                        }
                    });
                    return newBlobUrls;
                });

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
        [message, deriveStageFromProfile, profile, getAccessToken]
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

    // Cleanup: Revoke blob URLs when component unmounts
    useEffect(() => {
        return () => {
            Object.values(imageBlobUrls).forEach(url => {
                if (url && typeof url === 'string') {
                    window.URL.revokeObjectURL(url);
                }
            });
        };
    }, [imageBlobUrls]);

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

            const res = await fetch(`${apiClient.defaults.baseURL}/kyc/documents/${documentId}/file`, {
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
            // استفاده از baseURL از apiClient
            const uploadUrl = `${apiClient.defaults.baseURL}/kyc/my/upload-file`;
            xhr.open("POST", uploadUrl, true);
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

            await apiClient.post("/kyc/my/submit", payload);

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

        const hasPersistedFile = Boolean(persistedFilePath);
        const hasPersistedValue =
            persistedValue !== null &&
            persistedValue !== undefined &&
            String(persistedValue).trim() !== "";

        const showDraftBadge = (dt === 1 ? hasDraftFile : hasDraftValue);

        const requiredTag = r.isRequired ? <Tag color="red">اجباری</Tag> : null;

        if (dt === 1) {
            return (
                <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
                    {/* Header: نام فیلد و تگ‌ها */}
                    <Space>
                        <Text strong>{r.attributeDisplayName}</Text>
                        {requiredTag}
                        {renderDocStatusTag(doc?.status)}
                        {showDraftBadge && <Tag color="gold">در انتظار ارسال</Tag>}
                    </Space>

                    {r.description && <Text type="secondary">{r.description}</Text>}

                    {/* فایل ثبت‌شده (persisted) */}
                    {hasPersistedFile && !draft?.fileDraft && (
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
                                    const fileName = persistedFileName?.toLowerCase() || "";
                                    const isImage = /\.(jpg|jpeg|png|gif|webp|bmp)$/i.test(fileName);
                                    const isPdf = fileName.endsWith(".pdf");
                                    const isWord = /\.(doc|docx)$/i.test(fileName);
                                    
                                    if (isImage && persistedFilePath) {
                                        const blobUrl = imageBlobUrls[doc.id];
                                        if (blobUrl) {
                                            return (
                                                <Image
                                                    src={blobUrl}
                                                    alt={persistedFileName}
                                                    width={40}
                                                    height={40}
                                                    style={{
                                                        objectFit: "cover",
                                                        borderRadius: 4,
                                                        border: "1px solid #d9d9d9",
                                                    }}
                                                    preview={{
                                                        src: blobUrl
                                                    }}
                                                />
                                            );
                                        }
                                        // اگر blob URL هنوز آماده نیست، یک placeholder نمایش بده
                                        return (
                                            <div style={{
                                                width: 40,
                                                height: 40,
                                                display: "flex",
                                                alignItems: "center",
                                                justifyContent: "center",
                                                background: "#f0f0f0",
                                                borderRadius: 4,
                                                border: "1px solid #d9d9d9"
                                            }}>
                                                <Spin size="small" />
                                </div>
                                        );
                                    }
                                    if (isPdf) return <FilePdfOutlined style={{ fontSize: 24, color: "#f5222d" }} />;
                                    if (isWord) return <FileWordOutlined style={{ fontSize: 24, color: "#1890ff" }} />;
                                    return <FileOutlined style={{ fontSize: 24, color: "#666" }} />;
                                })()}
                                <div>
                                    <div style={{ fontWeight: 500, fontSize: 13 }}>{persistedFileName || "فایل"}</div>
                                    <div style={{ fontSize: 11, color: "#666" }}>فایل ثبت‌شده</div>
                            </div>
                            </Space>
                            <Space>
                                <Button
                                    size="small"
                                    icon={<DownloadOutlined />}
                                    onClick={() => downloadPersistedFile(doc.id, persistedFileName || "document")}
                                >
                                    دانلود
                                </Button>
                            </Space>
                        </div>
                    )}

                    {/* فایل جدید (draft) - فقط اگر فایل persist نشده */}
                    {draft?.fileDraft && !hasPersistedFile && (
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
                                        {(() => {
                                            // برای draft files، از localPreviewUrl استفاده کن
                                            if (draft.localPreviewUrl) {
                                                return (
                                                    <Image
                                                        src={draft.localPreviewUrl}
                                                        alt="preview"
                                                        width={50}
                                                        height={50}
                                                        style={{
                                                            objectFit: "cover",
                                                            borderRadius: 4,
                                                            border: "1px solid #d9d9d9",
                                                        }}
                                                        preview={{
                                                            src: draft.localPreviewUrl
                                                        }}
                                                    />
                                                );
                                            }
                                            // در غیر این صورت آیکون فایل
                                            const fileName = draft.fileDraft.fileName?.toLowerCase() || "";
                                            const mimeType = draft.fileDraft.mimeType || "";
                                            const isPdf = mimeType === "application/pdf" || fileName.endsWith(".pdf");
                                            const isWord = mimeType.includes("word") || /\.(doc|docx)$/i.test(fileName);
                                            
                                            if (isPdf) return <FilePdfOutlined style={{ fontSize: 32, color: "#f5222d" }} />;
                                            if (isWord) return <FileWordOutlined style={{ fontSize: 32, color: "#1890ff" }} />;
                                            return <FileOutlined style={{ fontSize: 32, color: "#666" }} />;
                                        })()}
                            </Space>
                                    <Space>
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
                                            // Clear local draft state
                                            setDraftByAttrId((prev) => ({
                                                ...prev,
                                                [attrId]: {
                                                    uploading: false,
                                                    localPreviewUrl: null,
                                                    fileDraft: null,
                                                    uploadProgress: 0,
                                                },
                                            }));
                                            message.info("فایل حذف شد.");
                                        }}
                                    >
                                        حذف فایل
                                    </Button>
                            )}
                        </Space>
                        </div>
                    )}

                    {/* Upload Button */}
                    {(!hasPersistedFile || (hasPersistedFile && !draft?.fileDraft)) && (
                            <Upload
                                beforeUpload={(file) => uploadKycFileWithProgress(attrId, file)}
                                showUploadList={false}
                            >
                                <Button
                                icon={<UploadOutlined />} 
                                disabled={isUploading}
                                loading={isUploading}
                                >
                                {hasPersistedFile ? "تغییر فایل" : "انتخاب فایل"}
                                </Button>
                        </Upload>
                            )}

                    {doc?.adminNote && (
                        <Alert type="warning" message={doc.adminNote} showIcon />
                    )}
                </div>
            );
        }

        const showPersistedLine = hasPersistedValue && !showDraftBadge;

        if (dt === 2) {
            return (
                <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
                    <Space>
                        <Text strong>{r.attributeDisplayName}</Text>
                        {requiredTag}
                        {renderDocStatusTag(doc?.status)}
                        {showDraftBadge && <Tag color="gold">در انتظار ارسال</Tag>}
                    </Space>

                    {r.description && <Text type="secondary">{r.description}</Text>}

                    {showPersistedLine && (
                        <div
                            style={{
                                background: "#f6ffed",
                                padding: "8px 12px",
                                borderRadius: 6,
                                border: "1px solid #b7eb8f",
                            }}
                        >
                            <Text type="secondary">مقدار ثبت‌شده: </Text>
                            <Text strong>{String(persistedValue)}</Text>
                        </div>
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
                </div>
            );
        }

        if (dt === 3) {
            return (
                <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
                    <Space>
                        <Text strong>{r.attributeDisplayName}</Text>
                        {requiredTag}
                        {renderDocStatusTag(doc?.status)}
                        {showDraftBadge && <Tag color="gold">در انتظار ارسال</Tag>}
                    </Space>

                    {r.description && <Text type="secondary">{r.description}</Text>}

                    {showPersistedLine && (
                        <div
                            style={{
                                background: "#f6ffed",
                                padding: "8px 12px",
                                borderRadius: 6,
                                border: "1px solid #b7eb8f",
                            }}
                        >
                            <Text type="secondary">مقدار ثبت‌شده: </Text>
                            <Text strong>{String(persistedValue)}</Text>
                        </div>
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
                </div>
            );
        }

        if (dt === 4) {
            return (
                <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
                    <Space>
                        <Text strong>{r.attributeDisplayName}</Text>
                        {requiredTag}
                        {renderDocStatusTag(doc?.status)}
                        {showDraftBadge && <Tag color="gold">در انتظار ارسال</Tag>}
                    </Space>

                    {r.description && <Text type="secondary">{r.description}</Text>}

                    {showPersistedLine && (
                        <div
                            style={{
                                background: "#f6ffed",
                                padding: "8px 12px",
                                borderRadius: 6,
                                border: "1px solid #b7eb8f",
                            }}
                        >
                            <Text type="secondary">مقدار ثبت‌شده: </Text>
                            <Text strong>{String(persistedValue)}</Text>
                        </div>
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
                </div>
            );
        }

        if (dt === 5) {
            return (
                <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
                    <Space>
                        <Text strong>{r.attributeDisplayName}</Text>
                        {requiredTag}
                        {renderDocStatusTag(doc?.status)}
                        {showDraftBadge && <Tag color="gold">در انتظار ارسال</Tag>}
                    </Space>

                    {r.description && <Text type="secondary">{r.description}</Text>}

                    {showPersistedLine && (
                        <div
                            style={{
                                background: "#f6ffed",
                                padding: "8px 12px",
                                borderRadius: 6,
                                border: "1px solid #b7eb8f",
                            }}
                        >
                            <Text type="secondary">مقدار ثبت‌شده: </Text>
                            <Text strong>{String(persistedValue)}</Text>
                        </div>
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
                </div>
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
                <Form form={form} layout="vertical" onFinish={handleSaveProfile} style={{ maxWidth: 800 }}>
                    <Row gutter={16}>
                        <Col span={12}>
                            <Form.Item
                                label="نوع کسب‌وکار"
                                name="businessType"
                                rules={[{ required: true, message: "نوع کسب‌وکار الزامی است" }]}
                            >
                                <Select placeholder="انتخاب نوع کسب‌وکار">
                                    <Option value={1}>حقیقی</Option>
                                    <Option value={2}>حقوقی</Option>
                                </Select>
                            </Form.Item>
                        </Col>

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
                            <Form.Item label="کد ملی / شماره ملی" name="nationalId">
                                <Input />
                            </Form.Item>
                        </Col>

                        <Col span={12}>
                            <Form.Item label="شماره ثبت" name="licenseNumber">
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
                            <Form.Item
                                label="نام رابط"
                                name="contactName"
                                rules={[{ required: true, message: "نام رابط الزامی است" }]}
                            >
                                <Input />
                            </Form.Item>
                        </Col>

                        <Col span={12}>
                            <Form.Item
                                label="سمت رابط"
                                name="contactPosition"
                                rules={[{ required: true, message: "سمت رابط الزامی است" }]}
                            >
                                <Select placeholder="انتخاب سمت رابط">
                                    <Option value={1}>مسئول خرید</Option>
                                    <Option value={2}>مدیر عامل</Option>
                                </Select>
                            </Form.Item>
                        </Col>

                        <Col span={12}>
                            <Form.Item
                                label="شماره موبایل رابط"
                                name="contactMobile"
                                rules={[{ required: true, message: "شماره موبایل رابط الزامی است" }]}
                            >
                                <Input />
                            </Form.Item>
                        </Col>

                        <Col span={12}>
                            <Form.Item
                                label="شماره تماس کسب‌وکار"
                                name="contactPhone"
                                rules={[{ required: true, message: "شماره تماس کسب‌وکار الزامی است" }]}
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
                <Space direction="vertical" size="large" style={{ width: "100%", maxWidth: 800 }}>
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
                                        <Col span={24}>
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
                                            <Text type="secondary">نوع کسب‌وکار</Text>
                                            <div><Text strong>{getBusinessTypeLabel(profile?.businessType)}</Text></div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">نام کسب‌وکار</Text>
                                            <div>
                                                <Text strong>{profile?.businessName || "-"}</Text>
                                            </div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">کد ملی / شماره ملی</Text>
                                            <div><Text strong>{profile?.nationalId || "-"}</Text></div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">شماره ثبت</Text>
                                            <div><Text strong>{profile?.licenseNumber || "-"}</Text></div>
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
                                        <Col span={24}>
                                            <Text type="secondary">آدرس</Text>
                                            <div><Text strong>{profile?.address || "-"}</Text></div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">نام رابط</Text>
                                            <div><Text strong>{profile?.contactName || "-"}</Text></div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">سمت رابط</Text>
                                            <div><Text strong>{getContactPositionLabel(profile?.contactPosition)}</Text></div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">شماره موبایل رابط</Text>
                                            <div><Text strong>{profile?.contactMobile || "-"}</Text></div>
                                        </Col>
                                        <Col span={12}>
                                            <Text type="secondary">شماره تماس کسب‌وکار</Text>
                                            <div>
                                                <Text strong>{profile?.contactPhone || "-"}</Text>
                                            </div>
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
