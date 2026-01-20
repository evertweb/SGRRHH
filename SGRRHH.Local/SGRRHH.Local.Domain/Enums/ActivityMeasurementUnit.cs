namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Unidades de medida para cuantificar actividades silviculturales
/// </summary>
public enum ActivityMeasurementUnit
{
    /// <summary>Solo se mide en horas (sin cantidad adicional)</summary>
    HoursOnly = 0,
    
    /// <summary>Hectáreas tratadas/trabajadas</summary>
    Hectares = 1,
    
    /// <summary>Número de árboles (podados, plantados, etc.)</summary>
    Trees = 2,
    
    /// <summary>Metros lineales (cercas, cortafuegos, etc.)</summary>
    LinearMeters = 3,
    
    /// <summary>Metros cúbicos (madera apeada, etc.)</summary>
    CubicMeters = 4,
    
    /// <summary>Kilogramos (fertilizante aplicado, semilla, etc.)</summary>
    Kilograms = 5,
    
    /// <summary>Litros (herbicida, fungicida, etc.)</summary>
    Liters = 6,
    
    /// <summary>Número de plántulas (vivero, siembra)</summary>
    Seedlings = 7,
    
    /// <summary>Bolsas o contenedores (vivero)</summary>
    Bags = 8,
    
    /// <summary>Estacas o tutores</summary>
    Stakes = 9,
    
    /// <summary>Cargas (transporte, material)</summary>
    Loads = 10,
    
    /// <summary>Viajes (transporte)</summary>
    Trips = 11,
    
    /// <summary>Unidades genéricas</summary>
    Units = 99
}
