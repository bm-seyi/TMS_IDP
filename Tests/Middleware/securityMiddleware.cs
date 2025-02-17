using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using TMS_IDP.Middleware;

namespace TMS_IDP.Tests.Middleware
{
    [TestClass]
    public class ApiMiddlewareTests
    {

        private IConfiguration _configuration = null!;
        private ApiMiddleware _middleware  = null!;

        [TestInitialize]
        public void Setup()
        {
            _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "API:Key", "test-api-key" },
            }).Build();
            _middleware = new ApiMiddleware(_ => Task.CompletedTask, _configuration);

        }

        [TestMethod]
        public async Task InvokeAsync_NoApiKeyHeader_Returns401Unauthorized()
        {
            
            DefaultHttpContext context = new DefaultHttpContext();

            await _middleware.InvokeAsync(context);

            Assert.AreEqual((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }

        [TestMethod]
        public async Task InvokeAsync_NullOrEmptyConfigApiKey_ArgumentNullException()
        {
            IConfigurationRoot emptyConfig = new ConfigurationBuilder().Build();

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            {
               return Task.Run( () => new ApiMiddleware(_ => Task.CompletedTask, emptyConfig));
            } );
        }

        [TestMethod]
        public async Task InvokeAsync_IncorrectAPIKey_Returns401()
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Headers["x-api-key"] = "wrong-api-key";

            await _middleware.InvokeAsync(context);

            Assert.AreEqual((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }

        [TestMethod]
        public async Task InvokeAsync_CorrectKeyReturns200()
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Headers["x-api-key"] = "test-api-key";

            await _middleware.InvokeAsync(context);
            Assert.AreEqual((int)HttpStatusCode.OK, context.Response.StatusCode);
        }

        [TestMethod]
        public async Task InvokeAsync_EmptyHeader_Returns401()
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Headers[""] = "test-api-key";

            await _middleware.InvokeAsync(context);
            Assert.AreEqual((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }

        [TestMethod]
        public async Task InvokeAsync_MultipleHeaders_Returns401()
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Headers["x-api-key"] = new[] {"test-api-key", "extra-key"};

            await _middleware.InvokeAsync(context);
            Assert.AreEqual((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }

        [TestMethod]
        public async Task InvokeAsync_MissingConfigAPIKey_ArgumentNullException()
        {
            IConfigurationRoot empty_configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
            
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => 
            {
                return Task.Run(() => new ApiMiddleware(_ => Task.CompletedTask, empty_configuration));
            });
        }
    }
}