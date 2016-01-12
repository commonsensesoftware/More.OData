namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Core;
    using System;
    using System.Diagnostics.Contracts;

    [ContractClassFor( typeof( IODataSerializationFeature ) )]
    internal abstract class IODataSerializationFeatureContract : IODataSerializationFeature
    {
        void IODataSerializationFeature.Apply( ODataFeed feed, ODataSerializationFeatureContext context )
        {
            Contract.Requires<ArgumentNullException>( feed != null, nameof( feed ) );
            Contract.Requires<ArgumentNullException>( context != null, nameof( context ) );
        }

        void IODataSerializationFeature.Apply( ODataEntry entry, ODataSerializationFeatureContext context )
        {
            Contract.Requires<ArgumentNullException>( entry != null, nameof( entry ) );
            Contract.Requires<ArgumentNullException>( context != null, nameof( context ) );
        }

        void IODataSerializationFeature.Apply( ODataComplexValue complexValue, ODataSerializationFeatureContext context )
        {
            Contract.Requires<ArgumentNullException>( complexValue != null, nameof( complexValue ) );
            Contract.Requires<ArgumentNullException>( context != null, nameof( context ) );
        }
    }
}
