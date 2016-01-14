namespace System.Web.OData.Builder
{
    using Collections;
    using Diagnostics.CodeAnalysis;
    using More.Web.OData.Builder;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using static More.ExceptionMessage;
    using static System.Globalization.CultureInfo;

    /// <summary>
    /// Represents a collection of <see cref="IAnnotationConfiguration">annotation configurations</see>.
    /// </summary>
    public sealed class AnnotationConfigurationCollection : IReadOnlyList<IAnnotationConfiguration>
    {
        private readonly List<IAnnotationConfiguration> items = new List<IAnnotationConfiguration>();
        private readonly Dictionary<string, int> keyToIndexMap = new Dictionary<string, int>();

        internal void Clear()
        {
            keyToIndexMap.Clear();
            items.Clear();
        }

        /// <summary>
        /// Adds an annotation configuration with the specified key.
        /// </summary>
        /// <param name="key">The annotation key.</param>
        /// <param name="item">The <see cref="IAnnotationConfiguration">annotation configuration</see> to add.</param>
        /// <remarks>An annotation configuration can only be added for a key once.</remarks>
        public void Add( string key, IAnnotationConfiguration item )
        {
            Arg.NotNullOrEmpty( key, nameof( key ) );
            Arg.NotNull( item, nameof( item ) );

            if ( keyToIndexMap.ContainsKey( key ) )
                throw new InvalidOperationException( string.Format( CurrentCulture, MultipleAnnotationConfigsNotAllowed, key ) );

            items.Add( item );
            var index = items.Count - 1;
            keyToIndexMap.Add( key, index );
        }

        /// <summary>
        /// Attempts to get the annotation configuration with the specified key.
        /// </summary>
        /// <typeparam name="TConfiguration">The <see cref="Type">type</see> of annotation configuration.</typeparam>
        /// <param name="key">The annotation key.</param>
        /// <param name="item">The retrieved <see cref="IAnnotationConfiguration">item</see> or <c>null</c>.</param>
        /// <returns>True if the item is successfully retrieved; otherwise, false.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics" )]
        public bool TryGet<TConfiguration>( string key, out TConfiguration item )
            where TConfiguration : class, IAnnotationConfiguration
        {
            Arg.NotNullOrEmpty( key, nameof( key ) );
            Contract.Ensures( ( Contract.Result<bool>() && Contract.ValueAtReturn( out item ) != null ) || Contract.ValueAtReturn( out item ) == null );

            item = null;
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
