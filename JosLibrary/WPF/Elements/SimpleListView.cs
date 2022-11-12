using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace JosLibrary.WPF.Elements
{
    public class SimpleListView : ListBox
    {
        protected override void OnMouseMove(MouseEventArgs e)
        {
            e.Handled = true;
            base.OnMouseMove(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            e.Handled = true;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            e.Handled = true;
            base.OnMouseLeave(e);
        }

    }
}
