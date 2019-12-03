using System.Drawing;
using System.Threading;
using HalconDotNet;
using CLCamera;
using System;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using AdjustOriginPosition.Service;
using AdjustOriginPosition.CamRAC;
using AxisPositionInfo;
using Newtonsoft;
using Vision;
namespace AdjustOriginPosition
{
    partial class APAS_UserScript
    {
        #region Variables

        const string programDataFile = "c:\\ProgramData\\AWG";
        const string Aliger = "CWDM4";
        const double awgOriginX = 87682.1;
        const double awgOriginY = 39979.2;
        const double awgOriginAngle = -0.0410431;

       
        const double pdOriginX = 1021;
        const double pdOriginY = 1265;
        const double pdOriginAngle = -0.03379;

        const string PM1 = "PM1906A 1";

        static HTuple calibratedata;
       
        [DllImport("Kernel32.dll")]
        internal static extern void CopyMemory(IntPtr dest, IntPtr source, IntPtr size);

        #endregion
        
        /// <summary>
        /// The section of the user process.
        /// 用户自定义流程函数。
        /// 
        /// Please write your process in the following method.
        /// 请在以下函数中定义您的工艺流程。
        /// 
        /// </summary>
        /// <param name="Service"></param>
        /// <returns></returns>
        static void BenchMarkSet(SystemServiceClient Service, CamRemoteAccessContractClient Camera = null)
        {
            string message = "";

           
            double pdX = 0, pdY = 0, pdAngle = 0;
            double offsetX, offsetY, offsetAngle;

            Camera.SetExposure("AWG", 30000);
            Camera.SetExposure("Left", 16000);

            
            try
            {
                if (!Directory.Exists(programDataFile))
                {
                    Directory.CreateDirectory(programDataFile);                    
                }
                    
                if(!File.Exists(programDataFile + "\\calibratedata.tup"))
                {
                    throw new Exception("cannot find the calibratedata  file");
                }

                if (!File.Exists(programDataFile + "\\Model"))
                {
                    throw new Exception("cannot find the  model file");
                }

                HOperatorSet.ReadTuple(programDataFile + "\\calibratedata.tup", out calibratedata);
                HOperatorSet.ReadShapeModel(programDataFile + "\\Model", out HTuple model);

                PointInfo pdOriginAngle = new PointInfo();
                pdOriginAngle.AxisInfos.Add(GenerateSingleAxisInfo(Service, Aliger, "R"));
                pdOriginAngle.Aliger = Aliger;
                pdOriginAngle.PointName = "pd初始角度";
                pdOriginAngle.Save(programDataFile, "pd初始角度.json");

                PointInfo awgCouplingLower = new PointInfo();
                awgCouplingLower.AxisInfos.Add(GenerateSingleAxisInfo(Service, Aliger, "Z",20));
                awgCouplingLower.Aliger = Aliger;
                awgCouplingLower.PointName = "Awg耦合低位";
                awgCouplingLower.Save(programDataFile, "Awg耦合低位.json");
              
                Service.__SSC_MoveAxis(Aliger, "Z", SSC_MoveMode.REL, 100, -500);

                PointInfo awgCouplingHigher = new PointInfo();
                awgCouplingHigher.AxisInfos.Add(GenerateSingleAxisInfo(Service, Aliger, "X"));
                awgCouplingHigher.AxisInfos.Add(GenerateSingleAxisInfo(Service, Aliger, "Y"));
                awgCouplingHigher.AxisInfos.Add(GenerateSingleAxisInfo(Service, Aliger, "Z",100,2));
                awgCouplingHigher.Aliger = Aliger;
                awgCouplingHigher.PointName = "Awg耦合高位";
                awgCouplingHigher.Save(programDataFile, "Awg耦合高位.json");


                
                #region  pd初始位置获取并保存
                Service.__SSC_MoveToPresetPosition(Conditions.ALIGNER, "pd拍照位置");
                var image2 = Camera.GrabOneFrame("Left");
                HObject pdImage;
                visionFun.Bitmap2HObjectBpp32(image2, out pdImage);
                GetPdOriginPosition(pdImage, out double pdx, out double pdy, out double pdangle, out Bitmap pdresultImage);
                Service.__SSC_ShowImage(pdresultImage);
                HTuple PdOrigin = new HTuple();
                PdOrigin.Append(pdx);
                PdOrigin.Append(pdy);
                PdOrigin.Append(pdangle);
                HOperatorSet.WriteTuple(PdOrigin, programDataFile + "\\PdOrigin.tup");

                #endregion


                #region awg初始位置获取并保存
                Service.__SSC_MoveToPresetPosition(Conditions.ALIGNER, "awg拍照位置");
                Thread.Sleep(100);
                var image1 = Camera.GrabOneFrame("AWG");
                HObject awgImage;
                visionFun.Bitmap2HObjectBpp32(image1, out awgImage);
                GetAwgOriginPosition(awgImage, out double awgX, out double awgY, out double awgAngle, out Bitmap awgimage);
                Service.__SSC_ShowImage(awgimage);
                HTuple AwgOrigin = new HTuple();
                AwgOrigin.Append(awgX);
                AwgOrigin.Append(awgY);
                AwgOrigin.Append(awgAngle);
                HOperatorSet.WriteTuple(AwgOrigin, programDataFile + "\\AwgOrigin.tup");
                #endregion

            }
            catch (Exception ex)
            {
                Service.__SSC_LogError(ex.Message);
            }

            System.Threading.Thread.Sleep(100);
        }
       
