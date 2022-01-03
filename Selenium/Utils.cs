using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium
{
    public static class Utils
    {
        public static void TryEval(string EvalExpression, out object Result)
        {
            try
            {
                Result = new Expression(EvalExpression).Evaluate();
            }
            catch (Exception ex)
            {
                Result = EvalExpression;
                Console.WriteLine(ex.Message);
            }
        }
    }
}
