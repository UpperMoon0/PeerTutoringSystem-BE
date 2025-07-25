SET QUOTED_IDENTIFIER ON;
GO
SET ANSI_NULLS ON;
GO

USE PeerTutoringSystem;
GO

-- Initial Roles 
INSERT INTO Roles (RoleID, RoleName) VALUES
    (1, 'Student'),
    (2, 'Tutor'),
    (3, 'Admin');
GO

-- Initial Admin User 
INSERT INTO Users (
    UserID, FullName, Email, PasswordHash, DateOfBirth, PhoneNumber, 
    Gender, Hometown, CreatedDate, LastActive, IsOnline, Status, RoleID
)
VALUES (
    '4a1f2c3d-5e6f-7a8b-9c0d-1e2f3a4b5c6d', 'Admin User', 'admin@example.com', 'AQAAAAIAAYagAAAAEMIM2J9p/TTtaOBJcSwMTmnRzZdLWs+oMVkDo+fFkrW2FVI2j4aOBvMc2lsqgqDZpA==',
    '1990-01-01', '0901234567', 0, 'Hanoi', GETUTCDATE(), GETUTCDATE(),
    1, 0, 3
);
GO

-- Seed Data for Users 
-- Students
INSERT INTO Users (UserID, FullName, FirebaseUid, Email, PasswordHash, DateOfBirth, PhoneNumber, Gender, Hometown, School, AvatarUrl, CreatedDate, LastActive, IsOnline, Status, RoleID) VALUES
('1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c61', 'Nguyen Van An', NULL, 'an.nguyen@example.com', 'AQAAAAIAAYagAAAAEMIM2J9p/TTtaOBJcSwMTmnRzZdLWs+oMVkDo+fFkrW2FVI2j4aOBvMc2lsqgqDZpA==', '2002-05-10', '0912345678', 0, 'Ho Chi Minh City', 'National University', NULL, GETUTCDATE(), GETUTCDATE(), 0, 0, 1),
('1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c62', 'Tran Thi Binh', NULL, 'binh.tran@example.com', 'AQAAAAIAAYagAAAAEMIM2J9p/TTtaOBJcSwMTmnRzZdLWs+oMVkDo+fFkrW2FVI2j4aOBvMc2lsqgqDZpA==', '2003-01-15', '0912345679', 1, 'Da Nang', 'Foreign Trade University', NULL, GETUTCDATE(), GETUTCDATE(), 0, 0, 1),
('1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c63', 'Le Van Cuong', NULL, 'cuong.le@example.com', 'AQAAAAIAAYagAAAAEMIM2J9p/TTtaOBJcSwMTmnRzZdLWs+oMVkDo+fFkrW2FVI2j4aOBvMc2lsqgqDZpA==', '2001-11-20', '0912345680', 0, 'Can Tho', 'Hanoi University of Science and Technology', NULL, GETUTCDATE(), GETUTCDATE(), 0, 0, 1),
('1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c64', 'Pham Thi Dao', NULL, 'dao.pham@example.com', 'AQAAAAIAAYagAAAAEMIM2J9p/TTtaOBJcSwMTmnRzZdLWs+oMVkDo+fFkrW2FVI2j4aOBvMc2lsqgqDZpA==', '2004-03-25', '0912345681', 1, 'Hai Phong', 'RMIT University Vietnam', NULL, GETUTCDATE(), GETUTCDATE(), 0, 0, 1),
('1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c65', 'Hoang Van Duc', NULL, 'duc.hoang@example.com', 'AQAAAAIAAYagAAAAEMIM2J9p/TTtaOBJcSwMTmnRzZdLWs+oMVkDo+fFkrW2FVI2j4aOBvMc2lsqgqDZpA==', '2002-09-01', '0912345682', 0, 'Hue', 'FPT University', NULL, GETUTCDATE(), GETUTCDATE(), 0, 0, 1);

