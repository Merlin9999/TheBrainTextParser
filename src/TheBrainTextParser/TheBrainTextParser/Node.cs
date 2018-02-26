using System;
using System.Collections.Generic;
using System.Linq;

namespace TheBrainTextParser
{
    public class Node
    {
        public string Line { get; }
        public string Notes { get; }
        public List<Node> Children { get; }

        public Node()
            :this(string.Empty, (string)null)
        {
        }

        public Node(string line, params Node[] childNodes)
            : this(line, (string) null, childNodes.AsEnumerable())
        {
        }

        public Node(string line, IEnumerable<Node> childNodes)
            : this(line, (string) null, childNodes)
        {
        }

        public Node(string line, string notes, params Node[] childNodes)
            : this(line, notes, childNodes.AsEnumerable())
        {
        }

        public Node(string line, string notes, IEnumerable<Node> childNodes)
        {
            this.Line = line;
            this.Notes = notes ?? string.Empty;
            this.Children = childNodes.ToList();
        }

        public static Node Read(IEnumerable<string> lines)
        {
            return Read(lines.ToArray());
        }

        public static Node Read(string[] lines)
        {
            lines = TrimBlankLines(lines);

            var rootNodes = new List<Node>();
            
            ReadContext readContext = new ReadContext(lines, rootNodes);

            while (readContext.Next())
                ;

            if (rootNodes.Count == 1)
                return rootNodes[0];
            return new Node(string.Empty, rootNodes);
        }

        private static string[] TrimBlankLines(IEnumerable<string> lines)
        {
            return lines.SkipWhile(line => string.IsNullOrWhiteSpace(line))
                .ToArray();
        }

        internal class ReadContext
        {
            public string[] Lines { get; }
            public List<Node> NodeList { get; }

            private int curLineIdx;
            private Stack<Node> _nodeStack;

            public ReadContext(string[] lines, List<Node> nodeList)
            {
                this.Lines = lines;
                this.NodeList = nodeList;
                this.curLineIdx = 0;
                this._nodeStack = new Stack<Node>();
            }

            public bool Next()
            {
                string curLine = this.Lines[this.curLineIdx++];
                int currentAncestorCount = this._nodeStack.Count;
                int nextAncestorCount = curLine.TakeWhile(c => c == '\t').Count() + 1;
                if (currentAncestorCount + 1 == nextAncestorCount)
                    this.Push(curLine);
                else if (currentAncestorCount == nextAncestorCount + 1)
                    this.Pop(curLine);
                else if (currentAncestorCount == nextAncestorCount)
                    this.Next(curLine);
                else
                    throw new ArgumentException($"Invalid line: \"{curLine}\" Line Number {this.curLineIdx})");

                return this.Lines.Length > this.curLineIdx;
            }

            private void Pop(string curLine)
            {
                this._nodeStack.Pop();
                this._nodeStack.Pop();
                int nodeStackCount = this._nodeStack.Count;
                Node parentNode = nodeStackCount == 0 ? null : this._nodeStack.Peek();
                this.HandleNext(curLine, parentNode, nodeStackCount);
            }

            private void Push(string curLine)
            {
                int nodeStackCount = this._nodeStack.Count;
                Node parentNode = nodeStackCount == 0 ? null : this._nodeStack.Peek();
                this.HandleNext(curLine, parentNode, nodeStackCount);
            }

            private void Next(string curLine)
            {
                this._nodeStack.Pop();
                int nodeStackCount = this._nodeStack.Count;
                Node parentNode = nodeStackCount == 0 ? null : this._nodeStack.Peek();
                this.HandleNext(curLine, parentNode, nodeStackCount);
            }

            private void HandleNext(string curLine, Node parentNode, int currentAncestorCount)
            {
                Node node = new Node(curLine);
               
                if (currentAncestorCount == 0)
                    this.NodeList.Add(node);
                else
                    parentNode.Children.Add(node);

                this._nodeStack.Push(node);
            }
        }
    }
}