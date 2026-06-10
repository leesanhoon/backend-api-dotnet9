const fs = require("fs");
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  Header, Footer, AlignmentType, LevelFormat,
  HeadingLevel, BorderStyle, WidthType, ShadingType, PageNumber, PageBreak
} = require("docx");

const PAGE_WIDTH = 12240;
const MARGIN = 1440;
const CONTENT_WIDTH = PAGE_WIDTH - MARGIN * 2; // 9360

const COLORS = {
  primary: "1B4F72",
  accent: "2E86C1",
  headerBg: "D6EAF8",
  methodGet: "27AE60",
  methodPost: "2980B9",
  methodPut: "E67E22",
  methodDelete: "C0392B",
  codeBg: "F4F6F7",
  borderLight: "BDC3C7",
  borderMedium: "95A5A6",
  text: "2C3E50",
  textLight: "7F8C8D",
};

const border = { style: BorderStyle.SINGLE, size: 1, color: COLORS.borderLight };
const borders = { top: border, bottom: border, left: border, right: border };
const cellMargins = { top: 60, bottom: 60, left: 100, right: 100 };

function heading1(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_1,
    spacing: { before: 360, after: 200 },
    children: [new TextRun({ text, bold: true, size: 32, font: "Arial", color: COLORS.primary })],
  });
}

function heading2(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_2,
    spacing: { before: 280, after: 160 },
    children: [new TextRun({ text, bold: true, size: 26, font: "Arial", color: COLORS.accent })],
  });
}

function heading3(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_3,
    spacing: { before: 200, after: 120 },
    children: [new TextRun({ text, bold: true, size: 22, font: "Arial", color: COLORS.text })],
  });
}

function para(text, opts = {}) {
  return new Paragraph({
    spacing: { after: 120 },
    children: [new TextRun({ text, size: 20, font: "Arial", color: COLORS.text, ...opts })],
  });
}

function codeBlock(text) {
  return new Paragraph({
    spacing: { before: 80, after: 80 },
    shading: { fill: COLORS.codeBg, type: ShadingType.CLEAR },
    indent: { left: 200, right: 200 },
    children: [new TextRun({ text, size: 18, font: "Consolas", color: COLORS.text })],
  });
}

function methodBadge(method) {
  const colorMap = { GET: COLORS.methodGet, POST: COLORS.methodPost, PUT: COLORS.methodPut, DELETE: COLORS.methodDelete };
  return new TextRun({ text: method, bold: true, size: 20, font: "Consolas", color: colorMap[method] || COLORS.text });
}

function endpointLine(method, path, desc) {
  return new Paragraph({
    spacing: { before: 160, after: 80 },
    children: [
      methodBadge(method),
      new TextRun({ text: `  ${path}`, size: 20, font: "Consolas", color: COLORS.text }),
      new TextRun({ text: `  — ${desc}`, size: 20, font: "Arial", color: COLORS.textLight }),
    ],
  });
}

function fieldRow(name, type, required, desc) {
  return new TableRow({
    children: [
      new TableCell({
        borders, width: { size: 2000, type: WidthType.DXA }, margins: cellMargins,
        children: [new Paragraph({ children: [new TextRun({ text: name, size: 18, font: "Consolas", color: COLORS.primary, bold: true })] })],
      }),
      new TableCell({
        borders, width: { size: 1800, type: WidthType.DXA }, margins: cellMargins,
        children: [new Paragraph({ children: [new TextRun({ text: type, size: 18, font: "Consolas", color: COLORS.textLight })] })],
      }),
      new TableCell({
        borders, width: { size: 1200, type: WidthType.DXA }, margins: cellMargins,
        verticalAlign: "center",
        children: [new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [new TextRun({ text: required ? "Yes" : "No", size: 18, font: "Arial", color: required ? COLORS.methodDelete : COLORS.textLight, bold: required })]
        })],
      }),
      new TableCell({
        borders, width: { size: 4360, type: WidthType.DXA }, margins: cellMargins,
        children: [new Paragraph({ children: [new TextRun({ text: desc, size: 18, font: "Arial", color: COLORS.text })] })],
      }),
    ],
  });
}

