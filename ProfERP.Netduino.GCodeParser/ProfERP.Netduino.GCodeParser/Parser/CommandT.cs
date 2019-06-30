using System;


namespace ProfERP.Netduino.GCodeParser
{
    internal class CommandT: BaseCommand
    {
        public CommandT(GCodeParser _parser) : base (_parser)
        {
        }

        public int ToolIndex = -1;

        public override void Parse()
        {
            ToolIndex = parser.ParseInt();

            Logger.Log("T: Select tool: {0}", ToolIndex);
        }
    }
}
