using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace dcinside2csv.Model
{
	public class GalleryPost
	{
		public string? Category { get; set; }
		public string? Subject { get; set; }
		public string? Author { get; set; }
		public List<IHtmlElement>? RawContents { get; set; }
		public string Contents
		{
			get
			{
				string result = "";

				// Make string from RawContents
				foreach ( var rawContent in RawContents )
				{
					result += rawContent.InnerHtml;
				}

				return result;
			}
		}
		public List<IElement>? RawComments { get; set; }
		public List<GalleryComment> Comments { 
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
					result.Add( comment );
				}
				return result;
			}
		}

	}
}
