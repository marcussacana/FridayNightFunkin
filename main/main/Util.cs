using Newtonsoft.Json;
using Orbis.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics.Eventing.Reader;

namespace Orbis
{
    internal static class Util
    {

        static ZipFile Assets;

        public static Stream GetAsset(string FilePath)
        {
            if (Assets == null)
            {
#if ORBIS
                string RootDir = IO.GetAppBaseDirectory();
#else
                string RootDir = AppDomain.CurrentDomain.BaseDirectory;
#endif
                var ZipPath = Path.Combine(RootDir, "assets/misc/assets.zip");

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

        public static SongInfo GetSongByName(string Name)
        {
            using (var Data = CopyFileToMemory($"preload/data/{Name}/{Name}.json"))
            {
                var JSON = Encoding.UTF8.GetString(Data.ToArray());
                return JsonConvert.DeserializeObject<SongInfo>(JSON);
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
