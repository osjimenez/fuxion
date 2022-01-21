namespace Fuxion.Identity.Test.Dao;

using System.ComponentModel.DataAnnotations.Schema;

[Table(nameof(ScopeDao))]
public class ScopeDao : BaseDao, IScope
{
	public ScopeDao(string id, string name, IDiscriminator discriminator, ScopePropagation propagation) : base(id, name)
	{
		Discriminator = discriminator;
		Propagation = propagation;
	}
	public IDiscriminator Discriminator { get; set; }
	public ScopePropagation Propagation { get; set; }
	public override string ToString() => this.ToOneLineString();

	IDiscriminator IScope.Discriminator => Discriminator;
}