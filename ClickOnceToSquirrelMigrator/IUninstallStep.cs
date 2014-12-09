using System;
using System.Collections.Generic;

namespace ClickOnceToSquirrelMigrator
{
    internal interface IUninstallStep : IDisposable
    {
        void Execute();

        void Prepare(List<string> componentsToRemove);

        void PrintDebugInformation();
    }
}