using ExtremeSwank.OpenId;
using ExtremeSwank.OpenId.PlugIns.Discovery;
using System.Collections.Generic;
using ExtremeSwank.OpenId.PlugIns.Extensions;
using System;

namespace ExtremeSwank.OpenId.Tests
{
#if DEBUG
    public
#else
    internal
#endif
        static class UnitTests
    {
        public static void RunTests()
        {
            Normalization();
            Discovery();
        }

        static StateContainer SetupContainer()
        {
            StateContainer props = new StateContainer();

            new Xrds(props);
            new Yadis(props);
            new Html(props);

            new IdentityAuthentication(props);

            return props;
        }

        static void Normalization()
        {
            StateContainer props = SetupContainer();

            // Identity Normalization, per OpenID Spec
            Dictionary<string, string[]> TestList = new Dictionary<string, string[]>();
            TestList.Add("extremeswank.com", new string[] { "http://extremeswank.com/", "http://extremeswank.com/", "extremeswank.com" });
            TestList.Add("extremeswank.com/test", new string[] { "http://extremeswank.com/test", "http://extremeswank.com/test", "extremeswank.com/test" });
            TestList.Add("extremeswank.com/test/", new string[] { "http://extremeswank.com/test/", "http://extremeswank.com/test/", "extremeswank.com/test" });
            TestList.Add("http://extremeswank.com", new string[] { "http://extremeswank.com/", "http://extremeswank.com/", "extremeswank.com" });
            TestList.Add("http://extremeswank.com/", new string[] { "http://extremeswank.com/", "http://extremeswank.com/", "extremeswank.com" });
            TestList.Add("https://extremeswank.com", new string[] { "https://extremeswank.com/", "https://extremeswank.com/", "extremeswank.com" });
            TestList.Add("https://extremeswank.com/", new string[] { "https://extremeswank.com/", "https://extremeswank.com/", "extremeswank.com" });
            TestList.Add("=es", new string[] { "=es", "https://xri.net/=es", "=es" });
            TestList.Add("@xrid*extremeswank", new string[] { "@xrid*extremeswank", "https://xri.net/@xrid*extremeswank", "@xrid*extremeswank" });
            TestList.Add("xri://=es", new string[] { "=es", "https://xri.net/=es", "=es" });
            TestList.Add("xri://@xrid*extremeswank", new string[] { "@xrid*extremeswank", "https://xri.net/@xrid*extremeswank", "@xrid*extremeswank" });
            TestList.Add("getopenid.com/extremeswank", new string[] { "http://getopenid.com/extremeswank", "http://getopenid.com/extremeswank", "getopenid.com/extremeswank" });
            TestList.Add("profile.typekey.com/extremeswank", new string[] { "http://profile.typekey.com/extremeswank", "http://profile.typekey.com/extremeswank", "profile.typekey.com/extremeswank" });

            int countSuccess = 0;
            int countFailed = 0;
            foreach (string key in TestList.Keys)
            {
                NormalizationEntry ne = Utility.Normalize(key, props.DiscoveryPlugIns);
                if (ne.NormalizedId != TestList[key][0])
                {
                    Console.WriteLine("Failed Normalized ID test: " + ne.NormalizedId + " != " + TestList[key][0]);
                    countFailed++;
                }
                else { countSuccess++; }

                if (ne.DiscoveryUrl.AbsoluteUri != TestList[key][1])
                {
                    Console.WriteLine("Failed DiscoveryUrl test: " + ne.DiscoveryUrl + " != " + TestList[key][1]);
                    countFailed++;
                }
                else { countSuccess++; }

                if (ne.FriendlyId != TestList[key][2])
                {
                    Console.WriteLine("Failed Friendly ID test: " + ne.FriendlyId + " != " + TestList[key][2]);
                    countFailed++;
                }
                else { countSuccess++; }
            }
            Console.WriteLine("Identity Normalization tests: " + (countFailed + countSuccess) + " Total, " + countFailed + " Failed, " + countSuccess + " Passed");
            props = null;
        }

        static void Discovery()
        {
            StateContainer props = SetupContainer();

            Dictionary<string, string[]> TestList = new Dictionary<string, string[]>();

            TestList.Add("getopenid.com/extremeswank", new string[] { "http://getopenid.com/extremeswank" });
            TestList.Add("profile.typekey.com/extremeswank", new string[] { "http://profile.typekey.com/extremeswank/" });
            TestList.Add("extremeswank.com", new string[] { "http://extremeswank.com/" });
            TestList.Add("openid.aol.com/bugeyesx", new string[] { "http://openid.aol.com/bugeyesx" });
            TestList.Add("http://openid.aol.com/bugeyesx", new string[] { "http://openid.aol.com/bugeyesx" });
            TestList.Add("http://extremeswank.com", new string[] { "http://extremeswank.com/" });
            TestList.Add("http://extremeswank.com/", new string[] { "http://extremeswank.com/" });

            int countSuccess = 0;
            int countFailed = 0;

            foreach (string key in TestList.Keys)
            {
                DiscoveryResult dr = Utility.GetProviderUrl(key, props.DiscoveryPlugIns);
                if (dr.ClaimedId != TestList[key][0])
                {
                    Console.WriteLine("Failed Claimed ID discovery test: " + dr.ClaimedId + " != " + TestList[key][0]);
                    countFailed++;
                }
                else { countSuccess++;  }
            }
            Console.WriteLine("Identity Discovery tests: " + (countFailed + countSuccess) + " Total, " + countFailed + " Failed, " + countSuccess + " Passed");
            props = null;
        }
    }
}