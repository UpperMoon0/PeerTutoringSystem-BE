USE PeerTutoringSystem;
GO

-- Initial Roles (from original script)
INSERT INTO Roles (RoleID, RoleName) VALUES
    (1, 'Student'),
    (2, 'Tutor'),
    (3, 'Admin');
GO

-- Initial Admin User (from original script)
INSERT INTO Users (
    UserID, FullName, Email, PasswordHash, DateOfBirth, PhoneNumber, 
    Gender, Hometown, CreatedDate, LastActive, IsOnline, Status, RoleID
)
VALUES (
    'A0000000-0000-0000-0000-000000000001', 'Admin User', 'admin@example.com', 'AQAAAAIAAYagAAAAEBu/GxeoOcRJL1/6fxnjfarakRdfsAj/7K5s0ne2VbAgAPflGYuWWVjKGlEbKpNpCQ==', 
    '1990-01-01', '0901234567', 'Male', 'Hanoi', GETUTCDATE(), GETUTCDATE(), 
    1, 'Active', 3
);
GO

-- Seed Data for Users (Vietnamese names, various roles)
-- Students
INSERT INTO Users (UserID, FullName, FirebaseUid, Email, PasswordHash, DateOfBirth, PhoneNumber, Gender, Hometown, School, AvatarUrl, CreatedDate, LastActive, IsOnline, Status, RoleID) VALUES
('B0000000-0000-0000-0000-000000000001', 'Nguyen Van An', NULL, 'an.nguyen@example.com', 'hashedpassword1', '2002-05-10', '0912345678', 'Male', 'Ho Chi Minh City', 'National University', NULL, GETUTCDATE(), GETUTCDATE(), 0, 'Active', 1),
('B0000000-0000-0000-0000-000000000002', 'Tran Thi Binh', NULL, 'binh.tran@example.com', 'hashedpassword2', '2003-01-15', '0912345679', 'Female', 'Da Nang', 'Foreign Trade University', NULL, GETUTCDATE(), GETUTCDATE(), 0, 'Active', 1),
('B0000000-0000-0000-0000-000000000003', 'Le Van Cuong', NULL, 'cuong.le@example.com', 'hashedpassword3', '2001-11-20', '0912345680', 'Male', 'Can Tho', 'Hanoi University of Science and Technology', NULL, GETUTCDATE(), GETUTCDATE(), 0, 'Active', 1),
('B0000000-0000-0000-0000-000000000004', 'Pham Thi Dao', NULL, 'dao.pham@example.com', 'hashedpassword4', '2004-03-25', '0912345681', 'Female', 'Hai Phong', 'RMIT University Vietnam', NULL, GETUTCDATE(), GETUTCDATE(), 0, 'Active', 1),
('B0000000-0000-0000-0000-000000000005', 'Hoang Van Duc', NULL, 'duc.hoang@example.com', 'hashedpassword5', '2002-09-01', '0912345682', 'Male', 'Hue', 'FPT University', NULL, GETUTCDATE(), GETUTCDATE(), 0, 'Active', 1);

-- Tutors
INSERT INTO Users (UserID, FullName, FirebaseUid, Email, PasswordHash, DateOfBirth, PhoneNumber, Gender, Hometown, School, AvatarUrl, CreatedDate, LastActive, IsOnline, Status, RoleID) VALUES
('C0000000-0000-0000-0000-000000000001', 'Nguyen Thi Huong', NULL, 'huong.nguyen@example.com', 'hashedpassword6', '1998-07-10', '0987654321', 'Female', 'Hanoi', 'Hanoi University', NULL, GETUTCDATE(), GETUTCDATE(), 1, 'Active', 2),
('C0000000-0000-0000-0000-000000000002', 'Tran Van Khoa', NULL, 'khoa.tran@example.com', 'hashedpassword7', '1997-02-20', '0987654322', 'Male', 'Ho Chi Minh City', 'University of Science', NULL, GETUTCDATE(), GETUTCDATE(), 1, 'Active', 2),
('C0000000-0000-0000-0000-000000000003', 'Le Thi Lan', NULL, 'lan.le@example.com', 'hashedpassword8', '1999-04-05', '0987654323', 'Female', 'Da Nang', 'Danang University of Technology', NULL, GETUTCDATE(), GETUTCDATE(), 1, 'Active', 2),
('C0000000-0000-0000-0000-000000000004', 'Pham Van Minh', NULL, 'minh.pham@example.com', 'hashedpassword9', '1996-10-12', '0987654324', 'Male', 'Can Tho', 'Can Tho University', NULL, GETUTCDATE(), GETUTCDATE(), 1, 'Active', 2),
('C0000000-0000-0000-0000-000000000005', 'Vo Thi Ngoc', NULL, 'ngoc.vo@example.com', 'hashedpassword10', '2000-01-30', '0987654325', 'Female', 'Nha Trang', 'Nha Trang University', NULL, GETUTCDATE(), GETUTCDATE(), 1, 'Active', 2);
GO

