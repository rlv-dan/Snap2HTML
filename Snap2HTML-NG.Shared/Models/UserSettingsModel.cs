using System.Runtime.CompilerServices;

namespace Snap2HTMLNG.Shared.Models
{
    public class UserSettingsModel
    {
		private string _rootDirectory;

		public string RootDirectory
		{
			get { return _rootDirectory; }
			set { _rootDirectory = value; }
		}

		private string _title;

		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		private string _outputFile;

		public string OutputFile
		{
			get { return _outputFile; }
			set { _outputFile = value; }
		}

		private bool _skipHiddenItems;

		public bool SkipHiddenItems
		{
			get { return _skipHiddenItems; }
			set { _skipHiddenItems = value; }
		}

		private bool _skipSystemItems;

		public bool SkipSystemItems
		{
			get { return _skipSystemItems; }
			set { _skipSystemItems = value; }
		}

		private bool _openInBrowserAfterCapture;

		public bool OpenInBrowserAfterCapture
		{
			get { return _openInBrowserAfterCapture; }
			set { _openInBrowserAfterCapture = value; }
		}

		private bool _linkFiles;

		public bool LinkFiles
		{
			get { return _linkFiles; }
			set { _linkFiles = value; }
		}

		private string _linkRoot;

		public string LinkRoot
		{
			get { return _linkRoot; }
			set { _linkRoot = value; }
		}

		private string _searchPattern;

		public string SearchPattern
		{
			get { return _searchPattern; }
			set { _searchPattern = value; }
		}

	}
}
