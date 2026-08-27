# NASA APOD Gallery

An ASP.NET Core MVC web application that fetches NASA's Astronomy Picture of the Day (APOD) data, stores it in a SQL Server database using raw ADO.NET (`SqlClient` — no Entity Framework), and displays the saved entries in a responsive gallery on the home page.

## Features

- Fetches APOD data for a date range from NASA's public API
- Deserializes the JSON response into DTOs
- Saves new entries to SQL Server via parameterized ADO.NET queries (duplicates are skipped)
- Reads saved entries back from the database via `SqlDataReader`
- Displays all saved entries in a responsive Bootstrap card gallery, with a fallback link for video entries

## Tech Stack

- ASP.NET Core MVC (.NET 10)
- SQL Server (raw ADO.NET / `Microsoft.Data.SqlClient`)
- NASA Open API — APOD endpoint
- Bootstrap (bundled with the MVC template)

## Project Structure

```
NasaApodGallery/
├── Controllers/
│   └── HomeController.cs        # Index (reads DB) and Fetch (calls API + saves)
├── Models/
│   ├── ApodDto.cs                # Shape of NASA API's JSON response
│   └── ApodModel.cs              # Shape of a database row / view model
├── Services/
│   ├── NasaApodService.cs        # Calls NASA API via HttpClient
│   └── ApodRepository.cs         # Raw ADO.NET: save + retrieve
├── Views/Home/
│   └── Index.cshtml              # Bootstrap gallery view
├── Database/
│   └── CreateTable.sql           # Database + table creation script
├── appsettings.json              # Base config (placeholder API key)
└── appsettings.Development.json  # Local overrides (real API key — not committed)
```

## Prerequisites

- .NET SDK 8.0 or later
- SQL Server (Express or full) + SQL Server Management Studio (SSMS)
- A free NASA API key from https://api.nasa.gov (optional — `DEMO_KEY` works but has a very low rate limit)

## Setup Instructions

### 1. Clone the repository

```bash
git clone https://github.com/AshenKavisha/nasa-apod-gallery.git
cd nasa-apod-gallery/NasaApodGallery
```

### 2. Create the database

Open SSMS, connect to your SQL Server instance, open a new query window, and run the script at:

```
Database/CreateTable.sql
```

This creates the `NasaApodDb` database and the `ApodEntries` table.

### 3. Configure the connection string

In `appsettings.json`, update the `ConnectionStrings:DefaultConnection` value to match your SQL Server instance name:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=NasaApodDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Find your server name by opening SSMS — it's shown in the "Server name" field on the connection screen (e.g. `localhost`, `localhost\SQLEXPRESS`, or your machine name).

### 4. Set your NASA API key

Open (or create) `appsettings.Development.json` and add your real API key — get one for free at https://api.nasa.gov:

```json
{
  "NasaApi": {
    "ApiKey": "YOUR_REAL_API_KEY_HERE"
  }
}
```

This file is excluded from git via `.gitignore`, so your real key is never committed. `appsettings.json` keeps a `DEMO_KEY` placeholder for anyone else who clones the repo.

### 5. Run the project

```bash
dotnet run
```

Note the port shown in the terminal (e.g. `http://localhost:5230`).

### 6. Fetch and save data

Visit the following URL once to pull data from NASA and save it to your database:

```
http://localhost:5230/Home/Fetch
```

This fetches a fixed date range and inserts any new entries (existing dates are skipped).

### 7. View the gallery

```
http://localhost:5230/Home/Index
```

This reads directly from the database and displays all saved entries as a responsive card gallery.

## Notes

- Duplicate prevention is enforced at two levels: an application-side check before insert, and a `UNIQUE` constraint on the `ApodDate` column as a database-level backstop.
- All SQL queries use parameterized commands (`SqlCommand.Parameters.AddWithValue`) to prevent SQL injection.
- The `Fetch` and `Index` actions are intentionally separate: `Fetch` is the write path (API → DB), `Index` is the read path (DB → view). The home page never calls NASA's API directly.