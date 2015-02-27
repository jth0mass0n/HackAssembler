using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackAssembler
{
    public class Parser
    {
        private System.IO.StreamReader _sr;
        
        public string CurrentCommand { get; set; }
        public bool HasMoreCommands { get; set; }
        public HackAssembler.Enums.Enumerations.CommandType CommandType { get; set; }
        public string Symbol { get; set; }
        public string Comp { get; set; }
        public string Dest { get; set; }
        public string Jump { get; set; }
        

        public Parser(string fileName)
        {
            //open the file string 
            _sr = new System.IO.StreamReader(fileName);
            HasMoreCommands = !_sr.EndOfStream;
        }

        private void Clear(){
             CurrentCommand = "";
             Symbol = "";
             Comp = "";
             Dest = "";
             Jump = "";

        }   


        public void Advance()
        {
            Clear();//clear fields before proceeding

            CurrentCommand = _sr.ReadLine();

            //handle commented lines  and white space
            if (string.IsNullOrWhiteSpace(CurrentCommand))
            {
                CurrentCommand = "";
            }
            bool isComment = CurrentCommand.Contains("//");
            while (isComment || string.IsNullOrWhiteSpace(CurrentCommand))
            {
                CurrentCommand = _sr.ReadLine();
                if (string.IsNullOrWhiteSpace(CurrentCommand))
                {
                    CurrentCommand = "";
                }
                isComment = CurrentCommand.Contains("//");
            }

            HasMoreCommands = !_sr.EndOfStream;

            ProcessCommand(CurrentCommand);

           
        }

        private void ProcessCommand(string command)
        {
            if (command.Contains("@"))
            {
                CommandType = Enums.Enumerations.CommandType.A_COMMAND;
                Symbol = command.Replace("@", "").Trim();
            }
            else if (command.Contains("("))
            {
                CommandType = Enums.Enumerations.CommandType.L_COMMAND;
                Symbol = command.Replace("(", "").Replace(")","").Trim();
            }
            else if (command.Contains(";") || command.Contains("="))
            {
                CommandType = Enums.Enumerations.CommandType.C_COMMAND;
                if (command.Contains(";"))  //jump command
                {
                    char[] delimiterChars = { ';' };
                    var spt = command.Split(delimiterChars);
                    Comp = spt[0]; //not sure about this one...
                    Jump = spt[1];
                }
                else if (command.Contains("="))  //dest=comp command
                {
                    char[] delimiterChars = { '=' };
                    var spt = command.Split( delimiterChars);
                    Dest = spt[0];
                    Comp = spt[1];
                }
            }

        }

       public void GarbageCollection(){
           _sr.Close();
       }
    }
}
