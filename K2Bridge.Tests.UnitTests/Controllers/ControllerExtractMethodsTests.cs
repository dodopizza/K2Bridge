// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Controllers
{
    using System;
    using global::K2Bridge.Controllers;
    using NUnit.Framework;

    [TestFixture]
    public class ControllerExtractMethodsTests
    {
        private static readonly object[] PartitionStringTestCases = {
            new TestCaseData("header\r\nquery").Returns(("header", "query")).SetName("SplitQueryBody_WithSlashNSlashR_PartitionsStringsCorrectly"),
            new TestCaseData("header\rquery").Returns(("header", "query")).SetName("SplitQueryBody_WithSlashR_PartitionsStringsCorrectly"),
            new TestCaseData("header\nquery").Returns(("header", "query")).SetName("SplitQueryBody_WithSlashN_PartitionsStringsCorrectly"),
            new TestCaseData("headerquery").Returns(ValueTuple.Create<string, string>("headerquery", null)).SetName("SplitQueryBody_WhenNoCharacters_DoesNotPartitionsStrings"),
            new TestCaseData("header\nquery\nnotheader\nnotquery").Returns(("header", "query")).SetName("SplitQueryBody_WithMorePartitions_Ignores"),
            new TestCaseData("header").Returns(ValueTuple.Create<string, string>("header", null)).SetName("SplitQueryBody_WithLessPartitions_NoError"),
        };

        private static readonly object[] TemplateReplaceStringTestCases = {
            new TestCaseData("/_template/kibana_index_template:.kibana?include_type_name=true").Returns("/_template/kibana_index_template::kibana?include_type_name=true").SetName("ReplaceTemplateString_WithValidInput_ReplaceTokens"),
            new TestCaseData("/_template/kibana_index_template/kibana?include_type_name=true").Returns("/_template/kibana_index_template/kibana?include_type_name=true").SetName("ReplaceTemplateString_WithValidTemplateNoToken_NoAction"),
            new TestCaseData("/kibana_index_template:.kibana?include_type_name=true").Returns("/kibana_index_template:.kibana?include_type_name=true").SetName("ReplaceTemplateString_WithNoTemplate_NoAction"),
            new TestCaseData(string.Empty).Returns(string.Empty).SetName("ReplaceTemplateString_WithEmptyString_NoError"),
            new TestCaseData(null).Returns(null).SetName("ReplaceTemplateString_WithNullString_NoError"),
        };

        private static readonly object[] TemplateReplaceBackStringTestCases = {
            new TestCaseData("/_template/kibana_index_template::kibana?include_type_name=true").Returns("/_template/kibana_index_template:.kibana?include_type_name=true").SetName("ReplaceTemplateString_WithValidInput_ReplaceTokens"),
            new TestCaseData("/_template/kibana_index_template/kibana?include_type_name=true").Returns("/_template/kibana_index_template/kibana?include_type_name=true").SetName("ReplaceTemplateString_WithValidTemplateNoToken_NoAction"),
            new TestCaseData("/kibana_index_template::kibana?include_type_name=true").Returns("/kibana_index_template::kibana?include_type_name=true").SetName("ReplaceTemplateString_WithNoTemplate_NoAction"),
            new TestCaseData(string.Empty).Returns(string.Empty).SetName("ReplaceTemplateString_WithEmptyString_NoError"),
            new TestCaseData(null).Returns(null).SetName("ReplaceTemplateString_WithNullString_NoError"),
        };

        [TestCaseSource(nameof(PartitionStringTestCases))]
        public (string, string) ExtractHeaderQueryTests(string input)
        {
            return ControllerExtractMethods.SplitQueryBody(input);
        }

        [TestCaseSource(nameof(TemplateReplaceStringTestCases))]
        public string ReplaceTemplateStringTests(string input)
        {
            return ControllerExtractMethods.ReplaceTemplateString(input);
        }

        [TestCaseSource(nameof(TemplateReplaceBackStringTestCases))]
        public string ReplaceBackTemplateStringTests(string input)
        {
            return ControllerExtractMethods.ReplaceBackTemplateString(input);
        }
    }
}
