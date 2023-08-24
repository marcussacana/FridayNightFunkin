using OrbisGL.GL2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbis.Interfaces
{
    internal interface IBG : ILoadable
    {
        public event EventHandler<NewStatusEvent> OnMapStatusChanged;
        void SetCharacterPosition(SpriteAtlas2D Player1, SpriteAtlas2D Player2, SpriteAtlas2D Speaker);
    }
}
