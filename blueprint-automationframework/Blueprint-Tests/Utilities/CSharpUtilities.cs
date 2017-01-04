namespace Utilities
{
    public static class CSharpUtilities
    {
        /// <summary>
        /// Set one property to a specific value.
        /// </summary>
        /// <param name="propertyName">Name of the property in which value will be changed.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        /// <param name="obToUpdate">Object that contains the property to be changed.</param>
        public static void SetProperty<T>(string propertyName, T propertyValue, object obToUpdate)
        {
            ThrowIf.ArgumentNull(obToUpdate, nameof(obToUpdate));
            obToUpdate.GetType().GetProperty(propertyName).SetValue(obToUpdate, propertyValue, null);
        }
    }
}
