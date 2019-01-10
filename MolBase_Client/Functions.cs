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
            List<string> UserListString = ServerCommunication.Send_Get_Msg_To_Server("users.list", Parameters);
            List<User> Users = new List<User>();
            for (int i = 3; i < UserListString.Count - 1;)
            {
                bool NotReady = true;
                string StringToWork = "";
                string[] Params;
                do
                {
                    StringToWork += UserListString[i];
                    Params = StringToWork.Split('|');
                    NotReady = Params.Count() < 9;
                    i++;
                }
                while (NotReady);
                Users.Add(new User(Params[1], Params[2], Params[3], Params[4], Params[5], Params[6],
                    Params[7], Params[8]));
            }

            Users.Sort(new Compare_User("ID"));
            return Users;
        }
    }
}
