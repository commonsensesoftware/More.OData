namespace More.Examples
{
    using Microsoft.OData.Client;
    using Microsoft.OData.Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extensions methods for executing and deserializing OData queries that include instance annotations.
    /// For more information, see: http://odata.github.io/odata.net/04-05-client-annotation-support/
    /// </summary>
    public static class ODataClientExtensions
    {
        public static async Task<IEnumerable<Device>> ExecuteWithAnnotationsAsync( this IQueryable<Device> query )
        {
            var dsq = (DataServiceQuery<Device>) query;
            var context = dsq.Context;
            var entities = new List<Device>();

            // note: we can ask specificly for the "external.links" annotation or all annotations in the
            // "external" namespace with the "*" wildcard
            context.DisableInstanceAnnotationMaterialization = false;
            context.SendingRequest2 += ( s, e ) => e.RequestMessage.PreferHeader().AnnotationFilter = "my.*";

            foreach ( var entity in await dsq.ExecuteAsync() )
            {
                DateTimeOffset lastModified;
                string skuType;

                if ( context.TryGetAnnotation( entity, "my.lastModified", out lastModified ) )
                    entity.LastModified = lastModified;

                if ( context.TryGetAnnotation<Func<string>, string>( () => entity.Sku, "my.skuType", out skuType ) )
                    entity.SkuType = skuType;

                entities.Add( entity );
            }

            return entities;
        }

        public static async Task<IEnumerable<T>> ExecuteWithLinksAsync<T>( this IQueryable<T> query ) where T : IHaveExternalLinks
        {
            var dsq = (DataServiceQuery<T>) query;
            var context = dsq.Context;
            var entities = new List<T>();

            // note: we can ask specificly for the "external.links" annotation or all annotations in the
            // "external" namespace with the "*" wildcard
            context.DisableInstanceAnnotationMaterialization = false;
            context.SendingRequest2 += ( s, e ) => e.RequestMessage.PreferHeader().AnnotationFilter = "external.*";

            foreach ( var entity in await dsq.ExecuteAsync() )
            {
                ICollection<Link> links;

                if ( context.TryGetAnnotation( entity, "external.links", out links ) )
                {
                    foreach ( var link in links )
                        entity.Links.Add( link );
                }

                entities.Add( entity );
            }

            return entities;
        }
    }
}
