using Light_absorption.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Light_absorption.Core.Solvers
{
    public interface ISolver
    {
        List<ExperimentState> Solve(ExperimentState initialState, double maxZ, double dz);
    }
}
