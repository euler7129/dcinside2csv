namespace dcinsideLibrary
{
	public class Logger
	{
		private static Logger instance;
		private static readonly string filename = "logs.log";
		private Logger() { }

		public static Logger Instance
		{
			get
			{
				instance ??= new Logger();
				return instance;
			}
		}

		public void Log(string message)
		{
			Console.WriteLine(message);
			File.AppendAllText(filename, message + Environment.NewLine);
		}
	}
}
