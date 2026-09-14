# EShop Modular Monoliths — Bài tập thực hành 1

> Dự án minh họa kiến trúc **Modular Monolith** kết hợp **DDD**, **CQRS**, **Vertical Slice Architecture (VSA)** trên nền .NET, xây dựng theo khóa học *"Building Microservices with .NET"* (Mehmet Ozkaya).

## 1. Mục tiêu bài tập

Theo yêu cầu đề bài, bài tập thực hành 1 gồm 2 phần:

1. ✅ **Project chạy được** — API kết nối PostgreSQL qua Docker, tự động migrate + seed dữ liệu khi khởi động.
2. ✅ **Bổ sung tính năng** được đề cập ở cuối tài liệu đặc tả dự án — feature **`GetProductByName`** cho module Catalog.

## 2. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Framework | .NET 10 |
| Kiến trúc | Modular Monolith, Vertical Slice Architecture, DDD, CQRS |
| Web API | ASP.NET Core Minimal API + [Carter](https://github.com/CarterCommunity/Carter) |
| CQRS Mediator | [MediatR](https://github.com/jbogard/MediatR) |
| Validation | [FluentValidation](https://fluentvalidation.net/) |
| Mapping | [Mapster](https://github.com/MapsterMapper/Mapster) |
| ORM | Entity Framework Core 8/10 (Code-First) |
| Database | PostgreSQL (chạy qua Docker) |
| Containerization | Docker / Docker Compose |

## 3. Cấu trúc dự án

```
src/eshop-modular-monoliths/
├── Bootstrapper/
│   └── Api/                        # Entry point, Program.cs, appsettings
├── Modules/
│   ├── Catalog/
│   │   ├── Catalog/                # Domain + Application + Infrastructure (internal)
│   │   │   ├── data/                # DbContext, Configurations, Migrations, Seed
│   │   │   └── Products/
│   │   │       ├── Models/          # Product (Aggregate Root)
│   │   │       ├── Events/          # Domain Events
│   │   │       └── Features/        # CQRS Commands/Queries theo VSA
│   │   │           ├── CreateProduct/
│   │   │           ├── GetProducts/
│   │   │           ├── GetProductById/
│   │   │           ├── GetProductByCategory/
│   │   │           ├── GetProductByName/   ⭐ Tính năng bổ sung
│   │   │           ├── UpdateProduct/
│   │   │           └── DeleteProduct/
│   │   └── Catalog.Contracts/       # Public API của module (Dto, Query dùng chung)
│   ├── Basket/
│   └── Ordering/
├── Shared/
│   ├── Shared/                      # DDD base, Behaviors, Interceptors, Exceptions
│   └── Shared.Contracts/            # CQRS interfaces (ICommand, IQuery...)
└── docker-compose.yaml
```

## 4. Yêu cầu môi trường

- .NET 10 SDK
- Docker Desktop
- IDE: Visual Studio 2022 hoặc JetBrains Rider
- Postman (hoặc công cụ test API tương đương)

## 5. Hướng dẫn chạy project

### Bước 1 — Khởi động backing services

```bash
cd src/eshop-modular-monoliths
docker-compose up -d
```

Kiểm tra container PostgreSQL đã chạy:
```bash
docker ps
```

### Bước 2 — Chạy API

Mở solution bằng Visual Studio/Rider, đặt `Api` làm Startup Project, chọn launch profile `https`, nhấn **Run/Debug**.

API sẽ tự động:
- Áp dụng EF Core Migrations
- Seed dữ liệu mẫu vào bảng `catalog.Products`

### Bước 3 — Kiểm tra

Gọi `GET https://localhost:<port>/products` — nếu trả về danh sách sản phẩm, project đã chạy thành công.

## 6. Danh sách API — Module Catalog

| Method | Endpoint | Mô tả |
|---|---|---|
| `POST` | `/products` | Tạo sản phẩm mới |
| `GET` | `/products` | Lấy toàn bộ sản phẩm |
| `GET` | `/products/{id}` | Lấy sản phẩm theo Id |
| `GET` | `/products/category/{category}` | Lấy sản phẩm theo danh mục |
| `GET` | `/products/name/{name}` | **⭐ Tìm sản phẩm theo tên (partial match)** — tính năng bổ sung theo yêu cầu đề bài |
| `PUT` | `/products/{id}` | Cập nhật sản phẩm |
| `DELETE` | `/products/{id}` | Xóa sản phẩm |

### Ví dụ request tạo sản phẩm

```json
POST /products
{
  "name": "book",
  "category": ["detective"],
  "imageFile": "book.png",
  "price": 99
}
```
> Trường `description` là **tùy chọn** (nullable), đúng theo thiết kế gốc của dự án — không bắt buộc phải cung cấp.

## 7. Kiến trúc CQRS trong module Catalog

Luồng xử lý một request đi qua các tầng theo thứ tự:

```
Carter Endpoint → ISender.Send() → Pipeline Behaviors → Command/Query Handler → EF Core → PostgreSQL
                                    (ValidationBehavior → LoggingBehavior)
```

- **Command** (`ICommand<TResponse>`): dùng cho thao tác ghi (`Create`, `Update`, `Delete`), luôn đi qua `ValidationBehavior`.
- **Query** (`IQuery<TResponse>`): dùng cho thao tác đọc, không cần validate.
- **Handler**: xử lý logic nghiệp vụ, tương tác trực tiếp với `CatalogDbContext`.
- **Domain Event**: `Product` phát sinh `ProductCreatedEvent` / `ProductPriceChangedEvent`, được dispatch tự động qua `DispatchDomainEventsInterceptor` khi `SaveChanges()`.

## 8. Xử lý lỗi tập trung

Dự án sử dụng `CustomExceptionHandler` (implement `IExceptionHandler`) để chuẩn hóa response lỗi theo [RFC 7807 ProblemDetails](https://datatracker.ietf.org/doc/html/rfc7807):

| Exception | HTTP Status |
|---|---|
| `ProductNotFoundException` | `404 Not Found` |
| `FluentValidation.ValidationException` | `400 Bad Request` |
| Exception khác | `500 Internal Server Error` |

## 9. Ghi chú triển khai

- Dự án target **.NET 10** (khác với .NET 8 trong tài liệu gốc của khóa học) — lựa chọn có chủ đích.
- Đã phát hiện và sửa 2 lỗi tiềm ẩn trong quá trình triển khai theo slide:
  - `AddInterceptors` chỉ đăng ký được 1 trong 2 `ISaveChangesInterceptor` do dùng `GetService` (số ít) thay vì `GetServices` (số nhiều).
  - Domain event `ProductPriceChangedEvent` không bao giờ được raise do thứ tự gán giá trị sai trong `Product.Update()`.

## 10. Tác giả

Bài tập thực hành môn *Kiến trúc phần mềm* — dựa trên khóa học của Mehmet Ozkaya, giảng viên hướng dẫn: Khanh-Duy Nguyen @UIT.
