// src/core/constants/productAttributeDataTypes.js

/**
 * IMPORTANT:
 * This mapping MUST stay in sync with backend enum:
 * IngApp.Domain.Enums.ProductAttributeDataType
 *
 * public enum ProductAttributeDataType
 * {
 *   Text = 1,
 *   Number = 2,
 *   Boolean = 3,
 *   Date = 4,
 *   File = 5
 * }
 */

export const PRODUCT_ATTRIBUTE_DATA_TYPES = [
    { value: 1, key: "Text", label: "متن" },
    { value: 2, key: "Number", label: "عدد" },
    { value: 3, key: "Boolean", label: "بولی (بله / خیر)" },
    { value: 4, key: "Date", label: "تاریخ" },
    { value: 5, key: "File", label: "فایل" },
];

/**
 * Helpers (برای استفاده در Table / Form)
 */

export const getProductAttributeDataTypeLabel = (value) => {
    const item = PRODUCT_ATTRIBUTE_DATA_TYPES.find(
        (x) => x.value === value
    );
    return item ? item.label : value;
};

export const getProductAttributeDataTypeOptions = () =>
    PRODUCT_ATTRIBUTE_DATA_TYPES.map((x) => ({
        value: x.value,
        label: x.label,
    }));
