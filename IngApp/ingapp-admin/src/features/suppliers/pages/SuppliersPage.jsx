import React, { useEffect, useState } from "react";
import {
    Card,
    Table,
    Button,
    Space,
    Input,
    Select,
    Tag,
    message,
} from "antd";
import { EyeOutlined, ShoppingOutlined } from "@ant-design/icons";
import suppliersApi from "../api/suppliersApi";
import SupplierCaseDrawer from "../components/SupplierCaseDrawer";
import { getProvinces, getCitiesByProvince } from "../../../core/location/iranProvinces";
import supplierTypesApi from "../../supplierTypes/api/supplierTypesApi";
import { useLocation } from "react-router-dom";
import SupplierCategoryAccessModal from "../components/SupplierCategoryAccessModal";



const { Option } = Select;

const SuppliersPage = () => {
    // ----------------------------
    // State
    // ----------------------------
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState([]);
    const [totalCount, setTotalCount] = useState(0);

    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [selectedProvince, setSelectedProvince] = useState(null);

    const provinces = React.useMemo(() => getProvinces(), []);
    const cities = React.useMemo(
        () => getCitiesByProvince(selectedProvince),
        [selectedProvince]
    );

    const verificationStatusOptions = [
        { label: "در انتظار ارسال مدارک", value: "NotSubmitted" },
        { label: "در حال بررسی", value: "Pending" },
        { label: "تأیید شده", value: "Approved" },
        { label: "رد شده", value: "Rejected" },
    ];



    const [filters, setFilters] = useState({
        businessName: null,
        userPhoneNumber: null,
        supplierTypeId: null,
        province: null,
        city: null,
        verificationStatus: null,
    });

    const [selectedSupplierId, setSelectedSupplierId] = useState(null);
    const [drawerOpen, setDrawerOpen] = useState(false);
    const [supplierTypes, setSupplierTypes] = useState([]);

    const [categoryAccessSupplierId, setCategoryAccessSupplierId] = useState(null);
    const [isCategoryAccessModalOpen, setIsCategoryAccessModalOpen] = useState(false);

    // ----------------------------
    // Load data
    // ----------------------------
    const loadSuppliers = async (paramsOverride = {}) => {
        setLoading(true);
        try {
            const res = await suppliersApi.getPaged({
                page,
                pageSize,
                ...filters,
                ...paramsOverride,
            });

            setData(res.items || []);
            setTotalCount(res.totalCount || 0);
        } catch (err) {
            message.error(
                err?.response?.data?.message ||
                err?.response?.data?.Error ||
                "خطا در دریافت لیست تأمین‌کنندگان"
            );
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        supplierTypesApi.getAll()
            .then(setSupplierTypes)
            .catch(() => { });
        loadSuppliers();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [page, pageSize]);

    const location = useLocation();

    useEffect(() => {
        if (location.state?.defaultVerificationStatus) {
            setFilters(prev => ({
                ...prev,
                verificationStatus: location.state.defaultVerificationStatus,
            }));
            setPage(1);
            loadSuppliers({
                page: 1,
                verificationStatus: location.state.defaultVerificationStatus,
            });
        }
    }, [location.state]);


    const openCategoryAccessModal = (supplierUserId) => {
        setCategoryAccessSupplierId(supplierUserId);
        setIsCategoryAccessModalOpen(true);
    };

    const closeCategoryAccessModal = () => {
        setIsCategoryAccessModalOpen(false);
        setCategoryAccessSupplierId(null);
    };



    // ----------------------------
    // Table change (paging/sort)
    // ----------------------------
    const handleTableChange = (pagination, _filters, sorter) => {
        setPage(pagination.current);
        setPageSize(pagination.pageSize);

        if (sorter?.field) {
            loadSuppliers({
                sortBy: sorter.field,
                sortDesc: sorter.order === "descend",
            });
        }
    };

    // ----------------------------
    // Actions
    // ----------------------------
    const openCase = (supplierId) => {
        setSelectedSupplierId(supplierId);
        setDrawerOpen(true);
    };

    const refreshSuppliers = async () => {
        await loadSuppliers({
            page,
            pageSize,
        });
    };


    // ----------------------------
    // Columns
    // ----------------------------
    const columns = [
        {
            title: "نام کسب‌وکار",
            dataIndex: "businessName",
            sorter: true,
        },
        {
            title: "نوع تأمین‌کننده",
            dataIndex: "supplierTypeName",
            sorter: true,
        },
        {
            title: "استان",
            dataIndex: "province",
            sorter: true,
        },
        {
            title: "شهر",
            dataIndex: "city",
            sorter: true,
        },
        {
            title: "شماره موبایل",
            dataIndex: "userPhoneNumber",
        },
        {
            title: "وضعیت",
            dataIndex: "verificationStatus",
            sorter: true,
            render: (status) => {
                const color =
                    status === "Approved"
                        ? "green"
                        : status === "Rejected"
                            ? "red"
                            : status === "NotSubmitted"
                                ? "blue"
                                : "orange";

                return <Tag color={color}>{status}</Tag>;
            },
        },
        {
            title: "عملیات",
            key: "actions",
            render: (_, record) => (

                <Space>
                    <Button icon={<EyeOutlined />} onClick={() => openCase(record.id)}>
                        مشاهده پرونده
                    </Button>

                    <Button icon={<ShoppingOutlined />}
                        onClick={() => openCategoryAccessModal(record.userId)}
                    >
                        دسترسی محصولات
                    </Button>
                </Space>

            ),
        },
    ];

    // ----------------------------
    // Render
    // ----------------------------
    return (
        <>
            <Card
                title="لیست تأمین‌کنندگان"
                bordered={false}
            >
                {/* Filters */}
                <Space style={{ marginBottom: 16 }} wrap>
                    <Input
                        placeholder="نام کسب‌وکار"
                        allowClear
                        value={filters.businessName}
                        onChange={(e) =>
                            setFilters((prev) => ({
                                ...prev,
                                businessName: e.target.value || null,
                            }))
                        }
                    />


                    <Input
                        placeholder="شماره تماس"
                        allowClear
                        value={filters.userPhoneNumber}
                        onChange={(e) =>
                            setFilters((prev) => ({
                                ...prev,
                                userPhoneNumber: e.target.value || null,
                            }))
                        }
                    />

                    <Select
                        placeholder="استان"
                        allowClear
                        value={selectedProvince}
                        style={{ width: 160 }}
                        onChange={(value) => {
                            setSelectedProvince(value || null);
                            setFilters((prev) => ({
                                ...prev,
                                province: value || null,
                                city: null,
                            }));
                        }}
                    >
                        {provinces.map((p) => (
                            <Option key={p} value={p}>{p}</Option>
                        ))}
                    </Select>

                    <Select
                        placeholder="شهر"
                        allowClear
                        value={filters.city}
                        disabled={!selectedProvince}
                        style={{ width: 160 }}
                        onChange={(value) =>
                            setFilters((prev) => ({
                                ...prev,
                                city: value || null,
                            }))
                        }
                    >

                        {cities.map((c) => (
                            <Option key={c} value={c}>{c}</Option>
                        ))}
                    </Select>



                    <Select
                        placeholder="نوع تأمین‌کننده"
                        allowClear
                        value={filters.supplierTypeId}
                        style={{ width: 180 }}
                        onChange={(value) =>
                            setFilters((prev) => ({
                                ...prev,
                                supplierTypeId: value || null,
                            }))
                        }
                    >
                        {supplierTypes.map(t => (
                            <Option key={t.id} value={t.id}>
                                {t.name}
                            </Option>
                        ))}
                    </Select>
                    <Select
                        placeholder="وضعیت"
                        allowClear
                        value={filters.verificationStatus}
                        style={{ width: 180 }}
                        onChange={(value) =>
                            setFilters((prev) => ({
                                ...prev,
                                verificationStatus: value || null,
                            }))
                        }
                    >
                        {verificationStatusOptions.map((opt) => (
                            <Option key={opt.value} value={opt.value}>
                                {opt.label}
                            </Option>
                        ))}
                    </Select>


                    <Button
                        type="primary"
                        onClick={() => {
                            setPage(1);
                            loadSuppliers({ page: 1 });
                        }}
                    >
                        جستجو
                    </Button>

                    <Button
                        onClick={() => {
                            // 1️⃣ reset همه state ها
                            setFilters({
                                businessName: null,
                                userPhoneNumber: null,
                                supplierTypeId: null,
                                province: null,
                                city: null,
                                verificationStatus: null,
                            });

                            setSelectedProvince(null);
                            setPage(1);

                            // 2️⃣ load بدون filter
                            loadSuppliers({
                                page: 1,
                                businessName: null,
                                userPhoneNumber: null,
                                supplierTypeId: null,
                                province: null,
                                city: null,
                                verificationStatus: null,
                            });
                        }}
                    >
                        پاکسازی
                    </Button>

                </Space>

                {/* Table */}
                <Table
                    rowKey="id"
                    loading={loading}
                    columns={columns}
                    dataSource={data}
                    pagination={{
                        current: page,
                        pageSize,
                        total: totalCount,
                        showSizeChanger: true,
                    }}
                    onChange={handleTableChange}
                />
            </Card>

            {/* Drawer */}
            <SupplierCaseDrawer
                open={drawerOpen}
                supplierId={selectedSupplierId}
                onStatusChanged={refreshSuppliers}
                onClose={() => {
                    setDrawerOpen(false);
                    setSelectedSupplierId(null);
                }}
            />

            <SupplierCategoryAccessModal
                open={isCategoryAccessModalOpen}
                supplierUserId={categoryAccessSupplierId}
                onClose={closeCategoryAccessModal}
            />

        </>


    );

};

export default SuppliersPage;
