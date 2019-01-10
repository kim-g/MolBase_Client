using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Extentions;

namespace MolBase_Client
{
    public partial class UserList : Form
    {
        List<User> Users; // Список пользователей.

        public UserList()
        {
            InitializeComponent();
        }

        public static void Show(Form Owner)
        {
            UserList UL = new UserList();

            UL.LoadList();

            UL.ShowDialog(Owner);
            GC.Collect();
        }

        private void LoadList()
        {
            Users = Functions.GetUserList();
            UsersTable.DataSource = UsersToTable();
            UsersTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private DataTable UsersToTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID");
            dt.Columns.Add("ФИО");
            dt.Columns.Add("Лаборатория");
            dt.Columns.Add("Должность");

            foreach (User U in Users)
            {
                DataRow r = dt.NewRow();
                r["ID"] = U.ID.ToString("D3");
                r["ФИО"] = U.Surname + " " + U.Name + " " + U.SecondName;
                r["Лаборатория"] = U.Lab;
                r["Должность"] = U.Job;
                dt.Rows.Add(r);
            }

            return dt;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (EditUser.Add(this)) LoadList();
        }

        private void UsersTable_DoubleClick(object sender, EventArgs e)
        {
            if (UsersTable.SelectedRows.Count == 0) return;

            // Запрашиваем сервер и получаем ответ
            List<string> Answer = ServerCommunication.Send_Get_Msg_To_Server(
                Command: ServerCommunication.Commands.Search_Mol,
                Parameters: "user " + UsersTable.SelectedRows[0].Cells[0].Value);

           List<Molecule> Mols = Functions.GetMolListFromServerAnswer(Answer);

            MoleculesList ML = new MoleculesList();
            ML.DrawList(Mols);
            ML.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (UsersTable.SelectedRows.Count == 0) return;
            if (EditUser.Edit(this, UsersTable.SelectedRows[0].Cells[0].Value.To<int>()))
                LoadList();
        }
    }

    public class User
    {
        public int ID;
        public string Surname;
        public string Name;
        public string SecondName;
        public string Login;
        public string Lab;
        public string Job;
        public int Permissions;

        public User(string id, string surname, string name, string second_name, string login, 
            string laboratory, string job, string permissions)
        {
            ID = id.ToInt();
            Surname = surname.Trim();
            Name = name.Trim();
            SecondName = second_name.Trim();
            Login = login.Trim();
            Lab = laboratory.Trim();
            Job = job.Trim();
            Permissions = permissions.ToInt();
        }
    }

    // Класс сравнения
    public class Compare_User : IComparer<User>
    {
        string CompareField;
        bool Upwords;

        public Compare_User(string Field = "ID", bool upwords = true)
        {
            CompareField = Field;
            Upwords = upwords;
        }

        public int Compare(User x, User y)
        {
            int order = Upwords ? 1 : -1;
            switch (CompareField)
            {
                case "ID": 
                    if (x.ID == y.ID) return 0;
                    if (x.ID > y.ID) return 1 * order;
                    return -1 * order;

                case "FIO":
                    return (x.Surname + " " + x.Name + " " + x.SecondName).CompareTo(
                        y.Surname + " " + y.Name + " " + y.SecondName) * order;

                case "Lab":
                    return x.Lab.CompareTo(y.Lab) * order;

                case "Login":
                    return x.Login.CompareTo(y.Login) * order;

                case "Job":
                    return x.Job.CompareTo(y.Job) * order;

                case "Permissions":
                    if (x.Permissions == y.Permissions) return 0;
                    if (x.Permissions > y.Permissions) return -1 * order;
                    return 1 * order;
            }
            return 0;
        }
    }
}
