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

        public string SetFrequency(double frequency)
        {
            string command = freqToCommand(frequency);
            Write(command);
            return command;
        }

        string freqToCommand(double frequency)
        {
            int factor = (int)Math.Round(Math.Pow(2, 32) / 300000000);
            int commandFreq = (int)(frequency * factor);
            char w0, w1, w2, w3;
            w0 = (char)(commandFreq / Math.Pow(256, 3));
            w1 = (char)(commandFreq / Math.Pow(256, 2));
            w2 = (char)(commandFreq / Math.Pow(256, 1));
            w3 = (char)(commandFreq / Math.Pow(256, 0));

            return (w3 + w2 + w1 + w0).ToString();
        }
    }
}
