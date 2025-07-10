# 🎓 Learning Management System (LMS)

A full-featured Learning Management System built using **ASP.NET Core MVC**, **Web API**, **Entity Framework Core**, and **SQL Server**.  
It includes **role-based access** (Admin, Instructor, Student), secure **authentication using Identity**, and modular features like course & category management, enrollment, and feedback.

---

## 📽️ Demo Video

▶️ [Watch Full Demo on YouTube](https://youtu.be/ziBxxzjYvFE?si=giLpvNCFwMpD9Ezv)  
📁 [GitHub Repository](https://github.com/your-username/lms-project)

---

## 🧑‍💻 Technologies Used

| Area           | Stack/Tool                         |
|----------------|------------------------------------|
| Backend        | ASP.NET Core MVC, Web API          |
| Frontend       | Razor Views, Bootstrap             |
| Database       | SQL Server + Entity Framework Core |
| Auth & Roles   | ASP.NET Identity                   |
| API Testing    | Postman                            |
| Version Control| Git & GitHub                       |

---

## 🔐 Key Features

- **Authentication & Authorization**
  - ASP.NET Identity with password hashing
  - Role-based login redirection (Admin, Instructor, Student)
  - Session and Cookie-based login persistence

- **Admin Module**
  - Dashboard
  - Add/Edit/Delete Categories
  - Create Courses with Category binding
  - Soft delete support (IsActive flag)
  - View Feedback

- **Instructor Module**
  - Manage own courses
  - Upload course materials (PDF, Video)
  - View student enrollments

- **Student Module**
  - Enroll in courses
  - View progress (Completed %, materials)
  - Submit feedback
  - View profile and update password

- **Web API Integration**
  - Category API
  - Course API
  - Enrollment API
  - Feedback API
  - Used in both Admin & Instructor dashboard via HttpClient

- **Other Concepts Covered** 
  - Middleware for role restrictions
  - Exception handling in Web APIs 
  - CreatedAt, UpdatedAt, IsActive columns for all core entities
  - Clean folder structure and separation of concerns

---

## 🧠 Architecture Overview

Presentation Layer (MVC Views)
↓
Controller Layer (Admin, Instructor, Student)
↓
Service/API Layer (Web API + HttpClient)
↓
Data Access Layer (EF Core + DbContext)
↓
Database (SQL Server)


---

## 📁 Project Structure (Highlights)

LMSApp/
│
├── Controllers/
│ ├── AdminController.cs
│ ├── InstructorController.cs
│ ├── StudentController.cs
│
├── Models/
│ ├── ApplicationUser.cs
│ ├── Course.cs
│ ├── Category.cs
│ ├── Enrollment.cs
│ ├── Feedback.cs
│
├── Views/
│ ├── Admin/
│ ├── Instructor/
│ ├── Student/
│ ├── Shared/ (_Layout.cshtml files)
│
├── wwwroot/
│ ├── css/
│ ├── js/
│ ├── images/
│
├── Data/
│ ├── ApplicationDbContext.cs
│ ├── DbSeeder.cs
│
├── API/
│ ├── CategoryApiController.cs
│ ├── CourseApiController.cs
│ ├── EnrollmentApiController.cs
│
├── appsettings.json
├── Program.cs
├── README.md



---

## 🧪 API Testing (Postman)

Use Postman to:
- Add/Update/Delete Categories
- Add/Update/Delete Courses
- Enroll students in a course
- Submit & fetch feedback

---

## ✅ Setup Instructions

1. Clone the repo  
   `git clone https://github.com/your-username/lms-project.git`

2. Setup SQL Server and update `appsettings.json` connection string.

3. Run migrations:  
   `Update-Database` via Package Manager Console

4. Run the project

5. Admin login:  
   Email: `admin@lms.com`  
   Password: `Admin@123`

---

## 📸 Screenshots
 

---

## 🤝 Contributing
<img width="1920" height="1080" alt="Screenshot (49)" src="https://github.com/user-attachments/assets/e24d9389-0297-4ca5-b7d2-282499527df6" />

This is a personal learning project — suggestions are welcome!

---

## 📬 Contact

**Naveen Madiwal**  
📧 naveenmadiwal7777@gmail.com  
🔗 [LinkedIn] (https://www.linkedin.com/in/naveen-madiwal/)  
📁 [GitHub](https://github.com/your-username)

---

