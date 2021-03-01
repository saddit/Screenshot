using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var assemblyName = new AssemblyName(args.Name);

            var path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
                path = $@"{assemblyName.CultureInfo}\{path}";
            var stream = executingAssembly.GetManifestResourceStream(path);
            if (stream == null) return null;
            var assemblyRawBytes = new byte[stream.Length];
            stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
            return Assembly.Load(assemblyRawBytes);
        }

        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int WS_SHOWNORMAL = 1;

        private void HandleOtherProcess(IntPtr hWnd)
        {
            bool i = ShowWindowAsync(hWnd, WS_SHOWNORMAL);
            bool j = SetForegroundWindow(hWnd);
            Console.WriteLine("i:" + i + ",j:" + j);
        }

        private Process CheckProcess()
        {
            Process current = Process.GetCurrentProcess();
            Process[] others = Process.GetProcessesByName(current.ProcessName);
            foreach (var i in others)
            {
                if (i.Id != current.Id) return i;
            }
            return null;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Process other = CheckProcess();
            if (other != null)
            {
                //HandleOtherProcess(other.MainWindowHandle);
                MessageBox.Show("应用程序已经在运行了", "请勿多开");
                Shutdown();
            }
        }
    }
}
