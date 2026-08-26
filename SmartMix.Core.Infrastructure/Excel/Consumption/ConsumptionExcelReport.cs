namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data.Reporting;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.Consumption"/>.
    /// </summary>
    public class ConsumptionExcelReport : ConsumptionExcelReportBase
    {
        private ExpenditureReport[] _reportData;

        public ConsumptionExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter)
        {
            ReportName = "Отчет по расходу материалов"; //"Расход материала по заявкам";

            _reportData = report as ExpenditureReport[];
        }

        /// <inheritdoc/>
        protected override void GenerateReport()
        {
            RowNumber++;
            GenerateTableHeader();
            GenerateTableData(_reportData);
        }
    }
}