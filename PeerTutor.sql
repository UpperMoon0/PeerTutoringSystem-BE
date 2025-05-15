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

-- Create Indexes
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_FirebaseUid' AND object_id = OBJECT_ID('Users'))
BEGIN
    DROP INDEX IX_Users_FirebaseUid ON Users;
END
CREATE UNIQUE NONCLUSTERED INDEX IX_Users_FirebaseUid ON Users(FirebaseUid) WHERE FirebaseUid IS NOT NULL AND FirebaseUid != '';
CREATE NONCLUSTERED INDEX IX_TutorVerifications_CitizenID ON TutorVerifications(CitizenID);
CREATE NONCLUSTERED INDEX IX_TutorVerifications_StudentID ON TutorVerifications(StudentID);
GO

INSERT INTO Users (
    UserID, FullName, Email, PasswordHash, DateOfBirth, PhoneNumber, 
    Gender, Hometown, CreatedDate, LastActive, IsOnline, Status, RoleID
)
VALUES (
    NEWID(), 'Admin User', 'admin@example.com', 'AQAAAAIAAYagAAAAEBu/GxeoOcRJL1/6fxnjfarakRdfsAj/7K5s0ne2VbAgAPflGYuWWVjKGlEbKpNpCQ==', 
    '1990-01-01', '1234567890', 'Male', 'Hanoi', GETUTCDATE(), GETUTCDATE(), 
    1, 'Active', 3
);