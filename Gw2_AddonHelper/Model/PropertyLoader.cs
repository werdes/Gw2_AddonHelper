using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Model
{
    public abstract class PropertyLoader<T>
    {
        private Type _type;
        private PropertyInfo[] _properties;

        public PropertyLoader()
        {
            _type = typeof(T);
            _properties = _type.GetProperties();
        }

        /// <summary>
        /// Loads the properties of the supplied object
        /// </summary>
        /// <param name="obj"></param>
        public void Load(T obj)
        {
            if (!GetType().Equals(_type)) throw new ArgumentException($"Types are not equal");

            foreach (var property in _properties)
            {
                property.SetValue(this, property.GetValue(obj));
            }
        }
    }
}
