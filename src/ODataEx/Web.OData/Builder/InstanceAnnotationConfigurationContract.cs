namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using More.OData.Edm;
    using System;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Builder;

    [ContractClassFor( typeof( InstanceAnnotationConfiguration ) )]
    internal abstract class InstanceAnnotationConfigurationContract : InstanceAnnotationConfiguration
    {
        protected InstanceAnnotationConfigurationContract( IEdmTypeConfiguration typeConfiguration, string name, InstanceAnnotation annotation )
            : base( typeConfiguration, name, annotation )
        {
        }

        protected override void Apply( IEdmModel model )
        {
            Contract.Requires<ArgumentNullException>( model != null, nameof( model ) );
        }
    }
}
