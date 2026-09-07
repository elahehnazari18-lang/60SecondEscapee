# 60 Second Escape — GameCI Android APK

این نسخه برای Build ابری با GitHub Actions + GameCI آماده شده است.

## Build APK
- پروژه را به یک GitHub repository منتقل کنید.
- فایل `.github/workflows/build-apk.yml` را نگه دارید.
- در GitHub Actions، workflow با نام `Build 60 Second Escape APK` را اجرا کنید.
- Unity License باید طبق راهنمای GameCI در GitHub Secrets تنظیم شود.
- خروجی در Artifacts با نام `60SecondEscape-APK` قرار می‌گیرد.

## English
This Unity project is prepared for Android APK builds with GitHub Actions and GameCI.
The workflow targets Android and requests `androidPackage`, which produces an APK.

Unity version:
2022.3.62f1

Output artifact:
60SecondEscape-APK

Important:
A Unity license/activation secret is required by GameCI. Do not put passwords or license secrets inside project files.
