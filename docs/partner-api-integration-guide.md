# Partner API - Hướng dẫn tích hợp cho Client

## Base URL

```
{API_BASE_URL}/api/v1/partners
```

---

## 1. Danh sách đối tác (Phân trang)

Lấy danh sách tất cả đối tác, sắp xếp theo mới nhất.

### Request

```
GET /api/v1/partners?Page=1&PageSize=10
```

| Query Param | Type | Mặc định | Mô tả |
|-------------|------|----------|-------|
| `Page` | int | 1 | Trang hiện tại (tối thiểu 1) |
| `PageSize` | int | 10 | Số item mỗi trang (1-100) |

### Response `200 OK`

```json
{
  "items": [
    {
      "id": 1,
      "name": "Phúc Long Coffee & Tea",
      "address": "123 Nguyễn Huệ, Quận 1, TP.HCM",
      "phoneNumber": "0901234567",
      "description": "Chuỗi cà phê & trà nổi tiếng Việt Nam",
      "avatarImageUrl": "https://res.cloudinary.com/.../partners/avatar_abc123.jpg",
      "galleryImages": [
        {
          "id": 10,
          "imageUrl": "https://res.cloudinary.com/.../partners/gallery_img1.jpg",
          "displayOrder": 1
        },
        {
          "id": 11,
          "imageUrl": "https://res.cloudinary.com/.../partners/gallery_img2.jpg",
          "displayOrder": 2
        }
      ],
      "createdAtUtc": "2026-06-11T08:30:00Z"
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10
}
```

### Code ví dụ

**JavaScript (fetch)**
```javascript
const response = await fetch(`${API_BASE_URL}/api/v1/partners?Page=1&PageSize=10`);
const data = await response.json();

// data.items       - Mảng đối tác
// data.totalCount  - Tổng số đối tác
// data.page        - Trang hiện tại
// data.pageSize    - Số item mỗi trang
```

**React Query (TypeScript)**
```typescript
interface PartnerImage {
  id: number;
  imageUrl: string;
  displayOrder: number;
}

interface Partner {
  id: number;
  name: string;
  address: string;
  phoneNumber: string | null;
  description: string | null;
  avatarImageUrl: string | null;
  galleryImages: PartnerImage[];
  createdAtUtc: string;
}

interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

const fetchPartners = async (page: number, pageSize: number): Promise<PagedResult<Partner>> => {
  const res = await fetch(`${API_BASE_URL}/api/v1/partners?Page=${page}&PageSize=${pageSize}`);
  if (!res.ok) throw new Error("Failed to fetch partners");
  return res.json();
};
```

---

## 2. Chi tiết đối tác

Lấy thông tin chi tiết của một đối tác theo ID.

### Request

```
GET /api/v1/partners/{id}
```

| Path Param | Type | Mô tả |
|------------|------|-------|
| `id` | int | ID của đối tác |

### Response `200 OK`

```json
{
  "id": 1,
  "name": "Phúc Long Coffee & Tea",
  "address": "123 Nguyễn Huệ, Quận 1, TP.HCM",
  "phoneNumber": "0901234567",
  "description": "Chuỗi cà phê & trà nổi tiếng Việt Nam",
  "avatarImageUrl": "https://res.cloudinary.com/.../partners/avatar_abc123.jpg",
  "galleryImages": [
    {
      "id": 10,
      "imageUrl": "https://res.cloudinary.com/.../partners/gallery_img1.jpg",
      "displayOrder": 1
    },
    {
      "id": 11,
      "imageUrl": "https://res.cloudinary.com/.../partners/gallery_img2.jpg",
      "displayOrder": 2
    }
  ],
  "createdAtUtc": "2026-06-11T08:30:00Z"
}
```

### Response `404 Not Found`

Khi `id` không tồn tại.

### Code ví dụ

```javascript
const response = await fetch(`${API_BASE_URL}/api/v1/partners/1`);
if (response.status === 404) {
  console.log("Đối tác không tồn tại");
  return;
}
const partner = await response.json();
```

---

## 3. Tạo đối tác mới

Tạo đối tác kèm ảnh đại diện và gallery. Sử dụng `multipart/form-data`.

### Request

```
POST /api/v1/partners
Content-Type: multipart/form-data
```

