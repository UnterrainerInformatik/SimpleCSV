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

using System.IO;
using System.Text;

namespace SimpleCsv
{
    public partial class CsvWriter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvWriter" /> class.
        ///     Use CsvWriter in using(){} block if possible (otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while).
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        /// <param name="isAppendMode">
        ///     if set to <c>true</c> [is append mode].
        /// </param>
        /// <param name="encoding">The encoding.</param>
        public CsvWriter(string filePathAndName, bool isAppendMode, Encoding encoding)
        {
            textWriter = new StreamWriter(filePathAndName, isAppendMode, encoding);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvWriter" /> class.
        ///     Use CsvWriter in using(){} block if possible (otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while).
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        public CsvWriter(string filePathAndName) : this(filePathAndName, false, Encoding.Default)
        {
        }

        /// <summary>
        ///     Initializes the <see cref="CsvWriter" />.
        ///     Use CsvWriter in using(){} block if possible (otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while).
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">
        ///     Row Delimiter, e.g. Environment.NewLine
        /// </param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        /// <param name="isAppendMode">
        ///     if set to <c>true</c> [is append mode].
        /// </param>
        /// <param name="encoding">The encoding.</param>
        public CsvWriter(string filePathAndName, char columnSeparator, string rowSeparator, char? fieldDelimiter,
            bool isAppendMode, Encoding encoding) : this(filePathAndName, isAppendMode, encoding)
        {
            this.columnSeparator = columnSeparator;
            this.rowSeparator = rowSeparator;
            this.fieldDelimiter = fieldDelimiter;
        }

        /// <summary>
        ///     Initializes the <see cref="CsvWriter" />.
        ///     Use CsvWriter in using(){} block if possible (otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while).
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">
        ///     Row Delimiter, e.g. Environment.NewLine
        /// </param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        public CsvWriter(string filePathAndName, char columnSeparator, string rowSeparator, char? fieldDelimiter) :
            this(filePathAndName, columnSeparator, rowSeparator, fieldDelimiter, false, Encoding.Default)
        {
        }

        /// <summary>
        ///     Initializes the CsvWriter.
        ///     Use CsvWriter in using(){} block if possible (otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while).
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">
        ///     Row Delimiter, e.g. Environment.NewLine
        /// </param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        /// <param name="writeChunkSize">
        ///     Size of the read chunk. (minimal value
        ///     is <paramref name="rowSeparator" />.length and is automatically
        ///     assigned if the given value was too small). The bufferSize is
        ///     automatically allocated in any case. It will be
        ///     <paramref name="writeChunkSize" />Size of the write chunk. (minimal
        ///     and default value is 1)
        /// </param>
        /// <param name="isAppendMode">
        ///     if set to <c>true</c> [is append mode].
        /// </param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="quotingBehavior">The quoting behavior.</param>
        public CsvWriter(string filePathAndName, char columnSeparator, string rowSeparator, char? fieldDelimiter,
            int writeChunkSize, bool isAppendMode, Encoding encoding, QuotingBehavior quotingBehavior) : this(
            filePathAndName,
            columnSeparator,
            rowSeparator,
            fieldDelimiter,
            isAppendMode,
            encoding)
        {
            SetChunkAndBufferSize(writeChunkSize);
            this.quotingBehavior = quotingBehavior;
        }

        /// <summary>
        ///     Initializes the <see cref="CsvWriter" />.
        ///     Use CsvWriter in using(){} block if possible (otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while).
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">
        ///     Row Delimiter, e.g. Environment.NewLine
        /// </param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        /// <param name="writeChunkSize">
        ///     Size of the read chunk. (minimal value
        ///     is <paramref name="rowSeparator" />.length and is automatically
        ///     assigned if the given value was too small). The bufferSize is
        ///     automatically allocated in any case. It will be
        ///     <paramref name="writeChunkSize" /> +
        ///     <paramref name="rowSeparator" />.length due to the parsing technique
        ///     used.
        /// </param>
        public CsvWriter(string filePathAndName, char columnSeparator, string rowSeparator, char? fieldDelimiter,
            int writeChunkSize) : this(filePathAndName,
            columnSeparator,
            rowSeparator,
            fieldDelimiter,
            writeChunkSize,
            false,
            Encoding.Default,
            QuotingBehavior.MINIMAL)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvWriter" /> class.
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        public CsvWriter(TextWriter textWriter)
        {
            this.textWriter = textWriter;
        }

        /// <summary>
        ///     Initializes the CsvWriter.
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">
        ///     Row Delimiter, e.g. Environment.NewLine
        /// </param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        public CsvWriter(TextWriter textWriter, char columnSeparator, string rowSeparator, char? fieldDelimiter) :
            this(textWriter)
        {
            this.columnSeparator = columnSeparator;
            this.rowSeparator = rowSeparator;
            this.fieldDelimiter = fieldDelimiter;
        }

        /// <summary>
        ///     Initializes the CsvWriter.
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">
        ///     Row Delimiter, e.g. Environment.NewLine
        /// </param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        /// <param name="writeChunkSize">
        ///     Size of the write chunk. (minimal and
        ///     default value is 1)
        /// </param>
        /// <param name="quotingBehavior">The quoting behavior.</param>
        public CsvWriter(TextWriter textWriter, char columnSeparator, string rowSeparator, char? fieldDelimiter,
            int writeChunkSize, QuotingBehavior quotingBehavior) : this(textWriter,
            columnSeparator,
            rowSeparator,
            fieldDelimiter)
        {
            SetChunkAndBufferSize(writeChunkSize);
            this.quotingBehavior = quotingBehavior;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvWriter" /> class.
        /// </summary>
        /// <param name="stringBuilder">The string builder.</param>
        public CsvWriter(StringBuilder stringBuilder)
        {
            textWriter = new StringWriter(stringBuilder);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvWriter" /> class.
        /// </summary>
        /// <param name="stringBuilder">The string builder.</param>
        /// <param name="columnSeparator">The column separator.</param>
        /// <param name="rowSeparator">The row separator.</param>
        /// <param name="fieldDelimiter">The field delimiter.</param>
        public CsvWriter(StringBuilder stringBuilder, char columnSeparator, string rowSeparator, char fieldDelimiter) :
            this(new StringWriter(stringBuilder), columnSeparator, rowSeparator, fieldDelimiter)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvWriter" /> class.
        /// </summary>
        /// <param name="stringBuilder">The string builder.</param>
        /// <param name="columnSeparator">The column separator.</param>
        /// <param name="rowSeparator">The row separator.</param>
        /// <param name="fieldDelimiter">The field delimiter.</param>
        /// <param name="writeChunkSize">
        ///     Size of the write chunk. (minimal and
        ///     default value is 1)
        /// </param>
        /// <param name="quotingBehavior">The quoting behavior.</param>
        public CsvWriter(StringBuilder stringBuilder, char columnSeparator, string rowSeparator, char fieldDelimiter,
            int writeChunkSize, QuotingBehavior quotingBehavior) : this(new StringWriter(stringBuilder),
            columnSeparator,
            rowSeparator,
            fieldDelimiter,
            writeChunkSize,
            quotingBehavior)
        {
        }

        // these overloads are for the convenience of the programmer only.
        // it should help to tell him that a StringWriter exists and is
        // derived from a TextWriter...


        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvWriter" /> class.
        /// </summary>
        /// <param name="stringWriter">The string writer.</param>
        public CsvWriter(StringWriter stringWriter) : this((TextWriter) stringWriter)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvWriter" /> class.
        /// </summary>
        /// <param name="stringWriter">The string writer.</param>
        /// <param name="columnSeparator">The column separator.</param>
        /// <param name="rowSeparator">The row separator.</param>
        /// <param name="fieldDelimiter">The field delimiter.</param>
        public CsvWriter(StringWriter stringWriter, char columnSeparator, string rowSeparator, char fieldDelimiter) :
            this((TextWriter) stringWriter, columnSeparator, rowSeparator, fieldDelimiter)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvWriter" /> class.
        /// </summary>
        /// <param name="stringWriter">The string writer.</param>
        /// <param name="columnSeparator">The column separator.</param>
        /// <param name="rowSeparator">The row separator.</param>
        /// <param name="fieldDelimiter">The field delimiter.</param>
        /// <param name="writeChunkSize">
        ///     Size of the write chunk. (minimal and
        ///     default value is 1)
        /// </param>
        /// <param name="quotingBehavior">The quoting behavior.</param>
        public CsvWriter(StringWriter stringWriter, char columnSeparator, string rowSeparator, char fieldDelimiter,
            int writeChunkSize, QuotingBehavior quotingBehavior) : this((TextWriter) stringWriter,
            columnSeparator,
            rowSeparator,
            fieldDelimiter,
            writeChunkSize,
            quotingBehavior)
        {
        }
    }
}