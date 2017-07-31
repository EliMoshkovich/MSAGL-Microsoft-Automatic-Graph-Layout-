using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Layout.MDS;
using Color = Microsoft.Msagl.Drawing.Color;
using Label = Microsoft.Msagl.Drawing.Label;
using MouseButtons = System.Windows.Forms.MouseButtons;
using Node = Microsoft.Msagl.Core.Layout.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace WindowsApplicationSample {
    public partial class Form1 : Form {
        readonly ToolTip toolTip1 = new ToolTip();
        object selectedObject;
        AttributeBase selectedObjectAttr;

        public Form1() {
            Load += Form1Load;
            InitializeComponent();
            gViewer.MouseWheel += GViewerMouseWheel;
            toolTip1.Active = true;
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            gViewer.LayoutEditor.DecorateObjectForDragging = SetDragDecorator;
            gViewer.LayoutEditor.RemoveObjDraggingDecorations = RemoveDragDecorator;
            gViewer.MouseDown += WaMouseDown;
            gViewer.MouseUp += WaMouseUp;
            gViewer.MouseMove += GViewerOnMouseMove;
        }

        void GViewerOnMouseMove(object sender, MouseEventArgs mouseEventArgs) {
            if (labelToChange == null) return;
            labelToChange.Text = MousePosition.ToString();
            if (viewerEntityCorrespondingToLabelToChange == null) {
                foreach (var e in gViewer.Entities) {
                    if (e.DrawingObject == labelToChange) {
                        viewerEntityCorrespondingToLabelToChange = e;
                        break;
                    }
                }
            }
            if (viewerEntityCorrespondingToLabelToChange == null) return;
            var rect = labelToChange.BoundingBox;
            var font = new Font(labelToChange.FontName, (int) labelToChange.FontSize);
            double width;
            double height;
            StringMeasure.MeasureWithFont(labelToChange.Text, font, out width, out height);

            if (width <= 0)
                //this is a temporary fix for win7 where Measure fonts return negative lenght for the string " "
                StringMeasure.MeasureWithFont("a", font, out width, out height);

            labelToChange.Width = width;
            labelToChange.Height = height;
            rect.Add(labelToChange.BoundingBox);
            gViewer.Invalidate(gViewer.MapSourceRectangleToScreenRectangle(rect));
        }

        void WaMouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                myMouseUpPoint = e.Location;
        }

        void WaMouseDown(object sender, MouseEventArgs e) {
            if(e.Button==MouseButtons.Left)
                myMouseDownPoint = e.Location;
        }

        readonly Dictionary<object, Color> draggedObjectOriginalColors=new Dictionary<object, Color>();
        System.Drawing.Point myMouseDownPoint;
        System.Drawing.Point myMouseUpPoint;

        void SetDragDecorator(IViewerObject obj) {
            var dNode = obj as DNode;
            if (dNode != null) {
                draggedObjectOriginalColors[dNode] = dNode.DrawingNode.Attr.Color;
                dNode.DrawingNode.Attr.Color = Color.Magenta;
                gViewer.Invalidate(obj);
            }
        }

        void RemoveDragDecorator(IViewerObject obj) {
            var dNode = obj as DNode;
            if (dNode != null) {
                dNode.DrawingNode.Attr.Color = draggedObjectOriginalColors[dNode];
                draggedObjectOriginalColors.Remove(obj);
                gViewer.Invalidate(obj);
            }
        }

        void GViewerMouseWheel(object sender, MouseEventArgs e) {
            int delta = e.Delta;
            if (delta != 0)
                gViewer.ZoomF *= delta < 0 ? 0.9 : 1.1;
        }

        void Form1Load(object sender, EventArgs e) {
            gViewer.ObjectUnderMouseCursorChanged += GViewerObjectUnderMouseCursorChanged;

#if DEBUG
            DisplayGeometryGraph.SetShowFunctions();
#endif
            CreateGraph();
        }

        void GViewerObjectUnderMouseCursorChanged(object sender, ObjectUnderMouseCursorChangedEventArgs e) {
            selectedObject = e.OldObject != null ? e.OldObject.DrawingObject : null;

            if (selectedObject != null) {
                RestoreSelectedObjAttr();
                gViewer.Invalidate(e.OldObject);
                selectedObject = null;
            }

            if (gViewer.SelectedObject == null) {
                label1.Text = "No object under the mouse";
                gViewer.SetToolTip(toolTip1, "");
            } else {
                selectedObject = gViewer.SelectedObject;
                var edge = selectedObject as Edge;
                if (edge != null) {
                    selectedObjectAttr = edge.Attr.Clone();
                   edge.Attr.Color = Color.Blue;
                    gViewer.Invalidate(e.NewObject);

                    //  here we can use e.Attr.Id or e.UserData to get back to the user data
                    gViewer.SetToolTip(toolTip1, String.Format("edge from {0} to {1}", edge.Source, edge.Target));
                } else if (selectedObject is Microsoft.Msagl.Drawing.Node) {
                    selectedObjectAttr = (gViewer.SelectedObject as Microsoft.Msagl.Drawing.Node).Attr.Clone();
                    (selectedObject as Microsoft.Msagl.Drawing.Node).Attr.Color = Color.Green;
                    //  here you can use e.Attr.Id to get back to your data
                    gViewer.SetToolTip(toolTip1,
                                       String.Format("node {0}",
                                                     (selectedObject as Microsoft.Msagl.Drawing.Node).Attr.Id));
                    gViewer.Invalidate(e.NewObject);
                }
                label1.Text = selectedObject.ToString();
            }
           
        }

        void RestoreSelectedObjAttr() {
            var edge = selectedObject as Edge;
            if (edge != null) {
                edge.Attr = (EdgeAttr) selectedObjectAttr;
            }
            else {
                var node = selectedObject as Microsoft.Msagl.Drawing.Node;
                if (node != null)
                    node.Attr = (NodeAttr)selectedObjectAttr;

            }
            
        }

        void Button1Click(object sender, EventArgs e) {
            CreateGraph();
        }

        Label labelToChange;
        IViewerObject viewerEntityCorrespondingToLabelToChange;

        void CreateGraph() {
#if DEBUG
            DisplayGeometryGraph.SetShowFunctions();
#endif

            // * * * * * * * * * * *  start creation the graph task * * * * * * * * * * *  

            SQL_Manager sql = new SQL_Manager();
            List<ObjectTable> list = new List<ObjectTable>();

            //SQL connection
            if ((list = sql.getObjectTableList()) == null)
            {
                MessageBox.Show("Error to get Object for drawing");
                return;
            }

            List<Microsoft.Msagl.Drawing.Node> nodesList = new List<Microsoft.Msagl.Drawing.Node>();

            Graph graph = new Graph();

            var subgraph = new Subgraph("Adventure 2014");
            graph.RootSubgraph.AddSubgraph(subgraph);

            //create the graph content
            foreach (var itemA in list)
            {
                // get all the keys of the table name
                string str = "" + itemA.TableName + ":\n"; // add table name, for the uniqe id
                string keys = "";
                foreach (string key in itemA.primary_Keys) keys += key + '\n';
                str += keys;

                // add the node to graph and remove the edge
                var e = graph.AddEdge(str, str);
                graph.RemoveEdge(e);

                // create subgraph for the table name
                var table_as_subgraph = new Subgraph(itemA.TableName);
                table_as_subgraph.Attr.Color = Color.Black;
                table_as_subgraph.Attr.FillColor = Color.Yellow;

                // add the node to the subgraph
                var n = graph.FindNode(str);
                n.Attr.FillColor = Color.Cyan;
                table_as_subgraph.AddNode(n);
                n.LabelText = keys; // udate the text without table name

                // add the subgraph to the root subgraph
                subgraph.AddSubgraph(table_as_subgraph);
                
                //graph.Attr.LayerDirection = LayerDirection.LR;
            }

            // add the edges between the correct table object (subgraph) according to fk to pk
            foreach (var itemA in list)
            {
                foreach (var col in itemA.forigen_Keys)
                {
                    foreach (var itemB in list)
                    {
                        if (itemA != itemB && itemB.primary_Keys.Contains(col))
                        {
                            graph.AddEdge(itemA.TableName, itemB.TableName);
                        }

                    }
                }
            }

            // * * * * * * * * * * *  end creation the graph task * * * * * * * * * * * 

            //layout the graph and draw it
            gViewer.Graph = graph;
                //this.propertyGrid1.SelectedObject = graph;
            }

            void RecalculateLayoutButtonClick(object sender, EventArgs e) {
                //gViewer.Graph = propertyGrid1.SelectedObject as Graph;
            }


            bool MouseDownPointAndMouseUpPointsAreFarEnough()
            {
                double dx = myMouseDownPoint.X - myMouseUpPoint.X;
                double dy = myMouseDownPoint.Y - myMouseUpPoint.Y;
            
                return dx*dx + dy*dy >= 25; //so 5X5 pixels already give something
            }

            void ShowObjectsInTheLastRectClick(object sender, EventArgs e) {
                string message;
                if(gViewer.Graph==null) {
                    message = "there is no graph";
                }else {
                    if (MouseDownPointAndMouseUpPointsAreFarEnough()) {
                        var p0 = gViewer.ScreenToSource(myMouseDownPoint);
                        var p1 = gViewer.ScreenToSource(myMouseUpPoint);
                        var rubberRect = new Microsoft.Msagl.Core.Geometry.Rectangle(p0, p1);
                        var stringB = new StringBuilder();
                        foreach (var node in gViewer.Graph.Nodes)
                            if (rubberRect.Contains(node.BoundingBox))
                                stringB.Append(node.LabelText + "\n");

                        foreach (var edge in gViewer.Graph.Edges)
                            if (rubberRect.Contains(edge.BoundingBox))
                                stringB.Append(String.Format("edge from {0} to {1}\n", edge.SourceNode.LabelText,
                                                             edge.TargetNode.LabelText));

                        message = stringB.ToString();
                    }
                    else
                        message = "the window is not defined";
                }

                MessageBox.Show(message);

            }
    }
}