| Field | Type | Bắt buộc | Mô tả |
|-------|------|----------|-------|
| `Name` | string | **Có** | Tên quán (tối đa 200 ký tự, unique) |
| `Address` | string | **Có** | Địa chỉ quán (tối đa 500 ký tự) |
| `PhoneNumber` | string | Không | Số điện thoại (tối đa 20 ký tự) |
| `Description` | string | Không | Mô tả ngắn (tối đa 2000 ký tự) |
| `AvatarImage` | file | Không | Ảnh đại diện (jpg, png, webp, gif — tối đa 1200x1200) |
| `GalleryImages` | file[] | Không | Nhiều ảnh gallery (jpg, png, webp, gif — tối đa 1000x1000) |

### Response `201 Created`

Trả về object `PartnerResponse` (cùng format như GET chi tiết).

Header `Location` chứa URL chi tiết: `/api/v1/partners/{id}`

### Response `400 Bad Request`

```json
"Tên đối tác 'Phúc Long Coffee & Tea' đã tồn tại."
```

Hoặc lỗi upload ảnh:
```json
"File format không hợp lệ. Chỉ chấp nhận: .jpg, .jpeg, .png, .webp, .gif"
```

### Code ví dụ

**JavaScript (FormData)**
```javascript
const formData = new FormData();
formData.append("Name", "Phúc Long Coffee & Tea");
formData.append("Address", "123 Nguyễn Huệ, Quận 1, TP.HCM");
formData.append("PhoneNumber", "0901234567");
formData.append("Description", "Chuỗi cà phê & trà nổi tiếng Việt Nam");

// Ảnh đại diện (1 file)
formData.append("AvatarImage", avatarFile);

// Gallery (nhiều file)
galleryFiles.forEach(file => {
  formData.append("GalleryImages", file);
});

const response = await fetch(`${API_BASE_URL}/api/v1/partners`, {
  method: "POST",
  body: formData,
  // KHÔNG set Content-Type header — browser tự thêm boundary
});

if (response.status === 201) {
  const partner = await response.json();
  console.log("Tạo thành công:", partner.id);
} else if (response.status === 400) {
  const error = await response.text();
  console.error("Lỗi:", error);
}
```

**React (với input file)**
```tsx
function CreatePartnerForm() {
  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = e.currentTarget;
    const formData = new FormData(form);

    const res = await fetch(`${API_BASE_URL}/api/v1/partners`, {
      method: "POST",
      body: formData,
    });

    if (res.status === 201) {
      const partner = await res.json();
      alert(`Đối tác "${partner.name}" đã được tạo!`);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input name="Name" required placeholder="Tên quán" />
      <input name="Address" required placeholder="Địa chỉ" />
      <input name="PhoneNumber" placeholder="Số điện thoại" />
      <textarea name="Description" placeholder="Mô tả" />
      <input name="AvatarImage" type="file" accept="image/*" />
      <input name="GalleryImages" type="file" accept="image/*" multiple />
      <button type="submit">Tạo đối tác</button>
    </form>
  );
}
```

**cURL**
```bash
curl -X POST "${API_BASE_URL}/api/v1/partners" \
  -F "Name=Phúc Long Coffee & Tea" \
  -F "Address=123 Nguyễn Huệ, Quận 1, TP.HCM" \
  -F "PhoneNumber=0901234567" \
  -F "Description=Chuỗi cà phê nổi tiếng" \
  -F "AvatarImage=@/path/to/avatar.jpg" \
  -F "GalleryImages=@/path/to/photo1.jpg" \
  -F "GalleryImages=@/path/to/photo2.jpg"
```

---

## 4. Cập nhật đối tác

Cập nhật thông tin và/hoặc hình ảnh. Sử dụng `multipart/form-data`.

### Request

```
PUT /api/v1/partners/{id}
Content-Type: multipart/form-data
```

| Field | Type | Bắt buộc | Mô tả |
|-------|------|----------|-------|
| `Name` | string | **Có** | Tên quán |
| `Address` | string | **Có** | Địa chỉ quán |
| `PhoneNumber` | string | Không | Số điện thoại |
| `Description` | string | Không | Mô tả ngắn |
| `AvatarImage` | file | Không | Ảnh đại diện mới (thay thế ảnh cũ) |
| `GalleryImages` | file[] | Không | Gallery mới (thay thế toàn bộ gallery cũ) |

### Lưu ý quan trọng về hình ảnh khi cập nhật

