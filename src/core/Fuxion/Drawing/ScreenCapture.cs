using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
namespace Fuxion.Drawing
{
	public class ScreenCapture
	{
		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetDesktopWindow();

		[StructLayout(LayoutKind.Sequential)]
		private struct Rect
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public static implicit operator Point(POINT point)
			{
				return new Point(point.X, point.Y);
			}
		}
		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out POINT lpPoint);


		public static Image? CaptureDesktop(bool drawMousePoint = false)
		{
			return CaptureWindow(GetDesktopWindow(), drawMousePoint);
		}

		public static Bitmap? CaptureActiveWindow(bool drawMousePoint = false)
		{
			return CaptureWindow(GetForegroundWindow(), drawMousePoint);
		}

		public static Bitmap? CaptureWindow(IntPtr handle, bool drawMousePoint = false)
		{
			if (handle == IntPtr.Zero) return null;
			var rect = new Rect();
			GetWindowRect(handle, ref rect);
			var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
			var bitmap = new Bitmap(bounds.Width, bounds.Height);

			using (var graphics = Graphics.FromImage(bitmap))
			{
				graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
				if (drawMousePoint)
				{
					int cursorCrosshairSize = 12;
					GetCursorPos(out POINT po);
					graphics.FillEllipse(Brushes.Red, new Rectangle(po.X - bounds.Left - cursorCrosshairSize/2, po.Y - bounds.Top - cursorCrosshairSize/2, cursorCrosshairSize, cursorCrosshairSize));
					// Horizontal line
					graphics.DrawLine(Pens.White,
						new Point(
							po.X - bounds.Left - cursorCrosshairSize / 2,
							po.Y - bounds.Top),
						new Point(
							po.X - bounds.Left + cursorCrosshairSize / 2,
							po.Y - bounds.Top));
					// Vertical line
					graphics.DrawLine(Pens.White,
						new Point(
							po.X - bounds.Left,
							po.Y - bounds.Top - cursorCrosshairSize / 2),
						new Point(
							po.X - bounds.Left,
							po.Y - bounds.Top + cursorCrosshairSize / 2));
					graphics.FillEllipse(Brushes.White, new Rectangle(po.X - bounds.Left - cursorCrosshairSize / 6, po.Y - bounds.Top - cursorCrosshairSize / 6, cursorCrosshairSize / 3, cursorCrosshairSize / 3));
				}
			}
			return bitmap;
		}
	}
}
