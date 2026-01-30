// src/app/AppRouter.jsx
import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";

import LoginPage from "../features/auth/pages/LoginPage";
import DashboardPage from "../features/dashboard/pages/DashboardPage";
import AdminLayout from "../layout/AdminLayout";
import { useAuth } from "../core/auth/useAuth";

import MenuSettingsPage from "../features/menuSettings/pages/MenuSettingsPage";
import UsersPage from "../features/users/pages/UsersPage";
import PermissionsPage from '../features/permissions/pages/PermissionsPage';
import RolesPage from '../features/roles/pages/RolesPage';
import SupplierTypesPage from "../features/supplierTypes/pages/SupplierTypesPage";
import SuppliersPage from "../features/suppliers/pages/SuppliersPage";
import KycAttributeDefinitionsPage from "../features/kycAttributeDefinitions/pages/KycAttributeDefinitionsPage";
import KycTemplatesPage from "../features/kycTemplates/pages/KycTemplatesPage";
import SupplierOnboardingPage from "../features/suppliers/pages/SupplierOnboardingPage";
import ProductCategoriesPage from "../features/productCategories/pages/ProductCategoriesPage";
import ProductsPage from "../features/products/pages/ProductsPage";
import ProductAttributeDefinitionsPage from "../features/productAttributes/pages/ProductAttributeDefinitionsPage";
import ProductAttributeTemplatesPage from "../features/productAttributeTemplates/pages/ProductAttributeTemplatesPage";
import MyOffersPage from "../features/offers/pages/MyOffersPage";
import OfferManagementPage from "../features/offers/pages/OfferManagementPage";
import OffersSearchPage from "../features/offers/pages/OffersSearchPage";
import AdminOffersPage from "../features/offers/pages/AdminOffersPage";
import ProfilePage from "../features/auth/pages/ProfilePage";
import WalletPage from "../features/wallet/pages/WalletPage";
import WalletUsersListPage from "../features/walletManagement/pages/WalletUsersListPage";
import WalletManagementPage from "../features/walletManagement/pages/WalletManagementPage";
import WalletTransactionsReportPage from "../features/walletManagement/pages/WalletTransactionsReportPage";
import CommissionsReportPage from "../features/walletManagement/pages/CommissionsReportPage";
import IncomeExpenseReportPage from "../features/walletManagement/pages/IncomeExpenseReportPage";
import FinancialDashboardPage from "../features/walletManagement/pages/FinancialDashboardPage";
import BankTransactionsReportPage from "../features/walletManagement/pages/BankTransactionsReportPage";
import SubscriptionsPage from "../features/subscriptions/pages/SubscriptionsPage";
import TopUpPage from "../features/payments/pages/TopUpPage";
import CommissionsPage from "../features/commissions/pages/CommissionsPage";
import PlansManagementPage from "../features/plans/pages/PlansManagementPage";
import UserSubscriptionsManagementPage from "../features/userSubscriptions/pages/UserSubscriptionsManagementPage";
import VisitorManagementPage from "../features/visitorManagement/pages/VisitorManagementPage";
import BuyerManagementPage from "../features/buyerManagement/pages/BuyerManagementPage";
import MyBuyersPage from "../features/visitorProfiles/pages/MyBuyersPage";
import CommissionRulesPage from "../features/commissionRules/pages/CommissionRulesPage";



const PrivateRoute = ({ children }) => {
    const { isAuthenticated } = useAuth();
    if (!isAuthenticated) {
        return <Navigate to="/login" replace />;
    }
    return children;
};

