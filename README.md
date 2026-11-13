# RaceDay – Race Event Management System

**Module:** PROG6212  
**Project:** Portfolio of Evidence (POE) – Part 3: Automation of Web Application  
**Institution:** IIE Rosebank College International  
**Application:** RaceDay  
**Technology:** ASP.NET Core MVC, ASP.NET Core Web API, C#, Entity Framework Core and SQL Server

---

## 1. Project Overview

RaceDay is a web-based race event management system developed to simplify the organisation and participation of running events.

The system provides separate interfaces for Organisers and Participants. Organisers can manage race events and related information, while Participants can browse available events, enrol in races and view their participation information.

Part 3 focuses on developing an ASP.NET Core MVC front end that communicates with the RESTful API developed in Part 2.

The MVC application consumes the API for its operations instead of directly accessing the database.

## 2. Project Objectives

The main objectives are to:

- Develop an ASP.NET Core MVC web application.
- Integrate the MVC application with the existing RaceDay REST API.
- Implement session-based authentication.
- Provide role-specific functionality for Organisers and Participants.
- Enable event management and participant enrolments.
- Provide a user-friendly and responsive interface.
- Prepare the application for cloud image storage using Azure Blob Storage.
- Implement Docker containerisation.

## 3. Technologies Used

| Technology | Purpose |
|---|---|
| C# | Main programming language |
| ASP.NET Core MVC | Front-end web application |
| ASP.NET Core Web API | Backend services |
| .NET 10 | Application framework |
| Entity Framework Core | Database access in the API |
| SQL Server | Database management |
| HTML5 | Web page structure |
| CSS3 | Styling and layout |
| JavaScript | Client-side interaction |
| Bootstrap | Responsive interface design |
| Swagger | API testing and documentation |
| Git and GitHub | Source control and project submission |
| Docker | Application containerisation |
| Azure Blob Storage | Planned image storage integration |

## 4. System Architecture

RaceDay uses a client-server architecture.

**MVC Application**

The MVC application provides the user interface and communicates with the backend through HTTP requests.

**RESTful API**

The API processes application requests, handles business logic, authentication and database operations.

**SQL Server Database**

The database stores user accounts, organisers, events, categories and enrolment information.

The MVC application does not directly access SQL Server.

## 5. Main System Features

### Participant Features

- Register an account.
- Log in and log out.
- Browse available race events.
- View event information.
- View available race categories.
- Enrol in available races.
- View personal enrolment information.
- Access participant-specific pages.

### Organiser Features

- Access the organiser dashboard.
- Manage race events.
- Manage race categories.
- View participant enrolments.
- Manage event-related information.
- Access organiser-specific functionality.

### Authentication and Security

- Session-based authentication.
- Role-based access for Organisers and Participants.
- Password hashing handled by the backend API.
- Protected application pages.
- API communication through HTTP requests.

## 6. Project Structure

The solution contains separate API and MVC projects.

```text
RaceDay/
│
├── RaceDay.API/
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── Security/
│   ├── Services/
│   ├── Program.cs
│   └── appsettings.json
│
├── RaceDay.MVC/
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── Services/
│   ├── wwwroot/
│   ├── Program.cs
│   ├── appsettings.json
│   └── Dockerfile
│
├── .github/
│   └── workflows/
│
├── .dockerignore
├── docker-compose.yml
└── README.md
```

*The structure above is a guide. Update it to match the actual files in the submitted repository.*

## 7. Requirements

Before running the application, install:

- Visual Studio 2026.
- .NET 10 SDK.
- SQL Server / SQL Server LocalDB.
- Git.
- A modern web browser.
- Docker Desktop (for containerisation).

## 8. How to Run the Application

### Step 1: Clone the repository

```bash
git clone YOUR_GITHUB_REPOSITORY_URL
```

Open the downloaded RaceDay solution in Visual Studio.

### Step 2: Configure the API

Check the API database connection string in its configuration file or user secrets.

Make sure SQL Server is available and the database has been created.

### Step 3: Run the API

Start the `RaceDay.API` project.

The local API addresses used during development are:

- HTTPS: https://localhost:7297/
- HTTP: http://localhost:5208/

Swagger can be accessed at:

```text
https://localhost:7297/swagger
```

### Step 4: Configure the MVC application

Ensure the MVC application has the correct API base URL in its configuration.

Example:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7297/"
  }
}
```

The actual configuration key must match the implementation in the MVC project.

### Step 5: Run the MVC application

Start the `RaceDay.MVC` project in Visual Studio.

Open the local URL displayed in the browser when the application starts.

**Important:** The API and MVC application must both be running for API-dependent MVC functionality to work.

## 9. API Integration

The MVC application communicates with the backend API using HTTP requests.

The API provides functionality for:

- Authentication.
- User profiles.
- Events.
- Categories.
- Enrolments.
- Results.

The MVC application uses the API to retrieve and submit information.

## 10. Docker Containerisation

Docker containerisation is part of the RaceDay Part 3 implementation.

The project includes Docker configuration files intended to support building and running the MVC application in a container.

The containerisation setup must be successfully built and tested before it can be considered fully implemented.

## 11. Azure Blob Storage

Azure Blob Storage is intended to provide cloud-based image storage for the RaceDay application.

The planned integration supports storing event images and profile pictures through the backend API.

Azure Blob Storage configuration and successful upload testing must be completed before this feature can be considered fully operational.

## 12. Testing

The application should be tested to verify:

- User registration.
- User login and logout.
- Role-based access.
- API connectivity.
- Event management.
- Category management.
- Participant enrolments.
- Input validation.
- Error handling.
- Responsive interface behaviour.

Testing evidence, including screenshots, should be included where required by the POE submission instructions.

## 13. Version Control

GitHub is used to manage the source code and track development progress.

The repository contains the application source code, configuration files, documentation and relevant project resources.

Development changes should be committed regularly using meaningful commit messages.

## 14. Current Implementation Status

The following table should be updated to reflect the final tested submission.

| Component | Status |
|---|---|
| ASP.NET Core MVC application | Implemented – verify final build |
| REST API integration | Implemented – verify endpoints |
| Registration and authentication | Implemented – verify login |
| Role-specific interfaces | Verify functionality |
| Event and enrolment management | Verify functionality |
| Azure Blob Storage | Not yet completed |
| Docker containerisation | Not yet successfully tested |
| GitHub repository | Submission preparation |

## 15. Conclusion

RaceDay aims to provide an integrated race event management solution through an ASP.NET Core MVC application and RESTful API.

The project demonstrates the use of web application development, API integration, session-based authentication, database management and version control.

Further completion and testing of cloud storage and containerisation will strengthen the application's deployment readiness.

---

**End of README**
