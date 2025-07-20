# EShiftSystem

This project is a web application built with ASP.NET Core and uses Entity Framework Core for database operations with SQL Server.

## Prerequisites

Please ensure that all of the following components are installed before running the program:

* **Visual Studio 2015 or later** (Recommended: Visual Studio 2022 Community Edition or higher)
* **Microsoft SQL Server 2016 or later** (or SQL Server Express)
* **ASP.NET Core 8.0 SDK**
* **IIS Express** (included with Visual Studio)

---

## Step-by-Step Installation and Execution

Follow these steps to set up and run the application:

### 1. Extract and Open Project

* Extract the `EShiftSystem.zip` file to a local directory of your choice.
* Open the project in Visual Studio 2015 or later by selecting `EShiftSystem.sln`.

### 2. Database Configuration

* The application utilizes Entity Framework Core with SQL Server.
* The connection string is configured in both `appsettings.json` and `appsettings.Development.json`.
* The default connection string is:
    ```json
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EShiftSystemDb;Trusted_Connection=true;MultipleActiveResultSets=true"
    ```
    You may need to modify this to point to your SQL Server instance if you are not using `(localdb)\\MSSQLLocalDB`.

### 3. Database Migration

* In Visual Studio, open the **Package Manager Console** (Navigate to `Tools` > `NuGet Package Manager` > `Package Manager Console`).
* Execute the following command:
    ```powershell
    Update-Database
    ```
    This command will create the database schema and apply all necessary migrations. The application automatically seeds initial admin data through the `SeedData.Initialize()` method in `Program.cs`.

### 4. Build and Run the Application

* Press `Ctrl + Shift + B` to build the solution. Ensure there are no build errors.
* Press `F5` or click the "IIS Express" button in Visual Studio to run the application.
* The application will be launched in your default web browser (default URL: `https://localhost:7xxx`, where `7xxx` will be a dynamically assigned port number).

---

## Initial Access and User Accounts

* A default administrator account is pre-configured in the system with the following credentials:
    * **Username:** `admin@eshift.com`
    * **Password:** `Admin@123`
* New customers can register for an account via the registration page within the application.
