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
using Extentions;


namespace MolBase_Client
{
    public partial class Form1 : Form
    {
        // Текущий пользователь
        private static CurrentUser _CurUser;
        public static CurrentUser CurUser
        {
            get { return _CurUser; }
            set
            {
                _CurUser = value;
                ServerCommunication.UserLogin = _CurUser == null ? "" : _CurUser.Login;
                ServerCommunication.UserSecureCode = _CurUser == null ? "" : _CurUser.SessionCode;
            }
        }

        public static Statuses Known_Statuses;

        public Form1()
        {
            InitializeComponent();

            Application.ApplicationExit += OnApplicationExit;

            Login_Show();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            ServerCommunication.Send_Get_Msg_To_Server(ServerCommunication.Commands.QuitMsg);
        }

        private void Login_Show()
        {
            LoginForm LF = new LoginForm();
            CurUser = LF.LoginShow();

            if (CurUser == null) return;
            switch (CurUser.Special)
            {
                case 0: //Статус: пользователь
                    button7.Visible = true;     // Добавить структуру
                    button6.Visible = true;     // Поиск по структуре
                    button1.Visible = false;    // Консоль для прямых команд серверу
                    button4.Visible = false;    // Показ новых заявок
                    switch (CurUser.ShowMol)
                    {
                        case 0:
                            button9.Visible = false;    // Редактирование списка пользователей
                            break;
                        case 1: 
                        case 2:
                            button9.Visible = true;    // Редактирование списка пользователей
                            break;
                        default:
                            button9.Visible = false;    // Редактирование списка пользователей
                            break;
                    }
                    break;
                case 2: //Статус: глобальный админ
                    button7.Visible = true;     // Добавить структуру
                    button6.Visible = true;     // Поиск по структуре
                    button1.Visible = true;     // Консоль для прямых команд серверу
                    button4.Visible = true;     // Показ новых заявок
                    button9.Visible = true;     // Редактирование списка пользователей
                    break;
                case 1: //Статус: управляющий
                    button7.Visible = true;     // Добавить структуру
                    button6.Visible = true;     // Поиск по структуре
                    button1.Visible = false;    // Консоль для прямых команд серверу
                    button4.Visible = true;     // Показ новых заявок
                    button9.Visible = true;     // Редактирование списка пользователей
                    break;
                default:    // Статус: другое == обычный пользователь
                    button7.Visible = true;     // Добавить структуру
                    button6.Visible = true;     // Поиск по структуре
                    button1.Visible = false;    // Консоль для прямых команд серверу
                    button4.Visible = false;    // Показ новых заявок
                    button9.Visible = false;    // Редактирование списка пользователей
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

            label1.Text = "Здравствуйте, " + CurUser.FullNameSurname() + ".";
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

        private void button2_Click(object sender, EventArgs e)
        {           
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConsoleForm CF = new ConsoleForm();
            CF.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Visible = false;
            ServerCommunication.Send_Get_Msg_To_Server(ServerCommunication.Commands.QuitMsg);
            Login_Show();
            Visible = true;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            Get_Molecule_List("molecules.search", "my");
        }

        private static void Get_Molecule_List(string Message, string Parameters = "")
        {
            // Запрашиваем сервер и получаем ответ
            List<string> Answer = ServerCommunication.Send_Get_Msg_To_Server(Message, Parameters);

            // Преобразуем ответ в список молекул
            // И покажем окно со списком.
            MoleculesList ML = new MoleculesList(Functions.GetMolListFromServerAnswer(Answer));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Get_Molecule_List(ServerCommunication.Commands.Show_New_Mol, 
                ServerCommunication.Commands.Show_New_Mol_Param);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            UserList.Show(this);
        }
    }
}
