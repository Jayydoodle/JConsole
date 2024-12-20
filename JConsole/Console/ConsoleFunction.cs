﻿using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JConsole
{
    public abstract class ConsoleFunction : MenuOption
    {
        #region Base Class Overrides

        public abstract override string DisplayName { get; }
        public abstract string Documentation { get; }
        public override Action Function { get => Run; }

        #endregion

        #region Abstract API

        protected abstract List<MenuOption> GetMenuOptions();
        protected abstract bool Initialize();

        #endregion

        #region Private API

        private void Run()
        {
            bool initialized = false;

            try
            {
                initialized = Initialize();
            }
            catch (Exception)
            {
                throw;
            }

            if (initialized)
            {
                WriteHeaderToConsole();
                RunProgramLoop();
            }
        }

        private void RunProgramLoop()
        {
            while (true)
            {
                SelectionPrompt<MenuOption> prompt = new SelectionPrompt<MenuOption>();
                prompt.Title = "Select an option:";
                prompt.PageSize = 15;

                List<MenuOption> options = GetMenuOptions();

                if (options.Any(x => x.Function != null && x.Function.Method.HasAttribute<DocumentationAttribute>()))
                    options.Add(GetHelpOption());

                options.Add(new MenuOption(GlobalConstants.SelectionOptions.ReturnToMainMenu, () => throw new Exception(GlobalConstants.Commands.MENU)));

                prompt.AddChoices(options);

                MenuOption option = AnsiConsole.Prompt(prompt);

                if (option.Function != null || option.IsHelpOption)
                {
                    try
                    {
                        if (option.IsHelpOption)
                            ((MenuOption<List<MenuOption>>)option).Function(options);
                        else
                            option.Function();
                    }
                    catch (Exception e)
                    {
                        if (e.Message == GlobalConstants.Commands.CANCEL)
                            WriteHeaderToConsole();
                        else if (e.Message == GlobalConstants.Commands.MENU)
                            break;
                        else
                            e.LogException();
                    }
                }
                else
                {
                    break;
                }

                AnsiConsole.Write("\n\n");
            }
        }

        protected virtual void WriteHeaderToConsole()
        {
            AnsiConsole.Clear();

            Rule rule = new Rule(string.Format("[red]{0}[/]", DisplayName)).LeftJustified();
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine(string.Format("Enter [bold red]{0}[/] at any time to return to the main menu.", GlobalConstants.Commands.MENU));
            AnsiConsole.MarkupLine(string.Format("Enter [bold red]{0}[/] at any time to end the current operation and return to the {1} menu.", GlobalConstants.Commands.CANCEL, DisplayName));
            AnsiConsole.MarkupLine(string.Format("Enter [bold red]{0}[/] at any time to quit.", GlobalConstants.Commands.EXIT));
            AnsiConsole.Write("\n\n");
        }

        private static void PrintHelpText(List<MenuOption> options)
        {
            Rule rule = new Rule("[pink1]Help[/]");
            rule.RuleStyle("blue");
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine();

            options.ForEach(x =>
            {
                if(x is ConsoleFunction function)
                {
                    AnsiConsole.MarkupLine("[green]{0}[/]", function.DisplayName);
                    AnsiConsole.MarkupLine(function.Documentation);
                    AnsiConsole.WriteLine();
                }
                else if (x.Function != null && x.Function.Method.HasAttribute<DocumentationAttribute>())
                {
                    AnsiConsole.MarkupLine("[green]{0}[/]", x.DisplayName);

                    DocumentationAttribute attr = x.Function.Method.GetCustomAttribute(typeof(DocumentationAttribute)) as DocumentationAttribute;
                    AnsiConsole.MarkupLine(attr.Summary);
                    AnsiConsole.WriteLine();
                }
            });

            rule = new Rule();
            rule.RuleStyle("blue");
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine();
        }

        #endregion

        #region Public API

        public static MenuOption GetHelpOption()
        {
            return new MenuOption<List<MenuOption>>(GlobalConstants.SelectionOptions.Help, PrintHelpText);
        }

        #endregion
    }
}
