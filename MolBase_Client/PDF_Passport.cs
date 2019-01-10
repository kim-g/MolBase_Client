using iTextSharp.text;
using iTextSharp.text.pdf;
using OpenBabel;
using System;
using System.IO;
using System.Windows.Forms;

namespace MolBase_Client
{
    public abstract class PDF_Creator
    {
        /// <summary>
        /// Добавление параграфа с текстом
        /// </summary>
        /// <param name="Text">Текст</param>
        /// <param name="TextFont">Шрифт</param>
        /// <param name="Alignment">Выравнивание</param>
        /// <returns></returns>
        protected static Paragraph AddText(string Text, Font TextFont, int Alignment = Element.ALIGN_JUSTIFIED)
        {
            Paragraph Institute = new Paragraph(Text, TextFont);
            Institute.Alignment = Alignment;
            return Institute;
        }

        /// <summary>
        /// Добавление параграфи со сложным форматированием
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Phrases"></param>
        /// <param name="TextFont"></param>
        /// <param name="Alignment"></param>
        /// <returns></returns>
        protected static Paragraph AddText(string Text, Phrase[] Phrases, Font TextFont,
            int Alignment = Element.ALIGN_JUSTIFIED)
        {
            Paragraph Par = AddText(Text, TextFont, Alignment);
            Par.AddAll(Phrases);
            return Par;
        }

        /// <summary>
        /// Рисует линию
        /// </summary>
        /// <returns></returns>
        protected static Paragraph AddLine()
        {
            return new Paragraph(
                new Chunk(
                    new iTextSharp.text.pdf.draw.LineSeparator(
                        0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
        }

        protected static Image AddImage(string Name, int width, int height=0)
        {
            Image png = Image.GetInstance(Name);
            png.Alignment = Element.ALIGN_CENTER;
            if (height == 0)
            {
                float OldWidth = png.Width;
                float Oldheight = png.Height;
                png.ScaleAbsoluteWidth(width);
                png.ScaleAbsoluteHeight(width / OldWidth * Oldheight);
            }
            else
            {
                png.ScaleAbsoluteWidth(width);
                png.ScaleAbsoluteHeight(height);
            }
            return png;
        }

        protected const int SUBSCRIPT = -3;
        protected const int SUPERSCRIPT = 5;

        /// <summary>
        /// Добавляет верхний или нижний индекс к тексту
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="CurFont"></param>
        /// <param name="ScribeFont"></param>
        /// <param name="Rise"></param>
        /// <returns></returns>
        protected static Phrase SetSuperSubScript(string Text, Font CurFont, Font ScribeFont, int Rise)
        {
            Phrase Result = new Phrase("", CurFont);
            for (int i = 0; i < Text.Length; i++)
            {
                Chunk ck;
                if (Char.IsDigit(Text[i]))
                {
                    ck = new Chunk(Text[i], ScribeFont).SetTextRise(Rise);
                }
                else
                {
                    ck = new Chunk(Text[i], CurFont);
                }
                Result.Add(ck);
            }

            return Result;

        }
    }

    public class PDF_Passport : PDF_Creator
    {
        public static void Get_Passport(Molecule Mol, string Filename)
        {
            // Настраиваем конвертер
            OBConversion obconv = new OBConversion();
            obconv.SetOutFormat("_png2");
            Mol.Structure.SetTitle("");
            obconv.AddOption("w", OBConversion.Option_type.OUTOPTIONS, "1182");
            obconv.AddOption("h", OBConversion.Option_type.OUTOPTIONS, "1182");
            // Получаем файл с картинкой
            string TempPic = Functions.TempFile();
            obconv.WriteFile(Mol.Structure, TempPic);
            obconv.CloseOutFile();
            Mol.Structure.SetTitle(Mol.name);

            var doc = new Document(PageSize.A4);
            PdfWriter.GetInstance(doc, new FileStream(Filename, FileMode.Create));

            doc.Open();

            BaseFont Times = BaseFont.CreateFont(Application.StartupPath + @"\Fonts\times.ttf",
                BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            BaseFont TimesBold = BaseFont.CreateFont(Application.StartupPath + @"\Fonts\timesb.ttf",
                BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            BaseFont TimesItalic = BaseFont.CreateFont(Application.StartupPath + @"\Fonts\timesi.ttf",
                BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            BaseFont TimesBoldItalic = BaseFont.CreateFont(Application.StartupPath + @"\Fonts\timesbi.ttf",
                BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            Font Regular = new Font(Times, 14);
            Font Bold = new Font(TimesBold, 14);
            Font LargeBold = new Font(TimesBold, 18);
            Font SubScribe = new Font(Times, 10);

            doc.Add(AddText("Институт органического синтеза им. И.Я. Постовского УрО РАН", Bold, 
                Element.ALIGN_CENTER));
            doc.Add(AddText(" ", Bold));
            doc.Add(AddText("620041, г. Екатеринбург, ул. С. Ковалевской/Академическая, 20/22,\n" +
                "Телефоны (343) 374-11-89, (343) 369-30-58", Regular, Element.ALIGN_CENTER));
            doc.Add(AddLine());
            doc.Add(AddText("ПАСПОРТ НА ОБРАЗЕЦ № " + Mol.name, LargeBold, Element.ALIGN_CENTER));
            doc.Add(AddImage(TempPic,250));
            doc.Add(AddText("Молярная масса: ", 
                new[] { new Phrase(Math.Round(Mol.Structure.GetMolWt(), 2).ToString(), Regular)}, Bold));
            doc.Add(AddText("Брутто формула: ",
                new[] { SetSuperSubScript(Mol.Structure.GetFormula(), Regular, SubScribe, SUBSCRIPT) }, Bold));
            doc.Add(AddText("Физическое состояние: ",
                new[] { new Phrase(Mol.State, Regular) }, Bold));
            doc.Add(AddText("Температура плавления: ",
                new[] { new Phrase(Mol.MeltingPoint, Regular) }, Bold));
            doc.Add(AddText("Охарактеризовано: ",
                new[] { SetSuperSubScript(Mol.GetAnalys_Whom(), Regular, SubScribe, SUPERSCRIPT) }, Bold));
            doc.Add(AddText("Растворимость: ",
                new[] { new Phrase(Mol.Solution, Regular) }, Bold));
            doc.Add(AddText("Условия хранения: ",
                new[] { new Phrase(Mol.Conditions, Regular) }, Bold));
            doc.Add(AddText("Масса образца: ",
                new[] { new Phrase(Mol.Mass + " мг", Regular) }, Bold));
            doc.Add(AddText("Синтезировал: ",
                new[] { new Phrase(Mol.MadeBy.Job + ", " + Mol.MadeBy.GetInitNameFirst() +
                " ("+Mol.LaboratoryName+")", Regular) }, Bold));
            doc.Add(AddText("НАУЧНЫЙ РУКОВОДИТЕЛЬ", Bold));

            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.AddCell(new PdfPCell(new Phrase("академик", Bold))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                BorderWidth = 0
            });
            table.AddCell(new PdfPCell(new Phrase("В.Н. Чарушин", Bold))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                BorderWidth = 0
            });
            doc.Add(table);

            doc.Close();
        }

    }
}
