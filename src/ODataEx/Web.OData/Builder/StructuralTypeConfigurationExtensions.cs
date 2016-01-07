namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using System;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Builder;

    internal static class StructuralTypeConfigurationExtensions
    {
        private sealed class EdmTypeConfigurationAdapter<T> : IEdmTypeConfiguration where T : class
        {
            private readonly StructuralTypeConfiguration<T> configuration;
            private readonly Lazy<StructuralTypeConfiguration> innerConfiguration;

            internal EdmTypeConfigurationAdapter( StructuralTypeConfiguration<T> configuration )
            {
                Contract.Requires( configuration != null );
                this.configuration = configuration;
                innerConfiguration = new Lazy<StructuralTypeConfiguration>( configuration.GetInnerConfiguration );
            }

            public Type ClrType
            {
                get
                {
                    return innerConfiguration.Value.ClrType;
                }
            }

            public string FullName
            {
                get
                {
                    return configuration.FullName;
                }
            }

            public EdmTypeKind Kind
            {
                get
                {
                    return innerConfiguration.Value.Kind;
                }
            }

            public ODataModelBuilder ModelBuilder
            {
                get
                {
                    return innerConfiguration.Value.ModelBuilder;
                }
            }

            public string Name
            {
                get
                {
                    return configuration.Name;
                }
            }

            public string Namespace
            {
                get
                {
                    return configuration.Namespace;
                }
            }
        }

        internal static IEdmTypeConfiguration ToEdmTypeConfiguration<T>( this StructuralTypeConfiguration<T> configuration ) where T : class
        {
            Contract.Requires( configuration != null );
            Contract.Ensures( Contract.Result<IEdmTypeConfiguration>() != null );
            return new EdmTypeConfigurationAdapter<T>( configuration );
        }
    }
}
