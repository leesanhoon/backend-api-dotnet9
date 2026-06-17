# API Integration Guide v2

> **Base URL:** `{host}/api/v1`
> **API Versioning:** URL segment (`/api/v1/...`) hoặc header `x-api-version: 1`
> **Ngày cập nhật:** 2026-06-17

---

## Mục lục

- [Quy tắc chung](#quy-tắc-chung)
- [Image Upload — Quy tắc & Validation](#image-upload--quy-tắc--validation)
- [Categories API](#categories-api)
- [Products API](#products-api)
- [Lids API (Nắp ly)](#lids-api-nắp-ly)
- [Partners API (Đối tác)](#partners-api-đối-tác)
- [Orders API (Đơn hàng)](#orders-api-đơn-hàng)
- [Pagination](#pagination)
- [Error Handling](#error-handling)
- [Frontend Integration Examples](#frontend-integration-examples)
- [Changelog](#changelog)

---

## Quy tắc chung

| Mục | Giá trị |
|-----|---------|
| Response format | JSON |
| Date format | ISO 8601 UTC (`2026-06-17T08:30:00Z`) |
| Pagination mặc định | `page=1`, `pageSize=10` (max 50) |
| Image upload | `multipart/form-data` |
| JSON body | `application/json` |

---

## Image Upload — Quy tắc & Validation

### Định dạng được hỗ trợ

| Extension | Content-Type |
|-----------|-------------|
| `.jpg`, `.jpeg` | `image/jpeg` |
| `.png` | `image/png` |
| `.webp` | `image/webp` |
| `.gif` | `image/gif` |

### Giới hạn (Limits)

| Quy tắc | Giá trị | Ghi chú |
|----------|---------|---------|
| **Dung lượng tối đa / file** | 10 MB | Có thể cấu hình qua `ImageProcessing:MaxFileSizeBytes` |
| **Số lượng gallery tối đa / lần upload** | 10 ảnh | Có thể cấu hình qua `ImageProcessing:MaxGalleryImages` |
| **Tổng dung lượng request** | 50 MB | Giới hạn multipart body |
| **Avatar** | Tối đa 1 ảnh | Upload mới sẽ thay thế avatar cũ |

### Xử lý ảnh tự động

| Thuộc tính | Avatar | Gallery |
|------------|--------|---------|
| Max width | 1200 px | 1000 px |
| Max height | 1200 px | 1000 px |
| Resize mode | Proportional (max fit) | Proportional (max fit) |
| Output (PNG input) | PNG | PNG |
| Output (khác) | JPEG quality 82% | JPEG quality 82% |

- Ảnh nhỏ hơn max sẽ **không** bị phóng to.
- Tỉ lệ khung hình luôn được giữ nguyên.
- Lưu trữ trên **Cloudinary CDN** — URL vĩnh viễn và public.

### Validation flow

```
1. File != null && file.Length > 0
2. File size <= 10 MB
3. Extension thuộc [.jpg, .jpeg, .png, .webp, .gif]
4. Content-Type thuộc [image/jpeg, image/png, image/webp, image/gif]
5. Gallery count <= 10
6. Resize + encode → Upload Cloudinary
```

### Image Types

| Type | Mô tả | DisplayOrder |
|------|--------|-------------|
| `avatar` | Ảnh đại diện chính (tối đa 1) | `0` |
| `gallery` | Ảnh bổ sung | `1, 2, 3...` |

---

## Categories API

### GET /api/v1/categories

Lấy danh sách categories (phân trang).

**Query Parameters:**

| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `page` | int | 1 | Trang |
| `pageSize` | int | 10 | Số item/trang (max 50) |

**Response:** `200 OK`

```json
{
  "items": [
    {
      "id": 1,
      "name": "Ly nhựa",
      "description": "Danh mục ly nhựa",
      "parentId": null
    }
  ],
  "totalCount": 5,
  "page": 1,
  "pageSize": 10
}
```

### GET /api/v1/categories/tree

Lấy cây danh mục (tree structure).

**Response:** `200 OK`

```json
[
  {
    "id": 1,
    "name": "Ly nhựa",
    "description": "Danh mục ly nhựa",
    "children": [
      {
        "id": 3,
        "name": "Ly PP",
        "description": null,
        "children": []
      }
    ]
  }
]
```

### GET /api/v1/categories/{id}

**Response:** `200 OK` | `404 Not Found`

### POST /api/v1/categories

**Content-Type:** `application/json`

```json
{
  "name": "Ly nhựa PP",
  "description": "Mô tả",
  "parentId": 1
}
```

**Response:** `201 Created`

**Errors:**
| Status | Lý do |
|--------|-------|
| `400` | `ParentId` không tồn tại |

### PUT /api/v1/categories/{id}

**Content-Type:** `application/json`

```json
{
  "name": "Ly nhựa PP (cập nhật)",
  "description": "Mô tả mới",
  "parentId": 1
}
```

**Errors:**
| Status | Lý do |
|--------|-------|
| `404` | Category không tồn tại |
| `400` | Không thể sửa danh mục gốc |
| `400` | `ParentId` không tồn tại |
| `400` | Chọn danh mục con làm cha (vòng lặp) |

### DELETE /api/v1/categories/{id}

**Response:** `204 No Content`

**Errors:**
| Status | Lý do |
|--------|-------|
| `404` | Không tồn tại |
| `400` | Không thể xoá danh mục gốc |
| `400` | Đang có danh mục con |
| `400` | Đang có sản phẩm liên kết |
| `400` | Đang có nắp ly liên kết |

---

## Products API

### GET /api/v1/products

Lấy danh sách sản phẩm (phân trang).

**Query:** `page`, `pageSize`

**Response:** `200 OK` → `PagedResult<ProductResponse>`

### GET /api/v1/products/{id}

**Response:** `200 OK` | `404`

```json
{
  "id": 5,
  "name": "Ly nhựa PP 700ml",
  "description": "Ly nhựa PP trong suốt",
  "categoryId": 1,
  "categoryName": "Ly nhựa",
  "avatarImageUrl": "https://res.cloudinary.com/.../avatar-ly-nhua-pp-700ml-20260614.jpg",
  "galleryImages": [
    {
      "id": 10,
      "imageUrl": "https://res.cloudinary.com/.../gallery-photo1-20260614.jpg",
      "imageType": "gallery",
      "displayOrder": 1,
      "createdAtUtc": "2026-06-14T08:30:00Z"
    }
  ],
  "variants": [
    {
      "id": 1,
      "capacityMl": 700,
      "diameterMm": 95,
      "priceTiers": [
        { "id": 1, "minQuantity": 1, "unitPrice": 500.00 },
        { "id": 2, "minQuantity": 1000, "unitPrice": 400.00 }
      ]
    }
  ],
  "lids": [
    { "id": 1, "lidId": 3, "lidName": "Nắp ly phẳng 95mm" }
  ]
}
```

### POST /api/v1/products

**Content-Type:** `multipart/form-data`

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| `Name` | string | **Yes** | Tên sản phẩm |
| `Description` | string | No | Mô tả |
| `CategoryId` | int | **Yes** | ID danh mục |
| `AvatarImage` | file | No | Ảnh đại diện (max 10MB) |
| `GalleryImages` | file[] | No | Ảnh gallery (max 10 files, mỗi file max 10MB) |
| `Variants` | array | **Yes** | Ít nhất 1 variant |
| `LidIds` | int[] | No | Danh sách ID nắp ly tương thích |

**cURL:**

```bash
curl -X POST "{host}/api/v1/products" \
  -F "Name=Ly nhựa PP 700ml" \
  -F "Description=Ly nhựa PP trong suốt" \
  -F "CategoryId=1" \
  -F "AvatarImage=@/path/to/avatar.jpg" \
  -F "GalleryImages=@/path/to/photo1.jpg" \
  -F "GalleryImages=@/path/to/photo2.jpg" \
  -F "Variants[0].CapacityMl=700" \
  -F "Variants[0].DiameterMm=95" \
  -F "Variants[0].PriceTiers[0].MinQuantity=1" \
  -F "Variants[0].PriceTiers[0].UnitPrice=500" \
  -F "LidIds[0]=3"
```

**Response:** `201 Created` → `ProductResponse`

**Errors:**
| Status | Lý do |
|--------|-------|
| `400` | `CategoryId` không tồn tại |
| `400` | Validation lỗi (trùng dung tích, thiếu variant, v.v.) |
| `400` | Ảnh không hợp lệ (sai định dạng, quá dung lượng, quá 10 ảnh gallery) |

### PUT /api/v1/products/{id}

**Content-Type:** `application/json` (không hỗ trợ upload ảnh — dùng endpoint riêng)

```json
{
  "name": "Ly nhựa PP 700ml (cập nhật)",
  "description": "Mô tả mới",
  "categoryId": 1,
  "variants": [
    {
      "capacityMl": 700,
      "diameterMm": 95,
      "priceTiers": [
        { "minQuantity": 1, "unitPrice": 550 }
      ]
    }
  ],
  "lidIds": [3, 5]
}
```

### DELETE /api/v1/products/{id}

**Response:** `204 No Content` | `404`

### GET /api/v1/products/{id}/compatible-lids

Lấy danh sách nắp ly tương thích.

**Response:** `200 OK` → `LidResponse[]`

### POST /api/v1/products/{id}/images

Thêm ảnh cho sản phẩm đã tồn tại.

**Content-Type:** `multipart/form-data`

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| `AvatarImage` | file | No | Avatar mới (thay thế cũ) |
| `GalleryImages` | file[] | No | Ảnh gallery bổ sung (max 10/lần) |

**cURL:**

```bash
# Thêm gallery
curl -X POST "{host}/api/v1/products/5/images" \
  -F "GalleryImages=@/path/to/new-photo1.jpg" \
  -F "GalleryImages=@/path/to/new-photo2.jpg"

# Thay avatar
curl -X POST "{host}/api/v1/products/5/images" \
  -F "AvatarImage=@/path/to/new-avatar.jpg"
```

**Response:** `200 OK` → `ProductResponse`

### DELETE /api/v1/products/{id}/images/{imageId}

Xoá một ảnh cụ thể. Nếu xoá avatar → `avatarImageUrl` trở thành `null`.

**Response:** `204 No Content`

---

## Lids API (Nắp ly)

### GET /api/v1/lids

**Query:** `page`, `pageSize`

**Response:** `200 OK` → `PagedResult<LidResponse>`

### GET /api/v1/lids/{id}

```json
{
  "id": 3,
  "name": "Nắp ly phẳng 95mm",
  "description": "Nắp phẳng có lỗ",
  "categoryId": 2,
  "categoryName": "Nắp ly",
  "avatarImageUrl": "https://res.cloudinary.com/.../avatar-nap-ly-phang-20260614.jpg",
  "galleryImages": [
    {
      "id": 7,
      "imageUrl": "https://res.cloudinary.com/.../gallery-lid-detail1-20260614.jpg",
      "imageType": "gallery",
      "displayOrder": 1,
      "createdAtUtc": "2026-06-14T09:00:00Z"
    }
  ],
  "prices": [
    { "id": 1, "diameterMm": 95, "sizeName": "Lớn", "unitPrice": 200.00 }
  ]
}
```

### POST /api/v1/lids

**Content-Type:** `multipart/form-data`

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| `Name` | string | **Yes** | Tên nắp ly |
| `Description` | string | No | Mô tả |
| `CategoryId` | int | **Yes** | ID danh mục |
| `AvatarImage` | file | No | Ảnh đại diện (max 10MB) |
| `GalleryImages` | file[] | No | Ảnh gallery (max 10 files) |
| `Prices` | array | **Yes** | Danh sách giá |

**Prices item:**

```
Prices[0].DiameterMm = 95
Prices[0].SizeName = "Lớn"
Prices[0].UnitPrice = 200
```

**cURL:**

```bash
curl -X POST "{host}/api/v1/lids" \
  -F "Name=Nắp ly phẳng 95mm" \
  -F "CategoryId=2" \
  -F "AvatarImage=@/path/to/lid-avatar.jpg" \
  -F "Prices[0].DiameterMm=95" \
  -F "Prices[0].SizeName=Lớn" \
  -F "Prices[0].UnitPrice=200"
```

**Response:** `201 Created` → `LidResponse`

### PUT /api/v1/lids/{id}

**Content-Type:** `application/json` (không hỗ trợ ảnh — dùng endpoint `/images`)

```json
{
  "name": "Nắp ly phẳng 95mm (cập nhật)",
  "description": "Mô tả mới",
  "categoryId": 2,
  "prices": [
    { "diameterMm": 95, "sizeName": "Lớn", "unitPrice": 250 }
  ]
}
```

### DELETE /api/v1/lids/{id}

**Response:** `204 No Content`

**Errors:**
| Status | Lý do |
|--------|-------|
| `404` | Không tồn tại |
| `400` | Đang có sản phẩm sử dụng nắp ly này |

### POST /api/v1/lids/{id}/images

Thêm ảnh cho nắp ly (tương tự Products).

**Content-Type:** `multipart/form-data`

| Field | Type | Mô tả |
|-------|------|-------|
| `AvatarImage` | file | Avatar mới (thay thế cũ) |
| `GalleryImages` | file[] | Gallery bổ sung (max 10/lần) |

### DELETE /api/v1/lids/{id}/images/{imageId}

**Response:** `204 No Content`

---

## Partners API (Đối tác)

### GET /api/v1/partners

**Query:** `page`, `pageSize`

**Response:** `200 OK` → `PagedResult<PartnerResponse>`

### GET /api/v1/partners/{id}

```json
{
  "id": 1,
  "name": "Công ty TNHH ABC",
  "address": "123 Đường XYZ, Q.1, TP.HCM",
  "phoneNumber": "0901234567",
  "description": "Nhà cung cấp chính",
  "avatarImageUrl": "https://res.cloudinary.com/.../avatar-cong-ty-abc-20260617.jpg",
  "galleryImages": [
    {
      "id": 1,
      "imageUrl": "https://res.cloudinary.com/.../gallery-photo-20260617.jpg",
      "displayOrder": 1
    }
  ],
  "createdAtUtc": "2026-06-17T08:00:00Z"
}
```

### POST /api/v1/partners

**Content-Type:** `multipart/form-data`

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| `Name` | string | **Yes** | Tên đối tác |
| `Address` | string | **Yes** | Địa chỉ |
| `PhoneNumber` | string | No | Số điện thoại |
| `Description` | string | No | Mô tả |
| `AvatarImage` | file | No | Ảnh đại diện (max 10MB) |
| `GalleryImages` | file[] | No | Ảnh gallery (max 10 files) |

**cURL:**

```bash
curl -X POST "{host}/api/v1/partners" \
  -F "Name=Công ty TNHH ABC" \
  -F "Address=123 Đường XYZ, Q.1, TP.HCM" \
  -F "PhoneNumber=0901234567" \
  -F "Description=Nhà cung cấp chính" \
  -F "AvatarImage=@/path/to/partner-avatar.jpg" \
  -F "GalleryImages=@/path/to/photo1.jpg"
```

**Response:** `201 Created` → `PartnerResponse`

### PUT /api/v1/partners/{id}

**Content-Type:** `multipart/form-data` (hỗ trợ cập nhật ảnh)

Gửi cùng format như POST. Avatar mới sẽ thay thế avatar cũ.

**cURL:**

```bash
curl -X PUT "{host}/api/v1/partners/1" \
  -F "Name=Công ty TNHH ABC (cập nhật)" \
  -F "Address=456 Đường MNO, Q.2, TP.HCM" \
  -F "AvatarImage=@/path/to/new-avatar.jpg"
```

**Response:** `200 OK` → `PartnerResponse`

### DELETE /api/v1/partners/{id}

**Response:** `204 No Content` | `404`

---

## Orders API (Đơn hàng)

### POST /api/v1/orders

**Content-Type:** `application/json`

```json
{
  "customerName": "Nguyễn Văn A",
  "customerPhone": "0901234567",
  "customerEmail": "nguyenvana@email.com",
  "note": "Giao hàng buổi sáng",
  "items": [
    {
      "productId": 5,
      "quantity": 100,
      "unitPrice": 500.00,
      "materialId": null,
      "printTypeId": null
    }
  ]
}
```

**Response:** `201 Created`

```json
{
  "id": 1,
  "customerName": "Nguyễn Văn A",
  "customerPhone": "0901234567",
  "customerEmail": "nguyenvana@email.com",
  "note": "Giao hàng buổi sáng",
  "totalAmount": 50000.00,
  "status": "PendingConfirmation",
  "createdAtUtc": "2026-06-17T08:00:00Z",
  "items": [
    {
      "productId": 5,
      "productName": "Ly nhựa PP 700ml",
      "materialId": null,
      "materialName": null,
      "printTypeId": null,
      "printTypeName": null,
      "quantity": 100,
      "unitPrice": 500.00
    }
  ]
}
```

### GET /api/v1/orders/track?orderId={id}&phone={phone}

Tra cứu đơn hàng theo ID + số điện thoại.

**Response:** `200 OK` → `OrderDetailDto` | `404`

### GET /api/v1/orders

**Query:** `page`, `pageSize`, `status` (optional filter)

**Response:** `200 OK` → `PagedResult<OrderSummaryDto>`

```json
{
  "items": [
    {
      "id": 1,
      "customerName": "Nguyễn Văn A",
      "totalAmount": 50000.00,
      "status": "PendingConfirmation",
      "createdAtUtc": "2026-06-17T08:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10
}
```

### PATCH /api/v1/orders/{id}/status

**Content-Type:** `application/json`

```json
{
  "status": "Confirmed"
}
```

**Luồng trạng thái:**

```
PendingConfirmation → Confirmed → Shipping → Completed
                    → Cancelled
```

| Từ trạng thái | Cho phép chuyển sang |
|---------------|---------------------|
| `PendingConfirmation` | `Confirmed`, `Cancelled` |
| `Confirmed` | `Shipping` |
| `Shipping` | `Completed` |
| `Completed` | (không thể chuyển) |
| `Cancelled` | (không thể chuyển) |

**Response:** `200 OK` → `OrderDetailDto`

**Errors:**
| Status | Lý do |
|--------|-------|
| `404` | Đơn hàng không tồn tại |
| `400` | Trạng thái chuyển không hợp lệ |

---

## Pagination

Tất cả endpoint GET danh sách đều hỗ trợ pagination:

**Query Parameters:**

| Param | Type | Default | Min | Max |
|-------|------|---------|-----|-----|
| `page` | int | 1 | 1 | — |
| `pageSize` | int | 10 | 1 | 50 |

**Response format:**

```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10
}
```

---

## Error Handling

### HTTP Status Codes

| Code | Ý nghĩa |
|------|---------|
| `200` | Thành công |
| `201` | Tạo mới thành công |
| `204` | Xoá thành công (không có body) |
| `400` | Bad Request — validation lỗi |
| `404` | Không tìm thấy resource |

### Image Validation Errors (400)

| Lỗi | Message |
|------|---------|
| File rỗng | `"Image 'filename' is empty."` |
| Quá dung lượng | `"Image 'filename' exceeds the maximum file size of 10 MB."` |
| Sai extension | `"Image 'filename' must be a supported image file (.jpg, .jpeg, .png, .webp, .gif)."` |
| Sai content type | `"Image 'filename' has an unsupported content type 'xxx'."` |
| Quá số lượng gallery | `"Tối đa 10 ảnh gallery cho mỗi lần upload."` |
| Upload thất bại | `"Cloudinary upload failed."` |

---

## Frontend Integration Examples

### JavaScript / Fetch API

```javascript
// Helper: validate ảnh trước khi upload
function validateImageFile(file) {
  const MAX_SIZE = 10 * 1024 * 1024; // 10MB
  const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

  if (file.size > MAX_SIZE) {
    throw new Error(`File "${file.name}" vượt quá 10MB`);
  }
  if (!ALLOWED_TYPES.includes(file.type)) {
    throw new Error(`File "${file.name}" không đúng định dạng ảnh`);
  }
}

// Tạo sản phẩm với ảnh
async function createProduct(data, avatarFile, galleryFiles) {
  const formData = new FormData();
  formData.append('Name', data.name);
  formData.append('CategoryId', data.categoryId);

  if (data.description) {
    formData.append('Description', data.description);
  }

  // Variants
  data.variants.forEach((variant, vi) => {
    formData.append(`Variants[${vi}].CapacityMl`, variant.capacityMl);
    formData.append(`Variants[${vi}].DiameterMm`, variant.diameterMm);
    variant.priceTiers.forEach((tier, ti) => {
      formData.append(`Variants[${vi}].PriceTiers[${ti}].MinQuantity`, tier.minQuantity);
      formData.append(`Variants[${vi}].PriceTiers[${ti}].UnitPrice`, tier.unitPrice);
    });
  });

  // LidIds
  data.lidIds?.forEach((id, i) => {
    formData.append(`LidIds[${i}]`, id);
  });

  // Avatar
  if (avatarFile) {
    validateImageFile(avatarFile);
    formData.append('AvatarImage', avatarFile);
  }

  // Gallery (max 10)
  if (galleryFiles && galleryFiles.length > 10) {
    throw new Error('Tối đa 10 ảnh gallery');
  }
  galleryFiles?.forEach(file => {
    validateImageFile(file);
    formData.append('GalleryImages', file);
  });

  const response = await fetch('/api/v1/products', {
    method: 'POST',
    body: formData, // KHÔNG set Content-Type header
  });

  if (!response.ok) {
    const error = await response.text();
    throw new Error(error);
  }

  return response.json();
}

// Thêm ảnh cho sản phẩm
async function addProductImages(productId, avatarFile, galleryFiles) {
  const formData = new FormData();

  if (avatarFile) {
    validateImageFile(avatarFile);
    formData.append('AvatarImage', avatarFile);
  }

  galleryFiles?.forEach(file => {
    validateImageFile(file);
    formData.append('GalleryImages', file);
  });

  const response = await fetch(`/api/v1/products/${productId}/images`, {
    method: 'POST',
    body: formData,
  });

  return response.json();
}

// Xoá ảnh
async function deleteProductImage(productId, imageId) {
  await fetch(`/api/v1/products/${productId}/images/${imageId}`, {
    method: 'DELETE',
  });
}

// Tạo partner
async function createPartner(data, avatarFile, galleryFiles) {
  const formData = new FormData();
  formData.append('Name', data.name);
  formData.append('Address', data.address);

  if (data.phoneNumber) formData.append('PhoneNumber', data.phoneNumber);
  if (data.description) formData.append('Description', data.description);

  if (avatarFile) {
    validateImageFile(avatarFile);
    formData.append('AvatarImage', avatarFile);
  }

  galleryFiles?.forEach(file => {
    validateImageFile(file);
    formData.append('GalleryImages', file);
  });

  const response = await fetch('/api/v1/partners', {
    method: 'POST',
    body: formData,
  });

  return response.json();
}

// Tạo đơn hàng
async function createOrder(orderData) {
  const response = await fetch('/api/v1/orders', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(orderData),
  });

  return response.json();
}

// Cập nhật trạng thái đơn
async function updateOrderStatus(orderId, status) {
  const response = await fetch(`/api/v1/orders/${orderId}/status`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ status }),
  });

  return response.json();
}
```

### React / Axios

```jsx
import axios from 'axios';

const api = axios.create({ baseURL: '/api/v1' });

// Tạo lid với ảnh
export const createLid = async (data, avatarFile, galleryFiles) => {
  const formData = new FormData();
  formData.append('Name', data.name);
  formData.append('CategoryId', data.categoryId);
  if (data.description) formData.append('Description', data.description);

  data.prices.forEach((price, i) => {
    formData.append(`Prices[${i}].DiameterMm`, price.diameterMm);
    if (price.sizeName) formData.append(`Prices[${i}].SizeName`, price.sizeName);
    formData.append(`Prices[${i}].UnitPrice`, price.unitPrice);
  });

  if (avatarFile) formData.append('AvatarImage', avatarFile);
  galleryFiles?.forEach(f => formData.append('GalleryImages', f));

  const { data: result } = await api.post('/lids', formData);
  return result;
};

// Thêm ảnh cho lid
export const addLidImages = async (lidId, avatarFile, galleryFiles) => {
  const formData = new FormData();
  if (avatarFile) formData.append('AvatarImage', avatarFile);
  galleryFiles?.forEach(f => formData.append('GalleryImages', f));

  const { data } = await api.post(`/lids/${lidId}/images`, formData);
  return data;
};
```

---

## Endpoint Summary

| Method | Endpoint | Content-Type | Mô tả |
|--------|----------|-------------|-------|
| **Categories** ||||
| `GET` | `/categories` | — | Danh sách (phân trang) |
| `GET` | `/categories/tree` | — | Cây danh mục |
| `GET` | `/categories/{id}` | — | Chi tiết |
| `POST` | `/categories` | `application/json` | Tạo mới |
| `PUT` | `/categories/{id}` | `application/json` | Cập nhật |
| `DELETE` | `/categories/{id}` | — | Xoá |
| **Products** ||||
| `GET` | `/products` | — | Danh sách (phân trang) |
| `GET` | `/products/{id}` | — | Chi tiết |
| `POST` | `/products` | `multipart/form-data` | Tạo mới (với ảnh) |
| `PUT` | `/products/{id}` | `application/json` | Cập nhật (không ảnh) |
| `DELETE` | `/products/{id}` | — | Xoá |
| `GET` | `/products/{id}/compatible-lids` | — | Nắp ly tương thích |
| `POST` | `/products/{id}/images` | `multipart/form-data` | Thêm ảnh |
| `DELETE` | `/products/{id}/images/{imgId}` | — | Xoá ảnh |
| **Lids** ||||
| `GET` | `/lids` | — | Danh sách (phân trang) |
| `GET` | `/lids/{id}` | — | Chi tiết |
| `POST` | `/lids` | `multipart/form-data` | Tạo mới (với ảnh) |
| `PUT` | `/lids/{id}` | `application/json` | Cập nhật (không ảnh) |
| `DELETE` | `/lids/{id}` | — | Xoá |
| `POST` | `/lids/{id}/images` | `multipart/form-data` | Thêm ảnh |
| `DELETE` | `/lids/{id}/images/{imgId}` | — | Xoá ảnh |
| **Partners** ||||
| `GET` | `/partners` | — | Danh sách (phân trang) |
| `GET` | `/partners/{id}` | — | Chi tiết |
| `POST` | `/partners` | `multipart/form-data` | Tạo mới (với ảnh) |
| `PUT` | `/partners/{id}` | `multipart/form-data` | Cập nhật (với ảnh) |
| `DELETE` | `/partners/{id}` | — | Xoá |
| **Orders** ||||
| `POST` | `/orders` | `application/json` | Tạo đơn hàng |
| `GET` | `/orders/track` | — | Tra cứu đơn hàng |
| `GET` | `/orders` | — | Danh sách (phân trang, filter status) |
| `PATCH` | `/orders/{id}/status` | `application/json` | Cập nhật trạng thái |

---

## Changelog

### v2 — 2026-06-17

**Image Validation (MỚI):**
- Thêm giới hạn dung lượng file: tối đa **10 MB / file**
- Thêm giới hạn số lượng gallery: tối đa **10 ảnh / lần upload**
- Thêm validate **Content-Type** (MIME type) ngoài extension
- Thêm giới hạn tổng request body: **50 MB**
- Cấu hình mới trong `appsettings.json`: `MaxFileSizeBytes`, `MaxGalleryImages`

**Partners API (MỚI):**
- `POST /api/v1/partners` — Tạo đối tác với ảnh
- `PUT /api/v1/partners/{id}` — Cập nhật đối tác với ảnh
- `DELETE /api/v1/partners/{id}` — Xoá đối tác
- Ảnh partner lưu riêng folder `partners` trên Cloudinary (trước đó dùng chung `products`)

**Bug fixes:**
- Fix partner images upload vào sai folder Cloudinary (`products` → `partners`)
