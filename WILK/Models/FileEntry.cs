namespace WILK.Models
{
    public class FileEntry
    {
        public string FileName { get; }
        public string Content { get; }

        public FileEntry(string fileName, string content)
        {
            FileName = fileName;
            Content = content;
        }

        public override string ToString() => Path.GetFileName(FileName) ?? FileName;
    }
}
