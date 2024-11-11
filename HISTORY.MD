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

### 0.3.3
### Added
- Introduced unit tests for [Authentication.cs](./Tests/Utilities/Authentication.cs).

### Notes
- Upcoming commits will focus on adding unit tests for essential scripts.
- A GitHub action for running unit tests prior to merges will be implemented in the future.

