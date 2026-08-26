namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data.Reporting;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.ClientVolume"/>
    /// </summary>
    public class RecipeClientVolumeExcelReport : VolumeExcelReportBase
    {
        public RecipeClientVolumeExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report) : base(lineNumber, appFilter, firmName, report)
        {
            Description = "Отчет по выработке смесей по заказчикам";
            ReportName = "Отчет по выработке смесей по заказчикам";
        }

        /// <summary>
        /// Возвращает заголовок поля, в разрезе которого идет подсчет объемов.
        /// </summary>
        protected override string GroupHeader
        {
            get
            {
                return "Заказчик";
            }
        }
    }
}
