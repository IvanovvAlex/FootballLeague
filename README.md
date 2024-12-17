# FootballLeague

## Overview

The **FootballLeague** project is a modular and scalable .NET 8 solution for managing football leagues. It adheres to clean architecture principles and includes components for API, data management, domain logic, shared utilities, and unit tests.

---

## Table of Contents

- [Structure](#structure)
- [Requirements](#requirements)

---

## Structure

This solution follows a **layered architecture** to separate concerns:

1. **FootballLeague.API**

   - The main Web API project that serves HTTP endpoints.
   - Built using ASP.NET Core Minimal APIs or Controllers.

2. **FootballLeague.Common**

   - Contains shared request/response models.

3. **FootballLeague.Data**

   - Handles database context, repositories, and entity configuration.
   - Built using Entity Framework Core.

4. **FootballLeague.Domain**

   - Represents the core business logic.

5. **FootballLeague.Tests**

   - Contains partial unit tests for validating the solution.

6. **Configuration Files**
   - `.editorconfig`: Coding style configuration.
   - `.gitignore`: Specifies files and directories to exclude from source control.
   - `Directory.Packages.props`: Shared package versions for the solution.
   - `FootballLeague.sln`: Visual Studio solution file.
   - `LICENSE`: Project license file.
   - `README.md`: Project documentation.

---

## Requirements

- **.NET 8 SDK**  
   Download and install the latest version from [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

- **IDE**
  - Visual Studio 2022+
  - JetBrains Rider
  - VS Code (with C# Extension)

To run the application, you need to have a local SQL Server instance or connection string to a remote database. You need to update the connection string in the `appsettings.json` file.

When running the application, the database will be created.
