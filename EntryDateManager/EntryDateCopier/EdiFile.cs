using System;
using System.IO;
using Newtonsoft.Json;

#nullable enable
namespace EntryDateCopier {
	public class EdiFile {
		private static readonly JsonSerializerSettings SerializerSettings = new() {
			NullValueHandling = NullValueHandling.Ignore
		};

		public EdiFile(EntryDateInfo[] data, EdiConfiguration? config = null) {
			Configuration = config;
			Data = data;
		}

		public EdiConfiguration? Configuration { get; }

		public EntryDateInfo[] Data { get; }

		public static EdiFile Load(string path) => JsonConvert.DeserializeObject<EdiFile>(Utilities.Decompress(File.ReadAllBytes(path)), SerializerSettings) ?? throw new NullReferenceException();

		public void Save(string path) => File.WriteAllBytes(path, Utilities.Compress(JsonConvert.SerializeObject(this, SerializerSettings)));
	}
}