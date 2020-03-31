using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace stitch
{
    public partial class Form1 : Form
    {
        string savePath = "";
        int allNum = 0;
        public Action done;
        Stopwatch sw = new Stopwatch();
        public Form1()
        {
            InitializeComponent();
            done = () =>
            {
                allNum++;
                if (allNum >= 2)
                {
                    MessageBox.Show("完成");
                }
            };

        }



        void stitch(List<string> fileList, string saveFileName)
        {
            int n_rows = 4;
            int n_cols = 15;

            double or_hl = 0.23; // lower bound for horizontal overlap ratio
            double or_hu = 0.24; // upper
            double or_vl = 0.065; // vertical
            double or_vu = 0.08;
            double dr_hu = 0.01; // upper bound for horizontal drift ratio
            double dr_vu = 0.01; //
            Mat dst = null;
            Rectangle roi0 = new Rectangle(); //上一行第一张的区域
                                              // first row 
            Rectangle roi = new Rectangle(); // 左对齐的参考的区域
                                             // first row 
            for (int col = 0; col < n_cols; ++col)
            {
                Mat img = new Mat(fileList[col], Emgu.CV.CvEnum.LoadImageType.AnyColor);

                if (col == 0)
                {
                    roi0 = new Rectangle(Convert.ToInt32(img.Cols * (n_cols - 1) * dr_hu), Convert.ToInt32(img.Rows * (n_rows - 1) * dr_vu), img.Cols, img.Rows);
                    dst = new Mat(Convert.ToInt32(img.Rows * (n_rows + (n_rows - 1) * (dr_vu * 2 - or_vl))) + 100, Convert.ToInt32(img.Cols * (n_cols + (n_cols - 1) * (dr_hu * 2 - or_hl))) + 100, img.Depth, 3); // 第一张图不要0,0 最好留一些像素
                    roi = roi0;
                }
                else
                {
                    AoiAi.stitchv2(dst.Ptr, roi, img.Ptr, ref roi, (int)AoiAi.side.left, Convert.ToInt32(img.Cols * or_hl), Convert.ToInt32(img.Cols * or_hu), Convert.ToInt32(img.Rows * dr_vu));
                }

                AoiAi.copy_to(dst.Ptr, img.Ptr, roi);
                //#region 这里去掉
                CvInvoke.Resize(dst, dst, new Size(Convert.ToInt32(dst.Width * 0.3), Convert.ToInt32(dst.Height * 0.3)));
                CvInvoke.NamedWindow("AJpg", NamedWindowType.Normal); //创建一个显示窗口
                CvInvoke.Imshow("AJpg", dst);


                char key = (char)CvInvoke.WaitKey(1);
                if (key == 0x1b || key == 'q') continue;
                //#endregion 这里去掉

            }

            // other rows
            for (int row = 1; row < n_rows; ++row)
            {
                for (int col = 0; col < n_cols; ++col)
                {
                    Mat img = new Mat(fileList[n_cols * row + col], Emgu.CV.CvEnum.LoadImageType.AnyColor);
                    //std::cout << n_cols * row + col << "\n";

                    if (col == 0)
                    {
                        AoiAi.stitchv2(dst.Ptr, roi0, img.Ptr, ref roi0, (int)AoiAi.side.up, Convert.ToInt32(img.Cols * or_vl), Convert.ToInt32(img.Cols * or_vu), Convert.ToInt32(img.Rows * dr_hu));
                        roi = roi0;
                    }
                    else
                    {
                        AoiAi.stitchv2(dst.Ptr, roi, img.Ptr, ref roi, (int)AoiAi.side.left, Convert.ToInt32(img.Cols * or_hl), Convert.ToInt32(img.Cols * or_hu), Convert.ToInt32(img.Rows * dr_vu), (int)AoiAi.side.up, Convert.ToInt32(img.Rows * or_vl), Convert.ToInt32(img.Rows * or_vu), Convert.ToInt32(img.Cols * dr_hu));
                    }
                    AoiAi.copy_to(dst.Ptr, img.Ptr, roi);
                    //#region 这里去掉
                    //CvInvoke.NamedWindow("AJpg", NamedWindowType.Normal); //创建一个显示窗口
                    //CvInvoke.Imshow("AJpg", dst);
                    //char key = (char)CvInvoke.WaitKey(1);
                    //if (key == 0x1b || key == 'q') continue;
                    //#endregion 这里去掉
                }
            }
            dst.Save(saveFileName);
        }

        void stitch(OneSidePcb oneSidePcb)
        {
            double or_hl = 0.23; // lower bound for horizontal overlap ratio
            double or_hu = 0.24; // upper
            double or_vl = 0.065; // vertical
            double or_vu = 0.08;
            double dr_hu = 0.01; // upper bound for horizontal drift ratio
            double dr_vu = 0.01; //

            Bitmap bitmap = oneSidePcb.bitmaps.Dequeue();
            Emgu.CV.Image<Bgr, Byte> currentFrame = new Emgu.CV.Image<Bgr, Byte>(bitmap);
            Mat img = new Mat();
            CvInvoke.BitwiseAnd(currentFrame, currentFrame, img);

            //第一行
            if (oneSidePcb.currentRow == 0)
            {
                if (oneSidePcb.currentCol == 0)
                {
                    #region 判断s型还是z字形
                    if (oneSidePcb.zTrajectory) //Z形
                    {
                        oneSidePcb.trajectorySide = (int)AoiAi.side.left;
                        int x = Convert.ToInt32(img.Cols * (oneSidePcb.allCols - 1) * dr_hu);
                        int y = Convert.ToInt32(img.Rows * (oneSidePcb.allRows - 1) * dr_vu);
                        int dstRows = Convert.ToInt32(img.Rows * (oneSidePcb.allRows + (oneSidePcb.allRows - 1) * (dr_vu * 2 - or_vl)));
                        int dstCols = Convert.ToInt32(img.Cols * (oneSidePcb.allCols + (oneSidePcb.allCols - 1) * (dr_hu * 2 - or_hl)));
                        oneSidePcb.roi = new Rectangle(x, y, img.Cols, img.Rows);
                        oneSidePcb.dst = new Mat(dstRows, dstCols, img.Depth, 3); // 第一张图不要0,0 最好留一些像素
                    }
                    else // S型
                    {
                        oneSidePcb.trajectorySide = (int)AoiAi.side.right;
                  
                        int dstRows = Convert.ToInt32(img.Rows * (oneSidePcb.allRows + (oneSidePcb.allRows - 1) * (dr_vu * 2 - or_vl)));
                        int dstCols = Convert.ToInt32(img.Cols * (oneSidePcb.allCols + (oneSidePcb.allCols - 1) * (dr_hu * 2 - or_hl)));
                        int x = Convert.ToInt32((dstCols-img.Cols) * (1 - dr_hu));
                        int y = Convert.ToInt32(img.Rows * (oneSidePcb.allRows - 1) * dr_vu);
                        oneSidePcb.roi = new Rectangle(x, y, img.Cols, img.Rows);
                        oneSidePcb.dst = new Mat(dstRows, dstCols, img.Depth, 3); // 第一张图不要0,0 最好留一些像素
                    }
                    #endregion
                }
                else
                {
                    AoiAi.stitchv2(oneSidePcb.dst.Ptr, oneSidePcb.roi, img.Ptr, ref oneSidePcb.roi, oneSidePcb.trajectorySide, Convert.ToInt32(img.Cols * or_hl), Convert.ToInt32(img.Cols * or_hu), Convert.ToInt32(img.Rows * dr_vu));
                }
                //oneSidePcb.dst.Save(@"C:\Users\Administrator\Desktop\suomi-test-img\" + oneSidePcb.currentRow + "-" + oneSidePcb.currentCol + ".jpg");
                AoiAi.copy_to(oneSidePcb.dst.Ptr, img.Ptr, oneSidePcb.roi);
                //oneSidePcb.dst.Save(@"C:\Users\Administrator\Desktop\suomi-test-img\" + oneSidePcb.currentRow + "-" + oneSidePcb.currentCol + ".jpg");
                img.Dispose();
                currentFrame.Dispose();
                bitmap.Dispose();
                oneSidePcb.currentCol++;
                if (oneSidePcb.currentCol >= oneSidePcb.allCols)
                {
                    oneSidePcb.currentCol = 0;
                    oneSidePcb.currentRow++;
                    if (oneSidePcb.trajectorySide == (int)AoiAi.side.left)
                    {
                        oneSidePcb.trajectorySide = (int)AoiAi.side.right;
                    }
                    else if (oneSidePcb.trajectorySide == (int)AoiAi.side.right)
                    {
                        oneSidePcb.trajectorySide = (int)AoiAi.side.left;
                    }
                }
                //oneSidePcb.dst.Save(@"C:\Users\Administrator\Desktop\suomi-test-img\row1.jpg");
            }
            else // 其他行
            {
                if (Convert.ToBoolean(oneSidePcb.currentRow % 2)) //偶行
                {
                    if (oneSidePcb.currentCol == 0)
                    {
                        AoiAi.stitchv2(oneSidePcb.dst.Ptr, oneSidePcb.roi, img.Ptr, ref oneSidePcb.roi, (int)AoiAi.side.up, Convert.ToInt32(img.Cols * or_vl), Convert.ToInt32(img.Cols * or_vu), Convert.ToInt32(img.Rows * dr_hu));
                        //oneSidePcb.roi = oneSidePcb.roi0;
                    }
                    else
                    {
                        AoiAi.stitchv2(oneSidePcb.dst.Ptr, oneSidePcb.roi, img.Ptr, ref oneSidePcb.roi, oneSidePcb.trajectorySide, Convert.ToInt32(img.Cols * or_hl), Convert.ToInt32(img.Cols * or_hu), Convert.ToInt32(img.Rows * dr_vu), (int)AoiAi.side.up, Convert.ToInt32(img.Rows * or_vl), Convert.ToInt32(img.Rows * or_vu), Convert.ToInt32(img.Cols * dr_hu));
                    }
                }
                else
                {
                    if (oneSidePcb.currentCol == 0)
                    {
                        AoiAi.stitchv2(oneSidePcb.dst.Ptr, oneSidePcb.roi, img.Ptr, ref oneSidePcb.roi, (int)AoiAi.side.up, Convert.ToInt32(img.Cols * or_vl), Convert.ToInt32(img.Cols * or_vu), Convert.ToInt32(img.Rows * dr_hu));
                        //oneSidePcb.roi = oneSidePcb.roi0;
                    }
                    else
                    {
                        AoiAi.stitchv2(oneSidePcb.dst.Ptr, oneSidePcb.roi, img.Ptr, ref oneSidePcb.roi, oneSidePcb.trajectorySide, Convert.ToInt32(img.Cols * or_hl), Convert.ToInt32(img.Cols * or_hu), Convert.ToInt32(img.Rows * dr_vu), (int)AoiAi.side.up, Convert.ToInt32(img.Rows * or_vl), Convert.ToInt32(img.Rows * or_vu), Convert.ToInt32(img.Cols * dr_hu));
                    }
                }
                //oneSidePcb.dst.Save(@"C:\Users\Administrator\Desktop\suomi-test-img\" + oneSidePcb.currentRow + "-" + oneSidePcb.currentCol + ".jpg");
                AoiAi.copy_to(oneSidePcb.dst.Ptr, img.Ptr, oneSidePcb.roi);
                img.Dispose();
                currentFrame.Dispose();
                bitmap.Dispose();
                oneSidePcb.currentCol++;
                if (oneSidePcb.currentRow >= oneSidePcb.allRows - 1 && oneSidePcb.currentCol >= oneSidePcb.allCols)
                {
                    sw.Stop();
                    TimeSpan ts = sw.Elapsed;
                    Console.WriteLine("DateTime costed for Shuffle function is: {0}ms", ts.TotalMilliseconds);
                    this.BeginInvoke(done);
                    if (oneSidePcb.zTrajectory)
                        oneSidePcb.dst.Save(Path.Combine(savePath, "Front.jpg"));
                    else
                        oneSidePcb.dst.Save(Path.Combine(savePath, "Back.jpg"));

                }
                else if (oneSidePcb.currentCol >= oneSidePcb.allCols)
                {
                    oneSidePcb.currentCol = 0;
                    oneSidePcb.currentRow++;
                    if (oneSidePcb.trajectorySide == (int)AoiAi.side.left) oneSidePcb.trajectorySide = (int)AoiAi.side.right;
                    else if (oneSidePcb.trajectorySide == (int)AoiAi.side.right) oneSidePcb.trajectorySide = (int)AoiAi.side.left;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Visible = false;
            savePath = tbSavePath.Text.Trim();
            if (savePath == "") {
                MessageBox.Show("保存路径");return;
            }
            OneSidePcb frontSidePcb = new OneSidePcb() { allRows = 4, allCols = 15, zTrajectory = true, trajectorySide = (int)AoiAi.side.left };
            OneSidePcb backSidePcb = new OneSidePcb() { allRows = 4, allCols = 15, zTrajectory = false, trajectorySide = (int)AoiAi.side.right };
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string directory = Path.GetDirectoryName(dialog.FileName);
           

                for (int i = 0; i <=59; i++)
                {
                    frontSidePcb.bitmaps.Enqueue(new Bitmap(Path.Combine(directory, "F" + i + ".jpg")));
                }
                for (int i = 0; i <= 59; i++)
                {
                    backSidePcb.bitmaps.Enqueue(new Bitmap(Path.Combine(directory, "B" + i + ".jpg")));
                }
                //for (int i = 29; i >= 15; i--)
                //{
                //    oneSidePcb.bitmaps.Enqueue(new Bitmap(Path.Combine(directory, "F" + i + ".jpg")));
                //}
                //for (int i = 30; i <= 44; i++)
                //{
                //    oneSidePcb.bitmaps.Enqueue(new Bitmap(Path.Combine(directory, "F" + i + ".jpg")));
                //}
                //for (int i = 59; i >= 45; i--)
                //{
                //    oneSidePcb.bitmaps.Enqueue(new Bitmap(Path.Combine(directory, "F" + i + ".jpg")));
                //}
                sw.Start();
                //耗时程序

                
                for (int i = 0; i < 60; i++)
                {
                    MySmartThreadPool.Instance().QueueWorkItem(() =>
                    {
                        lock (frontSidePcb)
                        {
                            stitch(frontSidePcb);
                        }
                    });
                    MySmartThreadPool.Instance().QueueWorkItem(() =>
                    {
                        lock (backSidePcb)
                        {
                            stitch(backSidePcb);
                        }
                    });
                }

                //copypic(Path.Combine(directory, "1Ftotal.jpg"));

                //fileList.Clear();
                //for (int i = 0; i < 60; i++)
                //{
                //    fileList.Add(Path.Combine(directory, "B" + i + ".jpg"));
                //}
                //copypic(fileList, Path.Combine(directory, "1Btotal.jpg"));
            }
        }
    }
}
