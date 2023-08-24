using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Orbis
{
    internal class CharacterAnim
    {
        public Vector2 SHAKING_OFFSET { get; private set; } = Vector2.Zero;
        public string SHAKING { get; private set; }
        public Vector2 DANCING_OFFSET { get; private set; } = Vector2.Zero;
        public string DANCING { get; private set; }

        public Vector2 UP_OFFSET { get; private set; } = Vector2.Zero;
        public string UP { get; private set; }
        public Vector2 DOWN_OFFSET { get; private set; } = Vector2.Zero;
        public string DOWN { get; private set; }
        public Vector2 LEFT_OFFSET { get; private set; } = Vector2.Zero;
        public string LEFT { get; private set; }
        public Vector2 RIGHT_OFFSET { get; private set; } = Vector2.Zero;
        public string RIGHT { get; private set; }

        public Vector2 UP_MISS_OFFSET { get; private set; } = Vector2.Zero;
        public string UP_MISS { get; private set; }
        public Vector2 DOWN_MISS_OFFSET { get; private set; } = Vector2.Zero;
        public string DOWN_MISS { get; private set; }
        public Vector2 LEFT_MISS_OFFSET { get; private set; } = Vector2.Zero;
        public string LEFT_MISS { get; private set; }
        public Vector2 RIGHT_MISS_OFFSET { get; private set; } = Vector2.Zero;
        public string RIGHT_MISS { get; private set; }

        public Vector2 DIES_OFFSET { get; private set; } = Vector2.Zero;
        public string DIES { get; private set; }
        public Vector2 DEAD_OFFSET { get; private set; } = Vector2.Zero;
        public string DEAD { get; private set; }
        public Vector2 DEAD_CONFIRM_OFFSET { get; private set; } = Vector2.Zero;
        public string DEAD_CONFIRM { get; private set; }


        public Vector2 PRE_ATTACK_OFFSET { get; private set; } = Vector2.Zero;
        public string PRE_ATTACK { get; private set; }

        public Vector2 ATTACKING_OFFSET { get; private set; } = Vector2.Zero;
        public string ATTACKING { get; private set; }

        public Vector2 DOGGLE_OFFSET { get; private set; } = Vector2.Zero;
        public string DOGGLE { get; set; }

        public Vector2 HIT_OFFSET { get; private set; } = Vector2.Zero;
        public string HIT { get; private set; }

        public Vector2 HEY_OFFSET { get; private set; } = Vector2.Zero;
        public string HEY { get; private set; }

        public Vector2 HAIR_LANDING_OFFSET { get; private set; } = Vector2.Zero;
        public string HAIR_LANDING { get; private set; }
        public Vector2 HAIR_BLOWING_OFFSET { get; private set; } = Vector2.Zero;
        public string HAIR_BLOWING { get; private set; }


        Dictionary<string, Vector2> OffsetMap = new Dictionary<string, Vector2>();

        public CharacterAnim(string Character)
        {
            switch (Character) {
                case "bf":
                    SHAKING = "BF idle shaking";
                    DANCING = "BF idle dance";
                    DEAD = "BF Dead Loop";
                    DEAD_CONFIRM = "BF Dead confirm";
                    HEY = "BF HEY!!";
                    DOWN = "BF NOTE DOWN";
                    DOWN_MISS = "BF NOTE DOWN MISS";
                    LEFT = "BF NOTE LEFT";
                    LEFT_MISS = "BF NOTE LEFT MISS";
                    RIGHT = "BF NOTE RIGHT";
                    RIGHT_MISS = "BF NOTE RIGHT MISS";
                    UP = "BF NOTE UP";
                    UP_MISS = "BF NOTE UP MISS";
                    DIES = "BF dies";
                    HIT = "BF hit";
                    PRE_ATTACK = "bf pre attack";
                    ATTACKING = "boyfriend attack";
                    DOGGLE = "boyfriend dodge";
                    break;
                case "dad":
                    DOWN = "Dad Sing Note DOWN";
                    LEFT = "Dad Sing Note LEFT";
                    RIGHT = "Dad Sing Note RIGHT";
                    UP = "Dad Sing Note UP";
                    DANCING = "Dad idle dance";
                    break;
                case "spooky":
                    LEFT = "note sing left";
                    DOWN = "spooky DOWN note";
                    RIGHT = "spooky sing right";
                    UP = "spooky UP NOTE";
                    DANCING = "spooky dance idle";
                    break;
                case "pico":
                    DANCING = "Pico Idle Dance";
                    DOWN = "Pico Down Note";
                    DOWN_MISS = "Pico Down Note MISS";
                    LEFT = "Pico NOTE LEFT";
                    LEFT_MISS = "Pico NOTE LEFT miss";
                    RIGHT = "Pico Note Right";
                    RIGHT_MISS = "Pico Note Right Miss";
                    UP = "pico Up note";
                    UP_MISS = "pico Up note miss";
                    break;
                case "gf":
                    HEY = "GF Cheer";
                    DANCING = "GF Dancing Beat";
                    DOWN = "GF Down Note";
                    RIGHT = "GF Right Note";
                    UP = "GF Up Note";
                    LEFT = "GF left note";

                    HAIR_LANDING = "GF Dancing Beat Hair Landing";
                    HAIR_BLOWING = "GF Dancing Beat Hair blowing";

                    //wrong context but let's make the thing simpler
                    HIT = "GF FEAR ";
                    DIES = DEAD = UP_MISS = DOWN_MISS = LEFT_MISS = RIGHT_MISS = "gf sad";
                    break;
                case "mom":
                    DANCING = "Mom Idle";
                    DOWN = "MOM DOWN POSE";
                    LEFT = "Mom Left Pose";
                    RIGHT = "Mom Pose Left";//WTF
                    UP = "Mom Up Pose";
                    break;
            }

            string[] OffsetList;
            using (var OffsetStream = Util.CopyFileToMemory($"{Character}Offsets.txt"))
            {
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


                switch (Info[0])
                {
                    case "hey":
                    case "cheer":
                        HEY_OFFSET = Vector;
                        OffsetMap[HEY ?? ""] = Vector;
                        break;
                    case "sad":
                        DIES_OFFSET = DEAD_OFFSET = UP_MISS_OFFSET = DOWN_MISS_OFFSET = LEFT_MISS_OFFSET = RIGHT_MISS_OFFSET = Vector;
                        OffsetMap[DIES ?? ""] = OffsetMap[DEAD ?? ""] = OffsetMap[UP_MISS ?? ""] = OffsetMap[DOWN_MISS ?? ""] = OffsetMap[LEFT_MISS ?? ""] = OffsetMap[RIGHT_MISS ?? ""] = Vector;
                        break;
                    case "idle":
                    case "danceRight":
                    case "danceLeft":
                        DANCING_OFFSET = Vector;
                        OffsetMap[DANCING ?? ""] = Vector;
                        break;
                    case "singUPmiss":
                        UP_MISS_OFFSET = Vector;
                        OffsetMap[UP_MISS ?? ""] = Vector;
                        break;
                    case "singRIGHTmiss":
                        RIGHT_MISS_OFFSET = Vector;
                        OffsetMap[RIGHT_MISS ?? ""] = Vector;
                        break;
                    case "singLEFTmiss":
                        LEFT_MISS_OFFSET = Vector;
                        OffsetMap[LEFT_MISS ?? ""] = Vector;
                        break;
                    case "singDOWNmiss":
                        DOWN_MISS_OFFSET = Vector;
                        OffsetMap[DOWN_MISS ?? ""] = Vector;
                        break;
                    case "singUP":
                        UP_OFFSET = Vector;
                        OffsetMap[UP ?? ""] = Vector;
                        break;
                    case "singRIGHT":
                        RIGHT_OFFSET = Vector;
                        OffsetMap[RIGHT ?? ""] = Vector;
                        break;
                    case "singLEFT":
                        LEFT_OFFSET = Vector;
                        OffsetMap[LEFT ?? ""] = Vector;
                        break;
                    case "singDOWN":
                        DOWN_OFFSET = Vector;
                        OffsetMap[DOWN ?? ""] = Vector;
                        break;
                    case "hairBlow":
                        HAIR_BLOWING_OFFSET = Vector;
                        OffsetMap[HAIR_BLOWING ?? ""] = Vector;
                        break;
                    case "hairFall":
                        HAIR_LANDING_OFFSET = Vector;
                        OffsetMap[HAIR_LANDING ?? ""] = Vector;
                        break;
                    case "scared":
                        SHAKING_OFFSET = Vector;
                        OffsetMap[SHAKING ?? ""] = Vector;

                        if (Character != "bf")
                        {
                            HIT_OFFSET = Vector;
                            OffsetMap[HIT ?? ""] = Vector;
                        }

                        break;
                    case "firstDeath":
                        DIES_OFFSET = Vector;
                        OffsetMap[DIES ?? ""] = Vector;
                        break;
                    case "deathLoop":
                        DEAD_OFFSET = Vector;
                        OffsetMap[DEAD ?? ""] = Vector;
                        break;
                    case "deathConfirm":
                        DEAD_CONFIRM_OFFSET = Vector;
                        OffsetMap[DEAD_CONFIRM ?? ""] = Vector;
                        break;
                }
            }

        }

        public Vector2 GetAnimOffset(string Animation)
        {
            if (string.IsNullOrWhiteSpace(Animation))
                return Vector2.Zero;

            if (OffsetMap.TryGetValue(Animation, out Vector2 Offset))
                return Offset;

            return Vector2.Zero;
        }
    }
}
