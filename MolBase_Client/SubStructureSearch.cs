using OpenBabel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MolBase_Client
{
    public partial class SubStructureSearch : Form
    {
        List<Molecule> Mols;

        public SubStructureSearch()
        {
            GC.Collect();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OD.Filter = Functions.AllFormats.GetFilter();
            foreach (MoleculeFormat MF in Functions.Formats)
                OD.Filter += $" | {MF.GetFilter()}";
            OD.ShowDialog();
            textBox1.Text = OD.FileName;
            OpenStructure(textBox1.Text);
        }

        private void SubStructureSearch_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ?
                DragDropEffects.All : DragDropEffects.None;
        }

        private void SubStructureSearch_DragDrop(object sender, DragEventArgs e)
        {
            //Извлекаем имя перетаскиваемого файла
            string[] strings = (string[])e.Data.GetData(DataFormats.FileDrop, true);

            string File1 = strings[0];
            if (!Functions.CheckFile(File1))
            {
                MessageBox.Show("Неподдерживаемый формат файла", "Ошибка");
                return;
            }
            textBox1.Text = File1;
            OpenStructure(File1);
        }

        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private void OpenStructure(string StName)
        {
            // Создаём OpenBabel объекты
            OBConversion obconv = new OBConversion();
            OBMol mol = Functions.ReadMoleculeFromFile(StName);
            obconv.SetOutFormat("_png2");
            obconv.AddOption("w", OBConversion.Option_type.OUTOPTIONS, panel1.Width.ToString());
            obconv.AddOption("h", OBConversion.Option_type.OUTOPTIONS, panel1.Height.ToString());

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
            label3.Text = "Молярная масса: " + Math.Round( mol.GetExactMass(), 4);
            label4.Text = "Брутто-формула: " + mol.GetFormula();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e) //Поиск
        {
            // Настраиваем конвертер
            OBConversion obconv = new OBConversion();
            obconv.SetInFormat("cdx");
            OBMol molec = new OBMol();
            obconv.SetOutFormat("smi");
            obconv.ReadFile(molec, textBox1.Text);

            // Запрашиваем сервер и получаем ответ
            List<string> Answer = ServerCommunication.Send_Get_Msg_To_Server(
                ServerCommunication.Commands.Search_Mol, 
                "structure " + obconv.WriteString(molec) + " ");

            Mols = Functions.GetMolListFromServerAnswer(Answer);

            MoleculesList ML = new MoleculesList();
            ML.DrawList(Mols);
            Close();
            ML.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }
    }
}
