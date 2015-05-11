using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Collections;
using System.IO;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using System.Linq;

namespace Company.FastSwitchFile
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

    [ProvideOptionPage(typeof(FastSwitchOptions), "Fast Switch", "Options", 1000, 1001, true)] 
    [Guid(GuidList.guidSwitchFilesPkgString)]
    public sealed class FastSwitchFilePackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public FastSwitchFilePackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidSwitchFilesCmdSet, (int)PkgCmdIDList.cmdidSwitchFiles);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID );
                mcs.AddCommand( menuItem );
            }
        }
        #endregion

        static private IEnumerable<string> GetFileNames(ProjectItems projectItems)
        {
          var items = new List<string>();

          if (projectItems != null)
          {
            foreach (ProjectItem p in projectItems)
            {
              if (p.Kind != "{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}")
              {
                for (short i = 1; i <= p.FileCount; ++i)
                {
                  string fileName = p.FileNames[i];
                  if (!string.IsNullOrEmpty(fileName))
                  {
                    items.Add(fileName);
                  }
                }
              }

              items.AddRange(GetFileNames(p.ProjectItems));
            }
          }

          return items;
        }

        private string GetAlternatePathIfExists(Document document)
        {
          if (document == null)
          {
            return null;
          }

          var projectItem = document.ProjectItem;
          if (projectItem == null)
          {
            return null;
          }

          var project = projectItem.ContainingProject;
          if (project == null)
          {
            return null;
          }

          var name = Path.GetFileName(document.FullName);
          if (string.IsNullOrEmpty(name))
          {
            return null;
          }

          var options = GetOptionsPage<FastSwitchOptions>();
          if (options == null)
          {
            return null;
          }

          List<List<string>> extensionGroupList = options.GetFileExtensionGroupList();

          if (extensionGroupList == null)
          {
            return null;
          }

          foreach (var extensionGroup in extensionGroupList)
          {
            foreach (var extension in extensionGroup)
            {
              if (name.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase))
              {
                var prefix = name.Substring(0, name.Length - extension.Length);
                var names = from e in extensionGroup select prefix + e;

                var paths = new List<string>(
                  from string fileName in GetFileNames(project.ProjectItems)
                  where names.Contains(Path.GetFileName(fileName), StringComparer.CurrentCultureIgnoreCase)
                  let directory = Path.GetDirectoryName(fileName)
                  let suffix = Path.GetFileName(fileName).Substring(prefix.Length)
                  orderby directory, suffix
                  select fileName
                );

                if (paths.Count >= 2)
                {
                  int nextIndex = (1 + paths.IndexOf(document.FullName)) % paths.Count;
                  return paths[nextIndex];
                }
              }
            }
          }

          return null;
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
          var IDE = (DTE2)GetService(typeof(DTE));
          string alternatePath = GetAlternatePathIfExists(IDE.ActiveDocument);
          if (!String.IsNullOrEmpty(alternatePath))
          {
            IDE.ItemOperations.OpenFile(alternatePath, EnvDTE.Constants.vsViewKindAny);
          }
        }

        private T GetOptionsPage<T>()
          where T : DialogPage
        {
          return (T)GetDialogPage(typeof(T));
        }
    }
}
