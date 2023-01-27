// Copyright (c) 2021 Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using Glfw;
using vkvg;
using PointD = Drawing2D.PointD;
using RectangleD = Drawing2D.RectangleD;
using static System.Math;

namespace VkvgPainter
{
	public class BezierTest : Shape
	{
		public BezierTest(PointD position) : base (position)	{

		}
		public override void EmitPath(Context ctx, PointD? mouse = null)
		{
			if (mouse.HasValue) {
				PointD m = mouse.Value;
				switch (Points.Count) {
				case 1:
					test_bezier (ctx, Points[0].X, Points[0].Y, Points[0].X, m.Y, m.X, m.Y, m.X, Points[0].Y);
					break;
				case 2:
					test_bezier (ctx, Points[0].X, Points[0].Y, Points[1].X, Points[1].Y, m.X, m.Y, m.X, m.Y);
					break;
				case 3:
					test_bezier (ctx, Points[0].X, Points[0].Y, Points[1].X, Points[1].Y, m.X, m.Y, Points[2].X, Points[2].Y);
					break;
				}
			} else
				test_bezier (ctx, 
					Points[0].X,
					Points[0].Y,
					Points[1].X,
					Points[1].Y,
					Points[3].X,
					Points[3].Y,
					Points[2].X,
					Points[2].Y
				);
		}
		protected void drawControlPoint (Context ctx, PointD p, double r, double g, double b) {
			ctx.SetSource (r, g, b, 0.6);
			ctx.Arc (p.X, p.Y, selRadius, 0, Math.PI * 2);
			ctx.FillPreserve ();
			ctx.SetSource (r, g, b, 0.9);
			ctx.Stroke ();
		}
		public override void DrawPoints(Context ctx)
		{
			ctx.LineWidth = 1;
			for (int i = 0; i < Points.Count; i+=2) {
				if (i == selectedPoint)
					drawPoint (ctx, Points [i], 1, 0.1, 0.1);
				else
					drawPoint (ctx, Points [i], 0.1, 0.1, 1);
				ctx.SetSource (0.2,0.2,0.2);
				ctx.MoveTo (Points[i]);
				if (i == Points.Count - 1)
					break;
				ctx.LineTo (Points[i+1]);
				ctx.Stroke ();
				if (i+1 == selectedPoint)
					drawControlPoint (ctx, Points [i+1], 1, 0.1, 0.1);
				else
					drawControlPoint (ctx, Points [i+1], 0.1, 0.1, 1);
			}
		}
		public override RectangleD GetExtents(Context ctx)
		{
			ctx.MoveTo (Points[0]);
			for (int i = 1; i < Points.Count; i++)
				ctx.LineTo (Points[i]);
			ctx.PathExtents (out float x1, out float y1, out float x2, out float y2);
			ctx.NewPath ();
			RectangleD r = new RectangleD (x1, y1, x2 - x1, y2 -y1);
			if (HasStroke)
				r.Inflate ((float)LineWidth / 2);
			return r;
		}
		public override void Move(PointD delta)
		{
			base.Move(delta);
		}
		public override bool OnCreateMouseDown(MouseButton button, PointD m)
		{
			AddPoint (new PointD (m.X, m.Y));
			return false;
		}
		public override bool OnCreateMouseUp(MouseButton button, PointD m)
		{
			if (Points.Count == 1) {
				AddPoint (new PointD (Points[0].X, m.Y));
				return false;
			}
			AddPoint (new PointD (m.X, m.Y));
			return true;
		}

		void test_bezier (Context ctx, double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3) {
			ctx.SetSource (1,0,0,0.7);
			ctx.MoveTo (x0, y0);
			_recursive_bezier (ctx, x0,y0,x1,y1,x2,y2,x3,y3,0);
			ctx.LineTo (x3,y3);
		}

		double M_APPROXIMATION_SCALE			= 1.0f;
		double M_ANGLE_TOLERANCE				= 0.01f;
		double M_CUSP_LIMIT					= 0.01f;
		uint CURVE_RECURSION_LIMIT			= 8;
		double CURVE_COLLINEARITY_EPSILON	= 0.8f;
		double CURVE_ANGLE_TOLERANCE_EPSILON = 0.001f;
		double CURVE_DISTANCE_TOLERANCE		= 0.003f;

