using System.Drawing;
using System.Threading;
using UserScript.Service;
using HalconDotNet;
using CLCamera;
using System;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using UserScript.CamRAC;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using AxisPositionInfo;
using System.Threading.Tasks;
using Vision;
namespace UserScript
{
    partial class APAS_UserScript
    {
        #region Variables

        const string EQUIP_CAPTION = "MercuryHostBoard";
        const string programDataFile = "c:\\ProgramData\\AWG";

        static CameraBase camTop, camDown;

        



        static  double awgOriginX ;
        static double awgOriginY ;
        static double awgOriginAngle ;


        static double pdOriginX ;
        static double pdOriginY ;
        static double pdOriginAngle ;

        const string PM1 = "PM1906A 1";

        const string TEST_IO_REDLIGHT = "红灯";
        const string TEST_IO_YELLIGHT = "黄灯";
        const string TEST_IO_GRELIGHT = "绿灯";

        const string TEST_IO1 = "左后门关";

        static HTuple calibratedata;//标定数据
        static HTuple model;//模板数据       
        #endregion

        #region User Process

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
        static void UserProc(SystemServiceClient Service, CamRemoteAccessContractClient Camera = null)
        {
            string message = "";
            int cycles = 0;
            bool goodAlign = false;
            string alignmentProfile = "";

            double awgX = 0, awgY = 0, awgAngle = 0;
            double pdX = 0, pdY = 0, pdAngle = 0;
            double offsetX, offsetY, offsetAngle;

            Camera.SetExposure("AWG", 30000);
            Camera.SetExposure("Left", 16000);
            Init();

            try
            {
                // 抬起针筒
                Service.__SSC_WriteIO(Conditions.IO_INJECTOR, SSC_IOStatusEnum.Disabled);

                // 识别AWG角度
                Service.__SSC_LogInfo("移动到AWG角度识别位置...");
                Service.__SSC_MoveToPresetPosition(Conditions.ALIGNER, "awg拍照位置");

                MoveToPosition(Service, "pd初始角度");
               // Service.__SSC_MoveToPresetPosition(Conditions.ALIGNER, "pd初始角度");
                Thread.Sleep(100);

                Service.__SSC_LogInfo("识别AWG角度...");
                var image1 = Camera.GrabOneFrame("AWG");

                HObject awgImage;
               
                visionFun.Bitmap2HObjectBpp32(image1, out awgImage);
                GetAwgOffset(awgImage, ref awgX, ref awgY, ref awgAngle, out Bitmap awgimage);
                Service.__SSC_ShowImage(awgimage);
                awgImage.Dispose();
                awgimage.Dispose();
                awgImage = null;
                awgimage = null;

                Service.__SSC_MoveAxis(Conditions.ALIGNER, "R", SSC_MoveMode.REL, 100, -awgAngle);
                Thread.Sleep(100);

                // 识别PD Array角度
                Service.__SSC_LogInfo("移动到PD Array角度识别位置...");
                Service.__SSC_MoveToPresetPosition(Conditions.ALIGNER, "pd拍照位置");
                Thread.Sleep(100);
                Service.__SSC_LogInfo("识别PA Array角度...");
                var image2 = Camera.GrabOneFrame("Left");

                HObject pdImage;
               
                visionFun.Bitmap2HObjectBpp32(image2, out pdImage);
                GetPdOffset(pdImage, ref pdX, ref pdY, ref pdAngle, out Bitmap pdimage);
                Service.__SSC_ShowImage(pdimage);
                pdImage.Dispose();
                pdimage.Dispose();
                pdImage = null;
                pdimage = null;

                offsetX = -awgX - pdX;
                offsetY = -awgY - pdY;

                Service.__SSC_LogInfo($"x 方向总偏移量 {offsetX}");
                Service.__SSC_LogInfo($"y 方向总偏移量 {offsetY}");

                MoveToPosition(Service, "Awg耦合高位");
               // Service.__SSC_MoveToPresetPosition(Conditions.ALIGNER, "耦合高位");
                Thread.Sleep(100);


                Service.__SSC_MoveAxis(Conditions.ALIGNER, "X", SSC_MoveMode.REL, 100, -awgX);
                Thread.Sleep(100);

                Service.__SSC_MoveAxis(Conditions.ALIGNER, "X", SSC_MoveMode.REL, 100, -pdX);
                Thread.Sleep(100);


                Service.__SSC_MoveAxis(Conditions.ALIGNER, "Y", SSC_MoveMode.REL, 100, -awgY);
                Thread.Sleep(100);

                Service.__SSC_MoveAxis(Conditions.ALIGNER, "Y", SSC_MoveMode.REL, 100, -pdY);
                Thread.Sleep(100);

                var image3 = Camera.GrabOneFrame("Rear");

                HObject awgImage1;

                visionFun.Bitmap2HObjectBpp32(image3, out awgImage1);
                GetAwgVerticalHeight(Service, awgImage1);



                Console.WriteLine("Press any key to go to 耦合位置.");
                Console.ReadKey();
                Thread.Sleep(100);

                MoveToPosition(Service, "Awg耦合低位");
              //  Service.__SSC_MoveToPresetPosition(Conditions.ALIGNER, "耦合位置");

                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();


                int xmaxpath = 0;
                double[] data = new double[4];
                data[0] = Service.__SSC_MeasurableDevice_Read($"{EQUIP_CAPTION},0");
                data[1] = Service.__SSC_MeasurableDevice_Read($"{EQUIP_CAPTION},1");
                data[2] = Service.__SSC_MeasurableDevice_Read($"{EQUIP_CAPTION},2");
                data[3] = Service.__SSC_MeasurableDevice_Read($"{EQUIP_CAPTION},3");

                message = "预对准初始响应度：";
                for (int i = 0; i < 4; i++)
                {
                    message += $"[{i}]{data[i]}  ";
                }
                Service.__SSC_LogInfo(message);

                if (data[0] < Conditions.Resp_After_VisionAlign)
                {
                    Service.__SSC_LogError("视觉对准初始光功率过低，请检查产品。");
                    return;
                }

                #region 粗耦合

                // 如果初始响应度大于阈值，跳过粗找光。
                if (data[0] < Conditions.Resp_After_RoughAlign)
                {
                    cycles = 0;
                    goodAlign = false;

                    Service.__SSC_LogInfo("开始粗找光...");

                    while (cycles < 5)
                    {
                        cycles++;

                        alignmentProfile = "x&y_roughScan";
                        Service.__SSC_LogInfo($"执行Profile-ND，参数[{alignmentProfile}]，Cycle {cycles}/5...");
                        Service.__SSC_DoProfileND(alignmentProfile);

                        var resp = Service.__SSC_MeasurableDevice_Read($"{EQUIP_CAPTION},0");
                        Service.__SSC_LogInfo($"响应度：{resp}/目标值: {Conditions.Resp_After_RoughAlign}");

                        if (resp >= Conditions.Resp_After_RoughAlign)
                        {
                            goodAlign = true;
                            break;
                        }
                    }

                    if (!goodAlign)
                    {
                        Service.__SSC_LogError("CH1响应度无法达到规格。");
                        return;
                    }
                }

                #endregion

                #region 精细耦合

                cycles = 0;
                goodAlign = false;

                Service.__SSC_LogInfo("开始细找光...");

                while (cycles < 5)
                {
                    cycles++;

                    alignmentProfile = "x&y_detailScan";
                    Service.__SSC_LogInfo($"执行Profile-ND，参数[{alignmentProfile}]，Cycle {cycles}/5...");
                    Service.__SSC_DoProfileND(alignmentProfile);

                    var resp = Service.__SSC_MeasurableDevice_Read($"{EQUIP_CAPTION},0");
                    Service.__SSC_LogInfo($"响应度：{resp}/目标值: {Conditions.Resp_After_AccuracyAlign}");

                    if (resp >= Conditions.Resp_After_AccuracyAlign)
                    {
                        goodAlign = true;
                        break;
                    }
                }

                if (!goodAlign)
                {
                    Service.__SSC_LogError("CH1响应度无法达到规格。");
                    return;
                }

                #endregion

                #region 角度调整

                cycles = 0;
                goodAlign = false;

                Service.__SSC_LogInfo("开始角度调整...");

                while (cycles < 5)
                {
                    cycles++;

                    alignmentProfile = "mercury";
                    Service.__SSC_LogInfo($"执行Angle Tuning，参数[{alignmentProfile}]，Cycle {cycles}/5...");
                    var diff = (double)Service.__SSC_DoAngleTuning(alignmentProfile);
                    Service.__SSC_LogInfo($"1-4通道峰值位置误差：{diff.ToString("F3")}um/目标值： {Conditions.Resp_After_AngleTuning_PeakPosDiff}");

                    if (Math.Abs(diff) <= Conditions.Resp_After_AngleTuning_PeakPosDiff)
                    {
                        goodAlign = true;
                        break;
                    }
                }

                if (!goodAlign)
                    Service.__SSC_LogError("1-4通道峰值位置误差无法达到规格！");

                #endregion

                #region 角度调整后精细耦合

                cycles = 0;
                goodAlign = false;

                Service.__SSC_LogInfo("开始Final找光...");

                while (cycles < 5)
                {
                    cycles++;

                    alignmentProfile = "x&y_detailScan";
                    Service.__SSC_LogInfo($"执行Profile-ND，参数[{alignmentProfile}]，Cycle {cycles}/5...");
                    Service.__SSC_DoProfileND(alignmentProfile);

                    var resp = Service.__SSC_MeasurableDevice_Read($"{EQUIP_CAPTION},0");
                    Service.__SSC_LogInfo($"响应度：{resp}/目标值: {Conditions.Resp_Final}");

                    if (resp >= Conditions.Resp_Final)
                    {
                        goodAlign = true;
                        break;
                    }
                }

                if (!goodAlign)
                {
                    Service.__SSC_LogError("CH1响应度无法达到规格。");
                    return;
                }

                #endregion

                #region 检查四个通道响应度

                double[] finalResp = new double[4];
                for (int i = 0; i < 4; i++)
                {
                    finalResp[i] = Service.__SSC_MeasurableDevice_Read($"{EQUIP_CAPTION},{i}");
                }

                if (finalResp.Min() < Conditions.Resp_Final)
                {
                    Service.__SSC_LogError($"最小响应度低于规格，规格：{Conditions.Resp_Final}");
                    return;
                }
                else if (finalResp.Max() - finalResp.Min() > Conditions.Resp_Final_Diff)
                {
                    Service.__SSC_LogError($"通道平衡无法达到规格，规格：{Conditions.Resp_Final_Diff}");
                    return;
                }
                else
                {
                    Service.__SSC_LogInfo("耦合完成!");
                }


                #endregion

                #region 保存当前位置

                Service.__SSC_LogInfo("保存当前位置...");

                var final_x = Service.__SSC_GetAbsPosition(Conditions.ALIGNER, "X");
                var final_y = Service.__SSC_GetAbsPosition(Conditions.ALIGNER, "Y");
                var final_z = Service.__SSC_GetAbsPosition(Conditions.ALIGNER, "Z");

                Service.__SSC_LogInfo($"X: {final_x}, Y: {final_y}, Z: {final_z}");

                #endregion
                Console.WriteLine("请点胶");
                Console.ReadKey();
                Service.__SSC_MoveAxis(Conditions.ALIGNER, "Z", SSC_MoveMode.REL, 100, -10000);
                Thread.Sleep(100);
                Console.WriteLine("点胶完成，按任意键继续");
                Console.ReadKey();
                Service.__SSC_MoveAxis(Conditions.ALIGNER, "Z", SSC_MoveMode.REL, 100, 9500);
                Thread.Sleep(100);
                Service.__SSC_MoveAxis(Conditions.ALIGNER, "Z", SSC_MoveMode.REL, 20, 500);
                Thread.Sleep(100);
                return;
                #region 点胶

                Service.__SSC_LogInfo("开始点胶...");

                // 上提Z轴，AWG退到安全位置
                Service.__SSC_MoveAxis(Conditions.ALIGNER, "Z", SSC_MoveMode.REL, 20, -1000);

                // 移动到点胶位置
                Service.__SSC_MoveToPresetPosition(Conditions.ALIGNER, "点胶位置");
                Thread.Sleep(200);

                // 降下针筒
                Service.__SSC_WriteIO(Conditions.IO_INJECTOR, SSC_IOStatusEnum.Enabled);
                Thread.Sleep(200);

                // 启动点胶机
                Service.__SSC_LogInfo("点胶...");

                // 抬起针筒
                Service.__SSC_WriteIO(Conditions.IO_INJECTOR, SSC_IOStatusEnum.Disabled);

                #endregion

                #region 恢复AWG位置，重新对准

                Service.__SSC_LogInfo("恢复AWG位置...");

                Service.__SSC_MoveAxis(Conditions.ALIGNER, "X", SSC_MoveMode.ABS, 100, final_x);
                Service.__SSC_MoveAxis(Conditions.ALIGNER, "Y", SSC_MoveMode.ABS, 100, final_y);
                Service.__SSC_MoveAxis(Conditions.ALIGNER, "Z", SSC_MoveMode.ABS, 100, final_z);

                #endregion
            

            }
            catch (Exception ex)
            {
                Service.__SSC_LogError(ex.Message);
            }

            System.Threading.Thread.Sleep(100);
        }

