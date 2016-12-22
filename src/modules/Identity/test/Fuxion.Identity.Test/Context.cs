using Fuxion.Factories;
using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
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
                act.Item1.DynamicInvoke(act.Item2);
        }

        #region Discriminator
        public static DiscriminatorContext Discriminator { get; } = new DiscriminatorContext();
        public class DiscriminatorContext : Context<Discriminator>
        {
            public DiscriminatorContext()
            {
                //Location.Country.Usa.States = new[] { Location.State.California };
                //Location.State.California.Country = Location.Country.Usa;
            }
            #region Location
            public LocationContext Location { get; } = new LocationContext();
            public class LocationContext
            {
                #region Country
                public CountryContext Country { get; } = new CountryContext();
                public class CountryContext
                {
                    static Country Create(string name, Action<Country> configureAction = null)
                    {
                        var res = new Country { Id = name, Name = name };
                        configureAction?.Invoke(res);
                        return res;
                    }
                    public Country Usa { get; } = Create(nameof(Usa));//, c => c.States = new[] { Discriminator.Location.State.California });
                    public Country Spain { get; } = Create(nameof(Spain));
                }
                #endregion
                #region State
                public StateContext State { get; } = new StateContext();
                public class StateContext
                {
                    static State Create(string name, Action<State> configureAction = null)
                    {
                        var res = new State { Id = name, Name = name };
                        configureAction?.Invoke(res);
                        return res;
                    }
                    public State California { get; } = Create(nameof(California));//, s => s.Country = Discriminator.Location.Country.Usa);
                    public State NewYork { get; } = Create(nameof(NewYork));
                    public State Madrid { get; } = Create(nameof(Madrid));
                }
                #endregion
                #region City
                public CityContext City { get; } = new CityContext();
                public class CityContext
                {

                }
                #endregion
            }
            #endregion
            #region Category
            public CategoryContext Category { get; } = new CategoryContext();
            public class CategoryContext { }
            #endregion
            #region Tag
            public TagContext Tag { get; } = new TagContext();
            public class TagContext { }
            #endregion
        }
        #endregion
        #region Rol
        public static RolContext Rol { get; } = new RolContext();
        public class RolContext
        {
            #region Identity
            public IdentityContext Identity { get; } = new IdentityContext();
            public class IdentityContext : Context<Entity.Identity>
            {
                static Entity.Identity Create(string name, Action<Entity.Identity> configureAction = null)
                {
                    var res = new Entity.Identity { Id = name, UserName = name, Name = name };
                    byte[] salt, hash;
                    new PasswordProvider().Generate("test", out salt, out hash);
                    res.PasswordHash = hash;
                    res.PasswordSalt = salt;
                    AddConfigurationAction(configureAction, res);
                    //configureAction?.Invoke(res);
                    return res;
                }

                public Entity.Identity Root { get; } = Create(nameof(Root), ide =>
                    {
                        ide.Groups = new[] { Rol.Group.Admins };
                        ide.Permissions = new Permission[] {
                            new Permission
                            {
                                Value = true,
                                Function = ADMIN,
                                Rol = ide,
                            },
                            new Permission
                            {
                                Value = true,
                                Function = ADMIN,
                                Rol = ide,
                                Scopes = new[]
                                {
                                    new Scope {
                                        Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<Entity.Identity>(),
                                        Propagation = ScopePropagation.ToMe
                                    }
                                }
                            },
                        };
                    });
                public Entity.Identity Customer { get; } = Create(nameof(Customer));
                public Entity.Identity FilmManager { get; } = Create(nameof(FilmManager));
            }
            #endregion
            #region Group
            public GroupContext Group { get; } = new GroupContext();
            public class GroupContext : Context<Group>
            {
                static Group Create(string name, Action<Group> configureAction = null)
                {
                    var res = new Group { Id = name, Name = name };
                    configureAction?.Invoke(res);
                    return res;
                }
                public Group Admins { get; } = Create(nameof(Admins));
            }
            #endregion
        }
        #endregion
        #region Person
        public static PersonContext Person { get; } = new PersonContext();
        public class PersonContext
        {

        }
        #endregion
        #region Skill
        public static SkillContext Skill { get; } = new SkillContext();
        public class SkillContext
        {
            #region ActorSkill
            public ActorSkillContext Actor { get; } = new ActorSkillContext();
            public class ActorSkillContext { }
            #endregion
            #region SingerSkill
            public SingerSkillContext Singer { get; } = new SingerSkillContext();
            public class SingerSkillContext { }
            #endregion
            #region WriteSkill
            public WriterSkillContext Writer { get; } = new WriterSkillContext();
            public class WriterSkillContext { }
            #endregion
            #region DirectorSkill
            public DirectorSkill Director { get; } = new DirectorSkill();
            public class DirectorSkill { }
            #endregion
        }
        #endregion
        #region File
        public static FileContext File { get; } = new FileContext();
        public class FileContext
        {
            #region Document
            public DocumentContext Document { get; } = new DocumentContext();
            public class DocumentContext : Context<Document>
            {
                public override IEnumerable<Document> GetAll() => Word.GetAll().Cast<Document>().Union(Excel.GetAll()).Union(Pdf.GetAll());

                #region Word
                public WordDocumentContext Word { get; } = new WordDocumentContext();
                public class WordDocumentContext : Context<WordDocument> { }
                #endregion
                #region Excel
                public ExcelDocumentContext Excel { get; } = new ExcelDocumentContext();
                public class ExcelDocumentContext : Context<ExcelDocument> { }
                #endregion
                #region Pdf
                public PdfDocumentContext Pdf { get; } = new PdfDocumentContext();
                public class PdfDocumentContext : Context<PdfDocument> { }
                #endregion
            }
            #endregion
            #region Media
            public MediaContext Media { get; } = new MediaContext();
            public class MediaContext
            {
                #region Film
                public FilmContext Film { get; } = new FilmContext();
                public class FilmContext { }
                #endregion
                #region Song
                public SongContext Song { get; } = new SongContext();
                public class SongContext : Context<Song> { }
                #endregion
            }
            #endregion
            #region Package
            public PackageContext Package { get; } = new PackageContext();
            public class PackageContext
            {
                #region Album
                public AlbumContext Album { get; } = new AlbumContext();
                public class AlbumContext : Context<Album>
                {
                    static Album Create(string name, Action<Album> configureAction = null)
                    {
                        var res = new Album { Id = name, Name = name };
                        configureAction?.Invoke(res);
                        return res;
                    }
                    public Album Album1 { get; } = Create(nameof(Album1));
                }
                #endregion
                #region Software
                public SoftwareContext Sofware { get; } = new SoftwareContext();
                public class SoftwareContext { }
                #endregion
            }
            #endregion
        }
        #endregion
    }
}
