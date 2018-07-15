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
            Int32 x = rd.Next();
            Int32 y = rd.Next();

            x %= 2160;
            y %= 3840;
            ColorGridData cgd = new ColorGridData(x, y);

            cgd.generate_random_color_grid();
            Assert.AreEqual( x * y * 4, cgd.random_data.Length);
        }

        [TestMethod]
        public void TestRandomDataValue()
        {
            Random rd = new Random();
            Int32 x = rd.Next();
            Int32 y = rd.Next();

            x %= 2160;
            y %= 3840;
            ColorGridData cgd1 = new ColorGridData(x, y);
            ColorGridData cgd2 = new ColorGridData(x, y);

            cgd1.generate_random_color_grid();
            cgd2.generate_random_color_grid();
            Assert.AreNotEqual(cgd1.random_data, cgd2.random_data);
        }

        private float calculate_hue(Byte b, Byte g, Byte r)
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
            Int32 x = rd.Next();
            Int32 y = rd.Next();
            Byte[] bgr = new Byte[3];
            float expected = 0, actual = 0;

            x %= 2160;
            y %= 3840;
            ColorGridData cgd = new ColorGridData(x, y);

            rd.NextBytes(bgr);
            expected = this.calculate_hue(bgr[0], bgr[1], bgr[2]);
            actual = cgd.calculate_hue(bgr[0], bgr[1], bgr[2]);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void TestHUESorting()
        {
            Int32 x = 400;
            Int32 y = 400;
            Byte[] bgr = new Byte[3];
            Stopwatch stopwatch = new Stopwatch();
            long max_time_elapse = 2 * 1000;

            ColorGridData cgd = new ColorGridData(x, y);

            cgd.generate_random_color_grid();

            stopwatch.Start();
            cgd.color_sort_by_hue(ColorGridData.color_align.left, 360);
            stopwatch.Stop();

            if (max_time_elapse < stopwatch.ElapsedMilliseconds)
            {
                Assert.Fail("Spent " + stopwatch.ElapsedMilliseconds + " ms on sorting," + " beyond max [" + max_time_elapse + "] ms");
            }


        }
    }
}
