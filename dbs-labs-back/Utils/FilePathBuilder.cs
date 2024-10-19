namespace dbs_labs_back.Utils ;

    public class FilePathBuilder
    {
        private static readonly string BaseDirectory = "D:\\Studying\\4th year\\PDS\\labs\\dbs-labs-back\\dbs-labs-back\\";
        public static string GetSafeFilePath(string fileName)
        {
           
            var safeFileName = Path.GetFileName(fileName);

           
            var absoluteFilePath = Path.Combine(BaseDirectory, safeFileName);

           
            if (!absoluteFilePath.StartsWith(BaseDirectory))
            {
                throw new UnauthorizedAccessException("Access to the file is denied.");
            }

            return absoluteFilePath;
        }
    }