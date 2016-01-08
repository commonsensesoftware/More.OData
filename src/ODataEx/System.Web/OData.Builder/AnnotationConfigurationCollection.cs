namespace System.Web.OData.Builder
{
    using Collections;
    using Linq.Expressions;
    using More.Web.OData.Builder;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a collection of <see cref="IAnnotationConfiguration">annotation configurations</see>.
    /// </summary>
    public sealed class AnnotationConfigurationCollection : IReadOnlyList<IAnnotationConfiguration>
    {
        private readonly List<IAnnotationConfiguration> items = new List<IAnnotationConfiguration>();

        /// <summary>
        /// Adds an annotation configuration for the specified property.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> defining the annotation property.</typeparam>
        /// <typeparam name="T">The <see cref="Type">type</see> of annotation value.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> defining the annotation property.</param>
        /// <param name="item">The <see cref="IAnnotationConfiguration">annotation configuration</see> to add.</param>
        /// <returns>True if the annotation configuration is added for the property; otherwise, false.</returns>
        /// <remarks>An annotation configuration can only be added for a property once. If this method is called multiple times for the same property,
        /// then no action is performed.</remarks>
        public bool TryAdd<TStructuralType, T>( Expression<Func<TStructuralType, T>> propertyExpression, IAnnotationConfiguration item )
        {
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Arg.NotNull( item, nameof( item ) );

            //var key = $"{typeof( TStructuralType ).FullName}.{( (MemberExpression) propertyExpression.Body ).Member.Name}";

            return false;
        }

        /// <summary>
        /// Gets the annotation configuration for the specified property.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> defining the annotation property.</typeparam>
        /// <typeparam name="T">The <see cref="Type">type</see> of annotation value.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> defining the annotation property.</param>
        /// <returns>The <see cref="IAnnotationConfiguration">annotation configuration</see> for the specified property or <c>null</c> if the property
        /// has not been configured as an annotation.</returns>
        public IAnnotationConfiguration Get<TStructuralType, T>( Expression<Func<TStructuralType, T>> propertyExpression )
        {
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );

            return null;
        }

        /// <summary>
        /// Gets the item in the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to retrieve.</param>
        /// <returns>The <see cref="IAnnotationConfiguration">annotation configuration</see> at the specified <paramref name="index"/>.</returns>
        public IAnnotationConfiguration this[int index]
        {
            get
            {
                return items[index];
            }
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        /// <value>The total number of items in the collection.</value>
        public int Count
        {
            get
            {
                return items.Count;
            }
        }

        /// <summary>
        /// Gets an iterator that can enumerate the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}">iterator</see> for the colletion.</returns>
        public IEnumerator<IAnnotationConfiguration> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
