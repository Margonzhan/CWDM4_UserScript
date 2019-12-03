using Calibrate.Service;
using CLCamera;
using HalconDotNet;
using HalWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Calibrate.CamRAC;
using Vision;
using System.Threading;

namespace Calibrate
{
    public partial class FormCalibrate : Form
    {      
        
        Task m_taskCameraPaly;
        System.Threading.CancellationTokenSource cancellationTokenSource;
        HTuple calibratedata;

        CamRemoteAccessContractClient camClient ;

        // APAS Remote Access Service.
        SystemServiceClient service ;

        public FormCalibrate()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            try
            {
                camClient = new CamRAC.CamRemoteAccessContractClient();
                service = new SystemServiceClient();
                camClient.SetExposure("AWG", 30000);            
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if(Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "CalibrateData"))
            {
                HTuple cameraX = new HTuple();
                HTuple cameraY = new HTuple();
                HTuple robotX = new HTuple();
                HTuple robotY = new HTuple();
                string _path = AppDomain.CurrentDomain.BaseDirectory + "CalibrateData";

                HOperatorSet.ReadTuple(_path + "\\cameraX.tup", out cameraX);
                HOperatorSet.ReadTuple(_path + "\\cameraY.tup", out cameraY);
                HOperatorSet.ReadTuple(_path + "\\robotX.tup", out robotX);
                HOperatorSet.ReadTuple(_path + "\\robotY.tup", out robotY);

                for (int i = 0; i < 9; i++)
                {
                    DataGridViewRow dataGridViewRow = new DataGridViewRow();
                    dataGridViewRow.CreateCells(dGV_machinePoint);
                    dataGridViewRow.Cells[0].Value = (i + 1).ToString();
                    dataGridViewRow.Cells[1].Value = robotX[i].S;
                    dataGridViewRow.Cells[2].Value = robotY[i].S;
                    dGV_machinePoint.Rows.Add(dataGridViewRow);
                }
                for (int i = 0; i < 9; i++)
                {
                    DataGridViewRow dataGridViewRow = new DataGridViewRow();
                    dataGridViewRow.CreateCells(dGV_pixelPoint);
                    dataGridViewRow.Cells[0].Value = (i + 1).ToString();
                    dataGridViewRow.Cells[1].Value = cameraX[i].S;
                    dataGridViewRow.Cells[2].Value = cameraY[i].S;
                    dGV_pixelPoint.Rows.Add(dataGridViewRow);
                }
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    DataGridViewRow dataGridViewRow = new DataGridViewRow();
                    dataGridViewRow.CreateCells(dGV_machinePoint);
                    dataGridViewRow.Cells[0].Value = (i + 1).ToString();
                    
                    dGV_machinePoint.Rows.Add(dataGridViewRow);
                }
                for (int i = 0; i < 9; i++)
                {
                    DataGridViewRow dataGridViewRow = new DataGridViewRow();
                    dataGridViewRow.CreateCells(dGV_pixelPoint);
                    dataGridViewRow.Cells[0].Value = (i + 1).ToString();
                    
                    dGV_pixelPoint.Rows.Add(dataGridViewRow);
                }
            }
           
        }

        private void btn_continuePlay_Click(object sender, EventArgs e)
        {
            cancellationTokenSource = new System.Threading.CancellationTokenSource();
            m_taskCameraPaly = new Task(() =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    Bitmap _image = camClient.GrabOneFrame("AWG");
                    visionFun.Bitmap2HObjectBpp32(_image, out HObject _himage);
                    
                    hDisplay1.BeginInvoke(new Action(() => { hDisplay1.HImageX = _himage; }));
                    GC.Collect(2);
                    System.Threading.Thread.Sleep(20);
                }
            }, cancellationTokenSource.Token);
            m_taskCameraPaly.Start();
            btn_continuePlay.Enabled = false;
            btn_stopPlay.Enabled = true;
        }

        private void btn_stopPlay_Click(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
            btn_continuePlay.Enabled = true;
            btn_stopPlay.Enabled = false;
        }

        private void btn_getPixelPoint_Click(object sender, EventArgs e)
        {
            DataGridViewRow dataGridViewRow = null;
            if (dGV_pixelPoint.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择一行来更新点位数据");
                return;
            }
            dataGridViewRow = dGV_pixelPoint.SelectedRows[0];


            Bitmap _image = camClient.GrabOneFrame("AWG");
            visionFun.Bitmap2HObjectBpp32(_image, out HObject _himage);
           
         
            GetAwgPosition(_himage, out HTuple  x, out HTuple  y, out HObject  corss);
            dataGridViewRow.Cells[1].Value = x.D;
            dataGridViewRow.Cells[2].Value = y.D;
            hDisplay1.HImageX = _himage;
            RegionX crossr = new RegionX(corss, "green");
            hDisplay1.HRegionXList = new List<RegionX>() { crossr };

        }

        void GetAwgPosition(HObject _image, out HTuple   awgX, out HTuple  awgY,out HObject cross)
        {
            try
            {
                HOperatorSet.Threshold(_image, out HObject region1, 120, 255);
                HOperatorSet.ErosionCircle(region1, out HObject region2, 5.5);
                region1.Dispose();
                HOperatorSet.DilationCircle(region2, out HObject region3, 5.5);
                region2.Dispose();
                HOperatorSet.Connection(region3, out HObject connectedregins1);
                region3.Dispose();
                HOperatorSet.SelectShape(connectedregins1, out HObject selectedregions, "area", "and", 100000, 10000000);
                connectedregins1.Dispose();
                HOperatorSet.Union1(selectedregions, out HObject regionunion);
                selectedregions.Dispose();
                HOperatorSet.ShapeTrans(regionunion, out HObject regiontrans, "rectangle2");
                regionunion.Dispose();

                HOperatorSet.RegionToBin(regiontrans, out HObject _transimage, 255, 0, 3840, 2748);

                regiontrans.Dispose();
                HOperatorSet.PointsFoerstner(_transimage, 1, 2, 3, 200, 0.3, "gauss", "false", out HTuple rowJunctions, out HTuple columnJunctions, out HTuple coRRJunctions,
                    out HTuple coRCJunctions, out HTuple coCCJunctions, out HTuple rowArea, out HTuple columnArea, out HTuple coRRArea, out HTuple coRCArea, out HTuple coCCArea);

                if (rowJunctions.Length < 2)
                {
                    throw new Exception("awgCheck error in PointsFoerstner");
                }
                HTuple phi;
                HTuple _angle;
                HTuple _row, _column;
                HOperatorSet.TupleRad(-75, out _angle);
                int hypotenuselength = 730;


                if (columnJunctions[0] < columnJunctions[1])
                {
                    HOperatorSet.AngleLx(rowJunctions[0], columnJunctions[0], rowJunctions[1], columnJunctions[1], out phi);
                    _angle += phi;
                    _row = rowJunctions[1] + Math.Cos(_angle) * hypotenuselength;
                    _column = columnJunctions[1] + Math.Sin(_angle) * hypotenuselength;

                }
                else
                {
                    HOperatorSet.AngleLx(rowJunctions[1], columnJunctions[1], rowJunctions[0], columnJunctions[0], out phi);
                    _angle += phi;
                    _row = rowJunctions[0] + Math.Cos(_angle) * hypotenuselength;
                    _column = columnJunctions[0] + Math.Sin(_angle) * hypotenuselength;

                }

                visionFun.scale_image_range(_image, out HObject imageScaled, 180, 230);
                HOperatorSet.MeanImage(imageScaled, out HObject imageMean, 5, 5);
                visionFun.findline(imageMean, out HObject line, _row, _column, phi, 150, 150, "nearest_neighbor", 2.5, 25, "positive", "last");
                HOperatorSet.WriteImage(_image, "bmp", 0, AppDomain.CurrentDomain.BaseDirectory + "awg.bmp");


                double rx = (rowJunctions[0] + rowJunctions[1]) / 2;
                double ry = (columnJunctions[0] + columnJunctions[1]) / 2;

                HOperatorSet.TupleRad(-90, out HTuple _angle90);
                visionFun.findline(_image, out HObject line1, rx, ry, phi + _angle90, 300, 500, "nearest_neighbor", 2.5, 50, "positive", "first");

                HOperatorSet.RegionToBin(line, out _transimage, 255, 0, 3840, 2748);


                HOperatorSet.PointsFoerstner(_transimage, 1, 2, 3, 200, 0.3, "gauss", "false", out HTuple rowJunctions1, out HTuple columnJunctions1, out HTuple coRRJunctions1,
                    out HTuple coRCJunctions1, out HTuple coCCJunctions1, out HTuple rowArea1, out HTuple columnArea1, out HTuple coRRArea1, out HTuple coRCArea1, out HTuple coCCArea1);


                HOperatorSet.RegionToBin(line1, out _transimage, 255, 0, 3840, 2748);

                HOperatorSet.PointsFoerstner(_transimage, 1, 2, 3, 200, 0.3, "gauss", "false", out HTuple rowJunctions2, out HTuple columnJunctions2, out HTuple coRRJunctions2,
                    out HTuple coRCJunctions2, out HTuple coCCJunctions2, out HTuple rowArea2, out HTuple columnArea2, out HTuple coRRArea2, out HTuple coRCArea2, out HTuple coCCArea2);


                _transimage.Dispose();
                HOperatorSet.IntersectionLines(rowJunctions1[0], columnJunctions1[0], rowJunctions1[1], columnJunctions1[1], rowJunctions2[0], columnJunctions2[0], rowJunctions2[1], columnJunctions2[1], out awgX, out awgY, out HTuple isoverlapping);
                HOperatorSet.GenCrossContourXld(out cross, awgX, awgY, 20, phi);

            }
            catch (Exception ex)
            {
                HOperatorSet.WriteImage(_image, "bmp", 0, AppDomain.CurrentDomain.BaseDirectory + "awg.bmp");
                throw ex;
            }
        }

      
        private void btn_calibrate_Click(object sender, EventArgs e)
        {
            double[] point_x = new double[9];
            double[] point_y = new double[9];
            double[] pixel_x = new double[9];
            double[] pixel_y = new double[9];
            for (int i = 0; i < 9; i++)
            {
                DataGridViewRow dataGridViewRow = dGV_machinePoint.Rows[i];
                if (dataGridViewRow.Cells[1].Value == null || dataGridViewRow.Cells[2].Value == null)
                {
                    MessageBox.Show($"机器人第{(i + 1).ToString()}行点位信息为空", "提示");
                    return;
                }
                point_x[i] = double.Parse(dataGridViewRow.Cells[1].Value.ToString());
                point_y[i] = double.Parse(dataGridViewRow.Cells[2].Value.ToString());
                DataGridViewRow dataGridViewRow1 = dGV_pixelPoint.Rows[i];
                if (dataGridViewRow1.Cells[1].Value == null || dataGridViewRow1.Cells[2].Value == null)
                {
                    MessageBox.Show($"相机第{(i + 1).ToString()}行点位信息为空", "提示");
                    return;
                }
                pixel_x[i] = double.Parse(dataGridViewRow1.Cells[1].Value.ToString());
                pixel_y[i] = double.Parse(dataGridViewRow1.Cells[2].Value.ToString());
            }
             calibratedata = Calib9(point_x, point_y, pixel_x, pixel_y);
        }
        private HTuple Calib9(double[] point_x, double[] point_y, double[] pixel_x, double[] pixel_y)
        {
            HTuple x = new HTuple(point_x);
            HTuple y = new HTuple(point_y);
            HTuple pix_x = new HTuple(pixel_x);
            HTuple pix_y = new HTuple(pixel_y);
            HTuple hom2d = null;
            HOperatorSet.VectorToHomMat2d(pixel_x, pixel_y, x, y, out hom2d);
            return hom2d;
        }

        private void btn_saveCalibrateData_Click(object sender, EventArgs e)
        {
            HOperatorSet.WriteTuple(calibratedata, AppDomain.CurrentDomain.BaseDirectory+"calibrateData.tup");
        }

        private void btn_savePointData_Click(object sender, EventArgs e)
        {
            HTuple cameraX = new HTuple();
            HTuple cameraY = new HTuple();
            HTuple robotX = new HTuple();
            HTuple robotY = new HTuple();

            for (int i = 0; i < 9; i++)
            {
                DataGridViewRow dataGridViewRow = dGV_machinePoint.Rows[i];
                if (dataGridViewRow.Cells[1].Value == null || dataGridViewRow.Cells[2].Value == null)
                {
                    MessageBox.Show($"机器人第{(i + 1).ToString()}行点位信息为空", "提示");
                    return;
                }
                robotX.Append(dataGridViewRow.Cells[1].Value.ToString());
                robotY.Append(dataGridViewRow.Cells[2].Value.ToString());
            }
            for (int i = 0; i < 9; i++)
            {
                DataGridViewRow dataGridViewRow = dGV_pixelPoint.Rows[i];
                if (dataGridViewRow.Cells[1].Value == null || dataGridViewRow.Cells[2].Value == null)
                {
                    MessageBox.Show($"机器人第{(i + 1).ToString()}行点位信息为空", "提示");
                    return;
                }
                cameraX.Append(dataGridViewRow.Cells[1].Value.ToString());
                cameraY.Append(dataGridViewRow.Cells[2].Value.ToString());
            }
            string _path = AppDomain.CurrentDomain.BaseDirectory + "CalibrateData";
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
            HOperatorSet.WriteTuple(cameraX, _path + "\\cameraX.tup");
            HOperatorSet.WriteTuple(cameraY, _path + "\\cameraY.tup");
            HOperatorSet.WriteTuple(robotX, _path + "\\robotX.tup");
            HOperatorSet.WriteTuple(robotY, _path + "\\robotY.tup");
        }

        private void btn_AutoCalibrate_Click(object sender, EventArgs e)
        {
            Task.Run(new Action(() => 
            {
                string Aliger = "CWDM4";
                if (MessageBox.Show("请确认目标已处于视野左上方", "提示") != DialogResult.OK)
                    return;

                int xOffset = (int)nmUD_XOffset.Value;
                int yOffset = (int)nmUD_YOffset.Value;

                for (int j = 0; j < 3; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (i != 0)
                            service.__SSC_MoveAxis(Aliger, "Y", SSC_MoveMode.REL, 80, -yOffset);
                        Thread.Sleep(200);
                        double x = service.__SSC_GetAbsPosition(Aliger, "X");
                        double y = service.__SSC_GetAbsPosition(Aliger, "Y");
                        //dGV_machinePoint.Rows[j * 3 + i].Cells[1].Value = x;
                        //dGV_machinePoint.Rows[j * 3 + i].Cells[2].Value = y;
                        UpdateDataGridView(dGV_machinePoint, j * 3 + i, x, y);

                        Bitmap _image = camClient.GrabOneFrame("AWG");
                        visionFun.Bitmap2HObjectBpp32(_image, out HObject _himage);
                        GetAwgPosition(_himage, out HTuple awgx, out HTuple awgy, out HObject cross);
                        //dGV_pixelPoint.Rows[j * 3 + i].Cells[1].Value = awgx.D;
                        //dGV_pixelPoint.Rows[j * 3 + i].Cells[2].Value = awgy.D;
                        UpdateDataGridView(dGV_pixelPoint, j * 3 + i, awgx.D, awgy.D);
                        UpdateDisplay(hDisplay1, _himage, cross);
                        Thread.Sleep(300);
                    }
                    if (j != 2)
                    {
                        service.__SSC_MoveAxis(Aliger, "X", SSC_MoveMode.REL, 80, -xOffset);
                        service.__SSC_MoveAxis(Aliger, "Y", SSC_MoveMode.REL, 80, 2 * yOffset);
                        Thread.Sleep(100);
                    }

                }
                MessageBox.Show("标定完成");

            }));

           
        }
        private void UpdateDisplay(HDisplay _hDisplay,HObject image,HObject region)
        {
            if(_hDisplay.InvokeRequired)
            {
                _hDisplay.BeginInvoke(new Action(() =>
                {
                    _hDisplay.HImageX = image;
                    RegionX crossr = new RegionX(region, "green");
                    _hDisplay.HRegionXList = new List<RegionX>() { crossr };
                }));
            }
            else
            {
                _hDisplay.HImageX = image;
                RegionX crossr = new RegionX(region, "green");
                _hDisplay.HRegionXList = new List<RegionX>() { crossr };
            }
        }
        private void UpdateDataGridView(DataGridView dataGridView,int rowindex,double value1,double value2)
        {
            if(dataGridView.InvokeRequired)
            {
                dataGridView.BeginInvoke(new Action(() => { dataGridView.Rows[rowindex].Cells[1].Value = value1.ToString("F4"); dataGridView.Rows[rowindex].Cells[2].Value = value2.ToString("F4"); }));
            }
            else
            {
                dataGridView.Rows[rowindex].Cells[1].Value = value1.ToString("F4"); dataGridView.Rows[rowindex].Cells[2].Value = value2.ToString("F4");
            }
        }
    }
}
