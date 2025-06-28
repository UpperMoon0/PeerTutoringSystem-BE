-- Drop and Create Database
DROP DATABASE IF EXISTS PeerTutoringSystem;
GO
CREATE DATABASE PeerTutoringSystem;
GO

USE PeerTutoringSystem;
GO

-- Create Roles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        RoleID INT PRIMARY KEY,
        RoleName NVARCHAR(50) NOT NULL UNIQUE
    );
END;
GO

-- Create Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserID UNIQUEIDENTIFIER PRIMARY KEY,
        FullName NVARCHAR(255) NOT NULL,
        FirebaseUid NVARCHAR(255) NULL,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NULL,
        DateOfBirth DATE NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Gender NVARCHAR(10) NOT NULL CHECK (Gender IN ('Male', 'Female', 'Other')),
        Hometown NVARCHAR(255) NOT NULL,
        School NVARCHAR(255) NULL,
        AvatarUrl NVARCHAR(255) NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        LastActive DATETIME NOT NULL DEFAULT GETUTCDATE(),
        IsOnline BIT NOT NULL DEFAULT 0,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Banned')),
        RoleID INT NOT NULL,
        CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
    );

    -- Thêm chỉ mục cho Status
    CREATE NONCLUSTERED INDEX IX_Users_Status ON Users(Status);
END;
GO

-- Create UserTokens Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserTokens')
BEGIN
    CREATE TABLE UserTokens (
        TokenID UNIQUEIDENTIFIER PRIMARY KEY,
        UserID UNIQUEIDENTIFIER NOT NULL,
        AccessToken NVARCHAR(MAX) NOT NULL,
        RefreshToken NVARCHAR(255) NOT NULL,
        IssuedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME NOT NULL,
        RefreshTokenExpiresAt DATETIME NOT NULL,
        IsRevoked BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_UserTokens_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );

    -- Thêm chỉ mục cho UserID
    CREATE NONCLUSTERED INDEX IX_UserTokens_UserID ON UserTokens(UserID);
END;
GO

-- Create TutorVerifications Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TutorVerifications')
BEGIN
    CREATE TABLE TutorVerifications (
        VerificationID UNIQUEIDENTIFIER PRIMARY KEY,
        UserID UNIQUEIDENTIFIER NOT NULL,
        CitizenID NVARCHAR(50) NOT NULL UNIQUE,
        StudentID NVARCHAR(50) NOT NULL UNIQUE,
        University NVARCHAR(255) NOT NULL,
        Major NVARCHAR(255) NOT NULL,
        VerificationStatus NVARCHAR(20) NOT NULL CHECK (VerificationStatus IN ('Pending', 'Approved', 'Rejected')),
        VerificationDate DATETIME NULL DEFAULT GETUTCDATE(),
        AdminNotes NVARCHAR(MAX) NULL,
        AccessLevel NVARCHAR(50) NOT NULL,
        CONSTRAINT FK_TutorVerifications_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );

    -- Thêm chỉ mục cho VerificationStatus
    CREATE NONCLUSTERED INDEX IX_TutorVerifications_VerificationStatus ON TutorVerifications(VerificationStatus);
END;
GO

-- Create Documents Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Documents')
BEGIN
    CREATE TABLE Documents (
        DocumentID UNIQUEIDENTIFIER PRIMARY KEY,
        VerificationID UNIQUEIDENTIFIER NOT NULL,
        DocumentType NVARCHAR(50) NOT NULL,
        DocumentPath NVARCHAR(255) NOT NULL,
        UploadDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        FileSize INT NOT NULL,
        AccessLevel NVARCHAR(50) NOT NULL,
        CONSTRAINT FK_Documents_TutorVerifications FOREIGN KEY (VerificationID) REFERENCES TutorVerifications(VerificationID)
    );

    -- Thêm chỉ mục cho VerificationID
    CREATE NONCLUSTERED INDEX IX_Documents_VerificationID ON Documents(VerificationID);
END;
GO

