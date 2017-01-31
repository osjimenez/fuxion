using Fuxion.Factories;
using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Helpers;
using Fuxion.Identity.Test.Mocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Functions;
namespace Fuxion.Identity.Test
{
    public class Context<T>
    {
        public virtual IEnumerable<T> GetAll()
        {
            return GetType().GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(T))
                .Select(p => (T)p.GetValue(this));
        }
    }
    public class Context
    {
        static List<Tuple<Delegate, object>> configurationActions = new List<Tuple<Delegate, object>>();
        public static void AddConfigurationAction(Delegate action, object parameter)
        {
            if (action != null)
                configurationActions.Add(new Tuple<Delegate, object>(action, parameter));
        }
        public static void RunConfigurationActions()
        {
            foreach (var act in configurationActions)
                try
                {
                    act.Item1.DynamicInvoke(act.Item2);
                }
                catch
                {
                    Printer.WriteLine("ERROR al procesar una configuracion del contexto");
                }
        }

        #region Discriminator
        static DiscriminatorContext _Discriminator;
        public static DiscriminatorContext Discriminator
        {
            get
            {
                if (_Discriminator == null)
                {
                    _Discriminator = new DiscriminatorContext();
                    RunConfigurationActions();
                }
                return _Discriminator;
            }
        }
        public class DiscriminatorContext : Context<DiscriminatorDao>
        {
            #region Location
            public LocationContext Location { get; } = new LocationContext();
            public class LocationContext : Context<LocationDao>
            {
                #region Country
                public CountryContext Country { get; } = new CountryContext();
                public class CountryContext : Context<CountryDao>
                {
                    static CountryDao Create(string name, Action<Dao.CountryDao> configureAction = null)
                    {
                        var res = new Dao.CountryDao { Id = name, Name = name };
                        AddConfigurationAction(configureAction, res);
                        return res;
                    }
                    public CountryDao Usa { get; } = Create(nameof(Usa), me => me.States = new[] { Discriminator.Location.State.California, Discriminator.Location.State.NewYork });
                    public CountryDao Spain { get; } = Create(nameof(Spain), me => me.States = new[] { Discriminator.Location.State.Madrid });
                }
                #endregion
                #region State
                public StateContext State { get; } = new StateContext();
                public class StateContext : Context<StateDao>
                {
                    static StateDao Create(string name, Action<StateDao> configureAction = null)
                    {
                        var res = new StateDao { Id = name, Name = name };
                        AddConfigurationAction(configureAction, res);
                        return res;
                    }
                    public StateDao California { get; } = Create(nameof(California), me =>
                    {
                        me.Country = Discriminator.Location.Country.Usa;
                        me.Cities = new[] { Discriminator.Location.City.SanFrancisco, Discriminator.Location.City.Berkeley };
                    });
                    public StateDao NewYork { get; } = Create(nameof(NewYork), me =>
                    {
                        me.Country = Discriminator.Location.Country.Usa;
                        me.Cities = new[] { Discriminator.Location.City.NewYork, Discriminator.Location.City.Buffalo };
                    });
                    public StateDao Madrid { get; } = Create(nameof(Madrid), me =>
                    {
                        me.Country = Discriminator.Location.Country.Spain;
                        me.Cities = new[] { Discriminator.Location.City.Madrid, Discriminator.Location.City.Alcorcon };
                    });
                }
                #endregion
                #region City
                public CityContext City { get; } = new CityContext();
                public class CityContext : Context<CityDao>
                {
                    static CityDao Create(string name, Action<CityDao> configureAction = null)
                    {
                        var res = new CityDao { Id = name, Name = name };
                        AddConfigurationAction(configureAction, res);
                        return res;
                    }
                    public CityDao SanFrancisco { get; } = Create(nameof(SanFrancisco), me => me.State = Discriminator.Location.State.California);
                    public CityDao Berkeley { get; } = Create(nameof(Berkeley), me => me.State = Discriminator.Location.State.California);
                    public CityDao NewYork { get; } = Create(nameof(NewYork), me => me.State = Discriminator.Location.State.NewYork);
                    public CityDao Buffalo { get; } = Create(nameof(Buffalo), me => me.State = Discriminator.Location.State.NewYork);
                    public CityDao Madrid { get; } = Create(nameof(Madrid), me => me.State = Discriminator.Location.State.Madrid);
                    public CityDao Alcorcon { get; } = Create(nameof(Alcorcon), me => me.State = Discriminator.Location.State.Madrid);
                }
                #endregion
            }
            #endregion
            #region Category
            public CategoryContext Category { get; } = new CategoryContext();
            public class CategoryContext : Context<CategoryDao> {
                static CategoryDao Create(string name, Action<CategoryDao> configureAction = null)
                {
                    var res = new CategoryDao { Id = name, Name = name };
                    AddConfigurationAction(configureAction, res);
                    return res;
                }
                public CategoryDao Sales { get; set; } = Create(nameof(Sales));
                public CategoryDao Purchases { get; set; } = Create(nameof(Purchases));
            }
            #endregion
            #region Tag
            public TagContext Tag { get; } = new TagContext();
            public class TagContext : Context<TagDao> {
                static TagDao Create(string name, Action<TagDao> configureAction = null)
                {
                    var res = new TagDao { Id = name, Name = name };
                    AddConfigurationAction(configureAction, res);
                    return res;
                }
                public TagDao Urgent { get; set; } = Create(nameof(Urgent));
                public TagDao Important { get; set; } = Create(nameof(Important));
            }
            #endregion
        }
        #endregion
        #region Rol
        public static RolContext Rol { get; } = new RolContext();
        public class RolContext : Context<RolDao>
        {
            #region Identity
            public IdentityContext Identity { get; } = new IdentityContext();
            public class IdentityContext : Context<IdentityDao>
            {
                static IdentityDao Create(string name, Action<Dao.IdentityDao> configureAction = null)
                {
                    var res = new IdentityDao
                    {
                        Id = name,
                        UserName = name,
                        Name = name
                    };
                    byte[] salt, hash;
                    new PasswordProviderMock().Generate("test", out salt, out hash);
                    res.PasswordHash = hash;
                    res.PasswordSalt = salt;
                    AddConfigurationAction(configureAction, res);
                    //configureAction?.Invoke(res);
                    return res;
                }

                public IdentityDao Root { get; } = Create(nameof(Root), ide =>
                    {
                        ide.Groups = new[] { Rol.Group.Admins };
                        ide.Permissions = new PermissionDao[] {
                            new PermissionDao
                            {
                                Value = true,
                                Function = ADMIN,
                                Rol = ide,
                            },
                        };
                    });
                public Dao.IdentityDao Customer { get; } = Create(nameof(Customer));
                public Dao.IdentityDao FilmManager { get; } = Create(nameof(FilmManager));
            }
            #endregion
            #region Group
            public GroupContext Group { get; } = new GroupContext();
            public class GroupContext : Context<GroupDao>
            {
                static GroupDao Create(string name, Action<GroupDao> configureAction = null)
                {
                    var res = new GroupDao { Id = name, Name = name };
                    configureAction?.Invoke(res);
                    return res;
                }
                public GroupDao Admins { get; } = Create(nameof(Admins));
            }
            #endregion
        }
        #endregion
        #region Person
        public static PersonContext Person { get; } = new PersonContext();
        public class PersonContext : Context<PersonDao>
        {

        }
        #endregion
        #region Skill
        public static SkillContext Skill { get; } = new SkillContext();
        public class SkillContext : Context<SkillDao>
        {
            #region ActorSkill
            public ActorSkillContext Actor { get; } = new ActorSkillContext();
            public class ActorSkillContext : Context<ActorSkillDao> { }
            #endregion
            #region SingerSkill
            public SingerSkillContext Singer { get; } = new SingerSkillContext();
            public class SingerSkillContext : Context<SingerSkillDao> { }
            #endregion
            #region WriteSkill
            public WriterSkillContext Writer { get; } = new WriterSkillContext();
            public class WriterSkillContext : Context<WriterSkillDao> { }
            #endregion
            #region DirectorSkill
            public DirectorSkill Director { get; } = new DirectorSkill();
            public class DirectorSkill : Context<DirectorSkillDao> { }
            #endregion
        }
        #endregion
        #region File
        public static FileContext File { get; } = new FileContext();
        public class FileContext : Context<FileDao>
        {
            #region Document
            public DocumentContext Document { get; } = new DocumentContext();
            public class DocumentContext : Context<DocumentDao>
            {
                public override IEnumerable<DocumentDao> GetAll() => Word.GetAll().Cast<DocumentDao>().Union(Excel.GetAll()).Union(Pdf.GetAll());

                #region Word
                public WordDocumentContext Word { get; } = new WordDocumentContext();
                public class WordDocumentContext : Context<WordDocumentDao> {
                    static WordDocumentDao Create(string name, Action<WordDocumentDao> configureAction = null)
                    {
                        var res = new WordDocumentDao { Id = name, Name = name };
                        AddConfigurationAction(configureAction, res);
                        return res;
                    }
                    public WordDocumentDao Word1 { get; } = Create(nameof(Word1));
                }
                #endregion
                #region Excel
                public ExcelDocumentContext Excel { get; } = new ExcelDocumentContext();
                public class ExcelDocumentContext : Context<ExcelDocumentDao> { }
                #endregion
                #region Pdf
                public PdfDocumentContext Pdf { get; } = new PdfDocumentContext();
                public class PdfDocumentContext : Context<PdfDocumentDao> { }
                #endregion
            }
            #endregion
            #region Media
            public MediaContext Media { get; } = new MediaContext();
            public class MediaContext : Context<MediaDao>
            {
                #region Film
                public FilmContext Film { get; } = new FilmContext();
                public class FilmContext : Context<FilmDao> { }
                #endregion
                #region Song
                public SongContext Song { get; } = new SongContext();
                public class SongContext : Context<SongDao> { }
                #endregion
            }
            #endregion
            #region Package
            public PackageContext Package { get; } = new PackageContext();
            public class PackageContext : Context<PackageDao>
            {
                #region Album
                public AlbumContext Album { get; } = new AlbumContext();
                public class AlbumContext : Context<AlbumDao>
                {
                    static AlbumDao Create(string name, Action<AlbumDao> configureAction = null)
                    {
                        var res = new AlbumDao { Id = name, Name = name };
                        configureAction?.Invoke(res);
                        return res;
                    }
                    public AlbumDao Album1 { get; } = Create(nameof(Album1));
                }
                #endregion
                #region Software
                public SoftwareContext Sofware { get; } = new SoftwareContext();
                public class SoftwareContext : Context<SoftwareDao> { }
                #endregion
            }
            #endregion
        }
        #endregion
    }
}
