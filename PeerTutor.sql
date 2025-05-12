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
        FirebaseUid NVARCHAR(255) NULL UNIQUE,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NULL,
        DateOfBirth DATE NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Gender NVARCHAR(10) NOT NULL CHECK (Gender IN ('Male', 'Female', 'Other')),
        Hometown NVARCHAR(255) NOT NULL,
        School NVARCHAR(255) NULL, -- Thêm cột School, không bắt buộc
        AvatarUrl NVARCHAR(255) NULL, -- Đường dẫn ảnh đại diện, không bắt buộc
        CreatedDate DATETIME NOT NULL,
        LastActive DATETIME NOT NULL,
        IsOnline BIT NOT NULL DEFAULT 0,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active', 'Banned')),
        RoleID INT NOT NULL,
        CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
    );

    -- Thêm chỉ mục cho Status (tùy chọn)
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

-- Create Indexes
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
CREATE NONCLUSTERED INDEX IX_Users_FirebaseUid ON Users(FirebaseUid);
CREATE NONCLUSTERED INDEX IX_TutorVerifications_CitizenID ON TutorVerifications(CitizenID);
CREATE NONCLUSTERED INDEX IX_TutorVerifications_StudentID ON TutorVerifications(StudentID);
GO