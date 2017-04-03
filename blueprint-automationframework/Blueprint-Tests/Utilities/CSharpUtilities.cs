using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utilities
{
    public static class CSharpUtilities
    {
        /// <summary>
        /// Set one property to a specific value.
        /// </summary>
        /// <param name="propertyName">Name of the property in which value will be changed.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        /// <param name="objectToUpdate">Object that contains the property to be changed.</param>
        public static void SetProperty<T>(string propertyName, T propertyValue, object objectToUpdate)
        {
            ThrowIf.ArgumentNull(objectToUpdate, nameof(objectToUpdate));
            objectToUpdate.GetType().GetProperty(propertyName).SetValue(objectToUpdate, propertyValue, null);
        }

        /// <summary>
        /// Replaces all non-null properties from the source object into the destination object.
        /// </summary>
        /// <typeparam name="T">The object type of the source and destination.</typeparam>
        /// <param name="source">The object whose non-null properties will be copied.</param>
        /// <param name="destination">The object whose properties will be replaced.</param>
        /// <param name="propertiesNotToBeReplaced">(optional) A list of property names which should not be replaced whether it's null or not.</param>
        public static void ReplaceAllNonNullProperties<T>(T source, T destination, List<string> propertiesNotToBeReplaced = null)
        {
            foreach (PropertyInfo sourcePropertyInfo in source.GetType().GetProperties())
            {
                PropertyInfo destinationPropertyInfo = destination.GetType().GetProperties().FirstOrDefault(p => p.Name == sourcePropertyInfo.Name);

                if (destinationPropertyInfo != null && destinationPropertyInfo.CanWrite)
                {
                    if ((propertiesNotToBeReplaced == null) || !propertiesNotToBeReplaced.Contains(destinationPropertyInfo.Name))
                    {
                        var value = sourcePropertyInfo.GetValue(source);

                        if (value != null)
                        {
                            destinationPropertyInfo.SetValue(destination, value);
                        }
                    }
                }
            }
        }
    }
}
