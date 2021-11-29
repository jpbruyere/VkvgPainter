// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using vkvg;
using PointD = Drawing2D.PointD;
using RectangleD = Drawing2D.RectangleD;

namespace VkvgPainter
{
	public class Curve : Shape
	{
		public Curve(PointD position) : base(position)	{}
		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			PointD p0 = Points[0];
			PointD p1 = mouse.HasValue ? mouse.Value : Points[1];
			ctx.MoveTo (p0.X, p0.Y);
			ctx.LineTo (p1.X, p1.Y);
		}
	}
}
