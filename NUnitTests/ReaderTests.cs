// *************************************************************************** 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>
// ***************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SimpleCsv;

namespace NUnitTests
{
    [TestFixture]
    [Category("SimpleCsv.Reader")]
    public class ReaderTests
    {
        private CsvReader csvReader;
        private StringReader stringReader;
        private const string NEW_LINE = "\n";

        [Test]
        public void SpecifyingEncodingWithAStringBuilderThrowsException()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                CsvReader.Builder(new StringReader("A;;A;T;;\r\nGreat")).Endcoding(Encoding.UTF8).Build();
            });
        }

        [Test]
        public void SpecifyingEncodingWithATextReaderThrowsException()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                CsvReader.Builder(new StringReader("A;;A;T;;\r\nGreat")).Endcoding(Encoding.UTF8).Build();
            });
        }

        [Test]
        public void EmptyFieldsTest()
        {
            stringReader = new StringReader("\"test\";test1;A 01;t;;");

            csvReader = CsvReader.Builder(stringReader).ColumnSeparator(';').RowSeparator(NEW_LINE).FieldDelimiter('\"')
                .Build();
            Assert.IsNotNull(csvReader);

            var row = csvReader.ReadRow();
            Assert.IsNotNull(row);

            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"test", "test1", "A 01", "t", "", ""}, row);
        }

        [Test]
        public void EmptyFieldsVariantTest()
        {
            stringReader = new StringReader("A;;A;T;;\r\nGreat");

            csvReader = new CsvReader(stringReader);
            Assert.IsNotNull(csvReader);

            var row = csvReader.ReadRow();
            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"A", "", "A", "T", "", ""}.ToList(), row);
        }

        [Test]
        public void SpecialCharactersTest()
        {
            stringReader = new StringReader("����;\"!\"\"�$%&/()=?\"\r\n\"_:;'*\";<>.-,,#+");

            csvReader = CsvReader.Builder(stringReader).ColumnSeparator(';').RowSeparator("\r\n").FieldDelimiter('\"')
                .Build();
            Assert.IsNotNull(csvReader);

            var row = csvReader.ReadRow();
            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"����", "!\"�$%&/()=?"}.ToList(), row);

            row = csvReader.ReadRow();
            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"_:;'*", "<>.-,,#+"}.ToList(), row);
        }

        [Test]
        public void EscapedFieldDelimitersTest()
        {
            stringReader = new StringReader("\"A\",01,\"A\"\" \"\"01\",\"t\"\"\",,\"\"");

            csvReader = CsvReader.Builder(stringReader).ColumnSeparator(',').RowSeparator(NEW_LINE).FieldDelimiter('\"')
                .Build();
            Assert.IsNotNull(csvReader);

            var row = csvReader.ReadRow();
            Assert.IsNotNull(row);

            Assert.AreEqual(new[] {"A", "01", "A\" \"01", "t\"", "", ""}.ToList(), row);

            row = csvReader.ReadRow();
            Assert.IsNull(row);
        }

        [Test]
        public void QuoteSpecialTest()
        {
            stringReader = new StringReader("9008390101544,öäü,\"Normal\" Test a string.,,");

            csvReader = CsvReader.Builder(stringReader).ColumnSeparator(',').RowSeparator(NEW_LINE).FieldDelimiter(null)
                .Build();
            Assert.IsNotNull(csvReader);

            var row = csvReader.ReadRow();
            Assert.IsNotNull(row);

            Assert.AreEqual(new[] {"9008390101544", "öäü", "\"Normal\" Test a string.", "", ""}.ToList(), row);

            row = csvReader.ReadRow();
            Assert.IsNull(row);
        }

        [Test]
        public void ReadAllRowsTest()
        {
            stringReader = new StringReader("test;test1;test5\r\n\"test2\";test3\r\ntest6;;\"test8\"");

            csvReader = CsvReader.Builder(stringReader).Build();
            Assert.IsNotNull(csvReader);

            var data = csvReader.ReadAllRows();
            Assert.IsNotNull(data);

            var d = new List<List<string>>
            {
                new[] {"test", "test1", "test5"}.ToList(),
                new[] {"test2", "test3"}.ToList(),
                new[] {"test6", "", "test8"}.ToList()
            };

            Assert.AreEqual(d, data);
        }

        [Test]
        public void ThreeLineFileWithLineBreaksAndEvilDelimitersTest()
        {
            stringReader = new StringReader("\"Great\";\"Totally\";\"Cool\"" + NEW_LINE + "\"Gr" + NEW_LINE +
                                            "eat\";\"Totally\";Cool" + NEW_LINE + "Great;Totally;Cool");

            csvReader = CsvReader.Builder(stringReader).ColumnSeparator(';').RowSeparator(NEW_LINE).FieldDelimiter('\"')
                .Build();
            Assert.IsNotNull(csvReader);

            var row = csvReader.ReadRow();
            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"Great", "Totally", "Cool"}.ToList(), row);

            row = csvReader.ReadRow();
            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"Gr" + NEW_LINE + "eat", "Totally", "Cool"}.ToList(), row);

            row = csvReader.ReadRow();
            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"Great", "Totally", "Cool"}.ToList(), row);

            row = csvReader.ReadRow();
            Assert.IsNull(row);
        }

        [Test]
        public void TwoLinesWithEmptyLastLineAndPreviousFieldDelimiterTest()
        {
            stringReader = new StringReader("Great;Totally;");
            csvReader = CsvReader.Builder(stringReader).ColumnSeparator(';').RowSeparator(NEW_LINE).FieldDelimiter(null)
                .Build();

            var row = csvReader.ReadRow();
            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"Great", "Totally", ""}.ToList(), row);

            row = csvReader.ReadRow();
            Assert.IsNull(row);
        }

        [Test]
        public void BufferNormalTest()
        {
            // 10 rows with 5 fields each containing 100 characters each.
            // = 5000 characters.
            const string fieldData =
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
            var rowData = new[] {fieldData, fieldData, fieldData, fieldData, fieldData};
            var csvData = new[]
                {rowData, rowData, rowData, rowData, rowData, rowData, rowData, rowData, rowData, rowData};
            var csvToWrite = WriteCsv(csvData, ';', NEW_LINE);
            stringReader = new StringReader(csvToWrite);

            csvReader = CsvReader.Builder(stringReader).ColumnSeparator(';').RowSeparator(NEW_LINE).FieldDelimiter('\"')
                .Build();
            // ReadAllRows calls ReadRow consecutively...
            var csv = csvReader.ReadAllRows();
            Assert.IsNotNull(csv);

            CheckCsvValues(csv, csvData);

            // all following read-attempts should return null...
            var row = csvReader.ReadRow();
            Assert.IsNull(row);
        }

        [Test]
        public void BufferSmallTest()
        {
            // 10 rows with 5 fields each containing 100 characters each.
            // = 5000 characters.
            const string fieldData =
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
            var rowData = new[] {fieldData, fieldData, fieldData, fieldData, fieldData};
            var csvData = new[]
                {rowData, rowData, rowData, rowData, rowData, rowData, rowData, rowData, rowData, rowData};
            var csvToWrite = WriteCsv(csvData, ';', "tooLong" + NEW_LINE);
            stringReader = new StringReader(csvToWrite);
            csvReader = CsvReader.Builder(stringReader).ColumnSeparator(';').RowSeparator("tooLong" + NEW_LINE)
                .FieldDelimiter('\"').ReadChunkSize(1).Build();
            // ChunkSize should be RowSeparator.length after this constructor call ("tooLong\r\n" = 9 characters).
            // ReadAllRows calls ReadRow consecutively...
            var csv = csvReader.ReadAllRows();
            Assert.IsNotNull(csv);
            CheckCsvValues(csv, csvData);
            // all following read-attempts should return null...
            var row = csvReader.ReadRow();
            Assert.IsNull(row);
            var rows = csvReader.ReadAllRows();
            Assert.IsNull(rows);
        }

        [Test]
        public void ReadRowTest()
        {
            stringReader = new StringReader("test;test1;test2\r\ntest4;test5;test6");

            csvReader = CsvReader.Builder(stringReader).Build();
            Assert.IsNotNull(csvReader);

            var row = csvReader.ReadRow();
            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"test", "test1", "test2"}.ToList(), row);

            row = csvReader.ReadRow();
            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"test4", "test5", "test6"}.ToList(), row);
        }

        [Test]
        public void BreaksBetweenDelimitersTest()
        {
            stringReader = new StringReader("test;\"test\r\nnewlineTest\";test2\r\ntest4;test5;test6");

            csvReader = CsvReader.Builder(stringReader).Build();
            Assert.IsNotNull(csvReader);

            var row = csvReader.ReadRow();
            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"test", "test\r\nnewlineTest", "test2"}.ToList(), row);

            row = csvReader.ReadRow();
            Assert.IsNotNull(row);
            Assert.AreEqual(new[] {"test4", "test5", "test6"}.ToList(), row);
        }

        private static string WriteCsv(IEnumerable<string[]> csv, char columnSeparator, string rowSeparator)
        {
            var result = "";
            var isFirst = true;
            foreach (var row in csv)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result += rowSeparator;
                }

                var stringList = new List<string>(row);
                result += WriteRow(stringList, columnSeparator);
            }

            return result;
        }

        private static string WriteRow(IEnumerable<string> row, char columnSeparator)
        {
            var result = "";
            var isFirst = true;
            foreach (var s in row)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result += columnSeparator;
                }

                result += s;
            }

            return result;
        }

        private static void CheckCsvValues(IReadOnlyList<List<string>> csvToCheck,
            IReadOnlyList<string[]> rightCsvValues)
        {
            Assert.AreEqual(rightCsvValues.Count, csvToCheck.Count);
            for (var i = 0; i < csvToCheck.Count; i++)
            {
                CheckRowValues(csvToCheck[i], rightCsvValues[i]);
            }
        }

        private static void CheckRowValues(IReadOnlyList<string> rowToCheck, IReadOnlyList<string> rightFieldValues)
        {
            Assert.AreEqual(rightFieldValues.Count, rowToCheck.Count);
            for (var i = 0; i < rowToCheck.Count; i++)
            {
                Assert.AreEqual(rightFieldValues[i], rowToCheck[i]);
            }
        }
    }
}