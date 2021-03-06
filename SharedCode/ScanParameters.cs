﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
    public class ScanParameters : Parameters
    {
        public double ScanStart { get; set; }
        public double ScanStop { get; set; }
        public int NumberOfPoints { get; set; }
        public double ScanHoldValue { get; set; }
        public int Sleep { get; set; }
        public string ScanParameterName { get; set; }
        public double[] ScanParameterValues { get; set; }
        public bool AcquireDataDuringScan { get; set; }
        public bool StopOnEOS { get; set; }
    }
}
