using System;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SGRRHH.WPF.Services
{
    /// <summary>
    /// Servicio de impresión para documentos y reportes con estilo Legacy.
    /// </summary>
    public class PrintService
    {
        private static PrintService? _instance;
        public static PrintService Instance => _instance ??= new PrintService();

        private PrintService() { }

        /// <summary>
        /// Imprime un control visual directamente.
        /// </summary>
        /// <param name="visual">El control a imprimir</param>
        /// <param name="title">Título del documento</param>
        /// <param name="showDialog">Mostrar diálogo de impresión</param>
        public bool PrintVisual(Visual visual, string title, bool showDialog = true)
        {
            try
            {
                var printDialog = new PrintDialog();

                if (showDialog)
                {
                    if (printDialog.ShowDialog() != true)
                        return false;
                }

                printDialog.PrintVisual(visual, title);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error de Impresión",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Imprime un FlowDocument con formato.
        /// </summary>
        public bool PrintFlowDocument(FlowDocument document, string title, bool showDialog = true)
        {
            try
            {
                var printDialog = new PrintDialog();

                if (showDialog)
                {
                    if (printDialog.ShowDialog() != true)
                        return false;
                }

                // Configurar el documento para la impresión
                document.PageHeight = printDialog.PrintableAreaHeight;
                document.PageWidth = printDialog.PrintableAreaWidth;
                document.PagePadding = new Thickness(50);
                document.ColumnWidth = double.PositiveInfinity;

                var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
                printDialog.PrintDocument(paginator, title);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error de Impresión",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Crea un FlowDocument con estilo Legacy para imprimir datos tabulares.
        /// </summary>
        /// <param name="title">Título del reporte</param>
        /// <param name="headers">Encabezados de las columnas</param>
        /// <param name="rows">Filas de datos</param>
        public FlowDocument CreateTableDocument(string title, string[] headers, string[][] rows)
        {
            var document = new FlowDocument
            {
                FontFamily = new FontFamily("Tahoma"),
                FontSize = 10,
                PagePadding = new Thickness(50)
            };

            // Título
            var titleParagraph = new Paragraph(new Run(title))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            document.Blocks.Add(titleParagraph);

            // Fecha de impresión
            var dateParagraph = new Paragraph(new Run($"Fecha de impresión: {DateTime.Now:dd/MM/yyyy HH:mm}"))
            {
                FontSize = 9,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 0, 0, 10)
            };
            document.Blocks.Add(dateParagraph);

            // Tabla
            var table = new Table
            {
                CellSpacing = 0,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };

            // Columnas
            foreach (var _ in headers)
            {
                table.Columns.Add(new TableColumn());
            }

            // Grupo de filas
            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            // Fila de encabezados
            var headerRow = new TableRow
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 128)),
                Foreground = Brushes.White
            };

            foreach (var header in headers)
            {
                var cell = new TableCell(new Paragraph(new Run(header))
                {
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5, 3, 5, 3)
                })
                {
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(0, 0, 1, 1)
                };
                headerRow.Cells.Add(cell);
            }
            rowGroup.Rows.Add(headerRow);

            // Filas de datos
            bool alternate = false;
            foreach (var row in rows)
            {
                var dataRow = new TableRow
                {
                    Background = alternate 
                        ? new SolidColorBrush(Color.FromRgb(240, 240, 240)) 
                        : Brushes.White
                };

                foreach (var cellData in row)
                {
                    var cell = new TableCell(new Paragraph(new Run(cellData ?? string.Empty))
                    {
                        Margin = new Thickness(5, 2, 5, 2)
                    })
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(0, 0, 1, 1)
                    };
                    dataRow.Cells.Add(cell);
                }
                rowGroup.Rows.Add(dataRow);
                alternate = !alternate;
            }

            document.Blocks.Add(table);

            // Pie de página
            var footerParagraph = new Paragraph(new Run($"Total de registros: {rows.Length}"))
            {
                FontSize = 9,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            document.Blocks.Add(footerParagraph);

            return document;
        }

        /// <summary>
        /// Crea un FlowDocument simple con texto.
        /// </summary>
        public FlowDocument CreateSimpleDocument(string title, string content)
        {
            var document = new FlowDocument
            {
                FontFamily = new FontFamily("Tahoma"),
                FontSize = 11,
                PagePadding = new Thickness(50)
            };

            // Título
            var titleParagraph = new Paragraph(new Run(title))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            document.Blocks.Add(titleParagraph);

            // Línea separadora
            var separator = new Paragraph(new Run(new string('-', 60)))
            {
                FontFamily = new FontFamily("Courier New"),
                FontSize = 10,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };
            document.Blocks.Add(separator);

            // Contenido
            var contentParagraph = new Paragraph(new Run(content))
            {
                TextAlignment = TextAlignment.Left
            };
            document.Blocks.Add(contentParagraph);

            // Pie de página
            var footerParagraph = new Paragraph(new Run($"\n\nImpreso: {DateTime.Now:dd/MM/yyyy HH:mm}"))
            {
                FontSize = 9,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Right
            };
            document.Blocks.Add(footerParagraph);

            return document;
        }

        /// <summary>
        /// Imprime la ficha de un empleado.
        /// </summary>
        public FlowDocument CreateEmployeeDocument(
            string nombre, 
            string cedula, 
            string cargo, 
            string departamento,
            string fechaIngreso,
            string email,
            string telefono,
            string observaciones)
        {
            var document = new FlowDocument
            {
                FontFamily = new FontFamily("Tahoma"),
                FontSize = 11,
                PagePadding = new Thickness(50)
            };

            // Encabezado con estilo Legacy
            var headerParagraph = new Paragraph(new Run("FICHA DEL EMPLEADO"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 128)),
                Foreground = Brushes.White,
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 0, 20)
            };
            document.Blocks.Add(headerParagraph);

            // Datos principales
            AddLabelValuePair(document, "Nombre Completo:", nombre);
            AddLabelValuePair(document, "Cédula:", cedula);
            AddLabelValuePair(document, "Cargo:", cargo);
            AddLabelValuePair(document, "Departamento:", departamento);
            AddLabelValuePair(document, "Fecha de Ingreso:", fechaIngreso);
            AddLabelValuePair(document, "Email:", email);
            AddLabelValuePair(document, "Teléfono:", telefono);

            // Separador
            var separator = new Paragraph(new Run(new string('─', 50)))
            {
                FontFamily = new FontFamily("Courier New"),
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 15, 0, 15)
            };
            document.Blocks.Add(separator);

            // Observaciones
            if (!string.IsNullOrWhiteSpace(observaciones))
            {
                var obsHeader = new Paragraph(new Run("OBSERVACIONES:"))
                {
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                document.Blocks.Add(obsHeader);

                var obsContent = new Paragraph(new Run(observaciones))
                {
                    Margin = new Thickness(0, 0, 0, 15)
                };
                document.Blocks.Add(obsContent);
            }

            // Pie de página
            var footer = new Paragraph(new Run($"Documento generado el {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:HH:mm}"))
            {
                FontSize = 9,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 30, 0, 0)
            };
            document.Blocks.Add(footer);

            return document;
        }

        private void AddLabelValuePair(FlowDocument document, string label, string value)
        {
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0, 0, 0, 5)
            };

            paragraph.Inlines.Add(new Run(label)
            {
                FontWeight = FontWeights.Bold
            });
            paragraph.Inlines.Add(new Run($" {value}"));

            document.Blocks.Add(paragraph);
        }

        /// <summary>
        /// Obtiene lista de impresoras disponibles.
        /// </summary>
        public string[] GetAvailablePrinters()
        {
            try
            {
                var printServer = new LocalPrintServer();
                var queues = printServer.GetPrintQueues();
                var printers = new System.Collections.Generic.List<string>();
                
                foreach (var queue in queues)
                {
                    printers.Add(queue.Name);
                }
                
                return printers.ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }
}
