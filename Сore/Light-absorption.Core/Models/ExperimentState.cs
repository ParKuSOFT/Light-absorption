using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Light_absorption.Core.Models
{
    public class ExperimentState
    {
        public double[] Values { get; set; }

        public int StateSize => Values.Length;

        public ExperimentState(double[] values)
        {
            Values = values;
        }

        
        public ExperimentState Clone()
        {
            double[] clonedValues = (double[])this.Values.Clone();
            return new ExperimentState(clonedValues);
        }
    }
}
