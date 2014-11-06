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
    public partial class FTP
    {
        #region Command send functions

        /// <summary>
        /// Send USER command to server
        /// USER
        ///     230
        ///     530
        ///     500, 501, 421
        ///     331, 332
        /// </summary>
        /// <param name="username">User name, login</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return server response code.</returns>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_332_need_account_for_login_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string USER(string username, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("USER {0}\r\n", username);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("230"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("331"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("336"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("332"))
                        {
                            throw new _332_need_account_for_login_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }

        }

        /// <summary>
        /// Send PASS command to server
        /// PASS
        ///     230
        ///     202
        ///     530
        ///     500, 501, 503, 421
        ///     332
        /// </summary>
        /// <param name="password">user password</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return server response code.</returns>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_503_bad_sequence_of_commands_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_332_need_account_for_login_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string PASS(string password, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("PASS {0}\r\n", password);
                    OnClientCommand("PASS (hidden)\r\n");
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("230"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("503"))
                        {
                            throw new _503_bad_sequence_of_commands_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("332"))
                        {
                            throw new _332_need_account_for_login_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send NOOP command to server
        /// NOOP
        ///     200
        ///     500 421
        /// </summary>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string NOOP(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {

                    OnClientCommand("NOOP\r\n");
                    byte[] buffer = Encoding.ASCII.GetBytes("NOOP\r\n");

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }


        /// <summary>
        /// SITE
        ///       200
        ///       202
        ///       500, 501, 530
        /// </summary>
        /// <param name="command">Command like CHMOD</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return server response code.</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_202_command_not_implemented_superfluous_at_this_site"></exception>
        /// <exception cref="IOException"></exception>
        private string SITE(string command, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("SITE {0}\r\n", command);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("202"))
                        {
                            throw new _202_command_not_implemented_superfluous_at_this_site(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send QUIT command to server
        /// QUIT
        ///     221
        ///     500
        /// </summary>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <param name="workTcpCl">TcpClient instance for sending control messages</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string QUIT(ref NetworkStream iostream, ref TcpClient workTcpCl)
        {
            try
            {
                if (iostream.CanWrite)
                {

                    OnClientCommand("QUIT\r\n");
                    byte[] buffer = Encoding.ASCII.GetBytes("QUIT\r\n");

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("221"))
                    {
                        OnServerResponse(GetString(response));

                        iostream.Close();
                        workTcpCl.Close();
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send PWD command to server
        /// PWD
        ///     257
        ///     500, 501, 502, 421, 550
        /// </summary>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return current work directory</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string PWD(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {

                    OnClientCommand("PWD\r\n");
                    byte[] buffer = Encoding.ASCII.GetBytes("PWD\r\n");

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("257"))
                    {
                        OnServerResponse(GetString(response));

                        Regex reg = new Regex("[ a-zA-Z_0-9]*\"(?<dir>[ a-zA-Z_0-9/]*)\"[ a-zA-Z_0-9.]*");
                        Match match = reg.Match(GetString(response));

                        return match.Groups["dir"].Value;
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                    }
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }

            return "";
        }

        /// <summary>
        /// Send CWD command to server
        /// CWD
        ///     250
        ///     500, 501, 502, 421, 530, 550
        /// </summary>
        /// <param name="directory">path to directory</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string CWD(string directory, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("CWD {0}\r\n", directory);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("250"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send MKD command to server
        /// This command causes the directory specified in the pathname
        /// to be created as a directory (if the pathname is absolute)
        /// or as a subdirectory of the current working directory (if
        /// the pathname is relative).
        /// MKD
        ///     257
        ///     500, 501, 502, 421, 530, 550
        /// </summary>
        /// <param name="directory">name of new directory</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string MKD(string directory, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("MKD {0}\r\n", directory);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("257"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send RMD command to server
        /// This command causes the directory specified in the pathname
        /// to be removed as a directory (if the pathname is absolute)
        /// or as a subdirectory of the current working directory (if
        /// the pathname is relative).  
        /// RMD
        ///     250
        ///     500, 501, 502, 421, 530, 550
        /// </summary>
        /// <param name="directory">directory name</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string RMD(string directory, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("RMD {0}\r\n", directory);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("250"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send CDUP command to server, change to parent of current working directory
        /// CDUP
        ///     200
        ///     500, 501, 502, 421, 530, 550
        /// </summary>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string CDUP(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {

                    OnClientCommand("CDUP\r\n");
                    byte[] buffer = Encoding.ASCII.GetBytes("CDUP\r\n");

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("250"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send DELE command to server
        /// DELE
        ///     250
        ///     450, 550
        ///     500, 501, 502, 421, 530
        /// </summary>
        /// <param name="pathname">Path to the file that will be deleted</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="_450_file_unavailable_busy_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string DELE(string pathname, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("DELE {0}\r\n", pathname);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("250"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("450"))
                        {
                            throw new _450_file_unavailable_busy_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send TYPE command to server
        /// TYPE
        ///     200
        ///     500, 501, 504, 421, 530
        /// </summary>
        /// <param name="type">ASCII, EBCDIC or IMAGE</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_504_command_not_implemented_for_that_parameter_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string TYPE(_TYPE type, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    string str = "";

                    switch (type)
                    {
                        case _TYPE.ASCII:
                            str = "TYPE A\r\n";
                            break;
                        case _TYPE.EBCDIC:
                            str = "TYPE E\r\n";
                            break;
                        case _TYPE.IMAGE:
                            str = "TYPE I\r\n";
                            break;
                        /*case TYPE.LOCAL:
                            str = "TYPE L\r\n";
                            break;*/
                    }

                    OnClientCommand(str);
                    byte[] buffer = Encoding.ASCII.GetBytes(str);

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("504"))
                        {
                            throw new _504_command_not_implemented_for_that_parameter_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send MODE command to server
        /// MODE
        ///     200
        ///     500, 501, 504, 421, 530
        /// </summary>
        /// <param name="mode">STREAM, BLOCK, COMPRESSED or ZLIB</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_504_command_not_implemented_for_that_parameter_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string MODE(_MODE mode, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    string str = "";

                    switch (mode)
                    {
                        case _MODE.STREAM:
                            str = "MODE S\r\n";
                            break;
                        case _MODE.BLOCK:
                            str = "MODE B\r\n";
                            break;
                        case _MODE.COMPRESSED:
                            str = "MODE C\r\n";
                            break;
                        case _MODE.ZLIB:
                            str = "MODE Z\r\n";
                            break;
                    }

                    OnClientCommand(str);
                    byte[] buffer = Encoding.ASCII.GetBytes(str);

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("504"))
                        {
                            throw new _504_command_not_implemented_for_that_parameter_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send ABOR command to server
        /// ABOR
        ///     225, 226
        ///     500, 501, 502, 421
        /// </summary>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string ABOR(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {

                    OnClientCommand("ABOR\r\n");
                    byte[] buffer = Encoding.ASCII.GetBytes("ABOR\r\n");

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("225"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("226"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send PORT command to server
        /// PORT
        ///     200
        ///     500, 501, 421, 530
        /// </summary>
        /// <param name="ip">ip</param>
        /// <param name="port">number of port</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string PORT(IPAddress ip, int port, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    String address = ip.ToString();
                    address = address.Replace('.', ',');

                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("PORT {0},{1},{2}\r\n", address, ((port & 0xff00) >> 8), (port & 0x00ff));
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send SYST command to server
        /// SYST
        ///     215
        ///     500, 501, 502, 421
        /// </summary>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return system type</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string SYST(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {

                    OnClientCommand("SYST\r\n");
                    byte[] buffer = Encoding.ASCII.GetBytes("SYST\r\n");

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("215"))
                    {
                        OnServerResponse(GetString(response));
                        Regex reg = new Regex("[ 0-9]*(?<type>[a-zA-Z_0-9]*)[ a-zA-Z_0-9]*");
                        Match match = reg.Match(GetString(response));

                        return match.Groups["type"].Value;
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                    }
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }

            return "";
        }

        /// <summary>
        /// Send STRU command to server
        /// STRU
        ///     200
        ///     500, 501, 504, 421, 530
        /// </summary>
        /// <param name="structure">FILE, RECORD, PAGE or TIFF</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_504_command_not_implemented_for_that_parameter_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string STRU(STRUCTURE structure, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    string str = "";

                    switch (structure)
                    {
                        case STRUCTURE.FILE:
                            str = "STRU F\r\n";
                            break;
                        case STRUCTURE.RECORD:
                            str = "STRU R\r\n";
                            break;
                        case STRUCTURE.PAGE:
                            str = "STRU P\r\n";
                            break;
                        case STRUCTURE.TIFF:
                            str = "STRU T\r\n";
                            break;
                    }

                    OnClientCommand(str);
                    byte[] buffer = Encoding.ASCII.GetBytes(str);

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("504"))
                        {
                            throw new _504_command_not_implemented_for_that_parameter_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send LIST command to server
        /// LIST
        ///     125, 150
        ///         226, 250
        ///         425, 426, 451
        ///     450
        ///     500, 501, 502, 421, 530
        /// </summary>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <param name="DATA_iostream">NetworkStream which will be get the list of files and directorys</param>
        /// <returns>Return list of directorys and files</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_450_file_unavailable_busy_exception"></exception>
        /// <exception cref="_425_can_not_open_data_connection_exception"></exception>
        /// <exception cref="_426_connection_vlosed_transfer_aborted_exception"></exception>
        /// <exception cref="_451_local_error_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="_534_request_denied_for_policy_reasons"></exception>
        /// <exception cref="_535_failed_security_check"></exception>
        /// <exception cref="IOException"></exception>
        private string[] LIST(ref NetworkStream iostream, ref NetworkStream DATA_iostream)
        {
            return LIST(null, ref iostream, ref DATA_iostream);
        }

        /// <summary>
        /// Send LIST command to server
        /// LIST
        ///     125, 150
        ///         226, 250
        ///         425, 426, 451
        ///     450
        ///     500, 501, 502, 421, 530
        /// </summary>
        /// <param name="pathname">path</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <param name="DATA_iostream">NetworkStream which will be get the list of files and directory</param>
        /// <returns>Return list of directory and files</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_450_file_unavailable_busy_exception"></exception>
        /// <exception cref="_425_can_not_open_data_connection_exception"></exception>
        /// <exception cref="_426_connection_vlosed_transfer_aborted_exception"></exception>
        /// <exception cref="_451_local_error_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="_534_request_denied_for_policy_reasons"></exception>
        /// <exception cref="_535_failed_security_check"></exception>
        /// <exception cref="IOException"></exception>
        private string[] LIST(string pathname, ref NetworkStream iostream, ref NetworkStream DATA_iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();

                    if (pathname == null)
                    {
                        sb.Append("LIST -aL\r\n");
                    }
                    else
                    {
                        sb.AppendFormat("LIST -aL {0}\r\n", pathname);
                    }

                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("125"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("150"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (mode == ConnectionMode.Passive)
                        {
                            DicsonnectFrom(ref dtpPassive, ref DATA_iostream);
                        }

                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("450"))
                        {
                            throw new _450_file_unavailable_busy_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("504"))
                        {
                            throw new _504_command_not_implemented_for_that_parameter_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("553"))
                        {
                            throw new _553_file_name_not_allowed_exception(GetString(response));
                        }
                        if (ServerResponseCode(response).Trim().Equals("425"))
                        {
                            throw new _425_can_not_open_data_connection_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("426"))
                        {
                            throw new _426_connection_vlosed_transfer_aborted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("451"))
                        {
                            throw new _451_local_error_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("534"))
                        {
                            throw new _534_request_denied_for_policy_reasons(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("535"))
                        {
                            throw new _535_failed_security_check(GetString(response));
                        }
                    }


                    byte[] list = ReadServerDATA(ref DATA_iostream);


                    if (mode == ConnectionMode.Passive)
                    {
                        DicsonnectFrom(ref dtpPassive, ref DATA_iostream);
                    }

                    response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("226"))
                    {
                        OnServerResponse(GetString(response));

                        return GetString(list).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else if (ServerResponseCode(response).Trim().Equals("250"))
                    {
                        OnServerResponse(GetString(response));

                        return GetString(list).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("425"))
                        {
                            throw new _425_can_not_open_data_connection_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("426"))
                        {
                            throw new _426_connection_vlosed_transfer_aborted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("451"))
                        {
                            throw new _451_local_error_exception(GetString(response));
                        }
                    }
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }

            return new string[0];
        }

        /// <summary>
        /// Send NLIST command to server
        /// NLIST
        ///     125, 150
        ///         226, 250
        ///         425, 426, 451
        ///     450
        ///     500, 501, 502, 421, 530
        /// </summary>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <param name="DATA_iostream">NetworkStream which will be get the list of files and directorys</param>
        /// <returns>Return list of directorys and files</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_450_file_unavailable_busy_exception"></exception>
        /// <exception cref="_425_can_not_open_data_connection_exception"></exception>
        /// <exception cref="_426_connection_vlosed_transfer_aborted_exception"></exception>
        /// <exception cref="_451_local_error_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="_534_request_denied_for_policy_reasons"></exception>
        /// <exception cref="_535_failed_security_check"></exception>
        /// <exception cref="IOException"></exception>
        private string[] NLIST(ref NetworkStream iostream, ref NetworkStream DATA_iostream)
        {
            return NLIST(null, ref iostream, ref DATA_iostream);
        }

        /// <summary>
        /// Send NLIST command to server
        /// NLIST
        ///     125, 150
        ///         226, 250
        ///         425, 426, 451
        ///     450
        ///     500, 501, 502, 421, 530
        /// </summary>
        /// <param name="pathname">path</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <param name="DATA_iostream">NetworkStream which will be get the list of files and directorys</param>
        /// <returns>Return list of directorys and files</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_450_file_unavailable_busy_exception"></exception>
        /// <exception cref="_425_can_not_open_data_connection_exception"></exception>
        /// <exception cref="_426_connection_vlosed_transfer_aborted_exception"></exception>
        /// <exception cref="_451_local_error_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="_534_request_denied_for_policy_reasons"></exception>
        /// <exception cref="_535_failed_security_check"></exception>
        /// <exception cref="IOException"></exception>
        private string[] NLIST(string pathname, ref NetworkStream iostream, ref NetworkStream DATA_iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();

                    if (pathname == null)
                    {
                        sb.Append("NLIST\r\n");
                    }
                    else
                    {
                        sb.AppendFormat("NLIST {0}\r\n", pathname);
                    }

                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("125"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("150"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (mode == ConnectionMode.Passive)
                        {
                            DicsonnectFrom(ref dtpPassive, ref DATA_iostream);
                        }

                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("450"))
                        {
                            throw new _450_file_unavailable_busy_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("504"))
                        {
                            throw new _504_command_not_implemented_for_that_parameter_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("553"))
                        {
                            throw new _553_file_name_not_allowed_exception(GetString(response));
                        }
                        if (ServerResponseCode(response).Trim().Equals("425"))
                        {
                            throw new _425_can_not_open_data_connection_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("426"))
                        {
                            throw new _426_connection_vlosed_transfer_aborted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("451"))
                        {
                            throw new _451_local_error_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("534"))
                        {
                            throw new _534_request_denied_for_policy_reasons(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("535"))
                        {
                            throw new _535_failed_security_check(GetString(response));
                        }
                    }


                    byte[] list = ReadServerDATA(ref DATA_iostream);


                    if (mode == ConnectionMode.Passive)
                    {
                        DicsonnectFrom(ref dtpPassive, ref DATA_iostream);
                    }

                    response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("226"))
                    {
                        OnServerResponse(GetString(response));

                        return GetString(list).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else if (ServerResponseCode(response).Trim().Equals("250"))
                    {
                        OnServerResponse(GetString(response));

                        return GetString(list).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("425"))
                        {
                            throw new _425_can_not_open_data_connection_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("426"))
                        {
                            throw new _426_connection_vlosed_transfer_aborted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("451"))
                        {
                            throw new _451_local_error_exception(GetString(response));
                        }
                    }
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }

            return new string[0];
        }

        /// <summary>
        /// Returns the size (in bytes) of the given regular file. This is the
        /// size on the server and may not accurately represent the file size 
        /// once the file has been transferred (particularly via ASCII mode)
        /// </summary>
        /// <param name="pathname">Path to the file</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>size in bytes</returns>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="IOException"></exception>
        private long SIZE(string pathname, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("SIZE {0}\r\n", pathname);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("213"))
                    {
                        OnServerResponse(GetString(response));

                        return Convert.ToInt64(GetString(response).Substring(4).Trim());
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                    }
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }

            return 0;
        }

        /// <summary>
        /// This command specifies the old pathname of the file which is
        /// to be renamed.  This command must be immediately followed by
        /// a "rename to" command specifying the new file pathname.
        /// RNFR
        ///     450, 550
        ///     500, 501, 502, 421, 530
        ///     350
        /// </summary>
        /// <param name="filename">Path</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_450_file_unavailable_busy_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string RNFR(string filename, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("RNFR {0}\r\n", filename);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("350"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("450"))
                        {
                            throw new _450_file_unavailable_busy_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// This command specifies the new pathname of the file
        /// specified in the immediately preceding "rename from"
        /// command.  Together the two commands cause a file to be
        /// renamed.
        /// RNTO
        ///     250
        ///     532, 553
        ///     500, 501, 502, 503, 421, 530
        /// </summary>
        /// <param name="filename">Path</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_503_bad_sequence_of_commands_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_553_file_name_not_allowed_exception"></exception>
        /// <exception cref="_532_need_account_for_storing_files_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string RNTO(string filename, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("RNTO {0}\r\n", filename);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("250"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("503"))
                        {
                            throw new _503_bad_sequence_of_commands_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("553"))
                        {
                            throw new _553_file_name_not_allowed_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("532"))
                        {
                            throw new _532_need_account_for_storing_files_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// This command requests the server-DTP to "listen" on a data
        /// port (which is not its default data port) and to wait for a
        /// connection rather than initiate one upon receipt of a
        /// transfer command.  The response to this command includes the
        /// host and port address this server is listening on.
        /// ip1,ip2,ip3,ip4,p1,p2
        /// PASV
        ///     227
        ///     500, 501, 502, 421, 530
        /// </summary>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return ip and port otherwise null</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="IOException"></exception>
        private PassiveConnectionInfo PASV(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    OnClientCommand("PASV\r\n");
                    byte[] buffer = Encoding.ASCII.GetBytes("PASV\r\n");

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("227"))
                    {
                        OnServerResponse(GetString(response));

                        PassiveConnectionInfo pInfo = new PassiveConnectionInfo();

                        Regex reg = new Regex("(?<ip1>[ a-zA-Z_0-9]*),(?<ip2>[ a-zA-Z_0-9]*),(?<ip3>[ a-zA-Z_0-9]*),(?<ip4>[ a-zA-Z_0-9]*),(?<p1>[ a-zA-Z_0-9]*),(?<p2>[ a-zA-Z_0-9]*)");
                        Match match = reg.Match(GetString(response));
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("{0}.{1}.{2}.{3}", match.Groups["ip1"].Value, match.Groups["ip2"].Value, match.Groups["ip3"].Value, match.Groups["ip4"].Value);

                        pInfo.ip = sb.ToString();
                        pInfo.port = Convert.ToInt32(match.Groups["p1"].Value) * 256 + Convert.ToInt32(match.Groups["p2"].Value);

                        return pInfo;
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                    }
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }

            return new PassiveConnectionInfo();
        }

        /// <summary>
        /// This command causes the server-DTP to transfer a copy of the
        /// file, specified in the pathname, to the server- or user-DTP
        /// at the other end of the data connection.  The status and
        /// contents of the file at the server site shall be unaffected.
        /// RETR
        ///     125, 150
        ///         (110)
        ///         226, 250
        ///         425, 426, 451
        ///     450, 550
        ///     500, 501, 421, 530
        /// </summary>
        /// <param name="filename">Name of file which will be downloaded from server</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <param name="DATA_iostream">NetworkStream which will be download file</param>
        /// <param name="stream">Stream in which data will be written</param>
        /// <returns>Return server response code.</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_450_file_unavailable_busy_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="_425_can_not_open_data_connection_exception"></exception>
        /// <exception cref="_426_connection_vlosed_transfer_aborted_exception"></exception>
        /// <exception cref="_451_local_error_exception"></exception>
        /// <exception cref="_534_request_denied_for_policy_reasons"></exception>
        /// <exception cref="_535_failed_security_check"></exception>
        /// <exception cref="IOException"></exception>
        private string RETR(string filename, ref NetworkStream iostream, ref NetworkStream DATA_iostream, ref Stream stream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("RETR {0}\r\n", filename);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("120"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("150"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (mode == ConnectionMode.Passive)
                        {
                            DicsonnectFrom(ref dtpPassive, ref DATA_iostream);
                        }

                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("450"))
                        {
                            throw new _450_file_unavailable_busy_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("534"))
                        {
                            throw new _534_request_denied_for_policy_reasons(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("535"))
                        {
                            throw new _535_failed_security_check(GetString(response));
                        }
                    }

                    ReadWriteServerDATA(ref DATA_iostream, ref stream);

                    if (mode == ConnectionMode.Passive)
                    {
                        DicsonnectFrom(ref dtpPassive, ref DATA_iostream);
                    }

                    response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("226"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("250"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("425"))
                        {
                            throw new _425_can_not_open_data_connection_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("426"))
                        {
                            throw new _426_connection_vlosed_transfer_aborted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("451"))
                        {
                            throw new _451_local_error_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// This command causes the server-DTP to accept the data
        /// transferred via the data connection and to store the data as
        /// a file at the server site.  If the file specified in the
        /// pathname exists at the server site, then its contents shall
        /// be replaced by the data being transferred.  A new file is
        /// created at the server site if the file specified in the
        /// pathname does not already exist.
        ///  STOR
        ///     125, 150
        ///         (110)
        ///         226, 250
        ///         425, 426, 451, 551, 552
        ///     532, 450, 452, 553
        ///     500, 501, 421, 530
        /// </summary>
        /// <param name="filename">Under this name will be created a file on the server</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <param name="DATA_iostream">NetworkStream which will be upload file</param>
        /// <param name="stream">Stream from which will be read data</param>
        /// <returns>Return server response code.</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_450_file_unavailable_busy_exception"></exception>
        /// <exception cref="_553_file_name_not_allowed_exception"></exception>
        /// <exception cref="_532_need_account_for_storing_files_exception"></exception>
        /// <exception cref="_452_insufficient_storage_space_in_system_exception"></exception>
        /// <exception cref="_425_can_not_open_data_connection_exception"></exception>
        /// <exception cref="_426_connection_vlosed_transfer_aborted_exception"></exception>
        /// <exception cref="_451_local_error_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="_551_page_type_unknown_exception"></exception>
        /// <exception cref="_552_exceeded_storage_allocation_exception"></exception>
        /// <exception cref="_534_request_denied_for_policy_reasons"></exception>
        /// <exception cref="_535_failed_security_check"></exception>
        /// <exception cref="IOException"></exception>
        private string STOR(string filename, ref NetworkStream iostream, ref NetworkStream DATA_iostream, ref Stream stream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("STOR {0}\r\n", filename);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("125"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("150"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (mode == ConnectionMode.Passive)
                        {
                            DicsonnectFrom(ref dtpPassive, ref DATA_iostream);
                        }

                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("450"))
                        {
                            throw new _450_file_unavailable_busy_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("553"))
                        {
                            throw new _553_file_name_not_allowed_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("532"))
                        {
                            throw new _532_need_account_for_storing_files_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("452"))
                        {
                            throw new _452_insufficient_storage_space_in_system_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("534"))
                        {
                            throw new _534_request_denied_for_policy_reasons(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("535"))
                        {
                            throw new _535_failed_security_check(GetString(response));
                        }
                    }

                    ReadWriteServerDATA(ref stream, ref DATA_iostream);

                    if (mode == ConnectionMode.Passive)
                    {
                        DicsonnectFrom(ref dtpPassive, ref DATA_iostream);
                    }

                    response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("226"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("250"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("425"))
                        {
                            throw new _425_can_not_open_data_connection_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("426"))
                        {
                            throw new _426_connection_vlosed_transfer_aborted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("451"))
                        {
                            throw new _451_local_error_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("551"))
                        {
                            throw new _551_page_type_unknown_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("552"))
                        {
                            throw new _552_exceeded_storage_allocation_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send FEAT command to server
        /// </summary>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return server response with list of features.</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string FEAT(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    OnClientCommand("FEAT\r\n");
                    byte[] buffer = Encoding.ASCII.GetBytes("FEAT\r\n");

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("211"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                    }

                    return GetString(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// The OPTS (options) command allows a user-PI to specify the desired behavior of a server-FTP process
        /// </summary>
        /// <param name="command"></param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return server response.</returns>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_451_local_error_exception"></exception>
        /// <exception cref="IOException"></exception>
        private byte[] OPTS(string command, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("OPTS {0}\r\n", command);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("451"))
                        {
                            throw new _451_local_error_exception(GetString(response));
                        }
                    }

                    return response;
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// Send name of program which using this library
        /// </summary>
        /// <param name="name">Name of program which using this library</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return server response code.</returns>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_451_local_error_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string CLNT(string name, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("CLNT {0}\r\n", name);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("451"))
                        {
                            throw new _451_local_error_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marker"></param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return server response code.</returns>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string REST(long marker, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("REST {0}\r\n", marker);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("350"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        private string MLST(ref NetworkStream iostream)
        {
            return MLST(null, ref iostream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathname">Path name</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns></returns>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string MLST(string pathname, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();

                    if (pathname == null)
                    {
                        sb.AppendFormat("MLST\r\n");
                    }
                    else
                    {
                        sb.AppendFormat("MLST {0}\r\n", pathname);
                    }

                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("250"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("504"))
                        {
                            throw new _504_command_not_implemented_for_that_parameter_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("553"))
                        {
                            throw new _553_file_name_not_allowed_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        private string[] MLSD(ref NetworkStream iostream, ref NetworkStream DATA_iostream)
        {
            return MLSD(null, ref iostream, ref DATA_iostream);
        }

        /// <summary>
        /// Send MLSD command to server
        /// </summary>
        /// <param name="pathname">path</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <param name="DATA_iostream">NetworkStream which will be get the list of files and directory</param>
        /// <returns>Return list of directory and files</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_530_not_logged_exception"></exception>
        /// <exception cref="_450_file_unavailable_busy_exception"></exception>
        /// <exception cref="_425_can_not_open_data_connection_exception"></exception>
        /// <exception cref="_426_connection_vlosed_transfer_aborted_exception"></exception>
        /// <exception cref="_451_local_error_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="IOException"></exception>
        private string[] MLSD(string pathname, ref NetworkStream iostream, ref NetworkStream DATA_iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();

                    if (pathname == null)
                    {
                        sb.Append("MLSD\r\n");
                    }
                    else
                    {
                        sb.AppendFormat("MLSD {0}\r\n", pathname);
                    }

                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("125"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("150"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (mode == ConnectionMode.Passive)
                        {
                            DicsonnectFrom(ref dtpPassive, ref DATA_iostream);
                        }

                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("450"))
                        {
                            throw new _450_file_unavailable_busy_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("504"))
                        {
                            throw new _504_command_not_implemented_for_that_parameter_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("553"))
                        {
                            throw new _553_file_name_not_allowed_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("425"))
                        {
                            throw new _425_can_not_open_data_connection_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("426"))
                        {
                            throw new _426_connection_vlosed_transfer_aborted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("451"))
                        {
                            throw new _451_local_error_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                    }


                    byte[] list = ReadServerDATA(ref DATA_iostream);


                    if (mode == ConnectionMode.Passive)
                    {
                        DicsonnectFrom(ref dtpPassive, ref DATA_iostream);
                    }

                    response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("226"))
                    {
                        OnServerResponse(GetString(response));

                        return GetString(list).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else if (ServerResponseCode(response).Trim().Equals("250"))
                    {
                        OnServerResponse(GetString(response));

                        return GetString(list).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("425"))
                        {
                            throw new _425_can_not_open_data_connection_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("426"))
                        {
                            throw new _426_connection_vlosed_transfer_aborted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("451"))
                        {
                            throw new _451_local_error_exception(GetString(response));
                        }
                    }
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }

            return new string[0];
        }

        /// <summary>
        /// Send MDTM command to server
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return time.</returns>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_450_file_unavailable_busy_exception"></exception>
        /// <exception cref="_451_local_error_exception"></exception>
        /// <exception cref="IOException"></exception>
        private DateTime MDTM(string filename, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("MDTM {0}\r\n", filename);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("213"))
                    {
                        OnServerResponse(GetString(response));

                        Regex reg = new Regex("([0-9]{3}) (?<time>[0-9/.]*)");
                        Match match = reg.Match(GetString(response));
                        sb = new StringBuilder();
                        sb.AppendFormat("{0}", match.Groups["time"].Value);

                        int year = Convert.ToInt32(sb.ToString().Substring(0, 4));
                        int month = Convert.ToInt32(sb.ToString().Substring(4, 2));
                        int day = Convert.ToInt32(sb.ToString().Substring(6, 2));
                        double val = Convert.ToDouble(sb.ToString().Substring(8));

                        DateTime time = new DateTime(year, month, day);

                        return time;
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("550"))
                        {
                            throw new _550_file_unavailable_not_found_no_access_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("450"))
                        {
                            throw new _450_file_unavailable_busy_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("451"))
                        {
                            throw new _451_local_error_exception(GetString(response));
                        }
                    }
                    return new DateTime();
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }


        /// <summary>
        /// Extended PORT
        /// </summary>
        /// <param name="NetPrtAddrPort">Example: |1|132.235.1.2|6275| (IPv4) or |2|1080::8:800:200C:417A|5282| (IPv6)</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return server response code.</returns>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_522_protocol_not_supported"></exception>
        /// <exception cref="IOException"></exception>
        private string EPRT(string NetPrtAddrPort, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("EPRT {0}\r\n", NetPrtAddrPort);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("522"))
                        {
                            //Example of server response:
                            //  Network protocol not supported, use (1)
                            //  Network protocol not supported, use (1,2)
                            //where 1 is IPv4 and 2 is IPv6
                            throw new _522_protocol_not_supported(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }


        /// <summary>
        /// Extended Passive Mode
        /// </summary>
        /// <param name="netPrt">Network protocol type</param>
        /// <param name="iostream">NetworkStream which will be sent the command</param>
        /// <returns>Return ip and port otherwise null</returns>
        /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
        /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
        /// <exception cref="_502_command_not_implemented_exception"></exception>
        /// <exception cref="_421_service_not_available_exception"></exception>
        /// <exception cref="_522_protocol_not_supported"></exception>
        /// <exception cref="IOException"></exception>
        private PassiveConnectionInfo EPSV(NetworkProtocol netPrt, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    byte[] buffer = null;
                    StringBuilder sb;

                    switch (netPrt)
                    {
                        case NetworkProtocol.IPv4:
                            sb = new StringBuilder();
                            sb.AppendFormat("EPSV {0}\r\n", 1);
                            OnClientCommand(sb.ToString());
                            buffer = Encoding.ASCII.GetBytes(sb.ToString());
                            break;
                        case NetworkProtocol.IPv6:
                            sb = new StringBuilder();
                            sb.AppendFormat("EPSV {0}\r\n", 2);
                            OnClientCommand(sb.ToString());
                            buffer = Encoding.ASCII.GetBytes(sb.ToString());
                            break;
                        case NetworkProtocol.ALL:
                            sb = new StringBuilder();
                            sb.AppendFormat("EPSV {0}\r\n", "ALL");
                            OnClientCommand(sb.ToString());
                            buffer = Encoding.ASCII.GetBytes(sb.ToString());
                            break;
                        default:
                            OnClientCommand("EPSV\r\n");
                            buffer = Encoding.ASCII.GetBytes("EPSV\r\n");
                            break;
                    }

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("229"))
                    {
                        OnServerResponse(GetString(response));

                        PassiveConnectionInfo pInfo = new PassiveConnectionInfo();

                        Regex reg = new Regex(@"\|(?<prt>.*)\|(?<addr>.*)\|(?<port>.*)\|");
                        Match match = reg.Match(GetString(response));

                        pInfo.ip = this.host;
                        pInfo.port = Convert.ToInt32(match.Groups["port"].Value);

                        return pInfo;
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("522"))
                        {
                            //Example of server response:
                            //  Network protocol not supported, use (1)
                            //  Network protocol not supported, use (1,2)
                            //where 1 is IPv4 and 2 is IPv6
                            throw new _522_protocol_not_supported(GetString(response));
                        }
                    }

                    return new PassiveConnectionInfo();
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// AUTHENTICATION/SECURITY MECHANISM
        /// AUTH
        ///     234
        ///     334
        ///     502, 504, 534, 431
        ///     500, 501, 421
        /// </summary>
        /// <param name="mechanism_name"></param>
        /// <param name="iostream"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private string AUTH(string mechanism_name, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();

                    if (mechanism_name == null)
                    {
                        sb.Append("AUTH\r\n");
                    }
                    else
                    {
                        sb.AppendFormat("AUTH {0}\r\n", mechanism_name);
                    }

                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("234"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("334"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("504"))
                        {
                            throw new _504_command_not_implemented_for_that_parameter_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("534"))
                        {
                            throw new _534_request_denied_for_policy_reasons(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("431"))
                        {
                            throw new _431_need_some_unavailable_resource_to_process_security(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }


        /// <summary>
        /// AUTHENTICATION/SECURITY DATA
        /// ADAT
        ///     235
        ///     335
        ///     503, 501, 535
        ///     500, 501, 421
        /// </summary>
        /// <param name="base64data"></param>
        /// <param name="iostream"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private string ADAT(string base64data, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("ADAT {0}\r\n", Convert.ToBase64String(Encoding.UTF8.GetBytes(base64data)));

                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("235"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("335"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("503"))
                        {
                            throw new _503_bad_sequence_of_commands_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("504"))
                        {
                            throw new _504_command_not_implemented_for_that_parameter_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("534"))
                        {
                            throw new _534_request_denied_for_policy_reasons(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("431"))
                        {
                            throw new _431_need_some_unavailable_resource_to_process_security(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("535"))
                        {
                            throw new _535_failed_security_check(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// PROTECTION BUFFER SIZE
        /// PBSZ
        ///     200
        ///     503
        ///     500, 501, 421, 530
        /// </summary>
        /// <param name="buffer_size"></param>
        /// <param name="iostream"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private string PBSZ(int buffer_size, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("PBSZ {0}\r\n", buffer_size);
                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("503"))
                        {
                            throw new _503_bad_sequence_of_commands_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }


        /// <summary>
        /// DATA CHANNEL PROTECTION LEVEL
        /// PROT
        ///     200
        ///     504, 536, 503, 534, 431
        ///     500, 501, 421, 530
        /// </summary>
        /// <param name="prot_code"></param>
        /// <param name="iostream"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private string PROT(ProtectionLevel prot_code, ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();

                    switch (prot_code)
                    {
                        case ProtectionLevel.Clear:
                            sb.AppendFormat("PROT C\r\n");
                            OnClientCommand(sb.ToString());
                            break;
                        case ProtectionLevel.Safe:
                            sb.AppendFormat("PROT S\r\n");
                            OnClientCommand(sb.ToString());
                            break;
                        case ProtectionLevel.Confidential:
                            sb.AppendFormat("PROT E\r\n");
                            OnClientCommand(sb.ToString());
                            break;
                        case ProtectionLevel.Private:
                            sb.AppendFormat("PROT P\r\n");
                            OnClientCommand(sb.ToString());
                            break;
                    }

                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else if (ServerResponseCode(response).Trim().Equals("150"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("503"))
                        {
                            throw new _503_bad_sequence_of_commands_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("530"))
                        {
                            throw new _530_not_logged_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("504"))
                        {
                            throw new _504_command_not_implemented_for_that_parameter_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("536"))
                        {
                            throw new _536_requested_PROT_level_not_supported_by_mechanism(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("534"))
                        {
                            throw new _534_request_denied_for_policy_reasons(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("431"))
                        {
                            throw new _431_need_some_unavailable_resource_to_process_security(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("521"))
                        {
                            throw new _521_data_connection_cannot_be_opened_with_this_PROT_setting(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("522"))
                        {
                            throw new _522_TLS_negotiation_failed_or_was_unacceptable(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }


        /// <summary>
        /// CLEAR COMMAND CHANNEL
        /// </summary>
        /// <param name="iostream"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private string CCC(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    OnClientCommand("CCC\r\n");
                    byte[] buffer = Encoding.ASCII.GetBytes("CCC\r\n");

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("200"))
                    {
                        OnServerResponse(GetString(response));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("503"))
                        {
                            throw new _503_bad_sequence_of_commands_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("533"))
                        {
                            // If the control connection is not protected with TLS
                            throw new _533_command_protection_level_denied_for_policy_reasons(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("534"))
                        {
                            // If the server does not wish to allow the control connection to be cleared at this time
                            throw new _534_request_denied_for_policy_reasons(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// INTEGRITY PROTECTED COMMAND 
        /// </summary>
        /// <param name="iostream"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private string MIC(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("MIC {0}\r\n", Convert.ToBase64String(Encoding.UTF8.GetBytes("safe")));

                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("631"))
                    {
                        OnServerResponse(GetString(Convert.FromBase64String(GetString(response))));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            // The server cannot base 64 decode the argument.
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            // A server may require that the first command after a successful
                            // security data exchange be CCC, and not implement the protection
                            // commands at all.
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("503"))
                        {
                            // If the server has not completed a security data exchange with the client
                            //
                            // or
                            //
                            // If the server has completed a security data exchange with the
                            // client using a mechanism which supports integrity, and requires a
                            // CCC command due to policy or implementation limitations
                            throw new _503_bad_sequence_of_commands_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("535"))
                        {
                            // If the server rejects the command (if a checksum fails, for instance)
                            throw new _535_failed_security_check(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("533"))
                        {
                            // If the server is not willing to accept the command (if privacy is
                            // required by policy, for instance, or if a CONF command is received
                            // before a CCC command)
                            throw new _533_command_protection_level_denied_for_policy_reasons(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("537"))
                        {
                            // If the server rejects the command because it is not supported by 
                            // the current security mechanism
                            throw new _537_command_protection_level_not_supported_by_security_mechanism(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// CONFIDENTIALITY PROTECTED COMMAND 
        /// </summary>
        /// <param name="iostream"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private string CONF(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("CONF {0}\r\n", Convert.ToBase64String(Encoding.UTF8.GetBytes("confidential")));

                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("633"))
                    {
                        OnServerResponse(GetString(Convert.FromBase64String(GetString(response))));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            // The server cannot base 64 decode the argument.
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            // A server may require that the first command after a successful
                            // security data exchange be CCC, and not implement the protection
                            // commands at all.
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("503"))
                        {
                            // If the server has not completed a security data exchange with the client
                            //
                            // or
                            //
                            // If the server has completed a security data exchange with the
                            // client using a mechanism which supports integrity, and requires a
                            // CCC command due to policy or implementation limitations
                            throw new _503_bad_sequence_of_commands_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("535"))
                        {
                            // If the server rejects the command (if a checksum fails, for instance)
                            throw new _535_failed_security_check(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("533"))
                        {
                            // If the server is not willing to accept the command (if privacy is
                            // required by policy, for instance, or if a CONF command is received
                            // before a CCC command)
                            throw new _533_command_protection_level_denied_for_policy_reasons(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("537"))
                        {
                            // If the server rejects the command because it is not supported by 
                            // the current security mechanism
                            throw new _537_command_protection_level_not_supported_by_security_mechanism(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// PRIVACY PROTECTED COMMAND 
        /// </summary>
        /// <param name="iostream"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private string ENC(ref NetworkStream iostream)
        {
            try
            {
                if (iostream.CanWrite)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("ENC {0}\r\n", Convert.ToBase64String(Encoding.UTF8.GetBytes("confidential")));

                    OnClientCommand(sb.ToString());
                    byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());

                    iostream.Write(buffer, 0, buffer.Length);
                    iostream.Flush();

                    byte[] response = ReadServerResponseMultiline(ref iostream);

                    if (ServerResponseCode(response).Trim().Equals("632"))
                    {
                        OnServerResponse(GetString(Convert.FromBase64String(GetString(response))));
                    }
                    else
                    {
                        if (ServerResponseCode(response).Trim().Equals("501"))
                        {
                            // The server cannot base 64 decode the argument.
                            throw new _501_command_syntax_error_invalid_parameter_or_argument_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("502"))
                        {
                            // A server may require that the first command after a successful
                            // security data exchange be CCC, and not implement the protection
                            // commands at all.
                            throw new _502_command_not_implemented_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("503"))
                        {
                            // If the server has not completed a security data exchange with the client
                            //
                            // or
                            //
                            // If the server has completed a security data exchange with the
                            // client using a mechanism which supports integrity, and requires a
                            // CCC command due to policy or implementation limitations
                            throw new _503_bad_sequence_of_commands_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("500"))
                        {
                            throw new _500_ñommand_syntax_error_could_not_interpreted_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("421"))
                        {
                            throw new _421_service_not_available_exception(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("535"))
                        {
                            // If the server rejects the command (if a checksum fails, for instance)
                            throw new _535_failed_security_check(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("533"))
                        {
                            // If the server is not willing to accept the command (if privacy is
                            // required by policy, for instance, or if a CONF command is received
                            // before a CCC command)
                            throw new _533_command_protection_level_denied_for_policy_reasons(GetString(response));
                        }
                        else if (ServerResponseCode(response).Trim().Equals("537"))
                        {
                            // If the server rejects the command because it is not supported by 
                            // the current security mechanism
                            throw new _537_command_protection_level_not_supported_by_security_mechanism(GetString(response));
                        }
                    }

                    return ServerResponseCode(response);
                }
                else
                {
                    throw new Exception("Client closed the connection.");
                }
            }
            catch (IOException ex)
            {
                OnServerResponse(ex.Message);

                DicsonnectFrom(ref ftp, ref iostream);

                throw new IOException(ex.Message);
            }
        }

        #endregion
    }
}
