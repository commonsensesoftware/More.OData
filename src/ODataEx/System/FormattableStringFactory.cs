namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Backported type used by the compiler for string interpolation.
    /// </summary>
    internal static class FormattableStringFactory
    {
        public static FormattableString Create( string messageFormat, params object[] args ) => new FormattableString( messageFormat, args );
    }
}
