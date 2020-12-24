using System;
using System.Collections.Generic;
using DWrite = SharpDX.DirectWrite;

namespace FlysEngine.Managers
{
    public class TextFormatManager : IDisposable
    {
        private readonly Dictionary<string, DWrite.TextFormat> _formatMap = new Dictionary<string, DWrite.TextFormat>();
        private readonly Dictionary<string, DWrite.TextFormat> _sizeDependentFormatMap = new Dictionary<string, DWrite.TextFormat>();
        private readonly DWrite.Factory _factory;

        public TextFormatManager(DWrite.Factory factory)
        {
            _factory = factory;
        }

        public DWrite.TextFormat this[float fontSize, string fontFamilyName = "Consolas", bool isSizeDependent = false]
        {
            get
            {
                var key = $"{fontFamilyName}:{fontSize}";
                Dictionary<string, DWrite.TextFormat> map = isSizeDependent ? _sizeDependentFormatMap : _formatMap;

                if (!_formatMap.ContainsKey(key))
                {
                    map[key] = new DWrite.TextFormat(_factory, fontFamilyName, fontSize);
                }

                return map[key];
            }
        }

        public int Count => _formatMap.Count + _sizeDependentFormatMap.Count;
        public int SizeDependentCount => _sizeDependentFormatMap.Count;

        public void ReleaseResources(bool includeSizeDependent = false, bool includeSizeIndependent = false)
        {
            if (includeSizeDependent)
            {
                foreach (var format in _formatMap.Values)
                {
                    format.Dispose();
                }
                _formatMap.Clear();
            }

            if (includeSizeIndependent)
            {
                foreach (var format in _sizeDependentFormatMap.Values)
                {
                    format.Dispose();
                }
                _sizeDependentFormatMap.Clear();
            }
        }

        public void Dispose()
        {
            ReleaseResources(includeSizeDependent: true, includeSizeIndependent: true);
        }
    }
}
