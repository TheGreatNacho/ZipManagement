using ZipManagement;

namespace ZipConsoleSandbox
{
    public class Program
    {
        public static void Main()
        {
            Stream s = new FileStream("TestData - Store.zip", FileMode.Open);
            using (ZipReader reader = new ZipReader(s))
            {
                EOCDRecord eocd = reader.GetEOCDRecord();
                CentralDirectoryRecord[] cd_dirs = reader.GetCentralDirectoryRecords(eocd);
                byte[] record = reader.GetEncryptedFile(cd_dirs[1]);

                foreach (byte b in record)
                {
                    Console.Write(((char)b));

                }
            }
         }
    }
}