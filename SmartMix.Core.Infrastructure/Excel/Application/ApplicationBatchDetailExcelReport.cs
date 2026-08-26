namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data.Reporting;

    using OfficeOpenXml.Style;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using TTS.SmartMix.Export.Helpers;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.ApplicationsWithBatchsDetail"/>.
    /// </summary>
    public class ApplicationBatchDetailExcelReport : ApplicationBatchSummaryExcelReport
    {
        /// <inheritdoc/>
        public ApplicationBatchDetailExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, appFilter, firmName, report)
        {
        }

        protected override void GenerateBatchsInfo(ApplicationReport applicationReport, int idLayer)
        {
            foreach (BatchReport batch in applicationReport.Batches.Where(b => b.LayerNumber == idLayer))
            {
                RowNumber++;

                GenerateBatchHeader(batch);
                GenerateBatchInfo(batch);

                GenerateBatchMaterialTableHeader();
                RowNumber++;

                GenerateBatchMaterialTable(ReportHelper.Sort(batch.BatchMaterials));
            }
        }

        /// <summary>
        /// Выводит заголовок для указанного замеса.
        /// </summary>
        /// <param name="batchReport">Данные по замесу.</param>
        private void GenerateBatchHeader(BatchReport batchReport)
        {
            int column = StartColumnNumber + 1;

            ExcelPage.Cells[RowNumber, column].Value = $"Замес № {batchReport.Number}";
            ExcelPage.Cells[RowNumber, column].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, column].Style.Font.Italic = true;
            for (int i = column; i <= WidthInCells; i++)
                ExcelPage.Cells[RowNumber, i].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;

            RowNumber++;
        }

        /// <summary>
        /// Формирует и выводит базовую информацию по указанному замесу.
        /// </summary>
        /// <param name="batchReport">Данные замеса.</param>
        private void GenerateBatchInfo(BatchReport batchReport)
        {
            int column = StartColumnNumber + 1;

            ExcelPage.Cells[RowNumber, column].Value = "Начат";

            ExcelPage.Cells[RowNumber, column + 1].Value = CheckDate(batchReport.StartTime);
            ExcelPage.Cells[RowNumber, column + 1].Style.Numberformat.Format = DATE_TIME_FORMAT;
            ExcelPage.Cells[RowNumber, column + 1, RowNumber, column + 2].Merge = true;

            ExcelPage.Cells[RowNumber, column + 4].Value = "Влажность по рецепту";
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 4 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column + 6].Value = batchReport.HumidityByRecipe;
            ExcelPage.Cells[RowNumber, column + 6].Style.Numberformat.Format = FLOAT_FORMAT;

            ExcelPage.Cells[RowNumber, column + 8].Value = "Ток смесителя";
            ExcelPage.Cells[RowNumber, column + 8, RowNumber, column + 9].Merge = true;

            ExcelPage.Cells[RowNumber, column + 10].Value = $"{batchReport.MixerCurrent.ToString(FLOAT_FORMAT, CultureInfo.InvariantCulture)} A";
            ExcelPage.Cells[RowNumber, column + 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            RowNumber++;

            ExcelPage.Cells[RowNumber, column].Value = "Завершён";

            ExcelPage.Cells[RowNumber, column + 1].Value = CheckDate(batchReport.EndTime);
            ExcelPage.Cells[RowNumber, column + 1].Style.Numberformat.Format = DATE_TIME_FORMAT;
            ExcelPage.Cells[RowNumber, column + 1, RowNumber, column + 2].Merge = true;

            ExcelPage.Cells[RowNumber, column + 4].Value = "Влажность фактическая";
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 4 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column + 6].Value = batchReport.ActualHumidity;
            ExcelPage.Cells[RowNumber, column + 6].Style.Numberformat.Format = FLOAT_FORMAT;

            ExcelPage.Cells[RowNumber, column + 8].Value = "Время перемешивания";
            ExcelPage.Cells[RowNumber, column + 8, RowNumber, column + 9].Merge = true;

            ExcelPage.Cells[RowNumber, column + 10].Value = $"{batchReport.ActualMixingTime} сек";
            ExcelPage.Cells[RowNumber, column + 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            RowNumber++;

            ExcelPage.Cells[RowNumber, column].Value = "Объём, м³";

            ExcelPage.Cells[RowNumber, column + 1].Value = batchReport.Volume;
            ExcelPage.Cells[RowNumber, column + 1].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, column + 1, RowNumber, column + 2].Merge = true;

            RowNumber++;

            ExcelPage.Cells[RowNumber, column].Value = "Объём факт, м³";

            ExcelPage.Cells[RowNumber, column + 1].Value = batchReport.RealVolume;
            ExcelPage.Cells[RowNumber, column + 1].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, column + 1, RowNumber, column + 2].Merge = true;

            RowNumber += 2;
        }
    }
}