        #region Private Methods
       static void GetAwgOriginPosition(HObject _image, out double awgX, out double awgY, out  double awgAngle, out Bitmap resultImage)
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



                HOperatorSet.IntersectionLines(rowJunctions1[0], columnJunctions1[0], rowJunctions1[1], columnJunctions1[1], rowJunctions2[0], columnJunctions2[0], rowJunctions2[1], columnJunctions2[1], out HTuple finalRow, out HTuple finalColumn, out HTuple isoverlapping);


                HOperatorSet.AffineTransPoint2d(calibratedata, finalRow, finalColumn, out HTuple x, out HTuple y);

                Console.WriteLine($"awg row value: {finalRow.D}");
                Console.WriteLine($"awg column value: {finalColumn.D}");

                visionFun.HObject2Bpp8(_image, out Bitmap bitmap);
                resultImage = BitMapZd.DrawCross(bitmap, (float)finalColumn.D, (float)finalRow.D, 45, 30, 10, Color.Red);

                awgX = x;
                awgY = y;
                awgAngle =  phi;
          
            }
            catch (Exception ex)
            {
                HOperatorSet.WriteImage(_image, "bmp", 0, AppDomain.CurrentDomain.BaseDirectory + "awg.bmp");
                throw ex;
            }
        }

       
        static void GetPdOriginPosition(HObject _image, out double pdX, out  double pdY, out  double pdAngle, out Bitmap pdresult)
        {
            try
            {

                HObject ho_Region, ho_RegionErosion;
                HObject ho_ConnectedRegions, ho_SelectedRegions, ho_RegionDilation;
                HObject ho_RegionFillUp, ho_ImageReduced, ho_Region1, ho_RegionErosion1;
                HObject ho_ConnectedRegions1, ho_SelectedRegions1, ho_RegionUnion;
                HObject ho_RegionTrans, ho_ImageReduced1, ho_Region2, ho_RegionFillUp1;
                HObject ho_ImageReduced2, ho_Region3, ho_RegionErosion2;
                HObject ho_ConnectedRegions2, ho_SelectedRegions2, ho_RegionTrans1;
                HObject ho_RegionUnion1, ho_RegionTrans2, ho_Cross;

                // Local control variables 

                HTuple hv_Area = null, hv_Row = null, hv_Column = null;
                HTuple hv_Value = null;
                // Initialize local and output iconic variables 

                HOperatorSet.GenEmptyObj(out ho_Region);
                HOperatorSet.GenEmptyObj(out ho_RegionErosion);
                HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
                HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
                HOperatorSet.GenEmptyObj(out ho_RegionDilation);
                HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced);
                HOperatorSet.GenEmptyObj(out ho_Region1);
                HOperatorSet.GenEmptyObj(out ho_RegionErosion1);
                HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
                HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
                HOperatorSet.GenEmptyObj(out ho_RegionUnion);
                HOperatorSet.GenEmptyObj(out ho_RegionTrans);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
                HOperatorSet.GenEmptyObj(out ho_Region2);
                HOperatorSet.GenEmptyObj(out ho_RegionFillUp1);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced2);
                HOperatorSet.GenEmptyObj(out ho_Region3);
                HOperatorSet.GenEmptyObj(out ho_RegionErosion2);
                HOperatorSet.GenEmptyObj(out ho_ConnectedRegions2);
                HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
                HOperatorSet.GenEmptyObj(out ho_RegionTrans1);
                HOperatorSet.GenEmptyObj(out ho_RegionUnion1);
                HOperatorSet.GenEmptyObj(out ho_RegionTrans2);
                HOperatorSet.GenEmptyObj(out ho_Cross);

                ho_Region.Dispose();
                HOperatorSet.Threshold(_image, out ho_Region, 0, 155);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionCircle(ho_Region, out ho_RegionErosion, 6.5);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_RegionErosion, out ho_ConnectedRegions);
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area",
                    "and", 500000, 99999999);
                ho_RegionDilation.Dispose();
                HOperatorSet.DilationCircle(ho_SelectedRegions, out ho_RegionDilation, 3.5);
                ho_RegionFillUp.Dispose();
                HOperatorSet.FillUpShape(ho_RegionDilation, out ho_RegionFillUp, "area", 1, 300000);
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(_image, ho_RegionFillUp, out ho_ImageReduced);
                ho_Region1.Dispose();
                HOperatorSet.Threshold(ho_ImageReduced, out ho_Region1, 240, 255);
                HOperatorSet.WriteImage(ho_ImageReduced, "bmp", 0, AppDomain.CurrentDomain.BaseDirectory + "01.bmp");
                ho_RegionErosion1.Dispose();
                HOperatorSet.ErosionCircle(ho_Region1, out ho_RegionErosion1, 1.5);
                ho_ConnectedRegions1.Dispose();
                HOperatorSet.Connection(ho_RegionErosion1, out ho_ConnectedRegions1);
                ho_SelectedRegions1.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions1, "area",
                    "and", 700, 99999);
                ho_RegionUnion.Dispose();
                HOperatorSet.Union1(ho_SelectedRegions1, out ho_RegionUnion);
                ho_RegionTrans.Dispose();
                HOperatorSet.ShapeTrans(ho_RegionUnion, out ho_RegionTrans, "rectangle2");

                ho_ImageReduced1.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageReduced, ho_RegionTrans, out ho_ImageReduced1
                    );
                HOperatorSet.WriteImage(ho_ImageReduced1, "bmp", 0, AppDomain.CurrentDomain.BaseDirectory + "02.bmp");

                ho_Region2.Dispose();
                HOperatorSet.Threshold(ho_ImageReduced1, out ho_Region2, 148, 255);

                ho_RegionFillUp1.Dispose();
                HOperatorSet.FillUp(ho_Region2, out ho_RegionFillUp1);




                ho_ImageReduced2.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageReduced1, ho_RegionFillUp1, out ho_ImageReduced2
                    );

                ho_Region3.Dispose();
                HOperatorSet.Threshold(ho_ImageReduced2, out ho_Region3, 0, 128);
                ho_RegionErosion2.Dispose();
                HOperatorSet.ErosionCircle(ho_Region3, out ho_RegionErosion2, 1.5);
                ho_ConnectedRegions2.Dispose();
                HOperatorSet.Connection(ho_RegionErosion2, out ho_ConnectedRegions2);
                ho_SelectedRegions2.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions2, out ho_SelectedRegions2, "area",
                    "and", 100, 1000);
                HOperatorSet.SelectShape(ho_SelectedRegions2, out HObject ho_SelectedRegions3, "outer_radius", "and", 5, 20);
                ho_RegionTrans1.Dispose();
                HOperatorSet.ShapeTrans(ho_SelectedRegions3, out ho_RegionTrans1, "outer_circle");

                ho_RegionUnion1.Dispose();
                HOperatorSet.Union1(ho_RegionTrans1, out ho_RegionUnion1);
                ho_RegionTrans2.Dispose();
                HOperatorSet.ShapeTrans(ho_RegionUnion1, out ho_RegionTrans2, "rectangle2");

                HOperatorSet.AreaCenter(ho_RegionTrans2, out hv_Area, out hv_Row, out hv_Column);
                ho_Cross.Dispose();
                //  HOperatorSet.GenCrossContourXld(out ho_Cross, hv_Row, hv_Column, 50, 0.785398);

                HOperatorSet.RegionFeatures(ho_RegionTrans2, "rect2_phi", out hv_Value);
                //  hv_Value = (hv_Value * 180) / 3.1415926;

                visionFun.HObject2Bpp8(_image, out Bitmap bitmap);
                pdresult = BitMapZd.DrawCross(bitmap, (float)hv_Column.D, (float)hv_Row.D, 45, 30, 10, Color.Red);
                HOperatorSet.WriteImage(_image, "bmp", 0, AppDomain.CurrentDomain.BaseDirectory + "pd.bmp");
                Console.WriteLine($"pd row value{hv_Row.D}");
                Console.WriteLine($"pd column value{hv_Column.D}");

                pdX = hv_Row.D;
                pdY = hv_Column.D;
                pdAngle = hv_Value.D;

            

                ho_Region.Dispose();
                ho_RegionErosion.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions.Dispose();
                ho_RegionDilation.Dispose();
                ho_RegionFillUp.Dispose();
                ho_ImageReduced.Dispose();
                ho_Region1.Dispose();
                ho_RegionErosion1.Dispose();
                ho_ConnectedRegions1.Dispose();
                ho_SelectedRegions1.Dispose();
                ho_RegionUnion.Dispose();
                ho_RegionTrans.Dispose();
                ho_ImageReduced1.Dispose();
                ho_Region2.Dispose();
                ho_RegionFillUp1.Dispose();
                ho_ImageReduced2.Dispose();
                ho_Region3.Dispose();
                ho_RegionErosion2.Dispose();
                ho_ConnectedRegions2.Dispose();
                ho_SelectedRegions2.Dispose();
                ho_RegionTrans1.Dispose();
                ho_RegionUnion1.Dispose();
                ho_RegionTrans2.Dispose();
                ho_Cross.Dispose();
          

            }
            catch (Exception ex)
            {
                HOperatorSet.WriteImage(_image, "bmp", 0, AppDomain.CurrentDomain.BaseDirectory + "pd.bmp");

                throw ex;
                //  MessageBox.Show(ex.Message);
            }
        }

        static SingleAxisInfo GenerateSingleAxisInfo(SystemServiceClient Service,string aliger,string axisname,int speed=100,int rank=1)
        {
            axisname = axisname.ToUpper();
            double positon = Service.__SSC_GetAbsPosition(aliger, axisname);

            return new SingleAxisInfo(axisname, positon, speed, rank);
        }
        #endregion
    }

}
