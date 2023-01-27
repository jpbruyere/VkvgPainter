// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using Crow;
using Drawing2D;
using vkvg;
using PointD = Drawing2D.PointD;
using RectangleD = Drawing2D.RectangleD;
using System.Linq;

namespace VkvgPainter
{
	public class Group : Drawable
	{
		public List<Drawable> Elements = new List<Drawable>();
		public Drawable[] ElementsInReverseOrder {
			get {
				Drawable[] tmp = Elements.ToArray();
				tmp.Reverse();
				return tmp;
			}
		}

		public Group (IEnumerable<Drawable> shapes) {
			Elements.AddRange (shapes);
		}
		public override uint LineWidth {
			get => Elements[0].LineWidth;
			set {
				foreach (Drawable d in Elements)
					d.LineWidth = value;
				if (value != LineWidth)
					NotifyValueChanged (value);
			}
		}
		public override LineJoin LineJoin {
			get => Elements[0].LineJoin;
			set {
				foreach (Drawable d in Elements)
					d.LineJoin = value;
				if (value != LineJoin)
					NotifyValueChanged (value);
			}
		}
		public override LineCap LineCap {
			get => Elements[0].LineCap;
			set {
				foreach (Drawable d in Elements)
					d.LineCap = value;
				if (value != LineCap)
					NotifyValueChanged (value);
			}
		}
		public override Color FillColor {
			get => Elements[0].FillColor;
			set {
				foreach (Drawable d in Elements)
					d.FillColor = value;
				if (value != FillColor)
					NotifyValueChanged (value);
			}
		}
		public override Color StrokeColor {
			get => Elements[0].StrokeColor;
			set {
				foreach (Drawable d in Elements)
					d.StrokeColor = value;
				if (value != StrokeColor)
					NotifyValueChanged (value);
			}
		}

		public override bool HasStroke => Elements[0].HasStroke;
		public override bool HasFill => Elements[0].HasFill;
		public override void SetFillAsSource(Context ctx) => Elements[0].SetFillAsSource (ctx);
		public override void SetStrokeAsSource(Context ctx) => Elements[0].SetStrokeAsSource (ctx);
		public override bool EnableDash {
			get => Elements[0].EnableDash;
			set {
				foreach (Drawable d in Elements)
					d.EnableDash = value;
				if (value != EnableDash)
					NotifyValueChanged (value);
			}
		}
		public override ObservableList<ValueContainer<float>> Dashes {
			get => Elements[0].Dashes;
			set {
				foreach (Drawable d in Elements)
					d.Dashes = value;
				if (value != Dashes)
					NotifyValueChanged (value);
			}
		}

		public override void DrawPoints(Context ctx)
		{
			foreach (Drawable d in Elements)
				d.DrawPoints (ctx);
		}

		public override void EmitDraw (Context ctx) => Elements[0].EmitDraw (ctx);
		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			foreach (Drawable d in Elements) {
				ctx.NewSubPath ();
				d.EmitPath (ctx, mouse);
			}
		}

		public override RectangleD GetExtents(Context ctx)
		{
			RectangleD tmp = Elements[0].GetExtents (ctx);
			for (int i = 1; i < Elements.Count; i++)
				tmp = tmp.Union (Elements[i].GetExtents (ctx));
			return tmp;
		}
		Drawable hoverPointShape;
		public override bool HasHoverPoint(PointD mouse)
		{
			foreach (Drawable d in Elements) {
				if (d.HasHoverPoint (mouse)) {
					hoverPointShape = d;
					return true;
				}
			}
			hoverPointShape = null;
			return false;
		}

		public override void Move(PointD delta)
		{
			if (hoverPointShape != null)
				hoverPointShape.Move (delta);
			else {
				foreach (Drawable d in Elements)
					d.Move (delta);
			}
		}

		public override void ResetHoverPoint()
		{
			foreach (Drawable d in Elements)
				d.ResetHoverPoint();
		}


		internal override void draw_internal (Context ctx, PointD? mouse = null) {
			ctx.FillRule = FillRule.NonZero;

			if (EnableDash && Dashes.Count > 0)
				ctx.Dashes = Dashes.Select (d => d.Value).ToArray ();

			ctx.LineWidth = LineWidth;
			ctx.LineJoin = LineJoin;
			ctx.LineCap = LineCap;

			EmitPath (ctx, mouse);
			EmitDraw (ctx);
		}
	}
}
