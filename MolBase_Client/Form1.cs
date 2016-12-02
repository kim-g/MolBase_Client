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
        static public string StartMsg = "<@Begin_Of_Session@>"; // Начало сессии передачи ответа сервера
        static public string EndMsg = "<@End_Of_Session@>";     // Конец сессии передачи ответа сервера
        static public string Search_Mol = "<@Search_Molecule@>";    // Команда поиска молекулы
        static private string IP_Server = "195.19.140.174";     // IP сервера

        const string Add_User = "<@Add_User@>";                 // Команда добавления пользователя
        public const string Login = "<@Login_User@>";           // Команда входа в систему
        public const string FN_msg = "<@GetFileName@>";         // Команда получения имени файла (не используется)
        public const string LoginOK = "<@Login_OK@>";           // Ответ сервера об успешном входе в систему
        public const string NoLogin = "<Error 100: No such loged in user>";     // Ответ сервера о том, что имя пользователя-пароль не найдены
        public const string Add_Mol = "<@Add_Molecule@>";       // Команда на добавление молекулы
        public const string Answer_Admin = "AdminOK";           // Ответ сервера, что пользователь является админом
        public const string Answer_Manager = "ManagerOK";           // Ответ сервера, что пользователь является управляющим
        public const string Show_My_mol = "<@Show my molecules@>";  // Команда показать все молекулы пользователя
        public const string Increase_Status = "<@Increase status@>"; // Увеличеть значение статуса соединения
        public const string Show_New_Mol = "<@Show new molecules@>";  // Команда показать все молекулы новые

        // Текущий пользователь
        static string UserName = "NoUser";
        static string UserID = "NoUserID";
        static int ID = 0;
        static string UserFullName = "";

        public static Statuses Known_Statuses;

        public Form1()
        {
            InitializeComponent();

            Login_Show();
        }

        private void Login_Show()
        {
            LoginForm LF = new LoginForm();
            int Status = LF.LoginShow();

            switch (Status)
            {
                case 0: //Статус: пользователь
                    button7.Visible = true;     // Добавить структуру
                    button6.Visible = true;     // Поиск по структуре
                    button1.Visible = false;    // Консоль для прямых команд серверу
                    button4.Visible = false;    // Показ новых заявок
                    break;
                case 1: //Статус: глобальный админ
                    button7.Visible = true;     // Добавить структуру
                    button6.Visible = true;     // Поиск по структуре
                    button1.Visible = true;     // Консоль для прямых команд серверу
                    button4.Visible = true;     // Показ новых заявок
                    break;
                case 2: //Статус: управляющий
                    button7.Visible = true;     // Добавить структуру
                    button6.Visible = true;     // Поиск по структуре
                    button1.Visible = false;    // Консоль для прямых команд серверу
                    button4.Visible = true;     // Показ новых заявок
                    break;
                default:    // Статус: другое == обычный пользователь
                    button7.Visible = true;     // Добавить структуру
                    button6.Visible = true;     // Поиск по структуре
                    button1.Visible = false;    // Консоль для прямых команд серверу
                    break;
            }

            Known_Statuses = new Statuses();

            if (!Directory.Exists(Path.GetTempPath() + "MolBase"))
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
                Directory.GetFiles(Path.GetTempPath() + "MolBase\\", "MolBase*.tmp");
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

        static public List<string> Send_Get_Msg_To_Server(string Command, string Parameters = "", byte[] BinarInfo = null)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[1024];
            int port = 11000;
            IPAddress ipAddr = IPAddress.Parse(IP_Server);
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
            if (BinarInfo != null)
            {
                MemoryStream ms = new MemoryStream(BinarInfo);
                for (int i = 0; i < BinarInfo.Count(); i += 1024)
                {
                    int Size = 1024;
                    if (BinarInfo.Count() - i < 1024) { Size = BinarInfo.Count() - i; }
                    byte[] Block = new byte[Size];
                    ms.Read(Block, 0, Size);
                    senderSocket.Send(Block);
                }
                //senderSocket.Send(BinarInfo);
            }

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

        public static string SaveFileAs(string FileName)
        {
            using (var sfd = new SaveFileDialog())
            {
                // Задаём нужные параметры
                string FF = Path.GetExtension(FileName);
                string FFName = "*" + FF;
                if (FF == ".doc") FFName = "Документ Microsoft Word 97-2003 (*.doc)";
                if (FF == ".docx") FFName = "Документ Microsoft Word (*.docx)";
                if (FF == ".xls") FFName = "Документ Microsoft Excel 97-2003 (*.xls)";
                if (FF == ".xlsx") FFName = "Документ Microsoft Excel (*.xlsx)";
                if (FF == ".pdf") FFName = "Документ Portable Document Format (*.pdf)";
                if (FF == ".jpg") FFName = "Изображение в формате JPEG (*.jpg)";
                if (FF == ".png") FFName = "Изображение в формате Portable Network Graphics (*.png)";
                sfd.Filter = FFName + "|*" + FF;
                sfd.FileName = FileName;
                sfd.AddExtension = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    return sfd.FileName;
                }
                return "<Cancel>";
            }
        }

        public static string OpenFile()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Все файлы (*.*)|*.*";
                ofd.AddExtension = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    return ofd.FileName;
                }
                return "<Cancel>";
            }
        }


        public static void GetFile(int ID, string InFileName="")
        {
            string GotFN = InFileName;
            if (InFileName == "")
            {
                List<string> FN = Send_Get_Msg_To_Server(FN_msg, ID.ToString());
                GotFN = FN[1];
            }
            
            // Спрашиваем, куда сохранить. Если отменяем, то отменяем полностью, не спрашивая сервер.
            string FileName = SaveFileAs(GotFN);
            if (FileName == "<Cancel>")
            {
                return;
            }     // Если пользователь отменил, то прекращаем всё.

            int port = 11000;
            IPAddress ipAddr = IPAddress.Parse(IP_Server);

            // Соединяемся с удаленным устройством
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            senderSocket.Connect(ipEndPoint);


            byte[] msg = Encoding.UTF8.GetBytes("<@*Send_File*@>" + "\n" + UserName + "\n" + UserID + "\n" +
                ID.ToString() + "\n" + " ");

            // Отправляем данные через сокет
            int bytesSentS = senderSocket.Send(BitConverter.GetBytes(msg.Length));
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

            // Получаем и записываем в файл по кусочкам
            FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write);

            int FtR_Size = Convert.ToInt32(F_Size[2]);
            for (int i = 0; i < FtR_Size; i += 1024)
            {
                int block;
                if (FtR_Size - i < 1024) { block = FtR_Size - i; }
                else { block = 1024; }
                byte[] buf = new byte[block];
                senderSocket.Receive(buf);
                fs.Write(buf, 0, block);
                fs.Flush();
            }
            fs.Close();

            // Освобождаем сокет
            senderSocket.Shutdown(SocketShutdown.Both);
            senderSocket.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Visible = false;
            Send_Get_Msg_To_Server("<@*Quit*@>");
            Login_Show();
            Visible = true;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            Get_Molecule_List(Show_My_mol);
        }

        private static void Get_Molecule_List(string Message)
        {
            // Запрашиваем сервер и получаем ответ
            List<string> Answer = Form1.Send_Get_Msg_To_Server(Message);

            // Преобразуем ответ в список молекул
            List<Molecule> Mols = Functions.GetMolListFromServerAnswer(Answer);

            // И покажем окно со списком.
            MoleculesList ML = new MoleculesList();
            ML.DrawList(Mols);
            ML.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Get_Molecule_List(Show_New_Mol);
        }
    }
}
