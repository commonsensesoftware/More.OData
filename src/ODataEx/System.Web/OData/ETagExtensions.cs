namespace System.Web.OData
{
    using Collections;
    using Diagnostics.CodeAnalysis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.OData.Formatter;

    /// <summary>
    /// Provides extension methods for working with entity tags (ETags).
    /// </summary>
    public static class ETagExtensions
    {
        /// <summary>
        /// Gets a value from the specified entity tag.
        /// </summary>
        /// <typeparam name="TValue">The <see cref="Type">type</see> of value to retrieve.</typeparam>
        /// <param name="etag">The <see cref="ETag"/> to extract the value from.</param>
        /// <param name="name">The name of the value to retrieve.</param>
        /// <returns>The extracted value or the default value of <typeparamref name="TValue"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "By design to express intended use." )]
        public static TValue GetValue<TValue>( this ETag etag, string name )
        {
            Arg.NotNullOrEmpty( name, nameof( name ) );

            if ( etag == null )
                return default( TValue );

            var binder = new SimpleMemberBinder( name );
            object value;

            if ( etag.TryGetMember( binder, out value ) )
                return (TValue) value;

            return default( TValue );
        }

        /// <summary>
        /// Returns a value indicating whether the specified entity tag has the specified value.
        /// </summary>
        /// <param name="etag">The <see cref="ETag"/> to match.</param>
        /// <param name="memberName">The value member name of the entity tag containing the value.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>True if the <paramref name="etag"/> has a matching value; otherwise, false.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "By design to express intended use." )]
        public static bool Has<TValue>( this ETag etag, string memberName, TValue value ) =>
            etag == null ? false : Equal( value, etag.GetValue<TValue>( memberName ) );

        /// <summary>
        /// Returns a value indicating whether the specified entity tag has the specified row version.
        /// </summary>
        /// <param name="etag">The <see cref="ETag"/> to match.</param>
        /// <param name="memberName">The value member name of the entity tag containing the row version value.</param>
        /// <param name="rowVersion">The binary representation of the row version value to compare against.</param>
        /// <returns>True if the <paramref name="etag"/> has a matching row version; otherwise, false.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "By design to express intended use." )]
        public static bool HasRowVersion( this ETag etag, string memberName, byte[] rowVersion )
        {
            if ( etag == null || ( rowVersion?.Length ?? 0 ) == 0 )
                return false;

            var value = etag.GetValue<byte[]>( memberName );

            return value?.SequenceEqual( rowVersion ) ?? false;
        }

        /// <summary>
        /// Returns a value indicating whether the specified entity tag has all of the specified values.
        /// </summary>
        /// <param name="etag">The <see cref="ETag"/> to match.</param>
        /// <param name="values">The <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="KeyValuePair{TKey, TValue}">key/value pairs</see> to compare against.</param>
        /// <returns>True if the <paramref name="etag"/> has all of matching values; otherwise, false.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "By design to express intended use." )]
        public static bool HasAll( this ETag etag, IEnumerable<KeyValuePair<string, object>> values )
        {
            if ( etag == null || values == null )
                return false;

            return values.Any() && values.All( p => Equal( p.Value, etag.GetValue<object>( p.Key ) ) );
        }

        private static bool Equal( object value, object otherValue )
        {
            var sequence = value as IEnumerable;

            if ( sequence == null || value is string )
                return Equals( value, otherValue );

            return SequenceEqual( sequence, otherValue as IEnumerable );
        }

        private static bool SequenceEqual( IEnumerable first, IEnumerable second )
        {
            if ( first == null )
                return second == null;

            if ( second == null )
                return false;

            var enumerator = first.GetEnumerator();
            IEnumerator enumerator2 = null;

            try
            {
                enumerator2 = second.GetEnumerator();

                while ( enumerator.MoveNext() )
                {
                    if ( !enumerator2.MoveNext() || !Equals( enumerator.Current, enumerator2.Current ) )
                        return false;
                }

                if ( enumerator2.MoveNext() )
                    return false;
            }
            finally
            {
                ( enumerator as IDisposable )?.Dispose();
                ( enumerator2 as IDisposable )?.Dispose();
            }

            return true;
        }
    }
}
