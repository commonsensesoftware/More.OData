namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using System;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IAnnotationConfiguration ) )]
    internal abstract class IAnnotationConfigurationContract : IAnnotationConfiguration
    {
        void IAnnotationConfiguration.Apply( IEdmModel model )
        {
            Contract.Requires<ArgumentNullException>( model != null, nameof( model ) );
        }
    }
}