        #endregion

        #region Private Methods
        static void  Init()
        {
            if (!Directory.Exists(programDataFile))
            {
                Directory.CreateDirectory(programDataFile);
            }

            if (!File.Exists(programDataFile + "\\calibratedata.tup"))
            {
                throw new Exception("cannot find the calibratedata  file");
            }

            if (!File.Exists(programDataFile + "\\Model"))
            {
                throw new Exception("cannot find the  model file");
            }

            if(!File.Exists(programDataFile+ "\\AwgOrigin.tup"))
            {
                throw new Exception("cannot find the  AwgOrigin.tup file");
            }

            if (!File.Exists(programDataFile + "\\PdOrigin.tup"))
            {
                throw new Exception("cannot find the  PdOrigin.tup file");
            }

            
           

            HOperatorSet.ReadTuple(programDataFile + "\\calibratedata.tup", out calibratedata);
            HOperatorSet.ReadShapeModel(programDataFile + "\\Model", out HTuple model);

            HOperatorSet.ReadTuple(programDataFile + "\\AwgOrigin.tup", out HTuple awgorigin);
            if(awgorigin.Length!=3)
            {
                throw new Exception("  AwgOrigin.tup file is damage");
            }
            awgOriginX = awgorigin[0];
            awgOriginY = awgorigin[1];
            awgOriginAngle = awgorigin[2];

            HOperatorSet.ReadTuple(programDataFile + "\\PdOrigin.tup", out HTuple pdOrigin);
            if (pdOrigin.Length != 3)
            {
                throw new Exception("  pdOrigin.tup file is damage");
            }
            pdOriginX = pdOrigin[0];
            pdOriginY = pdOrigin[1];
            pdOriginAngle = pdOrigin[2];



        }
        static void MoveToPosition(SystemServiceClient Service,string pName)
        {
            string filepath = programDataFile + $"\\{pName}.json";
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException($"{ pName }.json is not found");
            }
           string data= File.ReadAllText(filepath);
            PointInfo pointInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PointInfo>(data);
            
