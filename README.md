# 🏥 Local Clinic Queue Management System

A real-time, web-based queue management system for public clinics in South Africa. Built with **ASP.NET Core MVC**, **SignalR**, **Entity Framework Core**, and **SQL Server**, this system replaces paper-based queues and provides live updates, priority handling, and actionable analytics.

---

## 🚀 Features

- 📲 **Patient Registration** on a tablet or kiosk
- 🔁 **Live Queue Updates** via SignalR (no refresh needed)
- 🧓 **Priority for Elderly & Emergencies**
- 📊 **Admin Dashboard** with:
  - Average wait time
  - Emergency case stats
  - Patients served today
  - Peak registration hours

---

## 💡 Why This Project Stands Out

> A real-world solution to a real South African public health service issue.

- 📡 Demonstrates **real-time communication** using SignalR
- 📈 Showcases **data analytics** in a healthcare context
- 🛠️ Built with **production-ready technologies**
- 🎯 Solves a specific, impactful community problem

---

## 🛠 Tech Stack

- **Frontend**: ASP.NET Core MVC, Bootstrap 5
- **Backend**: ASP.NET Core, C#
- **Real-Time Messaging**: SignalR
- **Database**: SQL Server + Entity Framework Core
- **IDE**: Visual Studio Code / Visual Studio

---

## 📷 Screenshots

<details>
<summary>🔽 Click to view</summary>

![Registration](screenshots/registration.png)
![Live Queue](screenshots/live-queue.png)
![Admin Dashboard](screenshots/dashboard.png)

</details>

---

## ⚙️ How to Run

### Prerequisites
- [.NET SDK 7+](https://dotnet.microsoft.com/en-us/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/)

### Steps

1. **Clone the Repo**
   ```bash
   git clone https://github.com/yourusername/clinic-queue-system.git
   cd clinic-queue-system
   ```

2. **Update Database Connection String**

   Update the `DefaultConnection` string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ClinicQueueDb;Trusted_Connection=True;"
   }
   ```

3. **Run Migrations**

   Apply the database migrations:
   ```bash
   dotnet ef database update
   ```

4. **Run App**

   Start the application:
   ```bash
   dotnet run
   ```

---

## 🧪 Key Folders
- Controllers/        → MVC controllers (Patient, Admin)
- Models/             → EF Core models
- Views/              → Razor views (Live queue, dashboard, registration)
- Hubs/               → SignalR hub for real-time communication
- Data/               → EF Core DB context
- wwwroot/            → Static files

---

## 🛡️ Future Improvements
- Authentication and role-based access
- SMS/Email queue alerts
- Mobile-responsive enhancements
- Unit + integration testing

---

## 👨‍💻 Author
Lehlonolo Mokoena  
🎓 Software Development Graduate  
📍 South Africa  
📧 mokoenalehlonolo27@gmail.com  

