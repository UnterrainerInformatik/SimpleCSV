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
using JetBrains.Annotations;

namespace SimpleCsv
{
    /// <summary>
    ///     This data-structure represents a comma-separated-values file.
    ///     It helps in dealing with such files and delivers various manipulation routines.
    /// </summary>
    [PublicAPI]
    public partial class CsvReader : IDisposable
    {
        /// <summary>
        ///     The default chunk size (bufferSize / 2 - DEFAULT_ROW_SEPARATOR)...
        /// </summary>
        internal const int DEFAULT_CHUNK_SIZE = 16384;

        /// <summary>
        ///     The internal buffer that holds the data to be read or to be written.
        ///     Default value is the chunk size + the length of the default field delimiter.
        /// </summary>
        private char[] buffer = new char[DEFAULT_CHUNK_SIZE + 2];

        /// <summary>
        ///     The definitive chunkSize preset with the default value.
        /// </summary>
        private int chunkSize = DEFAULT_CHUNK_SIZE;

        private TextReader textReader;

        /// <summary>
        ///     The next character to be parsed.
        /// </summary>
        private char? nextChar;

        /// <summary>
        ///     The index of the next character in the buffer.
        /// </summary>
        private int nextCharBufferIndex;

        /// <summary>
        ///     The number of unparsed characters in the buffer.
        /// </summary>
        private int numberOfUnparsedChars;

