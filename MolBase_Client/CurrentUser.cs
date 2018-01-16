using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MolBase_Client
{
    public class CurrentUser
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string Surname { get; set; }
        public string Login { get; set; }
        public string SessionCode { get; set; }
        public int ShowMol { get; set; }
        public int AddMol { get; set; }
        public int Special { get; set; }

        public CurrentUser(string login)
        {
            Login = login;
        }

        public string FullSurnameName()
        {
            return Surname + " " + Name + " " + SecondName;
        }

        public string FullNameSurname()
        {
            return Name + " " + SecondName + " " + Surname;
        }

        public void SetRights(string RightsString)
        {
            string[] Rights = RightsString.Split(',');

            switch (Rights[0])
            {
                case "user":
                    ShowMol = 0; break;
                case "lab":
                    ShowMol = 1; break;
                case "ios":
                    ShowMol = 2; break;
            }
            switch (Rights[1])
            {
                case "user":
                    AddMol = 0; break;
                case "lab":
                    AddMol = 1; break;
                case "ios":
                    AddMol = 2; break;
            }
            switch (Rights[2])
            {
                case "null":
                    AddMol = 0; break;
                case "manager":
                    AddMol = 1; break;
                case "admin":
                    AddMol = 2; break;
            }
        }
    }
}
