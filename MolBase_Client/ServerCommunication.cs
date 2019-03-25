using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MoleculeDataBase;

namespace MolBase_Client
{
    public static class ServerCommunication
    {
        private static readonly string IP_Server = "195.19.140.174";     // IP сервера
        private static readonly IPAddress ipAddr = IPAddress.Parse(IP_Server);
        private static readonly int port = 11000;
        private static readonly IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);
        public static ServerAnswers Answers = new ServerAnswers();
        public static ServerCommands Commands = new ServerCommands();
        public static string UserLogin = "";
        public static string UserSecureCode = "";
        private static AES_Data Crypt;

        /// <summary>
        /// Посылает запрос на сервер от имени пользователя и выдаёт его ответ
        /// </summary>
        /// <param name="CurUser">Пользователь, от имени которого подаётся запрос</param>
        /// <param name="Command">Команда запроса</param>
        /// <param name="Parameters">Параметры запроса</param>
        /// <param name="BinarInfo">Бинарная часть запроса</param>
        /// <returns></returns>
        static public List<string> Send_Get_Msg_To_Server(string Command, 
            string Parameters = "", byte[] BinarInfo = null)
        {
            // Буфер для входящих данных
            List<string> Res;

            // Получаем ответ от сервера
            if (Command == Commands.Login)
            {
                UserInfo UI = Login(new string[] { Parameters });
                Crypt = UI?.Key;
                Res = UI == null
                    ? new List<string>() { Answers.StartMsg, "", "NoUserID", Answers.EndMsg }
                    : Get_ListString_from_bytes(UI.Info);
            }
            else
            {
                MemoryStream Answer = Query(Command, new string[] { Parameters }, BinarInfo);
                Res = Get_ListString_from_bytes(Answer);
            }

            if (Res[1] == Answers.NoLogin)
            {
                Form1.CurUser = null;
            }

            return Res;
        }

        private static MemoryStream RecieveData(Socket senderSocket, int Length=0)
        {
            MemoryStream GotData = new MemoryStream();
            int FileSize;
            // Если сервер высылает размер передаваемого блока, то примем его
            if (Length == 0)
            {
                byte[] FSize = new byte[BitConverter.GetBytes((int)0).Length];
                int Got = senderSocket.Receive(FSize);
                FileSize = BitConverter.ToInt32(FSize, 0);
            }
            else FileSize = Length;

            // Получим сообщение
            for (int i = 0; i < FileSize; i += 1024)
            {
                int BytesSize = FileSize - i > 1024
                    ? 1024
                    : FileSize - i;
                byte[] bytes = new byte[BytesSize];
                int bytesRec = senderSocket.Receive(bytes);
                GotData.Write(bytes, 0, bytes.Length);
            }
            GotData.Position = 0;
            return GotData;
        }

        /// <summary>
        /// Превращает бинарный поток ответа в List
        /// </summary>
        /// <param name="senderSocket">Сокет, с которого получаем ответ</param>
        /// <returns></returns>
        private static List<string> Get_ListString_from_bytes(MemoryStream Answer)
        {
            List<string> Res = new List<string>();
            byte[] bytes = new byte[Answer.Length];
            Answer.Read(bytes, 0, bytes.Length);
            string ResList = Crypt.DecryptStringFromBytes(bytes);
            bool Stop = false;
            Res.AddRange(Parce_StringList(ref Stop, ResList));

            return Res;
        }

        /// <summary>
        /// Принимает бинарный ответ от сервера и превращает его в List(string)
        /// </summary>
        /// <param name="senderSocket">Сокет, с которого получаем ответ</param>
        /// <returns></returns>
        private static List<string> Get_ListString_from_bytes(Socket senderSocket)
        {
            return Get_ListString_from_bytes(RecieveData(senderSocket));
        }

        /// <summary>
        /// Получить файл с сервера
        /// </summary>
        /// <param name="ID">Номер файла на сервере</param>
        /// <param name="InFileName">Имя по умолчанию, с которым предлагается сохранять файл. 
        /// В случае отсутствия предлагается исходное название файла.</param>
        public static void GetFile(int ID, string InFileName = "")
        {
            if (UserLogin == "") return;

            string GotFN = InFileName;
            if (InFileName == "")
            {
                List<string> FN = Send_Get_Msg_To_Server(Commands.FN_msg, ID.ToString());
                GotFN = FN[1];
            }

            // Спрашиваем, куда сохранить. Если отменяем, то отменяем полностью, не спрашивая сервер.
            string FileName = Functions.SaveFileAs(GotFN);
            if (FileName == "<Cancel>")
            {
                return;
            }     // Если пользователь отменил, то прекращаем всё.

            // Запос на получение файла
            Socket senderSocket = SendQuery("file.get", new string[] { ID.ToString() });
            //List<string> Answer = Get_ListString_from_bytes(senderSocket);

            using (MemoryStream Ans = RecieveData(senderSocket))
            {
                using (FileStream FS = new FileStream("temp.dat", FileMode.Create))
                {
                    FS.Write(Crypt.AesKey, 0, Crypt.AesKey.Count());
                    FS.Write(Crypt.AesIV, 0, Crypt.AesIV.Count());
                    FS.Close();
                }
                MemoryStream MS =
                    new MemoryStream(Crypt.DecryptBytes(Ans));
                // Получаем и записываем в файл по кусочкам
                FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write);
                MS.CopyTo(fs);
                fs.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Stop"></param>
        /// <param name="ResList"></param>
        /// <returns></returns>
        private static List<string> Parce_StringList(ref bool Stop, string ResList)
        {
            string ResMsg = "";
            List<string> Res = new List<string>();
            string[] ResArray = ResList.Split("\n"[0]);
            for (int i = 0; i < ResArray.Count(); i++)
            {
                ResMsg = ResArray[i];
                if (ResMsg == "") { continue; };
                if (ResMsg == "<@None@>") { Res.Add(""); continue; };
                Res.Add(ResMsg);
                if (ResMsg == Answers.EndMsg) { Stop = true; };
            }
            return Res;
        }

        /// <summary>
        /// Подключается к сокету
        /// </summary>
        /// <returns></returns>
        private static Socket Connect()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            senderSocket.Connect(ipEndPoint);

            return senderSocket;
        }

