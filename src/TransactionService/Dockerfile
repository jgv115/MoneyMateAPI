FROM public.ecr.aws/lambda/dotnet:5.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim as build
WORKDIR /src
COPY ["TransactionService.csproj", "TransactionService/"]
RUN dotnet restore "TransactionService/TransactionService.csproj"

WORKDIR "/src/TransactionService"
COPY . .
RUN dotnet build "TransactionService.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN pwd
RUN ls
RUN dotnet publish "TransactionService.csproj" \
            --configuration Release \
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .