# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["HomeCarePH.csproj", "."]
RUN dotnet restore "HomeCarePH.csproj"
COPY . .
RUN dotnet publish "HomeCarePH.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:3000
EXPOSE 3000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HomeCarePH.dll"]
