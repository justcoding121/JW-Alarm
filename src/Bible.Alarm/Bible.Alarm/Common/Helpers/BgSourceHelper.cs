using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bible.Alarm.Common.Helpers
{
    public class BgSourceHelper
    {
        private static string[] bookCodes = new string[] { "Gen", "Exod", "Lev", "Num", "Deut", "Josh", "Judg", "Ruth", "1Sam", "2Sam", "1Kgs", "2Kgs", "1Chr", "2Chr", "Ezra", "Neh", "Esth", "Job", "Ps", "Prov", "Eccl", "Song", "Isa", "Jer", "Lam", "Ezek", "Dan", "Hos", "Joel", "Amos", "Obad", "Jonah", "Mic", "Nah", "Hab", "Zeph", "Hag", "Zech", "Mal", "Matt", "Mark", "Luke", "John", "Acts", "Rom", "1Cor", "2Cor", "Gal", "Eph", "Phil", "Col", "1Thess", "2Thess", "1Tim", "2Tim", "Titus", "Phlm", "Heb", "Jas", "1Pet", "2Pet", "1John", "2John", "3John", "Jude", "Rev" };
        public static Dictionary<int, string> BookNumberToBookCodeMap = bookCodes
                                                       .Select((s, i) => new { i = i + 1, s })
                                                       .ToDictionary(x => x.i, x => x.s);

        public static Dictionary<string, string> PublisherCodeToAuthorsCodeMap = new[]
        {
            new KeyValuePair<string, string>("kjv", "mims"),
            new KeyValuePair<string, string>("nivuk", "suchet")

        }.ToDictionary(x => x.Key, x => x.Value);

        public static Dictionary<string, string> AuthorCodeToAuthorNameMap = new[]
        {
            new KeyValuePair<string, string>("mims", "David suchet"),
            new KeyValuePair<string, string>("suchet","Paul Mims")

        }.ToDictionary(x => x.Key, x => x.Value);

        public static Dictionary<string, string> PublicationCodeToNameMappings = new[]
        {
            new KeyValuePair<string, string>("kjv","King James Version (1987)"),
            new KeyValuePair<string, string>("nivuk","New International Version—Anglicised (1984)")

        }.ToDictionary(x => x.Key, x => x.Value);

        public static Dictionary<int, string> BookNumberToNamesMap = getBookNames();

        public static Dictionary<string, int> BookCodeToTotalChaptersMap = getChapterNumbers();

        private static Dictionary<string, int> getChapterNumbers()
        {
            var chapterDict = new Dictionary<string, int>();

            chapterDict["Gen"] = 50;
            chapterDict["Exod"] = 40;
            chapterDict["Lev"] = 27;
            chapterDict["Num"] = 36;
            chapterDict["Deut"] = 34;
            chapterDict["Josh"] = 24;
            chapterDict["Judg"] = 21;
            chapterDict["Ruth"] = 4;
            chapterDict["1Sam"] = 31;
            chapterDict["2Sam"] = 24;
            chapterDict["1Kgs"] = 22;
            chapterDict["2Kgs"] = 25;
            chapterDict["1Chr"] = 29;
            chapterDict["2Chr"] = 36;
            chapterDict["Ezra"] = 10;
            chapterDict["Neh"] = 13;
            chapterDict["Esth"] = 10;
            chapterDict["Job"] = 42;
            chapterDict["Ps"] = 150;
            chapterDict["Prov"] = 31;
            chapterDict["Eccl"] = 12;
            chapterDict["Song"] = 8;
            chapterDict["Isa"] = 66;
            chapterDict["Jer"] = 52;
            chapterDict["Lam"] = 5;
            chapterDict["Ezek"] = 48;
            chapterDict["Dan"] = 12;
            chapterDict["Hos"] = 14;
            chapterDict["Joel"] = 3;
            chapterDict["Amos"] = 9;
            chapterDict["Obad"] = 1;
            chapterDict["Jonah"] = 4;
            chapterDict["Mic"] = 7;
            chapterDict["Nah"] = 3;
            chapterDict["Hab"] = 3;
            chapterDict["Zeph"] = 3;
            chapterDict["Hag"] = 2;
            chapterDict["Zech"] = 14;
            chapterDict["Mal"] = 4;
            chapterDict["Matt"] = 28;
            chapterDict["Mark"] = 16;
            chapterDict["Luke"] = 24;
            chapterDict["John"] = 21;
            chapterDict["Acts"] = 28;
            chapterDict["Rom"] = 16;
            chapterDict["1Cor"] = 16;
            chapterDict["2Cor"] = 13;
            chapterDict["Gal"] = 6;
            chapterDict["Eph"] = 6;
            chapterDict["Phil"] = 4;
            chapterDict["Col"] = 4;
            chapterDict["1Thess"] = 5;
            chapterDict["2Thess"] = 3;
            chapterDict["1Tim"] = 6;
            chapterDict["2Tim"] = 4;
            chapterDict["Titus"] = 3;
            chapterDict["Phlm"] = 1;
            chapterDict["Heb"] = 13;
            chapterDict["Jas"] = 5;
            chapterDict["1Pet"] = 5;
            chapterDict["2Pet"] = 3;
            chapterDict["1John"] = 5;
            chapterDict["2John"] = 1;
            chapterDict["3John"] = 1;
            chapterDict["Jude"] = 1;
            chapterDict["Rev"] = 22;

            return chapterDict;
        }

        private static Dictionary<int, string> getBookNames()
        {
            return new string[] { "Genesis", "Exodus", "Leviticus", "Numbers", "Deuteronomy", "Joshua", "Judges", "Ruth", "1 Samuel", "2 Samuel", "1 Kings", "2 Kings", "1 Chronicles", "2 Chronicles", "Ezra", "Nehemiah", "Esther", "Job", "Psalms", "Proverbs", "Ecclesiastes", "Song of Solomon", "Isaiah", "Jeremiah", "Lamentations", "Ezekiel", "Daniel", "Hosea", "Joel", "Amos", "Obadiah", "Jonah", "Micah", "Nahum", "Habakkuk", "Zephaniah", "Haggai", "Zechariah", "Malachi", "Matthew", "Mark", "Luke", "John", "Acts (of the Apostles)", "Romans", "1 Corinthians", "2 Corinthians", "Galatians", "Ephesians", "Philippians", "Colossians", "1 Thessalonians", "2 Thessalonians", "1 Timothy", "2 Timothy", "Titus", "Philemon", "Hebrews", "James", "1 Peter", "2 Peter", "1 John", "2 John", "3 John", "Jude", "Revelation" }
                       .Select((s, i) => new { i = i + 1, s })
                       .ToDictionary(x => x.i, x => x.s);
        }
    }

}
