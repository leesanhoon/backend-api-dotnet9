# API Backend - ASP.NET Core .NET 9

Backend API phục vụ dự án bán lẻ và in ấn, xây dựng bằng ASP.NET Core .NET 9.

## Chức năng chính

- Quản lý danh mục sản phẩm (Category): CRUD đầy đủ
- Quản lý sản phẩm (Product): CRUD + upload ảnh avatar/gallery qua Cloudinary
- Quản lý đơn hàng (Order): tạo, xem, cập nhật trạng thái, xóa
- Phân trang (Pagination) cho tất cả danh sách

## Công nghệ

- ASP.NET Core .NET 9
- EF Core + PostgreSQL
- Cloudinary (lưu trữ ảnh)
- Swagger (Development)
- API versioning: URL segment (`/api/v1/...`) + header (`x-api-version`)

## API Endpoints

### Categories — `/api/v1/categories`
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `?page=1&pageSize=10` | Danh sách (phân trang) |
| GET | `/{id}` | Chi tiết |
| POST | `/` | Tạo mới |
| PUT | `/{id}` | Cập nhật |
| DELETE | `/{id}` | Xóa (phải xóa sản phẩm liên kết trước) |

### Products — `/api/v1/products`
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `?page=1&pageSize=10` | Danh sách (phân trang) |
| GET | `/{id}` | Chi tiết |
| POST | `/` | Tạo mới (`multipart/form-data`) |
| PUT | `/{id}` | Cập nhật (JSON) |
| DELETE | `/{id}` | Xóa |

### Orders — `/api/v1/orders`
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `?page=1&pageSize=10` | Danh sách (phân trang) |
| GET | `/{id}` | Chi tiết |
| POST | `/` | Tạo đơn hàng |
| PUT | `/{id}/status` | Cập nhật trạng thái |
| DELETE | `/{id}` | Xóa (chỉ đơn draft) |

**Luồng trạng thái đơn hàng:**
```
draft → confirmed → shipping → completed
draft → cancelled
```

### Health check: `/health`

## Cấu trúc source code

- `Controllers/` — xử lý HTTP request/response
- `Services/` — business logic
- `Services/Interfaces/` — service contracts
- `Data/AppDbContext.cs` — EF Core DbContext
- `Models/` — entities + DTOs
- `Extensions/` — cấu hình service và middleware
- `Infrastructure/` — helpers (connection string, Cloudinary, image processing)

## Database Schema

1. **categories** — `id`, `name` (unique), `description`
2. **products** — `id`, `category_id` (FK), `name`, `description`, `price`, `stock_quantity`, `avatar_image_url`
3. **product_images** — `id`, `product_id` (FK), `image_url`, `image_type`, `display_order`, `created_at_utc`
4. **orders** — `id`, `customer_name`, `customer_phone`, `customer_email`, `note`, `total_amount`, `status`, `created_at_utc`
5. **order_items** — `id`, `order_id` (FK), `product_id` (FK), `quantity`, `unit_price`

## Chạy local

```bash
dotnet restore
dotnet run
```

## Cấu hình môi trường

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
ConnectionStrings__DefaultConnection=postgresql://<username>:<password>@<host>/<database>?sslmode=require
```

## Docker deploy (Render)

- Có sẵn `Dockerfile`
- Port expose: `8080`
