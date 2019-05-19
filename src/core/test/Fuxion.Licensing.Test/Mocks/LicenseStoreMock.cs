using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fuxion.Licensing.Test.Mocks
{
	public class LicenseStoreMock : ILicenseStore
	{
		public LicenseStoreMock() => licenses = JsonConvert.DeserializeObject<LicenseContainer[]>(File.ReadAllText("licenses.json")).ToList();

		private readonly List<LicenseContainer> licenses;

		public event EventHandler<EventArgs<LicenseContainer>> LicenseAdded;
		public event EventHandler<EventArgs<LicenseContainer>> LicenseRemoved;

		public IQueryable<LicenseContainer> Query() => licenses.AsQueryable();
		public void Add(LicenseContainer license) { licenses.Add(license); LicenseAdded?.Invoke(this, new EventArgs<LicenseContainer>(license)); }
		public bool Remove(LicenseContainer license) { var res = licenses.Remove(license); LicenseRemoved?.Invoke(this, new EventArgs<LicenseContainer>(license)); return res; }
	}
}
