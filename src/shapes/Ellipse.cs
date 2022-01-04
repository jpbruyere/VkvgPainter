// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using vkvg;
using PointD = Drawing2D.PointD;

namespace VkvgPainter
{
	public class Ellipse : Shape
	{
		static double minLenght = 2;

		void bezierEllipse (vkvg.Context ctx, double radiusX, double radiusY, double x, double y, double rotationAngle = 0.0) {
			double width_two_thirds = radiusX * 4 / 3;

			double dx1 = Math.Sin(rotationAngle) * radiusY;
			double dy1 = Math.Cos(rotationAngle) * radiusY;
			double dx2 = Math.Cos(rotationAngle) * width_two_thirds;
			double dy2 = Math.Sin(rotationAngle) * width_two_thirds;

			double topCenterX = x - dx1;
			double topCenterY = y + dy1;
			double topRightX = topCenterX + dx2;
			double topRightY = topCenterY + dy2;
			double topLeftX = topCenterX - dx2;
			double topLeftY = topCenterY - dy2;

			double bottomCenterX = x + dx1;
			double bottomCenterY = y - dy1;
			double bottomRightX = bottomCenterX + dx2;
			double bottomRightY = bottomCenterY + dy2;
			double bottomLeftX = bottomCenterX - dx2;
			double bottomLeftY = bottomCenterY - dy2;

			ctx.MoveTo(bottomCenterX, bottomCenterY);
			ctx.CurveTo(bottomRightX, bottomRightY, topRightX, topRightY, topCenterX, topCenterY);
			ctx.CurveTo(topLeftX, topLeftY, bottomLeftX, bottomLeftY, bottomCenterX, bottomCenterY);
			ctx.ClosePath();
		}
		void midptellipse (vkvg.Context ctx, double rx, double ry, double xc, double yc)
		{
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

		public double RadiusX {
			get => Math.Abs(Points[0].X - Points[1].X);
			set {
				if (value == RadiusX)
					return;
				Points[1] = new PointD (Points[1].X + value, Points[0].Y);
				NotifyValueChanged (RadiusX);
			}
		}
		public double RadiusY {
			get => Math.Abs(Points[0].Y - Points[1].Y);
			set {
				if (value == RadiusX)
					return;
				Points[1] = new PointD (Points[1].X, Points[0].Y + value);
				NotifyValueChanged (RadiusY);
			}
		}
		public Ellipse(PointD position) : base(position)	{}

		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			PointD radii = (mouse.HasValue ? mouse.Value: Points[1]) - Points[0];
			//midptellipse (ctx, Math.Abs (radii.X), Math.Abs (radii.Y), Points[0].X, Points[0].Y);
			bezierEllipse (ctx, Math.Abs (radii.X), Math.Abs (radii.Y), Points[0].X, Points[0].Y);
		}
	}
}
