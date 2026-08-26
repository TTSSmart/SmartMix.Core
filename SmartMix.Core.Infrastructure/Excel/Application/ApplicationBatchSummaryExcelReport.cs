namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data;
    using BSU.API.Data.Recipes;
    using BSU.API.Data.Reporting;

    using OfficeOpenXml.Style;
    using System.Collections.Generic;


    /// <summary>
    /// Генератор отчета <see cref="ReportType.ApplicationsWithBatchsSummary"/>.
    /// </summary>
    public class ApplicationBatchSummaryExcelReport : ExcelReportBase
    {
        private readonly ApplicationReport[] _report;

        public ApplicationBatchSummaryExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter)
        {
            _report = report as ApplicationReport[];

            Description = "Отчет по дозированию заявок";
            ReportName = "Паспорт дозирования компонентов смеси";

            DictionaryColumnWidth = new Dictionary<int, double>()
            {
                {1, 7}, // Левая граница
                {2, 9}, //  смещение для замесов
                {3, 24}, // материал
                {4, 12}, // вес по рецепту, кг
                {5, 15}, // корректировка по влажности
                {6, 15}, // корректировка (авто)
                {7, 15}, // корректировка (ручн)
                {8, 12}, // вес факт, кг
                {9, 12}, // в т.ч. в ручном
                {10, 14}, // Погрешность, кг
                {11, 14}, // Погрешность, %
                {12, 13}, // Погрешность факт., кг
                {13, 13} // Погрешность факт., %
            };
            WidthInCells = DictionaryColumnWidth.Count;

            IsLandscape = true;
            UseFooter = true;
        }

        protected override void GenerateReport()
        {
            foreach (ApplicationReport applicationReport in _report)
            {
                RowNumber += 2;

                GenerateApplicationHeader(applicationReport.Id);
                RowNumber += 1;

                GenerateApplicationInfo(applicationReport);
                RowNumber += 2;

                foreach (LayerApplication layer in applicationReport.Layers)
                {
                    if (layer.Recipe.Structures.Count > 0)
                    {
                        int column = StartColumnNumber + 1;

                        ExcelPage.Cells[RowNumber, column].Value = $"Состав рецепта: {layer.Recipe?.Name}";
                        ExcelPage.Cells[RowNumber, column].Style.Font.Bold = true;
                        ExcelPage.Cells[RowNumber, column].Style.Font.Italic = true;
                        RowNumber++;

                        GenerateRecipeTableHeader(column);
                        RowNumber++;

                        GenerateRecipeTable(layer.Recipe);
                        RowNumber++;
                    }

                    RowNumber--;
                    GenerateBatchsInfo(applicationReport, layer.Id);
                    RowNumber++;
                }

                GenerateApplicationSummary(applicationReport);
            }
        }

        /// <summary>
        /// Выводит заголовок по заявке
        /// </summary>
        /// <param name="appId">Номер заявки</param>
        private void GenerateApplicationHeader(int appId)
        {
            ExcelPage.Cells[RowNumber, StartColumnNumber].Value = $"Заявка № {appId}";
            ExcelPage.Cells[RowNumber, StartColumnNumber].Style.Font.Bold = true;

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
        }

        /// <summary>
        /// Выводит общие данные по заявке
        /// </summary>
        /// <param name="applicationReport">Данные по заявке</param>
        private void GenerateApplicationInfo(ApplicationReport applicationReport)
        {
            int column1 = StartColumnNumber;
            const int column2 = 6;
            const int skip = 2;

            ExcelPage.Cells[RowNumber, column1].Value = "№ заявки";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = CheckId(applicationReport.Id);
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.Numberformat.Format = INT_FORMAT;

            if (applicationReport.Layers.Count == 1)
            {
                ExcelPage.Cells[RowNumber, column2].Value = "Рецепт";
                ExcelPage.Cells[RowNumber, column2 + skip].Value = applicationReport.Layers[0].Recipe?.Name;
                ExcelPage.Cells[RowNumber, column2 + skip, RowNumber, column2 + skip + 1].Merge = true;
            }
            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "№ накладной";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = CheckName(applicationReport.WayBill);
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ExcelPage.Cells[RowNumber, column2].Value = "Начат";
            ExcelPage.Cells[RowNumber, column2 + skip].Value = CheckDate(applicationReport.StartTime);
            ExcelPage.Cells[RowNumber, column2 + skip].Style.Numberformat.Format = DATE_TIME_FORMAT;
            ExcelPage.Cells[RowNumber, column2 + skip, RowNumber, column2 + skip + 1].Merge = true;

            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "Заказчик";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = CheckName(applicationReport.Client?.Name);
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ExcelPage.Cells[RowNumber, column2].Value = "Завершен";
            ExcelPage.Cells[RowNumber, column2 + skip].Value = CheckDate(applicationReport.EndTime);
            ExcelPage.Cells[RowNumber, column2 + skip].Style.Numberformat.Format = DATE_TIME_FORMAT;
            ExcelPage.Cells[RowNumber, column2 + skip, RowNumber, column2 + skip + 1].Merge = true;

            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "Машина";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = CheckName(applicationReport.Car?.Name);
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ExcelPage.Cells[RowNumber, column2].Value = "Объём, м³";
            ExcelPage.Cells[RowNumber, column2 + skip].Value = applicationReport.Volume;
            ExcelPage.Cells[RowNumber, column2 + skip].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, column2 + skip, RowNumber, column2 + skip + 1].Merge = true;

            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "№ смесителя:";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = applicationReport.MixerNumber;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.Numberformat.Format = INT_FORMAT;

            ExcelPage.Cells[RowNumber, column2].Value = "Расчетный объём, м³";
            ExcelPage.Cells[RowNumber, column2 + skip].Value = applicationReport.FactualVolume;
            ExcelPage.Cells[RowNumber, column2 + skip].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, column2 + skip, RowNumber, column2 + skip + 1].Merge = true;

            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "Оператор:";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = CheckName(applicationReport.ModifiedBy?.Name);
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            if (applicationReport.RunTime.HasValue)
            {
                ExcelPage.Cells[RowNumber, column2].Value = "Время выполнения:";
                ExcelPage.Cells[RowNumber, column2 + skip].Value = applicationReport.RunTime.Value;
                ExcelPage.Cells[RowNumber, column2 + skip].Style.Numberformat.Format = INT_FORMAT;
                ExcelPage.Cells[RowNumber, column2 + skip, RowNumber, column2 + skip + 1].Merge = true;
            }
        }

        /// <summary>
        /// Формирует заголовок таблицы по структуре/всем материалам рецепта.
        /// </summary>
        /// <param name="column">Начальный номер столбца</param>
        private void GenerateRecipeTableHeader(int column)
        {
            ExcelPage.Cells[RowNumber, column].ApplyHeaderStyle().Value = "Материал";
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 1].Merge = true;

            ExcelPage.Cells[RowNumber, column + 2].ApplyHeaderStyle().Value = "кг/м³";

            ExcelPage.Cells[RowNumber, column, RowNumber, column + 2].ApplyBorders(ExcelBorderStyle.Thin);
        }

        /// <summary>
        /// Выводит информацию по структуре/материалам рецепта.
        /// </summary>
        /// <param name="recipe">Рецепт.</param>
        private void GenerateRecipeTable(Recipe recipe)
        {
            int column = StartColumnNumber + 1;
            int startRow = RowNumber;

            RecipeStructure[] structureData = ReportHelper.Sort(recipe.Structures);

            for (int i = 0; i < structureData.Length; i++)
            {
                ExcelPage.Cells[RowNumber, column].Value = CheckName(structureData[i].Component.Name);
                ExcelPage.Cells[RowNumber, column].Style.WrapText = true;
                ExcelPage.Cells[RowNumber, column, RowNumber, column + 1].Merge = true;

                ExcelPage.Cells[RowNumber, column + 2].Value = structureData[i].Amount; // кг/м3
                ExcelPage.Cells[RowNumber, column + 2].Style.Numberformat.Format = FLOAT_FORMAT;

                //for (int j = 2; j <= colCount + column; j++)
                //    ExcelPage.Cells[RowNumber, j].ApplyBorders(ExcelBorderStyle.Thin);
                RowNumber++;
            }
            ExcelPage.Cells[startRow, column, RowNumber - 1, column + 2].ApplyBorders(ExcelBorderStyle.Thin);
        }

        private void GenerateApplicationSummaryHeader(double volume, double factVolume = 0)
        {
            int column1 = StartColumnNumber;

            ExcelPage.Cells[RowNumber, column1].Value = "Итого по заявке";
            ExcelPage.Cells[RowNumber, column1].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, column1].Style.Font.Italic = true;
            //for (int i = 2; i <= WidthInCells; i++)
            //{
            //    ExcelPage.Cells[RowNumber, i].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            //}
            ExcelPage.Cells[RowNumber, column1, RowNumber, WidthInCells].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;

            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "Объём, м³";
            ExcelPage.Cells[RowNumber, column1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ExcelPage.Cells[RowNumber, column1].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;

            ExcelPage.Cells[RowNumber, column1 + 2].Value = volume;
            ExcelPage.Cells[RowNumber, column1 + 2].Style.Numberformat.Format = FLOAT_FORMAT;
            //ExcelPage.Cells[RowNumber, column1 + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ExcelPage.Cells[RowNumber, column1 + 2, RowNumber, column1 + 3].Merge = true;

            if (factVolume > 0)
            {
                RowNumber++;
                ExcelPage.Cells[RowNumber, column1].Value = "Расчетный объём, м³";
                ExcelPage.Cells[RowNumber, column1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ExcelPage.Cells[RowNumber, column1].Style.Font.Bold = true;
                ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;

                ExcelPage.Cells[RowNumber, column1 + 2].Value = factVolume;
                ExcelPage.Cells[RowNumber, column1 + 2].Style.Numberformat.Format = FLOAT_FORMAT;
                //ExcelPage.Cells[RowNumber, column1 + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ExcelPage.Cells[RowNumber, column1 + 2, RowNumber, column1 + 3].Merge = true;
            }
        }

        private void GenerateApplicationSummary(ApplicationReport applicationReport)
        {
            double volume = 0;
            double factVolume = 0; // объем фактический
            var materialSummary = new Dictionary<string, BatchReportMaterial>();

            foreach (BatchReport batch in applicationReport.Batches)
            {
                volume += batch.Volume;
                factVolume += batch.RealVolume;

                foreach (BatchReportMaterial material in batch.BatchMaterials)
                {
                    if (materialSummary.ContainsKey(material.Material))
                    {
                        materialSummary[material.Material].Manual += material.Manual;
                        materialSummary[material.Material].Need += material.Need;
                        materialSummary[material.Material].Auto += material.Auto;
                        materialSummary[material.Material].ManualCorrection += material.ManualCorrection;
                        materialSummary[material.Material].HumidityCorrection += material.HumidityCorrection;
                    }
                    else
                    {
                        materialSummary.Add(material.Material, material.Clone()); // todo клонирование из-за foreach
                    }
                }
            }

            List<BatchReportMaterial> batchMaterialList = new List<BatchReportMaterial>();
            foreach (KeyValuePair<string, BatchReportMaterial> kvp in materialSummary)
            {
                BatchReportMaterial clone = kvp.Value.Clone();
                clone.Material = kvp.Key; // todo точно нужно?
                batchMaterialList.Add(clone);
            }

            GenerateApplicationSummaryHeader(volume, factVolume);
            RowNumber += 2;

            GenerateBatchMaterialTableHeader(ExcelBorderStyle.Medium, System.Drawing.Color.Gainsboro);
            RowNumber++;

            GenerateBatchMaterialTable(ReportHelper.Sort(batchMaterialList));
        }

        /// <summary>
        /// Формирует заголовок для таблицы по материалам замеса.
        /// </summary>
        /// <param name="borderStyle">Границы.</param>
        /// <param name="color">Цвет заголовка.</param>
        protected void GenerateBatchMaterialTableHeader(ExcelBorderStyle borderStyle = ExcelBorderStyle.Thin, System.Drawing.Color? color = null)
        {
            int colNumber = StartColumnNumber + 1;
            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Материал";
            ExcelPage.Cells[RowNumber, colNumber, RowNumber + 1, colNumber].Merge = true;
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Вес план, кг"; // по рецепту
            ExcelPage.Cells[RowNumber, colNumber, RowNumber + 1, colNumber].Merge = true;
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Корректировка по влажности";
            ExcelPage.Cells[RowNumber, colNumber, RowNumber + 1, colNumber].Merge = true;
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Корректировка";
            ExcelPage.Cells[RowNumber, colNumber, RowNumber, colNumber + 1].Merge = true;
            ExcelPage.Cells[RowNumber + 1, colNumber].ApplyHeaderStyle().Value = "авто";
            colNumber++;

            ExcelPage.Cells[RowNumber + 1, colNumber].ApplyHeaderStyle().Value = "ручная";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Вес факт, кг";
            ExcelPage.Cells[RowNumber, colNumber, RowNumber + 1, colNumber].Merge = true;
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "В т.ч. в ручном режиме";
            ExcelPage.Cells[RowNumber, colNumber].Style.Font.Size = 9;
            ExcelPage.Cells[RowNumber, colNumber, RowNumber + 1, colNumber].Merge = true;
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Погрешность";
            ExcelPage.Cells[RowNumber, colNumber, RowNumber, colNumber + 1].Merge = true;
            ExcelPage.Cells[RowNumber + 1, colNumber].ApplyHeaderStyle().Value = "кг";
            colNumber++;

            ExcelPage.Cells[RowNumber + 1, colNumber].ApplyHeaderStyle().Value = "%";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Погрешность факт";
            ExcelPage.Cells[RowNumber, colNumber, RowNumber, colNumber + 1].Merge = true;
            ExcelPage.Cells[RowNumber + 1, colNumber].ApplyHeaderStyle().Value = "кг";
            colNumber++;

            ExcelPage.Cells[RowNumber + 1, colNumber].ApplyHeaderStyle().Value = "%";

            ExcelPage.Cells[RowNumber, StartColumnNumber + 1, RowNumber + 1, colNumber].ApplyBorders(borderStyle);
            if (color.HasValue)
                ExcelPage.Cells[RowNumber, StartColumnNumber + 1, RowNumber + 1, colNumber].ApplyBackground(color.Value);

            RowNumber++;
        }

        /// <summary>
        /// Выводит информацию по материалам замесов.
        /// </summary>
        /// <param name="batchReportMaterials">Массив материалов по замесу.</param>
        protected void GenerateBatchMaterialTable(BatchReportMaterial[] batchReportMaterials)
        {
            int column = StartColumnNumber + 1;
            int startRow = RowNumber;

            for (int i = 0; i < batchReportMaterials.Length; i++)
            {
                int collCount = 1;
                ExcelPage.Cells[RowNumber, column].Value = batchReportMaterials[i].Material;
                ExcelPage.Cells[RowNumber, column].Style.WrapText = true;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].Need;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].HumidityCorrection;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].AutoCorrection;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].ManualCorrection;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].Real;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].Manual;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].BalanceError;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].BalanceErrorPercent;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = PERCENT_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].FactBalanceError;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].FactBalanceErrorPercent;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = PERCENT_FORMAT;

                RowNumber++;
            }
            ExcelPage.Cells[startRow, column, RowNumber - 1, WidthInCells].ApplyBorders(ExcelBorderStyle.Thin);
        }

        protected virtual void GenerateBatchsInfo(ApplicationReport applicationReport, int idLayer)
        {
        }
    }
}