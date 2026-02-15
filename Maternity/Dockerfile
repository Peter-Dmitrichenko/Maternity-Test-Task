FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy project files
COPY ["Maternity/Maternity.csproj", "Maternity/"]
COPY ["DB/DB.csproj", "DB/"]
COPY ["BL/BL.csproj", "BL/"]
COPY ["DTO/DTO.csproj", "DTO/"]
RUN dotnet restore "Maternity/Maternity.csproj"

COPY . .
WORKDIR "/src/Maternity"
RUN dotnet publish "Maternity.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Maternity.dll"]