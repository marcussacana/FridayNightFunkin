using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbis
{
    internal class CharacterAnim
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
        }
    }
}
