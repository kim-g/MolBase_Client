using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                if (Answer[i] == Form1.StartMsg || Answer[i] == Form1.EndMsg) { continue; }; // Отсеиваем служебные команды

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
    }
}
