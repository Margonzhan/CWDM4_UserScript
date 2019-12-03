using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Dynamic;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace Vision
{
    public static  class visionFun
    {
        [DllImport("Kernel32.dll")]
        internal static extern void CopyMemory(IntPtr dest, IntPtr source, IntPtr size);


        public static void scale_image_range(HObject ho_Image, out HObject ho_ImageScaled, HTuple hv_Min, HTuple hv_Max)
        {
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_ImageSelected = null, ho_SelectedChannel = null;
            HObject ho_LowerRegion = null, ho_UpperRegion = null, ho_ImageSelectedScaled = null;

            // Local copy input parameter variables 
            HObject ho_Image_COPY_INP_TMP;
            ho_Image_COPY_INP_TMP = ho_Image.CopyObj(1, -1);


            HTuple hv_LowerLimit = new HTuple(), hv_UpperLimit = new HTuple();
            HTuple hv_Mult = null, hv_Add = null, hv_NumImages = null;
            HTuple hv_ImageIndex = null, hv_Channels = new HTuple();
            HTuple hv_ChannelIndex = new HTuple(), hv_MinGray = new HTuple();
            HTuple hv_MaxGray = new HTuple(), hv_Range = new HTuple();
            HTuple hv_Max_COPY_INP_TMP = hv_Max.Clone();
            HTuple hv_Min_COPY_INP_TMP = hv_Min.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_ImageSelected);
            HOperatorSet.GenEmptyObj(out ho_SelectedChannel);
            HOperatorSet.GenEmptyObj(out ho_LowerRegion);
            HOperatorSet.GenEmptyObj(out ho_UpperRegion);
            HOperatorSet.GenEmptyObj(out ho_ImageSelectedScaled);
            try
            {
                if ((int)(new HTuple((new HTuple(hv_Min_COPY_INP_TMP.TupleLength())).TupleEqual(
                    2))) != 0)
                {
                    hv_LowerLimit = hv_Min_COPY_INP_TMP.TupleSelect(1);
                    hv_Min_COPY_INP_TMP = hv_Min_COPY_INP_TMP.TupleSelect(0);
                }
                else
                {
                    hv_LowerLimit = 0.0;
                }
                if ((int)(new HTuple((new HTuple(hv_Max_COPY_INP_TMP.TupleLength())).TupleEqual(
                    2))) != 0)
                {
                    hv_UpperLimit = hv_Max_COPY_INP_TMP.TupleSelect(1);
                    hv_Max_COPY_INP_TMP = hv_Max_COPY_INP_TMP.TupleSelect(0);
                }
                else
                {
                    hv_UpperLimit = 255.0;
                }
                //
                //Calculate scaling parameters.
                hv_Mult = (((hv_UpperLimit - hv_LowerLimit)).TupleReal()) / (hv_Max_COPY_INP_TMP - hv_Min_COPY_INP_TMP);
                hv_Add = ((-hv_Mult) * hv_Min_COPY_INP_TMP) + hv_LowerLimit;
                //
                //Scale image.
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ScaleImage(ho_Image_COPY_INP_TMP, out ExpTmpOutVar_0, hv_Mult,
                        hv_Add);
                    ho_Image_COPY_INP_TMP.Dispose();
                    ho_Image_COPY_INP_TMP = ExpTmpOutVar_0;
                }
                //
                //Clip gray values if necessary.
                //This must be done for each image and channel separately.
                ho_ImageScaled.Dispose();
                HOperatorSet.GenEmptyObj(out ho_ImageScaled);
                HOperatorSet.CountObj(ho_Image_COPY_INP_TMP, out hv_NumImages);
                HTuple end_val49 = hv_NumImages;
                HTuple step_val49 = 1;
                for (hv_ImageIndex = 1; hv_ImageIndex.Continue(end_val49, step_val49); hv_ImageIndex = hv_ImageIndex.TupleAdd(step_val49))
                {
                    ho_ImageSelected.Dispose();
                    HOperatorSet.SelectObj(ho_Image_COPY_INP_TMP, out ho_ImageSelected, hv_ImageIndex);
                    HOperatorSet.CountChannels(ho_ImageSelected, out hv_Channels);
                    HTuple end_val52 = hv_Channels;
                    HTuple step_val52 = 1;
                    for (hv_ChannelIndex = 1; hv_ChannelIndex.Continue(end_val52, step_val52); hv_ChannelIndex = hv_ChannelIndex.TupleAdd(step_val52))
                    {
                        ho_SelectedChannel.Dispose();
                        HOperatorSet.AccessChannel(ho_ImageSelected, out ho_SelectedChannel, hv_ChannelIndex);
                        HOperatorSet.MinMaxGray(ho_SelectedChannel, ho_SelectedChannel, 0, out hv_MinGray,
                            out hv_MaxGray, out hv_Range);
                        ho_LowerRegion.Dispose();
                        HOperatorSet.Threshold(ho_SelectedChannel, out ho_LowerRegion, ((hv_MinGray.TupleConcat(
                            hv_LowerLimit))).TupleMin(), hv_LowerLimit);
                        ho_UpperRegion.Dispose();
                        HOperatorSet.Threshold(ho_SelectedChannel, out ho_UpperRegion, hv_UpperLimit,
                            ((hv_UpperLimit.TupleConcat(hv_MaxGray))).TupleMax());
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.PaintRegion(ho_LowerRegion, ho_SelectedChannel, out ExpTmpOutVar_0,
                                hv_LowerLimit, "fill");
                            ho_SelectedChannel.Dispose();
                            ho_SelectedChannel = ExpTmpOutVar_0;
                        }
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.PaintRegion(ho_UpperRegion, ho_SelectedChannel, out ExpTmpOutVar_0,
                                hv_UpperLimit, "fill");
                            ho_SelectedChannel.Dispose();
                            ho_SelectedChannel = ExpTmpOutVar_0;
                        }
                        if ((int)(new HTuple(hv_ChannelIndex.TupleEqual(1))) != 0)
                        {
                            ho_ImageSelectedScaled.Dispose();
                            HOperatorSet.CopyObj(ho_SelectedChannel, out ho_ImageSelectedScaled,
                                1, 1);
                        }
                        else
                        {
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.AppendChannel(ho_ImageSelectedScaled, ho_SelectedChannel,
                                    out ExpTmpOutVar_0);
                                ho_ImageSelectedScaled.Dispose();
                                ho_ImageSelectedScaled = ExpTmpOutVar_0;
                            }
                        }
                    }
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_ImageScaled, ho_ImageSelectedScaled, out ExpTmpOutVar_0
                            );
                        ho_ImageScaled.Dispose();
                        ho_ImageScaled = ExpTmpOutVar_0;
                    }
                }
                ho_Image_COPY_INP_TMP.Dispose();
                ho_ImageSelected.Dispose();
                ho_SelectedChannel.Dispose();
                ho_LowerRegion.Dispose();
                ho_UpperRegion.Dispose();
                ho_ImageSelectedScaled.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image_COPY_INP_TMP.Dispose();
                ho_ImageSelected.Dispose();
                ho_SelectedChannel.Dispose();
                ho_LowerRegion.Dispose();
                ho_UpperRegion.Dispose();
                ho_ImageSelectedScaled.Dispose();

                throw HDevExpDefaultException;
            }
        }
        public static void findline(HObject ho_Image, out HObject ho_Line, HTuple hv_Rectangle2Row, HTuple hv_Rectangle2Column,
           HTuple hv_Rectangle2Phi, HTuple hv_Rectangle2Length1, HTuple hv_Rectangle2Length2, HTuple hv_Interpolation,
           HTuple hv_Sigma, HTuple hv_Threshold, HTuple hv_Transition, HTuple hv_Select)
        {
            // Local iconic variables 

            HObject ho_Cross3 = null, ho_Contour;

            // Local control variables 

            HTuple hv_Width = null, hv_Height = null, hv_Cos = null;
            HTuple hv_Sin = null, hv_row1 = null, hv_column1 = null;
            HTuple hv_row2 = null, hv_column2 = null, hv_pointsX = null;
            HTuple hv_pointsY = null, hv_Function = new HTuple(), hv_num = new HTuple();
            HTuple hv_Index = new HTuple(), hv_x = new HTuple(), hv_y = new HTuple();
            HTuple hv_MeasureHandle = new HTuple(), hv_RowEdge = new HTuple();
            HTuple hv_ColumnEdge = new HTuple(), hv_Amplitude = new HTuple();
            HTuple hv_Distance = new HTuple(), hv_RowBegin = null;
            HTuple hv_ColBegin = null, hv_RowEnd = null, hv_ColEnd = null;
            HTuple hv_Nr = null, hv_Nc = null, hv_Dist = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Line);
            HOperatorSet.GenEmptyObj(out ho_Cross3);
            HOperatorSet.GenEmptyObj(out ho_Contour);
            try
            {
                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);

                HOperatorSet.TupleCos(hv_Rectangle2Phi, out hv_Cos);
                HOperatorSet.TupleSin(hv_Rectangle2Phi, out hv_Sin);
                hv_row1 = hv_Rectangle2Row - (hv_Rectangle2Length2 * hv_Cos);
                hv_column1 = hv_Rectangle2Column - (hv_Rectangle2Length2 * hv_Sin);
                hv_row2 = hv_Rectangle2Row + (hv_Rectangle2Length2 * hv_Cos);
                hv_column2 = hv_Rectangle2Column + (hv_Rectangle2Length2 * hv_Sin);
                hv_pointsX = new HTuple();
                hv_pointsY = new HTuple();
                if ((int)((new HTuple(((hv_Rectangle2Phi.TupleAbs())).TupleGreater(0.785))).TupleAnd(
                    new HTuple(((hv_Rectangle2Phi.TupleAbs())).TupleLess(2.345)))) != 0)
                {
                    if ((int)(new HTuple(hv_column1.TupleLess(hv_column2))) != 0)
                    {

                        HOperatorSet.CreateFunct1dPairs(hv_column1.TupleConcat(hv_column2), hv_row1.TupleConcat(
                            hv_row2), out hv_Function);
                    }
                    else
                    {
                        HOperatorSet.CreateFunct1dPairs(hv_column2.TupleConcat(hv_column1), hv_row2.TupleConcat(
                            hv_row1), out hv_Function);
                    }
                    hv_num = ((hv_Rectangle2Length2 / 3)).TupleInt();
                    HTuple end_val18 = hv_num;
                    HTuple step_val18 = 1;
                    for (hv_Index = 1; hv_Index.Continue(end_val18, step_val18); hv_Index = hv_Index.TupleAdd(step_val18))
                    {
                        if ((int)(new HTuple(hv_Index.TupleEqual(1))) != 0)
                        {
                            hv_x = (hv_column1.TupleMin2(hv_column2)) + (((((hv_column1 - hv_column2)).TupleAbs()
                                ) / hv_num) / 2);
                        }
                        else
                        {
                            hv_x = (hv_column1.TupleMin2(hv_column2)) + (((((hv_column1 - hv_column2)).TupleAbs()
                                ) / hv_num) * (hv_Index - 0.5));
                        }
                        HOperatorSet.GetYValueFunct1d(hv_Function, hv_x, "constant", out hv_y);

                        //gen_cross_contour_xld (Cross2, x, y, 6, Phi)


                        //gen_rectangle2 (Rectangle1, y, x, Phi, Length1, abs(row1-row2)/num/2)
                        HOperatorSet.GenMeasureRectangle2(hv_y, hv_x, hv_Rectangle2Phi, hv_Rectangle2Length1,
                            ((((hv_column1 - hv_column2)).TupleAbs()) / hv_num) / 2, hv_Width, hv_Height,
                            hv_Interpolation, out hv_MeasureHandle);
                        HOperatorSet.MeasurePos(ho_Image, hv_MeasureHandle, hv_Sigma, hv_Threshold,
                            hv_Transition, hv_Select, out hv_RowEdge, out hv_ColumnEdge, out hv_Amplitude,
                            out hv_Distance);
                        ho_Cross3.Dispose();
                        HOperatorSet.GenCrossContourXld(out ho_Cross3, hv_RowEdge, hv_ColumnEdge,
                            10, hv_Rectangle2Phi);
                        HOperatorSet.CloseMeasure(hv_MeasureHandle);
                        hv_pointsX = hv_pointsX.TupleConcat(hv_RowEdge);
                        hv_pointsY = hv_pointsY.TupleConcat(hv_ColumnEdge);
                    }


                }
                else
                {
                    if ((int)(new HTuple(hv_row1.TupleLess(hv_row2))) != 0)
                    {

                        HOperatorSet.CreateFunct1dPairs(hv_row1.TupleConcat(hv_row2), hv_column1.TupleConcat(
                            hv_column2), out hv_Function);
                    }
                    else
                    {
                        HOperatorSet.CreateFunct1dPairs(hv_row2.TupleConcat(hv_row1), hv_column2.TupleConcat(
                            hv_column1), out hv_Function);
                    }

                    hv_num = ((hv_Rectangle2Length2 / 3)).TupleInt();

                    HTuple end_val49 = hv_num;
                    HTuple step_val49 = 1;
                    for (hv_Index = 1; hv_Index.Continue(end_val49, step_val49); hv_Index = hv_Index.TupleAdd(step_val49))
                    {
                        if ((int)(new HTuple(hv_Index.TupleEqual(1))) != 0)
                        {
                            hv_x = (hv_row1.TupleMin2(hv_row2)) + ((((((hv_row1 - hv_row2)).TupleAbs()
                                ) / hv_num) / 2) * hv_Index);
                        }
                        else
                        {
                            hv_x = (hv_row1.TupleMin2(hv_row2)) + (((((hv_row1 - hv_row2)).TupleAbs()
                                ) / hv_num) * (hv_Index - 0.5));
                        }
                        HOperatorSet.GetYValueFunct1d(hv_Function, hv_x, "constant", out hv_y);

                        //gen_cross_contour_xld (Cross2, x, y, 6, Phi)


                        //gen_rectangle2 (Rectangle1, x, y, Phi, Length1, abs(row1-row2)/num/2)
                        HOperatorSet.GenMeasureRectangle2(hv_x, hv_y, hv_Rectangle2Phi, hv_Rectangle2Length1,
                            ((((hv_row1 - hv_row2)).TupleAbs()) / hv_num) / 2, hv_Width, hv_Height, hv_Interpolation,
                            out hv_MeasureHandle);
                        HOperatorSet.MeasurePos(ho_Image, hv_MeasureHandle, hv_Sigma, hv_Threshold,
                            hv_Transition, hv_Select, out hv_RowEdge, out hv_ColumnEdge, out hv_Amplitude,
                            out hv_Distance);
                        ho_Cross3.Dispose();
                        HOperatorSet.GenCrossContourXld(out ho_Cross3, hv_RowEdge, hv_ColumnEdge,
                            10, hv_Rectangle2Phi);
                        HOperatorSet.CloseMeasure(hv_MeasureHandle);
                        hv_pointsX = hv_pointsX.TupleConcat(hv_RowEdge);
                        hv_pointsY = hv_pointsY.TupleConcat(hv_ColumnEdge);
                    }
                }

                ho_Contour.Dispose();
                if (hv_pointsX.Length < 2)
                    throw new Exception("not get enough point in findline operater");
                HOperatorSet.GenContourPolygonXld(out ho_Contour, hv_pointsX, hv_pointsY);
                HOperatorSet.FitLineContourXld(ho_Contour, "tukey", -1, 0, 5, 2, out hv_RowBegin,
                    out hv_ColBegin, out hv_RowEnd, out hv_ColEnd, out hv_Nr, out hv_Nc, out hv_Dist);
                ho_Line.Dispose();
                HOperatorSet.GenRegionLine(out ho_Line, hv_RowBegin, hv_ColBegin, hv_RowEnd,
                    hv_ColEnd);

                ho_Cross3.Dispose();
                ho_Contour.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Cross3.Dispose();
                ho_Contour.Dispose();

                throw HDevExpDefaultException;
            }
        }



       public  static void HObject2Bpp8(HObject image, out Bitmap res)
        {
            HTuple hpoint, type, width, height;
            const int Alpha = 255;
            IntPtr[] ptr = new IntPtr[2];
            HOperatorSet.GetImagePointer1(image, out hpoint, out type, out width, out height);
            res = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            ColorPalette pal = res.Palette;
            for (int i = 0; i <= 255; i++)
            {
                pal.Entries[i] = Color.FromArgb(Alpha, i, i, i);
            }



            res.Palette = pal;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = res.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            int PixelSize = Bitmap.GetPixelFormatSize(bmpData.PixelFormat) / 8;
            ptr[0] = bmpData.Scan0;
            ptr[1] = (IntPtr)hpoint.L;
            if (width % 4 == 0)
                CopyMemory(ptr[0], ptr[1], width * height * PixelSize);
            else
            {
                for (int i = 0; i < height - 1; i++)
                {
                    ptr[1] += width.I;

                    CopyMemory(ptr[0], ptr[1], width * PixelSize);
                    ptr[0] += bmpData.Stride;
                }
            }
            res.UnlockBits(bmpData);
        }

      public   static void Bitmap2HObjectBpp32(Bitmap bmp, out HObject image)
        {
            try
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

                BitmapData srcBmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                HOperatorSet.GenImageInterleaved(out image, srcBmpData.Scan0, "rgbx", bmp.Width, bmp.Height, 0, "byte", 0, 0, 0, 0, -1, 0);
                bmp.UnlockBits(srcBmpData);
            }
            catch (Exception ex)
            {
                throw new Exception($"unable to convert bitmap to hobject, {ex.Message}");
            }
        }
    }
}
