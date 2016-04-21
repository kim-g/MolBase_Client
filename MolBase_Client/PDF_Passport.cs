using iTextSharp.text;
using iTextSharp.text.pdf;
using OpenBabel;
using System;
using System.IO;
using System.Windows.Forms;

namespace MolBase_Client
{
    public class PDF_Passport
    {

        public static void Get_Passport(Molecule Mol, string Filename)
        {
            // Настраиваем конвертер
            OBConversion obconv = new OBConversion();
            obconv.SetOutFormat("_png2");
            Mol.Structure.SetTitle("");
            obconv.AddOption("w", OBConversion.Option_type.OUTOPTIONS, 1182.ToString());
            obconv.AddOption("h", OBConversion.Option_type.OUTOPTIONS, 1182.ToString());
            // Получаем файл с картинкой
            string TempPic = Form1.TempFile();
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

            Paragraph Institute = new Paragraph("Институт органического синтеза им. И.Я. Постовского УрО РАН", Bold);
            Institute.Alignment = Element.ALIGN_CENTER;
            doc.Add(Institute);

            Paragraph BlankCenter = new Paragraph(" ", Bold);
            BlankCenter.Alignment = Element.ALIGN_CENTER;
            doc.Add(BlankCenter);

            Paragraph Address = new Paragraph("620041, г. Екатеринбург, ул. С. Ковалевской/Академическая, 20/22,\n" +
                "Телефоны (343) 374-11-89, (343) 369-30-58", Regular);
            Address.Alignment = Element.ALIGN_CENTER;
            doc.Add(Address);

            Paragraph p = new Paragraph(
                new Chunk(
                    new iTextSharp.text.pdf.draw.LineSeparator(
                        0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
            doc.Add(p);

            string MolName = Mol.name;
            Paragraph Title = new Paragraph("ПАСПОРТ НА ОБРАЗЕЦ № " + MolName, LargeBold);
            Title.Alignment = Element.ALIGN_CENTER;
            doc.Add(Title);

            Image png = Image.GetInstance(TempPic);
            png.Alignment = Element.ALIGN_CENTER;
            png.ScaleAbsoluteWidth(250);
            png.ScaleAbsoluteHeight(250);
            doc.Add(png);

            Paragraph Mm = new Paragraph("Молярная масса: ", Bold);
            Phrase MmValue = new Phrase(Math.Round(Mol.Structure.GetMolWt(), 2).ToString(), Regular);
            Mm.Add(MmValue);
            doc.Add(Mm);

            Paragraph Brutto = new Paragraph("Брутто формула: ", Bold);
            Phrase BruttoValue = SetSuperSubScript(Mol.Structure.GetFormula(), Regular, SubScribe, SUBSCRIPT);
            Brutto.Add(BruttoValue);
            doc.Add(Brutto);

            Paragraph State = new Paragraph("Физическое состояние: ", Bold);
            Phrase StateValue = new Phrase(Mol.State, Regular);
            State.Add(StateValue);
            doc.Add(State);

            Paragraph MP = new Paragraph("Температура плавления: ", Bold);
            Phrase MPValue = new Phrase(Mol.MeltingPoint, Regular);
            MP.Add(MPValue);
            doc.Add(MP);

            Paragraph Character = new Paragraph("Охарактеризовано: ", Bold);
            Phrase CharacterValue = SetSuperSubScript(Mol.GetAnalys_Whom(), Regular, SubScribe, SUPERSCRIPT);
            Character.Add(CharacterValue);
            doc.Add(Character);

            Paragraph Solution = new Paragraph("Растворимость: ", Bold);
            Phrase SolutionValue = new Phrase(Mol.Solution, Regular);
            Solution.Add(SolutionValue);
            doc.Add(Solution);

            Paragraph Conditions = new Paragraph("Условия хранения: ", Bold);
            Phrase ConditionsValue = new Phrase(Mol.Conditions, Regular);
            Conditions.Add(ConditionsValue);
            doc.Add(Conditions);

            Paragraph Mass = new Paragraph("Масса образца: ", Bold);
            Phrase MassValue = new Phrase(Mol.Mass + " мг", Regular);
            Mass.Add(MassValue);
            doc.Add(Mass);

            Paragraph Synthetic = new Paragraph("Синтезировал: ", Bold);
            Phrase SyntheticValue = new Phrase(Mol.MadeBy.Job + ", " + Mol.MadeBy.GetInitNameFirst() +
                " ("+Mol.LaboratoryName+")", Regular);
            Synthetic.Add(SyntheticValue);
            doc.Add(Synthetic);

            Paragraph SciLead = new Paragraph("НАУЧНЫЙ РУКОВОДИТЕЛЬ", Bold);
            doc.Add(SciLead);

            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;

            PdfPCell cell1 = new PdfPCell(new Phrase("академик", Bold));
            cell1.HorizontalAlignment = Element.ALIGN_LEFT;
            cell1.BorderWidth = 0;

            PdfPCell cell2 = new PdfPCell(new Phrase("В.Н. Чарушин", Bold));
            cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
            cell2.BorderWidth = 0;

            table.AddCell(cell1);
            table.AddCell(cell2);
            doc.Add(table);


            doc.Close();
        }

        private const int SUBSCRIPT = -3;
        private const int SUPERSCRIPT = 5;

        private static Phrase SetSuperSubScript(string Text, Font CurFont, Font ScribeFont, int Rise)
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
}
