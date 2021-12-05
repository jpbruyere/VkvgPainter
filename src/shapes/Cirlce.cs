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
	public class Circle : Shape
	{
		public double Radius {
			get => (Points[0] - Points[1]).Length;
			set {
				if (value == Radius)
					return;
				Points[1] = new PointD (Points[0].X + value, Points[0].Y);
				NotifyValueChanged ("Radius", Radius);
			}
		}
		public double StartAngle {
			get => Points.Count > 2 ? getAngle (Points[0], Points[1]) : 0;
			set {
				if (Points.Count < 2 || value == StartAngle)
					return;
				double x = Math.Cos (value) * Radius;
				double y = Math.Sin (value) * Radius;
				Points[1] = new PointD (value < Math.PI ? x : -x, y) + Points[0];
				NotifyValueChanged ("StartAngle", Radius);
			}
		}
		public double EndAngle {
			get => Points.Count > 2 ? getAngle (Points[0], Points[2]) : Math.PI * 2.0;
			set {
				if (Points.Count < 3 || value == EndAngle)
					return;
				double x = Math.Cos (value) * Radius;
				double y = Math.Sin (value) * Radius;
				Points[2] = new PointD (value < Math.PI ? x : -x, y) + Points[0];
				NotifyValueChanged ("EndAngle", Radius);
			}
		}
		public Circle(PointD position) : base(position)	{}

		double getAngle (PointD c, PointD p) {
			PointD v = p - c;
			v = v.Normalized;
			if (v.Y < 0)
				return -Math.Acos (v.X);
			else
				return Math.Acos (v.X);
		}
		public override void DrawPoints(Context ctx)
		{
			base.DrawPoints(ctx);
			if (Points.Count == 3) {
				double sa = getAngle (Points[0], Points[1]);
				double ea = getAngle (Points[0], Points[2]);
				ctx.SetSource (0,0,1);
				ctx.MoveTo (Points[1] + (Points[1] - Points[0]).Normalized * 10);
				ctx.ShowText ($"a1={sa*Extensions.radToDg:0.00}°");
				ctx.MoveTo (Points[2] + (Points[2] - Points[0]).Normalized * 10);
				ctx.ShowText ($"a2={ea*Extensions.radToDg:0.00}°");
			}
		}

		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			PointD p0 = Points[0];
			double r = Points.Count == 1 ? (p0 - mouse.Value).Length : Radius;
			double sa = Points.Count == 3 || (mouse.HasValue && Points.Count > 1) ? getAngle (p0, Points[1]) : 0;
			double ea = Points.Count == 2 && mouse.HasValue ? getAngle (p0, mouse.Value) : EndAngle;

			//double sa = Points.Count > 1 ? Math.Acos ((Points[1].X - p0.X) / r) : 0;
			//if (StartAngle < ea)
			if (Points.Count > 2) {
				ctx.MoveTo (p0);
				ctx.LineTo (Points[1]);
				ctx.Arc (p0.X, p0.Y, r, sa, ea);
				ctx.ClosePath();
			}else
				ctx.Arc (p0.X, p0.Y, r, sa, ea);
			/*else
				ctx.ArcNegative (p0.X, p0.Y, r, StartAngle, ea);*/
		}
		public override bool OnCreateMouseDown(MouseButton button, PointD m)
		{
			return (button == MouseButton.Right);
		}
		public override void Move (PointD delta) {
			if (selectedPoint < 0) {
				base.Move (delta);
				return;
			}
			Points[selectedPoint] += delta;
			switch (selectedPoint) {
				case 0:
					//Points[selectedPoint] += delta;
					if (Points.Count > 2)
						Points[2] = (Points[2] - Points[0]).Normalized * Radius + Points[0];
					break;
				case 1:
					if (Points.Count > 2)
						Points[2] = (Points[2] - Points[0]).Normalized * Radius + Points[0];
					break;
				case 2:
					Points[1] = (Points[1] - Points[0]).Normalized * (Points[2] - Points[0]).Length + Points[0];
					break;
			}
		}
		public override bool OnCreateMouseUp(MouseButton button, PointD m)
		{
			if (Points.Count == 1) {
				AddPoint (m);
				return false;
			}
			double ea = getAngle (Points[0], m);
			double x = Math.Cos (ea) * Radius;
			double y = Math.Sin (ea) * Radius;
			AddPoint (new PointD (
				ea < Math.PI ? x : -x,
				y
			)+Points[0]);
			return true;
		}
	}
}
