using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MolBase_Client
{
    public partial class Molecule_Info : Form
    {
        Molecule CurrentMolecule;

        public Molecule_Info(Molecule Mol)
        {
            InitializeComponent();
            CurrentMolecule = Mol;
            pictureBox1.Image = Mol.Picture;
            label1.Text = "ПАСПОРТ НА ОБРАЗЕЦ № " + Mol.name;

            List<string> Info = new List<string>();
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
    }
}
