## 0.0.1
API has two endpoints each be able to deal with post requests entailing authentication and registration. 

## 0.0.2
API now uses API Keys for authentication, which is done by using middleware. A unnecessary assembly was removed from the DatabaseHelper.cs. A method was created in the Authentication.cs to generate API Keys. This method is currently unused but will be used in the future. 

## 0.0.3
Token Bucket rate limiting has been configured in the program.cs. Changes has been made to various files to support this. Currently the rate limiting has only been applied to the Authentication endpoint. 

## 0.0.4
Token bucket configuration readability and maintainability has been improved. Also one the rate limit has been succeeded the correct error code is returned. 

## 0.0.5
Various changes have been made to improve code readability and maintainability. Also error handling has been added to various methods. 

## 0.0.6
Dependency Injection is now used to query the database. The relevant scripts have been updated to support this. 

## 0.0.7
API key header has been updated. 

## 0.1.0
Various additional methods, classes and scripts have been created to support JWT Tokens for the API. Also multiple methods have been redone to improve efficiency. Authentication endpoint has changed due to the JWT changes. Additional validations have been added to models. Environments variables are able to located through the configuration file.

## 0.1.1
Removed the .http file due to avoid a potential leak if sensitive information is not removed before pushing to repo. Correct changes suggested by CodeQL on Github such as preventing logging injection. As well providing safeguards to user controlled methods. Added a Misc.cs script to store various utility methods.

## 0.1.2
Corrected additional changes from CodeQL, refresh tokens aren't provided as plaintext direct in the json response. Instead refresh token are now provided in a cookie.  

## 0.1.3
### Changed
- Created a Dockerfile to utilize a multi-stage build for the TMS_API application.
- Introduced a build stage using the .NET SDK image (mcr.microsoft.com/dotnet/sdk:8.0) for restoring dependencies and publishing the application.
- Added a runtime stage using the .NET ASP.NET image (mcr.microsoft.com/dotnet/aspnet:8.0) to run the application.
- Installed the New Relic agent during the runtime stage to monitor application performance.
- Configured environment variables for New Relic profiling and application binding.

### Notes
- The multi-stage build optimizes image size by separating build and runtime dependencies.
- Installing the New Relic agent allows for better monitoring and performance tracking of the application.
- This update enables application performance monitoring.
- The DockerFile won't be used in a production environment due to secrets being passed in ARG.

## 0.1.4
### Added
- A .dockerignore file was created to specify which file types and directories should be excluded during the Docker build process. This helps to streamline the build by preventing unnecessary files from being included, ultimately reducing the image size and improving build times.

###  Fixed
- Resolved SQL Server connectivity issues by establishing a network bridge, ensuring that the necessary communication channels are available between the API and SQL Server. Additionally, the .env file was excluded from the build process to maintain security and avoid exposing sensitive configurations.
-  Updated the URL from "http://localhost:5188" to "http://*:5188" to facilitate port binding for the Docker container, allowing it to accept requests from external sources rather than just localhost.
- Modified the loading mechanism for the .env file, ensuring it is only loaded when the API__KEY environment variable is not found. This change aims to streamline configuration management, with plans for a more dynamic and flexible solution in a future update.

### Removed
- The New Relic agent was removed from the Dockerfile.

### Notes
- The new relic agent was removed to facilitate easier debugging and manual performance improvements, rather than depending on third-party software.

## 0.1.5
### Added
-  Added Endpoint Performance Metrics section to the [README](README.MD)

### Notes
- Endpoint Performance Metric was added to track and monitor performance for each endpoints. Additional changes are expected to added in the future. 

## 0.1.6
### Added
- Updated the README.MD with a new section in the roadmap addressing improvements to the API's response time.

### Fixed
- Implemented ArrayPool to reduce repeated array creation and enhance memory efficiency.
- Optimized performance by converting multiple method-level instances to private static readonly properties.

### Notes
- Adjustments in [AuthenticationController.cs](./Controllers/AuthenticationController.cs) and [RegistrationController.cs](./Controllers/RegistrationController.cs) were made to accommodate the changes within [Authentication.cs](./Utilities/Authentication.cs).
- The project now uses connection pooling for better resource management.