            int maxRank = pointInfo.AxisInfos.Last().Rank;
            for(int i=0;i<maxRank;i++)
            {
                List<Task> tasks = new List<Task>();
                foreach (var mem in pointInfo.AxisInfos)
                {
                    if (mem.Rank == i+1)
                    {
                        var m= Service.__SSC_MoveAxisAsync(pointInfo.Aliger, mem.AxisName, SSC_MoveMode.ABS, mem.Speed, mem.Position);
                       // Task t = new Task(() => { Service.__SSC_MoveAxisAsync(pointInfo.Aliger, mem.AxisName, SSC_MoveMode.ABS, mem.Speed, mem.Position); });
                     //   t.Start();

                        tasks.Add(m);
                    }
                }
                
                Task.WaitAll(tasks.ToArray());
            }


            
            

        }
        
        static void GetAwgOffset(HObject _image, ref double awgX, ref double awgY, ref double awgAngle, out Bitmap resultImage)
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
              
                Vision.visionFun.findline(imageMean, out HObject line, _row, _column, phi, 150, 150, "nearest_neighbor", 2.5, 25, "positive", "last");

                double rx = (rowJunctions[0] + rowJunctions[1]) / 2;
                double ry = (columnJunctions[0] + columnJunctions[1]) / 2;

                HOperatorSet.TupleRad(-90, out HTuple _angle90);
                Vision.visionFun.findline(_image, out HObject line1, rx, ry, phi + _angle90, 300, 500, "nearest_neighbor", 2.5, 50, "positive", "first");


