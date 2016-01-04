namespace More.Web.OData
{
    using System;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="Link"/>.
    /// </summary>
    public class LinkTest
    {
        [Fact( DisplayName = "new link should set expected properties" )]
        public void ConstructorShouldSetExpectedProperties()
        {
            // arrange
            var relation = "test";
            var url = new Uri( "http://tempuri.org" );

            // act
            var link = new Link( relation, url );


            // assert
            Assert.Equal( relation, link.Relation );
            Assert.Equal( url.OriginalString, link.Url );
        }
    }
}
