FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FlowBoard.Gateway.csproj", "./"]
RUN dotnet restore "FlowBoard.Gateway.csproj"
COPY . .
RUN dotnet build "FlowBoard.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlowBoard.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowBoard.Gateway.dll"]
