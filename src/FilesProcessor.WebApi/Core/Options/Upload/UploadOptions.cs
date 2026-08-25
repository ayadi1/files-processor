namespace FilesProcessor.WebApi.Core.Options.Upload
{
    public class UploadOptions
    {
        public static string SectionName = "Upload";
        public string RootPath { get; init; } = string.Empty;
        public long MaxFileBytes { get; init; }
    }
}