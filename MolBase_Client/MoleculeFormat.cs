using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MolBase_Client
{
    public class MoleculeFormat
    {
        /// <summary>
        /// Название формата
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Поддерживаемые расширения
        /// </summary>
        public string[] Extentions { get; set; }

        /// <summary>
        /// Выдаёт фильтр для FileDialog.Filter
        /// </summary>
        /// <returns></returns>
        public string GetFilter()
        {
            string Res = Name + " (";
            bool start = true;
            foreach(string Ext in Extentions)
            {
                Res += start ? "" : ", ";
                Res += $"*.{Ext}";
                start = false;
            }

            Res += ") | ";

            start = true;
            foreach (string Ext in Extentions)
            {
                Res += start ? "" : ";";
                Res += $"*.{Ext}";
            }

            return Res;
        }

        public static MoleculeFormat Combine(string FormatName, MoleculeFormat[] Formats)
        {
            MoleculeFormat ResFormat = new MoleculeFormat { Name = FormatName, Extentions = new string[0] };
            foreach (MoleculeFormat MF in Formats)
                ResFormat.Extentions = ResFormat.Extentions.Concat(MF.Extentions).ToArray();

            return ResFormat;
        }
    }
}
