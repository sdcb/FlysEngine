using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DWrite = SharpDX.DirectWrite;

namespace FlysEngine.Managers
{
    public class TextFormatManager : IDisposable
    {
        private readonly Dictionary<string, DWrite.TextFormat> formatMap = new Dictionary<string, DWrite.TextFormat>();
        private readonly DWrite.Factory _factory;

        public TextFormatManager(DWrite.Factory factory)
        {
            _factory = factory;
        }

        public DWrite.TextFormat this[float fontSize, string fontFamilyName = "Consolas"]
        {
            get
            {
                var key = $"{fontFamilyName}:{fontSize}";
                if (!formatMap.ContainsKey(key))
                {
                    formatMap[key] = new DWrite.TextFormat(_factory, fontFamilyName, fontSize);
                }

                return formatMap[key];
            }
        }

        public void Dispose()
        {
            foreach (var format in formatMap.Values)
            {
                format.Dispose();
            }
        }
    }
}
