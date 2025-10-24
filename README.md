# SoNice - C# .NET 8 Clean Architecture

This is a complete migration of the ExpressJS SoNice e-commerce project to C# .NET 8 using Clean Architecture principles, Repository pattern, and Unit of Work pattern.

## Project Structure

The solution follows Clean Architecture with the following layers:

```
SoNice_C#/
├── SoNice.sln                    # Solution file
├── SoNice.Domain/                # Domain Layer (Core Business Logic)
│   ├── Entities/                 # Domain entities (POCOs)
│   ├── Enums/                    # Domain enumerations
│   └── Interfaces/               # Repository interfaces
├── SoNice.Application/           # Application Layer (Business Logic)
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Interfaces/               # Service interfaces
│   ├── Services/                 # Service implementations
│   └── Validators/               # FluentValidation rules
├── SoNice.Infrastructure/        # Infrastructure Layer (Data Access)
│   ├── Data/                     # Database context
│   ├── Repositories/             # Repository implementations
│   └── DependencyInjection.cs    # DI configuration
└── SoNice.Api/                   # Presentation Layer (Web API)
    ├── Controllers/              # API controllers
    ├── Middleware/               # Custom middleware
    ├── Program.cs                # Application startup
    └── appsettings.json          # Configuration
```

## Features Migrated

### User Management
- ✅ User registration with email verification
- ✅ User login (email/phone + password)
- ✅ Google OAuth login (structure ready)
- ✅ Password reset with OTP
- ✅ User profile management
- ✅ Admin user management
- ✅ Account ban/unban functionality
- ✅ User statistics

### Authentication & Authorization
- ✅ JWT-based authentication
- ✅ Role-based authorization (Customer, Admin)
- ✅ Token validation middleware
- ✅ Rate limiting for login attempts

### Database
- ✅ MongoDB integration with official C# driver
- ✅ Repository pattern implementation
- ✅ Unit of Work pattern
- ✅ Connection retry logic

### API Features
- ✅ RESTful API endpoints
- ✅ Swagger/OpenAPI documentation
- ✅ Global error handling
- ✅ Request validation with FluentValidation
- ✅ CORS configuration
- ✅ Health check endpoint

## Getting Started

### Prerequisites
- .NET 8 SDK
- MongoDB (local or cloud instance)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd SoNice_C#
   ```

2. **Configure MongoDB**
   - Install MongoDB locally or use MongoDB Atlas
   - Update connection string in `appsettings.json`

3. **Configure JWT Settings**
   - Update JWT secret, issuer, and audience in `appsettings.json`
   - Generate a secure secret key (at least 32 characters)

4. **Configure Email Settings** (Optional)
   - Update SMTP settings in `appsettings.json` for email functionality

5. **Run the application**
   ```bash
   dotnet run --project SoNice.Api
   ```

6. **Access the API**
   - API: `https://localhost:7000`
   - Swagger UI: `https://localhost:7000/api-docs`
   - Health Check: `https://localhost:7000/health`

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/loginGoogle` - Google OAuth login
- `POST /api/auth/verify-email` - Verify email
- `POST /api/auth/logout` - Logout user

### User Management
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/current` - Get current user
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users` - Update current user
- `DELETE /api/users/{id}` - Delete user (Admin only)
- `POST /api/users/forgotPassword` - Send password reset email
- `POST /api/users/verify-otp` - Verify OTP
- `POST /api/users/resetPassword` - Reset password
- `PUT /api/users/changePassword/{id}` - Change password
- `GET /api/users/statisticsAccount` - Get account statistics (Admin only)
- `GET /api/users/searchAccountByEmail` - Search users by email (Admin only)
- `PATCH /api/users/banAccountByAdmin/{id}` - Ban account (Admin only)
- `PATCH /api/users/unBanAccountByAdmin/{id}` - Unban account (Admin only)

## Architecture Benefits

### Clean Architecture
- **Dependency Inversion**: Dependencies point inward toward the domain
- **Separation of Concerns**: Each layer has a single responsibility
- **Testability**: Easy to unit test with dependency injection
- **Maintainability**: Changes in one layer don't affect others

### Repository Pattern
- **Data Access Abstraction**: Business logic doesn't depend on data access implementation
- **Testability**: Easy to mock repositories for unit testing
- **Flexibility**: Can switch between different data sources

### Unit of Work Pattern
- **Transaction Management**: Ensures data consistency
- **Performance**: Reduces database round trips
- **Atomicity**: All operations succeed or fail together

## Reusability & Template Structure

This project is designed as a reusable template:

### To Remove User Feature
1. Delete User-related files:
   - `SoNice.Domain/Entities/User.cs`
   - `SoNice.Domain/Interfaces/IUserRepository.cs`
   - `SoNice.Application/DTOs/UserDtos.cs`
   - `SoNice.Application/Interfaces/IUserService.cs`
   - `SoNice.Application/Services/UserService.cs`
   - `SoNice.Application/Validators/UserValidators.cs`
   - `SoNice.Infrastructure/Repositories/UserRepository.cs`
   - `SoNice.Api/Controllers/AuthController.cs`
   - `SoNice.Api/Controllers/UserController.cs`

2. Update dependency injection in `SoNice.Infrastructure/DependencyInjection.cs`

3. The core infrastructure (Repository, UnitOfWork, MongoDB context) remains intact for new features

### To Add New Feature
1. Create domain entity in `SoNice.Domain/Entities/`
2. Create repository interface in `SoNice.Domain/Interfaces/`
3. Create DTOs in `SoNice.Application/DTOs/`
4. Create service interface in `SoNice.Application/Interfaces/`
5. Implement service in `SoNice.Application/Services/`
6. Create validators in `SoNice.Application/Validators/`
7. Implement repository in `SoNice.Infrastructure/Repositories/`
8. Create controller in `SoNice.Api/Controllers/`
9. Register services in `SoNice.Infrastructure/DependencyInjection.cs`

## Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Set to Development/Production
- `CONNECTION_STRING` - MongoDB connection string
- `JWT_SECRET` - JWT signing key
- `EMAIL_USERNAME` - SMTP username
- `EMAIL_PASSWORD` - SMTP password

### appsettings.json
```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "DatabaseName": "SoNice"
  },
  "Jwt": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "SoNice",
    "Audience": "SoNiceUsers",
    "ExpiryInDays": 1
  }
}
```

## Testing

The project is structured to support comprehensive testing:

- **Unit Tests**: Test individual services and repositories
- **Integration Tests**: Test API endpoints with test database
- **Repository Tests**: Test data access layer with MongoDB

## Deployment

### Docker Support
The project can be containerized with Docker:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SoNice.Api/SoNice.Api.csproj", "SoNice.Api/"]
COPY ["SoNice.Application/SoNice.Application.csproj", "SoNice.Application/"]
COPY ["SoNice.Infrastructure/SoNice.Infrastructure.csproj", "SoNice.Infrastructure/"]
COPY ["SoNice.Domain/SoNice.Domain.csproj", "SoNice.Domain/"]
RUN dotnet restore "SoNice.Api/SoNice.Api.csproj"
COPY . .
WORKDIR "/src/SoNice.Api"
RUN dotnet build "SoNice.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SoNice.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoNice.Api.dll"]
```

### Production Considerations
- Use environment variables for sensitive configuration
- Implement proper logging and monitoring
- Set up health checks and metrics
- Configure reverse proxy (nginx/Apache)
- Use HTTPS in production
- Implement proper backup strategies for MongoDB

## Contributing

1. Follow Clean Architecture principles
2. Write unit tests for new features
3. Update documentation
4. Follow C# coding conventions
5. Use meaningful commit messages

## License

This project is licensed under the MIT License.
