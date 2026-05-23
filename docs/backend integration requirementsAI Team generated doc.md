# وثيقة متطلبات تقنية: تكاملات الباك اند (AiGenda AI Team)

## 1. نظرة عامة
يعمل AI Agent في AiGenda على Google ADK. لا يتواصل الوكيل مع GitHub أو Gmail مباشرة؛ بدلاً من ذلك، تقوم كل أداة (tool) في الوكيل بإرسال طلب HTTP إلى Backend API، والذي يتولى بدوره التواصل مع الخدمات الخارجية[cite: 1].

## 2. القواعد العامة للـ Endpoints
- **تنسيق الاستجابة الموحد:** يجب أن تعيد جميع الـ Endpoints استجابة JSON بالتنسيق التالي[cite: 1]:
    - نجاح: `{ "status": "success", "data": { ... } }`[cite: 1]
    - فشل: `{ "status": "error", "message": "سبب المشكلة" }`[cite: 1]
    - *ملاحظة:* كلمة "error" أو "failed" هي المحرك لـ retry logic في الوكيل[cite: 1].
- **المصادقة (Authentication):**
    - الوكيل يرسل `user_id` مع كل طلب[cite: 1].
    - الباك اند مسؤول عن تخزين واستخدام OAuth tokens الخاصة بكل مستخدم[cite: 1].
    - الـ Agent لا يرى التوكنز؛ كل العمليات تتم عند الباك اند[cite: 1].
- **الرأس (Headers) المطلوب:**
    - `Authorization: Bearer <user_jwt_token>`[cite: 1]
    - `Content-Type: application/json`[cite: 1]
- **Base URL:** `https://api.igenda.app/integrations/v1/`[cite: 1]

## 3. الـ Endpoints المطلوبة
### GitHub[cite: 1]
- `GET /github/issues`: جلب الـ issues (Parameters: repo, state, user_id)[cite: 1].
- `GET /github/prs`: جلب الـ Pull Requests (Parameters: repo, state, user_id)[cite: 1].
- `POST /github/issues`: إنشاء issue جديد (Parameters: repo, title, body, labels, user_id)[cite: 1].
- `POST /github/issues/close`: إغلاق issue (Parameters: repo, issue_number, user_id)[cite: 1].
- `GET /github/repos`: جلب مستودعات المستخدم (Parameters: user_id)[cite: 1].

### Gmail[cite: 1]
- `POST /gmail/send`: إرسال إيميل (Parameters: to, subject, body, cc, user_id)[cite: 1].
- `GET /gmail/inbox`: جلب الـ inbox (Parameters: max_results, query, user_id)[cite: 1].
- `GET /gmail/message`: جلب تفاصيل إيميل معين (Parameters: message_id, user_id)[cite: 1].
- `POST /gmail/reply`: الرد على إيميل (Parameters: message_id, body, user_id)[cite: 1].
- `POST /gmail/draft`: حفظ مسودة (Parameters: to, subject, body, user_id)[cite: 1].

### Google Calendar[cite: 1]
- `GET /calendar/events`: جلب الأحداث (Parameters: date_from, date_to, user_id)[cite: 1].
- `POST /calendar/events`: إنشاء حدث جديد (Parameters: title, start, end, description, location, user_id)[cite: 1].
- `PUT /calendar/events`: تعديل حدث (Parameters: event_id, title, start, end, user_id)[cite: 1].
- `DELETE /calendar/events`: حذف حدث (Parameters: event_id, user_id)[cite: 1].

## 4. نظام OAuth المطلوب
- الباك اند مسؤول عن دورة OAuth بالكامل[cite: 1]:
    - `GET /integrations/connect/{service}?user_id=xxx`: لبدء الاتصال[cite: 1].
    - معالجة الـ Callback وتخزين الـ Access/Refresh tokens لكل `user_id`[cite: 1].
    - تجديد التوكنز تلقائياً عند انتهائها[cite: 1].
- `GET /integrations/status?user_id=xxx`: لمعرفة الخدمات المتصلة (الاستجابة ترجع true/false لكل خدمة)[cite: 1].

## 5. أكواد الخطأ المتوقعة (HTTP Codes)
- `401`: Unauthorized (التوكن منتهي أو غير موجود)[cite: 1].
- `403`: Integration Missing (المستخدم لم يربط الخدمة بعد)[cite: 1].
- `404`: Not Found[cite: 1].
- `429`: Rate Limited (من الـ API الخارجية)[cite: 1].
- `500`: Internal Error[cite: 1].