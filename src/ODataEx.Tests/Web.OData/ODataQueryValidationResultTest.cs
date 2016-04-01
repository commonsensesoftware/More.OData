namespace More.Web.OData
{
    using Microsoft.OData.Core;
    using Moq;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="ODataQueryValidationResult"/>.
    /// </summary>
    public class ODataQueryValidationResultTest
    {
        [Fact( DisplayName = "new validation result should be invalid when error is provided with a controller" )]
        public void IsValidShouldBeFalseWhenErrorIsProvidedWithController()
        {
            // arrange
            var error = new ODataException( "Failed validation." );
            var controller = new Mock<ApiController>().Object;
            var result = new ODataQueryValidationResult( error, controller );
            var expected = false;

            // act
            var actual = result.IsValid;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "new validation result should be valid when no error is provided with a controller" )]
        public void IsValidShouldBeTrueWhenNoErrorIsProvidedWithController()
        {
            // arrange
            ODataException error = null;
            var controller = new Mock<ApiController>().Object;
            var result = new ODataQueryValidationResult( error, controller );
            var expected = true;

            // act
            var actual = result.IsValid;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "new validation result should be invalid when error is provided without a controller" )]
        public void IsValidShouldBeFalseWhenErrorIsProvidedWithoutController()
        {
            // arrange
            var error = new ODataException( "Failed validation." );
            var contentNegotiator = new Mock<IContentNegotiator>().Object;
            var request = new HttpRequestMessage();
            var formatters = Enumerable.Empty<MediaTypeFormatter>();
            var result = new ODataQueryValidationResult( error, contentNegotiator, request, formatters );
            var expected = false;

            // act
            var actual = result.IsValid;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "new validation result should be valid when no error is provided without a controller" )]
        public void IsValidShouldBeTrueWhenNoErrorIsProvidedWithoutController()
        {
            // arrange
            ODataException error = null;
            var contentNegotiator = new Mock<IContentNegotiator>().Object;
            var request = new HttpRequestMessage();
            var formatters = Enumerable.Empty<MediaTypeFormatter>();
            var result = new ODataQueryValidationResult( error, contentNegotiator, request, formatters );
            var expected = true;

            // act
            var actual = result.IsValid;

            // assert
            Assert.Equal( expected, actual );
        }
    }
}
