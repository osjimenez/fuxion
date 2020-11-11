using Fuxion.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Fuxion.Licensing
{
	public class JsonFileLicenseStore : ILicenseStore
	{
		public JsonFileLicenseStore(Type[]? knownLicenseTypes = null, string licensesFileName = "licenses.json", bool listenFileForChanges = true)
		{
			this.knownLicenseTypes = knownLicenseTypes ?? new Type[] { };
			var assDirPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
			if (assDirPath == null) throw new FileLoadException($"Assembly directory cannot be determined");
			var licensesFilePath = Path.Combine(assDirPath, licensesFileName);
			pathLocker = new Locker<string>(licensesFilePath);
			LoadFile(licensesFilePath);
			if (listenFileForChanges)
			{
				var assFilPath = Path.GetDirectoryName(licensesFilePath);
				if (assFilPath == null) throw new FileLoadException($"License file path cannot be determined from '{licensesFilePath}'");
				var w = new FileSystemWatcher(assFilPath, licensesFileName);
				w.Created += (s, e) => LoadFile(e.FullPath);
				w.Changed += (s, e) => LoadFile(e.FullPath);
				w.Deleted += (s, e) => LoadFile(e.FullPath);
				w.Renamed += (s, e) => LoadFile(e.FullPath);
				w.EnableRaisingEvents = true;
			}
		}

		private readonly Type[] knownLicenseTypes;
		private Locker<string> pathLocker;
		private readonly Locker<List<LicenseContainer>> listLocker = new Locker<List<LicenseContainer>>(new List<LicenseContainer>());

		public event EventHandler<EventArgs<LicenseContainer>>? LicenseAdded;
		public event EventHandler<EventArgs<LicenseContainer>>? LicenseRemoved;

		private void LoadFile(string licensesFilePath)
		{
			listLocker.Write(l => l.Clear());
			if (File.Exists(pathLocker.Read(path => path)))
				listLocker.Write(l => l.AddRange(JsonConvert.DeserializeObject<LicenseContainer[]>(
					pathLocker.Read(path => File.ReadAllText(path))
					)));
		}
		public IQueryable<LicenseContainer> Query() => listLocker.Read(licenses => licenses.AsQueryable());
		public void Add(LicenseContainer license)
		{
			Type? licenseType = null;
			foreach (var type in knownLicenseTypes)
				if (license.Is(type))
					licenseType = type;
			listLocker.Write(licenses =>
			{
				foreach (var l in (licenseType == null
					? Enumerable.Empty<LicenseContainer>()
					: licenses.AsQueryable().OfType(licenseType))
					.Except(new[] { license }).ToList())
				{
					licenses.Remove(l);
				}
				licenses.Add(license);
				pathLocker.Write(path => File.WriteAllText(path, licenses.ToJson()));
				LicenseAdded?.Invoke(this, new EventArgs<LicenseContainer>(license));
			});
		}
		public bool Remove(LicenseContainer license)
		{
			return listLocker.Write(licenses =>
			{
				var res = licenses.Remove(license);
				pathLocker.Write(path => File.WriteAllText(path, licenses.ToJson()));
				LicenseRemoved?.Invoke(this, new EventArgs<LicenseContainer>(license));
				return res;
			});
		}
	}
}