using Light_absorption.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Light_absorption.Core.Export
{
    public interface IExporter
    {
        string ExportToCsv(List<ExperimentState> history, double dz);
    }
}
