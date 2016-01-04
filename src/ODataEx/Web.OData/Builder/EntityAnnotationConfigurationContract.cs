namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using System;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( EntityAnnotationConfiguration ) )]
    internal abstract class EntityAnnotationConfigurationContract : EntityAnnotationConfiguration
    {
        protected EntityAnnotationConfigurationContract( string entityTypeName )
            : base( entityTypeName )
        {
        }

        public override void Apply( IEdmModel model )
        {
            Contract.Requires<ArgumentNullException>( model != null, nameof( model ) );
        }
    }
}
