using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenBabel;

namespace MolBase_Client
{
    public partial class MoleculesList : Form
    {
        //Параметры
        const int Element_Width = 200;
        const int Element_Height = 270;
        const int BMP_Width = 200;
        const int BMP_Height = 250;
        const int TextPanel = 50;
        const int Marging_Vert = 20;
        const int MouseDelta = 1;

        // Полный набор молекул и элементов
        List<Molecule> Molecules = new List<Molecule>();
        List<PicElement> Elements = new List<PicElement>();

        // Отфильтрованный набор молекул и элементов
        List<Molecule> Filtered;
        List<PicElement> FilteredElements;

        int CurrentSelected = -1;

        public MoleculesList()
        {
            InitializeComponent();
        }

        public void DrawMyList()
        {
            // Очищаем Elements
            Elements.Clear();
            
            // Перемещаем всё в «свою корзину» 
            for (int i = 0; i < Molecules.Count(); i++)
            {
                Elements.Add(DrawMoleculeElement(Molecules[i]));
            }

            // Определяем параметры скрола
            SetScroll();
        }

        public bool DrawList(List<Molecule> Mols)
        {
            for (int i = 0; i < Mols.Count(); i++)
            {
                Molecules.Add(Mols[i]);
            }

            

            DrawMyList();
            Filter();
            this.Refresh();
            return true;
        }

        private PicElement DrawMoleculeElement(Molecule Mol)
        {
            // Настраиваем конвертер
            OBConversion obconv = new OBConversion();
            obconv.SetOutFormat("_png2");
            Mol.Structure.SetTitle("");
            obconv.AddOption("w", OBConversion.Option_type.OUTOPTIONS, BMP_Width.ToString());
            obconv.AddOption("h", OBConversion.Option_type.OUTOPTIONS, (BMP_Height - TextPanel).ToString());
            // Получаем файл с картинкой
            string TempPic = Form1.TempFile();
            obconv.WriteFile(Mol.Structure, TempPic);
            obconv.CloseOutFile();

            // Рисуем структуру
            Image IM = Image.FromFile(TempPic);
            //Img
            Bitmap bmp = new Bitmap(BMP_Width, BMP_Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.DrawImage(IM, 0, 0);
            g.DrawString(Mol.name,
                new Font("Arial", 14, FontStyle.Bold),
                new SolidBrush(Color.Black),
                new PointF(5, BMP_Height - TextPanel + 5));

            g.DrawString(Mol.Status,
                new Font("Arial", 12, FontStyle.Regular),
                new SolidBrush(Color.Black),
                new PointF(5, BMP_Height - TextPanel + 5 + new Font("Arial", 14, FontStyle.Bold).GetHeight()));
            g.Dispose();

            Mol.Structure.SetTitle(Mol.name);

            PicElement El = new PicElement();
            El.Image = bmp;
            El.ID = Mol.ID;

            return El;
            //Всё подчищаем
            // bmp.Dispose();
            // IM.Dispose();
        }

        private void MoleculesList_Paint(object sender, PaintEventArgs e)
        {
            //Подсчёт параметров таблицы
            int Colomns = ClientSize.Width <= Element_Width ? 1 : Convert.ToInt32(ClientSize.Width / Element_Width);
            int Rows = Convert.ToInt32(Filtered.Count() / Colomns) + 1;
            if (Colomns * (Rows - 1) == Filtered.Count()) { Rows--; };
            int DeltaX = Colomns == 1
                ? 0
                : Convert.ToInt32((ClientSize.Width - vScrollBar1.Width - Colomns * Element_Width) / (Colomns - 1));

            //Рисование таблицы
            e.Graphics.Clear(Color.White);
            Graphics Pic = e.Graphics;
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Colomns; j++)
                {
                    if (i * Colomns + j >= Filtered.Count()) { break; }
                    Pic.DrawImage(FilteredElements[i * Colomns + j].Image,
                        j * (Element_Width + DeltaX),
                        i * Element_Height - vScrollBar1.Value + Marging_Vert + panel1.Height);
                    if (i * Colomns + j == CurrentSelected)
                    {
                        Pic.DrawRectangle(new Pen(Color.Red, 2),
                            j * (Element_Width + DeltaX) + 1,
                            i * Element_Height - vScrollBar1.Value + 1 + Marging_Vert + panel1.Height,
                            BMP_Width - 1, BMP_Height - 1);
                    }
                }

            // Рисование отладочной информации
            /*Pic.DrawString("Colomns = " + Colomns.ToString() + 
                "\nRows = " + Rows.ToString(),
                new Font("Arial", 10, FontStyle.Regular),
                new SolidBrush(Color.Black), 5, 5);*/
        }

