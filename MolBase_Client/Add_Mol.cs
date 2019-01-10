using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using OpenBabel;

namespace MolBase_Client
{
    public partial class Add_Mol : Form
    {
        OBMol mol;

        public Add_Mol()
        {
            GC.Collect();
            InitializeComponent();
        }

        private void OpenStructure(string StName)
        {
            // Создаём OpenBabel объекты
            OBConversion obconv = new OBConversion();
            obconv.SetInFormat("cdx"); //Читаем ChemDraw файл (Потом расширить список)
            mol = new OBMol();
            obconv.ReadFile(mol, StName);  //Читаем из файла

            //Пишем название
            S_Name.Text = mol.GetTitle();
            //Убираем название
            mol.SetTitle("");

            //Рисуем в файл
            obconv.SetOutFormat("_png2");
            obconv.AddOption("w", OBConversion.Option_type.OUTOPTIONS, panel1.Width.ToString());
            obconv.AddOption("h", OBConversion.Option_type.OUTOPTIONS, panel1.Height.ToString());
            
            Random rnd = new Random();
            string TempPic = Functions.TempFile();
            obconv.WriteFile(mol, TempPic); // Пишем картинку в temp // Это такое колдунство // Мне стыдно, но по-другому не выходит
            obconv.CloseOutFile();


            Bitmap bmp = new Bitmap(panel1.Width, panel1.Height);

            // Рисуем на панели
            panel1.BackgroundImage = bmp;

            Image IM = Image.FromFile(TempPic);
            Graphics g = Graphics.FromImage(panel1.BackgroundImage);
            g.DrawImage(IM, 0, 0);
            IM.Dispose();
            g.Dispose();
            panel1.Invalidate();

            // Высчитываем параметры
            
            label3.Text = "Молярная масса: " + Math.Round(mol.GetExactMass(), 4);
            label4.Text = "Брутто-формула: " + mol.GetFormula();
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            //Извлекаем имя перетаскиваемого файла
            string[] strings = (string[])e.Data.GetData(DataFormats.FileDrop, true);

            string File1 = strings[0];
            if (Path.GetExtension(File1) != ".cdx")
            {
                MessageBox.Show("Можно использовать только файл ChemDraw (cdx)\n" + Path.GetExtension(File1), "Ошибка");
                return;
            }
            textBox1.Text = File1;
            OpenStructure(File1);
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ?
                DragDropEffects.All : DragDropEffects.None;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OD.ShowDialog();
            textBox1.Text = OD.FileName;
            OpenStructure(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private string ListOfStringToString(List<string> Input)
        {
            string Res = "";
            for (int i = 0; i<Input.Count; i++)
            {
                Res += Input[i];
                if (i<Input.Count-1)
                { Res += "\n"; }
            }
            return Res;
        }

        private bool CheckInputData()
        {
            if (S_Name.Text == "")
            {
                MessageBox.Show("Введите шифр образца.", "Ошибка");
                return false;
            }

            if (PhysState.Text == "")
            {
                MessageBox.Show("Укажите физическое состояние образца.", "Ошибка");
                return false;
            }

            if (Melt.Text == "")
            {
                MessageBox.Show("Укажите температуру плавления образца.", "Ошибка");
                return false;
            }

            if (Conditions.Text == "")
            {
                Conditions.Text = "Обычные";
            }

            if (Properties.Text == "")
            {
                Properties.Text = "Отсутствуют";
            }

            if (Mass.Text == "")
            {
                MessageBox.Show("Укажите массу образца.", "Ошибка");
                return false;
            }
            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OBConversion obconv = new OBConversion();
            obconv.SetOutFormat("smi");

            if (!CheckInputData()) { return; }

            List<string> message = new List<string>();
            message.Add("code " + S_Name.Text);               // Шифр
            message.Add("laboratory 1");                       // Лаборатория
            message.Add("person 1");                       // Персона
            message.Add("structure " + obconv.WriteString(mol).Trim("\n"[0]));   // Структура
            message.Add("phys_state " + PhysState.Text);            // Физ. состояние
            message.Add("melting_point " + Melt.Text);                 // Т. плавления
            message.Add("conditions " + Conditions.Text);           // Условия хранения
            message.Add("properties " + Properties.Text);           // Особые свойства
            message.Add("mass " + Mass.Text);                 // Масса образца
            message.Add("solution " + Solution.Text);             // Растворимость образца



            List<string> Answer = ServerCommunication.Send_Get_Msg_To_Server(
                ServerCommunication.Commands.Add_Mol, 
                ListOfStringToString(message));

            if (Answer[1]== "Add_Molecule: done")
            {
                Close();
            }
            else
            {
                MessageBox.Show(Answer[1], Answer[2]);
            }
        }
    }
}
