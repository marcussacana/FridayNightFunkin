using System.Collections.Generic;
using System.Dynamic;
using System.Numerics;
using System.Text;

namespace Orbis
{
    public class CharacterAnim
    {
        public string SHAKING { get; private set; }
        public string DANCING { get; private set; }
        public string UP { get; private set; }
        public string DOWN { get; private set; }
        public string LEFT { get; private set; }
        public string RIGHT { get; private set; }
        public string UP_MISS { get; private set; }
        public string DOWN_MISS { get; private set; }
        public string LEFT_MISS { get; private set; }
        public string RIGHT_MISS { get; private set; }
        public string DIES { get; private set; }
        public string DEAD { get; private set; }
        public string DEAD_CONFIRM { get; private set; }
        public string PRE_ATTACK { get; private set; }
        public string ATTACKING { get; private set; }
        public string DOGGLE { get; set; }
        public string HIT { get; private set; }
        public string HEY { get; private set; }
        public string HAIR_LANDING { get; private set; }
        public string HAIR_BLOWING { get; private set; }

        public bool Mirror { get; set; }

        Dictionary<string, Vector2> OffsetMap = new Dictionary<string, Vector2>();

        public CharacterAnim(string Character)
        {
            SetupAnimNames(ref Character, out bool Alt);
            SetupAnimOffsets(Character, Alt);
        }

