using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JConsole.Console
{
    public class ConsoleProgram
    {
        #region Properties

        public string ApplicationName { get; set; }
        public string VersionNumber { get; set; }
        public List<MenuOption> MenuOptions { get; set; }

        #endregion

        #region Public API

        public void Run()
        {
            SelectionPrompt<MenuOption> prompt = new SelectionPrompt<MenuOption>();
            prompt.Title = "Select an option:";
            prompt.AddChoices(MenuOptions);

            bool printMenuHeading = true;

            while (true)
            {
                if (printMenuHeading)
                    PrintMenuHeading();

                MenuOption option = AnsiConsole.Prompt(prompt);

                if (option.Function != null || option.IsHelpOption)
                {
                    try
                    {
                        printMenuHeading = true;

                        if (option.IsHelpOption)
                        {
                            printMenuHeading = false;
                            AnsiConsole.Clear();
                            ((MenuOption<List<MenuOption>>)option).Function(MenuOptions);
                        }
                        else
                        {
                            option.Function();
                            AnsiConsole.Clear();
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.Message == GlobalConstants.Commands.EXIT)
                            break;
                        else

                            AnsiConsole.Clear();

                        if (e.Message != GlobalConstants.Commands.MENU)
                            AnsiConsole.Write(string.Format("{0}\n\n", e.Message));
                    }
                }
                else
                {
                    break;
                }
            }
        }

        #endregion

        #region Private API

        private void PrintMenuHeading()
        {
            Rule rule = new Rule(string.Format("[green]{0} v{1}[/]\n", ApplicationName, VersionNumber)).DoubleBorder<Rule>();
            AnsiConsole.Write(rule);
        }

        #endregion
    }
}
