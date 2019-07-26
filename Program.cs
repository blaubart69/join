using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using System.IO;

namespace join
{
    class Program
    {
        static log4net.ILog log = log4net.LogManager.GetLogger("root");

        static void Main(string[] args)
        {
            string tmpDbName = "joinTemp.db";

            SQLiteConnection.CreateFile(tmpDbName);
            SQLiteConnection db = new SQLiteConnection($"Data Source={tmpDbName};Version=3;");
            db.Open();

            var insertPwd = Task.Run(() =>
           {
               //new SQLiteCommand("create table pwd (hostname varchar(32), pwdLastSet varchar(32))", db).ExecuteNonQuery();
               new SQLiteCommand("create table pwd (hostname text, pwdLastSet text)", db).ExecuteNonQuery();
               long rows = InsertIntoTable(db, "pwd", Misc.ReadTsv(new StreamReader(@"u:\spindi\_werkbank\join\pwdLastSet.tsv")));
               log.Debug($"pwd inserted: {rows}");
           });

            var insertEudb = Task.Run(() =>
            {
                new SQLiteCommand("create table eurodb (BuildingID	varchar(32), city varchar(32),	street varchar(32),	Site varchar(32),	Hostname varchar(32),	BootFlag varchar(32),	ProductsToInstall varchar(32),	ReleasesToInstall varchar(32),	Status varchar(32),	LastAliveDateTime varchar(32))", db).ExecuteNonQuery();
                long rows = InsertIntoTable(db, "eurodb", Misc.ReadTsv(new StreamReader(@"u:\spindi\_werkbank\join\hd04machs.tsv")));
                log.Debug($"eurodb inserted: {rows}");
            });

            Task.WaitAll(insertPwd, insertEudb);

            string sql =
                @"select date(p.pwdLastSet) as pwdLastSet, e.* 
                from eurodb e join pwd p on p.hostname = e.Hostname";

            StringBuilder line = new StringBuilder();
            bool headerPrinted = false;
            foreach ( var row in Misc.ForEachRow(db, sql, null, rdr => rdr))
            {
                if (!headerPrinted)
                {
                    headerPrinted = true;
                    for (int i = 0; i < row.FieldCount; ++i)
                    {
                        line.Append(row.GetName(i));
                        line.Append("\t");
                    }
                    line.Length -= 1;
                    Console.WriteLine(line.ToString());
                }

                line.Length = 0;
                for (int i=0; i < row.FieldCount; ++i)
                {
                    line.Append(row[i] as string);
                    line.Append("\t");
                }
                if ( line.Length>0) line.Length -= 1;
                Console.WriteLine(line.ToString());
            }

            db.Close();
        }
        private static long InsertIntoTable(SQLiteConnection db, string tablename, IEnumerable<string[]> enumerable)
        {
            long rowsInserted = 0;

            using (var cmd = new SQLiteCommand() { Connection = db })
            using (SQLiteTransaction trans = db.BeginTransaction())
            {
                foreach (string[] vals in enumerable)
                {
                    string valString = String.Join(",", vals.Select(i => $"'{i}'"));
                    string stmt = $"insert into {tablename} values ({valString})";
                    log.Debug(stmt);
                    cmd.CommandText = stmt;
                    cmd.ExecuteNonQuery();
                    ++rowsInserted;
                }
                trans.Commit();
            }
            return rowsInserted;
        }

        static SQLiteConnection CreateAndConnect(string tmpDatabaseFile)
        {
            SQLiteConnection.CreateFile(tmpDatabaseFile);
            SQLiteConnection m_dbConnection = new SQLiteConnection($"Data Source={tmpDatabaseFile};Version=3;");
            m_dbConnection.Open();
            return m_dbConnection;
        }
    }
}
