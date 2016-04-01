namespace More.Web.OData.Routing
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Web.Http.Controllers;
    using System.Web.OData.Routing;
    using System.Web.OData.Routing.Conventions;

    /// <summary>
    /// Represents the OData routing convention for raw values.
    /// </summary>
    /// <value>This routing convention matches routes with the form: "~/entityset/key/$value".</value>
    public class RawValueRoutingConvention : EntityRoutingConvention
    {
        private const string ODataValuePath = "~/entityset/key/$value";
        private const string Value = nameof( Value );

        /// <summary>
        /// Selects a controller action for a media link entry based on the given request and path information.
        /// </summary>
        /// <param name="odataPath">The <see cref="ODataPath">OData path</see> to select the action for.</param>
        /// <param name="controllerContext">The current <see cref="HttpControllerContext">controller context</see>.</param>
        /// <param name="actionMap">The <see cref="ILookup{TKey,TValue}">lookup</see> of available <see cref="HttpActionDescriptor">actions</see>
        /// to select from.</param>
        /// <returns>The matching controller action or <c>null</c> if no match is found.</returns>
        /// <remarks>The action to select is based upon the current HTTP verb and the name 'Value'. Examples include: GetValue, PostValue,
        /// PutValue, DeleteValue, PatchValue, HeadValue.</remarks>
        [SuppressMessage( "Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "This is not a normalization. The characters must be Pascal-cased." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "The framework will never call this method with a null object." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "The framework will never call this method with a null object." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2", Justification = "The framework will never call this method with a null object." )]
        public override string SelectAction( ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap )
        {
            Contract.Assume( odataPath != null );
            Contract.Assume( controllerContext != null );
            Contract.Assume( actionMap != null );

            if ( odataPath.PathTemplate != ODataValuePath )
                return base.SelectAction( odataPath, controllerContext, actionMap );

            controllerContext.RouteData.Values["key"] = ( (KeyValuePathSegment) odataPath.Segments[1] ).Value;

            var action = new StringBuilder( controllerContext.Request.Method.ToString().ToLowerInvariant() );

            // select action based on the syntax: <verb>'Value', where <verb> is Pascal case
            action[0] = char.ToUpperInvariant( action[0] );
            action.Append( Value );

            var actionName = action.ToString();

            return actionMap.Contains( actionName ) ? actionName : null;
        }
    }
}
