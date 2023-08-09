﻿using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.XPath;
using CsvHelper;
using dcinside2csv.Model;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

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
		dcinside2csv(inputDirPath, outputDirPath, "https://example.com/wp/");
	}

	private void dcinside2csv(string inputDirPath, string outputDirPath, string blogHome)
	{
		string imageDirPath = Path.Combine(outputDirPath, "images");

		List<GalleryPost> posts = new List<GalleryPost>();
		// Get all HTML files in the input directory.
		var htmlFiles = Directory.GetFiles(inputDirPath, "*.html", SearchOption.AllDirectories);
		Console.WriteLine($"Found {htmlFiles.Length} HTML files.");
		// Using AngleSharp to parse HTML files.
		var parser = new HtmlParser();
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
			var date = document.QuerySelector("span.gall_date").GetAttribute("title");
			var postId = getPostId(document, date);
			var author = document.Body.SelectSingleNode("//*[@id=\"container\"]/section/article[2]/div[1]/header/div/div/div[1]/span[1]/a/em").TextContent;
			// Get write_div div element
			var writeDiv = document.QuerySelector("div.write_div");
			// Filter only HtmlDivElement elements in writeDiv
			var divs = writeDiv.Children.Where(x => x is AngleSharp.Html.Dom.IHtmlDivElement).Cast<AngleSharp.Html.Dom.IHtmlDivElement>();

			// Save each div element's InnerHtml to a text file
			int index = 0;
			List<IHtmlElement> galleryContents = new List<IHtmlElement>();
			foreach ( var div in divs )
			{
				galleryContents.Add(div);
				//var divHtml = div.InnerHtml;
				//var divHtmlFilePath = Path.Combine(outputDirPath, $"{div.Id}{index++}.html");
				//File.WriteAllText(divHtmlFilePath, divHtml);
			}

			var commentDivs = document.QuerySelectorAll("div.cmt_info");

			// Save to GalleryPost object
			var galleryPost = new GalleryPost(blogHome, postId, imageDirPath)
			{
				Category = category,
				Subject = subject,
				Author = author,
				RawContents = galleryContents,
				RawComments = commentDivs.ToList()
			};
			posts.Add(galleryPost);
			var htmlContents = galleryPost.Contents;
			var cdataContents = $"<![CDATA[{htmlContents}]]>";

			//break;
		}
		GenerateCSV(inputDirPath, outputDirPath, posts);
	}

	private void GenerateCSV(string inputDirPath, string outputDirPath, List<GalleryPost> posts)
	{
		throw new NotImplementedException();
	}

	private int getPostId(IHtmlDocument document, string? date)
	{
		var postId = 0;
		// Maybe we should additionaly match the date with the value of title.
		var td = document.QuerySelectorAll("td").First(x => x.HasAttribute("title") && x.GetAttribute("title").Equals(date));
		// Search the td element which has "title" attribute and contains text date.
		var parent = td.ParentElement;
		// Get the children td element which has "class" attribute and contains text "gall_tit".
		if (parent == null)
		{
			return postId;
		}
		else
		{
			foreach (var childTd in parent.Children)
			{
				if (childTd.ClassList.Contains("gall_tit")) {
					var linkTag = childTd.Children[0] as IHtmlElement;
					if (linkTag == null)
					{
						return postId;
					}
					var link = linkTag.GetAttribute("href");
					// extract postId using Regex pattern "&no=(\d+)"
					if (link == null) { return  postId; }
					Match match = Regex.Match(link, @"&no=(\d+)");

					if (match.Success)
					{
						string no = match.Groups[1].Value;
						postId = int.Parse(no);
					}

					break;
				}
			}
		}
		return postId;
	}
}