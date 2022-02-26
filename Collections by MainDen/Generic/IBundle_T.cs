namespace MainDen.Collections.Generic
{
    /// <summary>
    ///  Defines methods to manipulate generic bundles.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the bundle.</typeparam>
    public interface IBundle<T>
    {
        /// <summary>
        /// Takes any object from the <see cref="IBundle{T}"/>.
        /// </summary>
        /// <param name="item">An object located in the <see cref="IBundle{T}"/>.</param>
        /// <returns>true if item was successfully taken from the <see cref="IBundle{T}"/>; otherwise, false. This method also returns false if no items are found in the original <see cref="IBundle{T}"/>.</returns>
        bool Take(out T item);

        /// <summary>
        /// Puts an item to the <see cref="IBundle{T}"/>.
        /// </summary>
        /// <param name="item">The object to put to the <see cref="IBundle{T}"/>.</param>
        /// <returns>true if item was successfully putted to the <see cref="IBundle{T}"/>; otherwise, false. This method also returns false if item already exists in the original <see cref="IBundle{T}"/>.</returns>
        bool Put(T item);
    }
}
