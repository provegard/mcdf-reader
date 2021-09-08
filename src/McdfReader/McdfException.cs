using System;

namespace McdfReader
{
    /// <summary>
    /// Exception type for errors encountered while parsing an MCDF file.
    /// </summary>
    public class McdfException : Exception
    {
        public McdfException(string message) : base(message)
        {
        }
    }
}