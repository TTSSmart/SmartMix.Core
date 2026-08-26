namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data.Reporting;
    using OfficeOpenXml.Style;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using TTS.SmartMix.Export.DataExport.ExcelExport.Extensions;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.CustomMaterials"/>.
    /// </summary>
    public class ApplicationCustomMaterialExcelReport : AppMaterialReportBase
    {
        /// <summary>
        /// Представляет цвет ячейки, если дата окончания выполнения заявки не совпадает с датой начала (так как по таблице предусмотрено только ВремяПо)
        /// </summary>
        private readonly Color _warnColor = System.Drawing.ColorTranslator.FromHtml("#FEA918");

        /// <summary>
        /// Инициализирует новый экземпляр класса по указанным данным.
        /// </summary>
        /// <param name="lineNumber">Номер линии.</param>
        /// <param name="appFilter">Пользовательский фильтр.</param>
        /// <param name="firmName">Название компании.</param>
        /// <param name="report">Набор данных по отчету типа <see cref="ApplicationReport"/>.</param>
        public ApplicationCustomMaterialExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter, report)
        {
            DictionaryColumnWidth = new Dictionary<int, double>()
            {
                {1, 9},  // Левая граница: пустая колонка A
                {2, 9},  // "№ заявки по заводу";
                {3, 10}, // "Дата с";
                {4, 9}, // "Время с";
                {5, 9}, // "Время по";
                {6, 10}, // "Заказчик";
                {7, 10}, // "Адрес заказчика";
                {8, 14}, // "Миксер"; как гос. номер автобетоносмесителя
                {9, 14}, // "Рецепт";
                {10, 7} // "Куб. м.";
            };
            WidthInCells = DictionaryColumnWidth.Count;

            for (int i = 0; i < MaterialList.Count; i++)
            {
                // планируемое значение по рецепту
                DictionaryColumnWidth.Add(++WidthInCells, 10);
                // фактическое значение
                DictionaryColumnWidth.Add(++WidthInCells, 10);
            }
        }

        /// <inheritdoc/>
        protected override void GenerateReport()
        {
            // формируем заголовок таблицы
            RowNumber++;
            int iColumn = StartColumnNumber; // индекс

            ExcelPage.Cells[RowNumber, iColumn, RowNumber, iColumn + 1].Merge = true;
            ExcelPage.Cells[RowNumber + 1, iColumn].ApplyHeaderStyle().Value = "№ заявки по заводу";
            ExcelPage.Cells[RowNumber + 1, ++iColumn].ApplyHeaderStyle().Value = "Дата";

            ExcelPage.Cells[RowNumber, ++iColumn].ApplyHeaderStyle().Value = "c";
            ExcelPage.Cells[RowNumber, iColumn + 1].ApplyHeaderStyle().Value = "по";
            ExcelPage.Cells[RowNumber + 1, iColumn].ApplyHeaderStyle().Value = "Время";
            ExcelPage.Cells[RowNumber + 1, iColumn, RowNumber + 1, ++iColumn].Merge = true;

            ExcelPage.Cells[RowNumber, ++iColumn, RowNumber, iColumn + 4].Merge = true;
            ExcelPage.Cells[RowNumber + 1, iColumn].ApplyHeaderStyle().Value = "Клиент";
            ExcelPage.Cells[RowNumber + 1, ++iColumn].ApplyHeaderStyle().Value = "Адрес";
            ExcelPage.Cells[RowNumber + 1, ++iColumn].ApplyHeaderStyle().Value = "Миксер";
            ExcelPage.Cells[RowNumber + 1, ++iColumn].ApplyHeaderStyle().Value = "Рецепт";
            ExcelPage.Cells[RowNumber + 1, ++iColumn].ApplyHeaderStyle().Value = "Куб. м.";

            int startPlanColumn = ++iColumn; // запомнили
            ExcelPage.Cells[RowNumber, startPlanColumn].ApplyHeaderStyle().Value = "план"; // в кг

            int startFactColumn;
            int endTableColumn;
            if (MaterialList.Count > 0)
            {
                ExcelPage.Cells[RowNumber, iColumn, RowNumber, iColumn + MaterialList.Count - 1].Merge = true;

                startFactColumn = iColumn + MaterialList.Count; // запомнили
                ExcelPage.Cells[RowNumber, iColumn + MaterialList.Count].ApplyHeaderStyle().Value = "факт"; // в кг

                endTableColumn = iColumn + 2 * MaterialList.Count - 1;
                ExcelPage.Cells[RowNumber, iColumn + MaterialList.Count, RowNumber, endTableColumn].Merge = true;
            }
            else
            {
                startFactColumn = ++iColumn;
                ExcelPage.Cells[RowNumber, startFactColumn].ApplyHeaderStyle().Value = "факт"; // в кг
                endTableColumn = iColumn;
            }

            for (int i = 0; i < MaterialList.Count; i++)
            {
                ExcelPage.Cells[RowNumber + 1, iColumn + i].ApplyHeaderStyle().Value = MaterialList[i].Component.Name; // планируемое значение
                ExcelPage.Cells[RowNumber + 1, iColumn + i].Style.Font.Size = 10;

                ExcelPage.Cells[RowNumber + 1, iColumn + i + MaterialList.Count].ApplyHeaderStyle().Value = MaterialList[i].Component.Name; // фактическое значение
                ExcelPage.Cells[RowNumber + 1, iColumn + i + MaterialList.Count].Style.Font.Size = 10;
            }

            int tableRowNumber = RowNumber;
            RowNumber++;

            // выводим данные в таблицу
            bool overday;
            for (int i = 0; i < ReportData.Length; i++)
            {
                for (int j = 0; j < ReportData[i].Layers.Count; j++)
                {
                    RowNumber++;
                    iColumn = 0;
                    overday = false; // сброс 

                    if (ReportData[i].EndTime.Date > ReportData[i].StartTime.Date)
                        overday = true;

                    // ID заявки
                    ExcelPage.Cells[RowNumber, StartColumnNumber].Value = CheckId(ReportData[i].Id);
                    ExcelPage.Cells[RowNumber, StartColumnNumber].Style.Numberformat.Format = INT_FORMAT;
                    ExcelPage.Cells[RowNumber, StartColumnNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelPage.Cells[RowNumber, StartColumnNumber].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Дата начала выполнения
                    ExcelPage.Cells[RowNumber, StartColumnNumber + ++iColumn].Value = CheckDate(ReportData[i].StartTime, DATE_FORMAT);
                    if (overday) ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].ApplyBackground(_warnColor);
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Время с
                    ExcelPage.Cells[RowNumber, StartColumnNumber + ++iColumn].Value = CheckDate(ReportData[i].StartTime, TIME_FORMAT);
                    if (overday) ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].ApplyBackground(_warnColor);
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Время по
                    ExcelPage.Cells[RowNumber, StartColumnNumber + ++iColumn].Value = CheckDate(ReportData[i].EndTime, TIME_FORMAT);
                    if (overday) ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].ApplyBackground(_warnColor);
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Клиент
                    ExcelPage.Cells[RowNumber, StartColumnNumber + ++iColumn].Value = CheckName(ReportData[i].Client?.Name);
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Адрес клиента
                    ExcelPage.Cells[RowNumber, StartColumnNumber + ++iColumn].Value = CheckName(ReportData[i].Client?.Address);
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Миксер
                    ExcelPage.Cells[RowNumber, StartColumnNumber + ++iColumn].Value = CheckName(ReportData[i].Car?.Name);
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Рецепт
                    ExcelPage.Cells[RowNumber, StartColumnNumber + ++iColumn].Value = ReportData[i].Layers[j].Recipe?.Name;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.WrapText = true;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Куб. м.
                    ExcelPage.Cells[RowNumber, StartColumnNumber + ++iColumn].Value = ReportData[i].Layers[j].Volume;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.Numberformat.Format = FLOAT_FORMAT;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // вывод значений

                    // сколько нужно материала
                    BatchReport[] batchData = ReportData[i].Batches.Where(b => b.LayerNumber == ReportData[i].Layers[j].Id).ToArray();

                    if (ReportData[i].Layers[j].Recipe != null) // у ручных замесов нет структуры данных по рецепту
                    {
                        ++iColumn;
                        for (int m = 0; m < MaterialList.Count; m++)
                        {
                            // планируемое значение, в кг  - отсутствует у ручных замесов
                            float? componentWeight = GetRecipeWeight(MaterialList[m].Component.Id, ReportData[i].Layers[j].Volume, ReportData[i].Layers[j].Recipe.Structures);
                            if (componentWeight.HasValue)
                            {
                                MaterialList[m].TotalRecipeWeight += componentWeight.Value;

                                ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn + m].Value = componentWeight.Value;
                                ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn + m].Style.Numberformat.Format = FLOAT_FORMAT;
                                ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn + m].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }

                            // фактическое значение
                            double componentTotalWeight = GetBatchWeight(MaterialList[m].Component.Id, batchData);
                            if (componentTotalWeight > 0) // todo лучше завязаться на вариант componentWeight.HasValue || IsManual = true
                            {
                                MaterialList[m].TotalWeight += componentTotalWeight;

                                ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn + m + MaterialList.Count].Value = componentTotalWeight;
                                ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn + m + MaterialList.Count].Style.Numberformat.Format = FLOAT_FORMAT;
                                ExcelPage.Cells[RowNumber, StartColumnNumber + iColumn + m + MaterialList.Count].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }
                        }
                    }

                }
            }

            ExcelPage.Cells[tableRowNumber, StartColumnNumber, RowNumber, endTableColumn].ApplyBorders(ExcelBorderStyle.Thin);
            // дополнительно выделили области планируемого/фактического веса
            ExcelPage.Cells[tableRowNumber, startPlanColumn, RowNumber, startPlanColumn].Style.Border.Left.Style = ExcelBorderStyle.Medium;
            ExcelPage.Cells[tableRowNumber, startFactColumn, RowNumber, startFactColumn].Style.Border.Left.Style = ExcelBorderStyle.Medium;
            ExcelPage.Cells[tableRowNumber, endTableColumn, RowNumber, endTableColumn].Style.Border.Right.Style = ExcelBorderStyle.Medium;
        }
    }
}