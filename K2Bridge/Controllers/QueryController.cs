﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Controllers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using K2Bridge.HttpMessages;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models.Response;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Handles requests to ADX.
    /// </summary>
    [Route("")]
    [ApiController]
    public class QueryController : ControllerBase
    {
        private readonly IQueryExecutor queryExecutor;
        private readonly ITranslator translator;
        private readonly ILogger<QueryController> logger;
        private readonly IResponseParser responseParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryController"/> class.
        /// </summary>
        /// <param name="queryExecutor"></param>
        /// <param name="translator"></param>
        /// <param name="logger"></param>
        public QueryController(
            IQueryExecutor queryExecutor,
            ITranslator translator,
            ILogger<QueryController> logger,
            IResponseParser responseParser)
        {
            this.queryExecutor = queryExecutor ?? throw new ArgumentNullException(nameof(queryExecutor));
            this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.responseParser = responseParser ?? throw new ArgumentNullException(nameof(responseParser));
        }

        /// <summary>
        /// Perform a Kibana search query against the data backend.
        /// </summary>
        /// <param name="totalHits"></param>
        /// <param name="ignoreThrottled"></param>
        /// <returns>An ElasticResponse object or a passthrough object if an error occured.</returns>
        [HttpPost(template: "_msearch")]
        [Consumes("application/json", "application/x-ndjson")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ElasticResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HttpResponseMessageResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(
            [FromQuery(Name = "rest_total_hits_as_int")] bool totalHits,
            [FromQuery(Name = "ignore_throttled")] bool ignoreThrottled)

        // Model binding does not work as the application/json message contains an application/nd-json payload
        // once Kibana sends the right ContentType the line below can be commented in.
        // [FromBody] IEnumerable<string> rawQueryData
        {
            var reqStrings = await this.ExtractRequestStrings(this.Request);
            return await this.SearchInternal(totalHits, ignoreThrottled, reqStrings);
        }

        /// <summary>
        /// Internal implementation of the search API logic.
        /// Mainly used to improve testability (as certain parameters needs to be extracted from the body).
        /// </summary>
        /// <param name="totalHits"></param>
        /// <param name="ignoreThrottled"></param>
        /// <param name="rawQueryData">Body Payload.</param>
        /// <returns>An ElasticResponse object.</returns>
        internal async Task<IActionResult> SearchInternal(bool totalHits, bool ignoreThrottled, string[] rawQueryData)
        {
            if (rawQueryData == null || rawQueryData.Length < 2)
            {
                throw new ArgumentException("Invalid request payload", nameof(rawQueryData));
            }

            this.logger.LogDebug($"Elastic search request:\n{rawQueryData[0]}\n{rawQueryData[1]}");
            var translatedResponse = this.translator.Translate(rawQueryData[0], rawQueryData[1]);
            this.logger.LogDebug($"Translated query:\n{translatedResponse.KQL}");

            var (timeTaken, dataReader) = this.queryExecutor.ExecuteQuery(translatedResponse);
            var elasticResponse = this.responseParser.ParseElasticResponse(dataReader, translatedResponse, timeTaken);

            return this.Ok(elasticResponse);
        }

        /// <summary>
        /// Extracts the request and request metadata strings from the HttpRequest object.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<string[]> ExtractRequestStrings(HttpRequest request)
        {
            string[] lst = null;
            using (var reader = new StreamReader(request.Body))
            {
                var content = await reader.ReadToEndAsync();
                if (!string.IsNullOrEmpty(content))
                {
                    lst = content.Split(
                        new[] { "\r\n", "\r", "\n" },
                        StringSplitOptions.RemoveEmptyEntries);
                }
            }

            return lst;
        }
    }
}