// Copyright (c) 2021  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using Crow;
using Drawing2D;
using OpenTK.Mathematics;
using static System.Math;

namespace VkvgPainter
{
	public static class Extensions {
		public static double radToDg = 180.0 / PI;
		public static PointD ToPointD (this Vector2d v) => new PointD (v.X, v.Y);
		public static Vector2d ToVector2d (this PointD v) => new Vector2d (v.X, v.Y);
		public static void SetAsSource (this Fill f, vkvg.Context ctx, Rectangle bounds = default (Rectangle)) {
			if (f is SolidColor sc)
				sc.SetAsSource (ctx, bounds);
			else
				throw new NotImplementedException ();
		}
		public static void SetAsSource (this SolidColor sc, vkvg.Context ctx, Rectangle bounds = default (Rectangle)) {
			float[] c = sc.color.floatArray;
			ctx.SetSource(c [0], c [1], c [2], c [3]);
		}
		public static void DrawCross (this vkvg.Context ctx, PointD p, double r, double g, double b, double size = 4, double LineWidth = 1) {
			ctx.LineWidth = LineWidth;
			ctx.SetSource (r,g,b);

			ctx.MoveTo (p.X, p.Y - size);
			ctx.LineTo (p.X, p.Y + size);
			ctx.MoveTo (p.X - size, p.Y);
			ctx.LineTo (p.X + size, p.Y);
			ctx.Stroke ();
		}
		public static void DrawEllipticArc (this vkvg.Context ctx, PointD x1, PointD x2, bool largeArc, bool counterClockWise, PointD radii, double phi)
			=> ctx.DrawEllipticArc (x1.X, x1.Y, x2.X, x2.Y, largeArc, counterClockWise, radii.X, radii.Y,phi);
		public static void DrawEllipticArc (this vkvg.Context ctx, double x1, double y1, double x2, double y2,
				bool largeArc, bool counterClockWise, double rx, double ry, double phi) {
			Matrix2d m = new Matrix2d (
				 Cos (phi), Sin (phi),
				-Sin (phi), Cos (phi)
			);
			Vector2d p = new Vector2d ((x1 - x2)/2, (y1 - y2)/2);
			Vector2d p1 = m * p;

			//radii corrections
			double lambda = Pow (p1.X, 2) / Pow (rx, 2) + Pow (p1.Y, 2) / Pow (ry, 2);
			if (lambda > 1) {
				lambda = Sqrt (lambda);
				rx *= lambda;
				ry *= lambda;
			}

			p = new Vector2d (rx * p1.Y / ry, -ry * p1.X / rx);
			Vector2d cp = Sqrt (Abs(
				(Pow (rx,2) * Pow (ry,2) - Pow (rx,2) * Pow (p1.Y, 2) - Pow (ry,2) * Pow (p1.X, 2)) /
				(Pow (rx,2) * Pow (p1.Y, 2) + Pow (ry,2) * Pow (p1.X, 2))
			)) * p;
			if (largeArc == counterClockWise)
				cp = -cp;

			m = new Matrix2d (
				Cos (phi),-Sin (phi),
				Sin (phi), Cos (phi)
			);
			p = new Vector2d ((x1 + x2)/2, (y1 + y2)/2);
			Vector2d c = m * cp + p;

			Vector2d u = Vector2d.UnitX;
			Vector2d v = new Vector2d ((p1.X-cp.X)/rx, (p1.Y-cp.Y)/ry);
			double sa = Acos (Vector2d.Dot (u, v) / (v.Length * u.Length));
			if (u.X*v.Y-u.Y*v.X < 0)
				sa = -sa;

			u = v;
			v = new Vector2d ((-p1.X-cp.X)/rx, (-p1.Y-cp.Y)/ry);
			double delta_theta = Acos (Vector2d.Dot (u, v) / (v.Length * u.Length));
			if (u.X*v.Y-u.Y*v.X < 0)
				delta_theta = -delta_theta;
			if (counterClockWise) {
				if (delta_theta < 0)
					delta_theta += PI * 2.0;
			} else if (delta_theta > 0)
				delta_theta -= PI * 2.0;

			m = new Matrix2d (
				Cos (phi),-Sin (phi),
				Sin (phi), Cos (phi)
			);

			double theta = sa;
			double ea = sa + delta_theta;
			double step = 0.1f;


			Console.WriteLine ($"fromx1x2:{phi*Extensions.radToDg,8:0.0}{sa*radToDg,8:0.0}{ea*radToDg,8:0.0}{delta_theta*radToDg,8:0.0}");

			List<PointD> pts = new List<PointD> (1000);
			Vector2d pT = default;
			if (sa < ea) {
				while (theta < ea) {
					p = new Vector2d (
						rx * Cos(theta),
						ry * Sin(theta)
					);
					Vector2d xy = (m * p) + c;
					pts.Add (new PointD(xy.X, xy.Y));
					theta += step;
				}
			} else {
				while (theta > ea) {
					p = new Vector2d (
						rx * Cos(theta),
						ry * Sin(theta)
					);
					Vector2d xy = (m * p) + c;
					pts.Add (new PointD(xy.X, xy.Y));
					theta -= step;
				}
			}
			pT = new Vector2d (rx*Cos(ea),ry*Sin(ea));
			Vector2d lp = m * pT + c;
			if ((lp - p).Length > float.Epsilon)
				pts.Add (new PointD (lp.X, lp.Y));

			if (pts.Count == 0)
				return;

			ctx.MoveTo (pts[0]);
			for (int i = 1; i < pts.Count; i++)
				ctx.LineTo (pts[i]);
		}
		public static void DrawEllipse (this vkvg.Context ctx, double rx, double ry, double xc, double yc)
		{
			const double minLenght = 2;
			List<PointD>[] pts = new List<PointD>[4] {
				new List<PointD>(100),
				new List<PointD>(100),
				new List<PointD>(100),
				new List<PointD>(100)
			};
			double dx, dy, d1, d2, x, y;
			x = 0;
			y = ry;
			// Initial decision parameter of region 1
			d1 = (ry * ry) - (rx * rx * ry) + (0.25 * rx * rx);
			dx = 2 * ry * ry * x;
			dy = 2 * rx * rx * y;

			// For region 1
			while (dx < dy)
			{
				pts[0].Add (new PointD (x + xc, y + yc));
				pts[1].Add (new PointD (-x + xc, y + yc));
				pts[2].Add (new PointD (x + xc, -y + yc));
				pts[3].Add (new PointD (-x + xc, -y + yc));

				// Checking and updating value of
				// decision parameter based on algorithm
				if (d1 < 0)
				{
					x++;
					dx = dx + (2 * ry * ry);
					d1 = d1 + dx + (ry * ry);
				}
				else
				{
					x++;
					y--;
					dx = dx + (2 * ry * ry);
					dy = dy - (2 * rx * rx);
					d1 = d1 + dx - dy + (ry * ry);
				}
			}

			// Decision parameter of region 2
			d2 = ((ry * ry) * ((x + 0.5f) * (x + 0.5f))) +
				((rx * rx) * ((y - 1) * (y - 1))) -
				(rx * rx * ry * ry);

			// Plotting points of region 2
			while (y >= 0)
			{
				pts[0].Add (new PointD (x + xc, y + yc));
				pts[1].Add (new PointD (-x + xc, y + yc));
				pts[2].Add (new PointD (x + xc, -y + yc));
				pts[3].Add (new PointD (-x + xc, -y + yc));
				// Checking and updating parameter
				// value based on algorithm
				if (d2 > 0)
				{
					y--;
					dy = dy - (2 * rx * rx);
					d2 = d2 + (rx * rx) - dy;
				}
				else
				{
					y--;
					x++;
					dx = dx + (2 * ry * ry);
					dy = dy - (2 * rx * rx);
					d2 = d2 + dx - dy + (rx * rx);
				}
			}

			PointD lp = pts[0][pts[0].Count -1];
			ctx.MoveTo (lp);
			for (int i = pts[0].Count - 2; i >= 0; i--)
			{
				PointD p = pts[0][i];
				if ((p - lp).Length < minLenght)
					continue;
				ctx.LineTo (p);
				lp = p;
			}
			for (int i = 0; i < pts[1].Count; i++)
			{
				PointD p = pts[1][i];
				if ((p - lp).Length < minLenght)
					continue;
				ctx.LineTo (p);
				lp = p;
			}
			for (int i = pts[3].Count - 2; i >= 0; i--)
			{
				PointD p = pts[3][i];
				if ((p - lp).Length < minLenght)
					continue;
				ctx.LineTo (p);
				lp = p;
			}
			for (int i = 0; i < pts[2].Count; i++)
			{
				PointD p = pts[2][i];
				if ((p - lp).Length < minLenght)
					continue;
				ctx.LineTo (p);
				lp = p;
			}
			ctx.ClosePath ();
		}
		public static void MoveTo (this vkvg.Context ctx, PointD p) => ctx.MoveTo (p.X, p.Y);
		public static void LineTo (this vkvg.Context ctx, PointD p) => ctx.LineTo (p.X, p.Y);
		public static void CurveTo (this vkvg.Context ctx, PointD cp1, PointD cp2, PointD p)
			=> ctx.CurveTo (cp1.X, cp1.Y, cp2.X, cp2.Y, p.X, p.Y);
	}
}
