namespace Fuxion.Repositories;

public interface IKeyValueRepository<TKey, TValue>
{
	bool    Exist(TKey  key);
	TValue? Find(TKey   key);
	TValue  Get(TKey    key);
	void    Set(TKey    key, TValue value);
	void    Remove(TKey key);
}