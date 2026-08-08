namespace Infrastructure.Common.Files
{
    public class FileUploadOptions
    {
        public const string SectionName = "FileSettings";

        /// <summary>
        /// 文件访问基址，用于拼接上传后的完整 URL（如 http://localhost:9080）。
        /// </summary>
        public string BaseUrl { get; set; } = default!;

        /// <summary>
        /// 单次上传文件数量上限，默认 10。
        /// </summary>
        public int MaxFileCount { get; set; } = 10;
    }
}
