using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TetrisBot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Threading.Thread.Sleep(1000);
            ScreenGrab s = new ScreenGrab();
        }
    }
}
