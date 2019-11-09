using System;

namespace sotsedit
{
    class Arguments
    {
        public enum Operation
        {
            Invalid,
            Change,
            Insert,
            Delete
        };

        public string files;
        public string key;
        public string details;
        public Operation operation;
    }

    class Program
    {
        static void Main(string[] args)
        {
            try{
                Arguments arguments;
                if (!parseArguments(args, out arguments))
                    return;
#if SOTS1
				s1edit.Processor.process(arguments);
#else
				s2edit.Processor.process(arguments);
#endif
			}
			catch (Exception e)
            {
                Version version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

                Console.WriteLine(e.Message + "\n\n Version: " + version + "\n\n" + "\n\n" + e.Source + "\n\n" + e.StackTrace);
                Console.WriteLine("Failed. The previous text will help diagnose the problem.");
                return;
            }
        }

        static bool showHelp()
        {
            Console.WriteLine("Usage:\n");
            Console.WriteLine("s1edit * FILES KEY MODIFICATION");
            Console.WriteLine("s1edit + FILES KEY VALUE");
            Console.WriteLine("s1edit - FILES KEY");
            Console.WriteLine("Example: s1edit * *anitmatter* shipsection.ftlspeed \"*= 1.5\"");
            return false;
        }

        static bool parseArguments(string[] args, out Arguments arguments)
        {
            arguments = new Arguments();
            if (args.Length < 3)
                return showHelp();

            switch (args[0])
            {
                case "*":
                    arguments.operation = Arguments.Operation.Change;
                    if (args.Length < 4)
                        return showHelp();
                    break;
                case "+":
                    arguments.operation = Arguments.Operation.Insert;
                    if (args.Length < 4)
                        return showHelp();
                    break;
                case "-":
                    arguments.operation = Arguments.Operation.Delete;
                    break;
                default:
                    return showHelp();
            }

            arguments.files = args[1];
            arguments.key = args[2];
            if (args.Length >= 4)
                arguments.details = args[3];
            return true;
        }
    }
}
