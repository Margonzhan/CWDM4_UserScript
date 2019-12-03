namespace Calibrate
{
    partial class FormCalibrate
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dGV_machinePoint = new System.Windows.Forms.DataGridView();
            this.C_index = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.C_X = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.C_y = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dGV_pixelPoint = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btn_getPixelPoint = new System.Windows.Forms.Button();
            this.btn_calibrate = new System.Windows.Forms.Button();
            this.btn_saveCalibrateData = new System.Windows.Forms.Button();
            this.hDisplay1 = new HalWindow.HDisplay();
            this.btn_continuePlay = new System.Windows.Forms.Button();
            this.btn_stopPlay = new System.Windows.Forms.Button();
            this.btn_savePointData = new System.Windows.Forms.Button();
            this.btn_AutoCalibrate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.nmUD_XOffset = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nmUD_YOffset = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGV_machinePoint)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGV_pixelPoint)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmUD_XOffset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmUD_YOffset)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dGV_machinePoint);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(295, 305);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "机械坐标：";
            // 
            // dGV_machinePoint
            // 
            this.dGV_machinePoint.AllowUserToAddRows = false;
            this.dGV_machinePoint.AllowUserToResizeColumns = false;
            this.dGV_machinePoint.AllowUserToResizeRows = false;
            this.dGV_machinePoint.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGV_machinePoint.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.C_index,
            this.C_X,
            this.C_y});
            this.dGV_machinePoint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dGV_machinePoint.Location = new System.Drawing.Point(3, 17);
            this.dGV_machinePoint.Name = "dGV_machinePoint";
            this.dGV_machinePoint.RowHeadersVisible = false;
            this.dGV_machinePoint.RowTemplate.Height = 23;
            this.dGV_machinePoint.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dGV_machinePoint.Size = new System.Drawing.Size(289, 285);
            this.dGV_machinePoint.TabIndex = 0;
            // 
            // C_index
            // 
            this.C_index.HeaderText = "序号";
            this.C_index.Name = "C_index";
            this.C_index.ReadOnly = true;
            this.C_index.Width = 80;
            // 
            // C_X
            // 
            this.C_X.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.C_X.HeaderText = "X";
            this.C_X.Name = "C_X";
            // 
            // C_y
            // 
            this.C_y.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.C_y.HeaderText = "Y";
            this.C_y.Name = "C_y";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dGV_pixelPoint);
            this.groupBox2.Location = new System.Drawing.Point(15, 320);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(295, 301);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "像素坐标：";
            // 
            // dGV_pixelPoint
            // 
            this.dGV_pixelPoint.AllowUserToAddRows = false;
            this.dGV_pixelPoint.AllowUserToResizeColumns = false;
            this.dGV_pixelPoint.AllowUserToResizeRows = false;
            this.dGV_pixelPoint.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGV_pixelPoint.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3});
            this.dGV_pixelPoint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dGV_pixelPoint.Location = new System.Drawing.Point(3, 17);
            this.dGV_pixelPoint.Name = "dGV_pixelPoint";
            this.dGV_pixelPoint.RowHeadersVisible = false;
            this.dGV_pixelPoint.RowTemplate.Height = 23;
            this.dGV_pixelPoint.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dGV_pixelPoint.Size = new System.Drawing.Size(289, 281);
            this.dGV_pixelPoint.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "序号";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 80;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.HeaderText = "X";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn3.HeaderText = "Y";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // btn_getPixelPoint
            // 
            this.btn_getPixelPoint.Location = new System.Drawing.Point(316, 511);
            this.btn_getPixelPoint.Name = "btn_getPixelPoint";
            this.btn_getPixelPoint.Size = new System.Drawing.Size(102, 44);
            this.btn_getPixelPoint.TabIndex = 1;
            this.btn_getPixelPoint.Text = "获取像素坐标";
            this.btn_getPixelPoint.UseVisualStyleBackColor = true;
            this.btn_getPixelPoint.Click += new System.EventHandler(this.btn_getPixelPoint_Click);
            // 
            // btn_calibrate
            // 
            this.btn_calibrate.Location = new System.Drawing.Point(435, 511);
            this.btn_calibrate.Name = "btn_calibrate";
            this.btn_calibrate.Size = new System.Drawing.Size(102, 44);
            this.btn_calibrate.TabIndex = 1;
            this.btn_calibrate.Text = "标定";
            this.btn_calibrate.UseVisualStyleBackColor = true;
            this.btn_calibrate.Click += new System.EventHandler(this.btn_calibrate_Click);
            // 
            // btn_saveCalibrateData
            // 
            this.btn_saveCalibrateData.Location = new System.Drawing.Point(435, 563);
            this.btn_saveCalibrateData.Name = "btn_saveCalibrateData";
            this.btn_saveCalibrateData.Size = new System.Drawing.Size(102, 44);
            this.btn_saveCalibrateData.TabIndex = 1;
            this.btn_saveCalibrateData.Text = "保存标定数据";
            this.btn_saveCalibrateData.UseVisualStyleBackColor = true;
            this.btn_saveCalibrateData.Click += new System.EventHandler(this.btn_saveCalibrateData_Click);
            // 
            // hDisplay1
            // 
            this.hDisplay1.HImageX = null;
            this.hDisplay1.HRegionXList = null;
            this.hDisplay1.HStringXList = null;
            this.hDisplay1.IsCancelImageMove = false;
            this.hDisplay1.Location = new System.Drawing.Point(316, 12);
            this.hDisplay1.Name = "hDisplay1";
            this.hDisplay1.Size = new System.Drawing.Size(625, 493);
            this.hDisplay1.TabIndex = 2;
            // 
            // btn_continuePlay
            // 
            this.btn_continuePlay.Location = new System.Drawing.Point(839, 511);
            this.btn_continuePlay.Name = "btn_continuePlay";
            this.btn_continuePlay.Size = new System.Drawing.Size(102, 44);
            this.btn_continuePlay.TabIndex = 1;
            this.btn_continuePlay.Text = "视频开始";
            this.btn_continuePlay.UseVisualStyleBackColor = true;
            this.btn_continuePlay.Click += new System.EventHandler(this.btn_continuePlay_Click);
            // 
            // btn_stopPlay
            // 
            this.btn_stopPlay.Enabled = false;
            this.btn_stopPlay.Location = new System.Drawing.Point(839, 561);
            this.btn_stopPlay.Name = "btn_stopPlay";
            this.btn_stopPlay.Size = new System.Drawing.Size(102, 44);
            this.btn_stopPlay.TabIndex = 1;
            this.btn_stopPlay.Text = "视频停止";
            this.btn_stopPlay.UseVisualStyleBackColor = true;
            this.btn_stopPlay.Click += new System.EventHandler(this.btn_stopPlay_Click);
            // 
            // btn_savePointData
            // 
            this.btn_savePointData.Location = new System.Drawing.Point(316, 563);
            this.btn_savePointData.Name = "btn_savePointData";
            this.btn_savePointData.Size = new System.Drawing.Size(102, 44);
            this.btn_savePointData.TabIndex = 3;
            this.btn_savePointData.Text = "保存点位数据";
            this.btn_savePointData.UseVisualStyleBackColor = true;
            this.btn_savePointData.Click += new System.EventHandler(this.btn_savePointData_Click);
            // 
            // btn_AutoCalibrate
            // 
            this.btn_AutoCalibrate.Location = new System.Drawing.Point(554, 511);
            this.btn_AutoCalibrate.Name = "btn_AutoCalibrate";
            this.btn_AutoCalibrate.Size = new System.Drawing.Size(99, 94);
            this.btn_AutoCalibrate.TabIndex = 4;
            this.btn_AutoCalibrate.Text = "自动标定";
            this.btn_AutoCalibrate.UseVisualStyleBackColor = true;
            this.btn_AutoCalibrate.Click += new System.EventHandler(this.btn_AutoCalibrate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(661, 527);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "XOffset:";
            // 
            // nmUD_XOffset
            // 
            this.nmUD_XOffset.Location = new System.Drawing.Point(720, 525);
            this.nmUD_XOffset.Maximum = new decimal(new int[] {
            500000,
            0,
            0,
            0});
            this.nmUD_XOffset.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nmUD_XOffset.Name = "nmUD_XOffset";
            this.nmUD_XOffset.Size = new System.Drawing.Size(96, 21);
            this.nmUD_XOffset.TabIndex = 6;
            this.nmUD_XOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nmUD_XOffset.Value = new decimal(new int[] {
            800,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(661, 579);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "YOffset:";
            // 
            // nmUD_YOffset
            // 
            this.nmUD_YOffset.Location = new System.Drawing.Point(720, 575);
            this.nmUD_YOffset.Maximum = new decimal(new int[] {
            500000,
            0,
            0,
            0});
            this.nmUD_YOffset.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nmUD_YOffset.Name = "nmUD_YOffset";
            this.nmUD_YOffset.Size = new System.Drawing.Size(96, 21);
            this.nmUD_YOffset.TabIndex = 6;
            this.nmUD_YOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nmUD_YOffset.Value = new decimal(new int[] {
            700,
            0,
            0,
            0});
            // 
            // FormCalibrate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(953, 619);
            this.Controls.Add(this.nmUD_YOffset);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.nmUD_XOffset);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_AutoCalibrate);
            this.Controls.Add(this.btn_savePointData);
            this.Controls.Add(this.hDisplay1);
            this.Controls.Add(this.btn_stopPlay);
            this.Controls.Add(this.btn_continuePlay);
            this.Controls.Add(this.btn_saveCalibrateData);
            this.Controls.Add(this.btn_calibrate);
            this.Controls.Add(this.btn_getPixelPoint);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormCalibrate";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dGV_machinePoint)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dGV_pixelPoint)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmUD_XOffset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmUD_YOffset)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dGV_machinePoint;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btn_getPixelPoint;
        private System.Windows.Forms.Button btn_calibrate;
        private System.Windows.Forms.Button btn_saveCalibrateData;
        private System.Windows.Forms.DataGridView dGV_pixelPoint;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private HalWindow.HDisplay hDisplay1;
        private System.Windows.Forms.Button btn_continuePlay;
        private System.Windows.Forms.Button btn_stopPlay;
        private System.Windows.Forms.Button btn_savePointData;
        private System.Windows.Forms.DataGridViewTextBoxColumn C_index;
        private System.Windows.Forms.DataGridViewTextBoxColumn C_X;
        private System.Windows.Forms.DataGridViewTextBoxColumn C_y;
        private System.Windows.Forms.Button btn_AutoCalibrate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nmUD_XOffset;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nmUD_YOffset;
    }
}

