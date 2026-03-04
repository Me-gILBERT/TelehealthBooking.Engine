# Stage 1: The lightweight runtime environment (for production)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: The heavy SDK environment (for compiling the code)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all the .csproj files first to cache the NuGet restore process
COPY ["TelehealthBooking.Api/TelehealthBooking.Api.csproj", "TelehealthBooking.Api/"]
COPY ["TelehealthBooking.Application/TelehealthBooking.Application.csproj", "TelehealthBooking.Application/"]
COPY ["TelehealthBooking.Domain/TelehealthBooking.Domain.csproj", "TelehealthBooking.Domain/"]
COPY ["TelehealthBooking.Infrastructure/TelehealthBooking.Infrastructure.csproj", "TelehealthBooking.Infrastructure/"]
RUN dotnet restore "TelehealthBooking.Api/TelehealthBooking.Api.csproj"

# Copy the rest of the code and build
COPY . .
WORKDIR "/src/TelehealthBooking.Api"
RUN dotnet build "TelehealthBooking.Api.csproj" -c Release -o /app/build

# Stage 3: Publish the compiled code
FROM build AS publish
RUN dotnet publish "TelehealthBooking.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final output
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelehealthBooking.Api.dll"]