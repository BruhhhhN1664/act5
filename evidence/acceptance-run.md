# Acceptance run (generated)

- Started: 2026-11-20 21:43:28 (machine local time)
- API: http://localhost:5080
- API local date (clock): **2026-11-20**  | database: `equipment_borrowing_dev`
- NOTE: the clock is not 2026-11-10; all dated requests below use the same offsets from this date (reference Section 3).
- PostgreSQL snapshots: enabled

## T01 — Independent API reads (API-01..03, 07, 13..16)

**PostgreSQL snapshot — before** (taken 2026-11-20 21:43:30)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**all equipment** `GET /api/v1/equipment`
→ **200**
```json
[
  {
    "id": "bc8e5771-f0dc-4e26-8e28-2efb2f086072",
    "equipmentCode": "EQ-001",
    "name": "Camera",
    "description": null,
    "isAvailable": false
  },
  {
    "id": "3ac4e4e3-5417-4bcf-9540-4bb762d1a287",
    "equipmentCode": "EQ-002",
    "name": "Laptop",
    "description": null,
    "isAvailable": true
  },
  {
    "id": "d768b03d-e50a-4226-a7dc-72e755c8e700",
    "equipmentCode": "EQ-003",
    "name": "Projector",
    "description": null,
    "isAvailable": true
  },
  {
    "id": "5f0c3fa5-aa43-4dea-abf9-f6388320c2e3",
    "equipmentCode": "EQ-004",
    "name": "Tablet",
    "description": null,
    "isAvailable": false
  },
  {
    "id": "2061881d-7499-42d9-9d05-fe712917abc0",
    "equipmentCode": "EQ-005",
    "name": "Microphone",
    "description": null,
    "isAvailable": false
  },
  {
    "id": "5e40d6f3-53fa-4e81-9ac5-47344aa86372",
    "equipmentCode": "EQ-006",
    "name": "Tripod",
    "description": null,
    "isAvailable": true
  },
  {
    "id": "c3526d97-bce5-462d-8df9-3f147f084702",
    "equipmentCode": "EQ-007",
    "name": "Speaker",
    "description": null,
    "isAvailable": true
  }
]
```
- PASS: 7 equipment items
**available only** `GET /api/v1/equipment?available=true`
→ **200**
```json
[
  {
    "id": "3ac4e4e3-5417-4bcf-9540-4bb762d1a287",
    "equipmentCode": "EQ-002",
    "name": "Laptop",
    "description": null,
    "isAvailable": true
  },
  {
    "id": "d768b03d-e50a-4226-a7dc-72e755c8e700",
    "equipmentCode": "EQ-003",
    "name": "Projector",
    "description": null,
    "isAvailable": true
  },
  {
    "id": "5e40d6f3-53fa-4e81-9ac5-47344aa86372",
    "equipmentCode": "EQ-006",
    "name": "Tripod",
    "description": null,
    "isAvailable": true
  },
  {
    "id": "c3526d97-bce5-462d-8df9-3f147f084702",
    "equipmentCode": "EQ-007",
    "name": "Speaker",
    "description": null,
    "isAvailable": true
  }
]
```
- PASS: available excludes EQ-001/004/005 and includes 002/003/006/007
**** `GET /api/v1/equipment?available=maybe`
→ **400**
```json
{
  "code": "validation_failed",
  "message": "Query parameter \u0027available\u0027 must be true or false."
}
```
- PASS: available=maybe → 400
**all students** `GET /api/v1/students`
→ **200**
```json
[
  {
    "id": "a348f355-0f15-4f22-8b8d-948591fd643f",
    "studentNumber": "ST-100",
    "fullName": "Ada Cruz",
    "contact": null,
    "isEligible": true
  },
  {
    "id": "4292b059-b704-482b-a3c5-726780a63486",
    "studentNumber": "ST-200",
    "fullName": "Ben Diaz",
    "contact": null,
    "isEligible": false
  },
  {
    "id": "4731cce2-9534-4b9c-974a-cd44bc12247a",
    "studentNumber": "ST-300",
    "fullName": "Cara Lim",
    "contact": null,
    "isEligible": true
  },
  {
    "id": "596332f1-86aa-4db3-b8ab-fede3c8f9aa4",
    "studentNumber": "ST-400",
    "fullName": "Dani Ong",
    "contact": null,
    "isEligible": true
  }
]
```
- PASS: 4 students
**** `GET /api/v1/students/a348f355-0f15-4f22-8b8d-948591fd643f`
→ **200**
```json
{
  "id": "a348f355-0f15-4f22-8b8d-948591fd643f",
  "studentNumber": "ST-100",
  "fullName": "Ada Cruz",
  "contact": null,
  "isEligible": true
}
```
- PASS: S1 200 Ada Cruz eligible
**** `GET /api/v1/equipment/bc8e5771-f0dc-4e26-8e28-2efb2f086072`
→ **200**
```json
{
  "id": "bc8e5771-f0dc-4e26-8e28-2efb2f086072",
  "equipmentCode": "EQ-001",
  "name": "Camera",
  "description": null,
  "isAvailable": false
}
```
- PASS: E1 200 unavailable Camera
**S1 history** `GET /api/v1/students/a348f355-0f15-4f22-8b8d-948591fd643f/borrowings`
→ **200**
```json
[
  {
    "id": "0fa9ab36-9d13-456b-aa95-9880b016ce52",
    "studentId": "a348f355-0f15-4f22-8b8d-948591fd643f",
    "equipmentId": "bc8e5771-f0dc-4e26-8e28-2efb2f086072",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  },
  {
    "id": "2d762074-6a02-46ed-84e6-8dade3b7da5e",
    "studentId": "a348f355-0f15-4f22-8b8d-948591fd643f",
    "equipmentId": "d768b03d-e50a-4226-a7dc-72e755c8e700",
    "borrowedDate": "2026-11-02",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": "2026-11-08",
    "status": "Returned",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-003",
    "equipmentName": "Projector"
  }
]
```
- PASS: S1 history has 2 rows (1 Active, 1 Returned)
**** `GET /api/v1/borrowings/0fa9ab36-9d13-456b-aa95-9880b016ce52`
→ **200**
```json
{
  "id": "0fa9ab36-9d13-456b-aa95-9880b016ce52",
  "studentId": "a348f355-0f15-4f22-8b8d-948591fd643f",
  "equipmentId": "bc8e5771-f0dc-4e26-8e28-2efb2f086072",
  "borrowedDate": "2026-11-01",
  "expectedReturnDate": "2026-11-09",
  "returnedDate": null,
  "status": "Active",
  "studentNumber": "ST-100",
  "studentName": "Ada Cruz",
  "equipmentCode": "EQ-001",
  "equipmentName": "Camera"
}
```
- PASS: known borrowing 200 Active
**E1 history** `GET /api/v1/equipment/bc8e5771-f0dc-4e26-8e28-2efb2f086072/borrowings`
→ **200**
```json
[
  {
    "id": "0fa9ab36-9d13-456b-aa95-9880b016ce52",
    "studentId": "a348f355-0f15-4f22-8b8d-948591fd643f",
    "equipmentId": "bc8e5771-f0dc-4e26-8e28-2efb2f086072",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  }
]
```
- PASS: E1 history 1 Active row
**E3 history** `GET /api/v1/equipment/d768b03d-e50a-4226-a7dc-72e755c8e700/borrowings`
→ **200**
```json
[
  {
    "id": "2d762074-6a02-46ed-84e6-8dade3b7da5e",
    "studentId": "a348f355-0f15-4f22-8b8d-948591fd643f",
    "equipmentId": "d768b03d-e50a-4226-a7dc-72e755c8e700",
    "borrowedDate": "2026-11-02",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": "2026-11-08",
    "status": "Returned",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-003",
    "equipmentName": "Projector"
  }
]
```
- PASS: E3 history 1 Returned row
**empty history** `GET /api/v1/students/596332f1-86aa-4db3-b8ab-fede3c8f9aa4/borrowings`
→ **200**
```json
[]
```
- PASS: S4 empty history []
**all borrowings** `GET /api/v1/borrowings`
→ **200**
```json
[
  {
    "id": "0fa9ab36-9d13-456b-aa95-9880b016ce52",
    "studentId": "a348f355-0f15-4f22-8b8d-948591fd643f",
    "equipmentId": "bc8e5771-f0dc-4e26-8e28-2efb2f086072",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  },
  {
    "id": "2d762074-6a02-46ed-84e6-8dade3b7da5e",
    "studentId": "a348f355-0f15-4f22-8b8d-948591fd643f",
    "equipmentId": "d768b03d-e50a-4226-a7dc-72e755c8e700",
    "borrowedDate": "2026-11-02",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": "2026-11-08",
    "status": "Returned",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-003",
    "equipmentName": "Projector"
  },
  {
    "id": "3be0f873-940f-4a68-9741-12d1a6b9ba06",
    "studentId": "4731cce2-9534-4b9c-974a-cd44bc12247a",
    "equipmentId": "2061881d-7499-42d9-9d05-fe712917abc0",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-005",
    "equipmentName": "Microphone"
  },
  {
    "id": "a27df765-b3e3-4ca6-99df-e450c5e8d332",
    "studentId": "4731cce2-9534-4b9c-974a-cd44bc12247a",
    "equipmentId": "5f0c3fa5-aa43-4dea-abf9-f6388320c2e3",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-004",
    "equipmentName": "Tablet"
  }
]
```
- PASS: 4 borrowings
**** `GET /api/v1/students/89142699-e156-402d-bf2c-d51e26583fd3`
→ **404**
```json
{
  "code": "student_not_found",
  "message": "No student with id 89142699-e156-402d-bf2c-d51e26583fd3 exists."
}
```
- PASS: unknown student 404
**** `GET /api/v1/equipment/89142699-e156-402d-bf2c-d51e26583fd3`
→ **404**
```json
{
  "code": "equipment_not_found",
  "message": "No equipment with id 89142699-e156-402d-bf2c-d51e26583fd3 exists."
}
```
- PASS: unknown equipment 404
**** `GET /api/v1/borrowings/89142699-e156-402d-bf2c-d51e26583fd3`
→ **404**
```json
{
  "code": "borrowing_not_found",
  "message": "No borrowing with id 89142699-e156-402d-bf2c-d51e26583fd3 exists."
}
```
- PASS: unknown borrowing 404
**** `GET /api/v1/students/89142699-e156-402d-bf2c-d51e26583fd3/borrowings`
→ **404**
```json
{
  "code": "student_not_found",
  "message": "No student with id 89142699-e156-402d-bf2c-d51e26583fd3 exists."
}
```
- PASS: unknown student history 404
**** `GET /api/v1/equipment/89142699-e156-402d-bf2c-d51e26583fd3/borrowings`
→ **404**
```json
{
  "code": "equipment_not_found",
  "message": "No equipment with id 89142699-e156-402d-bf2c-d51e26583fd3 exists."
}
```
- PASS: unknown equipment history 404
**** `GET /api/v1/nothing-here`
→ **404**
```json
{
  "code": "not_found",
  "message": "The requested route or resource does not exist."
}
```
- PASS: bad route 404 with error body
**** `DELETE /api/v1/equipment`
→ **405**
```json
{
  "code": "method_not_allowed",
  "message": "That HTTP method is not allowed for this route."
}
```
- PASS: wrong method (DELETE /equipment) 405
**OpenAPI** `GET /openapi/v1.json`
→ **200**
```json
{
  "openapi": "3.0.3",
  "info": {
    "title": "Campus Equipment Borrowing API",
    "version": "1.0.0",
    "description": "ITSD 81 Laboratory Activity 5. JSON over HTTP, UTF-8, /api/v1 prefix. Dates are YYYY-MM-DD in the server\u0027s local calendar. Unknown routes return 404 and wrong methods 405, both with the standard error body. A 500 is a received server response; an unreachable API produces no HTTP status at all."
  },
  "servers": [
    {
      "url": "http://localhost:5080"
    }
  ],
  "tags": [
    {
      "name": "Equipment"
    },
    {
      "name": "Students"
    },
    {
      "name": "Borrowings"
    }
  ],
  "paths": {
    "/api/v1/equipment": {
      "get": {
        "operationId": "listEquipment",
        "summary": "API-01: List equipment (optionally only available)",
        "tags": [
          "Equipment"
        ],
        "x-api-id": "API-01",
        "x-use-cases": "UC-01, UC-02",
        "x-tests": "T01, T07",
        "x-business-rules": "BR-05..07, 10",
        "parameters": [
          {
            "name": "available",
            "in": "query",
            "required": false,
            "schema": {
              "type": "boolean"
            },
            "description": "true = only available items; false = only unavailable"
          }
        ],
        "responses": {
          "200": {
            "description": "Array of equipment ([] if none)",
            "content": {
              "application/json": {
                "schema": {
                  "type": "array",
                  "items": {
                    "$ref": "#/components/schemas/Equipment"
                  }
                }
              }
            }
          },
          "400": {
            "descr
... (truncated)
```
- PASS: OpenAPI describes all 16 operations (API-01..16)
**PostgreSQL snapshot — after (must be identical: reads only)** (taken 2026-11-20 21:43:30)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```

## T02 — Students: create/edit/eligibility (API-04..06)

**PostgreSQL snapshot — before** (taken 2026-11-20 21:43:30)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**create** `POST /api/v1/students` body `{"studentNumber":"ST-500","fullName":"Eli Tan","contact":null}`
→ **201** Location: `/api/v1/students/cc3167a7-be5e-4735-a7f5-5882f380d15f`
```json
{
  "id": "cc3167a7-be5e-4735-a7f5-5882f380d15f",
  "studentNumber": "ST-500",
  "fullName": "Eli Tan",
  "contact": null,
  "isEligible": true
}
```
- PASS: 201 + Location
**** `GET /api/v1/students/cc3167a7-be5e-4735-a7f5-5882f380d15f`
→ **200**
```json
{
  "id": "cc3167a7-be5e-4735-a7f5-5882f380d15f",
  "studentNumber": "ST-500",
  "fullName": "Eli Tan",
  "contact": null,
  "isEligible": true
}
```
- PASS: GET new student 200
**edit** `PUT /api/v1/students/cc3167a7-be5e-4735-a7f5-5882f380d15f` body `{"studentNumber":"ST-500","fullName":"Eli Tan Jr.","contact":"eli@example.test"}`
→ **200**
```json
{
  "id": "cc3167a7-be5e-4735-a7f5-5882f380d15f",
  "studentNumber": "ST-500",
  "fullName": "Eli Tan Jr.",
  "contact": "eli@example.test",
  "isEligible": true
}
```
- PASS: PUT 200 persisted new name/contact
**** `POST /api/v1/students` body `{"studentNumber":"ST-501","fullName":"  "}`
→ **400**
```json
{
  "code": "validation_failed",
  "message": "fullName is required and cannot be blank."
}
```
- PASS: blank name → 400
**** `POST /api/v1/students` body `{"studentNumber":" st-100 ","fullName":"Dup"}`
→ **409**
```json
{
  "code": "duplicate_student_number",
  "message": "A student with number \u0027st-100\u0027 already exists."
}
```
- PASS: duplicate ' st-100 ' → 409
- PASS: student count is 5 (no duplicate/blank added)
**restrict** `PATCH /api/v1/students/4710a9ee-cce0-4035-a409-53a76ef642d4/eligibility` body `{"isEligible":false}`
→ **200**
```json
{
  "id": "4710a9ee-cce0-4035-a409-53a76ef642d4",
  "studentNumber": "ST-100",
  "fullName": "Ada Cruz",
  "contact": null,
  "isEligible": false
}
```
- PASS: restrict S1 → 200
**issue while restricted** `POST /api/v1/borrowings` body `{"studentId":"4710a9ee-cce0-4035-a409-53a76ef642d4","equipmentId":"035519b1-115c-4970-a445-28d72914e99e","expectedReturnDate":"2026-11-22"}`
→ **409**
```json
{
  "code": "student_ineligible",
  "message": "Student ST-100 is not eligible to borrow equipment."
}
```
- PASS: issue to restricted S1 → 409
- PASS: restricting kept S1 history (2 rows) and active loan
**return E1** `POST /api/v1/borrowings/94eddb2c-6907-4bd1-a21e-bad9ae43ac63/return` body `{}`
→ **200**
```json
{
  "id": "94eddb2c-6907-4bd1-a21e-bad9ae43ac63",
  "studentId": "4710a9ee-cce0-4035-a409-53a76ef642d4",
  "equipmentId": "9885350a-0012-48ac-924f-6ec3a1696798",
  "borrowedDate": "2026-11-01",
  "expectedReturnDate": "2026-11-09",
  "returnedDate": "2026-11-20",
  "status": "Returned",
  "studentNumber": "ST-100",
  "studentName": "Ada Cruz",
  "equipmentCode": "EQ-001",
  "equipmentName": "Camera"
}
```
- PASS: return E1 → 200
**permit** `PATCH /api/v1/students/4710a9ee-cce0-4035-a409-53a76ef642d4/eligibility` body `{"isEligible":true}`
→ **200**
```json
{
  "id": "4710a9ee-cce0-4035-a409-53a76ef642d4",
  "studentNumber": "ST-100",
  "fullName": "Ada Cruz",
  "contact": null,
  "isEligible": true
}
```
- PASS: permit S1 → 200
**valid issue** `POST /api/v1/borrowings` body `{"studentId":"4710a9ee-cce0-4035-a409-53a76ef642d4","equipmentId":"035519b1-115c-4970-a445-28d72914e99e","expectedReturnDate":"2026-11-22"}`
→ **201** Location: `/api/v1/borrowings/141a5710-70f2-4bb0-912a-1244a16fc9a0`
```json
{
  "id": "141a5710-70f2-4bb0-912a-1244a16fc9a0",
  "studentId": "4710a9ee-cce0-4035-a409-53a76ef642d4",
  "equipmentId": "035519b1-115c-4970-a445-28d72914e99e",
  "borrowedDate": "2026-11-20",
  "expectedReturnDate": "2026-11-22",
  "returnedDate": null,
  "status": "Active",
  "studentNumber": "ST-100",
  "studentName": "Ada Cruz",
  "equipmentCode": "EQ-002",
  "equipmentName": "Laptop"
}
```
- PASS: valid issue after permit → 201
**same eligibility again** `PATCH /api/v1/students/4710a9ee-cce0-4035-a409-53a76ef642d4/eligibility` body `{"isEligible":true}`
→ **200**
```json
{
  "id": "4710a9ee-cce0-4035-a409-53a76ef642d4",
  "studentNumber": "ST-100",
  "fullName": "Ada Cruz",
  "contact": null,
  "isEligible": true
}
```
- PASS: same state → 200 no-op
- PASS: S1 history now 3 rows (nothing erased)
**PostgreSQL snapshot — after** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
   ST-500 | Eli Tan Jr. | True
-- Equipment: code | name | available
   EQ-001 | Camera | True
   EQ-002 | Laptop | False
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | 20/11/2026 12:00:00 AM
   ST-100 | EQ-002 | 20/11/2026 12:00:00 AM | 22/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```

