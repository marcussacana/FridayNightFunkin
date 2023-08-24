using OrbisGL.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbis.Interfaces
{
    public interface ILoadable : IRenderable
    {
        bool Loaded { get; }

        int TotalProgress { get; }
        void Load(Action<int> OnProgressChanged);
    }
}
