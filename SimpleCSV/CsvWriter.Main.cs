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

namespace SimpleCsv
{
    /// <summary>
    ///     This data-structure represents a comma-separated-values file.
    ///     It helps in dealing with such files and delivers various manipulation routines.
    /// </summary>
    [PublicAPI]
    public partial class CsvWriter : CsvBase, IDisposable
    {
        private TextWriter textWriter;

        /// <summary>
        ///     The default chunk size (bufferSize / 2 - DEFAULT_ROW_SEPARATOR)...
        /// </summary>
        private const int DEFAULT_CHUNK_SIZE = 1;

        /// <summary>
        ///     The internal buffer that holds the data to be read or to be written.
        ///     Default value is the chunk size + the length of the default field delimiter.
        /// </summary>
        private char[] buffer = new char[DEFAULT_CHUNK_SIZE];

        /// <summary>
        ///     The definitive chunkSize preset with the default value.
        /// </summary>
        private int chunkSize = DEFAULT_CHUNK_SIZE;

        private int bufferCount;
        private int numberOfUnusedBufferCharacters;

        /// <summary>
        ///     Boolean variable that tells the program if it is necessary to insert
        ///     a columnSeparator or not...
        /// </summary>
        private bool isFirstFieldInRow = true;

        /// <summary>
        ///     Tells the program if the fieldDelimiter is initialized (set) or
        ///     not to execute the initializations only once...
        /// </summary>
        private bool isInitialized;

        /// <summary>
        ///     Is the fieldDelimiter that is really used (string.Empty if it was
        ///     <c>null</c>, etc...)...
        /// </summary>
        private string usedFieldDelimiter = string.Empty;

        /// <summary>
        ///     Prevents multiple concatenation of the escaped fieldDelimiter by
        ///     doing it once...
        /// </summary>
        private string doubleFieldDelimiter = string.Empty;

        /// <summary>
        ///     Tells the program to always set quotes (fieldDelimiters) or only if
        ///     necessary...
        /// </summary>
        private readonly QuotingBehavior quotingBehavior = QuotingBehavior.MINIMAL;

        /// <summary>
        ///     Initializes the used field delimiter, the double field delimiter and various other things.
        ///     It does this only once (with the help to the boolean variable
        ///     <see cref="isInitialized" />.
        /// </summary>
        private void _Initialize()
        {
            lock (LockObject)
            {
                if (!isInitialized)
                {
                    if (fieldDelimiter.HasValue)
                    {
                        usedFieldDelimiter = string.Empty + fieldDelimiter.Value;
                        doubleFieldDelimiter = string.Empty + fieldDelimiter.Value + fieldDelimiter.Value;
                    }

                    numberOfUnusedBufferCharacters = buffer.Length;
                    isInitialized = true;
                }
            }
        }

        /// <summary>
        ///     Writes a string to the buffer.
        ///     Flushes it if necessary.
        /// </summary>
        /// <param name="text">The text to write.</param>
        private void _Write(string text)
        {
            if (numberOfUnusedBufferCharacters < text.Length)
            {
                // the buffer isn't large enough...
                // we have to split, flush and add the rest after flushing...
                string t = text;
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

                    _Write(sub);
                }
            }
            else
            {
                // everything is allright. The buffer is big enough...
                foreach (char c in text)
                {
                    _WriteToBuffer(c);
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
        public void Flush()
        {
            textWriter.Write(buffer, 0, bufferCount);
            bufferCount = 0;
            numberOfUnusedBufferCharacters = buffer.Length;
        }

        /// <summary>
        ///     Calls flush on the underlying text-writer.
        /// </summary>
        public void FlushUnderlyingWriter()
        {
            textWriter?.Flush();
        }

        private void _WriteToBuffer(char c)
        {
            buffer[bufferCount] = c;
            bufferCount++;
            numberOfUnusedBufferCharacters--;
        }

        private bool _IsUseFieldDelimiter(string csvData)
        {
            bool isFieldDelimiterNeeded = csvData.Contains(usedFieldDelimiter) || csvData.Contains(rowSeparator) ||
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
        public void Write()
        {
            lock (LockObject)
            {
                Write(string.Empty);
            }
        }

        /// <summary>
        ///     Writes a rowSeparator.
        /// </summary>
        public void WriteLine()
        {
            lock (LockObject)
            {
                _Write(rowSeparator);
                isFirstFieldInRow = true;
            }
        }

        /// <summary>
        ///     Writes a field to the CSV and appends a columnSeparator in front of
        ///     the new entry if necessary. Appends a rowSeparator at the end.
        /// </summary>
        /// <param name="csvData">The CSV data.</param>
        public void WriteLine(string csvData)
        {
            lock (LockObject)
            {
                Write(csvData);
                WriteLine();
            }
        }

        /// <summary>
        ///     Writes a field to the CSV and appends a columnSeparator in front of
        ///     the new entry if necessary.
        /// </summary>
        /// <param name="csvData">The CSV data.</param>
        public void Write(string csvData)
        {
            lock (LockObject)
            {
                if (isFirstFieldInRow)
                {
                    isFirstFieldInRow = false;
                }
                else
                {
                    _Write(columnSeparator.ToString());
                }

                string dataToWrite = csvData ?? string.Empty;

                _Initialize();
                if (_IsUseFieldDelimiter(dataToWrite))
                {
                    // quote the field...
                    _Write(usedFieldDelimiter);
                    // escape string delimiter...
                    _Write(dataToWrite.Replace(usedFieldDelimiter, doubleFieldDelimiter));
                    _Write(usedFieldDelimiter);
                }
                else
                {
                    _Write(dataToWrite);
                }
            }
        }

        /// <summary>
        ///     Writes a row to the CSV ending with a rowSeparator. Treats the
        ///     target as if it was empty, or at least in a newline-position (empty
        ///     row, no field inserted yet).
        /// </summary>
        /// <param name="csvData">The CSV data.</param>
        public void WriteLine(List<string> csvData)
        {
            lock (LockObject)
            {
                _Initialize();
                Write(csvData);
                WriteLine();
            }
        }

        /// <summary>
        ///     Writes a row to the CSV not ending with a rowSeparator. Treats the
        ///     target as if it was empty, or at least in a newline-position (empty
        ///     row, no field inserted yet).
        /// </summary>
        /// <param name="csvData">The CSV data.</param>
        public void Write(List<string> csvData)
        {
            lock (LockObject)
            {
                if (csvData == null || csvData.Count == 0)
                {
                    return;
                }

                _Initialize();
                int count = csvData.Count;
                for (int i = 0; i < count; i++)
                {
                    string fieldData = csvData[i];
                    Write(fieldData);
                }
            }
        }

        /// <summary>
        ///     Writes all rows to a CSV. Treats the target as if it was empty, or
        ///     at least in a newline-position (empty row, no field inserted yet).
        /// </summary>
        /// <param name="csvData">The CSV data.</param>
        public void Write(List<List<string>> csvData)
        {
            lock (LockObject)
            {
                _Initialize();
                int count = csvData.Count;
                for (int i = 0; i < count; i++)
                {
                    List<string> rowData = csvData[i];
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
        }

        /// <summary>
        ///     Sets the size of the buffer and of the chunk.
        /// </summary>
        /// <param name="size">Size of the chunk.</param>
        private void _SetChunkAndBufferSize(int size)
        {
            chunkSize = size;
            if (size < 1)
            {
                chunkSize = 1;
            }

            buffer = new char[chunkSize];
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (textWriter == null)
            {
                return;
            }

            Flush();
            textWriter.Flush();
            textWriter.Close();
            textWriter.Dispose();
            textWriter = null;
        }
    }
}