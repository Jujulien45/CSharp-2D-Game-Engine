using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Swift;

class Window
{

    // --------------- constantes WIN32 -------------------- //
    private const int WM_CLOSE = 0x0010;
    private const int WM_DESTROY = 0x0002;
    private const int WM_PAINT = 0x000F;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_MOUSEMOVE = 0x0200;
    private const int WM_SIZE = 0x0005;
    private const int CW_USEDEFAULT = unchecked((int)0x80000000);
    private const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint PM_REMOVE = 0x0001;
    private const uint WM_QUIT = 0x0012;

    // defines a pointer towards a function that windows will use to send what the window detect a click, size shift, ...
    private delegate IntPtr WndProc(IntPtr hWnd, // pointer, basicly the id of the window in case if there are multiples
                                    uint msg, // id of the event WM_CLOSE, WM_KEYDOWN, WM_MOUSEMOVE, etc.
                                    IntPtr wParam, // type of the variable can change based on the id of the msg.
                                    // it give data about the msg
                                    IntPtr lParam // same here
                                    );

    /* tells the C# compiler to keep all the variables in the order I puted them in memory.
    we need that because the order of variables mater when it changes to C. */
    [StructLayout(LayoutKind.Sequential)] 
    private struct WNDCLASSEX // a type of window that holds data to create a widow with a specific behavior. a window template.
    {
        public uint cbSize;
        public uint style;
        public WndProc lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int pt_x;
        public int pt_y;
    }

    [DllImport("user32.dll")] // links to the windows function
    private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx); // gives to Widows a window template with a callback
    [DllImport("user32.dll")]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName,
        uint dwStyle, int X, int Y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
    
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern void PostQuitMessage(int nExitCode);

    [DllImport("user32.dll")] // callback
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam); // takes the parameters of windowProc and does the default behavior

    // defines the default behavior of the window
    private static IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg) {
            case WM_DESTROY:
                PostQuitMessage(0);
                return IntPtr.Zero;
            case WM_CLOSE:
                break;
            
        }
        /*Console.WriteLine("\n\nWindowProc: ");
        Console.WriteLine($"hWnd: {hWnd}");
        Console.WriteLine($"msg: {msg}");
        Console.WriteLine($"wParam: {wParam}");
        Console.WriteLine($"lParam: {lParam}");*/
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }


    /*
    private struct WNDCLASSEX // a type of window that holds data to create a widow with a specific behavior. a window template.
    {
        public uint cbSize;
        public uint style;
        public WndProc lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }
    */

    struct WindowTemplate
    {
        public string name;
        public ushort classId;
        public int width;
        public int height;
        //public WindowFlags flags;
    }

    struct WindowInstance
    {
        public ushort id;
        public IntPtr hwnd;
        public string title;
        //public WindowFlags flags;
    }
    
    private static Dictionary<ushort, WindowInstance> windows = new Dictionary<ushort, WindowInstance>();
    private static Dictionary<IntPtr, ushort> hwndToWindowId = new Dictionary<IntPtr, ushort>();
    private static Dictionary<ushort, WindowTemplate> templates = new Dictionary<ushort, WindowTemplate>();
    private static Dictionary<string, ushort> templateNametoId = new Dictionary<string, ushort>();

    private static ushort nextWindowId = 1;
    public static int numOfWindows;

    public Window(string windowName = "EngineWindow", string templateName = "default")
    {

        ushort classId = CreateWindowTemplate(templateName);

        // ID of the window, the handle
        nint id_hwnd = CreateWindowEx(
            0,
            templateName,
            windowName,
            WS_OVERLAPPEDWINDOW | WS_VISIBLE,
            CW_USEDEFAULT,
            CW_USEDEFAULT,
            1920,
            1080,
            IntPtr.Zero,IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        
        if (id_hwnd == 0) {
            Console.WriteLine("Window can't be created.");
        } else {

            WindowInstance win = new WindowInstance();
            win.id = nextWindowId++;
            win.hwnd = id_hwnd;
            win.title = windowName;

            try {
                windows.Add(win.id, win);
                hwndToWindowId.Add(id_hwnd, win.id);
                ShowWindow(id_hwnd, 1);
                UpdateWindow(id_hwnd);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Window couldn't be saved");
                // destroy the window here
            }
        }

        numOfWindows = windows.Count();
        Console.WriteLine($"\n\nWindow Data:\n-windows: {windows}\n-hwndToWindowId: {hwndToWindowId}\n-templates: {templates}\n-templateNametoId: {templateNametoId}");
    }



    public ushort CreateWindowTemplate(string templateName="default")
    {
        ushort classId;
        Dictionary<string, ushort>.KeyCollection keyColl = templateNametoId.Keys;

        bool templateExist = false;

        foreach (string name in keyColl)
        {
            if (templateName == name)
            {
                templateExist = true;
            }
        }
        if (templateExist)
        {
            classId = templateNametoId[templateName];
            Console.WriteLine("template already exist (not a problem)");
        } else
        {
            // creates an instance of WNDCLASSEX
            WNDCLASSEX windowClass = new WNDCLASSEX();
            windowClass.cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX));

            // give a name to the window template
            windowClass.lpszClassName = templateName;

            // links the function so that windows can call it
            windowClass.lpfnWndProc = WindowProc;

            // registers the template so bascly giving it to windows and telling it to save the template
            classId = RegisterClassEx(ref windowClass);

            if (classId == 0)
            {
                Console.WriteLine("Template save failed !");
                return classId;
            }
            Console.WriteLine("window template saved !");


            WindowTemplate template = new WindowTemplate();
            template.height = 1080;
            template.width = 1920;
            template.name = windowClass.lpszClassName;
            template.classId = classId;

            templates.Add(classId, template);
            templateNametoId.Add(windowClass.lpszClassName, classId);
        }

        return classId;
    }
        

}