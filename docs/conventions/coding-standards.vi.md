# Quy tắc Coding — Tóm tắt nhanh

**Phạm vi:** bản tóm tắt tiếng Việt, súc tích, dành cho dev khi mới tham gia hoặc cần tra cứu nhanh trước khi sửa code. Đây **không phải** tài liệu quy tắc gốc — tài liệu gốc (đầy đủ, có ví dụ, có lý do) luôn là bản tiếng Anh dưới đây; khi có mâu thuẫn, bản tiếng Anh thắng:

- [04-coding-rules.md](../04-coding-rules.md) — naming, CQRS, endpoint, DI, cache, transaction (đặc thù NovaCore)
- [personal-coding-standards.md](personal-coding-standards.md) — style C# tổng quát, tái sử dụng được cho project khác
- [domain-coding-conventions.md](domain-coding-conventions.md) / [application-coding-conventions.md](application-coding-conventions.md) / [persistence-coding-conventions.md](persistence-coding-conventions.md) — quy tắc theo từng layer

## 1. Phần nào áp dụng chung, phần nào chỉ riêng NovaCore

| Nhóm | Dùng được ở project khác? |
|---|---|
| Syntax C#/.NET hiện đại, đặt tên boolean, tránh hardcode, XML doc, thứ tự member, region, comma cuối enum | ✅ Có — xem `personal-coding-standards.md` |
| Transaction (`ExecuteTransactionAsync`), `SaveChangesAsync`, Repository/Read-Write Service, DbContext | ❌ Không — đặc thù kiến trúc NovaCore, xem `persistence-coding-conventions.md` |

## 2. Syntax — luôn dùng bản mới nhất project hỗ trợ

Project target **.NET 10 / C# 13** (`Directory.Build.props`). Ưu tiên collection expression (`List<string> x = [];`), primary constructor, pattern matching... thay vì cú pháp cũ làm được việc tương tự. Đừng viết code "trông cũ hơn" version project đang dùng.

## 3. Dữ liệu ngoài (API/MongoDB) — không để tên field ngoài "rò rỉ" vào C#

Property C# luôn PascalCase theo convention C#, kể cả khi nguồn dữ liệu dùng `lowerCamelCase`. Map tường minh bằng attribute:

```csharp
[JsonPropertyName("externalPropertyName")]
public string ExternalPropertyName { get; set; }
```

## 4. Code mới không được có warning

Sửa warning ngay khi viết, không để "biên dịch được là xong". Suppress warning chỉ khi thật sự cần và phải ghi rõ lý do.

## 5. Comment / XML doc — ngắn gọn, đúng chỗ

- **Property đơn giản:** 1 dòng — `/// <summary>Current stock quantity.</summary>`.
- **Property phức tạp** (nhiều giá trị hợp lệ, có ý nghĩa riêng): dùng khối nhiều dòng, liệt kê `Values: 1. ... 2. ...` — không trộn 1 dòng với nhiều dòng trong cùng 1 comment.
- **Method/class:** luôn dùng khối nhiều dòng (`<summary>`, `<param>`, `<returns>` mỗi tag một dòng riêng), kể cả khi chỉ có 1 tham số — để sau này thêm tham số chỉ cần thêm 1 dòng, không phải format lại cả khối (diff Git sạch hơn).
- `<summary>` là **tiêu đề**, tối đa 1–2 câu — không giải thích cách implement.
- `CancellationToken` **không cần** `<param>` riêng, nhưng vẫn phải viết đủ `<summary>`/`<returns>` cho method đó.
- `<remarks>` chỉ dùng khi có hành vi đặc biệt không nhét vừa vào `<summary>`.
- **Không bắt buộc comment** cho: CQRS handler thông thường, repository method quen thuộc (`GetById`, `GetList`, `Search`), CRUD hiển nhiên, property tên đã tự giải thích.
- **Bắt buộc comment** cho: class/method nghiệp vụ, logic custom, tên method viết tắt/mơ hồ, business rule quan trọng, persistence method custom (giải thích **lý do tồn tại**, không lặp lại tên method).

## 6. Thứ tự thành phần trong class (entity/domain)

1. Const/field cục bộ → 2. Property của entity → 3. Navigation property → 4. Static factory method → 5. Constructor → 6. Public/protected method → 7. Private method/utility → 8. Property runtime/lifecycle (không thuộc dữ liệu domain)

## 7. Region

Chỉ dùng khi class đủ lớn để việc nhóm giúp ích thật sự — không thêm region máy móc cho class nhỏ. Region có ý nghĩa nên có 1 dòng chú thích ngắn giải thích nhóm đó là gì.

## 8. Format code (tham khảo đầy đủ ở `04-coding-rules.md#formatting`)

- Xuống dòng tham số khi lời gọi/khai báo dài hoặc có > 2 tham số — xuống dòng **toàn bộ** danh sách tham số, không chỉ xuống dòng tham số bị tràn màn hình.
- Dấu `)` đóng luôn nằm cùng dòng với tham số cuối.
- Enum: **luôn có dấu phẩy sau giá trị cuối** — để thêm giá trị mới sau này không phải sửa dòng trước đó.

## 9. Đặt tên boolean

Property/method trả `bool` nên đọc như một câu hỏi: `IsActive`, `CanManage`, `HasPermission`, `HasChildren` — tránh tên mơ hồ không rõ là boolean.

## 10. Tránh hardcode

Giá trị/chuỗi có ý nghĩa, dùng lặp lại từ 2 nơi trở lên → phải có **một nơi khai báo duy nhất** (constant có sẵn, hoặc tạo constant mới đúng chỗ). Danh sách/dictionary cố định dùng để tra cứu → khai báo `static readonly` một lần, không dựng lại mỗi lần gọi method.

## 11. Quy tắc riêng của NovaCore (không mang sang project khác)

- **Transaction:** mở transaction qua `IUnitOfWork.ExecuteTransactionAsync`, mở tại đúng thời điểm bắt đầu ghi dữ liệu (không mở sớm chỉ vì có validate/đọc dữ liệu trước đó).
- **`SaveChangesAsync`:** là chi tiết EF, ở lại trong Persistence layer. Write Service không tự gọi `ExecuteTransactionAsync` — ranh giới transaction luôn thuộc về caller (Application handler).
- **`DbContext`:** không inject thẳng vào khắp Persistence Service — đi qua Repository abstraction; chỉ mở rộng Repository khi generic repo không đáp ứng được nhu cầu truy vấn/ghi cụ thể.

Chi tiết đầy đủ + ví dụ code: xem [04-coding-rules.md](../04-coding-rules.md#transaction-management) và [persistence-coding-conventions.md](persistence-coding-conventions.md).
