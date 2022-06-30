FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine3.11 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine3.11 AS build
WORKDIR /src
COPY ["Take.Api.AsanaxAzure.Facades/Take.Api.AsanaxAzure.Facades.csproj", "Take.Api.AsanaxAzure.Facades/"]
COPY ["Take.Api.AsanaxAzure.Models/Take.Api.AsanaxAzure.Models.csproj", "Take.Api.AsanaxAzure.Models/"]
COPY ["Take.Api.AsanaxAzure.Services/Take.Api.AsanaxAzure.Services.csproj", "Take.Api.AsanaxAzure.Services/"]
COPY ["Take.Api.AsanaxAzure/Take.Api.AsanaxAzure.csproj", "Take.Api.AsanaxAzure/"]

RUN dotnet restore "Take.Api.AsanaxAzure/Take.Api.AsanaxAzure.csproj"
COPY . .
WORKDIR "/src/Take.Api.AsanaxAzure"
RUN dotnet build "Take.Api.AsanaxAzure.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Take.Api.AsanaxAzure.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Take.Api.AsanaxAzure.dll"]