// src/features/products/components/CategoryTreeSelect.jsx
import React, { useEffect, useState } from "react";
import { TreeSelect, Spin } from "antd";
import productCategoryApi from "../../productCategories/api/productCategoryApi";

const { SHOW_PARENT } = TreeSelect;

const CategoryTreeSelect = ({
    value,
    onChange,
    placeholder = "انتخاب دسته‌بندی",
    allowClear = true,
    disabled = false,
    style,
}) => {
    const [loading, setLoading] = useState(false);
    const [treeData, setTreeData] = useState([]);

    // ================================
    // Load Categories
    // ================================
    useEffect(() => {
        const load = async () => {
            try {
                setLoading(true);
                const data = await productCategoryApi.getAll();
                setTreeData(convertToTree(data));
            } catch (e) {
                console.error(e);
            } finally {
                setLoading(false);
            }
        };

        load();
    }, []);

    // ================================
    // Convert Flat → Tree
    // ================================
    const convertToTree = (items) => {
        if (!items || items.length === 0) return [];

        const map = {};
        items.forEach((c) => {
            map[c.id] = {
                key: c.id,
                value: c.id,
                disabled: !c.isActive,
                title: (
                    <span style={{ opacity: c.isActive ? 1 : 0.5 }}>
                        {c.name}
                        {!c.isActive && (
                            <span style={{ color: "red", marginRight: 6 }}>
                                (غیرفعال)
                            </span>
                        )}
                    </span>
                ),
                children: [],
            };
        });

        const roots = [];
        items.forEach((c) => {
            if (c.parentId) {
                map[c.parentId]?.children.push(map[c.id]);
            } else {
                roots.push(map[c.id]);
            }
        });

        return roots;
    };

    return (
        <TreeSelect
            value={value}
            onChange={onChange}
            treeData={treeData}
            placeholder={placeholder}
            allowClear={allowClear}
            disabled={disabled}
            loading={loading}
            showSearch
            treeDefaultExpandAll
            treeNodeFilterProp="title"
            dropdownStyle={{ maxHeight: 400, overflow: "auto" }}
            style={{ width: "100%", ...style }}
            notFoundContent={loading ? <Spin size="small" /> : "دسته‌بندی‌ای یافت نشد"}
        />
    );
};

export default CategoryTreeSelect;
