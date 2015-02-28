using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace HackAssembler
{
    public partial class Form1 : Form
    {
        private string _fileName = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            

            var dlg = new OpenFileDialog();
            
            dlg.Filter = "Assembly Files (*.asm)|*.asm";

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _fileName = dlg.FileName;
                lblStatus.Text = "File Loaded Successfully: " + _fileName;
                frmStatus.Refresh();
            }

           
            
            
            
           
        }

        /// <summary>
        /// first pass will parse labels and add them to the symbol table
        /// </summary>
        private SymbolTable FirstPass()
        {
            var symbols = new SymbolTable();//instantiate symbol table

            if (!string.IsNullOrEmpty(_fileName))
            {
                var prsr = new Parser(_fileName);
                var sbsource = new StringBuilder(); //used to hold source text
                
                int lineNo = 0; //used to track line number of current command

                lblStatus.Text = "Adding Labels to Symbol Table... ";
                frmStatus.Refresh();

                while (prsr.HasMoreCommands)
                {

                    prsr.Advance();
                    sbsource.AppendLine(prsr.CurrentCommand);
                   
                    if (prsr.CommandType == Enums.Enumerations.CommandType.A_COMMAND || prsr.CommandType == Enums.Enumerations.CommandType.C_COMMAND)
                    {
                        lineNo++; //increment line number
                    }
                    else if (prsr.CommandType == Enums.Enumerations.CommandType.L_COMMAND)
                    {
                        //need to add label to symbol table 
                        var smbl = prsr.Symbol;
                        if (!symbols.Contains(smbl))
                        {
                            symbols.AddEntry(smbl, lineNo + 1);  //add address of next command
                        }
                    }
                }

                prsr.GarbageCollection();
                rtbSource.Text = sbsource.ToString();
            }

            lblStatus.Text = "File " + _fileName + " Parsed Successfully.";
            frmStatus.Refresh();

            return symbols;
        }

        /// <summary>
        /// second pass will parse line by line and add remaining variables to symbol table
        /// </summary>
        private void SecondPass(SymbolTable symbols)
        {
            if (!string.IsNullOrEmpty(_fileName))
            {
                var prsr = new Parser(_fileName);
                var sbuilder = new StringBuilder(); //used to hold our output
                int currentRamAddress = 16; //starting ram position for variables is 16

                while (prsr.HasMoreCommands)
                {

                    prsr.Advance();

                    lblStatus.Text = "Parsing Line: " + sbuilder.Length;
                    frmStatus.Refresh();

                    if (prsr.CommandType == Enums.Enumerations.CommandType.A_COMMAND)
                    {
                        //need to see if symbol is in our symbol table already
                        var symbl = prsr.Symbol;
                        int numInt;
                        if(!int.TryParse(symbl, out numInt)){
                             //if this symbol is not a number then it needs to be in the symbol table
                            if (symbols.Contains(symbl))
                            {
                                //replace current symbol with numeric meaning
                                numInt = symbols.GetAddress(symbl);
                            }
                            else
                            {
                                numInt = currentRamAddress;

                                //no symbol in table yet, so put it in
                                symbols.AddEntry(symbl, currentRamAddress);
                                currentRamAddress++; //increment ram address for next variable
                            }
                        }

                        //need to translate decimal value to binary
                        var a = Convert.ToString(numInt, 2); //first convert to binary representation
                        var zro = 0;
                        var padding = zro.ToString("D" + (16 - a.Length));
                        
                        sbuilder.Append(padding + a);
                    }
                    else if (prsr.CommandType == Enums.Enumerations.CommandType.C_COMMAND)
                    {
                        //need to concatenate instruction fields to binary
                        //rtbDestination.Text += "111";
                        sbuilder.Append("111");
                        var compBits = Code.Comp(prsr.Comp);

                        foreach (bool bit in compBits)
                        {
                            //rtbDestination.Text += bit ? "1" : "0";
                            sbuilder.Append(bit ? "1" : "0");
                        }

                        var destBits = Code.Dest(prsr.Dest);

                        foreach (bool bit in destBits)
                        {
                            //rtbDestination.Text += bit ? "1" : "0";
                            sbuilder.Append(bit ? "1" : "0");
                        }

                        var jumpBits = Code.Jump(prsr.Jump);

                        foreach (bool bit in jumpBits)
                        {
                            //rtbDestination.Text += bit ? "1" : "0";
                            sbuilder.Append(bit ? "1" : "0");
                        }


                    }
                    sbuilder.Append(Environment.NewLine);
                    //rtbDestination.Text += Environment.NewLine;
                }

                prsr.GarbageCollection();
                rtbDestination.Text = sbuilder.ToString();
            }

            lblStatus.Text = "File " + _fileName + " Parsed Successfully.";
            frmStatus.Refresh();
        }

        private void ParseFile()
        {
            //process labels
            var symbols = FirstPass();

            //process everything else
            SecondPass(symbols);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            //read one line at a time from rtbSource and parse it with appropriate modules

            ParseFile();

            
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //open save file dialog
            var savefile = new SaveFileDialog();
            // set a default file name
            var fileName = System.IO.Path.GetFileName(_fileName).Replace("asm","hack");
            savefile.FileName = fileName;

            // set filters - this can be done in properties as well
            savefile.Filter = "Hack files (*.hack)|*.hack*";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                rtbDestination.SaveFile(savefile.FileName, RichTextBoxStreamType.PlainText);

                lblStatus.Text = "File " + savefile.FileName + " Saved Successfully.";
                frmStatus.Refresh();
            }
        }
    }
}
