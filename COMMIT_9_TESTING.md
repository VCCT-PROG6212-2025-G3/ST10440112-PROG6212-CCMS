# Commit 9: Validation & Error Handling - Test Plan

## Overview
Commit 9 adds comprehensive validation, custom validation rules, input sanitization, and detailed error messages throughout the application.

## Test Scenarios

### 1. Claim Model Validation

#### Hours Validation
```
Test Case 1: Negative Hours
- Action: Submit claim with -5 hours
- Expected: Error message "Total hours must be between 0.01 and 24 hours per day"
- Status: [  ] Pass  [  ] Fail

Test Case 2: Zero Hours
- Action: Submit claim with 0 hours
- Expected: Error message (same as above)
- Status: [  ] Pass  [  ] Fail

Test Case 3: Minimum Valid Hours
- Action: Submit claim with 0.01 hours
- Expected: Claim accepted ✓
- Status: [  ] Pass  [  ] Fail

Test Case 4: Maximum Valid Hours
- Action: Submit claim with 24 hours
- Expected: Claim accepted ✓
- Status: [  ] Pass  [  ] Fail

Test Case 5: Exceeds Maximum
- Action: Submit claim with 25 hours
- Expected: Error message (same as hours validation)
- Status: [  ] Pass  [  ] Fail
```

#### Date Validation
```
Test Case 6: Future Claim Date
- Action: Submit claim dated tomorrow
- Expected: Error "Claim date cannot be in the future"
- Status: [  ] Pass  [  ] Fail

Test Case 7: Valid Claim Date
- Action: Submit claim dated today or past
- Expected: Claim accepted ✓
- Status: [  ] Pass  [  ] Fail

Test Case 8: Submission Before Claim
- Action: Claim Date = 2025-01-15, Submission = 2025-01-10
- Expected: Error "Submission date cannot be before claim date"
- Status: [  ] Pass  [  ] Fail

Test Case 9: Valid Date Sequence
- Action: Claim Date = 2025-01-10, Submission = 2025-01-15
- Expected: Claim accepted ✓
- Status: [  ] Pass  [  ] Fail
```

### 2. Hourly Rate Validation

```
Test Case 10: Zero Rate
- Action: Submit claim with hourly rate R 0
- Expected: Error "Hourly rate must be a positive number"
- Status: [  ] Pass  [  ] Fail

Test Case 11: Negative Rate
- Action: Submit claim with hourly rate R -100
- Expected: Error (same as above)
- Status: [  ] Pass  [  ] Fail

Test Case 12: Valid Minimum Rate
- Action: Submit claim with hourly rate R 1
- Expected: Claim accepted ✓
- Status: [  ] Pass  [  ] Fail
```

### 3. Total Amount Validation

```
Test Case 13: Valid Amount
- Action: Submit 10 hours × R 350/hr = R 3,500
- Expected: Claim accepted ✓
- Status: [  ] Pass  [  ] Fail

Test Case 14: Exceeds Maximum
- Action: Submit 24 hours × R 5,000/hr = R 120,000
- Expected: Manager approval rejected with "exceeds maximum allowed"
- Status: [  ] Pass  [  ] Fail

Test Case 15: Exactly at Limit
- Action: Submit 100 hours × R 1,000/hr = R 100,000
- Expected: Claim accepted ✓
- Status: [  ] Pass  [  ] Fail
```

### 4. Claim Status Validation

```
Test Case 16: Valid Status Transitions
- Sequence: Pending → Verified → Approved → Settled
- Expected: All transitions succeed ✓
- Status: [  ] Pass  [  ] Fail

Test Case 17: Invalid Status
- Action: Try to set claim status to "InProgress"
- Expected: Error "Claim status must be one of: Pending, Verified, Approved, Rejected, Settled"
- Status: [  ] Pass  [  ] Fail

Test Case 18: Reject Claim
- Action: Coordinator rejects pending claim with reason
- Expected: Status changes to "Rejected" ✓
- Status: [  ] Pass  [  ] Fail
```

### 5. CSV Import Validation

#### Header Validation
```
Test Case 19: Missing Header
- Action: Upload CSV without header row
- Expected: Error "CSV header must be: Name,Email,Department,HourlyRate"
- Status: [  ] Pass  [  ] Fail

Test Case 20: Wrong Header Format
- Action: Upload CSV with header "Lecturer,EmailAddress,Dept,Rate"
- Expected: Error (same as above)
- Status: [  ] Pass  [  ] Fail

Test Case 21: Correct Header
- Action: Upload CSV with exact header "Name,Email,Department,HourlyRate"
- Expected: Processing starts ✓
- Status: [  ] Pass  [  ] Fail
```

#### Field Validation
```
Test Case 22: Missing Name
- Action: CSV row: ",email@example.com,Faculty,350"
- Expected: Error "Line X: Name is required"
- Status: [  ] Pass  [  ] Fail

Test Case 23: Missing Email
- Action: CSV row: "John Smith,,Faculty,350"
- Expected: Error "Line X: Email is required"
- Status: [  ] Pass  [  ] Fail

Test Case 24: Invalid Hourly Rate
- Action: CSV row: "John Smith,john@example.com,Faculty,abc"
- Expected: Error "Line X: Hourly rate must be a valid positive number"
- Status: [  ] Pass  [  ] Fail

Test Case 25: Negative Hourly Rate
- Action: CSV row: "John Smith,john@example.com,Faculty,-100"
- Expected: Error "Line X: must be a valid positive number"
- Status: [  ] Pass  [  ] Fail

Test Case 26: Missing Optional Department
- Action: CSV row: "John Smith,john@example.com,,350"
- Expected: Lecturer created with Department = "Unspecified" ✓
- Status: [  ] Pass  [  ] Fail

Test Case 27: All Fields Valid
- Action: CSV row: "John Smith,john@example.com,Faculty of Science,350"
- Expected: Lecturer created successfully ✓
- Status: [  ] Pass  [  ] Fail
```