        /// <summary>
        ///     Peeks into the buffer where all unhandled characters reside and
        ///     copies a lookahead-string consisting out of the next 'length'
        ///     unhandled characters. If the buffer ends before the next 'length'
        ///     characters could be read, then the string which is available is
        ///     returned. (Meaning that a _Peek(8) with the buffer [a23456] returns
        ///     'a23456'.)
        /// </summary>
        /// <param name="length">The length which should be read ahead.</param>
        /// <returns>
        ///     The preview-string consisting of the next 'length'
        ///     unhandled characters.
        /// </returns>
        private string Peek(int length)
        {
            lock (LockObject)
            {
                var sb = new StringBuilder();
                for (var i = 1; i <= length; i++)
                {
                    // nextChar has index: _nextCharBufferIndex...
                    var index = nextCharBufferIndex + i;
                    if (index < buffer.Length && index >= 0)
                    {
                        sb.Append(buffer[index]);
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        ///     Does essentially the same as <see cref="Peek" /> but counts and
        ///     returns the current nextChar as well. Example: buffer=[1234] &
        ///     nextChar=0 then _PeekInclusiveNextChar(3) would return [012].
        /// </summary>
        /// <param name="length">The length which should be read ahead.</param>
        /// <returns>
        ///     The preview-string consisting of the next 'length'
        ///     unhandled characters.
        /// </returns>
        private string PeekInclusiveNextChar(int length)
        {
            lock (LockObject)
            {
                return nextChar + Peek(length - 1);
            }
        }

        private void ReadNext()
        {
            lock (LockObject)
            {
                nextChar = GetNextChar();
                // EOF is not reached
                // AND the rest of the read chunk isn't long enough to contain a rowSeparator...
                // in this case we have to read ahead and keep the existing characters...
                if (nextChar != null && numberOfUnparsedChars + 1 < rowSeparator.Length)
                {
                    // copy the buffer to a temp array...
                    var temp = new char[numberOfUnparsedChars + 1];
                    Array.Copy(buffer, nextCharBufferIndex, temp, 0, numberOfUnparsedChars + 1);
                    // temp holds the unread chars plus the one just read.
                    // copy the temp-buffer back over the original buffer
                    // thus deleting all the old values except the ones
                    // we copied to the temp-buffer earlier on...
                    Array.Copy(temp, 0, buffer, 0, temp.Length);

                    ReadChunk(numberOfUnparsedChars + 1);
                    nextCharBufferIndex = 0;
                }
            }
        }

        /// <summary>
        ///     Omits some of the characters in the buffer by reading ahead for
        ///     <paramref name="numberOfCharacters" />. _ReadNext() is equivalent to
        ///     _ReadNext(1).
        /// </summary>
        /// <param name="numberOfCharacters">The number of characters.</param>
        private void ReadNext(int numberOfCharacters)
        {
            lock (LockObject)
            {
                for (var i = 0; i < numberOfCharacters; i++)
                {
                    ReadNext();
                }
            }
        }

        /// <summary>
        ///     Gets the next character in the buffer and reads the next chunk if
        ///     the buffer is empty.
        /// </summary>
        /// <returns>
        ///     The next character or <c>null</c> if the end of the file is
        ///     reached.
        /// </returns>
        private char? GetNextChar()
        {
            lock (LockObject)
            {
                if (numberOfUnparsedChars > 0)
                {
                    var c = buffer[nextCharBufferIndex + 1];
                    nextCharBufferIndex++;
                    numberOfUnparsedChars--;
                    return c;
                }

                // if the buffer is empty...
                ReadChunk(0);
                nextCharBufferIndex = -1;
                if (numberOfUnparsedChars <= 0)
                {
                    return null;
                }

                return GetNextChar();
            }
        }

        /// <summary>
        ///     Reads the next chunk of the input file.
        /// </summary>
        /// <param name="startIndex">
        ///     The start index in the buffer at which the
        ///     next chunk should be appended.
        /// </param>
        /// <exception cref="InvalidDataException">
        ///     The
        ///     <see cref="StreamReader" /> you provided is <c>null</c>.
        /// </exception>
        private void ReadChunk(int startIndex)
        {
            lock (LockObject)
            {
                var readChars = textReader.ReadBlock(buffer, startIndex, chunkSize);
                numberOfUnparsedChars += readChars;
            }
        }

        /// <summary>
        ///     Reads the next field of the CSV.
        /// </summary>
        /// <returns>
        ///     A string containing the next field of the underlying CSV.
        /// </returns>
        private string ReadField()
        {
            lock (LockObject)
            {
                var content = new StringBuilder();
                var isEscaped = false;
                while (nextChar != null)
                {
                    if (nextChar == fieldDelimiter)
                    {
                        if (isEscaped && Peek(1).Equals(fieldDelimiter + string.Empty))
                        {
                            // double fieldDelimiter.
                            // write one of them, omit the other...
                            ReadNext();
                        }
                        else
                        {
                            isEscaped = !isEscaped;
                            ReadNext();
                            continue;
                        }
                    }

                    if (PeekInclusiveNextChar(rowSeparator.Length).Equals(rowSeparator) && !isEscaped)
                    {
                        // we are at the last field in a row...
                        // and therefore this is the end of the field...
                        return content.ToString();
                    }

                    if (nextChar == columnSeparator && !isEscaped)
                    {
                        // we are at the end of this field...
                        return content.ToString();
                    }

                    content.Append(nextChar);
                    ReadNext();
                }

                return content.ToString();
            }
        }

        /// <summary>
        ///     Returns the next row, already split into fields. Returns <c>null</c>
        ///     if the end of the source is reached.
        /// </summary>
        /// <returns>
        ///     An array of string containing the fields of this row or
        ///     <c>null</c> if the end of the source has been reached.
        /// </returns>
        public List<string> ReadRow()
        {
            lock (LockObject)
            {
                var fields = new List<string>();
                if (nextChar == null)
                {
                    ReadNext();
                }

                while (nextChar != null && !PeekInclusiveNextChar(rowSeparator.Length).Equals(rowSeparator))
                {
                    fields.Add(ReadField());
                    if (nextChar == columnSeparator)
                    {
                        ReadNext();

                        // Without supression of this resharper error resharper tells us that the condition "_nextChar == null" in this
                        // case is always false.
                        // It's not right. The variable gets changed outside of the method via the previous call to _ReadNext();
                        // Resharper doesn't detect that and the NUnit-test "TestEmptyFieldDelimiters" fails if omitted.
                        // So the problem is connected with empty fields between field-delimiters which are not detected correctly when omitted.
// ReSharper disable ConditionIsAlwaysTrueOrFalse
                        if (nextChar == null || PeekInclusiveNextChar(rowSeparator.Length).Equals(rowSeparator))
// ReSharper restore ConditionIsAlwaysTrueOrFalse
                        {
                            fields.Add(string.Empty);
                        }
                    }
                }

                if (PeekInclusiveNextChar(rowSeparator.Length).Equals(rowSeparator))
                {
                    // reading fields stopped due to line end.
                    // omit the rowseperator...
                    ReadNext(rowSeparator.Length);
                }

                if (fields.Count == 0 && textReader.Peek() == -1)
                {
                    // this is the EOF and the result is empty...
                    return null;
                }

                return fields;
            }
        }

        /// <summary>
        ///     Reads all rows of the CSV and returns them in a List.
        ///     The resulting structure is a List-of-List-of-string.
        /// </summary>
        /// <returns></returns>
        public List<List<string>> ReadAllRows()
        {
            lock (LockObject)
            {
                var data = new List<List<string>>();
                var row = ReadRow();
                while (row != null)
                {
                    data.Add(row);
                    row = ReadRow();
                }

                if (data.Count == 0)
                {
                    // this is the EOF and the result is empty...
                    return null;
                }

                return data;
            }
        }

        /// <summary>
        ///     Gets the column separator.
        /// </summary>
        /// <value>The column separator.</value>
        public char ColumnSeparator => columnSeparator;

        /// <summary>
        ///     Gets the row separator.
        /// </summary>
        /// <value>The row separator.</value>
        public string RowSeparator => rowSeparator;

        /// <summary>
        ///     Gets the field delimiter.
        /// </summary>
        /// <value>The field delimiter.</value>
        public char? FieldDelimiter => fieldDelimiter;

        /// <summary>
        ///     Sets the size of the buffer and of the chunk.
        /// </summary>
        /// <param name="chunkSize">Size of the chunk.</param>
        private void SetChunkAndBufferSize(int chunkSize)
        {
            this.chunkSize = chunkSize;
            if (chunkSize < rowSeparator.Length)
            {
                this.chunkSize = rowSeparator.Length;
            }

            buffer = new char[this.chunkSize + rowSeparator.Length];
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (textReader == null)
            {
                return;
            }

            textReader.Close();
            textReader.Dispose();
            textReader = null;
        }
    }
}