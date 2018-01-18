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
using JetBrains.Annotations;

namespace SimpleCsv.Writer
{
    /// <summary>
    ///     This data-structure represents a comma-separated-values file.
    ///     It helps in dealing with such files and delivers various manipulation routines.
    /// </summary>
    [PublicAPI]
    public partial class CsvWriter : CsvBase, IDisposable
    {
        private TextWriter textWriter;
        internal const int DEFAULT_CHUNK_SIZE = 1;
        private char[] buffer = new char[DEFAULT_CHUNK_SIZE];
        private int chunkSize = DEFAULT_CHUNK_SIZE;
        private int bufferCount;
        private int numberOfUnusedBufferCharacters;
        private bool isFirstFieldInRow = true;
        private bool isInitialized;
        private string usedFieldDelimiter = string.Empty;
        private string doubleFieldDelimiter = string.Empty;
        private readonly QuotingBehavior quotingBehavior = QuotingBehavior.MINIMAL;

        /// <summary>
        ///     Initializes the used field delimiter, the double field delimiter and various other things.
        ///     It does this only once (with the help to the boolean variable
        ///     <see cref="isInitialized" />.
        /// </summary>
        private void Initialize()
        {
            lock (LockObject)
            {
                if (isInitialized) return;
                if (fieldDelimiter.HasValue)
                {
                    usedFieldDelimiter = string.Empty + fieldDelimiter.Value;
                    doubleFieldDelimiter = string.Empty + fieldDelimiter.Value + fieldDelimiter.Value;
                }

                numberOfUnusedBufferCharacters = buffer.Length;
                isInitialized = true;
            }
        }

        /// <summary>
        ///     Writes a string to the buffer.
        ///     Flushes it if necessary.
        /// </summary>
        /// <param name="text">The text to write.</param>
        private void WriteInternal(string text)
        {
            if (numberOfUnusedBufferCharacters < text.Length)
            {
                // the buffer isn't large enough...
                // we have to split, flush and add the rest after flushing...
                var t = text;
                while (!string.IsNullOrEmpty(t))
                {
                    string sub;
                    if (t.Length > numberOfUnusedBufferCharacters)
                    {
                        sub = t.Substring(0, numberOfUnusedBufferCharacters);
                        t = t.Substring(numberOfUnusedBufferCharacters);
                    }
                    else
                    {
                        sub = t;
                        t = string.Empty;
                    }

                    WriteInternal(sub);
                }
            }
            else
            {
                // everything is allright. The buffer is big enough...
                foreach (var c in text)
                {
                    WriteToBuffer(c);
                }

                if (numberOfUnusedBufferCharacters == 0)
                {
                    Flush();
                }
            }
        }

        /// <summary>
        ///     Flushes all internal buffers to the underlying writer.
        /// </summary>
        public CsvWriter Flush()
        {
            textWriter.Write(buffer, 0, bufferCount);
            bufferCount = 0;
            numberOfUnusedBufferCharacters = buffer.Length;
            return this;
        }

        /// <summary>
        ///     Calls flush on the underlying text-writer.
        /// </summary>
        public CsvWriter FlushUnderlyingWriter()
        {
            textWriter?.Flush();
            return this;
        }

        private void WriteToBuffer(char c)
        {
            buffer[bufferCount] = c;
            bufferCount++;
            numberOfUnusedBufferCharacters--;
        }

