﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Win32Api;
using WpfScreenHelper;
using static Win32Api.User32;

namespace SlideSharp
{
    public class Windows
    {
        private readonly Queue<BoxedWindow> AllWindows;
        private Ray _ray;
        public Windows()
        {
            AllWindows = new Queue<BoxedWindow>(Screen.AllScreens.Count() * 5);
        }

        public void Dispose()
        {
            while (AllWindows.Count > 0) {
                var window = AllWindows.Dequeue();

                window.Slide = new CenterSlide(window.Slide);
                window.SetStatus(Status.Showing);
                while (!window.FinishedMoving()) {
                    window.Move();
                }
            }
        }
        public void SetRay(Ray ray)
        {
            Interlocked.Exchange(ref _ray, ray);
        }

        public void UpdateWindows()
        {
            BoxedWindow newWindow = WindowFromRay();

            IntPtr WindowAtCursor = GetRootWindow(GetCursorPos());

            int numOfItems = AllWindows.Count;
            for (int i = 0; i < numOfItems; i++) {
                var window = AllWindows.Dequeue();

                if (newWindow != null) {


                    if (newWindow.hWnd == window.hWnd) {
                        window.SetStatus(Status.Undefined);
                        continue;
                    }
                    if (newWindow.Slide.Equals(window.Slide)) {
                        window.Slide = new CenterSlide(window.Slide);
                        window.SetStatus(Status.Undefined);
                    }
                }

                if (!User32.IsWindow(window.hWnd) || (window.Slide is CenterSlide && window.FinishedMoving())) {
                    continue;
                }

                window.SetStatus(window.hWnd == WindowAtCursor ? Status.Showing : Status.Hiding);
                window.Move();

                AllWindows.Enqueue(window);
            }

            if (newWindow != null) {
                newWindow.SetStatus(Status.Hiding);
                AllWindows.Enqueue(newWindow);
            }
        }

        private BoxedWindow WindowFromRay()
        {
            Ray ray = _ray;
            if (ray == null) return null;
            _ray = null;

            IntPtr RootWindowAtCursorTitlebar = GetRootWindowFromTitlebar(ray.Position);
            if (RootWindowAtCursorTitlebar == IntPtr.Zero) return null;

            return new BoxedWindow(RootWindowAtCursorTitlebar, SlideFactory.SlideFromRay(ray));
        }
    }
}