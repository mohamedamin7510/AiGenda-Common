# 1. إعدادات أسماء المشروع والمجلدات
$projectName = "AIgenda_API"
$targetProject = "./AI_genda_API.csproj"

# الحل المعماري: نقل مجلد الريليز خطوة للخلف بره فولدر المشروع تماماً لمنع الـ dotnet من قراءته كـ Content
$releasesFolder = "../AIgenda_Releases"

# 2. طلب رقم الإصدار (Versioning)
$version = Read-Host "Enter Release Version (e.g., 1.0.1)"
if ([string]::IsNullOrWhiteSpace($version)) { $version = "1.0.0" }

# تحديد المسارات بناء على الرقم المدخل بره نطاق السورس كود
$outputDir = "$releasesFolder/v$version"
$zipFileName = "$releasesFolder/${projectName}_v${version}.zip"

# 3. خطوة حاسمة: تنظيف أي بقايا قديمة لنفس الفيرجن لمنع تراكم الأنقاض
if (Test-Path $outputDir) {
    Write-Host " Found existing folder for v$version. Cleaning it up..." -ForegroundColor Yellow
    Remove-Item -Path $outputDir -Recurse -Force
}
if (Test-Path $zipFileName) {
    Remove-Item -Path $zipFileName -Force
}

Write-Host " Starting Fresh Build and Clean for Release..." -ForegroundColor Cyan
# تنظيف ملفات الـ bin والـ obj اللوكال القديمة لضمان عدم وجود كاش مهنج
dotnet clean $targetProject -c Release

Write-Host " Starting Build and Publish for v$version..." -ForegroundColor Green
# عمل Publish للإصدار الحالي في المجلد الخارجي الآمن
dotnet publish $targetProject -c Release -o $outputDir /p:Version=$version

# 4. ضغط الملفات في ZIP يحمل رقم الفيرجن
Write-Host " Compressing build artifact into ZIP..." -ForegroundColor Green
Compress-Archive -Path "$outputDir\*" -DestinationPath $zipFileName -Force

Write-Host " Done! Your clean release is ready at: $zipFileName" -ForegroundColor Green
Write-Host " Old versions are preserved safely outside the project directory."