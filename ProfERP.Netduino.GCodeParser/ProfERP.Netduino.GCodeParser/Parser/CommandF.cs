using System;


namespace ProfERP.Netduino.GCodeParser
{
    // Set feed rate
    internal class CommandF : BaseCommand
    {
        public CommandF(GCodeParser _parser) : base (_parser)
        {
        }

        public override void Parse()
        {
            float feedRate = (float)parser.ParseDouble();

            //throw new NotImplementedException("DeviceFactory.Get().SetFeedRate(feedRate);");
        }
    }
}
