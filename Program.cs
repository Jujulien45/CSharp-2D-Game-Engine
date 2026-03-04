using System;
using System.Globalization;
using System.Runtime.InteropServices;

class Program
{
    private const uint PM_REMOVE = 0x0001;
    private const uint WM_QUIT = 0x0012;

    static void Main()
    {
        Window window1 = new Window("Hi", "test1");
        Window window2 = new Window("Hello", "test1");

        Window.MSG msg;
        while (Window.numOfWindows > 0)
        {
            // Traiter TOUS les messages Windows disponibles
            while (Window.PeekMessage(out msg, IntPtr.Zero, 0, 0, PM_REMOVE))
            {
                if (msg.message == WM_QUIT)
                {
                    Window.numOfWindows -= 1;
                }

                Window.TranslateMessage(ref msg);
                Window.DispatchMessage(ref msg);
            }
        }
    }
    
}