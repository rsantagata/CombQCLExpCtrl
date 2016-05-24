using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;
using Ivi.Visa;


namespace DAQ
{
    public class USBVisaDDS
    {
        SerialSession serial;
        string physicalAddress;

        public USBVisaDDS(string physicalAddress)
        {
            this.physicalAddress = physicalAddress;
        }


        public void Connect()
        {
            serial = new SerialSession(physicalAddress);
            serial.BaudRate = 96000;
            serial.DataBits = 8;
            serial.StopBits = SerialStopBitsMode.One;
            serial.ReadTermination = SerialTerminationMethod.HighestBit;
            serial.Parity = SerialParity.None;
        }

        public void Disconnect()
        {
            serial.Dispose();
        }

        public void Write(string command)
        {
            serial.RawIO.Write(command);
        }

    }
}
