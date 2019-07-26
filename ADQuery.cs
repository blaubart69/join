using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace join
{
    public class UserPass
    {
        public string user;
        public string pass;

        public bool Empty()
        {
            return String.IsNullOrEmpty(user) || String.IsNullOrEmpty(pass);
        }
    }
    class ADQuery
    {
        static log4net.ILog log = log4net.LogManager.GetLogger("root");

        public static IEnumerable<DirectoryEntry> Filter(string DNSDomain, string LdapPath, string LdapFilter, UserPass creds, string[] ProperiesToLoad)
        {
            DirectoryEntry searchRoot;
            string FullPath = $"LDAP://{DNSDomain}/{LdapPath}";

            if (creds == null || creds.Empty())
            {
                log.DebugFormat("Full Ldap path [{0}]", FullPath);
                searchRoot = new DirectoryEntry(FullPath);
            }
            else
            {
                log.DebugFormat("using user [{0}] (with given password) connecting to [{1}]", creds.user, FullPath);
                searchRoot = new DirectoryEntry(path: FullPath, username: creds.user, password: creds.pass);
            }

            using (searchRoot)
            using (var dSearch = new DirectorySearcher(searchRoot)
            {
                SearchScope = SearchScope.Subtree,
                Filter = LdapFilter
            })
            {
                if (ProperiesToLoad != null)
                {
                    dSearch.PropertiesToLoad.AddRange(ProperiesToLoad);
                }

                using (SearchResultCollection results = dSearch.FindAll())
                {
                    foreach (SearchResult result in results)
                    {
                        yield return result.GetDirectoryEntry();
                    }
                }
            }
        }
        public static string DnsDomainToLdapDomain(string DNSdomainname)
        {
            return
                String.Join(
                    separator: ","
                    , values: DNSdomainname.Split('.').Select(part => "DC=" + part));
        }
    }
}
