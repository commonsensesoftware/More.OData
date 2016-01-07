namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using System;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IAnnotationConfiguration ) )]
    internal abstract class IAnnotationConfigurationContract : IAnnotationConfiguration
    {
        string IAnnotationConfiguration.Namespace
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );
                return null;
            }
            set
            {
                Contract.Requires<ArgumentNullException>( !string.IsNullOrEmpty( value ), nameof( value ) );
            }
        }

        string IAnnotationConfiguration.Name
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );
                return null;
            }
            set
            {
                Contract.Requires<ArgumentNullException>( !string.IsNullOrEmpty( value ), nameof( value ) );
            }
        }

        void IAnnotationConfiguration.Apply( IEdmModel model )
        {
            Contract.Requires<ArgumentNullException>( model != null, nameof( model ) );
        }
    }
}
