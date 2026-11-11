USE MotorRentalDB;
GO

UPDATE dbo.Users SET PasswordHash = 'Admin@123' WHERE Username = 'admin';
UPDATE dbo.Users SET PasswordHash = 'Staff@123' WHERE Username = 'staff';