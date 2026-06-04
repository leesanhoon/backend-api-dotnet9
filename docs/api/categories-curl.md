# Categories API CURL Reference

Base URL placeholder: `{{baseUrl}}`

## `GET /api/v1/Categories`

```bash
curl -X GET "{{baseUrl}}/api/v1/Categories" \
  -H "Accept: application/json"
```

## `GET /api/v1/Categories/{id}`

```bash
curl -X GET "{{baseUrl}}/api/v1/Categories/{{id}}" \
  -H "Accept: application/json"
```

## `POST /api/v1/Categories`

```bash
curl -X POST "{{baseUrl}}/api/v1/Categories" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "name": "Ly nhua",
    "description": "Danh muc ly nhua"
  }'
```

Notes:
- `201 Created` returns the created category.

## `PUT /api/v1/Categories/{id}`

```bash
curl -X PUT "{{baseUrl}}/api/v1/Categories/{{id}}" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "name": "Ly nhua cap nhat",
    "description": "Mo ta cap nhat"
  }'
```

Notes:
- `404 Not Found` if category does not exist.

## `DELETE /api/v1/Categories/{id}`

```bash
curl -X DELETE "{{baseUrl}}/api/v1/Categories/{{id}}" \
  -H "Accept: application/json"
```

Notes:
- `404 Not Found` if category does not exist.
- `204 No Content` on success.
