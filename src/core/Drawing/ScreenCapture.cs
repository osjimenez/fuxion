using System.Drawing;
using System.Runtime.InteropServices;

namespace Fuxion.Drawing;

public class ScreenCapture
{
	[DllImport("user32.dll")]
	static extern IntPtr GetForegroundWindow();
	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetDesktopWindow();
	[DllImport("user32.dll")]
	static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
	[DllImport("user32.dll")]
	public static extern bool GetCursorPos(out POINT lpPoint);
	public static Image?  CaptureDesktop(bool      drawMousePoint = false) => CaptureWindow(GetDesktopWindow(),    drawMousePoint);
	public static Bitmap? CaptureActiveWindow(bool drawMousePoint = false) => CaptureWindow(GetForegroundWindow(), drawMousePoint);
	public static Bitmap? CaptureWindow(IntPtr handle, bool drawMousePoint = false)
	{
		if (handle == IntPtr.Zero) return null;
		var rect = new Rect();
		GetWindowRect(handle, ref rect);
		var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
		var bitmap = new Bitmap(bounds.Width, bounds.Height);
		using (var graphics = Graphics.FromImage(bitmap))
		{
			graphics.CopyFromScreen(new(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
			if (drawMousePoint)
			{
				var cursorCrosshairSize = 12;
				GetCursorPos(out var po);
				graphics.FillEllipse(Brushes.Red, new(po.X - bounds.Left - cursorCrosshairSize / 2, po.Y - bounds.Top - cursorCrosshairSize / 2, cursorCrosshairSize, cursorCrosshairSize));
				// Horizontal line
				graphics.DrawLine(Pens.White, new(po.X - bounds.Left - cursorCrosshairSize / 2, po.Y - bounds.Top), new(po.X - bounds.Left + cursorCrosshairSize / 2, po.Y - bounds.Top));
				// Vertical line
				graphics.DrawLine(Pens.White, new(po.X       - bounds.Left, po.Y - bounds.Top - cursorCrosshairSize / 2), new(po.X - bounds.Left, po.Y - bounds.Top + cursorCrosshairSize / 2));
				graphics.FillEllipse(Brushes.White, new(po.X - bounds.Left       - cursorCrosshairSize / 6, po.Y - bounds.Top - cursorCrosshairSize / 6, cursorCrosshairSize / 3, cursorCrosshairSize / 3));
			}
		}
		return bitmap;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct Rect
	{
		public readonly int Left;
		public readonly int Top;
		public readonly int Right;
		public readonly int Bottom;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public                          int X;
		public                          int Y;
		public static implicit operator Point(POINT point) => new(point.X, point.Y);
	}
}