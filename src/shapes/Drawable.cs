// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Runtime.CompilerServices;
using Crow;
using Glfw;
using vkvg;
using PointD = Drawing2D.PointD;
using RectangleD = Drawing2D.RectangleD;

namespace VkvgPainter
{
	public abstract class Drawable : IValueChange {
		#region IValueChange implementation
		public event EventHandler<ValueChangeEventArgs> ValueChanged;
		public virtual void NotifyValueChanged (string MemberName, object _value) {
			ValueChanged?.Invoke (this, new ValueChangeEventArgs (MemberName, _value));
		}
		public void NotifyValueChanged (object _value, [CallerMemberName] string caller = null)
		{
			NotifyValueChanged (caller, _value);
		}
		#endregion

		public abstract uint LineWidth { get; set; }
		public abstract LineJoin LineJoin { get; set; }
		public abstract LineCap LineCap { get; set; }
		public abstract Drawing2D.Color FillColor { get; set; }
		public abstract Drawing2D.Color StrokeColor { get; set; }
		public abstract bool HasStroke  { get; }
		public abstract bool HasFill  { get; }
		public abstract void SetStrokeAsSource (vkvg.Context ctx);
		public abstract void SetFillAsSource (vkvg.Context ctx);
		public abstract bool EnableDash { get; set; }
		public abstract ObservableList<ValueContainer<float>> Dashes { get; set; }
		public abstract Drawing2D.RectangleD GetExtents (Context ctx);
		public double Rotation;
		public abstract void EmitPath (Context ctx, PointD? mouse = null);
		public abstract void Move (PointD delta);
		public abstract void EmitDraw (Context ctx);
		internal abstract void draw_internal (Context ctx, PointD? mouse = null);
		public abstract void DrawPoints (Context ctx);
		static double selectionInfl = 5.5;
		public void DrawBounds (vkvg.Context ctx, float r, float g, float b) {
			RectangleD rr = GetExtents (ctx).Inflated (selectionInfl);
			ctx.Rectangle (rr.X, rr.Y, rr.Width, rr.Height);
			ctx.SetSource (r,g,b);
			ctx.Dashes = new float[] {2,4};
			ctx.LineWidth = 2;
			ctx.Stroke();
			ctx.Dashes = null;
		}
		public bool Contains (vkvg.Context ctx, PointD p)
			=> GetExtents (ctx).Inflated (selectionInfl).ContainsOrIsEqual (p);

		//public abstract PointD Center { get; }
		public abstract bool HasHoverPoint (Drawing2D.PointD mouse);
		public abstract void ResetHoverPoint ();
		public override string ToString() => $"{this?.GetType()}";
	}
}
