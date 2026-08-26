namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data.Reporting;
    using OfficeOpenXml.Style;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using TTS.SmartMix.Export.DataExport.ExcelExport.Extensions;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.Materials"/>.
    /// </summary>
    public class ApplicationMaterialExcelReport : AppMaterialReportBase
    {
        /// <summary>
        /// Представляет цвет ячейки планируемого веса (цвет альтернативной строки таблицы).
        /// </summary>
        private readonly Color _alternalteRowColor = System.Drawing.ColorTranslator.FromHtml("#e6f1fa");

        /// <summary>
        /// Инициализирует новый экземпляр класса по указанным данным.
        /// </summary>
        /// <param name="lineNumber">Номер линии.</param>
        /// <param name="appFilter">Пользовательский фильтр.</param>
        /// <param name="firmName">Название компании.</param>
        /// <param name="report">Набор данных по отчету типа <see cref="ApplicationReport"/>.</param>
        public ApplicationMaterialExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter, report)
        {
            DictionaryColumnWidth = new Dictionary<int, double>()
            {
                {1, 7},  // Левая граница: пустая колонка A
                {2, 9},  // "№ заявки";
                {3, 18}, // "Время выполнения, сек";
                {4, 12}, // "Рецепт";
                {5, 15}, // "Куб. м.";
                {6, 9}  // "Тип";
            };
            WidthInCells = DictionaryColumnWidth.Count;

            // значение по рецепту
            for (int i = 0; i < MaterialList.Count; i++)
                DictionaryColumnWidth.Add(++WidthInCells, 12);
        }

        protected override void GenerateReport()
        {
            RowNumber++;
            int colNumber = StartColumnNumber;

            int startRowNumber = RowNumber;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "№ заявки";

            ExcelPage.Cells[RowNumber, colNumber + 1].ApplyHeaderStyle().Value = "Время выполнения, сек";
            ExcelPage.Cells[RowNumber, colNumber + 1].Style.Font.Size = 10;

            ExcelPage.Cells[RowNumber, colNumber + 2].ApplyHeaderStyle().Value = "Рецепт";
            ExcelPage.Cells[RowNumber, colNumber + 3].ApplyHeaderStyle().Value = "Куб. м.";
            ExcelPage.Cells[RowNumber, colNumber + 4].ApplyHeaderStyle().Value = "Тип";

            for (int i = 0; i < MaterialList.Count; i++)
            {
                ExcelPage.Cells[RowNumber, colNumber + 5 + i].ApplyHeaderStyle().Value = MaterialList[i].Component.Name;
                ExcelPage.Cells[RowNumber, colNumber + 5 + i].Style.Font.Size = 10;
            }

            Array.ForEach(ReportData, app =>
            {
                //объединение ячеек
                for (int i = 0; i <= 1; i++)
                    ExcelPage.Cells[RowNumber + 1, colNumber + i, RowNumber + app.Layers.Count * 2, colNumber + i].Merge = true;

                app.Layers.ForEach(l =>
                {
                    RowNumber++;

                    //ID заявки
                    ExcelPage.Cells[RowNumber, colNumber].Value = CheckId(app.Id);

                    //время выполнения заявки, в секундах
                    if (app.RunTime.HasValue)
                        ExcelPage.Cells[RowNumber, colNumber + 1].Value = app.RunTime;// app.EndTime != DateTime.MinValue && app.StartTime != DateTime.MinValue ? (app.EndTime - app.StartTime).TotalSeconds.ToString() : string.Empty;

                    //рецепт
                    ExcelPage.Cells[RowNumber, colNumber + 2].Value = CheckName(l.Recipe?.Name, false);

                    //куб. м.
                    ExcelPage.Cells[RowNumber, colNumber + 3].Value = l.Volume;
                    ExcelPage.Cells[RowNumber, colNumber + 3].Style.Numberformat.Format = FLOAT_FORMAT;

                    //объединение ячеек
                    for (int i = 2; i <= 3; i++)
                        ExcelPage.Cells[RowNumber, colNumber + i, RowNumber + 1, colNumber + i].Merge = true;

                    // колонка тип
                    ExcelPage.Cells[RowNumber, colNumber + 4].Value = "План";
                    ExcelPage.Cells[RowNumber, colNumber + 4].Style.Font.Size = 10;
                    ExcelPage.Cells[RowNumber, colNumber + 4].ApplyBackground(_alternalteRowColor);
                    ExcelPage.Cells[RowNumber, colNumber + 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelPage.Cells[RowNumber, colNumber + 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelPage.Cells[RowNumber + 1, colNumber + 4].Value = "Факт";
                    ExcelPage.Cells[RowNumber + 1, colNumber + 4].Style.Font.Size = 10;
                    ExcelPage.Cells[RowNumber + 1, colNumber + 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelPage.Cells[RowNumber + 1, colNumber + 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    //сколько нужно материала
                    BatchReport[] batchLayers = app.Batches.Where(b => b.LayerNumber == l.Id).ToArray();

                    for (int i = 0; i < MaterialList.Count; i++)
                    {
                        // планируемое значение, в кг  -отсутствует у ручных замесов
                        float? componentWeight = GetRecipeWeight(MaterialList[i].Component.Id, l.Volume, l.Recipe.Structures);
                        ExcelPage.Cells[RowNumber, colNumber + 5 + i].ApplyBackground(_alternalteRowColor); // закрашиваем в любом случае
                        if (componentWeight.HasValue)
                        {
                            MaterialList[i].TotalRecipeWeight += componentWeight.Value;
                            ExcelPage.Cells[RowNumber, colNumber + 5 + i].Value = componentWeight.Value;
                            ExcelPage.Cells[RowNumber, colNumber + 5 + i].Style.Numberformat.Format = FLOAT_FORMAT;
                        }

                        // фактическое значение, в кг
                        double componentTotalWeight = GetBatchWeight(MaterialList[i].Component.Id, batchLayers);
                        if (componentTotalWeight > 0)
                        {
                            MaterialList[i].TotalWeight += componentTotalWeight;

                            ExcelPage.Cells[RowNumber + 1, colNumber + 5 + i].Value = componentTotalWeight;
                            ExcelPage.Cells[RowNumber + 1, colNumber + 5 + i].Style.Numberformat.Format = FLOAT_FORMAT;
                        }
                    }

                    RowNumber++;
                });
            });

            RowNumber++;

            //ToDo перенести в стиль
            ExcelPage.Cells[RowNumber, colNumber + 3, RowNumber + 1, colNumber + 5 + MaterialList.Count - 1].ApplyBackground(Color.Gainsboro);

            ExcelPage.Cells[RowNumber, colNumber + 3, RowNumber + 1, colNumber + 3].Merge = true;

            ExcelPage.Cells[RowNumber, colNumber + 3].ApplyHeaderStyle().Value = "Итого:";
            ExcelPage.Cells[RowNumber, colNumber + 3, RowNumber + 1, colNumber + 3].ApplyBorders(ExcelBorderStyle.Thin);

            ExcelPage.Cells[RowNumber, colNumber + 4].ApplyHeaderStyle().Value = "План";
            ExcelPage.Cells[RowNumber, colNumber + 4].ApplyBorders(ExcelBorderStyle.Thin);

            ExcelPage.Cells[RowNumber + 1, colNumber + 4].ApplyHeaderStyle().Value = "Факт";
            ExcelPage.Cells[RowNumber + 1, colNumber + 4].ApplyBorders(ExcelBorderStyle.Thin);

            int materialIndex = 0;
            MaterialList.ForEach(mat =>
            {
                ExcelPage.Cells[RowNumber, colNumber + 5 + materialIndex].Value = mat.TotalRecipeWeight;//.ToString("0.00");
                ExcelPage.Cells[RowNumber, colNumber + 5 + materialIndex].Style.Numberformat.Format = FLOAT_FORMAT;
                ExcelPage.Cells[RowNumber, colNumber + 5 + materialIndex].ApplyBorders(ExcelBorderStyle.Thin);
                ExcelPage.Cells[RowNumber, colNumber + 5 + materialIndex].Style.Font.Bold = true;
                ExcelPage.Cells[RowNumber, colNumber + 5 + materialIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ExcelPage.Cells[RowNumber + 1, colNumber + 5 + materialIndex].Value = mat.TotalWeight; //.ToString("0.00");
                ExcelPage.Cells[RowNumber + 1, colNumber + 5 + materialIndex].Style.Numberformat.Format = FLOAT_FORMAT;
                ExcelPage.Cells[RowNumber + 1, colNumber + 5 + materialIndex].ApplyBorders(ExcelBorderStyle.Thin);
                ExcelPage.Cells[RowNumber + 1, colNumber + 5 + materialIndex].Style.Font.Bold = true;
                ExcelPage.Cells[RowNumber + 1, colNumber + 5 + materialIndex++].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            });

            //формат ячеек
            for (int i = 0; i < (MaterialList.Count + 5); i++)
            {
                //формат всех ячеек
                for (int j = 0; j < ReportData.Select(r => r.Layers.Count).Sum() * 2 + 1; j++)
                {
                    //ExcelPage.Cells[headerRowNumber + j, collNumber + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelPage.Cells[startRowNumber + j, colNumber + i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ExcelPage.Cells[startRowNumber + j, colNumber + i].ApplyBorders(ExcelBorderStyle.Thin);

                    // формат не числовых значений
                    if (i < 5 && j >= 1)
                        ExcelPage.Cells[startRowNumber + j, colNumber + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    //вкладка Рецепт
                    if (i == 2 && j >= 1)
                        ExcelPage.Cells[startRowNumber + j, colNumber + i].Style.WrapText = true;

                    //формат числовых значений
                    if (i >= 5 && j >= 1)
                        ExcelPage.Cells[startRowNumber + j, colNumber + i].Style.Numberformat.Format = FLOAT_FORMAT;
                }
            }
        }
    }
}