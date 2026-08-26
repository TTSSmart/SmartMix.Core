namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    /// <summary>
    /// Представляет базовый класс формирования отчета по дозированию компонентов по заявкам.
    /// </summary>
    public abstract class AppMaterialReportBase : ExcelReportBase
    {
        /// <summary>
        /// Представляет данные отчета.
        /// </summary>
        protected readonly ApplicationReport[] ReportData;

        /// <summary>
        /// Представляет [динамический] список используемых материалов по замесам.
        /// </summary>
        /// <remarks>Состав списка будет меняться в зависимости от выборки заявок.</remarks>
        private readonly List<MaterialReport> _materialList = new List<MaterialReport>();

        /// <inheritdoc/>
        protected AppMaterialReportBase(int lineNumber, string firmName, ApplicationFilter appFilter, object report) : base(lineNumber, firmName, appFilter)
        {
            ReportData = report as ApplicationReport[];

            Array.ForEach(ReportData, app => Array.ForEach(app.Batches, b => Array.ForEach(b.BatchMaterials, m =>
            {
                if (!_materialList.Any(x => x.Component.Id == m.idMaterial_Old))
                    _materialList.Add(new MaterialReport()
                    {
                        Component = new BSU.API.Models.BaseComponent()
                        {
                            Id = m.idMaterial_Old,
                            Name = m.Material,
                            Type = (ComponentType)m.Type
                        }
                    });
            })));
            _materialList = ReportHelper.Sort(_materialList); // чтобы новые компоненты вставали в конец списка

            ReportName = "Паспорт дозирования компонентов смеси";
            Description = "Отчет по дозированию заявок";
            IsLandscape = true;
            UseFooter = true;
        }

        /// <summary>
        /// Возвращает уникальный список используемых компонентов для выбранного набора заявок.
        /// </summary>
        internal List<MaterialReport> MaterialList
        {
            get
            {
                return _materialList;
            }
        }

        /// <summary>
        /// Выполняет поиск компонента с идентификатором <paramref name="componentId"/> в структуре рецепта <paramref name="structureList"/> и возвращает планируемую массу компонента, в килограммах.
        /// </summary>
        /// <param name="componentId">ID компонента.</param>
        /// <param name="volume">Планируемый объём бетона, м3.</param>
        /// <param name="structureList">Структура рецепта.</param>
        /// <returns>Планируемая масса компонента, в килограммах на указанный объём бетона, если компонент задействован в рецепте, иначе - значение <see langword="null"/>.</returns>
        protected static float? GetRecipeWeight(int componentId, float volume, List<RecipeStructure> structureList)
        {
            foreach (RecipeStructure rs in structureList) // todo проверить, что нельзя задублировать компоненты
            {
                if (rs.Component.Id == componentId)
                    return rs.Amount * volume;
            }

            return null;
        }

        /// <summary>
        /// Выполняет поиск компонента с идентификатором <paramref name="componentId"/> в наборе замесов <paramref name="data"/> и возвращает фактическую массу отдозированного компонента, в килограммах.
        /// </summary>
        /// <param name="componentId">ID компонента</param>
        /// <param name="data">Набор замесов по слою.</param>
        /// <returns>Суммарная фактическая масса отдозированного компонента, в килограммах.</returns>
        protected static double GetBatchWeight(int componentId, BatchReport[] data)
        {
            double total = 0;

            foreach (BatchReport batch in data)
            {
                foreach (BatchReportMaterial batchMaterial in batch.BatchMaterials)
                {
                    if (batchMaterial.idMaterial_Old == componentId)
                        total += batchMaterial.Real;
                }
            }

            return total;
        }
    }
}
