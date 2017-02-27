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

        public EditUser()
        {
            InitializeComponent();
        }

        public static bool Add()
        {
            // Создаём новое окно
            EditUser Form = new EditUser();
            Form.status = 1;

            // Правим тексты под добавление пользователя
            Form.Text = "Добавление нового пользователя";
            Form.button1.Text = "Добавить";

            // Показываем окно
            Form.ShowDialog();

            // Если всё плохо – возвращаем false
            return false;
        }

        public static bool Edit(int UserID)
        {
            // Создаём новое окно
            EditUser Form = new EditUser();
            Form.status = 2;

            // Правим тексты под добавление пользователя
            Form.Text = "Редактирование информации о пользователе";
            Form.button1.Text = "Изменить";

            // Показываем окно
            Form.ShowDialog();

            // Если всё плохо – возвращаем false
            return false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Проверим, что всё, что нужно, введено
            if (textBox1.Text == "") { MessageBox.Show("Введите фамилию пользователя","Ошибка"); return; }
            if (textBox2.Text == "") { MessageBox.Show("Введите имя пользователя", "Ошибка"); return; }
            if (status == 2)
            {
                if (textBox4.Text == "") { MessageBox.Show("Введите подтверждение пароля", "Ошибка"); return; }
                if (textBox5.Text == "") { MessageBox.Show("Введите или сгенерируйте пароль", "Ошибка"); return; }
            }
            if (textBox6.Text == "") { MessageBox.Show("Введите логин пользователя", "Ошибка"); return; }
        }
    }
}
