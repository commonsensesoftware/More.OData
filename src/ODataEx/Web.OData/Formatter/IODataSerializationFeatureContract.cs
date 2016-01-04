namespace More.Web.OData.Formatter
{
    using System;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IODataSerializationFeature ) )]
    internal abstract class IODataSerializationFeatureContract : IODataSerializationFeature
    {
        void IODataSerializationFeature.Apply( ODataEntrySerializationContext context )
        {
            Contract.Requires<ArgumentNullException>( context != null, nameof( context ) );
        }
    }
}
