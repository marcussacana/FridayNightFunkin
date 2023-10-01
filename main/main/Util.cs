using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using Orbis.Internals;

namespace Orbis
{
    public static class Util
    {
        public static Dictionary<char, string> FontBoldMap = new Dictionary<char, string>()
        {
            { '#', "hashtag" },
            { '$', "dollarsign" },
            { '%', "%" },
            { '&', "amp" },
            { '(', "(" },
            { ')', ")" },
            { '*', "*" },
            { '+', "+" },
            { '-', "-" },
            { '0', "0" },
            { '1', "1" },
            { '2', "2" },
            { '3', "3" },
            { '4', "4" },
            { '5', "5" },
            { '6', "6" },
            { '7', "7" },
            { '8', "8" },
            { '9', "9" },
            { ':', ":" },
            { ';', ";" },
            { '<', "<" },
            { '=', "=" },
            { '>', ">" },
            { '@', "@" },
            { 'A', "A bold" },
            { 'B', "B bold" },
            { 'C', "C bold" },
            { 'D', "D bold" },
            { 'E', "E bold" },
            { 'F', "F bold" },
            { 'G', "G bold" },
            { 'H', "H bold" },
            { 'I', "I bold" },
            { 'J', "J bold" },
            { 'K', "K bold" },
            { 'L', "L bold" },
            { 'M', "M bold" },
            { 'N', "N bold" },
            { 'O', "O bold" },
            { 'P', "P bold" },
            { 'Q', "Q bold" },
            { 'R', "R bold" },
            { 'S', "S bold" },
            { 'T', "T bold" },
            { 'U', "U bold" },
            { 'V', "V bold" },
            { 'W', "W bold" },
            { 'X', "X bold" },
            { 'Y', "Y bold" },
            { 'Z', "Z bold" },
            { '[', "[" },
            { '\\', "\\"},
            { '^', "^"},
            { '_', "_" },
            { 'a', "a lowercase" },
            { 'b', "b lowercase" },
            { 'c', "c lowercase" },
            { 'd', "d lowercase" },
            { 'e', "e lowercase" },
            { 'f', "f lowercase" },
            { 'g', "g lowercase" },
            { 'h', "h lowercase" },
            { 'i', "i lowercase" },
            { 'j', "j lowercase" },
            { 'k', "k lowercase" },
            { 'l', "l lowercase" },
            { 'm', "m lowercase" },
            { 'n', "n lowercase" },
            { 'o', "o lowercase" },
            { 'p', "p lowercase" },
            { 'q', "q lowercase" },
            { 'r', "r lowercase" },
            { 's', "s lowercase" },
            { 't', "t lowercase" },
            { 'u', "u lowercase" },
            { 'v', "v lowercase" },
            { 'w', "w lowercase" },
            { 'x', "x lowercase" },
            { 'y', "y lowercase" },
            { 'z', "z lowercase" },
            { '\x1', "angry faic" },
            { '’', "apostraphie" },
            { ',', "comma" },
            { '↓', "down arrow" },
            { '”', "end parentheses"},
            { '!', "exclamation point" },
            { '/', "forward slash" },
            { '\x2', "heart" },
            { '←', "left arrow" },
            { '✖', "multiply x" },
            { '¨', "period" },
            { '?', "question mark" },
            { '→', "right arrow"},
            { '“', "start parentheses"},
            { '↑', "up arrow" },
            { '|', "|" },
            { '~', "~" }
        };

