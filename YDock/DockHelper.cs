﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using YDock.Interface;
using YDock.View;

namespace YDock
{
    public class DockHelper
    {
        public static T GetTemplateChild<T>(FrameworkTemplate template, FrameworkElement templateParent, string name)
        {
            return (T)template.FindName(name, templateParent);
        }

        public static Rect CreateChildRectFromParent(FrameworkElement parent, FrameworkElement child)
        {
            var originP = parent.PointToScreenDPIWithoutFlowDirection(new Point());
            var childP = child.PointToScreenDPIWithoutFlowDirection(new Point());
            return new Rect(new Point(childP.X - originP.X, childP.Y - originP.Y), child.TransformActualSizeToAncestor());
        }

        public static Point GetMousePosition(FrameworkElement relativeTo)
        {
            return relativeTo.TransformToDeviceDPI(Win32Helper.GetMousePosition());
        }

        public static Point GetMousePositionRelativeTo(FrameworkElement relativeTo)
        {
            var mouseP = relativeTo.TransformToDeviceDPI(Win32Helper.GetMousePosition());
            var pToScreen = relativeTo.PointToScreenDPIWithoutFlowDirection(new Point());
            return new Point(mouseP.X - pToScreen.X, mouseP.Y - pToScreen.Y);
        }

        public static void ComputeSpliterLocation(Popup spliter, Point location, Size size)
        {
            if (location.X < 0)
            {
                spliter.HorizontalOffset = 0;
                spliter.Width = size.Width + location.X;
            }
            if (location.Y < 0)
            {
                spliter.VerticalOffset = 0;
                spliter.Height = size.Height + location.Y;
            }
            if (location.X + size.Width > SystemParameters.VirtualScreenWidth)
                spliter.Width = SystemParameters.VirtualScreenWidth - location.X;
            if (location.Y + size.Height > SystemParameters.VirtualScreenHeight)
                spliter.Height = SystemParameters.VirtualScreenHeight - location.Y;
        }

        public static void UpdateLocation(DropWindow wnd, double left, double top, double width, double height)
        {
            double hrectoffset = 0, vrectoffset = 0;
            if (wnd.Host is LayoutDocumentGroupControl)
            {
                var dcrt = wnd.Host as LayoutDocumentGroupControl;
                int index = dcrt.IndexOf();
                wnd.Width = width;
                wnd.Height = height;
                if (wnd.DropPanel.InnerRect.Width < wnd.MinWidth)
                {
                    hrectoffset = (wnd.MinWidth - wnd.DropPanel.InnerRect.Width) / 2;
                    if (index == 0)
                    {
                        wnd.Width += hrectoffset;
                        left -= hrectoffset;
                    }
                    if (index == dcrt.ParentChildrenCount - 1)
                        wnd.Width += hrectoffset;
                }
                if (wnd.DropPanel.InnerRect.Height < wnd.MinHeight)
                {
                    vrectoffset = (wnd.MinHeight - wnd.DropPanel.InnerRect.Height) / 2;
                    if (index == 0)
                    {
                        wnd.Height += vrectoffset;
                        top -= vrectoffset;
                    }
                    if (index == dcrt.ParentChildrenCount - 1)
                        wnd.Height += vrectoffset;
                }


                if (left < 0)
                {
                    wnd.HorizontalOffset = 0;
                    hrectoffset += left;
                }
                else if (left + wnd.Width > SystemParameters.VirtualScreenWidth)
                {
                    wnd.HorizontalOffset = SystemParameters.VirtualScreenWidth - wnd.Width;
                    hrectoffset += left + wnd.Width - SystemParameters.VirtualScreenWidth;
                }
                else wnd.HorizontalOffset = left;

                if (top < 0)
                {
                    wnd.VerticalOffset = 0;
                    vrectoffset += top;
                }
                else if (top + wnd.Height > SystemParameters.VirtualScreenHeight)
                {
                    wnd.VerticalOffset = SystemParameters.VirtualScreenHeight - wnd.Height;
                    vrectoffset += top + wnd.Height - SystemParameters.VirtualScreenHeight;
                }
                else wnd.VerticalOffset = top;

                wnd.DropPanel.OuterRect = new Rect(hrectoffset, vrectoffset, width, height);
            }
            else
            {
                wnd.Width = width;
                wnd.Height = height;
                if (wnd.MinWidth > width)
                {
                    hrectoffset = (wnd.MinWidth - width) / 2;
                    left -= hrectoffset;
                    width = wnd.MinWidth;
                }
                if (wnd.MinHeight > height)
                {
                    vrectoffset = (wnd.MinHeight - height) / 2;
                    top -= vrectoffset;
                    height = wnd.MinHeight;
                }


                if (left < 0)
                {
                    wnd.HorizontalOffset = 0;
                    hrectoffset += left;
                }
                else if (left + width > SystemParameters.VirtualScreenWidth)
                {
                    wnd.HorizontalOffset = SystemParameters.VirtualScreenWidth - width;
                    hrectoffset += left + width - SystemParameters.VirtualScreenWidth;
                }
                else wnd.HorizontalOffset = left;

                if (top < 0)
                {
                    wnd.VerticalOffset = 0;
                    vrectoffset += top;
                }
                else if (top + height > SystemParameters.VirtualScreenHeight)
                {
                    wnd.VerticalOffset = SystemParameters.VirtualScreenHeight - height;
                    vrectoffset += top + height - SystemParameters.VirtualScreenHeight;
                }
                else wnd.VerticalOffset = top;
                wnd.DropPanel.InnerRect = new Rect(hrectoffset, vrectoffset, wnd.Width, wnd.Height);
                wnd.DropPanel.OuterRect = new Rect(0, 0, wnd.Width, wnd.Height);
            }
        }

