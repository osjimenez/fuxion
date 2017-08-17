using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fuxion.ComponentModel;

namespace Fuxion.Identity.Test.Dvo
{
    public interface ISkillDvo : IBaseDvo<ISkillDvo>
    {

    }
    [TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
    public abstract class SkillDvo<TSkill> : BaseDvo<TSkill>, ISkillDvo
        where TSkill : SkillDvo<TSkill>
    {
        public SkillDvo()
        {
            PropertyChanged += (s, e) =>
            {
                foreach (var i in propertyChangedList)
                    i(s, e.ConvertToNotifier<ISkillDvo>(s));
            };
        }
        List<NotifierPropertyChangedEventHandler<ISkillDvo>> propertyChangedList = new List<NotifierPropertyChangedEventHandler<ISkillDvo>>();
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

    }
    [TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
    public class SingerSkillDvo : SkillDvo<SingerSkillDvo>
    {

    }
    [TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
    public class WriterSkillDvo : SkillDvo<WriterSkillDvo>
    {

    }
    [TypeDiscriminated(TypeDiscriminationDisableMode.DisableHierarchy)]
    public class DirectorSkillDvo : SkillDvo<DirectorSkillDvo>
    {

    }
}
