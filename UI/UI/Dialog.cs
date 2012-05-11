
namespace ExceptionExplorer.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using ExceptionExplorer.Config;
    using System.Reflection;

    public static class Dialog
    {
        public static TaskDialogResult Show(IWin32Window owner, string mainInstruction, TaskDialogStandardButtons buttons, TaskDialogStandardIcon icon)
        {
            return Show(owner, mainInstruction, null, null, buttons, icon);
        }
        public static TaskDialogResult Show(IWin32Window owner, string mainInstruction, string contentText, TaskDialogStandardButtons buttons, TaskDialogStandardIcon icon)
        {
            return Show(owner, mainInstruction, contentText, null, buttons, icon);
        }
        public static TaskDialogResult Show(IWin32Window owner, string mainInstruction, string contentText, string details, TaskDialogStandardButtons buttons, TaskDialogStandardIcon icon)
        {
            TaskDialog td = new TaskDialog()
            {
                InstructionText = mainInstruction,
                Text = contentText,
                DetailsExpandedText = details,
                Caption = "Exception Explorer",
                StandardButtons = buttons,
                Icon = icon
            };

            return td.Show(owner);
        }

        public static TaskDialogResult Show(this TaskDialog td, IWin32Window owner)
        {
            if (owner != null)
            {
                return td.Show(owner.Handle);
            }

            return td.Show(IntPtr.Zero);
        }

        public static TaskDialogResult Show(this TaskDialog td, IntPtr ownerHandle)
        {
            td.OwnerWindowHandle = ownerHandle;
            td.Fix();
            return td.Show();
        }

        public static void Fix(this TaskDialog td)
        {
            td.Opened += new EventHandler(TaskDialog_Opened);
        }

        private static T GetFieldValue<T>(this object obj, string name)
        {
            FieldInfo fi = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fi != null)
            {
                object value = fi.GetValue(obj);

                if (value is T)
                {
                    return (T)value;
                }
            }

            return default(T);
        }

        private static object GetNativeTaskDialog(this TaskDialog td)
        {
            return td.GetFieldValue<object>("nativeDialog");
        }

        public static IntPtr GetHandle(this TaskDialog td)
        {
            object nativeTaskDialog = td.GetNativeTaskDialog();
            return nativeTaskDialog.GetFieldValue<IntPtr>("hWndDialog");
        }

        private static void TaskDialog_Opened(object sender, EventArgs e)
        {
            TaskDialog td = (TaskDialog)sender;

            if (td.Icon != TaskDialogStandardIcon.None)
            {
                td.Icon = td.Icon;
            }

            if (td.FooterIcon != TaskDialogStandardIcon.None)
            {
                td.FooterIcon = td.FooterIcon;
            }
        }



    }
}
