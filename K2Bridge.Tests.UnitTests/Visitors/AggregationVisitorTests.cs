﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class AggregationVisitorTests
    {
        [TestCase(ExpectedResult = null)]
        public string AggregationVisit_WithEmptyAgg_ReturnsNoPrimary()
        {
            var aggregateClause = new Aggregation();

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "avg(fieldA)")]
        public string AggregationVisit_WithAvgAgg_ReturnsAvgPrimary()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new AvgAggregation() { FieldName = "fieldA" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "avg(fieldB), avg(fieldA)")]
        public string AggregationVisit_WithSubAvgAgg_ReturnsAvgAggregates()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new AvgAggregation() { FieldName = "fieldA" },
                SubAggregations = new Dictionary<string, Aggregation>
                {
                    { "sub", new Aggregation() { PrimaryAggregation = new AvgAggregation { FieldName = "fieldB" } } },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "dcount(fieldA)")]
        public string AggregationVisit_WithCardinalityAgg_ReturnsDCount()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new CardinalityAggregation() { FieldName = "fieldA" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "dcount(fieldB), dcount(fieldA)")]
        public string AggregationVisit_WithSubCardinalityAggs_ReturnsDCounts()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new CardinalityAggregation() { FieldName = "fieldA" },
                SubAggregations = new Dictionary<string, Aggregation>
                {
                    { "sub", new Aggregation() { PrimaryAggregation = new CardinalityAggregation { FieldName = "fieldB" } } },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }
    }
}
