using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlueDynamicManager.Converters
{
    internal class TypeHandler
    {
        public static bool GetPropValueIfExists(object instance, string propertyName, out object value)
        {
            var prop = instance.GetType().GetProperty(propertyName);
            if (prop != null)
            {
                var propValue = prop.GetValue(instance, null);

                value = propValue;
                return true;
            }

            prop = instance.GetType().BaseType.GetProperty(propertyName);
            if (prop != null)
            {
                var propValue = prop.GetValue(instance, null);

                value = propValue;
                return true;
            }

            value = null;
            return false;
        }

        public static bool GetFieldValueIfExists(object instance, string propertyName, out object value)
        {
            var prop = instance.GetType().GetField(propertyName);
            if (prop != null)
            {
                var propValue = prop.GetValue(instance);

                value = propValue;
                return true;
            }

            prop = instance.GetType().BaseType.GetField(propertyName);
            if (prop != null)
            {
                var propValue = prop.GetValue(instance);

                value = propValue;
                return true;
            }

            value = null;
            return false;
        }

        public static bool SetPropValueIfExists(object instance, string propertyName, object value)
        {
            var prop = instance.GetType().GetProperty(propertyName);
            if (prop != null)
            {
                prop.SetValue(instance, value);

                return true;
            }

            prop = instance.GetType().BaseType.GetProperty(propertyName);
            if (prop != null)
            {
                prop.SetValue(instance, value);

                return true;
            }

            return false;
        }

        public static bool CallMethodIfExists(object instance, string methodName, object[] args)
        {
            var argsTypeArray = args.Select(item => item.GetType()).ToArray();

            var method = instance.GetType().GetMethod(methodName, argsTypeArray);
            if (method != null)
            {
                method.Invoke(instance, args);
                return true;
            }

            method = instance.GetType().BaseType.GetMethod(methodName, argsTypeArray);
            if (method != null)
            {
                method.Invoke(instance, args);
                return true;
            }

            return false;
        }
    }
}
