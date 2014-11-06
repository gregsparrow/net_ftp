using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using FTPLibrary;

namespace FTPTest
{
    class Program
    {
        static long size;
        static void Main(string[] args)
        {
            //коды ответов сервера
            //110 Комментарий 
            //120 Функция будет реализована через nnn минут 
            //125 Канал открыт, обмен данными начат 
            //150 Статус файла правилен, подготавливается открытие канала 
            //200 Команда корректна 
            //211 Системный статус или отклик на справочный запрос 
            //212 Состояние каталога 
            //213 Состояние файла 
            //214 Справочное поясняющее сообщение 
            //220 Слишком много подключений к FTP-серверу (можете попробовать позднее).
            //    В некоторых версиях указывает на успешное завершение промежуточной процедуры 
            //221 Благополучное завершение по команде quit 
            //225 Канал сформирован, но информационный обмен отсутствует 
            //226 Закрытие канала, обмен завершен успешно 
            //227 Entering Passive Mode (h1,h2,h3,h4,p1,p2).
            //230 Пользователь идентифицирован, продолжайте 
            //250 Запрос прошел успешно 
            //257 "PATHNAME" created.
            //331 Имя пользователя корректно, нужен пароль 
            //332 Для входа в систему необходима аутентификация 
            //350 Requested file action pending further information.
            //421 Процедура не возможна, канал закрывается 
            //425 Открытие информационного канала не возможно 
            //426 Канал закрыт, обмен прерван 
            //450 Запрошенная функция не реализована, файл не доступен, например, занят 
            //451 Локальная ошибка, операция прервана 
            //452 Ошибка при записи файла (не достаточно места) 
            //500 Синтаксическая ошибка, команда не может быть интерпретирована (возможно она слишком длинна) 
            //501 Синтаксическая ошибка (неверный параметр или аргумент) 
            //502 Команда не используется (нелегальный тип MODE) 
            //503 Неудачная последовательность команд 
            //504 Команда не применима для такого параметра 
            //530 Система не загружена (not logged in) 
            //532 Необходима аутентификация для запоминания файла 
            //550 Запрошенная функция не реализована, файл не доступен, например, не найден 
            //551 Requested action aborted. Page type unknown.
            //552 Запрошенная операция прервана, недостаточно выделено памяти
            //553 Requested action not taken. File name not allowed.

            //232 User logged in, authorized by security data exchange.
            //234 Security data exchange complete.
            //235 [ADAT=base64data]
            //; This reply indicates that the security data exchange
            //; completed successfully.  The square brackets are not
            //; to be included in the reply, but indicate that
            //; security data in the reply is optional.

            //334 [ADAT=base64data]
            //; This reply indicates that the requested security mechanism
            //; is ok, and includes security data to be used by the client
            //; to construct the next command.  The square brackets are not
            //; to be included in the reply, but indicate that
            //; security data in the reply is optional.
            //335 [ADAT=base64data]
            //; This reply indicates that the security data is
            //; acceptable, and more is required to complete the
            //; security data exchange.  The square brackets
            //; are not to be included in the reply, but indicate
            //; that security data in the reply is optional.
            //
            //336 Username okay, need password.  Challenge is "...."
            //; The exact representation of the challenge should be chosen
            //; by the mechanism to be sensible to the human user of the
            //; system.
            //
            //431 Need some unavailable resource to process security.
            //
            //533 Command protection level denied for policy reasons.
            //534 Request denied for policy reasons.
            //535 Failed security check (hash, sequence, etc).
            //536 Requested PROT level not supported by mechanism.
            //537 Command protection level not supported by security mechanism
            //521 data connection cannot be opened with this PROT setting
            //522 reply, which indicates that the TLS negotiation failed or was unacceptable

            //AUTH <SP> <mechanism-name> <CRLF>
            //ADAT <SP> <base64data> <CRLF>
            //PROT <SP> <prot-code> <CRLF>
            //PBSZ <SP> <decimal-integer> <CRLF>
            //MIC <SP> <base64data> <CRLF>
            //CONF <SP> <base64data> <CRLF>
            //ENC <SP> <base64data> <CRLF>
            //
            //<mechanism-name> ::= <string>
            //<base64data> ::= <string>
            //  ; must be formatted as described in section 9
            //<prot-code> ::= C | S | E | P
            //<decimal-integer> ::= any decimal integer from 1 to (2^32)-1
            /*
            Security Association Setup
                   AUTH
            234
            334
            502, 504, 534, 431
            500, 501, 421
                   ADAT
            235
            335
            503, 501, 535
            500, 501, 421
                Data protection negotiation commands
                   PBSZ
            200
            503
            500, 501, 421, 530
                   PROT
            200
            504, 536, 503, 534, 431
            500, 501, 421, 530
                Command channel protection commands
                   MIC
            535, 533
            500, 501, 421
                   CONF
            535, 533
            500, 501, 421
                   ENC
            535, 533
            500, 501, 421
                Security-Enhanced login commands (only new replies listed)
                   USER
            232
            336
                Data channel commands (only new replies listed)
                   STOR
            534, 535
                   STOU
            534, 535
                   RETR
            534, 535

                   LIST
            534, 535
                   NLST
            534, 535
                   APPE
            534, 535
          */
            /*
                  Connection Establishment
           120
              220
           220
           421
                  Login
           USER
              230
              530
              500, 501, 421
              331, 332
           PASS
              230
              202
              530
              500, 501, 503, 421
              332
           ACCT
              230
              202
              530
              500, 501, 503, 421
           CWD
              250
              500, 501, 502, 421, 530, 550
           CDUP
              200
              500, 501, 502, 421, 530, 550
           SMNT
              202, 250
              500, 501, 502, 421, 530, 550
                  Logout
           REIN
              120
           220
              220
              421
              500, 502
           QUIT
              221
              500

                  Transfer parameters
           PORT
              200
              500, 501, 421, 530
           PASV
              227
              500, 501, 502, 421, 530
           MODE
              200
              500, 501, 504, 421, 530
           TYPE
              200
              500, 501, 504, 421, 530
           STRU
              200
              500, 501, 504, 421, 530
             * 
                  File action commands
           ALLO
              200
              202
              500, 501, 504, 421, 530
           REST
              500, 501, 502, 421, 530
              350
           STOR
              125, 150
           (110)
           226, 250
           425, 426, 451, 551, 552
              532, 450, 452, 553
              500, 501, 421, 530
           STOU
              125, 150
           (110)
           226, 250
           425, 426, 451, 551, 552
              532, 450, 452, 553
              500, 501, 421, 530
           RETR
              125, 150
           (110)
           226, 250
           425, 426, 451
              450, 550
              500, 501, 421, 530

           LIST
              125, 150
           226, 250
           425, 426, 451
              450
              500, 501, 502, 421, 530
           NLST
              125, 150
           226, 250
           425, 426, 451
              450
              500, 501, 502, 421, 530
           APPE
              125, 150
           (110)
           226, 250
           425, 426, 451, 551, 552
              532, 450, 550, 452, 553
              500, 501, 502, 421, 530
           RNFR
              450, 550
              500, 501, 502, 421, 530
              350
           RNTO
              250
              532, 553
              500, 501, 502, 503, 421, 530
           DELE
              250
              450, 550
              500, 501, 502, 421, 530
           RMD
              250
              500, 501, 502, 421, 530, 550
           MKD
              257
              500, 501, 502, 421, 530, 550
           PWD
              257
              500, 501, 502, 421, 550
           ABOR
              225, 226
              500, 501, 502, 421


                  Informational commands
           SYST
              215
              500, 501, 502, 421
           STAT
              211, 212, 213
              450
              500, 501, 502, 421, 530
           HELP
              211, 214
              500, 501, 502, 421
                  Miscellaneous commands
           SITE
              200
              202
              500, 501, 530
           NOOP
              200
              500 421
             * 
             * MAIL, MSND
              151, 152
           354
              250
              451, 552
              354
           250
           451, 552
              450, 550, 452, 553
              500, 501, 502, 421, 530
           MSOM, MSAM
              119, 151, 152
           354
              250
              451, 552
              354
           250
           451, 552
              450, 550, 452, 553
              500, 501, 502, 421, 530
           MRSQ
              200, 215
              500, 501, 502, 421, 530
           MRCP
              151, 152
           200
              200
              450, 550, 452, 553
              500, 501, 502, 503, 421
            */
            //The following are the FTP commands:
            //
            //USER <SP> <username> <CRLF>*
            //PASS <SP> <password> <CRLF>*
            //ACCT <SP> <account-information> <CRLF>
            //CWD  <SP> <pathname> <CRLF>*
            //CDUP <CRLF>*
            //SMNT <SP> <pathname> <CRLF>
            //QUIT <CRLF>*
            //REIN <CRLF>
            //PORT <SP> <host-port> <CRLF>*
            //PASV <CRLF>*
            //TYPE <SP> <type-code> <CRLF>*
            //STRU <SP> <structure-code> <CRLF>*
            //MODE <SP> <mode-code> <CRLF>*
            //RETR <SP> <pathname> <CRLF>*
            //STOR <SP> <pathname> <CRLF>
            //STOU <CRLF>
            //APPE <SP> <pathname> <CRLF>
            //ALLO <SP> <decimal-integer>
            //    [<SP> R <SP> <decimal-integer>] <CRLF>
            //REST <SP> <marker> <CRLF>
            //RNFR <SP> <pathname> <CRLF>*
            //RNTO <SP> <pathname> <CRLF>*
            //ABOR <CRLF>*
            //DELE <SP> <pathname> <CRLF>*
            //RMD  <SP> <pathname> <CRLF>*
            //MKD  <SP> <pathname> <CRLF>*
            //PWD  <CRLF>*
            //LIST [<SP> <pathname>] <CRLF>*
            //NLST [<SP> <pathname>] <CRLF>*
            //SITE <SP> <string> <CRLF>
            //SYST <CRLF>*
            //STAT [<SP> <pathname>] <CRLF>
            //HELP [<SP> <string>] <CRLF>
            //NOOP <CRLF>*
            //MLFL [<SP> <ident>] <CRLF>
            //MAIL [<SP> <ident>] <CRLF>
            //MSND [<SP> <ident>] <CRLF>
            //MSOM [<SP> <ident>] <CRLF>
            //MSAM [<SP> <ident>] <CRLF>
            //MRSQ [<SP> <scheme>] <CRLF>
            //MRCP <SP> <ident> <CRLF>

            //Entering Passive Mode (194,87,5,52,9,79) 
            //194.87.5.52 - IP адрес
            //2383 - номер порт, расчет порта 9*256+79=2383

            //Ты говоришь PASV, ждешь ответа, коннектишься дата-соединением,
            //шлешь STOR, ждешь ответ, шлешь файл, ждешь ответ, закрываешь data сокет,
            //говоришь QUIT и закрываешь control соединение. По-моему так.

            //Как известно, при работе с FTP сервером используются два соединения — одно для передачи команд,
            //второе данных. При этом первое соединение существует всегда, 
            //второе открывается и закрывается по приему данных.
            //В активном режиме сервер открывает соединение данных с клиентом (на клиенте accept),
            //в пассивном клиент открывает соединение данных к серверу (на клиенте connect).
            //http://www.rsdn.ru/forum/message/653880.1.aspx


            //[10:38:45] SmartFTP v2.5.1006.48
            //[10:38:51] Resolving host name "ftp.ukrainebride.net"
            //[10:38:51] Connecting to 207.234.155.33 Port: 21
            //[10:38:51] Connected to ftp.ukrainebride.net.
            //[10:39:02] 220 ftp.ukrainebride.net FTP Server ready
            //[10:39:02] USER adminub@ukrainebride.net
            //[10:39:02] 331 Password required for adminub@ukrainebride.net.
            //[10:39:02] PASS (hidden)
            //[10:39:03] 230 User adminub@ukrainebride.net logged in.
            //[10:39:03] SYST
            //[10:39:04] 215 UNIX Type: L8
            //[10:39:04] Detected Server Type: UNIX
            //[10:39:04] FEAT
            //[10:39:04] 211-Features:
            //[10:39:05]  MDTM
            //[10:39:05]  REST STREAM
            //[10:39:05]  SIZE
            //[10:39:05] 211 End
            //[10:39:05] PWD
            //[10:39:05] 257 "/home/adminub" is current directory.
            //[10:39:05] CWD /var/www/html/images
            //[10:39:07] 250 CWD command successful
            //[10:39:07] PWD
            //[10:39:07] 257 "/var/www/html/images" is current directory.
            //[10:39:07] TYPE A
            //[10:39:07] 200 Type set to A
            //[10:39:07] PASV
            //[10:39:08] 227 Entering Passive Mode (207,234,155,33,155,216).
            //[10:39:08] Opening data connection to 207.234.155.33 Port: 39896
            //[10:39:08] LIST -aL
            //[10:39:08] 150 Opening ASCII mode data connection for file list
            //[10:39:11] 17532 bytes transferred. (5,42 KB/s) (00:00:03)
            //[10:39:11] 226 Transfer complete.
            //[10:40:02] NOOP
            //[10:40:03] 200 NOOP command successful
            //[10:40:53] NOOP
            //[10:40:53] 200 NOOP command successful
            //[10:41:44] NOOP
            //[10:41:44] 200 NOOP command successful
            //[10:42:35] NOOP
            //[10:42:36] 200 NOOP command successful
            //[10:43:26] NOOP
            //[10:43:27] 200 NOOP command successful
            //[10:44:12] 421 No Transfer Timeout (300 seconds): closing control connection.
            //[10:44:13] An established connection was aborted by the software in your host machine.
            //[10:44:13] Server closed connection

            try
            {


                //string host = "nssdcftp.gsfc.nasa.gov";
                //string host = "ftp.nec.com";
                //string host = "ftp.caldera.com";
                //string host = "ftp.netscape.com";
                //string host = "ftp.adobe.com";
                //string host = "ftp.borland.com";
                //string host = "ftp.rarlabs.com";
                //string host = "ftp.wiley.com";
                //string host = "ftp.qualcomm.com";
                //string host = "download.nvidia.com";
                //string host = "risc.ua.edu";
                //string host = "ftp.corel.com";
                string host = "ftp.symantec.com";
                //string host = "ftp.mcafee.com";
                //string host = "ftp.us.nero.com";
                //string host = "ftp.microsoft.com";
                //string host = "ftp.winzip.com";
                //string host = "ftp.funet.fi";



                int port = 21;

                //host = "cosine-systems.com";
                //host = "ftp.ring.gr.jp";
                string username = "anonymous";
                string pass = "anonymous@gmail.com";

                FTP home = new FTP();
                //home.ConnectionType = ConnectionType.SecureConnection;
                home.ConnectionMode = ConnectionMode.Passive;
                //home.ConnectionMode = ConnectionMode.Active;
                home.OnServerResponse += new ServerResponseEventHendler(ftp_OnServerResponse);
                home.OnClientCommand += new ClientCommandEventHendler(ftp1_OnClientCommand);
                home.OnSpeed += new SpeedEventHendler(home_OnSpeed);
                home.OnFileTransfer += new FileTransferEventHendler(home_OnFileTransfer);
                home.Open(host, port);
                home.Login(username, pass);

                //size = home.GetFileSize("[l33t-raws]Nodame_Cantabile_01_(1280x720_XviD).[83A19B7F].avi");

                //FileStream fs = new FileStream(@"D:\projects\FTP\test.avi", FileMode.OpenOrCreate, FileAccess.Write);
                //home.Download("[l33t-raws]Nodame_Cantabile_01_(1280x720_XviD).[83A19B7F].avi", ref fs);

                List<ContentItemInformation> dirInfo = new List<ContentItemInformation>();

                //home.ChangeCurrentWorkDirectory("/var/www/html/test");
                //home.ChangeCurrentWorkDirectory("/bleach");
                //home.GetDirectoryContent("/var/www/html/test/", ref dirInfo);
                home.GetDirectoryContent("/", ref dirInfo);




                /*foreach (ContentItemInformation var in dirInfo)
                {
          Console.WriteLine("{0} {1,25} {2} {3} {4,10} {5}", var.Group, var.LastChange, var.Owner, var.Rights, var.Size, var.Name);
                }*/
                foreach (ContentItemInformation var in dirInfo)
                {
                    Console.WriteLine("{4,-10} {0,-20} {1,-20} {2,10} {3}", var.Created, var.LastChange, var.Size, var.Name, var.ItemType);
                }

                home.LogOut();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        static void home_OnFileTransfer(int totalTransferedBytes)
        {
            /*
            if (totalTransferedBytes != 0)
            {
                StringBuilder sb = new StringBuilder();
                double result = totalTransferedBytes * 100 / size;
                sb.AppendFormat("{2} : {0} : {1}", totalTransferedBytes, totalTransferedBytes * 100 / size, size);
                Console.Title = sb.ToString();
            }
             */
        }

        static void home_OnSpeed(FTP ftp)
        {
            Console.Title = ftp.Speed(SpeedPerSecond.KiB).ToString();
        }


        static void ftp1_OnClientCommand(string command)
        {
            Console.Write(command);
        }

        static void ftp_OnServerResponse(string response)
        {
            Console.WriteLine(response);
        }
    }
}
