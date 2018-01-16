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

using JetBrains.Annotations;

namespace SimpleCsv
{
    /// <summary>
    ///     Base class for all classes that handle CSV documents.
    /// </summary>
    [PublicAPI]
    public abstract class CsvBase
    {
        /// <summary>
        ///     The default value for the column separator.
        /// </summary>
        protected const char DEFAULT_COLUMN_SEPARATOR = ';';

        /// <summary>
        ///     The default value for the row separator.
        /// </summary>
        protected const string DEFAULT_ROW_SEPARATOR = "\r\n";

        /// <summary>
        ///     The default value for the field delimiter.
        /// </summary>
        protected static readonly char? DEFAULT_FIELD_DELIMITER = '"';

        /// <summary>
        ///     This is the lock-object for this class.
        /// </summary>
        protected readonly object LockObject = new object();

        /// <summary>
        ///     The value of the column separator that is used by the program.
        /// </summary>
        protected char columnSeparator = DEFAULT_COLUMN_SEPARATOR;

        /// <summary>
        ///     The value of the field delimiter that is used by the program.
        /// </summary>
        protected char? fieldDelimiter = DEFAULT_FIELD_DELIMITER;

        /// <summary>
        ///     The value of the row separator that is used by the program.
        /// </summary>
        protected string rowSeparator = DEFAULT_ROW_SEPARATOR;
    }
}