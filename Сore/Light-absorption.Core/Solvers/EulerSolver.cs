using Light_absorption.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Light_absorption.Core.Solvers
{
    public class EulerSolver
    {
        public List<ExperimentState> Solve(ExperimentState initialState, double maxZ, double dz)
        {
            var history = new List<ExperimentState>();
            history.Add(initialState.Clone());

            double[] currentValues = (double[])initialState.Values.Clone();
            double[] alphas = { BeerLambertModel.AlphaR, BeerLambertModel.AlphaG, BeerLambertModel.AlphaB };

            double currentZ = 0;

            while (currentZ < maxZ)
            {
                for (int i = 0; i < currentValues.Length; i++)
                {
                    double dI = -alphas[i] * currentValues[i];
                    currentValues[i] = currentValues[i] + dz * dI;
                }

                currentZ += dz;
                history.Add(new ExperimentState((double[])currentValues.Clone()));
            }

            return history;
        }
    }
}
