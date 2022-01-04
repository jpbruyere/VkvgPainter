using System;
using System.Collections.Generic;
using Glfw;
using vke;
using Vulkan;
using System.Linq;
using Drawing2D;

namespace VkvgPainter
{
	public enum DrawMode
	{
		Select,
		Lines,
		PathTest,
		Bezier,
		Quadratic,
		Rect,
		Circle,
		Ellipse,
		EllipticalArc,
		Arc,
		Star,
		Image,
	}
	class Program : BaseWindow
	{
		static void Main(string[] args)
		{
			using (Program app = new Program ())
				app.Run();
		}
		public List<Drawable> Shapes = new List<Drawable> ();
		public List<Drawable> SelectedShapes = new List<Drawable> ();

		DrawMode currentDrawMode = DrawMode.Select;
		Drawable currentShape, hoverShape;
		Shape newShape;
		public static float[][] PredefinedDashes = new float[][] {
			null,
			new float[] {10,4},
			new float[] {5,2},
			new float[] {0,3}
		};

		public uint LineWidth {
			get => currentShape == null ? Crow.Configuration.Global.Get ("LineWidth", 1u) : currentShape.LineWidth;
			set {
				if (LineWidth == value)
					return;
				Crow.Configuration.Global.Set ("LineWidth", value);
				if (currentShape != null)
					currentShape.LineWidth = value;
			}
		}
		public LineJoin LineJoin { get; set; }
		public LineCap LineCap { get; set; }
		public Drawing2D.Color FillColor { get; set; }
		public Drawing2D.Color StrokeColor { get; set; }
		public Drawable CurrentShape {
			get => currentShape;
			set {
				if (currentShape == value)
					return;
				currentShape = value;
				NotifyValueChanged ("CurrentShape", currentShape);
				redraw = true;
			}
		}
		public Drawable HoverShape {
			get => hoverShape;
			set {
				if (hoverShape == value)
					return;
				hoverShape?.ResetHoverPoint ();
				hoverShape = value;
				NotifyValueChanged ("HoverShape", hoverShape);
				redraw = true;
			}
		}
		public DrawMode CurrentDrawMode {
			get => currentDrawMode;
			set {
				if (value == currentDrawMode)
					return;
				currentDrawMode = value;
				if (currentDrawMode == DrawMode.Select && currentShape == null)
					CurrentShape = Shapes.LastOrDefault();
				else
					CurrentShape = null;
				NotifyValueChanged ("CurrentDrawMode", currentDrawMode);
				redraw = true;
			}
		}
		Drawable[] ShapesInReverseOrder {
			get {
				Drawable[] tmp = Shapes.ToArray();
				tmp.Reverse();
				return tmp;
			}
		}

		protected override void initVulkan()
		{
			base.initVulkan();

			loadWindow ("#ui.HelloWorld.crow", this);
		}
		void drawGrid (vkvg.Context ctx) {
			ctx.Clear();
			ctx.SetSource (1,1,1);
			ctx.Paint();
			ctx.SetSource (0.8,0.8,1);

			int step = 20;

			for (int x = step; x < this.Width; x+=step)	{
				ctx.MoveTo (-0.5f + x, 0);
				ctx.LineTo (-0.5f + x, Height);
			}
			for (int y = step; y < this.Height; y+=step)	{
				ctx.MoveTo (0, -0.5f + y);
				ctx.LineTo (Width, -0.5f + y);
			}
			ctx.LineWidth = 1;
			ctx.Stroke ();

		}

		public override void Update()
		{
			base.Update();

			if (redraw) {
				redraw = false;
				using (vkvg.Context ctx = new vkvg.Context(vkvgSurf))
				{
					ctx.Clear();

					drawGrid (ctx);

					foreach (Drawable shape in Shapes)
						shape.draw_internal (ctx);

					if (newShape != null) {
						newShape.draw_internal (ctx, mousePos);
						newShape.DrawPoints (ctx);
						return;
					}
					if (SelectedShapes.Count > 0) {
						foreach (Drawable shape in SelectedShapes)
							shape.DrawBounds (ctx, 0.1f, 1.0f, 0.1f);
					} else {
						if (currentShape != null) {
							//currentShape.draw_internal (ctx);
							currentShape.DrawPoints (ctx);
							currentShape.DrawBounds (ctx, 0.1f, 0.1f, 1.0f);
						}
					}
					if (hoverShape != null && hoverShape != currentShape)
						hoverShape.DrawBounds (ctx, 1.0f,0.2f,0.1f);
				}
			}
		}
		PointD? mousePos;
		internal static volatile bool redraw = true;

