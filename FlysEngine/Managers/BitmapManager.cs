using System;
using System.Collections.Generic;
using Vortice.Direct2D1;
using Vortice.WIC;

namespace FlysEngine.Managers;

/// <summary>
/// Manages loading and unloading bitmaps from files as ID2D1Bitmap1 objects.
/// </summary>
public class BitmapManager : IDisposable
{
    private readonly IWICImagingFactory _imagingFactory;
    private readonly Dictionary<string, ID2D1Bitmap1> _bmps = new();
    private ID2D1DeviceContext _renderTarget;

    /// <summary>
    /// Initializes a new instance of the BitmapManager class.
    /// </summary>
    /// <param name="imagingFactory">The imaging factory to use when loading bitmaps.</param>
    public BitmapManager(IWICImagingFactory imagingFactory)
    {
        _imagingFactory = imagingFactory;
    }

    /// <summary>
    /// Sets the device context to use when creating bitmaps from images.
    /// </summary>
    /// <param name="renderTarget">The device context to use.</param>
    public void SetRenderTarget(ID2D1DeviceContext renderTarget)
    {
        _renderTarget = renderTarget;
    }

    /// <summary>
    /// Gets the bitmap with the given filename.
    /// </summary>
    /// <param name="filename">The name of the file to load the bitmap from.</param>
    /// <returns>The loaded bitmap.</returns>
    public ID2D1Bitmap1 this[string filename]
    {
        get
        {
            LoadBitmap(filename);
            return _bmps[filename];
        }
    }

    private void LoadBitmap(string filename)
    {
        if (!_bmps.ContainsKey(filename))
        {
            _bmps[filename] = CreateD2dBitmap(
                _imagingFactory,
                filename,
                _renderTarget);
        }
    }

    /// <summary>
    /// Unloads the bitmap with the given filename.
    /// </summary>
    /// <param name="filename">The name of the file to unload the bitmap from.</param>
    /// <returns>true if the bitmap was unloaded, false if it was not loaded.</returns>
    public bool UnloadBitmap(string filename)
    {
        if (_bmps.ContainsKey(filename))
        {
            ID2D1Bitmap1 bmp = _bmps[filename];
            bmp.Dispose();
            _bmps.Remove(filename);
            return true;
        }
        return false;
    }

    private static ID2D1Bitmap1 CreateD2dBitmap(
        IWICImagingFactory imagingFactory,
        string filename,
        ID2D1DeviceContext renderTarget)
    {
        using IWICBitmapDecoder decoder = imagingFactory.CreateDecoderFromFileName(filename);
        using IWICBitmapFrameDecode frame = decoder.GetFrame(0);

        using IWICFormatConverter converter = imagingFactory.CreateFormatConverter();
        converter.Initialize(frame, PixelFormat.Format32bppPBGRA, BitmapDitherType.None, null, 0, BitmapPaletteType.Custom);
        return renderTarget.CreateBitmapFromWicBitmap(converter, null);
    }

    /// <summary>
    /// Releases the device resources used by the bitmaps.
    /// </summary>
    public void ReleaseDeviceResources()
    {
        foreach (KeyValuePair<string, ID2D1Bitmap1> kv in _bmps)
        {
            kv.Value.Dispose();
        }
        _bmps.Clear();
    }

    /// <summary>
    /// Disposes the BitmapManager object and releases all device resources used by the bitmaps.
    /// </summary>
    public void Dispose()
    {
        ReleaseDeviceResources();
        GC.SuppressFinalize(this);
    }
}
