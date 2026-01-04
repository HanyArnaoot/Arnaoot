
using ExCSS;
using System.Reflection.Metadata;
using Color = System.Drawing.Color;
using FontStyle = System.Drawing.FontStyle;
using Point = System.Drawing.Point;

namespace Arnaoot.VectorGraphics.UI
{
     partial class EngineControl : UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                _document?.Dispose();
                 _rasterizedImageBuffer?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        #region Declarations  of controls
        internal ToolStrip ToolStripMain;
        internal StatusStrip StatusStripMain;
        internal Panel PanelRightTools;
        internal Panel PanelBottomControls;
        internal Splitter SplitterHorizontal;

        // File operations
        internal ToolStripButton BtnLoad;
        internal ToolStripButton BtnSave;
        internal ToolStripButton BtnExport;
        internal ToolStripSeparator Sep1;

        // Edit operations
        internal ToolStripButton BtnUndo;
        internal ToolStripButton BtnRedo;
        internal ToolStripSeparator Sep2;

        // Drawing tools
        internal ToolStripButton BtnSelectTool;
        internal ToolStripButton BtnLineTool;
        internal ToolStripButton BtnCircleTool;
        internal ToolStripButton BtnRectangleTool;
        internal ToolStripButton BtnTextTool;
        internal ToolStripSeparator Sep3;
        internal ToolStripSeparator Sep4;

        // Toggle options
        internal ToolStripButton BtnToggleGrid;
        internal ToolStripButton BtnToggleAxes;

        // Color picker
        internal Button BtnColorPicker;
        internal TrackBar TrackBarXRotation;
        internal TrackBar TrackBarYRotation;
        internal TrackBar TrackBarZRotation;
        internal Label LblXRotation;
        internal Label LblYRotation;
        internal Label LblZRotation;

