using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MolBase_Client
{
    public partial class LoginForm : Form
    {
        bool OK_Close = false;
        CurrentUser CU = null;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*Login*/
            List<string> Ans = ServerCommunication.Send_Get_Msg_To_Server(ServerCommunication.Commands.Login, 
                "name " + textBox1.Text + "\npassword " + textBox2.Text);

            if (Ans[0] != ServerCommunication.Answers.StartMsg)
            {
                MessageBox.Show("Ошибка 3 – Неожиданный ответ сервера: «" + Ans[0] + "»",
                "Ошибка ответа сервера");
                return;
            };

            const string NoUser = "NoUserID";

            if (Ans[2] == NoUser)
            {
                MessageBox.Show("Ошибка: такие имя пользователя и пароль не зарегистрированы в системе.\n\n" +
                    "Проверьте правильность заполнения, язык ввода и клавиши \"Num Lock\" и \"Caps Lock\"",
                "Ошибка входа");
                return;
            }

            if (Ans[1] != ServerCommunication.Answers.LoginOK)
            {
                MessageBox.Show("Ошибка 3 – Неожиданный ответ сервера: «" + Ans[1] + "»",
                "Ошибка ответа сервера");
                return;
            };

            CU = new CurrentUser(textBox1.Text);
            CU.Name = Ans[4];
            CU.SecondName = Ans[5];
            CU.Surname = Ans[6];
            CU.ID = Convert.ToInt32(Ans[3]);
            CU.SessionCode = Ans[2];
            CU.SetRights(Ans[7]);
            OK_Close = true;

            Close();
        }

        public CurrentUser LoginShow()
        {
            ShowDialog();
            return CU;
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!OK_Close) Application.Exit();
        }
    }
}
