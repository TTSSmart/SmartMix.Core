namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data.Reporting;

    using OfficeOpenXml.Style;
    using System;
    using System.Collections.Generic;
    using TTS.SmartMix.Export.DataExport.ExcelExport.Extensions;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.ConsumptionByBunker"/>
    /// </summary>
    public class ConsumptionByBunkerExcelReport : ExcelReportBase
    {
        /// <summary>
        /// Представляет данные отчёта по расходу на бункере.
        /// </summary>
        private BunkerConsumptionReport[] _report;

        /// <summary>
        /// Инициализирует новый экземпляр класса по указанным данным.
        /// </summary>
        /// <param name="lineNumber">Номер линии.</param>
        /// <param name="appFilter">Пользовательский фильтр.</param>
        /// <param name="firmName">Название компании.</param>
        /// <param name="report">Набор данных по отчету типа <see cref="BunkerConsumptionReport"/>.</param>
        public ConsumptionByBunkerExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter)
        {
            _report = report as BunkerConsumptionReport[];

            ReportName = $"Расход по бункеру N{appFilter.BunkerNumber}";
            Description = $"Отчет по расходу материала в бункере [Номер датчика: {appFilter.SensorNumber}]";

            DictionaryColumnWidth = new Dictionary<int, double>()
            {
                {1, 4}, // Левая граница
                {2, 10}, // Дата снятия показаний
                {3, 10}, //
                {4, 10}, // Событие
                {5, 10}, // 
                {6, 10}, //
                {7, 10}, // Приход
                {8, 10}, // Расход
                {9, 15} // Расчетный остаток
            };
            WidthInCells = DictionaryColumnWidth.Count;

            IsLandscape = false;
            UseFooter = true;
        }

        /// <inheritdoc/>
        protected override void GenerateReport()
        {
            RowNumber++;
            GenerateTableHeader();

            int startTableRow = RowNumber + 1;
            foreach (BunkerConsumptionReport bcr in _report)
            {
                RowNumber++;
                GenerateTable(bcr);
            }
            int endTableRow = RowNumber;


            GenerateSummaryHeader();
            RowNumber++;

            GenerateSummaryTableHeader(ExcelBorderStyle.Medium, System.Drawing.Color.Gainsboro);
            GenerateSummaryTable(startTableRow, endTableRow, _report[_report.Length - 1].Remains);
        }

        /// <inheritdoc/>
        protected override void GenerateFilterInfo()
        {
            //base.GenerateFilterInfo(); слишком много лишнего
            int column = StartColumnNumber;
            int skip = 3;

            ExcelPage.Cells[RowNumber, column].Value = "Выбранные фильтры:";
            ExcelPage.Cells[RowNumber, column].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, column].Style.Font.Italic = true;
            ExcelPage.Cells[RowNumber, column, RowNumber, column + skip - 1].Merge = true;
            RowNumber++;

            if (AppFilter.StartDate > DateTime.MinValue)
            {
                AddGeneralFilterInfo("Начало периода:", AppFilter.StartDate, DATE_TIME_FORMAT);
                RowNumber++;
            }

            if (AppFilter.EndDate > DateTime.MinValue)
            {
                AddGeneralFilterInfo("Конец периода:", AppFilter.EndDate, DATE_TIME_FORMAT);
                RowNumber++;
            }

            if (AppFilter.IsTrainMode)
            {
                AddGeneralFilterInfo("Режим:", "Тренажёр");
                RowNumber++;
            }

            if (AppFilter.HasConsumptionByBunkerFilter())
            {
                AddGeneralFilterInfo("Датчик", AppFilter.SensorNumber);
                RowNumber++;
            }
        }

        /// <summary>
        /// Выполняет формирование заголовка таблицы.
        /// </summary>
        private void GenerateTableHeader()
        {
            int column = StartColumnNumber;

            ExcelPage.Cells[RowNumber, column].ApplyHeaderStyle().Value = "Дата показаний";
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 1].Merge = true;
            column += 2;

            ExcelPage.Cells[RowNumber, column].ApplyHeaderStyle().Value = "Событие";
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 2].Merge = true;
            column += 3;

            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Приход";
            ExcelPage.Cells[RowNumber, column++].ApplyHeaderStyle().Value = "Расход";
            ExcelPage.Cells[RowNumber, column].ApplyHeaderStyle().Value = "Расчетный остаток";

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].ApplyBorders(ExcelBorderStyle.Thin);
        }

        /// <summary>
        /// Выводит информацию в таблицу.
        /// </summary>
        /// <param name="bcr">Показания сенсора.</param>
        private void GenerateTable(BunkerConsumptionReport bcr)
        {
            int column = StartColumnNumber;

            ExcelPage.Cells[RowNumber, column].Value = bcr.Date;
            ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = DATE_TIME_FORMAT;
            ExcelPage.Cells[RowNumber, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 1].Merge = true;
            column += 2;

            // событие
            string eventString = "";
            switch (bcr.Type)
            {
                case BunkerLevelType.Consumption:
                    eventString = string.Format("Заявка {0} замес {1}", bcr.ApplicationNumber, bcr.BatchNumber);
                    break;
                case BunkerLevelType.Loading:
                    eventString = "Загрузка бункера";
                    break;
            }
            ExcelPage.Cells[RowNumber, column].Value = eventString;
            ExcelPage.Cells[RowNumber, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ExcelPage.Cells[RowNumber, column].Style.WrapText = true;
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 2].Merge = true;
            column += 3;

            // приход
            ExcelPage.Cells[RowNumber, column].Value = bcr.Load;
            ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, column++].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            // расход
            ExcelPage.Cells[RowNumber, column].Value = bcr.Consumption;
            ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, column++].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            // расчётный остаток
            ExcelPage.Cells[RowNumber, column].Value = bcr.Remains;
            ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, column].ApplyBorders(ExcelBorderStyle.Thin);
        }

        /// <summary>
        /// Выводит заголовок для итоговых данных.
        /// </summary>
        private void GenerateSummaryHeader()
        {
            RowNumber += 2;
            int column = StartColumnNumber + 1;

            ExcelPage.Cells[RowNumber, column].Value = "Итого за период";
            ExcelPage.Cells[RowNumber, column].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, column].Style.Font.Italic = true;

            for (int j = column; j <= WidthInCells; j++)
                ExcelPage.Cells[RowNumber, j].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;

            RowNumber++;
        }

        /// <summary>
        /// Выполняет формирование заголовка таблицы итогов.
        /// </summary>
        /// <param name="borderStyle">Границы.</param>
        /// <param name="color">Цвет заголовка.</param>
        private void GenerateSummaryTableHeader(ExcelBorderStyle borderStyle = ExcelBorderStyle.Thin, System.Drawing.Color? color = null)
        {
            int startColumn = StartColumnNumber + 1;
            int column = startColumn;

            ExcelPage.Cells[RowNumber, column, RowNumber, column + 3].Merge = true;
            column += 4;

            ExcelPage.Cells[RowNumber, column].ApplyHeaderStyle().Value = "Расчетные значения";
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 1].Merge = true;

            ExcelPage.Cells[RowNumber, startColumn, RowNumber, column + 1].ApplyBorders(borderStyle);
            if (color.HasValue)
                ExcelPage.Cells[RowNumber, startColumn, RowNumber, column + 1].ApplyBackground(color.Value);

            RowNumber++;
        }

        /// <summary>
        /// Выводит итоговую таблицу
        /// </summary>
        /// <param name="startRow">Начальная строка в таблице данных.</param>
        /// <param name="endRow">Последняя строка в таблице данных.</param>
        /// <param name="endingBalance">Остаток на конец периода.</param>
        private void GenerateSummaryTable(int startRow, int endRow, float endingBalance)
        {
            int startRowNumber = RowNumber;
            int column = StartColumnNumber + 1;
            ExcelPage.Cells[RowNumber, column].ApplyLongHeaderStyle(ExcelHorizontalAlignment.Left).Value = "Показание на начало периода";
            //ExcelPage.Cells[RowNumber, column].ApplyBackground(color);
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 3].Merge = true;

            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Formula = string.Format("I{0} - G{0} + H{0}", startRow);
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Merge = true;

            RowNumber++;

            ExcelPage.Cells[RowNumber, column].ApplyLongHeaderStyle(ExcelHorizontalAlignment.Left).Value = "Приход за период";
            //ExcelPage.Cells[RowNumber, column].ApplyBackground(color);
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 3].Merge = true;

            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Formula = string.Format("SUM(G{0}:G{1})", startRow, endRow);
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Merge = true;

            RowNumber++;

            ExcelPage.Cells[RowNumber, column].ApplyLongHeaderStyle(ExcelHorizontalAlignment.Left).Value = "Расход за период";
            //ExcelPage.Cells[RowNumber, column].ApplyBackground(color);
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 3].Merge = true;

            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Formula = string.Format("SUM(H{0}:H{1})", startRow, endRow);
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Merge = true;

            RowNumber++;

            ExcelPage.Cells[RowNumber, column].ApplyLongHeaderStyle(ExcelHorizontalAlignment.Left).Value = "Показание на конец периода";
            //ExcelPage.Cells[RowNumber, column].ApplyBackground(color);
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 3].Merge = true;

            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Value = endingBalance; // string.Format("I{0}", endRow);
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ExcelPage.Cells[RowNumber, column + 4, RowNumber, column + 5].Merge = true;

            ExcelPage.Cells[startRowNumber, column, RowNumber, column + 5].ApplyBorders(ExcelBorderStyle.Thin);
        }
    }
}