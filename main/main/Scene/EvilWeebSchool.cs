using Orbis.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbis.Scene
{
    internal class EvilWeebSchool : WeebSchool
    {
        public EvilWeebSchool(SongPlayer Player) : base(Player)
        {
        }

        public override int TotalProgress => base.TotalProgress + 2;

        public override void Load(Action<int> OnProgressChanged)
        {
            base.Load((i) =>
            {
                Loaded = false;
                OnProgressChanged?.Invoke(i);
            });

            this.Sky.Dispose();
            this.Tree.Dispose();
            this.TreeBack.Dispose();
            this.Freaks.Dispose();


            using (var Stream = Util.CopyFileToMemory("evilSchoolBG.dds"))
                this.School.Texture.SetDDS(Stream, false);

            OnProgressChanged?.Invoke(base.TotalProgress + 1);

            using (var Stream = Util.CopyFileToMemory("evilSchoolFG.dds"))
                this.Street.Texture.SetDDS(Stream, false);

            Loaded = true;

            OnProgressChanged?.Invoke(base.TotalProgress + 2);
        }
    }
}
