namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data.Reporting;
    using global::SmartMix.Core.Domain.Entities;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.RecipeVolume"/>
    /// </summary>
    public class RecipeVolumeExcelReport : VolumeExcelReportBase
    {
        public RecipeVolumeExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, appFilter, firmName, report)
        {
            Description = "Отчет по выработке смесей";
            ReportName = "Отчет по выработке смесей";
        }

        /// <summary>
        /// Возвращает заголовок поля, в разрезе которого идет подсчет объемов.
        /// </summary>
        protected override string GroupHeader
        {
            get
            {
                return "Рецепт";
            }
        }
    }
}