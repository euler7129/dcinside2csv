using CsvHelper;
using System.Globalization;

public class MyCommands : ConsoleAppBase
{
	[RootCommand]
	public void RootCommand(
		[Option("i", "Directory path containing input HTML files")] string inputDirPath,
		[Option("p", "Directory path of the output data")] string outputDirPath)
	{// -i "C:\temp\dcinside\gallery" -p "C:\temp\dcinside\gallery_csv"
		var arguments = Context.Arguments;
		Console.WriteLine($"Executing with arguments: \"{string.Join(" ", arguments)}\"");
		Console.WriteLine("dcinside2csv has been started.");

		// Each HTML file will generate BlogPost, Comment, and Image objects.
		dcinside2csv(inputDirPath, outputDirPath);
	}

	private void dcinside2csv(string inputDirPath, string outputDirPath)
	{
		throw new NotImplementedException();
	}
}