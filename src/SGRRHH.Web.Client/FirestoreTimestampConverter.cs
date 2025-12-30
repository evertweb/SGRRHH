using System.Text.Json;
using System.Text.Json.Serialization;

namespace SGRRHH.Web.Client;

/// <summary>
/// Convertidor JSON para manejar Firestore Timestamps.
/// Firestore envía timestamps como objetos {seconds, nanoseconds} o {_seconds, _nanoseconds}.
/// </summary>
public class FirestoreTimestampConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        
        // Si es una cadena ISO 8601, parsearla directamente
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrEmpty(dateString))
                return null;
            
            if (DateTime.TryParse(dateString, out var result))
                return result;
            
            return null;
        }
        
        // Si es un objeto (Firestore Timestamp)
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            long? seconds = null;
            int? nanoseconds = null;
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString()?.ToLower();
                    reader.Read();
                    
                    if (propertyName == "seconds" || propertyName == "_seconds")
                    {
                        if (reader.TokenType == JsonTokenType.Number)
                            seconds = reader.GetInt64();
                    }
                    else if (propertyName == "nanoseconds" || propertyName == "_nanoseconds")
                    {
                        if (reader.TokenType == JsonTokenType.Number)
                            nanoseconds = reader.GetInt32();
                    }
                    // Ignorar otras propiedades como "__type__", "value", etc.
                    else if (reader.TokenType == JsonTokenType.StartObject || reader.TokenType == JsonTokenType.StartArray)
                    {
                        reader.Skip();
                    }
                }
            }
            
            if (seconds.HasValue)
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(seconds.Value).DateTime;
                if (nanoseconds.HasValue)
                {
                    dateTime = dateTime.AddTicks(nanoseconds.Value / 100);
                }
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
            }
            
            return null;
        }
        
        // Si es número (Unix timestamp en segundos)
        if (reader.TokenType == JsonTokenType.Number)
        {
            var seconds = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime.ToLocalTime();
        }
        
        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            // Escribir como ISO 8601 para enviar a Firestore
            writer.WriteStringValue(value.Value.ToUniversalTime().ToString("o"));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

/// <summary>
/// Convertidor JSON para DateTime no nullable.
/// </summary>
public class FirestoreTimestampConverterNonNullable : JsonConverter<DateTime>
{
    private readonly FirestoreTimestampConverter _nullableConverter = new();
    
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = _nullableConverter.Read(ref reader, typeof(DateTime?), options);
        return result ?? DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        _nullableConverter.Write(writer, value, options);
    }
}

/// <summary>
/// Convertidor JSON para TimeSpan nullable.
/// Firestore almacena TimeSpan como string "HH:mm:ss" o como número de ticks.
/// </summary>
public class TimeSpanJsonConverter : JsonConverter<TimeSpan?>
{
    public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        
        // Si es una cadena "HH:mm:ss" o "HH:mm"
        if (reader.TokenType == JsonTokenType.String)
        {
            var timeString = reader.GetString();
            if (string.IsNullOrEmpty(timeString))
                return null;
            
            if (TimeSpan.TryParse(timeString, out var result))
                return result;
            
            return null;
        }
        
        // Si es número (ticks o segundos)
        if (reader.TokenType == JsonTokenType.Number)
        {
            var ticks = reader.GetInt64();
            // Si es un valor pequeño, probablemente son segundos
            if (ticks < 86400 * 10000000L)
                return TimeSpan.FromSeconds(ticks);
            // Si no, son ticks
            return TimeSpan.FromTicks(ticks);
        }
        
        // Si es un objeto (posiblemente formato extendido)
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            long? ticks = null;
            int? hours = null, minutes = null, seconds = null;
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString()?.ToLower();
                    reader.Read();
                    
                    if (propertyName == "ticks" && reader.TokenType == JsonTokenType.Number)
                        ticks = reader.GetInt64();
                    else if (propertyName == "hours" && reader.TokenType == JsonTokenType.Number)
                        hours = reader.GetInt32();
                    else if (propertyName == "minutes" && reader.TokenType == JsonTokenType.Number)
                        minutes = reader.GetInt32();
                    else if (propertyName == "seconds" && reader.TokenType == JsonTokenType.Number)
                        seconds = reader.GetInt32();
                    else if (reader.TokenType == JsonTokenType.StartObject || reader.TokenType == JsonTokenType.StartArray)
                        reader.Skip();
                }
            }
            
            if (ticks.HasValue)
                return TimeSpan.FromTicks(ticks.Value);
            
            if (hours.HasValue || minutes.HasValue || seconds.HasValue)
                return new TimeSpan(hours ?? 0, minutes ?? 0, seconds ?? 0);
            
            return null;
        }
        
        return null;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(@"hh\:mm\:ss"));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

/// <summary>
/// Convertidor JSON para TimeSpan no nullable.
/// </summary>
public class TimeSpanJsonConverterNonNullable : JsonConverter<TimeSpan>
{
    private readonly TimeSpanJsonConverter _nullableConverter = new();
    
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = _nullableConverter.Read(ref reader, typeof(TimeSpan?), options);
        return result ?? TimeSpan.Zero;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        _nullableConverter.Write(writer, value, options);
    }
}

