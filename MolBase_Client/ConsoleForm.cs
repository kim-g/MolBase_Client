﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MolBase_Client
{
    public partial class ConsoleForm : Form
    {
        public ConsoleForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> Ans = Form1.Send_Get_Msg_To_Server(textBox1.Text, textBox2.Text);
            Ans.RemoveAt(0);
            Ans.RemoveAt(Ans.Count - 1);

            Answer.Lines = Ans.ToArray();
        }
    }
}
