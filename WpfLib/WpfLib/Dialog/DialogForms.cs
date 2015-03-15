using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace Ai.Util
{
    // Error Dialog for Exceptions
    public class DialogError : IDialogError
    {
        Exception IDialogError.Exception { get; set; }

        public DialogError(Exception ex, string caption)
        {
            SqlConfig.Instance.OnExceptionLog(null, caption, ex, true);
        }

        public bool? ShowDialog(Exception exp, string caption)
        {
            return DialogError.Show(exp, caption) == MessageBoxResult.OK;
        }

        public static MessageBoxResult Show(Exception ex, string caption)
        {
            return Show(ex, "", caption);
        }

        public static MessageBoxResult Show(Exception ex, string topline, string caption)
        {
            SqlConfig.Instance.OnExceptionLog(null, topline + " " + caption, ex, true);
            return MessageBox.Show(topline + "\n" + ex.Message, caption);
        }

    }

    // MessageBox helper class
    public static class Dialog
    {
        public static void ShowError(string msg)
        {
            SqlConfig.Instance.OnExceptionLog(null, msg, null, false);
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowError(string msg, bool reconnect)
        {
            if (reconnect)
                SqlConfig.Instance.OnExceptionLog(null, msg, null, true);
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static string MainWindowTitle
        {
            get
            {
                Process process = Process.GetCurrentProcess();
                return process.MainWindowTitle;
            }
        }

        public static void ShowMsg(string msg)
        {
            MessageBox.Show(msg, MainWindowTitle);
        }

        public static bool YesNo(string msg)
        {
            MessageBoxResult ret = MessageBox.Show(msg, MainWindowTitle, MessageBoxButton.YesNo);
            return ret == MessageBoxResult.Yes;
        }

        /// <summary>
        /// true for Yes, false for No, null for Cancel
        /// </summary>
        public static bool? YesNoCancel(string msg)
        {
            MessageBoxResult ret = MessageBox.Show(msg, MainWindowTitle,
                         MessageBoxButton.YesNoCancel);

            if (ret == MessageBoxResult.Cancel) return null;
            return ret == MessageBoxResult.Yes;
        }

    }

    public interface IDialogError
    {
        Exception Exception { get; set; }
        bool? ShowDialog(Exception exp, string caption);
    }

}
