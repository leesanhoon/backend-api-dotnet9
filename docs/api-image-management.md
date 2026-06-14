# Image Management API — Integration Guide

> **Base URL:** `{host}/api/v1`
> **Content-Type for image uploads:** `multipart/form-data`
> **Supported image formats:** `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`

---

## Table of Contents

- [Image Types](#image-types)
- [Product Images](#product-images)
  - [Create Product (with images)](#1-create-product-with-images)
  - [Add Images to Existing Product](#2-add-images-to-existing-product)
  - [Delete a Product Image](#3-delete-a-product-image)
  - [Product Response Schema](#product-response-schema)
- [Lid Images](#lid-images)
  - [Create Lid (with images)](#4-create-lid-with-images)
  - [Add Images to Existing Lid](#5-add-images-to-existing-lid)
  - [Delete a Lid Image](#6-delete-a-lid-image)
  - [Lid Response Schema](#lid-response-schema)
- [Error Handling](#error-handling)
- [Image Processing Details](#image-processing-details)
- [Frontend Integration Examples](#frontend-integration-examples)

---

## Image Types

Each entity supports two image types:

| Type       | Description                        | DisplayOrder |
|------------|------------------------------------|--------------|
| `avatar`   | Primary/thumbnail image (max 1)    | `0`          |
| `gallery`  | Additional images (unlimited)      | `1, 2, 3...` |

- Uploading a new **avatar** replaces the existing one automatically.
- **Gallery** images are appended (not replaced). Remove individually via DELETE.

---

## Product Images

### 1. Create Product (with images)

Images can be included when creating a product.

```
POST /api/v1/products
Content-Type: multipart/form-data
```

**Form Fields:**

| Field            | Type       | Required | Description                       |
|------------------|------------|----------|-----------------------------------|
| `Name`           | string     | Yes      | Product name                      |
| `Description`    | string     | No       | Product description               |
| `CategoryId`     | int        | Yes      | Category ID                       |
| `AvatarImage`    | file       | No       | Single avatar image file          |
| `GalleryImages`  | file[]     | No       | Multiple gallery image files      |
| `Variants`       | json array | Yes      | Product variants (at least 1)     |
| `LidIds`         | int[]      | No       | Associated lid IDs                |

**cURL Example:**

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
  -F "Variants[0].PriceTiers[0].UnitPrice=500"
```

**Response:** `201 Created` → [ProductResponse](#product-response-schema)

---

### 2. Add Images to Existing Product

Upload additional images to a product that already exists. If an avatar is uploaded, it replaces the current avatar.

```
POST /api/v1/products/{id}/images
Content-Type: multipart/form-data
```

**Form Fields:**

| Field           | Type   | Required | Description                                      |
|-----------------|--------|----------|--------------------------------------------------|
| `AvatarImage`   | file   | No       | New avatar (replaces existing if any)             |
| `GalleryImages` | file[] | No       | Additional gallery images (appended, not replaced)|

> At least one of `AvatarImage` or `GalleryImages` should be provided.

**cURL Example:**

```bash
# Add gallery images only
curl -X POST "{host}/api/v1/products/5/images" \
  -F "GalleryImages=@/path/to/new-photo1.jpg" \
  -F "GalleryImages=@/path/to/new-photo2.jpg"

# Replace avatar
curl -X POST "{host}/api/v1/products/5/images" \
  -F "AvatarImage=@/path/to/new-avatar.jpg"

# Both at once
curl -X POST "{host}/api/v1/products/5/images" \
  -F "AvatarImage=@/path/to/new-avatar.jpg" \
  -F "GalleryImages=@/path/to/new-photo.jpg"
```

**Response:** `200 OK` → [ProductResponse](#product-response-schema) (full product with updated images)

**Errors:**

| Status | Condition              |
|--------|------------------------|
| `404`  | Product ID not found   |
| `400`  | Invalid image format or upload failure |

---

### 3. Delete a Product Image

Remove a single image from a product by its image ID.

```
DELETE /api/v1/products/{id}/images/{imageId}
```

**Path Parameters:**

| Parameter | Type | Description                        |
|-----------|------|------------------------------------|
| `id`      | int  | Product ID                         |
| `imageId` | int  | Image ID (from `galleryImages[].id` or avatar image ID) |

**cURL Example:**

```bash
curl -X DELETE "{host}/api/v1/products/5/images/12"
```

**Response:** `204 No Content`

**Behavior:**
- If the deleted image is the **avatar**, `avatarImageUrl` on the product becomes `null`.
- Gallery images are simply removed; remaining images keep their display order.

**Errors:**

| Status | Condition                              |
|--------|----------------------------------------|
| `404`  | Product not found OR Image not found   |

---

### Product Response Schema

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
    },
    {
      "id": 11,
      "imageUrl": "https://res.cloudinary.com/.../gallery-photo2-20260614.jpg",
      "imageType": "gallery",
      "displayOrder": 2,
      "createdAtUtc": "2026-06-14T08:30:01Z"
    }
  ],
  "variants": [ ... ],
  "lids": [ ... ]
}
```

> **Note:** The `id` field inside `galleryImages` is the `imageId` used for the DELETE endpoint.

---

## Lid Images

### 4. Create Lid (with images)

> **Breaking Change:** The Create Lid endpoint now uses `multipart/form-data` instead of `application/json`.

```
POST /api/v1/lids
Content-Type: multipart/form-data
```

**Form Fields:**

| Field           | Type       | Required | Description                  |
|-----------------|------------|----------|------------------------------|
| `Name`          | string     | Yes      | Lid name                     |
| `Description`   | string     | No       | Lid description              |
| `CategoryId`    | int        | Yes      | Category ID                  |
| `AvatarImage`   | file       | No       | Single avatar image file     |
| `GalleryImages` | file[]     | No       | Multiple gallery image files |
| `Prices`        | json array | Yes      | Price list (at least 1)      |

**cURL Example:**

```bash
curl -X POST "{host}/api/v1/lids" \
  -F "Name=Nắp ly phẳng 95mm" \
  -F "Description=Nắp phẳng có lỗ" \
  -F "CategoryId=2" \
  -F "AvatarImage=@/path/to/lid-avatar.jpg" \
  -F "GalleryImages=@/path/to/lid-photo1.jpg" \
  -F "Prices[0].DiameterMm=95" \
  -F "Prices[0].SizeName=Lớn" \
  -F "Prices[0].UnitPrice=200"
```

**Response:** `201 Created` → [LidResponse](#lid-response-schema)

---

### 5. Add Images to Existing Lid

```
POST /api/v1/lids/{id}/images
Content-Type: multipart/form-data
```

**Form Fields:**

| Field           | Type   | Required | Description                                       |
|-----------------|--------|----------|---------------------------------------------------|
| `AvatarImage`   | file   | No       | New avatar (replaces existing if any)              |
| `GalleryImages` | file[] | No       | Additional gallery images (appended, not replaced) |

**cURL Example:**

```bash
# Add gallery images
curl -X POST "{host}/api/v1/lids/3/images" \
  -F "GalleryImages=@/path/to/lid-detail1.jpg" \
  -F "GalleryImages=@/path/to/lid-detail2.jpg"

# Replace avatar
curl -X POST "{host}/api/v1/lids/3/images" \
  -F "AvatarImage=@/path/to/new-lid-avatar.jpg"
```

**Response:** `200 OK` → [LidResponse](#lid-response-schema) (full lid with updated images)

**Errors:**

| Status | Condition            |
|--------|----------------------|
| `404`  | Lid ID not found     |
| `400`  | Invalid image format or upload failure |

---

### 6. Delete a Lid Image

```
DELETE /api/v1/lids/{id}/images/{imageId}
```

**Path Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `id`      | int  | Lid ID      |
| `imageId` | int  | Image ID    |

**cURL Example:**

```bash
curl -X DELETE "{host}/api/v1/lids/3/images/8"
```

**Response:** `204 No Content`

**Behavior:**
- If the deleted image is the **avatar**, `avatarImageUrl` on the lid becomes `null`.

**Errors:**

| Status | Condition                           |
|--------|-------------------------------------|
| `404`  | Lid not found OR Image not found    |

---

### Lid Response Schema

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
    {
      "id": 1,
      "diameterMm": 95,
      "sizeName": "Lớn",
      "unitPrice": 200.00
    }
  ]
}
```

> **Note:** The `avatarImageUrl` and `galleryImages` fields are **new additions** to the LidResponse.
> The Update Lid endpoint (`PUT /api/v1/lids/{id}`) remains `application/json` and does **not** handle images — use the dedicated image endpoints instead.

---

## Error Handling

All image-related errors return a JSON string message in the response body:

```json
"Chỉ chấp nhận file ảnh định dạng: .jpg, .jpeg, .png, .webp, .gif"
```

| Status | Scenario                                      |
|--------|-----------------------------------------------|
| `400`  | Unsupported file format                       |
| `400`  | Empty file uploaded                           |
| `400`  | Cloudinary upload failure                     |
| `404`  | Entity (Product/Lid) not found                |
| `404`  | Image ID not found on the specified entity    |

---

## Image Processing Details

Images are automatically processed before upload:

| Setting              | Avatar       | Gallery       |
|----------------------|-------------|---------------|
| Max width            | 1200 px     | 1000 px       |
| Max height           | 1200 px     | 1000 px       |
| Resize mode          | Proportional (max fit) | Proportional (max fit) |
| Output format (PNG)  | PNG          | PNG           |
| Output format (other)| JPEG (82%)  | JPEG (82%)    |

- Images smaller than the max dimensions are **not** upscaled.
- Aspect ratio is always preserved.
- Images are stored on **Cloudinary CDN** — URLs are permanent and publicly accessible.

---

## Frontend Integration Examples

### JavaScript / Fetch API

```javascript
// Add images to a product
async function addProductImages(productId, avatarFile, galleryFiles) {
  const formData = new FormData();

  if (avatarFile) {
    formData.append('AvatarImage', avatarFile);
  }

  if (galleryFiles) {
    galleryFiles.forEach(file => {
      formData.append('GalleryImages', file);
    });
  }

  const response = await fetch(`/api/v1/products/${productId}/images`, {
    method: 'POST',
    body: formData,  // Do NOT set Content-Type header — browser sets it with boundary
  });

  return response.json();
}

// Delete a product image
async function deleteProductImage(productId, imageId) {
  await fetch(`/api/v1/products/${productId}/images/${imageId}`, {
    method: 'DELETE',
  });
}
```

### React / Axios

```jsx
import axios from 'axios';

// Add images to a lid
const addLidImages = async (lidId, avatarFile, galleryFiles) => {
  const formData = new FormData();

  if (avatarFile) {
    formData.append('AvatarImage', avatarFile);
  }

  galleryFiles?.forEach(file => {
    formData.append('GalleryImages', file);
  });

  const { data } = await axios.post(`/api/v1/lids/${lidId}/images`, formData);
  return data;  // Returns full LidResponse with updated images
};

// Delete a lid image
const deleteLidImage = async (lidId, imageId) => {
  await axios.delete(`/api/v1/lids/${lidId}/images/${imageId}`);
};
```

---

## Endpoint Summary

| Method   | Endpoint                               | Content-Type         | Description                    |
|----------|----------------------------------------|----------------------|--------------------------------|
| `POST`   | `/api/v1/products`                     | `multipart/form-data`| Create product (with images)   |
| `POST`   | `/api/v1/products/{id}/images`         | `multipart/form-data`| Add images to product          |
| `DELETE` | `/api/v1/products/{id}/images/{imgId}` | —                    | Delete one product image       |
| `POST`   | `/api/v1/lids`                         | `multipart/form-data`| Create lid (with images) ★ NEW |
| `POST`   | `/api/v1/lids/{id}/images`             | `multipart/form-data`| Add images to lid ★ NEW        |
| `DELETE` | `/api/v1/lids/{id}/images/{imgId}`     | —                    | Delete one lid image ★ NEW     |

> ★ **Breaking Change:** `POST /api/v1/lids` changed from `application/json` to `multipart/form-data`.
> The `PUT /api/v1/lids/{id}` endpoint remains `application/json` (no image fields).