## T03 — Equipment: create/edit, protected state (API-07..09)

**PostgreSQL snapshot — before** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**create** `POST /api/v1/equipment` body `{"equipmentCode":"EQ-008","name":"Charger","description":null}`
→ **201** Location: `/api/v1/equipment/73563c37-50c0-4d67-9dbf-fb95b5d4c555`
```json
{
  "id": "73563c37-50c0-4d67-9dbf-fb95b5d4c555",
  "equipmentCode": "EQ-008",
  "name": "Charger",
  "description": null,
  "isAvailable": true
}
```
- PASS: 201 + available=true
**** `GET /api/v1/equipment/73563c37-50c0-4d67-9dbf-fb95b5d4c555`
→ **200**
```json
{
  "id": "73563c37-50c0-4d67-9dbf-fb95b5d4c555",
  "equipmentCode": "EQ-008",
  "name": "Charger",
  "description": null,
  "isAvailable": true
}
```
- PASS: GET created 200
**edit** `PUT /api/v1/equipment/73563c37-50c0-4d67-9dbf-fb95b5d4c555` body `{"equipmentCode":"EQ-008","name":"Fast charger","description":null}`
→ **200**
```json
{
  "id": "73563c37-50c0-4d67-9dbf-fb95b5d4c555",
  "equipmentCode": "EQ-008",
  "name": "Fast charger",
  "description": null,
  "isAvailable": true
}
```
- PASS: PUT edited name 200
**describe E1** `PUT /api/v1/equipment/0213b767-44d7-456c-b428-afcaa15bd664` body `{"equipmentCode":"EQ-001","name":"Camera","description":"DSLR body"}`
→ **200**
```json
{
  "id": "0213b767-44d7-456c-b428-afcaa15bd664",
  "equipmentCode": "EQ-001",
  "name": "Camera",
  "description": "DSLR body",
  "isAvailable": false
}
```
- PASS: PUT E1 descriptive change 200
**** `POST /api/v1/equipment` body `{"equipmentCode":"EQ-009","name":""}`
→ **400**
```json
{
  "code": "validation_failed",
  "message": "name is required and cannot be blank."
}
```
- PASS: blank name → 400
**** `POST /api/v1/equipment` body `{"equipmentCode":" eq-001 ","name":"Dup"}`
→ **409**
```json
{
  "code": "duplicate_equipment_code",
  "message": "Equipment with code \u0027eq-001\u0027 already exists."
}
```
- PASS: duplicate ' eq-001 ' → 409
**protected field** `PUT /api/v1/equipment/0213b767-44d7-456c-b428-afcaa15bd664` body `{"equipmentCode":"EQ-001","name":"Camera","isAvailable":true}`
→ **400**
```json
{
  "code": "invalid_request",
  "message": "The request is not valid. Check the JSON syntax and that every field is present, correctly typed, and allowed."
}
```
- PASS: isAvailable in update → 400
- PASS: E1 still unavailable
- PASS: E1 history intact (1 row)
- PASS: equipment count is 8
**PostgreSQL snapshot — after** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
   EQ-008 | Fast charger | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```

## T04 — Single issue and return (API-10, 12, 13)

**PostgreSQL snapshot — before** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**issue S4/E2** `POST /api/v1/borrowings` body `{"studentId":"27045b1f-555f-4a2a-afe4-fd3b84cd1b95","equipmentId":"f9d4a770-f7ef-4e5e-a632-8710ae3d9750","expectedReturnDate":"2026-11-22"}`
→ **201** Location: `/api/v1/borrowings/c95c4940-1ec4-4c92-8748-f1f418a53a99`
```json
{
  "id": "c95c4940-1ec4-4c92-8748-f1f418a53a99",
  "studentId": "27045b1f-555f-4a2a-afe4-fd3b84cd1b95",
  "equipmentId": "f9d4a770-f7ef-4e5e-a632-8710ae3d9750",
  "borrowedDate": "2026-11-20",
  "expectedReturnDate": "2026-11-22",
  "returnedDate": null,
  "status": "Active",
  "studentNumber": "ST-400",
  "studentName": "Dani Ong",
  "equipmentCode": "EQ-002",
  "equipmentName": "Laptop"
}
```
- PASS: 201 Active
**** `GET /api/v1/borrowings/c95c4940-1ec4-4c92-8748-f1f418a53a99`
→ **200**
```json
{
  "id": "c95c4940-1ec4-4c92-8748-f1f418a53a99",
  "studentId": "27045b1f-555f-4a2a-afe4-fd3b84cd1b95",
  "equipmentId": "f9d4a770-f7ef-4e5e-a632-8710ae3d9750",
  "borrowedDate": "2026-11-20",
  "expectedReturnDate": "2026-11-22",
  "returnedDate": null,
  "status": "Active",
  "studentNumber": "ST-400",
  "studentName": "Dani Ong",
  "equipmentCode": "EQ-002",
  "equipmentName": "Laptop"
}
```
- PASS: GET borrowing 200
- PASS: E2 now unavailable
**** `POST /api/v1/borrowings` body `{"studentId":"30dd41ae-a886-4e2a-905e-8c0f6cf1c3b3","equipmentId":"0ae9d17b-b98a-4517-b135-24d5b39ca334","expectedReturnDate":"2026-11-22"}`
→ **409**
```json
{
  "code": "student_ineligible",
  "message": "Student ST-200 is not eligible to borrow equipment."
}
```
- PASS: ineligible S2 → 409
**** `POST /api/v1/borrowings` body `{"studentId":"27045b1f-555f-4a2a-afe4-fd3b84cd1b95","equipmentId":"3b0ec6f0-ed08-4c8d-97a5-139cf8b88987","expectedReturnDate":"2026-11-22"}`
→ **409**
```json
{
  "code": "equipment_unavailable",
  "message": "Not available: EQ-001."
}
```
- PASS: unavailable E1 → 409
**** `POST /api/v1/borrowings` body `{"studentId":"50aed96e-2a8a-4b02-8584-1cf89f182471","equipmentId":"0ae9d17b-b98a-4517-b135-24d5b39ca334","expectedReturnDate":"2026-11-22"}`
→ **404**
```json
{
  "code": "student_not_found",
  "message": "No student with id 50aed96e-2a8a-4b02-8584-1cf89f182471 exists."
}
```
- PASS: missing student → 404
**** `POST /api/v1/borrowings` body `{"studentId":"27045b1f-555f-4a2a-afe4-fd3b84cd1b95","equipmentId":"109baeea-5415-4fe4-9546-86ec0f56cc15","expectedReturnDate":"2026-11-22"}`
→ **404**
```json
{
  "code": "equipment_not_found",
  "message": "No equipment with id 109baeea-5415-4fe4-9546-86ec0f56cc15 exists."
}
```
- PASS: missing equipment → 404
**** `POST /api/v1/borrowings` body `{"studentId":"27045b1f-555f-4a2a-afe4-fd3b84cd1b95","equipmentId":"0ae9d17b-b98a-4517-b135-24d5b39ca334","expectedReturnDate":"2026-11-20"}`
→ **400**
```json
{
  "code": "invalid_return_date",
  "message": "expectedReturnDate must be later than today (2026-11-20)."
}
```
- PASS: date not after today → 400
**** `POST /api/v1/borrowings` body `{"studentId":"27045b1f-555f-4a2a-afe4-fd3b84cd1b95","equipmentId":"0ae9d17b-b98a-4517-b135-24d5b39ca334","expectedReturnDate":"2026-11-19"}`
→ **400**
```json
{
  "code": "invalid_return_date",
  "message": "expectedReturnDate must be later than today (2026-11-20)."
}
```
- PASS: date yesterday → 400
**** `POST /api/v1/borrowings` body `{"studentId":"27045b1f-555f-4a2a-afe4-fd3b84cd1b95","equipmentId":"f9d4a770-f7ef-4e5e-a632-8710ae3d9750","expectedReturnDate":"11/12/2026"}`
→ **400**
```json
{
  "code": "invalid_request",
  "message": "The request is not valid. Check the JSON syntax and that every field is present, correctly typed, and allowed."
}
```
- PASS: malformed date → 400
- PASS: rejected attempts wrote nothing (row count unchanged)
**return** `POST /api/v1/borrowings/c95c4940-1ec4-4c92-8748-f1f418a53a99/return` body `{}`
→ **200**
```json
{
  "id": "c95c4940-1ec4-4c92-8748-f1f418a53a99",
  "studentId": "27045b1f-555f-4a2a-afe4-fd3b84cd1b95",
  "equipmentId": "f9d4a770-f7ef-4e5e-a632-8710ae3d9750",
  "borrowedDate": "2026-11-20",
  "expectedReturnDate": "2026-11-22",
  "returnedDate": "2026-11-20",
  "status": "Returned",
  "studentNumber": "ST-400",
  "studentName": "Dani Ong",
  "equipmentCode": "EQ-002",
  "equipmentName": "Laptop"
}
```
- PASS: return 200 with returnedDate
- PASS: E2 available again
**repeat return** `POST /api/v1/borrowings/c95c4940-1ec4-4c92-8748-f1f418a53a99/return` body `{}`
→ **409**
```json
{
  "code": "already_returned",
  "message": "This borrowing has already been returned."
}
```
- PASS: repeat return → 409
**** `POST /api/v1/borrowings/f5c70a58-1fba-4636-9781-7a6f2e5fab93/return` body `{}`
→ **404**
```json
{
  "code": "borrowing_not_found",
  "message": "No borrowing with id f5c70a58-1fba-4636-9781-7a6f2e5fab93 exists."
}
```
- PASS: return unknown id → 404
**PostgreSQL snapshot — after (restart the API now and re-run the readbacks for the persistence evidence)** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-400 | EQ-002 | 20/11/2026 12:00:00 AM | 22/11/2026 12:00:00 AM | 20/11/2026 12:00:00 AM
```

