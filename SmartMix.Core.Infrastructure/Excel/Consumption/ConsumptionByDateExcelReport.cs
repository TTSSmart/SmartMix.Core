namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data;
    using BSU.API.Data.Reporting;
    using OfficeOpenXml.Style;
    using TTS.SmartMix.Export.Helpers;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.ConsuptionByPeriod"/>
    /// </summary>
    public class ConsumptionByDateExcelReport : ConsumptionExcelReportBase
    {
        private readonly ConsumptionByDateReport[] _reportData;

        public ConsumptionByDateExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter)
        {
            _reportData = report as ConsumptionByDateReport[];

            ReportName = BuildReportName(appFilter.ConsuptionGroupPeriod);
        }

        /// <inheritdoc/>
        protected override void GenerateReport()
        {
            if (_reportData.Length == 0) return;

            for (int i = 0; i < _reportData.Length - 1; i++) // без учёта строки с итогами
            {
                RowNumber += 2;

                string dateString = ExpenditureReport.ToGroupHeaderString(AppFilter.ConsuptionGroupPeriod, _reportData[i].Date, _reportData[i].EndDate);
                GenerateGroupHeader(dateString);
                RowNumber += 2;

                GenerateTableHeader();
                GenerateTableData(_reportData[i].Materials);
            }
            RowNumber += 2;

            // итоговая строка
            GenerateGroupHeader("Итого");
            RowNumber += 2;

            GenerateTableHeader(System.Drawing.Color.Gainsboro);
            GenerateTableData(_reportData[_reportData.Length - 1].Materials);
        }

        /// <inheritdoc/>
        protected override void GenerateFilterInfo()
        {
            base.GenerateFilterInfo();
            if (AppFilter.ConsuptionGroupPeriod > 0)
            {
                AddGeneralFilterInfo("Период группировки данных", ReportHelper.GetEnumDescription(AppFilter.ConsuptionGroupPeriod));
                RowNumber++;
            }
        }

        /// <summary>
        /// Выводит заголовок периода группировки.
        /// </summary>
        /// <param name="groupHeader">Заголовок группы по периоду.</param>
        private void GenerateGroupHeader(string groupHeader)
        {
            ExcelPage.Cells[RowNumber, StartColumnNumber].Value = groupHeader;
            ExcelPage.Cells[RowNumber, StartColumnNumber].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, StartColumnNumber].Style.Font.Italic = true;

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].Merge = true;
            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
        }

        /// <summary>
        /// Формирует полное название отчета с учетом указанной группировки. Возвращает результат выполнения операции.
        /// </summary>
        /// <param name="range">Тип периода группировки данных.</param>
        /// <returns>Название отчета с учетом периода группировки.</returns>
        private static string BuildReportName(RangeTemplateTypes range) // todo дублирование кода?
        {
            switch (range)
            {
                case RangeTemplateTypes.OneHour:
                    return "Расход материала по часам";

                case RangeTemplateTypes.ThreeHours:
                    return "Расход материала за 3 часа";

                case RangeTemplateTypes.SixHours:
                    return "Расход материала за 6 часов";

                case RangeTemplateTypes.TwelveHours:
                    return "Расход материала за 12 часов";

                case RangeTemplateTypes.Day:
                case RangeTemplateTypes.CustomDay:
                    return "Ежедневный расход материала";

                case RangeTemplateTypes.Week:
                    return "Еженедельный расход материала";

                case RangeTemplateTypes.Month:
                    return "Ежемесячный расход материала";

                case RangeTemplateTypes.Year:
                    return "Ежегодный расход материала";

                default:
                    return "Расход материала по периоду";
            }
        }
    }
}