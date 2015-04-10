using System;

namespace CoreInfrastructure.Logging
{
    public class StoredProcException : Exception
    {
        public string StoredProcName { get; private set; }
        public string ParameterString { get; private set; }

        public StoredProcException(string message, string storedProc, string paramString) : base(message)
        {
            StoredProcName = storedProc;
            ParameterString = paramString;
        }

        public StoredProcException(string message, string storedProc, string paramString, Exception innerException) : base(message, innerException)
        {
            StoredProcName = storedProc;
            ParameterString = paramString;
        }
    }
}
