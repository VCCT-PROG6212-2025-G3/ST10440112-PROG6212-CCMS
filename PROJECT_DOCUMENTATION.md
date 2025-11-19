# Contract Monthly Claims Management System (CCMS) - Implementation Guide

## System Overview

The Contract Monthly Claims Management System is an ASP.NET Core 8.0 web application designed to streamline the process of submitting, verifying, approving, and processing lecturer claims for monthly contract work. The system implements a complete workflow with role-based access control, validation, and automated processing.

## Architecture & Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, jQuery
- **Session Management**: HTTP Session middleware

## Implemented Features (10 Commits)

### Commit 1: ASP.NET Core Identity Authentication
- Real login/logout with Identity framework
- User registration (seeded users only, no self-registration)
- Role-based authorization (Lecturer, ProgrammeCoordinator, AcademicManager, HR)
- Password hashing and secure storage

### Commit 2: HR Role & User Management
- HR dashboard for system management
- Lecturer management interface (list, edit, delete)
- User role assignment

### Commit 3: Automate Lecturer Claim Submission
- Lecturers submit claims with hours worked
- Hourly rate automatically populated from Lecturer record (HR managed)
- Auto-calculation of total amount (Hours × HourlyRate)
- JavaScript validation on client-side

### Commit 4: Automatic Claim Verification Workflow
- Programme Coordinator reviews submitted claims
- Manual verification with approve/reject options
- Claim status tracking (Pending → Verified)
- Comments and feedback system

### Commit 5: Manager Approval Automation
- Academic Manager reviews verified claims
- Auto-approval workflow for valid claims
- Manager dashboard with claim statistics
- Ability to approve/reject verified claims

### Commit 6: Invoice/Report Generation
- HR generates invoices for approved claims
- CSV export for invoices and reports
- Date-based filtering and report generation
- Payment tracking (settled/pending)

### Commit 7: Session Management & Access Control
- User session tracking with IP address logging
- Role-based access control filter
- Prevents unauthorized page access
- 30-minute session idle timeout

### Commit 8: Enhanced Lecturer Data Management
- CSV bulk import for lecturer data
- Create/update lecturers via CSV upload
- Hourly rate management
- Data validation with detailed error reporting

### Commit 9: Validation & Error Handling
- Comprehensive model validation
- Custom claim validation rules
- Input sanitization against injection attacks
- Detailed error messages for users

### Commit 10: Final Testing, Cleanup & Documentation
- Complete documentation
- Code comments and inline documentation
- Error handling across all endpoints
- Production-ready configuration

## User Roles & Access Levels

### Lecturer
- **Access**: `/Home`, `/Lecturer`, `/Claim`
- **Capabilities**:
  - Submit new claims with hours worked
  - View submission history
  - Track claim status through workflow
  - View approved payment status

### Programme Coordinator
- **Access**: `/Home`, `/Coordinator`, `/Claim`, `/Lecturer`
- **Capabilities**:
  - Verify pending claims
  - Review claim details and documentation
  - Approve/reject claims with comments
  - Track verification history

### Academic Manager
- **Access**: `/Home`, `/Manager`, `/Claim`, `/Lecturer`
- **Capabilities**:
  - Approve verified claims
  - View claim approval dashboard
  - Track all claims through approval process
  - Manage claim status transitions

### HR Administrator
- **Access**: `/Home`, `/HR`, `/Lecturer`
- **Capabilities**:
  - Manage lecturer database
  - Bulk import lecturer data from CSV
  - Generate invoices for approved claims
  - Generate payment reports and export to CSV
  - Track payment settlements
  - View system activity logs

## Database Models

### Lecturer
```
- LecturerId (Guid) [Primary Key]
- Name (string, required)
- Email (string, required, unique)
- Department (string)
- HourlyRate (decimal, required)
- Claims (ICollection<Claim>)
```

### Claim
```
- ClaimId (Guid) [Primary Key]
- LecturerId (Guid) [Foreign Key]
- ClaimStatus (string: Pending, Verified, Approved, Rejected, Settled)
- HourlyRate (int)
- TotalHours (float)
- ClaimDate (DateTime)
- SubmissionDate (DateTime)
- ApprovedDate (DateTime?)
- IsSettled (bool)
- TotalAmount (calculated: TotalHours × HourlyRate)
- Documents (ICollection<Document>)
- Comments (ICollection<ClaimComment>)
```

### UserSession
```
- SessionId (Guid) [Primary Key]
- UserId (string)
- Email (string)
- UserRole (string)
- LoginTime (DateTime)
- LastActivityTime (DateTime)
- LogoutTime (DateTime?)
- IPAddress (string)
- IsActive (bool)
```

### UserActivity
```
- ActivityId (Guid) [Primary Key]
- UserId (string)
- Email (string)
- UserRole (string)
- Action (string)
- Controller (string)
- ActionName (string)
- ActivityTime (DateTime)
- IPAddress (string)
- Success (bool)
- Details (string)
```

