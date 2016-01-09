namespace More.Examples
{
    using Microsoft.OData.Core;
    using Microsoft.OData.Core.UriParser;
    using Microsoft.OData.Edm;
    using Models;
    using System;
    using System.Linq.Expressions;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Builder;
    using System.Web.OData.Extensions;
    using System.Web.OData.Routing;

    public static class WebApiConfig
    {
        public static void Register( HttpConfiguration configuration )
        {
            var builder = new ODataConventionModelBuilder();

            builder.Namespace = "Examples";
            builder.EnableLowerCamelCase();

            BuildDeviceModel( builder );
            BuildReceiptModel( builder );

            var model = builder.GetEdmModelWithAnnotations();

            configuration.EnableCaseInsensitive( true );
            configuration.EnableInstanceAnnotations();
            configuration.EnableMediaResources();
            configuration.MapODataServiceRoute( "odata", "api", model );
        }

        private static void BuildDeviceModel( ODataModelBuilder builder )
        {
            var devices = builder.EntitySet<Device>( "Devices" );
            var device = devices.EntityType;

            device.Namespace = builder.Namespace;
            device.HasKey( d => d.SerialNumber );
            device.HasAnnotation( d => d.SkuType ).ForProperty( d => d.Sku );
            device.Ignore( d => d.ReceiptId );
            devices.HasNavigationPropertyLink( device.HasOptional( d => d.Receipt ), d => d.ReceiptId, "Receipts" );
        }

        private static void BuildReceiptModel( ODataModelBuilder builder )
        {
            var receipt = builder.EntitySet<Receipt>( "Receipts" ).EntityType;

            receipt.Namespace = builder.Namespace;
            receipt.HasKey( r => r.Id );
            receipt.MediaType( r => r.ImageType );
            receipt.Ignore( r => r.ImagePath );
            receipt.Ignore( r => r.ImageSize );
        }
    }

    internal static class NavigationLinkExtensions
    {
        internal static void HasNavigationPropertyLink<TEntity, TValue>(
            this EntitySetConfiguration<TEntity> entitySet,
            NavigationPropertyConfiguration navigationProperty,
            Expression<Func<TEntity, TValue>> foreignKeyProperty,
            string targetEntitySetName ) where TEntity : class
        {
            var foreignKeyPropertyName = ( (MemberExpression) foreignKeyProperty.Body ).Member.Name;

            entitySet.HasNavigationPropertyLink(
                navigationProperty,
                ( context, property ) =>
                {
                    var entity = context.EdmObject;
                    object value;

                    if ( !entity.TryGetPropertyValue( foreignKeyPropertyName, out value ) )
                        return null;

                    if ( value == null )
                        return null;

                    var entitySetPath = new EntitySetPathSegment( targetEntitySetName );
                    var keyValuePath = new KeyValuePathSegment( ODataUriUtils.ConvertToUriLiteral( value, ODataVersion.V4 ) );
                    var url = new Uri( context.Url.CreateODataLink( entitySetPath, keyValuePath ) );

                    return url;
                },
                false );
        }
    }
}