const AppRouter = () => {
    return (
        <Routes>
            <Route path="/login" element={<LoginPage />} />

            <Route
                path="/"
                element={
                    <PrivateRoute>
                        <AdminLayout />
                    </PrivateRoute>
                }
            >
                {/* داشبورد */}
                <Route index element={<DashboardPage />} />

                {/* مدیریت کاربران */}
                <Route path="users" element={<UsersPage />} />

                {/* مدیریت نوع تامین کننده */}
                <Route path="supplier-types" element={<SupplierTypesPage />} />

                {/* مدیریت تامین کننده */}
                <Route path="suppliers" element={<SuppliersPage />} />

                {/* مدیریت فیلدهای KYC */}
                <Route path="kyc-attribute-definitions" element={<KycAttributeDefinitionsPage />} />

                {/* مدیریت KYC Templates */}
                <Route path="kyc-templates" element={<KycTemplatesPage />} />

                {/* مدیریت KYC Templates */}
                <Route path="supplier-onboarding" element={<SupplierOnboardingPage />} />


                {/* مدیریت محوزها */}
                <Route path="Permissions" element={<PermissionsPage />} />

                {/* مدیریت نقش ها */}
                <Route path="Roles" element={<RolesPage />} />

                {/* تنظیمات منو */}
                <Route path="menu-settings" element={<MenuSettingsPage />} />

                {/* مدیریت دسته محصولات */}
                <Route path="product-categories" element={<ProductCategoriesPage />} />
                {/* لیست محصولات */}
                <Route path="products" element={<ProductsPage />} />
                {/* مدیریت ویژگی محصولات */}
                <Route path="Product-attribute-definitions" element={<ProductAttributeDefinitionsPage />} />
                {/* Template ویژگی‌های محصول */}
                <Route path="product-attribute-templatesPage" element={<ProductAttributeTemplatesPage />} />


                {/* مدیریت آگهی ها*/}
                <Route path="my-offers" element={<MyOffersPage />} />
                <Route path="admin-offers" element={<AdminOffersPage />} />
                <Route path="/supplier/offers/manage/:id" element={<OfferManagementPage />} />
                <Route path="/supplier/offers/manage" element={<OfferManagementPage />} />               
                <Route path="offers-search" element={<OffersSearchPage />} />

                        {/* پروفایل کاربری */}
                        <Route path="profile" element={<ProfilePage />} />

                        {/* کیف پول */}
                        <Route path="wallet" element={<WalletPage />} />
                        {/* مدیریت کیف پول کاربران (لیست) */}
                        <Route path="wallet-management" element={<WalletUsersListPage />} />
                        {/* جزئیات کیف پول یک کاربر خاص */}
                        <Route path="wallet/admin/:userId" element={<WalletManagementPage />} />
                        {/* گزارش تراکنش‌های مالی */}
                        <Route path="wallet-transactions-report" element={<WalletTransactionsReportPage />} />
                        {/* گزارش پورسانت‌ها */}
                        <Route path="commissions-report" element={<CommissionsReportPage />} />
                        {/* گزارش درآمد/هزینه */}
                        <Route path="income-expense-report" element={<IncomeExpenseReportPage />} />
                        {/* داشبورد مالی */}
                        <Route path="financial-dashboard" element={<FinancialDashboardPage />} />
                        {/* گزارش تراکنش‌های بانکی */}
                        <Route path="bank-transactions-report" element={<BankTransactionsReportPage />} />

                        {/* اشتراک‌ها */}
                        <Route path="subscriptions" element={<SubscriptionsPage />} />

                        {/* پرداخت / شارژ کیف پول */}
                        <Route path="payments/topup" element={<TopUpPage />} />

                        {/* پورسانت‌ها */}
                        <Route path="commissions" element={<CommissionsPage />} />

                        {/* مدیریت قوانین پورسانت */}
                        <Route path="commission-rules" element={<CommissionRulesPage />} />

                        {/* خریداران من (Visitor) */}
                        <Route path="my-buyers" element={<MyBuyersPage />} />

                        {/* مدیریت پلن‌های اشتراک */}
                        <Route path="plans-management" element={<PlansManagementPage />} />

                {/* مدیریت اشتراک‌های خریداری شده */}
                <Route path="user-subscriptions-management" element={<UserSubscriptionsManagementPage />} />

                {/* مدیریت Visitor ها (Admin) */}
                <Route path="visitor-management" element={<VisitorManagementPage />} />

                {/* مدیریت Buyer ها (Admin) */}
                <Route path="buyer-management" element={<BuyerManagementPage />} />



            </Route>

            {/* هر مسیر ناشناخته → داشبورد */}
            <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
    );
};

export default AppRouter;
