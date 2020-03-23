using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI3_Image_Feature_Transform
{
    public class Check
    {
        public Int32 X { get; set; }
        public Int32 Y { get; set; }
        public Boolean? Positive { get; set; }
        public Boolean? Brighter { get; set; }
        public Check(Int32 X, Int32 Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
