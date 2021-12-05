// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using vkvg;
using PointD = Drawing2D.PointD;
using RectangleD = Drawing2D.RectangleD;
using static System.Math;
using OpenTK.Mathematics;
using Glfw;

namespace VkvgPainter
{
	public class CenteredEllipseArc : Shape
	{
		PointD Radii => Points.Count > 1 ? Points[0] - Points[1] : new PointD (10, 6);
		public double RadiusX {
			get => (Points[0] - Points[1]).Length;
			/*set {
				if (value == RadiusX)
					return;
				Points[1] = new PointD (Points[0].X + RadiusX, Points[0].Y);
				NotifyValueChanged ("RadiusX", RadiusX);
			}*/
		}
		public double RadiusY {
			get => (Points[0] - Points[1]).Length;
			/*set {
				if (value == RadiusX)
					return;
				Points[1] = new PointD (Points[0].X + RadiusX, Points[0].Y);
				NotifyValueChanged ("RadiusX", RadiusX);
			}*/
		}
		public double StartAngle {
			get => Points.Count > 3 ? getAngle (Points[0], Points[3]): 0;
			/*set {
				if (Points.Count < 2 || value == StartAngle)
					return;
				double x = Math.Cos (value) * Radius;
				double y = Math.Sin (value) * Radius;
				Points[1] = new PointD (value < Math.PI ? x : -x, y) + Points[0];
				NotifyValueChanged ("StartAngle", Radius);
			}*/
		}
		public double EndAngle {
			get => Points.Count > 4 ? getAngle (Points[0], Points[4]): Math.PI * 2.0;
			/*set {
				if (Points.Count < 3 || value == EndAngle)
					return;
				double x = Math.Cos (value) * Radius;
				double y = Math.Sin (value) * Radius;
				Points[2] = new PointD (value < Math.PI ? x : -x, y) + Points[0];
				NotifyValueChanged ("EndAngle", Radius);
			}*/
		}
		public double Phi {
			get => Points.Count > 2 ? getAngle (Points[0], Points[2]) : 0;
			/*set {
				if (Points.Count < 3 || value == EndAngle)
					return;
				double x = Math.Cos (value) * Radius;
				double y = Math.Sin (value) * Radius;
				Points[2] = new PointD (value < Math.PI ? x : -x, y) + Points[0];
				NotifyValueChanged ("EndAngle", Radius);
			}*/
		}
		public CenteredEllipseArc(PointD position) : base(position)	{}
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
		public override void DrawPoints(Context ctx)
		{
			base.DrawPoints(ctx);
			ctx.DrawCross (x1.ToPointD(), 1,0,0, 6, 2);
			ctx.DrawCross (x2.ToPointD(), 0,1,0, 6, 2);

			ctx.SetSource (0.0,0.0,0.0,0.5);
			ctx.LineWidth = 1;
			PointD p = center.ToPointD ();
			if (Points.Count>3) {
				ctx.MoveTo (p);
				ctx.LineTo (p.X + Min (radii.X, radii.Y), p.Y);
				ctx.MoveTo (p);
				ctx.LineTo (Points[3]);
				if (Points.Count>4) {
					ctx.MoveTo (p);
					ctx.LineTo (Points[4]);
				}
				ctx.Stroke();

			}

			ctx.Arc (p.X, p.Y, Min (radii.X, radii.Y), 0, PI * 2);
			ctx.Stroke ();
			p += 10;
			ctx.SetSource (0.1,0.0,1.0);
			ctx.FontSize = 8;
			ctx.MoveTo (p);
			ctx.ShowText (largeArc ? "large arc" : "small arc");
			p.Y += 15;
			ctx.MoveTo (p);
			ctx.ShowText (counterClockWise ? "counter clockwise" : "clockwise");

			ctx.SetSource (1,0.1,0.0,0.4);
			ctx.LineWidth = 10;
			ctx.DrawEllipticArc (x1.ToPointD(), x2.ToPointD(), largeArc, counterClockWise, radii, Phi);
			ctx.Stroke ();
		}
		Vector2d center, x1, x2;
		PointD radii;
		bool largeArc, counterClockWise;
		Matrix2d matPhiEllipsPoint;
		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			Vector2d p = default;
			radii = Points.Count == 1 && mouse.HasValue ? mouse.Value - Points[0]: Radii;
			radii.X = Math.Abs (radii.X);
			radii.Y = Math.Abs (radii.Y);
			if (radii.X == 0 || radii.Y == 0) {
				if (Points.Count > 4) {
					ctx.MoveTo (Points[3]);
					ctx.LineTo (Points[4]);
				}
				return;
			}
			double phi = Points.Count == 2 && mouse.HasValue ? getAngle (Points[0], mouse.Value) : Phi;
			double sa = (Points.Count == 3  && mouse.HasValue ? getAngle (Points[0], mouse.Value) : StartAngle);
			double ea = (Points.Count == 4  && mouse.HasValue ? getAngle (Points[0], mouse.Value) : EndAngle);

