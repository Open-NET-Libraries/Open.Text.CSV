using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Open.Text.CSV.Test;

public static class CsvWriterTests
{
	[Fact]
	public static void CsvWriter_BasicRowBuildTest()
	{
		MemoryStream ms = new MemoryStream();
		TextWriter tw = new StreamWriter(ms);
		string[] fields = { "Field1", "with_\"Doublequotes\"", "", "with_comma,", "with_doublequoted_comma\",\"", "" };
		CsvWriter csw = new CsvWriter(tw);

		csw.WriteRow(fields);
		tw.Flush();
		byte[] buff = new byte[ms.Length];
		Array.Copy(ms.GetBuffer(), buff, ms.Length);
		string line_without_quotes = Encoding.UTF8.GetString(buff);
		Assert.Equal(line_without_quotes, "Field1,with_\"Doublequotes\",,\"with_comma,\",\"with_doublequoted_comma\"\",\"\"\",,\r\n");

		ms.SetLength(0);
		csw.WriteRow(fields, omitLastComma:true);
		tw.Flush();
		buff = new byte[ms.Length];
		Array.Copy(ms.GetBuffer(), buff, ms.Length);
		string line_without_quotes_without_comma_at_end = Encoding.UTF8.GetString(buff);
		Assert.Equal(line_without_quotes_without_comma_at_end, "Field1,with_\"Doublequotes\",,\"with_comma,\",\"with_doublequoted_comma\"\",\"\"\",\r\n");

		ms.SetLength(0);
		csw.WriteRow(fields, true);
		tw.Flush();
		buff = new byte[ms.Length];
		Array.Copy(ms.GetBuffer(), buff, ms.Length);
		string line_with_quotes = Encoding.UTF8.GetString(buff);
		Assert.Equal(line_with_quotes, "\"Field1\",\"with_\"\"Doublequotes\"\"\",,\"with_comma,\",\"with_doublequoted_comma\"\",\"\"\",,\r\n");


		ms.SetLength(0);
		csw.WriteRow(fields, true, true);
		tw.Flush();
		buff = new byte[ms.Length];
		Array.Copy(ms.GetBuffer(), buff, ms.Length);
		string line_with_quotes_without_comma_at_end = Encoding.UTF8.GetString(buff);
		Assert.Equal(line_with_quotes_without_comma_at_end, "\"Field1\",\"with_\"\"Doublequotes\"\"\",,\"with_comma,\",\"with_doublequoted_comma\"\",\"\"\",\r\n");


	}
}