## T05 — Multi-item success, limit boundary, invalid requests, atomic rollback (API-10, 11)

**PostgreSQL snapshot — 1) before S4 E2+E6** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**1) checkout S4 E2/E6** `POST /api/v1/checkout-requests` body `{"studentId":"fd807b49-e789-4b62-9234-ecad7974feaa","equipmentIds":["2cbce7e2-da39-44e5-9269-ad9e7c53fda3","74ee685f-4409-4f54-b6b1-8717b57a1b65"],"expectedReturnDate":"2026-11-22"}`
→ **201** Location: `/api/v1/borrowings/31c449f7-8c4d-4ac0-a452-2e8852b891bd`
```json
{
  "borrowings": [
    {
      "id": "31c449f7-8c4d-4ac0-a452-2e8852b891bd",
      "studentId": "fd807b49-e789-4b62-9234-ecad7974feaa",
      "equipmentId": "2cbce7e2-da39-44e5-9269-ad9e7c53fda3",
      "borrowedDate": "2026-11-20",
      "expectedReturnDate": "2026-11-22",
      "returnedDate": null,
      "status": "Active",
      "studentNumber": "ST-400",
      "studentName": "Dani Ong",
      "equipmentCode": "EQ-002",
      "equipmentName": "Laptop"
    },
    {
      "id": "4c41f941-8858-457e-a015-1a564acb4567",
      "studentId": "fd807b49-e789-4b62-9234-ecad7974feaa",
      "equipmentId": "74ee685f-4409-4f54-b6b1-8717b57a1b65",
      "borrowedDate": "2026-11-20",
      "expectedReturnDate": "2026-11-22",
      "returnedDate": null,
      "status": "Active",
      "studentNumber": "ST-400",
      "studentName": "Dani Ong",
      "equipmentCode": "EQ-006",
      "equipmentName": "Tripod"
    }
  ]
}
```
- PASS: 201 with 2 borrowings
**PostgreSQL snapshot — 1) after** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | False
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | False
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-400 | EQ-002 | 20/11/2026 12:00:00 AM | 22/11/2026 12:00:00 AM | NULL
   ST-400 | EQ-006 | 20/11/2026 12:00:00 AM | 22/11/2026 12:00:00 AM | NULL
