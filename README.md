# WasteGlassAPI
WASTE GLASS COLLECTION APP -  BACKEND

1.  Title

WasteGlassAPI – Glass Collection & Route Optimization System

2. Description

WasteGlassAPI is a backend system designed for managing glass recycling collection operations. It enables supplier validation via barcode, records glass collection data, optimizes collection routes, and generates trip summary reports for analysis and decision-making.

The system is built to support efficient logistics planning for recycling collection teams.

3. Features
Supplier management system
Barcode-based supplier validation
Glass collection tracking (clear & coloured glass)
Route optimization for collection teams
Trip report generation with analytics
Status tracking (OK / Warning)

5. Technologies
ASP.NET Core Web API
Entity Framework Core
SQLite Database

C#
6.  Architecture

The project follows a simple layered structure:

Models → Database entities (Supplier, CollectionRecord)
DTOs → Data transfer objects (Trip reports, routes, requests)
Data → DbContext configuration
Program.cs → Minimal API endpoints and business logic

6. APIs
 Supplier APIs
GET /api/suppliers → Get all suppliers
GET /api/suppliers/{id} → Get supplier by ID
GET /api/suppliers/validate/{code} → Validate supplier barcode
 Collection APIs
POST /api/collections → Add collection record
 Route API
GET /api/route → Get optimized collection route
Trip Report API
GET /api/trip/report → Get collection summary report

8.  How to Run
git clone https://github.com/malsha7/WasteGlassAPI.git
cd WasteGlassAPI
dotnet restore
dotnet run

API will run at:

https://wasteglassapi-pvvm.onrender.com


9. Database
SQLite database is used
Database file: wasteglass.db
Automatically created on first run
Includes seed supplier data

10. Algorithms Used
Greedy Nearest Method
Haversine Formula

Used to calculate distance between two geographic coordinates.

Route Optimization

Uses a nearest-neighbour greedy algorithm to determine the shortest collection route.

11.  Testing

The API can be tested using Postman:

GET /api/suppliers
GET /api/suppliers/validate/SUP001
POST /api/collections
GET /api/route
GET /api/trip/report this is backend readme file description
