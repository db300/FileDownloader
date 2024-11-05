namespace FileDownloader
{
    /// <summary>
    /// 下载任务实体
    /// </summary>
    internal class DownloadTaskItem
    {
        /// <summary>
        /// 下载链接
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 保存文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 任务索引
        /// </summary>
        public int Index { get; set; } = -1;
        /// <summary>
        /// 下载临时文件
        /// </summary>
        public string TmpFileName { get; set; }
    }
}
