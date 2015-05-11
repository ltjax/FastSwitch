using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.FastSwitchFile
{
  class FastSwitchOptions : Microsoft.VisualStudio.Shell.DialogPage
  {
    private List<List<string>> fileExtensionGroupList;

    public FastSwitchOptions()
    {
      fileExtensionGroupList = new List<List<string>>();
      fileExtensionGroupList.Add(new List<string> { ".h", ".cpp", ".inl" });
    }

    public string RelatedFileExtensions
    {
      get
      {
        var groupList = new List<string> ();

        foreach (var group in fileExtensionGroupList)
          groupList.Add(string.Join(" ", group));

        return string.Join(";", groupList);
      }
      set
      {
        var newFileExtensionGroupList = new List<List<string>>();

        if (!string.IsNullOrEmpty(value))
        {
          foreach (var group in value.Split (';'))
          {
            var list = group.Split (' ')
                            .Select (item => item.Trim ().ToLower ())
                            .Where (x => !string.IsNullOrEmpty (x))
                            .ToList ();

            if (list.Count >= 2)
            {
              newFileExtensionGroupList.Add(list);
            }
          }
        }

        fileExtensionGroupList = newFileExtensionGroupList;
      }
    }

    public List<List<string>> GetFileExtensionGroupList()
    {
      return fileExtensionGroupList;
    }
  }
}
