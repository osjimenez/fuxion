using Fuxion.Factories;
using Fuxion.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    public class JsonFileLicenseStore : ILicenseStore
    {
        public JsonFileLicenseStore(Type[] knownLicenseTypes = null, string licensesFileName = "licenses.json", bool listenFileForChanges = true)
        {
            this.knownLicenseTypes = knownLicenseTypes ?? new Type[] { };
            var licensesFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), licensesFileName);
            LoadFile(licensesFilePath);
            if (listenFileForChanges)
            {
                var w = new FileSystemWatcher(Path.GetDirectoryName(licensesFilePath), licensesFileName);
                w.Created += (s, e) => LoadFile(e.FullPath);
                w.Changed += (s, e) => LoadFile(e.FullPath);
                w.Deleted += (s, e) => LoadFile(e.FullPath);
                w.Renamed += (s, e) => LoadFile(e.FullPath);
                w.EnableRaisingEvents = true;
            }
        }
        Type[] knownLicenseTypes;
        Locker<string> pathLocker;
        Locker<List<LicenseContainer>> listLocker= new Locker<List<LicenseContainer>>(new List<LicenseContainer>());

        public event EventHandler<EventArgs<LicenseContainer>> LicenseAdded;
        public event EventHandler<EventArgs<LicenseContainer>> LicenseRemoved;
        
        private void LoadFile(string licensesFilePath)
        {
            listLocker.Write(l => l.Clear());
            pathLocker = new Locker<string>(licensesFilePath);
            if (File.Exists(pathLocker.Read(path => path)))
                listLocker.Write(l => l.AddRange(JsonConvert.DeserializeObject<LicenseContainer[]>(
                    pathLocker.Read(path => File.ReadAllText(path))
                    )));
        }
        public IQueryable<LicenseContainer> Query() { return listLocker.Read(licenses => licenses.AsQueryable()); }
        public void Add(LicenseContainer license)
        {
            Type licenseType = null;
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