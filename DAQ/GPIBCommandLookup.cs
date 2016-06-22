using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI_VISA_GPIB
{
    public static class GPIBCommandLookup
    {
        public static string Convert(string input)
        {
            string gpibCommand = "";
            switch (input)
            {
                case "identity":
                    gpibCommand = "*IDN?" + "\n";
                    break;
                default:
                    gpibCommand = "Sorry, command not found.";
                    break;
            }
            return gpibCommand;
        }

    }
}
