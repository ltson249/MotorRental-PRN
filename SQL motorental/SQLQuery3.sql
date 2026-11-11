USE MotorRentalDB;
GO

-- Xóa tài khoản cũ nếu có để tránh trùng lặp
DELETE FROM dbo.Users WHERE Username IN ('admin', 'staff');

-- Thêm tài khoản Admin và Staff
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role, IsActive)
VALUES 
('admin', 'Admin@123', N'Quản trị viên', 'Admin', 1),
('staff', 'Staff@123', N'Nhân viên hệ thống', 'Staff', 1);