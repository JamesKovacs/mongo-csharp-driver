/* Copyright 2017-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver.Core.Configuration;
using Xunit;
using MongoDB.Bson.TestHelpers.XunitExtensions;
using System.Collections;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace MongoDB.Driver.Specifications.initial_dns_seedlist_discovery
{
    [Trait("Category", "ConnectionString")]
    [Trait("Category", "SupportLoadBalancing")]
    public class InitialDnsSeedlistDiscoveryTestRunner
    {
        [Theory]
        [ClassData(typeof(TestCaseFactory))]
        public void RunTestDefinition(TestCase testCase)
        {
            ConnectionString connectionString = null;
            Exception resolveException = Record.Exception(() => connectionString = new ConnectionString((string)testCase.Definition["uri"]).Resolve());
            Assert(connectionString, resolveException, testCase.Definition);
        }

        [Theory]
        [ClassData(typeof(TestCaseFactory))]
        public async Task RunTestDefinitionAsync(TestCase testCase)
        {
            ConnectionString connectionString = null;
            Exception resolveException = await Record.ExceptionAsync(async () => connectionString = await new ConnectionString((string)testCase.Definition["uri"]).ResolveAsync());
            Assert(connectionString, resolveException, testCase.Definition);
        }

        private void Assert(ConnectionString connectionString, Exception resolveException, BsonDocument definition)
        {
            if (definition.GetValue("error", false).ToBoolean())
            {
                resolveException.Should().BeOfType<MongoConfigurationException>();
            }
            else
            {
                if (resolveException != null)
                {
                    throw new AssertionException("failed to parse and resolve connection string", resolveException);
                }

                AssertValid(connectionString, definition);
            }
        }

        private void AssertValid(ConnectionString connectionString, BsonDocument definition)
        {
            if (definition.Contains("seeds"))
            {
                var expectedSeeds = definition["seeds"].AsBsonArray.Select(x => x.ToString()).ToList();
                var actualSeeds = connectionString.Hosts.Select(ConvertEndPointToString).ToList();

                actualSeeds.ShouldAllBeEquivalentTo(expectedSeeds);
            }

            if (definition.Contains("numSeeds"))
            {
                var numExpectedSeeds = definition["numSeeds"].AsInt32;
                var numActualSeeds = connectionString.Hosts.Count;

                numActualSeeds.Should().Be(numExpectedSeeds);
            }

            if (definition.Contains("options"))
            {
                foreach (BsonElement option in definition["options"].AsBsonDocument)
                {
                    var expectedValue = ValueToString(option.Name, option.Value);

                    var optionName = GetOptionName(option);
                    var actualValue = Uri.UnescapeDataString(connectionString.GetOption(optionName).Split(',').Last());

                    actualValue.Should().Be(expectedValue);
                }
            }
        }

        private string GetOptionName(BsonElement option)
        {
            switch (option.Name.ToLowerInvariant())
            {
                case "ssl" when option.Value == true:
                    // Needs to restore some json tests which expect "ssl=true" option as autogenerated option for mongo+srv if no user defined "ssl" options, but now, we generate "tls=true"
                    return "tls";
                default:
                    return option.Name;
            }
        }

        private string ValueToString(string name, BsonValue value)
        {
            if (value == BsonNull.Value)
            {
                return null;
            }

            return value.ToString();
        }

        private string ConvertEndPointToString(EndPoint ep)
        {
            if (ep is DnsEndPoint)
            {
                var dep = (DnsEndPoint)ep;
                return $"{dep.Host}:{dep.Port}";
            }
            else if (ep is IPEndPoint)
            {
                var iep = (IPEndPoint)ep;
                return $"{iep.Address}:{iep.Port}";
            }

            throw new AssertionException($"Invalid endpoint: {ep}");
        }

        public class TestCase : IXunitSerializable
        {
            public BsonDocument Definition;
            public string Name;

            public void Deserialize(IXunitSerializationInfo info)
            {
                Definition = BsonDocument.Parse(info.GetValue<string>("Definition"));
                Name = info.GetValue<string>("Name");
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue("Definition", Definition.ToString());
                info.AddValue("Name", Name);
            }

            public override string ToString()
            {
                return Name + "(" + Definition["uri"].ToString() + ")";
            }
        }

        private class TestCaseFactory : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                const string prefix = "MongoDB.Driver.Core.Tests.Specifications.initial_dns_seedlist_discovery.tests.";
                var executingAssembly = typeof(TestCaseFactory).GetTypeInfo().Assembly;
                return executingAssembly
                    .GetManifestResourceNames()
                    .Where(path => path.StartsWith(prefix) && path.EndsWith(".json"))
                    .Select(path =>
                    {
                        var test = ReadDefinition(path);
                        var fullName = path.Remove(0, prefix.Length);
                        var testName = fullName.Remove(fullName.Length - 5);
                        return new object[] { new TestCase { Name = testName, Definition = test } };
                    }).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private static BsonDocument ReadDefinition(string path)
            {
                var executingAssembly = typeof(TestCaseFactory).GetTypeInfo().Assembly;
                using (var definitionStream = executingAssembly.GetManifestResourceStream(path))
                using (var definitionStringReader = new StreamReader(definitionStream))
                {
                    var definitionString = definitionStringReader.ReadToEnd();
                    return BsonDocument.Parse(definitionString);
                }
            }
        }
    }
}
