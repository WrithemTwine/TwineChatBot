using System;

namespace SimpleTestFeature.Exceptions
{
    internal class UnauthorizedException(string message) : Exception(message)
    {
    }
}
