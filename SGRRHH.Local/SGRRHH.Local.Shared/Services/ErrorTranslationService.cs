using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;
using SGRRHH.Local.Domain.Interfaces;

namespace SGRRHH.Local.Shared.Services;

/// <summary>
/// Servicio de traducción de errores técnicos a mensajes amigables para el usuario.
/// Traduce excepciones de SQLite, validación, archivos y otros a mensajes en español.
/// </summary>
public static class ErrorTranslationService
{
    /// <summary>
    /// Traduce una excepción a un mensaje amigable para el usuario
    /// </summary>
    public static string Translate(Exception ex)
    {
        return ex switch
        {
            SqliteException sqliteEx => TranslateSqliteError(sqliteEx),
            ConcurrencyConflictException => "El registro fue modificado por otro usuario. Recargue la página e intente de nuevo.",
            InvalidOperationException ioe => TranslateInvalidOperation(ioe),
            IOException ioEx => TranslateIOError(ioEx),
            TimeoutException => "La operación tardó demasiado. Por favor intente nuevamente.",
            ArgumentNullException ane => $"El campo {ExtractFieldName(ane.ParamName)} es obligatorio.",
            _ => GetGenericMessage(ex)
        };
    }

    private static string TranslateSqliteError(SqliteException ex)
    {
        // SQLite error codes: https://www.sqlite.org/rescode.html
        return ex.SqliteErrorCode switch
        {
            19 => TranslateConstraintError(ex.Message),
            1 => "Error de configuración de base de datos. Por favor contacte a soporte técnico.",
            5 => "La base de datos está ocupada. Por favor espere un momento e intente nuevamente.",
            8 => "No hay permisos de escritura en la base de datos. Por favor contacte a soporte técnico.",
            11 => "Error de integridad en la base de datos. Es necesario restaurar un backup reciente.",
            13 => "No hay espacio suficiente en disco. Por favor libere espacio.",
            14 => "No se puede conectar a la base de datos. Verifique que el servidor esté en ejecución.",
            _ => $"Error de base de datos ({ex.SqliteErrorCode}). Por favor intente nuevamente."
        };
    }

    private static string TranslateConstraintError(string message)
    {
        // UNIQUE constraint
        if (message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase))
        {
            var field = ExtractFieldFromConstraint(message, "UNIQUE constraint failed:");
            return field.ToLower() switch
            {
                "empleados.cedula" => "Ya existe un empleado con esta cédula.",
                "empleados.codigo" => "Ya existe un empleado con este código.",
                "empleados.email" => "Ya existe un empleado con este email.",
                "usuarios.username" => "Ya existe un usuario con este nombre de usuario.",
                "departamentos.nombre" => "Ya existe un departamento con este nombre.",
                "cargos.nombre" => "Ya existe un cargo con este nombre.",
                "contratos.numero_contrato" => "Ya existe un contrato con este número.",
                "tipos_permiso.codigo" => "Ya existe un tipo de permiso con este código.",
                "tipos_permiso.nombre" => "Ya existe un tipo de permiso con este nombre.",
                "eps.codigo" => "Ya existe una EPS con este código.",
                "afp.codigo" => "Ya existe una AFP con este código.",
                "arl.codigo" => "Ya existe una ARL con este código.",
                "cajas_compensacion.codigo" => "Ya existe una Caja de Compensación con este código.",
                _ => $"Ya existe un registro con este valor de {FormatFieldName(field)}."
            };
        }

        // FOREIGN KEY constraint
        if (message.Contains("FOREIGN KEY constraint failed", StringComparison.OrdinalIgnoreCase))
        {
            return "No se puede realizar esta operación porque hay registros relacionados que dependen de este dato.";
        }

        // NOT NULL constraint
        if (message.Contains("NOT NULL constraint failed", StringComparison.OrdinalIgnoreCase))
        {
            var field = ExtractFieldFromConstraint(message, "NOT NULL constraint failed:");
            return $"El campo {FormatFieldName(field)} es obligatorio.";
        }

        // CHECK constraint
        if (message.Contains("CHECK constraint failed", StringComparison.OrdinalIgnoreCase))
        {
            return "El valor ingresado no cumple con las validaciones requeridas.";
        }

        return "Error de validación en la base de datos. Por favor revise los datos ingresados.";
    }

    private static string TranslateInvalidOperation(InvalidOperationException ex)
    {
        if (ex.Message.Contains("no encontrad", StringComparison.OrdinalIgnoreCase))
            return ex.Message; // Ya es un mensaje amigable
        
        if (ex.Message.Contains("Sequence contains no elements"))
            return "No se encontró el registro solicitado.";
        
        return "Operación no válida. Por favor verifique los datos e intente nuevamente.";
    }

    private static string TranslateIOError(IOException ex)
    {
        if (ex.Message.Contains("exceeds the maximum", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(ex.Message, @"maximum of (\d+) bytes");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var bytes))
            {
                var mb = bytes / (1024 * 1024);
                return $"El archivo excede el tamaño máximo permitido de {mb}MB.";
            }
            return "El archivo excede el tamaño máximo permitido.";
        }

        if (ex.Message.Contains("Access is denied") || ex.Message.Contains("acceso"))
            return "No hay permisos para acceder al archivo o carpeta.";

        if (ex.Message.Contains("disk full") || ex.Message.Contains("espacio"))
            return "No hay espacio suficiente en disco.";

        return "Error al procesar el archivo. Por favor intente nuevamente.";
    }

    private static string ExtractFieldFromConstraint(string message, string prefix)
    {
        var startIndex = message.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0) return "desconocido";
        
        var field = message[(startIndex + prefix.Length)..].Trim();
        // Limpiar caracteres adicionales
        field = field.Split(new[] { ' ', '\n', '\r', '"', '\'' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? field;
        return field;
    }

    private static string FormatFieldName(string dbField)
    {
        // Remover nombre de tabla si existe
        if (dbField.Contains('.'))
            dbField = dbField.Split('.').Last();

        // Convertir snake_case a palabras legibles
        return dbField
            .Replace("_", " ")
            .Replace("id", "")
            .Trim()
            .ToUpper();
    }

    private static string ExtractFieldName(string? paramName)
    {
        if (string.IsNullOrEmpty(paramName)) return "requerido";
        return FormatFieldName(paramName);
    }

    private static string GetGenericMessage(Exception ex)
    {
        // Si el mensaje ya parece amigable (en español), usarlo
        if (ex.Message.Contains("Ya existe") || 
            ex.Message.Contains("No se puede") ||
            ex.Message.Contains("debe") ||
            ex.Message.Contains("Error al") ||
            ex.Message.Contains("no encontrado") ||
            ex.Message.Contains("obligatorio"))
        {
            return ex.Message;
        }

        return "Ocurrió un error inesperado. Por favor intente nuevamente o contacte a soporte técnico.";
    }
}
