// src/core/menu/useMenu.js
import { useEffect, useState } from "react";
import menuApi from "../../features/menuSettings/api/menuApi";

/**
 * Hook خواندن منوی سمت چپ بر اساس نقش‌ها و پرمیشن‌های کاربر
 */
export function useMenu() {
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        let isMounted = true;

        const load = async () => {
            setLoading(true);
            setError("");

            try {
                const data = await menuApi.getMyMenu(); // => MenuItemDto[]
                if (!Array.isArray(data)) {
                    throw new Error("ساختار منو نامعتبر است.");
                }
                if (isMounted) {
                    setItems(data);
                }
            } catch (err) {
                console.error("Error loading menu:", err);
                if (isMounted) {
                    setError("خطا در دریافت منوها");
                }
            } finally {
                if (isMounted) {
                    setLoading(false);
                }
            }
        };

        load();

        return () => {
            isMounted = false;
        };
    }, []);

    return { items, loading, error };
}
