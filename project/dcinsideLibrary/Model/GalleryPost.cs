﻿using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Text;
using SkiaSharp;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WebMarkupMin.Core;

namespace dcinsideLibrary.Model
{
	public class GalleryPost
	{
		private static readonly Logger L = Logger.Instance;
		private GalleryPost() { }
		public GalleryPost(string blogHomeUrl, string fileHomeUrl, int postId, string imageDirPath)
		{
			BlogHomeUrl = blogHomeUrl; // "https://example.wordpress.com/"
			FileHomeUrl = fileHomeUrl; // "https://example.files.wordpress.com/"
			PostId = postId;
			ImageDirPath = imageDirPath;
			// Create directory if not exists
			if (!Directory.Exists(imageDirPath))
			{
				Directory.CreateDirectory(imageDirPath);
			}
		}

		public string? BlogHomeUrl { get; }
		public string? FileHomeUrl { get; }
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
					// Maybe image block
					if (rawContent.Children.Length == 1)
					{
						var childNode = rawContent.Children[0];
						// Skip DC Link block
						if (rawContent.ClassList.Contains("og-div"))
						{
							continue;
							//WrapParagraph(stringBuilder, rawContent);
						}

						// Skip <br> tag
						if (childNode.TagName.Equals("BR"))
						{
							continue;
						}

						// if childNode class has "imgwrap"...
						if (childNode.ClassList.Contains("imgwrap"))
						{
							// Save image from base64 data
							var imgNode = childNode.Children[0];

							try
							{
								ProcessImageBlock(stringBuilder, imgIndex, imgNode);
							}
							catch (Exception ex)
							{
								L.Log(ex.Message);
								L.Log($"Skipping image in post: {Subject}");
							}
							imgIndex++;
						}

						if (childNode.TagName.Equals("A"))
						{
							WrapParagraph(stringBuilder, rawContent);
						}
						else if (childNode.TagName.Equals("SPAN"))
						{
							if (childNode.InnerHtml == "")
							{
								continue;
							}
							WrapParagraph(stringBuilder, rawContent);
						}
					}
					else
					{
						if (rawContent.ClassList.Contains("imgwrap"))
						{// Image block
						 // Save image from base64 data
							var imgNode = rawContent.Children[0];

							try
							{
								ProcessImageBlock(stringBuilder, imgIndex, imgNode);
							}
							catch (Exception ex)
							{
								L.Log(ex.Message);
								L.Log("Skipping this image");
							}
							imgIndex++;
						}
						else
						{// paragraph block
						 // Skip if element is empty
							if (rawContent.InnerHtml == "")
							{
								continue;
							}
							// Skip if child node is dcCon
							if (rawContent.ChildElementCount > 0)
							{
								if (rawContent.Children[0].ClassList.Contains("written_dccon"))
									continue;
							}

							WrapParagraph(stringBuilder, rawContent);
						}
					}
				}

