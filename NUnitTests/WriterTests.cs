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
using System.Text;
using NUnit.Framework;
using SimpleCsv;
using SimpleCsv.Writer;

namespace NUnitTests
{
    [TestFixture]
    [Category("SimpleCsv.Writer")]
    public class WriterTests
    {
        private StringWriter stringWriter;
        private CsvWriter csvWriter;
        private const string NEW_LINE = "\n";

        [SetUp]
        public void Setup()
        {
            stringWriter = new StringWriter();
            csvWriter = CsvWriter.Builder(stringWriter).ColumnSeparator(';').RowSeparator(NEW_LINE).FieldDelimiter('\"')
                .Build();
        }

        [TearDown]
        public void TearDown()
        {
            csvWriter.Dispose();
        }

        [Test]
        public void SpecifyingEncodingWithAStringWriterThrowsException()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                CsvWriter.Builder(new StringWriter(new StringBuilder("A;;A;T;;\r\nGreat"))).Encoding(Encoding.UTF8)
                    .Build();
            });
        }

        [Test]
        public void SpecifyingEncodingWithATextWriterThrowsException()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                CsvWriter.Builder((TextWriter) new StringWriter(new StringBuilder("A;;A;T;;\r\nGreat")))
                    .Encoding(Encoding.UTF8).Build();
            });
        }

        [Test]
        public void SpecifyingEncodingWithAStringBuilderThrowsException()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                CsvWriter.Builder(new StringWriter(new StringBuilder("A;;A;T;;\r\nGreat"))).Encoding(Encoding.UTF8)
                    .Build();
            });
        }

        [Test]
        public void PassingStringBuilderDoesntCloseItOnDispose()
        {
            var b = new StringBuilder("A;;A;T;;\r\nGreat");
            var w = CsvWriter.Builder(b).Build();
            w.Dispose();
            b.Append("Should not throw exception");
        }

        [Test]
        public void PassingStringWriterDoesntCloseItOnDispose()
        {
            var b = new StringWriter(new StringBuilder("A;;A;T;;\r\nGreat"));
            var w = CsvWriter.Builder(b).Build();
            w.Dispose();
            b.Write("Should not throw exception");
            b.Flush();
            b.Close();
        }

        [Test]
        public void PassingTextWriterDoesntCloseItOnDispose()
        {
            var b = new StringWriter(new StringBuilder("A;;A;T;;\r\nGreat"));
            var w = CsvWriter.Builder((TextWriter) b).Build();
            w.Dispose();
            b.Write("Should not throw exception");
            b.Flush();
            b.Close();
        }

        [Test]
        public void MultiRowsWithSpecialCharactersAsDataTest()
        {
            var csv = new List<List<string>>();

            var row = new List<string> {"Great", "Totally", "This is a" + NEW_LINE + "break", ""};
            csv.Add(row);
            row = new List<string> {"", "Gr;eat", "Totally"};
            csv.Add(row);
            row = new List<string> {"Great", "���\"�", "", "Totally"};
            csv.Add(row);

            Assert.IsNotNull(csvWriter);
            csvWriter.Write(csv).Flush();

            Assert.AreEqual("Great;Totally;\"This is a" + NEW_LINE + "break\";" + NEW_LINE + ";\"Gr;eat\";Totally" +
                            NEW_LINE + "Great;\"���\"\"�\";;Totally",
                stringWriter.ToString());
        }

        [Test]
        public void MultiRowsWithSpecialCharactersTest()
        {
            Assert.IsNotNull(csvWriter);

            csvWriter.Write("Great").Write("Totally").Write("This is a" + NEW_LINE + "break");
            // The next two lines do the same as calling csvWriter.WriteLine("");
            csvWriter.Write().WriteLine();
            csvWriter.Write().Write("Gr;eat").WriteLine("Totally");
            csvWriter.Write("Great").Write("���\"�").Write("").Write("Totally").Flush();

            Assert.AreEqual("Great;Totally;\"This is a" + NEW_LINE + "break\";" + NEW_LINE + ";\"Gr;eat\";Totally" +
                            NEW_LINE + "Great;\"���\"\"�\";;Totally",
                stringWriter.ToString());
        }

        [Test]
        public void BuildingMethodsMinimalQuotingTest()
        {
            Assert.IsNotNull(csvWriter);

            using (csvWriter = CsvWriter.Builder(stringWriter).ColumnSeparator(';').RowSeparator("\r\n")
                .FieldDelimiter('\"').ChunkSize(10).QuotingBehavior(QuotingBehavior.MINIMAL).Build())
            {
                csvWriter.Write("Great").Write("Totally").Write("This is a\r\nbreak").WriteLine("").Write()
                    .Write("Gr;eat").WriteLine("Totally").Write("Great").Write("���\"�").Write().Write("Totally");
            }

            Assert.AreEqual("Great;Totally;\"This is a\r\nbreak\";" + "\r\n;\"Gr;eat\";Totally\r\n" +
                            "Great;\"���\"\"�\";;Totally",
                stringWriter.ToString());
        }

        [Test]
        public void BuildingMethodsMaximalQuotingTest()
        {
            using (csvWriter = CsvWriter.Builder(stringWriter).ColumnSeparator(';').RowSeparator("\r\n")
                .FieldDelimiter('\"').ChunkSize(10).QuotingBehavior(QuotingBehavior.ALL).Build())
            {
                Assert.IsNotNull(csvWriter);
                csvWriter.Write("Great").Write("Totally").Write("This is a\r\nbreak").WriteLine("").Write()
                    .Write("Gr;eat").WriteLine("Totally").Write("Great").Write("���\"�").Write().Write("Totally");
            }

            Assert.AreEqual("\"Great\";\"Totally\";\"This is a\r\nbreak" +
                            "\";\"\"\r\n\"\";\"Gr;eat\";\"Totally\"\r\n\"" + "Great\";\"���\"\"�\";\"\";\"Totally\"",
                stringWriter.ToString());
        }

        [Test]
        public void AppendingNullWithWriteStringTest()
        {
            using (csvWriter = CsvWriter.Builder(stringWriter).ColumnSeparator(';').RowSeparator("\r\n")
                .FieldDelimiter('\"').ChunkSize(10).QuotingBehavior(QuotingBehavior.ALL).Build())
            {
                Assert.IsNotNull(csvWriter);
                csvWriter.Write("").Write().Write("3").WriteLine().Write("1").Write((string) null).Write("3");
            }

            Assert.AreEqual("\"\";\"\";\"3\"\r\n\"1\";\"\";\"3\"", stringWriter.ToString());
        }

        [Test]
        public void AppendingNullWithWriteTest()
        {
            var csv = new List<List<string>>();

            var row = new List<string> {"", "", "3"};
            csv.Add(row);
            row = new List<string> {"1", null, "3"};
            csv.Add(row);

            using (csvWriter = CsvWriter.Builder(stringWriter).ColumnSeparator(';').RowSeparator("\r\n")
                .FieldDelimiter('\"').ChunkSize(200).QuotingBehavior(QuotingBehavior.ALL).Build())
            {
                Assert.IsNotNull(csvWriter);
                csvWriter.Write(csv);
            }

            Assert.AreEqual("\"\";\"\";\"3\"\r\n\"1\";\"\";\"3\"", stringWriter.ToString());
        }

        [Test]
        public void WriteEmptyStringTest()
        {
            Assert.IsNotNull(csvWriter);

            var ls = new List<string> {"test", "", "test2", "", "test3"};
            csvWriter.Write("test").Write().Write("test2").Write("").Write("test3").Flush();

            Assert.AreEqual(ConvertToSimpleCsvString(ls), stringWriter.ToString());
        }

        [Test]
        public void WriteSingleStringTest()
        {
            Assert.IsNotNull(csvWriter);

            const string str = "test";
            csvWriter.Write(str).Flush();

            Assert.AreEqual(str, stringWriter.ToString());
        }

        [Test]
        public void WriteListOfStringTest()
        {
            Assert.IsNotNull(csvWriter);

            var ls = new List<string> {"test", "test1", "test2"};
            csvWriter.Write(ls).Flush();

            Assert.AreEqual(ConvertToSimpleCsvString(ls), stringWriter.ToString());
        }

        [Test]
        public void WriteTest()
        {
            Assert.IsNotNull(csvWriter);

            var data = new List<List<string>>();
            var row = new List<string> {"test", "test1", "test2"};
            data.Add(row);
            row = new List<string> {"test3", "����!�$%&/()==?`�", "test5"};
            data.Add(row);

            csvWriter.Write(data).Flush();

            Assert.AreEqual(ConvertAllToSimpleCsvString(data), stringWriter.ToString());
        }

        [Test]
        public void WriteLineSingleStringTest()
        {
            Assert.IsNotNull(csvWriter);

            const string str = "test";
            csvWriter.WriteLine(str).Flush();

            Assert.AreEqual(str + NEW_LINE, stringWriter.ToString());
        }

        [Test]
        public void WriteLineListStringTest()
        {
            Assert.IsNotNull(csvWriter);

            var ls = new List<string> {"test", "test1", "test2"};
            csvWriter.WriteLine(ls).Flush();

            Assert.AreEqual(ConvertToSimpleCsvString(ls) + NEW_LINE, stringWriter.ToString());
        }

        private string ConvertToSimpleCsvString(List<string> ls)
        {
            var ret = "";
            for (var i = 0; i < ls.Count; i++)
            {
                ret += ls[i];
                if (i != ls.Count - 1)
                {
                    ret += ";";
                }
            }

            return ret;
        }

        private string ConvertAllToSimpleCsvString(List<List<string>> data)
        {
            var isFirst = true;
            var result = "";
            foreach (var row in data)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result += NEW_LINE;
                }

                result += ConvertToSimpleCsvString(row);
            }

            return result;
        }
    }
}