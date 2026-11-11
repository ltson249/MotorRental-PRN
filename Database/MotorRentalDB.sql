/* ===== SSMS 2022: mo file nay, bam Execute (F5) ===== */
USE master;
GO
IF DB_ID('MotorRentalDB') IS NOT NULL
BEGIN
    ALTER DATABASE MotorRentalDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MotorRentalDB;
END
GO
CREATE DATABASE MotorRentalDB;
GO
USE MotorRentalDB;
GO

CREATE TABLE Users (
    UserId       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    FullName     NVARCHAR(100) NOT NULL,
    [Role]       NVARCHAR(20)  NOT NULL CONSTRAINT CK_Users_Role CHECK ([Role] IN ('Admin','Staff')),
    IsActive     BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1
);

CREATE TABLE Customers (
    CustomerId     INT IDENTITY(1,1) PRIMARY KEY,
    FullName       NVARCHAR(100) NOT NULL,
    Phone          VARCHAR(10)   NOT NULL UNIQUE CONSTRAINT CK_Customers_Phone CHECK (Phone LIKE '0%' AND LEN(Phone) = 10),
    IdentityNumber VARCHAR(12)   NOT NULL UNIQUE CONSTRAINT CK_Customers_CCCD CHECK (LEN(IdentityNumber) IN (9,12)),
    DriverLicense  NVARCHAR(20)  NULL,
    [Address]      NVARCHAR(200) NULL,
    CreatedAt      DATETIME2 NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT SYSDATETIME()
);

CREATE TABLE Motorbikes (
    MotorbikeId    INT IDENTITY(1,1) PRIMARY KEY,
    LicensePlate   NVARCHAR(15)  NOT NULL UNIQUE,
    Name           NVARCHAR(100) NOT NULL,
    Brand          NVARCHAR(50)  NOT NULL,
    EngineCapacity INT NOT NULL CONSTRAINT CK_Motorbikes_Engine CHECK (EngineCapacity BETWEEN 50 AND 1000),
    PricePerDay    DECIMAL(18,0) NOT NULL CONSTRAINT CK_Motorbikes_Price CHECK (PricePerDay > 0),
    Status         NVARCHAR(20)  NOT NULL CONSTRAINT DF_Motorbikes_Status DEFAULT 'Available'
                   CONSTRAINT CK_Motorbikes_Status CHECK (Status IN ('Available','Rented','Maintenance'))
);

CREATE TABLE Rentals (
    RentalId    INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId  INT NOT NULL CONSTRAINT FK_Rentals_Customers  FOREIGN KEY REFERENCES Customers(CustomerId),
    MotorbikeId INT NOT NULL CONSTRAINT FK_Rentals_Motorbikes FOREIGN KEY REFERENCES Motorbikes(MotorbikeId),
    UserId      INT NOT NULL CONSTRAINT FK_Rentals_Users      FOREIGN KEY REFERENCES Users(UserId),
    StartDate   DATETIME2 NOT NULL,
    EndDate     DATETIME2 NOT NULL,
    ReturnDate  DATETIME2 NULL,
    PricePerDay DECIMAL(18,0) NOT NULL,
    Deposit     DECIMAL(18,0) NOT NULL CONSTRAINT DF_Rentals_Deposit DEFAULT 0 CONSTRAINT CK_Rentals_Deposit CHECK (Deposit >= 0),
    TotalPrice  DECIMAL(18,0) NOT NULL CONSTRAINT DF_Rentals_Total DEFAULT 0,
    Status      NVARCHAR(20)  NOT NULL CONSTRAINT DF_Rentals_Status DEFAULT 'Active'
                CONSTRAINT CK_Rentals_Status CHECK (Status IN ('Active','Completed','Cancelled')),
    Note        NVARCHAR(300) NULL,
    CONSTRAINT CK_Rentals_Dates CHECK (EndDate > StartDate)
);
GO

/* Du lieu mau (tai khoan Users se duoc API tu tao khi chay lan dau: admin/Admin@123 va staff/Staff@123) */
INSERT INTO Motorbikes (LicensePlate, Name, Brand, EngineCapacity, PricePerDay, Status) VALUES
(N'59X1-123.45', N'Honda Wave Alpha 110', N'Honda',  110, 100000, 'Available'),
(N'59X2-234.56', N'Honda Vision 110',     N'Honda',  110, 150000, 'Available'),
(N'59P1-345.67', N'Yamaha Exciter 155',   N'Yamaha', 155, 200000, 'Available'),
(N'59H1-456.78', N'Honda Air Blade 125',  N'Honda',  125, 170000, 'Available'),
(N'59C1-567.89', N'Honda SH Mode 125',    N'Honda',  125, 250000, 'Maintenance'),
(N'59D1-678.90', N'Honda Winner X 150',   N'Honda',  150, 220000, 'Available');

INSERT INTO Customers (FullName, Phone, IdentityNumber, DriverLicense, [Address]) VALUES
(N'Nguyễn Văn An', '0901234567', '079201001234', N'A1-123456', N'Quận 1, TP.HCM'),
(N'Trần Thị Bình', '0912345678', '079202005678', N'A1-654321', N'Quận 3, TP.HCM'),
(N'Lê Hoàng Cường', '0987654321', '079203009999', NULL,       N'Thủ Đức, TP.HCM');
GO
SELECT * FROM Motorbikes; SELECT * FROM Customers;
