namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using BSU.API.Data.Recipes;
    using BSU.API.Data.Reporting;
    using BSU.Utils.Attributes;
    using global::SmartMix.Core.Common.Attributes;
    using global::SmartMix.Core.Domain.Entities;
    using global::SmartMix.Core.Domain.Enums;
    using System.Collections.Generic;
    using TTS.SmartMix.Export.DataExport.ExcelExport.Extensions;

    /// <summary>
    /// Генератор отчета <see cref="ReportType.Recipes"/>
    /// </summary>
    public class RecipeListExcelReport : ExcelReportBase
    {
        private readonly Recipe[] _recipes;
        private static Dictionary<ComponentType, string> _typeComponents = Helpers.ReportHelper.GetEnumValues<ComponentType, TextAttribute>();

        /// <inheritdoc/>
        public RecipeListExcelReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter)
        {
            _recipes = report as Recipe[];

            Description = "Рецепты";
            ReportName = "Рецепты";

            DictionaryColumnWidth = new Dictionary<int, double>()
            {
                {1, 4}, // Левая граница
                {2, 25}, //
                {3, 20}, //
                {4, 15}, //
                {5, 20}, //
                {6, 10}
            };
            WidthInCells = DictionaryColumnWidth.Count;

            IsLandscape = false;
            UseFooter = true;
        }

        protected override void GenerateReport()
        {
            for (int i = 0; i < _recipes.Length; i++)
            {
                GenerateHeader(_recipes[i], System.Drawing.Color.Gainsboro);
                RowNumber += 2;

                GenerateComponentHeader(4, System.Drawing.Color.Gainsboro);
                RowNumber += 1;

                GenerateComponent(_recipes[i], 4);
                RowNumber += 2;
            }
        }

        /// <summary>
        /// Формирует заголовок с базовой информацией по рецепту.
        /// </summary>
        /// <param name="recipe">Данные рецепта.</param>
        /// <param name="color">Цвет заголовков</param>
        private void GenerateHeader(Recipe recipe, System.Drawing.Color? color = null)
        {
            int column = 2;
            int startRow = RowNumber;

            ExcelPage.Cells[RowNumber, column].ApplyLongHeaderStyle().Value = "№";
            ExcelPage.Cells[RowNumber, column + 1].Value = recipe.Id;
            ExcelPage.Cells[RowNumber, column + 1, RowNumber, column + 2].Merge = true;

            ExcelPage.Cells[RowNumber, column + 3].ApplyLongHeaderStyle().Value = "Изменен:";
            ExcelPage.Cells[RowNumber, column + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            ExcelPage.Cells[RowNumber, column + 4].Value = recipe.EditDate;
            ExcelPage.Cells[RowNumber, column + 4].Style.Numberformat.Format = DATE_FORMAT;
            RowNumber++;

            ExcelPage.Cells[RowNumber, column].ApplyLongHeaderStyle().Value = "Название";
            ExcelPage.Cells[RowNumber, column + 1].Value = recipe.Name;
            ExcelPage.Cells[RowNumber, column + 1, RowNumber, column + 2].Merge = true;
            RowNumber++;

            ExcelPage.Cells[RowNumber, column].ApplyLongHeaderStyle().Value = "Категория";
            ExcelPage.Cells[RowNumber, column + 1].Value = recipe.RecipeCategory.Name;
            ExcelPage.Cells[RowNumber, column + 1, RowNumber, column + 2].Merge = true;
            RowNumber++;

            ExcelPage.Cells[RowNumber, column].ApplyLongHeaderStyle().Value = "Время перемешивания";
            ExcelPage.Cells[RowNumber, column + 1].Value = recipe.RecipeTimesSet.MixTime;
            ExcelPage.Cells[RowNumber, column + 2].Style.Numberformat.Format = INT_FORMAT;
            ExcelPage.Cells[RowNumber, column + 1, RowNumber, column + 2].Merge = true;

            ExcelPage.Cells[RowNumber, column + 3].ApplyLongHeaderStyle().Value = "Время выгрузки";
            ExcelPage.Cells[RowNumber, column + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            ExcelPage.Cells[RowNumber, column + 4].Value = recipe.RecipeMixerSet.TimeDischarge;
            ExcelPage.Cells[RowNumber, column + 4].Style.Numberformat.Format = INT_FORMAT;

            ExcelPage.Cells[startRow, column, RowNumber, column + 4].ApplyBorders(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            ExcelPage.Cells[startRow, column, RowNumber, column + 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            if (color.HasValue)
                ExcelPage.Cells[startRow, column, RowNumber, column].ApplyBackground(color.Value);
        }

        /// <summary>
        /// Формирует заголовок таблицы компонентов.
        /// </summary>
        /// <param name="column">Начальный номер колонки.</param>
        private void GenerateComponentHeader(int column, System.Drawing.Color color)
        {
            ExcelPage.Cells[RowNumber, column].ApplyHeaderStyle().ApplyBackground(color).Value = "Тип компонента";
            ExcelPage.Cells[RowNumber, column + 1].ApplyHeaderStyle().ApplyBackground(color).Value = "Компонент";
            ExcelPage.Cells[RowNumber, column + 2].ApplyHeaderStyle().ApplyBackground(color).Value = "Вес, кг"; //ExcelHorizontalAlignment.Right

            ExcelPage.Cells[RowNumber, column, RowNumber, column + 2].ApplyBorders(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
        }

        /// <summary>
        /// Выводит информацию по компоненту
        /// </summary>
        /// <param name="recipe">Рецепт</param>
        /// <param name="column">Начальный номер колонки.</param>
        private void GenerateComponent(Recipe recipe, int column)
        {
            int startRow = RowNumber;

            foreach (RecipeStructure item in recipe.Structures)
            {
                ExcelPage.Cells[RowNumber, column].Value = _typeComponents[(ComponentType)item.Component.IdType];
                ExcelPage.Cells[RowNumber, column + 1].Value = item.Component.Name;

                ExcelPage.Cells[RowNumber, column + 2].Value = item.Amount;
                ExcelPage.Cells[RowNumber, column + 2].Style.Numberformat.Format = FLOAT_FORMAT;

                RowNumber++;
            }
            ExcelPage.Cells[startRow, column, RowNumber - 1, column + 2].ApplyBorders(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

            // разделительная линия
            ExcelPage.Cells[RowNumber, 2, RowNumber, WidthInCells].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
        }
    }
}