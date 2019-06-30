using Microsoft.SPOT;
using System;
using System.Text;


namespace ProfERP.Netduino.GCodeParser
{
    public class GCodeParser
    {
        private GCodeMachine machine;

        private char[] NumberBuffer = new char[20];
        private int NumberBufferIndex = 0;

        private bool DeletedLine = false;
        private int LineNumber = 0;

        private int CurrentIndex = 0;
        public string Line;
        public int ParserLineNumber = 0;

        public GCodeParser(GCodeMachine _machine)
        {
            machine = _machine;
        }

        public void ParseLines(string gcode)
        {
            var lines = gcode.Split(new[] { '\r', '\n' });
            foreach (var line in lines)
                ParseLine(line);
        }

        public void ParseLine(string line)
        {
            Debug.Print(line);
            ParserLineNumber++;
            Line = line.ToUpper();
            CurrentIndex = 0;
            ParseCommand();
        }

        internal void MachineStart()
        {
            machine.Start();
        }

        internal void MachineStop()
        {
            machine.Stop();
        }

        private bool SkipSpaces()
        {
            while (CurrentIndex < Line.Length)
            {
                char c = Line[CurrentIndex];

                switch (c)
                {
                    // Skip spaces and tabs
                    case ' ':
                    case '\t':
                        CurrentIndex++;
                        break;
                    default:
                        return true;
                }
            }

            return false;
        }

        internal void MachineMove(float toX, float toY, float toZ, float toI, float toJ, float toK, float toR)
        {
            machine.Move(toX, toY, toZ, toI, toJ, toK, toR);
        }

        internal void MachineSetState(MachineState ms)
        {
            machine.SetState(ms);
        }

        internal char PeekChar()
        {
            if (!SkipSpaces()) return (char)0;
            if (CurrentIndex >= Line.Length) return (char)0;                

            return Line[CurrentIndex];
        }

        internal char ReadChar()
        {
            if (!SkipSpaces()) return (char)0;
            if (CurrentIndex >= Line.Length) return (char)0;

            return Line[CurrentIndex++];
        }

        internal void MachineSetPlane(MachinePlane mp)
        {
            machine.SetPlane(mp);
        }

        internal string ReadNumber()
        { 
            NumberBufferIndex = 0;
            bool isFirst = true;

            while (true)
            {
                char c = PeekChar();

                switch (c)
                { 
                    case '(':
                        SkipToEndComment();
                        break;
                    case ' ': 
                    case '\t':
                        if (NumberBufferIndex > 0) return GetNumberString();
                        break;

                    case ';':
                        return GetNumberString();

                    case '-':
                        if (isFirst)
                            AddDigitToNumber(c);
                        else 
                            return GetNumberString();

                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '.':
                        AddDigitToNumber(c);
                        break;
                    
                    case '\0':
                    default:
                        return GetNumberString();
                }

                isFirst = false;
            }
        }

        internal void MachineSetMode(DistanceMode mode)
        {
            machine.SetMode(mode);
        }

        private void SkipToEndComment()
        {
            while (true)
            {
                char c = ReadChar();
             
                if (c == ')') break;
            }
        }

        private void AddDigitToNumber(char d)
        {
            ReadChar();
            NumberBuffer[NumberBufferIndex] = d;
            NumberBufferIndex++;
        }

        private string GetNumberString()
        {
            if (NumberBufferIndex > 0)
            {
                return new string(NumberBuffer, 0, NumberBufferIndex);
            }
            else
            {
                return null;
            }
        }

        internal int ParseInt()
        {
            
            string val = ReadNumber();

            if (val == null)
            {
                Logger.Error("ParseInt: no numbers at {0} in '{1}'. Skipped.", CurrentIndex, Line);
                return -1;
            }

            try
            {
                return int.Parse(val);
            }
            catch (Exception ex)
            {
                Logger.Error("ParseInt ERROR parsing '{0}': {1}", val, ex);
                return -1;
            }
        }

        internal double ParseDouble()
        {
            string val = ReadNumber();

            if (val == null) 
            {
                Logger.Error("ParseDouble: no numbers at {0} in '{1}'. Skipped.", CurrentIndex, Line);
                return double.MinValue;
            }

            try
            {
                return double.Parse(val);
            }
            catch (Exception ex)
            {
                Logger.Error("ParseDouble ERROR parsing '{0}': {1}", val, ex);
                return double.MinValue;
            }
        }

        private bool ParseComment()
        {
            while (true)
            {
                char c = ReadChar();

                switch (c)
                {
                    case '\0': return false;
                    case ')': return true;
                }
            }
        }

        private bool ParseDashComment()
        {
            int numDashes = 1;

            while (true)
            {
                char c = ReadChar();

                switch (c)
                {
                    case '\0': return false;
                    case '-': numDashes++; break;
                    default:
                        if (numDashes < 3)
                        {
                            Logger.Error("ERROR: Parse --- comment: not enough '-' to interpret as comment. Returning control to parser, but it's an unsuported combination.");
                            return true;
                        }
                        
                        return false;      // otherwise, the rest of the line is a comment, no need to continue.
                }
            }
        }

        private void ParseCommand()
        {
            while (true)
            {
                char c = ReadChar();

                switch (c)
                {
                    case '\0': return;  // EOL

                    case ';': return;  // comment line

                    case '(': // Begin comment
                        if (!ParseComment()) return;
                        break;

                    case '-':   // comment --- (not parsed by readnumber)
                        if (!ParseDashComment()) return;
                        break;

                    case '/': 
                        DeletedLine = true;
                        break;
                        
                    case 'N':
                        LineNumber = ParseInt();
                        break;
                    
                    case 'G':
                        ProcessCommand(new CommandG(this));    
                        break;

                    case 'T':
                        ProcessCommand(new CommandT(this));
                        break;

                    case 'F':
                        ProcessCommand(new CommandF(this));
                        break;

                    case 'M':
                        ProcessCommand(new CommandM(this));
                        break;

                    case 'S':
                        ProcessCommand(new CommandS(this));
                        break;

                    case 'D':
                        ProcessCommand(new CommandD(this));
                        break;

                    case 'X':  // Primary axes
                    case 'Y': 
                    case 'Z':
                    case 'I':  
                    case 'J':
                    case 'K':
                    case 'R':
                        ProcessCommand(new CommandAxis(c, this));
                        break;

                    default:
                        Logger.Log("ParseCommand: Unknown state at {0}: '{1}'", CurrentIndex, c);
                        ReadChar();
                        break;
                }
            }
        }

        private void ProcessCommand(BaseCommand cmd)
        {
            cmd.Deleted = DeletedLine;
            cmd.LineNumber = LineNumber;
            cmd.Parse();
        }
    }
}
