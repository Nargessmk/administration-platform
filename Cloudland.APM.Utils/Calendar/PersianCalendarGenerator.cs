using System.Globalization;

namespace Cloudland.APM.Utils.Calendar;

public static class PersianCalendarGenerator
{
    // تقویم‌ها
    private static readonly PersianCalendar PC = new PersianCalendar();
    private static readonly HijriCalendar HC = new HijriCalendar();

    /// <summary>
    /// تابع اصلی برای تولید تقویم یک سال شمسی کامل
    /// </summary>
    /// <param name="year">سال شمسی (مثلا 1403)</param>
    public static List<DayInfo> GetYearDays(int year)
    {
        var result = new List<DayInfo>();
        
        // تاریخ شروع: 1 فروردین سال درخواستی
        // نکته: تبدیل 1/1/Year شمسی به میلادی برای شروع حلقه
        DateTime startDate = PC.ToDateTime(year, 1, 1, 0, 0, 0, 0);
        
        // تشخیص کبیسه بودن برای تعداد روزهای اسفند
        int daysInYear = PC.IsLeapYear(year) ? 366 : 365;

        for (int i = 0; i < daysInYear; i++)
        {
            DateTime currentDt = startDate.AddDays(i);
            
            // اجزای شمسی
            int pYear = PC.GetYear(currentDt);
            int pMonth = PC.GetMonth(currentDt);
            int pDay = PC.GetDayOfMonth(currentDt);
            DayOfWeek dayOfWeek = PC.GetDayOfWeek(currentDt);

            // اجزای قمری (با یک روز ادجاسمنت معمول در تقویم ایران)
            // نکته: تقویم قمری محاسباتی ممکن است با رویت هلال 1 روز اختلاف داشته باشد
            // اینجا از الگوریتم استاندارد دات نت استفاده میکنیم
            // معمولا تقویم ایران +1 یا -1 روز با HijriCalendar دات نت تفاوت دارد.
            // برای سادگی ما همان تاریخ استاندارد را میگیریم.
            // برای دقت 100% شرعی نیاز به دیتابیس دستی است، اما برای محاسبات سال‌های آینده این بهترین تخمین است.
            DateTime hijriDtAdjusted = currentDt.AddDays(-1); // تنظیم تقریبی برای تطابق با تقویم رسمی ایران
            int hMonth = HC.GetMonth(hijriDtAdjusted);
            int hDay = HC.GetDayOfMonth(hijriDtAdjusted);

            // محاسبه اطلاعات روز
            bool isFriday = dayOfWeek == DayOfWeek.Friday;
            bool isThursday = dayOfWeek == DayOfWeek.Thursday;
            
            // تعیین نام روز هفته به فارسی
            string weekdayName = dayOfWeek switch
            {
                DayOfWeek.Saturday => "شنبه",
                DayOfWeek.Sunday => "یکشنبه",
                DayOfWeek.Monday => "دوشنبه",
                DayOfWeek.Tuesday => "سه‌شنبه",
                DayOfWeek.Wednesday => "چهارشنبه",
                DayOfWeek.Thursday => "پنج‌شنبه",
                DayOfWeek.Friday => "جمعه",
                _ => "نامشخص"
            };
            
            // بررسی تعطیلات مناسبتی و جمع‌آوری مناسبت‌ها
            bool isOccasionHoliday = false;
            List<string> occasions = new List<string>();

            // 1. مناسبت‌های شمسی (ثابت)
            var solarEvent = SolarEvents.FirstOrDefault(e => e.Month == pMonth && e.Day == pDay);
            if (solarEvent != null)
            {
                occasions.Add(solarEvent.Title);
                if (solarEvent.IsHoliday)
                {
                    isOccasionHoliday = true;
                }
            }

            // 2. مناسبت‌های قمری (چرخشی)
            var lunarEvent = LunarEvents.FirstOrDefault(e => e.Month == hMonth && e.Day == hDay);
            if (lunarEvent != null)
            {
                occasions.Add(lunarEvent.Title);
                if (lunarEvent.IsHoliday)
                {
                    isOccasionHoliday = true;
                }
            }

            // ساخت آبجکت روز
            var dayInfo = new DayInfo
            {
                Date = $"{pYear:0000}/{pMonth:00}/{pDay:00}",
                Title = weekdayName,
                Occasion = occasions.Count > 0 ? string.Join(" - ", occasions) : null,
                IsThursday = isThursday,
                IsFriday = isFriday,
                IsHoliday = isFriday || isOccasionHoliday
            };

            result.Add(dayInfo);
        }

        return result;
    }

