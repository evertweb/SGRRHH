using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;

namespace SGRRHH.Infrastructure.Data;

/// <summary>
/// Clase para inicializar la base de datos con datos semilla
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Inicializa la base de datos y crea los datos iniciales
    /// </summary>
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Crear la base de datos si no existe
        await context.Database.EnsureCreatedAsync();
        
        // Configurar SQLite para modo WAL (mejor rendimiento concurrente)
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
        
        // Si ya hay usuarios, no hacer nada
        if (await context.Usuarios.AnyAsync())
            return;
            
        // Crear usuario administrador por defecto
        var adminUser = new Usuario
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", BCrypt.Net.BCrypt.GenerateSalt(12)),
            NombreCompleto = "Administrador del Sistema",
            Email = "admin@empresa.com",
            Rol = RolUsuario.Administrador,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        context.Usuarios.Add(adminUser);
        
        // Crear usuario operador (Secretaria) por defecto
        var operadorUser = new Usuario
        {
            Username = "secretaria",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("secretaria123", BCrypt.Net.BCrypt.GenerateSalt(12)),
            NombreCompleto = "Usuario Secretaria",
            Email = "secretaria@empresa.com",
            Rol = RolUsuario.Operador,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        context.Usuarios.Add(operadorUser);
        
        // Crear usuario aprobador (Ingeniera) por defecto
        var aprobadorUser = new Usuario
        {
            Username = "ingeniera",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ingeniera123", BCrypt.Net.BCrypt.GenerateSalt(12)),
            NombreCompleto = "Usuario Ingeniera",
            Email = "ingeniera@empresa.com",
            Rol = RolUsuario.Aprobador,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        context.Usuarios.Add(aprobadorUser);
        
        // Crear departamentos iniciales
        var depAdministracion = new Departamento
        {
            Codigo = "ADM",
            Nombre = "Administración",
            Descripcion = "Departamento de Administración General",
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        var depOperaciones = new Departamento
        {
            Codigo = "OPE",
            Nombre = "Operaciones",
            Descripcion = "Departamento de Operaciones",
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        var depIngenieria = new Departamento
        {
            Codigo = "ING",
            Nombre = "Ingeniería",
            Descripcion = "Departamento de Ingeniería",
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        context.Departamentos.AddRange(depAdministracion, depOperaciones, depIngenieria);
        
        // Crear cargos iniciales
        var cargoGerente = new Cargo
        {
            Codigo = "GER",
            Nombre = "Gerente General",
            Descripcion = "Gerente General de la empresa",
            Nivel = 1,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        var cargoSecretaria = new Cargo
        {
            Codigo = "SEC",
            Nombre = "Secretaria",
            Descripcion = "Secretaria administrativa",
            Nivel = 3,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        var cargoIngeniero = new Cargo
        {
            Codigo = "ING",
            Nombre = "Ingeniero",
            Descripcion = "Ingeniero de proyectos",
            Nivel = 2,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        context.Cargos.AddRange(cargoGerente, cargoSecretaria, cargoIngeniero);
        
        await context.SaveChangesAsync();
        
        // Crear proyectos iniciales
        var proyectos = new List<Proyecto>
        {
            new Proyecto
            {
                Codigo = "PRY-0001",
                Nombre = "Proyecto Interno",
                Descripcion = "Actividades internas de la empresa",
                Cliente = "Interno",
                Estado = EstadoProyecto.Activo,
                FechaInicio = DateTime.Today.AddMonths(-6),
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Proyecto
            {
                Codigo = "PRY-0002",
                Nombre = "Proyecto Construcción Edificio Central",
                Descripcion = "Construcción del edificio central de oficinas",
                Cliente = "Cliente ABC",
                Estado = EstadoProyecto.Activo,
                FechaInicio = DateTime.Today.AddMonths(-3),
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Proyecto
            {
                Codigo = "PRY-0003",
                Nombre = "Mantenimiento Infraestructura",
                Descripcion = "Mantenimiento de infraestructura existente",
                Cliente = "Varios",
                Estado = EstadoProyecto.Activo,
                FechaInicio = DateTime.Today.AddYears(-1),
                Activo = true,
                FechaCreacion = DateTime.Now
            }
        };
        
        context.Proyectos.AddRange(proyectos);
        
        // Crear actividades iniciales
        var actividades = new List<Actividad>
        {
            // Actividades Administrativas
            new Actividad
            {
                Codigo = "ACT-0001",
                Nombre = "Gestión Documental",
                Descripcion = "Archivo, organización y gestión de documentos",
                Categoria = "Administrativa",
                RequiereProyecto = false,
                Orden = 1,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Actividad
            {
                Codigo = "ACT-0002",
                Nombre = "Atención al Cliente",
                Descripcion = "Atención telefónica y presencial a clientes",
                Categoria = "Administrativa",
                RequiereProyecto = false,
                Orden = 2,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Actividad
            {
                Codigo = "ACT-0003",
                Nombre = "Reuniones",
                Descripcion = "Reuniones de trabajo y coordinación",
                Categoria = "Administrativa",
                RequiereProyecto = false,
                Orden = 3,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            
            // Actividades de Ingeniería
            new Actividad
            {
                Codigo = "ACT-0004",
                Nombre = "Diseño Técnico",
                Descripcion = "Elaboración de diseños técnicos",
                Categoria = "Ingeniería",
                RequiereProyecto = true,
                Orden = 1,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Actividad
            {
                Codigo = "ACT-0005",
                Nombre = "Supervisión en Campo",
                Descripcion = "Supervisión de trabajos en campo",
                Categoria = "Ingeniería",
                RequiereProyecto = true,
                Orden = 2,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Actividad
            {
                Codigo = "ACT-0006",
                Nombre = "Elaboración de Informes",
                Descripcion = "Elaboración de informes técnicos",
                Categoria = "Ingeniería",
                RequiereProyecto = true,
                Orden = 3,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            
            // Actividades de Operaciones
            new Actividad
            {
                Codigo = "ACT-0007",
                Nombre = "Trabajo de Campo",
                Descripcion = "Ejecución de trabajos en campo",
                Categoria = "Operaciones",
                RequiereProyecto = true,
                Orden = 1,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Actividad
            {
                Codigo = "ACT-0008",
                Nombre = "Mantenimiento de Equipos",
                Descripcion = "Mantenimiento preventivo y correctivo de equipos",
                Categoria = "Operaciones",
                RequiereProyecto = false,
                Orden = 2,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Actividad
            {
                Codigo = "ACT-0009",
                Nombre = "Transporte de Materiales",
                Descripcion = "Transporte y logística de materiales",
                Categoria = "Operaciones",
                RequiereProyecto = true,
                Orden = 3,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            
            // Otras actividades
            new Actividad
            {
                Codigo = "ACT-0010",
                Nombre = "Capacitación",
                Descripcion = "Asistencia a capacitaciones y entrenamientos",
                Categoria = "General",
                RequiereProyecto = false,
                Orden = 1,
                Activo = true,
                FechaCreacion = DateTime.Now
            }
        };
        
        context.Actividades.AddRange(actividades);
        
        // Crear tipos de permiso colombianos
        var tiposPermiso = new List<TipoPermiso>
        {
            // Licencias legales Colombia
            new TipoPermiso
            {
                Nombre = "Licencia de Maternidad",
                Descripcion = "18 semanas de licencia remunerada (Art. 236 CST)",
                Color = "#E91E63",
                RequiereAprobacion = true,
                RequiereDocumento = true,
                DiasPorDefecto = 126, // 18 semanas
                EsCompensable = false,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new TipoPermiso
            {
                Nombre = "Licencia de Paternidad",
                Descripcion = "2 semanas de licencia remunerada (Art. 236 CST)",
                Color = "#2196F3",
                RequiereAprobacion = true,
                RequiereDocumento = true,
                DiasPorDefecto = 14, // 2 semanas
                EsCompensable = false,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new TipoPermiso
            {
                Nombre = "Licencia por Luto",
                Descripcion = "5 días hábiles por fallecimiento de familiar (Art. 57 CST)",
                Color = "#424242",
                RequiereAprobacion = true,
                RequiereDocumento = true,
                DiasPorDefecto = 5,
                EsCompensable = false,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new TipoPermiso
            {
                Nombre = "Licencia de Matrimonio",
                Descripcion = "Según convención colectiva o reglamento interno",
                Color = "#9C27B0",
                RequiereAprobacion = true,
                RequiereDocumento = true,
                DiasPorDefecto = 5,
                EsCompensable = false,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new TipoPermiso
            {
                Nombre = "Calamidad Doméstica",
                Descripcion = "Según gravedad del caso (Art. 57 CST)",
                Color = "#FF5722",
                RequiereAprobacion = true,
                RequiereDocumento = true,
                DiasPorDefecto = 1,
                EsCompensable = false,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            
            // Incapacidades
            new TipoPermiso
            {
                Nombre = "Incapacidad por Enfermedad General",
                Descripcion = "Días 1-2: empleador. Días 3-90: EPS (66.67%)",
                Color = "#F44336",
                RequiereAprobacion = true,
                RequiereDocumento = true,
                DiasPorDefecto = 3,
                EsCompensable = false,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new TipoPermiso
            {
                Nombre = "Incapacidad por Accidente de Trabajo",
                Descripcion = "Cubierta por la ARL",
                Color = "#FF9800",
                RequiereAprobacion = true,
                RequiereDocumento = true,
                DiasPorDefecto = 1,
                EsCompensable = false,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            
            // Permisos compensables
            new TipoPermiso
            {
                Nombre = "Diligencias Personales",
                Descripcion = "Permiso para trámites personales (citas, notaría, etc.)",
                Color = "#4CAF50",
                RequiereAprobacion = true,
                RequiereDocumento = false,
                DiasPorDefecto = 1,
                EsCompensable = true,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new TipoPermiso
            {
                Nombre = "Cita Médica",
                Descripcion = "Permiso para asistir a cita médica",
                Color = "#00BCD4",
                RequiereAprobacion = true,
                RequiereDocumento = true,
                DiasPorDefecto = 1,
                EsCompensable = true,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new TipoPermiso
            {
                Nombre = "Permiso Académico",
                Descripcion = "Permiso para actividades académicas o capacitación",
                Color = "#3F51B5",
                RequiereAprobacion = true,
                RequiereDocumento = false,
                DiasPorDefecto = 1,
                EsCompensable = true,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new TipoPermiso
            {
                Nombre = "Permiso por Hora",
                Descripcion = "Permiso por horas durante la jornada laboral",
                Color = "#607D8B",
                RequiereAprobacion = true,
                RequiereDocumento = false,
                DiasPorDefecto = 1,
                EsCompensable = true,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            
            // Otros permisos
            new TipoPermiso
            {
                Nombre = "Día de la Familia",
                Descripcion = "Jornada semestral para compartir con la familia (Ley 1857/2017)",
                Color = "#8BC34A",
                RequiereAprobacion = true,
                RequiereDocumento = false,
                DiasPorDefecto = 1,
                EsCompensable = false,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new TipoPermiso
            {
                Nombre = "Permiso Sindical",
                Descripcion = "Permiso para actividades sindicales",
                Color = "#795548",
                RequiereAprobacion = true,
                RequiereDocumento = true,
                DiasPorDefecto = 1,
                EsCompensable = false,
                Activo = true,
                FechaCreacion = DateTime.Now
            }
        };
        
        context.Set<TipoPermiso>().AddRange(tiposPermiso);
        
        await context.SaveChangesAsync();
        
        // Obtener IDs de departamentos y cargos para crear empleados
        var depAdmId = depAdministracion.Id;
        var depOpeId = depOperaciones.Id;
        var depIngId = depIngenieria.Id;
        var cargoGerId = cargoGerente.Id;
        var cargoSecId = cargoSecretaria.Id;
        var cargoIngId = cargoIngeniero.Id;
        
        // Crear empleados de prueba
        var empleados = new List<Empleado>
        {
            new Empleado
            {
                Codigo = "EMP001",
                Cedula = "1234567890",
                Nombres = "María",
                Apellidos = "García López",
                FechaNacimiento = new DateTime(1985, 3, 15),
                Genero = Genero.Femenino,
                EstadoCivil = EstadoCivil.Casado,
                Direccion = "Calle 100 # 15-25, Bogotá",
                Telefono = "3001234567",
                Email = "maria.garcia@empresa.com",
                FechaIngreso = new DateTime(2020, 1, 15),
                Estado = EstadoEmpleado.Activo,
                TipoContrato = TipoContrato.Indefinido,
                CargoId = cargoGerId,
                DepartamentoId = depAdmId,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Empleado
            {
                Codigo = "EMP002",
                Cedula = "9876543210",
                Nombres = "Carlos",
                Apellidos = "Rodríguez Pérez",
                FechaNacimiento = new DateTime(1990, 7, 22),
                Genero = Genero.Masculino,
                EstadoCivil = EstadoCivil.Soltero,
                Direccion = "Carrera 45 # 80-10, Bogotá",
                Telefono = "3109876543",
                Email = "carlos.rodriguez@empresa.com",
                FechaIngreso = new DateTime(2022, 6, 1),
                Estado = EstadoEmpleado.Activo,
                TipoContrato = TipoContrato.Fijo,
                CargoId = cargoIngId,
                DepartamentoId = depIngId,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Empleado
            {
                Codigo = "EMP003",
                Cedula = "5555666777",
                Nombres = "Ana",
                Apellidos = "Martínez Sánchez",
                FechaNacimiento = new DateTime(1992, 11, 8),
                Genero = Genero.Femenino,
                EstadoCivil = EstadoCivil.Soltero,
                Direccion = "Av. Suba # 120-45, Bogotá",
                Telefono = "3205551234",
                Email = "ana.martinez@empresa.com",
                FechaIngreso = new DateTime(2023, 3, 1),
                Estado = EstadoEmpleado.Activo,
                TipoContrato = TipoContrato.Fijo,
                CargoId = cargoSecId,
                DepartamentoId = depAdmId,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Empleado
            {
                Codigo = "EMP004",
                Cedula = "1112223334",
                Nombres = "Pedro",
                Apellidos = "González Ruiz",
                FechaNacimiento = new DateTime(1988, 5, 20),
                Genero = Genero.Masculino,
                EstadoCivil = EstadoCivil.Casado,
                Direccion = "Calle 50 # 30-15, Bogotá",
                Telefono = "3151112233",
                Email = "pedro.gonzalez@empresa.com",
                FechaIngreso = new DateTime(2021, 8, 15),
                Estado = EstadoEmpleado.Activo,
                TipoContrato = TipoContrato.Indefinido,
                CargoId = cargoIngId,
                DepartamentoId = depOpeId,
                Activo = true,
                FechaCreacion = DateTime.Now
            }
        };
        
        context.Empleados.AddRange(empleados);
        await context.SaveChangesAsync();
        
        // Crear contratos para los empleados
        var contratos = new List<Contrato>
        {
            // Contrato indefinido para María García
            new Contrato
            {
                EmpleadoId = empleados[0].Id,
                TipoContrato = TipoContrato.Indefinido,
                FechaInicio = new DateTime(2020, 1, 15),
                FechaFin = null, // Indefinido
                Salario = 8500000,
                CargoId = cargoGerId,
                Estado = EstadoContrato.Activo,
                Observaciones = "Contrato inicial",
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            // Contrato a término fijo para Carlos Rodríguez (próximo a vencer)
            new Contrato
            {
                EmpleadoId = empleados[1].Id,
                TipoContrato = TipoContrato.Fijo,
                FechaInicio = new DateTime(2024, 6, 1),
                FechaFin = DateTime.Today.AddDays(25), // Vence en 25 días
                Salario = 5500000,
                CargoId = cargoIngId,
                Estado = EstadoContrato.Activo,
                Observaciones = "Segunda renovación",
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            // Contrato a término fijo para Ana Martínez
            new Contrato
            {
                EmpleadoId = empleados[2].Id,
                TipoContrato = TipoContrato.Fijo,
                FechaInicio = new DateTime(2024, 3, 1),
                FechaFin = new DateTime(2025, 2, 28),
                Salario = 2800000,
                CargoId = cargoSecId,
                Estado = EstadoContrato.Activo,
                Observaciones = "Contrato anual",
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            // Contrato indefinido para Pedro González
            new Contrato
            {
                EmpleadoId = empleados[3].Id,
                TipoContrato = TipoContrato.Indefinido,
                FechaInicio = new DateTime(2021, 8, 15),
                FechaFin = null,
                Salario = 6200000,
                CargoId = cargoIngId,
                Estado = EstadoContrato.Activo,
                Observaciones = "Contrato inicial - Operaciones",
                Activo = true,
                FechaCreacion = DateTime.Now
            }
        };
        
        context.Contratos.AddRange(contratos);
        
        // Crear vacaciones de ejemplo
        var vacaciones = new List<Vacacion>
        {
            // Vacaciones disfrutadas por María García en 2024
            new Vacacion
            {
                EmpleadoId = empleados[0].Id,
                FechaInicio = new DateTime(2024, 7, 1),
                FechaFin = new DateTime(2024, 7, 12),
                DiasTomados = 10,
                PeriodoCorrespondiente = 2024,
                Estado = EstadoVacacion.Disfrutada,
                Observaciones = "Vacaciones de mitad de año",
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            // Vacaciones programadas para María García en 2025
            new Vacacion
            {
                EmpleadoId = empleados[0].Id,
                FechaInicio = new DateTime(2025, 12, 15),
                FechaFin = new DateTime(2025, 12, 26),
                DiasTomados = 10,
                PeriodoCorrespondiente = 2025,
                Estado = EstadoVacacion.Programada,
                Observaciones = "Vacaciones de fin de año",
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            // Vacaciones disfrutadas por Pedro González
            new Vacacion
            {
                EmpleadoId = empleados[3].Id,
                FechaInicio = new DateTime(2024, 12, 20),
                FechaFin = new DateTime(2025, 1, 3),
                DiasTomados = 10,
                PeriodoCorrespondiente = 2024,
                Estado = EstadoVacacion.Disfrutada,
                Observaciones = "Vacaciones navideñas",
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            // Vacaciones programadas para Carlos Rodríguez
            new Vacacion
            {
                EmpleadoId = empleados[1].Id,
                FechaInicio = new DateTime(2025, 6, 16),
                FechaFin = new DateTime(2025, 6, 27),
                DiasTomados = 10,
                PeriodoCorrespondiente = 2025,
                Estado = EstadoVacacion.Programada,
                Observaciones = "Vacaciones primer semestre",
                Activo = true,
                FechaCreacion = DateTime.Now
            }
        };
        
        context.Vacaciones.AddRange(vacaciones);
        
        await context.SaveChangesAsync();
    }
}
