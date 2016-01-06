namespace System
{
    using static Globalization.CultureInfo;

    /// <summary>
    /// Backported type used by the compiler for string interpolation.
    /// </summary>
    internal sealed class FormattableString
    {
        private readonly string messageFormat;
        private readonly object[] args;

        public FormattableString( string messageFormat, object[] args )
        {
            this.messageFormat = messageFormat;
            this.args = args;
        }

        public override string ToString() => ToString( CurrentCulture );

        public string ToString( IFormatProvider provider )  => string.Format( provider, messageFormat, args );
    }
}
