# PeerTutoringSystem Backend

Welcome to the backend of the **PeerTutoringSystem**, a platform designed to facilitate peer tutoring by managing user authentication, tutor verification, document uploads, tutor profiles, and tutoring session bookings. This project is built using **ASP.NET Core** with a layered architecture, integrating with **SQL Server** for data storage and **Firebase** for authentication.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
  - [Authentication & Authorization](#authentication--authorization)
  - [Users](#users)
  - [Tutor Verifications](#tutor-verifications)
  - [Documents](#documents)
  - [Profiles](#profiles)
  - [Tutor Availabilities](#tutor-availabilities)
  - [Bookings](#bookings)
- [Usage](#usage)
- [Testing](#testing)
- [Performance Optimization](#performance-optimization)
- [Links](#links)
- [Contributing](#contributing)
- [License](#license)

## Prerequisites
Before setting up the project, ensure you have the following installed:
- **.NET 8 SDK** (or the version compatible with the project)
- **SQL Server** (Express or full version) for the database
- **Visual Studio 2022** (or another IDE like Rider or VS Code with .NET support)
- **Firebase Account** (for authentication)
- **Postman** or **Swagger UI** (for testing API endpoints)
- **Git** (to clone the repository)

## Project Structure
The project follows a layered architecture with the following structure:

```
PeerTutoringSystem
├── PeerTutoringSystem.Api
│   ├── Controllers/                # API controllers (Auth, Users, TutorVerifications, Documents, Profiles, Bookings)
│   ├── Middleware/                 # Custom middleware (e.g., AuthorizeAdmin)
│   ├── wwwroot/documents/          # Folder for storing uploaded documents
│   ├── appsettings.json            # Configuration file
│   ├── Program.cs                  # Application entry point
│   └── serviceAccountKey.json      # Firebase service account key
├── PeerTutoringSystem.Application
│   ├── DTOs/                       # Data Transfer Objects
│   ├── Interfaces/                 # Service interfaces
│   └── Services/                   # Business logic implementation
├── PeerTutoringSystem.Domain
│   ├── Entities/                   # Domain entities (User, TutorVerification, Document, Profile, TutorAvailability, BookingSession)
│   └── Interfaces/                 # Repository interfaces
├── PeerTutoringSystem.Infrastructure
│   ├── Data/                       # DbContext (AppDbContext)
│   └── Repositories/               # Repository implementations
└── README.md                       # Project documentation
```

## Installation
Follow these steps to set up the project locally:

1. **Clone the repository**:
   ```bash
   git clone https://github.com/huy69185/PeerTutoringSystem-BE.git
   cd PeerTutoringSystem-BE
   ```

2. **Restore dependencies**:
   Open the solution in Visual Studio or run the following command in the root directory:
   ```bash
   dotnet restore
   ```

3. **Set up the database**:
   - Ensure SQL Server is running.
   - Update the connection string in `PeerTutoringSystem.Api/appsettings.json`:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=PeerTutoringSystem;Trusted_Connection=True;"
     }
     ```
   - Run migrations to create the database and tables, including the new `IsDailyRecurring` column:
     ```bash
     cd PeerTutoringSystem.Infrastructure
     dotnet ef migrations add InitialCreate --startup-project ../PeerTutoringSystem.Api
     dotnet ef migrations add AddIsDailyRecurring --startup-project ../PeerTutoringSystem.Api
     dotnet ef database update --startup-project ../PeerTutoringSystem.Api
     ```

4. **Set up Firebase**:
   - **Obtain Firebase Service Account Credentials**:
     1.  Go to the [Firebase Console](https://console.firebase.google.com/) and select your project.
     2.  Navigate to **Project settings** (gear icon) > **Service accounts** tab.
     3.  Click **Generate new private key** and then **Generate key** to download a JSON file (e.g., `your-project-name-firebase-adminsdk-xxxx.json`).
     4.  Open this JSON file with a text editor.
     5.  Copy the value of the `client_email` field for `AuthEmail`.
     6.  Copy the entire string value of the `private_key` field (including `-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n`) for `PrivateKey`.
   - **Obtain Firebase Storage Bucket Name**:
     1.  In the Firebase Console, navigate to **Storage** (under the "Build" section).
     2.  Your default storage bucket name will be displayed (e.g., `your-project-id.appspot.com` or `your-project-id.firebasestorage.app`). Copy this name.
   - **Update `appsettings.json`**:
     - Add a `Firebase` section to your `PeerTutoringSystem.Api/appsettings.json` (or `appsettings.Development.json` for local development) with the obtained values:
     ```json
     "Firebase": {
       "AuthEmail": "your-service-account-email@your-project-id.iam.gserviceaccount.com",
       "PrivateKey": "-----BEGIN PRIVATE KEY-----\nYOUR_PRIVATE_KEY_CONTENT_HERE\n-----END PRIVATE KEY-----\n",
       "BucketName": "your-firebase-storage-bucket-name.appspot.com"
     }
     ```
     - **Important**: Ensure the `PrivateKey` value is copied exactly as it appears in the JSON file, including all `\n` newline characters.

5. **Configure JWT settings**:
   - In `appsettings.json`, ensure the JWT settings are configured:
     ```json
     "Jwt": {
       "Key": "your-secure-jwt-key-here",
       "Issuer": "PeerTutoringSystem",
       "Audience": "PeerTutoringSystem"
     }
     ```
   - Replace `"your-secure-jwt-key-here"` with a strong secret key (at least 32 characters long).

## Configuration
- **Document Storage**:
  - Uploaded documents are stored in `PeerTutoringSystem.Api/wwwroot/documents/`.
  - Ensure this directory exists and is writable by the application.
  - To ignore uploaded files in Git, the `.gitignore` file includes:
    ```
    wwwroot/documents/*
    !wwwroot/documents/.gitkeep
    ```

- **CORS**:
  - The project allows all origins by default (configured in `Program.cs`). To restrict CORS, modify the policy:
    ```csharp
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.WithOrigins("http://your-frontend-url")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });
    ```

- **Time Zone Handling**:
  - All date and time values (`StartTime`, `EndTime`, `CreatedAt`, etc.) are stored and processed in **UTC** to ensure consistency.
  - For future extensibility, consider adding a `TimeZone` field to `TutorAvailabilities` or `Users` and using `TimeZoneInfo` for conversions.

## Running the Application
1. **Build the solution**:
   ```bash
   dotnet build
   ```

2. **Run the application**:
   ```bash
   cd PeerTutoringSystem.Api
   dotnet run
   ```

3. **Access the API**:
   - The API will run at `https://localhost:5001` (or the port specified in `launchSettings.json`).
   - Swagger UI is available at `https://localhost:5001/swagger` for testing endpoints (in development mode).

## API Endpoints

### Authentication & Authorization
| Method | Endpoint                     | Description                       | Authorization         |
|--------|------------------------------|-----------------------------------|-----------------------|
| POST   | `/api/auth/register`         | Register a new user (Student)     | Public                |
| POST   | `/api/auth/login`            | Login with email and password     | Public                |
| POST   | `/api/auth/login/google`     | Login with Google ID token        | Public                |
| POST   | `/api/auth/login/microsoft`  | Login with Microsoft ID token     | Public                |
| POST   | `/api/auth/refresh`          | Refresh access token              | Public                |
| POST   | `/api/auth/logout`           | Logout and invalidate token       | Authenticated (JWT)   |

### Users
| Method | Endpoint                     | Description                       | Authorization         |
|--------|------------------------------|-----------------------------------|-----------------------|
| GET    | `/api/users/{userId}`        | Get user by ID                    | Self or Admin         |
| PUT    | `/api/users/{userId}`        | Update user information           | Self or Admin         |
| DELETE | `/api/users/{userId}`        | Deactivate a user account         | Admin only            |
| POST   | `/api/users/{userId}/request-tutor` | Request to become a Tutor  | Student only          |

### Tutor Verifications
| Method | Endpoint                     | Description                       | Authorization         |
|--------|------------------------------|-----------------------------------|-----------------------|
| GET    | `/api/tutor-verifications`   | Get all tutor verification requests | Admin only          |
| GET    | `/api/tutor-verifications/{verificationId}` | Get a specific verification request | Admin or Tutor |
| PUT    | `/api/tutor-verifications/{verificationId}` | Update verification status | Admin only          |

### Documents
| Method | Endpoint                     | Description                       | Authorization         |
|--------|------------------------------|-----------------------------------|-----------------------|
| POST   | `/api/documents/upload`      | Upload a document (PDF/Word)      | Student only          |
| GET    | `/api/documents/{documentId}`| Download a document               | Admin or Tutor        |

### Profiles
| Method | Endpoint                     | Description                       | Authorization         |
|--------|------------------------------|-----------------------------------|-----------------------|
| POST   | `/api/profiles`              | Create a tutor profile            | Tutor only            |
| GET    | `/api/profiles/{profileId}`  | Get a tutor profile by ID         | Public                |
| GET    | `/api/profiles/user/{userId}`| Get a tutor profile by user ID    | Authenticated (JWT)   |
| PUT    | `/api/profiles/{profileId}`  | Update a tutor profile            | Self or Admin         |

### Tutor Availabilities
| Method | Endpoint                     | Description                       | Authorization         |
|--------|------------------------------|-----------------------------------|-----------------------|
| POST   | `/api/tutor-availability`    | Add a tutor availability slot     | Tutor only            |
| GET    | `/api/tutor-availability/tutor/{tutorId}` | Get availability slots for a tutor | Tutor, Admin, Student |
| GET    | `/api/tutor-availability/available?tutorId={tutorId}&startDate={date}&endDate={date}` | Get available slots for booking | Public                |
| DELETE | `/api/tutor-availability/{availabilityId}` | Delete an availability slot | Tutor only            |

**Notes**:
- The `/api/tutor-availability/tutor/{tutorId}` endpoint restricts Tutors to viewing only their own slots unless the user is an Admin.
- Supports both **weekly recurring** (`IsRecurring`) and **daily recurring** (`IsDailyRecurring`) availability slots.
- All times are handled in **UTC**. Future updates may include user-specific time zone support.

### Bookings
| Method | Endpoint                     | Description                       | Authorization         |
|--------|------------------------------|-----------------------------------|-----------------------|
| POST   | `/api/bookings`              | Create a booking for a slot        | Student only          |
| POST   | `/api/bookings/instant`      | Create an instant booking         | Student only          |
| GET    | `/api/bookings/{bookingId}`  | Get a booking by ID               | Student, Tutor, Admin |
| GET    | `/api/bookings/student`      | Get bookings for a student        | Student only          |
| GET    | `/api/bookings/tutor`        | Get bookings for a tutor          | Tutor only            |
| GET    | `/api/bookings/upcoming`     | Get upcoming bookings             | Student, Tutor        |
| PUT    | `/api/bookings/{bookingId}/status` | Update booking status       | Student, Tutor, Admin |

**Notes**:
- Bookings require a valid `SkillId` (checked against the `Skills` table).
- Status transitions are validated (e.g., `Pending` to `Confirmed` or `Cancelled`, but not from `Cancelled` to `Completed`).
- Only Students can cancel bookings, while Tutors can confirm or mark as completed.

#### Additional Notes:
- **Document Upload**:
  - Documents must be in PDF or Word format (`.pdf`, `.doc`, `.docx`).
  - Maximum file size: 5MB.
  - Use `/api/documents/upload` to upload documents first, then include the returned `DocumentPath` in the `/api/users/{userId}/request-tutor` request.

- **Access Control**:
  - Documents are accessible only to Admins and the Tutor who submitted the verification request.
  - Use the `/api/documents/{documentId}` endpoint to download documents securely.

- **Profile Management**:
  - Only Tutors can create and update their profiles.
  - Profiles include `Bio`, `Experience`, `HourlyRate`, `Availability`, and optionally `School` (if available in the user’s record).
  - The avatar is managed via the `/api/users/{userId}` endpoint (`AvatarUrl` field).

## Usage
1. **Register a new user**:
   - Send a `POST` request to `/api/auth/register` with:
     ```json
     {
       "Email": "student@example.com",
       "Password": "Password123!",
       "FullName": "John Doe",
       "DateOfBirth": "2000-01-01",
       "PhoneNumber": "1234567890",
       "Gender": "Male",
       "Hometown": "Hanoi"
     }
     ```
   - Receive an `AccessToken`, `RefreshToken`, `UserID`, and `Role`.

2. **Login**:
   - Send a `POST` request to `/api/auth/login` with:
     ```json
     {
       "Email": "student@example.com",
       "Password": "Password123!"
     }
     ```
   - Use the `AccessToken` in the `Authorization` header for authenticated requests: `Bearer {token}`.

3. **Upload Documents**:
   - Send a `POST` request to `/api/documents/upload` with `multipart/form-data`:
     - Field `file`: Upload a PDF or Word file.
     - Query parameter `userId`: The ID of the user uploading the document.
   - Example response:
     ```json
     {
       "DocumentPath": "/documents/12345678-1234-1234-1234-1234567890ab.pdf",
       "DocumentType": "PDF",
       "FileSize": 102400,
       "UserID": "guid-of-user"
     }
     ```

4. **Request Tutor Role**:
   - Send a `POST` request to `/api/users/{userId}/request-tutor` with:
     ```json
     {
       "CitizenID": "123456789",
       "StudentID": "STU12345",
       "University": "Example University",
       "Major": "Computer Science",
       "Documents": [
         {
           "DocumentType": "PDF",
           "DocumentPath": "/documents/12345678-1234-1234-1234-1234567890ab.pdf",
           "FileSize": 102400,
           "UserID": "guid-of-user"
         }
       ]
     }
     ```

5. **Create a Tutor Profile**:
   - After becoming a Tutor, send a `POST` request to `/api/profiles` with:
     ```json
     {
       "HourlyRate": 50000,
       "Bio": "I am a dedicated Math tutor with a passion for teaching.",
       "Experience": "3 years of tutoring experience.",
       "Availability": "{\"Monday\": \"9:00-12:00\", \"Tuesday\": \"14:00-17:00\"}"
     }
     ```
   - Example response:
     ```json
     {
       "ProfileID": 1,
       "UserID": "guid-of-tutor",
       "TutorName": "John Doe",
       "Bio": "I am a dedicated Math tutor with a passion for teaching.",
       "Experience": "3 years of tutoring experience.",
       "HourlyRate": 50000,
       "Availability": "{\"Monday\": \"9:00-12:00\", \"Tuesday\": \"14:00-17:00\"}",
       "AvatarUrl": "/avatars/tutor-avatar.jpg",
       "School": "Example University",
       "CreatedDate": "2025-05-13T12:00:00Z",
       "UpdatedDate": null
     }
     ```

6. **Add Tutor Availability**:
   - Send a `POST` request to `/api/tutor-availability` with:
     ```json
     {
       "StartTime": "2025-06-01T09:00:00Z",
       "EndTime": "2025-06-01T10:00:00Z",
       "IsRecurring": false,
       "IsDailyRecurring": true,
       "RecurrenceEndDate": "2025-12-31T23:59:59Z"
     }
     ```
   - This creates a daily recurring slot from June 1, 2025, to December 31, 2025, from 9:00 to 10:00 UTC.

7. **Create a Booking**:
   - Send a `POST` request to `/api/bookings` with:
     ```json
     {
       "TutorId": "guid-of-tutor",
       "AvailabilityId": "guid-of-availability",
       "SkillId": "guid-of-skill",
       "Topic": "Math Tutoring",
       "Description": "Need help with calculus."
     }
     ```
   - The `SkillId` must exist in the `Skills` table, and the slot must be available.

8. **Admin Actions**:
   - Use an Admin account to approve/reject tutor verification requests via `/api/tutor-verifications/{verificationId}`.

## Testing
- **Swagger UI**:
  - Run the application in development mode and navigate to `/swagger` to test endpoints interactively.
- **Postman**:
  - Import the API collection (if provided) or manually create requests based on the [API Endpoints](#api-endpoints) section.
- **Unit Tests**:
  - Use a testing framework like **xUnit** with **Moq** to test services and repositories.
  - Example: Test the `TutorAvailabilityService` to ensure daily recurring slots are generated correctly.

## Performance Optimization
- **Dynamic Slot Generation**:
  - Recurring availability slots (weekly and daily) are generated dynamically at runtime, reducing database storage requirements.
  - The `TutorAvailabilityService` uses efficient logic to create slots within the requested time range, minimizing queries.
- **Indexing**:
  - The database includes indexes on `TutorAvailabilities` (`TutorId`, `StartTime`, `EndTime`, `IsDailyRecurring`) to optimize slot retrieval.
- **Monitoring**:
  - For large datasets, monitor the performance of `GetAvailableSlotsByTutorIdAsync` and consider limiting the time range (e.g., 30 days) for recurring slots.

## Links
- **Repository**: [GitHub Repository](https://github.com/huy69185/PeerTutoringSystem-BE)
- **API Documentation**: Available via Swagger UI at `/swagger` when running the application.
- **Firebase Console**: [Firebase Console](https://console.firebase.google.com/) for managing authentication settings.
- **Database Schema**: *(Add a link to a schema diagram or documentation if available)*

## Contributing
1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -m "Add YourFeature"`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Open a Pull Request.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
