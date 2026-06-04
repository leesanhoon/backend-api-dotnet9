# Hướng dẫn làm việc với EF Migrations

Tài liệu này hướng dẫn cách khởi tạo và cập nhật schema database cho dự án `backend-api-dotnet9` mà không làm mất dữ liệu.

## Nguyên tắc chính

- Không dùng `EnsureCreated()` để khởi tạo database khi đã có nhu cầu thay đổi schema.
- Mỗi lần thêm/sửa/xóa field trong entity, hãy tạo migration mới.
- Không xóa database trên server nếu mục tiêu chỉ là cập nhật schema.
- Luôn backup database trước khi deploy thay đổi lớn.

## Khởi tạo lần đầu

Khi database chưa có table nào:

1. Tạo database rỗng trên PostgreSQL.
2. Cập nhật `ConnectionStrings:DefaultConnection` trong `appsettings.Development.json` hoặc biến môi trường trên server.
3. Chạy ứng dụng.
4. Ứng dụng sẽ gọi `dbContext.Database.Migrate()` và tự tạo table theo migration hiện có.

## Khi thêm field mới

Ví dụ bạn thêm `AvatarImageUrl` vào `Product`:

1. Cập nhật model/entity trong code.
2. Tạo migration mới:
   - `dotnet ef migrations add AddProductAvatarImageUrl`
3. Kiểm tra file migration được sinh ra.
4. Áp migration:
   - local: `dotnet ef database update`
   - server: chạy app để `Database.Migrate()` tự cập nhật schema, hoặc chạy lệnh update riêng trong pipeline.

## Khi đổi tên field

Nếu chỉ đổi tên cột mà muốn giữ dữ liệu cũ:

- Ưu tiên dùng `RenameColumn` trong migration.
- Không xóa rồi tạo lại cột nếu dữ liệu cần giữ.

## Khi xóa field

Trước khi xóa cột:

- Kiểm tra code không còn dùng field đó.
- Cân nhắc deploy theo 2 bước:
  1. Ngừng ghi vào field cũ.
  2. Sau đó mới xóa cột bằng migration tiếp theo.

## Quy trình an toàn khi deploy

- Backup database.
- Tạo migration trên máy local.
- Review nội dung migration.
- Commit cả code và migration.
- Deploy ứng dụng.
- Kiểm tra log startup để নিশ্চিত `Database.Migrate()` chạy thành công.

## Lưu ý cho repo này

- `Extensions/ApplicationBuilderExtensions.cs` đang dùng `Database.Migrate()`.
- Điều này giúp app tự cập nhật schema khi khởi động.
- Database vẫn phải tồn tại sẵn; `Migrate()` không tự tạo database server mới.

## Lệnh hữu ích

- Tạo migration: `dotnet ef migrations add TenMigrationMoi`
- Cập nhật database: `dotnet ef database update`
- Build dự án: `dotnet build`

## Kết luận

Mỗi khi thay đổi schema, hãy đi theo chu trình:

`Sửa entity -> Tạo migration -> Kiểm tra migration -> Cập nhật DB -> Deploy`

Làm đúng chu trình này sẽ tránh mất dữ liệu và giúp server luôn đồng bộ với code.