function fieldTableHeader() {
  const headerShading = { fill: COLORS.headerBg, type: ShadingType.CLEAR };
  const headerRun = (text, size) => new TextRun({ text, bold: true, size: 18, font: "Arial", color: COLORS.primary });
  return new TableRow({
    children: [
      new TableCell({ borders, width: { size: 2000, type: WidthType.DXA }, margins: cellMargins, shading: headerShading, children: [new Paragraph({ children: [headerRun("Field")] })] }),
      new TableCell({ borders, width: { size: 1800, type: WidthType.DXA }, margins: cellMargins, shading: headerShading, children: [new Paragraph({ children: [headerRun("Type")] })] }),
      new TableCell({ borders, width: { size: 1200, type: WidthType.DXA }, margins: cellMargins, shading: headerShading, children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [headerRun("Required")] })] }),
      new TableCell({ borders, width: { size: 4360, type: WidthType.DXA }, margins: cellMargins, shading: headerShading, children: [new Paragraph({ children: [headerRun("Description")] })] }),
    ],
  });
}

function fieldTable(fields) {
  return new Table({
    width: { size: CONTENT_WIDTH, type: WidthType.DXA },
    columnWidths: [2000, 1800, 1200, 4360],
    rows: [fieldTableHeader(), ...fields.map(f => fieldRow(f.name, f.type, f.required, f.desc))],
  });
}

function jsonBlock(title, json) {
  const lines = json.split("\n");
  return [
    para(title, { bold: true, italic: true, color: COLORS.textLight }),
    ...lines.map(line => codeBlock(line)),
  ];
}

function separator() {
  return new Paragraph({
    spacing: { before: 200, after: 200 },
    border: { bottom: { style: BorderStyle.SINGLE, size: 2, color: COLORS.borderLight, space: 1 } },
    children: [],
  });
}

// ─── BUILD DOCUMENT ───

const children = [];

// Title page
children.push(
  new Paragraph({ spacing: { before: 2400 } }),
  new Paragraph({
    alignment: AlignmentType.CENTER, spacing: { after: 200 },
    children: [new TextRun({ text: "API Documentation", bold: true, size: 52, font: "Arial", color: COLORS.primary })],
  }),
  new Paragraph({
    alignment: AlignmentType.CENTER, spacing: { after: 120 },
    children: [new TextRun({ text: "Product Management System", size: 32, font: "Arial", color: COLORS.accent })],
  }),
  new Paragraph({
    alignment: AlignmentType.CENTER, spacing: { after: 80 },
    children: [new TextRun({ text: "Backend API .NET 9 — Version 1.0", size: 22, font: "Arial", color: COLORS.textLight })],
  }),
  new Paragraph({
    alignment: AlignmentType.CENTER, spacing: { after: 600 },
    children: [new TextRun({ text: `Last updated: ${new Date().toISOString().split("T")[0]}`, size: 20, font: "Arial", color: COLORS.textLight })],
  }),
  separator(),
  new Paragraph({
    alignment: AlignmentType.CENTER, spacing: { after: 120 },
    children: [new TextRun({ text: "Base URL:  ", size: 20, font: "Arial", color: COLORS.textLight }), new TextRun({ text: "/api/v1", size: 22, font: "Consolas", bold: true, color: COLORS.primary })],
  }),
  new Paragraph({
    alignment: AlignmentType.CENTER,
    children: [new TextRun({ text: "Content-Type:  application/json  (unless noted otherwise)", size: 20, font: "Arial", color: COLORS.textLight })],
  }),
  new Paragraph({ children: [new PageBreak()] }),
);

// ═══════════════════════════════════════════════
// BREAKING CHANGES
// ═══════════════════════════════════════════════
children.push(
  heading1("⚠  Breaking Changes"),
  para("This version introduces significant schema changes. The following endpoints and data structures have been removed or restructured:"),
);

