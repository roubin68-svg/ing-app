// src/layout/Sidebar.jsx
import React, { useMemo, useState } from "react";
import { Menu, Spin, Alert } from "antd";
import { useLocation, useNavigate } from "react-router-dom";
import { useMenu } from "../core/menu/useMenu";
import { getAntIconComponent } from "../core/menu/iconMapper";
import "../theme/font.css";
import "../theme/layout.css";

/**
 * محاسبه کلید منو بر اساس route یا id/key
 * - اگر route معتبر باشد → خود route به‌عنوان key
 * - اگر route "#” یا خالی باشد → یک key مجازی مثل "menu-12"
 */
const getItemKey = (item) => {
  const hasRoute =
    item.route &&
    item.route.trim() !== "" &&
    item.route.trim() !== "#";

  return hasRoute ? item.route : `menu-${item.key || item.id}`;
};

/**
 * تبدیل MenuItemDto[] به items مورد نیاز AntD Menu
 * این تابع به‌شکل ریکرسیو کل درخت منو را map می‌کند.
 */
const buildMenuItems = (nodes) =>
  nodes.map((item) => {
    const hasChildren = item.children && item.children.length > 0;
    const IconComp = getAntIconComponent(item.icon);

    return {
      key: getItemKey(item),
      icon: IconComp ? <IconComp /> : null,
      label: item.title,
      children: hasChildren ? buildMenuItems(item.children) : undefined,
    };
  });

const Sidebar = ({ collapsed }) => {
  const { items: menuItemsDto, loading, error } = useMenu();
  const location = useLocation();
  const navigate = useNavigate();

  // ساخت لیست آیتم‌ها برای AntD
  const menuItems = useMemo(
    () =>
      menuItemsDto && menuItemsDto.length
        ? buildMenuItems(menuItemsDto)
        : [],
    [menuItemsDto]
  );

  // کلیدهای root برای کنترل باز/بسته شدن (فقط یک ریشه باز)
  const rootKeys = useMemo(
    () => (menuItemsDto || []).map((item) => getItemKey(item)),
    [menuItemsDto]
  );

  const [openKeys, setOpenKeys] = useState([]);

  const handleOpenChange = (keys) => {
    const latest = keys.find((k) => !openKeys.includes(k));
    if (!latest) {
      setOpenKeys([]);
      return;
    }

    // اگر latest یک ریشه است → فقط همان را باز نگه داریم (Accordion)
    if (rootKeys.includes(latest)) {
      setOpenKeys([latest]);
    } else {
      setOpenKeys(keys);
    }
  };

  const handleClick = ({ key }) => {
    // منوهایی که route "#” دارند، keyشان با "menu-" شروع می‌شود
    // این‌ها فقط برای باز/بسته شدن هستند و نباید ناوبری کنند
    if (key.startsWith("menu-")) return;

    navigate(key);
  };

  const selectedKeys = useMemo(
    () => [location.pathname || "/"],
    [location.pathname]
  );

  return (
    <>
      {loading && (
        <div style={{ padding: 16, textAlign: "center" }}>
          <Spin size="small" />
        </div>
      )}

      {error && !loading && (
        <div style={{ padding: 12 }}>
          <Alert
            type="error"
            message={error}
            showIcon
            style={{ fontSize: 12, textAlign: "right" }}
          />
        </div>
      )}

      {!loading && !error && (
        <Menu
          mode="inline"
          className="admin-menu"
          items={menuItems}
          selectedKeys={selectedKeys}
          openKeys={collapsed ? [] : openKeys}
          onOpenChange={handleOpenChange}
          onClick={handleClick}
          inlineCollapsed={collapsed}
        />
      )}
    </>
  );
};

export default Sidebar;