-- Create UserBio Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserBio')
BEGIN
    CREATE TABLE UserBio (
        BioID INT IDENTITY(1,1) PRIMARY KEY,
        UserID UNIQUEIDENTIFIER NOT NULL UNIQUE,
        Bio NVARCHAR(MAX) NULL,
        Experience NVARCHAR(MAX) NULL,
        HourlyRate DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        Availability NVARCHAR(MAX) NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME NULL,
        CONSTRAINT FK_UserBio_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );

    -- Thêm chỉ mục cho UserID
    CREATE NONCLUSTERED INDEX IX_UserBio_UserID ON UserBio(UserID);
END;
GO

-- Create TutorAvailabilities Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TutorAvailabilities')
BEGIN
    CREATE TABLE TutorAvailabilities (
        AvailabilityId UNIQUEIDENTIFIER PRIMARY KEY,
        TutorId UNIQUEIDENTIFIER NOT NULL,
        StartTime DATETIME NOT NULL,
        EndTime DATETIME NOT NULL,
        IsRecurring BIT NOT NULL DEFAULT 0,
        IsDailyRecurring BIT NOT NULL DEFAULT 0, -- Thêm cột IsDailyRecurring
        RecurringDay NVARCHAR(10) NULL, -- Store as string representation of DayOfWeek
        RecurrenceEndDate DATETIME NULL,
        IsBooked BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_TutorAvailabilities_Users FOREIGN KEY (TutorId) REFERENCES Users(UserID)
    );
    
    CREATE NONCLUSTERED INDEX IX_TutorAvailabilities_TutorId ON TutorAvailabilities(TutorId);
    CREATE NONCLUSTERED INDEX IX_TutorAvailabilities_TimeRange ON TutorAvailabilities(StartTime, EndTime);
    CREATE NONCLUSTERED INDEX IX_TutorAvailabilities_IsDailyRecurring ON TutorAvailabilities(IsDailyRecurring); -- Thêm chỉ mục cho IsDailyRecurring
END;
ELSE
BEGIN
    -- Nếu bảng đã tồn tại, kiểm tra và thêm cột IsDailyRecurring nếu chưa có
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TutorAvailabilities') AND name = 'IsDailyRecurring')
    BEGIN
        ALTER TABLE TutorAvailabilities
        ADD IsDailyRecurring BIT NOT NULL DEFAULT 0;

        -- Thêm chỉ mục cho cột mới
        CREATE NONCLUSTERED INDEX IX_TutorAvailabilities_IsDailyRecurring ON TutorAvailabilities(IsDailyRecurring);
    END;
END;
GO

-- Create BookingSessions Table with BookingStatus enum represented as string
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BookingSessions')
BEGIN
    CREATE TABLE BookingSessions (
        BookingId UNIQUEIDENTIFIER PRIMARY KEY,
        StudentId UNIQUEIDENTIFIER NOT NULL,
        TutorId UNIQUEIDENTIFIER NOT NULL,
        AvailabilityId UNIQUEIDENTIFIER NOT NULL,
        SessionDate DATE NOT NULL,
        StartTime DATETIME NOT NULL,
        EndTime DATETIME NOT NULL,
        SkillId UNIQUEIDENTIFIER NULL,
        Topic NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled', 'Completed','Rejected')),
        CreatedAt DATETIME NOT NULL,
        UpdatedAt DATETIME NULL,
        CONSTRAINT FK_BookingSessions_Students FOREIGN KEY (StudentId) REFERENCES Users(UserID),
        CONSTRAINT FK_BookingSessions_Tutors FOREIGN KEY (TutorId) REFERENCES Users(UserID),
        CONSTRAINT FK_BookingSessions_Availabilities FOREIGN KEY (AvailabilityId) REFERENCES TutorAvailabilities(AvailabilityId)
    );

    CREATE NONCLUSTERED INDEX IX_BookingSessions_StudentId ON BookingSessions(StudentId);
    CREATE NONCLUSTERED INDEX IX_BookingSessions_TutorId ON BookingSessions(TutorId);
    CREATE NONCLUSTERED INDEX IX_BookingSessions_Status ON BookingSessions(Status);
    CREATE NONCLUSTERED INDEX IX_BookingSessions_TimeRange ON BookingSessions(StartTime, EndTime);
