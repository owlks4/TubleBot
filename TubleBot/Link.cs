using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubleBot
{
    public class Link
    {
        public double cost = 0;
        public bool evaluated = false;
        public Node myNode;
        public Node otherNode;
        public bool travelled = false;
        public bool invalidForTravel = false; //set only if we already travelled down its counterpart in the other direction

        public Link() {
            }


        public void clean() {
             evaluated = false;
             travelled = false;
             invalidForTravel = false; //set only if we already travelled down its counterpart in the other direction
        }

    public Link(string text, List<Node> nodes, bool twoWayLinksAllowed, bool isCounterpart)
            {
            string[] splitString = text.Split(',');

            for (int i = 0; i < splitString.Length; i++) {
                splitString[i] = splitString[i].Trim().ToUpper().Replace(" ","");
            }

            myNode = null;

            if (!Utility.listContainsNodeWithName(nodes, splitString[0]))
            {
                myNode = new Node(splitString[0]);
                nodes.Add(myNode);
                Console.WriteLine("Adding " + myNode.name);
            }
            else
            {
                myNode = Utility.getNodeWithName(nodes, splitString[0]);
            }

            myNode.zone = double.Parse(splitString[1]);

            for (int j = 0; j < splitString.Length - 2; j++)
            {
                Link newLink = new Link();
                newLink.cost = 1;
                newLink.myNode = myNode;
                myNode.links.Add(newLink);

                if (!Utility.listContainsNodeWithName(nodes, splitString[2+j]))
                {
                    newLink.otherNode = new Node(splitString[2 + j]);
                    nodes.Add(newLink.otherNode);
                    Console.WriteLine("Adding " + newLink.otherNode.name);
                }
                else
                {
                    newLink.otherNode = Utility.getNodeWithName(nodes, splitString[2 + j]);
                }
            }
        }
    }
}
