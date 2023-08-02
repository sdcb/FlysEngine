using System;
using System.Collections.Generic;
using Direct2D1 = Vortice.Direct2D1;
using DWrite = Vortice.DirectWrite;

namespace FlysEngine.Managers;

/// <summary>
/// Provides functionality to manage and render text using DirectWrite.
/// </summary>
public class TextLayoutManager : IDisposable
{
    private readonly DWrite.IDWriteFactory _dwriteFactory;
    private readonly TextFormatManager _textManager;
    private readonly Dictionary<string, DWrite.IDWriteTextLayout> _bmps = new();

    /// <summary>
    /// Initializes a new instance of the TextLayoutManager class.
    /// </summary>
    /// <param name="dwriteFactory"> The DWrite factory to use to create text layouts.</param>
    /// <param name="textManager">The text format manager.</param>
    public TextLayoutManager(DWrite.IDWriteFactory dwriteFactory, TextFormatManager textManager)
    {
        _dwriteFactory = dwriteFactory;
        _textManager = textManager;
    }

    /// <summary>
    /// Gets the cached DirectWrite text layout associated with the given text parameters.
    /// </summary>
    /// <param name="text">The string of text to render with the layout.</param>
    /// <param name="fontSize">The font size to use for the text layout.</param>
    /// <param name="fontFamilyName">The optional name of the font family to use for the text layout (default: Consolas).</param>
    /// <returns>The DirectWrite text layout associated with the given text parameters.</returns>
    public DWrite.IDWriteTextLayout this[string text, float fontSize, string fontFamilyName = "Consolas"]
    {
        get
        {
            string key = $"{text}:{fontSize}";
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

    /// <summary>
    /// Removes the cached DirectWrite text layout associated with the given text parameters.
    /// </summary>
    /// <param name="text">The string of text associated with the layout to remove.</param>
    /// <param name="fontSize">The font size associated with the layout to remove.</param>
    public void Remove(string text, float fontSize)
    {
        string key = $"{text}:{fontSize}";
        if (_bmps.ContainsKey(key))
        {
            DWrite.IDWriteTextLayout val = _bmps[key];
            val.Dispose();
            _bmps.Remove(key);
        }
    }

    /// <summary>
    /// Frees the DirectWrite resources used by the text layout manager.
    /// </summary>
    public void ReleaseDeviceResources()
    {
        foreach (KeyValuePair<string, DWrite.IDWriteTextLayout> kv in _bmps)
        {
            kv.Value.Dispose();
        }
        _bmps.Clear();
    }

    /// <summary>
    /// Disposes the text layout manager and frees all DirectWrite resources used by it.
    /// </summary>
    public void Dispose()
    {
        ReleaseDeviceResources();
        GC.SuppressFinalize(this);
    }
}
