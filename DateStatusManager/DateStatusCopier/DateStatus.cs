using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DateStatusCopier {
    public class DateStatus {
        public DateTime? LastAccessTime { get; set; } = null;
        public DateTime? LastWriteTime { get; set; } = null;
        public DateTime? CreationTime { get; set; } = null;
        public TimeSpan? LastAccessOffset { get; set; } = null;
        public TimeSpan? LastWriteOffset { get; set; } = null;
        public TimeSpan? CreationOffset { get; set; } = null;
        public string Path { get; set; } = null;
        public DateStatus[] Subdirectories { get; set; } = null;
        [JsonIgnore]
        public bool IsUniversal { get => Path == null; }
        public bool IsDirectory { get; set; }
        public bool IsRecursive { get; set; } = false;

        public DateStatus() { }
        public DateStatus(string path, string relativePath = null, bool recursive = false, bool lastAccess = true, bool lastWrite = true, bool creation = true) {
            Path = string.IsNullOrEmpty(relativePath) ? null : relativePath;
            if (lastAccess)
                LastAccessTime = File.GetLastAccessTime(path);
            if (lastWrite)
                LastWriteTime = File.GetLastWriteTime(path);
            if (creation)
                CreationTime = File.GetCreationTime(path);
            IsDirectory = Directory.Exists(path);
            if (IsDirectory && this.Path != null && recursive) {
                string[] directories = Directory.GetDirectories(path);
                if (directories?.Length != 0) {
                    var subs = new List<DateStatus>();
                    foreach (string directory in directories)
                        subs.Add(new DateStatus(directory, System.IO.Path.GetFileName(directory), true, lastAccess, lastWrite, creation));
                    Subdirectories = subs.ToArray();
                }
            }
        }

        public string ToJson(bool ignoreNull = true) {
            JsonSerializerSettings settings = new JsonSerializerSettings {
                NullValueHandling = ignoreNull ? NullValueHandling.Ignore : NullValueHandling.Include
            };
            return JsonConvert.SerializeObject(this, settings);
        }
    }
}
