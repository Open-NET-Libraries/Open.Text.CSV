using System.Web;

namespace Open.Text.CSV
{
	public static class WebExtensions
	{
		public static void WriteCsvValue(this HttpResponse writer, object value, bool forceQuotes = false)
		{
			writer.Write(CsvUtility.ExportValue(value, forceQuotes));
		}
	}
}