        /// <summary>
        /// Посылает запрос с параметрами на сервер и выдаёт битовый поток ответа сервера
        /// </summary>
        /// <param name="Query">Основная команда серверу</param>
        /// <param name="Params">Параметры команды</param>
        /// <returns></returns>
        private static MemoryStream Query(string Query, string[] Params, byte[] BinarInfo = null)
        {
            // Отправим запрос на сервер
            Socket senderSocket = SendQuery(Query, Params, BinarInfo);

            // Получим ответ от сервера
            MemoryStream MS = RecieveData(senderSocket);

            // Освобождаем сокет
            senderSocket.Shutdown(SocketShutdown.Both);
            senderSocket.Close();

            // Возвращаем поток ответа
            return MS;
        }

        private static UserInfo Login(string[] Params)
        {
            UserInfo UI = new UserInfo();

            // Запос на вход
            Socket senderSocket = SendQuery(Commands.Login, Params);

            // Если возникла любая ошибка, то вход не выполнени
            try
            {
                // Получаем ключ
                UI.Key = (AES_Data)Serializable.FromBin(RecieveData(senderSocket));
                // Получаем данные
                UI.Info = RecieveData(senderSocket);
            }
            catch
            {
                UI = null;
            }
            
            // Освобождаем сокет
            senderSocket.Shutdown(SocketShutdown.Both);
            senderSocket.Close();

            return UI;
        }

        /// <summary>
        /// Отправка запроса на сервер с оставлением открытой сессии взаимодействия с сервером для приёма ответов
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="Params"></param>
        /// <param name="BinarInfo"></param>
        /// <returns></returns>
        private static Socket SendQuery(string Query, string[] Params, byte[] BinarInfo = null)
        {
            Socket senderSocket = Connect();
            string Login = UserLogin == "" ? "NoUser" : UserLogin;
            string SessionCode = UserSecureCode == "" ? "NoUserID" : UserSecureCode;

            string FullQuery = Query + "\n" + Login + "\n" + SessionCode;
            foreach (string Param in Params)
                FullQuery += "\n" + Param;
            byte[] msg = Encoding.UTF8.GetBytes(FullQuery);

            // Отправляем данные через сокет
            int bytesSentS = senderSocket.Send(BitConverter.GetBytes(msg.Length));
            int bytesSent = senderSocket.Send(msg);

            // Если есть бинарные данные, отправим и их
            if (BinarInfo != null)
            {
                MemoryStream ms = new MemoryStream(BinarInfo);
                for (int i = 0; i < BinarInfo.Count(); i += 1024)
                {
                    int Size = 1024;
                    if (BinarInfo.Count() - i < 1024) { Size = BinarInfo.Count() - i; }
                    byte[] Block = new byte[Size];
                    ms.Read(Block, 0, Size);
                    senderSocket.Send(Block);
                }
            }

            return senderSocket;
        }

        public static List<T> GetTransportList<T>(string Query, string[] Params)
        {
            List<T> Res = new List<T>();
            Socket senderSocket = SendQuery(Query, Params);
            bool Stop = false;
            do
            {
                MemoryStream ms = new MemoryStream(Crypt.DecryptBytes(RecieveData(senderSocket)));
                ms.Position = 0;
                if (ms.Length > 1) Res.Add((T)Serializable.FromSOAP(ms));
                else Stop = true;
            }
            while (!Stop);

            return Res;
        }
    }

    public class ServerAnswers
    {
        public string StartMsg = "<@Begin_Of_Session@>";    // Начало сессии передачи ответа сервера
        public string EndMsg = "<@End_Of_Session@>";        // Конец сессии передачи ответа сервера
        public string NoLogin = "<Error 100: No such loged in user>";     // Ответ сервера о том, что имя пользователя-пароль не найдены
        public string LoginOK = "<@Login_OK@>";             // Ответ сервера об успешном входе в систему
    }

    public class ServerCommands
    {
        public string FN_msg = "file.name";                 // Команда получения имени файла
        public string File_Get = "file.get";                //Получить файл от сервера
        public string QuitMsg = "account.quit";             // Команда на выход пользователя
        public string Add_User = "user.add";                // Команда добавления пользователя
        public string Login = "account.login";              // Команда входа в систему
        public string Add_Mol = "molecules.add";            // Команда на добавление молекулы
        public string Search_Mol = "molecules.search";      // Команда поиска молекулы
        public string Increase_Status = "status.increase";  // Увеличеть значение статуса соединения
        public string Show_New_Mol = "molecules.search";    // Команда показать все молекулы новые
        public string Show_New_Mol_Param = "status 1";      // Команда показать все молекулы новые
        public string GetStatuses = "status.list";          // Выдать список статусов
    }

    class UserInfo
    {
        public AES_Data Key;
        public MemoryStream Info;
    }
}
