namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data;
    using BSU.API.Data.Reporting;

    using OfficeOpenXml.Style;

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TTS.SmartMix.Export.DataExport.ExcelExport.Extensions;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.Summary"/>
    /// </summary>
    public class ApplicationSummaryExcelReport : ExcelReportBase
    {
        /// <summary>
        /// Представляет данные по заявкам.
        /// </summary>
        private readonly ApplicationReport[] _reportData;

        /// <summary>
        /// Представляет сводные данные по клиентам, где ключом является название рецепта, а значением - словарь с названием клиента и суммарным выполненным объемом, в куб.м.
        /// </summary>
        private Dictionary<string, Dictionary<string, double>> _summaryClientData = new Dictionary<string, Dictionary<string, double>>();

        /// <summary>
        /// Представляет сводные данные по материалам, где ключом является название рецепта, а значением - словарь с названием материала и суммарным фактическим весом по всем замесам, в кг.
        /// </summary>
        private Dictionary<string, Dictionary<string, double>> _summaryMaterialData = new Dictionary<string, Dictionary<string, double>>();


        public ApplicationSummaryExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter)
        {
            _reportData = report as ApplicationReport[];
            BuildSummaryData();

            Description = "Сводный отчет по заявкам";
            ReportName = "Сводный отчет";

            DictionaryColumnWidth = new Dictionary<int, double>()
            {
                {1, 9}, // Левая граница
                {2, 34}, // Клиент/Материал
                {3, 10}, // .. (динамические столбцы)
                {4, 10}, //
                {5, 10}, // 
                {6, 10}, //
                {7, 10}, //
                {8, 10}, //
                {9, 10}, //
                {10, 10}, //
                {11, 10}, //
                {12, 10}, //
                {13, 10} //
            };
            WidthInCells = DictionaryColumnWidth.Count;

            IsLandscape = true;
            UseFooter = true;
        }

        protected override void GenerateReport()
        {
            RowNumber++;

            ExcelPage.Cells[RowNumber, StartColumnNumber].Value = "Выполненный объём, куб.м";
            ExcelPage.Cells[RowNumber, StartColumnNumber].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, StartColumnNumber + 4].Merge = true;
            RowNumber++;

            GenerateTable(_summaryClientData, "Клиент", "Итого объём, куб.м");
            RowNumber += 3;

            ExcelPage.Cells[RowNumber, StartColumnNumber].Value = "Фактический вес, кг";
            ExcelPage.Cells[RowNumber, StartColumnNumber].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, StartColumnNumber + 4].Merge = true;
            RowNumber++;

            GenerateTable(_summaryMaterialData, "Материал", "Итого факт. вес, кг");
        }

        /// <summary>
        /// Выполняет генерацию сводной таблицы по указанным данным.
        /// </summary>
        /// <param name="dictSummary">Словарь данных.</param>
        /// <param name="titleHeader">Начальный заголовок таблицы.</param>
        /// <param name="summaryHeader">Заголовок таблицы с итогами.</param>
        private void GenerateTable(Dictionary<string, Dictionary<string, double>> dictSummary, string titleHeader, string summaryHeader)
        {
            List<string> keyList = new List<string>(dictSummary.Keys);
            keyList.Sort();  // сортируем рецепту по алфавиту

            // собираем уникальный список вложенных ключей (по значению: название материала/название клиента)
            List<string> valueKeyList = new List<string>();
            foreach (string key in keyList)
                foreach (string valueKey in dictSummary[key].Keys)
                    if (!valueKeyList.Contains(valueKey)) valueKeyList.Add(valueKey);

            double[,] sumMatrix = new double[keyList.Count, valueKeyList.Count];

            for (int i = 0; i < keyList.Count; i++)
                foreach (string valueKey in dictSummary[keyList[i]].Keys)
                {
                    int j = valueKeyList.IndexOf(valueKey);
                    sumMatrix[i, j] = dictSummary[keyList[i]][valueKey];
                }

            int lastCol = GenerateTableTopHeader(keyList.ToArray(), titleHeader, summaryHeader);
            RowNumber++;

            int lastRow = GenerateTableLeftHeader(valueKeyList.ToArray());

            GenerateSummary(lastCol, lastRow, valueKeyList.Count, keyList.Count, RowNumber);
            GenerateData(sumMatrix, valueKeyList.Count, keyList.Count);

            for (int i = 1; i <= keyList.Count; i++)
                ExcelPage.Column(i).AutoFit();
        }

        /// <summary>
        /// Выполняет генерацию верхнего заголовка таблицы. Возвращает номер последнего столбца таблицы.
        /// </summary>
        /// <param name="titleHeader">Начальный заголовок для таблицы.</param>
        /// <param name="headers">Массив заголовков.</param>
        /// <param name="summaryHeader">Заголовок таблицы с итогами.</param>
        /// <returns>Номер последнего столбца таблицы.</returns>
        private int GenerateTableTopHeader(string[] headers, string titleHeader, string summaryHeader)
        {
            // первая колонка таблицы
            ExcelPage.Cells[RowNumber, StartColumnNumber].ApplyHeaderStyle().Value = titleHeader;
            ExcelPage.Cells[RowNumber, StartColumnNumber].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;

            // динамические столбцы
            int startColumn = StartColumnNumber + 1;

            int iColumn = startColumn;
            foreach (string header in headers)
                ExcelPage.Cells[RowNumber, iColumn++].ApplyHeaderRotateStyle().Value = header;

            // последняя колонка таблицы
            ExcelPage.Cells[RowNumber, iColumn].ApplyHeaderRotateStyle().Value = summaryHeader;

            // границы
            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, iColumn].ApplyBorders(ExcelBorderStyle.Thin);

            return iColumn;
        }

        /// <summary>
        /// Выполняет генерацию заголовков таблицы слева. Возвращает номер последней строки таблицы.
        /// </summary>
        /// <param name="headers">Массив заголовков.</param>
        /// <returns>Номер последней строки таблицы.</returns>
        private int GenerateTableLeftHeader(string[] headers)
        {
            int irow = RowNumber;
            foreach (string header in headers)
                ExcelPage.Cells[irow++, StartColumnNumber].ApplyLongHeaderStyle(ExcelHorizontalAlignment.Right).Value = header;

            ExcelPage.Cells[RowNumber, StartColumnNumber, irow, StartColumnNumber].AutoFitColumns();
            ExcelPage.Cells[RowNumber, StartColumnNumber, irow, StartColumnNumber].ApplyBorders(ExcelBorderStyle.Thin);

            return irow;
        }

        /// <summary>
        /// Выводит информацию по значениям.
        /// </summary>
        /// <param name="volumes">Матрица значений.</param>
        /// <param name="rowCount">Количество динамических строк.</param>
        /// <param name="columnCount">Количество динамических колонок.</param>
        private void GenerateData(double[,] volumes, int rowCount, int columnCount)
        {
            int startRow = RowNumber;
            int column = StartColumnNumber + 1;

            for (int j = 0; j < rowCount; j++)
            {
                for (int i = 0; i < columnCount; i++)
                {
                    ExcelPage.Cells[RowNumber, column + i].Value = volumes[i, j];
                    ExcelPage.Cells[RowNumber, column + i].Style.Numberformat.Format = FLOAT_FORMAT;
                }
                RowNumber++;
            }
            ExcelPage.Cells[startRow, column, RowNumber, column + columnCount].ApplyBorders(ExcelBorderStyle.Thin);
        }

        /// <summary>
        /// Выводит итоговые значения по указанным параметрам.
        /// </summary>
        /// <param name="lastCol">Номер последней колонки.</param>
        /// <param name="lastRow">Номер последней строки.</param>
        /// <param name="rowCount">Количество динамических строк.</param>
        /// <param name="columnCount">Количество динамических колонок.</param>
        /// <param name="startRow">Начальный номер строки.</param>
        private void GenerateSummary(int lastCol, int lastRow, int rowCount, int columnCount, int startRow)
        {
            // итого по горизонтали
            ExcelPage.Cells[lastRow, StartColumnNumber].ApplyHeaderStyle(ExcelHorizontalAlignment.Right).Value = "Итого";
            ExcelPage.Cells[lastRow, StartColumnNumber].ApplyBackground(System.Drawing.Color.Gainsboro);

            int iColumn = StartColumnNumber + 1;
            for (int i = 0; i <= columnCount; i++)
            {
                ExcelPage.Cells[lastRow, iColumn].Formula = "SUM(" + ExcelPage.Cells[startRow, iColumn].Address + ":" + ExcelPage.Cells[lastRow - 1, iColumn].Address + ")";
                iColumn++;
            }
            //ExcelPage.Cells[lastRow, 3, lastRow, iColumn - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ExcelPage.Cells[lastRow, StartColumnNumber + 1, lastRow, iColumn - 1].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[lastRow, StartColumnNumber + 1, lastRow, iColumn - 1].Style.Font.Bold = true;
            ExcelPage.Cells[lastRow, StartColumnNumber + 1, lastRow, iColumn - 1].ApplyBackground(System.Drawing.Color.Gainsboro);
            ExcelPage.Cells[lastRow, StartColumnNumber + 1, lastRow, iColumn - 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            // итого по вертикали
            for (int i = 0; i < rowCount; i++)
            {
                ExcelPage.Cells[startRow, lastCol].Formula = "SUM(" + ExcelPage.Cells[startRow, 3].Address + ":" + ExcelPage.Cells[startRow, lastCol - 1].Address + ")";
                startRow++;
            }
            //ExcelPage.Cells[RowNumber, lastCol, startRow, lastCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ExcelPage.Cells[RowNumber, lastCol, startRow, lastCol].Style.Numberformat.Format = FLOAT_FORMAT;
            ExcelPage.Cells[RowNumber, lastCol, startRow, lastCol].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, lastCol, startRow, lastCol].ApplyBackground(System.Drawing.Color.Gainsboro);
            ExcelPage.Cells[RowNumber, lastCol, startRow, lastCol].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            for (int i = RowNumber; i <= lastRow; i++)
            {
                ExcelPage.Cells[i, lastCol].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
        }

        /// <summary>
        /// Формирует итоговые данные.
        /// </summary>
        private void BuildSummaryData()
        {
            _summaryClientData = new Dictionary<string, Dictionary<string, double>>();

            _summaryMaterialData = new Dictionary<string, Dictionary<string, double>>();

            Task[] tasks = new Task[2];
            tasks[0] = Task.Factory.StartNew(BuildMaterialData);
            tasks[1] = Task.Factory.StartNew(BuildClientData);

            Task.WaitAll(tasks);
        }

        /// <summary>
        /// Формирует итоговые данные по материалам замеса по рецептам.
        /// </summary>
        private void BuildMaterialData()
        {
            _summaryMaterialData.Clear();
            foreach (ApplicationReport app in _reportData)
            {
                foreach (LayerApplication layer in app.Layers)
                {
                    Dictionary<string, double> dict = new Dictionary<string, double>();
                    if (_summaryMaterialData.ContainsKey(layer.Recipe.Name))
                        dict = _summaryMaterialData[layer.Recipe.Name];
                    else
                        _summaryMaterialData.Add(layer.Recipe.Name, dict);

                    foreach (BatchReport batch in app.Batches.Where(b => b.LayerNumber == layer.Id))
                    {
                        foreach (BatchReportMaterial batchMaterial in batch.BatchMaterials)
                        {
                            if (!dict.ContainsKey(batchMaterial.Material))
                                dict.Add(batchMaterial.Material, 0);
                            dict[batchMaterial.Material] += batchMaterial.Real;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Формирует итоговые данные по выполненному объему в разрезе клиентов и рецептов.
        /// </summary>
        private void BuildClientData()
        {
            _summaryClientData.Clear();
            foreach (ApplicationReport app in _reportData)
            {
                if (app.Volume > 0)
                {
                    foreach (LayerApplication layer in app.Layers)
                    {
                        Dictionary<string, double> dict = new Dictionary<string, double>();
                        if (_summaryClientData.ContainsKey(layer.Recipe.Name))
                            dict = _summaryClientData[layer.Recipe.Name];
                        else
                            _summaryClientData.Add(layer.Recipe.Name, dict);

                        if (dict.ContainsKey(app.Client.Name))
                            dict[app.Client.Name] += app.CompletedVolume;
                        else
                        {
                            dict.Add(app.Client.Name, app.CompletedVolume);
                        }
                    }
                }
            }
        }
    }
}