			matPhiEllipsPoint = new Matrix2d (
				Cos (phi),-Sin (phi),
				Sin (phi), Cos (phi)
			);
			double theta = sa;
			double delta_theta = ea - sa;
			center = new Vector2d (Points[0].X, Points[0].Y);
			//get x1 x2 from angles
			x1 = matPhiEllipsPoint * new Vector2d (
											radii.X * Cos(sa),
											radii.Y * Sin(sa))
					+ center;
			x2 = matPhiEllipsPoint * new Vector2d (
											radii.X * Cos(ea),
											radii.Y * Sin(ea))
					+ center;
			largeArc = delta_theta > Math.PI;
			counterClockWise = delta_theta > 0;

			Console.WriteLine ($"centered:{phi*Extensions.radToDg,8:0.0}{sa*Extensions.radToDg,8:0.0}{ea*Extensions.radToDg,8:0.0}{delta_theta*Extensions.radToDg,8:0.0}");

			/*if (Points.Count > 3) {
				Points[3] = x1.ToPointD();
				if (Points.Count > 4)
					Points[4] = x2.ToPointD();
			}*/



			List<PointD> pts = new List<PointD> (1000);
			double step = 0.1;

			if (counterClockWise) {
				while (theta < ea) {
					p = new Vector2d (
						radii.X * Cos(theta),
						radii.Y * Sin(theta)
					);
					Vector2d xy = matPhiEllipsPoint * p + center;
					pts.Add (new PointD(xy.X, xy.Y));
					theta += step;
				}
			} else {
				while (theta > ea) {
					p = new Vector2d (
						radii.X * Cos(theta),
						radii.Y * Sin(theta)
					);
					Vector2d xy = matPhiEllipsPoint * p + center;
					pts.Add (new PointD(xy.X, xy.Y));
					theta -= step;
				}
			}
			p = new Vector2d (
				radii.X * Cos(ea),
				radii.Y * Sin(ea)
			);
			Vector2d lp = (matPhiEllipsPoint * p) + center;
			if ((lp - p).Length > float.Epsilon)
				pts.Add (new PointD (lp.X, lp.Y));

			if (pts.Count == 0)
				return;

			ctx.MoveTo (pts[0]);
			for (int i = 1; i < pts.Count; i++)
				ctx.LineTo (pts[i]);

			/*if (Points.Count > 3) {
				Points[3] = pts[0];
				if (Points.Count > 4)
					Points[4] = pts[pts.Count-1];
			}*/
		}
		/*double getAngle (PointD c, PointD p) {
			PointD v = p - c;
			v = v.Normalized;
			if (v.Y < 0)
				return -Math.Acos (v.X);
			else
				return Math.Acos (v.X);
		}*/
		double getAngle (PointD c, PointD p) {
			Vector2d u = Vector2d.UnitX;
			Vector2d v = new Vector2d (p.X-c.X, p.Y-c.Y).Normalized();
			double sa = Acos (Vector2d.Dot (u, v));
			return (u.X*v.Y-u.Y*v.X < 0) ? -sa : sa;
		}

		public override bool OnCreateMouseDown(MouseButton button, PointD m)
		{
			if (button == MouseButton.Right)
				return true;

			if (Points.Count < 4)
				AddPoint (m);
			return false;
		}
		public override bool OnCreateMouseUp(MouseButton button, PointD m)
		{
			AddPoint (m);
			return Points.Count == 5;
		}
	}
}
