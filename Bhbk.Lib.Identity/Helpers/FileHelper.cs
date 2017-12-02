using System.IO;
using System.Reflection;

namespace Bhbk.Lib.Identity.Helpers
{
    public class FileHelper
    {
        public static FileInfo FindFileInDefaultPaths(string file)
        {
            string result;

            result = Directory.GetCurrentDirectory() + @"\" + file;

            if (File.Exists(result))
                return new FileInfo(result);

            result = Directory.GetCurrentDirectory() + @"\..\..\..\" + file;

            if (File.Exists(result))
                return new FileInfo(result);

            result = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName + @"\" + file;

            if (File.Exists(result))
                return new FileInfo(result);

            result = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName + @"\..\..\..\" + file;

            if (File.Exists(result))
                return new FileInfo(result);

            throw new FileNotFoundException();
        }
    }
}
