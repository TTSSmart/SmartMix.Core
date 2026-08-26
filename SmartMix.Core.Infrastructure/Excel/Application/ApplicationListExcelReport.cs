namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data;
    using BSU.API.Data.Reporting;
    using OfficeOpenXml.Style;

    using System.Collections.Generic;
    using TTS.SmartMix.Export.DataExport.ExcelExport.Extensions;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.Applications"/>.
    /// </summary>
    public class ApplicationListExcelReport : ExcelReportBase
    {
        private readonly ApplicationReport[] _report;

        /// <inheritdoc/>
        public ApplicationListExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter)
        {
            _report = report as ApplicationReport[];

            Description = "История заявок";
            ReportName = "История заявок";

            DictionaryColumnWidth = new Dictionary<int, double>()
            {
                { 1, 4}, // Левая граница
                { 2, 10}, // Номер заявки
                { 3, 10}, // Порядок выполнения
                { 4, 10}, // Накладная
                { 5, 14}, // Заказчик
                { 6, 14}, // Машина
                { 7, 14}, // Рецепт
                { 8, 11}, // Объем планируемый
                { 9, 11}, // Объем выполненный
                { 10, 10}, // Объем расчетный
                { 11, 10}, // Смеситель
                { 12, 11}, // Время выполнения
                { 13, 18}, // Начат 
                { 14, 18}, // Завершен
                { 15, 18} // Оператор
            };
            WidthInCells = DictionaryColumnWidth.Count;

            IsLandscape = true;
            UseFooter = true;
        }

        /// <inheritdoc/>
        protected override void GenerateReport()
        {
            RowNumber++;
            GenerateApplicationTableHeader();

            foreach (ApplicationReport applicationReport in _report)
            {
                RowNumber++;
                GenerateApplicationTable(applicationReport);
            }
        }

        /// <summary>
        /// Выполняет формирование заголовка таблицы.
        /// </summary>
        private void GenerateApplicationTableHeader()
        {
            int column = StartColumnNumber;

            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "№ заявки";
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "№ п/п";
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "№ накладной";
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Заказчик";
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Машина";
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Рецепт";

            ExcelPage.Cells[RowNumber, column].Style.Font.Size = 10;
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Объем план, куб.м";

            ExcelPage.Cells[RowNumber, column].Style.Font.Size = 10;
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Объем выполненный, куб.м";

            ExcelPage.Cells[RowNumber, column].Style.Font.Size = 10;
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Объем факт, куб.м";

            ExcelPage.Cells[RowNumber, column].Style.Font.Size = 10;
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Смеситель";

            ExcelPage.Cells[RowNumber, column].Style.Font.Size = 10;
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Время выполнения, сек";

            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Начат";
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Завершен";

            ExcelPage.Cells[RowNumber, column].ApplyHeaderStyle().Value = "Оператор";

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].ApplyBorders(ExcelBorderStyle.Thin);
        }

        /// <summary>
        /// Выводит информацию в таблицу.
        /// </summary>
        /// <param name="applicationReport">Запись по заявке.</param>
        private void GenerateApplicationTable(ApplicationReport applicationReport)
        {
            int startRow = RowNumber;

            foreach (LayerApplication layer in applicationReport.Layers)
            {
                int column = StartColumnNumber;

                ExcelPage.Cells[RowNumber, column].Value = applicationReport.Id;
                ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = INT_FORMAT;
                ExcelPage.Cells[RowNumber, column++].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ExcelPage.Cells[RowNumber, column].Value = layer.Number;
                ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = INT_FORMAT;
                ExcelPage.Cells[RowNumber, column++].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ExcelPage.Cells[RowNumber, column].Value = CheckName(applicationReport.WayBill);
                ExcelPage.Cells[RowNumber, column++].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ExcelPage.Cells[RowNumber, column++].Value = CheckName(applicationReport.Client?.Name);

                ExcelPage.Cells[RowNumber, column].Value = CheckName(applicationReport.Car?.Name);
                ExcelPage.Cells[RowNumber, column++].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ExcelPage.Cells[RowNumber, column].Value = layer.Recipe?.Name;
                ExcelPage.Cells[RowNumber, column++].AutoFitColumns();

                ExcelPage.Cells[RowNumber, column].Value = layer.Volume;
                ExcelPage.Cells[RowNumber, column++].Style.Numberformat.Format = FLOAT_FORMAT;

                ExcelPage.Cells[RowNumber, column].Value = layer.CurVolume;
                ExcelPage.Cells[RowNumber, column++].Style.Numberformat.Format = FLOAT_FORMAT;

                ExcelPage.Cells[RowNumber, column].Value = layer.FactVolume;
                ExcelPage.Cells[RowNumber, column++].Style.Numberformat.Format = FLOAT_FORMAT;

                ExcelPage.Cells[RowNumber, column].Value = applicationReport.MixerNumber;
                ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = INT_FORMAT;
                ExcelPage.Cells[RowNumber, column++].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                if (applicationReport.RunTime.HasValue)
                {
                    ExcelPage.Cells[RowNumber, column].Value = applicationReport.RunTime.Value;
                    ExcelPage.Cells[RowNumber, column++].Style.Numberformat.Format = INT_FORMAT;
                }
                else
                {
                    column++;
                }

                ExcelPage.Cells[RowNumber, column].Value = CheckDate(applicationReport.StartTime);
                ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = DATE_TIME_FORMAT;
                ExcelPage.Cells[RowNumber, column++].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ExcelPage.Cells[RowNumber, column].Value = CheckDate(applicationReport.EndTime);
                ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = DATE_TIME_FORMAT;
                ExcelPage.Cells[RowNumber, column++].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ExcelPage.Cells[RowNumber, column].Value = CheckName(applicationReport.ModifiedBy?.Name);

                if (applicationReport.Layers.IndexOf(layer) != applicationReport.Layers.Count - 1)
                    RowNumber++;
            }
            ExcelPage.Cells[startRow, StartColumnNumber, startRow, WidthInCells].ApplyBorders(ExcelBorderStyle.Thin);
        }
    }
}