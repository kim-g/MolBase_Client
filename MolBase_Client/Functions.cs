using MoleculeDataBase;
using OpenBabel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MolBase_Client
{
    public class Functions
    {
        /// <summary>
        /// Список поддерживаемых форматов
        /// </summary>
        public static MoleculeFormat[] Formats = new MoleculeFormat[]
        {
            new MoleculeFormat
            {
                Name = "Alchemy format",
                Extentions = new string[] { "alc" }
            },
            new MoleculeFormat
            {
                Name = "Ball and Stick format",
                Extentions = new string[] { "bs" }
            },
            new MoleculeFormat
            {
                Name = "Chem3D Cartesian format",
                Extentions = new string[] { "c3d1", "c3d2" }
            },
            new MoleculeFormat
            {
                Name = "ChemDraw format",
                Extentions = new string[] { "cdx", "cdxml" }
            },
            new MoleculeFormat
            {
                Name = "Crystallographic Information format",
                Extentions = new string[] { "cif", "mcif", "mmcif" }
            },
            new MoleculeFormat
            {
                Name = "Chemical Markup Language",
                Extentions = new string[] { "cml", "cmlr" }
            },
            new MoleculeFormat
            {
                Name = "Gaussian CUBE format",
                Extentions = new string[] { "cub", "cube" }
            },
            new MoleculeFormat
            {
                Name = "DMol3 coordinates format",
                Extentions = new string[] { "dmol" }
            },
            new MoleculeFormat
            {
                Name = "GULP format",
                Extentions = new string[] { "got" }
            },
            new MoleculeFormat
            {
                Name = "HyperChem HIN format",
                Extentions = new string[] { "hin" }
            },
            new MoleculeFormat
            {
                Name = "MCDL format",
                Extentions = new string[] { "mcdl" }
            },
            new MoleculeFormat
            {
                Name = "MDL Mol format",
                Extentions = new string[] { "mol", "mdl", "mol2", "sd", "sdf" }
            },
            new MoleculeFormat
            {
                Name = "PubChem format",
                Extentions = new string[] { "pc" }
            },
            new MoleculeFormat
            {
                Name = "POS cartesian coordinates format",
                Extentions = new string[] { "pos" }
            },
            new MoleculeFormat
            {
                Name = "SMILES format",
                Extentions = new string[] { "smi", "smiles" }
            },
            new MoleculeFormat
            {
                Name = "XYZ cartesian coordinates format",
                Extentions = new string[] { "xyz" }
            }
        };

        /// <summary>
        /// Все расширения 
        /// </summary>
        public static MoleculeFormat AllFormats = MoleculeFormat.Combine("Все файлы молекул", Formats);

        // Возврает список молекул из ответа сервера
        static public List<Molecule> GetMolListFromServerAnswer(List<string> Answer)
        {
            List<Molecule> Mols = new List<Molecule>(); // создаём список молекул

            for (int i = 0; i < Answer.Count; i++)  // Ищем начало новой молекулы
            {
                if (Answer[i] == ServerCommunication.Answers.StartMsg || 
                    Answer[i] == ServerCommunication.Answers.EndMsg) { continue; }; // Отсеиваем служебные команды

                if (Answer[i] == "<?xml version=\"1.0\"?>\r")   // Когда нашли начало XML описания молеклы
                {
                    // Помещаем весь XML в одну строку
                    string XML_Mol = "";
                    int j = 0;
                    while (Answer[i + j] != "</Molecule_Transport>")
                    {
                        XML_Mol += Answer[i + j];
                        j++;
                    }
                    XML_Mol += "\n" + Answer[i + j];

                    // И, переведя XML в Molecule_Transport, а из него в Molecule добавляем в массив.
                    Mols.Add(Molecule.From_Molecule_Transport(ConsoleServer.Molecule_Transport.FromXML(XML_Mol)));
                }
            }

            return Mols;
        }

        /// <summary>
        /// Возвращает случайное имя файла в Temp директории
        /// </summary>
        /// <returns></returns>
        public static string TempFile()
        {
            Random rnd = new Random();
            return Path.GetTempPath() + "MolBase\\MolBase" + 
                rnd.Next(1000000, 9999999).ToString() + ".tmp";
        }

        /// <summary>
        /// Выдаёт окно сохранения файла
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static string SaveFileAs(string FileName)
        {
            using (var sfd = new SaveFileDialog())
            {
                // Задаём нужные параметры
                string FF = Path.GetExtension(FileName);
                string FFName = "*" + FF;
                if (FF == ".doc") FFName = "Документ Microsoft Word 97-2003 (*.doc)";
                if (FF == ".docx") FFName = "Документ Microsoft Word (*.docx)";
                if (FF == ".xls") FFName = "Документ Microsoft Excel 97-2003 (*.xls)";
                if (FF == ".xlsx") FFName = "Документ Microsoft Excel (*.xlsx)";
                if (FF == ".pdf") FFName = "Документ Portable Document Format (*.pdf)";
                if (FF == ".jpg") FFName = "Изображение в формате JPEG (*.jpg)";
                if (FF == ".png") FFName = "Изображение в формате Portable Network Graphics (*.png)";
                sfd.Filter = FFName + "|*" + FF;
                sfd.FileName = FileName;
                sfd.AddExtension = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    return sfd.FileName;
                }
                return "<Cancel>";
            }
        }


        /// <summary>
        /// Диалоговое окно открытия файла. Возвращает полный путь ити "<Cancel>" в случае отмены
        /// </summary>
        /// <returns></returns>
        public static string OpenFile()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Все файлы (*.*)|*.*";
                ofd.AddExtension = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    return ofd.FileName;
                }
                return "<Cancel>";
            }
        }

        /// <summary>
        /// Возвращает список пользователей
        /// </summary>
        /// <param name="Parameters">Доп. параметры</param>
        /// <returns></returns>
        public static List<User> GetUserList(string Parameters="")
        {
            List<UserTransport> UserListString = ServerCommunication.GetTransportList<UserTransport>("users.get",
                new string[] { Parameters });
            List<User> Users = new List<User>();
            foreach (UserTransport UT in UserListString)
            {
                Users.Add(new User(UT.id.ToString(), UT.Surname, UT.Name, UT.SecondName, UT.Login, UT.LaboratoruName,UT.Job,UT.Permissions.ToString()));
            }

            Users.Sort(new Compare_User("ID"));
            return Users;
        }

        /// <summary>
        /// Читает молекулу из файла
        /// </summary>
        /// <param name="FileName">Имя файла</param>
        /// <returns></returns>
        public static OBMol ReadMoleculeFromFile(string FileName)
        {
            OBConversion obconv = new OBConversion();
            OBFormat OBF = OBConversion.FormatFromExt(FileName);
            
            if (OBF == null)
            {
                MessageBox.Show("Неподдерживаемый формат данных", "Ошибка открытия файла");
                return null;
            }
            obconv.SetInFormat(OBF); //Читаем ChemDraw файл (Потом расширить список)
            OBMol mol = new OBMol();
            obconv.ReadFile(mol, FileName);  //Читаем из файла
            mol.SetTitle("");
            mol.DeleteHydrogens();

            return mol;
        }

        /// <summary>
        /// Выдаёт, поддерживает ли программа выбранный формат
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static bool CheckFile(string FileName)
        {
            string FMLow = FileName.ToLowerInvariant();
            string Ext = Path.GetExtension(FMLow).Remove(0,1);
            bool Res = AllFormats.Extentions.Contains(Ext);
            return Res;

            //return AllFormats.Extentions.Contains(Path.GetExtension(FileName.ToLowerInvariant()));
        }
    }
}
