using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Orbis
{
    internal static class Util
    {
        public static MemoryStream CopyFileToMemory(string FilePath)
        {
#if ORBIS
            string RootDir = IO.GetAppBaseDirectory();
#else
            string RootDir = AppDomain.CurrentDomain.BaseDirectory;
#endif

            var Hints = new string[] {
                "assets/shared/images/characters/",
                "assets/preload/images",
                "assets/shared/images",
                "assets/preload/images/icons"
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
                throw new FileNotFoundException(FilePath);

            MemoryStream Output = new MemoryStream();
            using (var Stream = File.OpenRead(FilePath))
                Stream.CopyTo(Output);

            Output.Position = 0;
            return Output;
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

        public static void PrepareAsemblies()
        {
            bool NewAssembly = true; 
            var AllMethods = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            List<Assembly> Ready = new List<Assembly>();

            while (NewAssembly)
            {
                NewAssembly = false;
                foreach (var Asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (Ready.Contains(Asm))
                        continue;

                    NewAssembly = true;
                    Ready.Add(Asm);
                    foreach (var type in Asm.GetTypes())
                    {
                        foreach (var method in type.GetMethods(AllMethods))
                        {
                            System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(method.MethodHandle);
                        }
                    }
                }
            }
        }
    }
}
