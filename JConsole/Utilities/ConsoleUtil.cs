using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JConsole
{
    public static class ConsoleUtil
    {
        public static void StartProcess(string filePath)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = filePath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        public static Dictionary<string, object> GetEditInput<T>(T item, List<PropertyInfo> props = null)
        {
            Dictionary<string, object> enteredValues = new Dictionary<string, object>();

            if (props == null)
                props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            AnsiConsole.MarkupLine("Press [green]ENTER[/] to skip editing the current field.\n");

            foreach (PropertyInfo prop in props)
            {
                if (item != null)
                {
                    object currentValue = prop.GetValue(item);
                    AnsiConsole.WriteLine(string.Format("Current {0}: {1}", prop.Name, currentValue));
                }

                string input = GetInput(string.Format("{0}:", prop.Name));

                bool isNumeric = int.TryParse(input, out int numValue);

                if (isNumeric && prop.PropertyType == typeof(string)
                    || !isNumeric && prop.PropertyType == typeof(int) && input != string.Empty)
                {
                    string message = prop.PropertyType == typeof(string) ? "Input cannot be a number" : "Input must be a number";
                    AnsiConsole.WriteLine(message);
                    continue;
                };

                if (isNumeric && numValue < 0)
                {
                    AnsiConsole.WriteLine("Number must be greater than 0");
                    continue;
                }

                object value = isNumeric ? numValue : !string.IsNullOrEmpty(input) ? input : null;

                if (value != null)
                    enteredValues.Add(prop.Name, value);
            }

            return enteredValues;
        }

        public static string GetInput(string message, Func<string, bool> validator = null, string errorMessage = null)
        {
            PromptSettings settings = new PromptSettings();
            settings.Prompt = message;
            settings.Validator = validator;
            settings.ValidationErrorMessage = errorMessage;
            return GetInput(settings);
        }

        public static string GetInput(PromptSettings settings)
        {
            string value = null;

            if (settings == null)
                settings = new PromptSettings();

            TextPrompt<string> prompt = new TextPrompt<string>(settings.Prompt);
            prompt.ValidationErrorMessage = settings.ValidationErrorMessage ?? "\nInvalid Input.\n";
            prompt.AllowEmpty = true;

            if (settings.IsSecret)
                prompt.Secret();

            value = settings.Validator != null ? AnsiConsole.Prompt(prompt.Validate(settings.Validator))
                                      : AnsiConsole.Prompt(prompt);

            string command = value.ToUpper();

            if (command == GlobalConstants.Commands.MENU || command == GlobalConstants.Commands.EXIT || command == GlobalConstants.Commands.CANCEL)
                throw new Exception(command);

            return value.Trim();
        }

        public static T GetActionApprovalInput<T>(Func<T> function, string message = "Would you like to proceed?")
        {
            bool confirmed = false;
            T item = default;
            int iteration = 0;

            while (!confirmed)
            {
                iteration++;

                if (iteration % 2 == 0)
                    AnsiConsole.MarkupLine(string.Format("[red]Reminder[/]: You may enter [red bold]{0}[/] at any time to return to the menu\n", GlobalConstants.Commands.CANCEL));

                item = function();

                AnsiConsole.WriteLine();
                confirmed = GetConfirmation(message);

                AnsiConsole.WriteLine();
            }

            return item;
        }

        public static bool GetConfirmation(string message)
        {
            string input = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                    .Title(message)
                    .AddChoices("Yes", "No"));

            return input == "Yes";
        }
    }
}
