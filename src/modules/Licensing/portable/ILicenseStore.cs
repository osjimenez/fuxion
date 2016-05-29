using System;
using System.Collections.Generic;
using System.Linq;

namespace Fuxion.Licensing
{
    public interface ILicenseStore
    {
        event EventHandler<EventArgs<LicenseContainer>> LicenseAdded;
        event EventHandler<EventArgs<LicenseContainer>> LicenseRemoved;
        IQueryable<LicenseContainer> Query();
        void Add(LicenseContainer license);
        bool Remove(LicenseContainer license);
    }
    public static class ILicenseStoreExtensions
    {
        public static IQueryable<LicenseContainer> WithValidSignature(this IQueryable<LicenseContainer> me, byte[] publicKey)
        {
            return me.Where(l => l.VerifySignature(publicKey));
        }
        public static IQueryable<LicenseContainer> WithValidSignature(this IQueryable<LicenseContainer> me, string publicKey) {
            return me.Where(l => l.VerifySignature(publicKey));
        }
        public static IQueryable<LicenseContainer> OfType<TLicense>(this IQueryable<LicenseContainer> me) where TLicense : License
        {
            return me.Where(l => l.LicenseIs<TLicense>());
        }
        public static IQueryable<LicenseContainer> OnlyValidOfType<TLicense>(this IQueryable<LicenseContainer> me, byte[] publicKey) where TLicense : License
        {
            string _;
            return me.WithValidSignature(publicKey).OfType<TLicense>().Where(l => l.LicenseAs<TLicense>().Validate(out _));
        }
        public static IQueryable<LicenseContainer> OnlyValidOfType<TLicense>(this IQueryable<LicenseContainer> me, string publicKey) where TLicense : License
        {
            string _;
            return me.WithValidSignature(publicKey).OfType<TLicense>().Where(l => l.LicenseAs<TLicense>().Validate(out _));
        }
    }
}