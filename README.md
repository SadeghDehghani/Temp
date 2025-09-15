# HR Workflow (ASP.NET Core MVC + EF Core + SQL Server)

سامانه مدیریت درخواست‌های پرسنلی با موتور گردش‌کار داینامیک.

- ASP.NET Core MVC (.NET 8)
- EF Core Code First + SQL Server
- Bootstrap 5 و jQuery
- دیتاتیبل‌ها با DataTables.js در همهٔ لیست‌ها

## پیش‌نیازها
- Docker (برای SQL Server) یا یک SQL Server قابل دسترس
- .NET 8 SDK (در محیط پروژه نصب محلی شده است)

## اجرای SQL Server با Docker
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_strong_password123" \
  -p 1433:1433 --name mssql -d mcr.microsoft.com/mssql/server:2022-latest
```

ConnectionString پیش‌فرض در `appsettings.json` تنظیم شده است:
```
Server=localhost;Database=HrWorkflowDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;MultipleActiveResultSets=True;
```
در صورت نیاز آن را تغییر دهید.

## ایجاد دیتابیس و اجرای برنامه
از ریشهٔ پروژه (`/workspace/HrWorkflow`):
```bash
export DOTNET_ROOT=/workspace/.dotnet
export PATH="$DOTNET_ROOT:$PATH:/home/ubuntu/.dotnet/tools"

dotnet build
# فقط یکبار لازم است تا اسکیما ایجاد شود (SQL Server باید در حال اجرا باشد)
dotnet ef database update

# اجرای برنامه
dotnet run
```
سپس در مرورگر: `http://localhost:5000`

## بخش‌ها
- کارمندان: CRUD کامل + نمایش با DataTables
- گروه‌های تایید: CRUD + مدیریت اعضا + DataTables
- انواع درخواست: CRUD + DataTables
- درخواست‌ها: ثبت، مشاهده، شروع گردش‌کار، اقدام (Approve/Reject/Return)، پیگیری مراحل به تفکیک گام‌ها

## موتور گردش‌کار (خلاصه)
- مدل‌های «تعریف» (Definition) برای گام‌ها و ترنزیشن‌ها و «نمونه» (Instance) برای اجرای هر درخواست.
- سرویس `IWorkflowEngine` شامل:
  - `StartWorkflowAsync(requestId)` شروع گردش‌کار بر اساس تعریف فعال نوع درخواست
  - `AdvanceAsync(instanceId, actionName, actorEmployeeId, comment)` پیش‌برد به گام بعدی با اعتبارسنجی گروه اقدام‌کننده
  - `GetAvailableActionsAsync(...)` فهرست اقدامات مجاز کاربر در گام جاری
- امکان رصد تاریخچهٔ گام‌ها در صفحهٔ جزئیات درخواست.

## نکات UI
- فقط از Bootstrap 5 و jQuery استفاده شده است.
- برای همهٔ جدول‌ها از DataTables (CDN) استفاده شده و زبان فارسی اعمال شده است.

## توسعه بعدی (پیشنهاد)
- صفحهٔ مدیریت «تعاریف گردش‌کار» (ایجاد/ویرایش گام‌ها و ترنزیشن‌ها) از داخل پنل
- کنترل دسترسی مبتنی بر کاربر لاگین‌شده به جای انتخاب دستی Actor
- Rule/Condition Builder برای `ConditionExpression` ترنزیشن‌ها