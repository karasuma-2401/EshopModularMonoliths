Phần 1: Basis nền tảng **(5 phút) (Chí Nguyên)**

- ORM là gì ? Lý do xuất hiện ?&nbsp;  
- So sánh [ADO.NET](http://ADO.NET) và EF Core: chủ yếu đề cập về boilerplate code và rủi ro SQL injection khi thực hiện \+ chuỗi  
- Giới thiệu Dbcontext \+ DbSet\<T\> , Repository Pattern  
- Code mẫu minh họa việc register DI

Phấn 2: Mapping & migration **(7 phút) (Thịnh)**

- Data Annotation \+ Fluent API \+ Giới thiệu pattern IEntityTypeConfiguration\<T\>. Lý do vì sao sử dụng nó thay vì OnModelCreating  
- Tập trung vào chu trình Code-First: Add-Migration \-\> Update-Database  
- Database First (có thể nói sơ qua là có 2 loai nhưng ít đc sử dụng hơn)  
- Đề cập đến Rollback migration \+ lỗi lệch schema

Phần 3: CRUD **(6 phút) (Chí Nguyên)**

- Chuẩn bị code mẫu CRUD (nhớ là async nha \!\!\!, demo đủ 4 tác vụ CRUD thôi)  
- Minh họa LINQ to Enities \+ giải thích cơ chế Deferred Execution (Khi nào thì SQL thực sự được gửi đi)

Phần 4: Hiệu năng \+ Lỗi kinh điển **(8 phút) (Khánh)**

- Giới thiệu về Change Tracker (4 trạng thái \+ cost khi mà giữ snapshot trên RAM)  
- Minh họa bài toán siêu điển N+1 và cách giải quyết  
- Đề cập hiện tượng Cartesian Explosion khi dùng nhiều include và giải pháp  
- Lazy Loading \+ vì sao  EFCore lại tắt \-\> loading strategies \+ AsNoTracking

Phần 5:  Demo live **(5 phút) (Thắng)**

- **Mục tiêu**: Chứng minh trực quan bằng số liệu (ms, RAM, số câu SQL) các lý thuyết ở Phần 1-4 trên hệ thống thực tế `EshopModularMonoliths`.
- **Công cụ thực hiện**: Chạy file `src/eshop-modular-monoliths/demo-efcore.http` (trên Rider / VS Code / Postman / Scalar UI `http://localhost:5000/scalar/v1`) kết hợp mở cửa sổ Terminal để khán giả thấy các câu SQL thực thi trực tiếp.

### Kịch bản chi tiết 5 phút (3 Demo Hiệu Năng Thực Tế):

1. **Phút 0:00 - 1:30: Bulk Insert & Cơ chế Batching (Cũ vs Mới - Tối ưu Ghi dữ liệu)**
   - **Thao tác**: Gửi `POST /demo/efcore/seed-products?count=1000&compareLoop=true`
   - **Lời thoại / Giải thích**:
     - *"Cách cũ/ngây thơ"*: Chạy vòng lặp `for` gọi `Add()` và `SaveChanges()` cho từng item. Chỉ 50 items mà mất tới ~600 - 2.000 ms vì mỗi lần `SaveChanges()` phải mở connection, tạo transaction và gửi round-trip mạng tới PostgreSQL.
     - *"Cách tối ưu"*: Dùng `AddRange()` và gọi `1 lần SaveChangesAsync()` để nạp 1.000 records. EF Core tự động gom các câu lệnh INSERT vào các Batch command, hoàn thành chỉ trong ~200ms.
   - **Kết luận**: Tuyệt đối không gọi `SaveChanges()` bên trong vòng lặp!

2. **Phút 1:30 - 3:00: Hiệu năng Change Tracker - `Tracking` vs `AsNoTracking` (Tối ưu Đọc dữ liệu)**
   - **Thao tác**: Gửi `GET /demo/efcore/tracking-vs-asnotracking?count=1000`
   - **Lời thoại / Giải thích**:
     - Khi truy vấn 1.000 records với `Tracking` (mặc định): Change Tracker phải tạo và giữ snapshot của 1.000 entities trên RAM.
     - Khi thêm `.AsNoTracking()`: Change Tracker = 0 entities! Tốc độ query nhanh hơn từ 3x đến hơn 7x và RAM hoàn toàn giải phóng vì EF Core bỏ qua bước snapshot.
   - **Kết luận**: Với mọi API Read-only (GET, báo cáo, export, phân trang), **bắt buộc** phải dùng `AsNoTracking()`.

3. **Phút 3:00 - 4:30: Cơn ác mộng N+1 Queries & Cứu tinh Eager Loading (`.Include`)**
   - **Thao tác**: Gửi `GET /demo/efcore/n-plus-one?orderCount=10` kết hợp chỉ tay vào màn hình Terminal xem SQL log nhảy.
   - **Lời thoại / Giải thích**:
     - *"Cách sai (N+1)"*: Lấy 10 Giỏ hàng, sau đó trong code duyệt vòng lặp `foreach` để load tiếp Items. Khán giả nhìn màn hình terminal sẽ thấy log bắn ra **11 câu SQL riêng rẽ** (1 câu lấy Carts + 10 câu lấy Items). Nếu có 1.000 giỏ hàng thì DB sẽ hứng 1.001 câu truy vấn, làm nghẽn DB!
     - *"Cách chuẩn (Eager Loading)"*: Dùng `.Include(c => c.Items)`. Terminal chỉ hiển thị **ĐÚNG 1 CÂU SQL `LEFT JOIN` duy nhất**.
   - **Kết luận**: Không truy vấn lặp trong vòng lặp, luôn tận dụng Eager Loading (`.Include`), Split Query (`.AsSplitQuery`) hoặc LINQ Projection (`.Select`).

4. **Phút 4:30 - 5:00: Tổng kết 3 nguyên tắc vàng & Chuyển sang Mini-game Kahoot**
   - Chốt lại 3 nguyên tắc vàng để làm chủ hiệu năng EF Core:
     1. Ghi dữ liệu nhiều: Gom batch với `AddRange` thay vì `SaveChanges` trong vòng lặp.
     2. Đọc dữ liệu: Luôn dùng `AsNoTracking()` cho các API Read-only.
     3. Tránh N+1: Luôn dùng `.Include()` hoặc Projection `.Select()` thay vì truy vấn con trong loop.
   - Chuyển giao phần trình bày cho người dẫn phần Mini-game.

Phần 6: Mini game kahoot hoặc là chiếu 4 câu rồi mời trả lời (khoảng 5-10 câu) \- tui nghĩ là ít thôi vừa đủ thì mới kịp thời gian