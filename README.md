# PeerTutoringSystem Backend

Welcome to the backend of the **PeerTutoringSystem**, a platform designed to facilitate peer tutoring by managing user authentication, tutor verification, and document uploads. This project is built using **ASP.NET Core** with a layered architecture, integrating with **SQL Server** for data storage and **Firebase** for authentication.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
  - [Authentication & Authorization](#authentication--authorization)
  - [Tutor Verifications](#tutor-verifications)
  - [Documents](#documents)
- [Usage](#usage)
- [Testing](#testing)
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
│   ├── Controllers/                # API controllers (Auth, Users, TutorVerifications, Documents)
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
│   ├── Entities/                   # Domain entities (User, TutorVerification, Document, etc.)
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
   git clone https://github.com/your-repo/PeerTutoringSystem.git
   cd PeerTutoringSystem
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
       "DefaultConnection": "Server=localhost;Database=PeerTutoringSystemDb;Trusted_Connection=True;"
     }
     ```
   - Run migrations to create the database and tables:
     ```bash
     cd PeerTutoringSystem.Infrastructure
     dotnet ef database update --startup-project ../PeerTutoringSystem.Api
     ```

4. **Set up Firebase**:
   - Download your Firebase service account key (`serviceAccountKey.json`) from the Firebase Console.
   - Place the `serviceAccountKey.json` file in the `PeerTutoringSystem.Api` directory.
   - Update `appsettings.json` with the path to the key:
     ```json
     "Firebase": {
       "CredentialPath": "serviceAccountKey.json"
     }
     ```

5. **Configure JWT settings**:
   - In `appsettings.json`, ensure the JWT settings are configured:
     ```json
     "Jwt": {
       "Key": "your-secure-jwt-key-here",
       "Issuer": "PeerTutoringSystem",
       "Audience": "PeerTutoringSystem"
     }
     ```
   - Replace `"your-secure-jwt-key-here"` with a strong secret key.

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
| POST   | `/api/auth/refresh`          | Refresh access token              | Public                |
| POST   | `/api/auth/logout`           | Logout and invalidate token       | Authenticated (JWT)   |

### Users
| Method | Endpoint                     | Description                       | Authorization         |
|--------|------------------------------|-----------------------------------|-----------------------|
| GET    | `/api/users`                 | Get all users                     | Admin only            |
| GET    | `/api/users/{userId}`        | Get user by ID                    | Self or Admin         |
| PUT    | `/api/users/{userId}`        | Update user information           | Self or Admin         |
| DELETE | `/api/users/{userId}`        | Ban a user                        | Admin only            |
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

#### Notes:
- **Document Upload**:
  - Documents must be in PDF or Word format (`.pdf`, `.doc`, `.docx`).
  - Maximum file size: 5MB.
  - Use `/api/documents/upload` to upload documents first, then include the returned `DocumentPath` in the `/api/users/{userId}/request-tutor` request.

- **Access Control**:
  - Documents are accessible only to Admins and the Tutor who submitted the verification request.
  - Use the `/api/documents/{documentId}` endpoint to download documents securely.

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
   - Receive an `AccessToken` and `RefreshToken`.

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

5. **Admin Actions**:
   - Use an Admin account to approve/reject tutor verification requests via `/api/tutor-verifications/{verificationId}`.

## Testing
- **Swagger UI**:
  - Run the application in development mode and navigate to `/swagger` to test endpoints interactively.
- **Postman**:
  - Import the API collection (if provided) or manually create requests based on the [API Endpoints](#api-endpoints) section.

## Links
- **Repository**: [GitHub Repository](https://github.com/your-repo/PeerTutoringSystem) *(Update with your actual repository link)*
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
