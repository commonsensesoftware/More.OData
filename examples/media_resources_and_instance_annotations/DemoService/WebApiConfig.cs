namespace More.Examples
{
    using Models;
    using System.Web.Http;
    using System.Web.OData.Builder;
    using System.Web.OData.Extensions;
    using Web.OData;

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
            device.HasAnnotation( d => d.LastModified ).Namespace = "my";
            device.HasAnnotation( d => d.SkuType ).Namespace = "my";
            device.HasAnnotation( d => d.SkuType ).ForProperty( d => d.Sku );
            device.HasComplexAnnotations( d => d.Links ).Namespace = "external";
            device.Ignore( d => d.ReceiptId );
            devices.HasNavigationPropertyLink( device.HasOptional( d => d.Receipt ), d => d.ReceiptId, "Receipts" );

            builder.ComplexType<Link>().Namespace = builder.Namespace;
        }

        private static void BuildReceiptModel( ODataModelBuilder builder )
        {
            var receipt = builder.EntitySet<Receipt>( "Receipts" ).EntityType;

            receipt.Namespace = builder.Namespace;
            receipt.HasKey( r => r.Id );
            receipt.MediaType( r => r.Image.Type );
        }
    }
}
