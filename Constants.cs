using System.IO;
using System.Windows.Forms;

namespace PlansParser
{
    class Constants
    {
        public static TextBox BaseFolderTextBox { get; set; }

        public static string BaseDir
        {
            get { return BaseFolderTextBox.Text; }
        }

        public static string PdfFolder
        {
            get { return Path.Combine(BaseDir, "pdf"); }
        }

        public static string CsvFolder
        {
            get { return Path.Combine(BaseDir, "csv"); }
        }

        public static string XlsFolder
        {
            get { return Path.Combine(BaseDir, "xls"); }
        }

        public static string IgnoreAskIdFilePath
        {
            get { return Path.Combine(new DirectoryInfo(BaseDir).Parent.FullName, "IgnoreAksID.csv"); }
        }

        public static string KeyWordsFilePath
        {
            get { return Path.Combine(BaseDir, "keyWordsFile.txt"); }
        }

        public static string GoogleScraperResultFileName
        {
            get { return KeyWordsFilePath.Replace(".", "_Result."); }
        }

        public static string ProblemXlsDir { get { return Path.Combine(BaseDir, "Problem"); } }
    }
}
