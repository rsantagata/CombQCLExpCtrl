using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedCode
{
    public class JSONExport
    {

        public void ExportToFile(string path, string text)
        {
            System.IO.File.WriteAllText(path, text);
        }
    }
}