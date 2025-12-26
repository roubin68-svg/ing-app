// src/core/menu/iconMapper.js
import * as Icons from "@ant-design/icons";

/**
 * بر اساس اسم آیکن که از دیتابیس می‌آید (مثلاً "DashboardOutlined")
 * کامپوننت آیکن Ant Design را برمی‌گرداند.
 */
export function getAntIconComponent(iconName) {
  if (!iconName || typeof iconName !== "string") return null;

  const trimmed = iconName.trim();
  const IconComp = Icons[trimmed];

  if (!IconComp) {
    if (process.env.NODE_ENV === "development") {
      console.warn("Unknown menu icon:", trimmed);
    }
    return null;
  }

  return IconComp;
}
