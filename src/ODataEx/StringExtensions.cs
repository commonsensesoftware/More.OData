namespace More
{
    using System;
    using System.Linq;
    using System.Web.OData.Builder;
    using static System.Globalization.CultureInfo;
    using static System.StringSplitOptions;

    internal static class StringExtensions
    {
        private readonly static LowerCamelCaser caser = new LowerCamelCaser();
        private readonly static char[] Period = new[] { '.' };

        internal static string Invariant( FormattableString formattable ) => formattable.ToString( InvariantCulture );

        internal static string ToCamelCase( this string @string )
        {
            if ( string.IsNullOrEmpty( @string ) )
                return @string;

            return string.Join( ".", @string.Split( Period, RemoveEmptyEntries ).Select( s => caser.ToLowerCamelCase( s ) ) );
        }
    }
}
