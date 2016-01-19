namespace Fuxion.Identity
{
    public interface IScope {
        IDiscriminator Discriminator { get; }
        ScopePropagation Propagation { get; }
    }
    public static class ScopeExtensions
    {
        public static bool IsValid(this IScope me) { return me.Discriminator != null && me.Discriminator.IsValid(); }
    }
}
