using CsvHelper.Configuration.Attributes;

namespace dcinsideLibrary.Model
{
	public class CsvComment
	{
		// comment_id,author,author_email,author_url,author_IP,date,content,approved,type,parent,user_id
		[Name("comment_id")]
		public string CommentId { get; set; }
		[Name("author")]
		public string Author { get; set; }
		[Name("author_email")]
		public string AuthorEmail { get; set; }
		[Name("author_url")]
		public string AuthorUrl { get; set; }
		[Name("author_IP")]
		public string AuthorIp { get; set; }
		[Name("date")]
		public string Date { get; set; }
		[Name("content")]
		public string Content { get; set; }
		[Name("approved")]
		public string Approved { get; set; }
		[Name("type")]
		public string Type { get; set; }
		[Name("parent_id")]
		public string Parent { get; set; }
		[Name("user_id")]
		public string UserId { get; set; }
	}
}
