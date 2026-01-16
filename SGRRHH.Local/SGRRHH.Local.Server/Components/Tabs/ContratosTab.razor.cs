using Microsoft.AspNetCore.Components;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Helpers;
using Microsoft.AspNetCore.Components.Routing;

namespace SGRRHH.Local.Server.Components.Tabs
{
    public class ContratosTabBase : ComponentBase
    {
        [Inject] protected NavigationManager Navigation { get; set; } = default!;

        [Parameter] public int EmpleadoId { get; set; }
        [Parameter] public List<Contrato> Contratos { get; set; } = new();

        protected void IrAContratos()
        {
            Navigation.NavigateTo($"/empleados/{EmpleadoId}/contratos/nuevo");
        }

        protected void IrAEditarContrato(int contratoId)
        {
            Navigation.NavigateTo($"/empleados/{EmpleadoId}/contratos/{contratoId}");
        }

        protected string GetTipoContratoDisplay(TipoContrato tipo)
        {
            return FormatHelper.GetTipoContratoDisplay(tipo);
        }
    }
}
