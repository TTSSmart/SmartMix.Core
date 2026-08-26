namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using TTS.Logger;
    using ValueType = Enums.ValueType;

    /// <summary>
    /// Представляет класс описания базового отчета выгрузки в MS Excel.
    /// </summary>
    public abstract class ExcelReportBase : IReport
    {
        public const string DATE_FORMAT = "dd.MM.yyyy";
        public const string DATE_TIME_FORMAT = "dd.MM.yyyy HH:mm:ss";
        public const string TIME_FORMAT = "HH:mm:ss";
        public const string FLOAT_FORMAT = "0.00";
        public const string PERCENT_FORMAT = "0.00%";
        public const string INT_FORMAT = "0";

        /// <summary>
        /// Инициализирует новый экземпляр класса по указанным параметрам.
        /// </summary>
        /// <param name="lineNumber">Номер линии.</param>
        /// <param name="firmName">Название компании.</param>
        /// <param name="appFilter">Пользовательский фильтр.</param>
        protected ExcelReportBase(int lineNumber, string firmName, ApplicationFilter appFilter)
        {
            LineNumber = lineNumber;
            FirmName = firmName;
            AppFilter = appFilter;
        }

        #region Properties

        /// <summary>
        /// Возвращает или задаёт описание отчета.
        /// Вставляется в комментарии к документу.
        /// </summary>
        protected string Description { get; set; }

        /// <summary>
        /// Возвращает или задаёт как название вкладки MS Excel, так и заголовок отчёта.
        /// </summary>
        protected string ReportName { get; set; }

        /// <summary>
        /// Представляет словарь колонок, где ключом является порядковый номер столбца, а значением - его ширина (в каких единицах??)
        /// </summary>
        protected Dictionary<int, double> DictionaryColumnWidth { get; set; } = new Dictionary<int, double>();

        private int _widthInCells;
        /// <summary>
        /// Значение исключительно больше "5". Если для отчета нужно меньше, переформируйте отчет, добавь слияние ячеек для основной информации.
        /// Если задать меньше 5 генерация шапки вызовет исключение из-за наложения слияния ячеек.
        /// </summary>
        /// <value>Минимальное значение: 5.</value>
        protected int WidthInCells
        {
            get { return _widthInCells; }
            set { _widthInCells = value < 5 ? 5 : value; }
        }

        /// <summary>
        /// Возвращает или задаёт признак альбомного расположения отчета.
        /// </summary>
        protected bool IsLandscape { get; set; }

        /// <summary>
        /// Возвращает или задаёт признак нижних колонтитулов с нумерацией страниц.
        /// </summary>
        /// <remarks>Страница &P из &N</remarks>
        protected bool UseFooter { get; set; }

        /// <summary>
        /// Возвращает или задаёт файл выгрузки MS Excel.
        /// </summary>
        protected ExcelPackage ExcelFile { get; private set; }

        /// <summary>
        /// Возвращает или задаёт страницу файла выгрузки.
        /// </summary>
        protected ExcelWorksheet ExcelPage { get; private set; }

        /// <summary>
        /// Возвращает или задаёт название объекта/компании.
        /// </summary>
        protected string FirmName { get; set; }

        /// <summary>
        /// Возвращает или задаёт номер линии.
        /// </summary>
        protected int LineNumber { get; set; }

        /// <summary>
        /// Возвращает или задаёт пользовательский фильтр.
        /// </summary>
        protected ApplicationFilter AppFilter { get; set; }

        /// <summary>
        /// Возвращает или задаёт текущий номер строки.
        /// </summary>
        protected int RowNumber { get; set; } = 1;

        /// <summary>
        /// Возвращает или задаёт начальный номер столбца на листе.
        /// </summary>
        protected int StartColumnNumber { get; set; } = 2;

        #endregion Properties

        /// <summary>
        /// Формирует отчет в формате MS EXCEL. Возвращает результат выполнения операции.
        /// </summary>
        /// <returns>MemoryStream</returns>
        public MemoryStream GetReport()
        {
            // создали файл и страницу
            InitExcel();

            // добавили верхний колонтитул
            GenerateReportHeader();
            RowNumber += 2;

            // добавили информацию по фильтрам
            GenerateFilterInfo();

            // вывели отчет
            GenerateReport();

            // вернули
            MemoryStream stream = new MemoryStream();
            ExcelFile.SaveAs(stream);
            try
            {
                ExcelFile.Dispose();
            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteError("Произошла ошибка при закрытии ExcelFile", ex);
            }
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        /// <summary>
        /// Создаёт файл EXCEL.
        /// </summary>
        protected virtual void InitExcel()
        {
            ExcelFile = new ExcelPackage();

            ExcelFile.Workbook.Properties.Created = DateTime.Now;
            ExcelFile.Workbook.Properties.Author = "SmartMix 2";
            ExcelFile.Workbook.Properties.Comments = Description;

            ExcelPage = ExcelFile.Workbook.Worksheets.Add(ReportName);

            foreach (KeyValuePair<int, double> kvp in DictionaryColumnWidth)
                ExcelPage.Column(kvp.Key).Width = kvp.Value;

            ExcelPage.PrinterSettings.RightMargin = (decimal)0.3;
            ExcelPage.PrinterSettings.LeftMargin = (decimal)0.3;
            ExcelPage.PrinterSettings.TopMargin = (decimal)0.3;
            ExcelPage.PrinterSettings.BottomMargin = (decimal)0.5;
            ExcelPage.PrinterSettings.FooterMargin = (decimal)0.3;

            ExcelPage.PrinterSettings.Orientation = IsLandscape ? eOrientation.Landscape : eOrientation.Portrait;
            ExcelPage.PrinterSettings.PaperSize = ePaperSize.A4;
            ExcelPage.PrinterSettings.FitToPage = true;
            ExcelPage.PrinterSettings.FitToWidth = 1;
            ExcelPage.PrinterSettings.FitToHeight = 0;

            if (UseFooter)
            {
                ExcelPage.HeaderFooter.EvenFooter.RightAlignedText = "Страница &P из &N";
                ExcelPage.HeaderFooter.OddFooter.RightAlignedText = "Страница &P из &N";
            }
        }

        /// <summary>
        /// Выполняет формирование отчета.
        /// </summary>
        protected abstract void GenerateReport();

        /// <summary>
        /// Добавляет информацию о пользовательских фильтрах.
        /// Используется только если отчет формируется по всем данным, а не по выборочным заявкам.
        /// </summary>
        protected virtual void GenerateFilterInfo()
        {
            if ((AppFilter.AppsNumbers == null || !AppFilter.AppsNumbers.Any()) && (AppFilter.HasGeneralFilter() || AppFilter.IsManual))
                GenerateGeneralFilterInfo();
        }

        /// <summary>
        /// Выводит информацию об общем фильтре отчета.
        /// Используется только если отчет формируется по всем данным, а не по выборочным заявкам
        /// </summary>
        private void GenerateGeneralFilterInfo()
        {
            int column = StartColumnNumber;
            int skip = 3;

            ExcelPage.Cells[RowNumber, column].Value = "Выбранные фильтры:";
            ExcelPage.Cells[RowNumber, column].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, column].Style.Font.Italic = true;
            ExcelPage.Cells[RowNumber, column, RowNumber, column + skip - 1].Merge = true;
            RowNumber++;

            if (AppFilter.Id > 0)
            {
                AddGeneralFilterInfo("№ заявки:", AppFilter.Id, INT_FORMAT);
                RowNumber++;
            }

            if (!string.IsNullOrEmpty(AppFilter.WayBill))
            {
                AddGeneralFilterInfo("№ накладной:", AppFilter.WayBill);
                RowNumber++;
            }

            if (!string.IsNullOrEmpty(AppFilter.ClientName))
            {
                AddGeneralFilterInfo("Заказчик:", AppFilter.ClientName);
                RowNumber++;
            }

            if (!string.IsNullOrEmpty(AppFilter.CarName))
            {
                AddGeneralFilterInfo("Номер машины:", AppFilter.CarName);
                RowNumber++;
            }

            if (!string.IsNullOrEmpty(AppFilter.RecipeName))
            {
                AddGeneralFilterInfo("Рецепт:", AppFilter.RecipeName);
                RowNumber++;
            }

            if (AppFilter.Volume > 0)
            {
                AddGeneralFilterInfo("Объем:", AppFilter.Volume, FLOAT_FORMAT);
                RowNumber++;
            }

            if (AppFilter.MixerNumber > 0)
            {
                AddGeneralFilterInfo("Смеситель:", AppFilter.MixerNumber, INT_FORMAT);
                RowNumber++;
            }

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

            AddGeneralFilterInfo("Включить ручные заявки:", AppFilter.IsManual ? "Да" : "Нет");
            RowNumber++;
        }

        /// <summary>
        /// Была идея при формировании отчета по отмеченным заявкам, вместо фильтра печатать номера этих заявок.
        /// Проблема с выводом данного списка в Excel. Если заявок много, они не умещаются в одну строку. 
        /// Если включить перенос, то нужно увеличивать ширину строки. 
        /// Надо как-то определять был перенос или нет и увеличивать ширину строки. 
        /// </summary>
        protected void GenerateReportInfoOnlyNumbersApps()
        {
            int leftColl = 2;
            int skip = 3;

            ExcelPage.Cells[RowNumber, leftColl].Value = "Выбранные заявки:";
            ExcelPage.Cells[RowNumber, leftColl].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, leftColl].Style.Font.Italic = true;
            ExcelPage.Cells[RowNumber, leftColl, RowNumber, leftColl + skip - 1].Merge = true;
            RowNumber++;

            var sb = new StringBuilder();
            foreach (var appNumber in AppFilter.AppsNumbers)
            {
                sb.Append(appNumber);
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2); // удалить последнюю запятую и пробел

            ExcelPage.Cells[RowNumber, leftColl].Value = sb.ToString();
            //ExcelPage.Cells[CurrentRowNumber, leftColl, CurrentRowNumber, WidthInCells].Merge = true;
            //ExcelPage.Cells[CurrentRowNumber, leftColl].Style.WrapText = true;
        }

        /// <summary>
        /// Выполняет обработку названия из справочника: убирает служебные слова.
        /// </summary>
        /// <param name="name">Входная строка.</param>
        /// <param name="manual">Признак обработки служебного слова для ручных замесов ("Ручной")</param>
        /// <returns>Название без служебных слов.</returns>
        protected static string CheckName(string name, bool manual = true)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            if (string.Compare(name, BSU.Utils.Resources.LocalizationAddResource.UndefinedListItemName, true) == 0 || string.Compare(name.Trim(), "-") == 0 || (manual && string.Compare(name.Trim(), "Ручной") == 0)) // для рецептов нужно оставить "Ручной [замес]"
                return string.Empty;

            return name.Trim();
        }

        /// <summary>
        /// Приводит дату к формату <paramref name="format"/>, если указанная дата <paramref name="date"/> содержит значение, отличное от <see cref="DateTime.MinValue"/> и <see cref="DateTime.MaxValue"/>,
        /// иначе - пустая строка.
        /// </summary>
        /// <param name="date">Входная дата.</param>
        /// <param name="format">Формат даты.</param>
        /// <returns>Обработанное значение даты.</returns>
        protected static string CheckDate(DateTime date, string format)
        {
            if (date == DateTime.MinValue || date == DateTime.MaxValue)
                return string.Empty;

            return date.ToString(format);
        }

        /// <summary>
        /// Возвращает дату, если указанная дата <paramref name="date"/> содержит значение, отличное от <see cref="DateTime.MinValue"/> и <see cref="DateTime.MaxValue"/>,
        /// иначе - значение <see langword="null"/>.
        /// </summary>
        /// <param name="date">Входная дата.</param>
        /// <returns>Обработанное значение даты.</returns>
        protected static DateTime? CheckDate(DateTime date)
        {
            if (date == DateTime.MinValue || date == DateTime.MaxValue)
                return null;

            return date;
        }

        /// <summary>
        /// Выполняет обработку ID.
        /// </summary>
        /// <param name="id">Входное значение.</param>
        /// <returns>Значение ID, если значение больше 0, иначе - пустая строка.</returns>
        protected static string CheckId(int id)
        {
            if (id < 1) return string.Empty;
            return id.ToString();
        }

        /// <summary>
        /// Формирует верхний колонтитул для всех видов отчетов.
        /// </summary>
        private void GenerateReportHeader()
        {
            // на каждой странице
            ExcelPage.PrinterSettings.RepeatRows = ExcelPage.Cells["1:2"];

            // ..выводим заголовок компании слева
            ExcelPage.Cells[RowNumber, StartColumnNumber].Value = FirmName;
            ExcelPage.Cells[RowNumber, StartColumnNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells - 3].Merge = true;

            // ..выводим дату формирования отчета справа
            ExcelPage.Cells[RowNumber, WidthInCells - 2].Value = DateTime.Now.ToString(DATE_TIME_FORMAT);
            ExcelPage.Cells[RowNumber, WidthInCells - 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ExcelPage.Cells[RowNumber, WidthInCells - 2].Style.Numberformat.Format = DATE_TIME_FORMAT;
            ExcelPage.Cells[RowNumber, WidthInCells - 2, RowNumber, WidthInCells].Merge = true;
            RowNumber++;

            // название отчета/страницы в центре
            ExcelPage.Cells[RowNumber, StartColumnNumber].Value = ReportName;
            ExcelPage.Cells[RowNumber, StartColumnNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ExcelPage.Cells[RowNumber, StartColumnNumber].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].Merge = true;

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
        }

        /// <summary>
        /// Выводить данные о фильтре отчета в шапку отчета.
        /// </summary>
        /// <param name="name">Название параметра.</param>
        /// <param name="value">Значение параметра.</param>
        /// <param name="numberFormat">Формат ячейки.</param>
        protected void AddGeneralFilterInfo(string name, object value, string numberFormat = "")
        {
            int column = StartColumnNumber;
            int skip = 3;

            ExcelPage.Cells[RowNumber, column].Value = name;
            ExcelPage.Cells[RowNumber, column].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, column, RowNumber, column + skip - 1].Merge = true;

            ExcelPage.Cells[RowNumber, column + skip].Value = value;
            ExcelPage.Cells[RowNumber, column + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ExcelPage.Cells[RowNumber, column + skip].Style.Font.Italic = true;
            if (!numberFormat.Equals(string.Empty))
                ExcelPage.Cells[RowNumber, column + skip].Style.Numberformat.Format = numberFormat;
            ExcelPage.Cells[RowNumber, column + skip, RowNumber, column + skip + 1].Merge = true;
        }

        protected virtual void FitAll(int minWidth = 16)
        {
            for (int i = 1; i <= WidthInCells; i++)
                ExcelPage.Column(i).AutoFit(minWidth);
        }

        #region Функции для облегчения использования

        public Point CurrentCoordinates;
        public ExcelRange CurrentCell => ExcelPage.Cells[CurrentCoordinates.Y, CurrentCoordinates.X];
        internal void WriteLine(ExcelWorksheet worksheet, object line, int fontSize = 9, bool isBold = false, int x = -1, int y = -1, ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left, bool parseNumbers = false, ValueType valueType = ValueType.Text, bool fillCellWhiteColor = true, bool mergeCells = false, int mergeNumber = 4, bool bordered = false, bool rotateText = false, int textRotation = 0)
        {
            WriteCell(worksheet, line, fontSize, isBold, x: x, y: y, align, parseNumbers, valueType, fillCellWhiteColor, mergeCell: mergeCells, mergeNumber: mergeNumber, bordered, textRotation: textRotation, rotateText: rotateText);

            CurrentCoordinates.Y += 1;
        }

        internal void WriteLine(ExcelWorksheet worksheet, object type, object data = null, int fontSize = 9, int x = -1, int y = -1, ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left, bool parseNumbers = false, ValueType valueType = ValueType.Text, bool fillCellWhiteColor = true)
        {
            WriteCell(worksheet, type, fontSize, true, x: x, y: y, align, parseNumbers, valueType, fillCellWhiteColor); CurrentCoordinates.X += 1;
            WriteCell(worksheet, data, fontSize, false, x: x, y: y, align, parseNumbers, valueType, fillCellWhiteColor); CurrentCoordinates.X -= 1;

            CurrentCoordinates.Y += 1;
        }
        /// <summary>
        /// Заполнить ячейку Integer значением
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fontSize"></param>
        /// <param name="isBold"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="align"></param>
        /// <param name="parseNumbers"></param>
        public void WriteIntegerCell(ExcelWorksheet worksheet, int value, int fontSize = 9, bool isBold = false, int x = -1, int y = -1, ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left, bool parseNumbers = false)
        {
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Value = value;
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Style.HorizontalAlignment = align;
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Style.Font.Size = fontSize;
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Style.Font.Bold = isBold;
        }
        /// <summary>
        /// Заполнить ячейку Double значением
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fontSize"></param>
        /// <param name="isBold"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="align"></param>
        /// <param name="parseNumbers"></param>
        public void WriteDoubleCell(ExcelWorksheet worksheet, double value, int fontSize = 9, bool isBold = false, int x = -1, int y = -1, ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left, bool parseNumbers = false)
        {
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Style.Numberformat.Format = "0.00";
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Value = value;
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Style.HorizontalAlignment = align;
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Style.Font.Size = fontSize;
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Style.Font.Bold = isBold;
        }
        /// <summary>
        /// Заполнить ячейку текстовым значением
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="isBold"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="align"></param>
        /// <param name="parseNumbers"></param>
        public void WriteTextCell(ExcelWorksheet worksheet, string text, int fontSize = 9, bool isBold = false, int x = -1, int y = -1, ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left, bool parseNumbers = false)
        {
            if (text.Contains("#empty"))
                text = "";


            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Value = text;
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Style.HorizontalAlignment = align;
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Style.Font.Size = fontSize;
            worksheet.Cells[y == -1 ? CurrentCoordinates.Y : y, x == -1 ? CurrentCoordinates.X : x].Style.Font.Bold = isBold;
        }
        /// <summary>
        /// Заполнить ячейку значением
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fontSize"></param>
        /// <param name="isBold"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="align"></param>
        /// <param name="parseNumbers"></param>
        internal void WriteCell(ExcelWorksheet worksheet, object value, int fontSize = 9, bool isBold = false, int x = -1, int y = -1, ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left, bool parseNumbers = false, ValueType valueType = ValueType.Text, bool fillCellWhiteColor = true, bool mergeCell = false, int mergeNumber = 4, bool bordered = false, bool rotateText = false, int textRotation = 0)
        {
            switch (valueType)
            {
                case ValueType.Text: WriteTextCell(worksheet, value.ToString(), fontSize, isBold, x, y, align, parseNumbers); break;
                case ValueType.Integer: WriteIntegerCell(worksheet, Convert.ToInt32(value), fontSize, isBold, x, y, align, parseNumbers); break;
                case ValueType.Double: WriteDoubleCell(worksheet, Convert.ToDouble(value), fontSize, isBold, x, y, align, parseNumbers); break;
            }

            if (mergeCell)
            {
                try
                {
                    ExcelPage.Cells[y == -1 ? CurrentCoordinates.Y : y,
                                    x == -1 ? CurrentCoordinates.X : x,
                                    y == -1 ? CurrentCoordinates.Y + mergeNumber - 1 : y + 2,
                                    x == -1 ? CurrentCoordinates.X : x].Merge = true;
                }
                catch
                {
                }

                ExcelPage.Cells[y == -1 ? CurrentCoordinates.Y : y,
                                x == -1 ? CurrentCoordinates.X : x,
                                y == -1 ? CurrentCoordinates.Y + mergeNumber - 1 : y + 2,
                                x == -1 ? CurrentCoordinates.X : x].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ExcelPage.Cells[y == -1 ? CurrentCoordinates.Y : y,
                               x == -1 ? CurrentCoordinates.X : x,
                               y == -1 ? CurrentCoordinates.Y + mergeNumber - 1 : y + 2,
                               x == -1 ? CurrentCoordinates.X : x].Style.WrapText = true;

                CurrentCoordinates.Y += mergeNumber - 1;
            }

            if (bordered)
            {
                ExcelPage.Cells[y == -1 ? CurrentCoordinates.Y - (mergeNumber - 1) : y,
                               x == -1 ? CurrentCoordinates.X : x,
                               y == -1 ? CurrentCoordinates.Y : y + 2,
                               x == -1 ? CurrentCoordinates.X : x].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            if (rotateText)
            {
                if (!mergeCell)
                {
                    ExcelPage.Cells[y == -1 ? CurrentCoordinates.Y : y,
                                        x == -1 ? CurrentCoordinates.X : x,
                                        y == -1 ? CurrentCoordinates.Y + mergeNumber - 1 : y + 2,
                                        x == -1 ? CurrentCoordinates.X : x].Style.TextRotation = textRotation;
                }
                else
                {
                    CurrentCoordinates.Y -= mergeNumber - 1;
                    ExcelPage.Cells[y == -1 ? CurrentCoordinates.Y : y,
                                       x == -1 ? CurrentCoordinates.X : x,
                                       y == -1 ? CurrentCoordinates.Y + mergeNumber - 1 : y + 2,
                                       x == -1 ? CurrentCoordinates.X : x].Style.TextRotation = textRotation;
                    CurrentCoordinates.Y += mergeNumber - 1;
                }
            }

            if (fillCellWhiteColor)
                FillCellWhiteColor(worksheet);
        }
        public void FillCellWhiteColor(ExcelWorksheet worksheet, int row1 = -1, int column1 = -1, int row2 = -1, int column2 = -1)
        {
            worksheet.Cells[row1 == -1 ? CurrentCoordinates.Y : row1, column1 == -1 ? CurrentCoordinates.X : column1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.LightGrid;
            worksheet.Cells[row1 == -1 ? CurrentCoordinates.Y : row1, column1 == -1 ? CurrentCoordinates.X : column1].Style.Fill.BackgroundColor.SetColor(Color.White);
        }

        public void FillCellsWhiteColor(ExcelWorksheet worksheet, int row1 = -1, int column1 = -1, int row2 = -1, int column2 = -1)
        {
            worksheet.Cells[row1 == -1 ? CurrentCoordinates.Y : row1, column1 == -1 ? CurrentCoordinates.X : column1,
                row2 == -1 ? CurrentCoordinates.Y : row2, column2 == -1 ? CurrentCoordinates.X : column2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.LightGrid;
            worksheet.Cells[row1 == -1 ? CurrentCoordinates.Y : row1, column1 == -1 ? CurrentCoordinates.X : column1,
                row2 == -1 ? CurrentCoordinates.Y : row2, column2 == -1 ? CurrentCoordinates.X : column2].Style.Fill.BackgroundColor.SetColor(Color.White);
        }
        public void MakeRangeMerged(ExcelWorksheet worksheet, int row1, int column1, int row2, int column2)
        {
            worksheet.Cells[row1, column1, row2, column2].Merge = true;
        }
        public void CreateDottedBorder(ExcelWorksheet worksheet, int row1, int column1, int row2, int column2)
        {
            worksheet.Cells[row1, column1, row2, column2].Style.Border.BorderAround(ExcelBorderStyle.Dotted);
        }
        /// <summary>
        /// Вставляет колонку с данными в текущие координаты, каждая строчка будет написана с новой строки.
        /// Автоматически добавляет +1 к значению Y в <see cref="CurrentCoordinates"/>.         
        /// Пример:
        /// <para>Время : 5:24</para>
        /// <para>Накл : 51234</para>
        /// </summary>
        /// <param name="texts"></param>
        /// <param name="fontSize"></param>
        /// <param name="isBold"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal void AppendColumn(ExcelWorksheet worksheet,
            IEnumerable<(object, object)> texts,
            int fontSize = 9,
            bool isBold = false,
            int x = -1, int y = -1,
            ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left,
            bool parseNumbers = false,
            ValueType valueType = ValueType.Text)
        {
            foreach (var text in texts)
            {
                WriteLine(worksheet, type: text.Item1, data: text.Item2, fontSize: fontSize, x, y, align, parseNumbers, valueType);
            }
        }
        /// <summary>
        /// Вставляет колонку с данными в текущие координаты, каждая строчка будет написана с новой строки.
        /// Автоматически добавляет +1 к значению Y в <see cref="CurrentCoordinates"/>.         
        /// Пример:
        /// <para>Каждый</para>
        /// <para>элемент</para>
        /// <para>массива</para>
        /// <para>с</para>
        /// <para>новой</para>
        /// <para>строки</para>
        /// </summary>
        /// <param name="texts">Массив в виде { "Каждый", "элемент", "массива", "с", "новой", "строки" }</param>
        /// <param name="fontSize">Размер шрифта</param>
        /// <param name="isBold">Установки жирности шрифта</param>
        /// <param name="x">X координата, если хотите вставить в определенное место, а не в текущее</param>
        /// <param name="y">Y координата, если хотите вставить в определенное место, а не в текущее</param>
        internal void AppendColumn(ExcelWorksheet worksheet,
            IEnumerable<object> texts,
            int fontSize = 0,
            bool isBold = false,
            int x = -1, int y = -1,
            ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left,
            bool parseNumbers = false, ValueType valueType = ValueType.Text)
        {
            foreach (var text in texts)
            {
                WriteLine(worksheet, line: text, fontSize, isBold, x, y, align, parseNumbers, valueType);
            }
        }

        /// <summary>
        /// Вставка строки. Вставляет строки из массива в ячейки. 
        /// <list type="table">A__________B_______C_______D___E_______F</list>
        /// <list type="table">Каждый элемент массива в новой ячейке.</list>
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="fontSize"></param>
        /// <param name="isBold"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="align"></param>
        /// <param name="parseNumbers"></param>
        /// <param name="valueType"></param>
        internal void AppendRow(
            ExcelWorksheet worksheet,
            IEnumerable<(object, ExcelHorizontalAlignment)> cells,
            int fontSize = 9,
            bool isBold = false,
            int x = -1, int y = -1,
            ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left,
            bool parseNumbers = false, ValueType valueType = ValueType.Text)
        {
            foreach (var text in cells)
            {
                WriteCell(worksheet, text.Item1, fontSize, isBold, x, y, text.Item2, parseNumbers, valueType);
                FillCellWhiteColor(worksheet);

                CurrentCoordinates.X += 1;
            }
            CurrentCoordinates.Y += 1;
        }
        public void AppendColumns(ExcelWorksheet worksheet, IEnumerable<(object, object)[]> texts, int fontSize = 9, bool isBold = false, int x = -1, int y = -1, ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left, bool parseNumbers = false)
        {
            var currentRow = CurrentCoordinates.Y;

            foreach (var text in texts)
            {
                AppendColumn(worksheet, text, fontSize, isBold, x, y, align, parseNumbers);
                CurrentCoordinates.X += 2;
                CurrentCoordinates.Y = currentRow;
            }
        }
        #endregion

        #region Функция для изменении стрима. Метод необходим если используется определенный шаблон отчета

        protected void ChangeStream(Stream stream)
        {
            try
            {
                if (ExcelFile != null)
                {
                    ExcelFile.Dispose();
                    ExcelFile = new ExcelPackage(stream);
                    ExcelPage = ExcelFile.Workbook.Worksheets.Add(ReportName);
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteError("Возникла ошибка при формировании отчета", ex);
            }
        }

        #endregion
    }
}