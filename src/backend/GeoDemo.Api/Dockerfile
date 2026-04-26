FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development

WORKDIR /app

COPY . .

RUN dotnet restore GeoDemo.slnx

WORKDIR /app/GeoDemo.Api

EXPOSE 8080

CMD ["dotnet", "watch", "run", "--no-restore", "--project", "GeoDemo.Api.csproj", "--urls", "http://0.0.0.0:8080"]