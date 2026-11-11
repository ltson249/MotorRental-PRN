USE MotorRentalDB;
GO

-- Xóa tài khoản cũ nếu đã tồn tại
DELETE FROM dbo.Users WHERE Username IN ('admin', 'staff');

-- Thêm mới tài khoản Admin và Staff
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role, IsActive)
VALUES 
('admin', 'Admin@123', N'Quản trị viên', 'Admin', 1),
('staff', 'Staff@123', N'Nhân viên hệ thống', 'Staff', 1);

-- Kiểm tra lại danh sách
SELECT * FROM dbo.Users;