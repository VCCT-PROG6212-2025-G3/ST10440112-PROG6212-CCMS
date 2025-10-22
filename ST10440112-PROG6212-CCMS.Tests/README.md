# Unit Tests for ST10440112-PROG6212-CCMS

## Overview
This test project contains comprehensive unit tests for the Contract Monthly Claim System controllers, services, and models.

## Test Framework
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **Entity Framework Core InMemory** - In-memory database for testing

## Running Tests

### Visual Studio
1. Open Test Explorer (Test > Test Explorer)
2. Click "Run All" to execute all tests
3. View results in Test Explorer window

### Command Line
```bash
cd ST10440112-PROG6212-CCMS.Tests
dotnet test
```

### With Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Test Coverage

### Controllers (27 tests)

#### ClaimsControllerTests (9 tests)
- ✅ Create GET returns view
- ✅ Create POST with valid model redirects to index
- ✅ Index returns view with claims
- ✅ Details with valid ID returns view
- ✅ Details with invalid ID returns not found
- ✅ Details with null ID returns not found
- ✅ AddDocuments with pending claim returns view
- ✅ AddDocuments POST with no documents redirects with error

#### HomeControllerTests (4 tests)
- ✅ Index returns view with statistics
- ✅ Index calculates statistics correctly
- ✅ Index returns recent claims
- ✅ Privacy returns view

#### CoordinatorControllerTests (7 tests)
- ✅ Dashboard returns view with statistics
- ✅ Review returns pending claims
- ✅ VerifyClaim with verify action updates status
- ✅ VerifyClaim with reject action updates status
- ✅ VerifyClaim adds comment when provided
- ✅ ReviewDetails with valid ID returns view
- ✅ ReviewDetails with invalid ID returns not found

#### ManagerControllerTests (7 tests)
- ✅ Dashboard returns view with statistics
- ✅ Review returns verified claims
- ✅ ApproveClaim with approve action updates status and date
- ✅ ApproveClaim with reject action updates status
- ✅ ApproveClaim adds comment when provided
- ✅ ReviewDetails with valid ID returns view
- ✅ ReviewDetails with invalid ID returns not found

### Services (9 tests)

#### FileUploadServiceTests (9 tests)
- ✅ GetFileExtension returns correct extension
- ✅ GetFileExtension handles no extension
- ✅ UploadFileAsync with valid PDF file returns success
- ✅ UploadFileAsync with valid DOCX file returns success
- ✅ UploadFileAsync with valid XLSX file returns success
- ✅ UploadFileAsync with invalid file type returns failure
- ✅ UploadFileAsync with oversized file returns failure
- ✅ UploadFileAsync with empty file returns failure
- ✅ UploadFileAsync creates directory if not exists

### Models (22 tests)

#### ClaimModelTests (10 tests)
- ✅ Claim with valid data passes validation
- ✅ Claim properties set correctly
- ✅ Claim can have documents
- ✅ Claim can have comments
- ✅ Claim default values are correct
- ✅ Claim calculates total amount correctly
- ✅ Claim status transition Pending to Verified
- ✅ Claim status transition Verified to Approved

#### DocumentModelTests (9 tests)
- ✅ Document with valid data passes validation
- ✅ Document properties set correctly
- ✅ Document supports PDF files
- ✅ Document supports DOCX files
- ✅ Document supports XLSX files
- ✅ Document can be linked to claim
- ✅ Document default values are correct
- ✅ Document URL can contain path
- ✅ Document upload date can be retrieved

#### ClaimCommentModelTests (3 tests)
- ✅ ClaimComment with valid data passes validation
- ✅ ClaimComment properties set correctly
- ✅ ClaimComment multiple comments can be added to claim

## Total: 58 Unit Tests

## Test Patterns

### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern:
```csharp
// Arrange - Set up test data
var claim = new Claim { ... };

// Act - Execute the method being tested
var result = await controller.Method(claim.Id);

// Assert - Verify the result
Assert.Equal(expected, actual);
```

### InMemory Database
Each test uses an isolated in-memory database:
```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;
```

### Mocking
External dependencies are mocked using Moq:
```csharp
var mockService = new Mock<IFileUploadService>();
mockService.Setup(s => s.Method()).Returns(value);
```

## Test Data
All tests use seeded test data with:
- Test lecturer (Computer Science department)
- Test claims (various statuses)
- Test documents and comments

## Code Coverage Goals
- Controllers: 80%+ coverage
- Services: 85%+ coverage
- Models: 90%+ coverage

## Notes
- Tests are isolated and independent
- Each test creates its own database instance
- Tests verify both success and failure scenarios
- All async operations are properly awaited
- File system operations use temporary directories
- All temporary files are cleaned up after tests
