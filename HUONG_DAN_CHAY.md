# Hướng dẫn chạy — Quản lý cửa hàng cho thuê xe máy (PRN232)

## 1. Tạo CSDL bằng SSMS 2022
1. Mở SSMS → kết nối SQL Server → mở `Database/MotorRentalDB.sql` → bấm **Execute (F5)**.
2. Mở `MotorRental.Api/appsettings.json`, sửa `ConnectionStrings:MyCnn` cho đúng máy bạn:
   - Windows Authentication: `Server=.;Database=MotorRentalDB;Trusted_Connection=True;TrustServerCertificate=True`
   - SQL Authentication:     `Server=.;Database=MotorRentalDB;User Id=sa;Password=123;TrustServerCertificate=True`
   - SQL Express:            `Server=.\SQLEXPRESS;...`   |   LocalDB: `Server=(localdb)\MSSQLLocalDB;...`

## 2. Mở bằng Visual Studio
1. Double-click `MotorRental.sln` (cần .NET 8 SDK; nếu máy chỉ có .NET 10 thì đổi `net8.0` → `net10.0` và nâng version package).
2. Chuột phải Solution → **Configure Startup Projects** → **Multiple startup projects** → cả `MotorRental.Api` và `MotorRental.Web` chọn **Start**.
3. F5. API: https://localhost:7295/swagger — Web: https://localhost:7140

## 3. Tài khoản (API tự tạo lần chạy đầu)
| Tài khoản | Mật khẩu   | Vai trò |
|-----------|------------|---------|
| admin     | Admin@123  | Admin   |
| staff     | Staff@123  | Staff   |

## 4. Thử API bằng Swagger
`POST /api/auth/login` → copy `token` → nút **Authorize** → dán token → thử các endpoint.
OData: `GET /api/motorbikes?$filter=Status eq 'Available' and PricePerDay lt 200000&$orderby=PricePerDay desc&$select=Name,PricePerDay&$count=true`

## 5. Lưu ý
- Giao diện dùng Bootstrap/jQuery qua CDN → cần Internet. Nếu phòng thi không có mạng, tải file về `wwwroot` và sửa `_Layout.cshtml`.
- Đổi `Jwt:Key` trong `MotorRental.Api/appsettings.json` nếu muốn (>= 32 ký tự).
- Nếu Web báo "Không kết nối được Web API": kiểm tra API đang chạy và `ApiBaseUrl` trong `MotorRental.Web/appsettings.json` đúng cổng.
