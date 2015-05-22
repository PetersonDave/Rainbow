﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gibson.Indexing;

namespace Gibson.Storage.Pathing
{
	public class IdBasedPathProvider : IFileSystemPathProvider
	{
		public string IndexFileName { get; set; }

		private string _fileExtension;
		public string FileExtension
		{
			get
			{
				return _fileExtension;
			}
			set
			{
				if(!Regex.IsMatch(value, "^\\.[a-zA-Z0-9]+$")) throw new InvalidOperationException("The file extension must start with a dot and be alphanumeric (e.g. '.json' but not 'json' or '.$5@#'");

				_fileExtension = value;
			}
		}

		public string GetIndexStoragePath(string database, string rootPath)
		{
			return Path.Combine(rootPath, database, IndexFileName);
		}

		public string GetStoragePath(IndexEntry indexData, string database, string rootPath)
		{
			var paths = new Stack<string>(4);
			paths.Push(indexData.Id.ToString("D").ToUpperInvariant() + FileExtension);
			paths.Push(indexData.Id.ToString("N").Substring(0, 1).ToUpperInvariant());
			paths.Push(database);
			paths.Push(rootPath);

			return Path.Combine(paths.ToArray());
		}

		public IEnumerable<string> GetAllStoredPaths(string rootPath, string database)
		{
			var dbPath = GetDatabasePath(database, rootPath);
			return Directory.GetFiles(dbPath, "*" + FileExtension, SearchOption.AllDirectories);
		}

		public IEnumerable<string> GetOrphans(string rootPath, string database)
		{
			var dbPath = GetDatabasePath(database, rootPath);

			var children = Directory.EnumerateDirectories(dbPath);

			return children.Where(child => !Directory.EnumerateFileSystemEntries(child).Any());
		}

		public IEnumerable<string> GetAllStoredDatabaseNames(string rootPath)
		{
			return Directory.GetDirectories(rootPath).Select(Path.GetDirectoryName);
		}

		protected string GetDatabasePath(string database, string rootPath)
		{
			return Path.Combine(rootPath, database);
		}
	}
}