# Backend API — ASP.NET Core (.NET 9)

Backend API cho hệ thống quản lý sản phẩm ly nhựa, nắp ly, đối tác và đơn hàng. Xây dựng trên ASP.NET Core .NET 9 với PostgreSQL và Cloudinary.

## Tech Stack

| Layer | Công nghệ |
|-------|-----------|
| Framework | ASP.NET Core .NET 9 |
| ORM | Entity Framework Core 9 |
| Database | PostgreSQL (Npgsql) |
| Image Storage | Cloudinary CDN |
| Image Processing | SixLabors.ImageSharp |
| API Docs | Swagger / OpenAPI |
| API Versioning | URL segment + header (`x-api-version`) |
| Notification | Telegram Bot |
| Container | Docker |

## Chức năng chính

- **Categories** — quản lý danh mục dạng cây (tree), hỗ trợ parent-child
- **Products** — CRUD sản phẩm với variant (dung tích), bảng giá theo mốc số lượng, ảnh avatar/gallery, liên kết nắp ly tương thích
- **Lids** — CRUD nắp ly với bảng giá theo đường kính, ảnh avatar/gallery
- **Partners** — CRUD đối tác với ảnh avatar/gallery
- **Orders** — tạo đơn hàng, tra cứu, quản lý trạng thái, thông báo Telegram
- **Image Management** — upload, validate, resize tự động, lưu trữ Cloudinary

## API Endpoints

Base URL: `/api/v1`

### Categories — `/api/v1/categories`

| Method | Endpoint | Content-Type | Mô tả |
|--------|----------|-------------|-------|
| GET | `/` | — | Danh sách (phân trang) |
| GET | `/tree` | — | Cây danh mục |
| GET | `/{id}` | — | Chi tiết |
| POST | `/` | `application/json` | Tạo mới |
| PUT | `/{id}` | `application/json` | Cập nhật |
| DELETE | `/{id}` | — | Xoá |

### Products — `/api/v1/products`

| Method | Endpoint | Content-Type | Mô tả |
|--------|----------|-------------|-------|
| GET | `/` | — | Danh sách (phân trang) |
| GET | `/{id}` | — | Chi tiết |
| POST | `/` | `multipart/form-data` | Tạo mới (với ảnh + variants) |
| PUT | `/{id}` | `application/json` | Cập nhật (không ảnh) |
| DELETE | `/{id}` | — | Xoá |
| GET | `/{id}/compatible-lids` | — | Nắp ly tương thích |
| POST | `/{id}/images` | `multipart/form-data` | Thêm ảnh |
| DELETE | `/{id}/images/{imgId}` | — | Xoá ảnh |

### Lids — `/api/v1/lids`

| Method | Endpoint | Content-Type | Mô tả |
|--------|----------|-------------|-------|
| GET | `/` | — | Danh sách (phân trang) |
| GET | `/{id}` | — | Chi tiết |
| POST | `/` | `multipart/form-data` | Tạo mới (với ảnh + giá) |
| PUT | `/{id}` | `application/json` | Cập nhật (không ảnh) |
| DELETE | `/{id}` | — | Xoá |
| POST | `/{id}/images` | `multipart/form-data` | Thêm ảnh |
| DELETE | `/{id}/images/{imgId}` | — | Xoá ảnh |

### Partners — `/api/v1/partners`

| Method | Endpoint | Content-Type | Mô tả |
|--------|----------|-------------|-------|
| GET | `/` | — | Danh sách (phân trang) |
| GET | `/{id}` | — | Chi tiết |
| POST | `/` | `multipart/form-data` | Tạo mới (với ảnh) |
| PUT | `/{id}` | `multipart/form-data` | Cập nhật (với ảnh) |
| DELETE | `/{id}` | — | Xoá |

### Orders — `/api/v1/orders`

| Method | Endpoint | Content-Type | Mô tả |
|--------|----------|-------------|-------|
| POST | `/` | `application/json` | Tạo đơn hàng |
| GET | `/track?orderId={id}&phone={phone}` | — | Tra cứu đơn hàng |
| GET | `/?status={status}` | — | Danh sách (phân trang, filter) |
| PATCH | `/{id}/status` | `application/json` | Cập nhật trạng thái |

