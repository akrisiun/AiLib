using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace Ai.Wpf.Models
{
    public static class CloneFactory
    {
        public static CheckBox Clone(CheckBox box1, string name, Func<string> content)
        {
            var clone = new CheckBox();
            // x:Name="Check1" Height="30" Margin="15,20,15,0" VerticalAlignment="Center" FontSize="18">
            clone.Name = Path.GetFileNameWithoutExtension(name);
            
            clone.Height = box1.Height;
            clone.Margin = box1.Margin;
            clone.VerticalAlignment = box1.VerticalAlignment;
            clone.FontSize = box1.FontSize;

            clone.Content = content();
            clone.Visibility = System.Windows.Visibility.Visible;
            return clone;
        }
    
    }

}
