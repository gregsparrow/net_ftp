using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace FTPLibrary
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="response">Response of the server</param>
    public delegate void ServerResponseEventHendler(string response);

    /// <summary>
    /// <param name="command">Commands of the client</param>
    /// </summary>
    public delegate void ClientCommandEventHendler(string command);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ftp">Current FTP instance</param>
    public delegate void ReConnectEventHendler(FTP ftp);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ftp">Current FTP instance</param>
    public delegate void SpeedEventHendler(FTP ftp);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="totalTransferedBytes"></param>
    public delegate void FileTransferEventHendler(int totalTransferedBytes);

    public partial class FTP
    {
        /// <summary>
        /// Connection which are sent commands
        /// </summary>
        private TcpClient ftp;

        /// <summary>
        /// Data transfer connection in passive mode
        /// </summary>
        private TcpClient dtpPassive;

        /// <summary>
        /// Data transfer connection in active mode
        /// </summary>
        private TcpListener dtpActive;

        /// <summary>
        /// Stream which are sent commands
        /// </summary>
        private NetworkStream iostream;

        /// <summary>
        /// Data transfer stream
        /// </summary>
        private NetworkStream DATA_iostream;

        /// <summary>
        /// Passive or Active connection mode
        /// </summary>
        private ConnectionMode mode;

        /// <summary>
        /// Secure connection or default
        /// </summary>
        private ConnectionType conType;

        /// <summary>
        /// The encoding of server response, default ASCII
        /// </summary>
        private Encoding encoding;

        /// <summary>
        /// Port
        /// </summary>
        private int port;

        /// <summary>
        /// Host name
        /// </summary>
        private string host;

        private string username;
        private string password;
        private string account_information;

        private _TYPE currentType = _TYPE.none;
        private int speed, totalDownloadedBytes;
        private Dictionary<string, int> monthName;
        private System system;
        private NetworkProtocol passiveNetPrt = NetworkProtocol.IPv4, activeNetPrt = NetworkProtocol.IPv4;
        private List<int> notSupportedProtocols = new List<int>();
        private Timer noopTimer, speedTimer;
        
        /// <summary>
        /// Tracking all the responses server
        /// </summary>
        public event ServerResponseEventHendler OnServerResponse;
        /// <summary>
        /// Tracking all the commands client
        /// </summary>
        public event ClientCommandEventHendler OnClientCommand;
        /// <summary>
        /// Occurs when login incorrect
        /// </summary>
        public event ReConnectEventHendler OnReConnect;
        /// <summary>
        /// called once per second
        /// </summary>
        public event SpeedEventHendler OnSpeed;
        /// <summary>
        /// called when downloading or uploading files
        /// </summary>
        public event FileTransferEventHendler OnFileTransfer;

        /// <summary>
        /// Create ftp instance
        /// </summary>
        public FTP()
        {
            ftp = new TcpClient();

            dtpPassive = new TcpClient();

            OnServerResponse += new ServerResponseEventHendler(FTP_OnServerResponse);
            OnClientCommand += new ClientCommandEventHendler(FTP_OnClientCommand);
            OnReConnect += new ReConnectEventHendler(FTP_OnReConnect);
            OnSpeed += new SpeedEventHendler(FTP_OnSpeed);
            OnFileTransfer += new FileTransferEventHendler(FTP_OnFileTransfer);

            encoding = Encoding.ASCII;
            mode = ConnectionMode.Passive;
            conType = ConnectionType.DefaultConnection;

            monthName = new Dictionary<string, int>();
            monthName.Add("JAN", 1);
            monthName.Add("FEB", 2);
            monthName.Add("MAR", 3);
            monthName.Add("APR", 4);
            monthName.Add("MAY", 5);
            monthName.Add("JUN", 6);
            monthName.Add("JUL", 7);
            monthName.Add("AUG", 8);
            monthName.Add("SEP", 9);
            monthName.Add("OCT", 10);
            monthName.Add("NOV", 11);
            monthName.Add("DEC", 12);
        }

        void FTP_OnFileTransfer(int totalTransferedBytes){}

        void FTP_OnSpeed(FTP ftp) { }

        void FTP_OnReConnect(FTP ftp){}

        void FTP_OnClientCommand(string command){}

        void FTP_OnServerResponse(string response){}

        /// <summary>
        /// Get current speed
        /// </summary>
        /// <param name="speedpersec">speed</param>
        /// <returns>Return speed in specified type</returns>
        public double Speed(SpeedPerSecond speedpersec)
        {
            return SpeedConvert(speedpersec, speed);
        }

        /// <summary>
        /// Open FTP connection
        /// </summary>
        /// <param name="hostname">Host name</param>
        /// <param name="port">Port</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Open(string hostname, int port)
        {
            noopTimer = new Timer(new TimerCallback(DoNOOP));
            noopTimer.Change(60000, 30000);

            speedTimer = new Timer(new TimerCallback(DoTimer));
            speedTimer.Change(0, 1000);

            if (string.IsNullOrEmpty(hostname))
            {
                throw new ArgumentNullException("hostname", "Cannot be empty or null");
            }

            this.host = hostname;
            this.port = port;



            TcpClient testAddressFamily = new TcpClient();
            testAddressFamily.Connect(hostname, port);
            switch (testAddressFamily.Client.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    passiveNetPrt = NetworkProtocol.IPv4;
                    break;
                case AddressFamily.InterNetworkV6:
                    passiveNetPrt = NetworkProtocol.IPv6;
                    break;
            }
            testAddressFamily.Close();

            switch (Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    activeNetPrt = NetworkProtocol.IPv4;
                    break;
                case AddressFamily.InterNetworkV6:
                    activeNetPrt = NetworkProtocol.IPv6;
                    break;
            }
            


            ConnectTo(hostname, port, ref ftp, ref iostream);

            byte[] response = ReadServerResponseMultiline(ref iostream);

            if (ServerResponseCode(response).Trim().Equals("220"))
            {
                OnServerResponse(Encoding.ASCII.GetString(response));
            }
            else
            {
                throw new Exception(Encoding.ASCII.GetString(response));
            }
        }

        /// <summary>
        /// login to the server
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="password">password</param>
        /// <exception cref="IOException"></exception>
        /// <returns>true if logged otherwise false</returns>
        public bool Login(string username, string password)
        {
            this.username = username;
            this.password = password;

            switch (conType)
            {
                case ConnectionType.SecureConnection:
                    try
                    {
                        if (AUTH("TLS", ref iostream).Equals("234"))
                        {
                            _TLS = true;
                        }
                    }
                    catch (ServerResponseException ex)
                    {
                        OnServerResponse(ex.Message);
                    }
                    break;
            }

            try
            {
                USER(username, ref iostream);
                PASS(password, ref iostream);
            }
            catch (_530_not_logged_exception ex)
            {
                OnServerResponse(ex.Message);

                OnReConnect(this);

                return false;
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);

                return false;
            }

            try
            {
                string result_syst = SYST(ref iostream);
                if (result_syst.IndexOf("Windows") > -1)
                {
                    system = System.Windows;
                }
                if (result_syst.IndexOf("UNIX") > -1)
                {
                    system = System.UNIX;
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
            }

            SetType(_TYPE.ASCII, ref iostream);

            try
            {
                CheckFEAT(FEAT(ref iostream));
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
            }

            if (_CLNT)
            {
                try
                {
                    CLNT("FTPLibrary", ref iostream);
                }
                catch (ServerResponseException ex)
                {
                    OnServerResponse(ex.Message);
                }
            }
            if (_UTF8)
            {
                try
                {
                    if (ServerResponseCode(OPTS("UTF8 ON", ref iostream)).Trim().Equals("200"))
                    {
                        encoding = Encoding.UTF8;
                    }
                }
                catch (ServerResponseException ex)
                {
                    OnServerResponse(ex.Message);
                }
            }
            if (_MLST)
            {
                try
                {
                    mlst_string = GetString(OPTS(mlst_string, ref iostream));
                }
                catch (ServerResponseException ex)
                {
                    OnServerResponse(ex.Message);
                }
            }

            return true;
        }

        /// <summary>
        /// Disconnect from FTP server
        /// </summary>
        /// <exception cref="IOException"></exception>
        public void LogOut()
        {
            try
            {
                QUIT(ref iostream, ref ftp);
                currentType = _TYPE.none;
                noopTimer.Dispose();
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
            }
        }

        /// <summary>
        /// Opens and Login again, using stored parameters
        /// </summary>
        /// <exception cref="IOException"></exception>
        public void ReConnect()
        {
            DicsonnectFrom(ref ftp, ref iostream);
            currentType = _TYPE.none;
            
            Open(host, port);
            Login(username, password);
        }

        /// <summary>
        /// Get current work directory
        /// </summary>
        /// <returns>Directory name</returns>
        /// <exception cref="IOException"></exception>
        public string GetCurrentWorkDirectory()
        {
            try
            {
                return PWD(ref iostream);
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);

                return "";
            }
        }

        /// <summary>
        /// Change work directory
        /// </summary>
        /// <param name="directory">Namo of directory</param>
        /// <returns>Return true if the directory is changed otherwise false</returns>
        /// <exception cref="IOException"></exception>
        public bool ChangeCurrentWorkDirectory(string directory)
        {
            try
            {
                if (CWD(directory, ref iostream).Equals("250"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Go to parent of current directory
        /// </summary>
        /// <returns>Return true if the directory is changed otherwise false</returns>
        /// <exception cref="IOException"></exception>
        public bool GotoParentOfCurrentDirectory()
        {
            try
            {
                if (CDUP(ref iostream).Equals("250"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Create directory
        /// </summary>
        /// <param name="directory">Name of directory</param>
        /// <returns>Return true if the directory is created otherwise false</returns>
        /// <exception cref="IOException"></exception>
        public bool CreateDirectory(string directory)
        {
            try
            {
                if (MKD(directory, ref iostream).Equals("257"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Remove directory on FTP server
        /// </summary>
        /// <param name="directory">Directory name for removing</param>
        /// <returns>Return true if the directory is removed otherwise false</returns>
        /// <exception cref="IOException"></exception>
        public bool RemoveDirectory(string directory)
        {
            try
            {
                if (RMD(directory, ref iostream).Equals("250"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Get directory content.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="itemCollection"></param>
        /// <exception cref="IOException"></exception>
        public void GetDirectoryContent(string path, ref List<ContentItemInformation> itemCollection)
        {
            try
            {
                itemCollection.Clear();

                SetType(_TYPE.ASCII, ref iostream);

                if (mode == ConnectionMode.Passive)
                {
                    PassiveConnectionInfo pInfo = new PassiveConnectionInfo();

                    if (_EPSV)
                    {
                        try
                        {
                            pInfo = EPSV(passiveNetPrt, ref iostream);
                        }
                        catch (ServerResponseException ex)
                        {
                            OnServerResponse(ex.Message);
                            _EPSV = false;
                        }

                    }
                    if (!_EPSV)
                    {
                        pInfo = PASV(ref iostream);
                    }

//                  ConnectTo(pInfo.ip, pInfo.port, ref dtpPassive, ref DATA_iostream);
                    ConnectTo(host, pInfo.port, ref dtpPassive, ref DATA_iostream); //trick
                }
                else if (mode == ConnectionMode.Active)
                {
                    if (_EPRT)
                    {/*
                    Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint ep = new IPEndPoint(((IPEndPoint)ftp.Client.LocalEndPoint).Address, ((IPEndPoint)ftp.Client.LocalEndPoint).Port + 1);
                    listener.Bind(ep);
                    listener.Listen(1);

                    DATA_iostream = new NetworkStream(listener.Accept(), FileAccess.ReadWrite);
                    PORT(((IPEndPoint)ftp.Client.LocalEndPoint).Address, ((IPEndPoint)ftp.Client.LocalEndPoint).Port, ref iostream);
                    */

                        dtpActive = new TcpListener(((IPEndPoint)ftp.Client.LocalEndPoint).Address, ((IPEndPoint)ftp.Client.LocalEndPoint).Port + 1);
                        dtpActive.Start();
                        PORT(((IPEndPoint)ftp.Client.LocalEndPoint).Address, ((IPEndPoint)ftp.Client.LocalEndPoint).Port + 1, ref iostream);

                        Console.WriteLine("Waiting for a connection on {0}:{1} ... ", ((IPEndPoint)ftp.Client.LocalEndPoint).Address, ((IPEndPoint)ftp.Client.LocalEndPoint).Port + 1);
                        TcpClient tmpTcpCl = dtpActive.AcceptTcpClient();
                        Console.WriteLine("Connected!");

                        // DoEPRT(activeNetPrt, ref notSupportedProtocols);

                        DATA_iostream = tmpTcpCl.GetStream();
                    }
                    if (!_EPRT)
                    {
                        PORT(((IPEndPoint)ftp.Client.LocalEndPoint).Address, ((IPEndPoint)ftp.Client.LocalEndPoint).Port + 1, ref iostream);
                    }
                    //AcceptFrom(((IPEndPoint)ftp.Client.LocalEndPoint).Address, ((IPEndPoint)ftp.Client.LocalEndPoint).Port + 1, ref dtpActive, ref DATA_iostream);
                }

                if (_MLST)
                {
                    string[] result = MLSD(path, ref iostream, ref DATA_iostream);

                    string object_parser = "";

                    if (mlst_string.IndexOf("size") > -1)
                    {
                        object_parser += @"(size=(?<size>[0-9]*);)|";
                    }

                    if (mlst_string.IndexOf("modify") > -1)
                    {
                        object_parser += @"(modify=(?<modify>[0-9/.]*);)|";
                    }

                    if (mlst_string.IndexOf("create") > -1)
                    {
                        object_parser += @"((created|create)=(?<create>[0-9/.]*);)|";
                    }

                    if (mlst_string.IndexOf("type") > -1)
                    {
                        object_parser += @"(type=(?<type>[a-z_A-Z]*);)|";
                    }

                    if (mlst_string.IndexOf("unique") > -1)
                    {
                        object_parser += @"(unique=(?<unique>.*);)|";
                    }

                    if (mlst_string.IndexOf("perm") > -1)
                    {
                        object_parser += @"(perm=(?<perm>[a-z_A-Z]*);)|";
                    }

                    if (mlst_string.IndexOf("lang") > -1)
                    {
                        object_parser += @"(lang=(?<lang>.*);)|";
                    }

                    if (mlst_string.IndexOf("media-type") > -1)
                    {
                        object_parser += @"(media-type=(?<media_type>.*);)|";
                    }

                    if (mlst_string.IndexOf("charset") > -1)
                    {
                        object_parser += @"(charset=(?<charset>.*);)|";
                    }

                    object_parser += @"\s(?<name>.*)$";

                    Regex parser = new Regex(object_parser, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                    for (int i = 0; i < result.Length; i++)
                    {
                        MatchCollection matchCollection = parser.Matches(result[i]);

                        ContentItemInformation item = new ContentItemInformation();

                        for (int j = 0; j < matchCollection.Count; j++)
                        {
                            if (matchCollection[j].Groups[0].Value.IndexOf("type") > -1)
                            {
                                if (matchCollection[j].Groups["type"].Value.Equals("file", StringComparison.OrdinalIgnoreCase) | matchCollection[j].Groups["type"].Value.Equals("dir", StringComparison.OrdinalIgnoreCase))
                                {
                                    switch (matchCollection[j].Groups["type"].Value)
                                    {
                                        case "dir":
                                            item.ItemType = ContentItemType.Directory;
                                            break;
                                        case "file":
                                            item.ItemType = ContentItemType.File;
                                            break;
                                        default:
                                            item.ItemType = ContentItemType.Unknown;
                                            break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (!string.IsNullOrEmpty(matchCollection[j].Groups["size"].Value))
                            {
                                item.Size = Convert.ToInt64(matchCollection[j].Groups["size"].Value);
                            }

                            if (!string.IsNullOrEmpty(matchCollection[j].Groups["create"].Value))
                            {
                                if (matchCollection[j].Groups["create"].Value.IndexOf(".") > -1)
                                {
                                    item.Created = new DateTime(
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(0, 4)),
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(4, 2)),
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(6, 2)),
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(8, 2)),
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(10, 2)),
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(12, 2)),
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(15)));
                                }
                                else
                                {
                                    item.Created = new DateTime(
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(0, 4)),
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(4, 2)),
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(6, 2)),
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(8, 2)),
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(10, 2)),
                                            int.Parse(matchCollection[j].Groups["create"].Value.Substring(12, 2)));
                                }
                            }

                            if (!string.IsNullOrEmpty(matchCollection[j].Groups["modify"].Value))
                            {

                                if (matchCollection[j].Groups["modify"].Value.IndexOf(".") > -1)
                                {
                                    item.LastChange = new DateTime(
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(0, 4)),
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(4, 2)),
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(6, 2)),
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(8, 2)),
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(10, 2)),
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(12, 2)),
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(15)));
                                }
                                else
                                {
                                    item.LastChange = new DateTime(
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(0, 4)),
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(4, 2)),
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(6, 2)),
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(8, 2)),
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(10, 2)),
                                    int.Parse(matchCollection[j].Groups["modify"].Value.Substring(12, 2)));
                                }
                            }

                            if (!string.IsNullOrEmpty(matchCollection[j].Groups["name"].Value))
                            {
                                item.Name = matchCollection[j].Groups["name"].Value;
                            }

                            if (j == matchCollection.Count - 1)
                            {
                                itemCollection.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    if (system == System.UNIX)
                    {
                        string[] result = LIST(path, ref iostream, ref DATA_iostream);

                        Regex listParser = new Regex(
                            @"^(?<fileType>\w|-)"                                            //  1. file type
                            + @"(?<fileRights>[r-][w-][x-][r-][w-][x-][r-][w-][x-])\s+"      //  2. file rights
                            + @"(?<linksCount>\d+)\s+"                                       //  3. hard links count
                            + @"(?<owner>\S+)\s+"                                            //  4. owner name or id
                            + @"(?<group>\S+)\s+"                                            //  5. group name or id
                            + @"(?<fileSize>\d+)\s+"                                         //  6. file size
                            + @"(?<month>\w+)\s+"                                            //  7. month name
                            + @"(?<day>\d+)\s+"                                              //  8. day of month
                            + @"(?<yearDayTime>\d\d\d\d|\d\d:\d\d)\s+"                       //  9. year or day time
                            + @"(?<fileName>.*)$",                                           // 10. name of the file
                            RegexOptions.IgnoreCase | RegexOptions.Compiled);

                        for (int i = 0; i < result.Length; i++)
                        {
                            Match match = listParser.Match(result[i]);

                            if (match.Success)
                            {
                                if (match.Groups["fileName"].Value != "." && match.Groups["fileName"].Value != "..")
                                {
                                    ContentItemInformation item = new ContentItemInformation();

                                    switch (match.Groups["fileType"].Value)
                                    {
                                        case "-":
                                            item.ItemType = ContentItemType.File;
                                            break;
                                        case "d":
                                            item.ItemType = ContentItemType.Directory;
                                            break;
                                        case "l":
                                            item.ItemType = ContentItemType.SoftLink;
                                            break;
                                        default:
                                            item.ItemType = ContentItemType.Unknown;
                                            break;
                                    }
                                    /*
                                     * Первая цифра в обозначении устанавливает права для группы user
                                     * (т.е. фактически для вас), вторая для группы group и третья для world.
                                     * 
                                     * 
                                     * 4 - чтение, можно писать r (read)  
                                       2 - запись, можно писать w (write)  
                                       1 - исполнение, можно писать x  (exec)
                                     * 
                                     * 4+1 = 5 - чтение, исполнение r-x  
                                       4+2 = 6 - чтение, запись rw-  
                                       4+2+1 - чтение, запись, исполнение  rwx
                                     * 
                                     * 
                                    7 = read, write & execute (чтение, запись, выполнение);
                                    6 = read & write (чтение и запись);
                                    5 = read & execute (чтение и выполнение);
                                    4 = read (чтение);
                                    3 = write & execute (запись и выполнение);
                                    2 = write (запись);
                                    1 = execute (выполнение).
                                    */
                                    int rightsUser = 0, rightsGroup = 0, rightsWorld = 0;
                                    string strRights = match.Groups["fileRights"].Value;

                                    for (int j = 0; j < 3; j++)
                                    {
                                        if (strRights.Substring(0, 3).Substring(j, 1) != "-")
                                        {
                                            switch (j)
                                            {
                                                case 0:
                                                    rightsUser += 4; // w--
                                                    break;
                                                case 1:
                                                    rightsUser += 2; // -r-
                                                    break;
                                                case 2:
                                                    rightsUser += 1; // --x
                                                    break;
                                            }
                                        }
                                    }

                                    for (int j = 0; j < 3; j++)
                                    {
                                        if (strRights.Substring(3, 3).Substring(j, 1) != "-")
                                        {
                                            switch (j)
                                            {
                                                case 0:
                                                    rightsGroup += 4; // w--
                                                    break;
                                                case 1:
                                                    rightsGroup += 2; // -r-
                                                    break;
                                                case 2:
                                                    rightsGroup += 1; // --x
                                                    break;
                                            }
                                        }
                                    }

                                    for (int j = 0; j < 3; j++)
                                    {
                                        if (strRights.Substring(6, 3).Substring(j, 1) != "-")
                                        {
                                            switch (j)
                                            {
                                                case 0:
                                                    rightsWorld += 4; // w--
                                                    break;
                                                case 1:
                                                    rightsWorld += 2; // -r-
                                                    break;
                                                case 2:
                                                    rightsWorld += 1; // --x
                                                    break;
                                            }
                                        }
                                    }

                                    item.Rights = int.Parse(new StringBuilder().AppendFormat("{0}{1}{2}", rightsUser, rightsGroup, rightsWorld).ToString());

                                    item.LinksCount = int.Parse(match.Groups["linksCount"].Value);

                                    item.Owner = match.Groups["owner"].Value;
                                    item.Group = match.Groups["group"].Value;

                                    item.Size = long.Parse(match.Groups["fileSize"].Value);

                                    DateTime date = DateTime.Now;

                                    int day = int.Parse(match.Groups["day"].Value);

                                    int month;
                                    monthName.TryGetValue(match.Groups["month"].Value.ToUpper(), out month);
                                    string timeTail = match.Groups["yearDayTime"].Value;
                                    int timeSep = timeTail.IndexOf(':');
                                    int year;
                                    int hour;
                                    int minute;

                                    if (timeSep >= 0)
                                    {
                                        year = date.Year;
                                        hour = int.Parse(timeTail.Substring(0, timeSep));
                                        minute = int.Parse(timeTail.Substring(timeSep + 1));
                                    }
                                    else
                                    {
                                        year = int.Parse(timeTail);
                                        hour = 0;
                                        minute = 0;
                                    }

                                    item.LastChange = new DateTime(year, month, day, hour, minute, 0);

                                    item.Name = match.Groups["fileName"].Value;

                                    itemCollection.Add(item);
                                }
                            }
                        }
                    }
                    else if (system == System.Windows)
                    {
                        string[] result = LIST(path, ref iostream, ref DATA_iostream);

                        for (int i = 0; i < result.Length; i++)
                        {
                            Console.WriteLine(result[i]);
                        }
                    }
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
            }
        }

        /// <summary>
        /// Set permition of the file or folder
        /// </summary>
        /// <param name="name">name of the file or folder, or full path to them</param>
        /// <param name="grouPermition">Permition for group</param>
        /// <param name="userPermition">Permition for user</param>
        /// <param name="worldPermition">Permition for others</param>
        /// <exception cref="IOException"></exception>
        public void SetPermition(string name, UserPermitionOptions userPermition, GroupPermitionOptions grouPermition, WorldPermitionOptions worldPermition)
        {
            try
            {
                SITE(new StringBuilder().AppendFormat("CHMOD {0}{1}{2} {3}", (int)userPermition, (int)grouPermition, (int)worldPermition, name).ToString(), ref iostream);
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
            }
        }

        /// <summary>
        /// Return file size in bytes, or -1 if server not supported comand SIZE
        /// </summary>
        /// <param name="filename">File name</param>
        /// <exception cref="IOException"></exception>
        /// <returns>File size</returns>
        public long GetFileSize(string filename)
        {
            try
            {
                if (_SIZE)
                {
                    SetType(_TYPE.IMAGE, ref iostream);

                    return SIZE(filename, ref iostream);
                }
                else
                {
                    return -1;
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);

                return -1;
            }
        }

        /// <summary>
        /// Remove file on FTP server
        /// </summary>
        /// <param name="filename">File name for removing</param>
        /// <returns>Return true if the file is removed otherwise false</returns>
        /// <exception cref="IOException"></exception>
        public bool RemoveFile(string filename)
        {
            try
            {
                if (DELE(filename, ref iostream).Equals("250"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Download from FTP server on local PC
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="stream">File stream</param>
        /// <exception cref="IOException"></exception>
        public void Download(string filename, ref FileStream stream)
        {
            try
            {
                SetType(_TYPE.IMAGE, ref iostream);

                if (mode == ConnectionMode.Passive)
                {
                    PassiveConnectionInfo pInfo = new PassiveConnectionInfo();

                    if (_EPSV)
                    {
                        try
                        {
                            pInfo = EPSV(passiveNetPrt, ref iostream);
                        }
                        catch (ServerResponseException ex)
                        {
                            OnServerResponse(ex.Message);
                            _EPSV = false;
                        }
                    }
                    if (!_EPSV)
                    {
                        pInfo = PASV(ref iostream);
                    }

                    ConnectTo(pInfo.ip, pInfo.port, ref dtpPassive, ref DATA_iostream);

                    Stream tmp_stream = (Stream)stream;

                    RETR(filename, ref iostream, ref DATA_iostream, ref tmp_stream);
                }
                else if (mode == ConnectionMode.Active)
                {

                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
            }
        }

        /// <summary>
        /// Download from the remote FTP server on local FTP server, only in Passive mode
        /// </summary>
        /// <param name="filename">File name on remote server</param>
        /// <param name="rcstream">Remote FTP, control connection. Use fo that <code> ftp.Control</code> property</param>
        /// <exception cref="IOException"></exception>
        public void Download_RS_to_LS(string filename, NetworkStream rcstream)
        {
            try
            {
                TYPE(_TYPE.IMAGE, ref rcstream);
                PassiveConnectionInfo pInfo = PASV(ref rcstream);

                TcpClient tmp_dtp = new TcpClient();
                NetworkStream rdstream = null;
                ConnectTo(pInfo.ip, pInfo.port, ref tmp_dtp, ref rdstream);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("RETR {0}\r\n", filename);
                byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                rcstream.Write(buffer, 0, buffer.Length);
                rcstream.Flush();

                byte[] response = ReadServerResponseMultiline(ref rcstream);

                if (ServerResponseCode(response).Trim().Equals("120"))
                {
                    OnServerResponse(Encoding.ASCII.GetString(response));
                }
                else if (ServerResponseCode(response).Trim().Equals("150"))
                {
                    OnServerResponse(Encoding.ASCII.GetString(response));
                }
                else
                {
                    if (ServerResponseCode(response).Trim().Equals("500"))
                    {
                        throw new _500_сommand_syntax_error_could_not_interpreted_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("501"))
                    {
                        throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("421"))
                    {
                        throw new _421_service_not_available_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("530"))
                    {
                        throw new _530_not_logged_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("450"))
                    {
                        throw new _450_file_unavailable_busy_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("550"))
                    {
                        throw new _550_file_unavailable_not_found_no_access_exception(Encoding.ASCII.GetString(response));
                    }
                }

                SetType(_TYPE.IMAGE, ref iostream);

                pInfo = PASV(ref iostream);

                ConnectTo(pInfo.ip, pInfo.port, ref dtpPassive, ref DATA_iostream);

                Stream tmp_stream = (Stream)rdstream;

                STOR(filename, ref iostream, ref DATA_iostream, ref tmp_stream);

                DicsonnectFrom(ref tmp_dtp, ref rdstream);

                response = ReadServerResponseMultiline(ref rcstream);

                if (ServerResponseCode(response).Trim().Equals("226"))
                {
                    OnServerResponse(Encoding.ASCII.GetString(response));
                }
                else if (ServerResponseCode(response).Trim().Equals("250"))
                {
                    OnServerResponse(Encoding.ASCII.GetString(response));
                }
                else
                {
                    if (ServerResponseCode(response).Trim().Equals("425"))
                    {
                        throw new _425_can_not_open_data_connection_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("426"))
                    {
                        throw new _426_connection_vlosed_transfer_aborted_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("451"))
                    {
                        throw new _451_local_error_exception(Encoding.ASCII.GetString(response));
                    }
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
            }
        }

        /// <summary>
        /// Upload from local PC to FTP server
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="stream">File stream</param>
        /// <exception cref="IOException"></exception>
        public void Upload(string filename, ref FileStream stream)
        {
            try
            {
                SetType(_TYPE.IMAGE, ref iostream);

                if (mode == ConnectionMode.Passive)
                {
                    PassiveConnectionInfo pInfo = new PassiveConnectionInfo();

                    if (_EPSV)
                    {
                        try
                        {
                            pInfo = EPSV(passiveNetPrt, ref iostream);
                        }
                        catch (ServerResponseException ex)
                        {
                            OnServerResponse(ex.Message);
                            _EPSV = false;
                        }
                    }
                    if (!_EPSV)
                    {
                        pInfo = PASV(ref iostream);
                    }

                    ConnectTo(pInfo.ip, pInfo.port, ref dtpPassive, ref DATA_iostream);

                    Stream tmp_stream = (Stream)stream;

                    try
                    {
                        STOR(filename, ref iostream, ref DATA_iostream, ref tmp_stream);
                    }
                    catch (ServerResponseException ex)
                    {
                        OnServerResponse(ex.Message);
                    }
                }
                else if (mode == ConnectionMode.Active)
                {

                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
            }
        }

        /// <summary>
        /// Upload from local FTP to remote FTP server, only in Passive mode
        /// </summary>
        /// <param name="filename">File name on local server</param>
        /// <param name="rcstream">Remote FTP, control connection. Use fo that <code> ftp.Control</code> property</param>
        /// <exception cref="IOException"></exception>
        public void Upload_LS_to_RS(string filename, NetworkStream rcstream)
        {
            try
            {
                SetType(_TYPE.IMAGE, ref iostream);

                PassiveConnectionInfo pInfo = PASV(ref iostream);

                ConnectTo(pInfo.ip, pInfo.port, ref dtpPassive, ref DATA_iostream);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("RETR {0}\r\n", filename);
                byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                iostream.Write(buffer, 0, buffer.Length);
                iostream.Flush();

                byte[] response = ReadServerResponseMultiline(ref iostream);

                if (ServerResponseCode(response).Trim().Equals("120"))
                {
                    OnServerResponse(Encoding.ASCII.GetString(response));
                }
                else if (ServerResponseCode(response).Trim().Equals("150"))
                {
                    OnServerResponse(Encoding.ASCII.GetString(response));
                }
                else
                {
                    if (ServerResponseCode(response).Trim().Equals("500"))
                    {
                        throw new _500_сommand_syntax_error_could_not_interpreted_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("501"))
                    {
                        throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("421"))
                    {
                        throw new _421_service_not_available_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("530"))
                    {
                        throw new _530_not_logged_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("450"))
                    {
                        throw new _450_file_unavailable_busy_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("550"))
                    {
                        throw new _550_file_unavailable_not_found_no_access_exception(Encoding.ASCII.GetString(response));
                    }
                }

                TYPE(_TYPE.IMAGE, ref rcstream);

                pInfo = PASV(ref rcstream);

                TcpClient tmp_dtp = new TcpClient();
                NetworkStream rdstream = null;
                ConnectTo(pInfo.ip, pInfo.port, ref tmp_dtp, ref rdstream);

                Stream tmp_stream = (Stream)DATA_iostream;

                STOR(filename, ref rcstream, ref rdstream, ref tmp_stream);

                DicsonnectFrom(ref tmp_dtp, ref rdstream);

                response = ReadServerResponseMultiline(ref rcstream);

                if (ServerResponseCode(response).Trim().Equals("226"))
                {
                    OnServerResponse(Encoding.ASCII.GetString(response));
                }
                else if (ServerResponseCode(response).Trim().Equals("250"))
                {
                    OnServerResponse(Encoding.ASCII.GetString(response));
                }
                else
                {
                    if (ServerResponseCode(response).Trim().Equals("425"))
                    {
                        throw new _425_can_not_open_data_connection_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("426"))
                    {
                        throw new _426_connection_vlosed_transfer_aborted_exception(Encoding.ASCII.GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("451"))
                    {
                        throw new _451_local_error_exception(Encoding.ASCII.GetString(response));
                    }
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
            }
        }

        /// <summary>
        /// Rename file or directory on FTP server
        /// </summary>
        /// <param name="old_fd_name">Old file/directory name</param>
        /// <param name="new_fd_name">New file/directory name</param>
        /// <returns>Return true if the file or directory is renamed otherwise false</returns>
        /// <exception cref="IOException"></exception>
        public bool Rename(string old_fd_name, string new_fd_name)
        {
            try
            {
                if (RNFR(old_fd_name, ref iostream).Equals("350"))
                {
                    if (RNTO(new_fd_name, ref iostream).Equals("250"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);

                return false;
            }
        }


        #region Helper function

        private void ConnectTo(string hostname, int port, ref TcpClient workTcpCl, ref NetworkStream iostream)
        {
            try
            {
                workTcpCl = new TcpClient();

                workTcpCl.Connect(hostname, port);

                iostream = workTcpCl.GetStream();
            }
            catch (SocketException ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Cannot connect to {0} on port: {1}\nSystem message: {2}", hostname, port, ex.Message);

                throw new Exception(sb.ToString());
            }
        }

        private void AcceptFrom(IPAddress ip, int port, ref TcpListener workTcpLis, ref NetworkStream iosream)
        {
            workTcpLis = new TcpListener(ip, port);
            workTcpLis.Start();

            Console.WriteLine("Waiting for a connection on {0}:{1} ... ", ip, port);
            TcpClient tmpTcpCl = workTcpLis.AcceptTcpClient();
            Console.WriteLine("Connected!");

            iosream = tmpTcpCl.GetStream();
            /*iosream = new NetworkStream(workTcpLis.AcceptSocket(), FileAccess.ReadWrite);
            workTcpLis.Stop();*/
/*            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(ip, port);
            listener.Bind(ep);
            listener.Listen(10);
            iosream = new NetworkStream(listener.Accept(), FileAccess.ReadWrite);*/
        }

        private void DicsonnectFrom(ref TcpClient workTcpCl, ref NetworkStream iostream)
        {
            iostream.Close();
            workTcpCl.Close();
        }

        private byte[] ReadServerResponseMultiline(ref NetworkStream iostream)
        {
            try
            {
                StreamReader reader = new StreamReader(iostream);
                String rc = "";
                String response = "";
                bool done = false;
                bool first = true;
                while (!done)
                {
                    String tmp = reader.ReadLine();
                    response += tmp + '\n';
                    if (tmp.Length >= 4)
                    {
                        speed += tmp.Length;

                        if (first == true)
                        {
                            rc = response.Substring(0, 3);
                            first = false;
                        }

                        if ((rc.Equals(tmp.Substring(0, 3))) && (tmp[3] == ' '))
                        {
                            done = true;
                        }

                        noopTimer.Change(60000, 30000);
                    }
                }


                return Encoding.ASCII.GetBytes(response);
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                return new byte[1];
            }
        }

        private byte[] ReadServerDATA(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanRead)
                {
                    byte[] rBuffer = new byte[1024];
                    List<byte> all_rBuffer = new List<byte>();

                    int nob = 0;

                    while (true)
                    {
                        nob = iostream.Read(rBuffer, 0, rBuffer.Length);
                        if (nob > 0)
                        {
                            speed += nob;

                            for (int i = 0; i < nob; i++)
                            {
                                all_rBuffer.Add(rBuffer[i]);
                            }

                            noopTimer.Change(60000, 30000);
                        }
                        else
                        {
                            break;
                        }
                    }

                    return all_rBuffer.ToArray();
                }
                else
                {
                    throw new Exception("Cannot read from NetworkStream.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                return new byte[1];
            }
        }

        /// <summary>
        /// Read from stream_r write to stream_w
        /// </summary>
        /// <param name="stream_r"></param>
        /// <param name="stream_w"></param>
        private void ReadWriteServerDATA(ref Stream stream_r, ref Stream stream_w)
        {
            try
            {
                totalDownloadedBytes = 0;

                if (stream_r.CanRead)
                {
                    byte[] rBuffer = new byte[1024];

                    int nob = 0;

                    while (true)
                    {
                        nob = stream_r.Read(rBuffer, 0, rBuffer.Length);
                        if (nob > 0)
                        {
                            speed += nob;
                            totalDownloadedBytes += nob;

                            OnFileTransfer(totalDownloadedBytes);

                            stream_w.Write(rBuffer, 0, nob);
                            stream_w.Flush();

                            noopTimer.Change(60000, 30000);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    throw new Exception("Cannot read from NetworkStream.");
                }

                totalDownloadedBytes = 0;
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);
            }
        }

        /// <summary>
        /// Read from stream_r write to stream_w
        /// </summary>
        /// <param name="stream_r"></param>
        /// <param name="stream_w"></param>
        private void ReadWriteServerDATA(ref NetworkStream stream_r, ref Stream stream_w)
        {
            Stream tmp = (NetworkStream)stream_r;
            ReadWriteServerDATA(ref tmp, ref stream_w);
        }

        /// <summary>
        /// Read from stream_r write to stream_w
        /// </summary>
        /// <param name="stream_r"></param>
        /// <param name="stream_w"></param>
        private void ReadWriteServerDATA(ref Stream stream_r, ref NetworkStream stream_w)
        {
            Stream tmp = (NetworkStream)stream_w;
            ReadWriteServerDATA(ref stream_r, ref tmp);
        }

        private string ServerResponseCode(byte[] data)
        {
            return GetString(data).Substring(0, 3);
        }

        private string GetString(byte[] data)
        {
            if (encoding == Encoding.UTF8)
            {
                return Encoding.UTF8.GetString(data);
            }
            else
            {
                return Encoding.ASCII.GetString(data);
            }
            
        }

        private void SetType(_TYPE type, ref NetworkStream iostream)
        {
            try
            {
                if (currentType != type)
                {
                    if (TYPE(type, ref iostream).Equals("200"))
                    {
                        currentType = type;
                    }
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
            }
        }

        private void DoNOOP(object state)
        {
            Timer timer = (Timer)state;

            try
            {
                NOOP(ref iostream);
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
            }
            catch (IOException ex)
            {
                timer.Dispose();
            }
        }

        private void DoTimer(object state)
        {
            OnSpeed(this);

            speed = 0;
        }

        private void DoEPRT(NetworkProtocol prt, ref List<int> notSupportedProtocols)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("|{2}|{0}|{1}|", Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString(), ((IPEndPoint)ftp.Client.LocalEndPoint).Port + 1, (int)prt);
                EPRT(sb.ToString(), ref iostream);
            }
            catch (_522_protocol_not_supported ex)
            {
                notSupportedProtocols.Add((int)prt);

                OnServerResponse(ex.Message);
                Regex reg = new Regex(@"\((?<prt>.*)\)");
                Match match = reg.Match(ex.Message);

                if (match.Success)
                {
                    string[] protocols = match.Groups["prt"].Value.Replace("(", "").Replace(")", "").Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < protocols.Length; i++)
                    {
                        int protocol = Convert.ToInt32(protocols[i]);
                        if (!notSupportedProtocols.Contains(protocol))
                        {
                            switch (protocol)
                            {
                                case 1:
                                    DoEPRT(NetworkProtocol.IPv4, ref notSupportedProtocols);
                                    i = protocols.Length;
                                    break;
                                case 2:
                                    DoEPRT(NetworkProtocol.IPv6, ref notSupportedProtocols);
                                    i = protocols.Length;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (ServerResponseException ex)
            {
                OnServerResponse(ex.Message);
                _EPRT = false;
            }
        }

        private double SpeedConvert(SpeedPerSecond type, int size)
        {
            return Math.Round(size / Math.Pow(2.0, (double)type), 1);
        }


        private void CheckFEAT(string feat)
        {
            if (feat.IndexOf("UTF8", StringComparison.OrdinalIgnoreCase) > -1)
            {
                _UTF8 = true;
            }

            if (feat.IndexOf("MDTM", StringComparison.OrdinalIgnoreCase) > -1)
            {
                _MDTM = true;
            }

            if (feat.IndexOf("CLNT", StringComparison.OrdinalIgnoreCase) > -1)
            {
                _CLNT = true;
            }

            if (feat.IndexOf("SIZE", StringComparison.OrdinalIgnoreCase) > -1)
            {
                _SIZE = true;
            }
            
            if (feat.IndexOf("REST STREAM", StringComparison.OrdinalIgnoreCase) > -1)
            {
                _REST = true;
            }

            if (feat.IndexOf("TVFS", StringComparison.OrdinalIgnoreCase) > -1)
            {
                _TVFS = true;
            }

            Regex parser = new Regex("(?<mlst>MLST .*)");
            Match match = parser.Match(feat);
            if (match.Success)
            {
                _MLST = true;
                mlst_string = match.Groups["mlst"].Value.Replace("*", "");
            }

            if (feat.IndexOf("EPRT", StringComparison.OrdinalIgnoreCase) > -1)
            {
                _EPRT = true;
            }

            if (feat.IndexOf("EPSV", StringComparison.OrdinalIgnoreCase) > -1)
            {
                _EPSV = true;
            }

        }

        #endregion

        #region FEAT variables

        private bool _UTF8 = false;
        private bool _MDTM = false;
        private bool _CLNT = false;
        private bool _SIZE = false;
        private bool _REST = false;
        private bool _TVFS = false; // I do not know how to use it
        private bool _MLST = false;
        private string mlst_string;
        private bool _EPRT = false;
        private bool _EPSV = false;
        private bool _TLS = false;

        #endregion

        #region Property

        /// <summary>
        /// Get or Set ConnectionMode
        /// </summary>
        public ConnectionMode ConnectionMode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }

        /// <summary>
        /// Get or Set ConnectionType
        /// </summary>
        public ConnectionType ConnectionType
        {
            get
            {
                return conType;
            }
            set
            {
                conType = value;
            }
        }

        /// <summary>
        /// Set user account information
        /// </summary>
        public string AccountInformation
        {
            set
            {
                account_information = value;
            }
        }

        /// <summary>
        /// Port, usually is 21
        /// </summary>
        public int Port
        {
            set
            {
                port = value;
            }
        }

        /// <summary>
        /// The host name of FTP server or his IP
        /// </summary>
        public string Host
        {
            set
            {
                host = value;
            }
        }

        /// <summary>
        /// user name
        /// </summary>
        public string Username
        {
            set
            {
                username = value;
            }
        }

        /// <summary>
        /// password
        /// </summary>
        public string Password
        {
            set
            {
                password = value;
            }
        }

        /// <summary>
        /// Get the control connection of the server, use this for downloading or uploading from server to server
        /// </summary>
        public NetworkStream Control
        {
            get
            {
                return iostream;
            }
        }

        /// <summary>
        /// Return the string representation of current speed
        /// </summary>
        public string SpeedFloating
        {
            get
            {
                string[] unit = new string[] { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };
                
                int i = 0;
                
                if (speed == 0)
                {
                    return "";
                }
                else
                {
                    return new StringBuilder().AppendFormat("{0} {1}/sec", Math.Round(speed / Math.Pow(1024, (i = (int)Math.Floor(Math.Log(speed, 1024)))), 2), unit[i]).ToString();
                }
            }
        }

        #endregion
    }
}
