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
        int Status = 0;
        bool OK_Close = false;

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
            List<string> Ans = Form1.Send_Get_Msg_To_Server(Form1.Login, textBox1.Text + "\n" + textBox2.Text);

            if (Ans[0] != Form1.StartMsg)
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

            if (Ans[1] != Form1.LoginOK)
            {
                MessageBox.Show("Ошибка 3 – Неожиданный ответ сервера: «" + Ans[1] + "»",
                "Ошибка ответа сервера");
                return;
            };

            if (Ans[5] == Form1.Answer_Admin)
            {
                Status = 1;
            }

            Form1.SetUserID( Ans[2] );
            Form1.SetLogin( textBox1.Text );
            Form1.SetID(Convert.ToInt32(Ans[3]));
            Form1.SetFullName(Ans[4]);
            OK_Close = true;
            Close();
        }

        public int LoginShow()
        {
            Status = 0;
            ShowDialog();
            return Status;
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!OK_Close) Application.Exit();
        }
    }
}
