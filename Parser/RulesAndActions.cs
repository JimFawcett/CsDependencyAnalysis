///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 2.3                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Platform:    Dell Precision T7400, Win7, SP1                      //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 2.4 : 09 Oct 2018
 * - modified comments
 * - removed unnecessary definition from repository class
 * - moved local semi definition inside display test in PopStack action
 * - replaced original toker and semi with new versions, e.g., a state-based
 *   toker and semi with some added helper functions.
 * ver 2.3 : 30 Sep 2014
 * - added scope-based complexity analysis
 *   Note: doesn't detect braceless scopes yet
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lexer;

namespace CodeAnalysis
{
  public class QualifierTable
  {
    public static HashSet<string> qualifiers { get; set; } 
      = new HashSet<string>();

    public QualifierTable()
    {
      add("public").add("protected").add("private").add("internal");
      add("abstract").add("async").add("const").add("event").add("extern");
      add("new").add("override").add("partial").add("readonly").add("sealed");
      add("static").add("unsafe").add("virtual").add("volatile");
      add("return").add("++").add("--");
      add("bool").add("int").add("string").add("double").add("List").add("Dictionary");
      add("Action").add("Func");
    }
    public QualifierTable add(string token)
    {
      qualifiers.Add(token);
      return this;
    }
    public bool contains(string token)
    {
      return qualifiers.Contains(token);
    }
  }
  ///////////////////////////////////////////////////////////////////
  // Repository class
  // - Specific to each application
  // - holds results of processing
  // - ScopeStack holds current state of scope processing
  // - List<Elem> holds start and end line numbers for each scope
  ///////////////////////////////////////////////////////////////////

  public class Repository
  {
    ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
    List<Elem> locations_ = new List<Elem>();

    static Repository instance;

    public Repository()
    {
      instance = this;
    }

    //----< provides all code access to Repository >-------------------

    public static Repository getInstance()
    {
      return instance;
    }
    //----< hold name of file and namespace being processed >----------

    public string currentFile { get; set; }
    public string currentNameSpace { get; set; }

    //----< provides access to type table >----------------------------

    public TypeTable typeTable { get; set; } = new TypeTable();

    //----< provides access to qualifier table >-----------------------

    public QualifierTable qualTable { get; set; } = new QualifierTable();

    //----< prevides access to dependency table >----------------------

    public DependencyTable dependencyTable { get; set; } = new DependencyTable();

    //----< provides all actions access to current semiExp >-----------

    public ITokenCollection semi
    {
      get;
      set;
    }

    // semi gets line count from toker who counts lines
    // while reading from its source

    public int lineCount  // saved by newline rule's action
    {
      get { return semi.lineCount(); }
    }
    public int prevLineCount  // not used in this demo
    {
      get;
      set;
    }

    //----< enables recursively tracking entry and exit from scopes >--

    public int scopeCount
    {
      get;
      set;
    }

    public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
    {
      get { return stack_; } 
    }
 
    // the locations table is the result returned by parser's actions
    // in this demo

    public List<Elem> locations
    {
      get { return locations_; }
      set { locations_ = value; }
    }
  }
  ///////////////////////////////////////////////////////////////////
  // Define Actions
  ///////////////////////////////////////////////////////////////////
  // - PushStack
  // - PopStack
  // - PrintFunction
  // - PrintSemi
  // - SaveDeclar

  ///////////////////////////////////////////////////////////////////
  // pushes scope info on stack when entering new scope
  // - pushes element with type and name onto stack
  // - records starting line number

  public class PushStack : AAction
  {
    public PushStack(Repository repo)
    {
      repo_ = repo;
    }

    public override void doAction(ITokenCollection semi)
    {
      Display.displayActions(actionDelegate, "action PushStack");
      ++repo_.scopeCount;
      Elem elem = new Elem();
      elem.type = semi[0];     // expects type, i.e., namespace, class, struct, ..
      elem.name = semi[1];     // expects name
      elem.file = repo_.currentFile;
      elem.nameSpace = repo_.currentNameSpace;
      elem.beginLine = repo_.semi.lineCount() - 1;
      elem.endLine = 0;        // will be set by PopStack action
      elem.beginScopeCount = repo_.scopeCount;
      elem.endScopeCount = 0;  // will be set by PopStack action
      repo_.stack.push(elem);

      // display processing details if requested

      if (AAction.displayStack)
        repo_.stack.display();
      if (AAction.displaySemi)
      {
        Console.Write("\n  line# {0,-5}", repo_.semi.lineCount() - 1);
        Console.Write("entering ");
        string indent = new string(' ', 2 * repo_.stack.count);
        Console.Write("{0}", indent);
        this.display(semi); // defined in abstract action
      }

      // add starting location if namespace, type, or function

      if (elem.type == "control" || elem.name == "anonymous")
        return;
      repo_.locations.Add(elem);
    }
  }
  ///////////////////////////////////////////////////////////////////
  // pops scope info from stack when leaving scope
  // - records end line number and scope count

