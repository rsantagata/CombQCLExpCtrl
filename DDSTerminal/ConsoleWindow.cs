using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DDSTerminal
{
    public partial class ConsoleWindow : Form
    {
        Controller controller;

        public ConsoleWindow(Controller c)
        {
            InitializeComponent();
            controller = c;
        }

        public void WriteToConsole(string text)
        {
            outputTextbox.Invoke(new writeToConsoleDelegate(writeToConsole), new object[] { text });
        }
        void writeToConsole(string text)
        {
            outputTextbox.AppendText(" >>  " + text + "\n");
        }
        private delegate void writeToConsoleDelegate(string text);

        private void clearConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            outputTextbox.Clear();
        }

        private void inputTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                controller.DispatchCommandToDDS(inputTextBox.Text);
                inputTextBox.Text = "";
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.Quit();
        }

        private void connectStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.ConnectToDDS();
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.DisconnectDDS();
        }
    }
}
