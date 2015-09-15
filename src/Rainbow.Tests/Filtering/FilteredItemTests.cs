﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Rainbow.Filtering;

namespace Rainbow.Tests.Filtering
{
	public class FilteredItemTests
	{
		[Fact]
		public void FilteredItem_DoesNotContainExpectedSharedField()
		{
			Guid fieldId = Guid.NewGuid();

			var filter = new TestFieldFilter(fieldId);
			var testItem = new FakeItem(sharedFields: new[] { new FakeFieldValue("Fake", fieldId: fieldId), });

			var filteredItem = new FilteredItem(testItem, filter);

			Assert.False(filteredItem.SharedFields.Any(field => field.FieldId == fieldId));
		}

		[Fact]
		public void FilteredItem_ContainsExpectedSharedField()
		{
			Guid fieldId = Guid.NewGuid();

			var filter = new TestFieldFilter();
			var testItem = new FakeItem(sharedFields: new[] { new FakeFieldValue("Fake", fieldId: fieldId) });

			var filteredItem = new FilteredItem(testItem, filter);

			Assert.True(filteredItem.SharedFields.Any(field => field.FieldId == fieldId));
		}

		[Fact]
		public void FilteredItem_DoesNotContainExpectedVersionedField()
		{
			Guid fieldId = Guid.NewGuid();

			var filter = new TestFieldFilter(fieldId);
			var testItem = new FakeItem(versions: new[] { new FakeItemVersion(fields: new FakeFieldValue("Fake", fieldId: fieldId) )});

			var filteredItem = new FilteredItem(testItem, filter);

			Assert.False(filteredItem.Versions.First().Fields.Any(field => field.FieldId == fieldId));
		}

		[Fact]
		public void FilteredItem_ContainsExpectedVersionedField()
		{
			Guid fieldId = Guid.NewGuid();

			var filter = new TestFieldFilter();
			var testItem = new FakeItem(versions: new[] { new FakeItemVersion(fields: new FakeFieldValue("Fake", fieldId: fieldId)) });

			var filteredItem = new FilteredItem(testItem, filter);

			Assert.True(filteredItem.Versions.First().Fields.Any(field => field.FieldId == fieldId));
		}

		private class TestFieldFilter : IFieldFilter
		{
			private readonly HashSet<Guid> _fields;
			public TestFieldFilter(params Guid[] excludedFieldIds)
			{
				_fields = new HashSet<Guid>(excludedFieldIds);
			}

			public bool Includes(Guid fieldId)
			{
				return !_fields.Contains(fieldId);
			}
		}
	}
}
