using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
namespace Fuxion
{
    public static class Extensions
    {
        /// <summary>
        /// Permite una clonación en profundidad de origen. 
        /// </summary>
        /// <param name="origen">Objeto serializable</param>
        /// <exception cref="ArgumentExcepcion">
        /// Se produce cuando el objeto no es serializable.
        /// </exception>
        /// <remarks>Extraido desde 
        /// http://es.debugmodeon.com/articulo/clonar-objetos-de-estructura-compleja
        /// </remarks>
        public static T Clone<T>(this T source)
        {
            // Verificamos que sea serializable antes de hacer la copia            
            if (!typeof(T).IsSerializable)
                throw new ArgumentException("La clase " + typeof(T).ToString() + " no es serializable");

            // En caso de ser nulo el objeto, se devuelve tal cual
            if (Object.ReferenceEquals(source, null))
                return default(T);

            //Creamos un stream en memoria            
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                try
                {
                    formatter.Serialize(stream, source);
                    stream.Seek(0, SeekOrigin.Begin);
                    //Deserializamos la porcón de memoria en el nuevo objeto                
                    return (T)formatter.Deserialize(stream);
                }
                catch (SerializationException ex)
                { throw new ArgumentException(ex.Message, ex); }
                catch { throw; }
            }
        }
    }
}
