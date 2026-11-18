using System.IO;
using System.Collections.Generic;
using Light_absorption.Core.Models;
using Light_absorption.Core.Export;

namespace Light_absorption.Core.Services
{
    public class ExportService
    {
        private readonly IExporter _exporter;

        public ExportService(IExporter exporter)
        {
            _exporter = exporter;
        }

        public void SaveExperimentResults(List<ExperimentState> history, double dz, string filePath)
        {
            string csvText = _exporter.ExportToCsv(history, dz);

            File.WriteAllText(filePath, csvText);
        }
    }
}