-- Seed Data for UserTokens
INSERT INTO UserTokens (TokenID, UserID, AccessToken, RefreshToken, IssuedAt, ExpiresAt, RefreshTokenExpiresAt, IsRevoked) VALUES
(NEWID(), 'B0000000-0000-0000-0000-000000000001', 'access_token_an', 'refresh_token_an', GETUTCDATE(), DATEADD(hour, 1, GETUTCDATE()), DATEADD(day, 7, GETUTCDATE()), 0),
(NEWID(), 'C0000000-0000-0000-0000-000000000001', 'access_token_huong', 'refresh_token_huong', GETUTCDATE(), DATEADD(hour, 1, GETUTCDATE()), DATEADD(day, 7, GETUTCDATE()), 0);
GO

-- Seed Data for TutorVerifications
INSERT INTO TutorVerifications (VerificationID, UserID, CitizenID, StudentID, University, Major, VerificationStatus, VerificationDate, AdminNotes, AccessLevel) VALUES
('D0000000-0000-0000-0000-000000000001', 'C0000000-0000-0000-0000-000000000001', '123456789012', 'SV001', 'Hanoi University', 'English Language', 'Approved', GETUTCDATE(), 'Verified documents', 'Public'),
('D0000000-0000-0000-0000-000000000002', 'C0000000-0000-0000-0000-000000000002', '234567890123', 'SV002', 'University of Science', 'Computer Science', 'Approved', GETUTCDATE(), 'Verified documents', 'Public');
GO

-- Seed Data for Documents
INSERT INTO Documents (DocumentID, VerificationID, DocumentType, DocumentPath, UploadDate, FileSize, AccessLevel) VALUES
(NEWID(), 'D0000000-0000-0000-0000-000000000001', 'CitizenID_Front', '/docs/huong_citizen_front.jpg', GETUTCDATE(), 500, 'Private'),
(NEWID(), 'D0000000-0000-0000-0000-000000000001', 'CitizenID_Back', '/docs/huong_citizen_back.jpg', GETUTCDATE(), 480, 'Private'),
(NEWID(), 'D0000000-0000-0000-0000-000000000001', 'StudentID_Card', '/docs/huong_student_id.jpg', GETUTCDATE(), 600, 'Private');
GO

-- Seed Data for UserBio
INSERT INTO UserBio (UserID, Bio, Experience, HourlyRate, Availability, CreatedDate, UpdatedDate) VALUES
('C0000000-0000-0000-0000-000000000001', 'Experienced English tutor with 5 years of teaching.', '5 years teaching English, IELTS preparation.', 150000.00, 'Mon-Fri 18:00-21:00', GETUTCDATE(), NULL),
('C0000000-0000-0000-0000-000000000002', 'Passionate Computer Science tutor, specializing in algorithms.', '3 years tutoring C++, Python, Data Structures.', 200000.00, 'Sat-Sun 09:00-12:00', GETUTCDATE(), NULL),
('B0000000-0000-0000-0000-000000000001', 'Student looking for help in Calculus.', NULL, 0.00, NULL, GETUTCDATE(), NULL);
GO

-- Seed Data for Skills
INSERT INTO Skills (SkillID, SkillName, SkillLevel, Description) VALUES
('E0000000-0000-0000-0000-000000000001', 'Mathematics', 'Advanced', 'High school and university level mathematics.'),
('E0000000-0000-0000-0000-000000000002', 'English', 'Expert', 'IELTS, TOEFL, General English.'),
('E0000000-0000-0000-0000-000000000003', 'Physics', 'Intermediate', 'High school physics.'),
('E0000000-0000-0000-0000-000000000004', 'Chemistry', 'Beginner', 'Basic chemistry concepts.'),
('E0000000-0000-0000-0000-000000000005', 'Computer Science', 'Advanced', 'Programming, algorithms, data structures.'),
('E0000000-0000-0000-0000-000000000006', 'Literature', 'Elementary', 'Vietnamese and world literature.'),
('E0000000-0000-0000-0000-000000000007', 'History', 'Intermediate', 'Vietnamese history.'),
('E0000000-0000-0000-0000-000000000008', 'Biology', 'Beginner', 'Basic biology concepts.');
GO

