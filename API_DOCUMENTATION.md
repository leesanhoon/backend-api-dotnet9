# Tai Lieu Tich Hop API

**Phien ban:** v1.0
**Ngay cap nhat:** 18/06/2026
**Base URL:** `https://<domain>/api/v1`
**Swagger UI:** `https://<domain>/swagger`
**Health Check:** `GET https://<domain>/health`

---

## Muc Luc

1. [Thong Tin Chung](#1-thong-tin-chung)
2. [Danh Muc (Categories)](#2-danh-muc-categories)
3. [San Pham (Products)](#3-san-pham-products)
4. [Don Hang (Orders)](#4-don-hang-orders)
5. [Doi Tac (Partners)](#5-doi-tac-partners)
6. [Ma Loi & Xu Ly Loi](#6-ma-loi--xu-ly-loi)

---

## 1. Thong Tin Chung

### Base URL

```
https://<domain>/api/v1
```

### API Versioning

API ho tro versioning qua 2 cach:

| Cach | Vi du |
|---|---|
| URL segment | `/api/v1/products` |
| Header | `x-api-version: 1.0` |

### Phan Trang (Pagination)

Tat ca cac endpoint tra ve danh sach deu ho tro phan trang:

| Tham so | Kieu | Mac dinh | Mo ta |
|---|---|---|---|
| `page` | int | 1 | So trang (bat dau tu 1) |
| `pageSize` | int | 10 | So ban ghi moi trang (toi da 100) |

**Response phan trang:**

```json
{
  "items": [],
  "totalCount": 50,
  "page": 1,
  "pageSize": 10
}
```

### Dinh Dang Chung

- Request body: `application/json` (tru khi ghi chu `multipart/form-data`)
- Response: `application/json`
- Ngay gio: ISO 8601 UTC (`2026-06-18T03:00:00Z`)
- Tien te: `decimal` (18,2)
- ID: `integer`

### Upload File

- Gioi han kich thuoc tong: **50 MB**
- Anh avatar toi da: **1200x1200 px**
- Anh gallery toi da: **1000x1000 px**
- Kich thuoc moi file toi da: **10 MB**
- So luong anh gallery toi da: **10 anh**
- Dinh dang ho tro: JPEG, PNG
- Anh tu dong nen ve JPEG voi chat luong 82%

---

## 2. Danh Muc (Categories)

Quan ly danh muc san pham theo cau truc phan cap (cay danh muc).

### 2.1 Lay Danh Sach Danh Muc

```
GET /api/v1/categories?page=1&pageSize=10
```

**Response** `200 OK`:

```json
{
  "items": [
    {
      "id": 1,
      "name": "Ly giay",
      "description": "Cac loai ly giay",
      "parentId": null,
      "isRoot": true
    },
    {
      "id": 2,
      "name": "Ly giay nong",
      "description": "Ly giay dung do nong",
      "parentId": 1,
      "isRoot": false
    }
  ],
  "totalCount": 2,
  "page": 1,
  "pageSize": 10
}
```

### 2.2 Lay Cay Danh Muc

```
GET /api/v1/categories/tree
```

**Response** `200 OK`:

```json
[
  {
    "id": 1,
    "name": "Ly giay",
    "description": "Cac loai ly giay",
    "parentId": null,
    "isRoot": true,
    "children": [
      {
        "id": 2,
        "name": "Ly giay nong",
        "description": "Ly giay dung do nong",
        "parentId": 1,
        "isRoot": false,
        "children": []
      },
      {
        "id": 3,
        "name": "Ly giay lanh",
        "description": null,
        "parentId": 1,
        "isRoot": false,
        "children": []
      }
    ]
  }
]
```

### 2.3 Lay Chi Tiet Danh Muc

```
GET /api/v1/categories/{id}
```

**Response** `200 OK`:

```json
{
  "id": 1,
  "name": "Ly giay",
  "description": "Cac loai ly giay",
  "parentId": null,
  "isRoot": true
}
```

**Response** `404 Not Found` — Khong tim thay danh muc.

### 2.4 Tao Danh Muc

```
POST /api/v1/categories
Content-Type: application/json
```

**Request body:**

```json
{
  "name": "Ly giay nong",
  "description": "Ly giay dung do uong nong",
  "parentId": 1
}
```

| Truong | Kieu | Bat buoc | Mo ta |
|---|---|---|---|
| `name` | string | Co | Ten danh muc (toi da 150 ky tu) |
| `description` | string | Khong | Mo ta (toi da 500 ky tu) |
| `parentId` | int | Khong | ID danh muc cha (`null` = danh muc goc) |

**Response** `201 Created`:

```json
{
  "id": 2,
  "name": "Ly giay nong",
  "description": "Ly giay dung do uong nong",
  "parentId": 1,
  "isRoot": false
}
```

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `400` | `parentId` khong ton tai |

### 2.5 Cap Nhat Danh Muc

```
PUT /api/v1/categories/{id}
Content-Type: application/json
```

**Request body:** Giong nhu tao danh muc.

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `404` | Khong tim thay danh muc |
| `400` | Khong the sua danh muc goc |
| `400` | ParentId khong ton tai |
| `400` | Khong the chon danh muc con lam cha (tranh vong lap) |

### 2.6 Xoa Danh Muc

```
DELETE /api/v1/categories/{id}
```

**Response** `204 No Content` — Xoa thanh cong.

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `404` | Khong tim thay danh muc |
| `400` | Khong the xoa danh muc goc |
| `400` | Khong the xoa danh muc dang co danh muc con |
| `400` | Khong the xoa danh muc dang co san pham |

---

## 3. San Pham (Products)

Quan ly san pham va nap (lid). Nap duoc luu tru nhu san pham voi `capacityMl = 0`.

### Cau Truc Du Lieu San Pham

```json
{
  "id": 1,
  "name": "Ly giay 500ml",
  "description": "Ly giay dung do uong lanh",
  "categoryId": 2,
  "categoryName": "Ly giay lanh",
  "avatarImageUrl": "https://res.cloudinary.com/.../avatar.jpg",
  "galleryImages": [
    {
      "id": 10,
      "imageUrl": "https://res.cloudinary.com/.../gallery1.jpg",
      "imageType": "Gallery",
      "displayOrder": 0,
      "createdAtUtc": "2026-06-18T00:00:00Z"
    }
  ],
  "variants": [
    {
      "id": 5,
      "capacityMl": 500,
      "diameterMm": 90,
      "sizeName": null,
      "priceTiers": [
        {
          "id": 1,
          "minQuantity": 100,
          "unitPrice": 150.00
        },
        {
          "id": 2,
          "minQuantity": 1000,
          "unitPrice": 120.00
        }
      ]
    }
  ],
  "lids": [
    {
      "id": 1,
      "compatibleProductId": 8,
      "compatibleProductName": "Nap phang 90mm"
    }
  ]
}
```

### Cau Truc Du Lieu Nap (Lid)

Nap cung la san pham, nhung co `capacityMl = 0` trong variant:

```json
{
  "id": 8,
  "name": "Nap phang 90mm",
  "description": "Nap phang cho ly 90mm",
  "categoryId": 3,
  "categoryName": "Nap",
  "avatarImageUrl": "https://res.cloudinary.com/.../lid-avatar.jpg",
  "galleryImages": [],
  "variants": [
    {
      "id": 12,
      "capacityMl": 0,
      "diameterMm": 90,
      "sizeName": "Tieu chuan",
      "priceTiers": [
        {
          "id": 5,
          "minQuantity": 1,
          "unitPrice": 50.00
        }
      ]
    }
  ],
  "lids": []
}
```

### 3.1 Lay Danh Sach San Pham

```
GET /api/v1/products?page=1&pageSize=10
```

Tra ve tat ca san pham (bao gom ca nap).

**Response** `200 OK`:

```json
{
  "items": [ /* Mang ProductResponse */ ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10
}
```

### 3.2 Lay Chi Tiet San Pham

```
GET /api/v1/products/{id}
```

**Response** `200 OK`: Tra ve `ProductResponse`.

**Response** `404 Not Found` — Khong tim thay san pham.

### 3.3 Tao San Pham

```
POST /api/v1/products
Content-Type: multipart/form-data
```

**Form fields:**

| Truong | Kieu | Bat buoc | Mo ta |
|---|---|---|---|
| `Name` | string | Co | Ten san pham (toi da 200 ky tu) |
| `Description` | string | Khong | Mo ta (toi da 1000 ky tu) |
| `CategoryId` | int | Co | ID danh muc |
| `AvatarImage` | file | Khong | Anh dai dien |
| `GalleryImages` | file[] | Khong | Anh bo sung (toi da 10 anh) |
| `Variants` | JSON string | Co | Danh sach bien the |
| `CompatibleProductIds` | JSON string | Khong | Danh sach ID nap tuong thich |

**Variants format (JSON string):**

```json
[
  {
    "capacityMl": 500,
    "diameterMm": 90,
    "priceTiers": [
      { "minQuantity": 100, "unitPrice": 150.00 },
      { "minQuantity": 1000, "unitPrice": 120.00 }
    ]
  }
]
```

**CompatibleProductIds format (JSON string):**

```json
[8, 9]
```

**Vi du tao nap:** Dat `capacityMl = 0`, su dung `diameterMm` de xac dinh kich thuoc:

```json
[
  {
    "capacityMl": 0,
    "diameterMm": 90,
    "priceTiers": [
      { "minQuantity": 1, "unitPrice": 50.00 }
    ]
  }
]
```

**Response** `201 Created`: Tra ve `ProductResponse`.

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `400` | CategoryId khong ton tai |
| `400` | Loi validation du lieu |
| `400` | Loi xu ly anh |

### 3.4 Cap Nhat San Pham

```
PUT /api/v1/products/{id}
Content-Type: application/json
```

**Request body:**

```json
{
  "name": "Ly giay 500ml (cap nhat)",
  "description": "Mo ta moi",
  "categoryId": 2,
  "variants": [
    {
      "capacityMl": 500,
      "diameterMm": 90,
      "priceTiers": [
        { "minQuantity": 100, "unitPrice": 160.00 },
        { "minQuantity": 1000, "unitPrice": 130.00 }
      ]
    }
  ],
  "compatibleProductIds": [8, 9]
}
```

| Truong | Kieu | Bat buoc | Mo ta |
|---|---|---|---|
| `name` | string | Co | Ten san pham |
| `description` | string | Khong | Mo ta |
| `categoryId` | int | Co | ID danh muc |
| `variants` | array | Co | Danh sach bien the (se thay the toan bo) |
| `compatibleProductIds` | int[] | Khong | ID nap tuong thich (se thay the toan bo) |

> **Luu y:** Cap nhat se **thay the toan bo** variants va compatibleProductIds. Gui lai day du du lieu.

**Response** `200 OK`: Tra ve `ProductResponse`.

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `404` | Khong tim thay san pham |
| `400` | CategoryId khong ton tai |
| `400` | Loi validation du lieu |

### 3.5 Xoa San Pham

```
DELETE /api/v1/products/{id}
```

**Response** `204 No Content` — Xoa thanh cong.

**Response** `404 Not Found` — Khong tim thay san pham.

### 3.6 Lay Danh Sach Nap Tuong Thich

```
GET /api/v1/products/{id}/compatible-lids
```

Tra ve danh sach san pham nap tuong thich voi san pham chi dinh.

**Response** `200 OK`:

```json
[
  {
    "id": 8,
    "name": "Nap phang 90mm",
    "description": "Nap phang cho ly 90mm",
    "categoryId": 3,
    "categoryName": "Nap",
    "avatarImageUrl": "https://...",
    "galleryImages": [],
    "variants": [
      {
        "id": 12,
        "capacityMl": 0,
        "diameterMm": 90,
        "sizeName": "Tieu chuan",
        "priceTiers": [
          { "id": 5, "minQuantity": 1, "unitPrice": 50.00 }
        ]
      }
    ],
    "lids": []
  }
]
```

### 3.7 Them Anh Cho San Pham

```
POST /api/v1/products/{id}/images
Content-Type: multipart/form-data
```

| Truong | Kieu | Bat buoc | Mo ta |
|---|---|---|---|
| `AvatarImage` | file | Khong | Anh dai dien moi (thay the anh cu) |
| `GalleryImages` | file[] | Khong | Anh gallery bo sung |

**Response** `200 OK`: Tra ve `ProductResponse` da cap nhat.

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `404` | Khong tim thay san pham |
| `400` | Loi xu ly anh |

### 3.8 Xoa Anh San Pham

```
DELETE /api/v1/products/{id}/images/{imageId}
```

**Response** `204 No Content` — Xoa thanh cong.

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `404` | Khong tim thay san pham |
| `404` | Image khong ton tai |

---

## 4. Don Hang (Orders)

### Trang Thai Don Hang

| Gia tri | Mo ta |
|---|---|
| `PendingConfirmation` | Cho xac nhan |
| `Confirmed` | Da xac nhan |
| `Shipping` | Dang giao hang |
| `Completed` | Hoan thanh |
| `Cancelled` | Da huy |

### 4.1 Tao Don Hang

```
POST /api/v1/orders
Content-Type: application/json
```

**Request body:**

```json
{
  "customerName": "Nguyen Van A",
  "customerPhone": "0901234567",
  "customerEmail": "nguyenvana@email.com",
  "note": "Giao truoc 5h chieu",
  "items": [
    {
      "productId": 1,
      "quantity": 1000,
      "unitPrice": 150.00,
      "materialId": null,
      "printTypeId": null,
      "lidId": 8
    },
    {
      "productId": 2,
      "quantity": 500,
      "unitPrice": 200.00,
      "materialId": null,
      "printTypeId": null,
      "lidId": null
    }
  ]
}
```

| Truong | Kieu | Bat buoc | Mo ta |
|---|---|---|---|
| `customerName` | string | Co | Ten khach hang (toi da 200 ky tu) |
| `customerPhone` | string | Co | So dien thoai (toi da 20 ky tu) |
| `customerEmail` | string | Khong | Email (toi da 200 ky tu) |
| `note` | string | Khong | Ghi chu (toi da 1000 ky tu) |
| `items` | array | Co | Danh sach san pham (it nhat 1) |

**Chi tiet item:**

| Truong | Kieu | Bat buoc | Mo ta |
|---|---|---|---|
| `productId` | int | Co | ID san pham |
| `quantity` | int | Co | So luong |
| `unitPrice` | decimal | Co | Don gia |
| `materialId` | int | Khong | ID chat lieu |
| `printTypeId` | int | Khong | ID kieu in |
| `lidId` | int | Khong | ID san pham nap di kem |

**Response** `201 Created`:

```json
{
  "id": 42,
  "customerName": "Nguyen Van A",
  "customerPhone": "0901234567",
  "customerEmail": "nguyenvana@email.com",
  "note": "Giao truoc 5h chieu",
  "totalAmount": 250000.00,
  "status": "PendingConfirmation",
  "createdAtUtc": "2026-06-18T03:00:00Z",
  "items": [
    {
      "productId": 1,
      "productName": "Ly giay 500ml",
      "materialId": null,
      "materialName": null,
      "printTypeId": null,
      "printTypeName": null,
      "lidId": 8,
      "lidName": "Nap phang 90mm",
      "quantity": 1000,
      "unitPrice": 150.00
    },
    {
      "productId": 2,
      "productName": "Ly giay 700ml",
      "materialId": null,
      "materialName": null,
      "printTypeId": null,
      "printTypeName": null,
      "lidId": null,
      "lidName": null,
      "quantity": 500,
      "unitPrice": 200.00
    }
  ]
}
```

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `400` | Danh sach san pham khong duoc trong |

> **Ghi chu:** Khi tao don hang thanh cong, he thong se gui thong bao qua Telegram.

### 4.2 Tra Cuu Don Hang

```
GET /api/v1/orders/track?orderId=42&phone=0901234567
```

| Tham so | Kieu | Bat buoc | Mo ta |
|---|---|---|---|
| `orderId` | int | Co | ID don hang |
| `phone` | string | Co | So dien thoai dat hang |

**Response** `200 OK`: Tra ve `OrderDetailDto` (giong response tao don hang).

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `400` | So dien thoai khong duoc trong |
| `404` | Khong tim thay don hang |

### 4.3 Lay Danh Sach Don Hang

```
GET /api/v1/orders?page=1&pageSize=10&status=Confirmed
```

| Tham so | Kieu | Bat buoc | Mo ta |
|---|---|---|---|
| `page` | int | Khong | So trang (mac dinh: 1) |
| `pageSize` | int | Khong | So ban ghi moi trang (mac dinh: 10, toi da: 100) |
| `status` | string | Khong | Loc theo trang thai |

**Response** `200 OK`:

```json
{
  "items": [
    {
      "id": 42,
      "customerName": "Nguyen Van A",
      "totalAmount": 250000.00,
      "status": "Confirmed",
      "createdAtUtc": "2026-06-18T03:00:00Z"
    }
  ],
  "totalCount": 5,
  "page": 1,
  "pageSize": 10
}
```

### 4.4 Cap Nhat Trang Thai Don Hang

```
PATCH /api/v1/orders/{id}/status
Content-Type: application/json
```

**Request body:**

```json
{
  "status": "Confirmed"
}
```

| Truong | Kieu | Bat buoc | Gia tri hop le |
|---|---|---|---|
| `status` | string | Co | `PendingConfirmation`, `Confirmed`, `Shipping`, `Completed`, `Cancelled` |

**Response** `200 OK`: Tra ve `OrderDetailDto`.

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `404` | Khong tim thay don hang |
| `400` | Trang thai khong hop le |

---

## 5. Doi Tac (Partners)

### Cau Truc Du Lieu Doi Tac

```json
{
  "id": 1,
  "name": "Cong ty TNHH ABC",
  "address": "123 Nguyen Hue, Q1, TP.HCM",
  "phoneNumber": "0281234567",
  "description": "Doi tac cung cap nguyen lieu",
  "avatarImageUrl": "https://res.cloudinary.com/.../avatar.jpg",
  "galleryImages": [
    {
      "id": 5,
      "imageUrl": "https://res.cloudinary.com/.../gallery1.jpg",
      "displayOrder": 0
    }
  ],
  "createdAtUtc": "2026-06-18T00:00:00Z"
}
```

### 5.1 Lay Danh Sach Doi Tac

```
GET /api/v1/partners?page=1&pageSize=10
```

**Response** `200 OK`:

```json
{
  "items": [ /* Mang PartnerResponse */ ],
  "totalCount": 3,
  "page": 1,
  "pageSize": 10
}
```

### 5.2 Lay Chi Tiet Doi Tac

```
GET /api/v1/partners/{id}
```

**Response** `200 OK`: Tra ve `PartnerResponse`.

**Response** `404 Not Found` — Khong tim thay doi tac.

### 5.3 Tao Doi Tac

```
POST /api/v1/partners
Content-Type: multipart/form-data
```

| Truong | Kieu | Bat buoc | Mo ta |
|---|---|---|---|
| `Name` | string | Co | Ten doi tac (toi da 200 ky tu) |
| `Address` | string | Co | Dia chi (toi da 500 ky tu) |
| `PhoneNumber` | string | Khong | So dien thoai (toi da 20 ky tu) |
| `Description` | string | Khong | Mo ta (toi da 2000 ky tu) |
| `AvatarImage` | file | Khong | Anh dai dien |
| `GalleryImages` | file[] | Khong | Anh bo sung |

**Response** `201 Created`: Tra ve `PartnerResponse`.

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `400` | Loi validation du lieu |
| `400` | Loi xu ly anh |

### 5.4 Cap Nhat Doi Tac

```
PUT /api/v1/partners/{id}
Content-Type: multipart/form-data
```

**Form fields:** Giong nhu tao doi tac.

**Loi co the gap:**

| Ma | Mo ta |
|---|---|
| `404` | Khong tim thay doi tac |
| `400` | Loi validation du lieu |
| `400` | Loi xu ly anh |

### 5.5 Xoa Doi Tac

```
DELETE /api/v1/partners/{id}
```

**Response** `204 No Content` — Xoa thanh cong.

**Response** `404 Not Found` — Khong tim thay doi tac.

---

## 6. Ma Loi & Xu Ly Loi

### Ma HTTP Tra Ve

| Ma | Y nghia | Mo ta |
|---|---|---|
| `200` | OK | Thanh cong |
| `201` | Created | Tao moi thanh cong |
| `204` | No Content | Xoa thanh cong |
| `400` | Bad Request | Du lieu gui len khong hop le |
| `404` | Not Found | Khong tim thay tai nguyen |
| `500` | Internal Server Error | Loi he thong |

### Dinh Dang Loi

**Loi validation (400):**

```json
{
  "message": "CategoryId 999 khong ton tai."
}
```

**Loi he thong (500):**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500
}
```

---

## Phu Luc: Bang Tong Hop API

| STT | Phuong thuc | Endpoint | Mo ta | Content-Type |
|---|---|---|---|---|
| 1 | `GET` | `/api/v1/categories` | Lay danh sach danh muc | - |
| 2 | `GET` | `/api/v1/categories/tree` | Lay cay danh muc | - |
| 3 | `GET` | `/api/v1/categories/{id}` | Lay chi tiet danh muc | - |
| 4 | `POST` | `/api/v1/categories` | Tao danh muc | `application/json` |
| 5 | `PUT` | `/api/v1/categories/{id}` | Cap nhat danh muc | `application/json` |
| 6 | `DELETE` | `/api/v1/categories/{id}` | Xoa danh muc | - |
| 7 | `GET` | `/api/v1/products` | Lay danh sach san pham | - |
| 8 | `GET` | `/api/v1/products/{id}` | Lay chi tiet san pham | - |
| 9 | `POST` | `/api/v1/products` | Tao san pham | `multipart/form-data` |
| 10 | `PUT` | `/api/v1/products/{id}` | Cap nhat san pham | `application/json` |
| 11 | `DELETE` | `/api/v1/products/{id}` | Xoa san pham | - |
| 12 | `GET` | `/api/v1/products/{id}/compatible-lids` | Lay nap tuong thich | - |
| 13 | `POST` | `/api/v1/products/{id}/images` | Them anh san pham | `multipart/form-data` |
| 14 | `DELETE` | `/api/v1/products/{id}/images/{imageId}` | Xoa anh san pham | - |
| 15 | `POST` | `/api/v1/orders` | Tao don hang | `application/json` |
| 16 | `GET` | `/api/v1/orders/track` | Tra cuu don hang | - |
| 17 | `GET` | `/api/v1/orders` | Lay danh sach don hang | - |
| 18 | `PATCH` | `/api/v1/orders/{id}/status` | Cap nhat trang thai | `application/json` |
| 19 | `GET` | `/api/v1/partners` | Lay danh sach doi tac | - |
| 20 | `GET` | `/api/v1/partners/{id}` | Lay chi tiet doi tac | - |
| 21 | `POST` | `/api/v1/partners` | Tao doi tac | `multipart/form-data` |
| 22 | `PUT` | `/api/v1/partners/{id}` | Cap nhat doi tac | `multipart/form-data` |
| 23 | `DELETE` | `/api/v1/partners/{id}` | Xoa doi tac | - |
