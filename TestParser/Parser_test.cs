using System;
using CRMLogObjectifier.LogParser;
using System.Text.RegularExpressions;

namespace TestParser
{
    class Parser_test
    {
        public enum InputType
        {
            File,
            Directory
        }

        public static void Main(string[] args)
        {

            InputType inputType = InputType.File;
            Console.WriteLine("Enter path to log file: ");
            string filepath = Console.ReadLine();
            //string filepath = @"C:\Users\paulj\Downloads\CRM02-w3wp(15248#E3C634E2)-Help-20151208-1\CRM02-w3wp(15248#65568210)-CRMWeb-20151208-1.log";

            LogParser lp = null;
            // Determin if we are dealing with a directory of single file
            switch (inputType)
            {
                case InputType.File:
                    lp = new LogParser(filepath);
                    break;
                case InputType.Directory:
                    //dostuff
                    break;
                default:
                    break;
            }

            int input = 0;
            while (input != '3')
            {
                Console.WriteLine("Parsed {0} log entries", lp.LogList.Count);
                Console.WriteLine("1) Output all logs");
                Console.WriteLine("2) Output specific log");
                Console.WriteLine("3) Exit");

                int value = readInput();
               

                switch (value)
                {
                    case 1:
                        value = -1;
                        break;
                    case 2:
                        Console.WriteLine("Which log do you want output?");
                        value = readInput();
                        break;
                    case 3:
                        return;
                }

                // Output to console
                if (lp != null)
                    lp.outputLogs(value);

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                Console.Clear();
            }


            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            System.Console.ReadKey();
        }

        public static int readInput()
        {
            int value = 0;
            while (!Int32.TryParse(Console.ReadLine(), out value))
            {
                Console.WriteLine("Invalide input!");
            }
            return value;
        }
    }

}
