using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Light_absorption.Core.Models;

namespace Light_absorption.Core.Export
{
    public class CsvExporter
    {
        public void ExportToCsv(List<ExperimentState> history, double dz, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("z,R,G,B");

            for (int i = 0; i < history.Count; i++)
            {
                double z = i * dz;
                var state = history[i];


                sb.AppendLine($"{z.ToString(System.Globalization.CultureInfo.InvariantCulture)},{state.Values[0].ToString(System.Globalization.CultureInfo.InvariantCulture)},{state.Values[1].ToString(System.Globalization.CultureInfo.InvariantCulture)},{state.Values[2].ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            }

            // Сохраняем файл
            File.WriteAllText(filePath, sb.ToString());
        }
    }

}