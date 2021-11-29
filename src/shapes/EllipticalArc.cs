// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using static System.Math;
using vkvg;
using PointD = Drawing2D.PointD;
using RectangleD = Drawing2D.RectangleD;

namespace VkvgPainter
{
	public class EllipticalArc : Shape
	{
		bool largeArc, clockwise;
		double angle;
		public bool LargeArc {
			get => largeArc;
			set {
				if (largeArc == value)
					return;
				largeArc = value;
				NotifyValueChanged (largeArc);
			}
		}
		public bool Clockwise {
			get => clockwise;
			set {
				if (clockwise == value)
					return;
				clockwise = value;
				NotifyValueChanged (clockwise);
			}
		}
		public double Angle {
			get => angle;
			set {
				if (angle == value)
					return;
				angle = value;
				NotifyValueChanged (angle);
			}
		}
		PointD find_center_elliptic_arc (double x1, double y1, double x2, double y2,
				bool largeArc, bool sweepAnglePositive, double rx, double ry, double phi) {
			Matrix2d m = new Matrix2d (
				 Cos (phi), Sin (phi),
				-Sin (phi), Cos (phi)
			);
			Vector2d p = new Vector2d ((x1 - x2)/2, (y1 - y2)/2);
			Vector2d p1 = m * p;

			p = new Vector2d (rx * p1.Y / ry, -ry * p1.X / rx);
			Vector2d cp = Sqrt (
				(Pow (rx,2) * Pow (ry,2) - Pow (rx,2) * Pow (p1.Y, 2) - Pow (ry,2) * Pow (p1.X, 2)) /
				(Pow (rx,2) * Pow (p1.Y, 2) + Pow (ry,2) * Pow (p1.X, 2))
			) * p;
			if (largeArc == sweepAnglePositive)
				cp = -cp;

			m = new Matrix2d (
				Cos (phi),-Sin (phi),
				Sin (phi), Cos (phi)
			);
			p = new Vector2d ((x1 + x2)/2, (y1 + y2)/2);
			Vector2d c = m * cp + p;
			return new PointD (c.X, c.Y);
		}
		void elliptic_arc (vkvg.Context ctx, double x1, double y1, double x2, double y2,
				bool largeArc, bool sweepAnglePositive, double rx, double ry, double phi) {
			Matrix2d m = new Matrix2d (
				 Cos (phi), Sin (phi),
				-Sin (phi), Cos (phi)
			);
			Vector2d p = new Vector2d ((x1 - x2)/2, (y1 - y2)/2);
			Vector2d p1 = m * p;

			p = new Vector2d (rx * p1.Y / ry, -ry * p1.X / rx);
			Vector2d cp = Sqrt (Abs(
				(Pow (rx,2) * Pow (ry,2) - Pow (rx,2) * Pow (p1.Y, 2) - Pow (ry,2) * Pow (p1.X, 2)) /
				(Pow (rx,2) * Pow (p1.Y, 2) + Pow (ry,2) * Pow (p1.X, 2))
			)) * p;
			if (largeArc == sweepAnglePositive)
				cp = -cp;

			m = new Matrix2d (
				Cos (phi),-Sin (phi),
				Sin (phi), Cos (phi)
			);
			p = new Vector2d ((x1 + x2)/2, (y1 + y2)/2);
			Vector2d c = m * cp + p;

			Vector2d u = Vector2d.UnitX;
			Vector2d v = new Vector2d ((p1.X-cp.X)/rx, (p1.Y-cp.Y)/ry);
			double theta1 = Acos (Vector2d.Dot (u, v) / (v.Length));
			if (u.X*v.Y-u.Y*v.X < 0)
				theta1 = -theta1;

			u = v;
			v = new Vector2d ((-p1.X-cp.X)/rx, (-p1.Y-cp.Y)/ry);
			double delta_theta = Acos (Vector2d.Dot (u, v) / (v.Length)) % (PI * 2);
			if (u.X*v.Y-u.Y*v.X < 0)
				delta_theta = -delta_theta;

			m = new Matrix2d (
				Cos (phi),-Sin (phi),
				Sin (phi), Cos (phi)
			);

			double a = theta1;
			double theta2 = theta1 + delta_theta;
			double step = 0.1f;

			List<PointD> pts = new List<PointD> (1000);
			Vector2d pT = default;
			if (delta_theta < 0) {
				while (a > theta2) {
					pT = new Vector2d (rx*Cos(a),ry*Sin(a));
					p = m * pT + c;
					pts.Add (new PointD (p.X, p.Y));
					a-=step;
				}
			} else {
				while (a < theta2) {
					pT = new Vector2d (rx*Cos(a),ry*Sin(a));
					p = m * pT + c;
					pts.Add (new PointD (p.X, p.Y));
					a+=step;
				}
			}
			pT = new Vector2d (rx*Cos(theta2),ry*Sin(theta2));
			Vector2d lp = m * pT + c;
			if ((lp - p).Length > float.Epsilon)
				pts.Add (new PointD (lp.X, lp.Y));

			if (pts.Count == 0)
				return;

			ctx.MoveTo (pts[0]);
			for (int i = 1; i < pts.Count; i++)
				ctx.LineTo (pts[i]);
		}

		public EllipticalArc(PointD position) : base(position)	{}

		PointD center;

		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			center = find_center_elliptic_arc (Points[0].X, Points[0].Y, Points[1].X, Points[1].Y, largeArc, clockwise, Points[2].X, Points[2].Y, angle);
			elliptic_arc (ctx, Points[0].X, Points[0].Y, Points[1].X, Points[1].Y, largeArc, clockwise, Points[2].X - center.X, Points[2].Y - center.Y, angle);
		}
		public override void DrawPoints(Context ctx)
		{
			base.DrawPoints(ctx);
			if (center != null) {
				ctx.SetSource (1,0,0);
				ctx.LineWidth = 2;
				ctx.MoveTo (center - new PointD(10,0));
				ctx.LineTo (center + new PointD(10,0));
				ctx.MoveTo (center - new PointD(0,10));
				ctx.LineTo (center + new PointD(0,10));
				ctx.Stroke ();
				Console.WriteLine (center);
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
	}
}
