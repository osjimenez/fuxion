using System.ComponentModel.DataAnnotations.Schema;

namespace Fuxion.Identity.Test.Dao;

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
	IDiscriminator IScope.Discriminator => Discriminator;
	public override string ToString() => this.ToOneLineString();
}