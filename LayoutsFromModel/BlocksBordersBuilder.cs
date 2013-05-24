/*
 * User: aleksey.nakoryakov
 * Date: 13.05.13
 * Time: 14:39
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace LayoutsFromModel
{
	/// <summary>
	/// Класс, создающий коллекцию границ чертежей из вхождений блоков
	/// </summary>
	public class BlocksBordersBuilder : IBordersCollectionBuilder
	{
		private Database _wdb = HostApplicationServices.WorkingDatabase;
		
		public int InitialBorderIndex { get; set; }
		
		/// <summary>
		/// Получение границ из вхождений блоков
		/// </summary>
		/// <returns>Массив границ чертежей</returns>
		public DrawingBorders[] GetDrawingBorders()
		{
			List<DrawingBorders> borders = new List<DrawingBorders>();
			
			string blockname = Configuration.AppConfig.Instance.BlockName;
			if (string.IsNullOrEmpty(blockname))
				throw new System.Exception("Не задано имя блока рамки");
			
			using (Transaction tr = _wdb.TransactionManager.StartTransaction())
			{
				// Получаем коллекцию ObjectId вхождений блока blockname, затем сортируем
				// "построчно"
				var blockRefs = GetBlockReferences(blockname)
					.Select(n => (BlockReference)tr.GetObject(n, OpenMode.ForRead))
					.OrderByDescending(n => n.Position.Y)
					.ThenBy(n => n.Position.X)
					.Select(n => n.ObjectId);
				
				int borderIndex = InitialBorderIndex;
				
				foreach (var bref in blockRefs)
				{
					string borderName = string.Format("{0}{1}{2}",
					                                  Configuration.AppConfig.Instance.Prefix,
					                                  borderIndex++,
					                                  Configuration.AppConfig.Instance.Suffix);
					borders.Add(CreateBorder(bref, borderName));
				}
				tr.Commit();
			}
			
			return borders.ToArray();
		}
		
		private List<ObjectId> GetBlockReferences(string blockname)
		{
			List<ObjectId> result = null;
			using (Transaction tr = _wdb.TransactionManager.StartTransaction())
			{
				BlockTable bt = (BlockTable)tr.GetObject(_wdb.BlockTableId, OpenMode.ForRead);
				BlockTableRecord btr = null;
				if (bt.Has(blockname))
					btr = (BlockTableRecord)tr.GetObject(bt[blockname], OpenMode.ForRead);
				else
					throw new ArgumentException("blockname");
				ObjectId modelId = ((BlockTableRecord)tr
				                    .GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead)).ObjectId;
				result = btr.GetAllBlockReferenceIds(true)
					.Select(n => (BlockReference)tr.GetObject(n, OpenMode.ForRead))
					.Where(n => n.OwnerId == modelId)
					.Select(n => n.ObjectId)
					.ToList();
				tr.Commit();
			}
			return result;
		}
		
		/// <summary>
		/// Создание объекта границы чертежа
		/// Масштаб берётся из масштаба вхождения блока по оси X
		/// </summary>
		/// <param name="brefId">ObjectId вхождения блока рамки</param>
		/// <param name="name">Имя будущего листа</param>
		/// <returns>Объект границ чертежа</returns>
		private DrawingBorders CreateBorder(ObjectId brefId, string name)
		{
			DrawingBorders border = null;
			
			using (Transaction tr = _wdb.TransactionManager.StartTransaction())
			{
				BlockReference bref = (BlockReference)tr.GetObject(brefId, OpenMode.ForRead);
				double scale = bref.ScaleFactors.X;
				border = DrawingBorders.CreateDrawingBorders(bref.GeometricExtents.MinPoint,
				                                             bref.GeometricExtents.MaxPoint,
				                                             name,
				                                             scale);
				tr.Commit();
			}
			return border;
		}
	}
	
	public static class BlockTableRecordExtensions
	{
		public static IEnumerable<ObjectId> GetAllBlockReferenceIds(this BlockTableRecord btr, bool directOnly)
		{
			IEnumerable<ObjectId> brefIds = btr
				.GetBlockReferenceIds(directOnly, false)
				.Cast<ObjectId>()
				.Concat(
					btr.GetAnonymousBlockIds()
					.Cast<ObjectId>()
					.SelectMany(
						n => ((BlockTableRecord)n.GetObject(OpenMode.ForRead))
						.GetBlockReferenceIds(directOnly, false)
						.Cast<ObjectId>()));
			return brefIds;
		}
	}
}
