using SmartMix.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TTS.SmartMix.Export.DataExport.ExcelExport
{
    public class TransportWayBillReport : ExcelReportBase
    {
        /// <summary>
        /// Представляет данные по заявкам.
        /// </summary>
        private readonly ApplicationReport[] _reportData;

        private Dictionary<string, string> _patterns;

        /// <summary>
        /// Транспортная накладная
        /// </summary>
        /// <param name="lineNumber">Номер линии</param>
        /// <param name="ids">Id-шники заявок</param>
        public TransportWayBillReport(int lineNumber, ApplicationFilter appFilter, string firmName, object report)
            : base(lineNumber, firmName, appFilter)
        {
            _reportData = report as ApplicationReport[];
            ReportName = "Транспортная накладная";
            Description = "Транспортная накладная";
            WidthInCells = 5;
        }

        protected override void GenerateReport()
        {
            if (_reportData.Length == 0) return;

            try
            {
                InitPatterns();
                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                string[] names = currentAssembly.GetManifestResourceNames();
                string ttnPathName = names.FirstOrDefault(name => name.EndsWith("TtnBlank.xlsx"));

                if (string.IsNullOrEmpty(ttnPathName))
                {
                    Logger.LogManager.Instance.WriteWarning("Ресурс TtnBlank.xlsx не найден в сборке");
                    return;
                }

                ExcelPage.Workbook.Properties.Created = DateTime.Now;
                Stream stream = currentAssembly.GetManifestResourceStream(ttnPathName);
                ChangeStream(stream);
                ExcelPage.Workbook.Properties.Created = DateTime.Now;
                ExcelPage.Workbook.Properties.Author = "SmartMix 2";
                ExcelPage.Workbook.Properties.Title = "Товарно-транспортная накладная";
                ExcelFile.Workbook.Worksheets.Delete(ReportName);
                var worksheets = ExcelFile.Workbook.Worksheets;

                foreach (var ws in worksheets)
                {
                    foreach (var key in _patterns.Keys)
                    {
                        var excelcells = ws.Cells.Select(cc => cc).Where(c => Convert.ToString(c.Value) == key).ToArray();
                        if (excelcells.Any())
                            foreach (var excelcell in excelcells)
                            {
                                excelcell.Value = _patterns[key];
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogManager.Instance.WriteException(ex);
            }
        }

        private void InitPatterns()
        {
            _patterns = new Dictionary<string, string>();
            if (_reportData.Length == 0) return;

            var _applicationReport = _reportData.FirstOrDefault();

            _patterns.Add("$FirmName", "Тестовая фирма 456");
            _patterns.Add("$Client_Name", (_applicationReport.Client != null ? _applicationReport.Client.Name : ""));
            _patterns.Add("$Client_Address",
                (_applicationReport.Client != null ? _applicationReport.Client.Address : ""));
            _patterns.Add("$Application_Id", string.Format("{0}", _applicationReport.Id));

            var layers = _applicationReport.Layers.FirstOrDefault();

            _patterns.Add("$Application_Recipe_Name",
                (layers?.Recipe != null ? layers.Recipe.Name : ""));
            _patterns.Add("$Application_CompletedVolume", string.Format("{0} Куб.", _applicationReport.CompletedVolume));

            double sum = 0;
            if (_applicationReport.Batches != null && _applicationReport.Batches.Length > 0)
            {
                foreach (var batch in _applicationReport.Batches)
                {
                    if (batch.BatchMaterials != null && batch.BatchMaterials.Length > 0)
                    {
                        foreach (var material in batch.BatchMaterials)
                        {
                            sum += material.Real;
                        }
                    }
                }
            }

            string operatorName = _applicationReport?.CreatedBy?.Name;

            _patterns.Add("$Application_Batches_BatchMaterials_Real_Count",
                (sum.Equals(0) ? "" : string.Format("{0} кг", Math.Round(sum, 2))));
            _patterns.Add("$Application_Batches_BatchMaterials_Real_Count2",
                (sum.Equals(0) ? "" : string.Format("{0}", Math.Round(sum, 2))));
            _patterns.Add("$Application_OperatorName1",
                string.Format("оператор {0} (подпись)", operatorName));
            _patterns.Add("$Application_OperatorName2", string.Format("оператор {0}", operatorName));
            _patterns.Add("$Application_OperatorName3", operatorName);
            _patterns.Add("$Application_StartTime", _applicationReport.StartTime.ToShortDateString());
            _patterns.Add("$Application_EndTime", _applicationReport.EndTime.ToShortDateString());
            _patterns.Add("$Application_Car_CarNumber",
                (_applicationReport.Car != null ? _applicationReport.Car.Name : ""));
            _patterns.Add("$Application_Car_Driver",
                (_applicationReport.Car != null && !string.IsNullOrEmpty(_applicationReport.Car.Driver)
                    ? string.Format("водитель {0} (подпись)", _applicationReport.Car.Driver)
                    : ""));
            _patterns.Add("$Application_Car_Driver2",
                (_applicationReport.Car != null ? _applicationReport.Car.Driver : ""));
            _patterns.Add("$Application_Car_Model", (_applicationReport.Car != null ? _applicationReport.Car.Model : ""));
            _patterns.Add("$CurrentTime", DateTime.Now.ToShortDateString());
        }
    }
}
