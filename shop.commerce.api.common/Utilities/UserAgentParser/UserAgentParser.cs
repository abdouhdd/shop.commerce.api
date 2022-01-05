﻿namespace shop.commerce.api.common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    /// <summary>
    /// this class is used to parse the <see href="https://en.wikipedia.org/wiki/User_agent">UserAgent</see>
    /// to get information about the client request
    /// </summary>
    public sealed partial class UserAgentParser
    {
        /// <summary>
        /// The constant string value used to signal an unknown match for a given property or value. This
        /// is by default the string "Other".
        /// </summary>
        public const string Other = "Other";

        /// <summary>
        /// Returns a <see cref="UserAgentParser"/> instance based on the regex definitions in a yaml string
        /// </summary>
        /// <param name="yaml">a string containing yaml definitions of reg-ex</param>
        /// <param name="parserOptions">specifies the options for the parser</param>
        /// <returns>A <see cref="UserAgentParser"/> instance parsing user agent strings based on the regexes defined in the yaml string</returns>
        public static UserAgentParser FromYaml(string yaml, UserAgentParserOptions parserOptions = null)
            => new UserAgentParser(new MinimalYamlParser(yaml), parserOptions);

        /// <summary>
        /// Returns a <see cref="UserAgentParser"/> instance based on the embedded regex definitions.
        /// <remarks>The embedded regex definitions may be outdated. Consider passing in external yaml definitions using <see cref="UserAgentParser.FromYaml"/></remarks>
        /// </summary>
        /// <param name="parserOptions">specifies the options for the parser</param>
        /// <returns></returns>
        public static UserAgentParser GetDefault(UserAgentParserOptions parserOptions = null)
            => FromYaml(_regex, parserOptions);
        //{
        //    using (var stream = typeof(UserAgentParser).GetTypeInfo().Assembly.GetManifestResourceStream("shop.commerce.api.common.Utilities.Common.Resource.resources.regexes.yaml"))
        //    {
        //        using (var reader = new StreamReader(stream))
        //            return new UserAgentParser(new MinimalYamlParser(reader.ReadToEnd()), parserOptions);
        //    }
        //}

        /// <summary>
        /// Parse a user agent string and obtain all client information
        /// </summary>
        public UserAgentInfo Parse(string uaString)
        {
            var os = ParseOS(uaString);
            var device = ParseDevice(uaString);
            var ua = ParseUserAgent(uaString);
            return new UserAgentInfo(uaString, os, device, ua);
        }

        /// <summary>
        /// Parse a user agent string and obtain the OS information
        /// </summary>
        public OS ParseOS(string uaString) { return _osParser(uaString); }

        /// <summary>
        /// Parse a user agent string and obtain the device information
        /// </summary>
        public Device ParseDevice(string uaString) { return _deviceParser(uaString); }

        /// <summary>
        /// Parse a user agent string and obtain the UserAgent information
        /// </summary>
        public UserAgent ParseUserAgent(string uaString) { return _userAgentParser(uaString); }
    }

    /// <summary>
    /// partial part for <see cref="UserAgentParser"/>
    /// </summary>
    public sealed partial class UserAgentParser
    {
        private readonly Func<string, OS> _osParser;
        private readonly Func<string, Device> _deviceParser;
        private readonly Func<string, UserAgent> _userAgentParser;

        private UserAgentParser(MinimalYamlParser yamlParser, UserAgentParserOptions options)
        {
            var config = new Config(options ?? new UserAgentParserOptions());

            _userAgentParser = CreateParser(Read(yamlParser.ReadMapping("user_agent_parsers"), config.UserAgentSelector), new UserAgent(Other, null, null, null));
            _osParser = CreateParser(Read(yamlParser.ReadMapping("os_parsers"), config.OSSelector), new OS(Other, null, null, null, null));
            _deviceParser = CreateParser(Read(yamlParser.ReadMapping("device_parsers"), config.DeviceSelector), new Device(Other, string.Empty, string.Empty));
        }

        private static IEnumerable<T> Read<T>(IEnumerable<Dictionary<string, string>> entries, Func<Func<string, string>, T> selector)
            => from cm in entries select selector(cm.Find);

        private static Func<string, T> CreateParser<T>(IEnumerable<Func<string, T>> parsers, T defaultValue) where T : class
            => CreateParser(parsers, defaultValue, t => t);

        private static Func<string, TResult> CreateParser<T, TResult>(IEnumerable<Func<string, T>> parsers, T defaultValue, Func<T, TResult> selector) where T : class
        {
            parsers = parsers?.ToArray() ?? Enumerable.Empty<Func<string, T>>();
            return ua => selector(parsers.Select(p => p(ua)).FirstOrDefault(m => m != null) ?? defaultValue);
        }

        private static class Parsers
        {
            public static Func<string, OS> OS(Regex regex, string osReplacement, string v1Replacement, string v2Replacement, string v3Replacement, string v4Replacement)
            {
                // For variable replacements to be consistent the order of the linq statements are important ($1
                // is only available to the first 'from X in Replace(..)' and so forth) so a a bit of conditional
                // is required to get the creations to work. This is backed by unit tests
                if (v1Replacement == "$1")
                {
                    if (v2Replacement == "$2")
                    {
                        return Create(regex, from v1 in Replace(v1Replacement, "$1")
                                             from v2 in Replace(v2Replacement, "$2")
                                             from v3 in Replace(v3Replacement, "$3")
                                             from v4 in Replace(v4Replacement, "$4")
                                             from family in Replace(osReplacement, "$5")
                                             select new OS(family, v1, v2, v3, v4));
                    }

                    return Create(regex, from v1 in Replace(v1Replacement, "$1")
                                         from family in Replace(osReplacement, "$2")
                                         from v2 in Replace(v2Replacement, "$3")
                                         from v3 in Replace(v3Replacement, "$4")
                                         from v4 in Replace(v4Replacement, "$5")
                                         select new OS(family, v1, v2, v3, v4));
                }

                return Create(regex, from family in Replace(osReplacement, "$1")
                                     from v1 in Replace(v1Replacement, "$2")
                                     from v2 in Replace(v2Replacement, "$3")
                                     from v3 in Replace(v3Replacement, "$4")
                                     from v4 in Replace(v4Replacement, "$5")
                                     select new OS(family, v1, v2, v3, v4));
            }

            public static Func<string, Device> Device(Regex regex, string familyReplacement, string brandReplacement, string modelReplacement)
            {
                return Create(regex, from family in ReplaceAll(familyReplacement)
                                     from brand in ReplaceAll(brandReplacement)
                                     from model in ReplaceAll(modelReplacement)
                                     select new Device(family, brand, model));
            }

            public static Func<string, UserAgent> UserAgent(Regex regex, string familyReplacement, string majorReplacement, string minorReplacement, string patchReplacement)
            {
                return Create(regex, from family in Replace(familyReplacement, "$1")
                                     from v1 in Replace(majorReplacement, "$2")
                                     from v2 in Replace(minorReplacement, "$3")
                                     from v3 in Replace(patchReplacement, "$4")
                                     select new UserAgent(family, v1, v2, v3));
            }

            private static Func<Match, IEnumerator<int>, string> Replace(string replacement)
            {
                return replacement != null ? Select(_ => replacement) : Select();
            }

            private static Func<Match, IEnumerator<int>, string> Replace(string replacement, string token)
            {
                return replacement != null && replacement.Contains(token)
                     ? Select(s => s != null ? replacement.ReplaceFirstOccurrence(token, s) : replacement)
                     : Replace(replacement);
            }

            private static readonly string[] _allReplacementTokens = new string[]
            { "$1","$2","$3","$4","$5","$6","$7","$8","$91", };

            private static Func<Match, IEnumerator<int>, string> ReplaceAll(string replacement)
            {
                if (replacement == null)
                    return Select();

                string ReplaceFunction(string replacementString, string matchedGroup, string token)
                {
                    return matchedGroup != null
                        ? replacementString.ReplaceFirstOccurrence(token, matchedGroup)
                        : replacementString;
                }

                return (m, num) =>
                {
                    var finalString = replacement;
                    if (finalString.Contains("$"))
                    {
                        var groups = m.Groups;
                        for (int i = 0; i < _allReplacementTokens.Length; i++)
                        {
                            int tokenNumber = i + 1;
                            string token = _allReplacementTokens[i];
                            if (finalString.Contains(token))
                            {
                                var replacementText = string.Empty;
                                Group group;
                                if (tokenNumber <= groups.Count && (group = groups[tokenNumber]).Success)
                                    replacementText = group.Value;

                                finalString = ReplaceFunction(finalString, replacementText, token);
                            }
                            if (!finalString.Contains("$"))
                                break;
                        }
                    }
                    return finalString;
                };
            }

            private static Func<Match, IEnumerator<int>, string> Select()
            {
                return Select(v => v);
            }

            private static Func<Match, IEnumerator<int>, T> Select<T>(Func<string, T> selector)
            {
                return (m, num) =>
                {
                    if (!num.MoveNext()) throw new InvalidOperationException();
                    var groups = m.Groups; Group group;
                    return selector(num.Current <= groups.Count && (group = groups[num.Current]).Success
                                    ? group.Value : null);
                };
            }

            private static Func<string, T> Create<T>(Regex regex, Func<Match, IEnumerator<int>, T> binder)
            {
                return input =>
                {
#if REGEX_MATCHTIMEOUT
                    try
                    {
                        var m = regex.Match(input);
                        var num = Generate(1, n => n + 1);
                        return m.Success ? binder(m, num) : default(T);
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        // we'll simply swallow this exception and return the default (non-matched)
                        return default(T);
                    }
#else
                    var m = regex.Match(input);
                    var num = Generate(1, n => n + 1);
                    return m.Success ? binder(m, num) : default;
#endif
                };
            }

            private static IEnumerator<T> Generate<T>(T initial, Func<T, T> next)
            {
                for (var state = initial; ; state = next(state))
                    yield return state;
                // ReSharper disable once FunctionNeverReturns
            }
        }

        private class Config
        {
            private readonly UserAgentParserOptions _options;

            internal Config(UserAgentParserOptions options)
            {
                _options = options;
            }

            // ReSharper disable once InconsistentNaming
            public Func<string, OS> OSSelector(Func<string, string> indexer)
            {
                var regex = Regex(indexer, "OS");
                var os = indexer("os_replacement");
                var v1 = indexer("os_v1_replacement");
                var v2 = indexer("os_v2_replacement");
                var v3 = indexer("os_v3_replacement");
                var v4 = indexer("os_v4_replacement");
                return Parsers.OS(regex, os, v1, v2, v3, v4);
            }

            public Func<string, UserAgent> UserAgentSelector(Func<string, string> indexer)
            {
                var regex = Regex(indexer, "User agent");
                var family = indexer("family_replacement");
                var v1 = indexer("v1_replacement");
                var v2 = indexer("v2_replacement");
                var v3 = indexer("v3_replacement");
                return Parsers.UserAgent(regex, family, v1, v2, v3);
            }

            public Func<string, Device> DeviceSelector(Func<string, string> indexer)
            {
                var regex = Regex(indexer, "Device", indexer("regex_flag"));
                var device = indexer("device_replacement");
                var brand = indexer("brand_replacement");
                var model = indexer("model_replacement");
                return Parsers.Device(regex, device, brand, model);
            }

            private Regex Regex(Func<string, string> indexer, string key, string regexFlag = null)
            {
                var pattern = indexer("regex");
                if (pattern == null)
                    throw new Exception($"{key} is missing regular expression specification.");

                // Some expressions in the regex.yaml file causes parsing errors
                // in .NET such as the \_ token so need to alter them before
                // proceeding.

                if (pattern.IndexOf(@"\_", StringComparison.Ordinal) >= 0)
                    pattern = pattern.Replace(@"\_", "_");

                //Singleline: User agent strings do not contain newline characters. RegexOptions.Singleline improves performance.
                //CultureInvariant: The interpretation of a user agent never depends on the current locale.
                RegexOptions options = RegexOptions.Singleline | RegexOptions.CultureInvariant;

                if ("i".Equals(regexFlag))
                {
                    options |= RegexOptions.IgnoreCase;
                }

#if REGEX_COMPILATION
                if (_options.UseCompiledRegex)
                {
                    options |= RegexOptions.Compiled;
                }
#endif

#if REGEX_MATCHTIMEOUT

                return new Regex(pattern, options, _options.MatchTimeOut);
#else
                return new Regex(pattern, options);
#endif
            }
        }

        #region the regex patterns

        private static string _regex = @"
user_agent_parsers:
  #### SPECIAL CASES TOP ####

  # CFNetwork Podcast catcher Applications
  - regex: '(ESPN)[%20| ]+Radio/(\d+)\.(\d+)\.(\d+) CFNetwork'
  - regex: '(Antenna)/(\d+) CFNetwork'
    family_replacement: 'AntennaPod'
  - regex: '(TopPodcasts)Pro/(\d+) CFNetwork'
  - regex: '(MusicDownloader)Lite/(\d+)\.(\d+)\.(\d+) CFNetwork'
  - regex: '^(.*)-iPad\/(\d+)(?:\.(\d+)|)(?:\.(\d+)|)(?:\.(\d+)|) CFNetwork'
  - regex: '^(.*)-iPhone/(\d+)(?:\.(\d+)|)(?:\.(\d+)|)(?:\.(\d+)|) CFNetwork'
  - regex: '^(.*)/(\d+)(?:\.(\d+)|)(?:\.(\d+)|)(?:\.(\d+)|) CFNetwork'

  # Podcast catchers
  - regex: '(espn\.go)'
    family_replacement: 'ESPN'
  - regex: '(espnradio\.com)'
    family_replacement: 'ESPN'
  - regex: 'ESPN APP$'
    family_replacement: 'ESPN'
  - regex: '(audioboom\.com)'
    family_replacement: 'AudioBoom'
  - regex: ' (Rivo) RHYTHM'

  # @note: iOS / OSX Applications
  - regex: '(CFNetwork)(?:/(\d+)\.(\d+)(?:\.(\d+)|)|)'
    family_replacement: 'CFNetwork'

  # Pingdom
  - regex: '(Pingdom\.com_bot_version_)(\d+)\.(\d+)'
    family_replacement: 'PingdomBot'
  # 'Mozilla/5.0 (Unknown; Linux x86_64) AppleWebKit/534.34 (KHTML, like Gecko) PingdomTMS/0.8.5 Safari/534.34'
  - regex: '(PingdomTMS)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'PingdomBot'

  # PTST / WebPageTest.org crawlers
  - regex: ' (PTST)/(\d+)(?:\.(\d+)|)$'
    family_replacement: 'WebPageTest.org bot'

  # Datanyze.com spider
  - regex: 'X11; (Datanyze); Linux'

  # New Relic Pinger
  - regex: '(NewRelicPinger)/(\d+)\.(\d+)'
    family_replacement: 'NewRelicPingerBot'

  # Tableau
  - regex: '(Tableau)/(\d+)\.(\d+)'
    family_replacement: 'Tableau'

  # Salesforce
  - regex: '(Salesforce)(?:.)\/(\d+)\.(\d?)'

  #StatusCake
  - regex: '(\(StatusCake\))'
    family_replacement: 'StatusCakeBot'

  # Facebook
  - regex: '(facebookexternalhit)/(\d+)\.(\d+)'
    family_replacement: 'FacebookBot'

  # Google Plus
  - regex: 'Google.*/\+/web/snippet'
    family_replacement: 'GooglePlusBot'

  # Gmail
  - regex: 'via ggpht\.com GoogleImageProxy'
    family_replacement: 'GmailImageProxy'

  # Yahoo
  - regex: 'YahooMailProxy; https://help\.yahoo\.com/kb/yahoo-mail-proxy-SLN28749\.html'
    family_replacement: 'YahooMailProxy'

  # Twitter
  - regex: '(Twitterbot)/(\d+)\.(\d+)'
    family_replacement: 'Twitterbot'

  # Bots Pattern 'name/0.0.0'
  - regex: '/((?:Ant-|)Nutch|[A-z]+[Bb]ot|[A-z]+[Ss]pider|Axtaris|fetchurl|Isara|ShopSalad|Tailsweep)[ \-](\d+)(?:\.(\d+)|)(?:\.(\d+)|)'
  # Bots Pattern 'name/0.0.0'
  - regex: '\b(008|Altresium|Argus|BaiduMobaider|BoardReader|DNSGroup|DataparkSearch|EDI|Goodzer|Grub|INGRID|Infohelfer|LinkedInBot|LOOQ|Nutch|OgScrper|PathDefender|Peew|PostPost|Steeler|Twitterbot|VSE|WebCrunch|WebZIP|Y!J-BR[A-Z]|YahooSeeker|envolk|sproose|wminer)/(\d+)(?:\.(\d+)|)(?:\.(\d+)|)'

  # MSIECrawler
  - regex: '(MSIE) (\d+)\.(\d+)([a-z]\d|[a-z]|);.* MSIECrawler'
    family_replacement: 'MSIECrawler'

  # DAVdroid
  - regex: '(DAVdroid)/(\d+)\.(\d+)(?:\.(\d+)|)'

  # Downloader ...
  - regex: '(Google-HTTP-Java-Client|Apache-HttpClient|Go-http-client|scalaj-http|http%20client|Python-urllib|HttpMonitor|TLSProber|WinHTTP|JNLP|okhttp|aihttp|reqwest|axios|unirest-(?:java|python|ruby|nodejs|php|net))(?:[ /](\d+)(?:\.(\d+)|)(?:\.(\d+)|)|)'

  # Pinterestbot
  - regex: '(Pinterest(?:bot|))/(\d+)(?:\.(\d+)|)(?:\.(\d+)|)[;\s(]+\+https://www.pinterest.com/bot.html'
    family_replacement: 'Pinterestbot'

  # Bots
  - regex: '(CSimpleSpider|Cityreview Robot|CrawlDaddy|CrawlFire|Finderbots|Index crawler|Job Roboter|KiwiStatus Spider|Lijit Crawler|QuerySeekerSpider|ScollSpider|Trends Crawler|USyd-NLP-Spider|SiteCat Webbot|BotName\/\$BotVersion|123metaspider-Bot|1470\.net crawler|50\.nu|8bo Crawler Bot|Aboundex|Accoona-[A-z]{1,30}-Agent|AdsBot-Google(?:-[a-z]{1,30}|)|altavista|AppEngine-Google|archive.{0,30}\.org_bot|archiver|Ask Jeeves|[Bb]ai[Dd]u[Ss]pider(?:-[A-Za-z]{1,30})(?:-[A-Za-z]{1,30}|)|bingbot|BingPreview|blitzbot|BlogBridge|Bloglovin|BoardReader Blog Indexer|BoardReader Favicon Fetcher|boitho.com-dc|BotSeer|BUbiNG|\b\w{0,30}favicon\w{0,30}\b|\bYeti(?:-[a-z]{1,30}|)|Catchpoint(?: bot|)|[Cc]harlotte|Checklinks|clumboot|Comodo HTTP\(S\) Crawler|Comodo-Webinspector-Crawler|ConveraCrawler|CRAWL-E|CrawlConvera|Daumoa(?:-feedfetcher|)|Feed Seeker Bot|Feedbin|findlinks|Flamingo_SearchEngine|FollowSite Bot|furlbot|Genieo|gigabot|GomezAgent|gonzo1|(?:[a-zA-Z]{1,30}-|)Googlebot(?:-[a-zA-Z]{1,30}|)|Google SketchUp|grub-client|gsa-crawler|heritrix|HiddenMarket|holmes|HooWWWer|htdig|ia_archiver|ICC-Crawler|Icarus6j|ichiro(?:/mobile|)|IconSurf|IlTrovatore(?:-Setaccio|)|InfuzApp|Innovazion Crawler|InternetArchive|IP2[a-z]{1,30}Bot|jbot\b|KaloogaBot|Kraken|Kurzor|larbin|LEIA|LesnikBot|Linguee Bot|LinkAider|LinkedInBot|Lite Bot|Llaut|lycos|Mail\.RU_Bot|masscan|masidani_bot|Mediapartners-Google|Microsoft .{0,30} Bot|mogimogi|mozDex|MJ12bot|msnbot(?:-media {0,2}|)|msrbot|Mtps Feed Aggregation System|netresearch|Netvibes|NewsGator[^/]{0,30}|^NING|Nutch[^/]{0,30}|Nymesis|ObjectsSearch|OgScrper|Orbiter|OOZBOT|PagePeeker|PagesInventory|PaxleFramework|Peeplo Screenshot Bot|PlantyNet_WebRobot|Pompos|Qwantify|Read%20Later|Reaper|RedCarpet|Retreiver|Riddler|Rival IQ|scooter|Scrapy|Scrubby|searchsight|seekbot|semanticdiscovery|SemrushBot|Simpy|SimplePie|SEOstats|SimpleRSS|SiteCon|Slackbot-LinkExpanding|Slack-ImgProxy|Slurp|snappy|Speedy Spider|Squrl Java|Stringer|TheUsefulbot|ThumbShotsBot|Thumbshots\.ru|Tiny Tiny RSS|Twitterbot|WhatsApp|URL2PNG|Vagabondo|VoilaBot|^vortex|Votay bot|^voyager|WASALive.Bot|Web-sniffer|WebThumb|WeSEE:[A-z]{1,30}|WhatWeb|WIRE|WordPress|Wotbox|www\.almaden\.ibm\.com|Xenu(?:.s|) Link Sleuth|Xerka [A-z]{1,30}Bot|yacy(?:bot|)|YahooSeeker|Yahoo! Slurp|Yandex\w{1,30}|YodaoBot(?:-[A-z]{1,30}|)|YottaaMonitor|Yowedo|^Zao|^Zao-Crawler|ZeBot_www\.ze\.bz|ZooShot|ZyBorg)(?:[ /]v?(\d+)(?:\.(\d+)(?:\.(\d+)|)|)|)'

  # AWS S3 Clients
  # must come before ""Bots General matcher"" to catch ""boto""/""boto3"" before ""bot""
  - regex: '\b(Boto3?|JetS3t|aws-(?:cli|sdk-(?:cpp|go|java|nodejs|ruby2?|dotnet-(?:\d{1,2}|core)))|s3fs)/(\d+)\.(\d+)(?:\.(\d+)|)'

  # Facebook
  # Must come before ""Bots General matcher"" to catch OrangeBotswana
  # Facebook Messenger must go before Facebook
  - regex: '\[(FBAN/MessengerForiOS|FB_IAB/MESSENGER);FBAV/(\d+)(?:\.(\d+)(?:\.(\d+)|)|)'
    family_replacement: 'Facebook Messenger'
  # Facebook
  - regex: '\[FB.*;(FBAV)/(\d+)(?:\.(\d+)|)(?:\.(\d+)|)'
    family_replacement: 'Facebook'
  # Sometimes Facebook does not specify a version (FBAV)
  - regex: '\[FB.*;'
    family_replacement: 'Facebook'

  # Bots General matcher 'name/0.0'
  - regex: '(?:\/[A-Za-z0-9\.]+|) {0,5}([A-Za-z0-9 \-_\!\[\]:]{0,50}(?:[Aa]rchiver|[Ii]ndexer|[Ss]craper|[Bb]ot|[Ss]pider|[Cc]rawl[a-z]{0,50}))[/ ](\d+)(?:\.(\d+)(?:\.(\d+)|)|)'
  # Bots containing bot(but not CUBOT)
  - regex: '((?:[A-Za-z][A-Za-z0-9 -]{0,50}|)[^C][^Uu][Bb]ot)\b(?:(?:[ /]| v)(\d+)(?:\.(\d+)|)(?:\.(\d+)|)|)'
  # Bots containing spider|scrape|Crawl
  - regex: '((?:[A-z0-9]{1,50}|[A-z\-]{1,50} ?|)(?: the |)(?:[Ss][Pp][Ii][Dd][Ee][Rr]|[Ss]crape|[Cc][Rr][Aa][Ww][Ll])[A-z0-9]{0,50})(?:(?:[ /]| v)(\d+)(?:\.(\d+)|)(?:\.(\d+)|)|)'

  # HbbTV standard defines what features the browser should understand.
  # but it's like targeting ""HTML5 browsers"", effective browser support depends on the model
  # See os_parsers if you want to target a specific TV
  - regex: '(HbbTV)/(\d+)\.(\d+)\.(\d+) \('

  # must go before Firefox to catch Chimera/SeaMonkey/Camino/Waterfox
  - regex: '(Chimera|SeaMonkey|Camino|Waterfox)/(\d+)\.(\d+)\.?([ab]?\d+[a-z]*|)'

  # must be before Firefox / Gecko to catch SailfishBrowser properly
  - regex: '(SailfishBrowser)/(\d+)\.(\d+)(?:\.(\d+)|)'
    family_replacement: 'Sailfish Browser'

  # Social Networks (non-Facebook)
  # Pinterest
  - regex: '\[(Pinterest)/[^\]]+\]'
  - regex: '(Pinterest)(?: for Android(?: Tablet|)|)/(\d+)(?:\.(\d+)|)(?:\.(\d+)|)'
  # Instagram app
  - regex: 'Mozilla.*Mobile.*(Instagram).(\d+)\.(\d+)\.(\d+)'
  # Flipboard app
  - regex: 'Mozilla.*Mobile.*(Flipboard).(\d+)\.(\d+)\.(\d+)'
  # Flipboard-briefing app
  - regex: 'Mozilla.*Mobile.*(Flipboard-Briefing).(\d+)\.(\d+)\.(\d+)'
  # Onefootball app
  - regex: 'Mozilla.*Mobile.*(Onefootball)\/Android.(\d+)\.(\d+)\.(\d+)'
  # Snapchat
  - regex: '(Snapchat)\/(\d+)\.(\d+)\.(\d+)\.(\d+)'

  # Basilisk
  - regex: '(Firefox)/(\d+)\.(\d+) Basilisk/(\d+)'
    family_replacement: 'Basilisk'

  # Pale Moon
  - regex: '(PaleMoon)/(\d+)\.(\d+)(?:\.(\d+)|)'
    family_replacement: 'Pale Moon'

  # Firefox
  - regex: '(Fennec)/(\d+)\.(\d+)\.?([ab]?\d+[a-z]*)'
    family_replacement: 'Firefox Mobile'
  - regex: '(Fennec)/(\d+)\.(\d+)(pre)'
    family_replacement: 'Firefox Mobile'
  - regex: '(Fennec)/(\d+)\.(\d+)'
    family_replacement: 'Firefox Mobile'
  - regex: '(?:Mobile|Tablet);.*(Firefox)/(\d+)\.(\d+)'
    family_replacement: 'Firefox Mobile'
  - regex: '(Namoroka|Shiretoko|Minefield)/(\d+)\.(\d+)\.(\d+(?:pre|))'
    family_replacement: 'Firefox ($1)'
  - regex: '(Firefox)/(\d+)\.(\d+)(a\d+[a-z]*)'
    family_replacement: 'Firefox Alpha'
  - regex: '(Firefox)/(\d+)\.(\d+)(b\d+[a-z]*)'
    family_replacement: 'Firefox Beta'
  - regex: '(Firefox)-(?:\d+\.\d+|)/(\d+)\.(\d+)(a\d+[a-z]*)'
    family_replacement: 'Firefox Alpha'
  - regex: '(Firefox)-(?:\d+\.\d+|)/(\d+)\.(\d+)(b\d+[a-z]*)'
    family_replacement: 'Firefox Beta'
  - regex: '(Namoroka|Shiretoko|Minefield)/(\d+)\.(\d+)([ab]\d+[a-z]*|)'
    family_replacement: 'Firefox ($1)'
  - regex: '(Firefox).*Tablet browser (\d+)\.(\d+)\.(\d+)'
    family_replacement: 'MicroB'
  - regex: '(MozillaDeveloperPreview)/(\d+)\.(\d+)([ab]\d+[a-z]*|)'
  - regex: '(FxiOS)/(\d+)\.(\d+)(\.(\d+)|)(\.(\d+)|)'
    family_replacement: 'Firefox iOS'

  # e.g.: Flock/2.0b2
  - regex: '(Flock)/(\d+)\.(\d+)(b\d+?)'

  # RockMelt
  - regex: '(RockMelt)/(\d+)\.(\d+)\.(\d+)'

  # e.g.: Fennec/0.9pre
  - regex: '(Navigator)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Netscape'

  - regex: '(Navigator)/(\d+)\.(\d+)([ab]\d+)'
    family_replacement: 'Netscape'

  - regex: '(Netscape6)/(\d+)\.(\d+)\.?([ab]?\d+|)'
    family_replacement: 'Netscape'

  - regex: '(MyIBrow)/(\d+)\.(\d+)'
    family_replacement: 'My Internet Browser'

  # UC Browser
  # we need check it before opera. In other case case UC Browser detected look like Opera Mini
  - regex: '(UC? ?Browser|UCWEB|U3)[ /]?(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'UC Browser'

  # Opera will stop at 9.80 and hide the real version in the Version string.
  # see: http://dev.opera.com/articles/view/opera-ua-string-changes/
  - regex: '(Opera Tablet).*Version/(\d+)\.(\d+)(?:\.(\d+)|)'
  - regex: '(Opera Mini)(?:/att|)/?(\d+|)(?:\.(\d+)|)(?:\.(\d+)|)'
  - regex: '(Opera)/.+Opera Mobi.+Version/(\d+)\.(\d+)'
    family_replacement: 'Opera Mobile'
  - regex: '(Opera)/(\d+)\.(\d+).+Opera Mobi'
    family_replacement: 'Opera Mobile'
  - regex: 'Opera Mobi.+(Opera)(?:/|\s+)(\d+)\.(\d+)'
    family_replacement: 'Opera Mobile'
  - regex: 'Opera Mobi'
    family_replacement: 'Opera Mobile'
  - regex: '(Opera)/9.80.*Version/(\d+)\.(\d+)(?:\.(\d+)|)'

  # Opera 14 for Android uses a WebKit render engine.
  - regex: '(?:Mobile Safari).*(OPR)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Opera Mobile'

  # Opera >=15 for Desktop is similar to Chrome but includes an ""OPR"" Version string.
  - regex: '(?:Chrome).*(OPR)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Opera'

  # Opera Coast
  - regex: '(Coast)/(\d+).(\d+).(\d+)'
    family_replacement: 'Opera Coast'

  # Opera Mini for iOS (from version 8.0.0)
  - regex: '(OPiOS)/(\d+).(\d+).(\d+)'
    family_replacement: 'Opera Mini'

  # Opera Neon
  - regex: 'Chrome/.+( MMS)/(\d+).(\d+).(\d+)'
    family_replacement: 'Opera Neon'

  # Palm WebOS looks a lot like Safari.
  - regex: '(hpw|web)OS/(\d+)\.(\d+)(?:\.(\d+)|)'
    family_replacement: 'webOS Browser'

  # LuaKit has no version info.
  # http://luakit.org/projects/luakit/
  - regex: '(luakit)'
    family_replacement: 'LuaKit'

  # Snowshoe
  - regex: '(Snowshoe)/(\d+)\.(\d+).(\d+)'

  # Lightning (for Thunderbird)
  # http://www.mozilla.org/projects/calendar/lightning/
  - regex: 'Gecko/\d+ (Lightning)/(\d+)\.(\d+)\.?((?:[ab]?\d+[a-z]*)|(?:\d*))'

  # Swiftfox
  - regex: '(Firefox)/(\d+)\.(\d+)\.(\d+(?:pre|)) \(Swiftfox\)'
    family_replacement: 'Swiftfox'
  - regex: '(Firefox)/(\d+)\.(\d+)([ab]\d+[a-z]*|) \(Swiftfox\)'
    family_replacement: 'Swiftfox'

  # Rekonq
  - regex: '(rekonq)/(\d+)\.(\d+)(?:\.(\d+)|) Safari'
    family_replacement: 'Rekonq'
  - regex: 'rekonq'
    family_replacement: 'Rekonq'

  # Conkeror lowercase/uppercase
  # http://conkeror.org/
  - regex: '(conkeror|Conkeror)/(\d+)\.(\d+)(?:\.(\d+)|)'
    family_replacement: 'Conkeror'

  # catches lower case konqueror
  - regex: '(konqueror)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Konqueror'

  - regex: '(WeTab)-Browser'

  - regex: '(Comodo_Dragon)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Comodo Dragon'

  - regex: '(Symphony) (\d+).(\d+)'

  - regex: 'PLAYSTATION 3.+WebKit'
    family_replacement: 'NetFront NX'
  - regex: 'PLAYSTATION 3'
    family_replacement: 'NetFront'
  - regex: '(PlayStation Portable)'
    family_replacement: 'NetFront'
  - regex: '(PlayStation Vita)'
    family_replacement: 'NetFront NX'

  - regex: 'AppleWebKit.+ (NX)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'NetFront NX'
  - regex: '(Nintendo 3DS)'
    family_replacement: 'NetFront NX'

  # Amazon Silk, should go before Safari and Chrome Mobile
  - regex: '(Silk)/(\d+)\.(\d+)(?:\.([0-9\-]+)|)'
    family_replacement: 'Amazon Silk'

  # @ref: http://www.puffinbrowser.com
  - regex: '(Puffin)/(\d+)\.(\d+)(?:\.(\d+)|)'

  # Edge Mobile
  - regex: 'Windows Phone .*(Edge)/(\d+)\.(\d+)'
    family_replacement: 'Edge Mobile'

  # Samsung Internet (based on Chrome, but lacking some features)
  - regex: '(SamsungBrowser)/(\d+)\.(\d+)'
    family_replacement: 'Samsung Internet'

  # Seznam.cz browser (based on WebKit)
  - regex: '(SznProhlizec)/(\d+)\.(\d+)(?:\.(\d+)|)'
    family_replacement: 'Seznam prohlížeč'

  # Coc Coc browser, based on Chrome (used in Vietnam)
  - regex: '(coc_coc_browser)/(\d+)\.(\d+)(?:\.(\d+)|)'
    family_replacement: 'Coc Coc'

  # Baidu Browsers (desktop spoofs chrome & IE, explorer is mobile)
  - regex: '(baidubrowser)[/\s](\d+)(?:\.(\d+)|)(?:\.(\d+)|)'
    family_replacement: 'Baidu Browser'
  - regex: '(FlyFlow)/(\d+)\.(\d+)'
    family_replacement: 'Baidu Explorer'

  # MxBrowser is Maxthon. Must go before Mobile Chrome for Android
  - regex: '(MxBrowser)/(\d+)\.(\d+)(?:\.(\d+)|)'
    family_replacement: 'Maxthon'

  # Crosswalk must go before Mobile Chrome for Android
  - regex: '(Crosswalk)/(\d+)\.(\d+)\.(\d+)\.(\d+)'

  # LINE https://line.me/en/
  # Must go before Mobile Chrome for Android
  - regex: '(Line)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'LINE'

  # MiuiBrowser should got before Mobile Chrome for Android
  - regex: '(MiuiBrowser)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'MiuiBrowser'

  # Mint Browser should got before Mobile Chrome for Android
  - regex: '(Mint Browser)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Mint Browser'

  # Google Search App on Android, eg:
  - regex: 'Mozilla.+Android.+(GSA)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Google'

  # Chrome Mobile
  - regex: 'Version/.+(Chrome)/(\d+)\.(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Chrome Mobile WebView'
  - regex: '; wv\).+(Chrome)/(\d+)\.(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Chrome Mobile WebView'
  - regex: '(CrMo)/(\d+)\.(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Chrome Mobile'
  - regex: '(CriOS)/(\d+)\.(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Chrome Mobile iOS'
  - regex: '(Chrome)/(\d+)\.(\d+)\.(\d+)\.(\d+) Mobile(?:[ /]|$)'
    family_replacement: 'Chrome Mobile'
  - regex: ' Mobile .*(Chrome)/(\d+)\.(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Chrome Mobile'

  # Chrome Frame must come before MSIE.
  - regex: '(chromeframe)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Chrome Frame'

  # Tizen Browser (second case included in browser/major.minor regex)
  - regex: '(SLP Browser)/(\d+)\.(\d+)'
    family_replacement: 'Tizen Browser'

  # Sogou Explorer 2.X
  - regex: '(SE 2\.X) MetaSr (\d+)\.(\d+)'
    family_replacement: 'Sogou Explorer'

  # QQ Browsers
  - regex: '(MQQBrowser/Mini)(?:(\d+)(?:\.(\d+)|)(?:\.(\d+)|)|)'
    family_replacement: 'QQ Browser Mini'
  - regex: '(MQQBrowser)(?:/(\d+)(?:\.(\d+)|)(?:\.(\d+)|)|)'
    family_replacement: 'QQ Browser Mobile'
  - regex: '(QQBrowser)(?:/(\d+)(?:\.(\d+)\.(\d+)(?:\.(\d+)|)|)|)'
    family_replacement: 'QQ Browser'

  # Rackspace Monitoring
  - regex: '(Rackspace Monitoring)/(\d+)\.(\d+)'
    family_replacement: 'RackspaceBot'

  # PyAMF
  - regex: '(PyAMF)/(\d+)\.(\d+)\.(\d+)'

  # Yandex Browser
  - regex: '(YaBrowser)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Yandex Browser'

  # Mail.ru Amigo/Internet Browser (Chromium-based)
  - regex: '(Chrome)/(\d+)\.(\d+)\.(\d+).* MRCHROME'
    family_replacement: 'Mail.ru Chromium Browser'

  # AOL Browser (IE-based)
  - regex: '(AOL) (\d+)\.(\d+); AOLBuild (\d+)'

  # Podcast catcher Applications using iTunes
  - regex: '(PodCruncher|Downcast)[ /]?(\d+)(?:\.(\d+)|)(?:\.(\d+)|)(?:\.(\d+)|)'

  # Box Notes https://www.box.com/resources/downloads
  # Must be before Electron
  - regex: ' (BoxNotes)/(\d+)\.(\d+)\.(\d+)'

  # Whale
  - regex: '(Whale)/(\d+)\.(\d+)\.(\d+)\.(\d+) Mobile(?:[ /]|$)'
    family_replacement: 'Whale'

  - regex: '(Whale)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Whale'

  # Ghost
  # @ref: http://www.ghost.org
  - regex: '(Ghost)/(\d+)\.(\d+)\.(\d+)'

  #### END SPECIAL CASES TOP ####

  #### MAIN CASES - this catches > 50% of all browsers ####


  # Slack desktop client (needs to be before Apple Mail, Electron, and Chrome as it gets wrongly detected on Mac OS otherwise)
  - regex: '(Slack_SSB)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Slack Desktop Client'

  # HipChat provides a version on Mac, but not on Windows.
  # Needs to be before Chrome on Windows, and AppleMail on Mac.
  - regex: '(HipChat)/?(\d+|)'
    family_replacement: 'HipChat Desktop Client'

  # Browser/major_version.minor_version.beta_version
  - regex: '\b(MobileIron|FireWeb|Jasmine|ANTGalio|Midori|Fresco|Lobo|PaleMoon|Maxthon|Lynx|OmniWeb|Dillo|Camino|Demeter|Fluid|Fennec|Epiphany|Shiira|Sunrise|Spotify|Flock|Netscape|Lunascape|WebPilot|NetFront|Netfront|Konqueror|SeaMonkey|Kazehakase|Vienna|Iceape|Iceweasel|IceWeasel|Iron|K-Meleon|Sleipnir|Galeon|GranParadiso|Opera Mini|iCab|NetNewsWire|ThunderBrowse|Iris|UP\.Browser|Bunjalloo|Google Earth|Raven for Mac|Openwave|MacOutlook|Electron|OktaMobile)/(\d+)\.(\d+)\.(\d+)'

  # Outlook 2007
  - regex: 'Microsoft Office Outlook 12\.\d+\.\d+|MSOffice 12'
    family_replacement: 'Outlook'
    v1_replacement: '2007'

  # Outlook 2010
  - regex: 'Microsoft Outlook 14\.\d+\.\d+|MSOffice 14'
    family_replacement: 'Outlook'
    v1_replacement: '2010'

  # Outlook 2013
  - regex: 'Microsoft Outlook 15\.\d+\.\d+'
    family_replacement: 'Outlook'
    v1_replacement: '2013'

  # Outlook 2016
  - regex: 'Microsoft Outlook (?:Mail )?16\.\d+\.\d+|MSOffice 16'
    family_replacement: 'Outlook'
    v1_replacement: '2016'

  # Word 2014
  - regex: 'Microsoft Office (Word) 2014'

  # Windows Live Mail
  - regex: 'Outlook-Express\/7\.0.*'
    family_replacement: 'Windows Live Mail'

  # Apple Air Mail
  - regex: '(Airmail) (\d+)\.(\d+)(?:\.(\d+)|)'

  # Thunderbird
  - regex: '(Thunderbird)/(\d+)\.(\d+)(?:\.(\d+(?:pre|))|)'
    family_replacement: 'Thunderbird'

  # Postbox
  - regex: '(Postbox)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Postbox'

  # Barca
  - regex: '(Barca(?:Pro)?)/(\d+)\.(\d+)(?:\.(\d+)|)'
    family_replacement: 'Barca'

  # Lotus Notes
  - regex: '(Lotus-Notes)/(\d+)\.(\d+)(?:\.(\d+)|)'
    family_replacement: 'Lotus Notes'

  # Vivaldi uses ""Vivaldi""
  - regex: '(Vivaldi)/(\d+)\.(\d+)\.(\d+)'

  # Edge/major_version.minor_version
  # Edge with chromium Edg/major_version.minor_version.patch.minor_patch
  - regex: '(Edge?)/(\d+)(?:\.(\d+)|)(?:\.(\d+)|)(?:\.(\d+)|)'
    family_replacement: 'Edge'

  # Brave Browser https://brave.com/
  - regex: '(brave)/(\d+)\.(\d+)\.(\d+) Chrome'
    family_replacement: 'Brave'

  # Iron Browser ~since version 50
  - regex: '(Chrome)/(\d+)\.(\d+)\.(\d+)[\d.]* Iron[^/]'
    family_replacement: 'Iron'

  # Dolphin Browser
  # @ref: http://www.dolphin.com
  - regex: '\b(Dolphin)(?: |HDCN/|/INT\-)(\d+)\.(\d+)(?:\.(\d+)|)'

  # Headless Chrome
  # https://chromium.googlesource.com/chromium/src/+/lkgr/headless/README.md
  - regex: '(HeadlessChrome)(?:/(\d+)\.(\d+)\.(\d+)|)'

  # Evolution Mail CardDav/CalDav integration
  - regex: '(Evolution)/(\d+)\.(\d+)\.(\d+\.\d+)'

  # Roundcube Mail CardDav plugin
  - regex: '(RCM CardDAV plugin)/(\d+)\.(\d+)\.(\d+(?:-dev|))'

  # Browser/major_version.minor_version
  - regex: '(bingbot|Bolt|AdobeAIR|Jasmine|IceCat|Skyfire|Midori|Maxthon|Lynx|Arora|IBrowse|Dillo|Camino|Shiira|Fennec|Phoenix|Flock|Netscape|Lunascape|Epiphany|WebPilot|Opera Mini|Opera|NetFront|Netfront|Konqueror|Googlebot|SeaMonkey|Kazehakase|Vienna|Iceape|Iceweasel|IceWeasel|Iron|K-Meleon|Sleipnir|Galeon|GranParadiso|iCab|iTunes|MacAppStore|NetNewsWire|Space Bison|Stainless|Orca|Dolfin|BOLT|Minimo|Tizen Browser|Polaris|Abrowser|Planetweb|ICE Browser|mDolphin|qutebrowser|Otter|QupZilla|MailBar|kmail2|YahooMobileMail|ExchangeWebServices|ExchangeServicesClient|Dragon|Outlook-iOS-Android)/(\d+)\.(\d+)(?:\.(\d+)|)'

  # Chrome/Chromium/major_version.minor_version
  - regex: '(Chromium|Chrome)/(\d+)\.(\d+)(?:\.(\d+)|)(?:\.(\d+)|)'

  ##########
  # IE Mobile needs to happen before Android to catch cases such as:
  # Mozilla/5.0 (Mobile; Windows Phone 8.1; Android 4.0; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 920)...
  # Mozilla/5.0 (Mobile; Windows Phone 8.1; Android 4.0; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 920; ANZ821)...
  # Mozilla/5.0 (Mobile; Windows Phone 8.1; Android 4.0; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 920; Orange)...
  # Mozilla/5.0 (Mobile; Windows Phone 8.1; Android 4.0; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 920; Vodafone)...
  ##########

  # IE Mobile
  - regex: '(IEMobile)[ /](\d+)\.(\d+)'
    family_replacement: 'IE Mobile'

  # Baca Berita App News Reader
  - regex: '(BacaBerita App)\/(\d+)\.(\d+)\.(\d+)'

  # Podcast catchers
  - regex: '^(bPod|Pocket Casts|Player FM)$'
  - regex: '^(AlexaMediaPlayer|VLC)/(\d+)\.(\d+)\.([^.\s]+)'
  - regex: '^(AntennaPod|WMPlayer|Zune|Podkicker|Radio|ExoPlayerDemo|Overcast|PocketTunes|NSPlayer|okhttp|DoggCatcher|QuickNews|QuickTime|Peapod|Podcasts|GoldenPod|VLC|Spotify|Miro|MediaGo|Juice|iPodder|gPodder|Banshee)/(\d+)\.(\d+)(?:\.(\d+)|)(?:\.(\d+)|)'
  - regex: '^(Peapod|Liferea)/([^.\s]+)\.([^.\s]+|)\.?([^.\s]+|)'
  - regex: '^(bPod|Player FM) BMID/(\S+)'
  - regex: '^(Podcast ?Addict)/v(\d+) '
  - regex: '^(Podcast ?Addict) '
    family_replacement: 'PodcastAddict'
  - regex: '(Replay) AV'
  - regex: '(VOX) Music Player'
  - regex: '(CITA) RSS Aggregator/(\d+)\.(\d+)'
  - regex: '(Pocket Casts)$'
  - regex: '(Player FM)$'
  - regex: '(LG Player|Doppler|FancyMusic|MediaMonkey|Clementine) (\d+)\.(\d+)\.?([^.\s]+|)\.?([^.\s]+|)'
  - regex: '(philpodder)/(\d+)\.(\d+)\.?([^.\s]+|)\.?([^.\s]+|)'
  - regex: '(Player FM|Pocket Casts|DoggCatcher|Spotify|MediaMonkey|MediaGo|BashPodder)'
  - regex: '(QuickTime)\.(\d+)\.(\d+)\.(\d+)'
  - regex: '(Kinoma)(\d+)'
  - regex: '(Fancy) Cloud Music (\d+)\.(\d+)'
    family_replacement: 'FancyMusic'
  - regex: 'EspnDownloadManager'
    family_replacement: 'ESPN'
  - regex: '(ESPN) Radio (\d+)\.(\d+)(?:\.(\d+)|) ?(?:rv:(\d+)|) '
  - regex: '(podracer|jPodder) v ?(\d+)\.(\d+)(?:\.(\d+)|)'
  - regex: '(ZDM)/(\d+)\.(\d+)[; ]?'
  - regex: '(Zune|BeyondPod) (\d+)(?:\.(\d+)|)[\);]'
  - regex: '(WMPlayer)/(\d+)\.(\d+)\.(\d+)\.(\d+)'
  - regex: '^(Lavf)'
    family_replacement: 'WMPlayer'
  - regex: '^(RSSRadio)[ /]?(\d+|)'
  - regex: '(RSS_Radio) (\d+)\.(\d+)'
    family_replacement: 'RSSRadio'
  - regex: '(Podkicker) \S+/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Podkicker'
  - regex: '^(HTC) Streaming Player \S+ / \S+ / \S+ / (\d+)\.(\d+)(?:\.(\d+)|)'
  - regex: '^(Stitcher)/iOS'
  - regex: '^(Stitcher)/Android'
  - regex: '^(VLC) .*version (\d+)\.(\d+)\.(\d+)'
  - regex: ' (VLC) for'
  - regex: '(vlc)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'VLC'
  - regex: '^(foobar)\S+/([^.\s]+)\.([^.\s]+|)\.?([^.\s]+|)'
  - regex: '^(Clementine)\S+ ([^.\s]+)\.([^.\s]+|)\.?([^.\s]+|)'
  - regex: '(amarok)/([^.\s]+)\.([^.\s]+|)\.?([^.\s]+|)'
    family_replacement: 'Amarok'
  - regex: '(Custom)-Feed Reader'

  # Browser major_version.minor_version.beta_version (space instead of slash)
  - regex: '(iRider|Crazy Browser|SkipStone|iCab|Lunascape|Sleipnir|Maemo Browser) (\d+)\.(\d+)\.(\d+)'
  # Browser major_version.minor_version (space instead of slash)
  - regex: '(iCab|Lunascape|Opera|Android|Jasmine|Polaris|Microsoft SkyDriveSync|The Bat!) (\d+)\.(\d+)(?:\.(\d+)|)'

  # Kindle WebKit
  - regex: '(Kindle)/(\d+)\.(\d+)'

  # weird android UAs
  - regex: '(Android) Donut'
    v1_replacement: '1'
    v2_replacement: '2'

  - regex: '(Android) Eclair'
    v1_replacement: '2'
    v2_replacement: '1'

  - regex: '(Android) Froyo'
    v1_replacement: '2'
    v2_replacement: '2'

  - regex: '(Android) Gingerbread'
    v1_replacement: '2'
    v2_replacement: '3'

  - regex: '(Android) Honeycomb'
    v1_replacement: '3'

  # desktop mode
  # http://www.anandtech.com/show/3982/windows-phone-7-review
  - regex: '(MSIE) (\d+)\.(\d+).*XBLWP7'
    family_replacement: 'IE Large Screen'

  # Nextcloud desktop sync client
  - regex: '(Nextcloud)'

  # Generic mirall client
  - regex: '(mirall)/(\d+)\.(\d+)\.(\d+)'

  # Nextcloud/Owncloud android client
  - regex: '(ownCloud-android)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Owncloud'

  # Skype for Business
  - regex: '(OC)/(\d+)\.(\d+)\.(\d+)\.(\d+) \(Skype for Business\)'
    family_replacement: 'Skype'

  #### END MAIN CASES ####

  #### SPECIAL CASES ####
  - regex: '(Obigo)InternetBrowser'
  - regex: '(Obigo)\-Browser'
  - regex: '(Obigo|OBIGO)[^\d]*(\d+)(?:.(\d+)|)'
    family_replacement: 'Obigo'

  - regex: '(MAXTHON|Maxthon) (\d+)\.(\d+)'
    family_replacement: 'Maxthon'
  - regex: '(Maxthon|MyIE2|Uzbl|Shiira)'
    v1_replacement: '0'

  - regex: '(BrowseX) \((\d+)\.(\d+)\.(\d+)'

  - regex: '(NCSA_Mosaic)/(\d+)\.(\d+)'
    family_replacement: 'NCSA Mosaic'

  # Polaris/d.d is above
  - regex: '(POLARIS)/(\d+)\.(\d+)'
    family_replacement: 'Polaris'
  - regex: '(Embider)/(\d+)\.(\d+)'
    family_replacement: 'Polaris'

  - regex: '(BonEcho)/(\d+)\.(\d+)\.?([ab]?\d+|)'
    family_replacement: 'Bon Echo'

  # @note: iOS / OSX Applications
  - regex: '(iPod|iPhone|iPad).+GSA/(\d+)\.(\d+)\.(\d+) Mobile'
    family_replacement: 'Google'
  - regex: '(iPod|iPhone|iPad).+Version/(\d+)\.(\d+)(?:\.(\d+)|).*[ +]Safari'
    family_replacement: 'Mobile Safari'
  - regex: '(iPod|iPod touch|iPhone|iPad);.*CPU.*OS[ +](\d+)_(\d+)(?:_(\d+)|).* AppleNews\/\d+\.\d+\.\d+?'
    family_replacement: 'Mobile Safari UI/WKWebView'
  - regex: '(iPod|iPhone|iPad).+Version/(\d+)\.(\d+)(?:\.(\d+)|)'
    family_replacement: 'Mobile Safari UI/WKWebView'
  - regex: '(iPod|iPod touch|iPhone|iPad);.*CPU.*OS[ +](\d+)_(\d+)(?:_(\d+)|).*Mobile.*[ +]Safari'
    family_replacement: 'Mobile Safari'
  - regex: '(iPod|iPod touch|iPhone|iPad);.*CPU.*OS[ +](\d+)_(\d+)(?:_(\d+)|).*Mobile'
    family_replacement: 'Mobile Safari UI/WKWebView'
  - regex: '(iPod|iPhone|iPad).* Safari'
    family_replacement: 'Mobile Safari'
  - regex: '(iPod|iPhone|iPad)'
    family_replacement: 'Mobile Safari UI/WKWebView'
  - regex: '(Watch)(\d+),(\d+)'
    family_replacement: 'Apple $1 App'

  ##########################
  # Outlook on iOS >= 2.62.0
  ##########################
  - regex: '(Outlook-iOS)/\d+\.\d+\.prod\.iphone \((\d+)\.(\d+)\.(\d+)\)'

  - regex: '(AvantGo) (\d+).(\d+)'

  - regex: '(OneBrowser)/(\d+).(\d+)'
    family_replacement: 'ONE Browser'

  - regex: '(Avant)'
    v1_replacement: '1'

  # This is the Tesla Model S (see similar entry in device parsers)
  - regex: '(QtCarBrowser)'
    v1_replacement: '1'

  - regex: '^(iBrowser/Mini)(\d+).(\d+)'
    family_replacement: 'iBrowser Mini'
  - regex: '^(iBrowser|iRAPP)/(\d+).(\d+)'

  # nokia browsers
  # based on: http://www.developer.nokia.com/Community/Wiki/User-Agent_headers_for_Nokia_devices
  - regex: '^(Nokia)'
    family_replacement: 'Nokia Services (WAP) Browser'
  - regex: '(NokiaBrowser)/(\d+)\.(\d+).(\d+)\.(\d+)'
    family_replacement: 'Nokia Browser'
  - regex: '(NokiaBrowser)/(\d+)\.(\d+).(\d+)'
    family_replacement: 'Nokia Browser'
  - regex: '(NokiaBrowser)/(\d+)\.(\d+)'
    family_replacement: 'Nokia Browser'
  - regex: '(BrowserNG)/(\d+)\.(\d+).(\d+)'
    family_replacement: 'Nokia Browser'
  - regex: '(Series60)/5\.0'
    family_replacement: 'Nokia Browser'
    v1_replacement: '7'
    v2_replacement: '0'
  - regex: '(Series60)/(\d+)\.(\d+)'
    family_replacement: 'Nokia OSS Browser'
  - regex: '(S40OviBrowser)/(\d+)\.(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Ovi Browser'
  - regex: '(Nokia)[EN]?(\d+)'

  # BlackBerry devices
  - regex: '(PlayBook).+RIM Tablet OS (\d+)\.(\d+)\.(\d+)'
    family_replacement: 'BlackBerry WebKit'
  - regex: '(Black[bB]erry|BB10).+Version/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'BlackBerry WebKit'
  - regex: '(Black[bB]erry)\s?(\d+)'
    family_replacement: 'BlackBerry'

  - regex: '(OmniWeb)/v(\d+)\.(\d+)'

  - regex: '(Blazer)/(\d+)\.(\d+)'
    family_replacement: 'Palm Blazer'

  - regex: '(Pre)/(\d+)\.(\d+)'
    family_replacement: 'Palm Pre'

  # fork of Links
  - regex: '(ELinks)/(\d+)\.(\d+)'
  - regex: '(ELinks) \((\d+)\.(\d+)'
  - regex: '(Links) \((\d+)\.(\d+)'

  - regex: '(QtWeb) Internet Browser/(\d+)\.(\d+)'

  #- regex: '\(iPad;.+(Version)/(\d+)\.(\d+)(?:\.(\d+)|).*Safari/'
  #  family_replacement: 'iPad'

  # Phantomjs, should go before Safari
  - regex: '(PhantomJS)/(\d+)\.(\d+)\.(\d+)'

  # WebKit Nightly
  - regex: '(AppleWebKit)/(\d+)(?:\.(\d+)|)\+ .* Safari'
    family_replacement: 'WebKit Nightly'

  # Safari
  - regex: '(Version)/(\d+)\.(\d+)(?:\.(\d+)|).*Safari/'
    family_replacement: 'Safari'
  # Safari didn't provide ""Version/d.d.d"" prior to 3.0
  - regex: '(Safari)/\d+'

  - regex: '(OLPC)/Update(\d+)\.(\d+)'

  - regex: '(OLPC)/Update()\.(\d+)'
    v1_replacement: '0'

  - regex: '(SEMC\-Browser)/(\d+)\.(\d+)'

  - regex: '(Teleca)'
    family_replacement: 'Teleca Browser'

  - regex: '(Phantom)/V(\d+)\.(\d+)'
    family_replacement: 'Phantom Browser'

  - regex: '(Trident)/(7|8)\.(0)'
    family_replacement: 'IE'
    v1_replacement: '11'

  - regex: '(Trident)/(6)\.(0)'
    family_replacement: 'IE'
    v1_replacement: '10'

  - regex: '(Trident)/(5)\.(0)'
    family_replacement: 'IE'
    v1_replacement: '9'

  - regex: '(Trident)/(4)\.(0)'
    family_replacement: 'IE'
    v1_replacement: '8'

  # Espial
  - regex: '(Espial)/(\d+)(?:\.(\d+)|)(?:\.(\d+)|)'

  # Apple Mail

  # apple mail - not directly detectable, have it after Safari stuff
  - regex: '(AppleWebKit)/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Apple Mail'

  # AFTER THE EDGE CASES ABOVE!
  # AFTER IE11
  # BEFORE all other IE
  - regex: '(Firefox)/(\d+)\.(\d+)\.(\d+)'
  - regex: '(Firefox)/(\d+)\.(\d+)(pre|[ab]\d+[a-z]*|)'

  - regex: '([MS]?IE) (\d+)\.(\d+)'
    family_replacement: 'IE'

  - regex: '(python-requests)/(\d+)\.(\d+)'
    family_replacement: 'Python Requests'

  # headless user-agents
  - regex: '\b(Windows-Update-Agent|Microsoft-CryptoAPI|SophosUpdateManager|SophosAgent|Debian APT-HTTP|Ubuntu APT-HTTP|libcurl-agent|libwww-perl|urlgrabber|curl|PycURL|Wget|aria2|Axel|OpenBSD ftp|lftp|jupdate|insomnia|fetch libfetch|akka-http|got)(?:[ /](\d+)(?:\.(\d+)|)(?:\.(\d+)|)|)'

  # Asynchronous HTTP Client/Server for asyncio and Python (https://aiohttp.readthedocs.io/)
  - regex: '(Python/3\.\d{1,3} aiohttp)/(\d+)\.(\d+)\.(\d+)'
  # Asynchronous HTTP Client/Server for asyncio and Python (https://aiohttp.readthedocs.io/)
  - regex: '(Python/3\.\d{1,3} aiohttp)/(\d+)\.(\d+)\.(\d+)'

  - regex: '(Java)[/ ]{0,1}\d+\.(\d+)\.(\d+)[_-]*([a-zA-Z0-9]+|)'

  # Cloud Storage Clients
  - regex: '^(Cyberduck)/(\d+)\.(\d+)\.(\d+)(?:\.\d+|)'
  - regex: '^(S3 Browser) (\d+)-(\d+)-(\d+)(?:\s*http://s3browser\.com|)'
  - regex: '(S3Gof3r)'
  # IBM COS (Cloud Object Storage) API
  - regex: '\b(ibm-cos-sdk-(?:core|java|js|python))/(\d+)\.(\d+)(?:\.(\d+)|)'
  # rusoto - Rusoto - AWS SDK for Rust - https://github.com/rusoto/rusoto
  - regex: '^(rusoto)/(\d+)\.(\d+)\.(\d+)'
  # rclone - rsync for cloud storage - https://rclone.org/
  - regex: '^(rclone)/v(\d+)\.(\d+)'

  # Roku Digital-Video-Players https://www.roku.com/
  - regex: '^(Roku)/DVP-(\d+)\.(\d+)'

  # Kurio App News Reader https://kurio.co.id/
  - regex: '(Kurio)\/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'Kurio App'

  # Box Drive and Box Sync https://www.box.com/resources/downloads
  - regex: '^(Box(?: Sync)?)/(\d+)\.(\d+)\.(\d+)'

  # ViaFree streaming app https://www.viafree.{dk|se|no}
  - regex: '^(ViaFree|Viafree)-(?:tvOS-)?[A-Z]{2}/(\d+)\.(\d+)\.(\d+)'
    family_replacement: 'ViaFree'

os_parsers:
  ##########
  # HbbTV vendors
  ##########

  # starts with the easy one : Panasonic seems consistent across years, hope it will continue
  #HbbTV/1.1.1 (;Panasonic;VIERA 2011;f.532;0071-0802 2000-0000;)
  #HbbTV/1.1.1 (;Panasonic;VIERA 2012;1.261;0071-3103 2000-0000;)
  #HbbTV/1.2.1 (;Panasonic;VIERA 2013;3.672;4101-0003 0002-0000;)
  #- regex: 'HbbTV/\d+\.\d+\.\d+ \(;(Panasonic);VIERA ([0-9]{4});'

  # Sony is consistent too but do not place year like the other
  # Opera/9.80 (Linux armv7l; HbbTV/1.1.1 (; Sony; KDL32W650A; PKG3.211EUA; 2013;); ) Presto/2.12.362 Version/12.11
  # Opera/9.80 (Linux mips; U;  HbbTV/1.1.1 (; Sony; KDL40HX751; PKG1.902EUA; 2012;);; en) Presto/2.10.250 Version/11.60
  # Opera/9.80 (Linux mips; U;  HbbTV/1.1.1 (; Sony; KDL22EX320; PKG4.017EUA; 2011;);; en) Presto/2.7.61 Version/11.00
  #- regex: 'HbbTV/\d+\.\d+\.\d+ \(; (Sony);.*;.*; ([0-9]{4});\)'


  # LG is consistent too, but we need to add manually the year model
  #Mozilla/5.0 (Unknown; Linux armv7l) AppleWebKit/537.1+ (KHTML, like Gecko) Safari/537.1+ HbbTV/1.1.1 ( ;LGE ;NetCast 4.0 ;03.20.30 ;1.0M ;)
  #Mozilla/5.0 (DirectFB; Linux armv7l) AppleWebKit/534.26+ (KHTML, like Gecko) Version/5.0 Safari/534.26+ HbbTV/1.1.1 ( ;LGE ;NetCast 3.0 ;1.0 ;1.0M ;)
  - regex: 'HbbTV/\d+\.\d+\.\d+ \( ;(LG)E ;NetCast 4.0'
    os_v1_replacement: '2013'
  - regex: 'HbbTV/\d+\.\d+\.\d+ \( ;(LG)E ;NetCast 3.0'
    os_v1_replacement: '2012'

  # Samsung is on its way of normalizing their user-agent
  # HbbTV/1.1.1 (;Samsung;SmartTV2013;T-FXPDEUC-1102.2;;) WebKit
  # HbbTV/1.1.1 (;Samsung;SmartTV2013;T-MST12DEUC-1102.1;;) WebKit
  # HbbTV/1.1.1 (;Samsung;SmartTV2012;;;) WebKit
  # HbbTV/1.1.1 (;;;;;) Maple_2011
  - regex: 'HbbTV/1.1.1 \(;;;;;\) Maple_2011'
    os_replacement: 'Samsung'
    os_v1_replacement: '2011'
  # manage the two models of 2013
  - regex: 'HbbTV/\d+\.\d+\.\d+ \(;(Samsung);SmartTV([0-9]{4});.*FXPDEUC'
    os_v2_replacement: 'UE40F7000'
  - regex: 'HbbTV/\d+\.\d+\.\d+ \(;(Samsung);SmartTV([0-9]{4});.*MST12DEUC'
    os_v2_replacement: 'UE32F4500'
  # generic Samsung (works starting in 2012)
  #- regex: 'HbbTV/\d+\.\d+\.\d+ \(;(Samsung);SmartTV([0-9]{4});'

  # Philips : not found any other way than a manual mapping
  # Opera/9.80 (Linux mips; U; HbbTV/1.1.1 (; Philips; ; ; ; ) CE-HTML/1.0 NETTV/4.1.3 PHILIPSTV/1.1.1; en) Presto/2.10.250 Version/11.60
  # Opera/9.80 (Linux mips ; U; HbbTV/1.1.1 (; Philips; ; ; ; ) CE-HTML/1.0 NETTV/3.2.1; en) Presto/2.6.33 Version/10.70
  - regex: 'HbbTV/1\.1\.1 \(; (Philips);.*NETTV/4'
    os_v1_replacement: '2013'
  - regex: 'HbbTV/1\.1\.1 \(; (Philips);.*NETTV/3'
    os_v1_replacement: '2012'
  - regex: 'HbbTV/1\.1\.1 \(; (Philips);.*NETTV/2'
    os_v1_replacement: '2011'

  # the HbbTV emulator developers use HbbTV/1.1.1 (;;;;;) firetv-firefox-plugin 1.1.20
  - regex: 'HbbTV/\d+\.\d+\.\d+.*(firetv)-firefox-plugin (\d+).(\d+).(\d+)'
    os_replacement: 'FireHbbTV'

  # generic HbbTV, hoping to catch manufacturer name (always after 2nd comma) and the first string that looks like a 2011-2019 year
  - regex: 'HbbTV/\d+\.\d+\.\d+ \(.*; ?([a-zA-Z]+) ?;.*(201[1-9]).*\)'

  ##########
  # @note: Windows Phone needs to come before Windows NT 6.1 *and* before Android to catch cases such as:
  # Mozilla/5.0 (Mobile; Windows Phone 8.1; Android 4.0; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 920)...
  # Mozilla/5.0 (Mobile; Windows Phone 8.1; Android 4.0; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 920; ANZ821)...
  # Mozilla/5.0 (Mobile; Windows Phone 8.1; Android 4.0; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 920; Orange)...
  # Mozilla/5.0 (Mobile; Windows Phone 8.1; Android 4.0; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 920; Vodafone)...
  ##########

  - regex: '(Windows Phone) (?:OS[ /])?(\d+)\.(\d+)'

  # Again a MS-special one: iPhone.*Outlook-iOS-Android/x.x is erroneously detected as Android
  - regex: '(CPU[ +]OS|iPhone[ +]OS|CPU[ +]iPhone)[ +]+(\d+)[_\.](\d+)(?:[_\.](\d+)|).*Outlook-iOS-Android'
    os_replacement: 'iOS'

  ##########
  # Android
  # can actually detect rooted android os. do we care?
  ##########
  - regex: '(Android)[ \-/](\d+)(?:\.(\d+)|)(?:[.\-]([a-z0-9]+)|)'

  - regex: '(Android) Donut'
    os_v1_replacement: '1'
    os_v2_replacement: '2'

  - regex: '(Android) Eclair'
    os_v1_replacement: '2'
    os_v2_replacement: '1'

  - regex: '(Android) Froyo'
    os_v1_replacement: '2'
    os_v2_replacement: '2'

  - regex: '(Android) Gingerbread'
    os_v1_replacement: '2'
    os_v2_replacement: '3'

  - regex: '(Android) Honeycomb'
    os_v1_replacement: '3'

  # UCWEB
  - regex: '^UCWEB.*; (Adr) (\d+)\.(\d+)(?:[.\-]([a-z0-9]+)|);'
    os_replacement: 'Android'
  - regex: '^UCWEB.*; (iPad|iPh|iPd) OS (\d+)_(\d+)(?:_(\d+)|);'
    os_replacement: 'iOS'
  - regex: '^UCWEB.*; (wds) (\d+)\.(\d+)(?:\.(\d+)|);'
    os_replacement: 'Windows Phone'
  # JUC
  - regex: '^(JUC).*; ?U; ?(?:Android|)(\d+)\.(\d+)(?:[\.\-]([a-z0-9]+)|)'
    os_replacement: 'Android'

  # Salesforce
  - regex: '(android)\s(?:mobile\/)(\d+)(?:\.(\d+)(?:\.(\d+)|)|)'
    os_replacement: 'Android'

  ##########
  # Kindle Android
  ##########
  - regex: '(Silk-Accelerated=[a-z]{4,5})'
    os_replacement: 'Android'

  # Citrix Chrome App on Chrome OS
  # Note, this needs to come before the windows parsers as the app doesn't
  # properly identify as Chrome OS
  #
  # ex: Mozilla/5.0 (X11; Windows aarch64 10718.88.2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.118 Safari/537.36 CitrixChromeApp
  - regex: '(x86_64|aarch64)\ (\d+)\.(\d+)\.(\d+).*Chrome.*(?:CitrixChromeApp)$'
    os_replacement: 'Chrome OS'

  ##########
  # Windows
  # http://en.wikipedia.org/wiki/Windows_NT#Releases
  # possibility of false positive when different marketing names share same NT kernel
  # e.g. windows server 2003 and windows xp
  # lots of ua strings have Windows NT 4.1 !?!?!?!? !?!? !? !????!?! !!! ??? !?!?! ?
  # (very) roughly ordered in terms of frequency of occurence of regex (win xp currently most frequent, etc)
  ##########

  # ie mobile desktop mode
  # spoofs nt 6.1. must come before windows 7
  - regex: '(XBLWP7)'
    os_replacement: 'Windows Phone'

  # @note: This needs to come before Windows NT 6.1
  - regex: '(Windows ?Mobile)'
    os_replacement: 'Windows Mobile'

  - regex: '(Windows 10)'
    os_replacement: 'Windows'
    os_v1_replacement: '10'

  - regex: '(Windows (?:NT 5\.2|NT 5\.1))'
    os_replacement: 'Windows'
    os_v1_replacement: 'XP'

  - regex: '(Windows NT 6\.1)'
    os_replacement: 'Windows'
    os_v1_replacement: '7'

  - regex: '(Windows NT 6\.0)'
    os_replacement: 'Windows'
    os_v1_replacement: 'Vista'

  - regex: '(Win 9x 4\.90)'
    os_replacement: 'Windows'
    os_v1_replacement: 'ME'

  - regex: '(Windows NT 6\.2; ARM;)'
    os_replacement: 'Windows'
    os_v1_replacement: 'RT'

  - regex: '(Windows NT 6\.2)'
    os_replacement: 'Windows'
    os_v1_replacement: '8'

  - regex: '(Windows NT 6\.3; ARM;)'
    os_replacement: 'Windows'
    os_v1_replacement: 'RT 8'
    os_v2_replacement: '1'

  - regex: '(Windows NT 6\.3)'
    os_replacement: 'Windows'
    os_v1_replacement: '8'
    os_v2_replacement: '1'

  - regex: '(Windows NT 6\.4)'
    os_replacement: 'Windows'
    os_v1_replacement: '10'

  - regex: '(Windows NT 10\.0)'
    os_replacement: 'Windows'
    os_v1_replacement: '10'

  - regex: '(Windows NT 5\.0)'
    os_replacement: 'Windows'
    os_v1_replacement: '2000'

  - regex: '(WinNT4.0)'
    os_replacement: 'Windows'
    os_v1_replacement: 'NT 4.0'

  - regex: '(Windows ?CE)'
    os_replacement: 'Windows'
    os_v1_replacement: 'CE'

  - regex: 'Win(?:dows)? ?(95|98|3.1|NT|ME|2000|XP|Vista|7|CE)'
    os_replacement: 'Windows'
    os_v1_replacement: '$1'

  - regex: 'Win16'
    os_replacement: 'Windows'
    os_v1_replacement: '3.1'

  - regex: 'Win32'
    os_replacement: 'Windows'
    os_v1_replacement: '95'

  # Box apps (Drive, Sync, Notes) on Windows https://www.box.com/resources/downloads
  - regex: '^Box.*Windows/([\d.]+);'
    os_replacement: 'Windows'
    os_v1_replacement: '$1'

  ##########
  # Tizen OS from Samsung
  # spoofs Android so pushing it above
  ##########
  - regex: '(Tizen)[/ ](\d+)\.(\d+)'

  ##########
  # Mac OS
  # @ref: http://en.wikipedia.org/wiki/Mac_OS_X#Versions
  # @ref: http://www.puredarwin.org/curious/versions
  ##########
  - regex: '((?:Mac[ +]?|; )OS[ +]X)[\s+/](?:(\d+)[_.](\d+)(?:[_.](\d+)|)|Mach-O)'
    os_replacement: 'Mac OS X'
  - regex: '\w+\s+Mac OS X\s+\w+\s+(\d+).(\d+).(\d+).*'
    os_replacement: 'Mac OS X'
    os_v1_replacement: '$1'
    os_v2_replacement: '$2'
    os_v3_replacement: '$3'
  # Leopard
  - regex: ' (Dar)(win)/(9).(\d+).*\((?:i386|x86_64|Power Macintosh)\)'
    os_replacement: 'Mac OS X'
    os_v1_replacement: '10'
    os_v2_replacement: '5'
  # Snow Leopard
  - regex: ' (Dar)(win)/(10).(\d+).*\((?:i386|x86_64)\)'
    os_replacement: 'Mac OS X'
    os_v1_replacement: '10'
    os_v2_replacement: '6'
  # Lion
  - regex: ' (Dar)(win)/(11).(\d+).*\((?:i386|x86_64)\)'
    os_replacement: 'Mac OS X'
    os_v1_replacement: '10'
    os_v2_replacement: '7'
  # Mountain Lion
  - regex: ' (Dar)(win)/(12).(\d+).*\((?:i386|x86_64)\)'
    os_replacement: 'Mac OS X'
    os_v1_replacement: '10'
    os_v2_replacement: '8'
  # Mavericks
  - regex: ' (Dar)(win)/(13).(\d+).*\((?:i386|x86_64)\)'
    os_replacement: 'Mac OS X'
    os_v1_replacement: '10'
    os_v2_replacement: '9'
  # Yosemite is Darwin/14.x but patch versions are inconsistent in the Darwin string;
  # more accurately covered by CFNetwork regexes downstream

  # IE on Mac doesn't specify version number
  - regex: 'Mac_PowerPC'
    os_replacement: 'Mac OS'

  # builds before tiger don't seem to specify version?

  # ios devices spoof (mac os x), so including intel/ppc prefixes
  - regex: '(?:PPC|Intel) (Mac OS X)'

  # Box Drive and Box Sync on Mac OS X use OSX version numbers, not Darwin
  - regex: '^Box.*;(Darwin)/(10)\.(1\d)(?:\.(\d+)|)'
    os_replacement: 'Mac OS X'

  ##########
  # iOS
  # http://en.wikipedia.org/wiki/IOS_version_history
  ##########
  # keep this above generic iOS, since AppleTV UAs contain 'CPU OS'
  - regex: '(Apple\s?TV)(?:/(\d+)\.(\d+)|)'
    os_replacement: 'ATV OS X'

  - regex: '(CPU[ +]OS|iPhone[ +]OS|CPU[ +]iPhone|CPU IPhone OS)[ +]+(\d+)[_\.](\d+)(?:[_\.](\d+)|)'
    os_replacement: 'iOS'

  # remaining cases are mostly only opera uas, so catch opera as to not catch iphone spoofs
  - regex: '(iPhone|iPad|iPod); Opera'
    os_replacement: 'iOS'

  # few more stragglers
  - regex: '(iPhone|iPad|iPod).*Mac OS X.*Version/(\d+)\.(\d+)'
    os_replacement: 'iOS'

  # CFNetwork/Darwin - The specific CFNetwork or Darwin version determines
  # whether the os maps to Mac OS, or iOS, or just Darwin.
  # See: http://user-agents.me/cfnetwork-version-list
  - regex: '(CFNetwork)/(5)48\.0\.3.* Darwin/11\.0\.0'
    os_replacement: 'iOS'
  - regex: '(CFNetwork)/(5)48\.(0)\.4.* Darwin/(1)1\.0\.0'
    os_replacement: 'iOS'
  - regex: '(CFNetwork)/(5)48\.(1)\.4'
    os_replacement: 'iOS'
  - regex: '(CFNetwork)/(4)85\.1(3)\.9'
    os_replacement: 'iOS'
  - regex: '(CFNetwork)/(6)09\.(1)\.4'
    os_replacement: 'iOS'
  - regex: '(CFNetwork)/(6)(0)9'
    os_replacement: 'iOS'
  - regex: '(CFNetwork)/6(7)2\.(1)\.13'
    os_replacement: 'iOS'
  - regex: '(CFNetwork)/6(7)2\.(1)\.(1)4'
    os_replacement: 'iOS'
  - regex: '(CF)(Network)/6(7)(2)\.1\.15'
    os_replacement: 'iOS'
    os_v1_replacement: '7'
    os_v2_replacement: '1'
  - regex: '(CFNetwork)/6(7)2\.(0)\.(?:2|8)'
    os_replacement: 'iOS'
  - regex: '(CFNetwork)/709\.1'
    os_replacement: 'iOS'
    os_v1_replacement: '8'
    os_v2_replacement: '0.b5'
  - regex: '(CF)(Network)/711\.(\d)'
    os_replacement: 'iOS'
    os_v1_replacement: '8'
  - regex: '(CF)(Network)/(720)\.(\d)'
    os_replacement: 'Mac OS X'
    os_v1_replacement: '10'
    os_v2_replacement: '10'
  - regex: '(CF)(Network)/(760)\.(\d)'
    os_replacement: 'Mac OS X'
    os_v1_replacement: '10'
    os_v2_replacement: '11'
  - regex: 'CFNetwork/7.* Darwin/15\.4\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '9'
    os_v2_replacement: '3'
    os_v3_replacement: '1'
  - regex: 'CFNetwork/7.* Darwin/15\.5\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '9'
    os_v2_replacement: '3'
    os_v3_replacement: '2'
  - regex: 'CFNetwork/7.* Darwin/15\.6\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '9'
    os_v2_replacement: '3'
    os_v3_replacement: '5'
  - regex: '(CF)(Network)/758\.(\d)'
    os_replacement: 'iOS'
    os_v1_replacement: '9'
  - regex: 'CFNetwork/808\.3 Darwin/16\.3\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '10'
    os_v2_replacement: '2'
    os_v3_replacement: '1'
  - regex: '(CF)(Network)/808\.(\d)'
    os_replacement: 'iOS'
    os_v1_replacement: '10'

  ##########
  # CFNetwork macOS Apps (must be before CFNetwork iOS Apps
  # @ref: https://en.wikipedia.org/wiki/Darwin_(operating_system)#Release_history
  ##########
  - regex: 'CFNetwork/.* Darwin/17\.\d+.*\(x86_64\)'
    os_replacement: 'Mac OS X'
    os_v1_replacement: '10'
    os_v2_replacement: '13'
  - regex: 'CFNetwork/.* Darwin/16\.\d+.*\(x86_64\)'
    os_replacement: 'Mac OS X'
    os_v1_replacement: '10'
    os_v2_replacement: '12'
  - regex: 'CFNetwork/8.* Darwin/15\.\d+.*\(x86_64\)'
    os_replacement: 'Mac OS X'
    os_v1_replacement: '10'
    os_v2_replacement: '11'
  ##########
  # CFNetwork iOS Apps
  # @ref: https://en.wikipedia.org/wiki/Darwin_(operating_system)#Release_history
  ##########
  - regex: 'CFNetwork/.* Darwin/(9)\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '1'
  - regex: 'CFNetwork/.* Darwin/(10)\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '4'
  - regex: 'CFNetwork/.* Darwin/(11)\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '5'
  - regex: 'CFNetwork/.* Darwin/(13)\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '6'
  - regex: 'CFNetwork/6.* Darwin/(14)\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '7'
  - regex: 'CFNetwork/7.* Darwin/(14)\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '8'
    os_v2_replacement: '0'
  - regex: 'CFNetwork/7.* Darwin/(15)\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '9'
    os_v2_replacement: '0'
  - regex: 'CFNetwork/8.* Darwin/16\.5\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '10'
    os_v2_replacement: '3'
  - regex: 'CFNetwork/8.* Darwin/16\.6\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '10'
    os_v2_replacement: '3'
    os_v3_replacement: '2'
  - regex: 'CFNetwork/8.* Darwin/16\.7\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '10'
    os_v2_replacement: '3'
    os_v3_replacement: '3'
  - regex: 'CFNetwork/8.* Darwin/(16)\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '10'
  - regex: 'CFNetwork/8.* Darwin/17\.0\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '11'
    os_v2_replacement: '0'
  - regex: 'CFNetwork/8.* Darwin/17\.2\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '11'
    os_v2_replacement: '1'
  - regex: 'CFNetwork/8.* Darwin/17\.3\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '11'
    os_v2_replacement: '2'
  - regex: 'CFNetwork/8.* Darwin/17\.4\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '11'
    os_v2_replacement: '2'
    os_v3_replacement: '6'
  - regex: 'CFNetwork/8.* Darwin/17\.5\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '11'
    os_v2_replacement: '3'
  - regex: 'CFNetwork/9.* Darwin/17\.6\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '11'
    os_v2_replacement: '4'
  - regex: 'CFNetwork/9.* Darwin/17\.7\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '11'
    os_v2_replacement: '4'
    os_v3_replacement: '1'
  - regex: 'CFNetwork/8.* Darwin/(17)\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '11'
  - regex: 'CFNetwork/9.* Darwin/18\.0\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '12'
    os_v2_replacement: '0'
  - regex: 'CFNetwork/9.* Darwin/(18)\.\d+'
    os_replacement: 'iOS'
    os_v1_replacement: '12'
  - regex: 'CFNetwork/.* Darwin/'
    os_replacement: 'iOS'

  # iOS Apps
  - regex: '\b(iOS[ /]|iOS; |iPhone(?:/| v|[ _]OS[/,]|; | OS : |\d,\d/|\d,\d; )|iPad/)(\d{1,2})[_\.](\d{1,2})(?:[_\.](\d+)|)'
    os_replacement: 'iOS'
  - regex: '\((iOS);'

  ##########
  # Apple Watch
  ##########
  - regex: '(watchOS)/(\d+)\.(\d+)(?:\.(\d+)|)'
    os_replacement: 'WatchOS'

  ##########################
  # Outlook on iOS >= 2.62.0
  ##########################
  - regex: 'Outlook-(iOS)/\d+\.\d+\.prod\.iphone'

  ##########################
  # iOS devices, the same regex matches mobile safari webviews
  ##########################
  - regex: '(iPod|iPhone|iPad)'
    os_replacement: 'iOS'

  ##########
  # Apple TV
  ##########
  - regex: '(tvOS)[/ ](\d+)\.(\d+)(?:\.(\d+)|)'
    os_replacement: 'tvOS'

  ##########
  # Chrome OS
  # if version 0.0.0, probably this stuff:
  # http://code.google.com/p/chromium-os/issues/detail?id=11573
  # http://code.google.com/p/chromium-os/issues/detail?id=13790
  ##########
  - regex: '(CrOS) [a-z0-9_]+ (\d+)\.(\d+)(?:\.(\d+)|)'
    os_replacement: 'Chrome OS'

  ##########
  # Linux distros
  ##########
  - regex: '([Dd]ebian)'
    os_replacement: 'Debian'
  - regex: '(Linux Mint)(?:/(\d+)|)'
  - regex: '(Mandriva)(?: Linux|)/(?:[\d.-]+m[a-z]{2}(\d+).(\d)|)'

  ##########
  # Symbian + Symbian OS
  # http://en.wikipedia.org/wiki/History_of_Symbian
  ##########
  - regex: '(Symbian[Oo][Ss])[/ ](\d+)\.(\d+)'
    os_replacement: 'Symbian OS'
  - regex: '(Symbian/3).+NokiaBrowser/7\.3'
    os_replacement: 'Symbian^3 Anna'
  - regex: '(Symbian/3).+NokiaBrowser/7\.4'
    os_replacement: 'Symbian^3 Belle'
  - regex: '(Symbian/3)'
    os_replacement: 'Symbian^3'
  - regex: '\b(Series 60|SymbOS|S60Version|S60V\d|S60\b)'
    os_replacement: 'Symbian OS'
  - regex: '(MeeGo)'
  - regex: 'Symbian [Oo][Ss]'
    os_replacement: 'Symbian OS'
  - regex: 'Series40;'
    os_replacement: 'Nokia Series 40'
  - regex: 'Series30Plus;'
    os_replacement: 'Nokia Series 30 Plus'

  ##########
  # BlackBerry devices
  ##########
  - regex: '(BB10);.+Version/(\d+)\.(\d+)\.(\d+)'
    os_replacement: 'BlackBerry OS'
  - regex: '(Black[Bb]erry)[0-9a-z]+/(\d+)\.(\d+)\.(\d+)(?:\.(\d+)|)'
    os_replacement: 'BlackBerry OS'
  - regex: '(Black[Bb]erry).+Version/(\d+)\.(\d+)\.(\d+)(?:\.(\d+)|)'
    os_replacement: 'BlackBerry OS'
  - regex: '(RIM Tablet OS) (\d+)\.(\d+)\.(\d+)'
    os_replacement: 'BlackBerry Tablet OS'
  - regex: '(Play[Bb]ook)'
    os_replacement: 'BlackBerry Tablet OS'
  - regex: '(Black[Bb]erry)'
    os_replacement: 'BlackBerry OS'

  ##########
  # KaiOS
  ##########
  - regex: '(K[Aa][Ii]OS)\/(\d+)\.(\d+)(?:\.(\d+)|)'
    os_replacement: 'KaiOS'

  ##########
  # Firefox OS
  ##########
  - regex: '\((?:Mobile|Tablet);.+Gecko/18.0 Firefox/\d+\.\d+'
    os_replacement: 'Firefox OS'
    os_v1_replacement: '1'
    os_v2_replacement: '0'
    os_v3_replacement: '1'

  - regex: '\((?:Mobile|Tablet);.+Gecko/18.1 Firefox/\d+\.\d+'
    os_replacement: 'Firefox OS'
    os_v1_replacement: '1'
    os_v2_replacement: '1'

  - regex: '\((?:Mobile|Tablet);.+Gecko/26.0 Firefox/\d+\.\d+'
    os_replacement: 'Firefox OS'
    os_v1_replacement: '1'
    os_v2_replacement: '2'

  - regex: '\((?:Mobile|Tablet);.+Gecko/28.0 Firefox/\d+\.\d+'
    os_replacement: 'Firefox OS'
    os_v1_replacement: '1'
    os_v2_replacement: '3'

  - regex: '\((?:Mobile|Tablet);.+Gecko/30.0 Firefox/\d+\.\d+'
    os_replacement: 'Firefox OS'
    os_v1_replacement: '1'
    os_v2_replacement: '4'

  - regex: '\((?:Mobile|Tablet);.+Gecko/32.0 Firefox/\d+\.\d+'
    os_replacement: 'Firefox OS'
    os_v1_replacement: '2'
    os_v2_replacement: '0'

  - regex: '\((?:Mobile|Tablet);.+Gecko/34.0 Firefox/\d+\.\d+'
    os_replacement: 'Firefox OS'
    os_v1_replacement: '2'
    os_v2_replacement: '1'

  # Firefox OS Generic
  - regex: '\((?:Mobile|Tablet);.+Firefox/\d+\.\d+'
    os_replacement: 'Firefox OS'


  ##########
  # BREW
  # yes, Brew is lower-cased for Brew MP
  ##########
  - regex: '(BREW)[ /](\d+)\.(\d+)\.(\d+)'
  - regex: '(BREW);'
  - regex: '(Brew MP|BMP)[ /](\d+)\.(\d+)\.(\d+)'
    os_replacement: 'Brew MP'
  - regex: 'BMP;'
    os_replacement: 'Brew MP'

  ##########
  # Google TV
  ##########
  - regex: '(GoogleTV)(?: (\d+)\.(\d+)(?:\.(\d+)|)|/[\da-z]+)'

  - regex: '(WebTV)/(\d+).(\d+)'

  ##########
  # Chromecast
  ##########
  - regex: '(CrKey)(?:[/](\d+)\.(\d+)(?:\.(\d+)|)|)'
    os_replacement: 'Chromecast'

  ##########
  # Misc mobile
  ##########
  - regex: '(hpw|web)OS/(\d+)\.(\d+)(?:\.(\d+)|)'
    os_replacement: 'webOS'
  - regex: '(VRE);'

  ##########
  # Generic patterns
  # since the majority of os cases are very specific, these go last
  ##########
  - regex: '(Fedora|Red Hat|PCLinuxOS|Puppy|Ubuntu|Kindle|Bada|Sailfish|Lubuntu|BackTrack|Slackware|(?:Free|Open|Net|\b)BSD)[/ ](\d+)\.(\d+)(?:\.(\d+)|)(?:\.(\d+)|)'

  # Gentoo Linux + Kernel Version
  - regex: '(Linux)[ /](\d+)\.(\d+)(?:\.(\d+)|).*gentoo'
    os_replacement: 'Gentoo'

  # Opera Mini Bada
  - regex: '\((Bada);'

  # just os
  - regex: '(Windows|Android|WeTab|Maemo|Web0S)'
  - regex: '(Ubuntu|Kubuntu|Arch Linux|CentOS|Slackware|Gentoo|openSUSE|SUSE|Red Hat|Fedora|PCLinuxOS|Mageia|(?:Free|Open|Net|\b)BSD)'
  # Linux + Kernel Version
  - regex: '(Linux)(?:[ /](\d+)\.(\d+)(?:\.(\d+)|)|)'
  - regex: 'SunOS'
    os_replacement: 'Solaris'
  # Wget/x.x.x (linux-gnu)
  - regex: '\(linux-gnu\)'
    os_replacement: 'Linux'
  - regex: '\(x86_64-redhat-linux-gnu\)'
    os_replacement: 'Red Hat'
  - regex: '\((freebsd)(\d+)\.(\d+)\)'
    os_replacement: 'FreeBSD'
  - regex: 'linux'
    os_replacement: 'Linux'

  # Roku Digital-Video-Players https://www.roku.com/
  - regex: '^(Roku)/DVP-(\d+)\.(\d+)'

device_parsers:

  #########
  # Mobile Spiders
  # Catch the mobile crawler before checking for iPhones / Androids.
  #########
  - regex: '(?:(?:iPhone|Windows CE|Windows Phone|Android).*(?:(?:Bot|Yeti)-Mobile|YRSpider|BingPreview|bots?/\d|(?:bot|spider)\.html)|AdsBot-Google-Mobile.*iPhone)'
    regex_flag: 'i'
    device_replacement: 'Spider'
    brand_replacement: 'Spider'
    model_replacement: 'Smartphone'
  - regex: '(?:DoCoMo|\bMOT\b|\bLG\b|Nokia|Samsung|SonyEricsson).*(?:(?:Bot|Yeti)-Mobile|bots?/\d|(?:bot|crawler)\.html|(?:jump|google|Wukong)bot|ichiro/mobile|/spider|YahooSeeker)'
    regex_flag: 'i'
    device_replacement: 'Spider'
    brand_replacement: 'Spider'
    model_replacement: 'Feature Phone'

  # PTST / WebPageTest.org crawlers
  - regex: ' PTST/\d+(?:\.)?\d+$'
    device_replacement: 'Spider'
    brand_replacement: 'Spider'

  # Datanyze.com spider
  - regex: 'X11; Datanyze; Linux'
    device_replacement: 'Spider'
    brand_replacement: 'Spider'

  #########
  # WebBrowser for SmartWatch
  # @ref: https://play.google.com/store/apps/details?id=se.vaggan.webbrowser&hl=en
  #########
  - regex: '\bSmartWatch *\( *([^;]+) *; *([^;]+) *;'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'

  ######################################################################
  # Android parsers
  #
  # @ref: https://support.google.com/googleplay/answer/1727131?hl=en
  ######################################################################

  # Android Application
  - regex: 'Android Application[^\-]+ - (Sony) ?(Ericsson|) (.+) \w+ - '
    device_replacement: '$1 $2'
    brand_replacement: '$1$2'
    model_replacement: '$3'
  - regex: 'Android Application[^\-]+ - (?:HTC|HUAWEI|LGE|LENOVO|MEDION|TCT) (HTC|HUAWEI|LG|LENOVO|MEDION|ALCATEL)[ _\-](.+) \w+ - '
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'
  - regex: 'Android Application[^\-]+ - ([^ ]+) (.+) \w+ - '
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'

  #########
  # 3Q
  # @ref: http://www.3q-int.com/
  #########
  - regex: '; *([BLRQ]C\d{4}[A-Z]+) +Build/'
    device_replacement: '3Q $1'
    brand_replacement: '3Q'
    model_replacement: '$1'
  - regex: '; *(?:3Q_)([^;/]+) +Build'
    device_replacement: '3Q $1'
    brand_replacement: '3Q'
    model_replacement: '$1'

  #########
  # Acer
  # @ref: http://us.acer.com/ac/en/US/content/group/tablets
  #########
  - regex: 'Android [34].*; *(A100|A101|A110|A200|A210|A211|A500|A501|A510|A511|A700(?: Lite| 3G|)|A701|B1-A71|A1-\d{3}|B1-\d{3}|V360|V370|W500|W500P|W501|W501P|W510|W511|W700|Slider SL101|DA22[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Acer'
    model_replacement: '$1'
  - regex: '; *Acer Iconia Tab ([^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Acer'
    model_replacement: '$1'
  - regex: '; *(Z1[1235]0|E320[^/]*|S500|S510|Liquid[^;/]*|Iconia A\d+) Build'
    device_replacement: '$1'
    brand_replacement: 'Acer'
    model_replacement: '$1'
  - regex: '; *(Acer |ACER )([^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Acer'
    model_replacement: '$2'

  #########
  # Advent
  # @ref: https://en.wikipedia.org/wiki/Advent_Vega
  # @note: VegaBean and VegaComb (names derived from jellybean, honeycomb) are
  #   custom ROM builds for Vega
  #########
  - regex: '; *(Advent |)(Vega(?:Bean|Comb|)).* Build'
    device_replacement: '$1$2'
    brand_replacement: 'Advent'
    model_replacement: '$2'

  #########
  # Ainol
  # @ref: http://www.ainol.com/plugin.php?identifier=ainol&module=product
  #########
  - regex: '; *(Ainol |)((?:NOVO|[Nn]ovo)[^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Ainol'
    model_replacement: '$2'

  #########
  # Airis
  # @ref: http://airis.es/Tienda/Default.aspx?idG=001
  #########
  - regex: '; *AIRIS[ _\-]?([^/;\)]+) *(?:;|\)|Build)'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'Airis'
    model_replacement: '$1'
  - regex: '; *(OnePAD[^;/]+) Build'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'Airis'
    model_replacement: '$1'

  #########
  # Airpad
  # @ref: ??
  #########
  - regex: '; *Airpad[ \-]([^;/]+) Build'
    device_replacement: 'Airpad $1'
    brand_replacement: 'Airpad'
    model_replacement: '$1'

  #########
  # Alcatel - TCT
  # @ref: http://www.alcatelonetouch.com/global-en/products/smartphones.html
  #########
  - regex: '; *(one ?touch) (EVO7|T10|T20) Build'
    device_replacement: 'Alcatel One Touch $2'
    brand_replacement: 'Alcatel'
    model_replacement: 'One Touch $2'
  - regex: '; *(?:alcatel[ _]|)(?:(?:one[ _]?touch[ _])|ot[ \-])([^;/]+);? Build'
    regex_flag: 'i'
    device_replacement: 'Alcatel One Touch $1'
    brand_replacement: 'Alcatel'
    model_replacement: 'One Touch $1'
  - regex: '; *(TCL)[ _]([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'
  # operator specific models
  - regex: '; *(Vodafone Smart II|Optimus_Madrid) Build'
    device_replacement: 'Alcatel $1'
    brand_replacement: 'Alcatel'
    model_replacement: '$1'
  - regex: '; *BASE_Lutea_3 Build'
    device_replacement: 'Alcatel One Touch 998'
    brand_replacement: 'Alcatel'
    model_replacement: 'One Touch 998'
  - regex: '; *BASE_Varia Build'
    device_replacement: 'Alcatel One Touch 918D'
    brand_replacement: 'Alcatel'
    model_replacement: 'One Touch 918D'

  #########
  # Allfine
  # @ref: http://www.myallfine.com/Products.asp
  #########
  - regex: '; *((?:FINE|Fine)\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Allfine'
    model_replacement: '$1'

  #########
  # Allview
  # @ref: http://www.allview.ro/produse/droseries/lista-tablete-pc/
  #########
  - regex: '; *(ALLVIEW[ _]?|Allview[ _]?)((?:Speed|SPEED).*) Build/'
    device_replacement: '$1$2'
    brand_replacement: 'Allview'
    model_replacement: '$2'
  - regex: '; *(ALLVIEW[ _]?|Allview[ _]?|)(AX1_Shine|AX2_Frenzy) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Allview'
    model_replacement: '$2'
  - regex: '; *(ALLVIEW[ _]?|Allview[ _]?)([^;/]*) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Allview'
    model_replacement: '$2'

  #########
  # Allwinner
  # @ref: http://www.allwinner.com/
  # @models: A31 (13.3""),A20,A10,
  #########
  - regex: '; *(A13-MID) Build'
    device_replacement: '$1'
    brand_replacement: 'Allwinner'
    model_replacement: '$1'
  - regex: '; *(Allwinner)[ _\-]?([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Allwinner'
    model_replacement: '$1'

  #########
  # Amaway
  # @ref: http://www.amaway.cn/
  #########
  - regex: '; *(A651|A701B?|A702|A703|A705|A706|A707|A711|A712|A713|A717|A722|A785|A801|A802|A803|A901|A902|A1002|A1003|A1006|A1007|A9701|A9703|Q710|Q80) Build'
    device_replacement: '$1'
    brand_replacement: 'Amaway'
    model_replacement: '$1'

  #########
  # Amoi
  # @ref: http://www.amoi.com/en/prd/prd_index.jspx
  #########
  - regex: '; *(?:AMOI|Amoi)[ _]([^;/]+) Build'
    device_replacement: 'Amoi $1'
    brand_replacement: 'Amoi'
    model_replacement: '$1'
  - regex: '^(?:AMOI|Amoi)[ _]([^;/]+) Linux'
    device_replacement: 'Amoi $1'
    brand_replacement: 'Amoi'
    model_replacement: '$1'

  #########
  # Aoc
  # @ref: http://latin.aoc.com/media_tablet
  #########
  - regex: '; *(MW(?:0[789]|10)[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Aoc'
    model_replacement: '$1'

  #########
  # Aoson
  # @ref: http://www.luckystar.com.cn/en/mid.aspx?page=1
  # @ref: http://www.luckystar.com.cn/en/mobiletel.aspx?page=1
  # @note: brand owned by luckystar
  #########
  - regex: '; *(G7|M1013|M1015G|M11[CG]?|M-?12[B]?|M15|M19[G]?|M30[ACQ]?|M31[GQ]|M32|M33[GQ]|M36|M37|M38|M701T|M710|M712B|M713|M715G|M716G|M71(?:G|GS|T|)|M72[T]?|M73[T]?|M75[GT]?|M77G|M79T|M7L|M7LN|M81|M810|M81T|M82|M92|M92KS|M92S|M717G|M721|M722G|M723|M725G|M739|M785|M791|M92SK|M93D) Build'
    device_replacement: 'Aoson $1'
    brand_replacement: 'Aoson'
    model_replacement: '$1'
  - regex: '; *Aoson ([^;/]+) Build'
    regex_flag: 'i'
    device_replacement: 'Aoson $1'
    brand_replacement: 'Aoson'
    model_replacement: '$1'

  #########
  # Apanda
  # @ref: http://www.apanda.com.cn/
  #########
  - regex: '; *[Aa]panda[ _\-]([^;/]+) Build'
    device_replacement: 'Apanda $1'
    brand_replacement: 'Apanda'
    model_replacement: '$1'

  #########
  # Archos
  # @ref: http://www.archos.com/de/products/tablets.html
  # @ref: http://www.archos.com/de/products/smartphones/index.html
  #########
  - regex: '; *(?:ARCHOS|Archos) ?(GAMEPAD.*?)(?: Build|[;/\(\)\-])'
    device_replacement: 'Archos $1'
    brand_replacement: 'Archos'
    model_replacement: '$1'
  - regex: 'ARCHOS; GOGI; ([^;]+);'
    device_replacement: 'Archos $1'
    brand_replacement: 'Archos'
    model_replacement: '$1'
  - regex: '(?:ARCHOS|Archos)[ _]?(.*?)(?: Build|[;/\(\)\-]|$)'
    device_replacement: 'Archos $1'
    brand_replacement: 'Archos'
    model_replacement: '$1'
  - regex: '; *(AN(?:7|8|9|10|13)[A-Z0-9]{1,4}) Build'
    device_replacement: 'Archos $1'
    brand_replacement: 'Archos'
    model_replacement: '$1'
  - regex: '; *(A28|A32|A43|A70(?:BHT|CHT|HB|S|X)|A101(?:B|C|IT)|A7EB|A7EB-WK|101G9|80G9) Build'
    device_replacement: 'Archos $1'
    brand_replacement: 'Archos'
    model_replacement: '$1'

  #########
  # A-rival
  # @ref: http://www.a-rival.de/de/
  #########
  - regex: '; *(PAD-FMD[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Arival'
    model_replacement: '$1'
  - regex: '; *(BioniQ) ?([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Arival'
    model_replacement: '$1 $2'

  #########
  # Arnova
  # @ref: http://arnovatech.com/
  #########
  - regex: '; *(AN\d[^;/]+|ARCHM\d+) Build'
    device_replacement: 'Arnova $1'
    brand_replacement: 'Arnova'
    model_replacement: '$1'
  - regex: '; *(?:ARNOVA|Arnova) ?([^;/]+) Build'
    device_replacement: 'Arnova $1'
    brand_replacement: 'Arnova'
    model_replacement: '$1'

  #########
  # Assistant
  # @ref: http://www.assistant.ua
  #########
  - regex: '; *(?:ASSISTANT |)(AP)-?([1789]\d{2}[A-Z]{0,2}|80104) Build'
    device_replacement: 'Assistant $1-$2'
    brand_replacement: 'Assistant'
    model_replacement: '$1-$2'

  #########
  # Asus
  # @ref: http://www.asus.com/uk/Tablets_Mobile/
  #########
  - regex: '; *(ME17\d[^;/]*|ME3\d{2}[^;/]+|K00[A-Z]|Nexus 10|Nexus 7(?: 2013|)|PadFone[^;/]*|Transformer[^;/]*|TF\d{3}[^;/]*|eeepc) Build'
    device_replacement: 'Asus $1'
    brand_replacement: 'Asus'
    model_replacement: '$1'
  - regex: '; *ASUS[ _]*([^;/]+) Build'
    device_replacement: 'Asus $1'
    brand_replacement: 'Asus'
    model_replacement: '$1'

  #########
  # Garmin-Asus
  #########
  - regex: '; *Garmin-Asus ([^;/]+) Build'
    device_replacement: 'Garmin-Asus $1'
    brand_replacement: 'Garmin-Asus'
    model_replacement: '$1'
  - regex: '; *(Garminfone) Build'
    device_replacement: 'Garmin $1'
    brand_replacement: 'Garmin-Asus'
    model_replacement: '$1'

  #########
  # Attab
  # @ref: http://www.theattab.com/
  #########
  - regex: '; (@TAB-[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Attab'
    model_replacement: '$1'

  #########
  # Audiosonic
  # @ref: ??
  # @note: Take care with Docomo T-01 Toshiba
  #########
  - regex: '; *(T-(?:07|[^0]\d)[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Audiosonic'
    model_replacement: '$1'

  #########
  # Axioo
  # @ref: http://www.axiooworld.com/ww/index.php
  #########
  - regex: '; *(?:Axioo[ _\-]([^;/]+)|(picopad)[ _\-]([^;/]+)) Build'
    regex_flag: 'i'
    device_replacement: 'Axioo $1$2 $3'
    brand_replacement: 'Axioo'
    model_replacement: '$1$2 $3'

  #########
  # Azend
  # @ref: http://azendcorp.com/index.php/products/portable-electronics
  #########
  - regex: '; *(V(?:100|700|800)[^;/]*) Build'
    device_replacement: '$1'
    brand_replacement: 'Azend'
    model_replacement: '$1'

  #########
  # Bak
  # @ref: http://www.bakinternational.com/produtos.php?cat=80
  #########
  - regex: '; *(IBAK\-[^;/]*) Build'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'Bak'
    model_replacement: '$1'

  #########
  # Bedove
  # @ref: http://www.bedove.com/product.html
  # @models: HY6501|HY5001|X12|X21|I5
  #########
  - regex: '; *(HY5001|HY6501|X12|X21|I5) Build'
    device_replacement: 'Bedove $1'
    brand_replacement: 'Bedove'
    model_replacement: '$1'

  #########
  # Benss
  # @ref: http://www.benss.net/
  #########
  - regex: '; *(JC-[^;/]*) Build'
    device_replacement: 'Benss $1'
    brand_replacement: 'Benss'
    model_replacement: '$1'

  #########
  # Blackberry
  # @ref: http://uk.blackberry.com/
  # @note: Android Apps seams to be used here
  #########
  - regex: '; *(BB) ([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Blackberry'
    model_replacement: '$2'

  #########
  # Blackbird
  # @ref: http://iblackbird.co.kr
  #########
  - regex: '; *(BlackBird)[ _](I8.*) Build'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'
  - regex: '; *(BlackBird)[ _](.*) Build'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'

  #########
  # Blaupunkt
  # @ref: http://www.blaupunkt.com
  #########
  # Endeavour
  - regex: '; *([0-9]+BP[EM][^;/]*|Endeavour[^;/]+) Build'
    device_replacement: 'Blaupunkt $1'
    brand_replacement: 'Blaupunkt'
    model_replacement: '$1'

  #########
  # Blu
  # @ref: http://bluproducts.com
  #########
  - regex: '; *((?:BLU|Blu)[ _\-])([^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Blu'
    model_replacement: '$2'
  # BMOBILE = operator branded device
  - regex: '; *(?:BMOBILE )?(Blu|BLU|DASH [^;/]+|VIVO 4\.3|TANK 4\.5) Build'
    device_replacement: '$1'
    brand_replacement: 'Blu'
    model_replacement: '$1'

  #########
  # Blusens
  # @ref: http://www.blusens.com/es/?sg=1&sv=al&roc=1
  #########
  # tablet
  - regex: '; *(TOUCH\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Blusens'
    model_replacement: '$1'

  #########
  # Bmobile
  # @ref: http://bmobile.eu.com/?categoria=smartphones-2
  # @note: Might collide with Maxx as AX is used also there.
  #########
  # smartphone
  - regex: '; *(AX5\d+) Build'
    device_replacement: '$1'
    brand_replacement: 'Bmobile'
    model_replacement: '$1'

  #########
  # bq
  # @ref: http://bqreaders.com
  #########
  - regex: '; *([Bb]q) ([^;/]+);? Build'
    device_replacement: '$1 $2'
    brand_replacement: 'bq'
    model_replacement: '$2'
  - regex: '; *(Maxwell [^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'bq'
    model_replacement: '$1'

  #########
  # Braun Phototechnik
  # @ref: http://www.braun-phototechnik.de/en/products/list/~pcat.250/Tablet-PC.html
  #########
  - regex: '; *((?:B-Tab|B-TAB) ?\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Braun'
    model_replacement: '$1'

  #########
  # Broncho
  # @ref: http://www.broncho.cn/
  #########
  - regex: '; *(Broncho) ([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'

  #########
  # Captiva
  # @ref: http://www.captiva-power.de
  #########
  - regex: '; *CAPTIVA ([^;/]+) Build'
    device_replacement: 'Captiva $1'
    brand_replacement: 'Captiva'
    model_replacement: '$1'

  #########
  # Casio
  # @ref: http://www.casiogzone.com/
  #########
  - regex: '; *(C771|CAL21|IS11CA) Build'
    device_replacement: '$1'
    brand_replacement: 'Casio'
    model_replacement: '$1'

  #########
  # Cat
  # @ref: http://www.cat-sound.com
  #########
  - regex: '; *(?:Cat|CAT) ([^;/]+) Build'
    device_replacement: 'Cat $1'
    brand_replacement: 'Cat'
    model_replacement: '$1'
  - regex: '; *(?:Cat)(Nova.*) Build'
    device_replacement: 'Cat $1'
    brand_replacement: 'Cat'
    model_replacement: '$1'
  - regex: '; *(INM8002KP|ADM8000KP_[AB]) Build'
    device_replacement: '$1'
    brand_replacement: 'Cat'
    model_replacement: 'Tablet PHOENIX 8.1J0'

  #########
  # Celkon
  # @ref: http://www.celkonmobiles.com/?_a=products
  # @models: A10, A19Q, A101, A105, A107, A107\+, A112, A118, A119, A119Q, A15, A19, A20, A200, A220, A225, A22 Race, A27, A58, A59, A60, A62, A63, A64, A66, A67, A69, A75, A77, A79, A8\+, A83, A85, A86, A87, A89 Ultima, A9\+, A90, A900, A95, A97i, A98, AR 40, AR 45, AR 50, ML5
  #########
  - regex: '; *(?:[Cc]elkon[ _\*]|CELKON[ _\*])([^;/\)]+) ?(?:Build|;|\))'
    device_replacement: '$1'
    brand_replacement: 'Celkon'
    model_replacement: '$1'
  - regex: 'Build/(?:[Cc]elkon)+_?([^;/_\)]+)'
    device_replacement: '$1'
    brand_replacement: 'Celkon'
    model_replacement: '$1'
  - regex: '; *(CT)-?(\d+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Celkon'
    model_replacement: '$1$2'
  # smartphones
  - regex: '; *(A19|A19Q|A105|A107[^;/\)]*) ?(?:Build|;|\))'
    device_replacement: '$1'
    brand_replacement: 'Celkon'
    model_replacement: '$1'

  #########
  # ChangJia
  # @ref: http://www.cjshowroom.com/eproducts.aspx?classcode=004001001
  # @brief: China manufacturer makes tablets for different small brands
  #         (eg. http://www.zeepad.net/index.html)
  #########
  - regex: '; *(TPC[0-9]{4,5}) Build'
    device_replacement: '$1'
    brand_replacement: 'ChangJia'
    model_replacement: '$1'

  #########
  # Cloudfone
  # @ref: http://www.cloudfonemobile.com/
  #########
  - regex: '; *(Cloudfone)[ _](Excite)([^ ][^;/]+) Build'
    device_replacement: '$1 $2 $3'
    brand_replacement: 'Cloudfone'
    model_replacement: '$1 $2 $3'
  - regex: '; *(Excite|ICE)[ _](\d+[^;/]+) Build'
    device_replacement: 'Cloudfone $1 $2'
    brand_replacement: 'Cloudfone'
    model_replacement: 'Cloudfone $1 $2'
  - regex: '; *(Cloudfone|CloudPad)[ _]([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Cloudfone'
    model_replacement: '$1 $2'

  #########
  # Cmx
  # @ref: http://cmx.at/de/
  #########
  - regex: '; *((?:Aquila|Clanga|Rapax)[^;/]+) Build'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'Cmx'
    model_replacement: '$1'

  #########
  # CobyKyros
  # @ref: http://cobykyros.com
  # @note: Be careful with MID\d{3} from MpMan or Manta
  #########
  - regex: '; *(?:CFW-|Kyros )?(MID[0-9]{4}(?:[ABC]|SR|TV)?)(\(3G\)-4G| GB 8K| 3G| 8K| GB)? *(?:Build|[;\)])'
    device_replacement: 'CobyKyros $1$2'
    brand_replacement: 'CobyKyros'
    model_replacement: '$1$2'

  #########
  # Coolpad
  # @ref: ??
  #########
  - regex: '; *([^;/]*)Coolpad[ _]([^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Coolpad'
    model_replacement: '$1$2'

  #########
  # Cube
  # @ref: http://www.cube-tablet.com/buy-products.html
  #########
  - regex: '; *(CUBE[ _])?([KU][0-9]+ ?GT.*|A5300) Build'
    regex_flag: 'i'
    device_replacement: '$1$2'
    brand_replacement: 'Cube'
    model_replacement: '$2'

  #########
  # Cubot
  # @ref: http://www.cubotmall.com/
  #########
  - regex: '; *CUBOT ([^;/]+) Build'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'Cubot'
    model_replacement: '$1'
  - regex: '; *(BOBBY) Build'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'Cubot'
    model_replacement: '$1'

  #########
  # Danew
  # @ref: http://www.danew.com/produits-tablette.php
  #########
  - regex: '; *(Dslide [^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Danew'
    model_replacement: '$1'

  #########
  # Dell
  # @ref: http://www.dell.com
  # @ref: http://www.softbank.jp/mobile/support/product/101dl/
  # @ref: http://www.softbank.jp/mobile/support/product/001dl/
  # @ref: http://developer.emnet.ne.jp/android.html
  # @ref: http://www.dell.com/in/p/mobile-xcd28/pd
  # @ref: http://www.dell.com/in/p/mobile-xcd35/pd
  #########
  - regex: '; *(XCD)[ _]?(28|35) Build'
    device_replacement: 'Dell $1$2'
    brand_replacement: 'Dell'
    model_replacement: '$1$2'
  - regex: '; *(001DL) Build'
    device_replacement: 'Dell $1'
    brand_replacement: 'Dell'
    model_replacement: 'Streak'
  - regex: '; *(?:Dell|DELL) (Streak) Build'
    device_replacement: 'Dell $1'
    brand_replacement: 'Dell'
    model_replacement: 'Streak'
  - regex: '; *(101DL|GS01|Streak Pro[^;/]*) Build'
    device_replacement: 'Dell $1'
    brand_replacement: 'Dell'
    model_replacement: 'Streak Pro'
  - regex: '; *([Ss]treak ?7) Build'
    device_replacement: 'Dell $1'
    brand_replacement: 'Dell'
    model_replacement: 'Streak 7'
  - regex: '; *(Mini-3iX) Build'
    device_replacement: 'Dell $1'
    brand_replacement: 'Dell'
    model_replacement: '$1'
  - regex: '; *(?:Dell|DELL)[ _](Aero|Venue|Thunder|Mini.*|Streak[ _]Pro) Build'
    device_replacement: 'Dell $1'
    brand_replacement: 'Dell'
    model_replacement: '$1'
  - regex: '; *Dell[ _]([^;/]+) Build'
    device_replacement: 'Dell $1'
    brand_replacement: 'Dell'
    model_replacement: '$1'
  - regex: '; *Dell ([^;/]+) Build'
    device_replacement: 'Dell $1'
    brand_replacement: 'Dell'
    model_replacement: '$1'

  #########
  # Denver
  # @ref: http://www.denver-electronics.com/tablets1/
  #########
  - regex: '; *(TA[CD]-\d+[^;/]*) Build'
    device_replacement: '$1'
    brand_replacement: 'Denver'
    model_replacement: '$1'

  #########
  # Dex
  # @ref: http://dex.ua/
  #########
  - regex: '; *(iP[789]\d{2}(?:-3G)?|IP10\d{2}(?:-8GB)?) Build'
    device_replacement: '$1'
    brand_replacement: 'Dex'
    model_replacement: '$1'

  #########
  # DNS AirTab
  # @ref: http://www.dns-shop.ru/
  #########
  - regex: '; *(AirTab)[ _\-]([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'DNS'
    model_replacement: '$1 $2'

  #########
  # Docomo (Operator Branded Device)
  # @ref: http://www.ipentec.com/document/document.aspx?page=android-useragent
  #########
  - regex: '; *(F\-\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Fujitsu'
    model_replacement: '$1'
  - regex: '; *(HT-03A) Build'
    device_replacement: '$1'
    brand_replacement: 'HTC'
    model_replacement: 'Magic'
  - regex: '; *(HT\-\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'HTC'
    model_replacement: '$1'
  - regex: '; *(L\-\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'LG'
    model_replacement: '$1'
  - regex: '; *(N\-\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Nec'
    model_replacement: '$1'
  - regex: '; *(P\-\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Panasonic'
    model_replacement: '$1'
  - regex: '; *(SC\-\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'
  - regex: '; *(SH\-\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Sharp'
    model_replacement: '$1'
  - regex: '; *(SO\-\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'SonyEricsson'
    model_replacement: '$1'
  - regex: '; *(T\-0[12][^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Toshiba'
    model_replacement: '$1'

  #########
  # DOOV
  # @ref: http://www.doov.com.cn/
  #########
  - regex: '; *(DOOV)[ _]([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'DOOV'
    model_replacement: '$2'

  #########
  # Enot
  # @ref: http://www.enot.ua/
  #########
  - regex: '; *(Enot|ENOT)[ -]?([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Enot'
    model_replacement: '$2'

  #########
  # Evercoss
  # @ref: http://evercoss.com/android/
  #########
  - regex: '; *[^;/]+ Build/(?:CROSS|Cross)+[ _\-]([^\)]+)'
    device_replacement: 'CROSS $1'
    brand_replacement: 'Evercoss'
    model_replacement: 'Cross $1'
  - regex: '; *(CROSS|Cross)[ _\-]([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Evercoss'
    model_replacement: 'Cross $2'

  #########
  # Explay
  # @ref: http://explay.ru/
  #########
  - regex: '; *Explay[_ ](.+?)(?:[\)]| Build)'
    device_replacement: '$1'
    brand_replacement: 'Explay'
    model_replacement: '$1'

  #########
  # Fly
  # @ref: http://www.fly-phone.com/
  #########
  - regex: '; *(IQ.*) Build'
    device_replacement: '$1'
    brand_replacement: 'Fly'
    model_replacement: '$1'
  - regex: '; *(Fly|FLY)[ _](IQ[^;]+|F[34]\d+[^;]*);? Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Fly'
    model_replacement: '$2'

  #########
  # Fujitsu
  # @ref: http://www.fujitsu.com/global/
  #########
  - regex: '; *(M532|Q572|FJL21) Build/'
    device_replacement: '$1'
    brand_replacement: 'Fujitsu'
    model_replacement: '$1'

  #########
  # Galapad
  # @ref: http://www.galapad.net/product.html
  #########
  - regex: '; *(G1) Build'
    device_replacement: '$1'
    brand_replacement: 'Galapad'
    model_replacement: '$1'

  #########
  # Geeksphone
  # @ref: http://www.geeksphone.com/
  #########
  - regex: '; *(Geeksphone) ([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'

  #########
  # Gfive
  # @ref: http://www.gfivemobile.com/en
  #########
  #- regex: '; *(G\'?FIVE) ([^;/]+) Build' # there is a problem with python yaml parser here
  - regex: '; *(G[^F]?FIVE) ([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Gfive'
    model_replacement: '$2'

  #########
  # Gionee
  # @ref: http://www.gionee.com/
  #########
  - regex: '; *(Gionee)[ _\-]([^;/]+)(?:/[^;/]+|) Build'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'Gionee'
    model_replacement: '$2'
  - regex: '; *(GN\d+[A-Z]?|INFINITY_PASSION|Ctrl_V1) Build'
    device_replacement: 'Gionee $1'
    brand_replacement: 'Gionee'
    model_replacement: '$1'
  - regex: '; *(E3) Build/JOP40D'
    device_replacement: 'Gionee $1'
    brand_replacement: 'Gionee'
    model_replacement: '$1'
  - regex: '\sGIONEE[-\s_](\w*)'
    regex_flag: 'i'
    device_replacement: 'Gionee $1'
    brand_replacement: 'Gionee'
    model_replacement: '$1'

  #########
  # GoClever
  # @ref: http://www.goclever.com
  #########
  - regex: '; *((?:FONE|QUANTUM|INSIGNIA) \d+[^;/]*|PLAYTAB) Build'
    device_replacement: 'GoClever $1'
    brand_replacement: 'GoClever'
    model_replacement: '$1'
  - regex: '; *GOCLEVER ([^;/]+) Build'
    device_replacement: 'GoClever $1'
    brand_replacement: 'GoClever'
    model_replacement: '$1'

  #########
  # Google
  # @ref: http://www.google.de/glass/start/
  #########
  - regex: '; *(Glass \d+) Build'
    device_replacement: '$1'
    brand_replacement: 'Google'
    model_replacement: '$1'
  - regex: '; *(Pixel.*) Build'
    device_replacement: '$1'
    brand_replacement: 'Google'
    model_replacement: '$1'

  #########
  # Gigabyte
  # @ref: http://gsmart.gigabytecm.com/en/
  #########
  - regex: '; *(GSmart)[ -]([^/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Gigabyte'
    model_replacement: '$1 $2'

  #########
  # Freescale development boards
  # @ref: http://www.freescale.com/webapp/sps/site/prod_summary.jsp?code=IMX53QSB
  #########
  - regex: '; *(imx5[13]_[^/]+) Build'
    device_replacement: 'Freescale $1'
    brand_replacement: 'Freescale'
    model_replacement: '$1'

  #########
  # Haier
  # @ref: http://www.haier.com/
  # @ref: http://www.haier.com/de/produkte/tablet/
  #########
  - regex: '; *Haier[ _\-]([^/]+) Build'
    device_replacement: 'Haier $1'
    brand_replacement: 'Haier'
    model_replacement: '$1'
  - regex: '; *(PAD1016) Build'
    device_replacement: 'Haipad $1'
    brand_replacement: 'Haipad'
    model_replacement: '$1'

  #########
  # Haipad
  # @ref: http://www.haipad.net/
  # @models: V7P|M7SM7S|M9XM9X|M7XM7X|M9|M8|M7-M|M1002|M7|M701
  #########
  - regex: '; *(M701|M7|M8|M9) Build'
    device_replacement: 'Haipad $1'
    brand_replacement: 'Haipad'
    model_replacement: '$1'

  #########
  # Hannspree
  # @ref: http://www.hannspree.eu/
  # @models: SN10T1|SN10T2|SN70T31B|SN70T32W
  #########
  - regex: '; *(SN\d+T[^;\)/]*)(?: Build|[;\)])'
    device_replacement: 'Hannspree $1'
    brand_replacement: 'Hannspree'
    model_replacement: '$1'

  #########
  # HCLme
  # @ref: http://www.hclmetablet.com/india/
  #########
  - regex: 'Build/HCL ME Tablet ([^;\)]+)[\);]'
    device_replacement: 'HCLme $1'
    brand_replacement: 'HCLme'
    model_replacement: '$1'
  - regex: '; *([^;\/]+) Build/HCL'
    device_replacement: 'HCLme $1'
    brand_replacement: 'HCLme'
    model_replacement: '$1'

  #########
  # Hena
  # @ref: http://www.henadigital.com/en/product/index.asp?id=6
  #########
  - regex: '; *(MID-?\d{4}C[EM]) Build'
    device_replacement: 'Hena $1'
    brand_replacement: 'Hena'
    model_replacement: '$1'

  #########
  # Hisense
  # @ref: http://www.hisense.com/
  #########
  - regex: '; *(EG\d{2,}|HS-[^;/]+|MIRA[^;/]+) Build'
    device_replacement: 'Hisense $1'
    brand_replacement: 'Hisense'
    model_replacement: '$1'
  - regex: '; *(andromax[^;/]+) Build'
    regex_flag: 'i'
    device_replacement: 'Hisense $1'
    brand_replacement: 'Hisense'
    model_replacement: '$1'

  #########
  # hitech
  # @ref: http://www.hitech-mobiles.com/
  #########
  - regex: '; *(?:AMAZE[ _](S\d+)|(S\d+)[ _]AMAZE) Build'
    device_replacement: 'AMAZE $1$2'
    brand_replacement: 'hitech'
    model_replacement: 'AMAZE $1$2'

  #########
  # HP
  # @ref: http://www.hp.com/
  #########
  - regex: '; *(PlayBook) Build'
    device_replacement: 'HP $1'
    brand_replacement: 'HP'
    model_replacement: '$1'
  - regex: '; *HP ([^/]+) Build'
    device_replacement: 'HP $1'
    brand_replacement: 'HP'
    model_replacement: '$1'
  - regex: '; *([^/]+_tenderloin) Build'
    device_replacement: 'HP TouchPad'
    brand_replacement: 'HP'
    model_replacement: 'TouchPad'

  #########
  # Huawei
  # @ref: http://www.huaweidevice.com
  # @note: Needs to be before HTC due to Desire HD Build on U8815
  #########
  - regex: '; *(HUAWEI |Huawei-|)([UY][^;/]+) Build/(?:Huawei|HUAWEI)([UY][^\);]+)\)'
    device_replacement: '$1$2'
    brand_replacement: 'Huawei'
    model_replacement: '$2'
  - regex: '; *([^;/]+) Build[/ ]Huawei(MT1-U06|[A-Z]+\d+[^\);]+)[^\);]*\)'
    device_replacement: '$1'
    brand_replacement: 'Huawei'
    model_replacement: '$2'
  - regex: '; *(S7|M860) Build'
    device_replacement: '$1'
    brand_replacement: 'Huawei'
    model_replacement: '$1'
  - regex: '; *((?:HUAWEI|Huawei)[ \-]?)(MediaPad) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Huawei'
    model_replacement: '$2'
  - regex: '; *((?:HUAWEI[ _]?|Huawei[ _]|)Ascend[ _])([^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Huawei'
    model_replacement: '$2'
  - regex: '; *((?:HUAWEI|Huawei)[ _\-]?)((?:G700-|MT-)[^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Huawei'
    model_replacement: '$2'
  - regex: '; *((?:HUAWEI|Huawei)[ _\-]?)([^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Huawei'
    model_replacement: '$2'
  - regex: '; *(MediaPad[^;]+|SpringBoard) Build/Huawei'
    device_replacement: '$1'
    brand_replacement: 'Huawei'
    model_replacement: '$1'
  - regex: '; *([^;]+) Build/(?:Huawei|HUAWEI)'
    device_replacement: '$1'
    brand_replacement: 'Huawei'
    model_replacement: '$1'
  - regex: '; *([Uu])([89]\d{3}) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Huawei'
    model_replacement: 'U$2'
  - regex: '; *(?:Ideos |IDEOS )(S7) Build'
    device_replacement: 'Huawei Ideos$1'
    brand_replacement: 'Huawei'
    model_replacement: 'Ideos$1'
  - regex: '; *(?:Ideos |IDEOS )([^;/]+\s*|\s*)Build'
    device_replacement: 'Huawei Ideos$1'
    brand_replacement: 'Huawei'
    model_replacement: 'Ideos$1'
  - regex: '; *(Orange Daytona|Pulse|Pulse Mini|Vodafone 858|C8500|C8600|C8650|C8660|Nexus 6P|ATH-.+?) Build[/ ]'
    device_replacement: 'Huawei $1'
    brand_replacement: 'Huawei'
    model_replacement: '$1'
  - regex: '; *((?:[A-Z]{3})\-L[A-Za0-9]{2})[\)]'
    device_replacement: 'Huawei $1'
    brand_replacement: 'Huawei'
    model_replacement: '$1'

  #########
  # HTC
  # @ref: http://www.htc.com/www/products/
  # @ref: http://en.wikipedia.org/wiki/List_of_HTC_phones
  #########

  - regex: '; *HTC[ _]([^;]+); Windows Phone'
    device_replacement: 'HTC $1'
    brand_replacement: 'HTC'
    model_replacement: '$1'

  # Android HTC with Version Number matcher
  # ; HTC_0P3Z11/1.12.161.3 Build
  # ;HTC_A3335 V2.38.841.1 Build
  - regex: '; *(?:HTC[ _/])+([^ _/]+)(?:[/\\]1\.0 | V|/| +)\d+\.\d[\d\.]*(?: *Build|\))'
    device_replacement: 'HTC $1'
    brand_replacement: 'HTC'
    model_replacement: '$1'
  - regex: '; *(?:HTC[ _/])+([^ _/]+)(?:[ _/]([^ _/]+)|)(?:[/\\]1\.0 | V|/| +)\d+\.\d[\d\.]*(?: *Build|\))'
    device_replacement: 'HTC $1 $2'
    brand_replacement: 'HTC'
    model_replacement: '$1 $2'
  - regex: '; *(?:HTC[ _/])+([^ _/]+)(?:[ _/]([^ _/]+)(?:[ _/]([^ _/]+)|)|)(?:[/\\]1\.0 | V|/| +)\d+\.\d[\d\.]*(?: *Build|\))'
    device_replacement: 'HTC $1 $2 $3'
    brand_replacement: 'HTC'
    model_replacement: '$1 $2 $3'
  - regex: '; *(?:HTC[ _/])+([^ _/]+)(?:[ _/]([^ _/]+)(?:[ _/]([^ _/]+)(?:[ _/]([^ _/]+)|)|)|)(?:[/\\]1\.0 | V|/| +)\d+\.\d[\d\.]*(?: *Build|\))'
    device_replacement: 'HTC $1 $2 $3 $4'
    brand_replacement: 'HTC'
    model_replacement: '$1 $2 $3 $4'

  # Android HTC without Version Number matcher
  - regex: '; *(?:(?:HTC|htc)(?:_blocked|)[ _/])+([^ _/;]+)(?: *Build|[;\)]| - )'
    device_replacement: 'HTC $1'
    brand_replacement: 'HTC'
    model_replacement: '$1'
  - regex: '; *(?:(?:HTC|htc)(?:_blocked|)[ _/])+([^ _/]+)(?:[ _/]([^ _/;\)]+)|)(?: *Build|[;\)]| - )'
    device_replacement: 'HTC $1 $2'
    brand_replacement: 'HTC'
    model_replacement: '$1 $2'
  - regex: '; *(?:(?:HTC|htc)(?:_blocked|)[ _/])+([^ _/]+)(?:[ _/]([^ _/]+)(?:[ _/]([^ _/;\)]+)|)|)(?: *Build|[;\)]| - )'
    device_replacement: 'HTC $1 $2 $3'
    brand_replacement: 'HTC'
    model_replacement: '$1 $2 $3'
  - regex: '; *(?:(?:HTC|htc)(?:_blocked|)[ _/])+([^ _/]+)(?:[ _/]([^ _/]+)(?:[ _/]([^ _/]+)(?:[ _/]([^ /;]+)|)|)|)(?: *Build|[;\)]| - )'
    device_replacement: 'HTC $1 $2 $3 $4'
    brand_replacement: 'HTC'
    model_replacement: '$1 $2 $3 $4'

  # HTC Streaming Player
  - regex: 'HTC Streaming Player [^\/]*/[^\/]*/ htc_([^/]+) /'
    device_replacement: 'HTC $1'
    brand_replacement: 'HTC'
    model_replacement: '$1'
  # general matcher for anything else
  - regex: '(?:[;,] *|^)(?:htccn_chs-|)HTC[ _-]?([^;]+?)(?: *Build|clay|Android|-?Mozilla| Opera| Profile| UNTRUSTED|[;/\(\)]|$)'
    regex_flag: 'i'
    device_replacement: 'HTC $1'
    brand_replacement: 'HTC'
    model_replacement: '$1'
  # Android matchers without HTC
  - regex: '; *(A6277|ADR6200|ADR6300|ADR6350|ADR6400[A-Z]*|ADR6425[A-Z]*|APX515CKT|ARIA|Desire[^_ ]*|Dream|EndeavorU|Eris|Evo|Flyer|HD2|Hero|HERO200|Hero CDMA|HTL21|Incredible|Inspire[A-Z0-9]*|Legend|Liberty|Nexus ?(?:One|HD2)|One|One S C2|One[ _]?(?:S|V|X\+?)\w*|PC36100|PG06100|PG86100|S31HT|Sensation|Wildfire)(?: Build|[/;\(\)])'
    regex_flag: 'i'
    device_replacement: 'HTC $1'
    brand_replacement: 'HTC'
    model_replacement: '$1'
  - regex: '; *(ADR6200|ADR6400L|ADR6425LVW|Amaze|DesireS?|EndeavorU|Eris|EVO|Evo\d[A-Z]+|HD2|IncredibleS?|Inspire[A-Z0-9]*|Inspire[A-Z0-9]*|Sensation[A-Z0-9]*|Wildfire)[ _-](.+?)(?:[/;\)]|Build|MIUI|1\.0)'
    regex_flag: 'i'
    device_replacement: 'HTC $1 $2'
    brand_replacement: 'HTC'
    model_replacement: '$1 $2'

  #########
  # Hyundai
  # @ref: http://www.hyundaitechnologies.com
  #########
  - regex: '; *HYUNDAI (T\d[^/]*) Build'
    device_replacement: 'Hyundai $1'
    brand_replacement: 'Hyundai'
    model_replacement: '$1'
  - regex: '; *HYUNDAI ([^;/]+) Build'
    device_replacement: 'Hyundai $1'
    brand_replacement: 'Hyundai'
    model_replacement: '$1'
  # X900? http://www.amazon.com/Hyundai-X900-Retina-Android-Bluetooth/dp/B00AO07H3O
  - regex: '; *(X700|Hold X|MB-6900) Build'
    device_replacement: 'Hyundai $1'
    brand_replacement: 'Hyundai'
    model_replacement: '$1'

  #########
  # iBall
  # @ref: http://www.iball.co.in/Category/Mobiles/22
  #########
  - regex: '; *(?:iBall[ _\-]|)(Andi)[ _]?(\d[^;/]*) Build'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'iBall'
    model_replacement: '$1 $2'
  - regex: '; *(IBall)(?:[ _]([^;/]+)|) Build'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'iBall'
    model_replacement: '$2'

  #########
  # IconBIT
  # @ref: http://www.iconbit.com/catalog/tablets/
  #########
  - regex: '; *(NT-\d+[^ ;/]*|Net[Tt]AB [^;/]+|Mercury [A-Z]+|iconBIT)(?: S/N:[^;/]+|) Build'
    device_replacement: '$1'
    brand_replacement: 'IconBIT'
    model_replacement: '$1'

  #########
  # IMO
  # @ref: http://www.ponselimo.com/
  #########
  - regex: '; *(IMO)[ _]([^;/]+) Build'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'IMO'
    model_replacement: '$2'

  #########
  # i-mobile
  # @ref: http://www.i-mobilephone.com/
  #########
  - regex: '; *i-?mobile[ _]([^/]+) Build/'
    regex_flag: 'i'
    device_replacement: 'i-mobile $1'
    brand_replacement: 'imobile'
    model_replacement: '$1'
  - regex: '; *(i-(?:style|note)[^/]*) Build/'
    regex_flag: 'i'
    device_replacement: 'i-mobile $1'
    brand_replacement: 'imobile'
    model_replacement: '$1'

  #########
  # Impression
  # @ref: http://impression.ua/planshetnye-kompyutery
  #########
  - regex: '; *(ImPAD) ?(\d+(?:.)*) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Impression'
    model_replacement: '$1 $2'

  #########
  # Infinix
  # @ref: http://www.infinixmobility.com/index.html
  #########
  - regex: '; *(Infinix)[ _]([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Infinix'
    model_replacement: '$2'

  #########
  # Informer
  # @ref: ??
  #########
  - regex: '; *(Informer)[ \-]([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Informer'
    model_replacement: '$2'

  #########
  # Intenso
  # @ref: http://www.intenso.de
  # @models: 7"":TAB 714,TAB 724;8"":TAB 814,TAB 824;10"":TAB 1004
  #########
  - regex: '; *(TAB) ?([78][12]4) Build'
    device_replacement: 'Intenso $1'
    brand_replacement: 'Intenso'
    model_replacement: '$1 $2'

  #########
  # Intex
  # @ref: http://intexmobile.in/index.aspx
  # @note: Zync also offers a ""Cloud Z5"" device
  #########
  # smartphones
  - regex: '; *(?:Intex[ _]|)(AQUA|Aqua)([ _\.\-])([^;/]+) *(?:Build|;)'
    device_replacement: '$1$2$3'
    brand_replacement: 'Intex'
    model_replacement: '$1 $3'
  # matches ""INTEX CLOUD X1""
  - regex: '; *(?:INTEX|Intex)(?:[_ ]([^\ _;/]+))(?:[_ ]([^\ _;/]+)|) *(?:Build|;)'
    device_replacement: '$1 $2'
    brand_replacement: 'Intex'
    model_replacement: '$1 $2'
  # tablets
  - regex: '; *([iI]Buddy)[ _]?(Connect)(?:_|\?_| |)([^;/]*) *(?:Build|;)'
    device_replacement: '$1 $2 $3'
    brand_replacement: 'Intex'
    model_replacement: 'iBuddy $2 $3'
  - regex: '; *(I-Buddy)[ _]([^;/]+) *(?:Build|;)'
    device_replacement: '$1 $2'
    brand_replacement: 'Intex'
    model_replacement: 'iBuddy $2'

  #########
  # iOCEAN
  # @ref: http://www.iocean.cc/
  #########
  - regex: '; *(iOCEAN) ([^/]+) Build'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'iOCEAN'
    model_replacement: '$2'

  #########
  # i.onik
  # @ref: http://www.i-onik.de/
  #########
  - regex: '; *(TP\d+(?:\.\d+|)\-\d[^;/]+) Build'
    device_replacement: 'ionik $1'
    brand_replacement: 'ionik'
    model_replacement: '$1'

  #########
  # IRU.ru
  # @ref: http://www.iru.ru/catalog/soho/planetable/
  #########
  - regex: '; *(M702pro) Build'
    device_replacement: '$1'
    brand_replacement: 'Iru'
    model_replacement: '$1'

  #########
  # Ivio
  # @ref: http://www.ivio.com/mobile.php
  # @models: DG80,DG20,DE38,DE88,MD70
  #########
  - regex: '; *(DE88Plus|MD70) Build'
    device_replacement: '$1'
    brand_replacement: 'Ivio'
    model_replacement: '$1'
  - regex: '; *IVIO[_\-]([^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Ivio'
    model_replacement: '$1'

  #########
  # Jaytech
  # @ref: http://www.jay-tech.de/jaytech/servlet/frontend/
  #########
  - regex: '; *(TPC-\d+|JAY-TECH) Build'
    device_replacement: '$1'
    brand_replacement: 'Jaytech'
    model_replacement: '$1'

  #########
  # Jiayu
  # @ref: http://www.ejiayu.com/en/Product.html
  #########
  - regex: '; *(JY-[^;/]+|G[234]S?) Build'
    device_replacement: '$1'
    brand_replacement: 'Jiayu'
    model_replacement: '$1'

  #########
  # JXD
  # @ref: http://www.jxd.hk/
  #########
  - regex: '; *(JXD)[ _\-]([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'JXD'
    model_replacement: '$2'

  #########
  # Karbonn
  # @ref: http://www.karbonnmobiles.com/products_tablet.php
  #########
  - regex: '; *Karbonn[ _]?([^;/]+) *(?:Build|;)'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'Karbonn'
    model_replacement: '$1'
  - regex: '; *([^;]+) Build/Karbonn'
    device_replacement: '$1'
    brand_replacement: 'Karbonn'
    model_replacement: '$1'
  - regex: '; *(A11|A39|A37|A34|ST8|ST10|ST7|Smart Tab3|Smart Tab2|Titanium S\d) +Build'
    device_replacement: '$1'
    brand_replacement: 'Karbonn'
    model_replacement: '$1'

  #########
  # KDDI (Operator Branded Device)
  # @ref: http://www.ipentec.com/document/document.aspx?page=android-useragent
  #########
  - regex: '; *(IS01|IS03|IS05|IS\d{2}SH) Build'
    device_replacement: '$1'
    brand_replacement: 'Sharp'
    model_replacement: '$1'
  - regex: '; *(IS04) Build'
    device_replacement: '$1'
    brand_replacement: 'Regza'
    model_replacement: '$1'
  - regex: '; *(IS06|IS\d{2}PT) Build'
    device_replacement: '$1'
    brand_replacement: 'Pantech'
    model_replacement: '$1'
  - regex: '; *(IS11S) Build'
    device_replacement: '$1'
    brand_replacement: 'SonyEricsson'
    model_replacement: 'Xperia Acro'
  - regex: '; *(IS11CA) Build'
    device_replacement: '$1'
    brand_replacement: 'Casio'
    model_replacement: 'GzOne $1'
  - regex: '; *(IS11LG) Build'
    device_replacement: '$1'
    brand_replacement: 'LG'
    model_replacement: 'Optimus X'
  - regex: '; *(IS11N) Build'
    device_replacement: '$1'
    brand_replacement: 'Medias'
    model_replacement: '$1'
  - regex: '; *(IS11PT) Build'
    device_replacement: '$1'
    brand_replacement: 'Pantech'
    model_replacement: 'MIRACH'
  - regex: '; *(IS12F) Build'
    device_replacement: '$1'
    brand_replacement: 'Fujitsu'
    model_replacement: 'Arrows ES'
  # @ref: https://ja.wikipedia.org/wiki/IS12M
  - regex: '; *(IS12M) Build'
    device_replacement: '$1'
    brand_replacement: 'Motorola'
    model_replacement: 'XT909'
  - regex: '; *(IS12S) Build'
    device_replacement: '$1'
    brand_replacement: 'SonyEricsson'
    model_replacement: 'Xperia Acro HD'
  - regex: '; *(ISW11F) Build'
    device_replacement: '$1'
    brand_replacement: 'Fujitsu'
    model_replacement: 'Arrowz Z'
  - regex: '; *(ISW11HT) Build'
    device_replacement: '$1'
    brand_replacement: 'HTC'
    model_replacement: 'EVO'
  - regex: '; *(ISW11K) Build'
    device_replacement: '$1'
    brand_replacement: 'Kyocera'
    model_replacement: 'DIGNO'
  - regex: '; *(ISW11M) Build'
    device_replacement: '$1'
    brand_replacement: 'Motorola'
    model_replacement: 'Photon'
  - regex: '; *(ISW11SC) Build'
    device_replacement: '$1'
    brand_replacement: 'Samsung'
    model_replacement: 'GALAXY S II WiMAX'
  - regex: '; *(ISW12HT) Build'
    device_replacement: '$1'
    brand_replacement: 'HTC'
    model_replacement: 'EVO 3D'
  - regex: '; *(ISW13HT) Build'
    device_replacement: '$1'
    brand_replacement: 'HTC'
    model_replacement: 'J'
  - regex: '; *(ISW?[0-9]{2}[A-Z]{0,2}) Build'
    device_replacement: '$1'
    brand_replacement: 'KDDI'
    model_replacement: '$1'
  - regex: '; *(INFOBAR [^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'KDDI'
    model_replacement: '$1'

  #########
  # Kingcom
  # @ref: http://www.e-kingcom.com
  #########
  - regex: '; *(JOYPAD|Joypad)[ _]([^;/]+) Build/'
    device_replacement: '$1 $2'
    brand_replacement: 'Kingcom'
    model_replacement: '$1 $2'

  #########
  # Kobo
  # @ref: https://en.wikipedia.org/wiki/Kobo_Inc.
  # @ref: http://www.kobo.com/devices#tablets
  #########
  - regex: '; *(Vox|VOX|Arc|K080) Build/'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'Kobo'
    model_replacement: '$1'
  - regex: '\b(Kobo Touch)\b'
    device_replacement: '$1'
    brand_replacement: 'Kobo'
    model_replacement: '$1'

  #########
  # K-Touch
  # @ref: ??
  #########
  - regex: '; *(K-Touch)[ _]([^;/]+) Build'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'Ktouch'
    model_replacement: '$2'

  #########
  # KT Tech
  # @ref: http://www.kttech.co.kr
  #########
  - regex: '; *((?:EV|KM)-S\d+[A-Z]?) Build'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'KTtech'
    model_replacement: '$1'

  #########
  # Kyocera
  # @ref: http://www.android.com/devices/?country=all&m=kyocera
  #########
  - regex: '; *(Zio|Hydro|Torque|Event|EVENT|Echo|Milano|Rise|URBANO PROGRESSO|WX04K|WX06K|WX10K|KYL21|101K|C5[12]\d{2}) Build/'
    device_replacement: '$1'
    brand_replacement: 'Kyocera'
    model_replacement: '$1'

  #########
  # Lava
  # @ref: http://www.lavamobiles.com/
  #########
  - regex: '; *(?:LAVA[ _]|)IRIS[ _\-]?([^/;\)]+) *(?:;|\)|Build)'
    regex_flag: 'i'
    device_replacement: 'Iris $1'
    brand_replacement: 'Lava'
    model_replacement: 'Iris $1'
  - regex: '; *LAVA[ _]([^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Lava'
    model_replacement: '$1'

  #########
  # Lemon
  # @ref: http://www.lemonmobiles.com/products.php?type=1
  #########
  - regex: '; *(?:(Aspire A1)|(?:LEMON|Lemon)[ _]([^;/]+))_? Build'
    device_replacement: 'Lemon $1$2'
    brand_replacement: 'Lemon'
    model_replacement: '$1$2'

  #########
  # Lenco
  # @ref: http://www.lenco.com/c/tablets/
  #########
  - regex: '; *(TAB-1012) Build/'
    device_replacement: 'Lenco $1'
    brand_replacement: 'Lenco'
    model_replacement: '$1'
  - regex: '; Lenco ([^;/]+) Build/'
    device_replacement: 'Lenco $1'
    brand_replacement: 'Lenco'
    model_replacement: '$1'

  #########
  # Lenovo
  # @ref: http://support.lenovo.com/en_GB/downloads/default.page?#
  #########
  - regex: '; *(A1_07|A2107A-H|S2005A-H|S1-37AH0) Build'
    device_replacement: '$1'
    brand_replacement: 'Lenovo'
    model_replacement: '$1'
  - regex: '; *(Idea[Tp]ab)[ _]([^;/]+);? Build'
    device_replacement: 'Lenovo $1 $2'
    brand_replacement: 'Lenovo'
    model_replacement: '$1 $2'
  - regex: '; *(Idea(?:Tab|pad)) ?([^;/]+) Build'
    device_replacement: 'Lenovo $1 $2'
    brand_replacement: 'Lenovo'
    model_replacement: '$1 $2'
  - regex: '; *(ThinkPad) ?(Tablet) Build/'
    device_replacement: 'Lenovo $1 $2'
    brand_replacement: 'Lenovo'
    model_replacement: '$1 $2'
  - regex: '; *(?:LNV-|)(?:=?[Ll]enovo[ _\-]?|LENOVO[ _])(.+?)(?:Build|[;/\)])'
    device_replacement: 'Lenovo $1'
    brand_replacement: 'Lenovo'
    model_replacement: '$1'
  - regex: '[;,] (?:Vodafone |)(SmartTab) ?(II) ?(\d+) Build/'
    device_replacement: 'Lenovo $1 $2 $3'
    brand_replacement: 'Lenovo'
    model_replacement: '$1 $2 $3'
  - regex: '; *(?:Ideapad |)K1 Build/'
    device_replacement: 'Lenovo Ideapad K1'
    brand_replacement: 'Lenovo'
    model_replacement: 'Ideapad K1'
  - regex: '; *(3GC101|3GW10[01]|A390) Build/'
    device_replacement: '$1'
    brand_replacement: 'Lenovo'
    model_replacement: '$1'
  - regex: '\b(?:Lenovo|LENOVO)+[ _\-]?([^,;:/ ]+)'
    device_replacement: 'Lenovo $1'
    brand_replacement: 'Lenovo'
    model_replacement: '$1'

  #########
  # Lexibook
  # @ref: http://www.lexibook.com/fr
  #########
  - regex: '; *(MFC\d+)[A-Z]{2}([^;,/]*),? Build'
    device_replacement: '$1$2'
    brand_replacement: 'Lexibook'
    model_replacement: '$1$2'

  #########
  # LG
  # @ref: http://www.lg.com/uk/mobile
  #########
  - regex: '; *(E[34][0-9]{2}|LS[6-8][0-9]{2}|VS[6-9][0-9]+[^;/]+|Nexus 4|Nexus 5X?|GT540f?|Optimus (?:2X|G|4X HD)|OptimusX4HD) *(?:Build|;)'
    device_replacement: '$1'
    brand_replacement: 'LG'
    model_replacement: '$1'
  - regex: '[;:] *(L-\d+[A-Z]|LGL\d+[A-Z]?)(?:/V\d+|) *(?:Build|[;\)])'
    device_replacement: '$1'
    brand_replacement: 'LG'
    model_replacement: '$1'
  - regex: '; *(LG-)([A-Z]{1,2}\d{2,}[^,;/\)\(]*?)(?:Build| V\d+|[,;/\)\(]|$)'
    device_replacement: '$1$2'
    brand_replacement: 'LG'
    model_replacement: '$2'
  - regex: '; *(LG[ \-]|LG)([^;/]+)[;/]? Build'
    device_replacement: '$1$2'
    brand_replacement: 'LG'
    model_replacement: '$2'
  - regex: '^(LG)-([^;/]+)/ Mozilla/.*; Android'
    device_replacement: '$1 $2'
    brand_replacement: 'LG'
    model_replacement: '$2'
  - regex: '(Web0S); Linux/(SmartTV)'
    device_replacement: 'LG $1 $2'
    brand_replacement: 'LG'
    model_replacement: '$1 $2'

  #########
  # Malata
  # @ref: http://www.malata.com/en/products.aspx?classid=680
  #########
  - regex: '; *((?:SMB|smb)[^;/]+) Build/'
    device_replacement: '$1'
    brand_replacement: 'Malata'
    model_replacement: '$1'
  - regex: '; *(?:Malata|MALATA) ([^;/]+) Build/'
    device_replacement: '$1'
    brand_replacement: 'Malata'
    model_replacement: '$1'

  #########
  # Manta
  # @ref: http://www.manta.com.pl/en
  #########
  - regex: '; *(MS[45][0-9]{3}|MID0[568][NS]?|MID[1-9]|MID[78]0[1-9]|MID970[1-9]|MID100[1-9]) Build/'
    device_replacement: '$1'
    brand_replacement: 'Manta'
    model_replacement: '$1'

  #########
  # Match
  # @ref: http://www.match.net.cn/products.asp
  #########
  - regex: '; *(M1052|M806|M9000|M9100|M9701|MID100|MID120|MID125|MID130|MID135|MID140|MID701|MID710|MID713|MID727|MID728|MID731|MID732|MID733|MID735|MID736|MID737|MID760|MID800|MID810|MID820|MID830|MID833|MID835|MID860|MID900|MID930|MID933|MID960|MID980) Build/'
    device_replacement: '$1'
    brand_replacement: 'Match'
    model_replacement: '$1'

  #########
  # Maxx
  # @ref: http://www.maxxmobile.in/
  # @models: Maxx MSD7-Play, Maxx MX245+ Trance, Maxx AX8 Race, Maxx MSD7 3G- AX50, Maxx Genx Droid 7 - AX40, Maxx AX5 Duo,
  #   Maxx AX3 Duo, Maxx AX3, Maxx AX8 Note II (Note 2), Maxx AX8 Note I, Maxx AX8, Maxx AX5 Plus, Maxx MSD7 Smarty,
  #   Maxx AX9Z Race,
  #   Maxx MT150, Maxx MQ601, Maxx M2020, Maxx Sleek MX463neo, Maxx MX525, Maxx MX192-Tune, Maxx Genx Droid 7 AX353,
  # @note: Need more User-Agents!!!
  #########
  - regex: '; *(GenxDroid7|MSD7.*|AX\d.*|Tab 701|Tab 722) Build/'
    device_replacement: 'Maxx $1'
    brand_replacement: 'Maxx'
    model_replacement: '$1'

  #########
  # Mediacom
  # @ref: http://www.mediacomeurope.it/
  #########
  - regex: '; *(M-PP[^;/]+|PhonePad ?\d{2,}[^;/]+) Build'
    device_replacement: 'Mediacom $1'
    brand_replacement: 'Mediacom'
    model_replacement: '$1'
  - regex: '; *(M-MP[^;/]+|SmartPad ?\d{2,}[^;/]+) Build'
    device_replacement: 'Mediacom $1'
    brand_replacement: 'Mediacom'
    model_replacement: '$1'

  #########
  # Medion
  # @ref: http://www.medion.com/en/
  #########
  - regex: '; *(?:MD_|)LIFETAB[ _]([^;/]+) Build'
    regex_flag: 'i'
    device_replacement: 'Medion Lifetab $1'
    brand_replacement: 'Medion'
    model_replacement: 'Lifetab $1'
  - regex: '; *MEDION ([^;/]+) Build'
    device_replacement: 'Medion $1'
    brand_replacement: 'Medion'
    model_replacement: '$1'

  #########
  # Meizu
  # @ref: http://www.meizu.com
  #########
  - regex: '; *(M030|M031|M035|M040|M065|m9) Build'
    device_replacement: 'Meizu $1'
    brand_replacement: 'Meizu'
    model_replacement: '$1'
  - regex: '; *(?:meizu_|MEIZU )(.+?) *(?:Build|[;\)])'
    device_replacement: 'Meizu $1'
    brand_replacement: 'Meizu'
    model_replacement: '$1'

  #########
  # Micromax
  # @ref: http://www.micromaxinfo.com
  #########
  - regex: '; *(?:Micromax[ _](A111|A240)|(A111|A240)) Build'
    regex_flag: 'i'
    device_replacement: 'Micromax $1$2'
    brand_replacement: 'Micromax'
    model_replacement: '$1$2'
  - regex: '; *Micromax[ _](A\d{2,3}[^;/]*) Build'
    regex_flag: 'i'
    device_replacement: 'Micromax $1'
    brand_replacement: 'Micromax'
    model_replacement: '$1'
  # be carefull here with Acer e.g. A500
  - regex: '; *(A\d{2}|A[12]\d{2}|A90S|A110Q) Build'
    regex_flag: 'i'
    device_replacement: 'Micromax $1'
    brand_replacement: 'Micromax'
    model_replacement: '$1'
  - regex: '; *Micromax[ _](P\d{3}[^;/]*) Build'
    regex_flag: 'i'
    device_replacement: 'Micromax $1'
    brand_replacement: 'Micromax'
    model_replacement: '$1'
  - regex: '; *(P\d{3}|P\d{3}\(Funbook\)) Build'
    regex_flag: 'i'
    device_replacement: 'Micromax $1'
    brand_replacement: 'Micromax'
    model_replacement: '$1'

  #########
  # Mito
  # @ref: http://new.mitomobile.com/
  #########
  - regex: '; *(MITO)[ _\-]?([^;/]+) Build'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'Mito'
    model_replacement: '$2'

  #########
  # Mobistel
  # @ref: http://www.mobistel.com/
  #########
  - regex: '; *(Cynus)[ _](F5|T\d|.+?) *(?:Build|[;/\)])'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'Mobistel'
    model_replacement: '$1 $2'

  #########
  # Modecom
  # @ref: http://www.modecom.eu/tablets/portal/
  #########
  - regex: '; *(MODECOM |)(FreeTab) ?([^;/]+) Build'
    regex_flag: 'i'
    device_replacement: '$1$2 $3'
    brand_replacement: 'Modecom'
    model_replacement: '$2 $3'
  - regex: '; *(MODECOM )([^;/]+) Build'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'Modecom'
    model_replacement: '$2'

  #########
  # Motorola
  # @ref: http://www.motorola.com/us/shop-all-mobile-phones/
  #########
  - regex: '; *(MZ\d{3}\+?|MZ\d{3} 4G|Xoom|XOOM[^;/]*) Build'
    device_replacement: 'Motorola $1'
    brand_replacement: 'Motorola'
    model_replacement: '$1'
  - regex: '; *(Milestone )(XT[^;/]*) Build'
    device_replacement: 'Motorola $1$2'
    brand_replacement: 'Motorola'
    model_replacement: '$2'
  - regex: '; *(Motoroi ?x|Droid X|DROIDX) Build'
    regex_flag: 'i'
    device_replacement: 'Motorola $1'
    brand_replacement: 'Motorola'
    model_replacement: 'DROID X'
  - regex: '; *(Droid[^;/]*|DROID[^;/]*|Milestone[^;/]*|Photon|Triumph|Devour|Titanium) Build'
    device_replacement: 'Motorola $1'
    brand_replacement: 'Motorola'
    model_replacement: '$1'
  - regex: '; *(A555|A85[34][^;/]*|A95[356]|ME[58]\d{2}\+?|ME600|ME632|ME722|MB\d{3}\+?|MT680|MT710|MT870|MT887|MT917|WX435|WX453|WX44[25]|XT\d{3,4}[A-Z\+]*|CL[iI]Q|CL[iI]Q XT) Build'
    device_replacement: '$1'
    brand_replacement: 'Motorola'
    model_replacement: '$1'
  - regex: '; *(Motorola MOT-|Motorola[ _\-]|MOT\-?)([^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Motorola'
    model_replacement: '$2'
  - regex: '; *(Moto[_ ]?|MOT\-)([^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Motorola'
    model_replacement: '$2'

  #########
  # MpMan
  # @ref: http://www.mpmaneurope.com
  #########
  - regex: '; *((?:MP[DQ]C|MPG\d{1,4}|MP\d{3,4}|MID(?:(?:10[234]|114|43|7[247]|8[24]|7)C|8[01]1))[^;/]*) Build'
    device_replacement: '$1'
    brand_replacement: 'Mpman'
    model_replacement: '$1'

  #########
  # MSI
  # @ref: http://www.msi.com/product/windpad/
  #########
  - regex: '; *(?:MSI[ _]|)(Primo\d+|Enjoy[ _\-][^;/]+) Build'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'Msi'
    model_replacement: '$1'

  #########
  # Multilaser
  # http://www.multilaser.com.br/listagem_produtos.php?cat=5
  #########
  - regex: '; *Multilaser[ _]([^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Multilaser'
    model_replacement: '$1'

  #########
  # MyPhone
  # @ref: http://myphone.com.ph/
  #########
  - regex: '; *(My)[_]?(Pad)[ _]([^;/]+) Build'
    device_replacement: '$1$2 $3'
    brand_replacement: 'MyPhone'
    model_replacement: '$1$2 $3'
  - regex: '; *(My)\|?(Phone)[ _]([^;/]+) Build'
    device_replacement: '$1$2 $3'
    brand_replacement: 'MyPhone'
    model_replacement: '$3'
  - regex: '; *(A\d+)[ _](Duo|) Build'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'MyPhone'
    model_replacement: '$1 $2'

  #########
  # Mytab
  # @ref: http://www.mytab.eu/en/category/mytab-products/
  #########
  - regex: '; *(myTab[^;/]*) Build'
    device_replacement: '$1'
    brand_replacement: 'Mytab'
    model_replacement: '$1'

  #########
  # Nabi
  # @ref: https://www.nabitablet.com
  #########
  - regex: '; *(NABI2?-)([^;/]+) Build/'
    device_replacement: '$1$2'
    brand_replacement: 'Nabi'
    model_replacement: '$2'

  #########
  # Nec Medias
  # @ref: http://www.n-keitai.com/
  #########
  - regex: '; *(N-\d+[CDE]) Build/'
    device_replacement: '$1'
    brand_replacement: 'Nec'
    model_replacement: '$1'
  - regex: '; ?(NEC-)(.*) Build/'
    device_replacement: '$1$2'
    brand_replacement: 'Nec'
    model_replacement: '$2'
  - regex: '; *(LT-NA7) Build/'
    device_replacement: '$1'
    brand_replacement: 'Nec'
    model_replacement: 'Lifetouch Note'

  #########
  # Nextbook
  # @ref: http://nextbookusa.com
  #########
  - regex: '; *(NXM\d+[A-Za-z0-9_]*|Next\d[A-Za-z0-9_ \-]*|NEXT\d[A-Za-z0-9_ \-]*|Nextbook [A-Za-z0-9_ ]*|DATAM803HC|M805)(?: Build|[\);])'
    device_replacement: '$1'
    brand_replacement: 'Nextbook'
    model_replacement: '$1'

  #########
  # Nokia
  # @ref: http://www.nokia.com
  #########
  - regex: '; *(Nokia)([ _\-]*)([^;/]*) Build'
    regex_flag: 'i'
    device_replacement: '$1$2$3'
    brand_replacement: 'Nokia'
    model_replacement: '$3'

  #########
  # Nook
  # @ref:
  # TODO nook browser/1.0
  #########
  - regex: '; *(Nook ?|Barnes & Noble Nook |BN )([^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Nook'
    model_replacement: '$2'
  - regex: '; *(NOOK |)(BNRV200|BNRV200A|BNTV250|BNTV250A|BNTV400|BNTV600|LogicPD Zoom2) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Nook'
    model_replacement: '$2'
  - regex: '; Build/(Nook)'
    device_replacement: '$1'
    brand_replacement: 'Nook'
    model_replacement: 'Tablet'

  #########
  # Olivetti
  # @ref: http://www.olivetti.de/EN/Page/t02/view_html?idp=348
  #########
  - regex: '; *(OP110|OliPad[^;/]+) Build'
    device_replacement: 'Olivetti $1'
    brand_replacement: 'Olivetti'
    model_replacement: '$1'

  #########
  # Omega
  # @ref: http://omega-technology.eu/en/produkty/346/tablets
  # @note: MID tablets might get matched by CobyKyros first
  # @models: (T107|MID(?:700[2-5]|7031|7108|7132|750[02]|8001|8500|9001|971[12])
  #########
  - regex: '; *OMEGA[ _\-](MID[^;/]+) Build'
    device_replacement: 'Omega $1'
    brand_replacement: 'Omega'
    model_replacement: '$1'
  - regex: '^(MID7500|MID\d+) Mozilla/5\.0 \(iPad;'
    device_replacement: 'Omega $1'
    brand_replacement: 'Omega'
    model_replacement: '$1'

  #########
  # OpenPeak
  # @ref: https://support.google.com/googleplay/answer/1727131?hl=en
  #########
  - regex: '; *((?:CIUS|cius)[^;/]*) Build'
    device_replacement: 'Openpeak $1'
    brand_replacement: 'Openpeak'
    model_replacement: '$1'

  #########
  # Oppo
  # @ref: http://en.oppo.com/products/
  #########
  - regex: '; *(Find ?(?:5|7a)|R8[012]\d{1,2}|T703\d{0,1}|U70\d{1,2}T?|X90\d{1,2}) Build'
    device_replacement: 'Oppo $1'
    brand_replacement: 'Oppo'
    model_replacement: '$1'
  - regex: '; *OPPO ?([^;/]+) Build/'
    device_replacement: 'Oppo $1'
    brand_replacement: 'Oppo'
    model_replacement: '$1'

  #########
  # Odys
  # @ref: http://odys.de
  #########
  - regex: '; *(?:Odys\-|ODYS\-|ODYS )([^;/]+) Build'
    device_replacement: 'Odys $1'
    brand_replacement: 'Odys'
    model_replacement: '$1'
  - regex: '; *(SELECT) ?(7) Build'
    device_replacement: 'Odys $1 $2'
    brand_replacement: 'Odys'
    model_replacement: '$1 $2'
  - regex: '; *(PEDI)_(PLUS)_(W) Build'
    device_replacement: 'Odys $1 $2 $3'
    brand_replacement: 'Odys'
    model_replacement: '$1 $2 $3'
  # Weltbild - Tablet PC 4 = Cat Phoenix = Odys Tablet PC 4?
  - regex: '; *(AEON|BRAVIO|FUSION|FUSION2IN1|Genio|EOS10|IEOS[^;/]*|IRON|Loox|LOOX|LOOX Plus|Motion|NOON|NOON_PRO|NEXT|OPOS|PEDI[^;/]*|PRIME[^;/]*|STUDYTAB|TABLO|Tablet-PC-4|UNO_X8|XELIO[^;/]*|Xelio ?\d+ ?[Pp]ro|XENO10|XPRESS PRO) Build'
    device_replacement: 'Odys $1'
    brand_replacement: 'Odys'
    model_replacement: '$1'

  #########
  # OnePlus
  # @ref https://oneplus.net/
  #########
  - regex: '; (ONE [a-zA-Z]\d+) Build/'
    device_replacement: 'OnePlus $1'
    brand_replacement: 'OnePlus'
    model_replacement: '$1'
  - regex: '; (ONEPLUS [a-zA-Z]\d+)(?: Build/|)'
    device_replacement: 'OnePlus $1'
    brand_replacement: 'OnePlus'
    model_replacement: '$1'

  #########
  # Orion
  # @ref: http://www.orion.ua/en/products/computer-products/tablet-pcs.html
  #########
  - regex: '; *(TP-\d+) Build/'
    device_replacement: 'Orion $1'
    brand_replacement: 'Orion'
    model_replacement: '$1'

  #########
  # PackardBell
  # @ref: http://www.packardbell.com/pb/en/AE/content/productgroup/tablets
  #########
  - regex: '; *(G100W?) Build/'
    device_replacement: 'PackardBell $1'
    brand_replacement: 'PackardBell'
    model_replacement: '$1'

  #########
  # Panasonic
  # @ref: http://panasonic.jp/mobile/
  # @models: T11, T21, T31, P11, P51, Eluga Power, Eluga DL1
  # @models: (tab) Toughpad FZ-A1, Toughpad JT-B1
  #########
  - regex: '; *(Panasonic)[_ ]([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'
  # Toughpad
  - regex: '; *(FZ-A1B|JT-B1) Build'
    device_replacement: 'Panasonic $1'
    brand_replacement: 'Panasonic'
    model_replacement: '$1'
  # Eluga Power
  - regex: '; *(dL1|DL1) Build'
    device_replacement: 'Panasonic $1'
    brand_replacement: 'Panasonic'
    model_replacement: '$1'

  #########
  # Pantech
  # @href: http://www.pantech.co.kr/en/prod/prodList.do?gbrand=PANTECH
  # @href: http://www.pantech.co.kr/en/prod/prodList.do?gbrand=VEGA
  # @models: ADR8995, ADR910L, ADR930VW, C790, CDM8992, CDM8999, IS06, IS11PT, P2000, P2020, P2030, P4100, P5000, P6010, P6020, P6030, P7000, P7040, P8000, P8010, P9020, P9050, P9060, P9070, P9090, PT001, PT002, PT003, TXT8040, TXT8045, VEGA PTL21
  #########
  - regex: '; *(SKY[ _]|)(IM\-[AT]\d{3}[^;/]+).* Build/'
    device_replacement: 'Pantech $1$2'
    brand_replacement: 'Pantech'
    model_replacement: '$1$2'
  - regex: '; *((?:ADR8995|ADR910L|ADR930L|ADR930VW|PTL21|P8000)(?: 4G|)) Build/'
    device_replacement: '$1'
    brand_replacement: 'Pantech'
    model_replacement: '$1'
  - regex: '; *Pantech([^;/]+).* Build/'
    device_replacement: 'Pantech $1'
    brand_replacement: 'Pantech'
    model_replacement: '$1'

  #########
  # Papayre
  # @ref: http://grammata.es/
  #########
  - regex: '; *(papyre)[ _\-]([^;/]+) Build/'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'Papyre'
    model_replacement: '$2'

  #########
  # Pearl
  # @ref: http://www.pearl.de/c-1540.shtml
  #########
  - regex: '; *(?:Touchlet )?(X10\.[^;/]+) Build/'
    device_replacement: 'Pearl $1'
    brand_replacement: 'Pearl'
    model_replacement: '$1'

  #########
  # Phicomm
  # @ref: http://www.phicomm.com.cn/
  #########
  - regex: '; PHICOMM (i800) Build/'
    device_replacement: 'Phicomm $1'
    brand_replacement: 'Phicomm'
    model_replacement: '$1'
  - regex: '; PHICOMM ([^;/]+) Build/'
    device_replacement: 'Phicomm $1'
    brand_replacement: 'Phicomm'
    model_replacement: '$1'
  - regex: '; *(FWS\d{3}[^;/]+) Build/'
    device_replacement: 'Phicomm $1'
    brand_replacement: 'Phicomm'
    model_replacement: '$1'

  #########
  # Philips
  # @ref: http://www.support.philips.com/support/catalog/products.jsp?_dyncharset=UTF-8&country=&categoryid=MOBILE_PHONES_SMART_SU_CN_CARE&userLanguage=en&navCount=2&groupId=PC_PRODUCTS_AND_PHONES_GR_CN_CARE&catalogType=&navAction=push&userCountry=cn&title=Smartphones&cateId=MOBILE_PHONES_CA_CN_CARE
  # @TODO: Philips Tablets User-Agents missing!
  # @ref: http://www.support.philips.com/support/catalog/products.jsp?_dyncharset=UTF-8&country=&categoryid=ENTERTAINMENT_TABLETS_SU_CN_CARE&userLanguage=en&navCount=0&groupId=&catalogType=&navAction=push&userCountry=cn&title=Entertainment+Tablets&cateId=TABLETS_CA_CN_CARE
  #########
  # @note: this a best guess according to available philips models. Need more User-Agents
  - regex: '; *(D633|D822|D833|T539|T939|V726|W335|W336|W337|W3568|W536|W5510|W626|W632|W6350|W6360|W6500|W732|W736|W737|W7376|W820|W832|W8355|W8500|W8510|W930) Build'
    device_replacement: '$1'
    brand_replacement: 'Philips'
    model_replacement: '$1'
  - regex: '; *(?:Philips|PHILIPS)[ _]([^;/]+) Build'
    device_replacement: 'Philips $1'
    brand_replacement: 'Philips'
    model_replacement: '$1'

  #########
  # Pipo
  # @ref: http://www.pipo.cn/En/
  #########
  - regex: 'Android 4\..*; *(M[12356789]|U[12368]|S[123])\ ?(pro)? Build'
    device_replacement: 'Pipo $1$2'
    brand_replacement: 'Pipo'
    model_replacement: '$1$2'

  #########
  # Ployer
  # @ref: http://en.ployer.cn/
  #########
  - regex: '; *(MOMO[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Ployer'
    model_replacement: '$1'

  #########
  # Polaroid/ Acho
  # @ref: http://polaroidstore.com/store/start.asp?category_id=382&category_id2=0&order=title&filter1=&filter2=&filter3=&view=all
  #########
  - regex: '; *(?:Polaroid[ _]|)((?:MIDC\d{3,}|PMID\d{2,}|PTAB\d{3,})[^;/]*)(\/[^;/]*|) Build/'
    device_replacement: '$1'
    brand_replacement: 'Polaroid'
    model_replacement: '$1'
  - regex: '; *(?:Polaroid )(Tablet) Build/'
    device_replacement: '$1'
    brand_replacement: 'Polaroid'
    model_replacement: '$1'

  #########
  # Pomp
  # @ref: http://pompmobileshop.com/
  #########
  #~ TODO
  - regex: '; *(POMP)[ _\-](.+?) *(?:Build|[;/\)])'
    device_replacement: '$1 $2'
    brand_replacement: 'Pomp'
    model_replacement: '$2'

  #########
  # Positivo
  # @ref: http://www.positivoinformatica.com.br/www/pessoal/tablet-ypy/
  #########
  - regex: '; *(TB07STA|TB10STA|TB07FTA|TB10FTA) Build/'
    device_replacement: '$1'
    brand_replacement: 'Positivo'
    model_replacement: '$1'
  - regex: '; *(?:Positivo |)((?:YPY|Ypy)[^;/]+) Build/'
    device_replacement: '$1'
    brand_replacement: 'Positivo'
    model_replacement: '$1'

  #########
  # POV
  # @ref: http://www.pointofview-online.com/default2.php
  # @TODO: Smartphone Models MOB-3515, MOB-5045-B missing
  #########
  - regex: '; *(MOB-[^;/]+) Build/'
    device_replacement: '$1'
    brand_replacement: 'POV'
    model_replacement: '$1'
  - regex: '; *POV[ _\-]([^;/]+) Build/'
    device_replacement: 'POV $1'
    brand_replacement: 'POV'
    model_replacement: '$1'
  - regex: '; *((?:TAB-PLAYTAB|TAB-PROTAB|PROTAB|PlayTabPro|Mobii[ _\-]|TAB-P)[^;/]*) Build/'
    device_replacement: 'POV $1'
    brand_replacement: 'POV'
    model_replacement: '$1'

  #########
  # Prestigio
  # @ref: http://www.prestigio.com/catalogue/MultiPhones
  # @ref: http://www.prestigio.com/catalogue/MultiPads
  #########
  - regex: '; *(?:Prestigio |)((?:PAP|PMP)\d[^;/]+) Build/'
    device_replacement: 'Prestigio $1'
    brand_replacement: 'Prestigio'
    model_replacement: '$1'

  #########
  # Proscan
  # @ref: http://www.proscanvideo.com/products-search.asp?itemClass=TABLET&itemnmbr=
  #########
  - regex: '; *(PLT[0-9]{4}.*) Build/'
    device_replacement: '$1'
    brand_replacement: 'Proscan'
    model_replacement: '$1'

  #########
  # QMobile
  # @ref: http://www.qmobile.com.pk/
  #########
  - regex: '; *(A2|A5|A8|A900)_?(Classic|) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Qmobile'
    model_replacement: '$1 $2'
  - regex: '; *(Q[Mm]obile)_([^_]+)_([^_]+) Build'
    device_replacement: 'Qmobile $2 $3'
    brand_replacement: 'Qmobile'
    model_replacement: '$2 $3'
  - regex: '; *(Q\-?[Mm]obile)[_ ](A[^;/]+) Build'
    device_replacement: 'Qmobile $2'
    brand_replacement: 'Qmobile'
    model_replacement: '$2'

  #########
  # Qmobilevn
  # @ref: http://qmobile.vn/san-pham.html
  #########
  - regex: '; *(Q\-Smart)[ _]([^;/]+) Build/'
    device_replacement: '$1 $2'
    brand_replacement: 'Qmobilevn'
    model_replacement: '$2'
  - regex: '; *(Q\-?[Mm]obile)[ _\-](S[^;/]+) Build/'
    device_replacement: '$1 $2'
    brand_replacement: 'Qmobilevn'
    model_replacement: '$2'

  #########
  # Quanta
  # @ref: ?
  #########
  - regex: '; *(TA1013) Build'
    device_replacement: '$1'
    brand_replacement: 'Quanta'
    model_replacement: '$1'

  #########
  # RCA
  # @ref: http://rcamobilephone.com/
  #########
  - regex: '; (RCT\w+) Build/'
    device_replacement: '$1'
    brand_replacement: 'RCA'
    model_replacement: '$1'
  - regex: '; RCA (\w+) Build/'
    device_replacement: 'RCA $1'
    brand_replacement: 'RCA'
    model_replacement: '$1'

  #########
  # Rockchip
  # @ref: http://www.rock-chips.com/a/cn/product/index.html
  # @note: manufacturer sells chipsets - I assume that these UAs are dev-boards
  #########
  - regex: '; *(RK\d+),? Build/'
    device_replacement: '$1'
    brand_replacement: 'Rockchip'
    model_replacement: '$1'
  - regex: ' Build/(RK\d+)'
    device_replacement: '$1'
    brand_replacement: 'Rockchip'
    model_replacement: '$1'

  #########
  # Samsung Android Devices
  # @ref: http://www.samsung.com/us/mobile/cell-phones/all-products
  #########
  - regex: '; *(SAMSUNG |Samsung |)((?:Galaxy (?:Note II|S\d)|GT-I9082|GT-I9205|GT-N7\d{3}|SM-N9005)[^;/]*)\/?[^;/]* Build/'
    device_replacement: 'Samsung $1$2'
    brand_replacement: 'Samsung'
    model_replacement: '$2'
  - regex: '; *(Google |)(Nexus [Ss](?: 4G|)) Build/'
    device_replacement: 'Samsung $1$2'
    brand_replacement: 'Samsung'
    model_replacement: '$2'
  - regex: '; *(SAMSUNG |Samsung )([^\/]*)\/[^ ]* Build/'
    device_replacement: 'Samsung $2'
    brand_replacement: 'Samsung'
    model_replacement: '$2'
  - regex: '; *(Galaxy(?: Ace| Nexus| S ?II+|Nexus S| with MCR 1.2| Mini Plus 4G|)) Build/'
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'
  - regex: '; *(SAMSUNG[ _\-]|)(?:SAMSUNG[ _\-])([^;/]+) Build'
    device_replacement: 'Samsung $2'
    brand_replacement: 'Samsung'
    model_replacement: '$2'
  - regex: '; *(SAMSUNG-|)(GT\-[BINPS]\d{4}[^\/]*)(\/[^ ]*) Build'
    device_replacement: 'Samsung $1$2$3'
    brand_replacement: 'Samsung'
    model_replacement: '$2'
  - regex: '(?:; *|^)((?:GT\-[BIiNPS]\d{4}|I9\d{2}0[A-Za-z\+]?\b)[^;/\)]*?)(?:Build|Linux|MIUI|[;/\)])'
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'
  - regex: '; (SAMSUNG-)([A-Za-z0-9\-]+).* Build/'
    device_replacement: 'Samsung $1$2'
    brand_replacement: 'Samsung'
    model_replacement: '$2'
  - regex: '; *((?:SCH|SGH|SHV|SHW|SPH|SC|SM)\-[A-Za-z0-9 ]+)(/?[^ ]*|) Build'
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'
  - regex: '; *((?:SC)\-[A-Za-z0-9 ]+)(/?[^ ]*|)\)'
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'
  - regex: ' ((?:SCH)\-[A-Za-z0-9 ]+)(/?[^ ]*|) Build'
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'
  - regex: '; *(Behold ?(?:2|II)|YP\-G[^;/]+|EK-GC100|SCL21|I9300) Build'
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'
  - regex: '; *((?:SCH|SGH|SHV|SHW|SPH|SC|SM)\-[A-Za-z0-9]{5,6})[\)]'
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'

  #########
  # Sharp
  # @ref: http://www.sharp-phone.com/en/index.html
  # @ref: http://www.android.com/devices/?country=all&m=sharp
  #########
  - regex: '; *(SH\-?\d\d[^;/]+|SBM\d[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Sharp'
    model_replacement: '$1'
  - regex: '; *(SHARP[ -])([^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'Sharp'
    model_replacement: '$2'

  #########
  # Simvalley
  # @ref: http://www.simvalley-mobile.de/
  #########
  - regex: '; *(SPX[_\-]\d[^;/]*) Build/'
    device_replacement: '$1'
    brand_replacement: 'Simvalley'
    model_replacement: '$1'
  - regex: '; *(SX7\-PEARL\.GmbH) Build/'
    device_replacement: '$1'
    brand_replacement: 'Simvalley'
    model_replacement: '$1'
  - regex: '; *(SP[T]?\-\d{2}[^;/]*) Build/'
    device_replacement: '$1'
    brand_replacement: 'Simvalley'
    model_replacement: '$1'

  #########
  # SK Telesys
  # @ref: http://www.sk-w.com/phone/phone_list.jsp
  # @ref: http://www.android.com/devices/?country=all&m=sk-telesys
  #########
  - regex: '; *(SK\-.*) Build/'
    device_replacement: '$1'
    brand_replacement: 'SKtelesys'
    model_replacement: '$1'

  #########
  # Skytex
  # @ref: http://skytex.com/android
  #########
  - regex: '; *(?:SKYTEX|SX)-([^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Skytex'
    model_replacement: '$1'
  - regex: '; *(IMAGINE [^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Skytex'
    model_replacement: '$1'

  #########
  # SmartQ
  # @ref: http://en.smartdevices.com.cn/Products/
  # @models: Z8, X7, U7H, U7, T30, T20, Ten3, V5-II, T7-3G, SmartQ5, K7, S7, Q8, T19, Ten2, Ten, R10, T7, R7, V5, V7, SmartQ7
  #########
  - regex: '; *(SmartQ) ?([^;/]+) Build/'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'

  #########
  # Smartbitt
  # @ref: http://www.smartbitt.com/
  # @missing: SBT Useragents
  #########
  - regex: '; *(WF7C|WF10C|SBT[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Smartbitt'
    model_replacement: '$1'

  #########
  # Softbank (Operator Branded Devices)
  # @ref: http://www.ipentec.com/document/document.aspx?page=android-useragent
  #########
  - regex: '; *(SBM(?:003SH|005SH|006SH|007SH|102SH)) Build'
    device_replacement: '$1'
    brand_replacement: 'Sharp'
    model_replacement: '$1'
  - regex: '; *(003P|101P|101P11C|102P) Build'
    device_replacement: '$1'
    brand_replacement: 'Panasonic'
    model_replacement: '$1'
  - regex: '; *(00\dZ) Build/'
    device_replacement: '$1'
    brand_replacement: 'ZTE'
    model_replacement: '$1'
  - regex: '; HTC(X06HT) Build'
    device_replacement: '$1'
    brand_replacement: 'HTC'
    model_replacement: '$1'
  - regex: '; *(001HT|X06HT) Build'
    device_replacement: '$1'
    brand_replacement: 'HTC'
    model_replacement: '$1'
  - regex: '; *(201M) Build'
    device_replacement: '$1'
    brand_replacement: 'Motorola'
    model_replacement: 'XT902'

  #########
  # Trekstor
  # @ref: http://www.trekstor.co.uk/surftabs-en.html
  # @note: Must come before SonyEricsson
  #########
  - regex: '; *(ST\d{4}.*)Build/ST'
    device_replacement: 'Trekstor $1'
    brand_replacement: 'Trekstor'
    model_replacement: '$1'
  - regex: '; *(ST\d{4}.*) Build/'
    device_replacement: 'Trekstor $1'
    brand_replacement: 'Trekstor'
    model_replacement: '$1'

  #########
  # SonyEricsson
  # @note: Must come before nokia since they also use symbian
  # @ref: http://www.android.com/devices/?country=all&m=sony-ericssons
  # @TODO: type!
  #########
  # android matchers
  - regex: '; *(Sony ?Ericsson ?)([^;/]+) Build'
    device_replacement: '$1$2'
    brand_replacement: 'SonyEricsson'
    model_replacement: '$2'
  - regex: '; *((?:SK|ST|E|X|LT|MK|MT|WT)\d{2}[a-z0-9]*(?:-o|)|R800i|U20i) Build'
    device_replacement: '$1'
    brand_replacement: 'SonyEricsson'
    model_replacement: '$1'
  # TODO X\d+ is wrong
  - regex: '; *(Xperia (?:A8|Arc|Acro|Active|Live with Walkman|Mini|Neo|Play|Pro|Ray|X\d+)[^;/]*) Build'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'SonyEricsson'
    model_replacement: '$1'

  #########
  # Sony
  # @ref: http://www.sonymobile.co.jp/index.html
  # @ref: http://www.sonymobile.com/global-en/products/phones/
  # @ref: http://www.sony.jp/tablet/
  #########
  - regex: '; Sony (Tablet[^;/]+) Build'
    device_replacement: 'Sony $1'
    brand_replacement: 'Sony'
    model_replacement: '$1'
  - regex: '; Sony ([^;/]+) Build'
    device_replacement: 'Sony $1'
    brand_replacement: 'Sony'
    model_replacement: '$1'
  - regex: '; *(Sony)([A-Za-z0-9\-]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'
  - regex: '; *(Xperia [^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Sony'
    model_replacement: '$1'
  - regex: '; *(C(?:1[0-9]|2[0-9]|53|55|6[0-9])[0-9]{2}|D[25]\d{3}|D6[56]\d{2}) Build'
    device_replacement: '$1'
    brand_replacement: 'Sony'
    model_replacement: '$1'
  - regex: '; *(SGP\d{3}|SGPT\d{2}) Build'
    device_replacement: '$1'
    brand_replacement: 'Sony'
    model_replacement: '$1'
  - regex: '; *(NW-Z1000Series) Build'
    device_replacement: '$1'
    brand_replacement: 'Sony'
    model_replacement: '$1'

  ##########
  # Sony PlayStation
  # @ref: http://playstation.com
  # The Vita spoofs the Kindle
  ##########
  - regex: 'PLAYSTATION 3'
    device_replacement: 'PlayStation 3'
    brand_replacement: 'Sony'
    model_replacement: 'PlayStation 3'
  - regex: '(PlayStation (?:Portable|Vita|\d+))'
    device_replacement: '$1'
    brand_replacement: 'Sony'
    model_replacement: '$1'

  #########
  # Spice
  # @ref: http://www.spicemobilephones.co.in/
  #########
  - regex: '; *((?:CSL_Spice|Spice|SPICE|CSL)[ _\-]?|)([Mm][Ii])([ _\-]|)(\d{3}[^;/]*) Build/'
    device_replacement: '$1$2$3$4'
    brand_replacement: 'Spice'
    model_replacement: 'Mi$4'

  #########
  # Sprint (Operator Branded Devices)
  # @ref:
  #########
  - regex: '; *(Sprint )(.+?) *(?:Build|[;/])'
    device_replacement: '$1$2'
    brand_replacement: 'Sprint'
    model_replacement: '$2'
  - regex: '\b(Sprint)[: ]([^;,/ ]+)'
    device_replacement: '$1$2'
    brand_replacement: 'Sprint'
    model_replacement: '$2'

  #########
  # Tagi
  # @ref: ??
  #########
  - regex: '; *(TAGI[ ]?)(MID) ?([^;/]+) Build/'
    device_replacement: '$1$2$3'
    brand_replacement: 'Tagi'
    model_replacement: '$2$3'

  #########
  # Tecmobile
  # @ref: http://www.tecmobile.com/
  #########
  - regex: '; *(Oyster500|Opal 800) Build'
    device_replacement: 'Tecmobile $1'
    brand_replacement: 'Tecmobile'
    model_replacement: '$1'

  #########
  # Tecno
  # @ref: www.tecno-mobile.com/‎
  #########
  - regex: '; *(TECNO[ _])([^;/]+) Build/'
    device_replacement: '$1$2'
    brand_replacement: 'Tecno'
    model_replacement: '$2'

  #########
  # Telechips, Techvision evaluation boards
  # @ref:
  #########
  - regex: '; *Android for (Telechips|Techvision) ([^ ]+) '
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'

  #########
  # Telstra
  # @ref: http://www.telstra.com.au/home-phone/thub-2/
  # @ref: https://support.google.com/googleplay/answer/1727131?hl=en
  #########
  - regex: '; *(T-Hub2) Build/'
    device_replacement: '$1'
    brand_replacement: 'Telstra'
    model_replacement: '$1'

  #########
  # Terra
  # @ref: http://www.wortmann.de/
  #########
  - regex: '; *(PAD) ?(100[12]) Build/'
    device_replacement: 'Terra $1$2'
    brand_replacement: 'Terra'
    model_replacement: '$1$2'

  #########
  # Texet
  # @ref: http://www.texet.ru/tablet/
  #########
  - regex: '; *(T[BM]-\d{3}[^;/]+) Build/'
    device_replacement: '$1'
    brand_replacement: 'Texet'
    model_replacement: '$1'

  #########
  # Thalia
  # @ref: http://www.thalia.de/shop/tolino-shine-ereader/show/
  #########
  - regex: '; *(tolino [^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Thalia'
    model_replacement: '$1'
  - regex: '; *Build/.* (TOLINO_BROWSER)'
    device_replacement: '$1'
    brand_replacement: 'Thalia'
    model_replacement: 'Tolino Shine'

  #########
  # Thl
  # @ref: http://en.thl.com.cn/Mobile
  # @ref: http://thlmobilestore.com
  #########
  - regex: '; *(?:CJ[ -])?(ThL|THL)[ -]([^;/]+) Build/'
    device_replacement: '$1 $2'
    brand_replacement: 'Thl'
    model_replacement: '$2'
  - regex: '; *(T100|T200|T5|W100|W200|W8s) Build/'
    device_replacement: '$1'
    brand_replacement: 'Thl'
    model_replacement: '$1'

  #########
  # T-Mobile (Operator Branded Devices)
  #########
  # @ref: https://en.wikipedia.org/wiki/HTC_Hero
  - regex: '; *(T-Mobile[ _]G2[ _]Touch) Build'
    device_replacement: '$1'
    brand_replacement: 'HTC'
    model_replacement: 'Hero'
  # @ref: https://en.wikipedia.org/wiki/HTC_Desire_Z
  - regex: '; *(T-Mobile[ _]G2) Build'
    device_replacement: '$1'
    brand_replacement: 'HTC'
    model_replacement: 'Desire Z'
  - regex: '; *(T-Mobile myTouch Q) Build'
    device_replacement: '$1'
    brand_replacement: 'Huawei'
    model_replacement: 'U8730'
  - regex: '; *(T-Mobile myTouch) Build'
    device_replacement: '$1'
    brand_replacement: 'Huawei'
    model_replacement: 'U8680'
  - regex: '; *(T-Mobile_Espresso) Build'
    device_replacement: '$1'
    brand_replacement: 'HTC'
    model_replacement: 'Espresso'
  - regex: '; *(T-Mobile G1) Build'
    device_replacement: '$1'
    brand_replacement: 'HTC'
    model_replacement: 'Dream'
  - regex: '\b(T-Mobile ?|)(myTouch)[ _]?([34]G)[ _]?([^\/]*) (?:Mozilla|Build)'
    device_replacement: '$1$2 $3 $4'
    brand_replacement: 'HTC'
    model_replacement: '$2 $3 $4'
  - regex: '\b(T-Mobile)_([^_]+)_(.*) Build'
    device_replacement: '$1 $2 $3'
    brand_replacement: 'Tmobile'
    model_replacement: '$2 $3'
  - regex: '\b(T-Mobile)[_ ]?(.*?)Build'
    device_replacement: '$1 $2'
    brand_replacement: 'Tmobile'
    model_replacement: '$2'

  #########
  # Tomtec
  # @ref: http://www.tom-tec.eu/pages/tablets.php
  #########
  - regex: ' (ATP[0-9]{4}) Build'
    device_replacement: '$1'
    brand_replacement: 'Tomtec'
    model_replacement: '$1'

  #########
  # Tooky
  # @ref: http://www.tookymobile.com/
  #########
  - regex: ' *(TOOKY)[ _\-]([^;/]+) ?(?:Build|;)'
    regex_flag: 'i'
    device_replacement: '$1 $2'
    brand_replacement: 'Tooky'
    model_replacement: '$2'

  #########
  # Toshiba
  # @ref: http://www.toshiba.co.jp/
  # @missing: LT170, Thrive 7, TOSHIBA STB10
  #########
  - regex: '\b(TOSHIBA_AC_AND_AZ|TOSHIBA_FOLIO_AND_A|FOLIO_AND_A)'
    device_replacement: '$1'
    brand_replacement: 'Toshiba'
    model_replacement: 'Folio 100'
  - regex: '; *([Ff]olio ?100) Build/'
    device_replacement: '$1'
    brand_replacement: 'Toshiba'
    model_replacement: 'Folio 100'
  - regex: '; *(AT[0-9]{2,3}(?:\-A|LE\-A|PE\-A|SE|a|)|AT7-A|AT1S0|Hikari-iFrame/WDPF-[^;/]+|THRiVE|Thrive) Build/'
    device_replacement: 'Toshiba $1'
    brand_replacement: 'Toshiba'
    model_replacement: '$1'

  #########
  # Touchmate
  # @ref: http://touchmatepc.com/new/
  #########
  - regex: '; *(TM-MID\d+[^;/]+|TOUCHMATE|MID-750) Build'
    device_replacement: '$1'
    brand_replacement: 'Touchmate'
    model_replacement: '$1'
  # @todo: needs verification user-agents missing
  - regex: '; *(TM-SM\d+[^;/]+) Build'
    device_replacement: '$1'
    brand_replacement: 'Touchmate'
    model_replacement: '$1'

  #########
  # Treq
  # @ref: http://www.treq.co.id/product
  #########
  - regex: '; *(A10 [Bb]asic2?) Build/'
    device_replacement: '$1'
    brand_replacement: 'Treq'
    model_replacement: '$1'
  - regex: '; *(TREQ[ _\-])([^;/]+) Build'
    regex_flag: 'i'
    device_replacement: '$1$2'
    brand_replacement: 'Treq'
    model_replacement: '$2'

  #########
  # Umeox
  # @ref: http://umeox.com/
  # @models: A936|A603|X-5|X-3
  #########
  # @todo: guessed markers
  - regex: '; *(X-?5|X-?3) Build/'
    device_replacement: '$1'
    brand_replacement: 'Umeox'
    model_replacement: '$1'
  # @todo: guessed markers
  - regex: '; *(A502\+?|A936|A603|X1|X2) Build/'
    device_replacement: '$1'
    brand_replacement: 'Umeox'
    model_replacement: '$1'

  #########
  # Versus
  # @ref: http://versusuk.com/support.html
  #########
  - regex: '(TOUCH(?:TAB|PAD).+?) Build/'
    regex_flag: 'i'
    device_replacement: 'Versus $1'
    brand_replacement: 'Versus'
    model_replacement: '$1'

  #########
  # Vertu
  # @ref: http://www.vertu.com/
  #########
  - regex: '(VERTU) ([^;/]+) Build/'
    device_replacement: '$1 $2'
    brand_replacement: 'Vertu'
    model_replacement: '$2'

  #########
  # Videocon
  # @ref: http://www.videoconmobiles.com
  #########
  - regex: '; *(Videocon)[ _\-]([^;/]+) *(?:Build|;)'
    device_replacement: '$1 $2'
    brand_replacement: 'Videocon'
    model_replacement: '$2'
  - regex: ' (VT\d{2}[A-Za-z]*) Build'
    device_replacement: '$1'
    brand_replacement: 'Videocon'
    model_replacement: '$1'

  #########
  # Viewsonic
  # @ref: http://viewsonic.com
  #########
  - regex: '; *((?:ViewPad|ViewPhone|VSD)[^;/]+) Build/'
    device_replacement: '$1'
    brand_replacement: 'Viewsonic'
    model_replacement: '$1'
  - regex: '; *(ViewSonic-)([^;/]+) Build/'
    device_replacement: '$1$2'
    brand_replacement: 'Viewsonic'
    model_replacement: '$2'
  - regex: '; *(GTablet.*) Build/'
    device_replacement: '$1'
    brand_replacement: 'Viewsonic'
    model_replacement: '$1'

  #########
  # vivo
  # @ref: http://vivo.cn/
  #########
  - regex: '; *([Vv]ivo)[ _]([^;/]+) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'vivo'
    model_replacement: '$2'

  #########
  # Vodafone (Operator Branded Devices)
  # @ref: ??
  #########
  - regex: '(Vodafone) (.*) Build/'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'

  #########
  # Walton
  # @ref: http://www.waltonbd.com/
  #########
  - regex: '; *(?:Walton[ _\-]|)(Primo[ _\-][^;/]+) Build'
    regex_flag: 'i'
    device_replacement: 'Walton $1'
    brand_replacement: 'Walton'
    model_replacement: '$1'

  #########
  # Wiko
  # @ref: http://fr.wikomobile.com/collection.php?s=Smartphones
  #########
  - regex: '; *(?:WIKO[ \-]|)(CINK\+?|BARRY|BLOOM|DARKFULL|DARKMOON|DARKNIGHT|DARKSIDE|FIZZ|HIGHWAY|IGGY|OZZY|RAINBOW|STAIRWAY|SUBLIM|WAX|CINK [^;/]+) Build/'
    regex_flag: 'i'
    device_replacement: 'Wiko $1'
    brand_replacement: 'Wiko'
    model_replacement: '$1'

  #########
  # WellcoM
  # @ref: ??
  #########
  - regex: '; *WellcoM-([^;/]+) Build'
    device_replacement: 'Wellcom $1'
    brand_replacement: 'Wellcom'
    model_replacement: '$1'

  ##########
  # WeTab
  # @ref: http://wetab.mobi/
  ##########
  - regex: '(?:(WeTab)-Browser|; (wetab) Build)'
    device_replacement: '$1'
    brand_replacement: 'WeTab'
    model_replacement: 'WeTab'

  #########
  # Wolfgang
  # @ref: http://wolfgangmobile.com/
  #########
  - regex: '; *(AT-AS[^;/]+) Build'
    device_replacement: 'Wolfgang $1'
    brand_replacement: 'Wolfgang'
    model_replacement: '$1'

  #########
  # Woxter
  # @ref: http://www.woxter.es/es-es/categories/index
  #########
  - regex: '; *(?:Woxter|Wxt) ([^;/]+) Build'
    device_replacement: 'Woxter $1'
    brand_replacement: 'Woxter'
    model_replacement: '$1'

  #########
  # Yarvik Zania
  # @ref: http://yarvik.com
  #########
  - regex: '; *(?:Xenta |Luna |)(TAB[234][0-9]{2}|TAB0[78]-\d{3}|TAB0?9-\d{3}|TAB1[03]-\d{3}|SMP\d{2}-\d{3}) Build/'
    device_replacement: 'Yarvik $1'
    brand_replacement: 'Yarvik'
    model_replacement: '$1'

  #########
  # Yifang
  # @note: Needs to be at the very last as manufacturer builds for other brands.
  # @ref: http://www.yifangdigital.com/
  # @models: M1010, M1011, M1007, M1008, M1005, M899, M899LP, M909, M8000,
  #   M8001, M8002, M8003, M849, M815, M816, M819, M805, M878, M780LPW,
  #   M778, M7000, M7000AD, M7000NBD, M7001, M7002, M7002KBD, M777, M767,
  #   M789, M799, M769, M757, M755, M753, M752, M739, M729, M723, M712, M727
  #########
  - regex: '; *([A-Z]{2,4})(M\d{3,}[A-Z]{2})([^;\)\/]*)(?: Build|[;\)])'
    device_replacement: 'Yifang $1$2$3'
    brand_replacement: 'Yifang'
    model_replacement: '$2'

  #########
  # XiaoMi
  # @ref: http://www.xiaomi.com/event/buyphone
  #########
  - regex: '; *((Mi|MI|HM|MI-ONE|Redmi)[ -](NOTE |Note |)[^;/]*) (Build|MIUI)/'
    device_replacement: 'XiaoMi $1'
    brand_replacement: 'XiaoMi'
    model_replacement: '$1'
  - regex: '; *((Mi|MI|HM|MI-ONE|Redmi)[ -](NOTE |Note |)[^;/\)]*)'
    device_replacement: 'XiaoMi $1'
    brand_replacement: 'XiaoMi'
    model_replacement: '$1'
  - regex: '; *(MIX) (Build|MIUI)/'
    device_replacement: 'XiaoMi $1'
    brand_replacement: 'XiaoMi'
    model_replacement: '$1'
  - regex: '; *((MIX) ([^;/]*)) (Build|MIUI)/'
    device_replacement: 'XiaoMi $1'
    brand_replacement: 'XiaoMi'
    model_replacement: '$1'

  #########
  # Xolo
  # @ref: http://www.xolo.in/
  #########
  - regex: '; *XOLO[ _]([^;/]*tab.*) Build'
    regex_flag: 'i'
    device_replacement: 'Xolo $1'
    brand_replacement: 'Xolo'
    model_replacement: '$1'
  - regex: '; *XOLO[ _]([^;/]+) Build'
    regex_flag: 'i'
    device_replacement: 'Xolo $1'
    brand_replacement: 'Xolo'
    model_replacement: '$1'
  - regex: '; *(q\d0{2,3}[a-z]?) Build'
    regex_flag: 'i'
    device_replacement: 'Xolo $1'
    brand_replacement: 'Xolo'
    model_replacement: '$1'

  #########
  # Xoro
  # @ref: http://www.xoro.de/produkte/
  #########
  - regex: '; *(PAD ?[79]\d+[^;/]*|TelePAD\d+[^;/]) Build'
    device_replacement: 'Xoro $1'
    brand_replacement: 'Xoro'
    model_replacement: '$1'

  #########
  # Zopo
  # @ref: http://www.zopomobiles.com/products.html
  #########
  - regex: '; *(?:(?:ZOPO|Zopo)[ _]([^;/]+)|(ZP ?(?:\d{2}[^;/]+|C2))|(C[2379])) Build'
    device_replacement: '$1$2$3'
    brand_replacement: 'Zopo'
    model_replacement: '$1$2$3'

  #########
  # ZiiLabs
  # @ref: http://www.ziilabs.com/products/platforms/androidreferencetablets.php
  #########
  - regex: '; *(ZiiLABS) (Zii[^;/]*) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'ZiiLabs'
    model_replacement: '$2'
  - regex: '; *(Zii)_([^;/]*) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'ZiiLabs'
    model_replacement: '$2'

  #########
  # ZTE
  # @ref: http://www.ztedevices.com/
  #########
  - regex: '; *(ARIZONA|(?:ATLAS|Atlas) W|D930|Grand (?:[SX][^;]*|Era|Memo[^;]*)|JOE|(?:Kis|KIS)\b[^;]*|Libra|Light [^;]*|N8[056][01]|N850L|N8000|N9[15]\d{2}|N9810|NX501|Optik|(?:Vip )Racer[^;]*|RacerII|RACERII|San Francisco[^;]*|V9[AC]|V55|V881|Z[679][0-9]{2}[A-z]?) Build'
    device_replacement: '$1'
    brand_replacement: 'ZTE'
    model_replacement: '$1'
  - regex: '; *([A-Z]\d+)_USA_[^;]* Build'
    device_replacement: '$1'
    brand_replacement: 'ZTE'
    model_replacement: '$1'
  - regex: '; *(SmartTab\d+)[^;]* Build'
    device_replacement: '$1'
    brand_replacement: 'ZTE'
    model_replacement: '$1'
  - regex: '; *(?:Blade|BLADE|ZTE-BLADE)([^;/]*) Build'
    device_replacement: 'ZTE Blade$1'
    brand_replacement: 'ZTE'
    model_replacement: 'Blade$1'
  - regex: '; *(?:Skate|SKATE|ZTE-SKATE)([^;/]*) Build'
    device_replacement: 'ZTE Skate$1'
    brand_replacement: 'ZTE'
    model_replacement: 'Skate$1'
  - regex: '; *(Orange |Optimus )(Monte Carlo|San Francisco) Build'
    device_replacement: '$1$2'
    brand_replacement: 'ZTE'
    model_replacement: '$1$2'
  - regex: '; *(?:ZXY-ZTE_|ZTE\-U |ZTE[\- _]|ZTE-C[_ ])([^;/]+) Build'
    device_replacement: 'ZTE $1'
    brand_replacement: 'ZTE'
    model_replacement: '$1'
  # operator specific
  - regex: '; (BASE) (lutea|Lutea 2|Tab[^;]*) Build'
    device_replacement: '$1 $2'
    brand_replacement: 'ZTE'
    model_replacement: '$1 $2'
  - regex: '; (Avea inTouch 2|soft stone|tmn smart a7|Movistar[ _]Link) Build'
    regex_flag: 'i'
    device_replacement: '$1'
    brand_replacement: 'ZTE'
    model_replacement: '$1'
  - regex: '; *(vp9plus)\)'
    device_replacement: '$1'
    brand_replacement: 'ZTE'
    model_replacement: '$1'

  ##########
  # Zync
  # @ref: http://www.zync.in/index.php/our-products/tablet-phablets
  ##########
  - regex: '; ?(Cloud[ _]Z5|z1000|Z99 2G|z99|z930|z999|z990|z909|Z919|z900) Build/'
    device_replacement: '$1'
    brand_replacement: 'Zync'
    model_replacement: '$1'

  ##########
  # Kindle
  # @note: Needs to be after Sony Playstation Vita as this UA contains Silk/3.2
  # @ref: https://developer.amazon.com/sdk/fire/specifications.html
  # @ref: http://amazonsilk.wordpress.com/useful-bits/silk-user-agent/
  ##########
  - regex: '; ?(KFOT|Kindle Fire) Build\b'
    device_replacement: 'Kindle Fire'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle Fire'
  - regex: '; ?(KFOTE|Amazon Kindle Fire2) Build\b'
    device_replacement: 'Kindle Fire 2'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle Fire 2'
  - regex: '; ?(KFTT) Build\b'
    device_replacement: 'Kindle Fire HD'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle Fire HD 7""'
  - regex: '; ?(KFJWI) Build\b'
    device_replacement: 'Kindle Fire HD 8.9"" WiFi'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle Fire HD 8.9"" WiFi'
  - regex: '; ?(KFJWA) Build\b'
    device_replacement: 'Kindle Fire HD 8.9"" 4G'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle Fire HD 8.9"" 4G'
  - regex: '; ?(KFSOWI) Build\b'
    device_replacement: 'Kindle Fire HD 7"" WiFi'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle Fire HD 7"" WiFi'
  - regex: '; ?(KFTHWI) Build\b'
    device_replacement: 'Kindle Fire HDX 7"" WiFi'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle Fire HDX 7"" WiFi'
  - regex: '; ?(KFTHWA) Build\b'
    device_replacement: 'Kindle Fire HDX 7"" 4G'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle Fire HDX 7"" 4G'
  - regex: '; ?(KFAPWI) Build\b'
    device_replacement: 'Kindle Fire HDX 8.9"" WiFi'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle Fire HDX 8.9"" WiFi'
  - regex: '; ?(KFAPWA) Build\b'
    device_replacement: 'Kindle Fire HDX 8.9"" 4G'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle Fire HDX 8.9"" 4G'
  - regex: '; ?Amazon ([^;/]+) Build\b'
    device_replacement: '$1'
    brand_replacement: 'Amazon'
    model_replacement: '$1'
  - regex: '; ?(Kindle) Build\b'
    device_replacement: 'Kindle'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle'
  - regex: '; ?(Silk)/(\d+)\.(\d+)(?:\.([0-9\-]+)|) Build\b'
    device_replacement: 'Kindle Fire'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle Fire$2'
  - regex: ' (Kindle)/(\d+\.\d+)'
    device_replacement: 'Kindle'
    brand_replacement: 'Amazon'
    model_replacement: '$1 $2'
  - regex: ' (Silk|Kindle)/(\d+)\.'
    device_replacement: 'Kindle'
    brand_replacement: 'Amazon'
    model_replacement: 'Kindle'

  #########
  # Devices from chinese manufacturer(s)
  # @note: identified by x-wap-profile http://218.249.47.94/Xianghe/.*
  #########
  - regex: '(sprd)\-([^/]+)/'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'
  # @ref: http://eshinechina.en.alibaba.com/
  - regex: '; *(H\d{2}00\+?) Build'
    device_replacement: '$1'
    brand_replacement: 'Hero'
    model_replacement: '$1'
  - regex: '; *(iphone|iPhone5) Build/'
    device_replacement: 'Xianghe $1'
    brand_replacement: 'Xianghe'
    model_replacement: '$1'
  - regex: '; *(e\d{4}[a-z]?_?v\d+|v89_[^;/]+)[^;/]+ Build/'
    device_replacement: 'Xianghe $1'
    brand_replacement: 'Xianghe'
    model_replacement: '$1'

  #########
  # Cellular
  # @ref:
  # @note: Operator branded devices
  #########
  - regex: '\bUSCC[_\-]?([^ ;/\)]+)'
    device_replacement: '$1'
    brand_replacement: 'Cellular'
    model_replacement: '$1'

  ######################################################################
  # Windows Phone Parsers
  ######################################################################

  #########
  # Alcatel Windows Phones
  #########
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|)(?:ALCATEL)[^;]*; *([^;,\)]+)'
    device_replacement: 'Alcatel $1'
    brand_replacement: 'Alcatel'
    model_replacement: '$1'

  #########
  # Asus Windows Phones
  #########
  #~ - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|WpsLondonTest; ?|)(?:ASUS|Asus)[^;]*; *([^;,\)]+)'
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|WpsLondonTest; ?|)(?:ASUS|Asus)[^;]*; *([^;,\)]+)'
    device_replacement: 'Asus $1'
    brand_replacement: 'Asus'
    model_replacement: '$1'

  #########
  # Dell Windows Phones
  #########
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|)(?:DELL|Dell)[^;]*; *([^;,\)]+)'
    device_replacement: 'Dell $1'
    brand_replacement: 'Dell'
    model_replacement: '$1'

  #########
  # HTC Windows Phones
  #########
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|WpsLondonTest; ?|)(?:HTC|Htc|HTC_blocked[^;]*)[^;]*; *(?:HTC|)([^;,\)]+)'
    device_replacement: 'HTC $1'
    brand_replacement: 'HTC'
    model_replacement: '$1'

  #########
  # Huawei Windows Phones
  #########
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|)(?:HUAWEI)[^;]*; *(?:HUAWEI |)([^;,\)]+)'
    device_replacement: 'Huawei $1'
    brand_replacement: 'Huawei'
    model_replacement: '$1'

  #########
  # LG Windows Phones
  #########
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|)(?:LG|Lg)[^;]*; *(?:LG[ \-]|)([^;,\)]+)'
    device_replacement: 'LG $1'
    brand_replacement: 'LG'
    model_replacement: '$1'

  #########
  # Noka Windows Phones
  #########
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|)(?:rv:11; |)(?:NOKIA|Nokia)[^;]*; *(?:NOKIA ?|Nokia ?|LUMIA ?|[Ll]umia ?|)(\d{3,10}[^;\)]*)'
    device_replacement: 'Lumia $1'
    brand_replacement: 'Nokia'
    model_replacement: 'Lumia $1'
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|)(?:NOKIA|Nokia)[^;]*; *(RM-\d{3,})'
    device_replacement: 'Nokia $1'
    brand_replacement: 'Nokia'
    model_replacement: '$1'
  - regex: '(?:Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)]|WPDesktop;) ?(?:ARM; ?Touch; ?|Touch; ?|)(?:NOKIA|Nokia)[^;]*; *(?:NOKIA ?|Nokia ?|LUMIA ?|[Ll]umia ?|)([^;\)]+)'
    device_replacement: 'Nokia $1'
    brand_replacement: 'Nokia'
    model_replacement: '$1'

  #########
  # Microsoft Windows Phones
  #########
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|)(?:Microsoft(?: Corporation|))[^;]*; *([^;,\)]+)'
    device_replacement: 'Microsoft $1'
    brand_replacement: 'Microsoft'
    model_replacement: '$1'

  #########
  # Samsung Windows Phones
  #########
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|WpsLondonTest; ?|)(?:SAMSUNG)[^;]*; *(?:SAMSUNG |)([^;,\.\)]+)'
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'

  #########
  # Toshiba Windows Phones
  #########
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|WpsLondonTest; ?|)(?:TOSHIBA|FujitsuToshibaMobileCommun)[^;]*; *([^;,\)]+)'
    device_replacement: 'Toshiba $1'
    brand_replacement: 'Toshiba'
    model_replacement: '$1'

  #########
  # Generic Windows Phones
  #########
  - regex: 'Windows Phone [^;]+; .*?IEMobile/[^;\)]+[;\)] ?(?:ARM; ?Touch; ?|Touch; ?|WpsLondonTest; ?|)([^;]+); *([^;,\)]+)'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'

  ######################################################################
  # Other Devices Parser
  ######################################################################

  #########
  # Samsung Bada Phones
  #########
  - regex: '(?:^|; )SAMSUNG\-([A-Za-z0-9\-]+).* Bada/'
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'

  #########
  # Firefox OS
  #########
  - regex: '\(Mobile; ALCATEL ?(One|ONE) ?(Touch|TOUCH) ?([^;/]+)(?:/[^;]+|); rv:[^\)]+\) Gecko/[^\/]+ Firefox/'
    device_replacement: 'Alcatel $1 $2 $3'
    brand_replacement: 'Alcatel'
    model_replacement: 'One Touch $3'
  - regex: '\(Mobile; (?:ZTE([^;]+)|(OpenC)); rv:[^\)]+\) Gecko/[^\/]+ Firefox/'
    device_replacement: 'ZTE $1$2'
    brand_replacement: 'ZTE'
    model_replacement: '$1$2'

  #########
  # KaiOS
  #########
  - regex: '\(Mobile; ALCATEL([A-Za-z0-9\-]+); rv:[^\)]+\) Gecko/[^\/]+ Firefox/[^\/]+ KaiOS/'
    device_replacement: 'Alcatel $1'
    brand_replacement: 'Alcatel'
    model_replacement: '$1'
  - regex: '\(Mobile; LYF\/([A-Za-z0-9\-]+)\/.+;.+rv:[^\)]+\) Gecko/[^\/]+ Firefox/[^\/]+ KAIOS/'
    device_replacement: 'LYF $1'
    brand_replacement: 'LYF'
    model_replacement: '$1'
  - regex: '\(Mobile; Nokia_([A-Za-z0-9\-]+)_.+; rv:[^\)]+\) Gecko/[^\/]+ Firefox/[^\/]+ KAIOS/'
    device_replacement: 'Nokia $1'
    brand_replacement: 'Nokia'
    model_replacement: '$1'

  ##########
  # NOKIA
  # @note: NokiaN8-00 comes before iphone. Sometimes spoofs iphone
  ##########
  - regex: 'Nokia(N[0-9]+)([A-Za-z_\-][A-Za-z0-9_\-]*)'
    device_replacement: 'Nokia $1'
    brand_replacement: 'Nokia'
    model_replacement: '$1$2'
  - regex: '(?:NOKIA|Nokia)(?:\-| *)(?:([A-Za-z0-9]+)\-[0-9a-f]{32}|([A-Za-z0-9\-]+)(?:UCBrowser)|([A-Za-z0-9\-]+))'
    device_replacement: 'Nokia $1$2$3'
    brand_replacement: 'Nokia'
    model_replacement: '$1$2$3'
  - regex: 'Lumia ([A-Za-z0-9\-]+)'
    device_replacement: 'Lumia $1'
    brand_replacement: 'Nokia'
    model_replacement: 'Lumia $1'
  # UCWEB Browser on Symbian
  - regex: '\(Symbian; U; S60 V5; [A-z]{2}\-[A-z]{2}; (SonyEricsson|Samsung|Nokia|LG)([^;/]+)\)'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'
  # Nokia Symbian
  - regex: '\(Symbian(?:/3|); U; ([^;]+);'
    device_replacement: 'Nokia $1'
    brand_replacement: 'Nokia'
    model_replacement: '$1'

  ##########
  # BlackBerry
  # @ref: http://www.useragentstring.com/pages/BlackBerry/
  ##########
  - regex: 'BB10; ([A-Za-z0-9\- ]+)\)'
    device_replacement: 'BlackBerry $1'
    brand_replacement: 'BlackBerry'
    model_replacement: '$1'
  - regex: 'Play[Bb]ook.+RIM Tablet OS'
    device_replacement: 'BlackBerry Playbook'
    brand_replacement: 'BlackBerry'
    model_replacement: 'Playbook'
  - regex: 'Black[Bb]erry ([0-9]+);'
    device_replacement: 'BlackBerry $1'
    brand_replacement: 'BlackBerry'
    model_replacement: '$1'
  - regex: 'Black[Bb]erry([0-9]+)'
    device_replacement: 'BlackBerry $1'
    brand_replacement: 'BlackBerry'
    model_replacement: '$1'
  - regex: 'Black[Bb]erry;'
    device_replacement: 'BlackBerry'
    brand_replacement: 'BlackBerry'

  ##########
  # PALM / HP
  # @note: some palm devices must come before iphone. sometimes spoofs iphone in ua
  ##########
  - regex: '(Pre|Pixi)/\d+\.\d+'
    device_replacement: 'Palm $1'
    brand_replacement: 'Palm'
    model_replacement: '$1'
  - regex: 'Palm([0-9]+)'
    device_replacement: 'Palm $1'
    brand_replacement: 'Palm'
    model_replacement: '$1'
  - regex: 'Treo([A-Za-z0-9]+)'
    device_replacement: 'Palm Treo $1'
    brand_replacement: 'Palm'
    model_replacement: 'Treo $1'
  - regex: 'webOS.*(P160U(?:NA|))/(\d+).(\d+)'
    device_replacement: 'HP Veer'
    brand_replacement: 'HP'
    model_replacement: 'Veer'
  - regex: '(Touch[Pp]ad)/\d+\.\d+'
    device_replacement: 'HP TouchPad'
    brand_replacement: 'HP'
    model_replacement: 'TouchPad'
  - regex: 'HPiPAQ([A-Za-z0-9]+)/\d+.\d+'
    device_replacement: 'HP iPAQ $1'
    brand_replacement: 'HP'
    model_replacement: 'iPAQ $1'
  - regex: 'PDA; (PalmOS)/sony/model ([a-z]+)/Revision'
    device_replacement: '$1'
    brand_replacement: 'Sony'
    model_replacement: '$1 $2'

  ##########
  # AppleTV
  # No built in browser that I can tell
  # Stack Overflow indicated iTunes-AppleTV/4.1 as a known UA for app available and I'm seeing it in live traffic
  ##########
  - regex: '(Apple\s?TV)'
    device_replacement: 'AppleTV'
    brand_replacement: 'Apple'
    model_replacement: 'AppleTV'

  #########
  # Tesla Model S
  #########
  - regex: '(QtCarBrowser)'
    device_replacement: 'Tesla Model S'
    brand_replacement: 'Tesla'
    model_replacement: 'Model S'

  ##########
  # iSTUFF
  # @note: complete but probably catches spoofs
  #   ipad and ipod must be parsed before iphone
  #   cannot determine specific device type from ua string. (3g, 3gs, 4, etc)
  ##########
  # @note: on some ua the device can be identified e.g. iPhone5,1
  - regex: '(iPhone|iPad|iPod)(\d+,\d+)'
    device_replacement: '$1'
    brand_replacement: 'Apple'
    model_replacement: '$1$2'
  # @note: iPad needs to be before iPhone
  - regex: '(iPad)(?:;| Simulator;)'
    device_replacement: '$1'
    brand_replacement: 'Apple'
    model_replacement: '$1'
  - regex: '(iPod)(?:;| touch;| Simulator;)'
    device_replacement: '$1'
    brand_replacement: 'Apple'
    model_replacement: '$1'
  - regex: '(iPhone)(?:;| Simulator;)'
    device_replacement: '$1'
    brand_replacement: 'Apple'
    model_replacement: '$1'
  - regex: '(Watch)(\d+,\d+)'
    device_replacement: 'Apple $1'
    brand_replacement: 'Apple'
    model_replacement: 'Apple $1 $2'
  - regex: '(Apple Watch)(?:;| Simulator;)'
    device_replacement: '$1'
    brand_replacement: 'Apple'
    model_replacement: '$1'
  - regex: '(HomePod)(?:;| Simulator;)'
    device_replacement: '$1'
    brand_replacement: 'Apple'
    model_replacement: '$1'
  - regex: 'iPhone'
    device_replacement: 'iPhone'
    brand_replacement: 'Apple'
    model_replacement: 'iPhone'
  # @note: desktop applications show device info
  - regex: 'CFNetwork/.* Darwin/\d.*\(((?:Mac|iMac|PowerMac|PowerBook)[^\d]*)(\d+)(?:,|%2C)(\d+)'
    device_replacement: '$1$2,$3'
    brand_replacement: 'Apple'
    model_replacement: '$1$2,$3'
  # @note: newer desktop applications don't show device info
  # This is here so as to not have them recorded as iOS-Device
  - regex: 'CFNetwork/.* Darwin/\d+\.\d+\.\d+ \(x86_64\)'
    device_replacement: 'Mac'
    brand_replacement: 'Apple'
    model_replacement: 'Mac'
  # @note: iOS applications do not show device info
  - regex: 'CFNetwork/.* Darwin/\d'
    device_replacement: 'iOS-Device'
    brand_replacement: 'Apple'
    model_replacement: 'iOS-Device'

  ##########################
  # Outlook on iOS >= 2.62.0
  ##########################
  - regex: 'Outlook-(iOS)/\d+\.\d+\.prod\.iphone'
    brand_replacement: 'Apple'
    device_replacement: 'iPhone'
    model_replacement: 'iPhone'

  ##########
  # Acer
  ##########
  - regex: 'acer_([A-Za-z0-9]+)_'
    device_replacement: 'Acer $1'
    brand_replacement: 'Acer'
    model_replacement: '$1'

  ##########
  # Alcatel
  ##########
  - regex: '(?:ALCATEL|Alcatel)-([A-Za-z0-9\-]+)'
    device_replacement: 'Alcatel $1'
    brand_replacement: 'Alcatel'
    model_replacement: '$1'

  ##########
  # Amoi
  ##########
  - regex: '(?:Amoi|AMOI)\-([A-Za-z0-9]+)'
    device_replacement: 'Amoi $1'
    brand_replacement: 'Amoi'
    model_replacement: '$1'

  ##########
  # Asus
  ##########
  - regex: '(?:; |\/|^)((?:Transformer (?:Pad|Prime) |Transformer |PadFone[ _]?)[A-Za-z0-9]*)'
    device_replacement: 'Asus $1'
    brand_replacement: 'Asus'
    model_replacement: '$1'
  - regex: '(?:asus.*?ASUS|Asus|ASUS|asus)[\- ;]*((?:Transformer (?:Pad|Prime) |Transformer |Padfone |Nexus[ _]|)[A-Za-z0-9]+)'
    device_replacement: 'Asus $1'
    brand_replacement: 'Asus'
    model_replacement: '$1'
  - regex: '(?:ASUS)_([A-Za-z0-9\-]+)'
    device_replacement: 'Asus $1'
    brand_replacement: 'Asus'
    model_replacement: '$1'


  ##########
  # Bird
  ##########
  - regex: '\bBIRD[ \-\.]([A-Za-z0-9]+)'
    device_replacement: 'Bird $1'
    brand_replacement: 'Bird'
    model_replacement: '$1'

  ##########
  # Dell
  ##########
  - regex: '\bDell ([A-Za-z0-9]+)'
    device_replacement: 'Dell $1'
    brand_replacement: 'Dell'
    model_replacement: '$1'

  ##########
  # DoCoMo
  ##########
  - regex: 'DoCoMo/2\.0 ([A-Za-z0-9]+)'
    device_replacement: 'DoCoMo $1'
    brand_replacement: 'DoCoMo'
    model_replacement: '$1'
  - regex: '([A-Za-z0-9]+)_W;FOMA'
    device_replacement: 'DoCoMo $1'
    brand_replacement: 'DoCoMo'
    model_replacement: '$1'
  - regex: '([A-Za-z0-9]+);FOMA'
    device_replacement: 'DoCoMo $1'
    brand_replacement: 'DoCoMo'
    model_replacement: '$1'

  ##########
  # htc
  ##########
  - regex: '\b(?:HTC/|HTC/[a-z0-9]+/|)HTC[ _\-;]? *(.*?)(?:-?Mozilla|fingerPrint|[;/\(\)]|$)'
    device_replacement: 'HTC $1'
    brand_replacement: 'HTC'
    model_replacement: '$1'

  ##########
  # Huawei
  ##########
  - regex: 'Huawei([A-Za-z0-9]+)'
    device_replacement: 'Huawei $1'
    brand_replacement: 'Huawei'
    model_replacement: '$1'
  - regex: 'HUAWEI-([A-Za-z0-9]+)'
    device_replacement: 'Huawei $1'
    brand_replacement: 'Huawei'
    model_replacement: '$1'
  - regex: 'HUAWEI ([A-Za-z0-9\-]+)'
    device_replacement: 'Huawei $1'
    brand_replacement: 'Huawei'
    model_replacement: '$1'
  - regex: 'vodafone([A-Za-z0-9]+)'
    device_replacement: 'Huawei Vodafone $1'
    brand_replacement: 'Huawei'
    model_replacement: 'Vodafone $1'

  ##########
  # i-mate
  ##########
  - regex: 'i\-mate ([A-Za-z0-9]+)'
    device_replacement: 'i-mate $1'
    brand_replacement: 'i-mate'
    model_replacement: '$1'

  ##########
  # kyocera
  ##########
  - regex: 'Kyocera\-([A-Za-z0-9]+)'
    device_replacement: 'Kyocera $1'
    brand_replacement: 'Kyocera'
    model_replacement: '$1'
  - regex: 'KWC\-([A-Za-z0-9]+)'
    device_replacement: 'Kyocera $1'
    brand_replacement: 'Kyocera'
    model_replacement: '$1'

  ##########
  # lenovo
  ##########
  - regex: 'Lenovo[_\-]([A-Za-z0-9]+)'
    device_replacement: 'Lenovo $1'
    brand_replacement: 'Lenovo'
    model_replacement: '$1'

  ##########
  # HbbTV (European and Australian standard)
  # written before the LG regexes, as LG is making HbbTV too
  ##########
  - regex: '(HbbTV)/[0-9]+\.[0-9]+\.[0-9]+ \([^;]*; *(LG)E *; *([^;]*) *;[^;]*;[^;]*;\)'
    device_replacement: '$1'
    brand_replacement: '$2'
    model_replacement: '$3'
  - regex: '(HbbTV)/1\.1\.1.*CE-HTML/1\.\d;(Vendor/|)(THOM[^;]*?)[;\s].{0,30}(LF[^;]+);?'
    device_replacement: '$1'
    brand_replacement: 'Thomson'
    model_replacement: '$4'
  - regex: '(HbbTV)(?:/1\.1\.1|) ?(?: \(;;;;;\)|); *CE-HTML(?:/1\.\d|); *([^ ]+) ([^;]+);'
    device_replacement: '$1'
    brand_replacement: '$2'
    model_replacement: '$3'
  - regex: '(HbbTV)/1\.1\.1 \(;;;;;\) Maple_2011'
    device_replacement: '$1'
    brand_replacement: 'Samsung'
  - regex: '(HbbTV)/[0-9]+\.[0-9]+\.[0-9]+ \([^;]*; *(?:CUS:([^;]*)|([^;]+)) *; *([^;]*) *;.*;'
    device_replacement: '$1'
    brand_replacement: '$2$3'
    model_replacement: '$4'
  - regex: '(HbbTV)/[0-9]+\.[0-9]+\.[0-9]+'
    device_replacement: '$1'

  ##########
  # LGE NetCast TV
  ##########
  - regex: 'LGE; (?:Media\/|)([^;]*);[^;]*;[^;]*;?\); ""?LG NetCast(\.TV|\.Media|)-\d+'
    device_replacement: 'NetCast$2'
    brand_replacement: 'LG'
    model_replacement: '$1'

  ##########
  # InettvBrowser
  ##########
  - regex: 'InettvBrowser/[0-9]+\.[0-9A-Z]+ \([^;]*;(Sony)([^;]*);[^;]*;[^\)]*\)'
    device_replacement: 'Inettv'
    brand_replacement: '$1'
    model_replacement: '$2'
  - regex: 'InettvBrowser/[0-9]+\.[0-9A-Z]+ \([^;]*;([^;]*);[^;]*;[^\)]*\)'
    device_replacement: 'Inettv'
    brand_replacement: 'Generic_Inettv'
    model_replacement: '$1'
  - regex: '(?:InettvBrowser|TSBNetTV|NETTV|HBBTV)'
    device_replacement: 'Inettv'
    brand_replacement: 'Generic_Inettv'

  ##########
  # lg
  ##########
  # LG Symbian Phones
  - regex: 'Series60/\d\.\d (LG)[\-]?([A-Za-z0-9 \-]+)'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'
  # other LG phones
  - regex: '\b(?:LGE[ \-]LG\-(?:AX|)|LGE |LGE?-LG|LGE?[ \-]|LG[ /\-]|lg[\-])([A-Za-z0-9]+)\b'
    device_replacement: 'LG $1'
    brand_replacement: 'LG'
    model_replacement: '$1'
  - regex: '(?:^LG[\-]?|^LGE[\-/]?)([A-Za-z]+[0-9]+[A-Za-z]*)'
    device_replacement: 'LG $1'
    brand_replacement: 'LG'
    model_replacement: '$1'
  - regex: '^LG([0-9]+[A-Za-z]*)'
    device_replacement: 'LG $1'
    brand_replacement: 'LG'
    model_replacement: '$1'

  ##########
  # microsoft
  ##########
  - regex: '(KIN\.[^ ]+) (\d+)\.(\d+)'
    device_replacement: 'Microsoft $1'
    brand_replacement: 'Microsoft'
    model_replacement: '$1'
  - regex: '(?:MSIE|XBMC).*\b(Xbox)\b'
    device_replacement: '$1'
    brand_replacement: 'Microsoft'
    model_replacement: '$1'
  - regex: '; ARM; Trident/6\.0; Touch[\);]'
    device_replacement: 'Microsoft Surface RT'
    brand_replacement: 'Microsoft'
    model_replacement: 'Surface RT'

  ##########
  # motorola
  ##########
  - regex: 'Motorola\-([A-Za-z0-9]+)'
    device_replacement: 'Motorola $1'
    brand_replacement: 'Motorola'
    model_replacement: '$1'
  - regex: 'MOTO\-([A-Za-z0-9]+)'
    device_replacement: 'Motorola $1'
    brand_replacement: 'Motorola'
    model_replacement: '$1'
  - regex: 'MOT\-([A-z0-9][A-z0-9\-]*)'
    device_replacement: 'Motorola $1'
    brand_replacement: 'Motorola'
    model_replacement: '$1'

  ##########
  # nintendo
  ##########
  - regex: 'Nintendo WiiU'
    device_replacement: 'Nintendo Wii U'
    brand_replacement: 'Nintendo'
    model_replacement: 'Wii U'
  - regex: 'Nintendo (DS|3DS|DSi|Wii);'
    device_replacement: 'Nintendo $1'
    brand_replacement: 'Nintendo'
    model_replacement: '$1'

  ##########
  # pantech
  ##########
  - regex: '(?:Pantech|PANTECH)[ _-]?([A-Za-z0-9\-]+)'
    device_replacement: 'Pantech $1'
    brand_replacement: 'Pantech'
    model_replacement: '$1'

  ##########
  # philips
  ##########
  - regex: 'Philips([A-Za-z0-9]+)'
    device_replacement: 'Philips $1'
    brand_replacement: 'Philips'
    model_replacement: '$1'
  - regex: 'Philips ([A-Za-z0-9]+)'
    device_replacement: 'Philips $1'
    brand_replacement: 'Philips'
    model_replacement: '$1'

  ##########
  # Samsung
  ##########
  # Samsung Smart-TV
  - regex: '(SMART-TV); .* Tizen '
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'
  # Samsung Symbian Devices
  - regex: 'SymbianOS/9\.\d.* Samsung[/\-]([A-Za-z0-9 \-]+)'
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'
  - regex: '(Samsung)(SGH)(i[0-9]+)'
    device_replacement: '$1 $2$3'
    brand_replacement: '$1'
    model_replacement: '$2-$3'
  - regex: 'SAMSUNG-ANDROID-MMS/([^;/]+)'
    device_replacement: '$1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'
  # Other Samsung
  #- regex: 'SAMSUNG(?:; |-)([A-Za-z0-9\-]+)'
  - regex: 'SAMSUNG(?:; |[ -/])([A-Za-z0-9\-]+)'
    regex_flag: 'i'
    device_replacement: 'Samsung $1'
    brand_replacement: 'Samsung'
    model_replacement: '$1'

  ##########
  # Sega
  ##########
  - regex: '(Dreamcast)'
    device_replacement: 'Sega $1'
    brand_replacement: 'Sega'
    model_replacement: '$1'

  ##########
  # Siemens mobile
  ##########
  - regex: '^SIE-([A-Za-z0-9]+)'
    device_replacement: 'Siemens $1'
    brand_replacement: 'Siemens'
    model_replacement: '$1'

  ##########
  # Softbank
  ##########
  - regex: 'Softbank/[12]\.0/([A-Za-z0-9]+)'
    device_replacement: 'Softbank $1'
    brand_replacement: 'Softbank'
    model_replacement: '$1'

  ##########
  # SonyEricsson
  ##########
  - regex: 'SonyEricsson ?([A-Za-z0-9\-]+)'
    device_replacement: 'Ericsson $1'
    brand_replacement: 'SonyEricsson'
    model_replacement: '$1'

  ##########
  # Sony
  ##########
  - regex: 'Android [^;]+; ([^ ]+) (Sony)/'
    device_replacement: '$2 $1'
    brand_replacement: '$2'
    model_replacement: '$1'
  - regex: '(Sony)(?:BDP\/|\/|)([^ /;\)]+)[ /;\)]'
    device_replacement: '$1 $2'
    brand_replacement: '$1'
    model_replacement: '$2'

  #########
  # Puffin Browser Device detect
  # A=Android, I=iOS, P=Phone, T=Tablet
  # AT=Android+Tablet
  #########
  - regex: 'Puffin/[\d\.]+IT'
    device_replacement: 'iPad'
    brand_replacement: 'Apple'
    model_replacement: 'iPad'
  - regex: 'Puffin/[\d\.]+IP'
    device_replacement: 'iPhone'
    brand_replacement: 'Apple'
    model_replacement: 'iPhone'
  - regex: 'Puffin/[\d\.]+AT'
    device_replacement: 'Generic Tablet'
    brand_replacement: 'Generic'
    model_replacement: 'Tablet'
  - regex: 'Puffin/[\d\.]+AP'
    device_replacement: 'Generic Smartphone'
    brand_replacement: 'Generic'
    model_replacement: 'Smartphone'

  #########
  # Android General Device Matching (far from perfect)
  #########
  - regex: 'Android[\- ][\d]+\.[\d]+; [A-Za-z]{2}\-[A-Za-z]{0,2}; WOWMobile (.+)( Build[/ ]|\))'
    brand_replacement: 'Generic_Android'
    model_replacement: '$1'
  - regex: 'Android[\- ][\d]+\.[\d]+\-update1; [A-Za-z]{2}\-[A-Za-z]{0,2} *; *(.+?)( Build[/ ]|\))'
    brand_replacement: 'Generic_Android'
    model_replacement: '$1'
  - regex: 'Android[\- ][\d]+(?:\.[\d]+)(?:\.[\d]+|); *[A-Za-z]{2}[_\-][A-Za-z]{0,2}\-? *; *(.+?)( Build[/ ]|\))'
    brand_replacement: 'Generic_Android'
    model_replacement: '$1'
  - regex: 'Android[\- ][\d]+(?:\.[\d]+)(?:\.[\d]+|); *[A-Za-z]{0,2}\- *; *(.+?)( Build[/ ]|\))'
    brand_replacement: 'Generic_Android'
    model_replacement: '$1'
  # No build info at all - ""Build"" follows locale immediately
  - regex: 'Android[\- ][\d]+(?:\.[\d]+)(?:\.[\d]+|); *[a-z]{0,2}[_\-]?[A-Za-z]{0,2};?( Build[/ ]|\))'
    device_replacement: 'Generic Smartphone'
    brand_replacement: 'Generic'
    model_replacement: 'Smartphone'
  - regex: 'Android[\- ][\d]+(?:\.[\d]+)(?:\.[\d]+|); *\-?[A-Za-z]{2}; *(.+?)( Build[/ ]|\))'
    brand_replacement: 'Generic_Android'
    model_replacement: '$1'
  - regex: 'Android[\- ][\d]+(?:\.[\d]+)(?:\.[\d]+|)(?:;.*|); *(.+?)( Build[/ ]|\))'
    brand_replacement: 'Generic_Android'
    model_replacement: '$1'

  ##########
  # Google TV
  ##########
  - regex: '(GoogleTV)'
    brand_replacement: 'Generic_Inettv'
    model_replacement: '$1'

  ##########
  # WebTV
  ##########
  - regex: '(WebTV)/\d+.\d+'
    brand_replacement: 'Generic_Inettv'
    model_replacement: '$1'
  # Roku Digital-Video-Players https://www.roku.com/
  - regex: '^(Roku)/DVP-\d+\.\d+'
    brand_replacement: 'Generic_Inettv'
    model_replacement: '$1'

  ##########
  # Generic Tablet
  ##########
  - regex: '(Android 3\.\d|Opera Tablet|Tablet; .+Firefox/|Android.*(?:Tab|Pad))'
    regex_flag: 'i'
    device_replacement: 'Generic Tablet'
    brand_replacement: 'Generic'
    model_replacement: 'Tablet'

  ##########
  # Generic Smart Phone
  ##########
  - regex: '(Symbian|\bS60(Version|V\d)|\bS60\b|\((Series 60|Windows Mobile|Palm OS|Bada); Opera Mini|Windows CE|Opera Mobi|BREW|Brew|Mobile; .+Firefox/|iPhone OS|Android|MobileSafari|Windows *Phone|\(webOS/|PalmOS)'
    device_replacement: 'Generic Smartphone'
    brand_replacement: 'Generic'
    model_replacement: 'Smartphone'
  - regex: '(hiptop|avantgo|plucker|xiino|blazer|elaine)'
    regex_flag: 'i'
    device_replacement: 'Generic Smartphone'
    brand_replacement: 'Generic'
    model_replacement: 'Smartphone'

  ##########
  # Spiders (this is a hack...)
  ##########
  - regex: '(bot|BUbiNG|zao|borg|DBot|oegp|silk|Xenu|zeal|^NING|CCBot|crawl|htdig|lycos|slurp|teoma|voila|yahoo|Sogou|CiBra|Nutch|^Java/|^JNLP/|Daumoa|Daum|Genieo|ichiro|larbin|pompos|Scrapy|snappy|speedy|spider|msnbot|msrbot|vortex|^vortex|crawler|favicon|indexer|Riddler|scooter|scraper|scrubby|WhatWeb|WinHTTP|bingbot|BingPreview|openbot|gigabot|furlbot|polybot|seekbot|^voyager|archiver|Icarus6j|mogimogi|Netvibes|blitzbot|altavista|charlotte|findlinks|Retreiver|TLSProber|WordPress|SeznamBot|ProoXiBot|wsr\-agent|Squrl Java|EtaoSpider|PaperLiBot|SputnikBot|A6\-Indexer|netresearch|searchsight|baiduspider|YisouSpider|ICC\-Crawler|http%20client|Python-urllib|dataparksearch|converacrawler|Screaming Frog|AppEngine-Google|YahooCacheSystem|fast\-webcrawler|Sogou Pic Spider|semanticdiscovery|Innovazion Crawler|facebookexternalhit|Google.*/\+/web/snippet|Google-HTTP-Java-Client|BlogBridge|IlTrovatore-Setaccio|InternetArchive|GomezAgent|WebThumbnail|heritrix|NewsGator|PagePeeker|Reaper|ZooShot|holmes|NL-Crawler|Pingdom|StatusCake|WhatsApp|masscan|Google Web Preview|Qwantify|Yeti|OgScrper)'
    regex_flag: 'i'
    device_replacement: 'Spider'
    brand_replacement: 'Spider'
    model_replacement: 'Desktop'

  ##########
  # Generic Feature Phone
  # take care to do case insensitive matching
  ##########
  - regex: '^(1207|3gso|4thp|501i|502i|503i|504i|505i|506i|6310|6590|770s|802s|a wa|acer|acs\-|airn|alav|asus|attw|au\-m|aur |aus |abac|acoo|aiko|alco|alca|amoi|anex|anny|anyw|aptu|arch|argo|bmobile|bell|bird|bw\-n|bw\-u|beck|benq|bilb|blac|c55/|cdm\-|chtm|capi|comp|cond|dall|dbte|dc\-s|dica|ds\-d|ds12|dait|devi|dmob|doco|dopo|dorado|el(?:38|39|48|49|50|55|58|68)|el[3456]\d{2}dual|erk0|esl8|ex300|ez40|ez60|ez70|ezos|ezze|elai|emul|eric|ezwa|fake|fly\-|fly_|g\-mo|g1 u|g560|gf\-5|grun|gene|go.w|good|grad|hcit|hd\-m|hd\-p|hd\-t|hei\-|hp i|hpip|hs\-c|htc |htc\-|htca|htcg)'
    regex_flag: 'i'
    device_replacement: 'Generic Feature Phone'
    brand_replacement: 'Generic'
    model_replacement: 'Feature Phone'
  - regex: '^(htcp|htcs|htct|htc_|haie|hita|huaw|hutc|i\-20|i\-go|i\-ma|i\-mobile|i230|iac|iac\-|iac/|ig01|im1k|inno|iris|jata|kddi|kgt|kgt/|kpt |kwc\-|klon|lexi|lg g|lg\-a|lg\-b|lg\-c|lg\-d|lg\-f|lg\-g|lg\-k|lg\-l|lg\-m|lg\-o|lg\-p|lg\-s|lg\-t|lg\-u|lg\-w|lg/k|lg/l|lg/u|lg50|lg54|lge\-|lge/|leno|m1\-w|m3ga|m50/|maui|mc01|mc21|mcca|medi|meri|mio8|mioa|mo01|mo02|mode|modo|mot |mot\-|mt50|mtp1|mtv |mate|maxo|merc|mits|mobi|motv|mozz|n100|n101|n102|n202|n203|n300|n302|n500|n502|n505|n700|n701|n710|nec\-|nem\-|newg|neon)'
    regex_flag: 'i'
    device_replacement: 'Generic Feature Phone'
    brand_replacement: 'Generic'
    model_replacement: 'Feature Phone'
  - regex: '^(netf|noki|nzph|o2 x|o2\-x|opwv|owg1|opti|oran|ot\-s|p800|pand|pg\-1|pg\-2|pg\-3|pg\-6|pg\-8|pg\-c|pg13|phil|pn\-2|pt\-g|palm|pana|pire|pock|pose|psio|qa\-a|qc\-2|qc\-3|qc\-5|qc\-7|qc07|qc12|qc21|qc32|qc60|qci\-|qwap|qtek|r380|r600|raks|rim9|rove|s55/|sage|sams|sc01|sch\-|scp\-|sdk/|se47|sec\-|sec0|sec1|semc|sgh\-|shar|sie\-|sk\-0|sl45|slid|smb3|smt5|sp01|sph\-|spv |spv\-|sy01|samm|sany|sava|scoo|send|siem|smar|smit|soft|sony|t\-mo|t218|t250|t600|t610|t618|tcl\-|tdg\-|telm|tim\-|ts70|tsm\-|tsm3|tsm5|tx\-9|tagt)'
    regex_flag: 'i'
    device_replacement: 'Generic Feature Phone'
    brand_replacement: 'Generic'
    model_replacement: 'Feature Phone'
  - regex: '^(talk|teli|topl|tosh|up.b|upg1|utst|v400|v750|veri|vk\-v|vk40|vk50|vk52|vk53|vm40|vx98|virg|vertu|vite|voda|vulc|w3c |w3c\-|wapj|wapp|wapu|wapm|wig |wapi|wapr|wapv|wapy|wapa|waps|wapt|winc|winw|wonu|x700|xda2|xdag|yas\-|your|zte\-|zeto|aste|audi|avan|blaz|brew|brvw|bumb|ccwa|cell|cldc|cmd\-|dang|eml2|fetc|hipt|http|ibro|idea|ikom|ipaq|jbro|jemu|jigs|keji|kyoc|kyok|libw|m\-cr|midp|mmef|moto|mwbp|mywa|newt|nok6|o2im|pant|pdxg|play|pluc|port|prox|rozo|sama|seri|smal|symb|treo|upsi|vx52|vx53|vx60|vx61|vx70|vx80|vx81|vx83|vx85|wap\-|webc|whit|wmlb|xda\-|xda_)'
    regex_flag: 'i'
    device_replacement: 'Generic Feature Phone'
    brand_replacement: 'Generic'
    model_replacement: 'Feature Phone'
  - regex: '^(Ice)$'
    device_replacement: 'Generic Feature Phone'
    brand_replacement: 'Generic'
    model_replacement: 'Feature Phone'
  - regex: '(wap[\-\ ]browser|maui|netfront|obigo|teleca|up\.browser|midp|Opera Mini)'
    regex_flag: 'i'
    device_replacement: 'Generic Feature Phone'
    brand_replacement: 'Generic'
    model_replacement: 'Feature Phone'
";

        #endregion
    }
}
