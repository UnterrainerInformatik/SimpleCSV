using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SimpleCsv
{
    public class CsvBuilderBase <T> where T : CsvBuilderBase<T>
    {
        protected char columnSeparator = CsvBase.DEFAULT_COLUMN_SEPARATOR;
        protected string rowSeparator = CsvBase.DEFAULT_ROW_SEPARATOR;
        protected char? fieldDelimiter = CsvBase.DEFAULT_FIELD_DELIMITER;
        protected int chunkSize;
        [CanBeNull] protected Encoding encoding;

        public T ColumnSeparator(char separator)
        {
            columnSeparator = separator;
            return (T)this;
        }

        public T RowSeparator(string separator)
        {
            rowSeparator = separator;
            return (T)this;
        }

        public T FieldDelimiter(char? delimiter)
        {
            fieldDelimiter = delimiter;
            return (T) this;
        }

        public T Encoding(Encoding e)
        {
            encoding = e;
            return (T)this;
        }

        public T ChunkSize(int size)
        {
            chunkSize = size;
            return (T) this;
        }
    }
}
