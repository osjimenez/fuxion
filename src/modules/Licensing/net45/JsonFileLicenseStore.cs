using Fuxion.Factories;
using Fuxion.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    public class JsonFileLicenseStore : ILicenseStore
    {
        public JsonFileLicenseStore(string licensesFilePath = "licenses.json", Type[] knownLicenseTypes = null)
        {
            this.knownLicenseTypes = knownLicenseTypes ?? new Type[] { };
            pathLocker = new ValueLocker<string>(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), licensesFilePath));
            if (File.Exists(pathLocker.Read(path => path)))
                listLocker = new ValueLocker<List<LicenseContainer>>(JsonConvert.DeserializeObject<LicenseContainer[]>(
                    pathLocker.Read(path => File.ReadAllText(path))
                    ).ToList());
            else
                listLocker = new ValueLocker<List<LicenseContainer>>(new List<LicenseContainer>());
        }
        Type[] knownLicenseTypes;
        ValueLocker<string> pathLocker;
        ValueLocker<List<LicenseContainer>> listLocker;

        public event EventHandler<EventArgs<LicenseContainer>> LicenseAdded;
        public event EventHandler<EventArgs<LicenseContainer>> LicenseRemoved;

        public IQueryable<LicenseContainer> Query() { return listLocker.Read(licenses => licenses.ToArray().AsQueryable()); }
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