```
**2a) S3 (2 active) + E2 via API-10** `POST /api/v1/borrowings` body `{"studentId":"8142c8e9-78ab-4d59-98f9-1e9104c6b192","equipmentId":"78252b4c-c989-4516-a552-eb53df6feeb6","expectedReturnDate":"2026-11-22"}`
→ **201** Location: `/api/v1/borrowings/f8d46062-6f2c-4f0f-a461-f9913e5f98ad`
```json
{
  "id": "f8d46062-6f2c-4f0f-a461-f9913e5f98ad",
  "studentId": "8142c8e9-78ab-4d59-98f9-1e9104c6b192",
  "equipmentId": "78252b4c-c989-4516-a552-eb53df6feeb6",
  "borrowedDate": "2026-11-20",
  "expectedReturnDate": "2026-11-22",
  "returnedDate": null,
  "status": "Active",
  "studentNumber": "ST-300",
  "studentName": "Cara Lim",
  "equipmentCode": "EQ-002",
  "equipmentName": "Laptop"
}
```
- PASS: 2a 201 (exactly three active)
- PASS: 2a S3 has 3 active
**PostgreSQL snapshot — 2a) after** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | False
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-002 | 20/11/2026 12:00:00 AM | 22/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**2b) S1 (1 active) + E2/E6 via API-11** `POST /api/v1/checkout-requests` body `{"studentId":"08d998a9-3f33-4b8a-96ce-1d5b2bc76bee","equipmentIds":["ed94a32d-6954-4c31-80a5-883644f33a12","a8538021-5277-45f0-80eb-0e66cd1574ba"],"expectedReturnDate":"2026-11-22"}`
→ **201** Location: `/api/v1/borrowings/718a9708-7521-4e9d-8c12-638ef0a37416`
```json
{
  "borrowings": [
    {
      "id": "718a9708-7521-4e9d-8c12-638ef0a37416",
      "studentId": "08d998a9-3f33-4b8a-96ce-1d5b2bc76bee",
      "equipmentId": "ed94a32d-6954-4c31-80a5-883644f33a12",
      "borrowedDate": "2026-11-20",
      "expectedReturnDate": "2026-11-22",
      "returnedDate": null,
      "status": "Active",
      "studentNumber": "ST-100",
      "studentName": "Ada Cruz",
      "equipmentCode": "EQ-002",
      "equipmentName": "Laptop"
    },
    {
      "id": "bb014601-a86e-4c90-9a7a-acc35ff85d1f",
      "studentId": "08d998a9-3f33-4b8a-96ce-1d5b2bc76bee",
      "equipmentId": "a8538021-5277-45f0-80eb-0e66cd1574ba",
      "borrowedDate": "2026-11-20",
      "expectedReturnDate": "2026-11-22",
      "returnedDate": null,
      "status": "Active",
      "studentNumber": "ST-100",
      "studentName": "Ada Cruz",
      "equipmentCode": "EQ-006",
      "equipmentName": "Tripod"
    }
  ]
}
```
- PASS: 2b 201 with two
- PASS: 2b S1 has 3 active
**PostgreSQL snapshot — 2b) after** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | False
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | False
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-002 | 20/11/2026 12:00:00 AM | 22/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-100 | EQ-006 | 20/11/2026 12:00:00 AM | 22/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**PostgreSQL snapshot — 2c) before** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**2c) S3 (2 active) + E2/E6 → over limit** `POST /api/v1/checkout-requests` body `{"studentId":"5b08b811-87ad-4cfa-b21c-fcfc69f4073d","equipmentIds":["36c455e9-e6c8-4314-99a0-6037d1269d33","700221a7-307b-420f-9de7-0081c69ffb13"],"expectedReturnDate":"2026-11-22"}`
→ **409**
```json
{
  "code": "borrowing_limit_exceeded",
  "message": "Student ST-300 has 2 active borrowing(s); adding 2 would exceed the limit of 3."
}
```
- PASS: 2c 409
- PASS: 2c no rows added; E2/E6 still available
**PostgreSQL snapshot — 2c) after (must equal before)** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**PostgreSQL snapshot — 3) before invalid requests** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**** `POST /api/v1/checkout-requests` body `{"studentId":"b2773049-2977-4598-bc72-bde44a3ecd9b","equipmentIds":["cbf68b90-db9e-4dec-b49d-08ecb74e4a34","cbf68b90-db9e-4dec-b49d-08ecb74e4a34"],"expectedReturnDate":"2026-11-22"}`
→ **400**
```json
{
  "code": "validation_failed",
  "message": "equipmentIds must not contain duplicates."
}
```
- PASS: 3 duplicate ids → 400
**** `POST /api/v1/checkout-requests` body `{"studentId":"b2773049-2977-4598-bc72-bde44a3ecd9b","equipmentIds":["cbf68b90-db9e-4dec-b49d-08ecb74e4a34"],"expectedReturnDate":"2026-11-22"}`
→ **400**
```json
{
  "code": "validation_failed",
  "message": "A multi-item checkout needs at least two equipment ids. Use POST /api/v1/borrowings for one item."
}
```
- PASS: 3 one item → 400
**** `POST /api/v1/checkout-requests` body `{"studentId":"b2773049-2977-4598-bc72-bde44a3ecd9b","equipmentIds":["10685ce2-a1ca-4f89-b476-e414f686366c","cbf68b90-db9e-4dec-b49d-08ecb74e4a34"],"expectedReturnDate":"2026-11-22"}`
→ **409**
```json
{
  "code": "equipment_unavailable",
  "message": "Not available: EQ-001."
}
```
- PASS: 3 E1+E2 (E1 unavailable) → 409
**** `POST /api/v1/checkout-requests` body `{"studentId":"5ce802c1-0d61-4c85-9474-4e5a6c1b8978","equipmentIds":["cbf68b90-db9e-4dec-b49d-08ecb74e4a34","2dbb5bd3-fa03-451f-90d0-88af7f3a9b4a"],"expectedReturnDate":"2026-11-22"}`
→ **409**
```json
{
  "code": "student_ineligible",
  "message": "Student ST-200 is not eligible to borrow equipment."
}
```
- PASS: 3 ineligible S2 → 409
**** `POST /api/v1/checkout-requests` body `{"studentId":"b2773049-2977-4598-bc72-bde44a3ecd9b","equipmentIds":["cbf68b90-db9e-4dec-b49d-08ecb74e4a34","e4669147-2998-4529-9593-5aa575a61c60"],"expectedReturnDate":"2026-11-22"}`
→ **404**
```json
{
  "code": "equipment_not_found",
  "message": "No equipment with id e4669147-2998-4529-9593-5aa575a61c60 exists."
}
```
- PASS: 3 missing item → 404
**** `POST /api/v1/checkout-requests` body `{"studentId":"9af24701-69b6-476e-af8b-1e800ba4d37d","equipmentIds":["cbf68b90-db9e-4dec-b49d-08ecb74e4a34","2dbb5bd3-fa03-451f-90d0-88af7f3a9b4a"],"expectedReturnDate":"2026-11-22"}`
→ **404**
```json
{
  "code": "student_not_found",
  "message": "No student with id 9af24701-69b6-476e-af8b-1e800ba4d37d exists."
}
```
- PASS: 3 missing student → 404
**** `POST /api/v1/checkout-requests` body `{"studentId":"b2773049-2977-4598-bc72-bde44a3ecd9b","equipmentIds":["cbf68b90-db9e-4dec-b49d-08ecb74e4a34","2dbb5bd3-fa03-451f-90d0-88af7f3a9b4a"],"expectedReturnDate":"2026-11-19"}`
→ **400**
```json
{
  "code": "invalid_return_date",
  "message": "expectedReturnDate must be later than today (2026-11-20)."
}
```
- PASS: 3 invalid date → 400
- PASS: 3 no partial rows from any invalid request
**PostgreSQL snapshot — 3) after (must equal before)** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**PostgreSQL snapshot — 4) before injected failure** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**arm fault seam** `POST /dev/fault?armed=true` → 200
**4) checkout with failure injected AFTER the first row is written** `POST /api/v1/checkout-requests` body `{"studentId":"862153c9-a3f1-4a09-9152-103d20cf2588","equipmentIds":["01832edb-56f0-4dd7-950f-d86d7c851522","08e03d84-873b-44b4-b6b4-edc3e60d5316"],"expectedReturnDate":"2026-11-22"}`
→ **500**
```json
{
  "code": "server_error",
  "message": "An unexpected server error occurred. If you were saving data, check the current records before retrying."
}
```
- PASS: 4 received HTTP 500 (a server response, not a transport failure)
- PASS: 4 safe body (no stack trace / connection string)
- PASS: 4 ROLLBACK: borrowing row count unchanged
- PASS: 4 ROLLBACK: E2 and E6 still available
**PostgreSQL snapshot — 4) after (must equal before: documented failure point = after first borrowing row write inside the transaction)** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**4) retry after the one-shot fault disarmed** `POST /api/v1/checkout-requests` body `{"studentId":"862153c9-a3f1-4a09-9152-103d20cf2588","equipmentIds":["01832edb-56f0-4dd7-950f-d86d7c851522","08e03d84-873b-44b4-b6b4-edc3e60d5316"],"expectedReturnDate":"2026-11-22"}`
→ **201** Location: `/api/v1/borrowings/857c1d9d-f34b-45ad-8fd1-6b3d1ba936fb`
```json
{
  "borrowings": [
    {
      "id": "857c1d9d-f34b-45ad-8fd1-6b3d1ba936fb",
      "studentId": "862153c9-a3f1-4a09-9152-103d20cf2588",
      "equipmentId": "01832edb-56f0-4dd7-950f-d86d7c851522",
      "borrowedDate": "2026-11-20",
      "expectedReturnDate": "2026-11-22",
      "returnedDate": null,
      "status": "Active",
      "studentNumber": "ST-400",
      "studentName": "Dani Ong",
      "equipmentCode": "EQ-002",
      "equipmentName": "Laptop"
    },
    {
      "id": "16a8e78d-46da-4452-8231-6b8bc36afb39",
      "studentId": "862153c9-a3f1-4a09-9152-103d20cf2588",
      "equipmentId": "08e03d84-873b-44b4-b6b4-edc3e60d5316",
      "borrowedDate": "2026-11-20",
      "expectedReturnDate": "2026-11-22",
      "returnedDate": null,
      "status": "Active",
      "studentNumber": "ST-400",
      "studentName": "Dani Ong",
      "equipmentCode": "EQ-006",
      "equipmentName": "Tripod"
    }
  ]
}
```
- PASS: 4 retry succeeds 201

## T06 — Histories, search, filters, sorting (API-14..16)

**PostgreSQL snapshot — before** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```
**default (All, borrowedDate, asc)** `GET /api/v1/borrowings`
→ **200**
```json
[
  {
    "id": "83017a90-395c-4529-b33c-45c9673cc297",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "ead2e63e-899e-48de-9f07-ff1eb65a5836",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  },
  {
    "id": "0eafe204-788e-4538-aeee-96107a8d70ff",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "41dfcb27-35fc-49ad-892e-48837ab64dc4",
    "borrowedDate": "2026-11-02",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": "2026-11-08",
    "status": "Returned",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-003",
    "equipmentName": "Projector"
  },
  {
    "id": "40639009-bdb0-4ab5-865e-bb62abf47863",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "11af4494-9943-4caa-ba0e-bdc5183793ce",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-004",
    "equipmentName": "Tablet"
  },
  {
    "id": "4c2b7d8f-e909-4236-b68c-236c15af28cf",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "6488656b-4b6d-415d-9a46-380d43c52bcf",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-005",
    "equipmentName": "Microphone"
  }
]
```
- PASS: 4 rows; E1 (11-01) first, E3 (11-02) second
**** `GET /api/v1/borrowings?search=aDa`
→ **200**
```json
[
  {
    "id": "83017a90-395c-4529-b33c-45c9673cc297",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "ead2e63e-899e-48de-9f07-ff1eb65a5836",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  },
  {
    "id": "0eafe204-788e-4538-aeee-96107a8d70ff",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "41dfcb27-35fc-49ad-892e-48837ab64dc4",
    "borrowedDate": "2026-11-02",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": "2026-11-08",
    "status": "Returned",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-003",
    "equipmentName": "Projector"
  }
]
```
- PASS: search 'aDa' (mixed case name) → 2 rows (S1)
**** `GET /api/v1/borrowings?search=st-300`
→ **200**
```json
[
  {
    "id": "40639009-bdb0-4ab5-865e-bb62abf47863",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "11af4494-9943-4caa-ba0e-bdc5183793ce",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-004",
    "equipmentName": "Tablet"
  },
  {
    "id": "4c2b7d8f-e909-4236-b68c-236c15af28cf",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "6488656b-4b6d-415d-9a46-380d43c52bcf",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-005",
    "equipmentName": "Microphone"
  }
]
```
- PASS: search 'st-300' (number) → 2 rows (S3)
**** `GET /api/v1/borrowings?search=CAM`
→ **200**
```json
[
  {
    "id": "83017a90-395c-4529-b33c-45c9673cc297",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "ead2e63e-899e-48de-9f07-ff1eb65a5836",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  }
]
```
- PASS: search 'CAM' (equipment partial) → 1 row (E1)
**** `GET /api/v1/borrowings?status=Active`
→ **200**
```json
[
  {
    "id": "83017a90-395c-4529-b33c-45c9673cc297",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "ead2e63e-899e-48de-9f07-ff1eb65a5836",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  },
  {
    "id": "40639009-bdb0-4ab5-865e-bb62abf47863",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "11af4494-9943-4caa-ba0e-bdc5183793ce",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-004",
    "equipmentName": "Tablet"
  },
  {
    "id": "4c2b7d8f-e909-4236-b68c-236c15af28cf",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "6488656b-4b6d-415d-9a46-380d43c52bcf",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-005",
    "equipmentName": "Microphone"
  }
]
```
- PASS: status=Active → 3
**** `GET /api/v1/borrowings?status=Returned`
→ **200**
```json
[
  {
    "id": "0eafe204-788e-4538-aeee-96107a8d70ff",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "41dfcb27-35fc-49ad-892e-48837ab64dc4",
    "borrowedDate": "2026-11-02",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": "2026-11-08",
    "status": "Returned",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-003",
    "equipmentName": "Projector"
  }
]
```
- PASS: status=Returned → 1
**** `GET /api/v1/borrowings?status=All`
→ **200**
```json
[
  {
    "id": "83017a90-395c-4529-b33c-45c9673cc297",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "ead2e63e-899e-48de-9f07-ff1eb65a5836",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  },
  {
    "id": "0eafe204-788e-4538-aeee-96107a8d70ff",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "41dfcb27-35fc-49ad-892e-48837ab64dc4",
    "borrowedDate": "2026-11-02",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": "2026-11-08",
    "status": "Returned",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-003",
    "equipmentName": "Projector"
  },
  {
    "id": "40639009-bdb0-4ab5-865e-bb62abf47863",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "11af4494-9943-4caa-ba0e-bdc5183793ce",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-004",
    "equipmentName": "Tablet"
  },
  {
    "id": "4c2b7d8f-e909-4236-b68c-236c15af28cf",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "6488656b-4b6d-415d-9a46-380d43c52bcf",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-005",
    "equipmentName": "Microphone"
  }
]
```
- PASS: status=All → 4
**** `GET /api/v1/borrowings?sort=borrowedDate&direction=desc`
→ **200**
```json
[
  {
    "id": "4c2b7d8f-e909-4236-b68c-236c15af28cf",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "6488656b-4b6d-415d-9a46-380d43c52bcf",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-005",
    "equipmentName": "Microphone"
  },
  {
    "id": "40639009-bdb0-4ab5-865e-bb62abf47863",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "11af4494-9943-4caa-ba0e-bdc5183793ce",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-004",
    "equipmentName": "Tablet"
  },
  {
    "id": "0eafe204-788e-4538-aeee-96107a8d70ff",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "41dfcb27-35fc-49ad-892e-48837ab64dc4",
    "borrowedDate": "2026-11-02",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": "2026-11-08",
    "status": "Returned",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-003",
    "equipmentName": "Projector"
  },
  {
    "id": "83017a90-395c-4529-b33c-45c9673cc297",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "ead2e63e-899e-48de-9f07-ff1eb65a5836",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  }
]
```
- PASS: borrowedDate desc: E4/E5 (11-03) first
**** `GET /api/v1/borrowings?sort=expectedReturnDate&direction=asc`
→ **200**
```json
[
  {
    "id": "83017a90-395c-4529-b33c-45c9673cc297",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "ead2e63e-899e-48de-9f07-ff1eb65a5836",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  },
  {
    "id": "0eafe204-788e-4538-aeee-96107a8d70ff",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "41dfcb27-35fc-49ad-892e-48837ab64dc4",
    "borrowedDate": "2026-11-02",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": "2026-11-08",
    "status": "Returned",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-003",
    "equipmentName": "Projector"
  },
  {
    "id": "40639009-bdb0-4ab5-865e-bb62abf47863",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "11af4494-9943-4caa-ba0e-bdc5183793ce",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-004",
    "equipmentName": "Tablet"
  },
  {
    "id": "4c2b7d8f-e909-4236-b68c-236c15af28cf",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "6488656b-4b6d-415d-9a46-380d43c52bcf",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-005",
    "equipmentName": "Microphone"
  }
]
```
- PASS: expectedReturnDate asc: EQ-001 (11-09) first
**** `GET /api/v1/borrowings?sort=expectedReturnDate&direction=desc`
→ **200**
```json
[
  {
    "id": "4c2b7d8f-e909-4236-b68c-236c15af28cf",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "6488656b-4b6d-415d-9a46-380d43c52bcf",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-005",
    "equipmentName": "Microphone"
  },
  {
    "id": "40639009-bdb0-4ab5-865e-bb62abf47863",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "11af4494-9943-4caa-ba0e-bdc5183793ce",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-004",
    "equipmentName": "Tablet"
  },
  {
    "id": "0eafe204-788e-4538-aeee-96107a8d70ff",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "41dfcb27-35fc-49ad-892e-48837ab64dc4",
    "borrowedDate": "2026-11-02",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": "2026-11-08",
    "status": "Returned",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-003",
    "equipmentName": "Projector"
  },
  {
    "id": "83017a90-395c-4529-b33c-45c9673cc297",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "ead2e63e-899e-48de-9f07-ff1eb65a5836",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  }
]
```
- PASS: expectedReturnDate desc: EQ-001 last
**** `GET /api/v1/borrowings?search=cruz&status=Active`
→ **200**
```json
[
  {
    "id": "83017a90-395c-4529-b33c-45c9673cc297",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "ead2e63e-899e-48de-9f07-ff1eb65a5836",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  }
]
```
- PASS: combined search=cruz + status=Active → 1 (EQ-001)
**** `GET /api/v1/borrowings?search=&status=All`
→ **200**
```json
[
  {
    "id": "83017a90-395c-4529-b33c-45c9673cc297",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "ead2e63e-899e-48de-9f07-ff1eb65a5836",
    "borrowedDate": "2026-11-01",
    "expectedReturnDate": "2026-11-09",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-001",
    "equipmentName": "Camera"
  },
  {
    "id": "0eafe204-788e-4538-aeee-96107a8d70ff",
    "studentId": "0c0b777a-ad33-48d7-9245-67135d6ab568",
    "equipmentId": "41dfcb27-35fc-49ad-892e-48837ab64dc4",
    "borrowedDate": "2026-11-02",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": "2026-11-08",
    "status": "Returned",
    "studentNumber": "ST-100",
    "studentName": "Ada Cruz",
    "equipmentCode": "EQ-003",
    "equipmentName": "Projector"
  },
  {
    "id": "40639009-bdb0-4ab5-865e-bb62abf47863",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "11af4494-9943-4caa-ba0e-bdc5183793ce",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-004",
    "equipmentName": "Tablet"
  },
  {
    "id": "4c2b7d8f-e909-4236-b68c-236c15af28cf",
    "studentId": "cade76b0-d0f6-4cda-8da8-259923e8f369",
    "equipmentId": "6488656b-4b6d-415d-9a46-380d43c52bcf",
    "borrowedDate": "2026-11-03",
    "expectedReturnDate": "2026-11-12",
    "returnedDate": null,
    "status": "Active",
    "studentNumber": "ST-300",
    "studentName": "Cara Lim",
    "equipmentCode": "EQ-005",
    "equipmentName": "Microphone"
  }
]
```
- PASS: cleared criteria restores all 4
**** `GET /api/v1/borrowings?search=zzzz`
→ **200**
```json
[]
```
- PASS: no matches → 200 []
**** `GET /api/v1/borrowings?status=Bogus`
→ **400**
```json
{
  "code": "validation_failed",
  "message": "Unsupported status \u0027Bogus\u0027. Use All, Active or Returned."
}
```
- PASS: status=Bogus → 400
**** `GET /api/v1/borrowings?sort=name`
→ **400**
```json
{
  "code": "validation_failed",
  "message": "Unsupported sort \u0027name\u0027. Use borrowedDate or expectedReturnDate."
}
```
- PASS: sort=name → 400
**** `GET /api/v1/borrowings?direction=up`
→ **400**
```json
{
  "code": "validation_failed",
  "message": "direction must be asc or desc."
}
```
- PASS: direction=up → 400
- PASS: S1 history 2 / S4 history 0
**** `GET /api/v1/students/c26356a9-1ab6-4793-a1da-c0aa86a34c7c/borrowings`
→ **404**
```json
{
  "code": "student_not_found",
  "message": "No student with id c26356a9-1ab6-4793-a1da-c0aa86a34c7c exists."
}
```
- PASS: unknown ids 404
**PostgreSQL snapshot — after (must equal before)** (taken 2026-11-20 21:43:31)
```text
-- Students: number | name | eligible
   ST-100 | Ada Cruz | True
   ST-200 | Ben Diaz | False
   ST-300 | Cara Lim | True
   ST-400 | Dani Ong | True
-- Equipment: code | name | available
   EQ-001 | Camera | False
   EQ-002 | Laptop | True
   EQ-003 | Projector | True
   EQ-004 | Tablet | False
   EQ-005 | Microphone | False
   EQ-006 | Tripod | True
   EQ-007 | Speaker | True
-- Borrowings: student | item | borrowed | expected | returned
   ST-100 | EQ-001 | 01/11/2026 12:00:00 AM | 09/11/2026 12:00:00 AM | NULL
   ST-100 | EQ-003 | 02/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | 08/11/2026 12:00:00 AM
   ST-300 | EQ-004 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
   ST-300 | EQ-005 | 03/11/2026 12:00:00 AM | 12/11/2026 12:00:00 AM | NULL
```

---
**Summary: 97 passed, 0 failed.** Finished 2026-11-20 21:43:31.
T07 (desktop HTTP client) and T08 (unreachable vs 500, restart) are manual: see docs/test-evidence.md.
