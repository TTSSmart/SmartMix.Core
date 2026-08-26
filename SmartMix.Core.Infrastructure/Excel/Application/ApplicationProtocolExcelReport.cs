using BSU.API.Data;
using BSU.API.Data.Recipes;
using BSU.API.Data.Reporting;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using ValueType = TTS.SmartMix.Export.Enums.ValueType;

namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    public class ApplicationProtocolExcelReport : ExcelReportBase
    {
        //Строка, Ширина строки
        private readonly IEnumerable<ProtocolBatchReport> _applicationReports;
        private ProtocolBatchReport _applicationReport;

        /// <summary>
        /// Не исп.
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="appFilter"></param>
        /// <param name="firmName"></param>
        /// <param name="applicationReports"></param>
        public ApplicationProtocolExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object applicationReports) : base(lineNumber, firmName, appFilter)
        {
            _applicationReports = applicationReports as IEnumerable<ProtocolBatchReport>;

            Description = "Протокол замеса";
            ReportName = "Протокол замеса";
            WidthInCells = 20;

            CurrentCoordinates = new System.Drawing.Point(2, 4);
            ChartRanges = new List<(string, string, string)>();
        }

        private float CalculateVolume(int componentId)
        {
            float realVolume = 1.0f;

            new HumidityAndDensityCalculator().Calculate(_applicationReport.Recipe.Structures.Where(x => x.Component.Id == componentId), out realVolume);

            return realVolume;
        }
        private double GetWaterContent(int componentId, double real)
        {
            Component component = _applicationReport.Recipe.Structures.FirstOrDefault(x => x.ComponentId == componentId)?.Component;
            if (component == null)
                return 0.0;

            if ((component.TypeComp.Id == (int)ComponentType.Water || component.TypeComp.Id == (int)ComponentType.WaterShlam || component.TypeComp.Id == (int)ComponentType.Chemical) && component.Humidity == 0.0f)
                return Math.Round(1.0f * real, 2);

            return Math.Round((_applicationReport.Recipe.Structures.FirstOrDefault(x => x.ComponentId == componentId).Component.Humidity * real) / 100, 2);
        }

        private double GetWaterContentPercent(int componentId)
        {
            Component component = _applicationReport.Recipe.Structures.FirstOrDefault(x => x.ComponentId == componentId)?.Component;
            if (component == null)
                return 0.0f;

            if ((component.TypeComp.Id == (int)ComponentType.Water || component.TypeComp.Id == (int)ComponentType.WaterShlam || component.TypeComp.Id == (int)ComponentType.Chemical) && component.Humidity == 0.0f)
                return Math.Round(100.0f, 2);

            return Math.Round(component.Humidity, 2);
        }

        private double GetVolume(int componentId, double realValue)
        {
            Component component = _applicationReport.Recipe.Structures.FirstOrDefault(x => x.ComponentId == componentId)?.Component;
            if (component == null)
                return 0.0;

            return Math.Round(realValue / component.Density, 2);
        }

        public List<(string, string, string)> ChartRanges { get; set; }
        public (string, string, string) PerfectScheduleRange { get; set; }
        public System.Drawing.Point ComponentsEndPoint { get; set; }
        public void CreateCurrentValues(ExcelWorksheet worksheet)
        {
            CurrentCoordinates.X = 2;
            CurrentCoordinates.Y = 200;

            ChartRanges = new List<(string, string, string)>();

            //var current = report.MixerCurrentValues.FirstOrDefault(x => x.BatchNumber == batchNumber);

            foreach (var report in _applicationReports)
            {
                foreach (var current in report.MixerCurrentValues)
                {

                    var columnX = new SingleColumn(current.CurrentValuesX.ToDoubles().ToIntArray());
                    var columnY = new SingleColumn(current.CurrentValuesY.ToDoubles());
                    var rangeX1 = CurrentCell;

                    AppendColumn(worksheet, columnX.Values, parseNumbers: true, fontSize: 9, valueType: ValueType.Integer);

                    var rangeX2 = CurrentCell;

                    CurrentCoordinates.X += 1;
                    CurrentCoordinates.Y = 200;

                    var rangeY1 = CurrentCell;

                    AppendColumn(worksheet, columnY.Values, parseNumbers: true, fontSize: 9, valueType: ValueType.Double);

                    CurrentCoordinates.Y -= 1;

                    var rangeY2 = CurrentCell;

                    //ChartRanges.Add((string.Format("{0}:{1}", rangeX1.Address, rangeX2.Address), string.Format("'{2}'!{0}:{1}", rangeY1, rangeY2, worksheet.Name),
                    //    report.BatchReports.FirstOrDefault(x => x.Number == current.BatchNumber)?.BatchId.ToString()));

                    CurrentCoordinates.X += 1;
                    CurrentCoordinates.Y = 200;
                }
            }
            //var x = current.CurrentValuesX.ToDoubles();
            //var y = current.CurrentValuesY.ToDoubles();

        }

        public void CreatePerfectValues(ExcelWorksheet worksheet)
        {
            if (_applicationReport.PerfectValues == null || _applicationReport.PerfectValues.Id == -1)
                return;

            CurrentCoordinates.X = 200;
            CurrentCoordinates.Y = 200;

            var columnX = new SingleColumn(_applicationReport.PerfectValues.CurrentValuesX.ToDoubles().ToIntArray());
            var columnY = new SingleColumn(_applicationReport.PerfectValues.CurrentValuesY.ToDoubles());

            var rangeX1 = CurrentCell;

            AppendColumn(worksheet, columnX.Values, parseNumbers: true, fontSize: 9, valueType: ValueType.Integer);

            var rangeX2 = CurrentCell;

            CurrentCoordinates.X += 1;
            CurrentCoordinates.Y = 200;

            var rangeY1 = CurrentCell;

            AppendColumn(worksheet, columnY.Values, parseNumbers: true, fontSize: 9, valueType: ValueType.Double);

            CurrentCoordinates.Y -= 1;

            var rangeY2 = CurrentCell;

            PerfectScheduleRange = (string.Format("{0}:{1}", rangeX1.Address, rangeX2.Address), string.Format("'{2}'!{0}:{1}", rangeY1, rangeY2, worksheet.Name), "Эталон");
        }

        public void CreateChart(ExcelWorksheet worksheet)
        {
            var chart = worksheet.Drawings.AddChart("Диаграмма - Консистенция", OfficeOpenXml.Drawing.Chart.eChartType.Line);
            chart.SetSize(700, 275);
            chart.Fill.Style = eFillStyle.SolidFill;
            chart.Fill.Color = Color.White;
            chart.SetPosition(4, 0, 8, 0);

            WriteLine(worksheet, "Диаграмма - Консистенция", fontSize: 11, isBold: true, x: 11, y: 4);

            foreach (var range in ChartRanges)
            {
                chart.Series.Add(range.Item2, range.Item1).Header = $"Конс. зам. {range.Item3}";
            }

            if (_applicationReport.PerfectValues != null && _applicationReport.PerfectValues.Id != -1)
            {
                chart.Series.Add(PerfectScheduleRange.Item2, PerfectScheduleRange.Item1).Header = PerfectScheduleRange.Item3;
            }

            chart.RoundedCorners = false;
            chart.PlotArea.Fill.Style = eFillStyle.SolidFill;
            chart.PlotArea.Fill.Color = System.Drawing.Color.White;
            chart.Border.LineStyle = OfficeOpenXml.Drawing.eLineStyle.Solid;
            chart.Border.Fill.Color = System.Drawing.Color.Transparent;

            chart.XAxis.MinorUnit = 50;
            chart.XAxis.MinValue = 50;

            chart.YAxis.Title.Text = "Линейный (%)";
            chart.XAxis.Title.Text = "Время смешивания (с)";
            chart.YAxis.Title.Font.Size = 10;
            chart.XAxis.Title.Font.Size = 10;

            chart.XAxis.MajorUnit = 50;
            chart.XAxis.Border.Fill.Color = System.Drawing.Color.Transparent;
            chart.YAxis.Border.Fill.Color = System.Drawing.Color.Black;
            chart.XAxis.Border.Width = 1;

            chart.YAxis.MajorGridlines.Fill.Color = System.Drawing.Color.Black;
        }
        public void CreateComponents(BatchReport batch, ExcelWorksheet worksheet)
        {
            CurrentCoordinates.X = 2;
            CurrentCoordinates.Y = 19;

            var headerRow = new Row();
            headerRow.Append(ExcelHorizontalAlignment.Right, "Мат №");
            headerRow.Append(ExcelHorizontalAlignment.Left, "Наименование", "Задан. су", " Ед", "Задан.зна", "Ед", "Действ.з", "Ед", "Откл.", "Ед", "Откл(%)", "Вл(%)", "Объем", "Водосо");
            AppendRow(worksheet, headerRow.Cells, isBold: true);

            foreach (var component in batch.BatchMaterials)
            {
                CurrentCoordinates.X = 2;

                //var structure = report.Recipe.Structures.FirstOrDefault(x => x.ComponentId == component.Id);
                var componentRow = new Row();

                //TODO: Водосодержание = (Humidity * Real_Weight), нужно Получить где-то влажность 

                componentRow.Append(ExcelHorizontalAlignment.Right, component.idMaterial);
                componentRow.Append(ExcelHorizontalAlignment.Left, component.Material, Math.Round(component.Need, 2), "kg", Math.Round(component.Need, 2), "kg", Math.Round(component.Real, 2), "kg", Math.Round(component.BalanceError, 2), "kg", Math.Round((component.BalanceErrorPercent * 100), 2), GetWaterContentPercent(component.idMaterial_Old),
                    GetVolume(component.idMaterial_Old, component.Real),
                    GetWaterContent(component.idMaterial_Old, component.Real));

                AppendRow(worksheet, componentRow.Cells);
            }

            ComponentsEndPoint = CurrentCoordinates;
        }
        private void PrintDateAndSeite(ExcelWorksheet worksheet)
        {
            CurrentCoordinates = new Point(2, CurrentCoordinates.Y + 4);

            var dateString = DateTime.Now.ToString("f"); //13

            WriteCell(worksheet, dateString, 9, false);
        }
        private double GetWeight(IEnumerable<BatchReportMaterial> materials)
        {
            return materials.Select(x => x.Real).Sum();
        }

        private IEnumerable<BatchReportMaterial> GetWater(BatchReport batch)
        {
            return batch.BatchMaterials.Where(x => x.Type == (int)ComponentType.Water || x.Type == (int)ComponentType.WaterShlam);
        }
        private IEnumerable<BatchReportMaterial> GetCement(BatchReport batch)
        {
            return batch.BatchMaterials.Where(x => x.Type == (int)ComponentType.Cement);
        }
        public void CreateFooter(BatchReport batch, ExcelWorksheet worksheet)
        {
            CurrentCoordinates.X = 2;
            CurrentCoordinates.Y = ComponentsEndPoint.Y + 2;

            var column1 = new Column();
            column1.Append("В/Цк", Math.Round(GetWeight(GetWater(batch)) / GetWeight(GetCement(batch)), 2));
            column1.Append("В/Цк Макс", "0");
            AppendColumn(worksheet, column1.GetValues());

            CurrentCoordinates.X = 5;
            CurrentCoordinates.Y = ComponentsEndPoint.Y + 2;

            var column2 = new Column();
            column2.Append("Всего Задан (кг)", Math.Round(batch.BatchMaterials.Select(x => x.Need).Sum(), 2));
            column2.Append("Дейст (кг)", Math.Round(batch.BatchMaterials.Select(x => x.Real).Sum(), 2));
            AppendColumn(worksheet, column2.GetValues());

            CurrentCoordinates.X = 8;
            CurrentCoordinates.Y = ComponentsEndPoint.Y + 2;

            var column3 = new Column();
            column3.Append("Вода для В/Цк (кг)", Math.Round(GetWater(batch).Select(x => x.Need).Sum()), 2);
            column3.Append("Дейст (кг)", Math.Round(GetWater(batch).Select(x => x.Real).Sum(), 2));
            AppendColumn(worksheet, column3.GetValues());

            CurrentCoordinates.X = 11;
            CurrentCoordinates.Y = ComponentsEndPoint.Y + 2;

            var column4 = new Column();
            column4.Append("Мощность смесителя перед выгр.", Math.Round(batch.MixerCurrent, 3) + "%");
            AppendColumn(worksheet, column4.GetValues());
        }
        public double GetWaterContent(BatchReport batch, BatchReportMaterial material)
        {
            return batch.HumidityByRecipe * material.Real;
        }
        public void CreateMain(BatchReport batch, ExcelWorksheet worksheet)
        {
            CurrentCoordinates.X = 2;
            CurrentCoordinates.Y = 9;

            var column1 = new Column();
            column1.Append("Марка", string.Format("{0} {1} {2}", _applicationReport.RecipeNumber, _applicationReport.EnduranceClass, _applicationReport.RecipeName));
            column1.Append("Продукт", " ");
            column1.Append("Класс прочности", _applicationReport.EnduranceClass);
            column1.Append("Вр. смеш. (с)", "Задан     " + batch.ActualMixingTime); //TODO: нужно уточнить по поводу реального заданного времени
            column1.Append("#empty1", "Дейст     " + batch.ActualMixingTime);
            column1.AppendEmpty();
            //column1.Append("Консистенц", "Задан     " + batch.ConsOk); // TODO: нужно уточнить по поводу реально заданной и действительной консистенции
            //column1.Append("#empty2", "Дейст     " + batch.ActualConsis);
            column1.AppendEmpty();
            AppendColumn(worksheet, column1.GetValues());

            CurrentCoordinates.Y = 11;
            CurrentCoordinates.X = 4;

            var column2 = new Column();
            //column2.Append("Класс консистенц.", Enum.GetName(typeof(ConsistencyType), _applicationReport.Recipe.Consistency.Type));
            //column2.Append("Содерж. воздуха", _applicationReport.Recipe.AirPercent);
            column2.Append("Вода для мойки (кг)", "");
            column2.Append("Корр. воды на м3", GetWater(batch).FirstOrDefault()?.ManualCorrection);
            column2.Append("Температура бетона (°C)", batch.Temperature);
            column2.Append("Температура воздуха (°C)", _applicationReport.Temperatures.FirstOrDefault(x => x.UpdateDateTime.Date == batch.EndTime.Date)?.Value.ToString() ?? $"Нет записей за {batch.EndTime.ToString("dd.MM.yyyy")}");
            AppendColumn(worksheet, column2.GetValues());

            CurrentCoordinates.Y = 11;
            CurrentCoordinates.X = 6;

            var column3 = new Column();
            column3.Append("Макс. зерно", _applicationReport.MaxGranularity);
            AppendColumn(worksheet, column3.GetValues());
        }
        public void CreateHeaderTitle(BatchReport batch, ExcelWorksheet worksheet)
        {
            CurrentCoordinates.X = 2;
            CurrentCoordinates.Y = 3;

            WriteLine(worksheet, "Протокол замеса", fontSize: 16, isBold: true, x: 2, y: 3);
            //WriteLine(worksheet, $"№{batch.BatchId}", fontSize: 9, isBold: true, x: 4, y: 3, align: ExcelHorizontalAlignment.Left);

            CurrentCoordinates.X = 2;
            CurrentCoordinates.Y = 4;

        }
        public void CreateHeader(BatchReport batch, ExcelWorksheet worksheet)
        {
            CurrentCoordinates.X = 2;
            CurrentCoordinates.Y = 4;

            var column1 = new Column();
            column1.Append("Дата", batch.EndTime.ToString("dd.MM.yyyy"));
            column1.Append("Время", batch.EndTime.ToString("HH:mm"));
            column1.Append("Накладная", _applicationReport.WayBill);
            //column1.Append("Оператор", batch.Application.Creator.Name);
            AppendColumn(worksheet, column1.GetValues());

            CurrentCoordinates.X += 2;
            CurrentCoordinates.Y = 4;

            var column2 = new Column();
            column2.Append("Завод", _applicationReport.LineNumber);
            column2.Append("Смесит", _applicationReport.BatchCount);
            column2.Append("Вып. уст.", ""); // выполнено замесов
            column2.Append("Кол. замесов", _applicationReport.BatchCount);
            AppendColumn(worksheet, column2.GetValues());

            CurrentCoordinates.X += 2;
            CurrentCoordinates.Y = 4;

            var column3 = new Column();
            column3.Append("Кол-во", batch.Volume);
            column3.Append("Авто №", "#empty");
            column3.Append("Номер маш.", _applicationReport.Application.Car.Name);
            AppendColumn(worksheet, column3.GetValues());
        }

        protected override void GenerateReport()
        {
            _applicationReport = _applicationReports.FirstOrDefault();

            var workbook = ExcelPage.Workbook;

            if (_applicationReport.BatchCount > 0)
                workbook.Worksheets.Delete(workbook.Worksheets.FirstOrDefault());

            var value = 0.0f;

            _applicationReport.RecipeStructures = new HumidityAndDensityCalculator().Calculate(_applicationReport.Recipe.Structures, out value);

            //foreach (var batch in _applicationReport.BatchReports)
            //{
            //    var sheet = workbook.Worksheets.Add("Протокол замеса " + batch.BatchId);

            //    sheet.Column(4).Width = 20;
            //    sheet.DefaultColWidth = 15;

            //    CreateCurrentValues(sheet);
            //    CreatePerfectValues(sheet);
            //    CreateChart(sheet);
            //    CreateHeaderTitle(batch, sheet);
            //    CreateHeader(batch, sheet);
            //    CreateMain(batch, sheet);
            //    CreateComponents(batch, sheet);
            //    CreateFooter(batch, sheet);

            //    PrintDateAndSeite(sheet);

            //    CreateDottedBorder(sheet, 4, 2, 8, 7); // создание прерывистой линии (обводки для Header)
            //    CreateDottedBorder(sheet, 9, 2, 18, 7); // создание прерывистой линии (обводки для Main)
            //    CreateDottedBorder(sheet, 19, 2, ComponentsEndPoint.Y, 18); // Создание прерывистой линии (обводки для Components)
            //    CreateDottedBorder(sheet, 4, 8, 18, 18); // создание прерывистой линии (обводки для Диаграммы)
            //    FillCellsWhiteColor(sheet, 1, 2, 1000, 18);
            //}

        }
    }
    public class Row
    {
        public List<(object, ExcelHorizontalAlignment)> Cells { get; set; }
        public void Append(string cellText, ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left)
        {
            if (Cells == null)
                Cells = new List<(object, ExcelHorizontalAlignment)>();

            if (string.IsNullOrEmpty(cellText))
                cellText = "?";

            Cells.Add((cellText, align));
        }
        public void Append(object obj, ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left)
        {
            if (obj == null)
                obj = "";

            Append(obj.ToString(), align);
        }
        public void Append(ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left, params object[] objs)
        {
            foreach (var obj in objs)
            {
                if (obj is double[])
                {
                    foreach (var number in obj as double[])
                    {
                        Append(Math.Round(number, 2), align);
                    }

                    continue;
                }

                Append(obj, align);
            }
        }
        public Row() { }
        public Row(ExcelHorizontalAlignment align = ExcelHorizontalAlignment.Left, params object[] objs)
        {
            Append(align, objs);
        }
    }
    public class SingleColumn
    {
        public List<string> Values { get; set; }
        public void Append(string text)
        {
            Values.Add(text);
        }

        public void Append(object obj)
        {
            if (obj == null)
                obj = "";

            Append(obj.ToString());
        }
        public void Append(params object[] objs)
        {
            if (objs == null)
                return;

            foreach (var obj in objs)
            {
                if (obj is double[])
                {
                    foreach (var number in obj as double[])
                    {
                        Append(Math.Round(number, 2));
                    }

                    continue;
                }
                if (obj is int[])
                {
                    foreach (var number in obj as int[])
                    {
                        Append(number);
                    }

                    continue;
                }

                Append(obj);
            }
        }
        public SingleColumn()
        {
            Values = new List<string>();
        }
        public SingleColumn(params object[] objs)
        {
            Values = new List<string>();

            Append(objs);
        }
    }
    public class Column
    {
        public Dictionary<string, string> Data { get; set; }
        public void Append(string type, string data)
        {
            if (Data == null)
                Data = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(data))
                data = "?";

            Data.Add(type, data);
        }
        public void Append(object type, object data)
        {
            if (Data == null)
                Data = new Dictionary<string, string>();

            if (data == null)
                data = "?";

            if (string.IsNullOrEmpty(data.ToString()))
                data = "?";

            Data.Add(type.ToString(), data.ToString());
        }
        public void Append(params object[] data)
        {

        }
        //public IEnumerable<(string, string)> GetValues()
        //{
        //    return Data.Select(x => (x.Key, x.Value));
        //}
        public IEnumerable<(object, object)> GetValues()
        {
            return Data.Select(x => (x.Key as object, x.Value as object));
        }
        public void AppendEmpty()
        {
            Data.Add(string.Format("#empty {0}", Data.Values.Count()), "#empty");
        }
    }

}
