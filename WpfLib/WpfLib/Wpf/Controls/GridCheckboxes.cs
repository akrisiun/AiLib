using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ai.Wpf.Controls
{
    class GridCheckboxes
    {
    }

}

//My XAML is as follows:

//<UserControl.Resources>
//    <Style x:Key="itemstyle" TargetType="{x:Type DataGridRow}">
//        <Style.Resources>
//            <SolidColorBrush x:Key="{x:Static SystemColors.HighlightBrushKey}" Color="LightGoldenrodYellow" />
//            <SolidColorBrush x:Key="{x:Static SystemColors.ControlBrushKey}" Color="Transparent" />
//            <SolidColorBrush x:Key="{x:Static SystemColors.HighlightTextBrushKey}" Color="Black" />
//            <SolidColorBrush x:Key="{x:Static SystemColors.ControlTextBrushKey}" Color="Black" />
//        </Style.Resources>
//        <Setter Property="HorizontalContentAlignment" Value="Stretch" />
//        <Setter Property="IsSelected" Value="{Binding Path=IsSelected, Mode=TwoWay}" />
//        <Style.Triggers>
//            <MultiTrigger>
//                <MultiTrigger.Conditions>
//                    <Condition Property="ItemsControl.AlternationIndex" Value="1" />
//                    <Condition Property="IsSelected" Value="False" />
//                    <Condition Property="IsMouseOver" Value="False" />
//                </MultiTrigger.Conditions>
//                <Setter Property="Background" Value="#EEEEEEEE" />
//            </MultiTrigger>
//        </Style.Triggers>
//    </Style>
//</UserControl.Resources>

//<Grid Width="500" Height ="300">
//    <DataGrid ItemsSource="{Binding Path=Script}" HeadersVisibility="Column" SelectionMode="Single" AlternatingRowBackground="Gainsboro" Background="White" AutoGenerateColumns="False" ItemContainerStyle="{StaticResource itemstyle}" CanUserAddRows="True" GridLinesVisibility="None" Height="242" HorizontalAlignment="Left" HorizontalContentAlignment="Left"  IsEnabled="True" IsReadOnly="True"   Margin="10,14,0,44" Name="dgMain" RowHeight="23" VerticalAlignment="Center" VerticalContentAlignment="Center"  Width="478" >
//        <i:Interaction.Triggers>
//            <i:EventTrigger EventName="MouseDoubleClick">
//                <i:InvokeCommandAction Command="{Binding EditData}"/>
//            </i:EventTrigger>
//        </i:Interaction.Triggers>
//        <DataGrid.Columns>
//            <DataGridCheckBoxColumn Binding="{Binding Path=IsSelected}" Header="Select" Width="50" />
//            <DataGridTextColumn Binding="{Binding Path=Script_Text}" Header="Script" Width="400" />
//        </DataGrid.Columns>
//    </DataGrid>
//</Grid>

