<div dir="rtl">

# سامانه پشتیبان خدمات اجتماعی (Social Service System Backend)

این پروژه، بخش بک‌اند (Backend) سامانه جامع مدیریت خدمات اجتماعی (SSSB) است. این سیستم برای مدیریت متمرکز منابع، پرسنل، امکانات، قراردادها، خدمات (مانند تغذیه و حمل و نقل) و فرآیندهای رزرو در سازمان‌های خدمات اجتماعی طراحی شده است.

معماری این پروژه بر اساس **میکروسرویس (Microservices)** بنا شده و از یک **API Gateway (Ocelot)** برای مدیریت و مسیریابی درخواست‌ها به سرویس‌های مختلف استفاده می‌کند.

## معماری و تکنولوژی‌های اصلی

این پروژه با استفاده از پلتفرم .NET (احتمالاً .NET 6, 7 یا 8) توسعه یافته و از الگوها و ابزارهای مدرن زیر بهره می‌برد:

* **پلتفرم:** .NET Core / .NET 5+
* **زبان:** C#
* **معماری:** میکروسرویس (Microservices)
* **API Gateway:** [Ocelot](https://github.com/ThreeMammals/Ocelot)
* **دسترسی به داده (ORM):** Entity Framework Core
* **الگوی طراحی:** CQRS (Command Query Responsibility Segregation) - (بر اساس ساختار پوشه‌های `CQRS`)
* **لاگینگ (Logging):** [Serilog](https://serilog.net/) (با قابلیت ارسال به Elasticsearch)
* **کشینگ (Caching):** [Redis](https://redis.io/) (بر اساس فایل `RedisExtensions.cs`)
* **کانتینرسازی:** [Docker](https://www.docker.com/) (به دلیل وجود `Dockerfile` ها)
* **احراز هویت:** JWT (JSON Web Tokens)
* **وظایف پس‌زمینه (Background Tasks):** [Hangfire](https://www.hangfire.io/) (بر اساس فایل `HangfireDashboardJwtAuthorizationFilter.cs`)
* **جستجو:** [Elasticsearch](https://www.elastic.co/elasticsearch/) (برای لاگ‌ها و احتمالاً جستجوی داده‌ها)

## ساختار پروژه (میکروسرویس‌ها)

این راهکار (Solution) شامل چندین پروژه مجزا است که هر کدام مسئولیت بخشی از سیستم را بر عهده دارند:

### میکروسرویس‌های اصلی (SSSB)

* **Cloudland.SSSB.Microservices.AppManagement:**
    * سرویس اصلی مدیریت برنامه برای دامنه "خدمات اجتماعی".
    * شامل مدیریت موجودیت‌های کلیدی مانند: `Location` (مکان)، `Room` (اتاق)، `Bed` (تخت)، `Contractor` (پیمانکار)، `Contract` (قرارداد)، `Meal` (وعده غذایی)، `Food` (غذا)، `Personnel` (پرسنل)، `Vehicle` (خودرو) و مدیریت درخواست‌های رزرو (`ReservationRequestHeader`).

### میکروسرویس‌های پلتفرم (APM)

* **Cloudland.APM.Microservices.Gateway:**
    * نقش API Gateway را ایفا می‌کند و از Ocelot برای مسیریابی درخواست‌ها به سرویس‌های داخلی استفاده می‌کند (تنظیمات در `ocelot.json`).
* **Cloudland.APM.Microservices.Authentication:**
    * سرویس اختصاصی برای مدیریت احراز هویت، صدور توکن JWT و مدیریت کاربران.
* **Cloudland.APM.Microservices.AppManagement:**
    * سرویس مدیریت برنامه پلتفرم (احتمالاً مدیریت تنظیمات پایه).
* **Cloudland.APM.Microservices.AppPermissionManagement:**
    * سرویس مدیریت دسترسی‌ها و مجوزها.
* **Cloudland.APM.Microservices.ReportGenerator / ReportViewer:**
    * میکروسرویس‌های اختصاصی برای تولید و نمایش گزارش‌ها.
* **Cloudland.APM.Microservices.ChatGPT / LocalChatGPT:**
    * سرویس‌های یکپارچه‌سازی با مدل‌های زبانی (AI).
* **Cloudland.APM.BackgroundTask:**
    * سرویس اجرای وظایف زمان‌بندی شده و پس‌زمینه (مانند Hangfire).

### کتابخانه‌های مشترک (Shared Libraries)

* **Cloudland.APM.Models / Cloudland.SSSB.Models:**
    * شامل مدل‌های داده (Data Models) و موجودیت‌های EF Core.
* **Cloudland.APM.DTO / Cloudland.SSSB.DTO:**
    * شامل آبجکت‌های انتقال داده (Data Transfer Objects) که بین لایه‌ها و سرویس‌ها استفاده می‌شوند.
* **Cloudland.APM.DataBaseContext / Cloudland.SSSB.DataBaseContext:**
    * شامل `DbContext` های مربوط به Entity Framework Core.
* **Cloudland.APM.Repository:**
    * پیاده‌سازی الگوی Repository برای جداسازی منطق دسترسی به داده.
* **Cloudland.APM.Mapper.Profiles / Cloudland.SSSB.Mapper.Profiles:**
    * تنظیمات AutoMapper برای نگاشت DTO ها به Model ها و بالعکس.

## پیش‌نیازها

برای اجرای این پروژه به نرم‌افزارهای زیر نیاز دارید:

* [.NET SDK](https://dotnet.microsoft.com/download) (نسخه 6.0 یا بالاتر)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (برای اجرای کانتینرها)
* [docker-compose](https://docs.docker.com/compose/install/)
* یک سرور پایگاه داده (مانند SQL Server یا PostgreSQL)
* سرور Redis (برای کش)
* سرور Elasticsearch (برای لاگ)

## راه‌اندازی و اجرا (با استفاده از Docker)

این پروژه برای اجرا با Docker و docker-compose بهینه‌سازی شده است. (در صورتی که فایل `docker-compose.yml` در ریشه پروژه وجود داشته باشد):

1.  **تنظیمات:**
    * فایل‌های `appsettings.json` یا `appsettings.Development.json` را در هر میکروسرویس (به خصوص `Authentication`، `Gateway` و `...AppManagement`) باز کنید.
    * کانکشن استرینگ‌های مربوط به پایگاه داده (مانند `DefaultConnection`)، آدرس Redis و آدرس Elasticsearch را مطابق با محیط خود تنظیم کنید.

2.  **اجرا با Docker Compose:**
    در ترمینال، به ریشه پروژه (جایی که فایل `docker-compose.yml` قرار دارد) بروید و دستور زیر را اجرا کنید:

    ```bash
    docker-compose up -d --build
    ```

3.  **اجرای Migration ها (در صورت استفاده از EF Core):**
    پس از بالا آمدن سرویس‌ها، ممکن است نیاز باشد Migration های دیتابیس را اجرا کنید. این کار معمولاً از طریق `dotnet ef database update` در پروژه‌های `DataBaseContext` انجام می‌شود یا در خط لوله CI/CD مدیریت می‌گردد.

## دسترسی به API

پس از اجرای موفقیت‌آمیز، تمامی درخواست‌ها باید به **API Gateway** ارسال شوند. آدرس پایه Gateway (مثلاً `http://localhost:5000`) را می‌توانید از فایل `launchSettings.json` در پروژه `Cloudland.APM.Microservices.Gateway` پیدا کنید.

فایل `ocelot.json` در همان پروژه، نحوه مسیریابی درخواست‌ها از Gateway به میکروسرویس‌های داخلی را مشخص می‌کند.

</div>
