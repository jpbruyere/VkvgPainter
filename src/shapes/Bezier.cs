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
	public class Bezier : Shape
	{
		public Bezier(PointD position) : base (position)	{

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
				case 3:
					ctx.CurveTo (Points[1].X, Points[1].Y, m.X, m.Y, Points[2].X, Points[2].Y);
					break;
				}
			} else
				ctx.CurveTo (
					Points[1].X,
					Points[1].Y,
					Points[3].X,
					Points[3].Y,
					Points[2].X,
					Points[2].Y
				);
		}
		protected void drawControlPoint (Context ctx, PointD p, double r, double g, double b) {
			ctx.SetSource (r, g, b, 0.6);
			ctx.Arc (p.X, p.Y, selRadius, 0, Math.PI * 2);
			ctx.FillPreserve ();
			ctx.SetSource (r, g, b, 0.9);
			ctx.Stroke ();
		}
		public override void DrawPoints(Context ctx)
		{
			ctx.LineWidth = 1;
			for (int i = 0; i < Points.Count; i+=2) {
				if (i == selectedPoint)
					drawPoint (ctx, Points [i], 1, 0.1, 0.1);
				else
					drawPoint (ctx, Points [i], 0.1, 0.1, 1);
				ctx.SetSource (0.2,0.2,0.2);
				ctx.MoveTo (Points[i]);
				if (i == Points.Count - 1)
					break;
				ctx.LineTo (Points[i+1]);
				ctx.Stroke ();
				if (i+1 == selectedPoint)
					drawControlPoint (ctx, Points [i+1], 1, 0.1, 0.1);
				else
					drawControlPoint (ctx, Points [i+1], 0.1, 0.1, 1);
			}
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
		public override void Move(PointD delta)
		{
			base.Move(delta);
		}
		public override bool OnCreateMouseDown(MouseButton button, PointD m)
		{
			AddPoint (new PointD (m.X, m.Y));
			return false;
		}
		public override bool OnCreateMouseUp(MouseButton button, PointD m)
		{
			if (Points.Count == 1) {
				AddPoint (new PointD (Points[0].X, m.Y));
				return false;
			}
			AddPoint (new PointD (m.X, m.Y));
			return true;
		}
	}
}