## 0.2.0
### Added
- Included `tempkey.jwk` in `.gitignore` to prevent exposure of sensitive cryptographic keys.
- Created an [Attributes](./Attributes/) directory for custom attribute storage.
- Added [NoWhiteSpace.cs](./Attributes/NoWhiteSpace.cs) to validate that received values are not null, empty, or composed solely of white-space characters. This attribute was applied to properties in [AuthenticationModel.cs](./Models/AuthenticationModel.cs).
- Integrated ASP.NET Core Identity for user management and authentication in [AuthenticationController.cs](./Controllers/AuthenticationController.cs) and [RegistrationController.cs](./Controllers/RegistrationController.cs).
- Added configuration files for Duende Identity Server to the [configuration](./Configurations/) folder.
- Implemented two methods in [databaseHelper.cs](./Utilities/DatabaseHelper.cs) for user authentication using credentials and refresh token verification.
- Added [token.cs](./Utilities/token.cs) to facilitate communication with the Identity Server.
- Updated [program.cs](./Program.cs) to support ASP.NET Core Identity and Duende Identity Server.
- Created [entity.cs](./Models/entity.cs) to allow for custom properties within the user manager.

### Removed
- Removed `GrantType` and refresh token as they are currently not needed for this endpoint.
- Deleted unnecessary log messages and HTTP response messages from [Messages.cs](./Utilities/Messages.cs).
- Removed Argon2 settings from [appsettings.json](./appsettings.json) as they are no longer required.

### Fixed
- Enhanced encryption and decryption processes by embedding the IV within the ciphered text in [authentication.cs](./Utilities/Authentication.cs).
- Modified the refresh token storage approach in [databaseHelper.cs](./Utilities/DatabaseHelper.cs) to align with database schema updates.

### Notes
- The next commit will focus on removing redundant files and improving project structure.
- The [migrations](./Migrations/) folder was auto-generated due to the integration of Entity Framework into the project.


## 0.2.1
### Removed
- Deleted `Argon.cs` from the [Configuration](./Configurations/) folder, as it is no longer necessary.
- Removed `Misc.cs` from the [Utilities](./Utilities/) folder for the same reason.
- Eliminated Argon2-related methods from [Authentication.cs](./Utilities/Authentication.cs).
- Removed unused methods from [DatabaseHelper.cs](./Utilities/DatabaseHelper.cs), which are now redundant since their functions were replaced by ASP.NET Core Identity.

### Notes
- The next commit will aim to incorporate storage for client IDs and client secrets.

## 0.2.2
### Added
- Created a [folder](./DbContext/) to store the database context.
- Introduced a connection string variable in [Program.cs](./Program.cs) to avoid repetitive calls from configuration.
- Registered `Duende.IdentityServer.EntityFramework` database contexts for dependency injection in [Program.cs](./Program.cs).

### Fixed
- Adjusted the order in [Program.cs](./Program.cs) to ensure [appsettings.json](./appsettings.json) is added to the configuration before loading services.
- Updated the Duende Identity Server configuration to use the entity framework instead of in-memory storage.

### Removed
- Deleted JWT configuration from [Program.cs](./Program.cs) as it's now managed by Duende for token handling.
- Removed Duende configuration files from in-memory storage as they are now managed through `Duende.IdentityServer.EntityFramework`.
- Removed `Jwt.cs` as it is no longer necessary.

### Notes
- The upcoming release will include bootstrap seeding.
- Additional folders have been created to support the integration of `Duende.IdentityServer.EntityFramework`.

## 0.2.3
### Added
- Added a [GitHub Action](./.github/workflows/dependency-review.yml) to monitor dependencies for vulnerabilities and invalid licenses.

### Notes
- Future commits will focus on adding GitHub Actions for code quality, security, and unit testing.

## 0.3.0
### Added
- Introduced a [Tests](./Tests/) folder to organize unit tests.

### Fixed
- Restructured the folder layout to support unit testing.
- Excluded `bin` and `obj` folders as they are not required for building the project.

### Notes
- The upcoming commit will address the [Dockerfile](./Dockerfile), as the folder restructure may cause build issues.
- Secrets and keys in the `.env` file will be rotated.

## 0.3.1
### Added
- Implemented the first unit test for testing the [token.cs](./API/Utilities/token.cs).
- Included the [TMS_API.csproj](./API/TMS_API.csproj) as a project reference in [Tests](./Tests/Tests.csproj).

### Removed
- Deleted the bin and obj folders from the [Tests](./Tests/).

