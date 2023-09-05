using Fuxion.Identity.Test.Dao;

namespace Fuxion.Identity.Test;

public static class RolExtensions
{
	//public static bool AllLocations(this IRolCan me, params LocationDto[] locations) => me.ByAll(locations);
	//public static bool AnyLocations(this IRolCan me, params LocationDto[] locations) => me.ByAny(locations);
	//public static bool AllLocations(this IRolCan me, params LocationDao[] locations) => me.ByAll(locations);
	//public static bool AnyLocations(this IRolCan me, params LocationDao[] locations) => me.ByAny(locations);
	//public static bool AllLocations(this IRolCan me, params LocationDto[] locations) => me.AllInstances(locations);
	//public static bool AnyLocations(this IRolCan me, params LocationDto[] locations) => me.AnyInstance(locations);
	//public static bool AllLocations(this IRolCan me, params LocationDao[] locations) => me.AllInstances(locations);
	public static bool AllLocations2<TLocation>(this IRolCan me, params TLocation[] locations) where TLocation : LocationDao => me.AllInstances(locations);
	//public static bool AnyLocations(this IRolCan me, params LocationDao[] locations) => me.AnyInstance(locations);
}