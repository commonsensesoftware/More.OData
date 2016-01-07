namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using System;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Builder;

    [ContractClassFor( typeof( InstanceAnnotationConfiguration ) )]
    internal abstract class InstanceAnnotationConfigurationContract : InstanceAnnotationConfiguration
    {
        protected InstanceAnnotationConfigurationContract( IEdmTypeConfiguration typeConfiguration, string name )
            : base( typeConfiguration, name )
        {
        }

        protected override void Apply( IEdmModel model )
        {
            Contract.Requires<ArgumentNullException>( model != null, nameof( model ) );
        }
    }
}