-- Tutors
INSERT INTO Users (UserID, FullName, FirebaseUid, Email, PasswordHash, DateOfBirth, PhoneNumber, Gender, Hometown, School, AvatarUrl, CreatedDate, LastActive, IsOnline, Status, RoleID) VALUES
('2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c71', 'Nguyen Thi Huong', NULL, 'huong.nguyen@example.com', 'AQAAAAIAAYagAAAAEMIM2J9p/TTtaOBJcSwMTmnRzZdLWs+oMVkDo+fFkrW2FVI2j4aOBvMc2lsqgqDZpA==', '1998-07-10', '0987654321', 1, 'Hanoi', 'Hanoi University', NULL, GETUTCDATE(), GETUTCDATE(), 1, 0, 2),
('2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c72', 'Tran Van Khoa', NULL, 'khoa.tran@example.com', 'AQAAAAIAAYagAAAAEMIM2J9p/TTtaOBJcSwMTmnRzZdLWs+oMVkDo+fFkrW2FVI2j4aOBvMc2lsqgqDZpA==', '1997-02-20', '0987654322', 0, 'Ho Chi Minh City', 'University of Science', NULL, GETUTCDATE(), GETUTCDATE(), 1, 0, 2),
('2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c73', 'Le Thi Lan', NULL, 'lan.le@example.com', 'AQAAAAIAAYagAAAAEMIM2J9p/TTtaOBJcSwMTmnRzZdLWs+oMVkDo+fFkrW2FVI2j4aOBvMc2lsqgqDZpA==', '1999-04-05', '0987654323', 1, 'Da Nang', 'Danang University of Technology', NULL, GETUTCDATE(), GETUTCDATE(), 1, 0, 2),
('2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c74', 'Pham Van Minh', NULL, 'minh.pham@example.com', 'AQAAAAIAAYagAAAAEMIM2J9p/TTtaOBJcSwMTmnRzZdLWs+oMVkDo+fFkrW2FVI2j4aOBvMc2lsqgqDZpA==', '1996-10-12', '0987654324', 0, 'Can Tho', 'Can Tho University', NULL, GETUTCDATE(), GETUTCDATE(), 1, 0, 2),
('2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c75', 'Vo Thi Ngoc', NULL, 'ngoc.vo@example.com', 'AQAAAAIAAYagAAAAEMIM2J9p/TTtaOBJcSwMTmnRzZdLWs+oMVkDo+fFkrW2FVI2j4aOBvMc2lsqgqDZpA==', '2000-01-30', '0987654325', 1, 'Nha Trang', 'Nha Trang University', NULL, GETUTCDATE(), GETUTCDATE(), 1, 0, 2);
GO

-- Seed Data for UserTokens
INSERT INTO UserTokens (TokenID, UserID, AccessToken, RefreshToken, IssuedAt, ExpiresAt, RefreshTokenExpiresAt, IsRevoked) VALUES
(NEWID(), '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c61', 'access_token_an', 'refresh_token_an', GETUTCDATE(), DATEADD(hour, 1, GETUTCDATE()), DATEADD(day, 7, GETUTCDATE()), 0),
(NEWID(), '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c71', 'access_token_huong', 'refresh_token_huong', GETUTCDATE(), DATEADD(hour, 1, GETUTCDATE()), DATEADD(day, 7, GETUTCDATE()), 0);
GO