const breakingChanges = [
  ["Removed", "Orders module", "DELETE /orders, GET /orders, POST /orders — removed entirely. Order system will be redesigned in a future version."],
  ["Removed", "Product.price", "Single price field removed. Use variants[].priceTiers[] instead."],
  ["Removed", "Product.stockQuantity", "Stock tracking removed from product level."],
  ["Changed", "Category", "Added parentId for tree hierarchy. Added isRoot flag. Name uniqueness is now scoped to (name + parentId)."],
  ["New", "Lid module", "Full CRUD for lid management with price-by-diameter."],
  ["New", "Product variants", "Each product now has variants (capacity + diameter) with tiered pricing."],
  ["New", "Product-Lid linking", "Products can be linked to compatible lids via lidIds."],
];

children.push(new Table({
  width: { size: CONTENT_WIDTH, type: WidthType.DXA },
  columnWidths: [1200, 2200, 5960],
  rows: [
    new TableRow({
      children: [
        new TableCell({ borders, width: { size: 1200, type: WidthType.DXA }, margins: cellMargins, shading: { fill: COLORS.headerBg, type: ShadingType.CLEAR }, children: [new Paragraph({ children: [new TextRun({ text: "Type", bold: true, size: 18, font: "Arial", color: COLORS.primary })] })] }),
        new TableCell({ borders, width: { size: 2200, type: WidthType.DXA }, margins: cellMargins, shading: { fill: COLORS.headerBg, type: ShadingType.CLEAR }, children: [new Paragraph({ children: [new TextRun({ text: "Target", bold: true, size: 18, font: "Arial", color: COLORS.primary })] })] }),
        new TableCell({ borders, width: { size: 5960, type: WidthType.DXA }, margins: cellMargins, shading: { fill: COLORS.headerBg, type: ShadingType.CLEAR }, children: [new Paragraph({ children: [new TextRun({ text: "Details", bold: true, size: 18, font: "Arial", color: COLORS.primary })] })] }),
      ],
    }),
    ...breakingChanges.map(([type, target, details]) => {
      const typeColor = type === "Removed" ? COLORS.methodDelete : type === "Changed" ? COLORS.methodPut : COLORS.methodGet;
      return new TableRow({
        children: [
          new TableCell({ borders, width: { size: 1200, type: WidthType.DXA }, margins: cellMargins, children: [new Paragraph({ children: [new TextRun({ text: type, bold: true, size: 18, font: "Arial", color: typeColor })] })] }),
          new TableCell({ borders, width: { size: 2200, type: WidthType.DXA }, margins: cellMargins, children: [new Paragraph({ children: [new TextRun({ text: target, size: 18, font: "Consolas", color: COLORS.text })] })] }),
          new TableCell({ borders, width: { size: 5960, type: WidthType.DXA }, margins: cellMargins, children: [new Paragraph({ children: [new TextRun({ text: details, size: 18, font: "Arial", color: COLORS.text })] })] }),
        ],
      });
    }),
  ],
}));

children.push(new Paragraph({ children: [new PageBreak()] }));

