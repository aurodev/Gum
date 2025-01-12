﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RenderingLibrary.Graphics
{
    public interface IRenderableIpso : IRenderable, IPositionedSizedObject, IVisible
    {
        bool ClipsChildren { get;  }
        IRenderableIpso Parent { get; set; }
        ObservableCollection<IRenderableIpso> Children { get; }
        ColorOperation ColorOperation { get; }

        void SetParentDirect(IRenderableIpso newParent);

    }
}
