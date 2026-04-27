using FilenameBuddy;

namespace GameDonkey.Tests
{
    public static class TestHelpers
    {
        private static readonly string TestDir = Path.Combine(Path.GetTempPath(), "GameDonkeyTests");

        public static void InitFilePaths()
        {
            Directory.CreateDirectory(Path.Combine(TestDir, "Content"));
            Filename.SetCurrentDirectory(TestDir);
        }
    }
}
