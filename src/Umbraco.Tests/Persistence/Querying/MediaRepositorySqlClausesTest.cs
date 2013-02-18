﻿using System;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class MediaRepositorySqlClausesTest : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Verify_Base_Clause()
        {
            var NodeObjectTypeId = new Guid("b796f64c-1f99-4ffb-b886-4bf4bc011a9c");

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsContentVersion]")
                .InnerJoin("[cmsContent]").On("[cmsContentVersion].[ContentId] = [cmsContent].[nodeId]")
                .InnerJoin("[umbracoNode]").On("[cmsContent].[nodeId] = [umbracoNode].[id]")
                .Where("[umbracoNode].[nodeObjectType] = 'b796f64c-1f99-4ffb-b886-4bf4bc011a9c'");

            var sql = new Sql();
            sql.Select("*")
                .From<ContentVersionDto>()
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }
    }
}