        public static double GetTextWidth(string text)
        {
            return new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(new FontFamily("微软 雅黑"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 12, Brushes.Black).Width;
        }
    }

    public static class Win32Helper
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindow", CharSet = CharSet.Unicode)]
        static public extern IntPtr CreateWindow(
            string lpszClassName, string lpszWindowName,
            int style, int x, int y, int width, int height,
            IntPtr hwndParent, IntPtr hMenu, IntPtr hInst,
            [MarshalAs(UnmanagedType.AsAny)] object pvParam);

        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        static public extern IntPtr CreateWindowEx(int dwExStyle,
                                                      string lpszClassName,
                                                      string lpszWindowName,
                                                      int style,
                                                      int x, int y,
                                                      int width, int height,
                                                      IntPtr hwndParent,
                                                      IntPtr hMenu,
                                                      IntPtr hInst,
                                                      [MarshalAs(UnmanagedType.AsAny)] object pvParam);
        public const int
              WS_CHILD = 0x40000000,
              WS_VISIBLE = 0x10000000,
              WS_VSCROLL = 0x00200000,
              WS_BORDER = 0x00800000,
              WS_CLIPSIBLINGS = 0x04000000,
              WS_CLIPCHILDREN = 0x02000000,
              WS_TABSTOP = 0x00010000,
              WS_GROUP = 0x00020000;


        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        [Flags()]
        public enum SetWindowPosFlags : uint
        {
            /// <summary>If the calling thread and the thread that owns the window are attached to different input queues,
            /// the system posts the request to the thread that owns the window. This prevents the calling thread from
            /// blocking its execution while other threads process the request.</summary>
            /// <remarks>SWP_ASYNCWINDOWPOS</remarks>
            SynchronousWindowPosition = 0x4000,
            /// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
            /// <remarks>SWP_DEFERERASE</remarks>
            DeferErase = 0x2000,
            /// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
            /// <remarks>SWP_DRAWFRAME</remarks>
            DrawFrame = 0x0020,
            /// <summary>Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to
            /// the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE
            /// is sent only when the window's size is being changed.</summary>
            /// <remarks>SWP_FRAMECHANGED</remarks>
            FrameChanged = 0x0020,
            /// <summary>Hides the window.</summary>
            /// <remarks>SWP_HIDEWINDOW</remarks>
            HideWindow = 0x0080,
            /// <summary>Does not activate the window. If this flag is not set, the window is activated and moved to the
            /// top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
            /// parameter).</summary>
            /// <remarks>SWP_NOACTIVATE</remarks>
            DoNotActivate = 0x0010,
            /// <summary>Discards the entire contents of the client area. If this flag is not specified, the valid
            /// contents of the client area are saved and copied back into the client area after the window is sized or
            /// repositioned.</summary>
            /// <remarks>SWP_NOCOPYBITS</remarks>
            DoNotCopyBits = 0x0100,
            /// <summary>Retains the current position (ignores X and Y parameters).</summary>
            /// <remarks>SWP_NOMOVE</remarks>
            IgnoreMove = 0x0002,
            /// <summary>Does not change the owner window's position in the Z order.</summary>
            /// <remarks>SWP_NOOWNERZORDER</remarks>
            DoNotChangeOwnerZOrder = 0x0200,
            /// <summary>Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to
            /// the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent
            /// window uncovered as a result of the window being moved. When this flag is set, the application must
            /// explicitly invalidate or redraw any parts of the window and parent window that need redrawing.</summary>
            /// <remarks>SWP_NOREDRAW</remarks>
            DoNotRedraw = 0x0008,
            /// <summary>Same as the SWP_NOOWNERZORDER flag.</summary>
            /// <remarks>SWP_NOREPOSITION</remarks>
            DoNotReposition = 0x0200,
            /// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
            /// <remarks>SWP_NOSENDCHANGING</remarks>
            DoNotSendChangingEvent = 0x0400,
            /// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
            /// <remarks>SWP_NOSIZE</remarks>
            IgnoreResize = 0x0001,
            /// <summary>Retains the current Z order (ignores the hWndInsertAfter parameter).</summary>
            /// <remarks>SWP_NOZORDER</remarks>
            IgnoreZOrder = 0x0004,
            /// <summary>Displays the window.</summary>
            /// <remarks>SWP_SHOWWINDOW</remarks>
            ShowWindow = 0x0040,
        }

