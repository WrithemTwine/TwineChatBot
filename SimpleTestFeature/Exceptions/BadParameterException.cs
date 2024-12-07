using System;

namespace SimpleTestFeature.Exceptions
{
    internal class BadParameterException(string message) : Exception(message)
    {
    }
}
