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
using System.IO;
using System.Text;

namespace SimpleCsv
{
    public partial class CsvReader : CsvBase
    {
        public static CsvReaderBuilder Builder(StringReader reader)
        {
            return new CsvReaderBuilder(reader);
        }

        public static CsvReaderBuilder Builder(TextReader reader)
        {
            return new CsvReaderBuilder(reader);
        }

        public static CsvReaderBuilder Builder(string filePathAndName)
        {
            return new CsvReaderBuilder(filePathAndName);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvReader" /> class.
        ///     Use CsvReader in using(){} block if possible, otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while.
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        /// <param name="encoding">The encoding.</param>
        public CsvReader(string filePathAndName, Encoding encoding)
        {
            textReader = new StreamReader(filePathAndName, encoding);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvReader" /> class.
        ///     Use CsvReader in using(){} block if possible, otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while.
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        public CsvReader(string filePathAndName) : this(filePathAndName, Encoding.Default)
        {
        }

        /// <summary>
        ///     Initializes the CSV Reader
        ///     Use CsvReader in using(){} block if possible, otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while.
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">Row Delimiter, e.g. Environment.NewLine</param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        /// <param name="encoding">The encoding.</param>
        public CsvReader(string filePathAndName, char columnSeparator, string rowSeparator, char? fieldDelimiter,
            Encoding encoding) : this(filePathAndName, encoding)
        {
            this.columnSeparator = columnSeparator;
            this.rowSeparator = rowSeparator;
            this.fieldDelimiter = fieldDelimiter;
        }

        /// <summary>
        ///     Initializes the CSV Reader
        ///     Use CsvReader in using(){} block if possible, otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while.
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">Row Delimiter, e.g. Environment.NewLine</param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        public CsvReader(string filePathAndName, char columnSeparator, string rowSeparator, char? fieldDelimiter) :
            this(filePathAndName, columnSeparator, rowSeparator, fieldDelimiter, Encoding.Default)
        {
        }

        /// <summary>
        ///     Initializes the CSV Reader
        ///     Use CsvReader in using(){} block if possible, otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while.
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">
        ///     Row Delimiter, e.g. Environment.NewLine
        /// </param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        /// <param name="readChunkSize">
        ///     Size of the read chunk. (minimal value
        ///     is <paramref name="rowSeparator" />.length and is automatically
        ///     assigned if the given value was too small). The bufferSize is
        ///     automatically allocated in any case. It will be
        ///     <paramref name="readChunkSize" /> +
        ///     <paramref name="rowSeparator" />.length due to the parsing technique
        ///     used.
        /// </param>
        /// <param name="encoding">The encoding.</param>
        public CsvReader(string filePathAndName, char columnSeparator, string rowSeparator, char? fieldDelimiter,
            int readChunkSize, Encoding encoding) : this(filePathAndName,
            columnSeparator,
            rowSeparator,
            fieldDelimiter,
            encoding)
        {
            SetChunkAndBufferSize(readChunkSize);
        }

        /// <summary>
        ///     Initializes the CSV Reader
        ///     Use CsvReader in using(){} block if possible, otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while.
        /// </summary>
        /// <param name="filePathAndName">The path and name of the file.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">
        ///     Row Delimiter, e.g. Environment.NewLine
        /// </param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        /// <param name="readChunkSize">
        ///     Size of the read chunk. (minimal value
        ///     is <paramref name="rowSeparator" />.length and is automatically
        ///     assigned if the given value was too small). The bufferSize is
        ///     automatically allocated in any case. It will be
        ///     <paramref name="readChunkSize" /> +
        ///     <paramref name="rowSeparator" />.length due to the parsing technique
        ///     used.
        /// </param>
        public CsvReader(string filePathAndName, char columnSeparator, string rowSeparator, char? fieldDelimiter,
            int readChunkSize) : this(filePathAndName,
            columnSeparator,
            rowSeparator,
            fieldDelimiter,
            readChunkSize,
            Encoding.Default)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvReader" /> class.
        ///     Use CsvReader in using(){} block if possible, otherwise the dispose method (which closes the file handle) is only
        ///     called by the garbage collector which may take a while.
        /// </summary>
        /// <param name="textReader">The text reader.</param>
        /// <exception cref="ArgumentNullException"><paramref name="textReader" /> is <c>null</c>.</exception>
        public CsvReader(TextReader textReader)
        {
            this.textReader = textReader ??
                              throw new ArgumentNullException(nameof(textReader),
                                  "The StreamReader you provided is null.");
        }

        /// <summary>
        ///     Initializes the CSV Reader
        /// </summary>
        /// <param name="textReader">The text reader.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">Row Delimiter, e.g. Environment.NewLine</param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        public CsvReader(TextReader textReader, char columnSeparator, string rowSeparator, char? fieldDelimiter) :
            this(textReader)
        {
            this.columnSeparator = columnSeparator;
            this.rowSeparator = rowSeparator;
            this.fieldDelimiter = fieldDelimiter;
        }

        /// <summary>
        ///     Initializes the CSV Reader
        /// </summary>
        /// <param name="textReader">The text reader.</param>
        /// <param name="columnSeparator">Delimiter e.g. ';'</param>
        /// <param name="rowSeparator">Row Delimiter, e.g. Environment.NewLine</param>
        /// <param name="fieldDelimiter">e.g. " or just NULL</param>
        /// <param name="readChunkSize">
        ///     Size of the read chunk. (minimal value
        ///     is <paramref name="rowSeparator" />.length and is automatically
        ///     assigned if the given value was too small). The bufferSize is
        ///     automatically allocated in any case. It will be
        ///     <paramref name="readChunkSize" /> +
        ///     <paramref name="rowSeparator" />.length due to the parsing technique
        ///     used.
        /// </param>
        public CsvReader(TextReader textReader, char columnSeparator, string rowSeparator, char? fieldDelimiter,
            int readChunkSize) : this(textReader, columnSeparator, rowSeparator, fieldDelimiter)
        {
            SetChunkAndBufferSize(readChunkSize);
        }

        // these overloads are for the convenience of the programmer only.
        // it should help to tell him that a StringReader exists and is
        // derived from a TextReader...

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvReader" /> class.
        /// </summary>
        /// <param name="stringReader">The string reader.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stringReader" /> is <c>null</c>.</exception>
        public CsvReader(StringReader stringReader)
        {
            textReader = stringReader ??
                         throw new ArgumentNullException(nameof(stringReader),
                             "The StreamReader you provided is null.");
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvReader" /> class.
        /// </summary>
        /// <param name="stringReader">The string reader.</param>
        /// <param name="columnSeparator">The column separator.</param>
        /// <param name="rowSeparator">The row separator.</param>
        /// <param name="fieldDelimiter">The field delimiter.</param>
        public CsvReader(StringReader stringReader, char columnSeparator, string rowSeparator, char fieldDelimiter) :
            this((TextReader) stringReader, columnSeparator, rowSeparator, fieldDelimiter)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvReader" /> class.
        /// </summary>
        /// <param name="stringReader">The string reader.</param>
        /// <param name="columnSeparator">The column separator.</param>
        /// <param name="rowSeparator">The row separator.</param>
        /// <param name="fieldDelimiter">The field delimiter.</param>
        /// <param name="readChunkSize">
        ///     Size of the read chunk. (minimal value
        ///     is <paramref name="rowSeparator" />.length and is automatically
        ///     assigned if the given value was too small). The bufferSize is
        ///     automatically allocated in any case. It will be
        ///     <paramref name="readChunkSize" /> +
        ///     <paramref name="rowSeparator" />.length due to the parsing technique
        ///     used.
        /// </param>
        public CsvReader(StringReader stringReader, char columnSeparator, string rowSeparator, char fieldDelimiter,
            int readChunkSize) : this((TextReader) stringReader,
            columnSeparator,
            rowSeparator,
            fieldDelimiter,
            readChunkSize)
        {
        }
    }
}