        private bool IsUseFieldDelimiter(string csvData)
        {
            var isFieldDelimiterNeeded = csvData.Contains(usedFieldDelimiter) || csvData.Contains(rowSeparator) ||
                                         csvData.Contains(string.Empty + columnSeparator);
            if (!string.IsNullOrEmpty(usedFieldDelimiter) &&
                (isFieldDelimiterNeeded || quotingBehavior == QuotingBehavior.ALL))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Writes a columnSeparator.
        /// </summary>
        public CsvWriter Write()
        {
            lock (LockObject)
            {
                Write(string.Empty);
            }

            return this;
        }

        /// <summary>
        ///     Writes a rowSeparator.
        /// </summary>
        public CsvWriter WriteLine()
        {
            lock (LockObject)
            {
                WriteInternal(rowSeparator);
                isFirstFieldInRow = true;
            }

            return this;
        }

        /// <summary>
        ///     Writes a field to the CSV and appends a columnSeparator in front of
        ///     the new entry if necessary. Appends a rowSeparator at the end.
        /// </summary>
        /// <param name="csvData">The CSV data.</param>
        public CsvWriter WriteLine(string csvData)
        {
            lock (LockObject)
            {
                Write(csvData);
                WriteLine();
            }

            return this;
        }

        /// <summary>
        ///     Writes a field to the CSV and appends a columnSeparator in front of
        ///     the new entry if necessary.
        /// </summary>
        /// <param name="csvData">The CSV data.</param>
        public CsvWriter Write(string csvData)
        {
            lock (LockObject)
            {
                if (isFirstFieldInRow)
                {
                    isFirstFieldInRow = false;
                }
                else
                {
                    WriteInternal(columnSeparator.ToString());
                }

                var dataToWrite = csvData ?? string.Empty;

                Initialize();
                if (IsUseFieldDelimiter(dataToWrite))
                {
                    // quote the field...
                    WriteInternal(usedFieldDelimiter);
                    // escape string delimiter...
                    WriteInternal(dataToWrite.Replace(usedFieldDelimiter, doubleFieldDelimiter));
                    WriteInternal(usedFieldDelimiter);
                }
                else
                {
                    WriteInternal(dataToWrite);
                }
            }

            return this;
        }

        /// <summary>
        ///     Writes a row to the CSV ending with a rowSeparator. Treats the
        ///     target as if it was empty, or at least in a newline-position (empty
        ///     row, no field inserted yet).
        /// </summary>
        /// <param name="csvData">The CSV data.</param>
        public CsvWriter WriteLine(List<string> csvData)
        {
            lock (LockObject)
            {
                Initialize();
                Write(csvData);
                WriteLine();
            }

            return this;
        }

        /// <summary>
        ///     Writes a row to the CSV not ending with a rowSeparator. Treats the
        ///     target as if it was empty, or at least in a newline-position (empty
        ///     row, no field inserted yet).
        /// </summary>
        /// <param name="csvData">The CSV data.</param>
        public CsvWriter Write(List<string> csvData)
        {
            lock (LockObject)
            {
                if (csvData == null || csvData.Count == 0)
                {
                    return this;
                }

                Initialize();
                var count = csvData.Count;
                for (var i = 0; i < count; i++)
                {
                    var fieldData = csvData[i];
                    Write(fieldData);
                }
            }

            return this;
        }

        /// <summary>
        ///     Writes all rows to a CSV. Treats the target as if it was empty, or
        ///     at least in a newline-position (empty row, no field inserted yet).
        /// </summary>
        /// <param name="csvData">The CSV data.</param>
        public CsvWriter Write(List<List<string>> csvData)
        {
            lock (LockObject)
            {
                Initialize();
                var count = csvData.Count;
                for (var i = 0; i < count; i++)
                {
                    var rowData = csvData[i];
                    if (i == count - 1)
                    {
                        // this is the last one...
                        Write(rowData);
                    }
                    else
                    {
                        WriteLine(rowData);
                    }
                }
            }

            return this;
        }

        /// <summary>
        ///     Sets the size of the buffer and of the chunk.
        /// </summary>
        /// <param name="size">Size of the chunk.</param>
        private void SetChunkAndBufferSize(int size)
        {
            chunkSize = size;
            if (size < 1)
            {
                chunkSize = 1;
            }

            buffer = new char[chunkSize];
        }

        public void Dispose()
        {
            Flush();
            textWriter?.Flush();

            if (!closeStream)
                return;
            textWriter?.Close();
            textWriter?.Dispose();
        }
    }
}