namespace More.Web.OData
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.StringComparison;

    /// <summary>
    /// Represents a simple handler for the "Preference-Applied" HTTP header.
    /// </summary>
    /// <remarks>This is a temporary stop-gap fix. The current OData server-side implementation doesn't appear to handle or return the applied preferences.
    /// This class should be removed when preferences are correctly handled; either by correct configuration or library implementation.</remarks>
    internal sealed class SimplePreferenceAppliedHandler : DelegatingHandler
    {
        private const string Prefer = nameof( Prefer );
        private const string PreferenceApplied = "Preference-Applied";
        private const string IncludeAnnotations = "odata.include-annotations";

        protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken ) =>
            EnsurePreferenceApplied( await base.SendAsync( request, cancellationToken ) );

        private static HttpResponseMessage EnsurePreferenceApplied( HttpResponseMessage response )
        {
            Contract.Requires( response != null );

            IEnumerable<string> preferences;
            IEnumerable<string> preferencesApplied;

            // determine if "prefer" header was specified
            if ( !response.RequestMessage.Headers.TryGetValues( Prefer, out preferences ) )
                return response;

            // determine if "preferences-applied" header has been specified
            if ( response.Headers.TryGetValues( PreferenceApplied, out preferencesApplied ) )
            {
                // if the applied annotation filter is present, we're done
                if ( !string.IsNullOrEmpty( preferencesApplied.SingleOrDefault( s => s.StartsWith( IncludeAnnotations, OrdinalIgnoreCase ) ) ) )
                    return response;
            }
            else
            {
                preferencesApplied = Enumerable.Empty<string>();
            }

            // determine if an annotation filter was specified
            var annotationFilter = preferences.SingleOrDefault( s => s.StartsWith( IncludeAnnotations, OrdinalIgnoreCase ) );

            // add the annotation filter to the applied preferences, if any
            if ( !string.IsNullOrEmpty( annotationFilter ) )
            {
                response.Headers.Remove( PreferenceApplied );
                response.Headers.TryAddWithoutValidation( PreferenceApplied, preferencesApplied.Union( new[] { annotationFilter } ) );
            }

            return response;
        }
    }
}