        // Status bar
        internal ToolStripStatusLabel StatusCoordinates;
        internal ToolStripStatusLabel StatusZoom;
        private ToolStripButton BtnOrbitPanel;
        private ToolStripButton openToolStripButton;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripButton toolStripButton_New_file;
        private TextBox TxtLabelInput;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Label label1;
        private TableLayoutPanel tableLayoutPanel3;
        private Button btnWorldShiftYDec;
        private Button btnWorldShiftXDec;
        private Button btnWorldShiftYInc;
        private Button btnWorldShiftXInc;
        private Button BtnResetRotation;
        private Label label2;
        private Button btnWorldShiftZDec;
        private Button btnWorldShiftZInc;
        private TableLayoutPanel tableLayoutPanel4;
        private Button BtnResetWorldShift;
        private Label lblworldShift;
        private Label label3;
        private ToolStripDropDownButton toolStripDropDownButtonZoom;
        private ToolStripDropDownButton toolStripDropDownButtonLayers;
        private ToolStripMenuItem addNewLayerToolStripMenuItem;
        private ToolStripDropDownButton toolStripDropDownButtonFile;
        private ToolStripDropDownButton toolStripDropDownButtonItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem chooseColorToolStripMenuItem;
        private ColorDialog colorDialog1;
        private ToolStripMenuItem setLayerVisibilityToolStripMenuItem;
        private ToolStripMenuItem deleteLayerToolStripMenuItem;
        private ToolStripMenuItem setActiveLayerToolStripMenuItem;
        internal ToolStripStatusLabel StatusTool;


        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EngineControl));
            ToolStripMain = new ToolStrip();
            toolStripDropDownButtonFile = new ToolStripDropDownButton();
            toolStripButton_New_file = new ToolStripButton();
            BtnLoad = new ToolStripButton();
            openToolStripButton = new ToolStripButton();
            BtnSave = new ToolStripButton();
            BtnExport = new ToolStripButton();
            Sep1 = new ToolStripSeparator();
            BtnOptions = new ToolStripDropDownButton();
            BtnRenderMethodMenuItem = new ToolStripMenuItem();
            BtnSetRenderGDI = new ToolStripMenuItem();
            BtnSetRenderSkia = new ToolStripMenuItem();
            BtnSetRenderSVGFile = new ToolStripMenuItem();
            toolStripDropDownButtonItem = new ToolStripDropDownButton();
            BtnLineTool = new ToolStripButton();
            BtnCircleTool = new ToolStripButton();
            BtnRectangleTool = new ToolStripButton();
            BtnTextTool = new ToolStripButton();
            chooseColorToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            BtnOrbitPanel = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            BtnUndo = new ToolStripButton();
            BtnRedo = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripDropDownButtonZoom = new ToolStripDropDownButton();
            BtnZoomIn = new ToolStripMenuItem();
            BtnZoomFit = new ToolStripMenuItem();
            BtnZoomOut = new ToolStripMenuItem();
            MenuItemZoomPrevious = new ToolStripMenuItem();
            BtnPan = new ToolStripButton();
            toolStripDropDownButtonLayers = new ToolStripDropDownButton();
            addNewLayerToolStripMenuItem = new ToolStripMenuItem();
            deleteLayerToolStripMenuItem = new ToolStripMenuItem();
            setActiveLayerToolStripMenuItem = new ToolStripMenuItem();
            setLayerVisibilityToolStripMenuItem = new ToolStripMenuItem();
            Sep2 = new ToolStripSeparator();
            BtnSelectTool = new ToolStripButton();
            Sep3 = new ToolStripSeparator();
            BtnToggleGrid = new ToolStripButton();
            BtnToggleAxes = new ToolStripButton();
            toolStripSeparator = new ToolStripSeparator();
            toolStripLabelFileName = new ToolStripLabel();
            Sep4 = new ToolStripSeparator();
            PanelRightTools = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            lblworldShift = new Label();
            BtnResetWorldShift = new Button();
            tableLayoutPanel4 = new TableLayoutPanel();
            btnWorldShiftZDec = new Button();
            btnWorldShiftZInc = new Button();
            label3 = new Label();
            tableLayoutPanel3 = new TableLayoutPanel();
            btnWorldShiftYDec = new Button();
            btnWorldShiftXDec = new Button();
            btnWorldShiftYInc = new Button();
            btnWorldShiftXInc = new Button();
            label2 = new Label();
            tableLayoutPanel2 = new TableLayoutPanel();
            label1 = new Label();
            TrackBarZRotation = new TrackBar();
            LblZRotation = new Label();
            TrackBarYRotation = new TrackBar();
            LblYRotation = new Label();
            TrackBarXRotation = new TrackBar();
            LblXRotation = new Label();
            BtnResetRotation = new Button();
            BtnColorPicker = new Button();
            StatusStripMain = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            StatusLabelTotalMemory = new ToolStripStatusLabel();
            StatusMemoryUsed = new ToolStripStatusLabel();
            StatusLabelRenderTime = new ToolStripStatusLabel();
            StatusTool = new ToolStripStatusLabel();
            StatusZoom = new ToolStripStatusLabel();
            StatusCoordinates = new ToolStripStatusLabel();
            TxtLabelInput = new TextBox();
            colorDialog1 = new ColorDialog();
            toolStripSeparator4 = new ToolStripSeparator();
            ToolStripMain.SuspendLayout();
            PanelRightTools.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TrackBarZRotation).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TrackBarYRotation).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TrackBarXRotation).BeginInit();
            StatusStripMain.SuspendLayout();
            SuspendLayout();
            // 
            // ToolStripMain
            // 
            ToolStripMain.GripStyle = ToolStripGripStyle.Hidden;
            ToolStripMain.Items.AddRange(new ToolStripItem[] { toolStripDropDownButtonFile, Sep1, BtnOptions, toolStripDropDownButtonItem, toolStripSeparator1, BtnOrbitPanel, toolStripSeparator2, BtnUndo, BtnRedo, toolStripSeparator3, toolStripDropDownButtonZoom, toolStripDropDownButtonLayers, Sep2, BtnSelectTool, Sep3, BtnToggleGrid, BtnToggleAxes, toolStripSeparator, toolStripLabelFileName });
            ToolStripMain.Location = new Point(0, 0);
            ToolStripMain.Name = "ToolStripMain";
            ToolStripMain.Padding = new Padding(8, 4, 8, 4);
            ToolStripMain.RenderMode = ToolStripRenderMode.Professional;
            ToolStripMain.Size = new Size(973, 31);
            ToolStripMain.TabIndex = 0;
            // 
            // toolStripDropDownButtonFile
            // 
            toolStripDropDownButtonFile.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButtonFile.DropDownItems.AddRange(new ToolStripItem[] { toolStripButton_New_file, BtnLoad, openToolStripButton, BtnSave, BtnExport });
            toolStripDropDownButtonFile.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButtonFile.Name = "toolStripDropDownButtonFile";
            toolStripDropDownButtonFile.Size = new Size(38, 20);
            toolStripDropDownButtonFile.Text = "File";
            // 
            // toolStripButton_New_file
            // 
            toolStripButton_New_file.Image = (Image)resources.GetObject("toolStripButton_New_file.Image");
            toolStripButton_New_file.ImageTransparentColor = Color.Magenta;
            toolStripButton_New_file.Name = "toolStripButton_New_file";
            toolStripButton_New_file.Size = new Size(51, 20);
            toolStripButton_New_file.Text = "New";
            toolStripButton_New_file.Click += ToolStripButton_New_file_Click;
            // 
            // BtnLoad
            // 
            BtnLoad.Image = (Image)resources.GetObject("BtnLoad.Image");
            BtnLoad.Name = "BtnLoad";
            BtnLoad.Size = new Size(56, 20);
            BtnLoad.Text = "Open";
            BtnLoad.ToolTipText = "Open SVG file (Ctrl+O)";
            BtnLoad.Click += BtnLoad_Click;
            // 
            // openToolStripButton
            // 
            openToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            openToolStripButton.ImageTransparentColor = Color.Magenta;
            openToolStripButton.Name = "openToolStripButton";
            openToolStripButton.Size = new Size(23, 4);
            openToolStripButton.Text = "&Open";
            openToolStripButton.Visible = false;
            // 
            // BtnSave
            // 
            BtnSave.Image = (Image)resources.GetObject("BtnSave.Image");
            BtnSave.Name = "BtnSave";
            BtnSave.Size = new Size(51, 20);
            BtnSave.Text = "Save";
            BtnSave.ToolTipText = "Save SVG file (Ctrl+S)";
            BtnSave.Click += BtnSave_Click;
            // 
            // BtnExport
            // 
            BtnExport.Image = (Image)resources.GetObject("BtnExport.Image");
            BtnExport.Name = "BtnExport";
            BtnExport.Size = new Size(60, 20);
            BtnExport.Text = "Export";
            BtnExport.ToolTipText = "Export as image";
            BtnExport.Click += BtnExport_Click;
            // 
            // Sep1
            // 
            Sep1.Name = "Sep1";
            Sep1.Size = new Size(6, 23);
            // 
            // BtnOptions
            // 
            BtnOptions.DropDownItems.AddRange(new ToolStripItem[] { BtnRenderMethodMenuItem });
            BtnOptions.ImageTransparentColor = Color.Magenta;
            BtnOptions.Name = "BtnOptions";
            BtnOptions.Size = new Size(62, 20);
            BtnOptions.Text = "Options";
            // 
            // BtnRenderMethodMenuItem
            // 
            BtnRenderMethodMenuItem.DropDownItems.AddRange(new ToolStripItem[] { BtnSetRenderGDI, BtnSetRenderSkia, toolStripSeparator4, BtnSetRenderSVGFile });
            BtnRenderMethodMenuItem.Name = "BtnRenderMethodMenuItem";
            BtnRenderMethodMenuItem.Size = new Size(180, 22);
            BtnRenderMethodMenuItem.Text = "Render Method";
            // 
            // BtnSetRenderGDI
            // 
            BtnSetRenderGDI.Name = "BtnSetRenderGDI";
            BtnSetRenderGDI.Size = new Size(193, 22);
            BtnSetRenderGDI.Text = "Render Using GDI+";
            BtnSetRenderGDI.Click += BtnSetRender_Click;
            // 
            // BtnSetRenderSkia
            // 
            BtnSetRenderSkia.Checked = true;
            BtnSetRenderSkia.CheckState = CheckState.Checked;
            BtnSetRenderSkia.Name = "BtnSetRenderSkia";
            BtnSetRenderSkia.Size = new Size(193, 22);
            BtnSetRenderSkia.Text = "Render Using Skia";
            BtnSetRenderSkia.Click += BtnSetRender_Click;
            // 
            // BtnSetRenderSVGFile
            // 
            BtnSetRenderSVGFile.Name = "BtnSetRenderSVGFile";
            BtnSetRenderSVGFile.Size = new Size(193, 22);
            BtnSetRenderSVGFile.Text = "Create File Render SVG";
            BtnSetRenderSVGFile.Click += BtnSetRenderSVGFile_Click;
            // 
            // toolStripDropDownButtonItem
            // 
            toolStripDropDownButtonItem.DropDownItems.AddRange(new ToolStripItem[] { BtnLineTool, BtnCircleTool, BtnRectangleTool, BtnTextTool, chooseColorToolStripMenuItem });
            toolStripDropDownButtonItem.Image = (Image)resources.GetObject("toolStripDropDownButtonItem.Image");
            toolStripDropDownButtonItem.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButtonItem.Name = "toolStripDropDownButtonItem";
            toolStripDropDownButtonItem.Size = new Size(63, 20);
            toolStripDropDownButtonItem.Text = "Draw";
            // 
            // BtnLineTool
            // 
            BtnLineTool.Name = "BtnLineTool";
            BtnLineTool.Size = new Size(33, 19);
            BtnLineTool.Text = "Line";
            BtnLineTool.ToolTipText = "Draw line";
            BtnLineTool.Click += BtnLineTool_Click;
            // 
            // BtnCircleTool
            // 
            BtnCircleTool.Name = "BtnCircleTool";
            BtnCircleTool.Size = new Size(41, 19);
            BtnCircleTool.Text = "Circle";
            BtnCircleTool.ToolTipText = "Draw circle";
            BtnCircleTool.Click += BtnCircleTool_Click;
            // 
            // BtnRectangleTool
            // 
            BtnRectangleTool.Name = "BtnRectangleTool";
            BtnRectangleTool.Size = new Size(63, 19);
            BtnRectangleTool.Text = "Rectangle";
            BtnRectangleTool.ToolTipText = "Draw rectangle";
            BtnRectangleTool.Click += BtnRectangleTool_Click;
            // 
            // BtnTextTool
            // 
            BtnTextTool.Name = "BtnTextTool";
            BtnTextTool.Size = new Size(32, 19);
            BtnTextTool.Text = "Text";
            BtnTextTool.ToolTipText = "Add text";
            BtnTextTool.Click += BtnTextTool_Click;
            // 
            // chooseColorToolStripMenuItem
            // 
            chooseColorToolStripMenuItem.Name = "chooseColorToolStripMenuItem";
            chooseColorToolStripMenuItem.Size = new Size(130, 22);
            chooseColorToolStripMenuItem.Text = "DrawColor";
            chooseColorToolStripMenuItem.Click += chooseColorToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 23);
            // 
            // BtnOrbitPanel
            // 
            BtnOrbitPanel.Image = (Image)resources.GetObject("BtnOrbitPanel.Image");
            BtnOrbitPanel.ImageTransparentColor = Color.Magenta;
            BtnOrbitPanel.Name = "BtnOrbitPanel";
            BtnOrbitPanel.Size = new Size(54, 20);
            BtnOrbitPanel.Text = "Orbit";
            BtnOrbitPanel.Click += BtnOrbitPanel_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 23);
            // 
            // BtnUndo
            // 
            BtnUndo.Image = (Image)resources.GetObject("BtnUndo.Image");
            BtnUndo.Name = "BtnUndo";
            BtnUndo.Size = new Size(56, 20);
            BtnUndo.Text = "Undo";
            BtnUndo.ToolTipText = "Undo (Ctrl+Z)";
            BtnUndo.Click += BtnUndo_Click;
            // 
            // BtnRedo
            // 
            BtnRedo.Image = (Image)resources.GetObject("BtnRedo.Image");
            BtnRedo.Name = "BtnRedo";
            BtnRedo.Size = new Size(54, 20);
            BtnRedo.Text = "Redo";
            BtnRedo.ToolTipText = "Redo (Ctrl+Y)";
            BtnRedo.Click += BtnRedo_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 23);
            // 
            // toolStripDropDownButtonZoom
            // 
            toolStripDropDownButtonZoom.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripDropDownButtonZoom.DropDownItems.AddRange(new ToolStripItem[] { BtnZoomIn, BtnZoomFit, BtnZoomOut, MenuItemZoomPrevious, BtnPan });
            toolStripDropDownButtonZoom.Image = (Image)resources.GetObject("toolStripDropDownButtonZoom.Image");
            toolStripDropDownButtonZoom.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButtonZoom.Name = "toolStripDropDownButtonZoom";
            toolStripDropDownButtonZoom.Size = new Size(29, 20);
            toolStripDropDownButtonZoom.Text = "toolStripDropDownButton1";
            // 
            // BtnZoomIn
            // 
            BtnZoomIn.Image = (Image)resources.GetObject("BtnZoomIn.Image");
            BtnZoomIn.Name = "BtnZoomIn";
            BtnZoomIn.Size = new Size(129, 22);
            BtnZoomIn.Text = "Zoom In";
            BtnZoomIn.ToolTipText = "Zoom in (Ctrl+Plus)";
            BtnZoomIn.Click += BtnZoomIn_Click;
            // 
            // BtnZoomFit
            // 
            BtnZoomFit.Image = (Image)resources.GetObject("BtnZoomFit.Image");
            BtnZoomFit.Name = "BtnZoomFit";
            BtnZoomFit.ShortcutKeys = Keys.Control | Keys.F;
            BtnZoomFit.Size = new Size(129, 22);
            BtnZoomFit.Text = "Fit";
            BtnZoomFit.ToolTipText = "Fit to window";
            BtnZoomFit.Click += BtnZoomFit_Click;
            // 
            // BtnZoomOut
            // 
            BtnZoomOut.Image = (Image)resources.GetObject("BtnZoomOut.Image");
            BtnZoomOut.Name = "BtnZoomOut";
            BtnZoomOut.Size = new Size(129, 22);
            BtnZoomOut.Text = "Zoom Out";
            BtnZoomOut.ToolTipText = "Zoom out (Ctrl+Minus)";
            BtnZoomOut.Click += BtnZoomOut_Click;
            // 
            // MenuItemZoomPrevious
            // 
            MenuItemZoomPrevious.Image = (Image)resources.GetObject("MenuItemZoomPrevious.Image");
            MenuItemZoomPrevious.ImageTransparentColor = Color.Magenta;
            MenuItemZoomPrevious.Name = "MenuItemZoomPrevious";
            MenuItemZoomPrevious.Size = new Size(129, 22);
            MenuItemZoomPrevious.Text = "Previous";
            MenuItemZoomPrevious.Click += MenuItemZoomPrevious_Click;
            // 
            // BtnPan
            // 
            BtnPan.Name = "BtnPan";
            BtnPan.Size = new Size(31, 19);
            BtnPan.Text = "Pan";
            BtnPan.ToolTipText = "Pan view";
            BtnPan.Click += BtnPan_Click;
            // 
            // toolStripDropDownButtonLayers
            // 
            toolStripDropDownButtonLayers.DropDownItems.AddRange(new ToolStripItem[] { addNewLayerToolStripMenuItem, deleteLayerToolStripMenuItem, setActiveLayerToolStripMenuItem, setLayerVisibilityToolStripMenuItem });
            toolStripDropDownButtonLayers.Image = (Image)resources.GetObject("toolStripDropDownButtonLayers.Image");
            toolStripDropDownButtonLayers.Name = "toolStripDropDownButtonLayers";
            toolStripDropDownButtonLayers.Size = new Size(69, 20);
            toolStripDropDownButtonLayers.Text = "Layers";
            // 
            // addNewLayerToolStripMenuItem
            // 
            addNewLayerToolStripMenuItem.Image = (Image)resources.GetObject("addNewLayerToolStripMenuItem.Image");
            addNewLayerToolStripMenuItem.Name = "addNewLayerToolStripMenuItem";
            addNewLayerToolStripMenuItem.Size = new Size(168, 22);
            addNewLayerToolStripMenuItem.Text = "Add Layer";
            addNewLayerToolStripMenuItem.Click += AddNewLayerToolStripMenuItem_Click;
            // 
            // deleteLayerToolStripMenuItem
            // 
            deleteLayerToolStripMenuItem.Name = "deleteLayerToolStripMenuItem";
            deleteLayerToolStripMenuItem.Size = new Size(168, 22);
            deleteLayerToolStripMenuItem.Text = "delete Layer";
            // 
            // setActiveLayerToolStripMenuItem
            // 
            setActiveLayerToolStripMenuItem.Name = "setActiveLayerToolStripMenuItem";
            setActiveLayerToolStripMenuItem.Size = new Size(168, 22);
            setActiveLayerToolStripMenuItem.Text = "Set Active Layer";
            // 
            // setLayerVisibilityToolStripMenuItem
            // 
            setLayerVisibilityToolStripMenuItem.Name = "setLayerVisibilityToolStripMenuItem";
            setLayerVisibilityToolStripMenuItem.Size = new Size(168, 22);
            setLayerVisibilityToolStripMenuItem.Text = "Set Layer Visibility";
            // 
            // Sep2
            // 
            Sep2.Name = "Sep2";
            Sep2.Size = new Size(6, 23);
            // 
            // BtnSelectTool
            // 
            BtnSelectTool.Checked = true;
            BtnSelectTool.CheckState = CheckState.Checked;
            BtnSelectTool.Image = (Image)resources.GetObject("BtnSelectTool.Image");
            BtnSelectTool.Name = "BtnSelectTool";
            BtnSelectTool.Size = new Size(58, 20);
            BtnSelectTool.Text = "Select";
            BtnSelectTool.ToolTipText = "Selection tool";
            BtnSelectTool.Click += BtnSelectTool_Click;
            // 
            // Sep3
            // 
            Sep3.Name = "Sep3";
            Sep3.Size = new Size(6, 23);
            // 
            // BtnToggleGrid
            // 
            BtnToggleGrid.Checked = true;
            BtnToggleGrid.CheckOnClick = true;
            BtnToggleGrid.CheckState = CheckState.Checked;
            BtnToggleGrid.Image = (Image)resources.GetObject("BtnToggleGrid.Image");
            BtnToggleGrid.Name = "BtnToggleGrid";
            BtnToggleGrid.Size = new Size(49, 20);
            BtnToggleGrid.Text = "Grid";
            BtnToggleGrid.ToolTipText = "Toggle grid";
            BtnToggleGrid.Click += BtnToggleGrid_Click;
            // 
            // BtnToggleAxes
            // 
            BtnToggleAxes.Checked = true;
            BtnToggleAxes.CheckOnClick = true;
            BtnToggleAxes.CheckState = CheckState.Checked;
            BtnToggleAxes.Image = (Image)resources.GetObject("BtnToggleAxes.Image");
            BtnToggleAxes.Name = "BtnToggleAxes";
            BtnToggleAxes.Size = new Size(51, 20);
            BtnToggleAxes.Text = "Axes";
            BtnToggleAxes.ToolTipText = "Toggle axes";
            BtnToggleAxes.Click += BtnToggleAxes_Click;
            // 
            // toolStripSeparator
            // 
            toolStripSeparator.Name = "toolStripSeparator";
            toolStripSeparator.Size = new Size(6, 23);
            // 
            // toolStripLabelFileName
            // 
            toolStripLabelFileName.Name = "toolStripLabelFileName";
            toolStripLabelFileName.Size = new Size(79, 20);
            toolStripLabelFileName.Text = "no file loaded";
            // 
            // Sep4
            // 
            Sep4.Name = "Sep4";
            Sep4.Size = new Size(6, 6);
            // 
            // PanelRightTools
            // 
            PanelRightTools.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PanelRightTools.BackColor = SystemColors.Control;
            PanelRightTools.Controls.Add(tableLayoutPanel1);
            PanelRightTools.Controls.Add(BtnColorPicker);
            PanelRightTools.Location = new Point(774, 28);
            PanelRightTools.Name = "PanelRightTools";
            PanelRightTools.Padding = new Padding(8);
            PanelRightTools.Size = new Size(199, 583);
            PanelRightTools.TabIndex = 1;
            PanelRightTools.Visible = false;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(lblworldShift, 0, 2);
            tableLayoutPanel1.Controls.Add(BtnResetWorldShift, 0, 5);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel4, 0, 4);
            tableLayoutPanel1.Controls.Add(label3, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 3);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Font = new Font("Tahoma", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tableLayoutPanel1.Location = new Point(8, 8);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 6;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 45.16854F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 5.617978F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 5.16854F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25.8427F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 8.314607F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 9.662921F));
            tableLayoutPanel1.Size = new Size(183, 567);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // lblworldShift
            // 
            lblworldShift.AutoSize = true;
            lblworldShift.Font = new Font("Tahoma", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblworldShift.Location = new Point(3, 287);
            lblworldShift.Name = "lblworldShift";
            lblworldShift.Size = new Size(91, 16);
            lblworldShift.TabIndex = 3;
            lblworldShift.Text = "X: 0, Y: 0,Z:0";
            // 
            // BtnResetWorldShift
            // 
            BtnResetWorldShift.Dock = DockStyle.Fill;
            BtnResetWorldShift.Location = new Point(3, 512);
            BtnResetWorldShift.Name = "BtnResetWorldShift";
            BtnResetWorldShift.Size = new Size(177, 52);
            BtnResetWorldShift.TabIndex = 3;
            BtnResetWorldShift.Text = "Reset Pan (Shift)";
            BtnResetWorldShift.UseVisualStyleBackColor = true;
            BtnResetWorldShift.Click += BtnResetWorldShift_Click;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.Controls.Add(btnWorldShiftZDec, 1, 0);
            tableLayoutPanel4.Controls.Add(btnWorldShiftZInc, 0, 0);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(3, 465);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.Size = new Size(177, 41);
            tableLayoutPanel4.TabIndex = 3;
            // 
            // btnWorldShiftZDec
            // 
            btnWorldShiftZDec.Dock = DockStyle.Fill;
            btnWorldShiftZDec.Location = new Point(91, 3);
            btnWorldShiftZDec.Name = "btnWorldShiftZDec";
            btnWorldShiftZDec.Size = new Size(83, 35);
            btnWorldShiftZDec.TabIndex = 7;
            btnWorldShiftZDec.Text = "- Z";
            btnWorldShiftZDec.UseVisualStyleBackColor = true;
            btnWorldShiftZDec.Click += btnWorldShift_Click;
            // 
            // btnWorldShiftZInc
            // 
            btnWorldShiftZInc.Dock = DockStyle.Fill;
            btnWorldShiftZInc.Location = new Point(3, 3);
            btnWorldShiftZInc.Name = "btnWorldShiftZInc";
            btnWorldShiftZInc.Size = new Size(82, 35);
            btnWorldShiftZInc.TabIndex = 6;
            btnWorldShiftZInc.Text = "+ Z";
            btnWorldShiftZInc.UseVisualStyleBackColor = true;
            btnWorldShiftZInc.Click += btnWorldShift_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Times New Roman", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(3, 256);
            label3.Name = "label3";
            label3.Size = new Size(85, 15);
            label3.TabIndex = 3;
            label3.Text = "3D Pan (Shift)";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33332F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33334F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33334F));
            tableLayoutPanel3.Controls.Add(btnWorldShiftYDec, 1, 2);
            tableLayoutPanel3.Controls.Add(btnWorldShiftXDec, 0, 1);
            tableLayoutPanel3.Controls.Add(btnWorldShiftYInc, 1, 0);
            tableLayoutPanel3.Controls.Add(btnWorldShiftXInc, 2, 1);
            tableLayoutPanel3.Controls.Add(label2, 1, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 319);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 3;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33334F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel3.Size = new Size(177, 140);
            tableLayoutPanel3.TabIndex = 3;
            // 
            // btnWorldShiftYDec
            // 
            btnWorldShiftYDec.Dock = DockStyle.Fill;
            btnWorldShiftYDec.Location = new Point(61, 95);
            btnWorldShiftYDec.Name = "btnWorldShiftYDec";
            btnWorldShiftYDec.Size = new Size(53, 42);
            btnWorldShiftYDec.TabIndex = 4;
            btnWorldShiftYDec.Text = "- Y";
            btnWorldShiftYDec.UseVisualStyleBackColor = true;
            btnWorldShiftYDec.Click += btnWorldShift_Click;
            // 
            // btnWorldShiftXDec
            // 
            btnWorldShiftXDec.Dock = DockStyle.Fill;
            btnWorldShiftXDec.Location = new Point(3, 49);
            btnWorldShiftXDec.Name = "btnWorldShiftXDec";
            btnWorldShiftXDec.Size = new Size(52, 40);
            btnWorldShiftXDec.TabIndex = 4;
            btnWorldShiftXDec.Text = "- X";
            btnWorldShiftXDec.UseVisualStyleBackColor = true;
            btnWorldShiftXDec.Click += btnWorldShift_Click;
            // 
            // btnWorldShiftYInc
            // 
            btnWorldShiftYInc.Dock = DockStyle.Fill;
            btnWorldShiftYInc.Location = new Point(61, 3);
            btnWorldShiftYInc.Name = "btnWorldShiftYInc";
            btnWorldShiftYInc.Size = new Size(53, 40);
            btnWorldShiftYInc.TabIndex = 4;
            btnWorldShiftYInc.Text = "+ Y";
            btnWorldShiftYInc.UseVisualStyleBackColor = true;
            btnWorldShiftYInc.Click += btnWorldShift_Click;
            // 
            // btnWorldShiftXInc
            // 
            btnWorldShiftXInc.Dock = DockStyle.Fill;
            btnWorldShiftXInc.Location = new Point(120, 49);
            btnWorldShiftXInc.Name = "btnWorldShiftXInc";
            btnWorldShiftXInc.Size = new Size(54, 40);
            btnWorldShiftXInc.TabIndex = 3;
            btnWorldShiftXInc.Text = "+ X";
            btnWorldShiftXInc.UseVisualStyleBackColor = true;
            btnWorldShiftXInc.Click += btnWorldShift_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(61, 46);
            label2.Name = "label2";
            label2.Size = new Size(53, 46);
            label2.TabIndex = 5;
            label2.Text = "Pan";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(label1, 0, 0);
            tableLayoutPanel2.Controls.Add(TrackBarZRotation, 0, 6);
            tableLayoutPanel2.Controls.Add(LblZRotation, 0, 5);
            tableLayoutPanel2.Controls.Add(TrackBarYRotation, 0, 4);
            tableLayoutPanel2.Controls.Add(LblYRotation, 0, 3);
            tableLayoutPanel2.Controls.Add(TrackBarXRotation, 0, 2);
            tableLayoutPanel2.Controls.Add(LblXRotation, 0, 1);
            tableLayoutPanel2.Controls.Add(BtnResetRotation, 0, 7);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 8;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 7.731959F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.30769F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.30769F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.30769F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.30769F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 10.76923F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 15.89744F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 15.89744F));
            tableLayoutPanel2.Size = new Size(177, 250);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Times New Roman", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(72, 15);
            label1.TabIndex = 3;
            label1.Text = "3D Rotation";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TrackBarZRotation
            // 
            TrackBarZRotation.Dock = DockStyle.Fill;
            TrackBarZRotation.Location = new Point(3, 169);
            TrackBarZRotation.Maximum = 314;
            TrackBarZRotation.Minimum = -314;
            TrackBarZRotation.Name = "TrackBarZRotation";
            TrackBarZRotation.Size = new Size(171, 33);
            TrackBarZRotation.TabIndex = 5;
            TrackBarZRotation.TickFrequency = 30;
            TrackBarZRotation.Scroll += TrackBarRotation_Scroll;
            // 
            // LblZRotation
            // 
            LblZRotation.Dock = DockStyle.Fill;
            LblZRotation.Location = new Point(3, 139);
            LblZRotation.Name = "LblZRotation";
            LblZRotation.Size = new Size(171, 27);
            LblZRotation.TabIndex = 4;
            LblZRotation.Text = "Z: 0°";
            LblZRotation.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TrackBarYRotation
            // 
            TrackBarYRotation.Dock = DockStyle.Fill;
            TrackBarYRotation.Location = new Point(3, 112);
            TrackBarYRotation.Maximum = 314;
            TrackBarYRotation.Minimum = -314;
            TrackBarYRotation.Name = "TrackBarYRotation";
            TrackBarYRotation.Size = new Size(171, 24);
            TrackBarYRotation.TabIndex = 3;
            TrackBarYRotation.TickFrequency = 30;
            TrackBarYRotation.Scroll += TrackBarRotation_Scroll;
            // 
            // LblYRotation
            // 
            LblYRotation.Dock = DockStyle.Fill;
            LblYRotation.Location = new Point(3, 79);
            LblYRotation.Name = "LblYRotation";
            LblYRotation.Size = new Size(171, 30);
            LblYRotation.TabIndex = 2;
            LblYRotation.Text = "Y: 0°";
            LblYRotation.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TrackBarXRotation
            // 
            TrackBarXRotation.Dock = DockStyle.Fill;
            TrackBarXRotation.Location = new Point(3, 52);
            TrackBarXRotation.Maximum = 314;
            TrackBarXRotation.Minimum = -314;
            TrackBarXRotation.Name = "TrackBarXRotation";
            TrackBarXRotation.Size = new Size(171, 24);
            TrackBarXRotation.TabIndex = 1;
            TrackBarXRotation.TickFrequency = 30;
            TrackBarXRotation.Scroll += TrackBarRotation_Scroll;
            // 
            // LblXRotation
            // 
            LblXRotation.Dock = DockStyle.Fill;
            LblXRotation.Location = new Point(3, 19);
            LblXRotation.Name = "LblXRotation";
            LblXRotation.Size = new Size(171, 30);
            LblXRotation.TabIndex = 0;
            LblXRotation.Text = "X: 0°";
            LblXRotation.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // BtnResetRotation
            // 
            BtnResetRotation.Dock = DockStyle.Fill;
            BtnResetRotation.Location = new Point(3, 208);
            BtnResetRotation.Name = "BtnResetRotation";
            BtnResetRotation.Size = new Size(171, 39);
            BtnResetRotation.TabIndex = 6;
            BtnResetRotation.Text = "Reset Rotation";
            BtnResetRotation.UseVisualStyleBackColor = true;
            BtnResetRotation.Click += BtnResetRotation_Click;
            // 
            // BtnColorPicker
            // 
            BtnColorPicker.Location = new Point(12, 12);
            BtnColorPicker.Name = "BtnColorPicker";
            BtnColorPicker.Size = new Size(71, 32);
            BtnColorPicker.TabIndex = 0;
            BtnColorPicker.Text = "Color";
            BtnColorPicker.UseVisualStyleBackColor = true;
            // 
            // StatusStripMain
            // 
            StatusStripMain.BackColor = SystemColors.Control;
            StatusStripMain.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, StatusLabelTotalMemory, StatusMemoryUsed, StatusLabelRenderTime, StatusTool, StatusZoom, StatusCoordinates });
            StatusStripMain.Location = new Point(0, 659);
            StatusStripMain.Name = "StatusStripMain";
            StatusStripMain.Size = new Size(973, 24);
            StatusStripMain.TabIndex = 1;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(100, 19);
            toolStripStatusLabel1.Text = "zero items loaded";
            // 
            // StatusLabelTotalMemory
            // 
            StatusLabelTotalMemory.Name = "StatusLabelTotalMemory";
            StatusLabelTotalMemory.Size = new Size(78, 19);
            StatusLabelTotalMemory.Text = "TotalMemory";
            // 
            // StatusMemoryUsed
            // 
            StatusMemoryUsed.Name = "StatusMemoryUsed";
            StatusMemoryUsed.Size = new Size(79, 19);
            StatusMemoryUsed.Text = "BaseMemory ";
            // 
            // StatusLabelRenderTime
            // 
            StatusLabelRenderTime.Name = "StatusLabelRenderTime";
            StatusLabelRenderTime.Size = new Size(71, 19);
            StatusLabelRenderTime.Text = "RenderTime";
            // 
            // StatusTool
            // 
            StatusTool.Name = "StatusTool";
            StatusTool.Size = new Size(39, 19);
            StatusTool.Text = "Ready";
            // 
            // StatusZoom
            // 
            StatusZoom.BorderSides = ToolStripStatusLabelBorderSides.Right;
            StatusZoom.Name = "StatusZoom";
            StatusZoom.Size = new Size(39, 19);
            StatusZoom.Text = "100%";
            // 
            // StatusCoordinates
            // 
            StatusCoordinates.BorderSides = ToolStripStatusLabelBorderSides.Right;
            StatusCoordinates.Name = "StatusCoordinates";
            StatusCoordinates.Size = new Size(74, 19);
            StatusCoordinates.Text = "X: 0, Y: 0,Z:0";
            // 
            // TxtLabelInput
            // 
            TxtLabelInput.Location = new Point(391, 181);
            TxtLabelInput.Name = "TxtLabelInput";
            TxtLabelInput.Size = new Size(168, 23);
            TxtLabelInput.TabIndex = 2;
            TxtLabelInput.Visible = false;
            TxtLabelInput.KeyDown += TxtLabelInput_KeyDown;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(190, 6);
            // 
            // EngineControl
            // 
            Controls.Add(TxtLabelInput);
            Controls.Add(PanelRightTools);
            Controls.Add(ToolStripMain);
            Controls.Add(StatusStripMain);
            Name = "EngineControl";
            Size = new Size(973, 683);
            ToolStripMain.ResumeLayout(false);
            ToolStripMain.PerformLayout();
            PanelRightTools.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)TrackBarZRotation).EndInit();
            ((System.ComponentModel.ISupportInitialize)TrackBarYRotation).EndInit();
            ((System.ComponentModel.ISupportInitialize)TrackBarXRotation).EndInit();
            StatusStripMain.ResumeLayout(false);
            StatusStripMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }
        private ToolStripStatusLabel StatusMemoryUsed;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel StatusLabelRenderTime;
        private ToolStripStatusLabel StatusLabelTotalMemory;
        private ToolStripDropDownButton BtnOptions;
        private ToolStripMenuItem BtnRenderMethodMenuItem;
        private ToolStripMenuItem BtnSetRenderGDI;
        private ToolStripMenuItem BtnSetRenderSkia;
        private ToolStripLabel toolStripLabelFileName;
        internal ToolStripMenuItem BtnZoomIn;
        internal ToolStripMenuItem BtnZoomFit;
        internal ToolStripMenuItem BtnZoomOut;
        private ToolStripMenuItem MenuItemZoomPrevious;
        internal ToolStripButton BtnPan;
        private ToolStripMenuItem BtnSetRenderSVGFile;
        private ToolStripSeparator toolStripSeparator4;

        #endregion
        // VectorDraw
        // 
    }
    }
        #endregion

 

