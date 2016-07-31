using System;

namespace Stamps.View
{
    /// <summary>
    /// Some handy extension methods for less pain with exceptions in C#.
    /// </summary>
    public static class Exceptions
    {
        /// <summary>
        /// Allows to throw an exception inside of an expression, instead of inside of a statement only.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expression.
        /// </typeparam>
        /// <param name="e">
        /// The exception to throw.
        /// </param>
        /// <returns>
        /// Never returns, throws <paramref name="e"/>.
        /// </returns>
        public static T Throw<T>(Exception e)
        {
            throw e;
        }

        /// <summary>
        /// A variant of <see cref="Throw{T}(Exception)"/> as an extension method.
        /// </summary>      
        public static T Throw<T>(this Object obj, Exception e)
        {
            throw e;
        }
    }
}