-- Seed Data for UserSkills
INSERT INTO UserSkills (UserSkillID, UserID, SkillID, IsTutor) VALUES
(NEWID(), 'C0000000-0000-0000-0000-000000000001', 'E0000000-0000-0000-0000-000000000002', 1), -- Huong (Tutor) - English
(NEWID(), 'C0000000-0000-0000-0000-000000000001', 'E0000000-0000-0000-0000-000000000006', 1), -- Huong (Tutor) - Literature
(NEWID(), 'C0000000-0000-0000-0000-000000000002', 'E0000000-0000-0000-0000-000000000005', 1), -- Khoa (Tutor) - Computer Science
(NEWID(), 'C0000000-0000-0000-0000-000000000002', 'E0000000-0000-0000-0000-000000000001', 1), -- Khoa (Tutor) - Mathematics
(NEWID(), 'B0000000-0000-0000-0000-000000000001', 'E0000000-0000-0000-0000-000000000001', 0), -- An (Student) - Mathematics
(NEWID(), 'B0000000-0000-0000-0000-000000000002', 'E0000000-0000-0000-0000-000000000002', 0); -- Binh (Student) - English
GO

-- Seed Data for TutorAvailabilities
INSERT INTO TutorAvailabilities (AvailabilityId, TutorId, StartTime, EndTime, IsRecurring, IsDailyRecurring, RecurringDay, RecurrenceEndDate, IsBooked) VALUES
('F0000000-0000-0000-0000-000000000001', 'C0000000-0000-0000-0000-000000000001', '2025-07-01 18:00:00', '2025-07-01 19:00:00', 0, 0, NULL, NULL, 0),
('F0000000-0000-0000-0000-000000000002', 'C0000000-0000-0000-0000-000000000001', '2025-07-02 19:00:00', '2025-07-02 20:00:00', 0, 0, NULL, NULL, 0),
('F0000000-0000-0000-0000-000000000003', 'C0000000-0000-0000-0000-000000000002', '2025-07-05 10:00:00', '2025-07-05 11:00:00', 0, 0, NULL, NULL, 0),
('F0000000-0000-0000-0000-000000000004', 'C0000000-0000-0000-0000-000000000002', '2025-07-06 11:00:00', '2025-07-06 12:00:00', 0, 0, NULL, NULL, 0);
GO

-- Seed Data for BookingSessions
INSERT INTO BookingSessions (BookingId, StudentId, TutorId, AvailabilityId, SessionDate, StartTime, EndTime, SkillId, Topic, Description, Status, CreatedAt, UpdatedAt) VALUES
('G0000000-0000-0000-0000-000000000001', 'B0000000-0000-0000-0000-000000000001', 'C0000000-0000-0000-0000-000000000002', 'F0000000-0000-0000-0000-000000000003', '2025-07-05', '2025-07-05 10:00:00', '2025-07-05 11:00:00', 'E0000000-0000-0000-0000-000000000001', 'Calculus I', 'Need help with derivatives.', 'Confirmed', GETUTCDATE(), NULL),
('G0000000-0000-0000-0000-000000000002', 'B0000000-0000-0000-0000-000000000002', 'C0000000-0000-0000-0000-000000000001', 'F0000000-0000-0000-0000-000000000001', '2025-07-01', '2025-07-01 18:00:00', '2025-07-01 19:00:00', 'E0000000-0000-0000-0000-000000000002', 'IELTS Speaking', 'Practice for IELTS speaking test.', 'Completed', GETUTCDATE(), GETUTCDATE());
GO

-- Seed Data for Reviews
INSERT INTO Reviews (BookingID, StudentID, TutorID, Rating, Comment, ReviewDate) VALUES
('G0000000-0000-0000-0000-000000000002', 'B0000000-0000-0000-0000-000000000002', 'C0000000-0000-0000-0000-000000000001', 5, 'Excellent tutor, very helpful and patient!', GETUTCDATE());
GO

-- Seed Data for Sessions
INSERT INTO Sessions (SessionId, BookingId, VideoCallLink, SessionNotes, StartTime, EndTime, CreatedAt, UpdatedAt) VALUES
(NEWID(), 'G0000000-0000-0000-0000-000000000002', 'https://meet.google.com/abc-defg-hij', 'Covered IELTS speaking part 1 and 2.', '2025-07-01 18:00:00', '2025-07-01 19:00:00', GETUTCDATE(), NULL);
GO