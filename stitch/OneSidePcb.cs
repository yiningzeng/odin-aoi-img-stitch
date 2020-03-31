using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace stitch
{
    class OneSidePcb
    {

        #region 拼图参数
        public int allRows; // 总行数
        public int allCols; // 总列数
        public int currentRow = 0; // 拼图当前行
        public int currentCol = 0; // 拼图当前列
        public bool zTrajectory; // 默认是Z轨迹，背面的话是S字形轨迹
        public int trajectorySide;
        public Mat dst = null; // 最终输出大图
        public Rectangle roi = new Rectangle(); // 对齐的参考的区域
        //图片队列
        public Queue<Bitmap> bitmaps = new Queue<Bitmap>();
        #endregion
    }
}
