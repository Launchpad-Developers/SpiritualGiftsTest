using System.Runtime.CompilerServices;

namespace SpiritualGiftsSurvey.Tests;

/// <summary>
/// Module initializer to set up SQLite native libraries for test environment.
/// This runs once before any tests execute.
/// </summary>
internal static class TestInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Initialize SQLite native libraries for cross-platform test execution
        SQLitePCL.Batteries_V2.Init();
    }
}
