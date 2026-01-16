using Microsoft.AspNetCore.Components;

using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Server.Components.Expediente;

public partial class DocumentosTab
{
    [Parameter] public List<DocumentoEmpleado> Documentos { get; set; } = new();
    [Parameter] public EventCallback<DocumentoEmpleado> OnPrevisualizarDocumento { get; set; }
    [Parameter] public EventCallback<DocumentoEmpleado> OnDescargarDocumento { get; set; }
    [Parameter] public EventCallback<DocumentoEmpleado> OnImprimirDocumento { get; set; }
    [Parameter] public EventCallback<TipoDocumentoEmpleado> OnAbrirScannerParaTipo { get; set; }
    [Parameter] public EventCallback<TipoDocumentoEmpleado> OnAbrirUploadModalParaTipo { get; set; }
    [Parameter] public EventCallback<DocumentoEmpleado> OnConfirmarEliminarDocumento { get; set; }

    protected static readonly TipoDocumentoEmpleado[] TiposDocumentosRequeridos =
    {
        TipoDocumentoEmpleado.Cedula,
        TipoDocumentoEmpleado.HojaVida,
        TipoDocumentoEmpleado.ContratoFirmado,
        TipoDocumentoEmpleado.AfiliacionEPS,
        TipoDocumentoEmpleado.AfiliacionAFP,
        TipoDocumentoEmpleado.AfiliacionARL,
        TipoDocumentoEmpleado.AfiliacionCajaCompensacion,
        TipoDocumentoEmpleado.CertificadoBancario,
        TipoDocumentoEmpleado.ExamenMedicoIngreso,
        TipoDocumentoEmpleado.Antecedentes,
        TipoDocumentoEmpleado.RUT,
        TipoDocumentoEmpleado.ReferenciasPersonales,
        TipoDocumentoEmpleado.ReferenciasLaborales,
        TipoDocumentoEmpleado.CertificadoEstudios,
        TipoDocumentoEmpleado.Foto
    };
}