**Luồng trạng thái:**

```
PendingConfirmation → Confirmed → Shipping → Completed
                    ↘ Cancelled
```

### Health Check

```
GET /health
```

## Image Upload

### Quy tắc validation

| Quy tắc | Giá trị |
|----------|---------|
| Định dạng | `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif` |
| Content-Type | `image/jpeg`, `image/png`, `image/webp`, `image/gif` |
| Dung lượng tối đa / file | 10 MB |
| Số lượng gallery / lần upload | 10 ảnh |
| Tổng request body | 50 MB |

### Xử lý tự động

| Thuộc tính | Avatar | Gallery |
|------------|--------|---------|
| Max kích thước | 1200x1200 px | 1000x1000 px |
| Resize | Proportional (max fit) | Proportional (max fit) |
| Output format | JPEG 82% (PNG giữ nguyên) | JPEG 82% (PNG giữ nguyên) |

## Cấu trúc dự án

```
├── Controllers/          # HTTP endpoints
├── Services/             # Business logic
│   └── Interfaces/       # Service contracts
├── Models/               # Entities + enums
├── Data/                 # EF Core DbContext + Migrations
├── Extensions/           # DI registration + middleware pipeline
├── Infrastructure/       # Options classes (Cloudinary, ImageProcessing, Telegram)
├── docs/                 # API integration guides
├── Dockerfile
├── appsettings.json
└── Program.cs
```

## Database Schema

```
categories
├── id, name, description, parent_id (self-ref FK)

products
├── id, name, description, category_id (FK), avatar_image_url
├── product_variants → id, product_id, capacity_ml, diameter_mm
│   └── variant_price_tiers → id, variant_id, min_quantity, unit_price
├── product_images → id, product_id, image_url, image_type, display_order
└── product_lids → product_id (FK), lid_id (FK)

lids
├── id, name, description, category_id (FK), avatar_image_url
├── lid_images → id, lid_id, image_url, image_type, display_order
└── lid_prices → id, lid_id, diameter_mm, size_name, unit_price

partners
├── id, name, address, phone_number, description, avatar_image_url
└── partner_images → id, partner_id, image_url, display_order

orders
├── id, customer_name, customer_phone, customer_email, note, total_amount, status
└── order_items → id, order_id, product_id, quantity, unit_price, material_id, print_type_id
```

## Chạy local

### Yêu cầu

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL
- Tài khoản Cloudinary

### Cấu hình

Tạo file `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://user:password@localhost/dbname?sslmode=prefer"
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret",
    "Folder": "products"
  },
  "Telegram": {
    "BotToken": "your-bot-token",
    "ChatId": "your-chat-id"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### Khởi chạy

```bash
dotnet restore
dotnet run
```

Truy cập Swagger UI: [http://localhost:5000/swagger](http://localhost:5000/swagger)

Database migration tự động chạy khi khởi động.

## Docker

### Build & Run

```bash
docker build -t backend-api .
docker run -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=postgresql://..." \
  -e "Cloudinary__CloudName=..." \
  -e "Cloudinary__ApiKey=..." \
  -e "Cloudinary__ApiSecret=..." \
  backend-api
```

### Environment Variables

| Variable | Mô tả |
|----------|-------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Cloudinary__CloudName` | Cloudinary cloud name |
| `Cloudinary__ApiKey` | Cloudinary API key |
| `Cloudinary__ApiSecret` | Cloudinary API secret |
| `Cloudinary__Folder` | Folder gốc trên Cloudinary (mặc định: `products`) |
| `Telegram__BotToken` | Telegram Bot token |
| `Telegram__ChatId` | Telegram Chat ID nhận thông báo |
| `Cors__AllowedOrigins__0` | Origin được phép (CORS) |
| `ASPNETCORE_URLS` | URL binding (mặc định: `http://0.0.0.0:8080`) |

## Tài liệu API

- [API Integration Guide v2](docs/api-integration-guide-v2.md) — hướng dẫn tích hợp đầy đủ với code examples
- [Image Management Guide](docs/api-image-management.md) — chi tiết về upload/quản lý ảnh
- [Partner API Guide](docs/partner-api-integration-guide.md) — hướng dẫn API đối tác
- Swagger UI: `/swagger`
