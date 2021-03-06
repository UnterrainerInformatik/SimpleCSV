﻿// *************************************************************************** 
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

namespace SimpleCsv.Reader
{
    public class CsvReaderBuilder : CsvBuilderBase<CsvReaderBuilder>
    {
        private readonly StringReader stringReader;
        private readonly TextReader textReader;
        private readonly string filePathAndName;

        private CsvReaderBuilder()
        {
            chunkSize = CsvReader.DEFAULT_CHUNK_SIZE;
        }

        public CsvReaderBuilder(StringReader stringReader) : this()
        {
            this.stringReader = stringReader;
        }

        public CsvReaderBuilder(TextReader textReader) : this()
        {
            this.textReader = textReader;
        }

        public CsvReaderBuilder(string filePathAndName) : this()
        {
            this.filePathAndName = filePathAndName;
        }

        /// <summary>
        ///     Builds an instance of <see cref="Reader.CsvReader" />.
        /// </summary>
        /// <returns>A <see cref="Reader.CsvReader" /></returns>
        public CsvReader Build()
        {
            if (stringReader != null)
            {
                if (encoding != null)
                    throw new ArgumentException(
                        "Setting the encoding doesn't help you since the encoding is set in the StringReader you passed.");
                return new CsvReader(stringReader, columnSeparator, rowSeparator, fieldDelimiter, chunkSize);
            }

            if (textReader != null)
            {
                if (encoding != null)
                    throw new ArgumentException(
                        "Setting the encoding doesn't help you since the encoding is set in the TextReader you passed.");
                return new CsvReader(textReader, columnSeparator, rowSeparator, fieldDelimiter, chunkSize);
            }

            if (filePathAndName != null)
            {
                return new CsvReader(filePathAndName, columnSeparator, rowSeparator, fieldDelimiter, encoding);
            }

            throw new ArgumentException(
                "You have to at least specify a StringReader, TextReader or filePathAndName different than null.");
        }
    }
}