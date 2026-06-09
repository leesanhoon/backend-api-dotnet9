# Hướng dẫn tích hợp API bằng cURL

Base URL ví dụ:

```bash
https://localhost:5001/api/v1
```

Nếu chạy local, thay bằng port thực tế của bạn.

## 1) Categories

### Lấy danh sách (phân trang)
```bash
curl -X GET "https://localhost:5001/api/v1/Categories?page=1&pageSize=10" \
  -H "Accept: application/json"
```

### Lấy chi tiết
```bash
curl -X GET "https://localhost:5001/api/v1/Categories/1" \
  -H "Accept: application/json"
```

### Tạo mới
```bash
curl -X POST "https://localhost:5001/api/v1/Categories" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Áo thun",
    "description": "Danh mục áo thun"
  }'
```

### Cập nhật
```bash
curl -X PUT "https://localhost:5001/api/v1/Categories/1" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Áo khoác",
    "description": "Danh mục áo khoác"
  }'
```

### Xóa
```bash
curl -X DELETE "https://localhost:5001/api/v1/Categories/1"
```

## 2) Products

### Lấy danh sách (phân trang)
```bash
curl -X GET "https://localhost:5001/api/v1/Products?page=1&pageSize=10" \
  -H "Accept: application/json"
```

### Lấy chi tiết
```bash
curl -X GET "https://localhost:5001/api/v1/Products/1" \
  -H "Accept: application/json"
```

### Tạo mới (multipart/form-data với avatar + gallery)
```bash
curl -X POST "https://localhost:5001/api/v1/Products" \
  -F "Name=Áo thun basic" \
  -F "Description=Áo thun màu trắng" \
  -F "Price=199000" \
  -F "StockQuantity=100" \
  -F "CategoryId=1" \
  -F "AvatarImage=@C:/images/avatar.jpg" \
  -F "GalleryImages=@C:/images/gallery-1.jpg" \
  -F "GalleryImages=@C:/images/gallery-2.jpg"
```

### Cập nhật (JSON)
```bash
curl -X PUT "https://localhost:5001/api/v1/Products/1" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Áo thun basic updated",
    "description": "Mô tả mới",
    "price": 209000,
    "stockQuantity": 90,
    "categoryId": 1
  }'
```

### Xóa
```bash
curl -X DELETE "https://localhost:5001/api/v1/Products/1"
```

## 3) Orders

### Lấy danh sách (phân trang)
```bash
curl -X GET "https://localhost:5001/api/v1/Orders?page=1&pageSize=10" \
  -H "Accept: application/json"
```

### Lấy chi tiết
```bash
curl -X GET "https://localhost:5001/api/v1/Orders/1" \
  -H "Accept: application/json"
```

### Tạo đơn hàng
```bash
curl -X POST "https://localhost:5001/api/v1/Orders" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Nguyen Van A",
    "customerPhone": "0900000000",
    "customerEmail": "a@example.com",
    "note": "Giao giờ hành chính",
    "items": [
      {
        "productId": 1,
        "quantity": 10,
        "unitPrice": 15000
      }
    ]
  }'
```

### Cập nhật trạng thái đơn hàng
```bash
# draft → confirmed
curl -X PUT "https://localhost:5001/api/v1/Orders/1/status" \
  -H "Content-Type: application/json" \
  -d '{ "status": "confirmed" }'

# confirmed → shipping
curl -X PUT "https://localhost:5001/api/v1/Orders/1/status" \
  -H "Content-Type: application/json" \
  -d '{ "status": "shipping" }'

# shipping → completed
curl -X PUT "https://localhost:5001/api/v1/Orders/1/status" \
  -H "Content-Type: application/json" \
  -d '{ "status": "completed" }'

# draft → cancelled
curl -X PUT "https://localhost:5001/api/v1/Orders/1/status" \
  -H "Content-Type: application/json" \
  -d '{ "status": "cancelled" }'
```

**Luồng trạng thái hợp lệ:**
```
draft → confirmed → shipping → completed
draft → cancelled
```

### Xóa đơn hàng (chỉ đơn draft)
```bash
curl -X DELETE "https://localhost:5001/api/v1/Orders/1"
```

## Ghi chú

- Tất cả API dùng version `v1`.
- Tất cả danh sách hỗ trợ phân trang: `?page=1&pageSize=10` (mặc định page=1, pageSize=10).
- Response phân trang trả về: `{ "items": [...], "totalCount": N, "page": 1, "pageSize": 10 }`.
- Upload ảnh product dùng `multipart/form-data`, các API khác dùng `application/json`.
- Ảnh được resize và tối ưu trước khi upload Cloudinary.
- Chỉ đơn hàng ở trạng thái `draft` mới có thể xóa.