### Fixed
- Corrected the [Dockerfile](./Dockerfile) to use the proper directories for successful builds.


## 0.3.2
### Fixed
- Relocated [.gitignore](./.gitignore) and [.dockerignore](./.dockerignore) files to the root directory.

### Removed
- Deleted remaining `obj` folder in [Tests](./Tests/) that was not completely removed previously.

## 0.3.3
### Added
- Introduced unit tests for [Authentication.cs](./Tests/Utilities/Authentication.cs).

### Notes
- Upcoming commits will focus on adding unit tests for essential scripts.
- A GitHub action for running unit tests prior to merges will be implemented in the future.

## 0.3.4
### Added
- Unit tests for the [security middleware](./Tests/Middleware/securityMiddleware.cs).

### Fixed
- Updated the returned error code in [securityMiddleware.cs](./API/Middleware/securityMiddleware.cs) from "forbidden" to "unauthorized."
- Modified [program.cs](./API/Program.cs), [securityMiddleware.cs](./API/Middleware/securityMiddleware.cs), and [Controllers](./API/Controllers/) to utilize `HttpStatusCode` instead of hardcoded numeric values.
- Updated the test class in [Authentication.cs](./Tests/Utilities/Authentication.cs) to use `TestInitialize` instead of a constructor.

## 0.3.5
### Added
- Included unit tests for the [Authentication Controller](./Tests/Controllers/AuthenticationController.cs).

### Fixed
- Updated `StatusCode` in [AuthenticationController.cs](./API/Controllers/AuthenticationController.cs) to use `HttpStatusCode` instead of a hardcoded numeric value.
- Modified the properties in [Tokens.cs](./Tests/Utilities/Tokens.cs) to use null forgivers instead of being nullable.

## 0.3.6
### Added
- Implemented unit tests for [RegistrationController.cs](./API/Controllers/RegistrationController.cs).
- Revised the Roadmap in [README.md](README.MD) to reflect the project's updated goals.

## 0.3.7
### Added
- Implemented a [GitHub Action](./.github/workflows/ci.yml) to run unit tests.

### Fixed
- Removed the unnecessary token supplying the GitHub token, which resolved the error.

## 0.3.8
### Added
- Introduced [IdentityServer.cs](./API/Configurations/IdentityServer.cs) to handle data seeding for the database.

### Fixed
- Adjusted the middleware order to ensure data seeding occurs before enabling IdentityServer.

## 0.4.0
### Added
- Introduced TypeScript to the project to improve the login functionality for the Authorization Code Flow with Proof Key for Code Exchange (PKCE).
- Updated [.gitignore](.gitignore) and [.dockerignore](.dockerignore) to include additional folders necessary for integrating TypeScript into the project.
- Developed a new [controller](./API/Controllers/AccountController.cs) and [model](./API/Models/AccountModel.cs) to implement the PKCE authorization flow.
- Added [_Layout.cshtml](./API/Views/Shared/_Layout.cshtml) as a layout template for [login.cshtml](./API/Views/Account/login.cshtml).
- Created [login.tsc](./API/Scripts/login.ts) for enhanced validation of [login.cshtml](./API/Views/Account/login.cshtml). This TypeScript file was compiled to JavaScript, available [here](./API/wwwroot/js/login.js).
- Added a new [main.css](./API/wwwroot/css/main.css) stylesheet.

### Removed
- Deleted redundant Test packages from the [project](./API/TMS_API.csproj).

### Fixed
- Updated `program.cs` configuration to support the PKCE authorization flow, replacing the legacy ROPC flow.
- Modified the [IdentityServer](./API/Configurations/IdentityServer.cs) configuration to accommodate the PKCE authorization flow.


### 0.4.1
### Added
- Added a method in [Authentication](./API/Utilities/Authentication.cs) for generating code challengers.

### Removed
- Deleted the authentication controller and model, as they are now unnecessary due to the API's adoption of the PKCE authorization flow.
- Removed the redundant Microsoft Identity Client package from the project.

### Fixed
- Updated the project to build using the HTTPS protocol by modifying [launchSettings](./API/Properties/launchSettings.json) and [appsettings](./API/appsettings.json).
- Updated Authentication to support the PKCE authorization flow, adding required support for state and session management in [Program](./API/Program.cs).
- Resolved an issue where the seeded client was not loading into the database.
- Fixed `RedirectUris` to include the callback endpoint.
- Updated scopes to include `offline_access`, enabling the identity server to generate refresh tokens.
- Adjusted endpoints in [AccountController](./API/Controllers/AccountController.cs) to align with the PKCE authorization flow.
- Modified the method in [Token](./API/Utilities/Token.cs) to use the Authorization Code Token Request instead of the Password Token Request.

