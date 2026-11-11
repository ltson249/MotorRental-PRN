using MotorRental.Api.Models;
using MotorRental.Api.Services;

namespace MotorRental.Api.Data;

public static class DbSeeder
{
    // Tao tai khoan mac dinh neu bang Users dang trong
    public static void SeedUsers(MotorRentalContext db)
    {
        try
        {
            if (!db.Database.CanConnect())
            {
                Console.WriteLine("!!! Khong ket noi duoc SQL Server. Kiem tra ConnectionStrings:MyCnn va chay Database/MotorRentalDB.sql");
                return;
            }
            if (db.Users.Any()) return;

            db.Users.AddRange(
                new AppUser { Username = "admin", PasswordHash = PasswordHasher.Hash("Admin@123"), FullName = "Quản trị viên", Role = "Admin" },
                new AppUser { Username = "staff", PasswordHash = PasswordHasher.Hash("Staff@123"), FullName = "Nhân viên cho thuê", Role = "Staff" });
            db.SaveChanges();
            Console.WriteLine(">>> Da tao tai khoan: admin/Admin@123 va staff/Staff@123");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Seed loi: " + ex.Message);
        }
    }
}
