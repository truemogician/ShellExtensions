using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace ExplorerRestarter;

internal static class Resource {
	internal static string Root { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Assets");

	internal static Image Restart { get; } = Resize(new Bitmap(Path.Combine(Root, "Restart.png")), 24, 24);

	private static Image Resize(Image src, int width, int height) {
		var destRect = new Rectangle(0, 0, width, height);
		var destImage = new Bitmap(width, height);

		destImage.SetResolution(src.HorizontalResolution, src.VerticalResolution);

		using var graphics = Graphics.FromImage(destImage);
		graphics.CompositingMode = CompositingMode.SourceCopy;
		graphics.CompositingQuality = CompositingQuality.HighQuality;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.SmoothingMode = SmoothingMode.HighQuality;
		graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

		using var wrapMode = new ImageAttributes();
		wrapMode.SetWrapMode(WrapMode.TileFlipXY);
		graphics.DrawImage(src, destRect, 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, wrapMode);

		return destImage;
	}
}