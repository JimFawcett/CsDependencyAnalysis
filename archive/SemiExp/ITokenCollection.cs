/////////////////////////////////////////////////////////////////////////
// ITokenCollection.cs - interface for token collections, e.g., semi   //
// ver 1.1   grammatical analysis                                      //
//                                                                     //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2018     //
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
  using Token = String;
  using TokColl = List<String>;

  public interface ITokenCollection : IEnumerable<Token>
  {
    bool open(string source);                 // attach toker to source
    void close();                             // close toker's source
    TokColl get();                            // collect semi
    int size();                               // number of tokens
    Token this[int i] { get; set; }           // index semi
    ITokenCollection add(Token token);        // add a token to collection
    bool insert(int n, Token tok);            // insert tok at index n
    bool remove(int n);
    void clear();                             // clear all tokens
    bool contains(Token token);               // has token?
    bool find(Token tok, out int index);      // find tok if in semi
    Token predecessor(Token tok);             // find token before tok
    bool hasSequence(params Token[] tokSeq);  // does semi have this sequence of tokens?
    bool hasTerminator();                     // does semi have a valid terminator
    bool isDone();                            // at end of tokenSource?
    int lineCount();                          // get number of lines processed
    string ToString();                        // concatenate tokens with intervening spaces
    ITokenCollection clone();                 // create copy of semi
    ITokenCollection getFunctionParams();     // extract seq of tokens between ( and )
    void show();                              // display semi on console
  }
}
