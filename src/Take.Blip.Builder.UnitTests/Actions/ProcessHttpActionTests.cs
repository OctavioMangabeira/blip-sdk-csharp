﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Take.Blip.Builder.Actions.ProcessHttp;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ProcessHttpActionTests : CancellationTokenTestsBase
    {
        public ProcessHttpActionTests()
        {
            HttpClient = Substitute.For<IHttpClient>();
            Context = Substitute.For<IContext>();
        }

        public IHttpClient HttpClient { get; set; }
        public IContext Context { get; private set; }

        private ProcessHttpAction GetTarget()
        {
            return new ProcessHttpAction(HttpClient);
        }

        [Fact]
        public async Task ProcessPostActionShouldSucceed()
        {
            //Arrange
            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://blip.ai"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
                Variable = "httpResult"
            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpClient.SendAsync(Arg.Any<HttpRequestMessage>(), CancellationToken).Returns(httpResponseMessage);

            //Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            //Assert
            await HttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(
                    h => h.RequestUri.Equals(settings.Uri)), CancellationToken);

            await Context.Received(1).SetVariableAsync($"{settings.Variable}.status", ((int) HttpStatusCode.Accepted).ToString(),
                CancellationToken);
            await Context.Received(1).SetVariableAsync($"{settings.Variable}.body", "Some result", CancellationToken);
        }

        [Fact]
        public async Task ProcessPostActionWithoutValidSettingsShouldFailed()
        {
            //Arrange
            var settings = new ProcessHttpSettings
            {
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error")
            };

            HttpClient.SendAsync(Arg.Any<HttpRequestMessage>(), CancellationToken).Returns(httpResponseMessage);

            //Act
            try
            {
                await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);
                throw new Exception();
            }
            catch (ArgumentException exception)
            {
                //Assert
                await HttpClient.DidNotReceive().SendAsync(
                    Arg.Is<HttpRequestMessage>(
                        h => h.RequestUri.Equals(settings.Uri)), CancellationToken);
            }            
        }
    }
}