// ═══════════════════════════════════════════════
// MODULE 1: CATEGORIES
// ═══════════════════════════════════════════════
children.push(
  heading1("Module 1 — Categories"),
  para("Hierarchical category tree with two fixed root categories: \"Ly giấy\" (Paper cups) and \"Ly nhựa\" (Plastic cups). Root categories cannot be modified or deleted."),
  separator(),

  // GET /categories
  heading2("List Categories (Paginated)"),
  endpointLine("GET", "/api/v1/categories", "Flat list with pagination"),
  heading3("Query Parameters"),
  fieldTable([
    { name: "page", type: "int", required: false, desc: "Page number (default: 1, min: 1)" },
    { name: "pageSize", type: "int", required: false, desc: "Items per page (default: 10, max: 100)" },
  ]),
  ...jsonBlock("Response 200:", JSON.stringify({
    items: [{ id: 3, name: "Ly giấy 1 lớp", description: "...", parentId: 1, isRoot: false }],
    totalCount: 6, page: 1, pageSize: 10
  }, null, 2)),
  separator(),

  // GET /categories/tree
  heading2("Get Category Tree"),
  endpointLine("GET", "/api/v1/categories/tree", "Full tree structure (recursive children)"),
  ...jsonBlock("Response 200:", JSON.stringify([
    {
      id: 1, name: "Ly giấy", description: "Các loại ly giấy", parentId: null, isRoot: true,
      children: [
        { id: 3, name: "Ly giấy 1 lớp", description: "...", parentId: 1, isRoot: false, children: [] },
        { id: 4, name: "Ly giấy 2 lớp", description: "...", parentId: 1, isRoot: false, children: [] }
      ]
    },
    {
      id: 2, name: "Ly nhựa", description: "Các loại ly nhựa", parentId: null, isRoot: true,
      children: [
        { id: 5, name: "Ly PET", description: "...", parentId: 2, isRoot: false, children: [] }
      ]
    }
  ], null, 2)),
  separator(),

  // GET /categories/:id
  heading2("Get Category by ID"),
  endpointLine("GET", "/api/v1/categories/{id}", "Single category detail"),
  para("Returns 404 if not found."),
  separator(),

  // POST /categories
  heading2("Create Category"),
  endpointLine("POST", "/api/v1/categories", "Create a new sub-category"),
  heading3("Request Body"),
  fieldTable([
    { name: "name", type: "string", required: true, desc: "Category name (max 150 chars)" },
    { name: "description", type: "string?", required: false, desc: "Description (max 500 chars)" },
    { name: "parentId", type: "int?", required: false, desc: "Parent category ID. null = root (not allowed for user-created)" },
  ]),
  ...jsonBlock("Request:", JSON.stringify({ name: "Ly giấy kraft", description: "Ly giấy kraft thân thiện môi trường", parentId: 1 }, null, 2)),
  para("Returns 201 with created category. Returns 400 if parentId does not exist."),
  separator(),

  // PUT /categories/:id
  heading2("Update Category"),
  endpointLine("PUT", "/api/v1/categories/{id}", "Update an existing category"),
  heading3("Request Body"),
  para("Same as POST. Additional error cases:"),
  ...jsonBlock("Error responses:", [
    '400 "Không thể sửa danh mục gốc."  → root categories are protected',
    '400 "Không thể chọn danh mục con làm cha"  → cycle detection',
    '400 "ParentId không tồn tại."  → invalid parent',
  ].join("\n")),
  separator(),

  // DELETE /categories/:id
  heading2("Delete Category"),
  endpointLine("DELETE", "/api/v1/categories/{id}", "Delete a category"),
  para("Returns 204 on success. Deletion is blocked (400) if:"),
  ...jsonBlock("Error conditions:", [
    "• Category is a root category (isRoot = true)",
    "• Category has child categories",
    "• Category has linked products",
    "• Category has linked lids",
  ].join("\n")),
);

children.push(new Paragraph({ children: [new PageBreak()] }));

