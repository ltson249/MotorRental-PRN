namespace MotorRental.Api.Models;

public class AppUser
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = "Staff";
    public bool IsActive { get; set; } = true;
    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}

public class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string IdentityNumber { get; set; } = null!;
    public string? DriverLicense { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}

public class Motorbike
{
    public int MotorbikeId { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public int EngineCapacity { get; set; }
    public decimal PricePerDay { get; set; }
    public string Status { get; set; } = "Available";   // Available | Rented | Maintenance
    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}

public class Rental
{
    public int RentalId { get; set; }
    public int CustomerId { get; set; }
    public int MotorbikeId { get; set; }
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal Deposit { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Active";      // Active | Completed | Cancelled
    public string? Note { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual Motorbike Motorbike { get; set; } = null!;
    public virtual AppUser Staff { get; set; } = null!;
}
