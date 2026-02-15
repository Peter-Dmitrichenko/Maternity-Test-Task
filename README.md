1. Clone the repository to your local machine.  
2. Install the .NET 6 SDK.  
3. Install Docker Desktop and start it.  
4. Restore NuGet packages for the solution.  
5. Rebuild the solution.  
6. Open the solution in Visual Studio(2022).  
7. Select the Docker (Container) profile in Visual Studio.  
8. Run the Docker configuration from Visual Studio to start the application and database containers.  
9. Run the populate.exe tool from Maternity\Populate\bin\Debug\net6.0 to add sample patients to the database.  
10. Open the Swagger UI and test the endpoints. The getAll endpoint accepts multiple date expressions. In Swagger UI enter multiple date expressions on separate lines; in the actual request send them joined with &.
11. Import the provided Postman collection and run the requests to verify the API.
