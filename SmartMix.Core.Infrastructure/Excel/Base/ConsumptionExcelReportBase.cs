namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data.Reporting;
    using OfficeOpenXml.Style;
    using System.Collections.Generic;

    /// <summary>
    /// Представляет базовый класс формирования отчета по расходу материала.
    /// </summary>
    public abstract class ConsumptionExcelReportBase : ExcelReportBase
    {
        public ConsumptionExcelReportBase(int lineNumber, string firmName, ApplicationFilter appFilter) : base(lineNumber, firmName, appFilter)
        {
            Description = "Отчет по расходу материалов";

            DictionaryColumnWidth = new Dictionary<int, double>()
            {
                {1, 4}, // Левая граница
                {2, 14}, // Материал
                {3, 14}, //
                {4, 17}, // Расход номинальный
                {5, 15}, // Расход фактический 
                {6, 14}, // В т.ч. в ручном
                {7, 14}, // Погрешность автоматики
                {8, 14}, // Погрешность автоматики, %
                {9, 14}, // Погрешность фактическая
                {10, 14} // Погрешность фактическая, %
            };
            WidthInCells = DictionaryColumnWidth.Count;

            IsLandscape = false;
            UseFooter = true;
        }

        /// <summary>
        /// Формирует заголовки таблицы.
        /// </summary>
        /// <param name="color">Цвет заголовков.</param>
        protected void GenerateTableHeader(System.Drawing.Color? color = null)
        {
            int columnNum = StartColumnNumber;

            ExcelPage.Cells[RowNumber, columnNum].ApplyHeaderStyle().Value = "Материал";
            ExcelPage.Cells[RowNumber, columnNum, RowNumber + 1, columnNum + 1].Merge = true;
            columnNum += 2;

            ExcelPage.Cells[RowNumber, columnNum].ApplyHeaderStyle().Value = "Расход номинальный, кг";
            ExcelPage.Cells[RowNumber, columnNum, RowNumber + 1, columnNum].Merge = true;

            ExcelPage.Cells[RowNumber, ++columnNum].ApplyHeaderStyle().Value = "Расход факт, кг";
            ExcelPage.Cells[RowNumber, columnNum, RowNumber + 1, columnNum].Merge = true;

            ExcelPage.Cells[RowNumber, ++columnNum].ApplyHeaderStyle().Value = "В т.ч. в ручном режиме";
            ExcelPage.Cells[RowNumber, columnNum].Style.Font.Size = 10;
            ExcelPage.Cells[RowNumber, columnNum, RowNumber + 1, columnNum].Merge = true;

            ExcelPage.Cells[RowNumber, ++columnNum].ApplyHeaderStyle().Value = "Погрешность";
            ExcelPage.Cells[RowNumber, columnNum, RowNumber, columnNum + 1].Merge = true;
            ExcelPage.Cells[RowNumber + 1, columnNum].ApplyHeaderStyle().Value = "кг";
            ExcelPage.Cells[RowNumber + 1, columnNum + 1].ApplyHeaderStyle().Value = "%";
            columnNum += 2;

            ExcelPage.Cells[RowNumber, columnNum].ApplyHeaderStyle().Value = "Погрешность факт";
            ExcelPage.Cells[RowNumber, columnNum, RowNumber, columnNum + 1].Merge = true;
            ExcelPage.Cells[RowNumber + 1, columnNum].ApplyHeaderStyle().Value = "кг";
            ExcelPage.Cells[RowNumber + 1, ++columnNum].ApplyHeaderStyle().Value = "%";

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber + 1, columnNum].ApplyBorders(ExcelBorderStyle.Thin);
            if (color.HasValue)
                ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber + 1, columnNum].ApplyBackground(color.Value);

            RowNumber++;
        }

        protected void GenerateTableData(ExpenditureReport[] reportData)
        {
            ExpenditureReport[] data = ReportHelper.Sort(reportData); // здесь уже сгруппированы
            foreach (ExpenditureReport item in data)
            {
                RowNumber++;
                AddTableRow(item);
            }
        }

        private void AddTableRow(ExpenditureReport expenditureReport)
        {
            int col = StartColumnNumber;

            ExcelPage.Cells[RowNumber, col].Value = CheckName(expenditureReport.MaterialName);
            ExcelPage.Cells[RowNumber, col, RowNumber, col + 1].Merge = true;
            col += 2;

            ExcelPage.Cells[RowNumber, col].Value = expenditureReport.RatedConsumption;
            ExcelPage.Cells[RowNumber, col].Style.Numberformat.Format = FLOAT_FORMAT;
            col++;

            ExcelPage.Cells[RowNumber, col].Value = expenditureReport.ActualConsumption;
            ExcelPage.Cells[RowNumber, col].Style.Numberformat.Format = FLOAT_FORMAT;
            col++;

            ExcelPage.Cells[RowNumber, col].Value = expenditureReport.ManualConsumption;
            ExcelPage.Cells[RowNumber, col].Style.Numberformat.Format = FLOAT_FORMAT;
            col++;

            ExcelPage.Cells[RowNumber, col].Value = expenditureReport.BalanceError;
            ExcelPage.Cells[RowNumber, col].Style.Numberformat.Format = FLOAT_FORMAT;
            col++;

            ExcelPage.Cells[RowNumber, col].Value = expenditureReport.BalanceErrorPercent;
            ExcelPage.Cells[RowNumber, col].Style.Numberformat.Format = PERCENT_FORMAT;
            col++;

            ExcelPage.Cells[RowNumber, col].Value = expenditureReport.FactBalanceError;
            ExcelPage.Cells[RowNumber, col].Style.Numberformat.Format = FLOAT_FORMAT;
            col++;

            ExcelPage.Cells[RowNumber, col].Value = expenditureReport.FactBalanceErrorPercent;
            ExcelPage.Cells[RowNumber, col].Style.Numberformat.Format = PERCENT_FORMAT;

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, col].ApplyBorders(ExcelBorderStyle.Thin);
        }
    }
}
