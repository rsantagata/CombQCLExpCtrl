using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
    public class ScanParameters : Parameters
    {
        public int NumberOfPoints { get; set; }
        public int Sleep { get; set; }
        public string ScanParameterName { get; set; }
    }
}