#### Duplicate Detection
```
Test Case 28: Duplicate Email (New Lecturer)
- Setup: Email exists in system
- Action: Upload CSV with same email
- Expected: Existing lecturer updated, count shows as 1 updated ✓
- Status: [  ] Pass  [  ] Fail

Test Case 29: Bulk Import with Mixed New/Existing
- Action: Upload CSV with 3 new and 2 existing emails
- Expected: Success message "Successfully imported 5 lecturer(s)"
- Status: [  ] Pass  [  ] Fail
```

### 6. Input Sanitization (XSS Prevention)

```
Test Case 30: Script Tag in Comment
- Action: Add claim comment with `<script>alert('xss')</script>`
- Expected: Script tags removed, displays as plain text
- Status: [  ] Pass  [  ] Fail

Test Case 31: HTML Tags in Name
- Action: Update lecturer name with `John<b>Smith</b>`
- Expected: HTML tags escaped, stored as plain text
- Status: [  ] Pass  [  ] Fail

Test Case 32: Quote Escaping
- Action: Add comment with double quote: `He said "hello"`
- Expected: Quote escaped as `&quot;`, displays correctly
- Status: [  ] Pass  [  ] Fail

Test Case 33: Angle Bracket Escaping
- Action: Add comment with `<div>test</div>`
- Expected: Escaped as `&lt;div&gt;test&lt;/div&gt;`
- Status: [  ] Pass  [  ] Fail
```

### 7. Validation Service Integration

```
Test Case 34: Lecturer Submission Validation
- Context: Lecturer submitting claim
- Action: Submit claim with invalid data
- Expected: ClaimValidationService.ValidateClaimSubmission() called
- Status: [  ] Pass  [  ] Fail

Test Case 35: Coordinator Verification Validation
- Context: Coordinator verifying claim
- Action: Try to verify claim with invalid status
- Expected: ClaimValidationService.ValidateClaimVerification() called
- Status: [  ] Pass  [  ] Fail

Test Case 36: Manager Approval Validation
- Context: Manager approving claim
- Action: Try to approve claim exceeding limit
- Expected: ClaimValidationService.ValidateClaimApproval() called, rejected
- Status: [  ] Pass  [  ] Fail
```

### 8. Error Message Display

```
Test Case 37: Single Field Error
- Action: Submit with only hours invalid
- Expected: Error shows only for hours field
- Status: [  ] Pass  [  ] Fail

Test Case 38: Multiple Field Errors
- Action: Submit with invalid hours AND invalid date
- Expected: Both error messages display
- Status: [  ] Pass  [  ] Fail

Test Case 39: CSV Import Multiple Errors
- Action: Upload CSV with 3 invalid rows
- Expected: Display first 10 errors with line numbers
- Status: [  ] Pass  [  ] Fail

Test Case 40: User-Friendly Messages
- Action: View any error message
- Expected: Clear, non-technical language
- Status: [  ] Pass  [  ] Fail
```

### 9. Boundary Testing

```
Test Case 41: Minimum Hours = 0.01
- Action: Submit claim with exactly 0.01 hours
- Expected: Passes validation ✓
- Status: [  ] Pass  [  ] Fail

Test Case 42: Below Minimum = 0.009
- Action: Submit claim with 0.009 hours
- Expected: Fails validation
- Status: [  ] Pass  [  ] Fail

Test Case 43: Maximum Hours = 24
- Action: Submit claim with exactly 24 hours
- Expected: Passes validation ✓
- Status: [  ] Pass  [  ] Fail

Test Case 44: Above Maximum = 24.001
- Action: Submit claim with 24.001 hours
- Expected: Fails validation
- Status: [  ] Pass  [  ] Fail

Test Case 45: Exact Max Amount = R 100,000
- Action: Manager approves claim totaling exactly R 100,000
- Expected: Passes validation ✓
- Status: [  ] Pass  [  ] Fail

Test Case 46: Above Max Amount = R 100,000.01
- Action: Manager approves claim totaling R 100,000.01
- Expected: Fails with amount exceeded message
- Status: [  ] Pass  [  ] Fail
```

### 10. TotalAmount Property Calculation

```
Test Case 47: Calculation Display in Forms
- Action: View claim with 10 hours × R 350/hr
- Expected: All forms show R 3,500.00
- Status: [  ] Pass  [  ] Fail

Test Case 48: Calculation in Reports
- Action: Generate report with multiple claims
- Expected: Each row shows correct calculated total
- Status: [  ] Pass  [  ] Fail

Test Case 49: Calculation in Invoices
- Action: Generate invoice
- Expected: Invoice shows correct total amount
- Status: [  ] Pass  [  ] Fail
```

## Test Execution Summary

### Quick Smoke Test (10 minutes)
1. Submit invalid claim (50 hours) as Lecturer → Should fail ✓
2. Upload CSV with wrong header as HR → Should fail ✓
3. Add XSS comment → Should display as plain text ✓
4. Check calculation displays correctly → Should match Hours × Rate ✓

### Full Test Suite (1 hour)
Run all test cases above and mark pass/fail for each.

## Acceptance Criteria

✓ All test cases pass
✓ No XSS vulnerabilities exploitable
✓ Error messages are clear and helpful
✓ Validations prevent invalid data entry
✓ CSV import handles errors gracefully

---

**Last Updated**: November 19, 2025
