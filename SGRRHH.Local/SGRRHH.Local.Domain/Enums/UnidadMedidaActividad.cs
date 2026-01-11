namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Unidades de medida para cuantificar actividades silviculturales
/// </summary>
public enum UnidadMedidaActividad
{
    /// <summary>Solo se mide en horas (sin cantidad adicional)</summary>
    SoloHoras = 0,
    
    /// <summary>Hectáreas tratadas/trabajadas</summary>
    Hectareas = 1,
    
    /// <summary>Número de árboles (podados, plantados, etc.)</summary>
    Arboles = 2,
    
    /// <summary>Metros lineales (cercas, cortafuegos, etc.)</summary>
    MetrosLineales = 3,
    
    /// <summary>Metros cúbicos (madera apeada, etc.)</summary>
    MetrosCubicos = 4,
    
    /// <summary>Kilogramos (fertilizante aplicado, semilla, etc.)</summary>
    Kilogramos = 5,
    
    /// <summary>Litros (herbicida, fungicida, etc.)</summary>
    Litros = 6,
    
    /// <summary>Número de plántulas (vivero, siembra)</summary>
    Plantulas = 7,
    
    /// <summary>Bolsas o contenedores (vivero)</summary>
    Bolsas = 8,
    
    /// <summary>Estacas o tutores</summary>
    Estacas = 9,
    
    /// <summary>Cargas (transporte, material)</summary>
    Cargas = 10,
    
    /// <summary>Viajes (transporte)</summary>
    Viajes = 11,
    
    /// <summary>Unidades genéricas</summary>
    Unidades = 99
}
