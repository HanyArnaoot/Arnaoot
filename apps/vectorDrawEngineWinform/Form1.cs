using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Elements;
using Arnaoot.VectorGraphics.Scene;
using Arnaoot.VectorGraphics.UI;

namespace vectorDrawEngineWinform
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //string FileName = Application.StartupPath + "\\alexandria port.svg";
            // vectorDrawEngine1.LoadSVGFile(FileName);
            //vectorDrawEngine1.ZoomExtents(5);
            //Arnaoot.VectorGraphics.Formats.FlatFile.Import ff= new Arnaoot.VectorGraphics.Formats.FlatFile.Import;
            //ff.AddfromFlatFile("");
            Arnaoot.VectorGraphics.UI.EngineControl MyDataDisplayer = new Arnaoot.VectorGraphics.UI.EngineControl();
          ILayer MapLayer=  MyDataDisplayer.Document.Layers.AddLayer ("Map");
            MapLayer.AddElement ( new LineElement (  new Vector3D (10,5,20), new Vector3D(30, 40, 50),false,1,Arnaoot.VectorGraphics.Abstractions.ArgbColor.Black) , false); // wireframe only
            MapLayer.AddElement( new CircleElement(new Vector3D(0, 0, 0), // Center
                    10, // Initial Radius 
                    false,              // not fixed radius in pixel
                    1,                  // width or ID
                    ArgbColor.Black,     // Color
                    ArgbColor.Black,
                    false,
                    new Vector3D(0, 0, 1),
                    true
                ),false ); // wireframe only
            MapLayer.RebuildBounds();
            MyDataDisplayer.ZoomExtents(5f);

            //
            MyDataDisplayer.Dock = DockStyle.Fill;
            this.Controls.Add(MyDataDisplayer);

        }
    }
}
