# Database Migration Required

## New Features Added:
- ClaimComment model for storing coordinator and manager feedback
- Relationship between Claims and Comments

## Run these commands in Package Manager Console:

```powershell
Add-Migration AddClaimComments
Update-Database
```

## What this adds:
- ClaimComments table with CommentId, ClaimId, AuthorName, AuthorRole, CommentText, CreatedDate
- Foreign key relationship to Claims table
- Cascade delete (comments deleted when claim is deleted)
