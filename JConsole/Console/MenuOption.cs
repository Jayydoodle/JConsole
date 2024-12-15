using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JConsole
{
    public class MenuOption
    {
        #region Properties

        public virtual string DisplayName { get; set; }
        public virtual Action Function { get; set; }

        public bool IsHelpOption { get { return DisplayName == GlobalConstants.SelectionOptions.Help; } }

        #endregion

        #region Constructor

        protected MenuOption()
        {
        }

        public MenuOption(string displayName, Action function)
        {
            DisplayName = displayName;
            Function = function;
        }

        #endregion

        #region Public API

        public static MenuOption CancelOption(string text = null)
        {
            return new MenuOption(text ?? GlobalConstants.SelectionOptions.Cancel, () => throw new Exception(GlobalConstants.Commands.CANCEL));
        }

        public override string ToString()
        {
            return DisplayName;
        }

        #endregion
    }

    public class MenuOption<T> : MenuOption
    {
        #region Properties

        public new Action<T> Function { get; set; }

        #endregion

        #region Constructor

        public MenuOption(string displayName, Action<T> function)
        {
            DisplayName = displayName;
            Function = function;
        }

        #endregion
    }

    public class SelectMenuOption<T> : MenuOption
    {
        #region Properties

        public new Func<T> Function { get; set; }

        #endregion

        #region Constructor

        public SelectMenuOption(string displayName, Func<T> function)
        {
            DisplayName = displayName;
            Function = function;
        }

        #endregion

        #region Public API

        public static new SelectMenuOption<T> CancelOption(string text = null)
        {
            return new SelectMenuOption<T>(text ?? GlobalConstants.SelectionOptions.Cancel, () => throw new Exception(GlobalConstants.Commands.CANCEL));
        }

        #endregion
    }
}