        private void SetupAnimNames(ref string Character, out bool Alt)
        {
            Alt = false;

            switch (Character.ToLower())
            {
                case "bf-car":
                    SHAKING = DANCING = "BF idle dance";
                    DOWN = "BF NOTE DOWN";
                    DOWN_MISS = "BF NOTE DOWN MISS";
                    LEFT = "BF NOTE LEFT";
                    LEFT_MISS = "BF NOTE LEFT MISS";
                    RIGHT = "BF NOTE RIGHT";
                    RIGHT_MISS = "BF NOTE RIGHT MISS";
                    UP = "BF NOTE UP";
                    break;
                case "bf-christmas":
                    SHAKING = DANCING = "BF idle dance";
                    HEY = "BF HEY!!";
                    DOWN = "BF NOTE DOWN";
                    DOWN_MISS = "BF NOTE DOWN MISS";
                    LEFT = "BF NOTE LEFT";
                    LEFT_MISS = "BF NOTE LEFT MISS";
                    RIGHT = "BF NOTE RIGHT";
                    RIGHT_MISS = "BF NOTE RIGHT MISS";
                    UP = "BF NOTE UP";
                    UP_MISS = "BF NOTE UP MISS";
                    break;
                case "bf-pixel":
                    SHAKING = DANCING = "BF IDLE instance";
                    LEFT = "BF LEFT NOTE instance";
                    RIGHT = "BF RIGHT NOTE instance";
                    UP = "BF UP NOTE instance";
                    DOWN = "BF DOWN NOTE instance";
                    LEFT_MISS = "BF LEFT MISS instance";
                    RIGHT_MISS = "BF RIGHT MISS instance";
                    DOWN_MISS = "BF DOWN MISS instance";
                    UP_MISS = "BF UP MISS instance";
                    break;
                case "bf-pixel-dead":
                    DIES = "BF Dies pixel";
                    DEAD = "Retry Loop";
                    DEAD_CONFIRM = "RETRY CONFIRM";
                    break;
                case "bf-holding-gf":
                    DANCING = SHAKING = "BF idle dance w gf";
                    LEFT = "BF NOTE LEFT";
                    RIGHT = "BF NOTE RIGHT";
                    UP = "BF NOTE UP";
                    DOWN = "BF NOTE DOWN";

                    LEFT_MISS = "BF NOTE LEFT MISS";
                    RIGHT_MISS = "BF NOTE RIGHT MISS";
                    DOWN_MISS = "BF NOTE DOWN MISS";
                    UP_MISS = "BF NOTE UP MISS";

                    HIT = "BF catches GF";//not used but let's keep avaiable.

                    DIES = "BF Dies with GF";
                    DEAD = "BF Dead with GF Loop";
                    DEAD_CONFIRM = "RETRY confirm holding gf";
                    break;
                case "bf":
                    SHAKING = "BF idle shaking";
                    DANCING = "BF idle dance";
                    DEAD = "BF Dead Loop";
                    DEAD_CONFIRM = "BF Dead confirm";
                    HEY = "BF HEY!!";

                    LEFT = "BF NOTE LEFT";
                    LEFT_MISS = "BF NOTE LEFT MISS";
                    RIGHT = "BF NOTE RIGHT";
                    RIGHT_MISS = "BF NOTE RIGHT MISS";
                    UP = "BF NOTE UP";
                    UP_MISS = "BF NOTE UP MISS";
                    DOWN = "BF NOTE DOWN";
                    DOWN_MISS = "BF NOTE DOWN MISS";

                    DIES = "BF dies";
                    HIT = "BF hit";
                    PRE_ATTACK = "bf pre attack";
                    ATTACKING = "boyfriend attack";
                    DOGGLE = "boyfriend dodge";
                    break;
                case "dad":
                    LEFT = "Dad Sing Note LEFT";
                    RIGHT = "Dad Sing Note RIGHT";
                    UP = "Dad Sing Note UP";
                    DOWN = "Dad Sing Note DOWN";
                    DANCING = "Dad idle dance";
                    break;
                case "spooky":
                    LEFT = "note sing left";
                    RIGHT = "spooky sing right";
                    UP = "spooky UP NOTE";
                    DOWN = "spooky DOWN note";
                    DANCING = "spooky dance idle";
                    break;
                case "pico":
                    DANCING = "Pico Idle Dance";
                    LEFT = "Pico NOTE LEFT";
                    LEFT_MISS = "Pico NOTE LEFT miss";
                    RIGHT = "Pico Note Right";
                    RIGHT_MISS = "Pico Note Right Miss";
                    UP = "pico Up note";
                    UP_MISS = "pico Up note miss";
                    DOWN = "Pico Down Note";
                    DOWN_MISS = "Pico Down Note MISS";
                    break;
                case "gf":
                    HEY = "GF Cheer";
                    DANCING = "GF Dancing Beat";

                    LEFT = "GF left note";
                    RIGHT = "GF Right Note";
                    UP = "GF Up Note";
                    DOWN = "GF Down Note";

                    HAIR_LANDING = "GF Dancing Beat Hair Landing";
                    HAIR_BLOWING = "GF Dancing Beat Hair blowing";

                    //wrong context but let's make the thing simpler
                    HIT = "GF FEAR";
                    DIES = DEAD = UP_MISS = DOWN_MISS = LEFT_MISS = RIGHT_MISS = "gf sad";
                    break;
                case "gf-pixel":
                    HEY = DANCING = "GF IDLE";
                    break;
                case "mom-car":
                case "mom":
                    DANCING = "Mom Idle";
                    LEFT = "Mom Left Pose";
                    RIGHT = "Mom Pose Left";//WTF
                    UP = "Mom Up Pose";
                    DOWN = "MOM DOWN POSE";
                    break;
                case "parents-christmas":
                    Character = "parents-christmas";
                    SHAKING = DANCING = "Parent Christmas Idle";

                    LEFT = "Parent Left Note Dad";
                    RIGHT = "Parent Right Note Dad";
                    UP = "Parent Up Note Dad";
                    DOWN = "Parent Down Note Dad";
                    break;
                case "parents-christmas-alt":
                    Alt = true;
                    Character = "parents-christmas";
                    SHAKING = DANCING = "Parent Christmas Idle";

                    LEFT = "Parent Left Note Mom";
                    RIGHT = "Parent Right Note Mom";
                    UP = "Parent Up Note Mom";
                    DOWN = "Parent Down Note Mom";
                    break;
                case "monster":
                case "monster-christmas":
                    SHAKING = DANCING = "monster idle";
                    LEFT = "Monster left note";
                    RIGHT = "Monster Right note";
                    UP = "monster up note";
                    DOWN = "monster down";
                    break;
                case "senpai":
                    SHAKING = DANCING = "Senpai Idle instance";
                    LEFT = "SENPAI LEFT NOTE instance";
                    RIGHT = "SENPAI RIGHT NOTE instance";
                    UP = "SENPAI UP NOTE instance";
                    DOWN = "SENPAI DOWN NOTE instance";
                    break;
                case "senpai-angry":
                    SHAKING = DANCING = "Angry Senpai Idle instance";
                    LEFT = "Angry Senpai LEFT NOTE instance";
                    RIGHT = "Angry Senpai RIGHT NOTE instance";
                    UP = "Angry Senpai UP NOTE instance";
                    DOWN = "Angry Senpai DOWN NOTE instance";
                    break;
                case "tankman":
                    Alt = true;
                    SHAKING = DANCING = "Tankman Idle Dance instance";
                    HEY = "PRETTY GOOD tankman instance";
                    DOWN = "Tankman DOWN note instance";
                    LEFT = "Tankman Note Left instance";
                    RIGHT = "Tankman Right Note instance";
                    UP = "Tankman UP note instance";
                    DOWN_MISS = LEFT_MISS = RIGHT_MISS = UP_MISS = "TANKMAN UGH instance";
                    break;
                case "spirit":
                    SHAKING = DANCING = "idle spirit_";
                    DOWN = "spirit down_";
                    LEFT = "left_";
                    RIGHT = "right_";
                    UP = "up_";
                    break;
            }
        }

