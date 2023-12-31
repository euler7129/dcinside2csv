﻿using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.XPath;
using dcinsideLibrary;
using dcinsideLibrary.Model;
using dcinsideLibrary.Util;
using System.Globalization;
using System.IO.Hashing;
using System.Text;
using System.Text.RegularExpressions;

ConsoleApp.Run<MyCommands>(args);

public class MyCommands : ConsoleAppBase
{
	[RootCommand]
	public void RootCommand(
		[Option("i", "Directory path containing input HTML files")] string inputDirPath,
		[Option("o", "Directory path of the output data")] string outputDirPath,
		[Option("h", "Url of blog home")] string blogHomeUrl, // "https://example.wordpress.com/"
		[Option("f", "Url of file home")] string fileHomeUrl) // "https://example.files.wordpress.com/"
	{// -i "D:\temp\dcinside\gallery" -o "D:\temp\dcinside\gallery_csv"
		var arguments = Context.Arguments;
		Console.WriteLine($"Executing with arguments: \"{string.Join(" ", arguments)}\"");
		Console.WriteLine("dcinsideLibrary has been started.");

		// Each HTML file will generate BlogPost, Comment, and Image objects.
		dcinside2csv(inputDirPath, outputDirPath, blogHomeUrl, fileHomeUrl);
	}

	private void dcinside2csv(string inputDirPath, string outputDirPath, string blogHomeUrl, string fileHomeUrl)
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
			int postId;
			try
			{
				postId = getPostId(document, date);
			}
			catch (Exception ex)
			{
				Logger.Instance.Log($"Error: {ex.Message}");
				Logger.Instance.Log($"Skipping {subject}");
				continue;
			}
			
			var author = document.Body.SelectSingleNode("//*[@id=\"container\"]/section/article[2]/div[1]/header/div/div/div[1]/span[1]/a/em").TextContent;
			// Get write_div div element
			var writeDiv = document.QuerySelector("div.write_div");
			// Filter only HtmlDivElement elements in writeDiv
			var blocks = writeDiv.Children.Where(x => x is IHtmlDivElement || x is IHtmlParagraphElement).Cast<AngleSharp.Html.Dom.IHtmlElement>();

			// Save each div element's InnerHtml to a text file
			int index = 0;
			List<IHtmlElement> galleryContents = new List<IHtmlElement>();
			foreach ( var block in blocks )
			{
				galleryContents.Add(block);
				//var divHtml = div.InnerHtml;
				//var divHtmlFilePath = Path.Combine(outputDirPath, $"{div.Id}{index++}.html");
				//File.WriteAllText(divHtmlFilePath, divHtml);
			}

			var commentDivs = document.QuerySelectorAll("li.ub-content");

			// Save to GalleryPost object
			var galleryPost = new GalleryPost(blogHomeUrl, fileHomeUrl, postId, imageDirPath)
			{
				Category = category,
				Subject = subject,
				Author = author,
				Date = date,
				RawContents = galleryContents,
				RawComments = commentDivs.ToList()
			};
			posts.Add(galleryPost);
			//var htmlContents = galleryPost.Contents;
			//var cdataContents = $"<![CDATA[{htmlContents}]]>";
			//var comments = galleryPost.Comments;

			//break;
		}
		GenerateCSV(outputDirPath, posts);
	}

	private void GenerateCSV(string outputDirPath, List<GalleryPost> posts)
	{
		// 1. Generate CSV file for posts (input.csv)
		var inputCsvPath = Path.Combine(outputDirPath, "input.csv");
		var galleryPost2Csv = new GalleryPost2Csv();
		var index = 1;
		foreach (var post in posts)
		{
			var csvPost = new CsvPost
			{
				uniqueId = post.PostId.ToString(),
				title = post.Subject,
				link = post.BlogHomeUrl,
				pubDate = toPubDate(post.Date), // Should convert
				dc_creator = post.Author,
				content_encoded = $"<![CDATA[{post.Contents}]]>",
				wp_post_id = post.PostId.ToString(),
				wp_post_date = toWpDate(post.Date),
				category = post.Category,
				category_nice = post.Category,
				comment_status = "closed",
				ping_status = "closed"
			};
			galleryPost2Csv.Add(csvPost);
		}
		galleryPost2Csv.Save(inputCsvPath);

		// 2. Generate CSV file for comments (comments\postId.csv)
		var commentsDirPath = Path.Combine(outputDirPath, "comments");
		Directory.CreateDirectory(commentsDirPath);

		foreach (var post in posts)
		{
			var galleryComment2Csv = new GalleryComment2Csv();
			foreach(var comment in post.Comments)
			{
				var csvComment = new CsvComment
				{
					CommentId = comment.CommentId.ToString(),
					Author = comment.Author,
					AuthorEmail = $"{Convert.ToHexString(Crc32.Hash(Encoding.UTF8.GetBytes(comment.Author)))}@example.com",
					AuthorUrl = $"http://example.com",
					AuthorIp = "172.0.0.1",
					Date = comment.Date,
					Content = comment.Content,
					Approved = "1",
					Type = "",
					Parent = comment.ParentCommentId == -1 ? "" : comment.ParentCommentId.ToString(),
					UserId = "0",
				};
				galleryComment2Csv.Add(csvComment);
			}
			var csvPath = Path.Combine(commentsDirPath, $"{post.PostId}.csv");
			galleryComment2Csv.Save(csvPath);
		}
	}

	private string toWpDate(string date)
	{
		// 2023.05.25 17:23:51
		var dateTime = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		// 2022-08-08 20:48
		return dateTime.ToString("yyyy-MM-dd HH:mm",CultureInfo.InvariantCulture);
	}

	private string toPubDate(string date)
	{
		// 2023.05.25 17:23:51
		var dateTime = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		// Tue, 08 Aug 2023 01:55:45 +0000
		// ddd should be English day of the week
		return dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss +0000",
			CultureInfo.CreateSpecificCulture("en-US"));
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