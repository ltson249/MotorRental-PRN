using MotorRental.Api.Models;
using MotorRental.Api.Services;

namespace MotorRental.Api.Data;

public static class DbSeeder
{
    public static void SeedUsers(MotorRentalContext db)
    {
        try
        {
            if (!db.Database.CanConnect())
            {
                Console.WriteLine("!!! Khong ket noi duoc SQL Server. Kiem tra ConnectionStrings:MyCnn");
                return;
            }

            // 1. SEED APP USERS (4 ROLES LIÊN KẾT CHẶT CHẼ)
            if (!db.Users.Any())
            {
                db.Users.AddRange(
                    new AppUser
                    {
                        Username = "admin",
                        PasswordHash = PasswordHasher.Hash("Admin@123"),
                        FullName = "Lê Tiến Sơn (Chủ Showroom FPT Hòa Lạc)",
                        Role = "Admin",
                        Email = "admin@motorrental.vn",
                        IsActive = true
                    },
                    new AppUser
                    {
                        Username = "staff",
                        PasswordHash = PasswordHasher.Hash("Staff@123"),
                        FullName = "Nguyễn Văn Hùng (Nhân viên Giao nhận Hòa Lạc)",
                        Role = "Staff",
                        Email = "staff@motorrental.vn",
                        IsActive = true
                    },
                    new AppUser
                    {
                        Username = "sinhvien_fpt",
                        PasswordHash = PasswordHasher.Hash("Fpt@123"),
                        FullName = "Vũ Đức Anh (SV K17 ĐH FPT Hà Nội)",
                        Role = "Customer",
                        Email = "anhvdhe173456@fpt.edu.vn",
                        IsActive = true
                    },
                    new AppUser
                    {
                        Username = "kythuat_hld",
                        PasswordHash = PasswordHasher.Hash("Tech@123"),
                        FullName = "Nguyễn Văn Tuấn (Xưởng Sửa Xe Anh Tuấn Hòa Lạc)",
                        Role = "Technician",
                        Email = "xuonganhtuan.hoalac@gmail.com",
                        IsActive = true
                    }
                );
                db.SaveChanges();
            }

            // 2. SEED CUSTOMERS (SINH VIÊN, GIẢNG VIÊN ĐH FPT & KỸ SƯ CÔNG NGHỆ HÒA LẠC)
            if (!db.Customers.Any())
            {
                var fptStudentUser = db.Users.FirstOrDefault(u => u.Username == "sinhvien_fpt");
                int? fptUserId = fptStudentUser?.UserId;

                var customers = new List<Customer>
                {
                    new Customer
                    {
                        FullName = "Vũ Đức Anh",
                        Phone = "0981123456",
                        IdentityNumber = "001202008899",
                        DriverLicense = "A1-123456",
                        Address = "Phòng 304 KTX Dom A, Đại học FPT Hà Nội, Hòa Lạc",
                        UserId = fptUserId
                    },
                    new Customer
                    {
                        FullName = "Hoàng Phương Thảo",
                        Phone = "0972345678",
                        IdentityNumber = "001203007788",
                        DriverLicense = "A1-654321",
                        Address = "Xóm 2, Thôn Tân Xã, Xã Tân Xã, Huyện Thạch Thất, Hà Nội (Gần hồ Tân Xã)"
                    },
                    new Customer
                    {
                        FullName = "Đặng Minh Quân",
                        Phone = "0963456789",
                        IdentityNumber = "001201006655",
                        DriverLicense = "A2-998877",
                        Address = "FPT Software Hola Park, Khu Công Nghệ Cao Hòa Lạc, Thạch Thất"
                    },
                    new Customer
                    {
                        FullName = "Nguyễn Thu Trang",
                        Phone = "0912987654",
                        IdentityNumber = "001204005544",
                        DriverLicense = "A1-445566",
                        Address = "Phòng 412 KTX Dom C, Đại học FPT Hà Nội, Hòa Lạc"
                    },
                    new Customer
                    {
                        FullName = "Trần Tuấn Kiệt",
                        Phone = "0989876543",
                        IdentityNumber = "001200004433",
                        DriverLicense = "A1-778899",
                        Address = "Trung tâm Nghiên cứu Viettel R&D, Khu Công Nghệ Cao Hòa Lạc"
                    },
                    new Customer
                    {
                        FullName = "ThS. Lê Bảo Ngọc",
                        Phone = "0934567890",
                        IdentityNumber = "001195003322",
                        DriverLicense = "A1-332211",
                        Address = "Giảng viên ĐH FPT Hà Nội, Khu đô thị Bắc Phú Cát, Thạch Hòa, Thạch Thất"
                    },
                    new Customer
                    {
                        FullName = "Phạm Hùng Cường",
                        Phone = "0987112233",
                        IdentityNumber = "001201002211",
                        DriverLicense = "A2-883311",
                        Address = "Thôn 3, Xã Thạch Hòa, Thạch Thất (Đối diện cổng số 2 Hola Park)"
                    },
                    new Customer
                    {
                        FullName = "Đỗ Thanh Hằng",
                        Phone = "0904556677",
                        IdentityNumber = "001202001100",
                        DriverLicense = "A1-990011",
                        Address = "Homestay Thung Lũng Xanh, Bình Yên, Thạch Thất (Du khách cắm trại Đồng Mô)"
                    }
                };

                db.Customers.AddRange(customers);
                db.SaveChanges();

                if (fptStudentUser != null)
                {
                    fptStudentUser.CustomerId = customers[0].CustomerId;
                    db.SaveChanges();
                }
            }

            // 3. SEED MOTORBIKES (100% BIỂN SỐ HÀ NỘI 29 & 30, ĐÚNG ẢNH TỪNG DÒNG XE)
            if (!db.Motorbikes.Any())
            {
                db.Motorbikes.AddRange(
                    new Motorbike
                    {
                        LicensePlate = "29-X1 838.86",
                        Name = "Honda Wave Alpha 110cc",
                        Brand = "Honda",
                        EngineCapacity = 110,
                        PricePerDay = 100000,
                        Status = "Available",
                        Category = "Xe số",
                        Color = "Đỏ đen",
                        CurrentOdometer = 12500,
                        ImageUrl = "https://images.unsplash.com/photo-1558981403-c5f9899a28bc?w=600&auto=format&fit=crop&q=80"
                    },
                    new Motorbike
                    {
                        LicensePlate = "29-V2 568.99",
                        Name = "Honda Vision 110cc Smartkey",
                        Brand = "Honda",
                        EngineCapacity = 110,
                        PricePerDay = 150000,
                        Status = "Available",
                        Category = "Xe tay ga",
                        Color = "Trắng ngọc trai",
                        CurrentOdometer = 8400,
                        ImageUrl = "https://images.unsplash.com/photo-1568772585407-9361f9bf3a87?w=600&auto=format&fit=crop&q=80"
                    },
                    new Motorbike
                    {
                        LicensePlate = "29-M1 928.38",
                        Name = "Honda Air Blade 125cc",
                        Brand = "Honda",
                        EngineCapacity = 125,
                        PricePerDay = 170000,
                        Status = "Rented", // Đang được sinh viên FPT thuê đi thực tập
                        Category = "Xe tay ga",
                        Color = "Đen mờ thể thao",
                        CurrentOdometer = 6800,
                        ImageUrl = "https://images.unsplash.com/photo-1609630875171-b1321377ee65?w=600&auto=format&fit=crop&q=80"
                    },
                    new Motorbike
                    {
                        LicensePlate = "29-P1 777.88",
                        Name = "Yamaha Exciter 155 VVA",
                        Brand = "Yamaha",
                        EngineCapacity = 155,
                        PricePerDay = 200000,
                        Status = "Available",
                        Category = "Xe côn tay",
                        Color = "Xanh GP phong cách",
                        CurrentOdometer = 15200,
                        ImageUrl = "https://images.unsplash.com/photo-1571188654248-7a89213915f7?w=600&auto=format&fit=crop&q=80"
                    },
                    new Motorbike
                    {
                        LicensePlate = "29-C1 668.68",
                        Name = "Honda SH Mode 125cc",
                        Brand = "Honda",
                        EngineCapacity = 125,
                        PricePerDay = 250000,
                        Status = "Maintenance", // Đang ở xưởng Anh Tuấn bảo dưỡng nồi
                        Category = "Xe tay ga",
                        Color = "Xám xi măng cao cấp",
                        CurrentOdometer = 18000,
                        ImageUrl = "https://images.unsplash.com/photo-1558981806-ec527fa84c39?w=600&auto=format&fit=crop&q=80"
                    },
                    new Motorbike
                    {
                        LicensePlate = "29-D1 368.88",
                        Name = "Honda Winner X 150cc ABS",
                        Brand = "Honda",
                        EngineCapacity = 150,
                        PricePerDay = 220000,
                        Status = "Available",
                        Category = "Xe côn tay",
                        Color = "Đỏ bạc đen thể thao",
                        CurrentOdometer = 9300,
                        ImageUrl = "https://images.unsplash.com/photo-1558980394-4c7c9299fe96?w=600&auto=format&fit=crop&q=80"
                    },
                    new Motorbike
                    {
                        LicensePlate = "29-E2 234.56",
                        Name = "VinFast Feliz S (Xe điện)",
                        Brand = "VinFast",
                        EngineCapacity = 80,
                        PricePerDay = 140000,
                        Status = "Available",
                        Category = "Xe điện",
                        Color = "Xanh rêu Eco",
                        CurrentOdometer = 4100,
                        ImageUrl = "https://images.unsplash.com/photo-1596700813953-e99d123d4c3f?w=600&auto=format&fit=crop&q=80"
                    },
                    new Motorbike
                    {
                        LicensePlate = "30-F1 888.99",
                        Name = "Vespa Sprint 125cc ABS",
                        Brand = "Vespa",
                        EngineCapacity = 125,
                        PricePerDay = 300000,
                        Status = "Rented", // Giảng viên FPT thuê
                        Category = "Xe tay ga",
                        Color = "Vàng chanh cá tính",
                        CurrentOdometer = 11000,
                        ImageUrl = "https://images.unsplash.com/photo-1558981420-87aa9dad1c89?w=600&auto=format&fit=crop&q=80"
                    },
                    new Motorbike
                    {
                        LicensePlate = "29-S1 456.78",
                        Name = "Honda Lead 125cc (Cốp siêu rộng)",
                        Brand = "Honda",
                        EngineCapacity = 125,
                        PricePerDay = 160000,
                        Status = "Available",
                        Category = "Xe tay ga",
                        Color = "Bạc nâu thanh lịch",
                        CurrentOdometer = 13600,
                        ImageUrl = "https://images.unsplash.com/photo-1525160354320-d8e92641c563?w=600&auto=format&fit=crop&q=80"
                    },
                    new Motorbike
                    {
                        LicensePlate = "30-H2 588.66",
                        Name = "Honda SH 150i ABS",
                        Brand = "Honda",
                        EngineCapacity = 150,
                        PricePerDay = 350000,
                        Status = "Available",
                        Category = "Xe tay ga",
                        Color = "Đen mờ lịch lãm",
                        CurrentOdometer = 7500,
                        ImageUrl = "https://images.unsplash.com/photo-1558981408-db0ecd8a1ee4?w=600&auto=format&fit=crop&q=80"
                    },
                    new Motorbike
                    {
                        LicensePlate = "29-K1 678.90",
                        Name = "Yamaha Grande 125cc Hybrid",
                        Brand = "Yamaha",
                        EngineCapacity = 125,
                        PricePerDay = 180000,
                        Status = "Available",
                        Category = "Xe tay ga",
                        Color = "Trắng ánh kim thời thượng",
                        CurrentOdometer = 5200,
                        ImageUrl = "https://images.unsplash.com/photo-1511994298241-608e28f14fde?w=600&auto=format&fit=crop&q=80"
                    },
                    new Motorbike
                    {
                        LicensePlate = "29-M2 135.79",
                        Name = "VinFast Klara S (Xe điện)",
                        Brand = "VinFast",
                        EngineCapacity = 90,
                        PricePerDay = 150000,
                        Status = "Available",
                        Category = "Xe điện",
                        Color = "Đỏ đô thời trang",
                        CurrentOdometer = 3800,
                        ImageUrl = "https://images.unsplash.com/photo-1558981359-219d6364c9c8?w=600&auto=format&fit=crop&q=80"
                    }
                );
                db.SaveChanges();
            }

            // 4. SEED SAMPLE RENTALS (CÓ PHIẾU ĐANG THUÊ, ĐÃ THANH TOÁN VIETQR)
            if (!db.Rentals.Any())
            {
                var cust1 = db.Customers.FirstOrDefault(c => c.FullName == "Vũ Đức Anh") ?? db.Customers.First();
                var custLecturer = db.Customers.FirstOrDefault(c => c.FullName.Contains("Lê Bảo Ngọc")) ?? db.Customers.Skip(5).First();
                var staffUser = db.Users.FirstOrDefault(u => u.Role == "Staff") ?? db.Users.First();

                var bikeAirBlade = db.Motorbikes.FirstOrDefault(b => b.LicensePlate == "29-M1 928.38") ?? db.Motorbikes.First();
                var bikeVespa = db.Motorbikes.FirstOrDefault(b => b.LicensePlate == "30-F1 888.99") ?? db.Motorbikes.Skip(7).First();
                var bikeWave = db.Motorbikes.FirstOrDefault(b => b.LicensePlate == "29-X1 838.86") ?? db.Motorbikes.First();

                db.Rentals.AddRange(
                    // Phiếu của bạn Vũ Đức Anh (Sinh viên FPT - Đang thuê Air Blade)
                    new Rental
                    {
                        CustomerId = cust1.CustomerId,
                        MotorbikeId = bikeAirBlade.MotorbikeId,
                        UserId = staffUser.UserId,
                        StartDate = DateTime.Now.AddDays(-1),
                        EndDate = DateTime.Now.AddDays(2),
                        PricePerDay = 170000,
                        Deposit = 1000000,
                        TotalPrice = 510000, // 3 ngày
                        Status = "Active",
                        OdometerOut = 6800,
                        PaymentStatus = "DepositPaid", // Đã chuyển khoản Vietcombank cọc 1 triệu
                        PaymentMethod = "VietQR (Vietcombank)",
                        PaymentDate = DateTime.Now.AddDays(-1),
                        Note = "Khách hàng SV K17 ĐH FPT (KTX Dom A). Bàn giao 02 mũ bảo hiểm Hola, 01 áo mưa và cavet photo công chứng."
                    },
                    // Phiếu của Giảng viên Lê Bảo Ngọc (Đang thuê Vespa Sprint)
                    new Rental
                    {
                        CustomerId = custLecturer.CustomerId,
                        MotorbikeId = bikeVespa.MotorbikeId,
                        UserId = staffUser.UserId,
                        StartDate = DateTime.Now.AddDays(-2),
                        EndDate = DateTime.Now.AddDays(1),
                        PricePerDay = 300000,
                        Deposit = 2000000,
                        TotalPrice = 900000, // 3 ngày
                        Status = "Active",
                        OdometerOut = 11000,
                        PaymentStatus = "DepositPaid",
                        PaymentMethod = "VietQR (Vietcombank)",
                        PaymentDate = DateTime.Now.AddDays(-2),
                        Note = "Giảng viên thỉnh giảng ĐH FPT. Giao xe tại sảnh Alpha ĐH FPT Hà Nội."
                    },
                    // 1 Phiếu lịch sử đã hoàn tất và quyết toán xong
                    new Rental
                    {
                        CustomerId = cust1.CustomerId,
                        MotorbikeId = bikeWave.MotorbikeId,
                        UserId = staffUser.UserId,
                        StartDate = DateTime.Now.AddDays(-5),
                        EndDate = DateTime.Now.AddDays(-3),
                        ReturnDate = DateTime.Now.AddDays(-3),
                        PricePerDay = 100000,
                        Deposit = 500000,
                        TotalPrice = 200000,
                        Status = "Completed",
                        OdometerOut = 12350,
                        OdometerIn = 12500,
                        ActualPayment = 200000 - 500000, // Cửa hàng hoàn cọc 300k
                        PaymentStatus = "FullyPaid",
                        PaymentMethod = "VietQR (Vietcombank)",
                        PaymentDate = DateTime.Now.AddDays(-3),
                        ConditionNote = "Xe trả đúng hạn, máy móc êm, trả lại 300.000 ₫ tiền cọc cho khách."
                    }
                );
                db.SaveChanges();
            }

            // 5. SEED MAINTENANCES (CÓ HỒ SƠ CHỜ THANH TOÁN CHO BÊN BẢO DƯỠNG)
            if (!db.Maintenances.Any())
            {
                var bikeShMode = db.Motorbikes.FirstOrDefault(b => b.LicensePlate == "29-C1 668.68") ?? db.Motorbikes.First();
                var bikeWinner = db.Motorbikes.FirstOrDefault(b => b.LicensePlate == "29-D1 368.88") ?? db.Motorbikes.First();
                var techUser = db.Users.FirstOrDefault(u => u.Role == "Technician");
                var staffUser = db.Users.FirstOrDefault(u => u.Role == "Staff") ?? db.Users.First();

                db.Maintenances.AddRange(
                    // Xe đang trong xưởng Anh Tuấn bảo dưỡng chờ nhân viên thanh toán tiền phụ tùng
                    new Maintenance
                    {
                        MotorbikeId = bikeShMode.MotorbikeId,
                        StartDate = DateTime.Now.AddDays(-1),
                        Description = "Bảo dưỡng nồi tay ga, vệ sinh kim phun buồng đốt, thay nhớt hộp số và nhớt máy Motul Scooter",
                        Cost = 350000,
                        OdometerAtMaint = 18000,
                        Status = "InProgress",
                        TechnicianId = techUser?.UserId,
                        PaymentStatus = "Unpaid", // Showroom chưa thanh toán cho Xưởng
                        TechnicianBankInfo = "Vietcombank 1014306428 - LE TIEN SON (Xưởng Anh Tuấn Hòa Lạc)"
                    },
                    // Hồ sơ tuần trước đã thanh toán cho bên bảo trì
                    new Maintenance
                    {
                        MotorbikeId = bikeWinner.MotorbikeId,
                        StartDate = DateTime.Now.AddDays(-10),
                        EndDate = DateTime.Now.AddDays(-9),
                        Description = "Thay nhông sên dĩa DID vàng chính hãng, thay bố thắng đĩa trước Nissin",
                        Cost = 480000,
                        OdometerAtMaint = 9000,
                        Status = "Completed",
                        TechnicianId = techUser?.UserId,
                        PaymentStatus = "Paid",
                        PaidDate = DateTime.Now.AddDays(-9),
                        PaidByStaff = staffUser.FullName,
                        TechnicianBankInfo = "Vietcombank 1014306428 - LE TIEN SON"
                    }
                );
                db.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Seed loi: " + ex.Message);
        }
    }
}
