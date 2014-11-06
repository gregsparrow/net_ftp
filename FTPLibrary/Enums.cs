using System;
using System.Collections.Generic;
using System.Text;

namespace FTPLibrary
{
    /// <summary>
    /// Active or Passive connection
    /// </summary>
    public enum ConnectionMode
    {
        /// <summary>
        /// Active connection
        /// </summary>
        Active,

        /// <summary>
        /// Passive connection
        /// </summary>
        Passive
    };

    /// <summary>
    /// Secure connection or not
    /// </summary>
    public enum ConnectionType
    {
        /// <summary>
        /// Use TSL connection
        /// </summary>
        SecureConnection,

        /// <summary>
        /// Default connection
        /// </summary>
        DefaultConnection
    };

    /// <summary>
    /// Is used to determine the type of item
    /// </summary>
    public enum ContentItemType
    {
        /// <summary>
        /// File
        /// </summary>
        File,

        /// <summary>
        /// Directory
        /// </summary>
        Directory,

        /// <summary>
        /// Link
        /// </summary>
        SoftLink,

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    };

    /// <summary>
    /// Speed
    /// </summary>
    public enum SpeedPerSecond
    {
        /// <summary>
        /// byte
        /// </summary>
        B = 1,

        /// <summary>
        /// kibibyte = kilobit = 2^10  (kilobyte = 10^3)
        /// </summary>
        KiB = 10,

        /// <summary>
        /// mebibyte = megabit = 2^20 (megabyte = 10^6)
        /// </summary>
        MiB = 20,

        /// <summary>
        /// gibibyte = gigabit = 2^30 (gigabyte = 10^9)
        /// </summary>
        GiB = 30,

        /// <summary>
        /// tebibyte = terabit = 2^40 (terabyte = 10^12)
        /// </summary>
        TiB = 40,

        /// <summary>
        /// pebibyte = pebibit = 2^50 (petabyte = 10^15)
        /// </summary>
        PiB = 50,

        /// <summary>
        /// exbibyte = exbibit = 2^60 (exabyte = 10^18)
        /// </summary>
        EiB = 60,

        /// <summary>
        /// zebibyte = zebibit = 2^70 (zettabyte = 10^21)
        /// </summary>
        ZiB = 70,

        /// <summary>
        /// yobibyte = yobibit = 2^80 (yottabyte = 10^24)
        /// </summary>
        YiB = 80
    };


    /// <summary>
    /// Perpmition for user
    /// </summary>
    public enum UserPermitionOptions
    {
        /// <summary>
        /// Permition for reading
        /// </summary>
        Read = 4,

        /// <summary>
        /// Permition for writing
        /// </summary>
        Write = 2,

        /// <summary>
        /// Permition for executing
        /// </summary>
        Exec = 1,

        /// <summary>
        /// No permition
        /// </summary>
        Zero = 0
    };

    /// <summary>
    /// Permition for group
    /// </summary>
    public enum GroupPermitionOptions
    {
        /// <summary>
        /// Permition for reading
        /// </summary>
        Read = 4,

        /// <summary>
        /// Permition for writing
        /// </summary>
        Write = 2,

        /// <summary>
        /// Permition for executing
        /// </summary>
        Exec = 1,

        /// <summary>
        /// No permition
        /// </summary>
        Zero = 0
    };

    /// <summary>
    /// Permition for others
    /// </summary>
    public enum WorldPermitionOptions
    {
        /// <summary>
        /// Permition for reading
        /// </summary>
        Read = 4,

        /// <summary>
        /// Permition for writing
        /// </summary>
        Write = 2,

        /// <summary>
        /// Permition for executing
        /// </summary>
        Exec = 1,

        /// <summary>
        /// No permition
        /// </summary>
        Zero = 0
    };

    /// <summary>
    /// Class for ftp connection
    /// </summary>
    public partial class FTP
    {
        private enum ProtectionLevel
        {
            Clear,
            Safe,
            Confidential,
            Private
        };

        private enum NetworkProtocol
        {
            IPv4 = 1,
            IPv6 = 2,
            ALL = 0
        };

        private enum System
        {
            UNIX,
            Windows
        };

        private enum _TYPE
        {
            ASCII,
            EBCDIC,
            IMAGE,
            none
            //        LOCAL
        };

        private enum _MODE
        {
            STREAM,
            BLOCK,
            COMPRESSED,
            ZLIB
        };

        private enum STRUCTURE
        {
            FILE,
            RECORD,
            PAGE,
            TIFF
        };
    }
}
