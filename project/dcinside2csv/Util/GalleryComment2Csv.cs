using CsvHelper;
using dcinsideLibrary.Model;
using System.Globalization;

using System.Text;

namespace dcinsideLibrary.Util
{
	public class GalleryComment2Csv
	{
		public readonly string fields;
		private List<CsvComment> csvComments;

		public GalleryComment2Csv()
		{
			fields = "comment_id,author,author_email,author_url,author_IP,date,content,approved,type,parent_id,user_id";
			csvComments = new List<CsvComment>();
		}

		public void Add(CsvComment csvComment)
		{
			csvComments.Add(csvComment);
		}

		public bool Save(string filename)
		{
			using var writer = new StreamWriter(filename, false, Encoding.UTF8);
			using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
			csv.WriteRecords(csvComments);
			return true;
		}
	}
}
