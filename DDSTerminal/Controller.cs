using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace DDSTerminal
{
    public class Controller : MarshalByRefObject
    {
        ConsoleWindow window;
        USBVisaDDS dds;
        TCPListenerWrapper tcp;


        public Controller()
        {
            window = new ConsoleWindow(this);
            tcp = new TCPListenerWrapper(this);
        }

        public void Start()
        {
            Application.Run(window);
        }

        
        public void Quit()
        {
            DisconnectDDS();
            DisactivateRemoting();
            Application.Exit();
        }

        public void WriteToConsole(string text)
        {
            window.WriteToConsole(text);
        }


        #region DDS
        public void ConnectToDDS()
        {
            window.WriteToConsole("Connecting...");
            dds = new USBVisaDDS("ASRL4::INSTR");
            dds.Connect();
            window.WriteToConsole("Connected.");


        }

        public void DispatchCommandToDDS(string text)
        {
            window.WriteToConsole("Writing \"" + text + "\" to device.");
            try
            {
                dds.SetFrequency(Double.Parse(text));
            }
            catch (Exception e)
            {
                dds.SetFrequency(Double.Parse(text.Substring(0, text.Length - 1)), text.Substring(text.Length - 1));
            }
            window.WriteToConsole("Complete.");
        }

        public void DisconnectDDS()
        {
            window.WriteToConsole("Disconnecting...");
            dds.Disconnect();
            window.WriteToConsole("Disconnected.");
        }
        #endregion
      
        #region Remoting
        internal void ActivateRemoting()
        {
            //Start listening to TCP
            tcp.Start(1791);
        }

        internal void DisactivateRemoting()
        {
            tcp.StopTCPServer();
        }
        #endregion
    }
}