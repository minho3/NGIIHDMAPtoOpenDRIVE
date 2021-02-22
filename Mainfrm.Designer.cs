namespace ShapeToXodr
{
    partial class MainViewer
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Sel_path = new System.Windows.Forms.Button();
            this.Sel_path_lab = new System.Windows.Forms.Label();
            this.Out_ref_shape = new System.Windows.Forms.Button();
            this.Shp_name_text = new System.Windows.Forms.TextBox();
            this.Shp_name_lab = new System.Windows.Forms.Label();
            this.Xodr_name_lab = new System.Windows.Forms.Label();
            this.Xodr_name_text = new System.Windows.Forms.TextBox();
            this.Out_xodr = new System.Windows.Forms.Button();
            this.Layer_Conv = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // Sel_path
            // 
            this.Sel_path.Location = new System.Drawing.Point(12, 12);
            this.Sel_path.Name = "Sel_path";
            this.Sel_path.Size = new System.Drawing.Size(90, 23);
            this.Sel_path.TabIndex = 0;
            this.Sel_path.Text = "Select Path";
            this.Sel_path.UseVisualStyleBackColor = true;
            this.Sel_path.Click += new System.EventHandler(this.sel_path_Click);
            // 
            // Sel_path_lab
            // 
            this.Sel_path_lab.AutoSize = true;
            this.Sel_path_lab.Location = new System.Drawing.Point(108, 17);
            this.Sel_path_lab.Name = "Sel_path_lab";
            this.Sel_path_lab.Size = new System.Drawing.Size(77, 12);
            this.Sel_path_lab.TabIndex = 1;
            this.Sel_path_lab.Text = "Not Selected";
            // 
            // Out_ref_shape
            // 
            this.Out_ref_shape.Enabled = false;
            this.Out_ref_shape.Location = new System.Drawing.Point(362, 46);
            this.Out_ref_shape.Name = "Out_ref_shape";
            this.Out_ref_shape.Size = new System.Drawing.Size(75, 23);
            this.Out_ref_shape.TabIndex = 2;
            this.Out_ref_shape.Text = "Out Shape";
            this.Out_ref_shape.UseVisualStyleBackColor = true;
            this.Out_ref_shape.Click += new System.EventHandler(this.Out_ref_shape_Click);
            // 
            // Shp_name_text
            // 
            this.Shp_name_text.Enabled = false;
            this.Shp_name_text.Location = new System.Drawing.Point(192, 48);
            this.Shp_name_text.MaxLength = 100;
            this.Shp_name_text.Name = "Shp_name_text";
            this.Shp_name_text.Size = new System.Drawing.Size(164, 21);
            this.Shp_name_text.TabIndex = 3;
            // 
            // Shp_name_lab
            // 
            this.Shp_name_lab.AutoSize = true;
            this.Shp_name_lab.Location = new System.Drawing.Point(13, 51);
            this.Shp_name_lab.Name = "Shp_name_lab";
            this.Shp_name_lab.Size = new System.Drawing.Size(173, 12);
            this.Shp_name_lab.TabIndex = 4;
            this.Shp_name_lab.Text = "refLine Shape Output Name : ";
            // 
            // Xodr_name_lab
            // 
            this.Xodr_name_lab.AutoSize = true;
            this.Xodr_name_lab.Location = new System.Drawing.Point(44, 89);
            this.Xodr_name_lab.Name = "Xodr_name_lab";
            this.Xodr_name_lab.Size = new System.Drawing.Size(142, 12);
            this.Xodr_name_lab.TabIndex = 5;
            this.Xodr_name_lab.Text = "OpenDRIVE FIle name : ";
            // 
            // Xodr_name_text
            // 
            this.Xodr_name_text.Enabled = false;
            this.Xodr_name_text.Location = new System.Drawing.Point(192, 86);
            this.Xodr_name_text.MaxLength = 100;
            this.Xodr_name_text.Name = "Xodr_name_text";
            this.Xodr_name_text.Size = new System.Drawing.Size(164, 21);
            this.Xodr_name_text.TabIndex = 6;
            // 
            // Out_xodr
            // 
            this.Out_xodr.Enabled = false;
            this.Out_xodr.Location = new System.Drawing.Point(362, 84);
            this.Out_xodr.Name = "Out_xodr";
            this.Out_xodr.Size = new System.Drawing.Size(75, 23);
            this.Out_xodr.TabIndex = 7;
            this.Out_xodr.Text = "Out xodr";
            this.Out_xodr.UseVisualStyleBackColor = true;
            this.Out_xodr.Click += new System.EventHandler(this.Out_xodr_Click);
            // 
            // Layer_Conv
            // 
            this.Layer_Conv.Tick += new System.EventHandler(this.Layer_Conv_Tick);
            // 
            // MainViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 119);
            this.Controls.Add(this.Out_xodr);
            this.Controls.Add(this.Xodr_name_text);
            this.Controls.Add(this.Xodr_name_lab);
            this.Controls.Add(this.Shp_name_lab);
            this.Controls.Add(this.Shp_name_text);
            this.Controls.Add(this.Out_ref_shape);
            this.Controls.Add(this.Sel_path_lab);
            this.Controls.Add(this.Sel_path);
            this.Name = "MainViewer";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Shape to OpenDRIVE Convetor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Sel_path;
        private System.Windows.Forms.Label Sel_path_lab;
        private System.Windows.Forms.Button Out_ref_shape;
        private System.Windows.Forms.TextBox Shp_name_text;
        private System.Windows.Forms.Label Shp_name_lab;
        private System.Windows.Forms.Label Xodr_name_lab;
        private System.Windows.Forms.TextBox Xodr_name_text;
        private System.Windows.Forms.Button Out_xodr;
        private System.Windows.Forms.Timer Layer_Conv;
    }
}

