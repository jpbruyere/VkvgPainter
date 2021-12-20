// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using vkvg;
using PointD = Drawing2D.PointD;
using RectangleD = Drawing2D.RectangleD;
using Gradient = VkvgPainter.Gradient;

namespace VkvgPainter
{
	public class Gradient : Shape
	{
		public Gradient(PointD position) : base(position)	{}

		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			PointD p0 = Points[0];
			PointD p1 = mouse.HasValue ? mouse.Value : Points[1];

			double x,y,w,h;
			if (p0.X < p1.X) {
				x = p0.X;
				w = p1.X - p0.X;
			} else {
				x = p1.X;
				w = p0.X - p1.X;
			}
			if (p0.Y < p1.Y) {
				y = p0.Y;
				h = p1.Y - p0.Y;
			} else {
				y = p1.Y;
				h = p0.Y - p1.Y;
			}
			if (w > 0 && h > 0) {
				RectangleD r = new RectangleD (x,y,w,h);
				r.Inflate ((float)LineWidth/-2.0f);
				ctx.Rectangle (r.X,r.Y,r.Width,r.Height);
			}
		}
	}
}