        private void SetupAnimOffsets(string Character, bool Alt)
        {
            string[] OffsetList;
            using (var OffsetStream = Util.CopyFileToMemory($"{Character}Offsets.txt"))
            {
                if (OffsetStream == null)
                    return;

                var OffsetData = OffsetStream.ToArray();
                OffsetList = Encoding.UTF8.GetString(OffsetData).Replace("\r\n", "\n").Trim().Split('\n');
            }

            foreach (var Line in OffsetList)
            {
                var Info = Line.Split(' ', '\t');
                var Vector = Vector2.Zero;

                if (Info.Length >= 3)
                {
                    Vector = new Vector2(float.Parse(Info[1]), float.Parse(Info[2]));
                }

                ParseOffset(Character, Info, Vector);
            }

            if (Alt)
            {
                foreach (var Line in OffsetList)
                {
                    var Info = Line.Split(' ', '\t');
                    var Vector = Vector2.Zero;

                    if (Info.Length >= 3)
                    {
                        Vector = new Vector2(float.Parse(Info[1]), float.Parse(Info[2]));
                    }

                    if (Info[0].EndsWith("-alt"))
                    {
                        Info[0] = Info[0].Substring(0, Info[0].IndexOf("-alt"));
                        ParseOffset(Character, Info, Vector);
                    }
                }
            }
        }

        private void ParseOffset(string Character, string[] Info, Vector2 Value)
        {
            switch (Info[0])
            {
                case "hey":
                case "cheer":
                    OffsetMap[HEY ?? ""] = Value;
                    break;
                case "sad":                    
                    OffsetMap[DIES ?? ""] = OffsetMap[DEAD ?? ""] = OffsetMap[UP_MISS ?? ""] = OffsetMap[DOWN_MISS ?? ""] = OffsetMap[LEFT_MISS ?? ""] = OffsetMap[RIGHT_MISS ?? ""] = Value;
                    break;
                case "idle":
                case "danceRight":
                case "danceLeft":
                    OffsetMap[DANCING ?? ""] = Value;
                    break;
                case "singUPmiss":
                    OffsetMap[UP_MISS ?? ""] = Value;
                    break;
                case "singRIGHTmiss":
                    OffsetMap[RIGHT_MISS ?? ""] = Value;
                    break;
                case "singLEFTmiss":
                    OffsetMap[LEFT_MISS ?? ""] = Value;
                    break;
                case "singDOWNmiss":
                    OffsetMap[DOWN_MISS ?? ""] = Value;
                    break;
                case "singUP":
                    OffsetMap[UP ?? ""] = Value;
                    break;
                case "singRIGHT":
                    OffsetMap[RIGHT ?? ""] = Value;
                    break;
                case "singLEFT":
                    OffsetMap[LEFT ?? ""] = Value;
                    break;
                case "singDOWN":
                    OffsetMap[DOWN ?? ""] = Value;
                    break;
                case "hairBlow":
                    OffsetMap[HAIR_BLOWING ?? ""] = Value;
                    break;
                case "hairFall":
                    OffsetMap[HAIR_LANDING ?? ""] = Value;
                    break;
                case "scared":
                    OffsetMap[SHAKING ?? ""] = Value;

                    if (Character != "bf")                    
                        OffsetMap[HIT ?? ""] = Value;

                    break;
                case "firstDeath":
                    OffsetMap[DIES ?? ""] = Value;
                    break;
                case "deathLoop":
                    OffsetMap[DEAD ?? ""] = Value;
                    break;
                case "deathConfirm":
                    OffsetMap[DEAD_CONFIRM ?? ""] = Value;
                    break;
            }
        }

        public void CopyAnimFrom(CharacterAnim Source)
        {
            foreach (var Offset in Source.OffsetMap)
            {
                OffsetMap[Offset.Key] = Offset.Value;
            }

            SHAKING = Source.SHAKING;
            DANCING = Source.DANCING;
            UP = Source.UP;
            DOWN = Source.DOWN;
            LEFT = Source.LEFT;
            RIGHT = Source.RIGHT;
            UP_MISS = Source.UP_MISS;
            DOWN_MISS = Source.DOWN_MISS;
            LEFT_MISS = Source.LEFT_MISS;
            RIGHT_MISS = Source.RIGHT_MISS;
            DIES = Source.DIES;
            DEAD = Source.DEAD;
            DEAD_CONFIRM = Source.DEAD_CONFIRM;


            PRE_ATTACK = Source.PRE_ATTACK;

            ATTACKING = Source.ATTACKING;

            DOGGLE = Source.DOGGLE;

            HIT = Source.HIT;

            HEY = Source.HEY;

            HAIR_LANDING = Source.HAIR_LANDING;
            HAIR_BLOWING = Source.HAIR_BLOWING;
        }

        public void AddOffsetAlias(string OriAnimation, string TargetAnimation)
        {
            if (OffsetMap.TryGetValue(OriAnimation, out Vector2 Offset))
                OffsetMap[TargetAnimation] = Offset;
            else
                throw new KeyNotFoundException(OriAnimation);
        }

        public Vector2 GetAnimOffset(string Animation)
        {
            if (string.IsNullOrWhiteSpace(Animation))
                return Vector2.Zero;

            if (OffsetMap.TryGetValue(Animation, out Vector2 Offset))
                return Mirror ? (Offset * new Vector2(-1, 1)) : Offset;

            return Vector2.Zero;
        }
    }
}