## API Endpoints & Controllers

### Lecturer Controller
- `GET /Lecturer/MyClaims` - View submitted claims
- `GET /Lecturer/SubmitClaim` - Submit new claim form
- `POST /Lecturer/SubmitClaim` - Submit claim
- `GET /Lecturer/ClaimDetail/{id}` - View claim details

### Coordinator Controller
- `GET /Coordinator/Dashboard` - Verification dashboard
- `GET /Coordinator/VerifyClaims` - List claims to verify
- `POST /Coordinator/VerifyClaim/{id}` - Approve claim
- `POST /Coordinator/RejectClaim/{id}` - Reject claim

### Manager Controller
- `GET /Manager/Dashboard` - Approval dashboard with statistics
- `GET /Manager/Review` - List verified claims for approval
- `POST /Manager/ApproveClaim/{id}` - Approve claim
- `POST /Manager/RejectClaim/{id}` - Reject claim
- `GET /Manager/TrackClaims` - Audit trail view

### HR Controller
- `GET /HR/Dashboard` - HR dashboard with quick actions
- `GET /HR/ManageLecturers` - Lecturer list and management
- `GET /HR/BulkImport` - CSV import interface
- `POST /HR/ImportLecturersCSV` - Process CSV upload
- `GET /HR/ExportLecturersTemplate` - Download CSV template
- `GET /HR/ApprovedClaims` - View approved unpaid claims
- `POST /HR/MarkPaid/{id}` - Mark claim as settled
- `GET /HR/GenerateInvoice/{id}` - View invoice preview
- `GET /HR/ExportInvoicePDF/{id}` - Download invoice as CSV
- `GET /HR/GenerateReport` - Report generation interface
- `GET /HR/ExportReportCSV` - Download report as CSV

### Account Controller
- `GET /Account/Login` - Login page
- `POST /Account/Login` - Process login
- `POST /Account/Logout` - Logout
- `GET /Account/AccessDenied` - Access denied page

## Validation Rules

### Claim Submission
- Hours must be 0.01 - 24 per day
- Hourly rate must be positive
- Claim date cannot be in future
- Submission date cannot be before claim date

### CSV Import (Lecturers)
- Required fields: Name, Email, Department, HourlyRate
- Email must be valid
- Hourly rate must be positive number
- Duplicate detection by email (updates existing)

### Data Constraints
- Total claim amount limited to R 100,000
- Names and emails limited to 100 characters
- Department limited to 100 characters

## Configuration Files

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=CCMS;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Program.cs
- Configures Entity Framework Core with SQL Server
- Registers Identity for authentication
- Adds Session support (30-minute timeout)
- Registers custom services (FileUploadService, SessionManagementService, ClaimValidationService)
- Configures global role-based access filter
- Seeds initial users and roles

## Seeded Users

All users have password: `Password123`

```
Lecturer: michael.jones@newlands.ac.za
Coordinator: ebrahim.jacobs@newlands.ac.za
Manager: janet.duplessis@newlands.ac.za
HR: hr@newlands.ac.za
```

## File Uploads

- Document uploads for claims
- CSV imports for lecturer bulk data
- Files stored in `/uploads` directory
- Validated file types and sizes

## Session Management

- 30-minute idle timeout
- Session created on successful login
- Session terminated on logout
- IP address tracking
- Activity logging for audit trail

## Error Handling

- Global exception handling
- User-friendly error messages
- Detailed logging for debugging
- Validation feedback on forms
- Error pages with navigation

## Security Features

- ASP.NET Core Identity for authentication
- Role-based authorization attributes
- Anti-forgery token validation
- Input sanitization against injection
- SQL injection prevention via EF Core
- HTTPS enforcement in production
- Secure password hashing

## Testing

Run tests with:
```bash
dotnet test
```

Test files located in `ST10440112-PROG62121-CCMS-TEST/` directory with tests for:
- Lecturer workflow
- Coordinator verification
- Manager approval
- HR processing

## Database Migration

Create and apply migrations:
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Running the Application

### Development
```bash
dotnet run
```
Application runs on `https://localhost:7142`

### Production
```bash
dotnet publish -c Release
```

## Performance Considerations

- Claims are loaded with related Lecturer data
- Pagination on claim lists
- Indexed queries on frequently accessed fields
- Session timeout prevents memory leaks
- Activity logging stored in database

## Future Enhancements

- Email notifications for claim status changes
- Advanced reporting and analytics
- Claim amendment/resubmission workflow
- Batch payment processing
- Integration with payroll systems
- Mobile app for claim submission
- PDF invoice generation

## Support & Maintenance

For issues or feature requests, refer to the GitHub repository or contact the development team.

---

**Last Updated**: November 19, 2025
**Version**: 1.0 (Complete)
