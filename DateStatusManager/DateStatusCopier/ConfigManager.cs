using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace DateStatusCopier {
    public static class Applier {
        static void ApplyToFile(string filePath, DateStatus dateStatus) {
            if (dateStatus.LastAccessTime.HasValue)
                File.SetLastAccessTime(filePath, dateStatus.LastAccessTime.Value);
            else if (dateStatus.LastAccessOffset.HasValue)
                File.SetLastAccessTime(filePath, File.GetLastAccessTime(filePath) + dateStatus.LastAccessOffset.Value);
            if (dateStatus.LastWriteTime.HasValue)
                File.SetLastWriteTime(filePath, dateStatus.LastWriteTime.Value);
            else if (dateStatus.LastWriteOffset.HasValue)
                File.SetLastWriteTime(filePath, File.GetLastWriteTime(filePath) + dateStatus.LastWriteOffset.Value);
            if (dateStatus.CreationTime.HasValue)
                File.SetCreationTime(filePath, dateStatus.CreationTime.Value);
            else if (dateStatus.CreationOffset.HasValue)
                File.SetCreationTime(filePath, File.GetCreationTime(filePath) + dateStatus.CreationOffset.Value);
        }
        static void ApplyToDirectory(string folderPath, DateStatus dateStatus) {
            if (dateStatus.LastAccessTime.HasValue)
                Directory.SetLastAccessTime(folderPath, dateStatus.LastAccessTime.Value);
            else if (dateStatus.LastAccessOffset.HasValue)
                Directory.SetLastAccessTime(folderPath, Directory.GetLastAccessTime(folderPath) + dateStatus.LastAccessOffset.Value);
            if (dateStatus.LastWriteTime.HasValue)
                Directory.SetLastWriteTime(folderPath, dateStatus.LastWriteTime.Value);
            else if (dateStatus.LastWriteOffset.HasValue)
                Directory.SetLastWriteTime(folderPath, Directory.GetLastWriteTime(folderPath) + dateStatus.LastWriteOffset.Value);
            if (dateStatus.CreationTime.HasValue)
                Directory.SetCreationTime(folderPath, dateStatus.CreationTime.Value);
            else if (dateStatus.CreationOffset.HasValue)
                Directory.SetCreationTime(folderPath, Directory.GetCreationTime(folderPath) + dateStatus.CreationOffset.Value);
        }
        static void Apply(string[] paths, DateStatus[] dateStatuses) {
            if (dateStatuses.Length == 1 && dateStatuses[0].IsUniversal) {
                DateStatus dateStatus = dateStatuses[0];
                foreach (string path in paths) {
                    if (File.Exists(path))
                        ApplyToFile(path, dateStatus);
                    else {
                        ApplyToDirectory(path, dateStatus);
                        if (dateStatus.IsRecursive) {
                            string[] files = Directory.GetFiles(path);
                            string[] folders = Directory.GetDirectories(path);
                            Apply(files.Concat(folders).ToArray(), dateStatuses);
                        }
                    }
                }
            }
            else {
                var appliedFiles = new HashSet<string>();
                DateStatus defaultDateStatus = dateStatuses.First((dateStatus) => dateStatus.IsUniversal);
                foreach (DateStatus dateStatus in dateStatuses)
                    if (!dateStatus.IsUniversal) {
                        foreach (string path in paths)
                            if (Path.GetFileName(path) == dateStatus.Path) {
                                if (File.Exists(path))
                                    ApplyToFile(path, dateStatus);
                                else {
                                    ApplyToDirectory(path, dateStatus);
                                    if (dateStatus.IsRecursive) {
                                        string[] files = Directory.GetFiles(path);
                                        string[] folders = Directory.GetDirectories(path);
                                        Apply(files.Concat(folders).ToArray(), dateStatuses);
                                    }
                                }
                                appliedFiles.Add(path);
                            }
                    }
                if (defaultDateStatus != null) {
                    foreach (string path in paths)
                        if (!appliedFiles.Contains(path)) {
                            if (File.Exists(path))
                                ApplyToFile(path, defaultDateStatus);
                            else {
                                ApplyToDirectory(path, defaultDateStatus);
                                if (defaultDateStatus.IsRecursive) {
                                    string[] files = Directory.GetFiles(path);
                                    string[] folders = Directory.GetDirectories(path);
                                    Apply(files.Concat(folders).ToArray(), dateStatuses);
                                }
                            }
                        }
                }
            }
        }
        public static void Apply(string[] paths, string dateStatusFilePath) {
            var reader = new StreamReader(dateStatusFilePath);
            DateStatus[] dateStatuses = JsonConvert.DeserializeObject<DateStatus[]>(reader.ReadToEnd());
            Apply(paths, dateStatuses);
        }
    }
    public static class Generator {
        public static DateStatus Generate(string path, bool special = false, bool recursive = false, bool lastAccess = true, bool lastWrite = true, bool creation = true) {
            if (special)
                return new DateStatus(path, Path.GetFileName(path), recursive, lastAccess, lastWrite, creation);
            else
                return new DateStatus(path, null, recursive, lastAccess, lastWrite, creation);
        }
        public static DateStatus[] Generate(string[] paths, bool recursive = false, bool lastAccess = true, bool lastWrite = true, bool creation = true) {
            var result = new List<DateStatus>();
            foreach (string path in paths)
                result.Add(Generate(path, true, recursive, lastAccess, lastWrite, creation));
            return result.ToArray();
        }
        public static void WriteFile(string path, DateStatus src) => WriteFile(path, new DateStatus[] { src });
        public static void WriteFile(string path, DateStatus[] srcs) {
            var writer = new StreamWriter(path);
            writer.Write(JsonConvert.SerializeObject(srcs, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }
    }
}