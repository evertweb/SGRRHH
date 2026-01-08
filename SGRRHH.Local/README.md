# SGRRHH Local

Aplicación local para gestión de RRHH con base de datos SQLite y backend .NET 8.

## Requisitos
- .NET 8 SDK/Runtime
- Windows (probado en entorno local)

## Ejecución
1. Restaurar dependencias y compilar: `dotnet build`
2. Iniciar servidor local: `dotnet run --project SGRRHH.Local.Server`
3. Abrir `https://localhost:5001` o la URL indicada en consola.

## Tests
- Proyecto de pruebas: `SGRRHH.Local.Tests`
- Ejecutar: `dotnet test SGRRHH.Local.Tests/SGRRHH.Local.Tests.csproj`
- Las pruebas usan una base SQLite temporal; no tocan la base de producción.

## Deploy local
- Generar publish (Release): `dotnet publish SGRRHH.Local.Server -c Release -o publish`
- Instalar en C:\SGRRHH.Local: ejecutar `scripts\Install.ps1`

## Documentación de usuario
El manual se encuentra en `SGRRHH.Local.Server/wwwroot/docs/manual-usuario.md` y se publica junto a la app.
