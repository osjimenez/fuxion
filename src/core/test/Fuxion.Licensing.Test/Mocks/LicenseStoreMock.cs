﻿namespace Fuxion.Licensing.Test.Mocks;

using System.Text.Json;

public class LicenseStoreMock : ILicenseStore
{
	//public LicenseStoreMock() => licenses = (JsonSerializer.Deserialize<LicenseContainer[]>(File.ReadAllText("licenses.json"))
	//	?? throw new InvalidOperationException("Error deserializing licenses.json")).ToList();
	public LicenseStoreMock() => licenses = (File.ReadAllText("licenses.json").FromJson<LicenseContainer[]>()
		?? throw new InvalidOperationException("Error deserializing licenses.json")).ToList();

	private readonly List<LicenseContainer> licenses;

	public event EventHandler<EventArgs<LicenseContainer>>? LicenseAdded;
	public event EventHandler<EventArgs<LicenseContainer>>? LicenseRemoved;

	public IQueryable<LicenseContainer> Query() => licenses.AsQueryable();
	public void Add(LicenseContainer license) { licenses.Add(license); LicenseAdded?.Invoke(this, new EventArgs<LicenseContainer>(license)); }
	public bool Remove(LicenseContainer license) { var res = licenses.Remove(license); LicenseRemoved?.Invoke(this, new EventArgs<LicenseContainer>(license)); return res; }
}