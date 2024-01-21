using System;

namespace SimpleTestFeature.Exceptions
{
    internal class BadRequestException : Exception
    {
        internal BadRequestException(string message) : base(message) { }
    }
}