        public static Dictionary<char, string> FontCapitalMap = new Dictionary<char, string>()
        {
            { '#', "hashtag" },
            { '$', "dollarsign" },
            { '%', "%" },
            { '&', "amp" },
            { '(', "(" },
            { ')', ")" },
            { '*', "*" },
            { '+', "+" },
            { '-', "-" },
            { '0', "0" },
            { '1', "1" },
            { '2', "2" },
            { '3', "3" },
            { '4', "4" },
            { '5', "5" },
            { '6', "6" },
            { '7', "7" },
            { '8', "8" },
            { '9', "9" },
            { ':', ":" },
            { ';', ";" },
            { '<', "<" },
            { '=', "=" },
            { '>', ">" },
            { '@', "@" },
            { 'A', "A capital" },
            { 'B', "B capital" },
            { 'C', "C capital" },
            { 'D', "D capital" },
            { 'E', "E capital" },
            { 'F', "F capital" },
            { 'G', "G capital" },
            { 'H', "H capital" },
            { 'I', "I capital" },
            { 'J', "J capital" },
            { 'K', "K capital" },
            { 'L', "L capital" },
            { 'M', "M capital" },
            { 'N', "N capital" },
            { 'O', "O capital" },
            { 'P', "P capital" },
            { 'Q', "Q capital" },
            { 'R', "R capital" },
            { 'S', "S capital" },
            { 'T', "T capital" },
            { 'U', "U capital" },
            { 'V', "V capital" },
            { 'W', "W capital" },
            { 'X', "X capital" },
            { 'Y', "Y capital" },
            { 'Z', "Z capital" },
            { '[', "[" },
            { '\\', "\\"},
            { '^', "^"},
            { '_', "_" },
            { 'a', "a lowercase" },
            { 'b', "b lowercase" },
            { 'c', "c lowercase" },
            { 'd', "d lowercase" },
            { 'e', "e lowercase" },
            { 'f', "f lowercase" },
            { 'g', "g lowercase" },
            { 'h', "h lowercase" },
            { 'i', "i lowercase" },
            { 'j', "j lowercase" },
            { 'k', "k lowercase" },
            { 'l', "l lowercase" },
            { 'm', "m lowercase" },
            { 'n', "n lowercase" },
            { 'o', "o lowercase" },
            { 'p', "p lowercase" },
            { 'q', "q lowercase" },
            { 'r', "r lowercase" },
            { 's', "s lowercase" },
            { 't', "t lowercase" },
            { 'u', "u lowercase" },
            { 'v', "v lowercase" },
            { 'w', "w lowercase" },
            { 'x', "x lowercase" },
            { 'y', "y lowercase" },
            { 'z', "z lowercase" },
            { '\x1', "angry faic" },
            { '’', "apostraphie" },
            { ',', "comma" },
            { '↓', "down arrow" },
            { '”', "end parentheses"},
            { '!', "exclamation point" },
            { '/', "forward slash" },
            { '\x2', "heart" },
            { '←', "left arrow" },
            { '✖', "multiply x" },
            { '¨', "period" },
            { '?', "question mark" },
            { '→', "right arrow"},
            { '“', "start parentheses"},
            { '↑', "up arrow" },
            { '|', "|" },
            { '~', "~" }
        };

        static ZipFile Assets;

        public static Stream GetAsset(string FilePath)
        {
            if (Assets == null)
            {
#if ORBIS
                string RootDir = IO.GetAppBaseDirectory();
                var ZipPath = Path.Combine(RootDir, "assets/misc/assets.zip");
#else
                string RootDir = AppDomain.CurrentDomain.BaseDirectory;
                var ZipPath = Path.Combine(RootDir, "assets.zip");
#endif

                if (!File.Exists(ZipPath))
                    return null;

                Stream FStream = File.OpenRead(ZipPath);
                Assets = new ZipFile(FStream);
            }

            if (FilePath.StartsWith("assets/"))
                FilePath = FilePath.Substring(FilePath.IndexOf("/") + 1);

            FilePath = FilePath.Replace("\\", "/");


            var Files = Assets.GetEntries().Where(x => x.Name.ToLowerInvariant() == FilePath.ToLowerInvariant() || Path.GetFileName(x.Name) == FilePath);

            if (!Files.Any())
                return null;

            if (Files.Count() > 1)
                throw new FileNotFoundException("Duplicate file entry with name: " + FilePath);

            return Assets.GetInputStream(Files.Single());
        }

