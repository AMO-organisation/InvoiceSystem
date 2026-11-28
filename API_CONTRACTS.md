# API Contracts & Documentation — Group 2 Invoice Generation System

Welcome frontend teammate! This document outlines all REST API endpoints, query parameters, request payloads, response payloads, and error codes for the ASP.NET Core Web API backend.

Base URL: `https://localhost:5001` (or configured local URL)  
Swagger UI: `/swagger` (opens automatically when running the API)

---

## Enums Reference

- **`CatalogItemType`**: `"Product"` (1), `"Service"` (2)
- **`InvoiceStatus`**: `"Draft"` (0), `"Unpaid"` (1), `"Paid"` (2), `"Overdue"` (3), `"Cancelled"` (4)

---

## 1. Customers (`/api/customers`)

### `POST /api/customers`
- **Description**: Register a new customer.
- **Request Body (`CreateCustomerDto`)**:
  ```json
  {
    "fullName": "Acme Corp",
    "companyName": "Acme Corporation",
    "email": "billing@acme.com",
    "phone": "+1-555-0100",
    "address": "123 Industrial Pkwy",
    "city": "Chicago"
  }
  ```
- **Responses**:
  - `201 Created`: Returns created `CustomerDto`.
  - `400 BadRequest`: Validation error.
  - `409 Conflict`: Email already exists.

### `GET /api/customers?search={term}`
- **Description**: Retrieve active customers (filters by name, company, email, or phone).
- **Query Parameters**:
  - `search` (string, optional)
- **Response (`CustomerDto[]`)**:
  ```json
  [
    {
      "id": 1,
      "fullName": "Acme Corp",
      "companyName": "Acme Corporation",
      "email": "billing@acme.com",
      "phone": "+1-555-0100",
      "address": "123 Industrial Pkwy",
      "city": "Chicago",
      "isDeleted": false,
      "createdAt": "2026-11-28T00:00:00Z"
    }
  ]
  ```

### `GET /api/customers/{id}`
- **Description**: Get customer by ID.
- **Responses**:
  - `200 OK`: Returns `CustomerDto`.
  - `404 Not Found`: Customer not found or deleted.

### `PUT /api/customers/{id}`
- **Description**: Update customer details.
- **Request Body (`UpdateCustomerDto`)**: Same as `CreateCustomerDto`.
- **Responses**:
  - `200 OK`: Returns updated `CustomerDto`.
  - `404 Not Found`: Customer not found.
  - `409 Conflict`: Email conflict.

### `DELETE /api/customers/{id}`
- **Description**: Soft-deletes customer (`IsDeleted = true`).
- **Responses**:
  - `204 No Content`: Successful deletion.
  - `404 Not Found`: Customer not found.
  - `409 Conflict`: Customer has associated invoices.

---

## 2. Products & Services (`/api/products`)

### `POST /api/products`
- **Description**: Add a new product or service.
- **Request Body (`CreateProductDto`)**:
  ```json
  {
    "name": "Web Development",
    "sku": "SRV-WEB",
    "description": "Custom full-stack web application development",
    "type": "Service",
    "unitPrice": 1200.00,
    "isTaxable": true
  }
  ```
- **Responses**:
  - `201 Created`: Returns `ProductDto`.
  - `400 BadRequest`: Validation error.

### `GET /api/products?search={term}&type={Product|Service}`
- **Description**: Retrieve active products/services.
- **Query Parameters**:
  - `search` (string, optional)
  - `type` (`Product` or `Service`, optional)
- **Response (`ProductDto[]`)**: Array of products.

### `GET /api/products/{id}`
- **Description**: Get product by ID.
- **Responses**: `200 OK`, `404 Not Found`.

### `PUT /api/products/{id}`
- **Description**: Update product/service.
- **Request Body (`UpdateProductDto`)**:
  ```json
  {
    "name": "Web Development",
    "sku": "SRV-WEB",
    "description": "Custom full-stack web application development",
    "type": "Service",
    "unitPrice": 1250.00,
    "isTaxable": true,
    "isActive": true
  }
  ```
- **Responses**: `200 OK`, `404 Not Found`.

### `DELETE /api/products/{id}`
- **Description**: Soft-deactivates product (`IsActive = false`).
- **Responses**: `204 No Content`, `404 Not Found`.

---

## 3. Invoices (`/api/invoices`)

### `POST /api/invoices/calculate-preview`
- **Description**: Calculate totals preview without saving to DB.
- **Request Body (`CalculatePreviewDto`)**:
  ```json
  {
    "discountAmount": 20.00,
    "taxRate": 7.50,
    "items": [
      {
        "productId": 1,
        "quantity": 2,
        "unitPrice": 150.00,
        "isTaxable": true
      }
    ]
  }
  ```
- **Response (`InvoicePreviewResultDto`)**: Calculated subtotal, discount, tax, grand total, and items with line totals.

