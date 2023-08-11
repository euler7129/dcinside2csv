using CsvHelper.Configuration.Attributes;
using System.Text;

namespace dcinsideLibrary.Model
{
	public class CsvPost
	{
		// title,link,pubDate,dc:creator,content:encoded,wp:post_id,wp:post_date,category,category_nice,comment_status,ping_status
		[Name("title")]
		public string title { get; set; }
		[Name("link")]
		public string link { get; set; }
		[Name("pubDate")]
		public string pubDate { get; set; }
		[Name("dc:creator")]
		public string dc_creator { get; set; }
		[Name("content:encoded")]
		public string content_encoded { get; set; }
		[Name("wp:post_id")]
		public string wp_post_id { get; set; }
		[Name("wp:post_date")]
		public string wp_post_date { get; set; }
		[Name("category")]
		public string category { get; set; }
		[Name("category_nice")]
		public string category_nice { get; set; }
		[Name("comment_status")]
		public string comment_status { get; set; }
		[Name("ping_status")]
		public string ping_status { get; set; }

		// Override ToString() to return a CSV string.
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(title);
			sb.Append(",");
			sb.Append(link);
			sb.Append(",");
			sb.Append(pubDate);
			sb.Append(",");
			sb.Append(dc_creator);
			sb.Append(",");
			sb.Append(content_encoded);
			sb.Append(",");
			sb.Append(wp_post_id);
			sb.Append(",");
			sb.Append(wp_post_date);
			sb.Append(",");
			sb.Append(category);
			sb.Append(",");
			sb.Append(category_nice);
			sb.Append(",");
			sb.Append(comment_status);
			sb.Append(",");
			sb.Append(ping_status);
			return sb.ToString();
		}
	}
}
