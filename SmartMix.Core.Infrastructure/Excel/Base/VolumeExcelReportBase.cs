namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data.Reporting;
    using OfficeOpenXml.Style;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using TTS.SmartMix.Export.DataExport.ExcelExport.Extensions;

    /// <summary>
    /// Представляет базовый класс формирования отчета по объемам.
    /// </summary>
    public abstract class VolumeExcelReportBase : ExcelReportBase
    {
        /// <summary>
        /// Представляет цвет ячейки планируемого веса (цвет альтернативной строки таблицы).
        /// </summary>
        private readonly Color _alternalteRowColor = System.Drawing.ColorTranslator.FromHtml("#e6f1fa");

        /// <summary>
        /// Инициализирует новый экземпляр класса по указанным параметрам.
        /// </summary>
        /// <param name="lineNumber">Номер линии.</param>
        /// <param name="appFilter">Пользовательский фильтр.</param>
        /// <param name="firmName">Название компании.</param>
        /// <param name="report">Данные отчета.</param>
        protected VolumeExcelReportBase(int lineNumber, ApplicationFilter appFilter, string firmName, object report) : base(lineNumber, firmName, appFilter)
        {
            ReportData = report as VolumeReport[];

            DictionaryColumnWidth = new Dictionary<int, double>()
            {
                {1, 9}, // Левая граница
                {2, 7}, // Рецепт
                {3, 9}, // Рецепт
                {4, 18}, // Рецепт
                {5, 12}, // Рецепт
                {6, 10}, // Смеситель
                {7, 12}, // Объём заказанный
                {8, 12}, //
                {9, 13}, // Объём выполненный
                {10, 16}, //
                {11, 12}, // Объём фактический, расчетный
                {12, 13} //
            };
            WidthInCells = DictionaryColumnWidth.Count;

            IsLandscape = false;
            UseFooter = true;
        }

        /// <summary>
        /// Представляет исходные данные отчета.
        /// </summary>
        protected readonly VolumeReport[] ReportData;

        /// <summary>
        /// Возвращает заголовок поля, в разрезе которого идет подсчет объемов.
        /// </summary>
        /// <value>
        /// Например, рецепт для <see cref="ReportType.RecipeVolume"/>, заказчик для <see cref="ReportType.ClientVolume"/>
        /// </value>
        protected abstract string GroupHeader { get; }

        /// <inheritdoc/>
        protected override void GenerateReport()
        {
            RowNumber++;
            GenerateTableHeader();

            int alternateMixerNumber = 0;
            Dictionary<int, VolumeReport[]> dict = ReportData.GroupBy(x => x.MixerNumber).ToDictionary(x => x.Key, x => x.ToArray());
            if (dict.Count > 1)
                alternateMixerNumber = dict.Keys.ToArray().Max();

            // выводим построчно
            foreach (VolumeReport volumeReport in ReportData)
            {
                RowNumber++;
                if (volumeReport.MixerNumber == alternateMixerNumber)
                    GenerateTableVolumeReport(volumeReport, _alternalteRowColor);
                else
                    GenerateTableVolumeReport(volumeReport);
            }
            RowNumber++;

            // Итого по смесителям
            bool withHeader = true; // разовая операция: инициализация
            double totalVolume = 0;
            double totalFactVolume = 0;
            double totalCurVolume = 0;

            foreach (KeyValuePair<int, VolumeReport[]> kvp in dict)
            {
                double mixerVolume = 0;
                double mixerFactVolume = 0;
                double mixerCurVolume = 0;
                for (int i = 0; i < kvp.Value.Length; i++)
                {
                    mixerVolume += kvp.Value[i].CubicMetreQuantity;
                    mixerFactVolume += kvp.Value[i].CubicMetreQuantityFact;
                    mixerCurVolume += kvp.Value[i].TotalCurVolume;
                }

                Color rowColor = dict.Count == 1
                    ? System.Drawing.Color.Gainsboro
                    : (kvp.Key == alternateMixerNumber ? _alternalteRowColor : Color.Transparent);

                GenerateTableVolumeReportSummary(mixerVolume, mixerFactVolume, mixerCurVolume, rowColor, kvp.Key, withHeader ? "Итого:" : null);
                RowNumber++;
                withHeader = false; // сбросили

                totalVolume += mixerVolume;
                totalFactVolume += mixerFactVolume;
                totalCurVolume += mixerCurVolume;
            }

            // Итого
            if (dict.Count > 1)
                GenerateTableVolumeReportSummary(totalVolume, totalFactVolume, totalCurVolume, System.Drawing.Color.Gainsboro, header: "Общий итог:");
        }

        /// <summary>
        /// Выводит заголовок таблицы.
        /// </summary>
        private void GenerateTableHeader()
        {
            int colNumber = StartColumnNumber;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = GroupHeader;
            ExcelPage.Cells[RowNumber, colNumber, RowNumber, colNumber + 3].Merge = true;
            colNumber += 4;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Смеситель";
            ExcelPage.Cells[RowNumber, colNumber++].Style.Font.Size = 10;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Объем план, куб.м";
            ExcelPage.Cells[RowNumber, colNumber, RowNumber, colNumber + 1].Merge = true;
            colNumber += 2;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Объем выполненный, куб.м";
            ExcelPage.Cells[RowNumber, colNumber, RowNumber, colNumber + 1].Merge = true;
            colNumber += 2;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Объем факт, куб.м";
            ExcelPage.Cells[RowNumber, colNumber, RowNumber, colNumber + 1].Merge = true;

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, colNumber + 1].ApplyBorders(ExcelBorderStyle.Thin);
        }

        /// <summary>
        /// Выводит данные по объемам рецепта.
        /// </summary>
        /// <param name="volumeReport">Данные по объемам рецепта</param>
        private void GenerateTableVolumeReport(VolumeReport volumeReport, Color? color = null)
        {
            int column = StartColumnNumber;

            ExcelPage.Cells[RowNumber, column].Value = CheckName(volumeReport.RecipeName); // здесь на самом деле нет ручных замесов, так как объемы по ним равны 0
            if (color.HasValue) ExcelPage.Cells[RowNumber, column].ApplyBackground(color.Value);
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 3].Merge = true;
            column += 4;

            ExcelPage.Cells[RowNumber, column].Value = volumeReport.MixerNumber;
            if (color.HasValue) ExcelPage.Cells[RowNumber, column].ApplyBackground(color.Value);
            ExcelPage.Cells[RowNumber, column++].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            ExcelPage.Cells[RowNumber, column].Value = volumeReport.CubicMetreQuantity;
            ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = FLOAT_FORMAT;
            if (color.HasValue) ExcelPage.Cells[RowNumber, column].ApplyBackground(color.Value);
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 1].Merge = true;
            column += 2;

            ExcelPage.Cells[RowNumber, column].Value = volumeReport.TotalCurVolume;
            ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = FLOAT_FORMAT;
            if (color.HasValue) ExcelPage.Cells[RowNumber, column].ApplyBackground(color.Value);
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 1].Merge = true;
            column += 2;

            ExcelPage.Cells[RowNumber, column].Value = volumeReport.CubicMetreQuantityFact;
            ExcelPage.Cells[RowNumber, column].Style.Numberformat.Format = FLOAT_FORMAT;
            if (color.HasValue) ExcelPage.Cells[RowNumber, column].ApplyBackground(color.Value);
            ExcelPage.Cells[RowNumber, column, RowNumber, column + 1].Merge = true;

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].ApplyBorders(ExcelBorderStyle.Thin);
        }

        /// <summary>
        /// Выводит итоговую строку по таблице.
        /// </summary>
        /// <param name="volume">Общий планируемый/заказанный объем, куб.м.</param>
        /// <param name="factVolume">Общий фактический /расчётный объём, куб.м.</param>
        /// <param name="curVolume">Общий выполненный объём, куб.м.</param>
        /// <param name="color">Цвет ячейки</param>
        /// <param name="mixerNumber">Виртуальный номер смесителя.</param>
        /// <param name="header">Заголовок итоговой строки</param>
        private void GenerateTableVolumeReportSummary(double volume, double factVolume, double curVolume, Color color, int? mixerNumber = null, string header = null)
        {
            int colNumber = StartColumnNumber;

            if (!string.IsNullOrEmpty(header))
                ExcelPage.Cells[RowNumber, colNumber].ApplyLongHeaderStyle().Value = header;
            ExcelPage.Cells[RowNumber, colNumber].ApplyBackground(color);
            ExcelPage.Cells[RowNumber, colNumber, RowNumber, colNumber + 3].Merge = true;
            if (string.IsNullOrEmpty(header)) // объединяем итоги по смесителям
                ExcelPage.Cells[RowNumber - 1, colNumber, RowNumber, colNumber + 3].Merge = true;
            colNumber += 4;

            if (mixerNumber.HasValue)
                ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle(ExcelHorizontalAlignment.Center).Value = mixerNumber;
            ExcelPage.Cells[RowNumber, colNumber].ApplyBackground(color);
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyLongHeaderStyle().Value = volume;
            ExcelPage.Cells[RowNumber, colNumber].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, colNumber].ApplyBackground(color);
            ExcelPage.Cells[RowNumber, colNumber, RowNumber, colNumber + 1].Merge = true;
            colNumber += 2;

            ExcelPage.Cells[RowNumber, colNumber].ApplyLongHeaderStyle().Value = curVolume;
            ExcelPage.Cells[RowNumber, colNumber].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, colNumber].ApplyBackground(color);
            ExcelPage.Cells[RowNumber, colNumber, RowNumber, colNumber + 1].Merge = true;
            colNumber += 2;

            ExcelPage.Cells[RowNumber, colNumber].ApplyLongHeaderStyle().Value = factVolume;
            ExcelPage.Cells[RowNumber, colNumber].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, colNumber].ApplyBackground(color);
            ExcelPage.Cells[RowNumber, colNumber, RowNumber, colNumber + 1].Merge = true;

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].ApplyBorders(ExcelBorderStyle.Thin);
        }
    }
}