// ═══════════════════════════════════════════════
// MODULE 2: LIDS
// ═══════════════════════════════════════════════
children.push(
  heading1("Module 2 — Lids (Nắp ly)"),
  para("Each lid has a price table by mouth diameter (mm). A lid can be linked to multiple products that share compatible diameters."),
  separator(),

  // GET /lids
  heading2("List Lids (Paginated)"),
  endpointLine("GET", "/api/v1/lids", "List all lids with prices"),
  heading3("Query Parameters"),
  fieldTable([
    { name: "page", type: "int", required: false, desc: "Page number (default: 1)" },
    { name: "pageSize", type: "int", required: false, desc: "Items per page (default: 10, max: 100)" },
  ]),
  ...jsonBlock("Response 200:", JSON.stringify({
    items: [{
      id: 1, name: "Nắp vòm trong suốt", description: "Nắp vòm dùng cho đồ uống có topping",
      categoryId: 2, categoryName: "Ly nhựa",
      prices: [
        { id: 1, diameterMm: 90, sizeName: "Size S (90mm)", unitPrice: 350.00 },
        { id: 2, diameterMm: 95, sizeName: "Size M (95mm)", unitPrice: 380.00 },
        { id: 3, diameterMm: 106, sizeName: "Size L (106mm)", unitPrice: 420.00 }
      ]
    }],
    totalCount: 2, page: 1, pageSize: 10
  }, null, 2)),
  separator(),

  // GET /lids/:id
  heading2("Get Lid by ID"),
  endpointLine("GET", "/api/v1/lids/{id}", "Single lid with prices"),
  para("Returns 404 if not found."),
  separator(),

  // POST /lids
  heading2("Create Lid"),
  endpointLine("POST", "/api/v1/lids", "Create a new lid with prices"),
  heading3("Request Body"),
  fieldTable([
    { name: "name", type: "string", required: true, desc: "Lid name (max 200 chars)" },
    { name: "description", type: "string?", required: false, desc: "Description (max 500 chars)" },
    { name: "categoryId", type: "int", required: true, desc: "Category ID" },
    { name: "prices", type: "LidPriceItem[]", required: true, desc: "At least 1 price entry" },
  ]),
  heading3("LidPriceItem"),
  fieldTable([
    { name: "diameterMm", type: "int", required: true, desc: "Mouth diameter in mm (positive integer)" },
    { name: "sizeName", type: "string?", required: false, desc: 'Display name, e.g. "Size S (90mm)"' },
    { name: "unitPrice", type: "decimal", required: true, desc: "Unit price in VND (positive)" },
  ]),
  ...jsonBlock("Request:", JSON.stringify({
    name: "Nắp vòm trong suốt",
    description: "Nắp vòm dùng cho đồ uống có topping",
    categoryId: 2,
    prices: [
      { diameterMm: 90, sizeName: "Size S (90mm)", unitPrice: 350 },
      { diameterMm: 95, sizeName: "Size M (95mm)", unitPrice: 380 },
      { diameterMm: 106, sizeName: "Size L (106mm)", unitPrice: 420 }
    ]
  }, null, 2)),
  para("Returns 201. Validation: duplicate diameterMm within same lid returns 400."),
  separator(),

  // PUT /lids/:id
  heading2("Update Lid"),
  endpointLine("PUT", "/api/v1/lids/{id}", "Replace lid data and all prices"),
  para("Same request body as POST. All existing prices are replaced (full replace, not merge)."),
  separator(),

  // DELETE /lids/:id
  heading2("Delete Lid"),
  endpointLine("DELETE", "/api/v1/lids/{id}", "Delete a lid"),
  para("Returns 204 on success. Returns 400 if lid is linked to any product."),
);

children.push(new Paragraph({ children: [new PageBreak()] }));

