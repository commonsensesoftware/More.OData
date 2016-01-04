namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Library;
    using Microsoft.Spatial;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Edm = Microsoft.OData.Edm.EdmPrimitiveTypeKind;

    internal static class EdmHelper
    {
        private static EdmCoreModel model = EdmCoreModel.Instance;
        private static readonly Dictionary<Type, IEdmPrimitiveType> builtInTypesMapping = new Dictionary<Type, IEdmPrimitiveType>()
        {
            { typeof( string ), GetPrimitiveType( Edm.String ) },
            { typeof( bool ), GetPrimitiveType( Edm.Boolean ) },
            { typeof( bool? ), GetPrimitiveType( Edm.Boolean ) },
            { typeof( byte ), GetPrimitiveType( Edm.Byte  ) },
            { typeof( byte? ), GetPrimitiveType( Edm.Byte  ) },
            { typeof( DateTime ), GetPrimitiveType( Edm.DateTimeOffset ) },
            { typeof( DateTime? ), GetPrimitiveType( Edm.DateTimeOffset ) },
            { typeof( decimal ), GetPrimitiveType( Edm.Decimal ) },
            { typeof( decimal? ), GetPrimitiveType( Edm.Decimal ) },
            { typeof( double ), GetPrimitiveType( Edm.Double ) },
            { typeof( double? ), GetPrimitiveType( Edm.Double ) },
            { typeof( Guid ), GetPrimitiveType( Edm.Guid ) },
            { typeof( Guid? ), GetPrimitiveType( Edm.Guid ) },
            { typeof( short ), GetPrimitiveType( Edm.Int16 ) },
            { typeof( short? ), GetPrimitiveType( Edm.Int16 ) },
            { typeof( int ), GetPrimitiveType( Edm.Int32 ) },
            { typeof( int? ), GetPrimitiveType( Edm.Int32 ) },
            { typeof( long ), GetPrimitiveType( Edm.Int64 ) },
            { typeof( long? ), GetPrimitiveType( Edm.Int64 ) },
            { typeof( sbyte ), GetPrimitiveType( Edm.SByte ) },
            { typeof( sbyte? ), GetPrimitiveType( Edm.SByte ) },
            { typeof( float ), GetPrimitiveType( Edm.Single ) },
            { typeof( float? ), GetPrimitiveType( Edm.Single ) },
            { typeof( byte[] ), GetPrimitiveType( Edm.Binary ) },
            { typeof( Stream ), GetPrimitiveType( Edm.Stream ) },
            { typeof( Geography ), GetPrimitiveType( Edm.Geography ) },
            { typeof( GeographyPoint ), GetPrimitiveType( Edm.GeographyPoint ) },
            { typeof( GeographyLineString ), GetPrimitiveType( Edm.GeographyLineString ) },
            { typeof( GeographyPolygon ), GetPrimitiveType( Edm.GeographyPolygon ) },
            { typeof( GeographyCollection ), GetPrimitiveType( Edm.GeographyCollection ) },
            { typeof( GeographyMultiLineString ), GetPrimitiveType( Edm.GeographyMultiLineString ) },
            { typeof( GeographyMultiPoint ), GetPrimitiveType( Edm.GeographyMultiPoint ) },
            { typeof( GeographyMultiPolygon ), GetPrimitiveType( Edm.GeographyMultiPolygon ) },
            { typeof( Geometry ), GetPrimitiveType( Edm.Geometry ) },
            { typeof( GeometryPoint ), GetPrimitiveType( Edm.GeometryPoint ) },
            { typeof( GeometryLineString ), GetPrimitiveType( Edm.GeometryLineString ) },
            { typeof( GeometryPolygon ), GetPrimitiveType( Edm.GeometryPolygon ) },
            { typeof( GeometryCollection ), GetPrimitiveType( Edm.GeometryCollection ) },
            { typeof( GeometryMultiLineString ), GetPrimitiveType( Edm.GeometryMultiLineString ) },
            { typeof( GeometryMultiPoint ), GetPrimitiveType( Edm.GeometryMultiPoint ) },
            { typeof( GeometryMultiPolygon ), GetPrimitiveType( Edm.GeometryMultiPolygon ) },
            { typeof( TimeSpan ), GetPrimitiveType( Edm.TimeOfDay ) },
            { typeof( TimeSpan? ), GetPrimitiveType( Edm.TimeOfDay ) },
            { typeof( DateTimeOffset ), GetPrimitiveType( Edm.DateTimeOffset ) },
            { typeof( DateTimeOffset? ), GetPrimitiveType( Edm.DateTimeOffset ) },
            
            // Keep the Binary and XElement in the end, since there are not the default mappings for Edm.Binary and Edm.String.
            { typeof( XElement ), GetPrimitiveType( Edm.String ) },
            // TODO: unsure where the 'Binary' type comes from
            //{ typeof( Binary ), GetPrimitiveType( Edm.Binary ) },
            { typeof( ushort ), GetPrimitiveType( Edm.Int32 ) },
            { typeof( ushort? ), GetPrimitiveType( Edm.Int32 ) },
            { typeof( uint ), GetPrimitiveType( Edm.Int64 ) },
            { typeof( uint? ), GetPrimitiveType( Edm.Int64 ) },
            { typeof( ulong ), GetPrimitiveType( Edm.Int64 ) },
            { typeof( ulong? ), GetPrimitiveType( Edm.Int64 ) },
            { typeof( char[] ), GetPrimitiveType( Edm.String ) },
            { typeof( char ), GetPrimitiveType( Edm.String ) },
            { typeof( char? ), GetPrimitiveType( Edm.String ) },
        }.ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

        private static IEdmPrimitiveType GetPrimitiveType( EdmPrimitiveTypeKind primitiveKind ) => model.GetPrimitiveType( primitiveKind );

        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Reserved for future use." )]
        internal static bool IsPrimitive( Type clrType ) => builtInTypesMapping.ContainsKey( clrType );

        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Reserved for future use." )]
        internal static IEdmPrimitiveType GetEdmPrimitiveTypeOrNull( Type clrType )
        {
            IEdmPrimitiveType type;
            return builtInTypesMapping.TryGetValue( clrType, out type ) ? type : null;
        }

        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Reserved for future use." )]
        internal static IEdmPrimitiveTypeReference GetEdmPrimitiveTypeReferenceOrNull( Type clrType )
        {
            var edmPrimitiveTypeOrNull = GetEdmPrimitiveTypeOrNull( clrType );

            if ( edmPrimitiveTypeOrNull == null )
                return null;

            return model.GetPrimitive( edmPrimitiveTypeOrNull.PrimitiveKind, clrType.IsNullable() );
        }
    }
}
