using System;
using System.Collections.Generic;
using System.Text;

namespace FTPLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class ServerResponseException : Exception
    {
        private String message;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">Exception message</param>
        public ServerResponseException(String message)
        {
            this.message = message;
        }

        /// <summary>
        /// Code of the error response
        /// </summary>
        public int Code
        {
            get
            {
                return Int32.Parse(message.Substring(0, 4));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override String Message
        {
            get
            {
                return message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Message</returns>
        public override String ToString()
        {
            return message;
        }
    }

    /// <summary>
    /// Not logged in.
    /// </summary>
    /// <exception cref="_530_not_logged_exception">Not logged in.</exception>
    public class _530_not_logged_exception : ServerResponseException
    {
        /// <summary>
        /// Not logged in.
        /// </summary>
        public _530_not_logged_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Syntax error, command unrecognized.
    /// This may include errors such as command line too long.
    /// </summary>
    /// <exception cref="_500_ñommand_syntax_error_could_not_interpreted_exception"></exception>
    public class _500_ñommand_syntax_error_could_not_interpreted_exception : ServerResponseException
    {
        /// <summary>
        /// Syntax error, command unrecognized.
        /// This may include errors such as command line too long.
        /// </summary>
        public _500_ñommand_syntax_error_could_not_interpreted_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Syntax error in parameters or arguments.
    /// </summary>
    /// <exception cref="_501_command_syntax_error_invalid_parameter_or_argument_exception"></exception>
    public class _501_command_syntax_error_invalid_parameter_or_argument_exception : ServerResponseException
    {
        /// <summary>
        /// Syntax error in parameters or arguments.
        /// </summary>
        public _501_command_syntax_error_invalid_parameter_or_argument_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Service not available, closing control connection.
    /// This may be a reply to any command if the service knows it
    /// must shut down.
    /// </summary>
    /// <exception cref="_421_service_not_available_exception"></exception>
    public class _421_service_not_available_exception : ServerResponseException
    {
        /// <summary>
        /// Service not available, closing control connection.
        /// This may be a reply to any command if the service knows it
        /// must shut down.
        /// </summary>
        public _421_service_not_available_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Bad sequence of commands.
    /// </summary>
    /// <exception cref="_503_bad_sequence_of_commands_exception"></exception>
    public class _503_bad_sequence_of_commands_exception : ServerResponseException
    {
        /// <summary>
        /// Bad sequence of commands.
        /// </summary>
        public _503_bad_sequence_of_commands_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Command not implemented.
    /// </summary>
    /// <exception cref="_502_command_not_implemented_exception"></exception>
    public class _502_command_not_implemented_exception : ServerResponseException
    {
        /// <summary>
        /// Command not implemented.
        /// </summary>
        public _502_command_not_implemented_exception(String message)
            : base(message)
        {
        }
    }
    
    /// <summary>
    /// Requested action not taken.
    /// File unavailable (e.g., file not found, no access).
    /// </summary>
    /// <exception cref="_550_file_unavailable_not_found_no_access_exception"></exception>
    public class _550_file_unavailable_not_found_no_access_exception : ServerResponseException
    {
        /// <summary>
        /// Requested action not taken.
        /// File unavailable (e.g., file not found, no access).
        /// </summary>
        public _550_file_unavailable_not_found_no_access_exception(String message)
            : base(message)
        {
        }
    }
    
    /// <summary>
    /// Requested file action not taken.
    /// File unavailable (e.g., file busy).
    /// </summary>
    /// <exception cref="_450_file_unavailable_busy_exception"></exception>
    public class _450_file_unavailable_busy_exception : ServerResponseException
    {
        /// <summary>
        /// Requested file action not taken.
        /// File unavailable (e.g., file busy).
        /// </summary>
        public _450_file_unavailable_busy_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Command not implemented for that parameter.
    /// </summary>
    /// <exception cref="_504_command_not_implemented_for_that_parameter_exception"></exception>
    public class _504_command_not_implemented_for_that_parameter_exception : ServerResponseException
    {
        /// <summary>
        /// Command not implemented for that parameter.
        /// </summary>
        public _504_command_not_implemented_for_that_parameter_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Can't open data connection.
    /// </summary>
    /// <exception cref="_425_can_not_open_data_connection_exception"></exception>
    public class _425_can_not_open_data_connection_exception : ServerResponseException
    {
        /// <summary>
        /// Can't open data connection.
        /// </summary>
        public _425_can_not_open_data_connection_exception(String message)
            : base(message)
        {
        }
    }
    /// <summary>
    /// Connection closed; transfer aborted.
    /// </summary>
    /// <exception cref="_426_connection_vlosed_transfer_aborted_exception"></exception>
    public class _426_connection_vlosed_transfer_aborted_exception : ServerResponseException
    {
        /// <summary>
        /// Connection closed; transfer aborted.
        /// </summary>
        public _426_connection_vlosed_transfer_aborted_exception(String message)
            : base(message)
        {
        }
    }
    /// <summary>
    /// Requested action aborted: local error in processing.
    /// </summary>
    /// <exception cref="_451_local_error_exception"></exception>
    public class _451_local_error_exception : ServerResponseException
    {
        /// <summary>
        /// Requested action aborted: local error in processing.
        /// </summary>
        public _451_local_error_exception(String message)
            : base(message)
        {
        }
    }
    /// <summary>
    /// Requested action not taken.
    /// File name not allowed.
    /// </summary>
    /// <exception cref="_553_file_name_not_allowed_exception"></exception>
    public class _553_file_name_not_allowed_exception : ServerResponseException
    {
        /// <summary>
        /// Requested action not taken.
        /// File name not allowed.
        /// </summary>
        public _553_file_name_not_allowed_exception(String message)
            : base(message)
        {
        }
    }
    /// <summary>
    /// Need account for storing files.
    /// </summary>
    /// <exception cref="_532_need_account_for_storing_files_exception"></exception>
    public class _532_need_account_for_storing_files_exception : ServerResponseException
    {
        /// <summary>
        /// Need account for storing files.
        /// </summary>
        public _532_need_account_for_storing_files_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Need account for login.
    /// </summary>
    /// <exception cref="_332_need_account_for_login_exception"></exception>
    public class _332_need_account_for_login_exception : ServerResponseException
    {
        /// <summary>
        /// Need account for login.
        /// </summary>
        public _332_need_account_for_login_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Requested action not taken.
    /// Insufficient storage space in system.
    /// </summary>
    /// <exception cref="_452_insufficient_storage_space_in_system_exception"></exception>
    public class _452_insufficient_storage_space_in_system_exception : ServerResponseException
    {
        /// <summary>
        /// Requested action not taken.
        /// Insufficient storage space in system.
        /// </summary>
        public _452_insufficient_storage_space_in_system_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Requested file action aborted. Exceeded storage allocation (for current directory or dataset).
    /// </summary>
    /// <exception cref="_552_exceeded_storage_allocation_exception"></exception>
    public class _552_exceeded_storage_allocation_exception : ServerResponseException
    {
        /// <summary>
        /// Requested file action aborted. Exceeded storage allocation (for current directory or dataset).
        /// </summary>
        public _552_exceeded_storage_allocation_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Requested action aborted. Page type unknown.
    /// </summary>
    /// <exception cref="_551_page_type_unknown_exception"></exception>
    public class _551_page_type_unknown_exception : ServerResponseException
    {
        /// <summary>
        /// Requested action aborted. Page type unknown.
        /// </summary>
        public _551_page_type_unknown_exception(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Protocol not supported.
    /// Example of server response:
    ///     Network protocol not supported, use (1)
    ///     Network protocol not supported, use (1,2)
    /// where 1 is IPv4 and 2 is IPv6
    /// </summary>
    /// <exception cref="_522_protocol_not_supported"></exception>
    public class _522_protocol_not_supported : ServerResponseException
    {
        /// <summary>
        /// Protocol not supported.
        /// Example of server response:
        ///     Network protocol not supported, use (1)
        ///     Network protocol not supported, use (1,2)
        /// where 1 is IPv4 and 2 is IPv6
        /// </summary>
        public _522_protocol_not_supported(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Request denied for policy reasons
    /// </summary>
    /// <exception cref="_534_request_denied_for_policy_reasons"></exception>
    public class _534_request_denied_for_policy_reasons : ServerResponseException
    {
        /// <summary>
        /// Request denied for policy reasons
        /// </summary>
        public _534_request_denied_for_policy_reasons(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Need some unavailable resource to process security
    /// </summary>
    /// <exception cref="_431_need_some_unavailable_resource_to_process_security"></exception>
    public class _431_need_some_unavailable_resource_to_process_security : ServerResponseException
    {
        /// <summary>
        /// Need some unavailable resource to process security
        /// </summary>
        public _431_need_some_unavailable_resource_to_process_security(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Failed security check (hash, sequence, etc).
    /// </summary>
    /// <exception cref="_535_failed_security_check"></exception>
    public class _535_failed_security_check : ServerResponseException
    {
        /// <summary>
        /// Failed security check (hash, sequence, etc).
        /// </summary>
        public _535_failed_security_check(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Requested PROT level not supported by mechanism
    /// </summary>
    /// <exception cref="_536_requested_PROT_level_not_supported_by_mechanism"></exception>
    public class _536_requested_PROT_level_not_supported_by_mechanism : ServerResponseException
    {
        /// <summary>
        /// Requested PROT level not supported by mechanism
        /// </summary>
        public _536_requested_PROT_level_not_supported_by_mechanism(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Command protection level denied for policy reasons
    /// </summary>
    /// <exception cref="_533_command_protection_level_denied_for_policy_reasons"></exception>
    public class _533_command_protection_level_denied_for_policy_reasons : ServerResponseException
    {
        /// <summary>
        /// Command protection level denied for policy reasons
        /// </summary>
        public _533_command_protection_level_denied_for_policy_reasons(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Command protection level not supported by security mechanism
    /// </summary>
    /// <exception cref="_537_command_protection_level_not_supported_by_security_mechanism"></exception>
    public class _537_command_protection_level_not_supported_by_security_mechanism : ServerResponseException
    {
        /// <summary>
        /// Command protection level not supported by security mechanism
        /// </summary>
        public _537_command_protection_level_not_supported_by_security_mechanism(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Supported address families are af1, .., afn
    /// </summary>
    /// <exception cref="_521_data_connection_cannot_be_opened_with_this_PROT_setting"></exception>
    public class _521_data_connection_cannot_be_opened_with_this_PROT_setting : ServerResponseException
    {
        /// <summary>
        /// Supported address families are af1, .., afn
        /// </summary>
        public _521_data_connection_cannot_be_opened_with_this_PROT_setting(String message)
            : base(message)
        {
        }
    }

    
    /// <summary>
    /// Protocol not supported.
    /// </summary>
    /// <exception cref="_522_TLS_negotiation_failed_or_was_unacceptable"></exception>
    public class _522_TLS_negotiation_failed_or_was_unacceptable : ServerResponseException
    {
        /// <summary>
        /// Protocol not supported.
        /// </summary>
        public _522_TLS_negotiation_failed_or_was_unacceptable(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Command not implemented, superfluous at this site.
    /// </summary>
    /// <exception cref="_202_command_not_implemented_superfluous_at_this_site"></exception>
    public class _202_command_not_implemented_superfluous_at_this_site : ServerResponseException
    {
        /// <summary>
        /// Command not implemented, superfluous at this site.
        /// </summary>
        public _202_command_not_implemented_superfluous_at_this_site(String message)
            : base(message)
        {
        }
    }
}
