using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MolBase_Client
{
    public class MolStatus
    {
        int ID;
        string Name;
        int Next_Status;

        public MolStatus(int _ID, string _Name, int _Next_Status)
            {
            ID = _ID;
            Name = _Name;
            Next_Status = _Next_Status;
            }

        public int GetID()
        {
            return ID;
        }

        public string GetName()
        {
            return Name;
        }

        public int GetNext()
        {
            return Next_Status;
        }
    }
}
