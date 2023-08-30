using OrbisGL.GL2D;
using System;

namespace Orbis.Interfaces
{
    internal interface IBG : ILoadable
    {
        public event EventHandler<NewStatusEvent> OnMapStatusChanged;
        void SetCharacterPosition(TiledSpriteAtlas2D Player1, TiledSpriteAtlas2D Player2, TiledSpriteAtlas2D Speaker);
    }
}
