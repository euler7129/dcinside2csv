namespace dcinsideLibrary
{
	public class Logger
	{
		private static Logger instance;
		private static readonly DateTime now = DateTime.Now;
		private static readonly string filename = $"logs-{now.ToString("yy-MM-dd-HH-mm-ss")}.log";
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
