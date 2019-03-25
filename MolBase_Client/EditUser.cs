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
    public partial class EditUser : Form
    {
        int status = 0; // 0 - ERROR, 1 - ADD, 2 - EDIT
        Dictionary<int, string> LabsID;
        bool FormAnswer = false;
        User OldInfo;

        public EditUser()
        {
            InitializeComponent();
        }

        public static bool Add(IWin32Window Owner)
        {
            // Создаём новое окно
            EditUser Form = new EditUser();
            Form.status = 1;

            // Правим тексты под добавление пользователя
            Form.Text = "Добавление нового пользователя";
            Form.button1.Text = "Добавить";

            // Получаем список лабораторий
            GetLabs(Form);

            // Скрыть кнопку изменения пароля
            Form.ChangePasswordBtn.Visible = false;

            // Показываем окно
            Form.ShowDialog(Owner);

            // Если всё плохо – возвращаем false
            return Form.FormAnswer;
        }

        private static void GetLabs(EditUser Form)
        {
            Form.comboBox1.Items.Clear();
            Form.LabsID = new Dictionary<int, string>();

            List<string> Labs = ServerCommunication.Send_Get_Msg_To_Server("laboratories.names");
            if (Labs.Count < 3) { Form.comboBox1.Items.Add("Нет лабораторий в базе данных"); return; }

            for (int i = 1; i < Labs.Count-1; i++) //Игнорируем первую и последнюю записи
            {
                string[] Val = Labs[i].Split('=');
                Form.comboBox1.Items.Add(Val[1]);
                Form.LabsID.Add(Convert.ToInt32(Val[0]), Val[1]);
            }
 
        }

        public static bool Edit(IWin32Window Owner, int UserID)
        {
            // Получаем список пользователей.
            List<User> Users = Functions.GetUserList("id " + UserID.ToString());
            if (Users.Count < 1) return false;

            // Создаём новое окно
            EditUser Form = new EditUser();
            Form.OldInfo = Users[0];
            Form.status = 2;

            // Правим тексты под добавление пользователя
            Form.Text = "Редактирование информации о пользователе";
            Form.button1.Text = "Изменить";

            // Получаем список лабораторий
            GetLabs(Form);

            // Заполняем данные
            Form.textBox1.Text = Form.OldInfo.Surname;
            Form.textBox2.Text = Form.OldInfo.Name;
            Form.textBox3.Text = Form.OldInfo.SecondName;
            Form.textBox6.Text = Form.OldInfo.Login;
            Form.textBox6.Enabled = false;
            Form.textBox7.Text = Form.OldInfo.Job;
            Form.textBox5.Visible = false;
            Form.textBox4.Visible = false;
            Form.label5.Visible = false;
            Form.ChangePasswordBtn.Enabled = Form1.CurUser.Special > 0;
            Form.comboBox1.SelectedItem = Form.OldInfo.Lab;


            // Показываем окно
            Form.ShowDialog(Owner);

            // Если всё плохо – возвращаем false
            return Form.FormAnswer;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormAnswer = false;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Проверим, что всё, что нужно, введено
            if (textBox1.Text == "") { MessageBox.Show("Введите фамилию пользователя","Ошибка"); return; }
            if (textBox2.Text == "") { MessageBox.Show("Введите имя пользователя", "Ошибка"); return; }
            if (status == 1)
            {
                if (textBox5.Text == "") { MessageBox.Show("Введите или сгенерируйте пароль", "Ошибка"); return; }
                if (textBox4.Text == "") { MessageBox.Show("Введите подтверждение пароля", "Ошибка"); return; }
            }
            if (textBox6.Text == "") { MessageBox.Show("Введите логин пользователя", "Ошибка"); return; }

            switch (status)
            {
                case 1:
                    List<string> Answer = ServerCommunication.Send_Get_Msg_To_Server("users.add",
                    "name " + textBox2.Text +
                    "\nsecond_name " + textBox3.Text +
                    "\nsurname " + textBox1.Text +
                    "\nlogin " + textBox6.Text +
                    "\npassword " + textBox5.Text +
                    "\nconfirm " + textBox4.Text +
                    "\npermissions 1" +
                    "\njob " + textBox7.Text +
                    "\nlaboratory_id " + LabsID[comboBox1.SelectedIndex].ToString());
                    if (Answer[1] == "User added")
                    {
                        FormAnswer = true;
                        Close();
                    }
                    else MessageBox.Show(Answer[1], "Ошибка добавления пользователя");
                    break;
                case 2:
                    string Params = "";
                    if (textBox1.Text != OldInfo.Surname) Params += textBox1.Text != "" 
                            ? "\nsurname " + textBox1.Text
                            : "\nsurname {CLEAR}";
                    if (textBox2.Text != OldInfo.Name) Params += textBox2.Text != ""
                            ? "\nname " + textBox2.Text
                            : "\nname {CLEAR}";
                    if (textBox3.Text != OldInfo.SecondName) Params += textBox3.Text != ""
                            ? "\nsecond_name " + textBox3.Text
                            : "\nsecond_name {CLEAR}";
                    if (comboBox1.SelectedItem.ToString() != OldInfo.Lab)
                    {
                        int LabID = LabsID.Where(x => x.Value == comboBox1.SelectedItem.ToString()).FirstOrDefault().Key;

                        Params += "\nlaboratory_id " +
                            LabID.ToString();
                    }
                    if (textBox7.Text != OldInfo.Job) Params += textBox7.Text != ""
                            ? "\njob " + textBox7.Text
                            : "\njob {CLEAR}";

                    if (Params == "")
                    {
                        FormAnswer = true;
                        Close();
                        return;
                    }
                    Params += "\nlogin " + textBox6.Text;

                    List<string> AnswerEdit = ServerCommunication.Send_Get_Msg_To_Server("users.update", Params);
                    if (AnswerEdit[1] == "User information updated")
                    {
                        FormAnswer = true;
                        Close();
                    }
                    else MessageBox.Show(AnswerEdit[1], "Ошибка добавления пользователя");
                    break;
            }
        }
    }
}
