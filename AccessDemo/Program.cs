using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace AccessDemo
{
    //此class範圍內階允許指標操作
    unsafe class Program
    {

        static uint* ScreenBuf1x, ScreenBufDst;



        static int srcH = 1440;//default sample height
        static int srcW = 1536;//default sample width

        static void Main(string[] args)
        {
            //init test param
            int counts = 500;
            int fps;
            Stopwatch st = new Stopwatch();

            //load image
            Bitmap bp = new Bitmap(Application.StartupPath + "/ok.png");
            srcW = bp.Width;
            srcH = bp.Height;

            //allocate memory space
            ScreenBuf1x = (uint*)Marshal.AllocHGlobal(sizeof(uint) * srcW * srcH);
            ScreenBufDst = (uint*)Marshal.AllocHGlobal(sizeof(uint) * srcW * srcH);

            //init data
            BitmapData srcData = bp.LockBits(new Rectangle(Point.Empty, bp.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            for (int y = 0; y < srcH; y++)
                for (int x = 0; x < srcW; x++)
                    ScreenBuf1x[x + y * srcW] = ((uint*)srcData.Scan0.ToPointer())[x + y * srcW];
            bp.UnlockBits(srcData);

            //check running mode , 影響執行結果
            if (IntPtr.Size == 4) Console.WriteLine("x86 32bit mode");
            else if (IntPtr.Size == 8) Console.WriteLine("x64 64bit mode");

            //.net version
            Console.WriteLine("Version: {0}", Environment.Version.ToString());

            //check image data move speed
            Console.WriteLine("[測試資料搬移速度]\n");

            Console.WriteLine("Copy by bytes");
            st.Restart();
            for (int i = 0; i < counts; i++) CopyByByte(ScreenBuf1x, srcW, srcH, ScreenBufDst);
            st.Stop();
            new Bitmap(srcW, srcH, srcW * 4, PixelFormat.Format32bppRgb, (IntPtr)ScreenBufDst).Save(Application.StartupPath + "/r-1.png"); //for check result
            fps = (int)((double)counts / ((double)st.ElapsedMilliseconds / (double)1000));
            Console.WriteLine("cost : " + st.ElapsedMilliseconds);
            Console.WriteLine("fps : " + fps + "\n");

            Console.WriteLine("Copy by uint");
            st.Restart();
            for (int i = 0; i < counts; i++) CopyByUint(ScreenBuf1x, srcW, srcH, ScreenBufDst);
            st.Stop();
            new Bitmap(srcW, srcH, srcW * 4, PixelFormat.Format32bppRgb, (IntPtr)ScreenBufDst).Save(Application.StartupPath + "/r-2.png"); //for check result
            fps = (int)((double)counts / ((double)st.ElapsedMilliseconds / (double)1000));
            Console.WriteLine("cost : " + st.ElapsedMilliseconds);
            Console.WriteLine("fps : " + fps + "\n");

            
            Console.WriteLine("Copy by ulong"); // 需要被8byte整除
            st.Restart();
            for (int i = 0; i < counts; i++) CopyByUlong(ScreenBuf1x, srcW, srcH, ScreenBufDst);
            st.Stop();
            new Bitmap(srcW, srcH, srcW * 4, PixelFormat.Format32bppRgb, (IntPtr)ScreenBufDst).Save(Application.StartupPath + "/r-3.png"); //for check result
            fps = (int)((double)counts / ((double)st.ElapsedMilliseconds / (double)1000));
            Console.WriteLine("cost : " + st.ElapsedMilliseconds);
            Console.WriteLine("fps : " + fps + "\n");


            //check rgb process speed
            Console.WriteLine("[測試RGB各項存取處理搬移速度]\n");

            Console.WriteLine("deal by bytes");
            st.Restart();
            for (int i = 0; i < counts; i++) CopyByByte(ScreenBuf1x, srcW, srcH, ScreenBufDst);
            st.Stop();
            new Bitmap(srcW, srcH, srcW * 4, PixelFormat.Format32bppRgb, (IntPtr)ScreenBufDst).Save(Application.StartupPath + "/rr-1.png"); //for check result
            fps = (int)((double)counts / ((double)st.ElapsedMilliseconds / (double)1000));
            Console.WriteLine("cost : " + st.ElapsedMilliseconds);
            Console.WriteLine("fps : " + fps + "\n");


            Console.WriteLine("deal by uint way 1 (combine by bitwise)");
            st.Restart();
            for (int i = 0; i < counts; i++) DealByUint_1(ScreenBuf1x, srcW, srcH, ScreenBufDst);
            st.Stop();
            new Bitmap(srcW, srcH, srcW * 4, PixelFormat.Format32bppRgb, (IntPtr)ScreenBufDst).Save(Application.StartupPath + "/rr-2.png"); //for check result
            fps = (int)((double)counts / ((double)st.ElapsedMilliseconds / (double)1000));
            Console.WriteLine("cost : " + st.ElapsedMilliseconds);
            Console.WriteLine("fps : " + fps + "\n");

            Console.WriteLine("deal by uint way 2 (combine by byte loc)");
            st.Restart();
            for (int i = 0; i < counts; i++) DealByUint_2(ScreenBuf1x, srcW, srcH, ScreenBufDst);
            st.Stop();
            new Bitmap(srcW, srcH, srcW * 4, PixelFormat.Format32bppRgb, (IntPtr)ScreenBufDst).Save(Application.StartupPath + "/rr-3.png"); //for check result
            fps = (int)((double)counts / ((double)st.ElapsedMilliseconds / (double)1000));
            Console.WriteLine("cost : " + st.ElapsedMilliseconds);
            Console.WriteLine("fps : " + fps + "\n");


            Console.WriteLine("deal by ulong way 1 (combine by bitwise)");
            st.Restart();
            for (int i = 0; i < counts; i++) DealByUlong_1(ScreenBuf1x, srcW, srcH, ScreenBufDst);
            st.Stop();
            new Bitmap(srcW, srcH, srcW * 4, PixelFormat.Format32bppRgb, (IntPtr)ScreenBufDst).Save(Application.StartupPath + "/rr-4.png"); //for check result
            fps = (int)((double)counts / ((double)st.ElapsedMilliseconds / (double)1000));
            Console.WriteLine("cost : " + st.ElapsedMilliseconds);
            Console.WriteLine("fps : " + fps + "\n");

            Console.WriteLine("deal by ulong way 2 (combine by byte loc)");
            st.Restart();
            for (int i = 0; i < counts; i++) DealByUlong_2(ScreenBuf1x, srcW, srcH, ScreenBufDst);
            st.Stop();
            new Bitmap(srcW, srcH, srcW * 4, PixelFormat.Format32bppRgb, (IntPtr)ScreenBufDst).Save(Application.StartupPath + "/rr-5.png"); //for check result
            fps = (int)((double)counts / ((double)st.ElapsedMilliseconds / (double)1000));
            Console.WriteLine("cost : " + st.ElapsedMilliseconds);
            Console.WriteLine("fps : " + fps + "\n");



            Console.WriteLine("Enter to exit..");
            Console.ReadLine();


            Marshal.FreeHGlobal((IntPtr)ScreenBuf1x);
            Marshal.FreeHGlobal((IntPtr)ScreenBufDst);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CopyByByte(uint* _src, int srcW, int srcH, uint* _dst)
        {
            byte* src = (byte*)_src;
            byte* dst = (byte*)_dst;

            for (int srcY = 0; srcY < srcH; srcY++)
            {
                for (int srcX = 0; srcX < srcW; srcX++)
                {
                    int index = (srcX << 2) + ((srcY * srcW) << 2);
                    dst[index] = src[index];
                    dst[index | 1] = src[index | 1];
                    dst[index | 2] = src[index | 2];
                    dst[index | 3] = src[index | 3];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CopyByUint(uint* src, int srcW, int srcH, uint* dst)
        {
            for (int srcY = 0; srcY < srcH; srcY++)
            {
                for (int srcX = 0; srcX < srcW; srcX++)
                {
                    int index = srcX + srcY * srcW;
                    dst[index] = src[index];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CopyByUlong(uint* _src, int srcW, int srcH, uint* _dst)
        {
            ulong* src = (ulong*)_src;
            ulong* dst = (ulong*)_dst;

            for (int i = 0; i < (srcH * srcW) / 2; i++)
            {
                dst[i] = src[i];
            }
        }

        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void DealByByte(uint* _src, int srcW, int srcH, uint* _dst)
        {
            byte* src = (byte*)_src;
            byte* dst = (byte*)_dst;

            for (int srcY = 0; srcY < srcH; srcY++)
            {
                for (int srcX = 0; srcX < srcW; srcX++)
                {
                    int index = (srcX << 2) + ((srcY * srcW) << 2);
                    dst[index] = (byte)(src[index] >> 1);
                    dst[index | 1] = (byte)(src[index | 1] >> 1);
                    dst[index | 2] = (byte)(src[index | 2] >> 1);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void DealByUint_1(uint* src, int srcW, int srcH, uint* dst)
        {
            for (int srcY = 0; srcY < srcH; srcY++)
            {
                for (int srcX = 0; srcX < srcW; srcX++)
                {
                    int index = srcX + srcY * srcW;
                    uint color = src[index];
                    byte r = (byte)((color & 0xff0000) >> 16);
                    byte g = (byte)((color & 0xff00) >> 8);
                    byte b = (byte)(color & 0xff);
                    dst[index] = (uint)(((r >> 1) << 16) | ((g >> 1) << 8) | (b >> 1));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void DealByUint_2(uint* src, int srcW, int srcH, uint* dst)
        {
            for (int srcY = 0; srcY < srcH; srcY++)
            {
                for (int srcX = 0; srcX < srcW; srcX++)
                {
                    byte* t = (byte*)&src[srcX + srcY * srcW];
                    byte* _t = (byte*)&dst[srcX + srcY * srcW];
                    byte r = t[2];
                    byte g = t[1];
                    byte b = t[0];
                    _t[2] = (byte)(r >> 1);
                    _t[1] = (byte)(g >> 1);
                    _t[0] = (byte)(b >> 1);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void DealByUlong_1(uint* _src, int srcW, int srcH, uint* _dst)
        {
            ulong* src = (ulong*)_src;
            ulong* dst = (ulong*)_dst;

            for (int i = 0; i < (srcH * srcW) / 2; i++)
            {
                ulong color = src[i];

                ulong r1 = ((color & 0xff000000000000) >> 48);
                ulong g1 = ((color & 0xff0000000000) >> 40);
                ulong b1 = (color & 0xff00000000 >> 32);

                ulong r2 = ((color & 0xff0000) >> 16);
                ulong g2 = ((color & 0xff00) >> 8);
                ulong b2 = (color & 0xff);

                dst[i] = (((r1 >> 1) << 48) | ((g1 >> 1) << 40) | ((b1 >> 1) << 32) | ((r2 >> 1) << 16) | ((g2 >> 1) << 8) | (b2 >> 1));

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void DealByUlong_2(uint* _src, int srcW, int srcH, uint* _dst)
        {
            ulong* src = (ulong*)_src;
            ulong* dst = (ulong*)_dst;

            for (int i = 0; i < (srcH * srcW) / 2; i++)
            {
                byte* t = (byte*)&src[i];
                byte* _t = (byte*)&dst[i];

                byte r1 = t[6];
                byte g1 = t[5];
                byte b1 = t[4];

                byte r2 = t[2];
                byte g2 = t[1];
                byte b2 = t[0];

                _t[6] = (byte)(r1 >> 1);
                _t[5] = (byte)(g1 >> 1);
                _t[4] = (byte)(b1 >> 1);

                _t[2] = (byte)(r2 >> 1);
                _t[1] = (byte)(g2 >> 1);
                _t[0] = (byte)(b2 >> 1);
            }
        }
    }
}
