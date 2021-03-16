using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShapeToXodr
{
    public partial class MainViewer : Form
    {
        List<SHPReader.Single_Layer> allLayers;
        RefLineCreator refLineCreator;
        MovPoint movPoint_R;
        LaneCounter laneCounter_R;

        public MainViewer()
        {
            InitializeComponent();
        }

        private void sel_path_Click(object sender, EventArgs e) //경로선택 버튼
        {
            FolderBrowserDialog path_sel_dlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };
            DialogResult res_path_sel = path_sel_dlg.ShowDialog();
            if (res_path_sel == DialogResult.OK)
            {
                string sel_Path = path_sel_dlg.SelectedPath;
                string[] split_path = sel_Path.Split('\\');
                Sel_path_lab.Text = split_path.Last();
                ReadLayers(sel_Path);
            }
            else
            {
                Sel_path_lab.Text = "Not Selected";
                Out_ref_shape.Enabled = Out_xodr.Enabled = Xodr_name_text.Enabled = Shp_name_text.Enabled = false;
            }
        }

        private void ReadLayers(string sel_Path)
        {
            allLayers = SHPReader.readAllNGIIfiles(sel_Path);

            if(allLayers == null)
            {
                Sel_path_lab.Text = "Not in the path correct Shape Files"; // 해당 경로에 shp파일이 없는경우
                Out_ref_shape.Enabled = Out_xodr.Enabled = Xodr_name_text.Enabled = Shp_name_text.Enabled = false;
            }
            else // 있는경우 데이터 생성 및 연산
            {
                Layer_Conv.Enabled = true;
            }
        }

        private void Out_ref_shape_Click(object sender, EventArgs e)
        {
            ToSHP shp;
            if (Shp_name_text.Text == "")
            {
                shp = new ToSHP(movPoint_R.movPoints, allLayers[1].BoundBox, movPoint_R.lnkID, refLineCreator.linkPtCounter_R, SHPReader.SHPT_ARC);
            }
            else
            {
                shp = new ToSHP(movPoint_R.movPoints, allLayers[1].BoundBox, movPoint_R.lnkID, refLineCreator.linkPtCounter_R, SHPReader.SHPT_ARC, Shp_name_text.Text+"_R");
            }
            System.Diagnostics.Process.Start(@".\");
        }

        private void Layer_Conv_Tick(object sender, EventArgs e)
        {
            refLineCreator = new RefLineCreator(allLayers);
            movPoint_R = new MovPoint(ref refLineCreator.RefLinks_R);
            laneCounter_R = new LaneCounter(refLineCreator.A2_LINK, movPoint_R.lnkID);
            laneCounter_R.FixExtraLane(refLineCreator.rootToEnd_R);
            //movPoint_R.SubsGeo(allLayers[2].BoundBox);

            Layer_Conv.Enabled = false;
            Out_ref_shape.Enabled = Out_xodr.Enabled = Xodr_name_text.Enabled = Shp_name_text.Enabled = true;

			//add test or anything
        }

        private void Out_xodr_Click(object sender, EventArgs e)
        {
            toXODR OpenDRIVE = new toXODR();

			movPoint_R.SubsGeo(allLayers[2].BoundBox);

            for (int i = 0; i < movPoint_R.lnkID.Length; i++)//오른쪽 ReferenceLine 생성
            {
                OpenDRIVE.RefData(movPoint_R.hdgList2D[i], movPoint_R.movPoint2D[i], movPoint_R.Length2D[i], movPoint_R.lnkID[i], movPoint_R.ptData[i, 1]);
            }
            OpenDRIVE.AddLane(laneCounter_R.lanesCounter, refLineCreator.rootToEnd_R);
            OpenDRIVE.Make_Road_LinkData(refLineCreator.rootToEnd_R, movPoint_R.lnkID);

            if (Xodr_name_text.Text != "")//출력시 지정된 텍스트 여부 확인
            {
                OpenDRIVE.XMLSave(Xodr_name_text.Text);
            }
            else
            {
                OpenDRIVE.XMLSave();
            }
            System.Diagnostics.Process.Start(@".\");
        }
    }
}
