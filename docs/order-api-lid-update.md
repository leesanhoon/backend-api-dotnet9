# API Update — Merge Lid into Product

**Date:** 2026-06-18  
**Version:** v2.0.0  
**Status:** Breaking change — requires client update

---

## Summary

Nắp ly (Lid) không còn là entity riêng biệt. Nắp ly giờ được lưu trữ và quản lý như **Product** thông thường, chỉ khác ở `CapacityMl = 0` trong variants. Toàn bộ Lid API endpoints đã bị xoá và thay thế bằng Product API.

---

## Breaking Changes

### 1. Lid API Endpoints — REMOVED

Toàn bộ endpoints dưới đây **đã bị xoá**:

| Method | Endpoint | Replacement |
|--------|----------|-------------|
| `GET` | `/api/v1/lids` | `GET /api/v1/products` (filter by lid category) |
| `GET` | `/api/v1/lids/{id}` | `GET /api/v1/products/{id}` |
| `POST` | `/api/v1/lids` | `POST /api/v1/products` (with `capacityMl = 0`) |
| `PUT` | `/api/v1/lids/{id}` | `PUT /api/v1/products/{id}` |
| `DELETE` | `/api/v1/lids/{id}` | `DELETE /api/v1/products/{id}` |
| `POST` | `/api/v1/lids/{id}/images` | `POST /api/v1/products/{id}/images` |
| `DELETE` | `/api/v1/lids/{id}/images/{imageId}` | `DELETE /api/v1/products/{id}/images/{imageId}` |

### 2. Product Request Schema — Field Renamed

| Old Field | New Field | Affected Endpoints |
|-----------|-----------|--------------------|
| `lidIds` | `compatibleProductIds` | `POST /api/v1/products`, `PUT /api/v1/products/{id}` |

### 3. Product Response Schema — Updated

```json
{
  "id": 1,
  "name": "Ly giấy 12oz",
  "description": "...",
  "categoryId": 1,
  "categoryName": "Ly giấy",
  "avatarImageUrl": "https://...",
  "galleryImages": [...],
  "variants": [
    {
      "id": 1,
      "capacityMl": 350,
      "diameterMm": 90,
      "sizeName": null,        // <-- NEW (nullable, used by lid-type products)
      "priceTiers": [
        { "id": 1, "minQuantity": 100, "unitPrice": 500 }
      ]
    }
  ],
  "lids": [
    {
      "id": 1,
      "compatibleProductId": 5,       // <-- RENAMED (was "lidId")
      "compatibleProductName": "Nắp phẳng"  // <-- RENAMED (was "lidName")
    }
  ]
}
```

| Changed Field | Old Name | New Name | Notes |
|---------------|----------|----------|-------|
| `variants[].sizeName` | — | `sizeName` | New field. `null` for cups, contains size label for lids |
| `lids[].lidId` | `lidId` | `compatibleProductId` | Now references a product ID |
| `lids[].lidName` | `lidName` | `compatibleProductName` | Name of the compatible product |

---

## How to Create a Lid-Type Product

Nắp ly giờ là product với `capacityMl = 0` trong variants:

```json
POST /api/v1/products
Content-Type: multipart/form-data

{
  "name": "Nắp phẳng",
  "description": "Nắp phẳng cho ly 90mm",
  "categoryId": 2,
  "avatarImage": <file>,
  "galleryImages": [<file>, ...],
  "variants": [
    {
      "capacityMl": 0,
      "diameterMm": 90,
      "sizeName": "90mm",
      "priceTiers": [
        { "minQuantity": 1, "unitPrice": 200 }
      ]
    }
  ],
  "compatibleProductIds": []
}
```

**Key differences from cup products:**
- `capacityMl` = `0` (lids have no capacity)
- `sizeName` = tên kích cỡ miệng (e.g., "90mm", "80mm")
- `priceTiers` thường chỉ có 1 mốc giá với `minQuantity = 1`

---

## Order API — Minor Changes

### POST `/api/orders` — Create Order

Request body **không đổi**:

```json
{
  "items": [
    {
      "productId": 1,
      "quantity": 100,
      "unitPrice": 500,
      "materialId": 1,
      "printTypeId": 1,
      "lidId": 5        // Still "lidId" — now references a product ID
    }
  ]
}
```

Response body **không đổi** — `lidId` và `lidName` vẫn giữ nguyên field names.

> **Note:** `lidId` trong order items giờ trỏ đến `products.Id` thay vì `lids.Id`. Giá trị ID có thể khác vì dữ liệu đã được migrate sang bảng products.

---

## Database Migration

### Schema Changes

| Table | Change |
|-------|--------|
| `product_variants` | Added `SizeName` (varchar 100, nullable) |
| `product_variants` | Unique index changed: `(ProductId, CapacityMl)` → `(ProductId, CapacityMl, DiameterMm)` |
| `product_lids` | Column renamed: `LidId` → `CompatibleProductId` |
| `product_lids` | FK now references `products` instead of `lids` |
| `order_items` | FK `LidId` now references `products` instead of `lids` |
| `lids` | **DROPPED** (data migrated to `products`) |
| `lid_prices` | **DROPPED** (data migrated to `product_variants` + `variant_price_tiers`) |
| `lid_images` | **DROPPED** (data migrated to `product_images`) |

### Data Migration

Migration tự động chuyển:
- `lids` → `products`
- `lid_prices` → `product_variants` (CapacityMl = 0) + `variant_price_tiers` (MinQuantity = 1)
- `lid_images` → `product_images`
- Cập nhật `product_lids` và `order_items` FKs sang product IDs mới

### Run Migration

```bash
dotnet ef database update
```

---

## Compatible Lids Endpoint

`GET /api/v1/products/{id}/compatible-lids` vẫn hoạt động nhưng giờ trả về `ProductResponse[]` thay vì `LidResponse[]`. Response format giống hệt endpoint `GET /api/v1/products/{id}`.
