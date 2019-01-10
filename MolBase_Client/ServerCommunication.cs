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

            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            senderSocket.Connect(ipEndPoint);

            string Login = UserLogin == "" ? "NoUser" : UserLogin;
            string SessionCode = UserSecureCode == "" ? "NoUserID" : UserSecureCode;

            byte[] msg = Encoding.UTF8.GetBytes(Command + "\n" + Login + "\n" +
                SessionCode + "\n" + Parameters + " ");

            // Отправляем данные через сокет
            byte[] msg_size = BitConverter.GetBytes(msg.Length);
            int bytesSentS = senderSocket.Send(msg_size);
            int bytesSent = senderSocket.Send(msg);
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

            // Получаем ответ от сервера
            if (Command == Commands.Login)
            {
                Crypt = (AES_Data)AES_Data.FromBin(RecieveData(senderSocket));
            }
            Res = Get_ListString_from_bytes(senderSocket);

            // Освобождаем сокет
            senderSocket.Shutdown(SocketShutdown.Both);
            senderSocket.Close();

            if (Res[1] == Answers.NoLogin)
            {
                Form1.CurUser = null;
            }

            return Res;
        }

        private static MemoryStream RecieveData(Socket senderSocket)
        {
            MemoryStream GotData = new MemoryStream();
            byte[] FSize = new byte[BitConverter.GetBytes((int)0).Length];
            int Got = senderSocket.Receive(FSize);
            int FileSize = BitConverter.ToInt32(FSize, 0);
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
        /// Принимает бинарный ответ от сервера и превращает его в List(string)
        /// </summary>
        /// <param name="senderSocket">Сокет, с которого получаем ответ</param>
        /// <returns></returns>
        private static List<string> Get_ListString_from_bytes(Socket senderSocket)
        {
            List<string> Res = new List<string>();
            MemoryStream MS = RecieveData(senderSocket);
            byte[] bytes = new byte[MS.Length];
            MS.Read(bytes, 0, bytes.Length);
            using (FileStream FS = new FileStream("temp.dat", FileMode.Create))
            {
                FS.Write(bytes, 0, bytes.Length);
                FS.Close();
            };
            string ResList = Crypt.DecryptStringFromBytes(bytes);
            bool Stop = false;
            Res.AddRange(Parce_StringList(ref Stop, ResList));

            return Res;
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


            // Соединяемся с удаленным устройством
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            senderSocket.Connect(ipEndPoint);


            byte[] msg = Encoding.UTF8.GetBytes("<@*Send_File*@>" + "\n" + UserLogin + "\n" +
                UserSecureCode + "\n" +
                ID.ToString() + "\n" + " ");

            // Отправляем данные через сокет
            int bytesSentS = senderSocket.Send(BitConverter.GetBytes(msg.Length));
            int bytesSent = senderSocket.Send(msg);

            // Получаем длину текстовой записи
            byte[] SL_Size = new byte[4];
            senderSocket.Receive(SL_Size);
            int StringList_Size = BitConverter.ToInt32(SL_Size, 0);

            // Получаем текстовую запись
            byte[] SL = new byte[StringList_Size];
            senderSocket.Receive(SL);
            bool Stop = false;
            List<string> F_Size = Parce_StringList(ref Stop, Encoding.UTF8.GetString(SL, 0, StringList_Size));

            // Получаем и записываем в файл по кусочкам
            FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write);

            int FtR_Size = Convert.ToInt32(F_Size[2]);
            for (int i = 0; i < FtR_Size; i += 1024)
            {
                int block;
                if (FtR_Size - i < 1024) { block = FtR_Size - i; }
                else { block = 1024; }
                byte[] buf = new byte[block];
                senderSocket.Receive(buf);
                fs.Write(buf, 0, block);
                fs.Flush();
            }
            fs.Close();

            // Освобождаем сокет
            senderSocket.Shutdown(SocketShutdown.Both);
            senderSocket.Close();
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
    }

    public class ServerAnswers
    {
        public string StartMsg = "<@Begin_Of_Session@>"; // Начало сессии передачи ответа сервера
        public string EndMsg = "<@End_Of_Session@>";     // Конец сессии передачи ответа сервера
        public string NoLogin = "<Error 100: No such loged in user>";     // Ответ сервера о том, что имя пользователя-пароль не найдены
        public string LoginOK = "<@Login_OK@>";           // Ответ сервера об успешном входе в систему
    }

    public class ServerCommands
    {
        public string FN_msg = "file.name";         // Команда получения имени файла
        public string QuitMsg = "account.quit";           // Команда на выход пользователя
        public string Add_User = "user.add";                 // Команда добавления пользователя
        public string Login = "account.login";           // Команда входа в систему
        public string Add_Mol = "molecules.add";       // Команда на добавление молекулы
        public string Search_Mol = "molecules.search";    // Команда поиска молекулы
        public string Increase_Status = "status.increase"; // Увеличеть значение статуса соединения
        public string Show_New_Mol = "molecules.search";  // Команда показать все молекулы новые
        public string Show_New_Mol_Param = "status 1";  // Команда показать все молекулы новые
        public string GetStatuses = "status.list";      // Выдать список статусов
    }
}
