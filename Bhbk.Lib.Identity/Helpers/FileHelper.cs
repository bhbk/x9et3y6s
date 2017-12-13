using System.IO;
using System.Reflection;

namespace Bhbk.Lib.Identity.Helpers
{
    public class FileHelper
    {
        public static FileInfo SearchPaths(string file)
        {
            string result;

            result = Directory.GetCurrentDirectory() 
                + Path.DirectorySeparatorChar + file;

            if (File.Exists(result))
                return new FileInfo(result);
            
            result = Directory.GetCurrentDirectory() 
                + Path.DirectorySeparatorChar + ".."
                + Path.DirectorySeparatorChar + ".."
                + Path.DirectorySeparatorChar + ".."
                + Path.DirectorySeparatorChar + file;

            if (File.Exists(result))
                return new FileInfo(result);

            result = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName
                + Path.DirectorySeparatorChar + file;

            if (File.Exists(result))
                return new FileInfo(result);

            result = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName
                + Path.DirectorySeparatorChar + ".."
                + Path.DirectorySeparatorChar + ".."
                + Path.DirectorySeparatorChar + ".."
                + Path.DirectorySeparatorChar + file;

            if (File.Exists(result))
                return new FileInfo(result);

            throw new FileNotFoundException();
        }
    }
}