        public static MemoryStream CopyFileToMemory(string FilePath)
        {
            using (var ZStream = GetAsset(FilePath))
            {
                if (ZStream != null)
                {
                    MemoryStream Result = new MemoryStream();
                    ZStream.CopyTo(Result);
                    Result.Position = 0;
                    return Result;
                }
            }

#if ORBIS
            string RootDir = IO.GetAppBaseDirectory();
#else
            string RootDir = AppDomain.CurrentDomain.BaseDirectory;
#endif

            var Hints = new string[] {
                "assets/shared/images/characters/",
                "assets/preload/images",
                "assets/shared/images",
                "assets/preload/images/icons",
                "assets/week2/images",
                "assets/week3/images/philly",
                "assets/week4/images/limo",
                "assets/week5/images/christmas",
                "assets/",
            };

            if (!File.Exists(FilePath) && File.Exists(Path.Combine(RootDir, FilePath)))
            {
                FilePath = Path.Combine(RootDir, FilePath);
            }
            else
            {
                for (int i = 0; i < Hints.Length; i++)
                {
                    var Hint = Hints[i];
                    if (!File.Exists(FilePath) && File.Exists(Path.Combine(RootDir, Hint, FilePath)))
                    {
                        FilePath = Path.Combine(RootDir, Hint, FilePath);
                        break;
                    }
                }
            } 

            if (!File.Exists(FilePath))
                return null;

            MemoryStream Output = new MemoryStream();
            using (var Stream = File.OpenRead(FilePath))
                Stream.CopyTo(Output);

            Output.Position = 0;
            return Output;
        }

        public static SongInfo GetSongByName(string Name, Dificuty Dificuty)
        {
            var DificutyName = "";
            switch (Dificuty)
            {
                case Dificuty.Easy:
                    DificutyName = "-easy";
                    break;
                case Dificuty.Hard:
                    DificutyName = "-hard";
                    break;
            }

            using (var Data = CopyFileToMemory($"preload/data/{Name}/{Name}{DificutyName}.json") ?? CopyFileToMemory($"preload/data/{Name}/{Name}.json"))
            {
                var JSON = Encoding.UTF8.GetString(Data.ToArray());

                if (Name == "ugh")
                {
                    //Bad JSON in the original file, I wanna to make my code works
                    //with original assets so I'm 'fixing' with this instead
                    JSON = JSON.Replace(",true]]}", "]]}");
                }

                var Song = JsonConvert.DeserializeObject<SongInfo>(JSON);

                switch (Name.ToLowerInvariant().Trim())
                {
                    default:
                        Song.BG = Map.Stage;
                        break;
                    case "spookeez":
                    case "monster":
                    case "south":
                        Song.BG = Map.Halloween;
                        break;
                    case "pico":
                    case "philly":
                    case "blammed":
                        Song.BG = Map.Philly;
                        break;
                    case "satin-panties":
                    case "high":
                    case "milf":
                        Song.BG = Map.Limo;
                        break;
                    case "cocoa":
                    case "eggnog":
                        Song.BG = Map.Christmas;
                        break;
                    case "winter-horrorland":
                        Song.BG = Map.ChristmasEvil;
                        break;
                    case "senpai":
                        Song.BG = Map.WeebSchool;
                        Song.Speaker = "gf-pixel";
                        break;
                    case "roses":
                        Song.BG = Map.WeebSchool;
                        Song.Speaker = "gf-pixel";
                        break;
                }

                return Song;
            }
        }

        public static XmlDocument GetXML(string Name)
        {
            using (var XMLData = CopyFileToMemory(Name))
            {
                XmlDocument Document = new XmlDocument();
                Document.Load(XMLData);
                return Document;
            }
        }

        public static IEnumerable<ZipEntry> GetEntries(this ZipFile Archive)
        {
            var Enum = Archive.GetEnumerator();
            Enum.Reset();
            
            while (Enum.MoveNext())
            {
                yield return Enum.Current as ZipEntry;
            }
        }
    }
}
