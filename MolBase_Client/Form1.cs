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
        static public string Search_Mol = "<@Search_Molecule@>";

        const string Add_User = "<@Add_User@>";
        public const string Login = "<@Login_User@>";
        public const string LoginOK = "<@Login_OK@>";
        public const string NoLogin = "<Error 100: No such loged in user>";
        public const string Add_Mol = "<@Add_Molecule@>";
        public const string Answer_Admin = "AdminOK";

        // Текущий пользователь
        static string UserName = "NoUser";
        static string UserID = "NoUserID";
        static int ID = 0;
        static string UserFullName = "";

        public static Statuses Known_Statuses;

        public Form1()
        {
            InitializeComponent();

            LoginForm LF = new LoginForm();
            int Status = LF.LoginShow();

            switch (Status)
            {
                case 0: //Статус: пользователь
                    button7.Visible = true;     // Добавить структуру
                    button6.Visible = true;     // Поиск по структуре
                    button1.Visible = false;    // Консоль для прямых команд серверу
                    break;
                case 1: //Статус: глобальный админ
                    button7.Visible = true;     // Добавить структуру
                    button6.Visible = true;     // Поиск по структуре
                    button1.Visible = true;     // Консоль для прямых команд серверу
                    break;
                default:    // Статус: другое == обычный пользователь
                    button7.Visible = true;     // Добавить структуру
                    button6.Visible = true;     // Поиск по структуре
                    button1.Visible = false;    // Консоль для прямых команд серверу
                    break;
            }

            Known_Statuses = new Statuses();

            if (!Directory.Exists(Path.GetTempPath()+"MolBase"))
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

            label1.Text = "Здравствуйте, " + GetFullName() + ".";
        }

        public static string TempFile()
        {
            Random rnd = new Random();
            return Path.GetTempPath() + "MolBase\\MolBase" + rnd.Next(1000000, 9999999).ToString() + ".tmp";
        }

        static public List<string> Send_Get_Msg_To_Server(string Command, string Parameters, byte[] BinarInfo = null)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[1024];
            int port = 11000;
            IPAddress ipAddr = IPAddress.Parse("195.19.140.174");
            List<string> Res;

            // Соединяемся с удаленным устройством
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            senderSocket.Connect(ipEndPoint);

            byte[] msg = Encoding.UTF8.GetBytes(Command + "\n" + UserName + "\n" + UserID + "\n" + Parameters + " ");

            // Отправляем данные через сокет
            byte[] msg_size = BitConverter.GetBytes(msg.Length);
            int bytesSentS = senderSocket.Send(msg_size);
            int bytesSent = senderSocket.Send(msg);
            if (BinarInfo != null) senderSocket.Send(BinarInfo);

            // Получаем ответ от сервера
            Res = Get_ListString_from_bytes(bytes, senderSocket);

            // Освобождаем сокет
            senderSocket.Shutdown(SocketShutdown.Both);
            senderSocket.Close();

            if (Res[1] == NoLogin)
            {
                UserID = "NoUserID";
                UserName = "NoUser";
            }

            return Res;
        }

        private static List<string> Get_ListString_from_bytes(byte[] bytes, Socket senderSocket)
        {
            List<string> Res = new List<string>();
            bool Stop = false;
            do
            {
                int bytesRec = senderSocket.Receive(bytes);
                string ResList = Encoding.UTF8.GetString(bytes, 0, bytesRec);
                Res.AddRange( Parce_StringList(ref Stop, ResList));
            }
            while (!Stop);

            return Res;
        }

        private static List<string> Parce_StringList(ref bool Stop, string ResList)
        {
            string ResMsg = "";
            List<string> Res = new List<string>();
            string[] ResArray = ResList.Split("\n"[0]);
            for (int i = 0; i < ResArray.Count(); i++)
            {
                ResMsg = ResArray[i];
                if (ResMsg == "") { continue; };
                if (ResMsg == "<@None@>") { Res.Add(""); continue; };
                Res.Add(ResMsg);
                if (ResMsg == EndMsg) { Stop = true; };
            }
            return Res;
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

        public static void SetUserID(string UID)
        {
            UserID = UID;
        }

        public static void SetLogin(string login)
        {
            UserName = login;
        }

        public static void SetFullName(string FullName)
        {
            UserFullName = FullName;
        }

        public static string GetFullName()
        {
            return UserFullName;
        }

        public static void SetID(int _ID)
        {
            ID = _ID;
        }

        public static int GetID()
        {
            return ID;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConsoleForm CF = new ConsoleForm();
            CF.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //List<string> F_Size = Send_Get_Msg_To_Server("<@*Send_File_Size*@>", "");


            int port = 11000;
            IPAddress ipAddr = IPAddress.Parse("195.19.140.174");

            // Соединяемся с удаленным устройством
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            senderSocket.Connect(ipEndPoint);

            
            byte[] msg = Encoding.UTF8.GetBytes("<@*Send_File*@>" + "\n" + UserName + "\n" + UserID + "\n" + " ");

            // Отправляем данные через сокет
            int bytesSent = senderSocket.Send(msg);

            // Получаем длину текстовой записи
            byte[] SL_Size = new byte[4];
            senderSocket.Receive(SL_Size);
            int StringList_Size = BitConverter.ToInt32(SL_Size, 0);

            // Получаем текстовую запись
            byte[] SL = new byte[StringList_Size];
            senderSocket.Receive(SL);
            bool Stop = false;
            List<string> F_Size = Parce_StringList(ref Stop, Encoding.UTF8.GetString(SL, 0, StringList_Size));
                
            byte[] bytes = new byte[Convert.ToInt64(F_Size[2])];
            int bytesRec = senderSocket.Receive(bytes);

            // Записываем всё в файл
            string FileName = F_Size[1];
            FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write);
            fs.Write(bytes, 0, bytes.Count());
            fs.Flush();
            fs.Close();

            // Освобождаем сокет
            senderSocket.Shutdown(SocketShutdown.Both);
            senderSocket.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string FileName = "TestToSend.doc";
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Flush();
            fs.Close();

            Send_Get_Msg_To_Server("<@*Get_File*@>", FileName + "\n" + data.Length.ToString(), data);
        }
    }
}
