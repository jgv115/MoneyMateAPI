FROM public.ecr.aws/lambda/dotnet:8.2024.06.19.13 AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as build
WORKDIR /src
COPY ["MoneyMateApi.csproj", "MoneyMateApi/"]
RUN dotnet restore "MoneyMateApi/MoneyMateApi.csproj"

WORKDIR "/MoneyMateApi"
COPY . .
RUN dotnet build "MoneyMateApi.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "MoneyMateApi.csproj" \
            --configuration Release \
            --runtime linux-x64 \
            --self-contained false \
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .