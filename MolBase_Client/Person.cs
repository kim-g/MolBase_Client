using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MolBase_Client
{
    public class Person
    {
        public string Name;
        public string Fathers_Name;
        public string Surname;
        public int Laboratory;
        public int ID;
        public string Job;

        public Person(string _Name, string _Fathers_Name, string _Surname, string _Job, int _ID, int _Lab)
        {
            Name = _Name;
            Fathers_Name = _Fathers_Name;
            Surname = _Surname;
            Job = _Job;
            ID = _ID;
            Laboratory = _Lab;
        }

        public string GetFullNameFirst()
        {
            if (Fathers_Name != "")
            { return Name + " " + Fathers_Name + " " + Surname; }
            return Name + " " + Surname;
        }

        public string GetFullSurNameFirst()
        {
            if (Fathers_Name != "")
            { return Surname + " " + Name + " " + Fathers_Name; }
            return Surname + " " + Name;
        }

        public string GetInitNameFirst()
        {
            if (Fathers_Name != "")
            { return Name[0] + "." + Fathers_Name[0] + ". " + Surname; }
            return Name[0] + ". " + Surname;
        }

        public string GetInitSurNameFirst()
        {
            if (Fathers_Name != "")
            { return Surname + " " + Name[0] + "." + Fathers_Name[0] + "."; }
            return Surname + " " + Name[0] + ".";
        }
    }
}
