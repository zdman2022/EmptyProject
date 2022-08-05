using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager
{
    internal class InstanceInstantiator
    {
        internal static object Instantiate(string sourceClassType)
        {
            if(sourceClassType == "FlatRedBall.Math.Geometry.Circle")
            {
                return new FlatRedBall.Math.Geometry.Circle();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
