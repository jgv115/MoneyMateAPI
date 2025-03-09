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
            # We need to specify a musl runtime 
            --runtime linux-musl-amd64 \
            --self-contained true \
            --output /app/publish \
            -p:PublishReadyToRun=true \
            -p:PublishSingleFile=true \
            -p:EnableCompressionInSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine
WORKDIR /var/task
COPY --from=publish /app/publish .

EXPOSE 8080

#ENTRYPOINT ["dotnet", "MoneyMateApi.dll"]
ENTRYPOINT ["./MoneyMateApi", "MoneyMateApi.LocalEntryPoint::Main"]