### Notes
- Future updates will focus on enhancing the login page's quality.
- Unit tests will be included in the next commit.

## 0.4.2
### Added
- Wrote [unit tests](./Tests/Controllers/AccountController.cs) for the [AccountController](./API/Controllers/AccountController.cs).
- Added a unit test for the `GenerateCodeChallengerAsync` method in [Authentication](./API/Utilities/Authentication.cs).

### Fixed
- Amended the `GenerateRefreshToken_ShouldReturnNonEmptyString` test to `GenerateCodeVerifier_ShouldReturnNonEmptyString` and updated it to unit test the `GenerateCodeVerifier` method.
- Changed the variable name within the `GenerateCodeChallengerAsync` method to better describe its value.
- Fixed the [unit test](./Tests/Utilities/Tokens.cs) to now correctly test the `PCKEAsync` method.


## 0.4.3
### Fixed
- Updated several files to use the correct callback URI.
- Modified the callback endpoint in [AccountController](./API/Controllers/AccountController.cs) to redirect to state.

### Removed
- Removed the unnecessary packages: Duende.AccessTokenManagement and Duende.AccessTokenManagement.OpenIdConnect.

## 0.4.4
### Added
- Introduced new [unit tests](./Tests/Controllers/AccountController.cs) for the [AccountController](./API/Controllers/AccountController.cs), focusing on testing constructor-injected dependencies.

## Fixed
- Resolved an issue where the `authorizationUrl` in the [AccountController](./API/Controllers/AccountController.cs) was using an incorrect redirect URL.
- Included the redirect URL in the allowed redirect URLs within [IdentityServer](./API/Configurations/IdentityServer.cs).


## 0.4.5
### Fixed
- Updated [IdentityServer](./API/Configurations/IdentityServer.cs), [AccountController](./API/Controllers/AccountController.cs), and [appsettings.json](./API/appsettings.json) to use `localhost:5188` as the redirect URI.

### Removed
- Eliminated the callback endpoint, as it is redundant in the PKCE authorization flow.

