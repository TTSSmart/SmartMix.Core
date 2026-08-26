using BSU.API.Data.Recipes;
using BSU.API.Data.Reporting;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using TTS.SmartMix.Export.DataExport.ExcelExport.Extensions;
using TTS.SmartMix.Export.Helpers;

namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    /// <summary>
    /// Генератор отчета <see cref="ReportType.ConsumptionMaterial"/>.
    /// </summary>
    public class ConsumptionMaterialExcelReport : ExcelReportBase
    {
        private readonly ApplicationReport[] _report;
        public const string FLOAT3_FORMAT = "0.000";

        public ConsumptionMaterialExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter)
        {
            _report = report as ApplicationReport[];

            Description = "Расход материалов по отгрузке";
            ReportName = "Расход материалов по отгрузке";

            DictionaryColumnWidth = new Dictionary<int, double>()
            {
                {1, 7}, // Левая граница
                {2, 30}, //  Наименование компонента 
                {3, 12}, // Влажность (влагосодержание), %
                {4, 8}, // пробел
                {5, 15}, // Номинальный состав на 1м3
                {6, 15}, // Рабочий состав на 1м3 (с учетом влажности)
                {7, 15}, // Рабочий состав на 1м3 (с учетом рециклинга)
                {8, 15}, // Рабочий состав за заявку/ замес
                {9, 12}, // Корректировки (ручные)
                {10, 15}, // Факт.расход на заявку / замес
                {11, 15}, // Факт.расход (на влажные) на 1м3
                {12, 15}, // Факт.расход (на сухие) на 1м3
                {13, 15}, // Факт.расход на 1м3 с коэфф.выхода
                {14, 15}, // Отклонение по заданию на взвешивание
                {15, 15} // Отклонение по рецепту
            };
            WidthInCells = DictionaryColumnWidth.Count;

            IsLandscape = true;
            UseFooter = true;
        }

        protected override void GenerateReport()
        {
            foreach (ApplicationReport applicationReport in _report)
            {
                RowNumber += 2;

                GenerateApplicationHeader(applicationReport.Id);
                RowNumber += 1;

                GenerateApplicationInfo(applicationReport);
                RowNumber += 2;
                //шапка таблиц
                GenerateBatchMaterialTableHeader(ExcelBorderStyle.Medium, System.Drawing.Color.Gainsboro);
                //ИТОГО заявка
                GenerateApplicationSummary(applicationReport);

                RowNumber++;
                ExcelPage.Cells[RowNumber, StartColumnNumber].ApplyHeaderStyle().Value = "Информация по замесам";
                ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].ApplyBorders(ExcelBorderStyle.Thin);
                ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].ApplyBackground(System.Drawing.Color.Gainsboro);
                //по замесам
                foreach (var batch in applicationReport.Batches)
                {
                    RowNumber++;
                    ExcelPage.Cells[RowNumber, StartColumnNumber].Value = "Замес:";
                    ExcelPage.Cells[RowNumber, StartColumnNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + 1].Value = batch.Number;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + 1].Style.Font.Bold = true;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].ApplyBorders(ExcelBorderStyle.Thin);
                    ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].ApplyBackground(System.Drawing.Color.LightGray);
                    RowNumber++;

                    ExcelPage.Cells[RowNumber, StartColumnNumber].Value = "Кол-во:";
                    ExcelPage.Cells[RowNumber, StartColumnNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + 1].ApplyHeaderStyle().Value = batch.Volume;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + 1].Style.Font.Bold = true;
                    ExcelPage.Cells[RowNumber, StartColumnNumber + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    RowNumber++;

                    CalculateModels(batch.BatchMaterials, batch.Volume, applicationReport.Layers[0].Recipe);
                    GenerateBatchMaterialTable(ReportHelper.Sort(batch.BatchMaterials));
                }
            }
        }

        /// <summary>
        /// Выводит заголовок по заявке
        /// </summary>
        /// <param name="appId">Номер заявки</param>
        private void GenerateApplicationHeader(int appId)
        {
            ExcelPage.Cells[RowNumber, StartColumnNumber].Value = $"Заявка № {appId}";
            ExcelPage.Cells[RowNumber, StartColumnNumber].Style.Font.Bold = true;

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
        }

        /// <summary>
        /// Выводит общие данные по заявке
        /// </summary>
        /// <param name="applicationReport">Данные по заявке</param>
        private void GenerateApplicationInfo(ApplicationReport applicationReport)
        {
            int column1 = StartColumnNumber;
            const int skip = 2;

            ExcelPage.Cells[RowNumber, column1].Value = "Продукция (номенклатура)";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = applicationReport.Product?.Name;
            ExcelPage.Cells[RowNumber, column1 + skip, RowNumber, column1 + skip + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "Номер продукта";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = applicationReport.Layers[0].Recipe?.Name;
            ExcelPage.Cells[RowNumber, column1 + skip, RowNumber, column1 + skip + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "Средняя температура смеси";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip, RowNumber, column1 + skip + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            RowNumber++;


            ExcelPage.Cells[RowNumber, column1].Value = "Количество";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = applicationReport.Volume;
            ExcelPage.Cells[RowNumber, column1 + skip, RowNumber, column1 + skip + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.Numberformat.Format = INT_FORMAT;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "Смеситель и продолжительность отгрузки";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip, RowNumber, column1 + skip + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "Дата и время отгрузки";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = CheckDate(applicationReport.StartTime);
            ExcelPage.Cells[RowNumber, column1 + skip].Style.Numberformat.Format = DATE_TIME_FORMAT;
            ExcelPage.Cells[RowNumber, column1 + skip, RowNumber, column1 + skip + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ExcelPage.Cells[RowNumber, column1 + skip * 2].Value = CheckDate(applicationReport.EndTime);
            ExcelPage.Cells[RowNumber, column1 + skip * 2].Style.Numberformat.Format = DATE_TIME_FORMAT;
            ExcelPage.Cells[RowNumber, column1 + skip * 2, RowNumber, column1 + skip * 2 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip * 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ExcelPage.Cells[RowNumber, column1 + skip * 3 + 1].Value = "Продолжительность";
            ExcelPage.Cells[RowNumber, column1 + skip * 3 + 1, RowNumber, column1 + skip * 4].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip * 4 + 1].Value = applicationReport.RunTime.Value;
            ExcelPage.Cells[RowNumber, column1 + skip * 4 + 1].Style.Numberformat.Format = INT_FORMAT;

            ExcelPage.Cells[RowNumber, column1 + skip * 5].Value = TimeSpan.FromSeconds(applicationReport.RunTime.Value);
            ExcelPage.Cells[RowNumber, column1 + skip * 5].Style.Numberformat.Format = TIME_FORMAT;
            ExcelPage.Cells[RowNumber, column1 + skip * 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "Машина";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = CheckName(applicationReport.Car?.Name);
            ExcelPage.Cells[RowNumber, column1 + skip, RowNumber, column1 + skip + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "Заказчик";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = CheckName(applicationReport.Client?.Name);
            ExcelPage.Cells[RowNumber, column1 + skip, RowNumber, column1 + skip + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            RowNumber++;

            ExcelPage.Cells[RowNumber, column1].Value = "№ накладной";
            ExcelPage.Cells[RowNumber, column1, RowNumber, column1 + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Value = CheckName(applicationReport.WayBill);
            ExcelPage.Cells[RowNumber, column1 + skip, RowNumber, column1 + skip + 1].Merge = true;
            ExcelPage.Cells[RowNumber, column1 + skip].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            RowNumber++;
        }

        /// <summary>
        /// Формирует заголовок для таблицы по материалам замеса.
        /// </summary>
        /// <param name="borderStyle">Границы.</param>
        /// <param name="color">Цвет заголовка.</param>
        protected void GenerateBatchMaterialTableHeader(ExcelBorderStyle borderStyle = ExcelBorderStyle.Thin, System.Drawing.Color? color = null)
        {
            int colNumber = StartColumnNumber;
            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Наименование компонента";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Влажность (влагосодержание), %"; // по рецепту
            colNumber++;
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Номинальный состав на 1м3";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Рабочий состав на 1м3 (с учетом влажности)";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Рабочий состав на 1м3 (с учетом рециклинга)";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Рабочий состав за заявку/ замес";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Корректировки (ручные)";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Факт.расход на заявку / замес";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Факт.расход (на влажные) на 1м3";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Факт.расход (на сухие) на 1м3";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Факт.расход на 1м3 с коэфф.выхода";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Отклонение по заданию на взвешивание";
            colNumber++;

            ExcelPage.Cells[RowNumber, colNumber].ApplyHeaderStyle().Value = "Отклонение по рецепту";

            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, colNumber].ApplyBorders(borderStyle);
            if (color.HasValue)
                ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, colNumber].ApplyBackground(color.Value);

            RowNumber++;
        }

        /// <summary>
        /// Формирует таблицу итоговых даннх по заявке
        /// </summary>
        /// <param name="applicationReport">Данные по заявке</param>
        private void GenerateApplicationSummary(ApplicationReport applicationReport)
        {
            double volume = 0;
            double factVolume = 0; // объем фактический
            var materialSummary = new Dictionary<string, BatchReportMaterial>();

            foreach (BatchReport batch in applicationReport.Batches)
            {
                volume += batch.Volume;
                factVolume += batch.RealVolume;

                foreach (BatchReportMaterial material in batch.BatchMaterials)
                {
                    if (materialSummary.ContainsKey(material.Material))
                    {
                        materialSummary[material.Material].Manual += material.Manual;
                        materialSummary[material.Material].Need += material.Need;
                        materialSummary[material.Material].Auto += material.Auto;
                        materialSummary[material.Material].ManualCorrection += material.ManualCorrection;
                        materialSummary[material.Material].HumidityCorrection += material.HumidityCorrection;
                    }
                    else
                    {
                        materialSummary.Add(material.Material, material.Clone()); // todo клонирование из-за foreach
                    }
                }
            }

            List<BatchReportMaterial> batchMaterialList = new List<BatchReportMaterial>();
            foreach (KeyValuePair<string, BatchReportMaterial> kvp in materialSummary)
            {
                BatchReportMaterial clone = kvp.Value.Clone();
                clone.Material = kvp.Key; // todo точно нужно?
                batchMaterialList.Add(clone);
            }

            RowNumber++;

            ExcelPage.Cells[RowNumber, StartColumnNumber].ApplyHeaderStyle().Value = "Информация по отгрузке";
            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, StartColumnNumber].Style.Font.Bold = true;
            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].ApplyBorders(ExcelBorderStyle.Thin);
            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber, WidthInCells].ApplyBackground(System.Drawing.Color.Gainsboro);
            RowNumber++;
            CalculateModels(batchMaterialList.ToArray(), applicationReport.Volume, applicationReport.Layers[0].Recipe);
            GenerateBatchMaterialTable(ReportHelper.Sort(batchMaterialList));
        }

        /// <summary>
        /// Высчитывает дополнительные данные для отчета
        /// </summary>
        /// <param name="batchMaterialList">Список материалов</param>
        /// <param name="volume">Объем заявки/замеса</param>
        /// <param name="recipe">Рецепт (для номинального состава)</param>
        private void CalculateModels(BatchReportMaterial[] batchMaterialList, double volume, Recipe recipe)
        {
            Dictionary<int, double> recipeStructure = new Dictionary<int, double>();
            RecipeStructure[] structureData = ReportHelper.Sort(recipe.Structures);
            for (int i = 0; i < structureData.Length; i++)
            {
                recipeStructure.Add(structureData[i].ComponentId, structureData[i].Amount);
            }
            // считаем Рабочий состав на 1 м3 (с учетом влажности и с учетом рециклинга)
            // у ИМ = масса по рецепту + коэф влажности
            // у ИМ крупнограв - равны
            // у ИМ мелкограв - сейчас равны, если будет шлам, то пересчитается с учетом рециклинга 
            // у цемента и химии равны
            foreach (BatchReportMaterial material in batchMaterialList)
            {
                //чуть чуть подменяем свойства запроса на значение из рецепта
                if (recipeStructure.ContainsKey(material.idMaterial_Old)) material.Need = recipeStructure[material.idMaterial_Old];
                else material.Need /= volume;

                if (material.Type == (int)ComponentType.ImLargeGravel || material.Type == (int)ComponentType.ImFineGravel)
                {
                    material.WorkHumidity = material.WorkRecycle = material.Need + (material.Need * material.Humidity) / 100;
                }
                else if (material.Type == (int)ComponentType.Cement || material.Type == (int)ComponentType.Chemical)
                {
                    material.WorkHumidity = material.WorkRecycle = material.Need;
                }
            }
            //выбираем воду и шлам
            var water = batchMaterialList.FirstOrDefault(p => p.Type == (int)ComponentType.Water && p.Material == "Вода");
            var shlam = batchMaterialList.FirstOrDefault(p => p.Type == (int)ComponentType.WaterShlam);
            if (water != null)
            {
                //из воды по рецепту вычитаем воду в ИМ
                water.WorkHumidity = water.Need - batchMaterialList.Where(p => p.Type == (int)ComponentType.ImLargeGravel || p.Type == (int)ComponentType.ImFineGravel).Sum(q => q.WorkHumidity - q.Need);
                if (shlam != null)
                {
                    //у шлама задается % от воды который он будет заполнять (10% - 10% - шлам, 90% - вода)
                    //из воды находим сколько будет шлама
                    shlam.WorkHumidity = water.WorkHumidity * (shlam.Need / 100);
                    //вычисляем непосредственно воду (если 90% - 90% - чистая вода, 10% - ИМ мелкограв)
                    shlam.WorkRecycle = shlam.WorkHumidity / (shlam.Humidity / 100);
                }
                //теперь из воды вычитаем уже процент шлама
                water.WorkHumidity = water.WorkRecycle = water.WorkHumidity - (shlam?.Need ?? 0);
            }
            var im = batchMaterialList.Where(p => p.Type == (int)ComponentType.ImFineGravel);
            if (im != null && im.Count() > 0 && shlam != null)
            {
                var shlamPerIm = (shlam.WorkRecycle - shlam.WorkHumidity) / im.Count();
                //% заполнителя в шламе распределяем между ИМ мелкограв
                foreach (var p in im)
                {
                    p.WorkRecycle = p.WorkHumidity - shlamPerIm;
                }
            }

            foreach (var mat in batchMaterialList)
            {
                //пересчитаываем на объем
                mat.Work = mat.WorkRecycle * volume;
                //пересчитываем на куб факт
                mat.RealCubWet = mat.Real / volume;
                //высчитываем сколько было сухих материалов 
                if (mat.Type != (int)ComponentType.Chemical) mat.RealCubDry = mat.RealCubWet / (1.0 + mat.Humidity / 100);
                else mat.RealCubDry = mat.RealCubWet;
                //отклонение по заданию на взвешивание
                mat.DiffWork = (mat.RealCubWet - (mat.WorkHumidity + mat.ManualCorrection)) / (mat.WorkHumidity + mat.ManualCorrection) * 100; // у воды еще почему то влагосодержание прибавляется к WorkHumidity
            }

            if (water != null)
            {
                //к воде прибывляем влажность в ИМ
                water.RealCubDry = water.RealCubWet + batchMaterialList.Where(p => p.Type == (int)ComponentType.ImLargeGravel || p.Type == (int)ComponentType.ImFineGravel)
                                                    .Sum(q => (q.RealCubWet - q.RealCubDry)) * 1 /*какое то еще умножение*/;
            }
            double sumNeed = batchMaterialList.Sum(p => p.Need);
            double sumRealCubDry = batchMaterialList.Sum(p => p.RealCubDry);
            double outKoef = sumRealCubDry / sumNeed;

            foreach (var mat in batchMaterialList)
            {
                mat.RealCubKoef = mat.RealCubDry / outKoef;

                mat.DiffRecipe = (mat.RealCubKoef - mat.Need) / mat.Need * 100;
            }
        }

        /// <summary>
        /// Выводит информацию по материалам замесов.
        /// </summary>
        /// <param name="batchReportMaterials">Массив материалов по замесу.</param>
        protected void GenerateBatchMaterialTable(BatchReportMaterial[] batchReportMaterials)
        {
            int column = StartColumnNumber;
            int startRow = RowNumber;


            for (int i = 0; i < batchReportMaterials.Length; i++)
            {
                int collCount = 1;
                ExcelPage.Cells[RowNumber, column].Value = batchReportMaterials[i].Material;
                ExcelPage.Cells[RowNumber, column].Style.WrapText = true;

                //влажность
                if (batchReportMaterials[i].Type == (int)ComponentType.ImFineGravel || batchReportMaterials[i].Type == (int)ComponentType.ImLargeGravel
                            || batchReportMaterials[i].Type == (int)ComponentType.WaterShlam || batchReportMaterials[i].Type == (int)ComponentType.Chemical)
                {
                    ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].Humidity;
                }
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                collCount++;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].Need;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                collCount++;
                //корр влажности
                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].WorkHumidity;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                collCount++;
                //корр рециклинга
                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].WorkRecycle;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                collCount++;
                //состав на заявку/замес
                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].Work;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].ManualCorrection;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].Real;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].RealCubWet;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].RealCubDry;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].RealCubKoef;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].DiffWork;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                collCount++;

                ExcelPage.Cells[RowNumber, column + collCount].Value = batchReportMaterials[i].DiffRecipe;
                ExcelPage.Cells[RowNumber, column + collCount].Style.Numberformat.Format = FLOAT3_FORMAT;
                RowNumber++;
            }


            ExcelPage.Cells[startRow, column, RowNumber + 3, WidthInCells].ApplyBorders(ExcelBorderStyle.Thin);
            ExcelPage.Cells[RowNumber, StartColumnNumber, RowNumber + 3, WidthInCells].ApplyBackground(System.Drawing.Color.LightGray);

            ExcelPage.Cells[RowNumber, column].Value = "Водовяжущее отношение";
            var totalBindNeed = batchReportMaterials.Where(p => p.Type == (int)ComponentType.Cement).Sum(p => p.Need);
            var totalBindReal = batchReportMaterials.Where(p => p.Type == (int)ComponentType.Cement).Sum(p => p.RealCubKoef);
            //сумма воды, расходомера, шлама + водосодержание в химии / вяжущее (цемент)
            var waterOtnoshNeed = (batchReportMaterials.Where(p => p.Type == (int)ComponentType.Water || p.Type == (int)ComponentType.WaterShlam).Sum(p => p.Need) +
                                                     batchReportMaterials.Where(p => p.Type == (int)ComponentType.Chemical).Sum(p => p.Need * p.Humidity / 100))
                                                     / totalBindNeed;
            var waterOthoshReal = (batchReportMaterials.Where(p => p.Type == (int)ComponentType.Water || p.Type == (int)ComponentType.WaterShlam).Sum(p => p.RealCubKoef) +
                                                     batchReportMaterials.Where(p => p.Type == (int)ComponentType.Chemical).Sum(p => p.RealCubKoef * p.Humidity / 100))
                                                     / totalBindReal;
            double sumNeed = batchReportMaterials.Sum(p => p.Need);
            double sumRealCubDry = batchReportMaterials.Sum(p => p.RealCubDry);
            //коэф выхода
            double outKoef = sumRealCubDry / sumNeed;

            ExcelPage.Cells[RowNumber, column + 3].Value = waterOtnoshNeed;
            ExcelPage.Cells[RowNumber, column + 3].Style.Numberformat.Format = FLOAT3_FORMAT;
            ExcelPage.Cells[RowNumber, column + 11].Value = waterOthoshReal;
            ExcelPage.Cells[RowNumber, column + 11].Style.Numberformat.Format = FLOAT3_FORMAT;
            ExcelPage.Cells[RowNumber, column + 13].Value = waterOthoshReal - waterOtnoshNeed;
            ExcelPage.Cells[RowNumber, column + 13].Style.Numberformat.Format = FLOAT3_FORMAT;
            if (waterOthoshReal - waterOtnoshNeed > 0.02)
            {
                ExcelPage.Cells[RowNumber, column + 13].ApplyBackground(System.Drawing.Color.IndianRed);
            }
            RowNumber++;

            ExcelPage.Cells[RowNumber, column].Value = "Общее вяжущеее";
            ExcelPage.Cells[RowNumber, column + 3].Value = totalBindNeed;
            ExcelPage.Cells[RowNumber, column + 3].Style.Numberformat.Format = FLOAT3_FORMAT;
            ExcelPage.Cells[RowNumber, column + 11].Value = totalBindReal;
            ExcelPage.Cells[RowNumber, column + 11].Style.Numberformat.Format = FLOAT3_FORMAT;
            ExcelPage.Cells[RowNumber, column + 13].Value = totalBindReal - totalBindNeed;
            ExcelPage.Cells[RowNumber, column + 13].Style.Numberformat.Format = FLOAT3_FORMAT;
            if (totalBindReal - totalBindNeed < -8)
            {
                ExcelPage.Cells[RowNumber, column + 13].ApplyBackground(System.Drawing.Color.IndianRed);
            }
            RowNumber++;

            ExcelPage.Cells[RowNumber, column].Value = "Коэффициент выхода";
            ExcelPage.Cells[RowNumber, column + 11].Value = outKoef;
            ExcelPage.Cells[RowNumber, column + 11].Style.Numberformat.Format = FLOAT3_FORMAT;
            RowNumber++;

            ExcelPage.Cells[RowNumber, column].Value = "ИТОГО:";
            ExcelPage.Cells[RowNumber, column + 3].Value = batchReportMaterials.Sum(p => p.Need);
            ExcelPage.Cells[RowNumber, column + 3].Style.Numberformat.Format = FLOAT3_FORMAT;
            ExcelPage.Cells[RowNumber, column + 4].Value = batchReportMaterials.Sum(p => p.WorkHumidity);
            ExcelPage.Cells[RowNumber, column + 4].Style.Numberformat.Format = FLOAT3_FORMAT;
            ExcelPage.Cells[RowNumber, column + 5].Value = batchReportMaterials.Sum(p => p.WorkRecycle);
            ExcelPage.Cells[RowNumber, column + 5].Style.Numberformat.Format = FLOAT3_FORMAT;
            ExcelPage.Cells[RowNumber, column + 6].Value = batchReportMaterials.Sum(p => p.Work);
            ExcelPage.Cells[RowNumber, column + 6].Style.Numberformat.Format = FLOAT3_FORMAT;
            ExcelPage.Cells[RowNumber, column + 8].Value = batchReportMaterials.Sum(p => p.Real);
            ExcelPage.Cells[RowNumber, column + 8].Style.Numberformat.Format = FLOAT3_FORMAT;
            ExcelPage.Cells[RowNumber, column + 9].Value = batchReportMaterials.Sum(p => p.RealCubWet);
            ExcelPage.Cells[RowNumber, column + 9].Style.Numberformat.Format = FLOAT3_FORMAT;
            ExcelPage.Cells[RowNumber, column + 10].Value = batchReportMaterials.Sum(p => p.RealCubDry);
            ExcelPage.Cells[RowNumber, column + 10].Style.Numberformat.Format = FLOAT3_FORMAT;
            ExcelPage.Cells[RowNumber, column + 11].Value = batchReportMaterials.Sum(p => p.RealCubKoef);
            ExcelPage.Cells[RowNumber, column + 11].Style.Numberformat.Format = FLOAT3_FORMAT;
            RowNumber++;
        }
    }
}