    // ---------------------------------------------------------
    // دیتای مناسبت‌ها (Dictionary)
    // ---------------------------------------------------------

    private record EventItem(int Month, int Day, string Title, bool IsHoliday);

    // مناسبت‌های شمسی (ماه، روز، عنوان، آیا تعطیل است؟)
    private static readonly List<EventItem> SolarEvents = new List<EventItem>
    {
        // فروردین
        new(1, 1, "عید نوروز", true),
        new(1, 2, "عید نوروز", true),
        new(1, 3, "عید نوروز", true),
        new(1, 4, "عید نوروز", true),
        new(1, 12, "روز جمهوری اسلامی", true),
        new(1, 13, "روز طبیعت", true),
        new(1, 25, "روز بزرگداشت عطار", false),
        new(1, 29, "روز ارتش", false),
        
        // اردیبهشت
        new(2, 1, "روز بزرگداشت سعدی", false),
        new(2, 3, "روز بزرگداشت شیخ بهایی", false),
        new(2, 10, "روز ملی خلیج فارس", false),
        new(2, 12, "روز معلم", false),
        new(2, 25, "روز بزرگداشت فردوسی", false),
        new(2, 28, "روز بزرگداشت خیام", false),

        // خرداد
        new(3, 1, "روز بزرگداشت ملاصدرا", false),
        new(3, 14, "رحلت امام خمینی", true),
        new(3, 15, "قیام 15 خرداد", true),
        new(3, 31, "شهادت دکتر چمران", false),

        // تیر
        new(4, 1, "روز اصناف", false),
        new(4, 14, "روز قلم", false),
        
        // مرداد
        new(5, 1, "روز بزرگداشت فیض کاشانی", false),
        new(5, 14, "روز مشروطیت", false),
        new(5, 26, "سالروز بازگشت آزادگان", false),
        
        // شهریور
        new(6, 1, "روز پزشک (بزرگداشت ابن سینا)", false),
        new(6, 5, "روز داروساز (بزرگداشت رازی)", false),
        new(6, 13, "روز مردم‌شناسی (بزرگداشت ابوریحان)", false),
        new(6, 27, "روز شعر و ادب فارسی (شهریار)", false),
        new(6, 31, "آغاز هفته دفاع مقدس", false),

        // مهر
        new(7, 1, "آغاز سال تحصیلی", false),
        new(7, 7, "روز آتش نشانی", false),
        new(7, 8, "روز بزرگداشت مولوی", false),
        new(7, 20, "روز بزرگداشت حافظ", false),

        // آبان
        new(8, 8, "روز نوجوان", false),
        new(8, 13, "روز دانش آموز", false),
        new(8, 24, "روز کتاب و کتابخوانی", false),

        // آذر
        new(9, 7, "روز نیروی دریایی", false),
        new(9, 16, "روز دانشجو", false),
        new(9, 25, "روز پژوهش", false),
        new(9, 30, "شب یلدا", false),

        // دی
        new(10, 5, "سالروز زلزله بم", false),
        
        // بهمن
        new(11, 12, "بازگشت امام خمینی", false),
        new(11, 22, "پیروزی انقلاب اسلامی", true),
        new(11, 29, "روز اقتصاد مقاومتی", false),

        // اسفند
        new(12, 5, "روز مهندس (بزرگداشت خواجه نصیر)", false),
        new(12, 29, "روز ملی شدن صنعت نفت", true),
        // روز 30 اسفند در سال کبیسه تعطیل رسمی نیست اما آخرین روز سال است
    };

