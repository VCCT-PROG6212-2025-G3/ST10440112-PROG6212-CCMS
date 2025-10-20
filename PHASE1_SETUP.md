# Contract Monthly Claim System (CMCS)

## Setup Instructions for Phase 1

### Database Setup

1. **Open Package Manager Console** in Visual Studio:
   - Tools → NuGet Package Manager → Package Manager Console

2. **Run the following commands:**

```powershell
# Create initial migration
Add-Migration InitialCreate

# Update the database
Update-Database
```

### Connection String Options

#### Option 1: Local Database (Current Setup)
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CCMS_DB;Trusted_Connection=true;MultipleActiveResultSets=true"
```

#### Option 2: Azure SQL Database (Replace when ready)
```json
"DefaultConnection": "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=CCMS_DB;Persist Security Info=False;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

## Phase 1 Completed ✓

- ✓ Created all model classes (Lecturer, Claim, Document, Admin, ProgrammeCoordinator, AcademicManager)
- ✓ Added Entity Framework Core packages
- ✓ Created ApplicationDbContext with relationships
- ✓ Configured Program.cs for database support
- ✓ Added seed data for testing
- ✓ Ready for migration

## Next Steps

1. Run migrations (commands above)
2. Test the database connection
3. Start building controllers and views for Phase 2