  public class PopStack : AAction
  {
    public PopStack(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(ITokenCollection semi)
    {
      Display.displayActions(actionDelegate, "action SaveDeclar");
      Elem elem;
      try
      {
        // if stack is empty (shouldn't be) pop() will throw exception

        elem = repo_.stack.pop();

        // record ending line count and scope level

        for (int i = 0; i < repo_.locations.Count; ++i )
        {
          Elem temp = repo_.locations[i];
          if (elem.type == temp.type)
          {
            if (elem.name == temp.name)
            {
              if ((repo_.locations[i]).endLine == 0)
              {
                (repo_.locations[i]).endLine = repo_.semi.lineCount();
                (repo_.locations[i]).endScopeCount = repo_.scopeCount;
                break;
              }
            }
          }
        }
      }
      catch
      {
        return;
      }
      
      if (AAction.displaySemi)
      {
        Lexer.ITokenCollection local = Factory.create();
        local.add(elem.type).add(elem.name);
        if (local[0] == "control")
          return;

        Console.Write("\n  line# {0,-5}", repo_.semi.lineCount());
        Console.Write("leaving  ");
        string indent = new string(' ', 2 * (repo_.stack.count + 1));
        Console.Write("{0}", indent);
        this.display(local); // defined in abstract action
      }
    }
  }
  ///////////////////////////////////////////////////////////////////
  // action to print function signatures - not used in demo

  public class PrintFunction : AAction
  {
    public PrintFunction(Repository repo)
    {
      repo_ = repo;
    }
    public override void display(Lexer.ITokenCollection semi)
    {
      Console.Write("\n    line# {0}", repo_.semi.lineCount() - 1);
      Console.Write("\n    ");
      for (int i = 0; i < semi.size(); ++i)
      {
        if (semi[i] != "\n")
          Console.Write("{0} ", semi[i]);
      }
    }
    public override void doAction(ITokenCollection semi)
    {
      this.display(semi);
    }
  }
  ///////////////////////////////////////////////////////////////////
  // ITokenCollection printing action, useful for debugging

  public class PrintSemi : AAction
  {
    public PrintSemi(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(ITokenCollection semi)
    {
      Console.Write("\n  line# {0}", repo_.semi.lineCount() - 1);
      this.display(semi);
    }
  }
  ///////////////////////////////////////////////////////////////////
  // save declaration

  public class SaveDeclar : AAction
  {
    public SaveDeclar(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(ITokenCollection semi)
    {
      Display.displayActions(actionDelegate, "action SaveDeclar");
      Elem elem = new Elem();
      if (semi.size() == 3)   // type name ;
      {
        elem.type = semi[0];  // expects type
        elem.name = semi[1];  // expects name
        elem.file = repo_.currentFile;
        elem.nameSpace = repo_.currentNameSpace;
        elem.beginLine = repo_.lineCount;
        elem.endLine = elem.beginLine;
        elem.beginScopeCount = repo_.scopeCount;
        elem.endScopeCount = elem.beginScopeCount;
      }
      if(semi.size() == 5)    // namespace . type name ;
      {
        elem.type = semi[2];  // expects type
        elem.name = semi[3];  // expects name
        elem.file = repo_.currentFile;
        elem.nameSpace = semi[0];
        elem.beginLine = repo_.lineCount;
        elem.endLine = elem.beginLine;
        elem.beginScopeCount = repo_.scopeCount;
        elem.endScopeCount = elem.beginScopeCount;
      }

      if (!repo_.typeTable.contains(elem.type))
        return;
      string srcFile = repo_.typeTable.getFile(elem.type, elem.nameSpace);
      if (srcFile == "")
        return;
      repo_.dependencyTable.add(repo_.currentFile, srcFile);
    }
  }
  ///////////////////////////////////////////////////////////////////
  // Define Rules
  ///////////////////////////////////////////////////////////////////
  // - DetectNamespace
  // - DetectClass
  // - DetectFunction
  // - DetectAnonymousScope
  // - DetectPublicDeclaration
  // - DetectLeavingScope

  ///////////////////////////////////////////////////////////////////
  // rule to detect namespace declarations

