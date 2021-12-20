// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using OpenTK.Mathematics;
using Glfw;
using vkvg;
using PointD = Drawing2D.PointD;
using RectangleD = Drawing2D.RectangleD;
using static System.Math;
using System.Collections.Generic;

namespace VkvgPainter
{
	public class PathTest : Shape {
		bool closed;

		public PathTest(PointD position) : base(position){
			LineWidth = 60;
		}

		public bool Closed {
			get => closed;
			set {
				if (value == closed)
					return;
				closed = value;
				NotifyValueChanged ("Closed", closed);
			}
		}
		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			ctx.MoveTo (Points[0]);
			for (int i = 1; i < Points.Count; i++)
				ctx.LineTo (Points[i]);
			if (mouse.HasValue)
				ctx.LineTo (mouse.Value);
			if (closed)
				ctx.ClosePath ();
		}
		public override void EmitDraw (Context ctx) {
			ctx.LineWidth = 1;
			if (!Closed || fillColor == null) {
				if (strokeColor == null)
					return;
				strokeColor.SetAsSource (ctx);
				ctx.Stroke ();
				return;
			}
			fillColor.SetAsSource (ctx);
			if (StrokeColor == null)
				ctx.Fill ();
			else {
				ctx.FillPreserve ();
				strokeColor.SetAsSource (ctx);
				ctx.Stroke ();
			}
		}
		static PointD labelDist = new PointD(1,-3);
		static PointD labelSpace = new PointD(0,12);
		public override void DrawPoints(Context ctx)
		{
			base.DrawPoints(ctx);
			if (Points.Count < 2)
				return;
			ctx.FontSize = 12;
			ctx.SetSource (0,0.1,0);

			List<PointD> vertices = new List<PointD>();
			List<int> indices = new List<int>();

			double hw = LineWidth / 2.0;
			Vector2d a = Points[0].ToVector2d ();
			Vector2d b = Points[1].ToVector2d ();
			Vector2d n = Vector2d.Normalize (b-a);
			Vector2d vhw = (n * hw).Perp();
			vertices.Add ((a+vhw).ToPointD());
			vertices.Add ((a-vhw).ToPointD());

			indices.AddRange (new int[] {0,2,1,1,2,3});
			int ib = vertices.Count;

			for (int i = 0; i < Points.Count-2; i++) {
				a = Points[i].ToVector2d ();
				b = Points[i+1].ToVector2d ();
				Vector2d c = Points[i+2].ToVector2d ();
				Vector2d v0 = b - a;
				Vector2d v1 = c - b;
				Vector2d v0n = Vector2d.Normalize (v0);
				Vector2d v1n = Vector2d.Normalize (v1);
				Vector2d bisec_n = Vector2d.Normalize (v0n + v1n);
				double dot = Vector2d.Dot (v0n, v1n);
				double det = v0n.Det (v1n);
				double alpha = Acos (dot);
				if (det<0)
					alpha = -alpha;
				bisec_n = bisec_n.Perp();
				double hAlpha = alpha / 2.0;
				double lh = hw / Cos(hAlpha);

				bool reducedLH = (dot == -1) || (lh > Min (lh, Min (v0.Length, v1.Length)));

				double rlh = Min (lh, Min (v0.Length, v1.Length));

				double beta = (PI / 2.0) - Abs(hAlpha);
				double x = (lh - rlh) * Cos(hAlpha);

				double lbc = Cos(hAlpha) * rlh;
				Vector2d plbc = v0n.Perp() * -lbc;

				Vector2d bisec = bisec_n * rlh;
				Vector2d p = default;

				/*if (dot < 0 && reducedLH && det < 0) {
					if (v0.Length < v1.Length)
						p = v1n.Perp() * hw;
					else
						p = - v1n.Perp() * hw;
				} else*/

				p = bisec;
				if (det > 0 && x != 0) {
					Vector2d bisecPerp = bisec_n.Perp () * x;
					vertices.Add ((b+p+bisecPerp).ToPointD());
					vertices.Add ((b+p-bisecPerp).ToPointD());
				} else if (lbc < hw) {
					Vector2d pv0 = (b + p) + plbc;
					vertices.Add ((pv0+v0n.Perp()*hw).ToPointD());
				} else {
					vertices.Add ((b+p).ToPointD());
				}

				/*if (dot < 0 && reducedLH && det > 0) {
					if (v0.Length < v1.Length)
						p = - v1n.Perp() * hw;
					else
						p = v1n.Perp() * hw;
				} else*/
				p = - bisec;
				if (det < 0 && x != 0) {
					Vector2d bisecPerp = bisec_n.Perp () * x;
					vertices.Add ((b+p+bisecPerp).ToPointD());
					vertices.Add ((b+p-bisecPerp).ToPointD());
				} else if (lbc < hw) {
					Vector2d pv0 = (b + p) - plbc;
					vertices.Add ((pv0-v0n.Perp()*hw).ToPointD());
				} else {
					vertices.Add ((b+p).ToPointD());
				}

				if (rlh < lh) {
					if (det<0)
						indices.AddRange (new int[] {ib+0,ib+2,ib+1,ib+2,ib+4,ib+0,ib+0,ib+3,ib+4});
					else
						indices.AddRange (new int[] {ib+0,ib+2,ib+1,ib+2,ib+3,ib+1,ib+1,ib+3,ib+4});
				} else
					indices.AddRange (new int[] {ib+0,ib+2,ib+1,ib+1,ib+2,ib+3});

				ib = vertices.Count;

				PointD lp = p.ToPointD() * 1.5;
				ctx.SetSource (0,0.4,0);
				ctx.MoveTo (Points[i+1] + lp);
				ctx.ShowText ($"dot:{dot:0.0000}");
				ctx.MoveTo (Points[i+1] + lp + labelSpace);
				ctx.ShowText ($"det:{det:0.0000}");
				ctx.MoveTo (Points[i+1] + lp + 2*labelSpace);
				ctx.ShowText ($"reducedLH:{reducedLH}");
				ctx.MoveTo (Points[i+1] + lp + 3*labelSpace);
				ctx.ShowText ($"x:{x:0.0000}");
				ctx.MoveTo (Points[i+1] + lp + 4*labelSpace);
				ctx.ShowText ($"lbc:{lbc:0.0000}");
			}
			a = Points[Points.Count-2].ToVector2d ();
			b = Points[Points.Count-1].ToVector2d ();
			n = Vector2d.Normalize (b-a);
			vhw = (n * hw).Perp();
			vertices.Add ((b+vhw).ToPointD());
			vertices.Add ((b-vhw).ToPointD());

			ctx.LineWidth = 1.0;
			for (int i = 0; i < indices.Count; i+=3)
			{
				ctx.MoveTo (vertices[indices[i]]);
				ctx.LineTo (vertices[indices[i+1]]);
				ctx.LineTo (vertices[indices[i+2]]);
				ctx.ClosePath();

				ctx.SetSource (0.3,0.3,0.8,0.3);
				ctx.FillPreserve();
				ctx.SetSource (0.1,0.1,0.1,0.3);
				ctx.Stroke();
			}


			ctx.SetSource (0.4,0,0);
			ctx.FontSize = 8;
			for (int i = 0; i < vertices.Count; i++) {
				ctx.NewPath();
				ctx.Arc (vertices[i].X,vertices[i].Y,3.0,0,PI*2.0);
				ctx.Fill();
				ctx.MoveTo (vertices[i]+labelDist);
				ctx.ShowText ($"{i}");
			}
		}
		public override bool OnCreateMouseDown(MouseButton button, PointD m)
			=> (button == MouseButton.Right) ? true : base.OnCreateMouseDown(button, m);
		public override bool OnCreateMouseUp(MouseButton button, PointD m) => false;
	}
}
