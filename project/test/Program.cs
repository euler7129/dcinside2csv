// See https://aka.ms/new-console-template for more information

using CsvHelper;
using dcinside2csv.Model;
using System.Globalization;

using var reader = new StreamReader("D:\\temp\\dcinside\\gallery_csv\\result.csv");
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var records = csv.GetRecords<CsvPost>().ToList();
var a = 1;