        /// <summary>
        ///     Special window handles
        /// </summary>
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        [StructLayout(LayoutKind.Sequential)]
        public class WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        };

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool IsChild(IntPtr hWndParent, IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        public const int WM_WINDOWPOSCHANGED = 0x0047;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_NCMOUSEMOVE = 0xa0;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int WM_NCLBUTTONUP = 0xA2;
        public const int WM_NCLBUTTONDBLCLK = 0xA3;
        public const int WM_NCRBUTTONDOWN = 0xA4;
        public const int WM_NCRBUTTONUP = 0xA5;
        public const int WM_CAPTURECHANGED = 0x0215;
        public const int WM_EXITSIZEMOVE = 0x0232;
        public const int WM_ENTERSIZEMOVE = 0x0231;
        public const int WM_MOVE = 0x0003;
        public const int WM_MOVING = 0x0216;
        public const int WM_KILLFOCUS = 0x0008;
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_ACTIVATE = 0x0006;
        public const int WM_NCHITTEST = 0x0084;
        public const int WM_INITMENUPOPUP = 0x0117;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_PAINT = 0x000F;

        public const int WA_INACTIVE = 0x0000;

        public const int WM_SYSCOMMAND = 0x0112;
        // These are the wParam of WM_SYSCOMMAND
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_RESTORE = 0xF120;

        public const int
            WM_CREATE = 0x0001;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        public static extern bool DestroyWindow(IntPtr hwnd);

        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImportAttribute("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // Hook Types  
        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        public const int HCBT_SETFOCUS = 9;
        public const int HCBT_ACTIVATE = 5;

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        public delegate int HookProc(int code, IntPtr wParam,
           IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(HookType code,
            HookProc func,
            IntPtr hInstance,
            int threadID);
        [DllImport("user32.dll")]
        public static extern int UnhookWindowsHookEx(IntPtr hhook);
        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr hhook,
            int code, IntPtr wParam, IntPtr lParam);

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public RECT(int left_, int top_, int right_, int bottom_)
            {
                Left = left_; Top = top_; Right = right_; Bottom = bottom_;
            }

            public int Height
            {
                get { return Bottom - Top; }
            }

            public int Width
            {
                get { return Right - Left; }
            }
            public Size Size { get { return new Size(Width, Height); } }
            public Point Location { get { return new Point(Left, Top); } }
            // Handy method for converting to a System.Drawing.Rectangle  
            public Rect ToRectangle() { return new Rect(Left, Top, Right, Bottom); }
            public static RECT FromRectangle(Rect rectangle)
            { return new Rect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom); }
            public override int GetHashCode()
            { return Left ^ ((Top << 13) | (Top >> 0x13)) ^ ((Width << 0x1a) | (Width >> 6)) ^ ((Height << 7) | (Height >> 0x19)); }

            #region Operator overloads
            public static implicit operator Rect(RECT rect) { return rect.ToRectangle(); }
            public static implicit operator RECT(Rect rect) { return FromRectangle(rect); }
            #endregion
        }

