using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments.Visa;

namespace NI_VISA_GPIB
{
        public class GPIBInstrument
    { 
        GpibSession session;
        string address;

        public GPIBInstrument(String visaAddress)
        {
            this.address = visaAddress;
        }

        public string Connect()
        {
            session = new GpibSession(address);
            session.ReaddressingEnabled = true;
            session.IOProtocol = Ivi.Visa.IOProtocol.Normal;
            session.SendEndEnabled = true;
            session.TerminationCharacterEnabled = false;
            try
            {
                Ivi.Visa.StatusByteFlags flag = session.ReadStatusByte();
                return flag.ToString();
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }

        public void Disconnect()
        {
            session.Dispose();
        }

        public string Write(String command)
        {
            try
            {
                session.RawIO.Write(command);
                return "Complete.";
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }

        public string Read()
        {
            try
            {
                return session.RawIO.ReadString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
            
        }

        public string Read(int numChars)
        {
            try
            {
                return session.RawIO.ReadString(numChars);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        
    }
}