// ═══════════════════════════════════════════════
// MODULE 3: PRODUCTS
// ═══════════════════════════════════════════════
children.push(
  heading1("Module 3 — Products (Sản phẩm)"),
  para("Products are cups (paper or plastic) with multiple capacity variants. Each variant has tiered pricing by order quantity. Products can be linked to compatible lids."),
  separator(),

  // GET /products
  heading2("List Products (Paginated)"),
  endpointLine("GET", "/api/v1/products", "List products with full details"),
  heading3("Query Parameters"),
  fieldTable([
    { name: "page", type: "int", required: false, desc: "Page number (default: 1)" },
    { name: "pageSize", type: "int", required: false, desc: "Items per page (default: 10, max: 100)" },
  ]),
  separator(),

  // GET /products/:id
  heading2("Get Product by ID"),
  endpointLine("GET", "/api/v1/products/{id}", "Full product detail with variants, prices, lids"),
  ...jsonBlock("Response 200:", JSON.stringify({
    id: 1,
    name: "Ly PET trong suốt",
    description: "Ly nhựa PET trong suốt cho đồ uống lạnh",
    categoryId: 5,
    categoryName: "Ly PET",
    avatarImageUrl: null,
    galleryImages: [],
    variants: [
      {
        id: 1, capacityMl: 250, diameterMm: 90,
        priceTiers: [
          { id: 1, minQuantity: 1000, unitPrice: 850.00 },
          { id: 2, minQuantity: 5000, unitPrice: 780.00 },
          { id: 3, minQuantity: 10000, unitPrice: 720.00 }
        ]
      },
      {
        id: 2, capacityMl: 350, diameterMm: 95,
        priceTiers: [
          { id: 4, minQuantity: 1000, unitPrice: 950.00 },
          { id: 5, minQuantity: 5000, unitPrice: 880.00 }
        ]
      }
    ],
    lids: [
      { id: 1, lidId: 1, lidName: "Nắp vòm trong suốt" },
      { id: 2, lidId: 2, lidName: "Nắp phẳng PP" }
    ]
  }, null, 2)),
  separator(),

  // POST /products
  heading2("Create Product"),
  endpointLine("POST", "/api/v1/products", "Create product with variants + lids"),
  para("Content-Type: multipart/form-data (supports image upload)", { bold: true, color: COLORS.methodPut }),
  heading3("Request Fields"),
  fieldTable([
    { name: "name", type: "string", required: true, desc: "Product name (max 200 chars)" },
    { name: "description", type: "string?", required: false, desc: "Description (max 1000 chars)" },
    { name: "categoryId", type: "int", required: true, desc: "Category ID (must exist)" },
    { name: "avatarImage", type: "file", required: false, desc: "Avatar image file" },
    { name: "galleryImages", type: "file[]", required: false, desc: "Gallery image files" },
    { name: "variants", type: "Variant[]", required: true, desc: "At least 1 variant" },
    { name: "lidIds", type: "int[]", required: false, desc: "List of lid IDs to link" },
  ]),
  heading3("Variant (ProductVariantItem)"),
  fieldTable([
    { name: "capacityMl", type: "int", required: true, desc: "Cup capacity in ml (positive)" },
    { name: "diameterMm", type: "int", required: true, desc: "Mouth diameter in mm (positive)" },
    { name: "priceTiers", type: "PriceTier[]", required: true, desc: "At least 1 price tier" },
  ]),
  heading3("PriceTier (PriceTierItem)"),
  fieldTable([
    { name: "minQuantity", type: "int", required: true, desc: "Minimum order quantity threshold" },
    { name: "unitPrice", type: "decimal", required: true, desc: "Unit price in VND for this tier" },
  ]),
  ...jsonBlock("Variants + LidIds example (JSON part of multipart):", JSON.stringify({
    name: "Ly PET trong suốt",
    description: "Ly nhựa PET",
    categoryId: 5,
    variants: [
      {
        capacityMl: 250, diameterMm: 90,
        priceTiers: [
          { minQuantity: 1000, unitPrice: 850 },
          { minQuantity: 5000, unitPrice: 780 },
          { minQuantity: 10000, unitPrice: 720 }
        ]
      },
      {
        capacityMl: 350, diameterMm: 95,
        priceTiers: [
          { minQuantity: 1000, unitPrice: 950 },
          { minQuantity: 5000, unitPrice: 880 }
        ]
      }
    ],
    lidIds: [1, 2]
  }, null, 2)),
  para("Returns 201. Validation errors (400):"),
  ...jsonBlock("Validation rules:", [
    "• At least 1 variant required",
    "• No duplicate capacityMl within same product",
    "• Each variant must have at least 1 price tier",
    "• No duplicate minQuantity within same variant",
    "• categoryId must exist",
  ].join("\n")),
  separator(),

  // PUT /products/:id
  heading2("Update Product"),
  endpointLine("PUT", "/api/v1/products/{id}", "Update product (JSON, no images)"),
  para("Content-Type: application/json", { bold: true }),
  heading3("Request Body"),
  fieldTable([
    { name: "name", type: "string", required: true, desc: "Product name" },
    { name: "description", type: "string?", required: false, desc: "Description" },
    { name: "categoryId", type: "int", required: true, desc: "Category ID" },
    { name: "variants", type: "Variant[]", required: true, desc: "Full replace of all variants" },
    { name: "lidIds", type: "int[]", required: false, desc: "Full replace of lid links" },
  ]),
  para("All variants and lid links are fully replaced (not merged)."),
  separator(),

  // DELETE /products/:id
  heading2("Delete Product"),
  endpointLine("DELETE", "/api/v1/products/{id}", "Delete product and all related data"),
  para("Returns 204. Cascade deletes: variants, price tiers, product-lid links, images."),
  separator(),

  // GET /products/:id/compatible-lids
  heading2("Get Compatible Lids"),
  endpointLine("GET", "/api/v1/products/{id}/compatible-lids", "Lids matching product diameters"),
  para("Returns all lids that have at least one price entry with a diameterMm matching any of the product's variant diameters. Use this to populate the lid selector when editing a product."),
  ...jsonBlock("Response 200:", JSON.stringify([
    {
      id: 1, name: "Nắp vòm trong suốt", description: "...",
      categoryId: 2, categoryName: "Ly nhựa",
      prices: [
        { id: 1, diameterMm: 90, sizeName: "Size S (90mm)", unitPrice: 350.00 },
        { id: 2, diameterMm: 95, sizeName: "Size M (95mm)", unitPrice: 380.00 }
      ]
    }
  ], null, 2)),
);

