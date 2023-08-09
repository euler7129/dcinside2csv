using AngleSharp.Html.Parser;
using AngleSharp.XPath;
using CsvHelper;
using System.Globalization;

ConsoleApp.Run<MyCommands>(args);

public class MyCommands : ConsoleAppBase
{
	[RootCommand]
	public void RootCommand(
		[Option("i", "Directory path containing input HTML files")] string inputDirPath,
		[Option("p", "Directory path of the output data")] string outputDirPath)
	{// -i "D:\temp\dcinside\gallery" -p "D:\temp\dcinside\gallery_csv"
		var arguments = Context.Arguments;
		Console.WriteLine($"Executing with arguments: \"{string.Join(" ", arguments)}\"");
		Console.WriteLine("dcinside2csv has been started.");

		// Each HTML file will generate BlogPost, Comment, and Image objects.
		dcinside2csv(inputDirPath, outputDirPath);
	}

	private void dcinside2csv(string inputDirPath, string outputDirPath)
	{
		// Get all HTML files in the input directory.
		var htmlFiles = Directory.GetFiles(inputDirPath, "*.html", SearchOption.AllDirectories);
		Console.WriteLine($"Found {htmlFiles.Length} HTML files.");
		// Using AngleSharp to parse HTML files.
		var parser = new HtmlParser();
		// Using CsvHelper to write CSV files.
		foreach ( var htmlFile in htmlFiles )
		{
			Console.WriteLine($"Parsing {htmlFile}...");
			var html = File.ReadAllText(htmlFile);
			var document = parser.ParseDocument(html);
			// Narrow down the scope to the article node.
			//var articleNode = document.Body.SelectSingleNode("//*[@id=\"container\"]/section/article[2]");

			// Get element by class name
			var category = document.QuerySelector(".title_headtext").TextContent;
			var subject = document.QuerySelector(".title_subject").TextContent;
			var author = document.Body.SelectSingleNode("//*[@id=\"container\"]/section/article[2]/div[1]/header/div/div/div[1]/span[1]/a/em").TextContent;
			// Get write_div div element
			var writeDiv = document.QuerySelector("div.write_div");
			// Filter only HtmlDivElement elements in writeDiv
			var divs = writeDiv.Children.Where(x => x is AngleSharp.Html.Dom.IHtmlDivElement).Cast<AngleSharp.Html.Dom.IHtmlDivElement>();

			// Save each div element's InnerHtml to a text file
			int index = 0;
			foreach ( var div in divs )
			{
				var divHtml = div.InnerHtml;
				var divHtmlFilePath = Path.Combine(outputDirPath, $"{div.Id}{index++}.html");
				File.WriteAllText(divHtmlFilePath, divHtml);
			}

			break;
		}
	}
}