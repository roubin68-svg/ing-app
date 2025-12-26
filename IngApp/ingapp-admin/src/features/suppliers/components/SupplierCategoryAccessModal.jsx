import React, { useEffect, useMemo, useState } from "react";
import { App, Modal, Spin, Tree, Button, Space } from "antd";

import supplierCategoryAccessApi from "../api/supplierCategoryAccessApi";
import productCategoriesApi from "../../productCategories/api/productCategoryApi";

const SupplierCategoryAccessModal = ({
	open,
	supplierUserId,
	onClose,
}) => {
	const { message } = App.useApp();

	// ========================
	// States
	// ========================
	const [loading, setLoading] = useState(false);
	const [saving, setSaving] = useState(false);

	const [categoryTree, setCategoryTree] = useState([]);
	const [checkedKeys, setCheckedKeys] = useState([]);

	// ========================
	// Helpers
	// ========================
	const buildTreeFromFlatList = (items, parentId = null) =>
		items
			.filter(x => x.parentId === parentId)
			.map(x => ({
				key: x.id,
				title: x.name,
				children: buildTreeFromFlatList(items, x.id),
			}));

	// ========================
	// Load Category Tree
	// ========================
	// Load Category Tree
	useEffect(() => {
		if (!open) return;

		const loadCategories = async () => {
			try {
				setLoading(true);

				const res = await productCategoriesApi.getAll();
				const treeData = buildTreeFromFlatList(res || []);

				setCategoryTree(treeData);
			} catch (e) {
				console.error(e);
				message.error("خطا در دریافت دسته‌بندی‌ها");
			} finally {
				setLoading(false);
			}
		};

		loadCategories();
	}, [open, message]);


	// ========================
	// Load Supplier Access
	// ========================
	useEffect(() => {
		if (!open || !supplierUserId) {
			setCheckedKeys([]);
			return;
		}

		const loadAccess = async () => {
			try {
				setLoading(true);

				const res =
					await supplierCategoryAccessApi.getBySupplier(
						supplierUserId
					);

				setCheckedKeys(
					(res || [])
						.filter((x) => x.isActive)
						.map((x) => x.productCategoryId)
				);
			} catch (e) {
				console.error(e);
				message.error(
					"خطا در دریافت دسترسی دسته‌بندی‌های تأمین‌کننده"
				);
			} finally {
				setLoading(false);
			}
		};

		loadAccess();
	}, [open, supplierUserId, message]);

	// ========================
	// Tree Check Handler
	// ========================
	const handleCheck = (keys) => {
		setCheckedKeys(keys);
	};

	// ========================
	// Save (Sync)
	// ========================
	const handleSave = async () => {
		try {
			setSaving(true);

			await supplierCategoryAccessApi.sync(
				supplierUserId,
				{
					productCategoryIds: checkedKeys,
				}
			);

			message.success("دسترسی دسته‌بندی‌ها ذخیره شد");
			onClose();
		} catch (e) {
			console.error(e);
			message.error("خطا در ذخیره دسترسی‌ها");
		} finally {
			setSaving(false);
		}
	};

	// ========================
	// Render
	// ========================
	return (
		<Modal
			open={open}
			title="مدیریت دسترسی دسته‌بندی‌ها"
			width={900}
			onCancel={onClose}
			footer={
				<Space>
					<Button onClick={onClose}>
						انصراف
					</Button>
					<Button
						type="primary"
						loading={saving}
						onClick={handleSave}
						disabled={!supplierUserId}
					>
						ذخیره
					</Button>
				</Space>
			}
			destroyOnClose
		>
			{loading ? (
				<Spin />
			) : (
				<Tree
					checkable
					defaultExpandAll
					checkedKeys={checkedKeys}
					onCheck={handleCheck}
					treeData={categoryTree}
				/>
			)}
		</Modal>
	);
};

export default SupplierCategoryAccessModal;
