///////////////////////////////////////////////////////////////////////////
// Display.cs  -  Manage Display properties                              //
// ver 1.0                                                               //
// Language:    C#, Visual Studio 2013, .Net Framework 4.5               //
// Platform:    Dell XPS 2720 , Win 8.1 Pro                              //
// Application: Pr#2 Help, CSE681, Fall 2014                             //
// Author:      Jim Fawcett, CST 2-187, Syracuse University              //
//              (315) 443-3948, jfawcett@twcny.rr.com                    //
///////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * Display manages static public properties used to control what is displayed and
 * provides static helper functions to send information to MainWindow and Console.
 * 
 * Public Interface
 * ================
 * Display.showConsole = false;  // disables most writing to console
 * Display.showFooter = true;    // enables status information display in footer
 * ...
 * Display.displayRules(act, ruleStr)  // sends ruleStr to console and/or footer
 * ...
 */
/*
 * Build Process
 * =============
 * Required Files:
 *   FileMgr.cs
 *   
 * Compiler Command:
 *   devenv CSharp_Analyzer /rebuild debug
 * 
 * Maintenance History
 * ===================
 * ver 1.0 : 19 Oct 14
 *   - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CodeAnalysis
{
  static public class Display
  {
    static Display()
    {
      showFiles = true;
      showDirectories = true;
      showActions = false;
      showRules = false;
      useFooter = false;
      useConsole = false;
      goSlow = false;
      width = 33;
    }
    static public bool showFiles { get; set; }
    static public bool showDirectories { get; set; }
    static public bool showActions { get; set; }
    static public bool showRules { get; set; }
    static public bool showSemi { get; set; }
    static public bool useFooter { get; set; }
    static public bool useConsole { get; set; }
    static public bool goSlow { get; set; }
    static public int width { get; set; }

    static public void showMetricsTable()
    {
      Repository rep = Repository.getInstance();
      List<Elem> table = rep.locations;
      Console.Write(
          "\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}",
          "category", "name", "bLine", "eLine", "size", "cmplx"
      //"\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}  {6,5}  {7,5}",
      //"category", "name", "bLine", "eLine", "bScop", "eScop", "size", "cmplx"
      );
      Console.Write(
          "\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}",
          "--------", "----", "-----", "-----", "----", "-----"
      //"\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}  {6,5}  {7,5}",
      //"--------", "----", "-----", "-----", "-----", "-----", "----", "-----"
      );
      foreach (Elem e in table)
      {
        if (e.type == "class" || e.type == "struct")
          Console.Write("\n");
        Console.Write(
          "\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}",
          e.type, e.name, e.beginLine, e.endLine,
          e.endLine - e.beginLine + 1, e.endScopeCount - e.beginScopeCount + 1
        //"\n  {0,10}  {1,25}  {2,5}  {3,5}  {4,5}  {5,5}  {6,5}  {7,5}",
        //e.type, e.name, e.beginLine, e.endLine, e.beginScopeCount, e.endScopeCount + 1,
        //e.endLine - e.beginLine + 1, e.endScopeCount - e.beginScopeCount + 1
        );
      }
    }
    static public void displaySemiString(string semi)
    {
      if (showSemi && useConsole)
      {
        Console.Write("\n");
        System.Text.StringBuilder sb = new StringBuilder();
        for (int i = 0; i < semi.Length; ++i)
          if (!semi[i].Equals('\n'))
            sb.Append(semi[i]);
        Console.Write("\n  {0}", sb.ToString());
      }
    }

    static public void displayString(Action<string> act, string str)
    {
      if (goSlow) Thread.Sleep(200);
      if (act != null && useFooter)
        act.Invoke(str.Truncate(width));
      if (useConsole)
        Console.Write("\n  {0}", str);
    }

    static public void displayString(string str, bool force=false)
    {
      if (useConsole || force)
        Console.Write("\n  {0}", str);
    }

    static public void displayRules(Action<string> act, string msg)
    {
      if (showRules)
      {
        displayString(act, msg);
      }
    }

    static public void displayActions(Action<string> act, string msg)
    {
      if (showActions)
      {
        displayString(act, msg);
      }
    }

    static public void displayFiles(Action<string> act, string file)
    {
      if (showFiles)
      {
        displayString(act, file);
      }
    }

    static public void displayDirectory(Action<string> act, string file)
    {
      if (showDirectories)
      {
        displayString(act, file);
      }
    }

#if(TEST_DISPLAY)
    static void Main(string[] args)
    {
    }
#endif
  }

  //----< extension method to truncate strings
  public static class StringExt
  {
    public static string Truncate(this string value, int maxLength)
    {
      if (string.IsNullOrEmpty(value)) return value;
      return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
  }
}
