using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace stitch
{
    public class AoiAi
    {
        public enum side { none = 0, left = 1, up = 2, right = 4, down = 8 };


        #region wuran
        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct FP0
        {
            public int feature;          // 特征类型编号
            public int ks;               // 卷积核尺寸，与图像尺寸成正比
            public float f_lb;           // 特征下限
            public float f_ub;           // 特征上限
            public int a_lb;             // 面积下限
            public int a_ub;             // 面积上限
            public uint n_boxes;     // 输出的bbox个数
        };

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct feature_bbox_t_container
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public Rectangle[] bboxlist;
        };

        [DllImport("aoi.dll", EntryPoint = "feature_filter_csharp", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int feature_filter_csharp(IntPtr iplImage, ref feature_bbox_t_container f, FP0 fP0);
        #endregion


        [DllImport("aoi.dll", EntryPoint = "marker_match", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern double marker_match(IntPtr iplImage, IntPtr patch, ref Point point, bool binarize = false, int method = 1);

        [DllImport("aoi.dll", EntryPoint = "hello", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int hello(IntPtr iplImage);

        [DllImport("aoi.dll", EntryPoint = "copy_to", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int copy_to(IntPtr iplImage, IntPtr patch, Rectangle rectangle);

        [DllImport("aoi.dll", EntryPoint = "stitch", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void stitch(IntPtr img, Rectangle rectangle, IntPtr patch, ref Rectangle roi_patch, int side1, int overlap_lb1, int overlap_ub1, int drift_ub1, int side2 = 0, int overlap_lb2 = 0, int overlap_ub2 = 0, int drift_ub2 = 0);

        [DllImport("aoi.dll", EntryPoint = "stitch_v2", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void stitchv2(IntPtr img, Rectangle rectangle, IntPtr patch, ref Rectangle roi_patch, int side1, int overlap_lb1, int overlap_ub1, int drift_ub1, int side2 = 0, int overlap_lb2 = 0, int overlap_ub2 = 0, int drift_ub2 = 0);


        [DllImport("power_aoi.dll", EntryPoint = "testIplImage", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int testIplImage(IntPtr iplImage);

        [DllImport("power_aoi.dll", EntryPoint = "testMatOut", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int testMatOut(IntPtr mat, IntPtr res);

        /// <summary>
        /// 抽色
        /// </summary>
        /// <param name="gray">灰度图</param>
        /// <param name="rgb">彩色图</param>
        /// <param name="mask">输出的掩码图</param>
        /// <param name="pars">参数数组，依次为灰度的阈值下限、上限，r 通道的下限、上限，g 下限、上限，b 下限、上限</param>
        /// <returns></returns>
        [DllImport("power_aoi.dll", EntryPoint = "range_mask", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void range_mask(IntPtr gray, IntPtr rgb, IntPtr mask, int par);

        /// <summary>
        /// 灰度图生成直方图
        /// </summary>
        /// <param name="gray">灰度图</param>
        /// <param name="n_bins">柱子的个数</param>
        /// <param name="hist">输出的直方图，n_bins x 1 的矩阵，类型为 CV_32F 的，hist.data 是数据指针，float32 型的</param>
        [DllImport("power_aoi.dll", EntryPoint = "histogram", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void histogram(IntPtr gray, int n_bins, IntPtr hist);

        /// <summary>
        /// 图像拼接
        /// </summary>
        /// <param name="img">画布</param>
        /// <param name="patch">要贴上画布的图片</param>
        /// <param name="x">patch图片左上角x在画布的x</param>
        /// <param name="y">patch图片左上角y在画布的y</param>
        /// <returns></returns>
        [DllImport("power_aoi.dll", EntryPoint = "add_patch", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int addPatch(IntPtr img, IntPtr patch, int x, int y);
    }
}
