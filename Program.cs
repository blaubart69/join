using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace join
{
    class Program
    {
        static log4net.ILog log = log4net.LogManager.GetLogger("root");

        static void Main(string[] args)
        {
            log.Info("start");
            string DNSdomain = "eap.dev.lan.at";

            foreach (var i in ADQuery.Filter(DNSdomain, ADQuery.DnsDomainToLdapDomain(DNSdomain), "(objectClass=computer)", null, new string[] {"pwdLastSet" }))
            {
                long pwdlastset = (long)i.Properties["pwdLastSet"][0];
                var name = i.Properties["Name"].Value;
                log.Info($"{name}\t{pwdlastset}");

            }
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
