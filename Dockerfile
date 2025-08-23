# Stage 1: Build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["PeerTutoringSystem.sln", "."]
COPY ["PeerTutoringSystem.Api/PeerTutoringSystem.Api.csproj", "PeerTutoringSystem.Api/"]
COPY ["PeerTutoringSystem.Application/PeerTutoringSystem.Application.csproj", "PeerTutoringSystem.Application/"]
COPY ["PeerTutoringSystem.Domain/PeerTutoringSystem.Domain.csproj", "PeerTutoringSystem.Domain/"]
COPY ["PeerTutoringSystem.Infrastructure/PeerTutoringSystem.Infrastructure.csproj", "PeerTutoringSystem.Infrastructure/"]
COPY ["PeerTutoringSystem.Tests/PeerTutoringSystem.Tests.csproj", "PeerTutoringSystem.Tests/"]
RUN dotnet restore "PeerTutoringSystem.sln"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/PeerTutoringSystem.Api"
RUN dotnet publish "PeerTutoringSystem.Api.csproj" -c Release -o /app/publish

# Stage 2: Final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/etc /app/etc
COPY https /https

ENV ASPNETCORE_URLS="https://+:7258"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ARG cert_password
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=$cert_password
EXPOSE 7258

ENTRYPOINT ["dotnet", "PeerTutoringSystem.Api.dll"]