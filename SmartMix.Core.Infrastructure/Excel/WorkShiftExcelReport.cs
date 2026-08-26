namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using global::SmartMix.Core.Domain.Entities;
    using OfficeOpenXml.Style;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.WorkShift"/>.
    /// </summary>
    public class WorkShiftExcelReport : ExcelReportBase
    {
        /// <summary>
        /// Представляет набор данных по заявкам.
        /// </summary>
        private ApplicationReport[] _reportData;

        private int _lowerY;
        private int _highterX;

        private Dictionary<ApplicationReport, int> _applicationRows;

        private Dictionary<int, double> _resultByRecipe;
        private Dictionary<int, double> _resultByFact;

        private Dictionary<string, int> _componentsRows;

        /// <summary>
        /// Инициализирует новый экземпляр класса по указанным параметрам.
        /// </summary>
        /// <param name="lineNumber">Номер линии.</param>
        /// <param name="appFilter">Фильтр.</param>
        /// <param name="firmName">Название компании.</param>
        /// <param name="report">Набор данных типа <see cref="ApplicationReport"/>.</param>
        public WorkShiftExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter)
        {
            _reportData = report as ApplicationReport[];

            Description = $"Отчет за смену по материалам\r\nза период с {appFilter.StartDate.ToString("dd.MM.yyyy HH:mm:ss")} по {appFilter.EndDate.ToString("dd.MM.yyyy HH:mm:ss")}";
            ReportName = $"Отчет за смену по материалам\r\nза период с {appFilter.StartDate.ToString("dd.MM.yyyy HH:mm:ss")} по {appFilter.EndDate.ToString("dd.MM.yyyy HH:mm:ss")}";

            DictionaryColumnWidth = new Dictionary<int, double>()
            {
                {2, 8}, // Номер заявки
                {3, 12}, // Дата и время выполнения, линия
                {4, 34}, // Код, группа, рецепт
                {5, 10}, // Объем заказанный
                {6, 10}, // Объём выполненный
                {7, 9}, // Тип веса (по рец./факт)
                {8, 9}, // (динамический список компонент)
                {9, 9}, // ...
                {10, 9} // ...
            };
            WidthInCells = DictionaryColumnWidth.Count;
        }

        protected override void GenerateReport()
        {
            if (_reportData.Length == 0) return;

            try
            {
                GenerateHeaderRow();
                GenerateApplicationNumbersColumn();
                GenerateApplicationTimesColumn();
                GenerateApplicationRecipeColumn();
                GenerateApplicationVolumeColumn();
                GenerateApplicatioDoneVolumeColumn();
                GenerateComponentsTypesColumn();
                GenerateComponentsColumns();
                GenerateResultsRow();
                GenerateBackground();
                GenerateBorders();
            }
            catch (Exception ex)
            {
                Logger.LogManager.Instance.WriteException(ex);
            }
        }

        private void GenerateHeaderRow()
        {
            MakeRangeMerged(ExcelPage, 1, 1, 2, 10);

            CurrentCoordinates = new System.Drawing.Point(1, 1);

            WriteLine(worksheet: ExcelPage, line: ReportName, isBold: true, fontSize: 12, align: ExcelHorizontalAlignment.Center);

            ExcelPage.Cells[1, 1, 2, 10].Style.WrapText = true;
        }

        private void GenerateApplicationNumbersColumn()
        {
            CurrentCoordinates = new System.Drawing.Point(2, 8);

            WriteLine(worksheet: ExcelPage,
                line: "№\r\nЗаявки",
                 isBold: true,
                fillCellWhiteColor: false,
                bordered: true,
                align: ExcelHorizontalAlignment.Center,
                mergeCells: true,
                mergeNumber: 4);

            foreach (ApplicationReport application in _reportData)
            {
                WriteLine(worksheet: ExcelPage,
                    line: application.Id,
                    fillCellWhiteColor: false,
                    align: ExcelHorizontalAlignment.Center,
                    mergeCells: true,
                    bordered: true,
                    mergeNumber: 4);
            }
        }

        private void GenerateApplicationTimesColumn()
        {
            CurrentCoordinates = new System.Drawing.Point(3, 8);

            WriteLine(worksheet: ExcelPage,
                line: "Дата и время\r\nвыполнения\r\nзаявки,\r\nлиния",
                 isBold: true,
                fillCellWhiteColor: false,
                align: ExcelHorizontalAlignment.Center,
                mergeCells: true,
                bordered: true,
                mergeNumber: 4);

            foreach (ApplicationReport application in _reportData)
            {
                var line = string.Format("{0}{3}{1},{3}{2}",
                    application?.EndTime.ToString("dd.MM.yyyy"),
                    application?.EndTime.ToString("HH:mm:ss"),
                    string.Format("{0}{1}", "Линия ", LineNumber), Environment.NewLine);

                WriteLine(worksheet: ExcelPage,
                    line: line,
                    fillCellWhiteColor: false,
                    bordered: true,
                    align: ExcelHorizontalAlignment.Center,
                    mergeCells: true,
                    mergeNumber: 4);
            }
        }

        private void GenerateApplicationRecipeColumn()
        {
            CurrentCoordinates = new System.Drawing.Point(4, 8);

            WriteLine(worksheet: ExcelPage,
                line: "Код, Группа, Рецепт",
                 isBold: true,
                fillCellWhiteColor: false,
                bordered: true,
                align: ExcelHorizontalAlignment.Center,
                mergeCells: true,
                mergeNumber: 4);

            foreach (ApplicationReport application in _reportData)
            {
                string line = string.Empty;
                if (application != null && application.Layers.Count > 0 && application.Layers[0].Recipe != null)
                    line = string.Format("{0}, {1}, {2}", CheckId(application.Layers[0].Recipe.Id), CheckName(application.Layers[0].Recipe.RecipeCategory?.Name), application.Layers[0].Recipe.Name);

                WriteLine(worksheet: ExcelPage,
                    line: line,
                    fillCellWhiteColor: false,
                    bordered: true,
                    align: ExcelHorizontalAlignment.Center,
                    mergeCells: true,
                    mergeNumber: 4);
            }
        }

        private void GenerateApplicationVolumeColumn()
        {
            CurrentCoordinates = new System.Drawing.Point(5, 8);
            _applicationRows = new Dictionary<ApplicationReport, int>();

            WriteLine(worksheet: ExcelPage,
                line: "Объём заказанный, м³",
                isBold: true,
               fillCellWhiteColor: false,
               bordered: true,
               align: ExcelHorizontalAlignment.Center,
               mergeCells: true,
               mergeNumber: 4);

            foreach (ApplicationReport application in _reportData)
            {
                _applicationRows.Add(application, CurrentCoordinates.Y);

                string line = string.Format("{0:0.00}", application?.Layers?.FirstOrDefault()?.Volume ?? 0);

                WriteLine(worksheet: ExcelPage,
                    line: line,
                    fillCellWhiteColor: false,
                    bordered: true,
                    align: ExcelHorizontalAlignment.Center,
                    mergeCells: true,
                    mergeNumber: 4);
            }

            _lowerY = CurrentCoordinates.Y;
        }

        private void GenerateApplicatioDoneVolumeColumn()
        {
            CurrentCoordinates = new System.Drawing.Point(6, 8);
            _applicationRows = new Dictionary<ApplicationReport, int>();

            WriteLine(worksheet: ExcelPage,
                line: "Объём выполненный, м³",
                isBold: true,
                fillCellWhiteColor: false,
                bordered: true,
                align: ExcelHorizontalAlignment.Center,
                mergeCells: true,
                mergeNumber: 4);

            foreach (ApplicationReport application in _reportData)
            {
                _applicationRows.Add(application, CurrentCoordinates.Y);

                string line = string.Format("{0:0.00}", application?.Layers?.FirstOrDefault()?.CurVolume ?? 0);

                WriteLine(worksheet: ExcelPage,
                    line: line,
                    fillCellWhiteColor: false,
                    bordered: true,
                    align: ExcelHorizontalAlignment.Center,
                    mergeCells: true,
                    mergeNumber: 4);
            }

            _lowerY = CurrentCoordinates.Y;
        }

        private void GenerateBorders()
        {
            CurrentCoordinates.X = 7;
            CurrentCoordinates.Y = 12;

            for (int i = 7; i <= _highterX; i += 1)
            {
                for (int j = 12; j < _lowerY; j += 2)
                {
                    try
                    {
                        ExcelPage.Cells[j, i, j + 1, i].Merge = true;
                    }
                    catch
                    {
                    }
                    ExcelPage.Cells[j, i, j + 1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);


                }
            }
        }

        private void GenerateComponentsColumns()
        {
            _resultByFact = new Dictionary<int, double>();
            _resultByRecipe = new Dictionary<int, double>();
            _componentsRows = new Dictionary<string, int>();

            var componentsTemp = _reportData.Select(x => x.Layers.Select(l => l.Recipe).Select(r => r.Structures?.Select(s => s.Component)));
            List<Component> componentList = new List<Component>();
            foreach (var i in componentsTemp)
            {
                foreach (var j in i)
                {
                    componentList.AddRange(j);
                }
            }

            //работает только в таком виде, иначе не будет биться со значениями в ячейке Итого
            componentList = componentList
                .OrderBy(x => x.Id)
                .ToList();
            //componentList = componentList.OrderByDescending(x => x.IdType == (int)ComponentType.Cement)
            //    .ThenByDescending(x => x.IdType == (int)ComponentType.ImLargeGravel)
            //    .ThenByDescending(x => x.IdType == (int)ComponentType.ImFineGravel)
            //    .ThenByDescending(x => x.IdType == (int)ComponentType.Chemical)
            //    .ThenByDescending(x => x.IdType == (int)ComponentType.Pigment)
            //    .ThenByDescending(x => x.IdType == (int)ComponentType.Water)
            //    .ThenBy(x => x.Id) // нет значения oldId
            //    .ToList();

            foreach (Component component in componentList)
            {
                if (!_componentsRows.ContainsKey(component.Name))
                {
                    var x = _componentsRows.Count > 0 ? _componentsRows.LastOrDefault().Value + 1 : 8;

                    CurrentCoordinates = new System.Drawing.Point(x, 8);

                    WriteLine(worksheet: ExcelPage,
                      line: component.Name,
                      isBold: true,
                      bordered: true,
                      rotateText: true,
                      textRotation: 90,
                      fillCellWhiteColor: false,
                      align: ExcelHorizontalAlignment.Center,
                      mergeCells: true,
                      mergeNumber: 4);

                    _componentsRows.Add(component.Name, CurrentCoordinates.X);
                }
            }

            int currentApplication = 1;

            foreach (ApplicationReport application in _reportData)
            {
                if (application.Layers.Count == 0) continue;

                LayerApplication layer = application.Layers[0];


                if (layer.Recipe.Structures.Count > 0)
                {
                    foreach (var structure in layer.Recipe.Structures)
                    {
                        //if (!ComponentsRows.ContainsKey(structure.ComponentName))
                        //{
                        //    var x = ComponentsRows.Count > 0 ? ComponentsRows.LastOrDefault().Value + 1: 7;

                        //    CurrentCoordinates = new System.Drawing.Point(x, 8);

                        //    WriteLine(worksheet: ExcelPage,
                        //      line: structure.ComponentName,
                        //      isBold: true,
                        //      bordered: true,
                        //      rotateText: true,
                        //      textRotation: 90, 
                        //      fillCellWhiteColor: false,
                        //      align: ExcelHorizontalAlignment.Center,
                        //      mergeCells: true,
                        //      mergeNumber: 4);

                        //    ComponentsRows.Add(structure.ComponentName, CurrentCoordinates.X);
                        //}

                        if (!_resultByFact.ContainsKey(structure.ComponentId))
                            _resultByFact.Add(structure.ComponentId, 0);

                        if (!_resultByRecipe.ContainsKey(structure.ComponentId))
                            _resultByRecipe.Add(structure.ComponentId, 0);

                        CurrentCoordinates.Y = _applicationRows[application];

                        if (!_componentsRows.ContainsKey(structure.ComponentName))
                            continue;

                        CurrentCoordinates.X = _componentsRows[structure.ComponentName];

                        WriteLine(worksheet: ExcelPage,
                           line: string.Format("{0:0.00}", structure.Amount * application.CompletedVolume),
                           fillCellWhiteColor: false,
                           align: ExcelHorizontalAlignment.Center,
                           mergeCells: true,
                           bordered: true,
                           mergeNumber: 2);

                        if (_resultByRecipe.ContainsKey(structure.ComponentId))
                            _resultByRecipe[structure.ComponentId] += structure.Amount * application.CompletedVolume;

                        var factVolume = application.Batches.Select(b => b.BatchMaterials.Where(x => x.Material == structure.ComponentName).Select(x => x.Real).Sum()).Sum();

                        WriteLine(worksheet: ExcelPage,
                          line: string.Format("{0:0.00}", factVolume),
                          fillCellWhiteColor: false,
                          align: ExcelHorizontalAlignment.Center,
                          mergeCells: true,
                          bordered: true,
                          mergeNumber: 2);

                        if (_resultByFact.ContainsKey(structure.ComponentId))
                        {
                            _resultByFact[structure.ComponentId] += factVolume;
                        }

                        _componentsRows[structure.ComponentName] = CurrentCoordinates.X;
                    }
                }
                else if (layer.Recipe.Id == 0)
                {
                    BatchReportMaterial[] materials = application.Batches.FirstOrDefault()?.BatchMaterials?.OrderBy(m => m.Material).ToArray() ?? Array.Empty<BatchReportMaterial>();

                    foreach (var material in materials)
                    {
                        if (!_resultByFact.ContainsKey(material.idMaterial_Old))
                            _resultByFact.Add(material.idMaterial_Old, 0);

                        if (!_resultByRecipe.ContainsKey(material.idMaterial_Old))
                            _resultByRecipe.Add(material.idMaterial_Old, 0);

                        CurrentCoordinates.Y = _applicationRows[application];

                        if (!_componentsRows.ContainsKey(material.Material))
                            continue;

                        CurrentCoordinates.X = _componentsRows[material.Material];

                        WriteLine(worksheet: ExcelPage,
                            line: string.Format("{0:0.00}", material.Real),
                            fillCellWhiteColor: false,
                            align: ExcelHorizontalAlignment.Center,
                            mergeCells: true,
                            bordered: true,
                            mergeNumber: 2);


                        if (_resultByRecipe.ContainsKey(material.idMaterial_Old))
                            _resultByRecipe[material.idMaterial_Old] += material.Real;

                        var factVolume = application.Batches.Select(b => b.BatchMaterials.Where(x => x.Material == material.Material).Select(x => x.Real).Sum()).Sum();

                        WriteLine(worksheet: ExcelPage,
                            line: string.Format("{0:0.00}", factVolume),
                            fillCellWhiteColor: false,
                            align: ExcelHorizontalAlignment.Center,
                            mergeCells: true,
                            bordered: true,
                            mergeNumber: 2);

                        if (_resultByFact.ContainsKey(material.idMaterial_Old))
                            _resultByFact[material.idMaterial_Old] += factVolume;

                        _componentsRows[material.Material] = CurrentCoordinates.X;
                    }
                }

                currentApplication += 1;
            }

            _highterX = _componentsRows.LastOrDefault().Value;
        }

        private void GenerateComponentsTypesColumn()
        {
            CurrentCoordinates = new System.Drawing.Point(7, 8);

            WriteLine(worksheet: ExcelPage,
                line: "тип",
                isBold: true,
                bordered: true,
                fillCellWhiteColor: false,
                align: ExcelHorizontalAlignment.Center,
                mergeCells: true,
                mergeNumber: 4);

            for (int i = 0; i < _reportData.Length; i++)
            {
                WriteLine(worksheet: ExcelPage,
                  line: "по рец.",
                  isBold: true,
                  fillCellWhiteColor: false,
                  align: ExcelHorizontalAlignment.Center,
                  mergeCells: true,
                  bordered: true,
                  mergeNumber: 2);

                WriteLine(worksheet: ExcelPage,
                  line: "фактич",
                  isBold: true,
                  fillCellWhiteColor: false,
                  align: ExcelHorizontalAlignment.Center,
                  mergeCells: true,
                  bordered: true,
                  mergeNumber: 2);
            }
        }
        private void GenerateBackground()
        {
            FillCellsWhiteColor(ExcelPage, 8, 1, _lowerY + 4, _highterX);
        }

        private void GenerateResultsRow()
        {
            CurrentCoordinates = new System.Drawing.Point(4, _lowerY);

            WriteLine(worksheet: ExcelPage,
                  line: "\t\t\t\tИтого:",
                  fillCellWhiteColor: true,
                  align: ExcelHorizontalAlignment.Center,
                  mergeCells: true,
                  mergeNumber: 2,
                  isBold: true);

            CurrentCoordinates = new System.Drawing.Point(5, _lowerY);

            WriteLine(worksheet: ExcelPage,
                  line: string.Format("{0:0.00}", _reportData.Select(a => a.Layers).Select(l => l.Select(x => x.Volume).Sum()).Sum()),
                  fillCellWhiteColor: false,
                  align: ExcelHorizontalAlignment.Center,
                  mergeCells: true,
                  bordered: true,
                  isBold: true,
                  mergeNumber: 4);

            CurrentCoordinates = new System.Drawing.Point(6, _lowerY);

            WriteLine(worksheet: ExcelPage,
                line: string.Format("{0:0.00}", _reportData.Select(a => a.Layers).Select(l => l.Select(x => x.CurVolume).Sum()).Sum()),
                fillCellWhiteColor: false,
                align: ExcelHorizontalAlignment.Center,
                mergeCells: true,
                bordered: true,
                isBold: true,
                mergeNumber: 4);

            CurrentCoordinates = new System.Drawing.Point(7, _lowerY);

            WriteLine(worksheet: ExcelPage,
                 line: "по рец.",
                 isBold: true,
                 fillCellWhiteColor: false,
                 align: ExcelHorizontalAlignment.Center,
                 mergeCells: true,
                 mergeNumber: 2);

            WriteLine(worksheet: ExcelPage,
              line: "фактич",
              isBold: true,
              bordered: true,
              fillCellWhiteColor: false,
              align: ExcelHorizontalAlignment.Center,
              mergeCells: true,
              mergeNumber: 2);

            foreach (var result in _resultByRecipe.OrderBy(x => x.Key))
            {
                CurrentCoordinates = new System.Drawing.Point(CurrentCoordinates.X + 1, _lowerY);

                WriteLine(worksheet: ExcelPage,
                             line: string.Format("{0:0.00}", result.Value),
                             isBold: true,
                             fillCellWhiteColor: false,
                             align: ExcelHorizontalAlignment.Center,
                             mergeCells: true,
                             bordered: true,
                             mergeNumber: 2);
            }

            CurrentCoordinates = new System.Drawing.Point(7, _lowerY + 2);

            foreach (var result in _resultByFact.OrderBy(x => x.Key))
            {
                CurrentCoordinates = new System.Drawing.Point(CurrentCoordinates.X + 1, _lowerY + 2);

                WriteLine(worksheet: ExcelPage,
                             line: string.Format("{0:0.00}", result.Value),
                             isBold: true,
                             fillCellWhiteColor: false,
                             align: ExcelHorizontalAlignment.Center,
                             mergeCells: true,
                             bordered: true,
                             mergeNumber: 2);

            }
        }

    }
}
