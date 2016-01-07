namespace More.OData.Edm
{
    using System;

    /// <summary>
    /// Represents an annotation for lower camel casing.
    /// </summary>
    public sealed class LowerCamelCaseAnnotation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LowerCamelCaseAnnotation"/> class.
        /// </summary>
        /// <param name="enabled">True if the lower camel casing is enabled.</param>
        public LowerCamelCaseAnnotation( bool enabled )
        {
            IsEnabled = enabled;
        }

        /// <summary>
        /// Gets a value indicating whether lower camel casing is enabled.
        /// </summary>
        /// <value>True if lower camel casing is enabled; otherwise, false.</value>
        public bool IsEnabled
        {
            get;
        }
    }
}
