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
        void SetCharacterPosition(TiledSpriteAtlas2D Player1, TiledSpriteAtlas2D Player2, TiledSpriteAtlas2D Speaker);
    }
}
