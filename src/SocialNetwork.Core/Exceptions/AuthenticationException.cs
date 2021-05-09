using System;

namespace SocialNetwork.Core.Exceptions
{
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message)
        {
        }
    }

    public class UserNotFoundException : Exception
    {
        public UserNotFoundException()
        {
            
        }

        public UserNotFoundException(string message) : base(message)
        {
        }
    }
}