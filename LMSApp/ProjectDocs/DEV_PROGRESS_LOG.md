# 🛠️ LMS Project - Dev Progress Log

---

### 📅 2025-07-01 (Monday)

**✅ Tasks Completed:**
- Created ASP.NET Core MVC Project
- Set up Git & connected to GitHub repo
- Configured Identity with ApplicationDbContext
- Seeded roles: Admin, Instructor, Student
- Created root Admin user programmatically
- Scaffolding done: Login, Register, Logout
- Role-based login redirection added

**📚 Concepts Covered:**
- ASP.NET Core Identity
- Role-based Authorization
- Secure Password Hashing
- Session/Cookies (via Identity)
- Role redirection logic using `UserManager`

**⚠️ Issues Faced:**
- Identity scaffolding error due to missing packages
- Fixed via correct NuGet package versions

**⏭️ Next Steps:**
- Protect dashboards with `[Authorize]`
- Start Course module (CRUD + Role-based access)

---

### 📅 2025-07-02 (Monday)

✅ Finalized Professional LMS DB Table List
| Table Name        | Purpose                                        |
| ----------------- | ---------------------------------------------- |
| `AspNetUsers`     | Identity table extended with `FullName`, etc.  |
| `AspNetRoles`     | Built-in Identity roles                        |
| `Course`          | Course master data                             |
| `Enrollment`      | Tracks which students enrolled in which course |
| `CourseMaterial`  | Files (PDF/Video) for each course              |
| `Category`        | Optional grouping of courses                   |
| `Assignment`      | Future: quizzes or tasks per course            |
| `StudentProgress` | Track lesson/video completion (Optional)       |
| `Feedback`        | Student feedback per course (Optional)         |

---

✅ Entity Relationship Diagram (ERD) (Open in RAW mode to see below ERD Digram )

┌────────────────────┐
│   AspNetUsers      │
│--------------------│
│ Id (PK)            │
│ FullName           │
│ Email              │
│ ...                │
└────────┬───────────┘
         │1
         │
         │∞
┌────────▼────────────┐
│    Enrollment       │
│---------------------│
│ Id (PK)             │
│ UserId (FK)         │───▶ AspNetUsers
│ CourseId (FK)       │───▶ Course
│ EnrollmentDate      │
│ CompletionStatus    │
└────────┬────────────┘
         │
         │1
         │
         │∞
┌────────▼────────────┐
│     Course          │
│---------------------│
│ Id (PK)             │
│ Title               │
│ Description         │
│ CategoryId (FK)     │───▶ Category
│ CreatedBy (FK)      │───▶ AspNetUsers (Instructor/Admin)
└────────┬────────────┘
         │
         │1
         │
         │∞
┌────────▼────────────┐
│  CourseMaterial     │
│---------------------│
│ Id (PK)             │
│ CourseId (FK)       │
│ Title               │
│ FilePath            │
│ FileType (PDF/MP4)  │
└─────────────────────┘

┌────────────────────┐
│    Category        │
│--------------------│
│ Id (PK)            │
│ Name               │
└────────────────────┘

(Optional future tables)
┌────────────────────┐
│  Assignment        │
│--------------------│
│ Id (PK)            │
│ CourseId (FK)      │
│ Title, DueDate     │
└────────────────────┘

┌────────────────────┐
│  Feedback          │
│--------------------│
│ Id (PK)            │
│ CourseId (FK)      │
│ StudentId (FK)     │
│ Rating, Comment    │
└────────────────────┘

🔍 Explanation of Key Relationships

| Relationship              | Type      | Notes                                           |
| ------------------------- | --------- | ----------------------------------------------- |
| User → Enrollment         | 1-to-Many | A student can enroll in many courses            |
| Course → Enrollment       | 1-to-Many | A course can have many enrolled students        |
| Course → CourseMaterial   | 1-to-Many | Each course can have many files (notes, videos) |
| Course → Category         | Many-to-1 | Optional categorization                         |
| Course → CreatedBy (User) | Many-to-1 | Only Admin/Instructor can create courses        |

---

🧩 Database Setup
Planned and finalized LMS DB schema professionally

Created following core models:

Course

Enrollment

CourseMaterial

Category

Feedback

Added fields to all models:

CreatedAt

UpdatedAt

IsActive (except in CourseMaterial)

Setup ApplicationDbContext.cs with required DbSet<>

Ran migration: AddLMSCoreTables & AddAuditAndSoftDelete

---
🛠 Fixes & Improvements
Fixed Identity error: Cannot insert NULL into column 'FullName'

Modified Register.cshtml to include FullName input field

Fixed cascading delete issues by using soft delete (IsActive)

Applied best practices for avoiding foreign key violations

---
🧪 Testing
Ran project and verified:

Login

Register

Logout

Root admin creation on startup

Confirmed all Identity tables were created

Verified seeding worked correctly

---

🐞 Issues Faced
Package restore error during scaffolding Identity UI → fixed by installing:

Microsoft.VisualStudio.Web.CodeGeneration.Design

NULL insert error due to FullName not being passed during register → fixed by updating Register.cshtml & ViewModel

Soft deletion required to prevent FK conflicts → added IsActive + adjusted logic

---
📦 Next Steps (Planned for Tomorrow)
Implement CourseController (MVC) with full CRUD (Admin/Instructor)

Setup CourseApiController (REST) to test via Postman

Dashboard redirection based on role (Admin, Student, Instructor)

Create layout pages per role

Organize static resources under wwwroot (CSS, JS, images)

---

⚙️ Architecture Decisions
✅ Decided to use hybrid MVC + REST API structure in a single project

🔜 Planned to add ApiController endpoints and test via Postman (to be implemented tomorrow)

✅ Will maintain clean separation of MVC views (Controllers/) and APIs (Controllers/Api/)

---

### 📅 2025-07-04 (Friday)

1. What is IHttpClientFactory _httpClientFactory?
 IHttpClientFactory is a factory introduced by Microsoft to:
Create HttpClient instances the right way

Avoid socket exhaustion

Manage DNS refresh, retries, etc.

Centralize settings (base address, headers)

---
✅ Dev Progress Log (Admin Module)
Set up Admin Dashboard and displayed total counts (courses, enrollments, students, categories).

Created Api folder to implement Web APIs.

Designed Category model with CreatedAt, UpdatedAt, IsActive (for soft delete).

Created CategoryApiController with full CRUD (GET, POST, PUT, DELETE).

Consumed category API in AdminController using IHttpClientFactory.

Created Razor view (Categories.cshtml) to display and manage categories via controller methods (no JS).

Implemented Add, Update, Delete (soft) using form-based server-side methods.

---
## ✅ Application Flow
Instead of calling the API directly from the Razor view via JS (AJAX), you'll:

**Razor View ⟶ AdminController (MVC) ⟶ CategoryApiController (Web API) ⟶ DB**
---
🐞 Issues Faced & Resolved

❓ Identity table stores student data? → ✅ Yes, all users are stored in AspNetUsers.

✅ Decided to manage categories via controller (not JavaScript) for cleaner architecture.

---
### 📅 2025-07-05 (Saturday)

✅ Completed UI design of admin portal
✅ Created modern instructor layout with top navigation
✅ Built complete instructor dashboard with course management
✅ Implemented course CRUD operations with Bootstrap modals
✅ Added enrollment viewing functionality with course filtering
✅ Created responsive UI with FontAwesome icons
✅ Fixed AJAX integration between client and server
✅ Added proper error handling and validation
✅ Enhanced CourseApiController with new methods for instructor needs