children.push(new Paragraph({ children: [new PageBreak()] }));

// ═══════════════════════════════════════════════
// COMMON RESPONSE SHAPES
// ═══════════════════════════════════════════════
children.push(
  heading1("Common Response Shapes"),

  heading2("Paginated Response"),
  ...jsonBlock("PagedResult<T>:", JSON.stringify({
    items: ["T[]"],
    totalCount: 0,
    page: 1,
    pageSize: 10
  }, null, 2)),

  heading2("Error Response"),
  para("All validation errors return HTTP 400 with a plain text message body (not JSON)."),
  ...jsonBlock("Example:", '400 Bad Request\n"Không thể xoá danh mục đang có sản phẩm."'),

  heading2("HTTP Status Codes"),
  fieldTable([
    { name: "200", type: "OK", required: false, desc: "Successful GET or PUT" },
    { name: "201", type: "Created", required: false, desc: "Successful POST (includes Location header)" },
    { name: "204", type: "No Content", required: false, desc: "Successful DELETE" },
    { name: "400", type: "Bad Request", required: false, desc: "Validation error or business rule violation" },
    { name: "404", type: "Not Found", required: false, desc: "Resource does not exist" },
  ]),
);

// ─── ASSEMBLE DOCUMENT ───

const doc = new Document({
  styles: {
    default: { document: { run: { font: "Arial", size: 20 } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 32, bold: true, font: "Arial", color: COLORS.primary },
        paragraph: { spacing: { before: 360, after: 200 }, outlineLevel: 0 } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 26, bold: true, font: "Arial", color: COLORS.accent },
        paragraph: { spacing: { before: 280, after: 160 }, outlineLevel: 1 } },
      { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 22, bold: true, font: "Arial", color: COLORS.text },
        paragraph: { spacing: { before: 200, after: 120 }, outlineLevel: 2 } },
    ]
  },
  sections: [{
    properties: {
      page: {
        size: { width: 12240, height: 15840 },
        margin: { top: MARGIN, right: MARGIN, bottom: MARGIN, left: MARGIN },
      },
    },
    headers: {
      default: new Header({
        children: [new Paragraph({
          alignment: AlignmentType.RIGHT,
          border: { bottom: { style: BorderStyle.SINGLE, size: 1, color: COLORS.borderLight, space: 4 } },
          children: [new TextRun({ text: "API Documentation — Product Management System v1.0", size: 16, font: "Arial", color: COLORS.textLight })],
        })],
      }),
    },
    footers: {
      default: new Footer({
        children: [new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [
            new TextRun({ text: "Page ", size: 16, font: "Arial", color: COLORS.textLight }),
            new TextRun({ children: [PageNumber.CURRENT], size: 16, font: "Arial", color: COLORS.textLight }),
          ],
        })],
      }),
    },
    children,
  }],
});

Packer.toBuffer(doc).then(buffer => {
  const outPath = "D:\\MyProject\\backend-api-dotnet9\\docs\\API_Documentation_v1.0.docx";
  fs.writeFileSync(outPath, buffer);
  console.log("Created: " + outPath);
});
