// src/core/location/iranLocations.js
export const IRAN_LOCATIONS = [
    { province: "تهران", cities: ["تهران", "ری", "شمیرانات", "اسلام‌شهر", "قدس", "ورامین"] },
    { province: "اصفهان", cities: ["اصفهان", "کاشان", "نجف‌آباد", "خمینی‌شهر", "شاهین‌شهر"] },
    { province: "خراسان رضوی", cities: ["مشهد", "نیشابور", "سبزوار", "تربت حیدریه", "قوچان"] },
    { province: "فارس", cities: ["شیراز", "مرودشت", "کازرون", "جهرم", "فسا"] },
    { province: "آذربایجان شرقی", cities: ["تبریز", "مراغه", "میانه", "مرند", "اهر"] },
    { province: "آذربایجان غربی", cities: ["ارومیه", "خوی", "میاندوآب", "بوکان", "مهاباد"] },
    { province: "مازندران", cities: ["ساری", "بابل", "آمل", "قائم‌شهر", "نوشهر"] },
    { province: "گیلان", cities: ["رشت", "انزلی", "لاهیجان", "لنگرود", "آستارا"] },
    { province: "کرمان", cities: ["کرمان", "رفسنجان", "سیرجان", "جیرفت", "بم"] },
    { province: "خوزستان", cities: ["اهواز", "آبادان", "خرمشهر", "دزفول", "ماهشهر"] },
    // اگر خواستی بعداً کاملش می‌کنیم یا دیتای واقعی از بک‌اند می‌گیریم،
    // ولی API این فایل همین می‌مونه.
];

export const getProvinces = () => IRAN_LOCATIONS.map(x => x.province);

export const getCitiesByProvince = (province) => {
    const item = IRAN_LOCATIONS.find(x => x.province === province);
    return item ? item.cities : [];
};