### `POST /api/invoices`
- **Description**: Generate invoice number (`INV-{YYYY}-{0000}`), calculate totals, and save invoice + items.
- **Request Body (`CreateInvoiceDto`)**:
  ```json
  {
    "customerId": 1,
    "issueDate": "2026-11-28T00:00:00Z",
    "dueDate": "2026-12-28T00:00:00Z",
    "discountAmount": 20.00,
    "taxRate": 7.50,
    "notes": "Payment due within 30 days.",
    "items": [
      {
        "productId": 1,
        "quantity": 2,
        "unitPrice": 150.00,
        "isTaxable": true
      }
    ]
  }
  ```
- **Responses**:
  - `201 Created`: Returns `InvoiceDetailDto` with full customer and items.
  - `400 BadRequest`: Validation error (e.g. DueDate < IssueDate, empty items, negative prices).
  - `404 Not Found`: Customer or Product not found.

### `GET /api/invoices?status={status}&search={term}&customerId={id}`
- **Description**: Get invoices list (dynamically reports Unpaid invoices past DueDate as Overdue).
- **Query Parameters**:
  - `status` (`Draft|Unpaid|Paid|Overdue|Cancelled`, optional)
  - `search` (string, optional - searches invoice number, customer name/company/email)
  - `customerId` (int, optional)
- **Response (`InvoiceDto[]`)**: List of invoice summaries.

### `GET /api/invoices/{id}`
- **Description**: Get full invoice details including customer and line items.
- **Responses**: `200 OK`, `404 Not Found`.

### `PUT /api/invoices/{id}`
- **Description**: Update invoice and replace line items (allowed only when `Draft` or `Unpaid`).
- **Request Body (`UpdateInvoiceDto`)**: Same structure as `CreateInvoiceDto`.
- **Responses**:
  - `200 OK`: Returns updated `InvoiceDetailDto`.
  - `409 Conflict`: If invoice is already `Paid` or `Cancelled`.
  - `404 Not Found`.

### `PATCH /api/invoices/{id}/status`
- **Description**: Update invoice status (sets `PaidAt = DateTime.UtcNow` when marked `Paid`).
- **Request Body (`UpdateInvoiceStatusDto`)**:
  ```json
  {
    "status": "Paid"
  }
  ```
- **Responses**: `200 OK`, `404 Not Found`.

### `GET /api/invoices/{id}/pdf`
- **Description**: Download clean Zoho-style A4 portrait PDF invoice.
- **Response**: `application/pdf` file download with filename `{InvoiceNumber}.pdf`.

---

## 4. Company Profile / Business Settings (`/api/company-profile`)

### `GET /api/company-profile`
- **Description**: Returns the single company profile record used for invoice branding and contact details. If no profile exists, a default starter profile is inserted automatically, so `GET` always succeeds.
- **Response (`CompanyProfileDto`)**:
  ```json
  {
    "id": 1,
    "companyName": "My Business Name",
    "address": "123 Main Street",
    "city": "Lagos",
    "email": "info@mybusiness.com",
    "phone": "+234 000 000 0000",
    "currencySymbol": "₦",
    "defaultTaxRate": 7.50,
    "paymentInstructions": "Bank Transfer: Zenith Bank - 1234567890",
    "updatedAt": "2026-11-28T00:00:00Z"
  }
  ```
- **Responses**: `200 OK`.

### `PUT /api/company-profile`
- **Description**: Creates or updates the single company profile record. Drives invoice PDF headers/details and the default tax rate applied when creating invoices (when `taxRate` is not specified).
- **Request Body (`UpdateCompanyProfileRequest`)**:
  ```json
  {
    "companyName": "Acme Invoicing Ltd",
    "address": "456 Commerce Way",
    "city": "Lagos",
    "email": "billing@acme.com",
    "phone": "+234 800 000 0000",
    "currencySymbol": "₦",
    "defaultTaxRate": 7.50,
    "paymentInstructions": "Pay via bank transfer"
  }
  ```
- **Responses**:
  - `200 OK`: Returns the updated `CompanyProfileDto`.
  - `400 BadRequest`: Validation error (`companyName`, `address`, `email`, `phone`, `currencySymbol` required; valid email format; `0 <= defaultTaxRate <= 100`; max lengths enforced).

---

## 5. Dashboard (`/api/dashboard`)

### `GET /api/dashboard/summary`
- **Description**: Get high-level business metrics and recent invoices for dashboard.
- **Response (`DashboardSummaryDto`)**:
  ```json
  {
    "totalInvoices": 12,
    "paidCount": 5,
    "unpaidCount": 5,
    "overdueCount": 2,
    "totalRevenue": 4500.00,
    "totalReceivables": 3200.00,
    "recentInvoices": [...]
  }
  ```

---

## Error Handling (RFC 7807 ProblemDetails)

All errors return standard RFC 7807 JSON responses:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Validation Error / Not Found / Conflict",
  "status": 400,
  "detail": "Error description message.",
  "instance": "/api/invoices"
}
```
- `400 Bad Request`: Validation failure.
- `404 Not Found`: Resource ID does not exist.
- `409 Conflict`: Business rule violation (duplicate email, deleting customer with invoices, editing Paid invoice).
- `500 Internal Server Error`: Unexpected server error.
