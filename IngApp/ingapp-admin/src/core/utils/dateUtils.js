/**
 * Utility functions for date conversion between Gregorian (Miladi) and Shamsi (Jalali)
 * 
 * در دیتابیس همیشه تاریخ میلادی نگه می‌داریم
 * در UI همیشه تاریخ شمسی نمایش می‌دهیم
 */

import dayjs from "dayjs";
import jalaali from "jalaali-js";

/**
 * تبدیل تاریخ میلادی (string ISO یا Date) به dayjs شمسی
 * برای استفاده در JalaliDatePicker
 * 
 * @param {string|Date|dayjs.Dayjs|null} gregorian - تاریخ میلادی
 * @returns {dayjs.Dayjs|null} - dayjs object شمسی
 */
export const toShamsiDayjs = (gregorian) => {
    if (!gregorian) return null;

    try {
        let year, month, day;

        // اگر string است (ISO format: "2024-01-15" یا "2024-01-15T00:00:00")
        if (typeof gregorian === "string") {
            const datePart = gregorian.split("T")[0]; // فقط قسمت تاریخ را بگیر
            [year, month, day] = datePart.split("-").map(Number);
        }
        // اگر Date object است
        else if (gregorian instanceof Date) {
            year = gregorian.getFullYear();
            month = gregorian.getMonth() + 1;
            day = gregorian.getDate();
        }
        // اگر dayjs object است
        else if (dayjs.isDayjs(gregorian)) {
            year = gregorian.year();
            month = gregorian.month() + 1;
            day = gregorian.date();
        }
        else {
            return null;
        }

        // تبدیل به شمسی
        const j = jalaali.toJalaali(year, month, day);
        
        // ساخت dayjs object شمسی
        return dayjs(`${j.jy}/${j.jm}/${j.jd}`, "YYYY/M/D");
    } catch (error) {
        console.error("Error converting date to Shamsi:", error);
        return null;
    }
};

/**
 * تبدیل تاریخ شمسی (dayjs) به میلادی (ISO string)
 * برای ارسال به API
 * 
 * @param {dayjs.Dayjs|null} shamsiDayjs - dayjs object شمسی
 * @returns {string|null} - ISO string میلادی (format: "2024-01-15T00:00:00")
 */
export const toGregorianISO = (shamsiDayjs) => {
    if (!shamsiDayjs) return null;

    try {
        // اگر dayjs object نیست، تبدیل کن
        const date = dayjs.isDayjs(shamsiDayjs) ? shamsiDayjs : dayjs(shamsiDayjs);
        
        // استخراج سال، ماه، روز شمسی
        const [jy, jm, jd] = date.format("YYYY/M/D").split("/").map(Number);
        
        // تبدیل به میلادی
        const g = jalaali.toGregorian(jy, jm, jd);
        
        // ساخت ISO string
        return `${g.gy}-${String(g.gm).padStart(2, "0")}-${String(g.gd).padStart(2, "0")}T00:00:00`;
    } catch (error) {
        console.error("Error converting Shamsi to Gregorian:", error);
        return null;
    }
};

/**
 * تبدیل تاریخ میلادی به string شمسی برای نمایش
 * 
 * @param {string|Date|dayjs.Dayjs|null} gregorian - تاریخ میلادی
 * @returns {string} - string شمسی (format: "1403/01/15") یا "-" اگر null باشد
 */
export const toShamsiString = (gregorian) => {
    if (!gregorian) return "-";

    try {
        let year, month, day;

        if (typeof gregorian === "string") {
            const datePart = gregorian.split("T")[0];
            [year, month, day] = datePart.split("-").map(Number);
        } else if (gregorian instanceof Date) {
            year = gregorian.getFullYear();
            month = gregorian.getMonth() + 1;
            day = gregorian.getDate();
        } else if (dayjs.isDayjs(gregorian)) {
            year = gregorian.year();
            month = gregorian.month() + 1;
            day = gregorian.date();
        } else {
            return "-";
        }

        const j = jalaali.toJalaali(year, month, day);
        return `${j.jy}/${String(j.jm).padStart(2, "0")}/${String(j.jd).padStart(2, "0")}`;
    } catch (error) {
        console.error("Error converting date to Shamsi string:", error);
        return "-";
    }
};

/**
 * دریافت تاریخ امروز به صورت dayjs شمسی
 * 
 * @returns {dayjs.Dayjs} - dayjs object شمسی امروز
 */
export const todayShamsi = () => {
    const now = new Date();
    const j = jalaali.toJalaali(
        now.getFullYear(),
        now.getMonth() + 1,
        now.getDate()
    );
    return dayjs(`${j.jy}/${j.jm}/${j.jd}`, "YYYY/M/D");
};

/**
 * اطمینان از اینکه date شمسی است (برای JalaliDatePicker onChange)
 * اگر JalaliDatePicker تاریخ میلادی برگرداند، آن را به شمسی تبدیل می‌کند
 * 
 * @param {any} date - date از JalaliDatePicker
 * @returns {dayjs.Dayjs|null} - dayjs object شمسی
 */
export const ensureShamsiDayjs = (date) => {
    if (!date) return null;
    
    // اگر dayjs object است
    if (dayjs.isDayjs(date)) {
        try {
            const year = date.year();
            
            // اگر سال بزرگتر از 2000 است، میلادی است (سال‌های شمسی 1300-1500 هستند)
            if (year > 2000) {
                // میلادی است - به شمسی تبدیل می‌کنیم
                const dateStr = date.format("YYYY-MM-DD");
                return toShamsiDayjs(dateStr);
            }
            
            // سال بین 1300-2000 است - احتمالاً شمسی است
            return date;
        } catch {
            return date;
        }
    }
    
    // اگر Date object است (همیشه میلادی)
    if (date instanceof Date) {
        return toShamsiDayjs(date);
    }
    
    // اگر string است
    if (typeof date === "string") {
        return toShamsiDayjs(date) || todayShamsi();
    }
    
    return todayShamsi();
};

