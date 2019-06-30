using System;


namespace ProfERP.Netduino.GCodeParser
{
    // Set Spindle Speed
    internal class CommandS: BaseCommand
    {
        public CommandS(GCodeParser _parser) : base (_parser)
        {
        }

        public int SpindleSpeed = -1;

        public override void Parse()
        {
            SpindleSpeed = parser.ParseInt();

            Logger.Log("S: SetSpindleSpeed: {0}", SpindleSpeed);
        }
    }

}
