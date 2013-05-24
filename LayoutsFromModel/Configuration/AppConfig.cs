/*
 * User: aleksey.nakoryakov
 * Date: 08/01/2012
 * Time: 13:58
 */
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml.Serialization;

namespace LayoutsFromModel.Configuration
{
	/// <summary>
	/// Класс, представляющий конфигурацию приложения
	/// </summary>
	[Serializable]
	public sealed class AppConfig
	{
		string prefix = "Lay";
		/// <summary>
		/// Префикс имени Layout
		/// </summary>
		public string Prefix {
			get { return prefix; }
			set { prefix = value; }
		}
		
		string suffix = "";
		/// <summary>
		/// Суффикс имени Layout
		/// </summary>
		public string Suffix {
			get { return suffix; }
			set { suffix = value; }
		}
		
		int precision = 10;
		/// <summary>
		/// Точность определения формата бумаги
		/// </summary>
		public int Precision {
			get { return precision; }
			set { precision = value; }
		}
		
		bool deleteNonInitializedLayouts = false;
		/// <summary>
		/// Удалять ли неинициализированные листы
		/// </summary>
		public bool DeleteNonInitializedLayouts {
			get { return deleteNonInitializedLayouts; }
			set { deleteNonInitializedLayouts = value; }
		}
		
		double referenceDimension = 185.0;
		/// <summary>
		/// Эталонный размер. Используется для определения масштаба чертежа при
		/// выборе пользователем. По-умолчанию - длина основной надписи
		/// </summary>
		public double ReferenceDimension {
			get { return referenceDimension; }
			set { referenceDimension = value; }
		}
		
		bool tilemodeOn = true;
		/// <summary>
		/// Возвращаться ли в модель по окончанию созданию листов
		/// </summary>
		public bool TilemodeOn {
			get { return tilemodeOn; }
			set { tilemodeOn = value; }
		}
		
		string templatePath = "";
		/// <summary>
		/// Путь к шаблону с именованными настройками печати
		/// </summary>
		public string TemplatePath {
			get { return templatePath; }
			set { templatePath = value; }
		}
		
		string blockName = "";
		/// <summary>
		/// Имя блока рамки
		/// </summary>
		public string BlockName {
			get { return blockName; }
			set { blockName = value; }
		}
		
		const string FILENAME = "lfmsettings.xml"; // Имя файла конфигурации
		// Полный путь к файлу конфигурации
		private static string SettingsFile
		{
			get
			{
				return Path.Combine(Path.GetDirectoryName(
					Assembly.GetAssembly(typeof(AppConfig))
					.Location), FILENAME);
			}
		}
		
		
		private static AppConfig instance = Load();
		
		public static AppConfig Instance {
			get { return instance; }
		}
		
		private AppConfig()
		{}
		
		/// <summary>
		/// Сохранение конфигурации в файл
		/// </summary>
		public void Save()
		{
			using (Stream stream = File.Create(SettingsFile))
			{
				XmlSerializer ser = new XmlSerializer(this.GetType());
				ser.Serialize(stream, this);
			}
		}
		
		/// <summary>
		/// Загрузка конфигурации из файла
		/// </summary>
		/// <returns></returns>
		private static AppConfig Load()
		{
			if (!File.Exists(SettingsFile))
				return new AppConfig();
			using (Stream stream = File.OpenRead(SettingsFile))
			{
				try
				{
					XmlSerializer ser = new XmlSerializer(typeof(AppConfig));
					return (AppConfig)ser.Deserialize(stream);
				}
				catch (InvalidOperationException)
				{
					stream.Close();
					File.Delete(SettingsFile);
					return new AppConfig();
				}
			}
		}
		
		/// <summary>
		/// Метод вызывает диалог конфигурации и изменяет настройки в зависимости от результата вызова
		/// </summary>
		public void ShowDialog()
		{
			ConfigurationDialog win = new ConfigurationDialog(Prefix, Suffix,
			                                                  Precision,DeleteNonInitializedLayouts,
			                                                  ReferenceDimension,
			                                                  TilemodeOn,
			                                                  BlockName);
			win.ShowDialog();
			if (true == win.DialogResult)
			{
				this.Prefix = win.Prefix;
				this.Suffix = win.Suffix;
				this.Precision = win.Precision??new AppConfig().Precision;
				this.DeleteNonInitializedLayouts = win.DelNonInitializedLayouts;
				this.TilemodeOn = win.TilemodeOn;
				this.BlockName = win.BlockName;
				Save();
			}
		}
		
		public bool TemplateExists()
		{
			return !string.IsNullOrEmpty(this.TemplatePath)&&File.Exists(this.TemplatePath);
		}
		
		public override string ToString()
		{
			return string.Format(
				"[Configuration Prefix={0}, Suffix={1}, Precision={2}, DeleteNonInitializedLayouts={3}, ReferenceDimension={4}, TilemodeOn={5}]",
				Prefix, Suffix, Precision, DeleteNonInitializedLayouts, ReferenceDimension, TilemodeOn);
		}
	}
}