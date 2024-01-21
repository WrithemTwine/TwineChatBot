using System;

namespace SimpleTestFeature.Exceptions
{
    internal class BadParameterException : Exception
    {
        public BadParameterException(string message) : base(message) { }
    }
}