		protected override void onMouseButtonDown(MouseButton button, Modifier mods)
		{
			base.onMouseButtonDown(button, mods);

			if (MouseIsInInterface)
				return;

			if (currentDrawMode == DrawMode.Select) {
				if (button == MouseButton.Middle) {
					if (HoverShape is Group grp) {
						using (vkvg.Context ctx = new vkvg.Context(vkvgSurf)) {
							Drawable hs = null;
							foreach (Drawable shape in grp.ElementsInReverseOrder) {
								if (shape.Contains (ctx, mousePos.Value)) {
									hs = shape;
									break;
								}
							}
							HoverShape = hs;
							if (hs != null) {
								HoverShape.HasHoverPoint (mousePos.Value);
								redraw = true;
							}
						}
					}
				} else if (button == MouseButton.Left) {
					if (mods.HasFlag (Modifier.Control)) {
						CurrentShape = hoverShape;
						if (CurrentShape != null) {
							if (SelectedShapes.Contains(CurrentShape))
								SelectedShapes.Remove (CurrentShape);
							else
								SelectedShapes.Add (CurrentShape);
						}
					}
					if (hoverShape == null)
						CurrentShape = null;
					else if (mods.HasFlag (Modifier.Shift)){
						if (CurrentShape != null && !SelectedShapes.Contains(CurrentShape))
							SelectedShapes.Add (CurrentShape);
						CurrentShape = hoverShape;
						if (!SelectedShapes.Contains(CurrentShape))
							SelectedShapes.Add (CurrentShape);
					} else {
						CurrentShape = hoverShape;
						if (!SelectedShapes.Contains (CurrentShape))
							SelectedShapes.Clear();
					}
				}
			} else {
				if (newShape == null) {
					if (button == MouseButton.Right) {
						CurrentDrawMode = DrawMode.Select;
					} else {
						switch (currentDrawMode) {
							case DrawMode.Rect:
								newShape = new Rectangle (mousePos.Value);
								break;
							case DrawMode.Circle:
								newShape = new Circle (mousePos.Value);
								break;
							case DrawMode.Ellipse:
								newShape = new Ellipse (mousePos.Value);
								break;
							case DrawMode.Lines:
								newShape = new Path (mousePos.Value);
								break;
							case DrawMode.PathTest:
								newShape = new PathTest (mousePos.Value);
								break;
							case DrawMode.Bezier:
								newShape = new Bezier (mousePos.Value);
								break;
							case DrawMode.Quadratic:
								//newShape = new Quadratic (mousePos.Value);
								Quadratic q = new Quadratic (new PointD (100,250));
								q.AddPoint (new PointD (250,100));
								q.AddPoint (new PointD (400,250));
								Shapes.Add (q);
								break;
							case DrawMode.EllipticalArc:
								/*EllipticalArc ea = new EllipticalArc (mousePos.Value);
								ea.AddPoint (mousePos.Value + new PointD (100,10));
								ea.AddPoint (mousePos.Value + new PointD (30,40));
								Shapes.Add (ea);*/
								//newShape = new CenteredEllipseArc (mousePos.Value);
								EllipticalArc ea = new EllipticalArc (mousePos.Value);
								Shapes.Add (ea);
								loadWindow ("#ui.EllipticArc.crow").DataSource = ea;
								break;
						}
					}
				} else if (newShape.OnCreateMouseDown (button, mousePos.Value))
					finishCurrentShape();
			}

			redraw = true;
		}
		protected override void onMouseButtonUp(MouseButton button, Modifier mods)
		{
			base.onMouseButtonUp(button, mods);

			if (MouseIsInInterface || newShape == null)
				return;

			if (newShape.OnCreateMouseUp (button, mousePos.Value))
				finishCurrentShape ();

			redraw = true;
		}
		void finishCurrentShape () {
			Shapes.Add (newShape);
			newShape = null;
		}
		protected override void onMouseMove(double xPos, double yPos)
		{
			PointD lastM = new PointD (lastMouseX, lastMouseY);
			base.onMouseMove(xPos, yPos);

			if (MouseIsInInterface) {
				mousePos = null;
				return;
			}

			if (mousePos == null) {
				if (currentDrawMode == DrawMode.Select) {
					SetCursor (CursorShape.Arrow);
				} else
					SetCursor (CursorShape.Crosshair);
			}

			mousePos = new PointD (xPos, yPos);

			if (currentDrawMode == DrawMode.Select) {
				if (this.GetButton (MouseButton.Left) == InputAction.Press) {
					if (currentShape != null) {
						if (SelectedShapes.Count > 0) {
							foreach (Drawable shape in SelectedShapes)
								shape.Move (mousePos.Value - lastM);
						} else
							currentShape.Move (mousePos.Value - lastM);
						redraw = true;
					}
				} else if (this.GetButton (MouseButton.Right) == InputAction.Press) {
				}else {
					using (vkvg.Context ctx = new vkvg.Context(vkvgSurf)) {
						if (hoverShape != null && hoverShape.Contains (ctx, mousePos.Value)) {
							HoverShape.HasHoverPoint (mousePos.Value);
							redraw = true;
							return;
						}
						Drawable hs = null;
						foreach (Drawable shape in ShapesInReverseOrder) {
							if (shape.Contains (ctx, mousePos.Value)) {
								hs = shape;
								break;
							}
						}
						HoverShape = hs;
						if (hs != null) {
							HoverShape.HasHoverPoint (mousePos.Value);
							redraw = true;
						}
					}
				}
				return;
			}

			if (newShape != null)
				redraw = true;
		}
		protected override void onKeyDown(Key key, int scanCode, Modifier modifiers)
		{
			switch (key) {
			case Key.Delete:
				if (SelectedShapes.Count > 0) {
					Shapes.RemoveAll (s=>SelectedShapes.Contains(s));
					SelectedShapes.Clear();
				} else if (currentShape != null) {
					Shapes.Remove (currentShape);
				} else
					break;
				CurrentShape = null;
				HoverShape = null;
				redraw = true;
				break;
			case Key.G:
				if (SelectedShapes.Count > 0) {
					Group grp = new Group (SelectedShapes);
					Shapes.RemoveAll (s=>SelectedShapes.Contains(s));
					SelectedShapes.Clear();
					Shapes.Add (grp);
					CurrentShape = grp;
					HoverShape = null;
					redraw = true;
				}
				break;
			default:
				base.onKeyDown(key, scanCode, modifiers);
				break;
			}
		}
		protected override void OnResize()
		{
			base.OnResize();
			redraw = true;
		}
	}
}
