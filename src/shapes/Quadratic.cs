// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using Glfw;
using vkvg;
using PointD = Drawing2D.PointD;
using RectangleD = Drawing2D.RectangleD;

namespace VkvgPainter
{
	public class Quadratic : Shape
	{
		void QuadraticTo (vkvg.Context ctx, PointD start, PointD cp, PointD end) {
			ctx.MoveTo (start);
			PointD cp1 = start + 2.0*(cp - start)/3.0;
			PointD cp2 = end + 2.0*(cp - end)/3.0;
			ctx.CurveTo (cp1, cp2, end);
		}

		public Quadratic(PointD position) : base (position)	{

		}
		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			ctx.MoveTo (Points[0].X, Points[0].Y);
			if (mouse.HasValue) {
				PointD m = mouse.Value;
				switch (Points.Count) {
				case 1:
					ctx.CurveTo (Points[0].X, m.Y, m.X, m.Y, m.X, Points[0].Y);
					break;
				case 2:
					ctx.CurveTo (Points[1].X, Points[1].Y, m.X, m.Y, m.X, m.Y);
					break;
				}
			} else
				QuadraticTo (ctx, Points[0], Points[1], Points[2]);
		}
		protected void _drawPoint (vkvg.Context ctx, int idx) {
			if (idx == selectedPoint)
				drawPoint (ctx, Points [idx], 1, 0.1, 0.1);
			else
				drawPoint (ctx, Points [idx], 0.1, 0.1, 1);
		}
		public override void DrawPoints(Context ctx)
		{
			if (Points.Count < 3)
				return;
			ctx.LineWidth = 1;
			ctx.SetSource (0.2,0.2,0.2);
			ctx.MoveTo (Points[0]);
			ctx.LineTo (Points[1]);
			ctx.LineTo (Points[2]);
			ctx.Stroke();

			_drawPoint (ctx, 0);
			_drawPoint (ctx, 2);

			if (selectedPoint == 1)
				ctx.SetSource (0.9,0.2,0.2);
			else
				ctx.SetSource (0.2,0.2,0.8);
			ctx.Arc (Points[1].X, Points[1].Y, selRadius, 0, Math.PI * 2);
			ctx.FillPreserve ();
			ctx.SetSource (0.1,0.1,0.9);
			ctx.Stroke ();
		}
		public override RectangleD GetExtents(Context ctx)
		{
			ctx.MoveTo (Points[0]);
			for (int i = 1; i < Points.Count; i++)
				ctx.LineTo (Points[i]);
			ctx.PathExtents (out float x1, out float y1, out float x2, out float y2);
			ctx.NewPath ();
			RectangleD r = new RectangleD (x1, y1, x2 - x1, y2 -y1);
			if (HasStroke)
				r.Inflate ((float)LineWidth / 2);
			return r;
		}
		public override bool OnCreateMouseDown(MouseButton button, PointD m)
		{
			AddPoint (new PointD (m.X, m.Y));
			return true;
		}
		public override bool OnCreateMouseUp(MouseButton button, PointD m)
		{
			AddPoint (new PointD (Points[0].X, m.Y));
			return false;
		}
	}
}
