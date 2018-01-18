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

namespace SimpleCsv.Writer
{
    public class CsvWriterBuilder : CsvBuilderBase<CsvWriterBuilder>
    {
        private readonly StringBuilder stringBuilder;
        private readonly StringWriter stringWriter;
        private readonly TextWriter textWriter;
        private readonly string filePathAndName;

        private bool isAppendMode;
        private QuotingBehavior quotingBehavior;

        private CsvWriterBuilder()
        {
            chunkSize = CsvWriter.DEFAULT_CHUNK_SIZE;
        }

        public CsvWriterBuilder(StringBuilder stringBuilder) : this()
        {
            this.stringBuilder = stringBuilder;
        }

        public CsvWriterBuilder(StringWriter stringWriter) : this()
        {
            this.stringWriter = stringWriter;
        }

        public CsvWriterBuilder(TextWriter textWriter) : this()
        {
            this.textWriter = textWriter;
        }

        public CsvWriterBuilder(string filePathAndName) : this()
        {
            this.filePathAndName = filePathAndName;
        }

        /// <summary>
        ///     Builds an instance of <see cref="Reader.CsvReader" />.
        /// </summary>
        /// <returns>A <see cref="Reader.CsvReader" /></returns>
        public CsvWriter Build()
        {
            if (stringBuilder != null)
            {
                if (encoding != null)
                    throw new ArgumentException(
                        "Setting the encoding doesn't help you since the encoding is set in the StringReader you passed.");
                if (isAppendMode)
                    throw new ArgumentException(
                        "Setting the appendMode doesn't work when not using the filePathAndName parameter.");
                return new CsvWriter(stringBuilder,
                    columnSeparator,
                    rowSeparator,
                    fieldDelimiter,
                    chunkSize,
                    quotingBehavior);
            }

            if (stringWriter != null)
            {
                if (encoding != null)
                    throw new ArgumentException(
                        "Setting the encoding doesn't help you since the encoding is set in the TextReader you passed.");
                if (isAppendMode)
                    throw new ArgumentException(
                        "Setting the appendMode doesn't work when not using the filePathAndName parameter.");
                return new CsvWriter(stringWriter,
                    columnSeparator,
                    rowSeparator,
                    fieldDelimiter,
                    chunkSize,
                    quotingBehavior);
            }

            if (textWriter != null)
            {
                if (encoding != null)
                    throw new ArgumentException(
                        "Setting the encoding doesn't help you since the encoding is set in the TextReader you passed.");
                if (isAppendMode)
                    throw new ArgumentException(
                        "Setting the appendMode doesn't work when not using the filePathAndName parameter.");
                return new CsvWriter(textWriter,
                    columnSeparator,
                    rowSeparator,
                    fieldDelimiter,
                    chunkSize,
                    quotingBehavior);
            }

            if (filePathAndName != null)
            {
                return new CsvWriter(filePathAndName,
                    columnSeparator,
                    rowSeparator,
                    fieldDelimiter,
                    chunkSize,
                    isAppendMode,
                    encoding,
                    quotingBehavior);
            }

            throw new ArgumentException(
                "You have to at least specify a StringReader, TextReader or filePathAndName different than null.");
        }

        public CsvWriterBuilder IsAppendMode()
        {
            isAppendMode = true;
            return this;
        }

        public CsvWriterBuilder QuotingBehavior(QuotingBehavior behavior)
        {
            quotingBehavior = behavior;
            return this;
        }
    }
}