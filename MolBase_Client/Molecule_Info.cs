using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MolBase_Client
{
    public partial class Molecule_Info : Form
    {
        Molecule CurrentMolecule;
        List<int> FileIDs = new List<int>();
        int Cur_Element;

        // Текст кнопки повышения статуса
        private const string Status_Button_Begin = "Изменить на «";
        private const string Status_Button_End = "»";

        public Molecule_Info(Molecule Mol, int Element)
        {
            InitializeComponent();
            CurrentMolecule = Mol;
            Cur_Element = Element;
            pictureBox1.Image = Mol.Picture;
            label1.Text = "ПАСПОРТ НА ОБРАЗЕЦ № " + Mol.name;

            List<string> Info = new List<string>();
            StatusLabel.Text = "Статус заявки: " + Mol.Status;
            if (Form1.Known_Statuses.GetNextStatus(Mol.Status_Num) > -1)
            {
                button4.Text = Status_Button_Begin + 
                    Form1.Known_Statuses.GetStatus(Form1.Known_Statuses.GetNextStatus(Mol.Status_Num)) +
                    Status_Button_End;
                button4.Enabled = true;
            }
            else
            {
                button4.Text = Status_Button_Begin + Form1.Known_Statuses.GetStatus(Mol.Status_Num) +
                    Status_Button_End;
                button4.Enabled = false;
            }
            Info.Add("Молярная масса: " + Math.Round(Mol.Structure.GetMolWt()).ToString());
            Info.Add("Брутто-формула: " + Mol.Structure.GetFormula());
            Info.Add("Физическое состояние: " + Mol.State);
            Info.Add("Температура плавления: " + Mol.MeltingPoint);
            Info.Add("Охарактеризовано: " + Mol.GetAnalys());
            Info.Add("Растворимость: " + Mol.Solution);
            Info.Add("Условия хранения: " + Mol.Conditions);
            Info.Add("Масса образца: " + Mol.Mass);
            Info.Add("Синтезировано в: " + Mol.LaboratoryName);
            Info.Add("Синтезировал: " + Mol.MadeBy.Job + ", " + Mol.MadeBy.GetInitNameFirst());

            // Список приложенных файлов
            for (int i = 0; i < Mol.FileName.Count; i++)
            {
                FilesList.Items.Add(Mol.FileName[i]);
                FileIDs.Add(Mol.FileID[i]);
            }

            textBox1.Lines = Info.ToArray();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SD.ShowDialog();
            if (SD.FileName == null) { return; };
            PDF_Passport.Get_Passport(CurrentMolecule, SD.FileName);
            Process.Start(SD.FileName);
            Close();
        }

        private void FilesList_DoubleClick(object sender, EventArgs e)
        {
            if (FilesList.SelectedIndex == -1) { return; }

            Form1.GetFile(FileIDs[FilesList.SelectedIndex]);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Выбор файла
            string FileName = Form1.OpenFile();     if (FileName == "<Cancel>") return;
            string FileNameShort = Path.GetFileName(FileName);
            // Наименование файла
            string Name = Input_String.GetString("Название файла", "Название сохраняемого файла");
            if (Name == "@Cancel@") return;
            if (Name == "") return;

            // Отправление файл
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, Convert.ToInt32(fs.Length));
            fs.Flush();
            fs.Close();

            List<string> Answer = Form1.Send_Get_Msg_To_Server("<@*Get_File*@>", FileNameShort + "\n" + Name + 
                "\n" + data.Length.ToString() + "\n" + CurrentMolecule.ID.ToString(), data);

            // Просмотр ответа и добавление файла в список
            FilesList.Items.Add(Name);
            FileIDs.Add(Convert.ToInt32(Answer[1]));
        }

        // Изменение статуса заявки
        private void button4_Click(object sender, EventArgs e)
        {
            // Спросим, «Вы уверены?»
            if (MessageBox.Show("Вы уверены, что хотите изменить статус заявки на «" +
                Form1.Known_Statuses.GetStatus(Form1.Known_Statuses.GetNextStatus(CurrentMolecule.Status_Num)) + 
                "»?", "Изменение статуса заявки", MessageBoxButtons.YesNo) == DialogResult.No) return;

            // Если да
            // Отправим серверу запрос
            List<string> Answer = Form1.Send_Get_Msg_To_Server(Form1.Increase_Status, CurrentMolecule.ID.ToString());

            // Если сервер благополучно изменил, то
            if (Answer[1] == "OK")
            {
                // Вычислим и присвоим молекуле новый статус, а также напишем об этом
                CurrentMolecule.Status_Num = Form1.Known_Statuses.GetNextStatus(CurrentMolecule.Status_Num);
                CurrentMolecule.Status = Form1.Known_Statuses.GetStatus(CurrentMolecule.Status_Num);
                StatusLabel.Text = "Статус заявки: " + CurrentMolecule.Status;

                // Изменим рисунок молекулы в окне MoleculeList
                ((MoleculesList)Owner).Draw_Element(Cur_Element);
                Owner.Refresh();

                // И поменяем надпись на кнопке. Если мы получили финальный статус, кнопку отключим.
                if (Form1.Known_Statuses.GetNextStatus(CurrentMolecule.Status_Num) == -1)
                {
                    button4.Enabled = false;
                }
                else button4.Text = Status_Button_Begin + Form1.Known_Statuses.GetStatus(
                    Form1.Known_Statuses.GetNextStatus(CurrentMolecule.Status_Num)) + Status_Button_End;
            }
            // Если же сервер выдал ошибку, сообщим о ней.
            else MessageBox.Show(Answer[1], "Ошибка");
        }
    }
}
