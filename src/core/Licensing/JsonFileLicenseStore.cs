using System.Reflection;
using Fuxion.Threading;

namespace Fuxion.Licensing;

public class JsonFileLicenseStore : ILicenseStore
{
	public JsonFileLicenseStore(Type[]? knownLicenseTypes = null, string licensesFileName = "licenses.json", bool listenFileForChanges = true)
	{
		this.knownLicenseTypes = knownLicenseTypes ?? new Type[]
			{ };
		var assDirPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
		if (assDirPath == null) throw new FileLoadException("Assembly directory cannot be determined");
		var licensesFilePath = Path.Combine(assDirPath, licensesFileName);
		pathLocker = new(licensesFilePath);
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
	readonly Type[] knownLicenseTypes;
	readonly Locker<List<LicenseContainer>> listLocker = new(new());
	readonly Locker<string> pathLocker;
	public event EventHandler<EventArgs<LicenseContainer>>? LicenseAdded;
	public event EventHandler<EventArgs<LicenseContainer>>? LicenseRemoved;
	public IQueryable<LicenseContainer> Query() => listLocker.Read(licenses => licenses.AsQueryable());
	public void Add(LicenseContainer license)
	{
		Type? licenseType = null;
		foreach (var type in knownLicenseTypes)
			if (license.Is(type))
				licenseType = type;
		listLocker.Write(licenses => {
			foreach (var l in (licenseType == null ? Enumerable.Empty<LicenseContainer>() : licenses.AsQueryable().OfType(licenseType)).Except(new[] {
					license
				}).ToList())
				licenses.Remove(l);
			licenses.Add(license);
			pathLocker.Write(path => File.WriteAllText(path, licenses.ToJson()));
			LicenseAdded?.Invoke(this, new(license));
		});
	}
	public bool Remove(LicenseContainer license) =>
		listLocker.Write(licenses => {
			var res = licenses.Remove(license);
			pathLocker.Write(path => File.WriteAllText(path, licenses.ToJson()));
			LicenseRemoved?.Invoke(this, new(license));
			return res;
		});
	void LoadFile(string licensesFilePath)
	{
		listLocker.Write(l => l.Clear());
		if (File.Exists(pathLocker.Read(path => path)))
			listLocker.Write(
				l => l.AddRange(pathLocker.Read(path => File.ReadAllText(path)).FromJson<LicenseContainer[]>() ?? throw new InvalidOperationException("Error deserializing LicenseContainer")));

		//listLocker.Write(l => l.AddRange(JsonConvert.DeserializeObject<LicenseContainer[]>(
		//		pathLocker.Read(path => File.ReadAllText(path))
		//		) ?? throw new InvalidOperationException("Error deserializing LicenseContainer")));
	}
}