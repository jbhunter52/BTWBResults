using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTWBResults
{
    class MyMapper : nzy3D.Plot3D.Builder.Mapper
    {
        public double[] coeff;
        public override double f(double x, double y)
        {
            //double a = -5.829;
            //double b = -2.795;
            //double c = 103.2;
            //double d = -39.11;
            double a = coeff[0];
            double b = coeff[1];
            double c = coeff[2];

            return a* Math.Log(x) + b*Math.Log(y) + c;
            //return 10 * Math.Sin(x / 10) * Math.Cos(y / 20) * x;
        }

    }
}
