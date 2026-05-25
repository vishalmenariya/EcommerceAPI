# Stage 1: Build the code
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["EcommerceAPI.csproj", "./"]
RUN dotnet restore "EcommerceAPI.csproj"
COPY . .
RUN dotnet publish "EcommerceAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run the code
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install the Kerberos library to suppress Npgsql warnings and clean up the apt cache
RUN apt-get update \
    && apt-get install -y libkrb5-3 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EcommerceAPI.dll"]