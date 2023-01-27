// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using Crow;
using Drawing2D;
using Glfw;
using vkvg;
using PointD = Drawing2D.PointD;
using RectangleD = Drawing2D.RectangleD;

namespace VkvgPainter
{
	public abstract class Shape : Drawable {
		internal static double cpRadius = 10, selRadius = 3.5;
		internal static double rad = selRadius * 2 + 1;
		uint lineWidth = 1;
		LineJoin lineJoin = LineJoin.Miter;
		LineCap lineCap = LineCap.Butt;
		protected Fill fillColor = null;//Colors.White;
		protected Fill strokeColor = Drawing2D.Colors.Black;
		bool enableDash;
		ObservableList<ValueContainer<float>> dashes = new ObservableList<ValueContainer<float>> ();
		protected List<PointD> Points;
		protected int selectedPoint = -1;
		public virtual void AddPoint (PointD p) => Points.Add (p);

		public override uint LineWidth {
			get => lineWidth;
			set {
				if (value == lineWidth)
					return;
				lineWidth = value;
				Program.redraw = true;
				NotifyValueChanged (lineWidth);
			}
		}
		public override LineJoin LineJoin {
			get => lineJoin;
			set {
				if (value == lineJoin)
					return;
				lineJoin = value;
				Program.redraw = true;
				NotifyValueChanged (lineJoin);
			}
		}
		public override LineCap LineCap {
			get => lineCap;
			set {
				if (value == lineCap)
					return;
				lineCap = value;
				Program.redraw = true;
				NotifyValueChanged (lineCap);
			}
		}
		public override Drawing2D.Color FillColor {
			get => fillColor;
			set {
				if (value == fillColor)
					return;
				fillColor = value;
				Program.redraw = true;
				NotifyValueChanged (fillColor);
			}
		}
		public override Drawing2D.Color StrokeColor {
			get => strokeColor;
			set {
				if (value == strokeColor)
					return;
				strokeColor = value;
				Program.redraw = true;
				NotifyValueChanged (strokeColor);
			}
		}
		public override bool HasStroke => strokeColor != null;
		public override bool HasFill => fillColor != null;
		public override void SetStrokeAsSource (vkvg.Context ctx) => strokeColor.SetAsSource (ctx);
		public override void SetFillAsSource (vkvg.Context ctx) => fillColor.SetAsSource (ctx);
		public override bool EnableDash {
			get => enableDash;
			set {
				if (value == enableDash)
					return;
				enableDash = value;
				Program.redraw = true;
				NotifyValueChanged (enableDash);
			}
		}
		public override ObservableList<ValueContainer<float>> Dashes {
			set {
				if (dashes == value)
					return;
				dashes = value;
				Program.redraw = true;
				NotifyValueChanged (dashes);
			}
			get => dashes;
		}

		public override Drawing2D.RectangleD GetExtents (Context ctx) {
			EmitPath (ctx);
			ctx.PathExtents (out float x1, out float y1, out float x2, out float y2);
			ctx.NewPath ();
			RectangleD r = new RectangleD (x1, y1, x2 - x1, y2 -y1);
			if (HasStroke)
				r.Inflate ((float)LineWidth / 2);
			return r;
		}
		public override abstract void EmitPath (Context ctx, PointD? mouse = null);
		public override void Move (PointD delta) {
			if (selectedPoint < 0) {
				for (int i = 0; i < Points.Count; i++)
					Points[i] += delta;
			} else
				Points[selectedPoint] += delta;
		}
		/// <summary>
		/// Default mouse down action on creation, add point and return is complete = false
		/// </summary>
		/// <param name="button"></param>
		/// <param name="m"></param>
		/// <returns>true if shape is complete</returns>
		public virtual bool OnCreateMouseDown (MouseButton button, PointD m) {
			AddPoint (m);
			return false;
		}
		/// <summary>
		/// Default mouse up action on creation, add point and return is complete = true
		/// </summary>
		/// <param name="button"></param>
		/// <param name="m"></param>
		/// <returns>true if shape is complete</returns>
		public virtual bool OnCreateMouseUp (MouseButton button, PointD m) {
			AddPoint (m);
			return true;
		}

		public override void EmitDraw (Context ctx) {
			if (!HasFill) {
				if (!HasStroke)
					return;
				SetStrokeAsSource (ctx);
				ctx.Stroke ();
				return;
			}
			SetFillAsSource (ctx);
			if (!HasStroke)
				ctx.Fill ();
			else {
				ctx.FillPreserve ();
				SetStrokeAsSource (ctx);
				ctx.Stroke ();
			}
		}
		internal override void draw_internal (Context ctx, PointD? mouse = null) {
			ctx.FillRule = FillRule.EvenOdd;

			if (enableDash && dashes.Count > 0)
				ctx.Dashes = dashes.Select (d => d.Value).ToArray ();

			ctx.LineWidth = lineWidth;
			ctx.LineJoin = lineJoin;
			ctx.LineCap = lineCap;

			EmitPath (ctx, mouse);
			EmitDraw (ctx);
		}
		protected void drawPoint (Context ctx, PointD p, double r, double g, double b) {
			ctx.SetSource (r, g, b, 0.6);
			ctx.Rectangle (p.X - selRadius, p.Y - selRadius, rad, rad);
			ctx.FillPreserve ();
			ctx.SetSource (r, g, b, 0.9);
			ctx.Stroke ();

		}
		public override void DrawPoints (Context ctx) {
			ctx.LineWidth = 1;
			for (int i = 0; i < Points.Count; i++) {
				PointD p = Points [i];

				if (i == selectedPoint)
					drawPoint (ctx, p, 1, 0.1, 0.1);
				else
					drawPoint (ctx, p, 0.1, 0.1, 1);
			}
		}
		//public abstract PointD Center { get; }
		public override bool HasHoverPoint (Drawing2D.PointD mouse) {
			Drawing2D.RectangleD r = new Drawing2D.RectangleD (0 , 0, selRadius * 2, selRadius * 2);
			for (int i = 0; i < Points.Count; i++) {
				PointD p = Points [i];
				r.X = p.X - selRadius;
				r.Y = p.Y - selRadius;
				if (r.ContainsOrIsEqual (mouse)) {
					selectedPoint = i;
					return true;
				}
			}
			selectedPoint = -1;
			return false;
		}
		public override void ResetHoverPoint () => selectedPoint = -1;
		public Shape (PointD position) {
			Points = new List<PointD>();
			Points.Add (position);
		}
		public override string ToString() => $"{this?.GetType()}";
	}
}
