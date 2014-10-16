using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeRsolver
{
    public class VNode
    {
        public string curDir;
        public string rootDir;
        public string fileName;
        public string fileExtention;        
        public bool isFile;
        public bool hasPara;
        public bool isRemote;
        public bool isValid;
        public Dictionary<string, string> param = new Dictionary<string, string>();

        public bool extractFileParam(string[] nodeArray){
          
                string rmNode = nodeArray[nodeArray.Length - 1];
                if (rmNode.Contains("!"))
                    fileName = rmNode.Remove(rmNode.IndexOf("!"));
                else
                    fileName = rmNode;

                
                if (rmNode.Contains("."))
                {
                    isFile = true;
                    fileExtention = fileName.Substring(fileName.IndexOf(".") + 1);
                    fileName = rmNode.Remove(rmNode.IndexOf("."));
                    if (rmNode.Contains("!"))
                    {
                        hasPara = true;

                        string[] parameter = rmNode.Substring(rmNode.IndexOf("!") + 1).Split('&'); ;
                        foreach (string prm in parameter)
                        {
                            string[] keyval = prm.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (keyval == null || keyval.Length != 2)
                            {
                                isValid = false; return false;
                            }
                            else if(!param.Keys.Contains(keyval[0]))
                                param.Add(keyval[0], keyval[1]);



                        }

                    }
                    else
                    {
                        hasPara = false;
                        
                        isValid = true;
                    }

                }
                else {
                    fileName = rmNode;
                    hasPara = false;
                    isFile = false;
                    isValid = true;
                    isRemote = false;
                }
                return true;
            
          

        }
        public VNode(string path)
        {
            
            string[] nodeArray = path.Split(new char[]{'\\'},StringSplitOptions.RemoveEmptyEntries);
            //Current Directory is Root
            curDir="\\";
            rootDir="\\";
            fileName="";
            fileExtention="";            
            if (nodeArray==null || nodeArray.Length == 0)
            {
                isFile = false;
                isRemote = false;
                isValid = true;
                hasPara = false;

            }
            else if (nodeArray.Length == 1)
            {
                extractFileParam(nodeArray);
                if (!isFile)
                    curDir = nodeArray[nodeArray.Length - 1];
            }
            else if (nodeArray.Length == 2)
            {
                
                rootDir = nodeArray[nodeArray.Length - 2];
                extractFileParam(nodeArray);
                if(!isFile)
                    curDir = nodeArray[nodeArray.Length - 1];
                else
                    curDir = nodeArray[nodeArray.Length - 2];

                Console.WriteLine("Resolve {0} {1}",curDir,rootDir);
                
            }
            else
            {
                
                extractFileParam(nodeArray);
                rootDir = nodeArray[0];
                if(isFile)
                curDir = nodeArray[nodeArray.Length - 2];                
                else
                    curDir = nodeArray[nodeArray.Length - 1];                
            }
            
                
           
        }

    }
}
