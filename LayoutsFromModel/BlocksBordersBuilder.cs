/*
 * User: aleksey.nakoryakov
 * Date: 13.05.13
 * Time: 14:39
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

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
			
			string blockname = GetBordersBlockName();
			
			using (Transaction tr = _wdb.TransactionManager.StartTransaction())
			{
				// Получаем коллекцию ObjectId вхождений блока blockname, затем сортируем
				// "построчно"
				IEnumerable<ObjectId> blockRefIds = null;
				BlockTable bt = (BlockTable)tr.GetObject(_wdb.BlockTableId, OpenMode.ForRead);
				if (!bt.Has(blockname))
					throw new ArgumentException("blockname");
				ObjectId btrId = bt[blockname];
				
				Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
				PromptSelectionResult res = ed.SelectImplied();
				if (res.Status == PromptStatus.OK)
				{
					blockRefIds = res.Value
						.GetObjectIds()
						.Where(id => id.ObjectClass.Name == "AcDbBlockReference")
						.Where(id => ((BlockReference)tr.GetObject(id, OpenMode.ForRead)).DynamicBlockTableRecord == btrId);
				}
				else
				{
					blockRefIds = GetBlockReferences(btrId);
				}
				
				blockRefIds = blockRefIds
					.Select(n => (BlockReference)tr.GetObject(n, OpenMode.ForRead))
					.OrderByDescending(n => n.Position.Y)
					.ThenBy(n => n.Position.X)
					.Select(n => n.ObjectId);
				
				int borderIndex = InitialBorderIndex;
				
				foreach (var brefId in blockRefIds)
				{
					string borderName = string.Format("{0}{1}{2}",
					                                  Configuration.AppConfig.Instance.Prefix,
					                                  borderIndex++,
					                                  Configuration.AppConfig.Instance.Suffix);
					borders.Add(CreateBorder(brefId, borderName));
				}
				tr.Commit();
			}
			
			return borders.ToArray();
		}
		
		private string GetBordersBlockName()
		{
			string blockname = Configuration.AppConfig.Instance.BlockName;
			if (string.IsNullOrEmpty(blockname))
				throw new System.Exception("Не задано имя блока рамки!");
			return blockname;
		}
		
		private List<ObjectId> GetBlockReferences(ObjectId blockId)
		{
			List<ObjectId> result = null;
			using (Transaction tr = _wdb.TransactionManager.StartTransaction())
			{
				BlockTableRecord btr = (BlockTableRecord)tr.GetObject(blockId, OpenMode.ForRead);

				BlockTable bt = (BlockTable)tr.GetObject(_wdb.BlockTableId, OpenMode.ForRead);				
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