                HOperatorSet.RegionToBin(line, out  _transimage, 255, 0, 3840, 2748);

                
                HOperatorSet.PointsFoerstner(_transimage, 1, 2, 3, 200, 0.3, "gauss", "false", out HTuple rowJunctions1, out HTuple columnJunctions1, out HTuple coRRJunctions1,
                    out HTuple coRCJunctions1, out HTuple coCCJunctions1, out HTuple rowArea1, out HTuple columnArea1, out HTuple coRRArea1, out HTuple coRCArea1, out HTuple coCCArea1);

               

                HOperatorSet.RegionToBin(line1, out _transimage, 255, 0, 3840, 2748);


                HOperatorSet.PointsFoerstner(_transimage, 1, 2, 3, 200, 0.3, "gauss", "false", out HTuple rowJunctions2, out HTuple columnJunctions2, out HTuple coRRJunctions2,
                    out HTuple coRCJunctions2, out HTuple coCCJunctions2, out HTuple rowArea2, out HTuple columnArea2, out HTuple coRRArea2, out HTuple coRCArea2, out HTuple coCCArea2);


                _transimage.Dispose();



                HOperatorSet.IntersectionLines(rowJunctions1[0], columnJunctions1[0], rowJunctions1[ 1], columnJunctions1[1], rowJunctions2[0], columnJunctions2[0], rowJunctions2[ 1], columnJunctions2[ 1], out HTuple finalRow, out HTuple finalColumn, out HTuple isoverlapping);

