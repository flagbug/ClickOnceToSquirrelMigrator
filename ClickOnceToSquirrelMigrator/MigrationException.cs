using System;

namespace ClickOnceToSquirrelMigrator
{
    public class MigrationException : Exception
    {
        public MigrationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}