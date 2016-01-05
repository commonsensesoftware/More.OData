namespace More
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Runtime.Serialization;

    internal static class TypeExtensions
    {
        internal static bool IsEnumerable( this Type type ) => typeof( IEnumerable ).IsAssignableFrom( type ) && !typeof( string ).Equals( type );

        [SuppressMessage( "Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Justification = "String interpolation is invariant in this context." )]
        internal static string GetQualifiedEdmTypeName( this Type type )
        {
            Contract.Requires( type != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var @namespace = type.GetCustomAttribute<DataContractAttribute>()?.Namespace;

            if ( string.IsNullOrEmpty( @namespace ) )
                return type.FullName;

            return $"{@namespace}.{type.Name}";
        }

        internal static bool IsNullable( this Type type ) => type.IsValueType ? Nullable.GetUnderlyingType( type ) != null : true;

        internal static Type GetUnderlyingType( this Type type ) => type.IsValueType ? ( Nullable.GetUnderlyingType( type ) ?? type ) : type;
    }
}
