namespace Fuxion;

public abstract class StoredTimeProvider : ITimeProvider
{
	public            DateTime       Now()          => GetUtcTime().ToLocalTime();
	public            DateTimeOffset NowOffsetted() => GetUtcTime().ToLocalTime();
	public            DateTime       UtcNow()       => GetUtcTime();
	public abstract   void           SaveUtcTime(DateTime time);
	public abstract   DateTime       GetUtcTime();
	protected virtual string         Serialize(DateTime time)  => time.ToString();
	protected virtual DateTime       Deserialize(string value) => DateTime.Parse(value);
}

public class MemoryStoredTimeProvider : StoredTimeProvider
{
	DateTime                 dt = DateTime.UtcNow;
	public override DateTime GetUtcTime()               => dt;
	public override void     SaveUtcTime(DateTime time) => dt = time;
}