using System;
using System.Collections.Generic;
using System.Text;

namespace FTPLibrary
{
    /// <summary>
    /// This struct save ip and port after PASV command
    /// </summary>
    public struct PassiveConnectionInfo
    {
        /// <summary>
        /// IP for DATA connection
        /// </summary>
        public string ip;

        /// <summary>
        /// Posrt for DATA connection
        /// </summary>
        public int port;
    }
}
