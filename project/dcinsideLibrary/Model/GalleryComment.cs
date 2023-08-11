namespace dcinsideLibrary.Model
{
	public class GalleryComment
	{
		public string Author { get; set; }
		public string Content { get; set; }
		public string Date { get; set; }
		public int CommentId { get; set; }
		public bool IsReply { get; set; }
		public int ParentCommentId { get; set; }
	}
}
