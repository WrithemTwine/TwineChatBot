using System;

namespace SimpleTestFeature.Exceptions
{
    internal class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}
