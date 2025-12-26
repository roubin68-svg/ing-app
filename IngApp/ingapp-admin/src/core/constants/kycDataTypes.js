// src/core/constants/kycDataTypes.js

/**
 * IMPORTANT:
 * This mapping MUST stay in sync with backend enum:
 * IngApp.Domain.Enums.KycDataType
 *
 * public enum KycDataType
 * {
 *   File = 1,
 *   Text = 2,
 *   Number = 3,
 *   Boolean = 4,
 *   Enum = 5
 * }
 */

export const KYC_DATA_TYPES = [
    { value: 1, key: "File", label: "فایل" },
    { value: 2, key: "Text", label: "متن" },
    { value: 3, key: "Number", label: "عدد" },
    { value: 4, key: "Boolean", label: "بولی (بله/خیر)" },
    { value: 5, key: "Enum", label: "لیستی" },
];

/**
 * Helpers (برای استفاده راحت در Table / Form)
 */

export const getKycDataTypeLabel = (value) => {
    const item = KYC_DATA_TYPES.find((x) => x.value === value);
    return item ? item.label : value;
};

export const getKycDataTypeOptions = () =>
    KYC_DATA_TYPES.map((x) => ({
        value: x.value,
        label: x.label,
    }));