-- Seed Data for TutorVerifications
INSERT INTO TutorVerifications (VerificationID, UserID, CitizenID, StudentID, University, Major, VerificationStatus, VerificationDate, AdminNotes, AccessLevel) VALUES
('3a4b5c6d-7e8f-9a0b-1c2d-3e4f5a6b7c81', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c71', '123456789012', 'SV001', 'Hanoi University', 'English Language', 'Approved', GETUTCDATE(), 'Verified documents', 'Public'),
('3a4b5c6d-7e8f-9a0b-1c2d-3e4f5a6b7c82', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c72', '234567890123', 'SV002', 'University of Science', 'Computer Science', 'Approved', GETUTCDATE(), 'Verified documents', 'Public');
GO

-- Seed Data for Documents
INSERT INTO Documents (DocumentID, VerificationID, DocumentType, DocumentPath, UploadDate, FileSize, AccessLevel) VALUES
(NEWID(), '3a4b5c6d-7e8f-9a0b-1c2d-3e4f5a6b7c81', 'CitizenID_Front', '/docs/huong_citizen_front.jpg', GETUTCDATE(), 500, 'Private'),
(NEWID(), '3a4b5c6d-7e8f-9a0b-1c2d-3e4f5a6b7c81', 'CitizenID_Back', '/docs/huong_citizen_back.jpg', GETUTCDATE(), 480, 'Private'),
(NEWID(), '3a4b5c6d-7e8f-9a0b-1c2d-3e4f5a6b7c81', 'StudentID_Card', '/docs/huong_student_id.jpg', GETUTCDATE(), 600, 'Private');
GO

-- Seed Data for UserBio
INSERT INTO UserBio (UserID, Bio, Experience, HourlyRate, Availability, CreatedDate, UpdatedDate) VALUES
('2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c71', 'Experienced English tutor with 5 years of teaching.', '5 years teaching English, IELTS preparation.', 150000, 'Mon-Fri 18:00-21:00', GETUTCDATE(), NULL),
('2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c72', 'Passionate Computer Science tutor, specializing in algorithms.', '3 years tutoring C++, Python, Data Structures.', 100000, 'Sat-Sun 09:00-12:00', GETUTCDATE(), NULL),
('1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c61', 'Student looking for help in Calculus.', NULL, 0.00, NULL, GETUTCDATE(), NULL);
GO

-- Seed Data for Skills
INSERT INTO Skills (SkillID, SkillName, SkillLevel, Description) VALUES
('5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c01', 'Mathematics', 'Advanced', 'High school and university level mathematics.'),
('5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c02', 'English', 'Expert', 'IELTS, TOEFL, General English.'),
('5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c03', 'Physics', 'Intermediate', 'High school physics.'),
('5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c04', 'Chemistry', 'Beginner', 'Basic chemistry concepts.'),
('5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c05', 'Computer Science', 'Advanced', 'Programming, algorithms, data structures.'),
('5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c06', 'Literature', 'Elementary', 'Vietnamese and world literature.'),
('5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c07', 'History', 'Intermediate', 'Vietnamese history.'),
('5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c08', 'Biology', 'Beginner', 'Basic biology concepts.');
GO

-- Seed Data for UserSkills
INSERT INTO UserSkills (UserSkillID, UserID, SkillID, IsTutor) VALUES
(NEWID(), '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c71', '5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c02', 1), -- Huong (Tutor) - English
(NEWID(), '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c71', '5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c06', 1), -- Huong (Tutor) - Literature
(NEWID(), '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c72', '5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c05', 1), -- Khoa (Tutor) - Computer Science
(NEWID(), '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c72', '5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c01', 1), -- Khoa (Tutor) - Mathematics
(NEWID(), '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c61', '5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c01', 0), -- An (Student) - Mathematics
(NEWID(), '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c62', '5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c02', 0); -- Binh (Student) - English
GO

-- Seed Data for TutorAvailabilities
INSERT INTO TutorAvailabilities (AvailabilityId, TutorId, StartTime, EndTime, IsRecurring, IsDailyRecurring, RecurringDay, RecurrenceEndDate, IsBooked) VALUES
('6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c11', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c71', '2025-09-01 18:00:00', '2025-09-01 19:00:00', 0, 0, NULL, NULL, 0),
('6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c12', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c71', '2025-09-02 19:00:00', '2025-09-02 20:00:00', 0, 0, NULL, NULL, 0),
('6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c13', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c72', '2025-09-05 10:00:00', '2025-09-05 11:00:00', 0, 0, NULL, NULL, 0),
('6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c14', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c72', '2025-09-06 11:00:00', '2025-09-06 12:00:00', 0, 0, NULL, NULL, 0),
('6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c15', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c71', '2025-09-07 18:00:00', '2025-09-07 19:00:00', 0, 0, NULL, NULL, 0),
('6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c16', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c72', '2025-09-08 19:00:00', '2025-09-08 20:00:00', 0, 0, NULL, NULL, 0),
('6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c17', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c73', '2025-09-09 10:00:00', '2025-09-09 11:00:00', 0, 0, NULL, NULL, 0),
('6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c18', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c74', '2025-09-10 11:00:00', '2025-09-10 12:00:00', 0, 0, NULL, NULL, 0),
('6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c19', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c75', '2025-09-11 14:00:00', '2025-09-11 15:00:00', 0, 0, NULL, NULL, 0);
GO

-- Seed Data for BookingSessions
INSERT INTO BookingSessions (BookingId, StudentId, TutorId, AvailabilityId, SessionDate, StartTime, EndTime, SkillId, Topic, Description, Status, CreatedAt, UpdatedAt) VALUES
('7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c21', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c61', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c72', '6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c13', '2025-09-05', '2025-09-05 10:00:00', '2025-09-05 11:00:00', '5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c01', 'Calculus I', 'Need help with derivatives.', 1, GETUTCDATE(), NULL),
('7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c22', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c62', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c71', '6a7b8c9d-0e1f-2a3b-4c5d-6e7f8a9b0c11', '2025-09-01', '2025-09-01 18:00:00', '2025-09-01 19:00:00', '5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c02', 'IELTS Speaking', 'Practice for IELTS speaking test.', 3, GETUTCDATE(), GETUTCDATE());
GO

-- Seed Data for Reviews
INSERT INTO Reviews (BookingID, StudentID, TutorID, Rating, Comment, ReviewDate) VALUES
('7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c22', '1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c62', '2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c71', 5, 'Excellent tutor, very helpful and patient!', GETUTCDATE());
GO

-- Seed Data for Sessions
INSERT INTO Sessions (SessionId, BookingId, VideoCallLink, SessionNotes, StartTime, EndTime, CreatedAt, UpdatedAt) VALUES
(NEWID(), '7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c22', 'https://meet.google.com/abc-defg-hij', 'Covered IELTS speaking part 1 and 2.', '2025-09-01 18:00:00', '2025-09-01 19:00:00', GETUTCDATE(), NULL);
GO

-- Seed Data for Payments
INSERT INTO Payments (Id, BookingId, TransactionId, Amount, Currency, Description, Status, PaymentUrl, CreatedAt, UpdatedAt) VALUES
(NEWID(), '7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c22', 'txn_1234567890', 150000.00, 'VND', 'Payment for IELTS Speaking session', 'Success', 'http://example.com/payment/success', GETUTCDATE(), NULL);
GO