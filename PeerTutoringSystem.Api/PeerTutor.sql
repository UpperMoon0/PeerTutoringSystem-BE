-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PeerTutoringSystem')
BEGIN
    CREATE DATABASE PeerTutoringSystem;
END;
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

    -- Insert initial roles
    INSERT INTO Roles (RoleID, RoleName) VALUES
        (1, 'Student'),
        (2, 'Tutor'),
        (3, 'Admin');
END;
GO

-- Create Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserID UNIQUEIDENTIFIER PRIMARY KEY,
        AnonymousName NVARCHAR(255) NOT NULL,
        FirebaseUid NVARCHAR(255) NULL UNIQUE,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NULL,
        DateOfBirth DATE NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Gender NVARCHAR(10) NOT NULL CHECK (Gender IN ('Male', 'Female', 'Other')),
        Hometown NVARCHAR(255) NOT NULL,
        AvatarUrl NVARCHAR(255) NULL,
        CreatedDate DATETIME NOT NULL,
        LastActive DATETIME NOT NULL,
        IsOnline BIT NOT NULL DEFAULT 0,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Banned')),
        RoleID INT NOT NULL,
        CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
    );
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
        IssuedAt DATETIME NOT NULL,
        ExpiresAt DATETIME NOT NULL,
        IsRevoked BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_UserTokens_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
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
        VerificationDate DATETIME NULL,
        AdminNotes NVARCHAR(MAX) NULL,
        AccessLevel NVARCHAR(50) NOT NULL,
        CONSTRAINT FK_TutorVerifications_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
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
        UploadDate DATETIME NOT NULL,
        FileSize INT NOT NULL,
        AccessLevel NVARCHAR(50) NOT NULL,
        CONSTRAINT FK_Documents_TutorVerifications FOREIGN KEY (VerificationID) REFERENCES TutorVerifications(VerificationID)
    );
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
        RecurringDay NVARCHAR(10) NULL, -- Store as string representation of DayOfWeek
        RecurrenceEndDate DATETIME NULL,
        IsBooked BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_TutorAvailabilities_Users FOREIGN KEY (TutorId) REFERENCES Users(UserID)
    );
    
    CREATE NONCLUSTERED INDEX IX_TutorAvailabilities_TutorId ON TutorAvailabilities(TutorId);
    CREATE NONCLUSTERED INDEX IX_TutorAvailabilities_TimeRange ON TutorAvailabilities(StartTime, EndTime);
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
        Status NVARCHAR(20) NOT NULL CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled', 'Completed')),
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

-- Create Skills Table if not already created (for completeness)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Skills')
BEGIN
    CREATE TABLE Skills (
        SkillID UNIQUEIDENTIFIER PRIMARY KEY,
        SkillName NVARCHAR(100) NOT NULL,
        SkillLevel NVARCHAR(50) NULL,
        Description NVARCHAR(500) NULL
    );
    
    CREATE UNIQUE INDEX IX_Skills_Name ON Skills(SkillName);
END;
GO

-- Create UserSkills Table if not already created (for completeness)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserSkills')
BEGIN
    CREATE TABLE UserSkills (
        UserSkillID UNIQUEIDENTIFIER PRIMARY KEY,
        UserID UNIQUEIDENTIFIER NOT NULL,
        SkillID UNIQUEIDENTIFIER NOT NULL,
        IsTutor BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_UserSkills_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
        CONSTRAINT FK_UserSkills_Skills FOREIGN KEY (SkillID) REFERENCES Skills(SkillID)
    );
    
    CREATE NONCLUSTERED INDEX IX_UserSkills_UserID ON UserSkills(UserID);
    CREATE NONCLUSTERED INDEX IX_UserSkills_SkillID ON UserSkills(SkillID);
END;
GO

-- Create Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_FirebaseUid' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_FirebaseUid ON Users(FirebaseUid);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TutorVerifications_CitizenID' AND object_id = OBJECT_ID('TutorVerifications'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TutorVerifications_CitizenID ON TutorVerifications(CitizenID);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TutorVerifications_StudentID' AND object_id = OBJECT_ID('TutorVerifications'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TutorVerifications_StudentID ON TutorVerifications(StudentID);
END;
GO
