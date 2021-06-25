using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Model
{
    public abstract class PropertyLoader<T> : IPropertyLoader
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
        public void Load(object obj)
        {
            if (!GetType().Equals(_type)) throw new ArgumentException($"Types are not equal");

            foreach (var property in _properties)
            {
                if (typeof(IPropertyLoader).IsAssignableFrom(property.PropertyType))
                {
                    IPropertyLoader propertyLoader = (IPropertyLoader)property.GetValue(this);
                    propertyLoader.Load(property.GetValue(obj));
                }
                else
                {
                    property.SetValue(this, property.GetValue(obj));
                }
            }
        }
    }
}
