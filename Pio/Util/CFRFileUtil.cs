namespace Client.Util
{
    static class CFRFileUtil
    {
        public static string[] GetAllCFRFiles(string directory) => Directory.GetFiles(directory).Where(x => x.ToUpper().EndsWith(".CFR")).ToArray();
    }
}
