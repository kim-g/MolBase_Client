using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenBabel;
using System.Windows.Media.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;


namespace MolBase_Client
{
    public partial class Form1 : Form
    {
        static public string StartMsg = "<@Begin_Of_Session@>";
        static public string EndMsg = "<@End_Of_Session@>";
        static public string Search_Mol = "<@Search_Molecule@>\n";

        const string Add_User = "<@Add_User@>";
        public const string Add_Mol = "<@Add_Molecule@>";

        public Form1()
        {
            InitializeComponent();

            if(!Directory.Exists(Path.GetTempPath()+"MolBase"))
            {
                Directory.CreateDirectory(Path.GetTempPath() + "MolBase");
            }

            string[] fullfilesPath =
                Directory.GetFiles(Path.GetTempPath(), "MolBase*.tmp");
            foreach (string fileName in fullfilesPath)
            {
                try
                { File.Delete(fileName); }
                catch
                { }
            }
                
            fullfilesPath =
                Directory.GetFiles(Path.GetTempPath()+"MolBase\\", "MolBase*.tmp");
            foreach (string fileName in fullfilesPath)
            {
                try
                { File.Delete(fileName); }
                catch
                { }
            }
        }

        public static string TempFile()
        {
            Random rnd = new Random();
            return Path.GetTempPath() + "MolBase\\MolBase" + rnd.Next(1000000, 9999999).ToString() + ".tmp";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OBConversion obconv = new OBConversion();
            obconv.SetInFormat("cdx");
            OBMol mol = new OBMol();

            for (int i = 0; i <= 5; i++)
            {
                obconv.ReadFile(mol, @"CDX\\"+i.ToString()+".cdx");
                obconv.SetOutFormat("smi");
                mol.SetTitle("");
                obconv.WriteFile(mol, @"SMI\\" + i.ToString() + ".smi");
            };


            label1.Text ="Готово!";
        }

        static public List<string> Send_Get_Msg_To_Server(string Msg)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[1024];
            int port = 11000;
            IPAddress ipAddr = IPAddress.Parse("195.19.140.174");
            List<string> Res = new List<string>();

            // Соединяемся с удаленным устройством
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            senderSocket.Connect(ipEndPoint);

            byte[] msg = Encoding.UTF8.GetBytes(Msg + " ");

            // Отправляем данные через сокет
            int bytesSent = senderSocket.Send(msg);

            // Получаем ответ от сервера
            string ResMsg = "";
            bool Stop = false;
            do
            {
                int bytesRec = senderSocket.Receive(bytes);
                string ResList = Encoding.UTF8.GetString(bytes, 0, bytesRec);
                string[] ResArray = ResList.Split("\n"[0]);
                for (int i = 0; i < ResArray.Count(); i++)
                {
                    ResMsg = ResArray[i];
                    if (ResMsg == "") { continue; };
                    if (ResMsg == "<@None@>") { Res.Add(""); continue; };
                    Res.Add(ResMsg);
                    if (ResMsg == EndMsg) { Stop = true; };
                }
            }
            while (!Stop);

            // Освобождаем сокет
            senderSocket.Shutdown(SocketShutdown.Both);
            senderSocket.Close();

            return Res;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            /*Add_User*/
            Send_Get_Msg_To_Server(Add_User+"\n"+ textBox1.Text + "\n" + textBox2.Text);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            Answer.Lines = Send_Get_Msg_To_Server(textBox3.Text).ToArray();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SubStructureSearch SSS = new SubStructureSearch();
            SSS.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Add_Mol Add_Mol_Form = new Add_Mol();
            Add_Mol_Form.ShowDialog();
        }

    }
}
