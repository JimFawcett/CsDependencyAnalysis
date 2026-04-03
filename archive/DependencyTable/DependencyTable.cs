///////////////////////////////////////////////////////////////////////////
// DependencyTable.cs  -  Manage Dependency information                  //
// ver 1.0                                                               //
// Language:    C#, Visual Studio 2017, .Net Framework 4.5               //
// Platform:    Dell XPS 8920, Windows 10 Pro                            //
// Application: Pr#3 demo, CSE681, Fall 2018                             //
// Author:      Jim Fawcett, CST 2-187, Syracuse University              //
//              (315) 443-3948, jfawcett@twcny.rr.com                    //
///////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * DependencyTable class manages file information generated during type-based
 * code analysis, in its dependency analysis phase.
 * 
 * Required Files:
 * ---------------
 *   DependencyTable.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 29 Nov 2018
 *   - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
  public class DependencyTable
  {
    public Dictionary<string, List<string>> dependencies { get; set; }
      = new Dictionary<string, List<string>>();

    //----< add parent file >----------------------------------------

    public void addParent(string parentFile)
    {
      if (dependencies.ContainsKey(parentFile))
        return;
      List<string> deps = new List<string>();
      dependencies.Add(parentFile, deps);
    }
    //----< add child file dependency >------------------------------

    public void add(string parentFile, string childFile)
    {
      if (parentFile == childFile)
        return;
      if (dependencies.ContainsKey(parentFile))
      {
        if (dependencies[parentFile].Contains(childFile))
          return;
        dependencies[parentFile].Add(childFile);
      }
      else
      {
        List<string> children = new List<string>();
        children.Add(childFile);
        dependencies.Add(parentFile, children);
      }
    }
    //----< is parentFile a key for dependency table? >--------------

    public bool contains(string parentFile)
    {
      return dependencies.ContainsKey(parentFile);
    }
    //----< clear contents of table >--------------------------------

    public void clear()
    {
      dependencies.Clear();
    }
    //----< display contents of dependency table >-------------------

    public void show(bool fullyQualified = false)
    {
      foreach (var item in dependencies)
      {
        string file = item.Key;
        if (!fullyQualified)
          file = System.IO.Path.GetFileName(file);
        Console.Write("\n  {0}", file);
        if (item.Value.Count == 0)
          continue;
        Console.Write("\n    ");
        foreach (var elem in item.Value)
        {
          string child = elem;
          if (!fullyQualified)
            child = System.IO.Path.GetFileName(child);
          Console.Write("{0} ", child);
        }
      }
    }
  }
  class TestDependencyTable
  {
#if(TEST_DEPENDENCYTABLE)
    static void Main(string[] args)
    {
      Console.Write("\n  Tested in Executive");
      Console.Write("\n\n");
    }
#endif
  }
}
