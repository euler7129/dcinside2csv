using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using SkiaSharp;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WebMarkupMin.Core;

namespace dcinside2csv.Model
{
	public class GalleryPost
	{
		private GalleryPost() { }
		public GalleryPost(string blogHome, int postId, string imageDirPath)
		{
			BlogHome = blogHome; // "https://example.com/wp/"
			PostId = postId;
			ImageDirPath = imageDirPath;
			// Create directory if not exists
			if ( !Directory.Exists(imageDirPath) )
			{
				Directory.CreateDirectory(imageDirPath);
			}
		}

		public string? BlogHome { get; }
		public int PostId { get; }
		public string? ImageDirPath { get; set; }
		public string? Category { get; set; }
		public string? Subject { get; set; }
		public string? Author { get; set; }
		public string? Date { get; set; }
		public List<IHtmlElement>? RawContents { get; set; }
		public string Contents
		{
			get
			{
				var stringBuilder = new StringBuilder();

				int imgIndex = 1;
				// Make string from RawContents
				foreach (var rawContent in RawContents)
				{
					if (rawContent.Children.Length == 1)
					{// Maybe image block
						var childNode = rawContent.Children[0];
						// if childNode class has "imgwrap"...
						if (childNode.ClassList.Contains("imgwrap"))
						{
							// Save image from base64 data
							var imgNode = childNode.Children[0];
							var dataSource = imgNode.GetAttribute("src");
							string patern = @"(data:image\/.*);base64,(.*)";
							var match = Regex.Match(dataSource, patern);
							var dataType = match.Groups[1].Value;
							var base64data = match.Groups[2].Value;
							// Decode base64data and save into file
							var imageBytes = Convert.FromBase64String(base64data);
							using SKBitmap bitmap = SKBitmap.Decode(imageBytes);
							using SKImage image = SKImage.FromBitmap(bitmap);
							var imageFilenameStem = $"{PostId}-{imgIndex}";
							var imageFilePath = Path.Combine(ImageDirPath, imageFilenameStem);
							switch (dataType)
							{
								case "data:image/png":
									{
										using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
										File.WriteAllBytes($"{imageFilePath}.png", data.ToArray());
									}
									break;
								case "data:image/jpeg":
									{
										using SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
										File.WriteAllBytes($"{imageFilePath}.jpg", data.ToArray());
									}
									break;
								case "data:image/gif":
									{
										using SKData data = image.Encode(SKEncodedImageFormat.Gif, 100);
										File.WriteAllBytes($"{imageFilePath}.gif", data.ToArray());
									}
									break;
								case "data:image/bmp":
									{
										using SKData data = image.Encode(SKEncodedImageFormat.Bmp, 100);
										File.WriteAllBytes($"{imageFilePath}.bmp", data.ToArray());
									}
									break;
								case "data:image/webp":
									{
										using SKData data = image.Encode(SKEncodedImageFormat.Webp, 100);
										File.WriteAllBytes($"{imageFilePath}.webp", data.ToArray());
									}
									break;
								default:
									Console.WriteLine($"Unsupported image type: {dataType}");
									break;
							}


							// Create WXR image block
							stringBuilder.Append($"<!-- wp:image {{\"id\":{imgIndex},\"sizeSlug\":\"full\",\"linkDestination\":\"none\"}} -->");
							stringBuilder.Append($"<figure class=\"wp-block-image size-full\"><img src=\"{BlogHome}wp-content/uploads/2023/08/03-{PostId}-{imgIndex}.png\" alt=\"\" class=\"wp-image-{imgIndex}\" /></figure>");
							stringBuilder.Append("<!-- /wp:image -->");
							imgIndex++;
						}
					}
					else
					{
						stringBuilder.Append("<!-- wp:paragraph -->");
						// If element type is div, change it to p
						if (rawContent.TagName == "DIV")
						{
							var pElement = rawContent.Owner!.CreateElement<IHtmlParagraphElement>();
							foreach (var attribute in rawContent.Attributes)
							{
								pElement.SetAttribute(attribute.Name, attribute.Value);
							}
							pElement.InnerHtml = rawContent.InnerHtml;
							//rawContent.Parent!.ReplaceChild(pElement, rawContent); // I think I don't have to replace it
							stringBuilder.Append(pElement.OuterHtml);
						}
						else
						{
							stringBuilder.Append(rawContent.InnerHtml);
						}
						stringBuilder.Append("<!-- /wp:paragraph -->");
					}
				}

				//stringBuilder.Replace("\r\n", "\"\r\n\"");
				//stringBuilder.Replace("\n", "\"\n\"");
				return minify(stringBuilder.ToString());
			}
		}

		private string minify(string v)
		{
			var minifier = new HtmlMinifier();
			var minified = minifier.Minify(v);
			return minified.MinifiedContent;
		}

		public List<IElement>? RawComments { get; set; }
		public List<GalleryComment> Comments
		{
			get
			{
				var result = new List<GalleryComment>();
				// Make GalleryComment from RawComments
				foreach (var rawComment in RawComments)
				{
					// Get author inside rawComment. Query is "span.data-nick"
					string author = rawComment.QuerySelector("span.contextmenu")?.InnerHtml ?? "author";
					// Get content inside rawComment. Query is "p.usertxt"
					string content = rawComment.QuerySelector("p.usertxt")?.InnerHtml ?? "content";
					// Get date inside rawComment. Query is "span.date_time"
					string date = rawComment.QuerySelector("span.date_time")?.InnerHtml ?? "date";

					var comment = new GalleryComment
					{
						Author = author,
						Content = content,
						Date = date
					};
					result.Add(comment);
				}
				return result;
			}
		}

	}
}
