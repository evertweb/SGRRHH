using Microsoft.AspNetCore.Components;

using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Domain.Services;
using SGRRHH.Local.Shared.Interfaces;
using SGRRHH.Local.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace SGRRHH.Local.Server.Components.Tabs
{
    public partial class InformacionBancariaTab
    {
        [Parameter] public int EmpleadoId { get; set; }
        [Parameter] public Empleado? Empleado { get; set; }
        [Parameter] public List<DocumentoEmpleado> Documentos { get; set; } = new();
        [Parameter] public dynamic? MessageToast { get; set; }

        // Callbacks para interactuar con modales del padre
        [Parameter] public EventCallback<DocumentoEmpleado> OnPreviewDocumento { get; set; }
        [Parameter] public EventCallback<DocumentoEmpleado> OnDownloadDocumento { get; set; }
        [Parameter] public EventCallback<int> OnSolicitarScannerCertificado { get; set; }

        [Inject] private ICuentaBancariaRepository CuentaBancariaRepo { get; set; } = default!;
        [Inject] private IDocumentoEmpleadoRepository DocumentoRepo { get; set; } = default!;
        [Inject] private ILogger<InformacionBancariaTab> Logger { get; set; } = default!;

        // Estado local
        private List<CuentaBancaria> cuentasBancarias = new();
        private int? cuentaEditandoId = null;
        private CuentaBancaria? cuentaEnEdicion = null;
        private bool isSavingCuenta = false;
        private CuentaBancaria? cuentaAEliminar = null;
        private bool showConfirmDeleteCuenta = false;
        private bool isLoading = true;
        
        // Estado para selección de banco
        private BancoColombia? bancoSeleccionado = null;
        private string bancoOtroTexto = string.Empty;
        private List<BancoColombia> bancosDisponibles = new();

        protected override async Task OnParametersSetAsync()
        {
            if (EmpleadoId > 0 && isLoading)
            {
                bancosDisponibles = BancoService.GetAllBancos();
                await CargarCuentas();
            }
        }

        private async Task CargarCuentas()
        {
            try
            {
                await RecargarCuentasAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al cargar cuentas bancarias");
                MessageToast?.ShowError("Error al cargar cuentas bancarias");
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Recarga las cuentas bancarias desde la base de datos con ordenamiento
        /// </summary>
        private async Task RecargarCuentasAsync()
        {
            cuentasBancarias = (await CuentaBancariaRepo.GetByEmpleadoIdAsync(EmpleadoId))
                .OrderByDescending(c => c.EsCuentaNomina)
                .ThenByDescending(c => c.FechaCreacion)
                .ToList();
            StateHasChanged();
        }

        // Método público para ser llamado desde el padre cuando termina un scan
        public async Task AsociarCertificadoACuenta(int cuentaId, DocumentoEmpleado documento)
        {
            try
            {
                var cuenta = cuentasBancarias.FirstOrDefault(c => c.Id == cuentaId);
                if (cuenta != null)
                {
                    cuenta.DocumentoCertificacionId = documento.Id;
                    await CuentaBancariaRepo.UpdateAsync(cuenta);
                    
                    // Actualizar UI
                    Logger.LogInformation("Certificado bancario vinculado a cuenta {CuentaId}", cuenta.Id);
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error vinculando certificado a cuenta");
                MessageToast?.ShowError("Error al vincular el certificado a la cuenta");
            }
        }

        private void AgregarNuevaCuenta()
        {
            cuentaEditandoId = 0; // 0 indica nueva cuenta
            bancoSeleccionado = null;
            bancoOtroTexto = string.Empty;
            cuentaEnEdicion = new CuentaBancaria
            {
                EmpleadoId = EmpleadoId,
                EstaActiva = true,
                FechaApertura = DateTime.Today,
                TipoCuenta = TipoCuentaBancaria.Ahorros,
                NombreTitular = Empleado?.NombreCompleto ?? string.Empty,
                DocumentoTitular = Empleado?.Cedula ?? string.Empty,
                Observaciones = string.Empty
            };
        }

        private void EditarCuenta(CuentaBancaria cuenta)
        {
            cuentaEditandoId = cuenta.Id;
            // Clonar objeto para edición sin afectar la lista visualmente hasta guardar
            cuentaEnEdicion = new CuentaBancaria
            {
                Id = cuenta.Id,
                EmpleadoId = cuenta.EmpleadoId,
                Banco = cuenta.Banco,
                TipoCuenta = cuenta.TipoCuenta,
                NumeroCuenta = cuenta.NumeroCuenta,
                // Siempre usar los datos actuales del empleado para nombre y documento
                NombreTitular = Empleado?.NombreCompleto ?? cuenta.NombreTitular ?? string.Empty,
                DocumentoTitular = Empleado?.Cedula ?? cuenta.DocumentoTitular ?? string.Empty,
                FechaApertura = cuenta.FechaApertura,
                EsCuentaNomina = cuenta.EsCuentaNomina,
                EstaActiva = cuenta.EstaActiva,
                Observaciones = cuenta.Observaciones,
                DocumentoCertificacionId = cuenta.DocumentoCertificacionId,

            };
            
            // Intentar parsear el banco desde el string almacenado
            var bancoParseado = BancoService.ParseBanco(cuenta.Banco);
            if (bancoParseado.HasValue && bancoParseado.Value == BancoColombia.Otro)
            {
                bancoSeleccionado = BancoColombia.Otro;
                bancoOtroTexto = cuenta.Banco;
            }
            else if (bancoParseado.HasValue)
            {
                bancoSeleccionado = bancoParseado.Value;
                bancoOtroTexto = string.Empty;
            }
            else
            {
                bancoSeleccionado = null;
                bancoOtroTexto = cuenta.Banco;
            }
        }

        private void CancelarEdicionCuenta()
        {
            cuentaEditandoId = null;
            cuentaEnEdicion = null;
            bancoSeleccionado = null;
            bancoOtroTexto = string.Empty;
        }

        private async Task GuardarNuevaCuenta()
        {
            // Reutilizamos la misma lógica de guardado
            await GuardarCuentaEditada();
        }

        private async Task GuardarCuentaEditada()
        {
            if (cuentaEnEdicion == null) return;

            // Validaciones básicas
            if (!bancoSeleccionado.HasValue)
            {
                MessageToast?.ShowError("Debe seleccionar un banco");
                return;
            }
            
            // Establecer el nombre del banco según la selección
            if (bancoSeleccionado.Value == BancoColombia.Otro)
            {
                if (string.IsNullOrWhiteSpace(bancoOtroTexto))
                {
                    MessageToast?.ShowError("Debe especificar el nombre del banco");
                    return;
                }
                cuentaEnEdicion.Banco = bancoOtroTexto.Trim();
            }
            else
            {
                cuentaEnEdicion.Banco = BancoService.GetNombreBanco(bancoSeleccionado.Value);
            }
            
            if (string.IsNullOrWhiteSpace(cuentaEnEdicion.NumeroCuenta))
            {
                MessageToast?.ShowError("El número de cuenta es obligatorio");
                return;
            }
            
            // Asignar siempre los valores del empleado (los campos son readonly pero se aseguran aquí)
            if (Empleado != null)
            {
                cuentaEnEdicion.NombreTitular = Empleado.NombreCompleto;
                cuentaEnEdicion.DocumentoTitular = Empleado.Cedula;
            }
            
            // Validación de respaldo (nunca debería fallar si Empleado existe)
            if (string.IsNullOrWhiteSpace(cuentaEnEdicion.NombreTitular))
            {
                MessageToast?.ShowError("El nombre del titular es obligatorio");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(cuentaEnEdicion.DocumentoTitular))
            {
                MessageToast?.ShowError("El documento del titular es obligatorio");
                return;
            }
            
            if (!cuentaEnEdicion.FechaApertura.HasValue)
            {
                MessageToast?.ShowError("La fecha de apertura es obligatoria");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(cuentaEnEdicion.Observaciones))
            {
                MessageToast?.ShowError("Las observaciones son obligatorias");
                return;
            }

            isSavingCuenta = true;
            try
            {
                if (cuentaEnEdicion.Id == 0)
                {
                    // Nueva cuenta
                    var nuevaCuenta = await CuentaBancariaRepo.AddAsync(cuentaEnEdicion);
                    MessageToast?.ShowSuccess("Cuenta bancaria agregada exitosamente");
                    
                    // Si es nómina, desmarcar otras
                    if (nuevaCuenta.EsCuentaNomina)
                    {
                        await AsegurarUnicaCuentaNomina(nuevaCuenta.Id);
                    }
                    
                    // Recargar lista para mantener ordenamiento
                    await RecargarCuentasAsync();
                }
                else
                {
                    // Actualizar existente
                    await CuentaBancariaRepo.UpdateAsync(cuentaEnEdicion);
                    
                    MessageToast?.ShowSuccess("Cuenta bancaria actualizada exitosamente");

                    // Si se marcó como nómina, desmarcar otras
                    if (cuentaEnEdicion.EsCuentaNomina)
                    {
                        await AsegurarUnicaCuentaNomina(cuentaEnEdicion.Id);
                    }
                    
                    // Recargar lista para mantener ordenamiento
                    await RecargarCuentasAsync();
                }

                CancelarEdicionCuenta();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al guardar cuenta bancaria");
                MessageToast?.ShowError("Error al guardar la cuenta bancaria");
            }
            finally
            {
                isSavingCuenta = false;
            }
        }

        private async Task AsegurarUnicaCuentaNomina(int cuentaIdNomina)
        {
            // Busca otras cuentas marcadas como nómina y las desmarca
            var otrasCuentasNomina = cuentasBancarias.Where(c => c.EsCuentaNomina && c.Id != cuentaIdNomina).ToList();
            foreach (var cuenta in otrasCuentasNomina)
            {
                cuenta.EsCuentaNomina = false;
                await CuentaBancariaRepo.UpdateAsync(cuenta);
            }
            
            // Si hubo cambios, recargar lista para asegurar consistencia
            if (otrasCuentasNomina.Any())
            {
                 // Actualización local rápida
                 foreach (var c in cuentasBancarias.Where(c => c.Id != cuentaIdNomina))
                 {
                     c.EsCuentaNomina = false;
                 }
                 StateHasChanged();
            }
        }

        private async Task MarcarComoNomina(int cuentaId)
        {
            var cuenta = cuentasBancarias.FirstOrDefault(c => c.Id == cuentaId);
            if (cuenta == null) return;

            try
            {
                cuenta.EsCuentaNomina = true;
                await CuentaBancariaRepo.UpdateAsync(cuenta);
                await AsegurarUnicaCuentaNomina(cuentaId);
                MessageToast?.ShowSuccess("Cuenta marcada como principal de nómina");
                
                // Recargar lista para mantener ordenamiento
                await RecargarCuentasAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al marcar cuenta de nómina");
                MessageToast?.ShowError("Error al actualizar la cuenta");
            }
        }

        private void ConfirmarEliminarCuenta(CuentaBancaria cuenta)
        {
            cuentaAEliminar = cuenta;
            showConfirmDeleteCuenta = true;
        }

        private async Task EliminarCuenta()
        {
            if (cuentaAEliminar == null) return;

            try
            {
                await CuentaBancariaRepo.DeleteAsync(cuentaAEliminar.Id);
                MessageToast?.ShowSuccess("Cuenta bancaria eliminada");
                
                // Recargar lista para mantener ordenamiento
                await RecargarCuentasAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error eliminando cuenta bancaria");
                MessageToast?.ShowError("Error al eliminar la cuenta");
            }
            finally
            {
                showConfirmDeleteCuenta = false;
                cuentaAEliminar = null;
            }
        }
        
        // Helpers para UI

        private async Task PrevisualizarDocumento(DocumentoEmpleado doc)
        {
            await OnPreviewDocumento.InvokeAsync(doc);
        }

        private async Task DescargarDocumento(DocumentoEmpleado doc)
        {
            await OnDownloadDocumento.InvokeAsync(doc);
        }

        private async Task AbrirScannerParaCertificadoBancario(int cuentaId)
        {
            await OnSolicitarScannerCertificado.InvokeAsync(cuentaId);
        }
        
        // Métodos para manejar cambios en el banco
        private void OnBancoSelectChanged(ChangeEventArgs e)
        {
            if (e.Value != null && int.TryParse(e.Value.ToString(), out var bancoInt))
            {
                bancoSeleccionado = (BancoColombia)bancoInt;
                OnBancoChanged();
            }
            else
            {
                bancoSeleccionado = null;
            }
        }
        
        private void OnBancoChanged()
        {
            if (bancoSeleccionado.HasValue)
            {
                if (bancoSeleccionado.Value != BancoColombia.Otro)
                {
                    bancoOtroTexto = string.Empty;
                }
                // No es necesario cambiar el tipo de cuenta ya que todos los bancos tienen los mismos tipos disponibles
            }
        }
        
        private List<TipoCuentaBancaria> GetTiposCuentaDisponibles()
        {
            // Todos los bancos tienen los mismos tres tipos de cuenta disponibles
            return BancoService.GetTiposCuentaDisponibles(BancoColombia.Bancolombia);
        }
        
        /// <summary>
        /// Obtiene el nombre legible del tipo de cuenta
        /// </summary>
        private string GetNombreTipoCuenta(TipoCuentaBancaria tipo)
        {
            return tipo switch
            {
                TipoCuentaBancaria.Ahorros => "AHORROS",
                TipoCuentaBancaria.Corriente => "CORRIENTE",
                TipoCuentaBancaria.DepositoBajoMonto => "DEPÓSITO DE BAJO MONTO",
                _ => tipo.ToString().ToUpper()
            };
        }
    }
}
