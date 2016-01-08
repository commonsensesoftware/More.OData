using System.Diagnostics.Contracts;
namespace System.Web.OData.Builder
{
    using Collections;
    using Diagnostics.CodeAnalysis;
    using Linq.Expressions;
    using More.Web.OData.Builder;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using static More.ExceptionMessage;
    using static More.StringExtensions;
    using static System.Globalization.CultureInfo;

    /// <summary>
    /// Represents a collection of <see cref="IAnnotationConfiguration">annotation configurations</see>.
    /// </summary>
    public sealed class AnnotationConfigurationCollection : IReadOnlyList<IAnnotationConfiguration>
    {
        private readonly List<IAnnotationConfiguration> items = new List<IAnnotationConfiguration>();
        private readonly Dictionary<string, int> keyToIndexMap = new Dictionary<string, int>();

        private static string CreateKey<TStructuralType, T>( Expression<Func<TStructuralType, T>> propertyExpression )
        {
            Contract.Requires( propertyExpression != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var typeName = typeof( TStructuralType ).FullName;
            var propertyName = ( (MemberExpression) propertyExpression.Body ).Member.Name;
            return Invariant( $"{typeName}.{propertyName}" );
        }

        internal void Clear()
        {
            keyToIndexMap.Clear();
            items.Clear();
        }

        /// <summary>
        /// Adds an annotation configuration for the specified property.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> defining the annotation property.</typeparam>
        /// <typeparam name="T">The <see cref="Type">type</see> of annotation value.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> defining the annotation property.</param>
        /// <param name="item">The <see cref="IAnnotationConfiguration">annotation configuration</see> to add.</param>
        /// <remarks>An annotation configuration can only be added for a property once. If this method is called multiple times for the same property,
        /// then no action is performed.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics" )]
        public void Add<TStructuralType, T>( Expression<Func<TStructuralType, T>> propertyExpression, IAnnotationConfiguration item )
        {
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Arg.NotNull( item, nameof( item ) );

            var key = CreateKey( propertyExpression );

            if ( keyToIndexMap.ContainsKey( key ) )
                throw new InvalidOperationException( string.Format( CurrentCulture, MultipleAnnotationConfigsNotAllowed, key ) );

            items.Add( item );
            var index = items.Count - 1;
            keyToIndexMap.Add( key, index );
        }

        /// <summary>
        /// Attempts to get the annotation configuration for the specified property.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> defining the annotation property.</typeparam>
        /// <typeparam name="TProperty">The <see cref="Type">type</see> of annotation value.</typeparam>
        /// <typeparam name="TConfiguration">The <see cref="Type">type</see> of annotation configuration.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> defining the annotation property.</param>
        /// <param name="item">The retrieved <see cref="IAnnotationConfiguration">item</see> or <c>null</c>.</param>
        /// <returns>True if the item is successfully retrieved; otherwise, false.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics" )]
        public bool TryGet<TStructuralType, TProperty, TConfiguration>( Expression<Func<TStructuralType, TProperty>> propertyExpression, out TConfiguration item )
            where TConfiguration : class, IAnnotationConfiguration
        {
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( ( Contract.Result<bool>() && Contract.ValueAtReturn( out item ) != null ) || Contract.ValueAtReturn( out item ) == null );

            item = null;

            var key = CreateKey( propertyExpression );
            var index = 0;

            if ( !keyToIndexMap.TryGetValue( key, out index ) )
                return false;

            item = (TConfiguration) this[index];
            return true;
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
