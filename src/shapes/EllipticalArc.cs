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
				Program.redraw = true;
			}
		}
		public bool Clockwise {
			get => clockwise;
			set {
				if (clockwise == value)
					return;
				clockwise = value;
				NotifyValueChanged (clockwise);
				Program.redraw = true;
			}
		}
		public double Angle {
			get => angle;
			set {
				if (angle == value)
					return;
				angle = value;
				NotifyValueChanged (angle);
				Program.redraw = true;
			}
		}

		public PointD Start {
			get => Points[0];
			set {
				if (Points[0] == value)
					return;
				Points[0] = value;
				NotifyValueChanged (value);
				Program.redraw = true;
			}
		}
		public PointD End {
			get => Points[1];
			set {
				if (Points[1] == value)
					return;
				Points[1] = value;
				NotifyValueChanged (value);
				Program.redraw = true;
			}
		}
		public PointD Radii {
			get => Points[2];
			set {
				if (Points[2] == value)
					return;
				Points[2] = value;
				NotifyValueChanged (value);
				Program.redraw = true;
			}
		}
		public override void Move(PointD delta)
		{
			base.Move(delta);
			switch (selectedPoint) {
				case 0:
					NotifyValueChanged ("Start", Points[0]);
					break;
				case 1:
					NotifyValueChanged ("End", Points[1]);
					break;
				case 2:
					NotifyValueChanged ("Radii", Points[2]);
					break;
			}
		}
		public EllipticalArc(PointD position) : base(position)	{
			AddPoint (position + new PointD(200,0));
			AddPoint (new PointD(14,10));
		}

		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			ctx.DrawEllipticArc (Points[0].X, Points[0].Y, Points[1].X, Points[1].Y, largeArc, clockwise, Points[2].X, Points[2].Y, angle);
		}
	}
}
