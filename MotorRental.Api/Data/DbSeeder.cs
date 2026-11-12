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
                Console.WriteLine("!!! Khong ket noi duoc SQL Server. Kiem tra ConnectionStrings:MyCnn va chay Database/MotorRentalDB.sql");
                return;
            }

            // 1. Seed Users
            if (!db.Users.Any())
            {
                db.Users.AddRange(
                    new AppUser { Username = "admin", PasswordHash = PasswordHasher.Hash("Admin@123"), FullName = "Quản trị viên", Role = "Admin" },
                    new AppUser { Username = "staff", PasswordHash = PasswordHasher.Hash("Staff@123"), FullName = "Nhân viên hệ thống", Role = "Staff" }
                );
                db.SaveChanges();
                Console.WriteLine(">>> Da tao tai khoan mac dinh: admin/Admin@123 va staff/Staff@123");
            }

            // 2. Seed Motorbikes neu chua co hoac chua co anh
            if (!db.Motorbikes.Any())
            {
                db.Motorbikes.AddRange(
                    new Motorbike
                    {
                        LicensePlate = "59X1-123.45",
                        Name = "Honda Wave Alpha 110",
                        Brand = "Honda",
                        EngineCapacity = 110,
                        PricePerDay = 100000,
                        Status = "Available",
                        Category = "Xe số",
                        Color = "Đỏ đen",
                        CurrentOdometer = 12500,
                        ImageUrl = "https://images.unsplash.com/photo-1558981403-c5f9899a28bc?w=500&auto=format&fit=crop&q=60"
                    },
                    new Motorbike
                    {
                        LicensePlate = "59X2-234.56",
                        Name = "Honda Vision 110cc",
                        Brand = "Honda",
                        EngineCapacity = 110,
                        PricePerDay = 150000,
                        Status = "Available",
                        Category = "Xe tay ga",
                        Color = "Trắng ngọc trai",
                        CurrentOdometer = 8400,
                        ImageUrl = "https://images.unsplash.com/photo-1568772585407-9361f9bf3a87?w=500&auto=format&fit=crop&q=60"
                    },
                    new Motorbike
                    {
                        LicensePlate = "59P1-345.67",
                        Name = "Yamaha Exciter 155 VVA",
                        Brand = "Yamaha",
                        EngineCapacity = 155,
                        PricePerDay = 200000,
                        Status = "Available",
                        Category = "Xe côn tay",
                        Color = "Xanh GP",
                        CurrentOdometer = 15200,
                        ImageUrl = "https://images.unsplash.com/photo-1571188654248-7a89213915f7?w=500&auto=format&fit=crop&q=60"
                    },
                    new Motorbike
                    {
                        LicensePlate = "59H1-456.78",
                        Name = "Honda Air Blade 125",
                        Brand = "Honda",
                        EngineCapacity = 125,
                        PricePerDay = 170000,
                        Status = "Available",
                        Category = "Xe tay ga",
                        Color = "Đen mờ",
                        CurrentOdometer = 6800,
                        ImageUrl = "https://images.unsplash.com/photo-1609630875171-b1321377ee65?w=500&auto=format&fit=crop&q=60"
                    },
                    new Motorbike
                    {
                        LicensePlate = "59C1-567.89",
                        Name = "Honda SH Mode 125",
                        Brand = "Honda",
                        EngineCapacity = 125,
                        PricePerDay = 250000,
                        Status = "Maintenance",
                        Category = "Xe tay ga",
                        Color = "Xám xi măng",
                        CurrentOdometer = 18000,
                        ImageUrl = "https://images.unsplash.com/photo-1558981806-ec527fa84c39?w=500&auto=format&fit=crop&q=60"
                    },
                    new Motorbike
                    {
                        LicensePlate = "59D1-678.90",
                        Name = "Honda Winner X 150",
                        Brand = "Honda",
                        EngineCapacity = 150,
                        PricePerDay = 220000,
                        Status = "Available",
                        Category = "Xe côn tay",
                        Color = "Đỏ bạc đen",
                        CurrentOdometer = 9300,
                        ImageUrl = "https://images.unsplash.com/photo-1558980394-4c7c9299fe96?w=500&auto=format&fit=crop&q=60"
                    },
                    new Motorbike
                    {
                        LicensePlate = "59E1-789.01",
                        Name = "VinFast Feliz S (Điện)",
                        Brand = "VinFast",
                        EngineCapacity = 80,
                        PricePerDay = 140000,
                        Status = "Available",
                        Category = "Xe điện",
                        Color = "Xanh rêu",
                        CurrentOdometer = 4100,
                        ImageUrl = "https://images.unsplash.com/photo-1558981420-87aa9dad1c89?w=500&auto=format&fit=crop&q=60"
                    },
                    new Motorbike
                    {
                        LicensePlate = "59K1-890.12",
                        Name = "Vespa Sprint 125",
                        Brand = "Vespa",
                        EngineCapacity = 125,
                        PricePerDay = 300000,
                        Status = "Available",
                        Category = "Xe tay ga",
                        Color = "Vàng chanh",
                        CurrentOdometer = 11000,
                        ImageUrl = "https://images.unsplash.com/photo-1558981408-db0ecd8a1ee4?w=500&auto=format&fit=crop&q=60"
                    }
                );
                db.SaveChanges();
            }
            else
            {
                // Cập nhật ảnh và category cho các xe nếu trước đó bị null
                var bikes = db.Motorbikes.ToList();
                foreach (var b in bikes)
                {
                    if (string.IsNullOrEmpty(b.ImageUrl))
                    {
                        b.ImageUrl = "https://images.unsplash.com/photo-1558981403-c5f9899a28bc?w=500&auto=format&fit=crop&q=60";
                    }
                    if (string.IsNullOrEmpty(b.Category))
                    {
                        b.Category = b.Name.Contains("Vision") || b.Name.Contains("Air") || b.Name.Contains("SH") ? "Xe tay ga" : "Xe số";
                    }
                }
                db.SaveChanges();
            }

            // 3. Seed Customers
            if (!db.Customers.Any())
            {
                db.Customers.AddRange(
                    new Customer
                    {
                        FullName = "Nguyễn Văn An",
                        Phone = "0901234567",
                        IdentityNumber = "079201001234",
                        DriverLicense = "A1-123456",
                        Address = "Quận 1, TP. Hồ Chí Minh"
                    },
                    new Customer
                    {
                        FullName = "Trần Thị Bình",
                        Phone = "0912345678",
                        IdentityNumber = "079202005678",
                        DriverLicense = "A1-654321",
                        Address = "Quận 3, TP. Hồ Chí Minh"
                    },
                    new Customer
                    {
                        FullName = "Lê Hoàng Cường",
                        Phone = "0987654321",
                        IdentityNumber = "079203009999",
                        DriverLicense = "A2-999888",
                        Address = "TP. Thủ Đức, TP. Hồ Chí Minh"
                    }
                );
                db.SaveChanges();
            }

            // 4. Seed Maintenances
            if (!db.Maintenances.Any())
            {
                var bike = db.Motorbikes.FirstOrDefault(x => x.Status == "Maintenance") ?? db.Motorbikes.First();
                db.Maintenances.Add(new Maintenance
                {
                    MotorbikeId = bike.MotorbikeId,
                    StartDate = DateTime.Now.AddDays(-2),
                    Description = "Thay nhớt Motul 10W40, tăng xích và thay bố thắng đĩa trước",
                    Cost = 280000,
                    OdometerAtMaint = 18000,
                    Status = "InProgress"
                });
                db.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Seed loi: " + ex.Message);
        }
    }
}
