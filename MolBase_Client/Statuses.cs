using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MolBase_Client
{
    public class Statuses
    {
        const string GetStatuses = "database.status_list";

        List<MolStatus> Known_Statuses = new List<MolStatus>();

        public Statuses()
        {
            List<string> Answer = Form1.Send_Get_Msg_To_Server(GetStatuses, "");
            for (int i = 0; i < Answer.Count; i++)
            {
                if (Answer[i] == Form1.StartMsg) continue;
                if (Answer[i] == Form1.EndMsg) continue;

                MolStatus NewStatus = new MolStatus(Convert.ToInt32(Answer[i++]),
                    Answer[i++], Answer[i] == "" ? -1 : Convert.ToInt32(Answer[i]));

                Known_Statuses.Add(NewStatus);
            }
        }

        public string GetStatus(int ID)
        {
            List<MolStatus> Found_Statuses = Known_Statuses.Where(x => x.GetID() == ID).ToList();
            if (Found_Statuses.Count == 0)
                return "Неизвестный статус";
            return Found_Statuses[0].GetName();
        }

        public int GetNextStatus(int ID)
        {
            List<MolStatus> Found_Statuses = Known_Statuses.Where(x => x.GetID() == ID).ToList();
            if (Found_Statuses.Count == 0)
                return -1;
            return Found_Statuses[0].GetNext();
        }

    }
}
