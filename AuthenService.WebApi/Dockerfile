FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AuthenService.WebApi/AuthenService.WebApi.csproj", "AuthenService.WebApi/"]
COPY ["Authen.Appication/Authen.Application.csproj", "Authen.Appication/"]
COPY ["Authen.Infrastructure/Authen.Infrastructure.csproj", "Authen.Infrastructure/"]
COPY ["Authen.Domain/Authen.Domain.csproj", "Authen.Domain/"]
RUN dotnet restore "AuthenService.WebApi/AuthenService.WebApi.csproj"

COPY . .
WORKDIR "/src/AuthenService.WebApi"
RUN dotnet publish "AuthenService.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuthenService.WebApi.dll"]