        public static RECT GetClientRect(IntPtr hWnd)
        {
            RECT result = new RECT();
            GetClientRect(hWnd, out result);
            return result;
        }
        public static RECT GetWindowRect(IntPtr hWnd)
        {
            RECT result = new RECT();
            GetWindowRect(hWnd, out result);
            return result;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetTopWindow(IntPtr hWnd);

        public const uint GW_HWNDNEXT = 2;
        public const uint GW_HWNDPREV = 3;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        public static int MakeLParam(int LoWord, int HiWord)
        {

            //System.Diagnostics.Trace.WriteLine("LoWord: " + LoWord2(((HiWord << 16) |
            //(LoWord & 0xffff))));

            return (int)((HiWord << 16) | (LoWord & 0xffff));
        }


        public const int WM_MOUSEMOVE = 0x200;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_LBUTTONDBLCLK = 0x203;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_RBUTTONDBLCLK = 0x206;
        public const int WM_MBUTTONDOWN = 0x207;
        public const int WM_MBUTTONUP = 0x208;
        public const int WM_MBUTTONDBLCLK = 0x209;
        public const int WM_MOUSEWHEEL = 0x20A;
        public const int WM_MOUSEHWHEEL = 0x20E;


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        public struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowEnabled(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        /// <summary>
        /// Changes an attribute of the specified window. The function also sets the 32-bit (long) value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs..</param>
        /// <param name="nIndex">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of an integer. To set any other value, specify one of the following values: GWL_EXSTYLE, GWL_HINSTANCE, GWL_ID, GWL_STYLE, GWL_USERDATA, GWL_WNDPROC </param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>If the function succeeds, the return value is the previous value of the specified 32-bit integer.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError. </returns>
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public static void SetOwner(IntPtr childHandle, IntPtr ownerHandle)
        {
            SetWindowLong(
                childHandle,
                -8, // GWL_HWNDPARENT
                ownerHandle.ToInt32());
        }

        public static IntPtr GetOwner(IntPtr childHandle)
        {
            return new IntPtr(GetWindowLong(childHandle, -8));
        }


        //Monitor Patch #13440

        /// <summary>
        /// The MonitorFromRect function retrieves a handle to the display monitor that 
        /// has the largest area of intersection with a specified rectangle.
        /// </summary>
        /// <param name="lprc">Pointer to a RECT structure that specifies the rectangle of interest in 
        /// virtual-screen coordinates</param>
        /// <param name="dwFlags">Determines the function's return value if the rectangle does not intersect 
        /// any display monitor</param>
        /// <returns>
        /// If the rectangle intersects one or more display monitor rectangles, the return value 
        /// is an HMONITOR handle to the display monitor that has the largest area of intersection with the rectangle.
        /// If the rectangle does not intersect a display monitor, the return value depends on the value of dwFlags.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromRect([In] ref RECT lprc, uint dwFlags);

        /// <summary>
        /// The MonitorFromWindow function retrieves a handle to the display monitor that has the largest area of intersection with the bounding rectangle of a specified window. 
        /// </summary>
        /// <param name="hwnd">A handle to the window of interest.</param>
        /// <param name="dwFlags">Determines the function's return value if the window does not intersect any display monitor.</param>
        /// <returns>If the window intersects one or more display monitor rectangles, the return value is an HMONITOR handle to the display monitor that has the largest area of intersection with the window. 
        /// If the window does not intersect a display monitor, the return value depends on the value of dwFlags.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);


        /// <summary>
        /// The MONITORINFO structure contains information about a display monitor.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class MonitorInfo
        {
            /// <summary>
            /// The size of the structure, in bytes. 
            /// </summary>
            public int Size = Marshal.SizeOf(typeof(MonitorInfo));
            /// <summary>
            /// A RECT structure that specifies the display monitor rectangle, expressed 
            /// in virtual-screen coordinates. 
            /// Note that if the monitor is not the primary display monitor, 
            /// some of the rectangle's coordinates may be negative values. 
            /// </summary>
            public RECT Monitor;
            /// <summary>
            /// A RECT structure that specifies the work area rectangle of the display monitor, 
            /// expressed in virtual-screen coordinates. Note that if the monitor is not the primary 
            /// display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public RECT Work;
            /// <summary>
            /// A set of flags that represent attributes of the display monitor. 
            /// </summary>
            public uint Flags;
        }

        /// <summary>
        /// The GetMonitorInfo function retrieves information about a display monitor. 
        /// </summary>
        /// <param name="hMonitor">Handle to the display monitor of interest.</param>
        /// <param name="lpmi">Pointer to a MONITORINFO or MONITORINFOEX structure that receives 
        /// information about the specified display monitor</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, [In, Out] MonitorInfo lpmi);

    }

    public class ElementComparer<T> : IComparer<T>
    {
        private Func<T, T, int> _comparer;

        public ElementComparer(Func<T, T, int> comparer)
        {
            _comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            return _comparer(x, y);
        }
    }

    public static class ResourceManager
    {
        public static readonly SolidColorBrush SplitterBrushVertical;
        public static readonly SolidColorBrush SplitterBrushHorizontal;
        public static readonly SolidColorBrush DropRectBrush;
        public static readonly SolidColorBrush RectBrush;
        public static readonly SolidColorBrush WindowBorderBrush;
        public static readonly Pen ActiveDashPen;
        public static readonly Pen DisActiveDashPen;
        public static readonly Pen BlueDashPen;
        public static readonly Pen BorderPen;
        public static readonly Pen RectBorderPen;
        public static readonly Pen DropRectPen_Heavy;
        public static readonly Pen DropRectPen;

        static ResourceManager()
        {
            SplitterBrushVertical = new SolidColorBrush(new Color()
            {
                R = 0xEE,
                G = 0xEE,
                B = 0xF2,
                A = 0xFF
            });

            SplitterBrushHorizontal = new SolidColorBrush(new Color()
            {
                R = 0xCC,
                G = 0xCE,
                B = 0xDB,
                A = 0xFF
            });

            DropRectBrush = new SolidColorBrush(new Color()
            {
                R = 0x00,
                G = 0x7A,
                B = 0xCC,
                A = 0xFF
            });

            RectBrush = new SolidColorBrush(new Color()
            {
                R = 0x1D,
                G = 0x7A,
                B = 0xEE,
                A = 0xFF
            });

            WindowBorderBrush = new SolidColorBrush(new Color()
            {
                R = 0x9B,
                G = 0x9F,
                B = 0xB9,
                A = 0xFF
            });

            BorderPen = new Pen()
            {
                Brush = Brushes.Gray,
                Thickness = 1
            };

            RectBorderPen = new Pen()
            {
                Brush = Brushes.LightGray,
                Thickness = 6
            };

            DropRectPen_Heavy = new Pen()
            {
                Brush = DropRectBrush,
                Thickness = 3
            };

            DropRectPen = new Pen()
            {
                Brush = DropRectBrush,
                Thickness = 1
            };

            ActiveDashPen = new Pen()
            {
                Brush = Brushes.White,
                Thickness = 1,
                DashCap = PenLineCap.Flat,
                DashStyle = new DashStyle(new List<double>() { 1, 3 }, 0)
            };

            DisActiveDashPen = new Pen()
            {
                Brush = Brushes.Black,
                Thickness = 1,
                DashCap = PenLineCap.Flat,
                DashStyle = new DashStyle(new List<double>() { 1, 3 }, 0)
            };

            BlueDashPen = new Pen()
            {
                Brush = Brushes.Blue,
                Thickness = 1,
                DashCap = PenLineCap.Flat,
                DashStyle = new DashStyle(new List<double>() { 1, 3 }, 0)
            };
        }
    }
}