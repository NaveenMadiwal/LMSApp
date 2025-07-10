# 🎓 Learning Management System (LMS)

A full-featured Learning Management System built using **ASP.NET Core MVC**, **Web API**, **Entity Framework Core**, and **SQL Server**.  
It includes **role-based access** (Admin, Instructor, Student), secure **authentication using Identity**, and modular features like course & category management, enrollment, and feedback.

---

## 📽️ Demo Video

▶️ [Watch Full Demo on YouTube](https://youtu.be/ziBxxzjYvFE?si=giLpvNCFwMpD9Ezv)  
📁 [GitHub Repository](https://github.com/NaveenMadiwal/LMSApp)

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
 

---<img width="1920" height="1022" alt="Screenshot (53)" src="https://github.com/user-attachments/assets/ed318ce0-2731-4258-a186-cc5874ca9859" />
<img width="1920" height="1020" alt="Screenshot (52)" src="https://github.com/user-attachments/assets/af4a4ff6-690d-47cb-992f-0e55d0fd8032" />
<img width="1920" height="1024" alt="Screenshot (51)" src="https://github.com/user-attachments/assets/9ba90b6f-ad21-4417-b84b-fe69df0b4529" />
<img width="1920" height="1020" alt="Screenshot (50)" src="https://github.com/user-attachments/assets/5918246c-0b6b-4c83-851e-e22f397d7feb" />
<img width="1920" height="1013" alt="Screenshot (49)" src="https://github.com/user-attachments/assets/ccec36a7-0c87-487f-907b-c306378dcd05" />
<img width="1920" height="1019" alt="Screenshot (48)" src="https://github.com/user-attachments/assets/daa40199-d7ed-4776-bd66-dc97fc776bc0" />
<img width="1920" height="1020" alt="Screenshot (47)" src="https://github.com/user-attachments/assets/88b4b8eb-70c5-4237-b140-1fc474b358d1" />
<img width="1920" height="1020" alt="Screenshot (58)" src="https://github.com/user-attachments/assets/56537246-671b-47e1-bd9f-a989fddc0459" />
<img width="1920" height="1019" alt="Screenshot (57)" src="https://github.com/user-attachments/assets/51f1c3f9-0fbb-4be5-9ae8-8087baf00538" />
<img width="1920" height="1020" alt="Screenshot (56)" src="https://github.com/user-attachments/assets/668afc44-093d-4116-bbb5-779ea2c7851b" />
<img width="1920" height="1020" alt="Screenshot (55)" src="https://github.com/user-attachments/assets/4d090c68-58d3-4232-ab62-32e63868635a" />
<img width="1920" height="1028" alt="Screenshot (54)" src="https://github.com/user-attachments/assets/4237a77b-fd31-482a-b9ba-8457765687b4" />


## 🤝 Contributing
 

This is a personal learning project — suggestions are welcome!

---

## 📬 Contact

**Naveen Madiwal**  
📧 naveenmadiwal7777@gmail.com  
🔗 [LinkedIn] (https://www.linkedin.com/in/naveen-madiwal/)  
📁 [GitHub](https://github.com/NaveenMadiwal/LMSApp)

---

