using System;
using System.Collections.Generic;
using Direct2D1 = Vortice.Direct2D1;
using DWrite = Vortice.DirectWrite;

namespace FlysEngine.Managers;

public class TextLayoutManager : IDisposable
{
    private readonly DWrite.IDWriteFactory _dwriteFactory;
    private readonly TextFormatManager _textManager;
    private readonly Dictionary<string, DWrite.IDWriteTextLayout> _bmps = new();
    private Direct2D1.ID2D1DeviceContext _renderTarget;

    public TextLayoutManager(DWrite.IDWriteFactory dwriteFactory, TextFormatManager textManager)
    {
        _dwriteFactory = dwriteFactory;
        _textManager = textManager;
    }

    public void SetRenderTarget(Direct2D1.ID2D1DeviceContext renderTarget)
    {
        _renderTarget = renderTarget;
    }

    public DWrite.IDWriteTextLayout this[string text, float fontSize, string fontFamilyName = "Consolas"]
    {
        get
        {
            var key = $"{text}:{fontSize}";
            if (!_bmps.ContainsKey(key))
            {
                _bmps[key] = _dwriteFactory.CreateTextLayout(
                    text,
                    _textManager[fontSize, fontFamilyName],
                    float.MaxValue, fontSize * 2.0f);
            }

            return _bmps[key];
        }
    }

    public void Remove(string text, float fontSize)
    {
        var key = $"{text}:{fontSize}";
        if (_bmps.ContainsKey(key))
        {
            var val = _bmps[key];
            val.Dispose();
            _bmps.Remove(key);
        }
    }

    public void ReleaseDeviceResources()
    {
        foreach (var kv in _bmps)
        {
            kv.Value.Dispose();
        }
        _bmps.Clear();
    }

    public void Dispose()
    {
        ReleaseDeviceResources();
    }
}
