// See https://aka.ms/new-console-template for more information

using CsvHelper;
using dcinsideLibrary.Model;
using System.Globalization;

using var reader = new StreamReader("D:\\temp\\dcinside\\gallery_csv\\input.csv");
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var records = csv.GetRecords<CsvPost>().ToList();
var a = 1;