                HOperatorSet.AffineTransPoint2d(calibratedata, finalRow, finalColumn, out HTuple x, out HTuple y);


                Console.WriteLine($"awg row value: {finalRow.D}");
                Console.WriteLine($"awg column value: {finalColumn.D}");
              
                visionFun.HObject2Bpp8(_image, out Bitmap bitmap);
                resultImage = BitMapZd.DrawCross(bitmap, (float)finalColumn.D, (float)finalRow.D, 45, 30, 10, Color.Red);
      
                awgX = x - awgOriginX;
                awgY = y - awgOriginY;
            
                awgAngle = (awgOriginAngle - phi) * 180 / Math.PI;
                Console.WriteLine("awg x offset: " + awgX.ToString("F6"));
                Console.WriteLine("awg y offset: " + awgY.ToString("F6"));
                Console.WriteLine("awg angle offset: " + awgAngle.ToString("F6"));

            }
            catch (Exception ex)
            {
                HOperatorSet.WriteImage(_image, "bmp", 0, AppDomain.CurrentDomain.BaseDirectory + "awg.bmp");
                throw ex;


            }
        }
        static void GetPdOffset(HObject _image, ref double pdX, ref double pdY, ref double pdAngle, out Bitmap pdresult)
        {
            try
            {

                //     _image = camTop.SnapShot();
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
                HOperatorSet.Threshold(ho_ImageReduced2, out ho_Region3, 0, 140);
                ho_RegionErosion2.Dispose();
                HOperatorSet.ErosionCircle(ho_Region3, out ho_RegionErosion2, 1.5);
                ho_ConnectedRegions2.Dispose();
                HOperatorSet.Connection(ho_RegionErosion2, out ho_ConnectedRegions2);
                ho_SelectedRegions2.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions2, out ho_SelectedRegions2, "area",
                    "and", 250, 800);
                HOperatorSet.SelectShape(ho_SelectedRegions2, out HObject  ho_SelectedRegions3, "outer_radius", "and", 10, 25);
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

                Console.WriteLine($"pd row value{hv_Row.D}");
                Console.WriteLine($"pd column value{hv_Column.D}");

                pdX = (hv_Row- pdOriginX) / 236 * 200;
                pdY = (hv_Column- pdOriginY) / 236 * 200;
                pdAngle = (pdOriginAngle - hv_Value) * 180 / Math.PI;

                Console.WriteLine("pd x offset: " + pdX.ToString("F6"));
                Console.WriteLine("pd y offset: " + pdY.ToString("F6"));
                Console.WriteLine("pd angle offset: " + pdAngle.ToString("F6"));


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
                //  RegionX regionX1 = new RegionX(corss, "green");
                //  RegionX regionX2 = new RegionX(arrow, "green");

                //   ShowImage(hDisplay1, _image, new List<RegionX>() { regionX1, regionX2 });
                //   SetTextBox(txt_pdx, pdX.ToString("F6"));
                //    SetTextBox(txt_pdy, pdY.ToString("F6"));
                //    SetTextBox(txt_pdangle, pdAngle.ToString("F6"));

                //     offsetX = awgX + pdX;
                //     offsetY = awgY + pdY;

                //     SetTextBox(txt_x, offsetX.ToString("F6"));
                //     SetTextBox(txt_y, offsetY.ToString("F6"));

            }
            catch (Exception ex)
            {
                HOperatorSet.WriteImage(_image, "bmp", 0, AppDomain.CurrentDomain.BaseDirectory + "pd.bmp");

                throw ex;
                //  MessageBox.Show(ex.Message);
            }
        }
        static void GetAwgVerticalHeight(SystemServiceClient service,HObject _image)
        {
            double rRow = 941;
            double rColumn = 601;
            double rPhi = -1.57;
            double rLength1 = 75;
            double rLength2 = 383;
            HTuple minScaleValue = 130;
            HTuple maxScaleValue = 190;
            visionFun.scale_image_range(_image, out HObject scaleImage, minScaleValue, maxScaleValue);
            visionFun.findline(scaleImage, out HObject line, rRow, rColumn, rPhi, rLength1, rLength2, "nearest_neighbor", 1.5, 30, "positive", "first");
            HOperatorSet.AreaCenter(line, out HTuple area, out HTuple row, out HTuple column);
            HOperatorSet.GetRegionPoints(line, out HTuple rows, out HTuple columns);
            visionFun.HObject2Bpp8(_image, out Bitmap bitmap);
          var rtnImage=  BitMapZd.DrawLine(bitmap, columns[0], rows[0], columns[columns.Length - 1], rows[rows.Length - 1],40,Color.LightGreen );
            service.__SSC_ShowImage(rtnImage);
            bitmap.Dispose();
            Console.WriteLine($"the awg height is  {row.D},");
            Console.WriteLine($"the awg vertical offset is  {row.D-954},");
        }
        #endregion

    }
   
}
