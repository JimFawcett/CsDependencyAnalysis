///////////////////////////////////////////////////////////////////////
// DemoReqs.cs - Demonstrate Project #2 Requirements                 //
// ver 1.0                                                           //
// Language:    C#, 2017, .Net Framework 4.7.1                       //
// Platform:    Dell Precision T8900, Win10                          //
// Application: Demonstration for CSE681, Project #3, Fall 2018      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines the following class:
 *   Executive:
 *   - uses Parser, RulesAndActions, Semi, and Toker to perform basic
 *     code metric analyzes
 */
/* Required Files:
 *   Executive.cs
 *   Parser.cs
 *   IRulesAndActions.cs, RulesAndActions.cs, ScopeStack.cs, Elements.cs
 *   ITokenCollection.cs, Semi.cs, Toker.cs
 *   Display.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 09 Oct 2018
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeAnalysis
{
  using Lexer;

  class Executive
  {
    //----< process commandline to get file references >-----------------

    static List<string> ProcessCommandline(string[] args)
    {
      List<string> files = new List<string>();
      if (args.Length < 2)
      {
        Console.Write("\n  Please enter path and file(s) to analyze\n\n");
        return files;
      }
      string path = args[0];
      if (!Directory.Exists(path))
      {
        Console.Write("\n  invalid path \"{0}\"", System.IO.Path.GetFullPath(path));
        return files;
      }
      path = Path.GetFullPath(path);
      for (int i = 1; i < args.Length; ++i)
      {
        string filename = Path.GetFileName(args[i]);
        files.AddRange(Directory.GetFiles(path, filename));
      }
      return files;
    }

    bool testToker()
    {
      return false;
    }

    static void ShowCommandLine(string[] args)
    {
      Console.Write("\n  Commandline args are:\n  ");
      foreach (string arg in args)
      {
        Console.Write("  {0}", arg);
      }
      Console.Write("\n  current directory: {0}", System.IO.Directory.GetCurrentDirectory());
      Console.Write("\n");
    }

    static void Main(string[] args)
    {
      Console.Write("\n  Demonstrating Project #2 Requirements");
      Console.Write("\n =======================================\n");

      TestHarness.Tester tester = new TestHarness.Tester();

      TestReq3 tr3 = new TestReq3();
      tester.add(tr3);

      TestReq4 tr4 = new TestReq4();
      tester.add(tr4);

      TestReq5 tr5 = new TestReq5();
      tester.add(tr5);

      TestReq6 tr6 = new TestReq6();
      tester.add(tr6);

      TestReq7 tr7 = new TestReq7();
      tester.add(tr7);

      TestReq8 tr8 = new TestReq8();
      tester.add(tr8);

      TestReq9 tr9 = new TestReq9();
      tester.add(tr9);

      TestReq10a tr10a = new TestReq10a();
      tester.add(tr10a);

      TestReq10b tr10b = new TestReq10b();
      tester.add(tr10b);

      TestReq10c tr10c = new TestReq10c();
      tester.add(tr10c);

      tester.execute();

    //  ShowCommandLine(args);

    //  List<string> files = ProcessCommandline(args);
    //  foreach (string file in files)
    //  {
    //    Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));

    //    ITokenCollection semi = Factory.create();

    //    if (!semi.open(file as string))
    //    {
    //      Console.Write("\n  Can't open {0}\n\n", args[0]);
    //      return;
    //    }

    //    Console.Write("\n  Type and Function Analysis");
    //    Console.Write("\n ----------------------------");

    //    semi.close();
    //  }
      Console.Write("\n\n");
    }
  }
}
