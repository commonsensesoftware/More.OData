namespace More.Web.OData.Formatter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal sealed class ODataSerializationFeatureCollection : IList<IODataSerializationFeature>
    {
        private readonly IReadOnlyList<IList<IODataSerializationFeature>> sources;

        internal ODataSerializationFeatureCollection( params IList<IODataSerializationFeature>[] sources )
        {
            Contract.Requires( sources != null );
            this.sources = sources;
        }

        public IODataSerializationFeature this[int index]
        {
            get
            {
                if ( sources.Count == 0 )
                    throw new ArgumentOutOfRangeException( nameof( index ) );

                return sources[0][index];
            }
            set
            {
                if ( sources.Count == 0 )
                    throw new ArgumentOutOfRangeException( nameof( index ) );

                foreach ( var source in sources )
                    source[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return sources.Count == 0 ? 0 : sources[0].Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add( IODataSerializationFeature item )
        {
            foreach ( var source in sources ) source.Add( item );
        }

        public void Clear()
        {
            foreach ( var source in sources )
                source.Clear();
        }

        public bool Contains( IODataSerializationFeature item ) => sources.Count > 0 && sources[0].Contains( item );

        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Handled by the inner list." )]
        public void CopyTo( IODataSerializationFeature[] array, int arrayIndex )
        {
            if ( sources.Count > 0 )
                sources[0].CopyTo( array, arrayIndex );
        }

        public IEnumerator<IODataSerializationFeature> GetEnumerator() =>
            sources.Count == 0 ? Enumerable.Empty<IODataSerializationFeature>().GetEnumerator() : sources[0].GetEnumerator();

        public int IndexOf( IODataSerializationFeature item ) => sources.Count > 0 ? sources[0].IndexOf( item ) : -1;

        public void Insert( int index, IODataSerializationFeature item )
        {
            foreach ( var source in sources )
                source.Insert( index, item );
        }

        public bool Remove( IODataSerializationFeature item )
        {
            var removed = false;

            foreach ( var source in sources )
                removed |= source.Remove( item );

            return removed;
        }

        public void RemoveAt( int index )
        {
            foreach ( var source in sources )
                source.RemoveAt( index );
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
