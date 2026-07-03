# User Management System - ASP.NET Core MVC

This project is a web application built with **ASP.NET Core MVC**. It focuses on user management, role management, authentication, authorization, and reporting through an admin dashboard.

## Project Description

The application uses **ASP.NET Core Identity** to manage user registration, login, roles, and authentication. The system includes two main roles: **Admin** and **User**.

The admin can manage users and roles using full **CRUD** operations, while users with the User role have access only to their own user dashboard.

## Main Features

- User Register and Login with ASP.NET Core Identity
- Google Login/Register
- Email domain filtering: only `@imb.al` emails are allowed
- User and Role management with CRUD operations through Web API
- Admin Dashboard for system management
- User Dashboard for users with the User role
- Cookie/session authentication valid for 1 hour
- Automatic redirect if the user is already logged in
- Forgot Password functionality with email reset link
- Firestore check during registration to prevent duplicate users
- Extended Identity user model with extra fields:
  - `created_at`
  - `Department`
  - `Position`
- Registration reports in the Admin Dashboard
- SQL Server Stored Procedure for demonstration

## Technologies Used

- ASP.NET Core MVC
- ASP.NET Core Identity
- Entity Framework Core
- SQL Server
- Web API
- Bootstrap
- JavaScript / Fetch API
- Google Authentication
- Firebase Firestore
- SQL Server Stored Procedures

Admin Dashboard
The Admin Dashboard allows the administrator to manage users and roles through CRUD operations. The admin can create, view, update, and delete users, assign roles, and manage extra profile information such as Department and Position.
The dashboard also includes reports that display user registration statistics for the last 7 days.

User Dashboard
The User Dashboard is a dedicated page for users with the User role. After login, users are automatically redirected to this page.

Email Filter
The system only allows emails that end with:
@imb.al

This filter is applied in:
Register
Login
Google Login
Add User from Admin Dashboard
Edit User from Admin Dashboard
Forgot Password
Google Authentication

The project includes Google Login/Register functionality. Users can log in using the Continue with Google button.
The same email filter is also applied to Google accounts, so only Google accounts with @imb.al emails are allowed.

Firestore Check
During registration, the system checks Firestore to see whether the user already exists. If the email is found in Firestore, the registration is rejected.
This prevents duplicate registrations and improves user validation.

Reports
The Admin Dashboard includes registration reports that show:
Total number of users
Users registered today
Users registered during the last 7 days
A vertical chart showing registrations by day
The report data is retrieved asynchronously from the Web API using fetch().

Stored Procedure
A SQL Server Stored Procedure was created for demonstration purposes:
dbo.GetUserCountByRole
This procedure returns the number of users grouped by role. It demonstrates how the application can communicate with SQL Server using stored procedures.

Security
Sensitive files are excluded from GitHub using .gitignore.
The following files are not uploaded to the repository:
appsettings.Development.json
Firebase/key.json
These files may contain private credentials such as Google Client Secret, email password, Firebase keys, or local connection strings.

How to Run the Project
Clone the repository:
git clone https://github.com/JunaZyriqi/User-Management.git
Open the project in Visual Studio.
Create and configure appsettings.Development.json with your local settings:
{
  "ConnectionStrings": {
    "Default": "Server=localhost\\SQLEXPRESS;Database=NewUserRoles;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },

  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  }
}
Apply the database migrations:
Update-Database
Run the project from Visual Studio.
Author
This project was developed as an ASP.NET Core MVC assignment focused on user management, role management, authentication, authorization, and reporting.

