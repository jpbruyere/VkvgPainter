// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using Glfw;
using vkvg;
using PointD = Drawing2D.PointD;
using RectangleD = Drawing2D.RectangleD;

namespace VkvgPainter
{
	public class Path : Shape {
		bool closed;

		public Path(PointD position) : base(position){	}

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
		public override bool OnCreateMouseDown(MouseButton button, PointD m)
			=> (button == MouseButton.Right) ? true : base.OnCreateMouseDown(button, m);
		public override bool OnCreateMouseUp(MouseButton button, PointD m) => false;
	}
}
