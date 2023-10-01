using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Orbis
{

    //These classes make a clear and safe pattern for all file and animation names.

    public static class Character
    {
        public const string BoyfriendAssets = "BOYFRIEND.xml";
        public const string BoyfriendChristmasAssets = "bfChristmas.xml";
        public const string BoyfriendCarAssets = "bfCar.xml";
        public const string BoyfriendPixelAssets = "bfPixel.xml";
        public const string BoyfriendDeadPixelAssets = "bfPixelsDEAD.xml";
        public const string GirlfriendAssets = "GF_assets.xml";
        public const string DadAssets = "DADDY_DEAREST.xml";
        public const string SpookyKidsAssets = "spooky_kids_assets.xml";
        public const string MonsterAssets = "Monster_Assets.xml";
        public const string MonsterChristmasAssets = "monsterChristmas.xml";
        public const string MomAssets = "Mom_Assets.xml";
        public const string MomCarAssets = "momCar.xml";
        public const string ParentsChristmasAssets = "mom_dad_christmas_assets.xml";
        public const string PicoAssets = "Pico_FNF_assetss.xml";
        public const string Senpai = "senpai.xml";
        public const string TankmanAssets = "tankmanCaptain.xml";

        public const string BoyfriendIcon = "icon-bf.dds";
        public const string BoyfriendPixelIcon = "icon-bf-pixel.dds";
        public const string DadIcon = "icon-dad.dds";
        public const string GirlfriendIcon = "icon-gf.dds";
        public const string MomIcon = "icon-mom.dds";
        public const string MonsterIcon = "icon-monster.dds";
        public const string ParentsIcon = "icon-parents.dds";
        public const string PicoIcon = "icon-pico.dds";
        public const string SenpaiIcon = "icon-senpai.dds";
        public const string SpiritIcon = "icon-spirit.dds";
        public const string SpookyIcon = "icon-spooky.dds";
        public const string TankmanIcon = "icon-tankman.dds";


        public static readonly ReadOnlyDictionary<string, string> AssetsMap = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string> {
                { "bf" , BoyfriendAssets },
                { "bf-christmas" , BoyfriendChristmasAssets },
                { "bf-car" , BoyfriendCarAssets },
                { "bf-pixel" , BoyfriendPixelAssets },
                { "dad" , DadAssets },
                { "mom" , MomAssets },
                { "mom-car" , MomCarAssets },
                { "pico" , PicoAssets },
                { "monster" , MonsterAssets },
                { "monster-christmas" , MonsterChristmasAssets },
                { "senpai", Senpai},
                { "senpai-angry" , Senpai },
                { "spooky" , SpookyKidsAssets },
                { "gf" , GirlfriendAssets },
                { "parents-christmas", ParentsChristmasAssets },
                { "tankman", TankmanAssets}
            });

        public static readonly ReadOnlyDictionary<string, string> IconMap = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string> {
                { "bf" , BoyfriendIcon },
                { "bf-christmas" , BoyfriendIcon},
                { "bf-car" , BoyfriendIcon },
                { "bf-pixel" , BoyfriendPixelIcon },
                { "dad" , DadIcon },
                { "mom" , MomIcon },
                { "mom-car" , MomIcon },
                { "pico" , PicoIcon },
                { "monster" , MonsterIcon },
                { "monster-christmas" , MonsterIcon },
                { "senpai", SenpaiIcon },
                { "senpai-angry" , SenpaiIcon },
                { "spooky" , SpookyIcon },
                { "gf" , GirlfriendIcon },
                { "parents-christmas", ParentsIcon },
                { "tankman", TankmanIcon }
            });
    }

    static class NotesNames
    {
        public const string NotesAssets = "NOTE_assets.xml";

        public const string STATIC_ARROW = "arrow static instance";

        public const int LEFT_FRAME_ID = 0;
        public const int DOWN_FRAME_ID = 1;
        public const int RIGHT_FRAME_ID = 2;
        public const int UP_FRAME_ID = 3;

        public const string DOWN_NOTE = "blue instance";
        public const string DOWN_HIT = "down confirm instance";
        public const string DOWN_PRESS = "down press instance";

        public const string UP_NOTE = "green instance";
        public const string UP_HIT = "up confirm instance";
        public const string UP_PRESS = "up press instance";

        public const string LEFT_NOTE = "purple instance";
        public const string LEFT_HIT = "left confirm instance";
        public const string LEFT_PRESS = "left press instance";

        public const string RIGHT_NOTE = "red instance";
        public const string RIGHT_HIT = "right confirm instance";
        public const string RIGHT_PRESS = "right press instance";

        public const string DOWN_BAR = "blue hold piece instance";
        public const string DOWN_BAR_END = "blue hold end instance";
        public const string LEFT_BAR = "purple hold piece instance";
        public const string LEFT_BAR_END = "pruple end hold instance";
        public const string RIGHT_BAR = "red hold piece instance";
        public const string RIGHT_BAR_END = "red hold end instance";
        public const string UP_BAR = "green hold piece instance";
        public const string UP_BAR_END = "green hold end instance";
    }

    static class Speaker 
    {
        public const string GirlfriendAssets = Character.GirlfriendAssets;
        public const string GirlfriendChristmas = "gfChristmas.xml";
        public const string GirlfriendCarAssets = "bfCar.xml";
        public const string GirlfriendTankmanAssets = "gfTankmen.xml";
        public const string GirlfriendPixelAssets = "gfPixel.xml";
        public const string PicoAssets = "picoSpeaker.xml";

        public static readonly ReadOnlyDictionary<string, string> AssetsMap = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string> {
                { "gf" , GirlfriendAssets },
                { "gf-car", GirlfriendAssets },
                { "gf-pixel" , GirlfriendPixelAssets },
                { "gf-tankman", GirlfriendTankmanAssets },
                { "pico", PicoAssets }
            });
    }
}
