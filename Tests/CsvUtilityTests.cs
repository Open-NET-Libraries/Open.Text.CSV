using System.Linq;
using Xunit;

namespace Open.Text.CSV.Test
{
	public class CsvUtilityTests
	{
		[Fact]
		public void PatternTest0()
		{
			const string line = ",";
			var result = CsvUtility.GetRow(line).Select(e=>(string)e).ToArray();
			Assert.Equal(new[] { string.Empty }, result);
		}

		[Fact]
		public void PatternTest1()
		{
			const string line = "A,B,C, \"D\" ,E";
			var result = CsvUtility.GetRow(line).Select(e=>(string)e).ToArray();
			Assert.Equal(new[] { "A", "B", "C", "D", "E" }, result);
		}

		[Fact]
		public void PatternTest2()
		{
			const string line = "A,B,C,\"D\",,E";
			var result = CsvUtility.GetRow(line).Select(e=>(string)e).ToArray();
			Assert.Equal(new[] { "A", "B", "C", "D", string.Empty, "E" }, result);
		}

		[Fact]
		public void PatternTest3()
		{
			const string line = "A,B,C,\"D\",,E,";
			var result = CsvUtility.GetRow(line).Select(e=>(string)e).ToArray();
			Assert.Equal(new[] { "A", "B", "C", "D", string.Empty, "E" }, result);
		}


		[Fact]
		public void PatternTest4()
		{
			const string line = "A,B,C,\"D\",,\"E\"";
			var result = CsvUtility.GetRow(line).Select(e=>(string)e).ToArray();
			Assert.Equal(new[] { "A", "B", "C", "D", string.Empty, "E" }, result);
		}

		[Fact]
		public void PatternTest5()
		{
			const string line = "A,B,C,\"D\",,\"E\",";
			var result = CsvUtility.GetRow(line).Select(e=>(string)e).ToArray();
			Assert.Equal(new[] { "A", "B", "C", "D", string.Empty, "E" }, result);
		}

		[Fact]
		public void PatternTest6()
		{
			const string line = "A,B,\"\"\"C, c\"\"\",\"D\",,\"E\" ";
			var result = CsvUtility.GetRow(line).Select(e => (string)e).ToArray();
			Assert.Equal(new[] { "A", "B", "\"C, c\"", "D", string.Empty, "E" }, result);
		}

		[Fact]
		public void PatternTest7()
		{
			const string line = "A,B ,C, \"D\" ,E";
			var result = CsvUtility.GetRow(line).Select(e => (string)e).ToArray();
			Assert.Equal(new[] { "A", "B", "C", "D", "E" }, result);
		}

	}
}