| Trường hợp | Hành vi |
|------------|---------|
| Không gửi `AvatarImage` | Giữ nguyên avatar cũ |
| Gửi `AvatarImage` mới | **Thay thế** avatar cũ |
| Không gửi `GalleryImages` | Giữ nguyên gallery cũ |
| Gửi `GalleryImages` mới | **Thay thế toàn bộ** gallery cũ bằng ảnh mới |

> Nếu muốn giữ gallery cũ và chỉ cập nhật text, **không gửi** field `GalleryImages`.

### Response `200 OK`

Trả về `PartnerResponse` đã cập nhật.

### Response `404 Not Found`

Khi `id` không tồn tại.

### Response `400 Bad Request`

Tên trùng hoặc lỗi ảnh.

### Code ví dụ

```javascript
const formData = new FormData();
formData.append("Name", "Phúc Long Coffee & Tea - Chi nhánh 2");
formData.append("Address", "456 Lê Lợi, Quận 1, TP.HCM");

// Chỉ gửi avatar mới nếu muốn thay đổi
if (newAvatarFile) {
  formData.append("AvatarImage", newAvatarFile);
}

// Chỉ gửi gallery nếu muốn thay thế toàn bộ
if (newGalleryFiles.length > 0) {
  newGalleryFiles.forEach(file => {
    formData.append("GalleryImages", file);
  });
}

const response = await fetch(`${API_BASE_URL}/api/v1/partners/1`, {
  method: "PUT",
  body: formData,
});

if (response.ok) {
  const updated = await response.json();
  console.log("Cập nhật thành công:", updated);
}
```

---

## 5. Xoá đối tác

Xoá đối tác và toàn bộ hình ảnh liên quan.

### Request

```
DELETE /api/v1/partners/{id}
```

### Response `204 No Content`

Xoá thành công (không có body).

### Response `404 Not Found`

Khi `id` không tồn tại.

### Code ví dụ

```javascript
const response = await fetch(`${API_BASE_URL}/api/v1/partners/1`, {
  method: "DELETE",
});

if (response.status === 204) {
  console.log("Đã xoá đối tác");
} else if (response.status === 404) {
  console.log("Đối tác không tồn tại");
}
```

---

## Tổng hợp Response Schema

### PartnerResponse

```typescript
interface PartnerResponse {
  id: number;
  name: string;
  address: string;
  phoneNumber: string | null;
  description: string | null;
  avatarImageUrl: string | null;
  galleryImages: PartnerImageResponse[];
  createdAtUtc: string; // ISO 8601 UTC
}
```

### PartnerImageResponse

```typescript
interface PartnerImageResponse {
  id: number;
  imageUrl: string;
  displayOrder: number; // Gallery: 1, 2, 3...
}
```

### PagedResult\<T\>

```typescript
interface PagedResult<T> {
  items: T[];
  totalCount: number; // Tổng số records
  page: number;       // Trang hiện tại
  pageSize: number;   // Số item mỗi trang
}
```

---

## Tổng hợp HTTP Status Codes

| Code | Ý nghĩa | Khi nào |
|------|---------|---------|
| `200` | Thành công | GET list, GET detail, PUT |
| `201` | Tạo thành công | POST |
| `204` | Xoá thành công | DELETE |
| `400` | Dữ liệu không hợp lệ | Tên trùng, file sai định dạng |
| `404` | Không tìm thấy | ID không tồn tại |

---

## Hình ảnh - Quy tắc

| Thuộc tính | Avatar | Gallery |
|------------|--------|---------|
| Số lượng | 1 | Nhiều |
| Max kích thước | 1200x1200 px | 1000x1000 px |
| Định dạng | .jpg, .jpeg, .png, .webp, .gif | .jpg, .jpeg, .png, .webp, .gif |
| Lưu trữ | Cloudinary | Cloudinary |
| URL trả về | `avatarImageUrl` | `galleryImages[].imageUrl` |

Ảnh tự động resize nếu vượt kích thước tối đa (giữ tỷ lệ).

---

## Phân trang - Gợi ý Client

```typescript
// Tính tổng số trang
const totalPages = Math.ceil(data.totalCount / data.pageSize);

// Kiểm tra có trang tiếp không
const hasNextPage = data.page < totalPages;
const hasPrevPage = data.page > 1;
```
