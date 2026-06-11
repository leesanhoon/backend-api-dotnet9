# Partner API Design Spec

## Context

The business sells cups/lids to partner shops (cafes, tea shops, restaurants). To promote these partnerships and build trust with potential customers, we need a public-facing "Partners" section that showcases shops currently using our products — including their name, address, avatar image, and a gallery of photos.

This is a standalone, admin-managed feature with no authentication for end users. Partners are independent entities with no links to Products or Orders.

## Data Model

### Partner Entity

| Field | Type | Constraint |
|-------|------|-----------|
| Id | int | PK, auto-increment |
| Name | string | Required, MaxLength(200) |
| Address | string | Required, MaxLength(500) |
| PhoneNumber | string? | MaxLength(20) |
| Description | string? | MaxLength(2000) |
| AvatarImageUrl | string? | Cloudinary URL |
| CreatedAtUtc | DateTime | UTC timestamp |

- Table: `partners`
- Unique index on `Name`

### PartnerImage Entity

| Field | Type | Constraint |
|-------|------|-----------|
| Id | int | PK, auto-increment |
| PartnerId | int | FK → Partner, Cascade delete |
| ImageUrl | string | Required |
| ImageType | PartnerImageType | Enum: Avatar=0, Gallery=1 |
| DisplayOrder | int | Gallery ordering |
| CreatedAtUtc | DateTime | UTC timestamp |

- Table: `partner_images`
- `PartnerImageType` enum stored as string in DB

## API Endpoints

All endpoints under `api/v{version:apiVersion}/partners`, API version 1.0.

### GET /partners
- Query: `page`, `pageSize` (via `PaginationRequest`)
- Response: `PagedResult<PartnerResponse>` (200)

### GET /partners/{id}
- Response: `PartnerResponse` (200) or 404

### POST /partners
- Content-Type: `multipart/form-data`
- Fields: `Name` (required), `Address` (required), `PhoneNumber`, `Description`, `AvatarImage` (IFormFile), `GalleryImages` (List\<IFormFile\>)
- Response: `PartnerResponse` (201) via CreatedAtAction
- Errors: 400 (validation, image error)

### PUT /partners/{id}
- Content-Type: `multipart/form-data`
- Same fields as POST
- Avatar: new replaces old
- Gallery: new images replace entire gallery
- Response: `PartnerResponse` (200) or 404
- Errors: 400 (validation, image error)

### DELETE /partners/{id}
- Response: 204 or 404

## DTOs

### CreatePartnerRequest
```
Name: string (required)
Address: string (required)
PhoneNumber: string? (optional)
Description: string? (optional)
AvatarImage: IFormFile? (optional)
GalleryImages: List<IFormFile>? (optional)
```

### PartnerResponse
```
Id: int
Name: string
Address: string
PhoneNumber: string?
Description: string?
AvatarImageUrl: string?
GalleryImages: List<PartnerImageResponse>
CreatedAtUtc: DateTime
```

### PartnerImageResponse
```
Id: int
ImageUrl: string
DisplayOrder: int
```

### CreateOrUpdatePartnerResult
```
PartnerResponse?: PartnerResponse
PartnerNotFound: bool
ValidationError: string?
ImageError: string?
```

### DeletePartnerResult
```
NotFound: bool
Deleted: bool
```

## Image Handling

- Reuse existing `IImagePreparationService` and `ICloudinaryImageService`
- Cloudinary folder: `"partners"`
- Avatar: max 1200x1200 (ResizeMode.Max)
- Gallery: max 1000x1000
- Accepted formats: .jpg, .jpeg, .png, .webp, .gif
- On create: upload avatar + gallery images after entity creation; rollback (delete partner) if image upload fails
- On update: new avatar replaces old; new gallery replaces entire existing gallery (delete old PartnerImage records)

## Files to Create/Modify

### New files
- `Models/Partner.cs` — Partner entity
- `Models/PartnerImage.cs` — PartnerImage entity + PartnerImageType enum
- `Services/Interfaces/IPartnerService.cs` — interface + all DTOs
- `Services/PartnerService.cs` — business logic
- `Controllers/PartnersController.cs` — API endpoints

### Modified files
- `Data/AppDbContext.cs` — add DbSet<Partner>, DbSet<PartnerImage>, configure entities
- `Extensions/ServiceCollectionExtensions.cs` — register IPartnerService

### Migration
- Add EF Core migration for partners + partner_images tables

## Verification

1. Run `dotnet build` — no compilation errors
2. Run `dotnet ef migrations add AddPartners` — migration created successfully
3. Run `dotnet ef database update` — tables created in PostgreSQL
4. Test via Swagger UI:
   - POST /api/v1/partners with multipart form (name, address, avatar, gallery) → 201
   - GET /api/v1/partners → paged list with images
   - GET /api/v1/partners/{id} → single partner with gallery
   - PUT /api/v1/partners/{id} → updated partner, images replaced
   - DELETE /api/v1/partners/{id} → 204
5. Verify Cloudinary uploads in "partners" folder
