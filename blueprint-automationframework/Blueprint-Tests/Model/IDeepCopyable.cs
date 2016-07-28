namespace Model
{
    public interface IDeepCopyable<T>
    {
        /// <summary>
        /// Makes a deep copy of this object.
        /// </summary>
        /// <returns>The deep copy of this object.</returns>
        T DeepCopy();
    }
}
