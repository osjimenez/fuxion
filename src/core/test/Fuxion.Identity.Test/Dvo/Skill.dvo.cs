using Fuxion.ComponentModel;
using System.Collections.Generic;

namespace Fuxion.Identity.Test.Dvo
{
	public interface ISkillDvo : IBaseDvo<ISkillDvo>
	{

	}
	[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
	public abstract class SkillDvo<TSkill> : BaseDvo<TSkill>, ISkillDvo
		where TSkill : SkillDvo<TSkill>
	{
		public SkillDvo(string id, string name) : base(id, name)
		{
			PropertyChanged += (s, e) =>
			{
				foreach (var i in propertyChangedList)
					i(s, e.ConvertToNotifier<ISkillDvo>(s));
			};
		}

		private readonly List<NotifierPropertyChangedEventHandler<ISkillDvo>> propertyChangedList = new List<NotifierPropertyChangedEventHandler<ISkillDvo>>();
		event NotifierPropertyChangedEventHandler<ISkillDvo> INotifier<ISkillDvo>.PropertyChanged
		{
			add
			{
				propertyChangedList.Add(value);
				//PropertyChanged += SkillDvo_PropertyChanged;
				//PropertyChanged += (s, e) => value.Invoke(s, e.ConvertToNotifier<ISkillDvo>(s));
			}

			remove
			{
				propertyChangedList.Remove(value);
			}
		}
		//private void SkillDvo_PropertyChanged(TSkill notifier, NotifierPropertyChangedEventArgs<TSkill> e)
		//{
		//    PropertyChanged(null, null);
		//    ((INotifier<ISkillDvo>)this).PropertyChanged(null, null);
		//}
	}
	[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
	public class ActorSkillDvo : SkillDvo<ActorSkillDvo>
	{
		public ActorSkillDvo(string id, string name) : base(id, name) { }
	}
	[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
	public class SingerSkillDvo : SkillDvo<SingerSkillDvo>
	{
		public SingerSkillDvo(string id, string name) : base(id, name) { }
	}
	[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
	public class WriterSkillDvo : SkillDvo<WriterSkillDvo>
	{
		public WriterSkillDvo(string id, string name) : base(id, name) { }
	}
	[TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
	public class DirectorSkillDvo : SkillDvo<DirectorSkillDvo>
	{
		public DirectorSkillDvo(string id, string name) : base(id, name) { }
	}
}
