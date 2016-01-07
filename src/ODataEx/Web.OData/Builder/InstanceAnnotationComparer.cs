namespace More.Web.OData.Builder
{
    using More.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents an equality comparer for <see cref="InstanceAnnotation">instance annotations</see>.
    /// </summary>
    public sealed class InstanceAnnotationComparer : IEqualityComparer<InstanceAnnotation>
    {
        private static readonly IEqualityComparer<InstanceAnnotation> instance = new InstanceAnnotationComparer();
        private readonly IEqualityComparer<string> innerComparer = StringComparer.Ordinal;
        
        private InstanceAnnotationComparer()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the equality comparer.
        /// </summary>
        /// <value>An <see cref="IEqualityComparer{T}">equality comparer</see> object.</value>
        public static IEqualityComparer<InstanceAnnotation> Instance
        {
            get
            {
                Contract.Ensures( instance != null );
                return instance;
            }
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The object to compare.</param>
        /// <param name="y">The object to compare against.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public bool Equals( InstanceAnnotation x, InstanceAnnotation y )
        {
            if ( x == null )
                return y == null;

            if ( y == null )
                return false;

            return innerComparer.Equals( x.QualifiedName, y.QualifiedName );
        }

        /// <summary>
        /// Gets the hash code for the specified object.
        /// </summary>
        /// <param name="obj">The object to get the hash code for.</param>
        /// <returns></returns>
        public int GetHashCode( InstanceAnnotation obj ) => obj == null ? 0 : innerComparer.GetHashCode( obj.QualifiedName );
    }
}
