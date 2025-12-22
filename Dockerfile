# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copiar archivos de proyecto
COPY ["BackHerramientas.csproj", "./"]

# Restaurar dependencias
RUN dotnet restore "BackHerramientas.csproj"

# Copiar el resto del código
COPY . .

# Construir la aplicación
RUN dotnet build "BackHerramientas.csproj" -c Release -o /app/build

# Publicar
RUN dotnet publish "BackHerramientas.csproj" -c Release -o /app/publish

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copiar la aplicación publicada desde la etapa de construcción
COPY --from=build /app/publish .

# Exponer puerto
EXPOSE 5000

# Comando de inicio
ENTRYPOINT ["dotnet", "BackHerramientas.dll"]
