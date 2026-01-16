using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Services;

/// <summary>
/// Servicio para obtener información sobre bancos de Colombia y sus tipos de cuenta disponibles
/// </summary>
public static class BancoService
{
    /// <summary>
    /// Obtiene el nombre legible del banco
    /// </summary>
    public static string GetNombreBanco(BancoColombia banco)
    {
        return banco switch
        {
            BancoColombia.Bancolombia => "Bancolombia",
            BancoColombia.BancoDeBogota => "Banco de Bogotá",
            BancoColombia.Davivienda => "Davivienda",
            BancoColombia.BancoDeOccidente => "Banco de Occidente",
            BancoColombia.BancoPopular => "Banco Popular",
            BancoColombia.BancoAVVillas => "Banco AV Villas",
            BancoColombia.BancoCajaSocial => "Banco Caja Social",
            BancoColombia.BancoAgrario => "Banco Agrario de Colombia",
            BancoColombia.Bancoomeva => "Bancoomeva",
            BancoColombia.BancoFalabella => "Banco Falabella",
            BancoColombia.BancoPichincha => "Banco Pichincha",
            BancoColombia.BancoSantander => "Banco Santander",
            BancoColombia.BancoBBVA => "Banco BBVA Colombia",
            BancoColombia.BancoGNBSudameris => "Banco GNB Sudameris",
            BancoColombia.BancoCooperativoCoopcentral => "Banco Cooperativo Coopcentral",
            BancoColombia.BancoDeLaRepublica => "Banco de la República",
            BancoColombia.BancoW => "Banco W",
            BancoColombia.BancoSerfinanza => "Banco Serfinanza",
            BancoColombia.BancoColpatria => "Scotiabank Colpatria",
            BancoColombia.BancoCitibank => "Citibank Colombia",
            BancoColombia.BancoItau => "Banco Itaú Colombia",
            BancoColombia.BancoFinandina => "Banco Finandina",
            BancoColombia.LuloBank => "Lulo Bank",
            BancoColombia.Uala => "Ualá",
            BancoColombia.Bancoldex => "Bancoldex",
            BancoColombia.BancoDeComercio => "Banco de Comercio",
            BancoColombia.BancoProCredit => "Banco ProCredit",
            BancoColombia.BancoCompartir => "Banco Compartir",
            BancoColombia.BancoMundoMujer => "Banco Mundo Mujer",
            BancoColombia.BancoConfiar => "Banco Confiar",
            BancoColombia.Nequi => "Nequi",
            BancoColombia.Daviplata => "Daviplata",
            BancoColombia.Movii => "Movii",
            BancoColombia.TuyaPay => "Tuya Pay",
            BancoColombia.Dale => "Dale",
            BancoColombia.Rappipay => "Rappipay",
            BancoColombia.NuColombia => "Nu Colombia",
            BancoColombia.Otro => "Otro",
            _ => banco.ToString()
        };
    }

    /// <summary>
    /// Obtiene los tipos de cuenta disponibles para un banco específico
    /// Todos los bancos ofrecen los tres tipos: Ahorros, Corriente y Depósito de bajo monto
    /// </summary>
    public static List<TipoCuentaBancaria> GetTiposCuentaDisponibles(BancoColombia banco)
    {
        // Todos los bancos tienen los mismos tres tipos de cuenta disponibles
        return new List<TipoCuentaBancaria>
        {
            TipoCuentaBancaria.Ahorros,
            TipoCuentaBancaria.Corriente,
            TipoCuentaBancaria.DepositoBajoMonto
        };
    }

    /// <summary>
    /// Obtiene todos los bancos disponibles
    /// </summary>
    public static List<BancoColombia> GetAllBancos()
    {
        return Enum.GetValues<BancoColombia>()
            .Where(b => b != BancoColombia.Otro) // Excluir "Otro" de la lista principal
            .OrderBy(b => GetNombreBanco(b))
            .ToList();
    }

    /// <summary>
    /// Convierte un string a BancoColombia si coincide con algún nombre
    /// </summary>
    public static BancoColombia? ParseBanco(string nombreBanco)
    {
        if (string.IsNullOrWhiteSpace(nombreBanco))
            return null;

        var nombreNormalizado = nombreBanco.Trim().ToUpper();
        
        foreach (var banco in Enum.GetValues<BancoColombia>())
        {
            var nombreBancoNormalizado = GetNombreBanco(banco).ToUpper();
            if (nombreBancoNormalizado == nombreNormalizado || 
                nombreBancoNormalizado.Contains(nombreNormalizado) ||
                nombreNormalizado.Contains(nombreBancoNormalizado))
            {
                return banco;
            }
        }
        
        return BancoColombia.Otro;
    }
}
