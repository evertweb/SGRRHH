using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using iText.Forms;
using iText.Kernel.Pdf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SGRRHH.Local.Domain.Interfaces;
using SGRRHH.Local.Infrastructure.Services.Pdf;
using SGRRHH.Local.Shared.Interfaces;
using Xunit;

namespace SGRRHH.Local.Tests.Infrastructure
{
    public class PdfTemplateTests
    {
        [Fact]
        public async Task GenerarTemplatePdfAsync_DeberiaCrearArchivoConCamposCorrectos()
        {
            // Arrange
            // Configurar ruta temporal para el test
            string tempPath = Path.Combine(Path.GetTempPath(), "SGRRHH_Tests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(tempPath, "templates"));
            
            var logger = Substitute.For<ILogger<PdfHojaVidaService>>();
            var config = Substitute.For<IConfiguration>();
            // Simular configuración devolviendo la ruta temporal
            config["WebRootPath"].Returns(tempPath);
            
            var empleadoRepo = Substitute.For<IEmpleadoRepository>();
            var aspiranteRepo = Substitute.For<IAspiranteRepositorio>();
            
            // Usar implementaciones reales para la lógica PDF
            var loggerXmp = Substitute.For<ILogger<XmpMetadataHandler>>();
            var xmpHandler = new XmpMetadataHandler(loggerXmp);
            
            var loggerMapper = Substitute.For<ILogger<PdfFieldMapper>>();
            var fieldMapper = new PdfFieldMapper(loggerMapper);
            
            var service = new PdfHojaVidaService(
                logger, 
                config, 
                empleadoRepo, 
                aspiranteRepo, 
                xmpHandler, 
                fieldMapper);

            // Act
            // Al llamar a GenerarPdfVacioAsync, el servicio detectará que no existe el template y lo creará
            byte[] pdfBytes = await service.GenerarPdfVacioAsync();

            // Assert
            string templateGeneradoPath = Path.Combine(tempPath, "templates", "hoja_vida_template.pdf");
            Assert.True(File.Exists(templateGeneradoPath), $"El archivo template no fue creado en {templateGeneradoPath}");
            
            // Verificar contenido del PDF generado
            using (var reader = new PdfReader(templateGeneradoPath))
            using (var pdfDoc = new PdfDocument(reader))
            {
                var form = PdfAcroForm.GetAcroForm(pdfDoc, false);
                Assert.NotNull(form);
                
                var fields = form.GetAllFormFields();
                
                // Verificar campos críticos
                VerificarCampo(fields, "Nombres");
                VerificarCampo(fields, "Apellidos");
                VerificarCampo(fields, "Cedula");
                VerificarCampo(fields, "FechaNacimiento");
                VerificarCampo(fields, "Genero");
                VerificarCampo(fields, "EstadoCivil");
                VerificarCampo(fields, "Email");
                VerificarCampo(fields, "Telefono");
                
                // Verificar campos repetitivos
                VerificarCampo(fields, "Form1_Institucion");
                VerificarCampo(fields, "Exp1_Empresa");
                VerificarCampo(fields, "Ref1_Nombre");
                
                // Verificar firma
                VerificarCampo(fields, "Sig");
                
                // Imprimir ubicación para recolección manual si es necesario
                Console.WriteLine($"Template generado exitosamente en: {templateGeneradoPath}");
            }
            
            // Cleanup (Opcional: comentar si se quiere inspeccionar el archivo)
            // Directory.Delete(tempPath, true);
        }
        
        private void VerificarCampo(IDictionary<string, iText.Forms.Fields.PdfFormField> fields, string nombreCampo)
        {
            Assert.True(fields.ContainsKey(nombreCampo), $"El campo '{nombreCampo}' no existe en el PDF generado.");
        }
    }
}
