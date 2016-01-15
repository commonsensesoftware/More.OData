namespace More.Examples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using static System.Console;
    using ODataClient = Container;

    public class Program
    {
        private static readonly Uri ServiceRoot = new Uri( "http://localhost:31474/api" );

        public static void Main( string[] args )
        {
            // note: make sure both projects are set to start when running the demo
            RunDemoWithAnnotationsAsync().GetAwaiter().GetResult();
            Pause();

            RunDemoWithAnnotatedLinksAsync().GetAwaiter().GetResult();
            Pause();
        }

        private static void Pause()
        {
            Write( "\n\nPress any key to continue..." );
            ReadLine();
            WriteLine( "\n\n" );
        }

        // this example run a basic linq query against the client including annotations
        private static async Task RunDemoWithAnnotationsAsync()
        {
            WriteLine( "ANNOTATION DEMO -------------------------------------------------------\n" );

            var client = new ODataClient( ServiceRoot );
            var query = client.Devices.Where( d => d.SerialNumber == "12345" );

            foreach ( var device in await query.ExecuteWithAnnotationsAsync() )
            {
                WriteLine( $"Serial #={device.SerialNumber}, SKU='{device.Sku}' ({device.SkuType}), Last Modified={device.LastModified}" );
            }
        }

        // this example run a basic linq query against the client including link annotations
        private static async Task RunDemoWithAnnotatedLinksAsync()
        {
            WriteLine( "ANNOTATED LINKS DEMO -------------------------------------------------------\n" );

            var client = new ODataClient( ServiceRoot );
            var query = client.Devices.Where( d => d.SerialNumber == "12345" );

            foreach ( var device in await query.ExecuteWithLinksAsync() )
            {
                WriteLine( $"Serial #={device.SerialNumber}, SKU='{device.Sku}', Part #={device.PartNumber}" );

                foreach ( var link in device.Links )
                    WriteLine( $"\tName={link.Name}, Url={link.Url}" );
            }
        }
    }
}
