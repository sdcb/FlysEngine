using System;
using System.Collections.Generic;
using DWrite = Vortice.DirectWrite;

namespace FlysEngine.Managers;

/// <summary>
/// Manages a collection of DirectWrite IDWriteTextFormat objects used for rendering text.
/// </summary>
public class TextFormatManager : IDisposable
{
    private readonly Dictionary<string, DWrite.IDWriteTextFormat> _formatMap = new();
    private readonly Dictionary<string, DWrite.IDWriteTextFormat> _sizeDependentFormatMap = new();
    private readonly DWrite.IDWriteFactory _factory;

    /// <summary>
    /// Creates a TextFormatManager object with the specified DirectWrite IDWriteFactory.
    /// </summary>
    /// <param name="factory">The DirectWrite IDWriteFactory to use when creating IDWriteTextFormat objects.</param>
    public TextFormatManager(DWrite.IDWriteFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Gets the DirectWrite IDWriteTextFormat object for the specified font size and family name.
    /// </summary>
    /// <param name="fontSize">The desired font size.</param>
    /// <param name="fontFamilyName">The desired font family name. Defaults to "Consolas".</param>
    /// <param name="isSizeDependent">Indicates whether the format should be size dependent (true) or size independent (false). Defaults to false.</param>
    /// <returns>The DirectWrite IDWriteTextFormat object</returns>
    public DWrite.IDWriteTextFormat this[float fontSize, string fontFamilyName = "Consolas", bool isSizeDependent = false]
    {
        get
        {
            string key = $"{fontFamilyName}:{fontSize}";
            Dictionary<string, DWrite.IDWriteTextFormat> map = isSizeDependent ? _sizeDependentFormatMap : _formatMap;

            if (!_formatMap.ContainsKey(key))
            {
                map[key] = _factory.CreateTextFormat(fontFamilyName, fontSize);
            }

            return map[key];
        }
    }

    /// <summary>
    /// The number of DirectWrite IDWriteTextFormat objects managed by this TextFormatManager object.
    /// </summary>
    public int Count => _formatMap.Count + _sizeDependentFormatMap.Count;

    /// <summary>
    /// The number of size dependent DirectWrite IDWriteTextFormat objects managed by this TextFormatManager object.
    /// </summary>
    public int SizeDependentCount => _sizeDependentFormatMap.Count;

    /// <summary>
    /// Releases the resources held by this TextFormatManager object.
    /// </summary>
    /// <param name="includeSizeDependent">Indicates whether to release the size dependent DirectWrite IDWriteTextFormat objects. Defaults to false.</param>
    /// <param name="includeSizeIndependent">Indicates whether to release the size independent DirectWrite IDWriteTextFormat objects. Defaults to false.</param>
    public void ReleaseResources(bool includeSizeDependent = false, bool includeSizeIndependent = false)
    {
        if (includeSizeDependent)
        {
            foreach (DWrite.IDWriteTextFormat format in _formatMap.Values)
            {
                format.Dispose();
            }
            _formatMap.Clear();
        }

        if (includeSizeIndependent)
        {
            foreach (DWrite.IDWriteTextFormat format in _sizeDependentFormatMap.Values)
            {
                format.Dispose();
            }
            _sizeDependentFormatMap.Clear();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ReleaseResources(includeSizeDependent: true, includeSizeIndependent: true);
        GC.SuppressFinalize(this);
    }
}