        private void SetScroll()
        {
            // Определим число строк
            int Colomns = ClientSize.Width <= Element_Width ? 1 : Convert.ToInt32(ClientSize.Width / Element_Width);
            int Rows = Convert.ToInt32(Molecules.Count() / Colomns) + 1;
            if (Colomns * (Rows - 1) == Molecules.Count()) { Rows--; };

            // Установка скролла
            int TotalHeight = Rows * Element_Height + Marging_Vert * 2;
            if (TotalHeight <= ClientSize.Height)
            {
                vScrollBar1.Visible = false;
                vScrollBar1.Value = 1;
            }
            else
            {
                vScrollBar1.Visible = true;
                vScrollBar1.Maximum = TotalHeight - ClientSize.Height;
            }
        }

        private void MoleculesList_Resize(object sender, EventArgs e)
        {
            SetScroll();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            Refresh();
        }

        private void MoleculesList_MouseMove(object sender, MouseEventArgs e)
        {
            int Pos = GetElementUnderCursor(e);
            if (Pos != CurrentSelected)
            {
                CurrentSelected = Pos;
                Refresh();
            }
        }

        private int GetElementUnderCursor(MouseEventArgs e)
        {
            // Определение примерного положения курсора с точностью до ячейки с полями
            int Colomns = ClientSize.Width <= Element_Width ? 1 : 
                Convert.ToInt32(ClientSize.Width / Element_Width);
            int DeltaX = Colomns == 1 ?
                0 :
                Convert.ToInt32((ClientSize.Width - vScrollBar1.Width - Colomns * Element_Width) / (Colomns - 1));
            int MyCol = Convert.ToInt32(e.X / (Element_Width + DeltaX));
            int MyRow = Convert.ToInt32((e.Y - Marging_Vert + vScrollBar1.Value - panel1.Height) / Element_Height);

            // Определение попадания в вертикальное поле
            if (e.Y + vScrollBar1.Value < Marging_Vert) { return -1; };
            if (e.Y + vScrollBar1.Value > (MyRow * Element_Height) + BMP_Height + Marging_Vert + panel1.Height)
              { return -1; };
            
            // Определение попадания в горизонтальное поле
            if (e.X > MyCol * (Element_Width + DeltaX) + BMP_Width) { return -1; };

            // Определение элемента
            int Element = MyRow * Colomns + MyCol;
            if (Element >= Filtered.Count()) { return -1; }
            return MyRow * Colomns + MyCol;

        }

        private void MoleculesList_MouseWheel(object sender, MouseEventArgs e)
        {
            vScrollBar1.Value = ToScroll(vScrollBar1,e.Delta);
            Refresh();
        }

        int ToScroll(VScrollBar SB, int Delta)
        {
            if (!SB.Visible) { return 0; };
            int TempV = SB.Value - Delta * MouseDelta;
            if (TempV < SB.Minimum)
            {
                return SB.Minimum;
            }
            if (TempV > SB.Maximum)
            {
                return SB.Maximum;
            }
            return TempV;
        }

        private void MoleculesList_Click(object sender, EventArgs e)
        {
            if (CurrentSelected == -1) { return; };

            Molecule_Info MI = new Molecule_Info(Filtered[CurrentSelected], CurrentSelected);
            MI.Owner = this;
            MI.Show();
        }

        public void Draw_Element(int Element)
        {
            Elements[Element] = DrawMoleculeElement(Molecules[Element]);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Filter();

            Refresh();
        }

        private void Filter()
        {
            // Отбор молекул по локальным условиям
            Filtered = textBox1.Text == ""
                ? Molecules
                : Molecules.FindAll(p => p.name.ToLower().Contains(textBox1.Text.ToLower()));

            FilteredElements = new List<PicElement>();
            for (int i = 0; i < Filtered.Count; i++)
                FilteredElements.Add(Elements.Find(p => p.ID == Filtered[i].ID));  
        }
    }

    class PicElement
    {
        public Bitmap Image;
        public int ID;
    }
}
