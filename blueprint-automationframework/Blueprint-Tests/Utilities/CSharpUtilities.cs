using System.Diagnostics.CodeAnalysis;

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
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
        public static void SetProperty<T>(string propertyName, T propertyValue, object objectToUpdate)
        {
            ThrowIf.ArgumentNull(objectToUpdate, nameof(objectToUpdate));
            objectToUpdate.GetType().GetProperty(propertyName).SetValue(objectToUpdate, propertyValue, null);
        }
    }
}
