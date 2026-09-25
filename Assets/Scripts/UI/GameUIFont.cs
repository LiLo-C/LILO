using UnityEngine;

namespace Lilo.UI
{
    /// <summary>Loads the shared game font for UI elements created at runtime.</summary>
    public static class GameUIFont
    {
        private static Font _font;

        public static Font Get()
        {
            if (_font == null)
                _font = Resources.Load<Font>("American Typewriter Regular");
            return _font != null ? _font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
