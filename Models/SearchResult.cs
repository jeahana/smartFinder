using System;
using System.IO;

namespace smartFinder.Models
{
    public class SearchResult
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string Directory { get; set; }
        public string FullPath { get; set; }
        public long SizeBytes { get; set; }
        public DateTime LastModified { get; set; }
        public int MatchCount { get; set; }
        public int LineNo { get; set; } = 1; // 매칭된 라인 번호
        public string LineText { get; set; } = ""; // 매칭된 라인의 실제 텍스트
        public string Note { get; set; } // 비고 컬럼 (Editable)
        public string MatchedTerms { get; set; } = ""; // 매칭된 검색어 컬럼
        
        public SearchResult(string fullPath)
        {
            try
            {
                var info = new FileInfo(fullPath);
                FileName = info.Name;
                Extension = (info.Extension ?? "").TrimStart('.');
                Directory = info.DirectoryName ?? "";
                FullPath = fullPath;
                if (info.Exists)
                {
                    SizeBytes = info.Length;
                    LastModified = info.LastWriteTime;
                }
                else
                {
                    SizeBytes = 0;
                    LastModified = DateTime.Now;
                }
            }
            catch
            {
                FullPath = fullPath;
                FileName = Path.GetFileName(fullPath);
                Extension = (Path.GetExtension(fullPath) ?? "").TrimStart('.');
                Directory = Path.GetDirectoryName(fullPath) ?? "";
                SizeBytes = 0;
                LastModified = DateTime.Now;
            }
        }
    }
}