  public class DetectNamespace : ARule
  {
    public override bool test(ITokenCollection semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectNamespace");
      int index;
      semi.find("namespace", out index);
      if (index != -1 && semi.size() > index + 1)
      {
        ITokenCollection local = Factory.create();
        // create local semiExp with tokens for type and name
        local.add(semi[index]).add(semi[index + 1]);
        doActions(local);
        Repository repo = Repository.getInstance();
        repo.currentNameSpace = semi[index + 1];
        return true;
      }
      return false;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // rule to dectect class definitions

  public class DetectClass : ARule
  {
    public override bool test(ITokenCollection semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectClass");
      int indexCL;
      semi.find("class", out indexCL);
      int indexIF;
      semi.find("interface", out indexIF);
      int indexST;
      semi.find("struct", out indexST);
      int indexDL;
      semi.find("delegate", out indexDL);

      int index = Math.Max(indexCL, indexIF);
      index = Math.Max(index, indexST);
      index = Math.Max(index, indexDL);

      if (index != -1 && semi.size() > index + 1)
      {
        ITokenCollection local = Factory.create();
        // local semiExp with tokens for type and name
        local.add(semi[index]).add(semi[index + 1]);
        doActions(local);

        Repository repo_ = Repository.getInstance();
        TypeTable.LocPair pair;
        pair.file = repo_.currentFile;
        pair.nameSpace = repo_.currentNameSpace;
        repo_.typeTable.add(semi[index + 1], pair);

        return true;
      }
      return true;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // rule to dectect base class definitions

  public class DetectBaseClass : ARule
  {
    public override bool test(ITokenCollection semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectClass");

      if (!semi.hasSequence("class", ":", "{"))
        return true;

      int indexColon;
      semi.find(":", out indexColon);

      ITokenCollection local = Factory.create();
      local.add(semi[indexColon + 1]).add("base").add(";");
      doActions(local);

      return true;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // rule to dectect function definitions

  public class DetectFunction : ARule
  {
    public static bool isSpecialToken(string token)
    {
      string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
      foreach (string stoken in SpecialToken)
        if (stoken == token)
          return true;
      return false;
    }
    public override bool test(ITokenCollection semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectFunction");
      if (semi[semi.size() - 1] != "{")
        return false;

      int index;
      semi.find("(", out index);
      if (index > 0 && !isSpecialToken(semi[index - 1]))
      {
        ITokenCollection local = Factory.create();
        local.add("function").add(semi[index - 1]);
        doActions(local);
        return true;
      }
      return false;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // detect entering anonymous scope
  // - expects namespace, class, and function scopes
  //   already handled, so put this rule after those

  public class DetectAnonymousScope : ARule
  {
    public override bool test(ITokenCollection semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectAnonymousScope");
      int index;
      semi.find("{", out index);
      if (index != -1)
      {
        ITokenCollection local = Factory.create();
        // create local semiExp with tokens for type and name
        local.add("control").add("anonymous");
        doActions(local);
        return true;
      }
      return false;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // detect declaration

  public class DetectDeclar : ARule
  {
    ITokenCollection compactGeneric(ITokenCollection semi)
    {
      ITokenCollection aClone = new Semi();
      int start, stop;
      semi.find("<", out start);
      if (start == -1)
        return semi;
      semi.find(">", out stop);
      if (stop == -1)
        return semi;
      for (int i = 0; i <= start - 2; ++i)
        aClone.add(semi[i]);
      string newTok = "";
      for (int i = start - 1; i < stop + 1; ++i)
        newTok += semi[i];
      //aClone.add(newTok);
      aClone.add(semi[start - 1]);
      for (int i = stop + 1; i < semi.size(); ++i)
        aClone.add(semi[i]);
      return aClone;
    }
    public override bool test(ITokenCollection semi)
    {
      Repository repo = Repository.getInstance();

      ITokenCollection local = semi.clone();
      if (local[0] == "using")
        return false;
      Display.displayRules(actionDelegate, "rule   DetectDeclar");
      int index;
      local.find(";", out index);
      if (index != -1)
      {
        local = compactGeneric(local);
        int pos = 0;
        while (pos < local.size())
        {
          if (repo.qualTable.contains(local[pos]))
          {
            local.remove(pos);
          }
          else
          {
            ++pos;
          }
        }

        local.find("=", out index);
        if (index != -1)
        {
          while (index < local.size() - 1)
          {
            local.remove(index);
          }
        }

        int indexPar;
        local.find("(", out indexPar);
        if (indexPar > -1)
          return false;

        if (local.size() == 3)
        {
          doActions(local);
          return false;
        }
        if (local.size() == 5 && local[1] == ".")
        {
          doActions(local);
          return false;
        }
      }
      return false;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // detect function parameter declaration

  public class DetectParamDeclar : ARule
  {
    ITokenCollection compactGeneric(ITokenCollection semi)
    {
      ITokenCollection aClone = new Semi();
      int start, stop;
      semi.find("<", out start);
      if (start == -1)
        return semi;
      semi.find(">", out stop);
      if (stop == -1)
        return semi;
      for (int i = 0; i <= start - 2; ++i)
        aClone.add(semi[i]);
      string newTok = "";
      for (int i = start - 1; i < stop + 1; ++i)
        newTok += semi[i];
      //aClone.add(newTok);  // this comment causes compaction to remove generic params
      aClone.add(semi[start - 1]);
      for (int i = stop + 1; i < semi.size(); ++i)
        aClone.add(semi[i]);
      return aClone;
    }
    public override bool test(ITokenCollection semi)
    {
      Repository repo = Repository.getInstance();

      int parIndex;
      if (!semi.find("(", out parIndex))
        return false;

      ITokenCollection local = semi.clone().getFunctionParams();

      if (local.size() == 0 || local[0] == "using")
        return false;

      Display.displayRules(actionDelegate, "rule   DetectDeclar");
      int index;
      local.find(";", out index);
      if (index != -1)
      {
        local = compactGeneric(local);
        int pos = 0;
        while (pos < local.size())
        {
          if (repo.qualTable.contains(local[pos]))
          {
            local.remove(pos);
          }
          else
          {
            ++pos;
          }
        }

        local.find("=", out index);
        if (index != -1)
        {
          while (index < local.size() - 1)
          {
            local.remove(index);
          }
        }

        int indexPar;
        local.find("(", out indexPar);
        if (indexPar > -1)
          return false;

        if (local.size() == 3)
        {
          doActions(local);
          return false;
        }
        if (local.size() == 5 && local[1] == ".")
        {
          doActions(local);
          return false;
        }
      }
      return false;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // detect leaving scope

  public class DetectLeavingScope : ARule
  {
    public override bool test(ITokenCollection semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectLeavingScope");
      int index;
      semi.find("}", out index);
      if (index != -1)
      {
        doActions(semi);
        return true;
      }
      return false;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // BuildTypeAnalyzer class
  ///////////////////////////////////////////////////////////////////

  public class BuildTypeAnalyzer
  {
    Repository repo = new Repository();

    public BuildTypeAnalyzer(Lexer.ITokenCollection semi)
    {
      repo.semi = semi;
    }
    public virtual Parser build()
    {
      Parser parser = new Parser();

      // decide what to show
      AAction.displaySemi = false;
      AAction.displayStack = false;  // false is default

      // action used for namespaces, classes, and functions
      PushStack push = new PushStack(repo);

      // capture namespace info
      DetectNamespace detectNS = new DetectNamespace();
      detectNS.add(push);
      parser.add(detectNS);

      // capture class info
      DetectClass detectCl = new DetectClass();
      detectCl.add(push);
      parser.add(detectCl);

      //// capture function info
      //DetectFunction detectFN = new DetectFunction();
      //detectFN.add(push);
      //parser.add(detectFN);

      // handle entering anonymous scopes, e.g., if, while, etc.
      DetectAnonymousScope anon = new DetectAnonymousScope();
      anon.add(push);
      parser.add(anon);

      //// show public declarations
      //DetectPublicDeclar pubDec = new DetectPublicDeclar();
      //SaveDeclar print = new SaveDeclar(repo);
      //pubDec.add(print);
      //parser.add(pubDec);

      // handle leaving scopes
      DetectLeavingScope leave = new DetectLeavingScope();
      PopStack pop = new PopStack(repo);
      leave.add(pop);
      parser.add(leave);

      // parser configured
      return parser;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // BuildDepAnalyzer class
  ///////////////////////////////////////////////////////////////////

  public class BuildDepAnalyzer
  {
    Repository repo = Repository.getInstance();

    public BuildDepAnalyzer(Lexer.ITokenCollection semi)
    {
      repo.semi = semi;
    }
    public virtual Parser build()
    {
      Parser parser = new Parser();

      // decide what to show
      AAction.displaySemi = false;
      AAction.displayStack = false;  // false is default

      // save declarations
      DetectDeclar Decl = new DetectDeclar();
      SaveDeclar saveDecl = new SaveDeclar(repo);
      Decl.add(saveDecl);
      parser.add(Decl);

      DetectParamDeclar Decl2 = new DetectParamDeclar();
      Decl2.add(saveDecl);
      parser.add(Decl2);

      DetectBaseClass Decl3 = new DetectBaseClass();
      Decl3.add(saveDecl);
      parser.add(Decl3);

      // parser configured
      return parser;
    }
  }
}

