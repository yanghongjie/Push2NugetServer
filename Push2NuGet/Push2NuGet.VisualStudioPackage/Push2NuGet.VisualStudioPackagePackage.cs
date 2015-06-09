using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Xml;
using EnvDTE;
using EnvDTE80;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Process = System.Diagnostics.Process;

namespace 逗豆豆.Push2NuGet_VisualStudioPackage
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidPush2NuGet_VisualStudioPackagePkgString)]
    public sealed class Push2NuGetVisualStudioPackagePackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public Push2NuGetVisualStudioPackagePackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                var menuCommandID = new CommandID(GuidList.guidPush2NuGet_VisualStudioPackageCmdSet, (int)PkgCmdIDList.cmdidPush2NuGet);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
            }
        }
        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                var dte = GetGlobalService(typeof(DTE)) as DTE2;
                var rootPath = Path.GetDirectoryName(dte.Solution.FullName);
                var itemName = dte.SelectedItems.Item(1).Name;
                var itemFullName = ((dte.ActiveSolutionProjects as Array).GetValue(0) as Project).FullName;

                var hasNugetExe = File.Exists(rootPath + "\\.nuget\\NuGet.exe");
                if (hasNugetExe)
                {
                    var nugetConfigfXml = rootPath + "\\.nuget\\Nuget.xml";
                    if (!File.Exists(nugetConfigfXml))
                    {
                        MessageBox.Show(Resources.Push2NuGetVisualStudioPackagePackage_MenuItemCallback_NugetXmlNotFound, Resources.Push2NuGetVisualStudioPackagePackage_MenuItemCallback_Push2NuGet_Infomation, MessageBoxButtons.OK);
                        return;
                    }
                    var nuget = GetSelfServer(nugetConfigfXml);
                    if (nuget == null || nuget.Url == null || nuget.ApiKey == null)
                    {
                        MessageBox.Show(Resources.Push2NuGetVisualStudioPackagePackage_MenuItemCallback_ResolveNugetXmlFaild, Resources.Push2NuGetVisualStudioPackagePackage_MenuItemCallback_Push2NuGet_Infomation, MessageBoxButtons.OK);
                        return;
                    }
                    var nugetexePath = rootPath + "\\.nuget\\NuGet.exe";
                    var tempPackageFloder = rootPath + "\\.nuget\\temp\\";
                    if (!Directory.Exists(tempPackageFloder))
                        Directory.CreateDirectory(tempPackageFloder);
                    var itemPackageFloder = rootPath + "\\.nuget\\" + itemName + "\\";
                    if (!Directory.Exists(itemPackageFloder))
                        Directory.CreateDirectory(itemPackageFloder);

                    RunCmd("@ECHO OFF");
                    var packMsg = RunCmd(nugetexePath + " pack " + itemFullName + " -Build -o " + tempPackageFloder);
                    var pushMsg = RunCmd("nuget push  " + tempPackageFloder + "*.nupkg -s " + nuget.Url + " " + nuget.ApiKey);
                    var moveMsg = RunCmd("move " + tempPackageFloder + "*.nupkg " + itemPackageFloder);
                    RunCmd("@ECHO ON");

                    var showMsg = packMsg + Environment.NewLine + pushMsg + Environment.NewLine + moveMsg +
                                  Environment.NewLine + "Pack and Push Success ！";
                    MessageBox.Show(showMsg,Resources.Push2NuGetVisualStudioPackagePackage_MenuItemCallback_Push2NuGet_Infomation,MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                MessageBox.Show(Resources.Push2NuGetVisualStudioPackagePackage_MenuItemCallback_NuGetExeNotFound, Resources.Push2NuGetVisualStudioPackagePackage_MenuItemCallback_Push2NuGet_Infomation, MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.Push2NuGetVisualStudioPackagePackage_MenuItemCallback_Push2NuGet_Error, MessageBoxButtons.OK);
            }
        }
        /// <summary>
        /// 运行命令行
        /// </summary>
        /// <param name="command">Cmd命令</param>
        /// <returns></returns>
        private string RunCmd(string command)
        {
            //实例一个Process类，启动一个独立进程
            var p = new Process();
            //Process类有一个StartInfo属性
            //设定程序名
            p.StartInfo.FileName = "cmd.exe";
            //设定程式执行参数   
            p.StartInfo.Arguments = "/c " + command;
            //关闭Shell的使用  
            p.StartInfo.UseShellExecute = false;
            //重定向标准输入     
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            //重定向错误输出  
            p.StartInfo.RedirectStandardError = true;
            //设置不显示窗口
            p.StartInfo.CreateNoWindow = true;
            //启动
            p.Start();
            //从输出流取得命令执行结果
            return p.StandardOutput.ReadToEnd();
        }
        /// <summary>
        /// 获取服务
        /// </summary>
        /// <param name="xmlConfig"></param>
        /// <returns></returns>
        private NugetSelf GetSelfServer(string xmlConfig)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlConfig);
            var root = xmlDoc.SelectSingleNode("SelfServer");
            var url = root.ChildNodes[0].InnerText;
            var apikey = root.ChildNodes[1].InnerText;

            return new NugetSelf { Url = url, ApiKey = apikey };
        }
    }

    public class NugetSelf
    {
        public string Url { get; set; }
        public string ApiKey { get; set; }
    }
}
