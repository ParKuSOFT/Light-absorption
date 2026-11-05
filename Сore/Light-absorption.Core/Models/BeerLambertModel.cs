using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Light_absorption.Core.Models
{
    public class BeerLambertModel
    {
        public const double AlphaR = 0.8; 
        public const double AlphaG = 0.3; 
        public const double AlphaB = 0.1;
        public static double CalculateIntensity(double alpha, double z)
        {
            return Math.Exp(-alpha * z);
        }
    }
}
