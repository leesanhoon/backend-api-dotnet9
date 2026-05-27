# API Ban Ly Va In Ly Nhua

Backend API duoc xay dung bang ASP.NET Core .NET 9, phuc vu du an ban ly nhua va in ly theo yeu cau thiet ke.

## Muc tieu nghiep vu

API phuc vu cac nhu cau:

- Quan ly danh muc san pham (category): ly, to, ong hut, bao bi, giay in, ...
- Quan ly san pham theo tung loai va thuoc tinh chi tiet.
- Cau hinh vat lieu, kieu in (1 mau, nhieu mau), va thong tin file thiet ke in.
- Mo rong de tao bao gia, don hang, va quy trinh san xuat/in an.

## Cong nghe va kien truc

- ASP.NET Core .NET 9
- EF Core + PostgreSQL (Neon hoac PostgreSQL thong thuong)
- Swagger (chi bat o moi truong Development)
- API versioning: URL segment (`/api/v1/...`) + header (`x-api-version`)
- DI + service layer: `Controller -> Service -> DbContext`

## Chuc nang API hien tai

- Category CRUD: `/api/v1/Categories`
- Product CRUD: `/api/v1/Products`
- Material CRUD co ban: `/api/v1/Materials`
- PrintType CRUD co ban: `/api/v1/PrintTypes`
- Gan cau hinh cho product:
  - `GET /api/v1/products/{productId}/configurations`
  - `POST /api/v1/products/{productId}/configurations/materials`
  - `POST /api/v1/products/{productId}/configurations/print-options`
- Tao va xem don hang:
  - `POST /api/v1/Orders`
  - `GET /api/v1/Orders`
  - `GET /api/v1/Orders/{id}`
- Health check: `/health`

## Cau truc source code

- `Controllers/`: xu ly HTTP request/response
- `Services/`: xu ly business logic
- `Services/Interfaces/`: hop dong service
- `Data/AppDbContext.cs`: EF Core DbContext
- `Models/`: cac entity
- `Extensions/`: cau hinh service va middleware
- `Infrastructure/`: helper ha tang (parse connection string, ...)

Luu y: tat ca controller hien tai deu di qua service interface, khong truy cap truc tiep `DbContext`.

## De xuat thiet ke co so du lieu

De phu hop bai toan in ly nhua, nen mo rong schema theo huong sau:

1. `categories`
- `id` (PK)
- `name` (unique)
- `description`

2. `products`
- `id` (PK)
- `category_id` (FK -> categories.id)
- `name`
- `description`
- `base_price`
- `status` (active/inactive)

3. `materials`
- `id` (PK)
- `name` (PP, PET, ... )
- `description`

4. `product_materials`
- `id` (PK)
- `product_id` (FK -> products.id)
- `material_id` (FK -> materials.id)
- `extra_price`

5. `print_types`
- `id` (PK)
- `name` (in 1 mau, in nhieu mau)
- `color_count`
- `description`

6. `product_print_options`
- `id` (PK)
- `product_id` (FK -> products.id)
- `print_type_id` (FK -> print_types.id)
- `extra_price`

7. `design_files`
- `id` (PK)
- `product_id` (FK -> products.id, nullable neu gan theo don hang)
- `file_url`
- `file_name`
- `file_type`
- `uploaded_at_utc`

8. `orders` (phase tiep theo)
- `id` (PK)
- `customer_name`
- `customer_phone`
- `customer_email`
- `note`
- `total_amount`
- `status`
- `created_at_utc`

9. `order_items` (phase tiep theo)
- `id` (PK)
- `order_id` (FK -> orders.id)
- `product_id` (FK -> products.id)
- `material_id` (FK -> materials.id, nullable)
- `print_type_id` (FK -> print_types.id, nullable)
- `quantity`
- `unit_price`
- `design_file_id` (FK -> design_files.id, nullable)

## Quy trinh tao data de chay nghiep vu

Nhap lieu theo dung thu tu de tranh loi rang buoc FK:

1. Tao `Category` (ly, to, ong hut, bao bi, giay in, ...)
2. Tao `Material` (PP, PET, ...)
3. Tao `PrintType` (in 1 mau, in 2 mau, ...)
4. Tao `Product` (thuoc category)
5. Gan `Material` cho tung `Product` qua endpoint configurations/materials
6. Gan `PrintType` cho tung `Product` qua endpoint configurations/print-options
7. Tao `Order` voi danh sach `OrderItem`

Vi du payload tao order:

```json
{
  "customerName": "Nguyen Van A",
  "customerPhone": "0900000000",
  "customerEmail": "a@example.com",
  "note": "In logo mau do",
  "items": [
    {
      "productId": 1,
      "materialId": 1,
      "printTypeId": 2,
      "quantity": 1000,
      "unitPrice": 1500
    }
  ]
}
```

## Ly do chon PostgreSQL

- Phu hop du lieu quan he va nhieu rang buoc FK.
- Ho tro truy van linh hoat cho bao gia/don hang.
- Tot cho EF Core migration va mo rong ve sau.

## Chay local

```bash
dotnet restore
dotnet run
```

## Cau hinh moi truong

Dat bien moi truong khi deploy:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
ConnectionStrings__DefaultConnection=postgresql://<username>:<password>@<host>/<database>?sslmode=require
```

## Docker deploy (Render)

- Da co `Dockerfile`
- Port expose: `8080`
- Root endpoint `/` redirect sang swagger o Development

## Lo trinh tiep theo

1. Tach service layer cho `Material`, `PrintType`, `Order` de dong bo kien truc.
2. Bo sung validation request (FluentValidation hoac DataAnnotations).
3. Tao migration chinh thuc (thay vi `EnsureCreated`) de quan ly schema production.
4. Bo sung authentication/authorization cho admin API.
5. Viet integration test cho luong Category/Product/Order.
