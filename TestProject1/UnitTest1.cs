using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using color_sort_for_Tecan;

namespace TestProject1
{
    /// <summary>
    /// UnitTest1 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性:
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestRandomDataLength()
        {
            Random rd = new Random();
            Int32 width = rd.Next();
            Int32 height = rd.Next();

            width %= 2160;
            height %= 3840;
            ColorGridData cgd = new ColorGridData(width, height);

            cgd.generate_random_color_grid();
            Assert.AreEqual(width * height * 4, cgd.random_data.Length);
        }

        [TestMethod]
        public void TestRandomDataValue()
        {
            Random rd = new Random();
            Int32 width = rd.Next();
            Int32 height = rd.Next();

            width %= 2160;
            height %= 3840;
            ColorGridData cgd1 = new ColorGridData(width, height);
            ColorGridData cgd2 = new ColorGridData(width, height);

            cgd1.generate_random_color_grid();
            cgd2.generate_random_color_grid();
            Assert.AreNotEqual(cgd1.random_data, cgd2.random_data);
        }

        private float TestCalculateHUE(Byte b, Byte g, Byte r)
        {
            Byte M = Math.Max(r, g);
            Byte m = Math.Min(r, g);
            Byte C = 0;
            float h = 0, hue = 0;

            M = Math.Max(M, b);
            m = Math.Min(m, b);

            C = (Byte)(M - m);

            if (0 == C)
            {
                hue = 0;
                return hue;
            }

            if (M == r)
            {
                h = ((float)(g - b) / (float)C) % 6;
            }
            else if (M == g)
            {
                h = ((float)(b - r) / (float)C) + 2;
            }
            else if (M == b)
            {
                h = ((float)(r - g) / (float)C) + 4;
            }

            hue = 60 * h;

            if (hue < 0)
            {
                hue += 360;
            }

            if (360 == hue)
            {
                hue = 0;
            }

            return hue;
        }

        [TestMethod]
        public void TestHUECalculating()
        {
            Random rd = new Random();
            Byte[] bgr = new Byte[3];
            float expected = 0, actual = 0;


            rd.NextBytes(bgr);
            expected = this.TestCalculateHUE(bgr[0], bgr[1], bgr[2]);
            actual = ColorGridData.calculate_hue(bgr[0], bgr[1], bgr[2]);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void TestHUESorting()
        {
            Int32 width = 600;
            Int32 height = 600;
            Byte[] bgr = new Byte[3], sorted_data;
            Stopwatch stopwatch = new Stopwatch();
            long max_time_elapse = 2 * 1000;
            Int32 x = 0, y = 0, pixel_index = 0;
            float previous_hue = 0, this_hue = 0;
            ColorGridData.color_align align = ColorGridData.color_align.left;
            

            ColorGridData cgd = new ColorGridData(width, height);

            cgd.generate_random_color_grid();

            stopwatch.Start();
            cgd.color_sort_by_hue(align, 360);
            stopwatch.Stop();

            if (max_time_elapse < stopwatch.ElapsedMilliseconds)
            {
                Assert.Fail("Spent " + stopwatch.ElapsedMilliseconds + " ms on sorting," + " beyond max [" + max_time_elapse + "] ms");
            }

            sorted_data = cgd.sorted_data;

            Assert.AreEqual(cgd.random_data.Length, cgd.sorted_data.Length);

            switch (align)
            {
                case ColorGridData.color_align.top:
                case ColorGridData.color_align.left:
                    x = 0;
                    y = 0;
                    break;
                case ColorGridData.color_align.right:
                    x = width - 1;
                    y = 0;
                    break;
                case ColorGridData.color_align.buttom:
                    x = 0;
                    y = height - 1;
                    break;
                default:
                    return;
            }

            while((x >= 0) && (x < width) && (y >= 0) && (y < height))
            {
                pixel_index = y * width + x;
                this_hue = this.TestCalculateHUE(sorted_data[pixel_index * 4], sorted_data[pixel_index * 4 + 1], sorted_data[pixel_index * 4 + 2]);
                if (this_hue < previous_hue)
                {
                    Assert.Fail("wrong sorting by HUE at point [" + x + "," + y + "]");
                }
                previous_hue = this_hue;
                switch (align)
                {
                    case ColorGridData.color_align.left:
                        y++;
                        if (y == height)
                        {
                            x++;
                            y = 0;
                        }
                        break;
                    case ColorGridData.color_align.top:
                        x++;
                        if (x == width)
                        {
                            y++;
                            x = 0;
                        }
                        break;
                    case ColorGridData.color_align.right:
                        y++;
                        if (y == height)
                        {
                            x--;
                            y = 0;
                        }
                        break;
                    case ColorGridData.color_align.buttom:
                        x++;
                        if (x == width)
                        {
                            y--;
                            x = 0;
                        }
                        break;
                    default:
                        return;
                }
            }
        }
    }
}