## 0.4.6
### Fixed/Updated
- Enhanced unit tests to align with changes introduced in [0.4.5](#045).

### Removed
- Eliminated the redundant [token](./API/Utilities/token.cs) dependency from [AccountController](./API/Controllers/AccountController.cs).

## 0.4.7
### Changes and Updates
- The project has been renamed to **TMS_IDP**, and all namespaces within the project have been updated accordingly.

### Additional Information
- As the project now focuses solely on authentication and authorization, it will no longer be actively developed. However, it will continue to receive regular updates to enhance performance and improve the user interface.


## 0.4.7
### Fixed/Updated
- Excluded the [Tests](./Tests/) directory from the [Dockerfile](./Dockerfile).
- Revised the [.dockerignore](.dockerignore) file to exclude unnecessary folders for the project build.
- Enhanced the [README](README.MD) with an improved description, feature list, and roadmap.
- Updated the [CI configuration](.github/workflows/ci.yml) to ensure it can be located and added to the required status checks on GitHub.


## 0.4.8
### Added
- Conditional loading of environment variables in development using `#if DEBUG`.
- Redis cache (`StackExchangeRedisCache`) for session state, replacing `DistributedMemoryCache`.
- Token cleanup for Duende IdentityServer with configurable cleanup options.
- Custom authentication failure handling with redirection to `/error`.

###  Fixed/Updated
- Updated IdentityServer login/logout URLs to `/auth/login` and `/auth/logout`.
- Enhanced cookie security:
  - `SecurePolicy` set to `Always`.
  - `SameSiteMode.Lax` enforced.
  - `ExpireTimeSpan` set to 1 hour with `SlidingExpiration`.
  - Added `AccessDeniedPath`.
- Strengthened session cookie security to match authentication settings.
- Improved token validation with stricter checks and 5-minute `ClockSkew`.

### Removed
- Unconditional environment variable checks for `API__Key`.

## 0.4.9
### Added
- Introduced a new migration, `IdsIntWithGuidsKeys`, to facilitate the transition to GUID-based primary keys in the database schema. This change aligns with updates made in [Application](./API/DbContext/Application.cs) to improve data integrity and consistency.

### Fixed/Updated
- Refactored [Program](./API/Program.cs) to utilize custom user and role store classes, ensuring enhanced flexibility and maintainability of identity operations.
- Updated [Application](./API/DbContext/Application.cs) to convert table ID columns to the `uniqueidentifier` datatype, replacing the previous `nvarchar` and `int` types. This change provides better alignment with best practices for modern database design.
- Significantly modified the [IdsIntWithGuidsKeys migration](./API/Migrations/AspNetCoreIdentity/20241225180411_IdsIntWithGuidsKeys.cs) to accommodate the shift to GUID-based keys and ensure compatibility with the updated schema.


## 0.4.10
### Added
- Introduced the [CertificateModel](./API/Models/CertificateModel.cs) to facilitate JSON deserialization for API responses.
- Developed the [DataProtection](./API/ProgramExtension/DataProtection.cs) extension to enhance services and enable custom configurations for Data Protection.
- Created the [CertificateService](./API/Utilities/CertificateService.cs) to fetch certificates from [TMS_VAULT](https://github.com/bm-seyi/TMS_VAULT).
- Updated [appsettings](./API/appsettings.json) with new entries for retrieving secrets directly from the environment through the configuration.
- Implemented [unit tests](./Tests/Utilities/CertificateService.cs) for the [CertificateService](./API/Utilities/CertificateService.cs).

### Fixed/Updated
- Reorganized the [program](./API/Program.cs), moving the dependency injection setup to the top of the file for improved clarity.
- Adjusted the `AddStackExchangeRedisCache` configuration to integrate with [TMS-REDIS](https://github.com/bm-seyi/TMS_REDIS).

### Notes
- Future updates will focus on adding more unit tests and implementing a certificate renewal background service.


## 0.5.0
### Added
- **Vault Token Retrieval**: Now fetching the vault token from configuration in [Program.cs](./API/Program.cs).
- **Models for Certificate Deserialization**: Introduced  
  - [`ListCertificateModel.cs`](./API/Models/DataProtection/ListCertificateModel.cs)  
  - [`RetrieveCertificateModel.cs`](./API/Models/DataProtection/RetrieveCertificateModel.cs)  
  These models facilitate JSON deserialization within [`CertificateService.cs`](./API/Utilities/CertificateService.cs).
- **Certificate Operations in `CertificateService`**:  
  - `RetrieveAsync`: Fetches a certificate from Vault.  
  - `DeleteAsync`: Removes a certificate from Vault.  
  - `StoreAsync`: Saves a certificate to Vault.  
  - `ListAsync`: Lists stored certificates.  

### Fixed/Updated
- **Refactored Model Organization**:  
  - Created separate folders for models related to [Controllers](./API/Models/Controller/) and [Data Protection](./API/Models/DataProtection/) for better maintainability.  
  - Updated namespaces accordingly.
- **Enhanced Data Protection Logic**:  
  - Updated [`DataProtection.cs`](./API/ProgramExtension/DataProtection.cs) to dynamically load required certificates for `ProtectKeysWithCertificate` and `UnprotectKeysWithAnyCertificate`.  
  - Specified cryptographic algorithms used.
- **Refactored `CertificateService`**:  
  - Now uses `HttpClient` directly instead of `IHttpClientFactory`.  
  - Moved configuration retrieval from `IConfiguration` to environment variables.  
  - Replaced `ILogger` logging with `Console.WriteLine`.  
  - Renamed `GetCertificateFromVault` to `GenerateAsync` for clarity.
- **Updated Unit Tests**:  
  - Refactored [`CertificateService` unit tests](./Tests/Utilities/CertificateService.cs) to align with recent changes.

### Removed
- **Dependency Cleanup**:  
  - Removed `HttpClient` and `ICertificateService` dependency injection from [Program.cs](./API/Program.cs).  
  - Eliminated `ICertificateService` interface in [`CertificateService.cs`](./API/Utilities/CertificateService.cs).  
  - Removed reliance on `IHttpClientFactory` and `IConfiguration`.  
  - Eliminated `ILogger` logging.  


##  0.5.1
### Added  
- Implemented [unit tests](./Tests/Attributes/NoWhiteSpace.cs) for the [NoWhiteSpace](./API/Attributes/NoWhiteSpace.cs) attribute.  

### Fixed/Updated  
- Refactored namespaces in [Tests](./Tests/) to align with best practices.


## 0.5.2
### Fixed/Updated
- Removed references to `ApiMiddleware` in [Program](./API/Program.cs) and `IDatabaseActions` dependency injection.

### Removed
- Deleted `securityMiddleware.cs` from `API/Middleware/`.
- Removed `DatabaseHelper.cs` and `DatabaseActions` from `API/Utilities/`.
- Deleted associated unit tests for `securityMiddleware.cs` from `Tests/Middleware/`.

## 0.5.3
### Added
- Added `Microsoft.EntityFrameworkCore.InMemory` package (version 8.0.13) to [Tests](./Tests/Tests.csproj) for in-memory database testing.
- Implemented [unit tests](./Tests/Configurations/IdentityServerTests.cs) to validate the functionality of [IdentityServer](./API/Configurations/IdentityServer.cs).

### Fixed/Updated
- Improved database initialization in [IdentityServer](./API/Configurations/IdentityServer.cs) by replacing `MigrateAsync()` with `EnsureCreatedAsync()` and adding a check for relational databases before applying migrations.
- Fixed inconsistent indentation in [IdentityServer](./API/Configurations/IdentityServer.cs) for better readability.

## 0.5.4
### Added:
- Added a new unit test class `ProgramExtensionsTests` for testing the `AddDataProtectionAsync` extension method.
- Introduced various test methods to verify different scenarios for the `AddDataProtectionAsync` method:
  - **AddDataProtectionAsync_UsesExistingValidCertificate**: Ensures the existing valid certificate is used when available.
  - **AddDataProtectionAsync_GeneratesNewCertificate_WhenNoneExists**: Verifies the generation of a new certificate when no existing certificate is found.
  - **AddDataProtectionAsync_ReplacesExpiredCertificate**: Ensures expired certificates are replaced with a newly generated certificate and that the expired certificate is deleted.
  - **AddDataProtectionAsync_RetrieveArchivedCertificate**: Verifies retrieval of archived certificates, ensuring the method checks multiple potential certificates.
- Mocked `ICertificateService` and `IConnectionMultiplexer` using `Moq` for unit testing.
- Added helper method `CreateTestCertificate` to generate a self-signed certificate for testing purposes.

### Fixed/Updated:
- Updated `Program.cs` to pass instances of `ICertificateService` and `IConnectionMultiplexer` into the `AddDataProtectionAsync` method.
- Refactored `AddDataProtectionAsync` in `ProgramExtension/DataProtection.cs` to support both `ICertificateService` and `IConnectionMultiplexer` parameters instead of using `IConfiguration` for Redis configuration.
- Moved Redis password configuration from `ProgramExtension/DataProtection.cs` to `Program.cs` for better clarity and organization of settings.
- Adjusted certificate retrieval and generation flow to ensure it works properly with the new dependencies (Redis and CertificateService).

### Removed:
- Removed unnecessary `IConfiguration` dependency for Redis configuration within `AddDataProtectionAsync`.

## 0.6.0
### Added
- Added `.vscode/` directory to `.gitignore` to exclude VS Code settings from version control.

### Fixed/Updated
- Added `[ExcludeFromCodeCoverage]` attribute to all identity-related classes in `API/DbContext` to exclude them from code coverage analysis.
- Updated `LoginViewModel` in `API/Models/Controller/AccountModel.cs` with simplified validation attributes and default values.
- Updated `Program.cs` to load environment variables from the parent directory in debug mode and improved rate limiting configuration.
- Simplified login view (`API/Views/Auth/login.cshtml`) by removing complex validation and using a more straightforward form.
-  Refactored `AuthController.cs` and its associated login/logout functionality. This has been renamed to `AccountController.cs`

### Removed
- Removed `IdentityServer.cs` and `rateLimit.cs` configuration files from `API/Configurations/` as they are no longer needed.
- Removed `Authentication.cs` and `token.cs` utility classes from `API/Utilities/` as they are no longer used.
- Removed all test files (`IdentityServerTests.cs`, `AuthController.cs`, `RegistrationController.cs`, `Authentication.cs`, `Tokens.cs`) from the `Tests/` directory as they are no longer relevant.
- Removed `ClientSecret` property from `RegistrationModel.cs` as it's no longer required.


## 0.6.1
### Added
- Injected `IIdentityServerInteractionService` into `AuthController`.
- Added `[ValidateAntiForgeryToken]` attribute to the `POST /login` endpoint.
- Implemented return URL validation during login using `Url.IsLocalUrl` and `_interactionService.IsValidReturnUrl`.

### Fixed/Updated
- Renamed `AccountController` to `AuthController`.
- Updated logger dependency from `ILogger<AccountController>` to `ILogger<AuthController>`.
- Updated constructor to accept `IIdentityServerInteractionService`.
- Improved login flow to ensure only safe URLs are redirected after successful authentication.


## 0.7.0
### Added
- Migrated database context and related files to new project `TMS_MIGRATE`
- Updated project to .NET 9.0
- Added project reference to TMS_MIGRATE in API project
- Created console app `TMS_SEED` which is responsible for seeding the database. 

### Changed/Updated
- Updated all NuGet packages to latest versions
- Modified Identity Server configuration to use migrations from TMS_MIGRATE assembly
- Changed namespace from `TMS_IDP.DbContext` to `TMS_MIGRATE.DbContext` in controllers
- Updated Program.cs to use correct migration assemblies

### Removed
- Removed all entity configuration files from original DbContext folder


## 0.7.1
### Added
- Added `permissions: contents: read` to `.github/workflows/ci.yml` and `.github/workflows/dependency-review.yml` to explicitly define GitHub Actions permissions.


## 0.7.2
### Added
- MVC-based registration view and actions in `RegistrationController`.
- Input validation with `ModelState.IsValid` and form model binding for login and registration views.
- `Register` GET and POST routes with anti-forgery protection.

### Fixed/Updated
- Changed `RegistrationController` from API-based (`ControllerBase`) to MVC-based (`Controller`) to support views.
- Refactored model usage: replaced `TMS_IDP.Models.Controllers` with `TMS_IDP.Models.ViewModel`.
- Improved login flow in `AuthController` to validate model state and return the view with input data on failure.
- Cleaned up `_Layout.cshtml` by removing comment clutter and aligning spacing.

### Removed
- Deleted obsolete `AccountModel.cs` and `RegistrationModel.cs` under `TMS_IDP.Models.Controllers`.
- Removed unused usings and API rate limiting from `RegistrationController`.

## 0.7.3
### Added
- Added `CreateAccountUrl` configuration in IdentityServer setup to support direct navigation to registration page.

### Fixed/Updated
- Updated `RegistrationController` to use `RegisterViewModel` instead of the deleted `RegistrationViewModel`.
- Improved registration logic by adding password confirmation check with logging for mismatches and invalid submissions.
- Cleaned up spacing and formatting in `CertificateService.cs`.

### Removed
- Deleted `RegistrationViewModel.cs` from `TMS_IDP.Models.ViewModel` in favor of `RegisterViewModel`.
- Removed unused authorization policy for `ApiScope` in `Program.cs`.


## 0.7.4
### Fixed/Updated
- Sanitized email input in registration logging to prevent newline injection (`\r`, `\n`) vulnerabilities in `RegistrationController`.


## 0.7.5
### Added
- Added DotNetEnv package for environment variable management in development
- Added explicit content inclusion for `appsettings.json` with copy to output directory setting
- Added debug configuration to load `.env` file from project root during development

### Fixed/Updated
- Updated `IdentityServerModel` properties to use `required` modifier and nullable types where appropriate
  - Made `DisplayName`, `Description`, and `UserClaims` nullable in `IdentityResourceModel`
  - Changed `AllowedCorsOrigins` to initialize with empty list by default
- Updated Program.cs configuration:
  - Improved connection string retrieval from configuration
  - Changed IdentityServerModel registration to use Options pattern
  - Added better error handling for parent directory detection in debug mode
- Updated Seeder.cs:
  - Added check for relational database before seeding
  - Improved null handling for UserClaims
  - Made variable declarations more explicit
- Updated appsettings.json structure:
  - Changed Clients from object to array
  - Updated redirect URIs format
  - Changed enum values from strings to integers for RefreshTokenUsage/Expiration

### Removed
- Removed unused `using Duende.IdentityServer.EntityFramework.Entities` from IdentityServerModel.cs
- Removed manual instantiation of empty lists in IdentityServerModel properties
- Removed redundant blank line at start of IdentityServerModel.cs