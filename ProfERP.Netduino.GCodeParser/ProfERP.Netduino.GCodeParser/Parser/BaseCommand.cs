using System;
using System.Text;

namespace ProfERP.Netduino.GCodeParser
{
    public class BaseCommand
    {
        public GCodeParser parser;

        public bool Deleted = false;
        public int LineNumber = -1;

        public BaseCommand(GCodeParser _parser)
        {
            parser = _parser;
        }

        public virtual void Parse()
        { 
            
        }

        public virtual void Run()
        { 
            
        }
    }
}
