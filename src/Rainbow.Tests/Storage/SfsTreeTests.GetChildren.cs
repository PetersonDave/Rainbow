﻿using System.Linq;
using Xunit;

namespace Rainbow.Tests.Storage
{
	partial class SfsTreeTests
	{
		[Fact]
		public void GetChildren_ReturnsExpectedItem_WhenRootPathIsParent_AndTreeIsAtRoot()
		{
			using (var testTree = new TestSfsTree())
			{
				CreateTestTree("/sitecore/templates", testTree);

				var root = testTree.GetRootItem();

				var children = testTree.GetChildren(root).ToArray();

				Assert.NotNull(children);
				Assert.Equal(1, children.Length);
				Assert.Equal(children[0].Name, "templates");
			}
		}

		[Fact]
		public void GetChildren_ReturnsExpectedItem_WhenRootPathIsParent_AndTreeIsNested()
		{
			using (var testTree = new TestSfsTree("/sitecore/templates"))
			{
				CreateTestTree("/sitecore/templates/User Defined", testTree);

				var root = testTree.GetItemsByPath("/sitecore/templates").First();

				var children = testTree.GetChildren(root).ToArray();

				Assert.NotNull(children);
				Assert.Equal(1, children.Length);
				Assert.Equal(children[0].Name, "User Defined");
			}
		}

		[Fact]
		public void GetChildren_ReturnsExpectedItem_WhenRootPathIsParent_AndTreeIsNested_AndCacheIsCleared()
		{
			using (var testTree = new TestSfsTree("/sitecore/templates"))
			{
				CreateTestTree("/sitecore/templates/User Defined", testTree);

				var root = testTree.GetItemsByPath("/sitecore/templates").First();

				testTree.ClearAllCaches();

				var children = testTree.GetChildren(root).ToArray();

				Assert.NotNull(children);
				Assert.Equal(1, children.Length);
				Assert.Equal(children[0].Name, "User Defined");
			}
		}

		[Fact]
		public void GetChildren_ReturnsExpectedItems_WhenMultipleMatchesExist()
		{
			using (var testTree = new TestSfsTree())
			{
				const string treePath = "/sitecore/templates";
				CreateTestTree(treePath, testTree);

				var testItem = testTree.GetItemsByPath(treePath);

				// add a second child item
				testTree.Save(CreateTestItem("/sitecore/system", testItem.First().ParentId));

				// get the children of the root, which should include the two items
				var results = testTree.GetChildren(testTree.GetRootItem()).ToArray();

				Assert.Equal(2, results.Length);
				Assert.NotEqual(results[0].Id, results[1].Id);
				Assert.NotEqual(results[0].SerializedItemId, results[1].SerializedItemId);
				Assert.True(results.Any(result => result.Name == "templates"));
				Assert.True(results.Any(result => result.Name == "system"));
			}
		}

		[Fact]
		public void GetChildren_ReturnsExpectedItems_WhenMultipleSameNamedMatchesExist()
		{
			using (var testTree = new TestSfsTree())
			{
				const string treePath = "/sitecore/templates";
				CreateTestTree(treePath, testTree);

				var testItem = testTree.GetItemsByPath(treePath);

				// add a second templates item
				testTree.Save(CreateTestItem(treePath, testItem.First().ParentId));

				// get the children of the root, which should include the two same named items
				var results = testTree.GetChildren(testTree.GetRootItem()).ToArray();

				Assert.Equal(2, results.Length);
				Assert.NotEqual(results[0].Id, results[1].Id);
				Assert.NotEqual(results[0].SerializedItemId, results[1].SerializedItemId);
			}
		}

		[Fact]
		public void GetChildren_ReturnsExpectedItems_WhenNameTruncationCausesSimilarNames()
		{
			using (var testTree = new TestSfsTree())
			{
				testTree.MaxFileNameLengthForTests = 10;

				const string treePath = "/sitecore/templates";
				CreateTestTree(treePath, testTree);

				var testItem = testTree.GetItemsByPath(treePath).First();

				var multilist = CreateTestItem("/sitecore/templates/Multilist", testItem.Id);
				var multilistWithSearch = CreateTestItem("/sitecore/templates/Multilist with Search", testItem.Id);

				var multilistChild = CreateTestItem("/sitecore/templates/Multilist/Menu", multilist.Id);
				var multilistWithSearchChild = CreateTestItem("/sitecore/templates/Multilist with Search/Menu", multilistWithSearch.Id);

				testTree.Save(multilist);
				testTree.Save(multilistWithSearch);
				testTree.Save(multilistChild);
				testTree.Save(multilistWithSearchChild);

				// now we'll have "Multilist.yml" and "Multilist .yml" - make sure we get the right children from each

				// get the children of the root, which should include the two same named items
				var multilistChildren = testTree.GetChildren(multilist).ToArray();
				var multilistWithSearchChildren = testTree.GetChildren(multilistWithSearch).ToArray();

				Assert.Equal(1, multilistChildren.Length);
				Assert.Equal(multilistChild.Id, multilistChildren.First().Id);
				Assert.Equal(1, multilistWithSearchChildren.Length);
				Assert.Equal(multilistWithSearchChild.Id, multilistWithSearchChildren.First().Id);
			}
		}

		[Fact]
		public void GetChildren_ReturnsExpectedItems_WhenMultipleMatchesExist_ThroughSeparateParents()
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

				// get the children of templates1, which should NOT include templates2's child
				var results = testTree.GetChildren(templates1).ToArray();

				Assert.Equal(1, results.Length);
				Assert.Equal("User Defined", results[0].Name);
			}
		}

		[Fact]
		public void GetChildren_ReturnsEmptyEnumerable_WhenNoChildrenExist()
		{
			using (var testTree = new TestSfsTree())
			{
				CreateTestTree("/sitecore", testTree);

				// get the children of the root, which be empty
				var results = testTree.GetChildren(testTree.GetRootItem());

				Assert.NotNull(results);
				Assert.Empty(results);
			}
		}

		[Fact]
		public void GetChildren_ReturnsExpectedItem_WhenNamesContainInvalidPathChars()
		{
			using (var testTree = new TestSfsTree("/<html>"))
			{
				CreateTestTree("/<html>/$head", testTree);

				var root = testTree.GetRootItem();

				var children = testTree.GetChildren(root).ToArray();

				Assert.NotNull(children);
				Assert.Equal(1, children.Length);
				Assert.Equal(children[0].Name, "$head");
			}
		}
	}
}
