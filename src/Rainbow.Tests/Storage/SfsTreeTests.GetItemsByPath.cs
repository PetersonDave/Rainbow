﻿using System.Linq;
using Xunit;

namespace Rainbow.Tests.Storage
{
	partial class SfsTreeTests
	{
		[Fact]
		public void GetItemsByPath_ReturnsExpectedItem_WhenRootPathIsRequested_AndTreeIsAtRoot()
		{
			using (var testTree = new TestSfsTree())
			{
				CreateTestTree("/sitecore", testTree);

				var root = testTree.GetItemsByPath("/sitecore").ToArray();

				Assert.NotNull(root);
				Assert.NotEmpty(root);
				Assert.Equal(root.First().Name, "sitecore");
			}
		}

		[Fact]
		public void GetItemsByPath_ReturnsExpectedItem_WhenRootPathIsRequested_AndTreeIsNested()
		{
			using (var testTree = new TestSfsTree("/sitecore/templates"))
			{
				CreateTestTree("/sitecore/templates", testTree);

				var root = testTree.GetItemsByPath("/sitecore/templates").ToArray();

				Assert.NotNull(root);
				Assert.NotEmpty(root);
				Assert.Equal(root.First().Name, "templates");
			}
		}

		[Fact]
		public void GetItemsByPath_ReturnsExpectedItem_WhenChildPathIsRequested_AndTreeIsAtRoot()
		{
			using (var testTree = new TestSfsTree())
			{
				CreateTestTree("/sitecore/templates/User Defined", testTree);

				var root = testTree.GetItemsByPath("/sitecore/templates/User Defined").ToArray();

				Assert.NotNull(root);
				Assert.NotEmpty(root);
				Assert.Equal(root.First().Name, "User Defined");
			}
		}

		[Fact]
		public void GetItemsByPath_ReturnsExpectedItem_WhenChildPathIsRequested_AndTreeIsAtRoot_CaseInsensitive()
		{
			using (var testTree = new TestSfsTree())
			{
				CreateTestTree("/sitecore/templates/User Defined", testTree);

				testTree.ClearAllCaches();

				var root = testTree.GetItemsByPath("/sitecore/templates/uSer dEfiNed").ToArray();

				Assert.NotNull(root);
				Assert.NotEmpty(root);
				Assert.Equal(root.First().Name, "User Defined");
			}
		}

		[Fact]
		public void GetItemsByPath_ReturnsExpectedItem_WhenChildPathIsRequested_AndTreeIsNested()
		{
			using (var testTree = new TestSfsTree("/sitecore/templates"))
			{
				CreateTestTree("/sitecore/templates/User Defined", testTree);

				var root = testTree.GetItemsByPath("/sitecore/templates/User Defined").ToArray();

				Assert.NotNull(root);
				Assert.NotEmpty(root);
				Assert.Equal(root.First().Name, "User Defined");
			}
		}

		[Fact]
		public void GetItemsByPath_ReturnsExpectedItem_WhenChildPathIsRequested_AndNamesContainInvalidPathChars()
		{
			using (var testTree = new TestSfsTree("/?hello*"))
			{
				CreateTestTree("/?hello*/%there%", testTree);

				var root = testTree.GetItemsByPath("/?hello*/%there%").ToArray();

				Assert.NotNull(root);
				Assert.NotEmpty(root);
				Assert.Equal(root.First().Name, "%there%");
			}
		}

		[Fact]
		public void GetItemsByPath_ReturnsExpectedItems_WhenChildPathIsRequested_AndMultipleMatchesExist()
		{
			using (var testTree = new TestSfsTree())
			{
				const string treePath = "/sitecore/templates/User Defined";
				CreateTestTree(treePath, testTree);

				var testItem = testTree.GetItemsByPath(treePath);

				// add a second User Defined item
				testTree.Save(CreateTestItem(treePath, testItem.First().ParentId));

				var results = testTree.GetItemsByPath(treePath).ToArray();

				Assert.Equal(2, results.Length);
				Assert.NotEqual(results[0].Id, results[1].Id);
				Assert.NotEqual(results[0].SerializedItemId, results[1].SerializedItemId);
			}
		}

		[Fact]
		public void GetItemsByPath_ReturnsExpectedItems_WhenChildPathIsRequested_AndMultipleMatchesExist_ThroughSeparateParents()
		{
			using (var testTree = new TestSfsTree())
			{
				const string treePath = "/sitecore/templates/User Defined";

				CreateTestTree(treePath, testTree);

				var testItem = testTree.GetItemsByPath("/sitecore/templates");

				var templates1 = testItem.First();

				// add a second Templates item
				var templates2 = CreateTestItem("/sitecore/templates", templates1.ParentId);
				testTree.Save(templates2);

				// add a child under the second templates item, giving us '/sitecore/templates/User Defined' under templates1, and '/sitecore/templates/Evil' under templates2
				// P.S. don't actually do this in real life. Please? But I'm testing it, because I'm an effing pedant :)
				testTree.Save(CreateTestItem("/sitecore/templates/Evil", templates2.Id));

				var results = testTree.GetItemsByPath("/sitecore/templates").ToArray();

				Assert.Equal(2, results.Length);
				Assert.NotEqual(results[0].Id, results[1].Id);
				Assert.NotEqual(results[0].SerializedItemId, results[1].SerializedItemId);
				Assert.True(results.Any(result => result.Id == templates1.Id));
				Assert.True(results.Any(result => result.Id == templates2.Id));
			}
		}

		[Fact]
		public void GetItemByPath_GetsExpectedItem_WhenItemNameIsTooLong()
		{
			using (var testTree = new TestSfsTree())
			{
				// force the tree to shorten after 10 char names
				testTree.MaxFileNameLengthForTests = 10;
				CreateTestTree("/sitecore/hello hello", testTree);

				var overlengthItem = testTree.GetItemsByPath("/sitecore/hello hello").ToArray();

				Assert.Equal(1, overlengthItem.Count());

				Assert.Equal("/sitecore/hello hello", overlengthItem.First().Path);
			}
		}

		[Fact]
		public void GetItemByPath_GetsExpectedItem_WhenPathIsAChildOfShortenedName()
		{
			using (var testTree = new TestSfsTree())
			{
				// force the tree to shorten after 10 char names
				testTree.MaxFileNameLengthForTests = 10;
				CreateTestTree("/sitecore/hello hello/goodbye", testTree);

				var overlengthChild = testTree.GetItemsByPath("/sitecore/hello hello/goodbye").ToArray();

				Assert.Equal(1, overlengthChild.Count());

				Assert.Equal("/sitecore/hello hello/goodbye", overlengthChild.First().Path);
			}
		}

		[Fact]
		public void GetItemByPath_GetsExpectedItem_WhenItemNameIsTooLong_AndItemsWithSameShortenedNameExist()
		{
			using (var testTree = new TestSfsTree())
			{
				// force the tree to shorten after 10 char names
				testTree.MaxFileNameLengthForTests = 10;
				CreateTestTree("/sitecore/hello hello", testTree);

				testTree.Save(CreateTestItem("/sitecore/hello hello hello", testTree.GetRootItem().Id));

				var overlengthItem = testTree.GetItemsByPath("/sitecore/hello hello").ToArray();

				Assert.Equal(1, overlengthItem.Count());
				Assert.Equal("/sitecore/hello hello", overlengthItem.First().Path);
			}
		}
	}
}
