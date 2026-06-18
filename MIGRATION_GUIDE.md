# API Migration Guide — Lid Merged into Product

**Date:** 2026-06-18
**Version:** Breaking Change
**Affected endpoints:** Products, Orders

---

## Overview

The separate **Lid** entity has been removed. Lids are now stored as regular **Products** in the `products` table. All lid-related API endpoints (`/api/v1/lids`) have been removed. Clients must update their integrations to use the unified Product API.

---

## What Changed

| Before (Old) | After (New) |
|---|---|
| Separate `Lid` entity with its own table | Lids are now `Product` records |
| `GET /api/v1/lids` | Removed — use `GET /api/v1/products` |
| `POST /api/v1/lids` | Removed — use `POST /api/v1/products` |
| `PUT /api/v1/lids/{id}` | Removed — use `PUT /api/v1/products/{id}` |
| `DELETE /api/v1/lids/{id}` | Removed — use `DELETE /api/v1/products/{id}` |
| `LidId` in orders references `lids` table | `LidId` in orders now references `products` table |
| `product_lids.LidId` column | Renamed to `product_lids.CompatibleProductId` |

---

## Breaking Changes

### 1. Lid endpoints removed

All `/api/v1/lids` endpoints no longer exist. Use the `/api/v1/products` endpoints instead to create, read, update, and delete lids.

### 2. Lids are created as Products

To create a lid, use `POST /api/v1/products` with `CapacityMl = 0` in the variant to distinguish it from a regular product.

### 3. Compatible lids use Product IDs

The field `CompatibleProductIds` in create/update product requests now expects Product IDs (not old Lid IDs).

### 4. Order items reference Product IDs for lids

`LidId` in order items now points to the `products` table. Old Lid IDs have been remapped to new Product IDs.

---

## API Reference (Current)

### Products

#### `GET /api/v1/products`

List all products (including lids) with pagination.

**Query Parameters:**

| Param | Type | Default | Description |
|---|---|---|---|
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page (max 100) |

**Response:**

```json
{
  "items": [
    {
      "id": 1,
      "name": "Paper Cup 500ml",
      "description": "Disposable paper cup",
      "categoryId": 2,
      "categoryName": "Cups",
      "avatarImageUrl": "https://...",
      "galleryImages": [
        {
          "id": 10,
          "imageUrl": "https://...",
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
              "unitPrice": 0.15
            }
          ]
        }
      ],
      "lids": [
        {
          "id": 1,
          "compatibleProductId": 8,
          "compatibleProductName": "Flat Lid 90mm"
        }
      ]
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10
}
```

#### `GET /api/v1/products/{id}`

Get a single product by ID.

**Response:** Same shape as a single item in the list above. Returns `404` if not found.

#### `POST /api/v1/products`

Create a new product or lid. Uses `multipart/form-data`.

**Form Fields:**

| Field | Type | Required | Description |
|---|---|---|---|
| `Name` | string | Yes | Product name (max 200 chars) |
| `Description` | string | No | Description (max 1000 chars) |
| `CategoryId` | int | Yes | Category ID |
| `AvatarImage` | file | No | Avatar image file |
| `GalleryImages` | file[] | No | Gallery image files |
| `Variants` | JSON array | Yes | Product variants (see below) |
| `CompatibleProductIds` | JSON array | No | IDs of compatible lid products |

**Variants format:**

```json
[
  {
    "capacityMl": 500,
    "diameterMm": 90,
    "priceTiers": [
      { "minQuantity": 100, "unitPrice": 0.15 },
      { "minQuantity": 1000, "unitPrice": 0.12 }
    ]
  }
]
```

> **Tip:** To create a lid product, set `capacityMl` to `0` and use `diameterMm` + `sizeName` to define lid sizes.

**Response:** `201 Created` with `ProductResponse` body.

#### `PUT /api/v1/products/{id}`

Update an existing product. Uses `application/json`.

**Request Body:**

```json
{
  "name": "Updated Cup Name",
  "description": "Updated description",
  "categoryId": 2,
  "variants": [
    {
      "capacityMl": 500,
      "diameterMm": 90,
      "priceTiers": [
        { "minQuantity": 100, "unitPrice": 0.16 }
      ]
    }
  ],
  "compatibleProductIds": [8, 9]
}
```

**Response:** `ProductResponse`

#### `DELETE /api/v1/products/{id}`

Delete a product. Returns `204 No Content` on success, `404` if not found.

#### `GET /api/v1/products/{id}/compatible-lids`

Get all compatible lid products for a given product.

**Response:**

```json
[
  {
    "id": 8,
    "name": "Flat Lid 90mm",
    "description": "Flat lid for 90mm cups",
    "categoryId": 3,
    "categoryName": "Lids",
    "avatarImageUrl": "https://...",
    "galleryImages": [],
    "variants": [
      {
        "id": 12,
        "capacityMl": 0,
        "diameterMm": 90,
        "sizeName": "Standard",
        "priceTiers": [
          { "id": 5, "minQuantity": 1, "unitPrice": 0.05 }
        ]
      }
    ],
    "lids": []
  }
]
```

