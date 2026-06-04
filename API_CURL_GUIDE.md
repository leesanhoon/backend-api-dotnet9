# Hướng dẫn tích hợp API bằng cURL

Base URL ví dụ:

```bash
https://localhost:5001/api/v1
```

Nếu chạy local, thay bằng port thực tế của bạn.

## 1) Categories

### Lấy danh sách
```bash
curl -X GET "https://localhost:5001/api/v1/Categories" \
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

## 2) Materials

### Lấy danh sách
```bash
curl -X GET "https://localhost:5001/api/v1/Materials" \
  -H "Accept: application/json"
```

### Tạo mới
```bash
curl -X POST "https://localhost:5001/api/v1/Materials" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Cotton",
    "description": "Vải cotton mềm"
  }'
```

## 3) Print types

### Lấy danh sách
```bash
curl -X GET "https://localhost:5001/api/v1/PrintTypes" \
  -H "Accept: application/json"
```

### Tạo mới
```bash
curl -X POST "https://localhost:5001/api/v1/PrintTypes" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "In lụa",
    "colorCount": 4,
    "description": "Kỹ thuật in lụa 4 màu"
  }'
```

## 4) Products

### Lấy danh sách
```bash
curl -X GET "https://localhost:5001/api/v1/Products" \
  -H "Accept: application/json"
```

### Lấy chi tiết
```bash
curl -X GET "https://localhost:5001/api/v1/Products/1" \
  -H "Accept: application/json"
```

### Tạo mới product kèm avatar + gallery
```bash
curl -X POST "https://localhost:5001/api/v1/Products" \
  -H "Content-Type: multipart/form-data" \
  -F "Name=Áo thun basic" \
  -F "Description=Áo thun màu trắng" \
  -F "Price=199000" \
  -F "StockQuantity=100" \
  -F "CategoryId=1" \
  -F "AvatarImage=@C:/images/avatar.jpg" \
  -F "GalleryImages=@C:/images/gallery-1.jpg" \
  -F "GalleryImages=@C:/images/gallery-2.jpg"
```

### Cập nhật product
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

### Xóa product
```bash
curl -X DELETE "https://localhost:5001/api/v1/Products/1"
```

## 5) Product configurations

### Lấy cấu hình product
```bash
curl -X GET "https://localhost:5001/api/v1/products/1/configurations" \
  -H "Accept: application/json"
```

### Thêm material cho product
```bash
curl -X POST "https://localhost:5001/api/v1/products/1/configurations/materials" \
  -H "Content-Type: application/json" \
  -d '{
    "materialId": 1,
    "extraPrice": 15000
  }'
```

### Thêm print option cho product
```bash
curl -X POST "https://localhost:5001/api/v1/products/1/configurations/print-options" \
  -H "Content-Type: application/json" \
  -d '{
    "printTypeId": 1,
    "extraPrice": 20000
  }'
```

## 6) Orders

### Lấy danh sách đơn hàng
```bash
curl -X GET "https://localhost:5001/api/v1/Orders" \
  -H "Accept: application/json"
```

### Lấy chi tiết đơn hàng
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
    "items": []
  }'
```

## Ghi chú cho client

- Tất cả API dùng version `v1`.
- Với upload ảnh product, dùng `multipart/form-data` thay vì JSON.
- Các field `GalleryImages` có thể lặp nhiều lần trong cùng một request.
- Ảnh sẽ được resize và tối ưu trước khi upload Cloudinary.