    // مناسبت‌های قمری (ماه قمری، روز قمری، عنوان، آیا تعطیل است؟)
    private static readonly List<EventItem> LunarEvents = new List<EventItem>
    {
        // محرم (Month 1)
        new(1, 9, "تاسوعای حسینی", true),
        new(1, 10, "عاشورای حسینی", true),
        new(1, 12, "شهادت امام سجاد (ع)", false),
        
        // صفر (Month 2)
        new(2, 20, "اربعین حسینی", true),
        new(2, 28, "رحلت پیامبر (ص) و شهادت امام حسن (ع)", true),
        new(2, 29, "شهادت امام رضا (ع)", true), // گاهی در تقویم 30 صفر است، هندل کردنش سخت است، معمولا روز آخر صفر است
        new(2, 30, "شهادت امام رضا (ع)", true), 

        // ربیع الاول (Month 3)
        new(3, 8, "شهادت امام حسن عسکری (ع)", true),
        new(3, 12, "میلاد پیامبر (ص) به روایت اهل سنت", false),
        new(3, 17, "میلاد پیامبر (ص) و امام صادق (ع)", true),

        // ربیع الثانی (Month 4)
        new(4, 8, "ولادت امام حسن عسکری (ع)", false),
        new(4, 10, "وفات حضرت معصومه (س)", false),

        // جمادی الاول (Month 5)
        new(5, 5, "ولادت حضرت زینب (س) - روز پرستار", false),

        // جمادی الثانی (Month 6)
        new(6, 3, "شهادت حضرت فاطمه (س)", true),
        new(6, 13, "وفات حضرت ام البنین (س)", false),
        new(6, 20, "ولادت حضرت فاطمه (س) - روز زن", false),

        // رجب (Month 7)
        new(7, 1, "ولادت امام محمد باقر (ع)", false),
        new(7, 3, "شهادت امام هادی (ع)", false),
        new(7, 10, "ولادت امام جواد (ع)", false),
        new(7, 13, "ولادت امام علی (ع) - روز پدر", true),
        new(7, 25, "شهادت امام موسی کاظم (ع)", false),
        new(7, 27, "مبعث پیامبر (ص)", true),

        // شعبان (Month 8)
        new(8, 3, "ولادت امام حسین (ع) - روز پاسدار", false),
        new(8, 4, "ولادت حضرت ابوالفضل (ع) - روز جانباز", false),
        new(8, 5, "ولادت امام سجاد (ع)", false),
        new(8, 11, "ولادت حضرت علی اکبر (ع) - روز جوان", false),
        new(8, 15, "ولادت امام زمان (عج) - نیمه شعبان", true),

        // رمضان (Month 9)
        new(9, 15, "ولادت امام حسن مجتبی (ع)", false),
        new(9, 19, "ضربت خوردن امام علی (ع)", false),
        new(9, 21, "شهادت امام علی (ع)", true),
        new(9, 23, "شب قدر", false),

        // شوال (Month 10)
        new(10, 1, "عید سعید فطر", true),
        new(10, 2, "تعطیلی عید فطر", true),
        new(10, 25, "شهادت امام جعفر صادق (ع)", true),

        // ذی القعده (Month 11)
        new(11, 1, "ولادت حضرت معصومه (س) - روز دختر", false),
        new(11, 11, "ولادت امام رضا (ع)", false), // در برخی ادارات مشهد تعطیل است اما رسمی کشوری نیست
        new(11, 30, "شهادت امام محمد تقی (ع)", false),

        // ذی الحجه (Month 12)
        new(12, 7, "شهادت امام محمد باقر (ع)", false),
        new(12, 9, "روز عرفه", false),
        new(12, 10, "عید سعید قربان", true),
        new(12, 15, "ولادت امام هادی (ع)", false),
        new(12, 18, "عید سعید غدیر خم", true),
        new(12, 24, "روز مباهله", false),
    };
}