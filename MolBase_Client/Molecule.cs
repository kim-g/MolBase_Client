using OpenBabel;
using System;
using System.Collections.Generic;
using System.Drawing;
using MoleculeDataBase;
using System.IO;

namespace MolBase_Client
{
    public class Molecule
    {
        public Molecule(int _id, string _smiles, string _name)
        {
            ID = _id;
            SMILES = _smiles;
            OBConversion obconv = new OBConversion();
            obconv.SetInFormat("smi");
            Structure = new OBMolecule();
            name = _name;
            obconv.ReadString(Structure, SMILES);
            Structure.SetTitle(name);
            using (FileStream FS = new FileStream("temp.bin", FileMode.Create))
            {
                Structure.ToBin().CopyTo(FS);
                FS.Close();
            }
            obconv.SetOutFormat("_png2");
            string TempPic = Functions.TempFile();
            obconv.WriteFile(Structure, TempPic); // Пишем картинку в temp // Это такое колдунство // Мне стыдно, но по-другому не выходит
            obconv.CloseOutFile();
            Picture = Image.FromFile(TempPic);
            Weight = Structure.GetMolWt();
            Brutto = Structure.GetFormula();
        }

        public static Molecule From_Molecule_Transport(MoleculeTransport MT)
        {
            Molecule Mol = new Molecule(MT.ID, MT.Structure, MT.Name);
            Mol.SetAdditionalInfoFromString(MT.Laboratory.ID.ToString(), MT.Person.ID.ToString(),
                MT.Other_Properties, MT.State, MT.Melting_Point, MT.Conditions, MT.Mass, MT.Solution,
                MT.Laboratory.Name, MT.Laboratory.Abb, MT.Person.Name, MT.Person.FathersName,
                MT.Person.Surname, MT.Person.Job, MT.Status);

            List<string> Analys = new List<string>();
            List<string> Analys_Whom = new List<string>();
            for (int i=0; i < MT.Analysis.Count; i+=2)
            {
                Analys.Add(MT.Analysis[i]);
                Analys_Whom.Add(MT.Analysis[i+1]);
            }
            Mol.AddAnalys(Analys, Analys_Whom);

            Mol.FileID = new List<int>();
            Mol.FileName = new List<string>();
            for (int i=0; i<MT.Files.Count; i++)
            {
                Mol.FileID.Add(MT.Files[i].ID);
                Mol.FileName.Add(MT.Files[i].Name);
            }

            return Mol;
        }

        public Image Picture;      // Изображение молекулы
        public OBMolecule Structure;    // Структура OpenBabel для дальнейшей работы
        public string SMILES;      // Структура SMILES
        public string name;        // Шифр
        public double Weight;      // Молекулярная масса
        public string Brutto;      // Брутто-формула
        public int ID;             // ID в БД
        public int LabID;          // Номер лаборатории в БД
        public int PersID;         // Номер владельца в БД
        public string Laboratory;  // Название лаборатории
        public string Person;      // Имя владельца
        public string Add_Info;    // Особые свойства
        public string State;       // Физическое состояние
        public string MeltingPoint;// Температура плавления
        public string Conditions;  // Условия хранения
        public string Mass;        // Масса образца
        public string Solution;    // Растворимость образца 
        public string LaboratoryName;   // Название лаборатории
        public string LaboratoryAbbr;   // Аббривиатура названия лаборатории
        public Person MadeBy;      // Синтетик
        public List<string> Analys;     // Методы анализа в именительном падеже
        public List<string> Analys_Whom;     // Методы анализа в творительном падеже
        public int Status_Num;     // Номер статуса
        public string Status;      // Название статуса
        public List<int> FileID;   // Номера файлов в БД
        public List<string> FileName;   // Имена файлов в БД


          public void SetAdditionalInfoFromString(string _LabID, string _PersID, string _Add_Info, string _State,
            string _MeltingPoint, string _Conditions, string _Mass, string _Solution, string _LaboratoryName,
            string _LaboratoryAbbr, string PersName, string PersFName, string PersSurName, string PersJob,
            int _Status)
        {
            LabID = Convert.ToInt32(_LabID);
            PersID = Convert.ToInt32(_PersID);
            Add_Info = _Add_Info;
            State = _State;
            MeltingPoint = _MeltingPoint;
            Conditions = _Conditions;
            Mass = _Mass;
            Solution = _Solution;
            MadeBy = new Person(PersName, PersFName, PersSurName, PersJob, PersID, LabID);
            LaboratoryName = _LaboratoryName;
            LaboratoryAbbr = _LaboratoryAbbr;
            Status_Num = _Status;
            Status = Form1.Known_Statuses.GetStatus(Status_Num);


        }

        public void AddAnalys(List<string> _Analys, List<string> _Analys_Whom)
        {
            Analys = _Analys;
            Analys_Whom = _Analys_Whom;
        }

        public string GetAnalys()
        {
            return GetAnalysInfo(Analys);
        }

        public string GetAnalys_Whom()
        {
            return GetAnalysInfo(Analys_Whom);
        }

        private string GetAnalysInfo(List<string> Info)
        {
            if (Info.Count == 0) { return "—"; }
            if (Info.Count == 1) { return Info[0]; }
            string Result = "";
            for (int i = 0; i < Info.Count - 1; i++)
            {
                Result += Info[i] + ", ";
            }
            Result += Info[Info.Count - 1];

            return Result;
        }

    }
}
