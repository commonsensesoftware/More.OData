namespace More.Examples
{
    using System;
    using static System.Web.Http.GlobalConfiguration;

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start( object sender, EventArgs e ) => Configure( WebApiConfig.Register );
    }
}