END;
GO

-- Create Skills Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Skills')
BEGIN
    CREATE TABLE Skills (
        SkillID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        SkillName NVARCHAR(100) NOT NULL,
        SkillLevel NVARCHAR(50) NULL CHECK (SkillLevel IN ('Beginner', 'Elementary', 'Intermediate', 'Advanced', 'Expert')),
        Description NVARCHAR(500) NULL
    );
END;
GO

-- Nếu bảng đã tồn tại, xóa constraint cũ và thêm constraint mới
IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CHK_SkillLevel' AND parent_object_id = OBJECT_ID('Skills'))
BEGIN
    ALTER TABLE Skills
    DROP CONSTRAINT CHK_SkillLevel;
END;

ALTER TABLE Skills
ADD CONSTRAINT CHK_SkillLevel CHECK (SkillLevel IN ('Beginner', 'Elementary', 'Intermediate', 'Advanced', 'Expert'));
GO

-- Create UserSkills Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserSkills')
BEGIN
    CREATE TABLE UserSkills (
        UserSkillID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserID UNIQUEIDENTIFIER NOT NULL,
        SkillID UNIQUEIDENTIFIER NOT NULL,
        IsTutor BIT NOT NULL,
        CONSTRAINT FK_UserSkills_Users FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
        CONSTRAINT FK_UserSkills_Skills FOREIGN KEY (SkillID) REFERENCES Skills(SkillID) ON DELETE CASCADE
    );
END;
GO

-- Create Indexes
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_FirebaseUid' AND object_id = OBJECT_ID('Users'))
BEGIN
    DROP INDEX IX_Users_FirebaseUid ON Users;
END;
CREATE UNIQUE NONCLUSTERED INDEX IX_Users_FirebaseUid ON Users(FirebaseUid) WHERE FirebaseUid IS NOT NULL AND FirebaseUid != '';
CREATE NONCLUSTERED INDEX IX_TutorVerifications_CitizenID ON TutorVerifications(CitizenID);
CREATE NONCLUSTERED INDEX IX_TutorVerifications_StudentID ON TutorVerifications(StudentID);
GO

-- Create Review table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
BEGIN
    CREATE TABLE Reviews (
        ReviewID INT IDENTITY(1,1) PRIMARY KEY,
        BookingID UNIQUEIDENTIFIER NOT NULL,
        StudentID UNIQUEIDENTIFIER NOT NULL,
        TutorID UNIQUEIDENTIFIER NOT NULL,
        Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
        Comment NVARCHAR(500) NULL,
        ReviewDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Reviews_BookingSessions FOREIGN KEY (BookingID) REFERENCES BookingSessions(BookingID),
        CONSTRAINT FK_Reviews_Students FOREIGN KEY (StudentID) REFERENCES Users(UserID),
        CONSTRAINT FK_Reviews_Tutors FOREIGN KEY (TutorID) REFERENCES Users(UserID)
    );

    -- Add indexes for faster querying
    CREATE NONCLUSTERED INDEX IX_Reviews_BookingID ON Reviews(BookingID);
    CREATE NONCLUSTERED INDEX IX_Reviews_StudentID ON Reviews(StudentID);
    CREATE NONCLUSTERED INDEX IX_Reviews_TutorID ON Reviews(TutorID);
END;
GO

-- Session Management
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sessions')
BEGIN
    CREATE TABLE Sessions (
        SessionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        BookingId UNIQUEIDENTIFIER NOT NULL,
        VideoCallLink NVARCHAR(255) NULL,
        SessionNotes NVARCHAR(500) NULL,
        StartTime DATETIME NOT NULL,
        EndTime DATETIME NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME NULL,
        CONSTRAINT FK_Sessions_BookingSessions FOREIGN KEY (BookingId) REFERENCES BookingSessions(BookingId) ON DELETE CASCADE
    );

    -- Thêm chỉ mục cho BookingId
    CREATE NONCLUSTERED INDEX IX_Sessions_BookingId ON Sessions(BookingId);
END;
GO