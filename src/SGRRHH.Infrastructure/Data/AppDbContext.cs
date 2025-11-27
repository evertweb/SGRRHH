using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;

namespace SGRRHH.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos para el sistema SGRRHH
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// Usuarios del sistema
    /// </summary>
    public DbSet<Usuario> Usuarios { get; set; }
    
    /// <summary>
    /// Empleados de la empresa
    /// </summary>
    public DbSet<Empleado> Empleados { get; set; }
    
    /// <summary>
    /// Departamentos de la empresa
    /// </summary>
    public DbSet<Departamento> Departamentos { get; set; }
    
    /// <summary>
    /// Cargos de la empresa
    /// </summary>
    public DbSet<Cargo> Cargos { get; set; }
    
    /// <summary>
    /// Proyectos de la empresa
    /// </summary>
    public DbSet<Proyecto> Proyectos { get; set; }
    
    /// <summary>
    /// Catálogo de actividades
    /// </summary>
    public DbSet<Actividad> Actividades { get; set; }
    
    /// <summary>
    /// Registros diarios de empleados
    /// </summary>
    public DbSet<RegistroDiario> RegistrosDiarios { get; set; }
    
    /// <summary>
    /// Detalles de actividades por registro diario
    /// </summary>
    public DbSet<DetalleActividad> DetallesActividades { get; set; }
    
    /// <summary>
    /// Tipos de permiso (catálogo)
    /// </summary>
    public DbSet<TipoPermiso> TiposPermiso { get; set; }
    
    /// <summary>
    /// Solicitudes de permisos
    /// </summary>
    public DbSet<Permiso> Permisos { get; set; }
    
    /// <summary>
    /// Vacaciones de empleados
    /// </summary>
    public DbSet<Vacacion> Vacaciones { get; set; }
    
    /// <summary>
    /// Contratos de empleados
    /// </summary>
    public DbSet<Contrato> Contratos { get; set; }
    
    /// <summary>
    /// Configuraciones del sistema
    /// </summary>
    public DbSet<ConfiguracionSistema> Configuraciones { get; set; }
    
    /// <summary>
    /// Logs de auditoría
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuración de Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
        });
        
        // Configuración de Empleado
        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.HasIndex(e => e.Cedula).IsUnique();
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Cedula).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Nombres).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellidos).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(300);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.TelefonoEmergencia).HasMaxLength(20);
            entity.Property(e => e.ContactoEmergencia).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FotoPath).HasMaxLength(500);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            
            // Ignorar propiedad calculada
            entity.Ignore(e => e.NombreCompleto);
            entity.Ignore(e => e.Antiguedad);
            
            // Relaciones
            entity.HasOne(e => e.Cargo)
                  .WithMany(c => c.Empleados)
                  .HasForeignKey(e => e.CargoId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.Departamento)
                  .WithMany(d => d.Empleados)
                  .HasForeignKey(e => e.DepartamentoId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.Supervisor)
                  .WithMany()
                  .HasForeignKey(e => e.SupervisorId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.CreadoPor)
                  .WithMany()
                  .HasForeignKey(e => e.CreadoPorId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.AprobadoPor)
                  .WithMany()
                  .HasForeignKey(e => e.AprobadoPorId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.Property(e => e.MotivoRechazo).HasMaxLength(500);
        });
        
        // Configuración de Departamento
        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
        });
        
        // Configuración de Cargo
        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            
            entity.HasOne(e => e.Departamento)
                  .WithMany(d => d.Cargos)
                  .HasForeignKey(e => e.DepartamentoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Configuración de Proyecto
        modelBuilder.Entity<Proyecto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.Cliente).HasMaxLength(200);
        });
        
        // Configuración de Actividad
        modelBuilder.Entity<Actividad>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Categoria).HasMaxLength(100);
        });
        
        // Configuración de RegistroDiario
        modelBuilder.Entity<RegistroDiario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Fecha, e.EmpleadoId }).IsUnique();
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            
            // Ignorar propiedad calculada
            entity.Ignore(e => e.TotalHoras);
            entity.Ignore(e => e.EstaCompleto);
            
            entity.HasOne(e => e.Empleado)
                  .WithMany()
                  .HasForeignKey(e => e.EmpleadoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configuración de DetalleActividad
        modelBuilder.Entity<DetalleActividad>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Horas).HasPrecision(5, 2);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            
            entity.HasOne(e => e.RegistroDiario)
                  .WithMany(r => r.DetallesActividades)
                  .HasForeignKey(e => e.RegistroDiarioId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Actividad)
                  .WithMany(a => a.DetallesActividades)
                  .HasForeignKey(e => e.ActividadId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Proyecto)
                  .WithMany(p => p.DetallesActividades)
                  .HasForeignKey(e => e.ProyectoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Configuración de TipoPermiso
        modelBuilder.Entity<TipoPermiso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Color).HasMaxLength(20);
        });
        
        // Configuración de Permiso
        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NumeroActa).IsUnique();
            entity.Property(e => e.NumeroActa).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Motivo).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            entity.Property(e => e.DocumentoSoportePath).HasMaxLength(500);
            entity.Property(e => e.MotivoRechazo).HasMaxLength(500);
            
            entity.HasOne(e => e.Empleado)
                  .WithMany()
                  .HasForeignKey(e => e.EmpleadoId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.TipoPermiso)
                  .WithMany()
                  .HasForeignKey(e => e.TipoPermisoId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.SolicitadoPor)
                  .WithMany()
                  .HasForeignKey(e => e.SolicitadoPorId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.AprobadoPor)
                  .WithMany()
                  .HasForeignKey(e => e.AprobadoPorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Configuración de Vacacion
        modelBuilder.Entity<Vacacion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            
            entity.HasOne(e => e.Empleado)
                  .WithMany()
                  .HasForeignKey(e => e.EmpleadoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configuración de Contrato
        modelBuilder.Entity<Contrato>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Salario).HasPrecision(18, 2);
            entity.Property(e => e.ArchivoAdjuntoPath).HasMaxLength(500);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            
            entity.HasOne(e => e.Empleado)
                  .WithMany()
                  .HasForeignKey(e => e.EmpleadoId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Cargo)
                  .WithMany()
                  .HasForeignKey(e => e.CargoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configuración de ConfiguracionSistema
        modelBuilder.Entity<ConfiguracionSistema>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Clave).IsUnique();
            entity.Property(e => e.Clave).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Valor).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Categoria).HasMaxLength(50);
        });
        
        // Configuración de AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FechaHora);
            entity.HasIndex(e => new { e.Entidad, e.EntidadId });
            entity.Property(e => e.UsuarioNombre).HasMaxLength(200);
            entity.Property(e => e.Accion).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Entidad).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.DireccionIp).HasMaxLength(50);
            entity.Property(e => e.DatosAdicionales).HasMaxLength(4000);
            
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
