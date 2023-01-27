// Copyright (c) 2021-2022 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
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
	//this output stroke debug helpers to help solve stoking gliches
	public class PathTest : Shape
	{
		bool closed;
		bool strokeMode;

		public PathTest(PointD position) : base(position)
		{
			LineWidth = 60;
		}
		public bool StrokeMode
		{
			get => strokeMode;
			set
			{
				if (strokeMode == value)
					return;
				strokeMode = value;
				NotifyValueChanged(strokeMode);
				Program.redraw = true;
			}
		}
		public bool Closed
		{
			get => closed;
			set
			{
				if (value == closed)
					return;
				closed = value;
				NotifyValueChanged("Closed", closed);
			}
		}
		public int CurrentTriangle = -1;
		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			ctx.MoveTo(Points[0]);
			for (int i = 1; i < Points.Count; i++)
				ctx.LineTo(Points[i]);
			if (mouse.HasValue)
				ctx.LineTo(mouse.Value);
			if (closed)
				ctx.ClosePath();
		}
		public override void EmitDraw(Context ctx)
		{
			ctx.LineWidth = 1;
			if (!strokeMode || !Closed || fillColor == null)
			{
				if (strokeColor == null)
					return;
				strokeColor.SetAsSource(ctx);
				ctx.Stroke();
				return;
			}
			fillColor.SetAsSource(ctx);
			if (StrokeColor == null)
				ctx.Fill();
			else
			{
				ctx.FillPreserve();
				strokeColor.SetAsSource(ctx);
				ctx.Stroke();
			}
		}
		static PointD labelDist = new PointD(1, -3);
		static PointD labelSpace = new PointD(0, 14);

		double diamondAngle(Vector2d p)
			=> p.Y < 0 ?
				p.X < 0 ? 2 - p.Y / (-p.X - p.Y) : 3 + p.X / (p.X - p.Y) :
				p.X < 0 ? 1 - p.X / (-p.X + p.Y) : p.Y / (p.X + p.Y);
		

		public override async void DrawPoints(Context ctx)
		{
			base.DrawPoints(ctx);
			if (Points.Count < 2)
				return;
			ctx.FontSize = 14;
			ctx.SetSource(0, 0.1, 0);

			List<PointD> vertices = new List<PointD>();
			List<int> indices = new List<int>();

			if (strokeMode)
			{
				double hw = LineWidth / 2.0;
				Vector2d a = Points[0].ToVector2d();
				Vector2d b = Points[1].ToVector2d();
				Vector2d n = Vector2d.Normalize(b - a);
				Vector2d vhw = (n * hw).Perp();
				vertices.Add((a + vhw).ToPointD());
				vertices.Add((a - vhw).ToPointD());

				indices.AddRange(new int[] { 0, 2, 1, 1, 2, 3 });
				int ib = vertices.Count;

				for (int i = 0; i < Points.Count - 2; i++)
				{
					a = Points[i].ToVector2d();
					b = Points[i + 1].ToVector2d();
					Vector2d c = Points[i + 2].ToVector2d();
					Vector2d v0 = b - a;
					Vector2d v1 = c - b;
					Vector2d v0n = Vector2d.Normalize(v0);
					Vector2d v1n = Vector2d.Normalize(v1);
					Vector2d bisec_n = Vector2d.Normalize(v0n + v1n);
					double dot = Vector2d.Dot(v0n, v1n);
					double det = v0n.Det(v1n);

					Vector2d bisec = default;

					if (System.Math.Abs(dot + 1.0f) < float.Epsilon)
					{
						Vector2d vPerp = v0n.Perp() * hw;
						vertices.Add((b + vPerp).ToPointD());
						vertices.Add((b - vPerp).ToPointD());

						indices.AddRange(new int[] { ib + 0, ib + 1, ib + 2 });
						indices.AddRange(new int[] { ib + 0, ib + 2, ib + 3 });
						ib = vertices.Count;
					}
					else
					{
						double alpha = Acos(dot);
						if (det < 0)
							alpha = -alpha;
						Vector2d bisec_n_perp = bisec_n.Perp();
						double halfAlpha = alpha / 2.0;
						double lh = hw / Cos(halfAlpha);

						double rlh = lh;
						if (dot < 0.0)
							rlh = Min(lh, Min(v0.Length, v1.Length));

						bisec = bisec_n_perp * rlh;

						Vector2d rlh_inside_pos, rlh_outside_pos;
						Vector2d vnPerp;
						if (v0.Length < v1.Length)
							vnPerp = v1n.Perp();
						else
							vnPerp = v0n.Perp();
						Vector2d vHwPerp = vnPerp * hw;
						double lbc = Cos(halfAlpha) * rlh;

						if (det < 0.0)
						{
							if (rlh < lh)
							{
								rlh_inside_pos = vnPerp * -lbc + b + bisec + vHwPerp;
								rlh_outside_pos = b - bisec_n_perp * lh;
							}
							else
							{
								rlh_inside_pos = b + bisec;
								rlh_outside_pos = b - bisec;
							}
						}
						else
						{
							if (rlh < lh)
							{
								rlh_inside_pos = vnPerp * lbc + b - bisec - vHwPerp;
								rlh_outside_pos = b + bisec_n_perp * lh;
							}
							else
							{
								rlh_inside_pos = b - bisec;
								rlh_outside_pos = b + bisec;
							}
						}

						double x = (lh - rlh) * Cos(halfAlpha);

						if (rlh < lh)
						{
							Vector2d bisecPerp = bisec_n * x;
							if (det < 0.0)
							{
								Vector2d p = b - bisec;

								vertices.Add(rlh_inside_pos.ToPointD());
								vertices.Add((p - bisecPerp).ToPointD());
								vertices.Add((p + bisecPerp).ToPointD());

								indices.AddRange(new int[] { ib + 0, ib + 2, ib + 1, ib + 2, ib + 4, ib + 0, ib + 0, ib + 3, ib + 4 });
							}
							else
							{
								Vector2d p = b + bisec;

								vertices.Add((p - bisecPerp).ToPointD());
								vertices.Add(rlh_inside_pos.ToPointD());
								vertices.Add((p + bisecPerp).ToPointD());
								if (v0.Length < v1.Length){
									indices.AddRange(new int[] { ib + 0, ib + 2, ib + 1, ib + 2, ib + 3, ib + 1, ib + 1, ib + 3, ib + 4 });//
								}else
									indices.AddRange(new int[] { ib + 0, ib + 2, ib + 1, ib + 2, ib + 3, ib + 1, ib + 1, ib + 3, ib + 4 });
							}
						}
						else
						{
							if (det < 0.0)
							{
								vertices.Add(rlh_inside_pos.ToPointD());
								vertices.Add(rlh_outside_pos.ToPointD());
								indices.AddRange(new int[] { ib + 0, ib + 2, ib + 3, ib + 1, ib + 0, ib + 3 });
							}
							else
							{
								vertices.Add(rlh_outside_pos.ToPointD());
								vertices.Add(rlh_inside_pos.ToPointD());
								/*if (v0.Length < v1.Length)
									indices.AddRange (new int[] {ib+0,ib+2,ib+3,ib+1,ib+0,ib+3});
								else*/
								indices.AddRange(new int[] { ib + 0, ib + 2, ib + 1, ib + 1, ib + 2, ib + 3 });
							}
						}
						ib = vertices.Count;

						PointD lp = det < 0 ? (bisec.ToPointD() * -1.5) : (bisec.ToPointD() * 1.5);
						ctx.SetSource(0, 0.4, 0);
						ctx.MoveTo(Points[i + 1] + lp);
						ctx.ShowText($"dotNorm:{dot:0.0000}");
						ctx.MoveTo(Points[i + 1] + lp + labelSpace);
						ctx.ShowText($"detNorm:{det:0.0000}");
						ctx.MoveTo(Points[i + 1] + lp + 2 * labelSpace);
						ctx.ShowText($"reducedLH:{rlh < lh}");
						ctx.MoveTo(Points[i + 1] + lp + 3 * labelSpace);
						ctx.ShowText($"x:{x:0.0000}");
						ctx.MoveTo(Points[i + 1] + lp + 4 * labelSpace);
						ctx.ShowText($"lbc:{lbc:0.0000}");
						ctx.MoveTo(Points[i + 1] + lp + 5 * labelSpace);
						ctx.ShowText($"diamond angle:{diamondAngle(v1 - v0):0.0000}");
						ctx.MoveTo(Points[i + 1] + lp + 6 * labelSpace);
						ctx.ShowText($"dot:{Vector2d.Dot(v0, v1):0.0000}");
						ctx.MoveTo(Points[i + 1] + lp + 7 * labelSpace);
						ctx.ShowText($"det:{v0.Det(v1):0.0000}");
					}

				}

				a = Points[Points.Count - 2].ToVector2d();
				b = Points[Points.Count - 1].ToVector2d();
				n = Vector2d.Normalize(b - a);
				vhw = (n * hw).Perp();
				vertices.Add((b + vhw).ToPointD());
				vertices.Add((b - vhw).ToPointD());
			}
			else if (Points.Count > 2)
			{
				//triangulation test
				//clockwise contour
				List<int> reflexPoints = new List<int>();
				Vector2d a = Points[Points.Count - 1].ToVector2d();

				for (int i = 0; i < Points.Count; i++)
				{
					Vector2d b = Points[i].ToVector2d();
					Vector2d c = Points[i < Points.Count - 1 ? i + 1 : 0].ToVector2d();
					Vector2d v0 = b - a;
					Vector2d v1 = c - b;
					Vector2d v0n = Vector2d.Normalize(v0);
					Vector2d v1n = Vector2d.Normalize(v1);
					double det = v0.Det(v1);					
					if (det < 0)
						reflexPoints.Add(i);
					a = Points[i].ToVector2d();
				}

				//draw reflex points
				ctx.LineWidth = 1.0;
				ctx.SetSource(1.0, 0.2, 0.0, 0.7);
				for (int i = 0; i < reflexPoints.Count; i++)
				{
					ctx.NewPath();
					ctx.Arc(Points[reflexPoints[i]].X, Points[reflexPoints[i]].Y, 10.0, 0, PI * 2.0);
					ctx.Stroke();

				}
			}


			ctx.LineWidth = 1.0;
			if (CurrentTriangle >= 0)
			{
				int i = CurrentTriangle * 3;
				if (i > indices.Count - 3)
				{
					CurrentTriangle = indices.Count / 3 - 1;
					i = CurrentTriangle * 3;
				}
				ctx.MoveTo(vertices[indices[i]]);
				ctx.LineTo(vertices[indices[i + 1]]);
				ctx.LineTo(vertices[indices[i + 2]]);
				ctx.ClosePath();

				ctx.SetSource(0.3, 0.3, 0.8, 0.3);
				ctx.FillPreserve();
				ctx.SetSource(0.1, 0.1, 0.1, 0.3);
				ctx.Stroke();
				ctx.SetSource(0.0, 0.2, 0.0, 0.7);
				ctx.MoveTo((vertices[indices[i]].X + vertices[indices[i + 1]].X + vertices[indices[i + 2]].X) / 3.0,
							(vertices[indices[i]].Y + vertices[indices[i + 1]].Y + vertices[indices[i + 2]].Y) / 3.0);
				ctx.ShowText(CurrentTriangle.ToString());
			}
			else
			{
				for (int i = 0; i < indices.Count; i += 3)
				{
					ctx.MoveTo(vertices[indices[i]]);
					ctx.LineTo(vertices[indices[i + 1]]);
					ctx.LineTo(vertices[indices[i + 2]]);
					ctx.ClosePath();

					ctx.SetSource(0.3, 0.3, 0.8, 0.3);
					ctx.FillPreserve();
					ctx.SetSource(0.1, 0.1, 0.1, 0.3);
					ctx.Stroke();

					ctx.SetSource(0.0, 0.2, 0.0, 0.7);
					ctx.MoveTo((vertices[indices[i]].X + vertices[indices[i + 1]].X + vertices[indices[i + 2]].X) / 3.0,
								(vertices[indices[i]].Y + vertices[indices[i + 1]].Y + vertices[indices[i + 2]].Y) / 3.0);
					ctx.ShowText((i / 3).ToString());
				}
			}

			//draw vertices
			ctx.SetSource(0.4, 0, 0);
			ctx.FontSize = 8;
			for (int i = 0; i < vertices.Count; i++)
			{
				ctx.NewPath();
				ctx.Arc(vertices[i].X, vertices[i].Y, 3.0, 0, PI * 2.0);
				ctx.Fill();
				ctx.MoveTo(vertices[i] + labelDist);
				ctx.ShowText($"{i}");
			}
		}
		public override bool OnCreateMouseDown(MouseButton button, PointD m)
			=> (button == MouseButton.Right) ? true : base.OnCreateMouseDown(button, m);
		public override bool OnCreateMouseUp(MouseButton button, PointD m) => false;
	}
}
