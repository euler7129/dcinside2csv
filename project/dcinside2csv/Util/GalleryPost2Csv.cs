﻿using dcinside2csv.Model;
using System.Text;

namespace dcinside2csv.Util
{
	public class GalleryPost2Csv
	{
		public readonly string fields;
		private List<CsvPost> csvPosts;

		public GalleryPost2Csv()
		{
			fields = "title,link,pubDate,dc:creator,content:encoded,wp:post_id,wp:post_date,category,category_nice,comment_status,ping_status";
			csvPosts = new List<CsvPost>();
		}

		public void Add(CsvPost csvPost)
		{
			csvPosts.Add(csvPost);
		}

		public bool Save(string filename)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(fields);
			foreach (CsvPost csvPost in csvPosts)
			{
				stringBuilder.AppendLine(csvPost.ToString());
			}

			try
			{
				File.WriteAllText($"{filename}.csv", stringBuilder.ToString());
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}
	}
}