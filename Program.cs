using System;

namespace XHRTools
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Application.EnableVisualStyles();
            // Application.SetCompatibleTextRenderingDefault(false);
            // var mainForm = new Form1();
            // mainForm.Disposed += (sender, args) => Application.Exit();
            // Application.Run(mainForm);
            XhrRequest.Learn(new User("",""));
        }
    }
}