#if WIN32

using System;
using System.Runtime.InteropServices;
using static PixelEngineDotNet.Win32;
using System.Collections.Generic;

namespace PixelEngineDotNet
{
    public partial class GameWindow
    {
        // This static field exists so that the delegat pointing to StaticWindowProc won't be GC'd.
        private static WndProc s_windowProc = StaticWindowProc;
        private static ushort s_windowClass;
        private static readonly Dictionary<IntPtr, GameWindow> s_windows = new Dictionary<IntPtr, GameWindow>();

        private IntPtr _hWnd;
        private int _borderWidth;
        private int _borderHeight;

        private void PlatformInitialize(Size size)
        {
            IntPtr hInstance = GetModuleHandle(null);

            if (s_windowClass == 0)
            {
                WNDCLASSEX wc = new WNDCLASSEX
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
                    style = 0,
                    lpfnWndProc = s_windowProc,
                    cbClsExtra = 0,
                    cbWndExtra = 0,
                    hInstance = hInstance,
                    hIcon = LoadIcon(IntPtr.Zero, IDI_APPLICATION),
                    hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW),
                    hbrBackground = (IntPtr)(COLOR_WINDOW + 1),
                    lpszMenuName = null,
                    lpszClassName = typeof(GameWindow).FullName,
                    hIconSm = LoadIcon(IntPtr.Zero, IDI_APPLICATION)
                };

                s_windowClass = RegisterClassEx(ref wc);

                if (s_windowClass == 0)
                    throw new InvalidOperationException($"RegisterClassEx failed: {GetLastErrorString()}");
            }

            _hWnd = CreateWindowEx(
                WS_EX_APPWINDOW | WS_EX_WINDOWEDGE,
                typeof(GameWindow).FullName,
                string.Empty,
                WS_MINIMIZEBOX | WS_SYSMENU | WS_OVERLAPPED | WS_CAPTION,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                size.Width,
                size.Height,
                IntPtr.Zero,
                IntPtr.Zero,
                hInstance,
                IntPtr.Zero);

            if (_hWnd == IntPtr.Zero)
                throw new InvalidOperationException($"CreateWindowEx failed: {GetLastErrorString()}");

            s_windows.Add(_hWnd, this);

            GetClientRect(_hWnd, out var clientRect);

            _borderWidth = size.Width - clientRect.right;
            _borderHeight = size.Height - clientRect.bottom;

            SetWindowPos(_hWnd, IntPtr.Zero, 0, 0, size.Width + _borderWidth, size.Height + _borderHeight, SWP_NOMOVE | SWP_NOZORDER);
        }

        private static IntPtr StaticWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (s_windows.TryGetValue(hWnd, out var window))
                return window.WindowProc(msg, wParam, lParam);

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private IntPtr WindowProc(uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_MOVE:
                    TriggerPositionChanged(LOWORD(lParam), HIWORD(lParam));
                    break;

                case WM_SIZE:
                    TriggerSizeChanged(LOWORD(lParam), HIWORD(lParam));
                    break;

                case WM_CLOSE:
                    if (TriggerClose())
                    {
                        DestroyWindow(_hWnd);
                    }
                    break;
            }

            return DefWindowProc(_hWnd, msg, wParam, lParam);
        }

        private void PlatformDispose(bool disposing)
        {
            s_windows.Remove(_hWnd);

            if (disposing)
            {
                if (_hWnd != IntPtr.Zero)
                {
                    DestroyWindow(_hWnd);
                }
            }
        }

        private IntPtr PlatformGetHandle()
        {
            return _hWnd;
        }

        private void PlatformSetTitle(string title)
        {
            SetWindowText(_hWnd, title);
        }

        private void PlatformSetPosition(int x, int y)
        {
            SetWindowPos(_hWnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        private void PlatformSetSize(int width, int height)
        {
            SetWindowPos(_hWnd, IntPtr.Zero, 0, 0, width, height, SWP_NOMOVE | SWP_NOZORDER);
        }

        private void PlatformShow()
        {
            ShowWindow(_hWnd, SW_SHOWNORMAL);
            UpdateWindow(_hWnd);
        }

        private void PlatformHide()
        {
            ShowWindow(_hWnd, SW_HIDE);
        }

        private void PlatformPollEvents()
        {
            while (PeekMessage(out var message, IntPtr.Zero, 0, 0, PM_REMOVE))
            {
                TranslateMessage(ref message);
                DispatchMessage(ref message);
            }
        }
    }
}

#endif