		public double ApproximationScale {
			get => M_APPROXIMATION_SCALE;
			set {
				if (M_APPROXIMATION_SCALE == value)
					return;
				M_APPROXIMATION_SCALE = value;
				NotifyValueChanged (M_APPROXIMATION_SCALE);
				Program.redraw = true;
			} 
		}
		public double AngleTolerance {
			get => M_ANGLE_TOLERANCE;
			set {
				if (M_ANGLE_TOLERANCE == value)
					return;
				M_ANGLE_TOLERANCE = value;
				NotifyValueChanged (M_ANGLE_TOLERANCE);
				Program.redraw = true;
			} 
		}
		public double CupsLimit {
			get => M_CUSP_LIMIT;
			set {
				if (M_CUSP_LIMIT == value)
					return;
				M_CUSP_LIMIT = value;
				NotifyValueChanged (M_CUSP_LIMIT);
				Program.redraw = true;
			} 
		}
		public double CollinearityEpsilon {
			get => CURVE_COLLINEARITY_EPSILON;
			set {
				if (CURVE_COLLINEARITY_EPSILON == value)
					return;
				CURVE_COLLINEARITY_EPSILON = value;
				NotifyValueChanged (CURVE_COLLINEARITY_EPSILON);
				Program.redraw = true;
			} 
		}
		public double AngleToleranceEpsilon {
			get => CURVE_ANGLE_TOLERANCE_EPSILON;
			set {
				if (CURVE_ANGLE_TOLERANCE_EPSILON == value)
					return;
				CURVE_ANGLE_TOLERANCE_EPSILON = value;
				NotifyValueChanged (CURVE_ANGLE_TOLERANCE_EPSILON);
				Program.redraw = true;
			} 
		}
		public double DistanceTolerance {
			get => CURVE_DISTANCE_TOLERANCE;
			set {
				if (CURVE_DISTANCE_TOLERANCE == value)
					return;
				CURVE_DISTANCE_TOLERANCE = value;
				NotifyValueChanged (CURVE_DISTANCE_TOLERANCE);
				Program.redraw = true;
			} 
		}						
		void _add_point (Context ctx, double x, double y) {
			ctx.Arc (x, y, 0.5, 0, Math.PI * 2);
		}
		void _recursive_bezier (Context ctx, 
								double x1, double y1, double x2, double y2,
								double x3, double y3, double x4, double y4,
								uint level) {
			if(level > CURVE_RECURSION_LIMIT)
			{
				return;
			}

			// Calculate all the mid-points of the line segments
			//----------------------
			double x12	= (x1 + x2) / 2;
			double y12	= (y1 + y2) / 2;
			double x23	= (x2 + x3) / 2;
			double y23	= (y2 + y3) / 2;
			double x34	= (x3 + x4) / 2;
			double y34	= (y3 + y4) / 2;
			double x123	= (x12 + x23) / 2;
			double y123	= (y12 + y23) / 2;
			double x234	= (x23 + x34) / 2;
			double y234	= (y23 + y34) / 2;
			double x1234 = (x123 + x234) / 2;
			double y1234 = (y123 + y234) / 2;

			if(level > 0) // Enforce subdivision first time
			{
				// Try to approximate the full cubic curve by a single straight line
				//------------------
				double dx = x4-x1;
				double dy = y4-y1;

				double d2 = Abs(((x2 - x4) * dy - (y2 - y4) * dx));
				double d3 = Abs(((x3 - x4) * dy - (y3 - y4) * dx));

				double da1, da2;

				if(d2 > CURVE_COLLINEARITY_EPSILON && d3 > CURVE_COLLINEARITY_EPSILON)
				{
					// Regular care
					//-----------------
					if((d2 + d3)*(d2 + d3) <= (dx*dx + dy*dy) * CURVE_DISTANCE_TOLERANCE)
					{
						// If the curvature doesn't exceed the distance_tolerance value
						// we tend to finish subdivisions.
						//----------------------
						if (M_ANGLE_TOLERANCE < CURVE_ANGLE_TOLERANCE_EPSILON) {
							_add_point(ctx, x1234, y1234);
							return;
						}

						// Angle & Cusp Condition
						//----------------------
						double a23 = Atan2(y3 - y2, x3 - x2);
						da1 = Abs(a23 - Atan2(y2 - y1, x2 - x1));
						da2 = Abs(Atan2(y4 - y3, x4 - x3) - a23);
						if(da1 >= PI) da1 = 2.0/PI - da1;
						if(da2 >= PI) da2 = 2.0/PI - da2;

						if(da1 + da2 < (double)M_ANGLE_TOLERANCE)
						{
							// Finally we can stop the recursion
							//----------------------
							_add_point (ctx, x1234, y1234);
							return;
						}

						if(M_CUSP_LIMIT != 0.0)
						{
							if(da1 > M_CUSP_LIMIT)
							{
								_add_point (ctx, x2, y2);
								return;
							}

							if(da2 > M_CUSP_LIMIT)
							{
								_add_point (ctx, x3, y3);
								return;
							}
						}
					}
				} else {
					if(d2 > CURVE_COLLINEARITY_EPSILON)
					{
						// p1,p3,p4 are collinear, p2 is considerable
						//----------------------
						if(d2 * d2 <= CURVE_DISTANCE_TOLERANCE * (dx*dx + dy*dy))
						{
							if(M_ANGLE_TOLERANCE < CURVE_ANGLE_TOLERANCE_EPSILON)
							{
								_add_point (ctx, x1234, y1234);
								return;
							}

							// Angle Condition
							//----------------------
							da1 = Abs(Atan2(y3 - y2, x3 - x2) - Atan2(y2 - y1, x2 - x1));
							if(da1 >= PI) da1 = 2.0/PI - da1;

							if(da1 < M_ANGLE_TOLERANCE)
							{
								_add_point (ctx, x2, y2);
								_add_point (ctx, x3, y3);
								return;
							}

							if(M_CUSP_LIMIT != 0.0)
							{
								if(da1 > M_CUSP_LIMIT)
								{
									_add_point (ctx, x2, y2);
									return;
								}
							}
						}
					} else if(d3 > CURVE_COLLINEARITY_EPSILON) {
						// p1,p2,p4 are collinear, p3 is considerable
						//----------------------
						if(d3 * d3 <= CURVE_DISTANCE_TOLERANCE* (dx*dx + dy*dy))
						{
							if(M_ANGLE_TOLERANCE < CURVE_ANGLE_TOLERANCE_EPSILON)
							{
								_add_point (ctx, x1234, y1234);
								return;
							}

							// Angle Condition
							//----------------------
							da1 = Abs(Atan2(y4 - y3, x4 - x3) - Atan2(y3 - y2, x3 - x2));
							if(da1 >= PI) da1 = 2.0/PI - da1;

							if(da1 < M_ANGLE_TOLERANCE)
							{
								_add_point (ctx, x2, y2);
								_add_point (ctx, x3, y3);
								return;
							}

							if(M_CUSP_LIMIT != 0.0)
							{
								if(da1 > M_CUSP_LIMIT)
								{
									_add_point (ctx, x3, y3);
									return;
								}
							}
						}
					}
					else
					{
						// Collinear case
						//-----------------
						dx = x1234 - (x1 + x4) / 2;
						dy = y1234 - (y1 + y4) / 2;
						if(dx*dx + dy*dy <= CURVE_DISTANCE_TOLERANCE)
						{
							_add_point (ctx, x1234, y1234);
							return;
						}
					}
				}
			}

			// Continue subdivision
			//----------------------
			_recursive_bezier (ctx, x1, y1, x12, y12, x123, y123, x1234, y1234, level + 1);
			_recursive_bezier (ctx, x1234, y1234, x234, y234, x34, y34, x4, y4, level + 1);
		}		
	}
}
