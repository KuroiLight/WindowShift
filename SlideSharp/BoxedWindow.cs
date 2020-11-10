﻿using System;
using Win32Api;

namespace SlideSharp
{


    public class BoxedWindow
    {
        public IntPtr hWnd;
        private Easer Easer;
        private Status Status;
        public Slide Slide;

        public BoxedWindow(IntPtr hwnd, Slide slide)
        {
            Status = Status.Undefined;
            Slide = slide;
            hWnd = hwnd;
            Easer = new Easer();
        }

        public void SetStatus(Status status)
        {
            if (status != Status) {
                Status = status;
                RECT windowRect = User32.GetWindowRect(hWnd);
                Easer = new Easer(windowRect.ToPoint, Status != Status.Showing ?  Slide.HiddenPosition(windowRect) : Slide.ShownPosition(windowRect));
            }
        }

        public void Move()
        {
            User32.SetWindowPos(hWnd, Easer.TakeStep());
        }

        public bool IsMoving()
        {
            return Easer.Percent != 100;
        }
    }
}