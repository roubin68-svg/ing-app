# راهنمای استفاده از Date Utils

این فایل شامل توابع یکپارچه برای تبدیل تاریخ بین میلادی (Gregorian) و شمسی (Jalali) است.

## اصول کلی

- **در دیتابیس**: همیشه تاریخ میلادی نگه می‌داریم (ISO format: `"2024-01-15T00:00:00"`)
- **در UI**: همیشه تاریخ شمسی نمایش می‌دهیم
- **در فرم‌ها**: از `JalaliDatePicker` استفاده می‌کنیم که تاریخ شمسی برمی‌گرداند

## توابع موجود

### `toShamsiDayjs(gregorian)`
تبدیل تاریخ میلادی به dayjs object شمسی (برای استفاده در JalaliDatePicker)

```javascript
import { toShamsiDayjs } from "../../../core/utils/dateUtils";

// از API می‌آید (میلادی)
const gregorianDate = "2024-01-15T00:00:00";

// تبدیل به شمسی برای فرم
const shamsiDate = toShamsiDayjs(gregorianDate);
form.setFieldValue("dateField", shamsiDate);
```

### `toGregorianISO(shamsiDayjs)`
تبدیل تاریخ شمسی (dayjs) به میلادی (ISO string) برای ارسال به API

```javascript
import { toGregorianISO } from "../../../core/utils/dateUtils";

// از فرم می‌آید (شمسی)
const shamsiDate = form.getFieldValue("dateField");

// تبدیل به میلادی برای API
const gregorianISO = toGregorianISO(shamsiDate);
await api.update({ date: gregorianISO });
```

### `toShamsiString(gregorian)`
تبدیل تاریخ میلادی به string شمسی برای نمایش در جدول

```javascript
import { toShamsiString } from "../../../core/utils/dateUtils";

// در جدول
{
    title: "تاریخ",
    dataIndex: "createdAt",
    render: (date) => toShamsiString(date), // "1403/01/15"
}
```

### `todayShamsi()`
دریافت تاریخ امروز به صورت dayjs شمسی

```javascript
import { todayShamsi } from "../../../core/utils/dateUtils";

<JalaliDatePicker defaultPickerValue={todayShamsi()} />
```

### `ensureShamsiDayjs(date)`
اطمینان از اینکه date شمسی است (برای JalaliDatePicker onChange)

```javascript
import { ensureShamsiDayjs } from "../../../core/utils/dateUtils";

<JalaliDatePicker
    onChange={(date) => {
        const shamsiDate = ensureShamsiDayjs(date);
        form.setFieldValue("dateField", shamsiDate);
    }}
/>
```

## مثال کامل: استفاده در فرم

```javascript
import { DatePicker as JalaliDatePicker } from "antd-jalali";
import { toShamsiDayjs, toGregorianISO, ensureShamsiDayjs } from "../../../core/utils/dateUtils";

// 1. هنگام بارگذاری داده از API
useEffect(() => {
    if (data?.date) {
        form.setFieldsValue({
            dateField: toShamsiDayjs(data.date) // تبدیل میلادی به شمسی
        });
    }
}, [data]);

// 2. در فرم
<Form.Item name="dateField">
    <JalaliDatePicker
        style={{ width: "100%" }}
        onChange={(date) => {
            const shamsiDate = ensureShamsiDayjs(date);
            form.setFieldValue("dateField", shamsiDate);
        }}
    />
</Form.Item>

// 3. هنگام ارسال به API
const handleSubmit = async (values) => {
    await api.create({
        ...values,
        date: toGregorianISO(values.dateField) // تبدیل شمسی به میلادی
    });
};
```

## مثال: استفاده در جدول

```javascript
import { toShamsiString } from "../../../core/utils/dateUtils";

const columns = [
    {
        title: "تاریخ",
        dataIndex: "createdAt",
        render: (date) => toShamsiString(date) // "1403/01/15" یا "-"
    }
];
```

## نکات مهم

1. **همیشه از این توابع استفاده کنید** - کد inline ننویسید
2. **JalaliDatePicker همیشه onChange با ensureShamsiDayjs استفاده کنید** - چون ممکن است تاریخ میلادی برگرداند
3. **هیچ وقت Date object نسازید** - فقط ISO string به API بفرستید
4. **در دیتابیس همیشه میلادی** - هیچ وقت شمسی ذخیره نکنید











