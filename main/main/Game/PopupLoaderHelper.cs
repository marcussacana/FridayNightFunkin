using Orbis.Interfaces;
using OrbisGL.GL;
using System;
using System.Collections.Generic;

namespace Orbis.Game
{
    public class PopupLoaderHelper : ILoadable
    {
        public string Sufix = null;

        public float ZoomFactor = 0;

        public enum Popup {
            Perfect,
            Good,
            Bad,
            Miss,
            Combo0,
            Combo1,
            Combo2,
            Combo3,
            Combo4,
            Combo5,
            Combo6,
            Combo7,
            Combo8,
            Combo9
        }
        Dictionary<Popup, Texture> Textures = new Dictionary<Popup, Texture>();
        public bool Loaded { get; private set; }

        public int TotalProgress => 14;

        public void Dispose()
        {
            foreach (var Tex in Textures)
                Tex.Value.Dispose();
        }

        public void Draw(long Tick)
        {
            throw new NotImplementedException();
        }

        public Texture GetTexture(Popup Type) => Textures[Type];

        public void Load(Action<int> OnProgressChanged)
        {
            if (Loaded)
                return;

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"sick{Sufix}.dds" : "sick.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Perfect] = Tex;
            }

            OnProgressChanged(1);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"good{Sufix}.dds" : "good.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Good] = Tex;
            }

            OnProgressChanged(2);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"bad{Sufix}.dds" : "bad.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Bad] = Tex;
            }

            OnProgressChanged(3);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"shit{Sufix}.dds" : "shit.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Miss] = Tex;
            }

            OnProgressChanged(4);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"num0{Sufix}.dds" : "num0.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Combo0] = Tex;
            }

            OnProgressChanged(5);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"num1{Sufix}.dds" : "num1.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Combo1] = Tex;
            }

            OnProgressChanged(6);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"num2{Sufix}.dds" : "num2.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Combo2] = Tex;
            }

            OnProgressChanged(7);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"num3{Sufix}.dds" : "num3.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Combo3] = Tex;
            }

            OnProgressChanged(8);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"num4{Sufix}.dds" : "num4.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Combo4] = Tex;
            }

            OnProgressChanged(9);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"num5{Sufix}.dds" : "num5.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Combo5] = Tex;
            }

            OnProgressChanged(10);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"num6{Sufix}.dds" : "num6.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Combo6] = Tex;
            }

            OnProgressChanged(11);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"num7{Sufix}.dds" : "num7.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Combo7] = Tex;
            }

            OnProgressChanged(12);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"num8{Sufix}.dds" : "num8.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Combo8] = Tex;
            }

            OnProgressChanged(13);

            using (var TexData = Util.CopyFileToMemory(Sufix != null ? $"num9{Sufix}.dds" : "num9.dds"))
            {
                Texture Tex = new Texture(true);
                Tex.SetDDS(TexData, true);

                Textures[Popup.Combo9] = Tex;
            }
            
            Loaded = true;

            OnProgressChanged(14);
        }

    }
}
