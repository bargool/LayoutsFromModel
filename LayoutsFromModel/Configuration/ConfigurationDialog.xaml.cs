/*
 * User: aleksey.nakoryakov
 * Date: 01.08.12
 * Time: 13:37
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LayoutsFromModel.Configuration
{
	/// <summary>
	/// Interaction logic for ConfigurationDialog.xaml
	/// </summary>
	public partial class ConfigurationDialog : Window
	{
		//TODO: Напрямую связать конфигурацию и поля ввода окна
		public string Prefix { get; private set; }
		public string Suffix { get; private set; }
		public int? Precision { get; set; }
		public bool DelNonInitializedLayouts { get; set; }
		
		public double? ReferenceDimension { get; set; }
		public bool TilemodeOn { get; set; }
		
		public string BlockName { get; set; }
		public bool LockViewPorts { get; set; }
		
		public ConfigurationDialog()
		{
			InitializeComponent();
		}
		
		public ConfigurationDialog
			(string prefix, string suffix, Nullable<int> precision,
			 bool delNonInitializedLayouts, double referenceDimension,
			 bool tilemodeOn, string blockname, bool lockViewPorts)
			:this()
		{
			this.Prefix = prefix;
			this.Suffix = suffix;
			this.Precision = precision;
			this.DelNonInitializedLayouts = delNonInitializedLayouts;
			this.ReferenceDimension = referenceDimension;
			this.TilemodeOn = tilemodeOn;
			this.BlockName = blockname;
			this.LockViewPorts = lockViewPorts;
		}
		
		void Window_Loaded(object sender, RoutedEventArgs e)
		{
			txtPrefix.Text = this.Prefix;
			txtSuffix.Text = this.Suffix;
			txtPrecision.Text = this.Precision.ToString();
			chkDeleteNonInitialized.IsChecked = this.DelNonInitializedLayouts;
			chkTileModeOn.IsChecked = this.TilemodeOn;
			txtBlockName.Text = this.BlockName;
			chkLockVP.IsChecked = this.LockViewPorts;
		}
		
		void BtnOK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
		
		void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Prefix = txtPrefix.Text;
			Suffix = txtSuffix.Text;
			int precision;
			Precision = int.TryParse(txtPrecision.Text, out precision) ? (int?)Math.Abs(precision) : null;
			DelNonInitializedLayouts = chkDeleteNonInitialized.IsChecked??false;
			this.TilemodeOn = chkTileModeOn.IsChecked??false;
			this.BlockName = txtBlockName.Text;
			this.LockViewPorts = chkLockVP.IsChecked ?? false;
		}
	}
}