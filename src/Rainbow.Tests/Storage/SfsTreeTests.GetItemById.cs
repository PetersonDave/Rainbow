﻿using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Rainbow.Tests.Storage
{
	partial class SfsTreeTests
	{
		[Test]
		public void GetItemById_ResolvesItem_WhenItemIsRoot()
		{
			using (var testTree = new TestSfsTree())
			{
				CreateTestTree("/sitecore", testTree);

				var root = testTree.GetRootItem();

				var byId = testTree.GetItemById(root.Id);

				Assert.IsNotNull(byId);
				Assert.AreEqual(root.Id, byId.Id);
			}
		}

		[Test]
		public void GetItemById_ResolvesItem_WhenItemIsChild()
		{
			using (var testTree = new TestSfsTree())
			{
				CreateTestTree("/sitecore/content/foo", testTree);

				var item = testTree.GetItemsByPath("/sitecore/content/foo").First();

				var byId = testTree.GetItemById(item.Id);

				Assert.IsNotNull(byId);
				Assert.AreEqual(item.Id, byId.Id);
			}
		}

		[Test]
		public void GetItemById_ResolvesItem_WhenItemIsRoot_AndCacheIsEmpty()
		{
			using (var testTree = new TestSfsTree())
			{
				CreateTestTree("/sitecore", testTree);

				var root = testTree.GetRootItem();

				testTree.ClearCaches();

				var byId = testTree.GetItemById(root.Id);

				Assert.IsNotNull(byId);
				Assert.AreEqual(root.Id, byId.Id);
			}
		}

		[Test]
		public void GetItemById_ResolvesItem_WhenItemIsChild_AndCacheIsEmpty()
		{
			using (var testTree = new TestSfsTree())
			{
				CreateTestTree("/sitecore/content/foo", testTree);

				var item = testTree.GetItemsByPath("/sitecore/content/foo").First();

				testTree.ClearCaches();

				var byId = testTree.GetItemById(item.Id);

				Assert.IsNotNull(byId);
				Assert.AreEqual(item.Id, byId.Id);
			}
		}
	}
}
