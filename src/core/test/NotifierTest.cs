using System.ComponentModel;
using System.Diagnostics;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fuxion.ComponentModel;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using Fuxion.Threading.Tasks;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Fuxion.Threading;
using Xunit;

namespace Fuxion.Test
{
    public class ReferenceObjectMock
    {
        public int Entero { get; set; }
        public string Cadena { get; set; }
        public override bool Equals(object obj)
        {
            if (!(obj is ReferenceObjectMock)) return false;
            var c = obj as ReferenceObjectMock;
            return c.Entero == Entero && c.Cadena == Cadena;
        }
        public override int GetHashCode() { return Entero.GetHashCode() + Cadena.GetHashCode(); }
        public static bool operator ==(ReferenceObjectMock a, ReferenceObjectMock b) { return EqualityComparer<ReferenceObjectMock>.Default.Equals(a, b); }
        public static bool operator !=(ReferenceObjectMock a, ReferenceObjectMock b) { return !EqualityComparer<ReferenceObjectMock>.Default.Equals(a, b); }
        public static ReferenceObjectMock DefaultValue { get { return new ReferenceObjectMock { Cadena = "aaa", Entero = 111 }; } }
    }
    public class NotifierMock : Notifier<NotifierMock>
    {
        #region Protected
        protected int Integer_Protected_AutoImplemented_WithoutDefault_RaiseAlways_NotLocked
        {
            get { return GetValue<int>(); }
            set { SetValue(value, false); }
        }
        public void ChangeProtected(int value) { Integer_Protected_AutoImplemented_WithoutDefault_RaiseAlways_NotLocked = value; }
        #endregion
        #region Integer_AutoImplemented
        public int Integer_AutoImplemented_WithoutDefault_RaiseAlways_Locked
        {
            get { return GetLockedValue<int>(); }
            set { SetLockedValue(value, false); }
        }
        public int Integer_AutoImplemented_WithoutDefault_RaiseAlways_NotLocked
        {
            get { return GetValue<int>(); }
            set { SetValue(value, false); }
        }
        public int Integer_AutoImplemented_WithoutDefault_RaiseOnChange_Locked
        {
            get { return GetLockedValue<int>(); }
            set { SetLockedValue(value); }
        }
        public int Integer_AutoImplemented_WithoutDefault_RaiseOnChange_NotLocked
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }
        public int Integer_AutoImplemented_WithDefault_RaiseAlways_Locked
        {
            get { return GetLockedValue(() => 111); }
            set { SetLockedValue(value, false); }
        }
        public int Integer_AutoImplemented_WithDefault_RaiseAlways_NotLocked
        {
            get { return GetValue(() => 111); }
            set { SetValue(value, false); }
        }
        public int Integer_AutoImplemented_WithDefault_RaiseOnChange_Locked
        {
            get { return GetLockedValue(() => 111); }
            set { SetLockedValue(value); }
        }
        public int Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked
        {
            get { return GetValue(() => 111); }
            set { SetValue(value); }
        }
        #endregion
        #region Integer_FieldImplemented
        private ValueLocker<int> _Integer_FieldImplemented_WithoutDefault_RaiseAlways_Locked;
        public int Integer_FieldImplemented_WithoutDefault_RaiseAlways_Locked
        {
            get { return GetLockedField(ref _Integer_FieldImplemented_WithoutDefault_RaiseAlways_Locked); }
            set { SetLockedField(ref _Integer_FieldImplemented_WithoutDefault_RaiseAlways_Locked, value, false); }
        }
        private int _Integer_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked;
        public int Integer_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked
        {
            get { return GetField(ref _Integer_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked); }
            set { SetField(ref _Integer_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked, value, false); }
        }
        private ValueLocker<int> _Integer_FieldImplemented_WithoutDefault_RaiseOnChange_Locked;
        public int Integer_FieldImplemented_WithoutDefault_RaiseOnChange_Locked
        {
            get { return GetLockedField(ref _Integer_FieldImplemented_WithoutDefault_RaiseOnChange_Locked); }
            set { SetLockedField(ref _Integer_FieldImplemented_WithoutDefault_RaiseOnChange_Locked, value); }
        }
        private int _Integer_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked;
        public int Integer_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked
        {
            get { return GetField(ref _Integer_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked); }
            set { SetField(ref _Integer_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked, value); }
        }
        private ValueLocker<int> _Integer_FieldImplemented_WithDefault_RaiseAlways_Locked = new ValueLocker<int>(111);
        public int Integer_FieldImplemented_WithDefault_RaiseAlways_Locked
        {
            get { return GetLockedField(ref _Integer_FieldImplemented_WithDefault_RaiseAlways_Locked); }
            set { SetLockedField(ref _Integer_FieldImplemented_WithDefault_RaiseAlways_Locked, value, false); }
        }
        private int _Integer_FieldImplemented_WithDefault_RaiseAlways_NotLocked = 111;
        public int Integer_FieldImplemented_WithDefault_RaiseAlways_NotLocked
        {
            get { return GetField(ref _Integer_FieldImplemented_WithDefault_RaiseAlways_NotLocked); }
            set { SetField(ref _Integer_FieldImplemented_WithDefault_RaiseAlways_NotLocked, value, false); }
        }
        private ValueLocker<int> _Integer_FieldImplemented_WithDefault_RaiseOnChange_Locked = new ValueLocker<int>(111);
        public int Integer_FieldImplemented_WithDefault_RaiseOnChange_Locked
        {
            get { return GetLockedField(ref _Integer_FieldImplemented_WithDefault_RaiseOnChange_Locked); }
            set { SetLockedField(ref _Integer_FieldImplemented_WithDefault_RaiseOnChange_Locked, value); }
        }
        private int _Integer_FieldImplemented_WithDefault_RaiseOnChange_NotLocked = 111;
        public int Integer_FieldImplemented_WithDefault_RaiseOnChange_NotLocked
        {
            get { return GetField(ref _Integer_FieldImplemented_WithDefault_RaiseOnChange_NotLocked); }
            set { SetField(ref _Integer_FieldImplemented_WithDefault_RaiseOnChange_NotLocked, value); }
        }
        #endregion
        #region String_AutoImplemented
        public string String_AutoImplemented_WithoutDefault_RaiseAlways_Locked
        {
            get { return GetLockedValue(); }
            set { SetLockedValue(value, false); }
        }
        public string String_AutoImplemented_WithoutDefault_RaiseAlways_NotLocked
        {
            get { return GetValue<string>(); }
            set { SetValue(value, false); }
        }
        public string String_AutoImplemented_WithoutDefault_RaiseOnChange_Locked
        {
            get { return GetLockedValue(); }
            set { SetLockedValue(value); }
        }
        public string String_AutoImplemented_WithoutDefault_RaiseOnChange_NotLocked
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string String_AutoImplemented_WithDefault_RaiseAlways_Locked
        {
            get { return GetLockedValue(() => "aaa"); }
            set { SetLockedValue(value, false); }
        }
        public string String_AutoImplemented_WithDefault_RaiseAlways_NotLocked
        {
            get { return GetValue(() => "aaa"); }
            set { SetValue(value, false); }
        }
        public string String_AutoImplemented_WithDefault_RaiseOnChange_Locked
        {
            get { return GetLockedValue(() => "aaa"); }
            set { SetLockedValue(value); }
        }
        public string String_AutoImplemented_WithDefault_RaiseOnChange_NotLocked
        {
            get { return GetValue(() => "aaa"); }
            set { SetValue(value); }
        }
        #endregion
        #region String_FieldImplemented
        private ValueLocker<string> _String_FieldImplemented_WithoutDefault_RaiseAlways_Locked;
        public string String_FieldImplemented_WithoutDefault_RaiseAlways_Locked
        {
            get { return GetLockedField(ref _String_FieldImplemented_WithoutDefault_RaiseAlways_Locked); }
            set { SetLockedField(ref _String_FieldImplemented_WithoutDefault_RaiseAlways_Locked, value, false); }
        }
        private string _String_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked;
        public string String_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked
        {
            get { return GetField(ref _String_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked); }
            set { SetField(ref _String_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked, value, false); }
        }
        private ValueLocker<string> _String_FieldImplemented_WithoutDefault_RaiseOnChange_Locked;
        public string String_FieldImplemented_WithoutDefault_RaiseOnChange_Locked
        {
            get { return GetLockedField(ref _String_FieldImplemented_WithoutDefault_RaiseOnChange_Locked); }
            set { SetLockedField(ref _String_FieldImplemented_WithoutDefault_RaiseOnChange_Locked, value); }
        }
        private string _String_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked;
        public string String_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked
        {
            get { return GetField(ref _String_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked); }
            set { SetField(ref _String_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked, value); }
        }
        private ValueLocker<string> _String_FieldImplemented_WithDefault_RaiseAlways_Locked = new ValueLocker<string>("aaa");
        public string String_FieldImplemented_WithDefault_RaiseAlways_Locked
        {
            get { return GetLockedField(ref _String_FieldImplemented_WithDefault_RaiseAlways_Locked); }
            set { SetLockedField(ref _String_FieldImplemented_WithDefault_RaiseAlways_Locked, value, false); }
        }
        private string _String_FieldImplemented_WithDefault_RaiseAlways_NotLocked = "aaa";
        public string String_FieldImplemented_WithDefault_RaiseAlways_NotLocked
        {
            get { return GetField(ref _String_FieldImplemented_WithDefault_RaiseAlways_NotLocked); }
            set { SetField(ref _String_FieldImplemented_WithDefault_RaiseAlways_NotLocked, value, false); }
        }
        private ValueLocker<string> _String_FieldImplemented_WithDefault_RaiseOnChange_Locked = new ValueLocker<string>("aaa");
        public string String_FieldImplemented_WithDefault_RaiseOnChange_Locked
        {
            get { return GetLockedField(ref _String_FieldImplemented_WithDefault_RaiseOnChange_Locked); }
            set { SetLockedField(ref _String_FieldImplemented_WithDefault_RaiseOnChange_Locked, value); }
        }
        private string _String_FieldImplemented_WithDefault_RaiseOnChange_NotLocked = "aaa";
        public string String_FieldImplemented_WithDefault_RaiseOnChange_NotLocked
        {
            get { return GetField(ref _String_FieldImplemented_WithDefault_RaiseOnChange_NotLocked); }
            set { SetField(ref _String_FieldImplemented_WithDefault_RaiseOnChange_NotLocked, value); }
        }
        #endregion
        #region ReferenceObject_AutoImplemented
        public ReferenceObjectMock ReferenceObject_AutoImplemented_WithoutDefault_RaiseAlways_NotLocked
        {
            get { return GetValue<ReferenceObjectMock>(); }
            set { SetValue(value, false); }
        }
        public ReferenceObjectMock ReferenceObject_AutoImplemented_WithoutDefault_RaiseOnChange_NotLocked
        {
            get { return GetValue<ReferenceObjectMock>(); }
            set { SetValue(value); }
        }
        public ReferenceObjectMock ReferenceObject_AutoImplemented_WithDefault_RaiseAlways_NotLocked
        {
            get { return GetValue(() => ReferenceObjectMock.DefaultValue); }
            set { SetValue(value, false); }
        }
        public ReferenceObjectMock ReferenceObject_AutoImplemented_WithDefault_RaiseOnChange_NotLocked
        {
            get { return GetValue(() => ReferenceObjectMock.DefaultValue); }
            set { SetValue(value); }
        }
        #endregion
        #region ReferenceObject_FieldImplemented
        private ReferenceObjectMock _ReferenceObject_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked;
        public ReferenceObjectMock ReferenceObject_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked
        {
            get { return GetField(ref _ReferenceObject_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked); }
            set { SetField(ref _ReferenceObject_FieldImplemented_WithoutDefault_RaiseAlways_NotLocked, value, false); }
        }
        private ReferenceObjectMock _ReferenceObject_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked;
        public ReferenceObjectMock ReferenceObject_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked
        {
            get { return GetField(ref _ReferenceObject_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked); }
            set { SetField(ref _ReferenceObject_FieldImplemented_WithoutDefault_RaiseOnChange_NotLocked, value); }
        }
        private ReferenceObjectMock _ReferenceObject_FieldImplemented_WithDefault_RaiseAlways_NotLocked = ReferenceObjectMock.DefaultValue;
        public ReferenceObjectMock ReferenceObject_FieldImplemented_WithDefault_RaiseAlways_NotLocked
        {
            get { return GetField(ref _ReferenceObject_FieldImplemented_WithDefault_RaiseAlways_NotLocked); }
            set { SetField(ref _ReferenceObject_FieldImplemented_WithDefault_RaiseAlways_NotLocked, value, false); }
        }
        private ReferenceObjectMock _ReferenceObject_FieldImplemented_WithDefault_RaiseOnChange_NotLocked = ReferenceObjectMock.DefaultValue;
        public ReferenceObjectMock ReferenceObject_FieldImplemented_WithDefault_RaiseOnChange_NotLocked
        {
            get { return GetField(ref _ReferenceObject_FieldImplemented_WithDefault_RaiseOnChange_NotLocked); }
            set { SetField(ref _ReferenceObject_FieldImplemented_WithDefault_RaiseOnChange_NotLocked, value); }
        }
        #endregion
    }
    public class DerivedNotifierMock : NotifierMock { }
    public class Notifier_Test
    {
        [Fact]
        public void ProtectedPropertyTest()
        {
            var o = new DerivedNotifierMock();
            o.ChangeProtected(1);
        }
        #region Variables
        //private static TestContext context;
        private NotifierMock mock;
        private const string PROPERTY_CHANGED = "PropertyChanged";
        private const string PROPERTY_CHANGED_BASE = "PropertyChangedBase";
        //private const string PROPERTY_CHANGING = "PropertyChanging";
        //private const string PROPERTY_CHANGING_BASE = "PropertyChangingBase";
        //private const string PROPERTY_READ = "PropertyRead";
        private Dictionary<string, int> counters = new Dictionary<string, int>();
        #endregion
        #region Inicializacion
        //[ClassInitialize]
        //public static void ClassInitialize(TestContext testContext) { context = testContext; }
        public Notifier_Test() {
            TestInitialize();
        }
        //[TestInitialize]
        public void TestInitialize()
        {
            //Eliminar todos los contadores
            counters.Clear();
            //Crear todos los contadores
            foreach (var pro in typeof(NotifierMock).GetProperties())
            {
                counters.Add(pro.Name + "_" + PROPERTY_CHANGED, 0);
                counters.Add(pro.Name + "_" + PROPERTY_CHANGED_BASE, 0);
                //counters.Add(pro.Name + "_" + PROPERTY_CHANGING, 0);
                //counters.Add(pro.Name + "_" + PROPERTY_CHANGING_BASE, 0);
                //counters.Add(pro.Name + "_" + PROPERTY_READ, 0);
            }
            //Crear el notificador
            mock = new NotifierMock();
            //Registrar los eventos del notificador para incrementar los contadores
            //mock.PropertyChanging += (n, e) => IncrementCounter(e.PropertyName, PROPERTY_CHANGING);
            //(mock as INotifyPropertyChanging).PropertyChanging += (s, e) => IncrementCounter(e.PropertyName, PROPERTY_CHANGING_BASE);
            mock.PropertyChanged += (n, e) => IncrementCounter(e.PropertyName, PROPERTY_CHANGED);
            (mock as INotifyPropertyChanged).PropertyChanged += (s, e) => IncrementCounter(e.PropertyName, PROPERTY_CHANGED_BASE);
            //mock.PropertyRead += (n, e) => IncrementCounter(e.PropertyName, PROPERTY_READ);
        }
        private int GetCounter<T>(Expression<Func<T>> expression, params string[] counterNames)
        {
            var counter = -1;
            foreach (var actual in counterNames.Select(name => counters[expression.GetMemberName() + "_" + name]))
            {
                if (counter != -1 && counter != actual)
                    throw new ArgumentException("Los contadores tienen valores diferentes");
                counter = actual;
            }
            return counter;
        }
        private void CheckCounter<T>(Expression<Func<T>> expression, int value, params string[] counterNames) { CheckCounter(expression.GetMemberName(), value, counterNames); }
        private void CheckCounter(string propertyName, int value, params string[] counterNames)
        {
            foreach (var name in counterNames)
            {
                var counterValue = counters[propertyName + "_" + name];
                Assert.Equal(counterValue, value);
                //if (counterValue != value)
                //    throw new AssertFailedException("El valor del contador '" + propertyName + "_" + name + "' no es '" + value + "' sino '" + counterValue + "'.");
            }
        }
        private void IncrementCounter(string propertyName, string counterName)
        {
            counters[propertyName + "_" + counterName] = counters[propertyName + "_" + counterName] + 1;
        }
        #endregion
        #region PropertyReadRaise
        //private void PropertyReadRaise<T>(params PropertyInfo[] properties)
        //{
        //    foreach (var pro in properties)
        //    {
        //        var value = (T)pro.GetValue(mock, null);
        //        CheckCounter(pro.Name, 1, PROPERTY_READ);
        //        TestInitialize();
        //    }
        //    context.WriteLine("Se han comprobado " + properties.Count() + " propiedades de tipo '" + typeof(T).Name + "'");
        //}
        //[TestMethod]
        //public void PropertyReadRaise()
        //{
        //    var props = mock.GetType().GetProperties().AsQueryable();//.Where(p=>!p.Name.Contains("WithoutDefault"));
        //    PropertyReadRaise<int>(props.Where(p => p.PropertyType == typeof(int)).ToArray());
        //    PropertyReadRaise<string>(props.Where(p => p.PropertyType == typeof(string)).ToArray());
        //    PropertyReadRaise<ReferenceObjectMock>(props.Where(p => p.PropertyType == typeof(ReferenceObjectMock)).ToArray());
        //}
        #endregion
        #region PreviousValueIsDefaultValue
        /// <summary>
        /// Compruebo que cuando seteamos una propiedad por primera vez lo que obtenemos como PreviousValue es el valor por defecto que le hemos especificado en el Get
        /// </summary>
        private void PreviousValueIsDefaultValue<T>(T change, T defaultValue, params PropertyInfo[] properties)
        {
            foreach (var pro in properties)
            {
                var pro1 = pro;
                mock.PropertyChanged += (s, e) => e.Case<T>(pro1.Name, a => Assert.Equal(a.PreviousValue, defaultValue));
                var pro2 = pro;
                //mock.PropertyChanging += (s, e) => e.Case<T>(pro2.Name, a => Assert.AreEqual(a.ActualValue, defaultValue));
                pro.SetValue(mock, change, null);
                CheckCounter(pro.Name, 1, PROPERTY_CHANGED, PROPERTY_CHANGED_BASE);//, PROPERTY_CHANGING, PROPERTY_CHANGING_BASE, PROPERTY_READ);
                TestInitialize();
            }
            //context.WriteLine("Se han comprobado " + properties.Count() + " propiedades de tipo '" + typeof(T).Name + "'");
        }
        [Fact]
        public void PreviousValueIsDefaultValue2()
        {
            var props = mock.GetType().GetProperties().AsQueryable().Where(p => p.Name.Contains("WithDefault"));
            PreviousValueIsDefaultValue(222, 111, props.Where(p => p.PropertyType == typeof(int)).ToArray());
            PreviousValueIsDefaultValue("bbb", "aaa", props.Where(p => p.PropertyType == typeof(string)).ToArray());
            PreviousValueIsDefaultValue(
                new ReferenceObjectMock { Cadena = "bbb", Entero = 222 },
                ReferenceObjectMock.DefaultValue,
                props.Where(p => p.PropertyType == typeof(ReferenceObjectMock)).ToArray());
        }
        #endregion
        #region CancelChange
        /// <summary>
        /// Compruebo la cancelación de un cambio en el evento PropertyChanging
        /// </summary>
        //private void CancelChange<T>(T change1, T change2, params PropertyInfo[] properties)
        //{
        //    foreach (var pro in properties)
        //    {
        //        //Seteo la propiedad
        //        pro.SetValue(mock, change1, null);
        //        //Verifico que se ha llevado a cabo el cambio
        //        CheckCounter(pro.Name, 1, PROPERTY_CHANGED, PROPERTY_CHANGED_BASE);//, PROPERTY_CHANGING, PROPERTY_CHANGING_BASE);
        //        //Me suscribo al evento PropertyChanging y cancelo los cambios que se hagan a partir de ahora
        //        //mock.PropertyChanging += (s, e) => { e.CancelChange = true; };
        //        //Seteo la propiedad
        //        pro.SetValue(mock, change2, null);
        //        //Compruebo que PropertyChanging se ha llamado 2 veces
        //        //CheckCounter(pro.Name, 2,PROPERTY_CHANGING, PROPERTY_CHANGING_BASE);
        //        //Compruebo que PropertyChanged se ha llamado 1 vez
        //        CheckCounter(pro.Name, 1, PROPERTY_CHANGED, PROPERTY_CHANGED_BASE);
        //        //Compruebo que el valor de la propiedad no ha cambiado
        //        Assert.AreEqual((T)pro.GetValue(mock, null), change1);
        //        TestInitialize();
        //    }
        //    context.WriteLine("Se han comprobado " + properties.Count() + " propiedades de tipo '" + typeof(T).Name + "'");
        //}
        //[TestMethod]
        //public void CancelChange()
        //{
        //    var props = mock.GetType().GetProperties().AsQueryable();
        //    CancelChange(222, 333, props.Where(p => p.PropertyType == typeof(int)).ToArray());
        //    CancelChange("bbb", "ccc", props.Where(p => p.PropertyType == typeof(string)).ToArray());
        //    CancelChange(
        //        new ReferenceObjectMock { Cadena = "bbb", Entero = 222 },
        //        new ReferenceObjectMock { Cadena = "ccc", Entero = 333 },
        //        props.Where(p => p.PropertyType == typeof(ReferenceObjectMock)).ToArray());
        //}
        #endregion
        #region CalculateValueOnRead
        /// <summary>
        /// Calculo el valor de la propiedad justo en el momento en el que es leida
        /// </summary>
        //private void CalculateValueOnRead<T>(T readCalculatedValue, params PropertyInfo[] properties)
        //{
        //    foreach (var pro in properties)
        //    {
        //        var pro1 = pro;
        //        mock.PropertyRead += (s, e) => e.Case<T>(pro1.Name, a => readCalculatedValue);
        //        Assert.AreEqual(pro.GetValue(mock, null), readCalculatedValue);
        //    }
        //    context.WriteLine("Se han comprobado " + properties.Count() + " propiedades de tipo '" + typeof(T).Name + "'");
        //}
        //[TestMethod]
        //public void CalculateValueOnRead()
        //{
        //    var props = mock.GetType().GetProperties().AsQueryable();
        //    CalculateValueOnRead(999, props.Where(p => p.PropertyType == typeof(int)).ToArray());
        //    CalculateValueOnRead("zzz", props.Where(p => p.PropertyType == typeof(string)).ToArray());
        //    CalculateValueOnRead(
        //        new ReferenceObjectMock { Cadena = "zzz", Entero = 999 },
        //        props.Where(p => p.PropertyType == typeof(ReferenceObjectMock)).ToArray());
        //}
        #endregion
        #region RaiseOnChangeOrAlways
        /// <summary>
        /// Compruebo el comportamiento de la función de propagar el evento siempre o solo cuando haya cambio de valor
        /// </summary>
        private void RaiseOnChangeOrAlways<T>(T change, params PropertyInfo[] properties)
        {
            foreach (var pro in properties)
            {
                var proname = pro.Name;
                pro.SetValue(mock, change, null);
                CheckCounter(pro.Name, 1, PROPERTY_CHANGED, PROPERTY_CHANGED_BASE);//, PROPERTY_CHANGING, PROPERTY_CHANGING_BASE, PROPERTY_READ);
                pro.SetValue(mock, change, null);
                if (pro.Name.Contains("RaiseAlways"))
                    CheckCounter(pro.Name, 2, PROPERTY_CHANGED, PROPERTY_CHANGED_BASE);//, PROPERTY_CHANGING, PROPERTY_CHANGING_BASE, PROPERTY_READ);
                else
                {
                    CheckCounter(pro.Name, 1, PROPERTY_CHANGED, PROPERTY_CHANGED_BASE);//, PROPERTY_CHANGING, PROPERTY_CHANGING_BASE);
                    //CheckCounter(pro.Name, 2, PROPERTY_READ);
                }
                TestInitialize();
            }
            //context.WriteLine("Se han comprobado " + properties.Count() + " propiedades de tipo '" + typeof(T).Name + "'");
        }
        [Fact]
        public void RaiseOnChangeOrAlways2()
        {
            var props = mock.GetType().GetProperties().AsQueryable();
            RaiseOnChangeOrAlways(999, props.Where(p => p.PropertyType == typeof(int)).ToArray());
            RaiseOnChangeOrAlways("zzz", props.Where(p => p.PropertyType == typeof(string)).ToArray());
            RaiseOnChangeOrAlways(
                new ReferenceObjectMock { Cadena = "zzz", Entero = 999 },
                props.Where(p => p.PropertyType == typeof(ReferenceObjectMock)).ToArray());
        }
        #endregion
        #region ReadInterceptionSetValue
        /// <summary>
        /// Probar que pasa cuando intercepto la lectura y devuelvo otro valor, ese valor se aplica al campo interno, es decir,
        /// si despues de interceptar una lectura hago un seteo, que valor me pone en previousValue
        /// </summary>
        //private void ReadInterceptionSetValue<T>(T readCalculatedValue, T change, params PropertyInfo[] properties)
        //{
        //    foreach (var pro in properties)
        //    {
        //        bool intercept = true;
        //        //Interceptar la lectura
        //        //mock.PropertyRead += (s, e) => e.Case<T>(pro.Name, a => intercept ? readCalculatedValue : a.Value);
        //        //Leer la propiedad
        //        Assert.AreEqual((T)pro.GetValue(mock, null), readCalculatedValue);
        //        //Quitar la interceptacion
        //        intercept = false;
        //        //Volver a leer la propiedad para comprobar que tiene el valor que se devolvió en la interceptación anterior
        //        Assert.AreEqual((T)pro.GetValue(mock, null), readCalculatedValue);
        //        //Setear la propiedad y comprobar los parametros de PropertyChanging y PropertyChanged
        //        //mock.PropertyChanging += (s, e) => e.Case<T>(pro.Name, a => Assert.AreEqual(a.ActualValue, readCalculatedValue));
        //        mock.PropertyChanged += (s, e) => e.Case<T>(pro.Name, a => Assert.AreEqual(a.PreviousValue, readCalculatedValue));
        //        pro.SetValue(mock, change, null);
        //        //CheckCounter(pro.Name, 1, PROPERTY_CHANGING);
        //        TestInitialize();
        //    }
        //    context.WriteLine("Se han comprobado " + properties.Count() + " propiedades de tipo '" + typeof(T).Name + "'");
        //}
        //[TestMethod]
        //public void ReadInterceptionSetValue()
        //{
        //    var props = mock.GetType().GetProperties().AsQueryable();
        //    ReadInterceptionSetValue(999, 222, props.Where(p => p.PropertyType == typeof(int)).ToArray());
        //    ReadInterceptionSetValue("zzz", "bbb", props.Where(p => p.PropertyType == typeof(string)).ToArray());
        //    ReadInterceptionSetValue(
        //        new ReferenceObjectMock { Cadena = "zzz", Entero = 999 },
        //        new ReferenceObjectMock { Cadena = "bbb", Entero = 222 },
        //        props.Where(p => p.PropertyType == typeof(ReferenceObjectMock)).ToArray());
        //}
        #endregion
        #region Method IS
        [Fact]
        public void Method_IsAny()
        {
            var passed = false;
            mock.PropertyChanged += (s, e) =>
            {
                if (e.IsAny(
                    () => mock.Integer_FieldImplemented_WithoutDefault_RaiseAlways_Locked,
                    () => mock.ReferenceObject_AutoImplemented_WithDefault_RaiseOnChange_NotLocked))
                {
                    passed = true;
                }
            };
            mock.Integer_FieldImplemented_WithoutDefault_RaiseAlways_Locked = 3;
            Assert.True(passed);
            passed = false;
            mock.ReferenceObject_AutoImplemented_WithDefault_RaiseOnChange_NotLocked = null;
            Assert.True(passed);
        }
        #endregion
        #region Binding
        [Fact]
        public void Notifier_Binding_OneWay()
        {
            var source = new NotifierMock();
            var target = new NotifierMock();
            source.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked = 111;
            source.Binding(() => source.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked).OneWayTo(target, () => target.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked);
            // Check that the target property will set with source property value
            Assert.Equal(111, target.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked);
            source.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked = 123;
            Assert.Equal(123, target.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked);
        }
        [Fact]
        public void Notifier_Binding_OneWay_WithTransformation()
        {
            var source = new NotifierMock();
            var target = new NotifierMock();
            source.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked = 111;
            source.Binding(() => source.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked).OneWayTo(target, () => target.String_AutoImplemented_WithDefault_RaiseOnChange_NotLocked, i => i.ToString());
            // Check that the target property will set with source property value
            Assert.Equal(111.ToString(), target.String_AutoImplemented_WithDefault_RaiseOnChange_NotLocked);
            source.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked = 123;
            Assert.Equal(123.ToString(), target.String_AutoImplemented_WithDefault_RaiseOnChange_NotLocked);
        }
        [Fact]
        public void Notifier_Binding_TwoWay()
        {
            var source = new NotifierMock();
            var target = new NotifierMock();
            source.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked = 111;
            source.Binding(() => source.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked)
                .TwoWayTo(target, () => target.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked);
            // Check that the target property will set with source property value
            Assert.Equal(111, target.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked);

            source.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked = 123;
            // Check that the bind works in source=>target direction
            Assert.Equal(123, target.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked);

            target.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked = 321;
            // Check that the bind works in target=>source direction
            Assert.Equal(321, source.Integer_AutoImplemented_WithDefault_RaiseOnChange_NotLocked);
        }
        #endregion
    }
}
