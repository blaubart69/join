using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace join
{
    class Misc
    {
        public static IEnumerable<string[]> ReadTsv(TextReader tr)
        {
            return ReadLines(tr).Select(line => line.Split('\t'));
        }
        public static IEnumerable<string> ReadLines(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
        public static IEnumerable<T> ForEachRow<T>(SQLiteConnection OpenedConnection, string SqlCommand, SQLiteParameter[] parameters, Func<SQLiteDataReader, T> Converter)
        {
            using (SQLiteCommand SqlCmd = new SQLiteCommand(SqlCommand))
            {
                SqlCmd.Connection = OpenedConnection;

                if (parameters != null && parameters.Length > 0)
                {
                    SqlCmd.Parameters.AddRange(parameters);
                }

                using (SQLiteDataReader rdr = SqlCmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        yield return Converter(rdr);
                    }
                }
            }
        }

    }
}
