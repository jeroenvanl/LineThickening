using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TranslateToFloorplan
{
    public class TranslateToFloorplanComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TranslateToFloorplanComponent()
          : base("Thicken Walls", "ThickenW",
              "Thickens walls with width. Works with orthogonal lines and polylines",
              "FPM", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Outer walls apartment", "C", "Outer walls apartment. Area of Polyline should be between 20 and 150 sqm for best results", GH_ParamAccess.list);
            pManager.AddNumberParameter("Width interior walls", "W", "Width interior walls", GH_ParamAccess.item);
            pManager[0].DataMapping = GH_DataMapping.Flatten;
            pManager[1].Optional = true;
            
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Best Fitting Boundary", "F", "Floorplan", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> curveList = new List<Curve>();
            DA.GetDataList(0, curveList);
            //GH_Structure<GH_Activ> in_IDtree = new GH_Structure<Curve>();

            double width = new double();
            if(!DA.GetData(1, ref width))
            {
                width = .1;
            }
            double halfwidth = width / 2.0;

            CurveOffsetCornerStyle cornerstyle = new CurveOffsetCornerStyle();
            List<PolylineCurve> plines = new List<PolylineCurve>();
            Vector3d normal = new Vector3d(0.0, 0.0, 1.0);
            Point3d origin = new Point3d(0.0, 0.0, 0.0);
            Plane plane = new Plane(origin, normal);
            foreach (Curve c in curveList)
            {
                Polyline pl = new Polyline();
                if(c.TryGetPolyline(out pl))
                {
                    int segCount = pl.SegmentCount;
                    Line[] segments = pl.GetSegments();
                    foreach(Line l in segments)
                    {
                        l.Extend(halfwidth, halfwidth);
                        Curve crv = l.ToNurbsCurve();
                        Curve[] offset1 = crv.Offset(plane, halfwidth, 0.0, cornerstyle);
                        Curve[] offset2 = crv.Offset(plane, -halfwidth, 0.0, cornerstyle);
                        Point3d[] rect = new Point3d[5] { offset1[0].PointAtStart, offset1[0].PointAtEnd, offset2[0].PointAtEnd, offset2[0].PointAtStart, offset1[0].PointAtStart};
                        PolylineCurve plc = new PolylineCurve(rect);
                        plines.Add(plc);
                    }
                }
            }

            // 2. Boolean Union
            Curve[] final =PolylineCurve.CreateBooleanUnion(plines);

            // 3. Set data back
            DA.SetDataList(0, final);
           
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{22136950-0833-4e89-baf5-eccfaae0dd87}"); }
        }
    }
}