				//stringBuilder.Replace("\r\n", "\"\r\n\"");
				//stringBuilder.Replace("\n", "\"\n\"");
				return minify(stringBuilder.ToString());
			}
		}

		private static void WrapParagraph(StringBuilder stringBuilder, IHtmlElement rawContent)
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
				stringBuilder.Append(rawContent.OuterHtml);
			}
			stringBuilder.Append("<!-- /wp:paragraph -->");
		}

		private void ProcessImageBlock(StringBuilder stringBuilder, int imgIndex, IElement imgNode)
		{
			var dataSource = imgNode.GetAttribute("src");
			string patern = @"(data:image\/.*);base64,(.*)";
			var match = Regex.Match(dataSource, patern);
			var dataType = match.Groups[1].Value;
			var base64data = match.Groups[2].Value;
			// Decode base64data and save into file
			var imageBytes = Convert.FromBase64String(base64data);
			SKBitmap bitmap;
			SKImage image;
			bitmap = SKBitmap.Decode(imageBytes);

			image = SKImage.FromBitmap(bitmap);
			var imageFilenameStem = $"img-{PostId}-{imgIndex}";
			var imageFilePath = Path.Combine(ImageDirPath, imageFilenameStem);
			string ext;
			switch (dataType)
			{
				case "data:image/png":
					{
						ext = "png";
						using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
						File.WriteAllBytes($"{imageFilePath}.{ext}", data.ToArray());
					}
					break;
				case "data:image/jpeg":
					{
						ext = "jpg";
						using SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
						File.WriteAllBytes($"{imageFilePath}.{ext}", data.ToArray());
					}
					break;
				case "data:image/gif":
					{
						ext = "gif";
						using SKData data = image.Encode(SKEncodedImageFormat.Gif, 100);
						File.WriteAllBytes($"{imageFilePath}.{ext}", data.ToArray());
					}
					break;
				case "data:image/bmp":
					{
						ext = "bmp";
						using SKData data = image.Encode(SKEncodedImageFormat.Bmp, 100);
						File.WriteAllBytes($"{imageFilePath}.{ext}", data.ToArray());
					}
					break;
				case "data:image/webp":
					{
						ext = "webp";
						using SKData data = image.Encode(SKEncodedImageFormat.Webp, 100);
						File.WriteAllBytes($"{imageFilePath}.{ext}", data.ToArray());
					}
					break;
				default:
					L.Log($"Unsupported image type: {dataType}");
					ext = "png";
					break;
			}

			// Create WXR image block
			var now = DateTime.Now;
			stringBuilder.Append($"<!-- wp:image {{\"id\":{imgIndex},\"sizeSlug\":\"full\",\"linkDestination\":\"none\"}} -->");
			// https://{FileHomeUrl}/2023/08/img-1-1.png
			var fileUrl = FileHomeUrl.Last().Equals("/") ? FileHomeUrl : $"{FileHomeUrl}/";
			stringBuilder.Append($"<figure class=\"wp-block-image size-full\"><img src=\"{fileUrl}{now.Year}/{now.Month.ToString("d2")}/img-{PostId}-{imgIndex}.{ext}\" alt=\"\" class=\"wp-image-{imgIndex}\" /></figure>");
			stringBuilder.Append("<!-- /wp:image -->");
		}

		private string minify(string v)
		{
			var minifier = new HtmlMinifier(new HtmlMinificationSettings { RemoveHtmlComments = false });
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
					// general comment
					if (rawComment.GetAttribute("id").Contains("comment"))
					{
						// Get author inside rawComment. Query is "span.data-nick"
						string author = rawComment.QuerySelector("span.gall_writer")?.GetAttribute("data-nick") ?? "author";
						if (author.Equals("댓글돌이")) continue;
						// Get content inside rawComment. Query is "p.usertxt"
						string content = rawComment.QuerySelector("p.usertxt")?.InnerHtml ?? "content";
						int commentId = rawComment.QuerySelector("div.cmt_info").GetAttribute("data-no").ToInteger(0);
						// if div class "comment_dccon"
						if (content.Equals("content"))
						{
							if (rawComment.QuerySelector("div.comment_dccon") != null)
							{
								content = "디시콘";
							}
							if (rawComment.QuerySelector("p.del_reply") != null)
							{
								content = "(해당 댓글은 삭제되었습니다.)";
							}
						}

						// Get date inside rawComment. Query is "span.date_time"
						string date = rawComment.QuerySelector("span.date_time")?.InnerHtml ?? "date";
						if (date.Equals("date"))
						{
							// 03.29 09:12:16
							// Initialize with default date by referring post date
							if (Date != null)
							{
								date = postDateToCommentDate(Date);
							}
							else
							{
								date = "01.01 00:00:00";
							}
						}

						var comment = new GalleryComment
						{
							Author = author,
							Content = content,
							Date = dcCommentDateToCommentDate(date),
							CommentId = commentId,
							IsReply = false,
							ParentCommentId = -1
						};
						result.Add(comment);

					}
					else if (rawComment.GetAttribute("id").Contains("reply")) // reply comment
					{
						string author = rawComment.QuerySelector("span.contextmenu")?.InnerHtml ?? "author";
						if (author.Equals("댓글돌이")) continue;
						// Get content inside rawComment. Query is "p.usertxt"
						string content = rawComment.QuerySelector("p.usertxt")?.InnerHtml ?? "content";
						int commentId = rawComment.QuerySelector("div.reply_info").GetAttribute("data-no").ToInteger(0);
						// if div class "comment_dccon"
						if (content.Equals("content"))
						{
							if (rawComment.QuerySelector("div.comment_dccon") != null)
							{
								content = "디시콘";
							}
							if (rawComment.QuerySelector("p.del_reply") != null)
							{
								content = "(해당 댓글은 삭제되었습니다.)";
							}
						}

						// Get date inside rawComment. Query is "span.date_time"
						string date = rawComment.QuerySelector("span.date_time")?.InnerHtml ?? "date";
						if (date.Equals("date"))
						{
							// 03.29 09:12:16
							// Initialize with default date by referring post date
							if (Date != null)
							{
								date = postDateToCommentDate(Date);
							}
							else
							{
								date = "01.01 00:00:00";
							}
						}
						// Get parent comment id inside rawComment. It's inside attribute "id" and it's value is "reply_list_XXXXXX"
						int parentCommentId = rawComment.ParentElement.GetAttribute("id").Replace("reply_list_", "").ToInteger(0);

						var comment = new GalleryComment
						{
							Author = author,
							Content = content,
							CommentId = commentId,
							Date = dcCommentDateToCommentDate(date),
							IsReply = true,
							ParentCommentId = parentCommentId
						};

						result.Add(comment);
					}
				}
				return result;
			}
		}

		private string postDateToCommentDate(string? date)
		{
			// 2023.05.25 17:23:51
			var dateTime = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
			// 05.25 17:23:51
			return dateTime.ToString("MM.dd HH:mm:ss",
				CultureInfo.CreateSpecificCulture("en-US"));
		}

		private string dcCommentDateToCommentDate(string? date)
		{
			// 2023.05.25 17:23:51
			var dateTime = DateTime.ParseExact(date, "MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
			// 2023-03-29 04:02
			var postDate = DateTime.ParseExact(Date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
			var postYear = postDate.Year;
			// 05.25 17:23:51
			var dateResult = dateTime.ToString("MM-dd HH:mm",
				CultureInfo.CreateSpecificCulture("en-US"));
			return $"{postYear}-{dateResult}";
		}
	}
}