#### `POST /api/v1/products/{id}/images`

Upload images for a product. Uses `multipart/form-data`.

| Field | Type | Required | Description |
|---|---|---|---|
| `AvatarImage` | file | No | New avatar image (replaces existing) |
| `GalleryImages` | file[] | No | Additional gallery images |

**Response:** `ProductResponse`

#### `DELETE /api/v1/products/{id}/images/{imageId}`

Delete a specific image. Returns `204 No Content`.

---

### Orders

#### `POST /api/v1/orders`

Create a new order.

**Request Body (`application/json`):**

```json
{
  "customerName": "John Doe",
  "customerPhone": "+84123456789",
  "customerEmail": "john@example.com",
  "note": "Deliver before 5pm",
  "items": [
    {
      "productId": 1,
      "quantity": 1000,
      "unitPrice": 0.15,
      "lidId": 8
    }
  ]
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `customerName` | string | Yes | Customer name |
| `customerPhone` | string | Yes | Customer phone |
| `customerEmail` | string | No | Customer email |
| `note` | string | No | Order note |
| `items` | array | Yes | At least 1 item |
| `items[].productId` | int | Yes | Product ID |
| `items[].quantity` | int | Yes | Quantity |
| `items[].unitPrice` | decimal | Yes | Unit price |
| `items[].lidId` | int | No | Lid product ID (now references `products` table) |

**Response:** `201 Created`

```json
{
  "id": 42,
  "customerName": "John Doe",
  "customerPhone": "+84123456789",
  "customerEmail": "john@example.com",
  "note": "Deliver before 5pm",
  "totalAmount": 150.00,
  "status": "PendingConfirmation",
  "createdAtUtc": "2026-06-18T03:00:00Z",
  "items": [
    {
      "productId": 1,
      "productName": "Paper Cup 500ml",
      "materialId": null,
      "materialName": null,
      "printTypeId": null,
      "printTypeName": null,
      "lidId": 8,
      "lidName": "Flat Lid 90mm",
      "quantity": 1000,
      "unitPrice": 0.15
    }
  ]
}
```

#### `GET /api/v1/orders/track`

Track an order by ID and phone number.

| Param | Type | Required |
|---|---|---|
| `orderId` | int | Yes |
| `phone` | string | Yes |

**Response:** `OrderDetailDto` (same shape as create response). Returns `404` if not found.

#### `GET /api/v1/orders`

List all orders with pagination and optional status filter.

| Param | Type | Default | Description |
|---|---|---|---|
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page (max 100) |
| `status` | string | null | Filter by status |

**Response:**

```json
{
  "items": [
    {
      "id": 42,
      "customerName": "John Doe",
      "totalAmount": 150.00,
      "status": "PendingConfirmation",
      "createdAtUtc": "2026-06-18T03:00:00Z"
    }
  ],
  "totalCount": 5,
  "page": 1,
  "pageSize": 10
}
```

#### `PATCH /api/v1/orders/{id}/status`

Update order status.

**Request Body:**

```json
{
  "status": "Confirmed"
}
```

**Valid statuses:** `PendingConfirmation`, `Confirmed`, `Shipping`, `Completed`, `Cancelled`

**Response:** `OrderDetailDto`

---

### Categories

#### `GET /api/v1/categories`

List categories with pagination.

#### `GET /api/v1/categories/tree`

Get full category tree structure.

#### `GET /api/v1/categories/{id}`

Get a single category.

#### `POST /api/v1/categories`

Create a category.

```json
{
  "name": "Cups",
  "description": "All types of cups",
  "parentId": null
}
```

#### `PUT /api/v1/categories/{id}`

Update a category (same body as create).

#### `DELETE /api/v1/categories/{id}`

Delete a category. Returns `204`.

---

### Partners

#### `GET /api/v1/partners`

List partners with pagination.

#### `GET /api/v1/partners/{id}`

Get a single partner.

#### `POST /api/v1/partners`

Create a partner (`multipart/form-data`).

| Field | Type | Required |
|---|---|---|
| `Name` | string | Yes |
| `Address` | string | Yes |
| `PhoneNumber` | string | No |
| `Description` | string | No |
| `AvatarImage` | file | No |
| `GalleryImages` | file[] | No |

#### `PUT /api/v1/partners/{id}`

Update a partner (same form as create).

#### `DELETE /api/v1/partners/{id}`

Delete a partner. Returns `204`.

---

## Migration Checklist for Clients

- [ ] Remove all calls to `/api/v1/lids` endpoints
- [ ] Use `/api/v1/products` to manage lids (create with `capacityMl = 0`)
- [ ] Update any stored Lid IDs — they are now Product IDs
- [ ] Update order creation to pass lid Product IDs in `lidId` field
- [ ] Update UI to fetch compatible lids via `GET /api/v1/products/{id}/compatible-lids`
- [ ] Update any references from `LidId` (old lid table) to new Product IDs
