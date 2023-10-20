namespace Snap2HTMLNG.Shared.Models
{
    public class CommandLineModel
    {
		private string _name;

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		private string _value;

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}
	}
}
