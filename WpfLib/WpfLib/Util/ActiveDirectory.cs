using System;
using System.Security.Principal;

#if LDAP
using System.DirectoryServices;         // via project reference System.DirectoryServices.dll

namespace Ai.Util
{
    public class DomainData
    {
        // environment data
        public string user;
        public string domain;
        public string workstation;

        // LDAP data
        public string displayName;
        public string firstName;
        public string lastName;
        public string email;

        // return Sql data
        public int domainUserId;
        public int DomainUserId { get { return domainUserId; } set { domainUserId = value; } }

        public string FullName(int maxLen)
        {
            string ret = firstName ?? user;

            if (lastName != null && lastName.Length > 0)
                ret += " " + lastName;

            if (maxLen <= 0 || maxLen > ret.Length) return ret;

            return ret.Substring(0, maxLen);
        }
    }

    public class ActiveDirectory
    {
        public static void SetData(DomainData data, bool isNoDomain)
        {
            data.user = Environment.UserName;
            data.domain = Environment.UserDomainName;
            data.workstation = Environment.MachineName;

            data.displayName = "";
            data.email = "";
            data.domainUserId = 0;
            if (isNoDomain) return;

            string filter = string.Format("(&(ObjectClass={0})(sAMAccountName={1}))", "person", data.user);
            string[] properties = new string[] { "fullname" };

            // System.DirectoryServices.
            DirectoryEntry adRoot = new DirectoryEntry("LDAP://" + data.domain, null, null, AuthenticationTypes.Secure);
            DirectorySearcher searcher = new DirectorySearcher(adRoot);
            searcher.SearchScope = SearchScope.Subtree;
            searcher.ReferralChasing = ReferralChasingOption.All;
            searcher.PropertiesToLoad.AddRange(properties);
            searcher.Filter = filter;

            TimeSpan span = new TimeSpan(0, 0, 1);  // 2 secs
            searcher.ServerTimeLimit = span;
            searcher.ServerPageTimeLimit = span;

            try
            {
                SearchResult result = searcher.FindOne();

                if (result == null)
                {
                    Dialog.ShowError("Domain user not found");
                    return;
                }

                DirectoryEntry directoryEntry = result.GetDirectoryEntry();

                data.displayName = directoryEntry.Properties["displayName"][0].ToString();
                data.firstName = directoryEntry.Properties["givenName"][0].ToString();
                data.lastName = directoryEntry.Properties["sn"][0].ToString();
                data.email = directoryEntry.Properties["mail"][0].ToString();
            }
            catch
            {
                data.displayName = data.user;
                data.firstName = data.user;
                data.lastName = "";
                data.email = "";
            }

        }
    }
}

#endif