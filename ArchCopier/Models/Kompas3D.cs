using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using KAPITypes;
using Kompas6API3D5COM;
using Kompas6API5;
using Kompas6Constants;
using KompasAPI7;
using Serilog;
using ArchCopier.Infrastructure.Utilities;
using IEntity = ArchCopier.Models.Interfaces.IEntity;

namespace ArchCopier.Models;

public enum HideMessageEnum { Show,  AlwaysYes, AlwaysNo}
public enum DocType { Drawing, Part, Assembly, Spec, Fragment, Textual, Unknown }

public class Kompas3D : IEntity
{
	public int Id { get; set; }
	private KompasObject? KsObject { get; set; }
	private IApplication? _kompas7 { get; set; }
	private ksDocument2D? _document2D;
	private ksDocument3D? _document3D;
	private KompasDocument _activeDocument;
	private DocType _activeDocumentType;
	private IKompasDocument3D _partOrAssembly;
	private bool _visibility;
	private HideMessageEnum _hideMassegeMode;
	private HideMessageEnum _hideMassageMode;
	private ILogger _logger;

	public Kompas3D(ILogger logger)
	{
		_logger = logger;
		
	}

	public bool Visibility
	{
		set
		{
			if (_kompas7 is null)
				return;
			_visibility = value;
			if (KsObject != null) KsObject.Visible = _visibility;
		}
	}
	
	public HideMessageEnum HideMassageMode
	{
		set
		{
			if (_kompas7 is null)
				return;
			_hideMassageMode = value;
			if (KsObject != null) _kompas7.HideMessage = value switch
			{
				HideMessageEnum.Show => ksHideMessageEnum.ksShowMessage,
				HideMessageEnum.AlwaysNo => ksHideMessageEnum.ksHideMessageNo,
				HideMessageEnum.AlwaysYes => ksHideMessageEnum.ksHideMessageYes,
				_ => ksHideMessageEnum.ksShowMessage
			};
		}
	}
	public string ActiveDocumentFullName
	{
		get
		{
			var document = (KompasDocument)_kompas7?.ActiveDocument;
			if (document is null)
				return string.Empty;
			return document.PathName;
		}
	}

	internal byte LoadKompas(bool visibility)
	{
		if (KsObject is null)
		{
			var kType = Type.GetTypeFromProgID("KOMPAS.Application.5");
			KsObject = (KompasObject)Activator.CreateInstance(kType ?? throw new InvalidOperationException())!;
		}

		if (KsObject == null) return 0;  //если Компас не запустился, возвращаем 0
		Visibility = visibility;
		KsObject.Visible = visibility;
		KsObject.ActivateControllerAPI();
		_logger.Information("Компас запустился");
		_kompas7 = KsObject.ksGetApplication7() as IApplication;
		if (_kompas7 != null) _kompas7.HideMessage = ksHideMessageEnum.ksHideMessageYes;
		return 2; //елси запустился возвращаем 2
	}

	internal byte TryGetActiveKompas()
	{
		const string progId = "KOMPAS.Application.5";
		KsObject = (KompasObject)Marshal2.GetActiveObject(progId, out var kompasResult)!;
		switch (kompasResult)
		{
			case 2: //Компас запущен
				KsObject.Visible = true;
				KsObject.ActivateControllerAPI();
				_kompas7 = KsObject.ksGetApplication7() as IApplication;
				if (_kompas7 != null) _activeDocument = (KompasDocument)_kompas7.ActiveDocument;
				AssignActiveDocumentType();
				_logger.Information("Компас уже запущен, связь с Компасом установлена");
				return 2;
				break;
			case 1: //Компас установлен, но не запущен
				_logger.Information("Компас установлен, но не запущен");
				return 1;
				break;
			default: //Компас не установлен
			{
				_logger.Information("Компас не установлен, или его установленный экземпляр не найден");
				return 0;
			}
		}
	}

	public ObservableCollection<ComponentModel> GetAllPartsOfActiveAssembly()
	{
		GetActive3DDocument();
		IPart7 part = _partOrAssembly.TopPart;
		List<IPart7> parts = new List<IPart7>();
		GetAllComponentsByRecursion(part, parts);
		var originalsParts = parts.GroupBy(p => p.FileName).Where(p => p.Count() == 1).Select(p => p.First()).ToList();
		return ConvertIPartListToNormalStringCollection(originalsParts);
	}

	public int GetActive3DDocument()
	{
		AssignActiveDocumentType();
		if(_kompas7 is null)
			return 0;
		_partOrAssembly = (IKompasDocument3D)_kompas7.ActiveDocument;
		switch (_activeDocumentType)
		{
			case DocType.Part:
				return 2;
			case DocType.Assembly:
				return 3;
		}
		return 1;
	}
	
	public void GetAllComponentsByRecursion(IPart7 part, List<IPart7> parts)
	{
		parts.Add(part);
		foreach (IPart7 item in part.Parts)
		{
			if(item.Detail == true) parts.Add(item);
			if(item.Detail == false) GetAllComponentsByRecursion(item, parts);
		}
	}
	public void OpenComponent(string fullFileNameOfComponent)
	{
		IOpenDocumentParam openDocumentParam = null;
		if (KsObject is not null)
		{
			_document3D = (ksDocument3D)KsObject.Document3D();
		}
		if(string.IsNullOrEmpty(fullFileNameOfComponent))
			return;
		if (!File.Exists(fullFileNameOfComponent))
			throw new FileNotFoundException($"There is no such a file here");
		if (!fullFileNameOfComponent.ToLower().EndsWith("a3d"))
			throw new ArgumentException("File is not a assembly");
		
		_document3D?.Open(fullFileNameOfComponent, false);
	}
	
	private void AssignActiveDocumentType()
	{
		KompasDocument activeDocument = null;
		if(_kompas7 is not null)
		{
			activeDocument = (KompasDocument)_kompas7.ActiveDocument;
		}
		if(activeDocument is null)
			return;
		switch (activeDocument.DocumentType)
		{
			case DocumentTypeEnum.ksDocumentDrawing:
				_activeDocumentType = DocType.Drawing;
				break;
			case DocumentTypeEnum.ksDocumentAssembly:
				_activeDocumentType = DocType.Assembly;
				break;
			case DocumentTypeEnum.ksDocumentPart:
				_activeDocumentType = DocType.Part;
				break;
			case DocumentTypeEnum.ksDocumentSpecification:
				_activeDocumentType = DocType.Spec;
				break;
			case DocumentTypeEnum.ksDocumentFragment:
				_activeDocumentType = DocType.Fragment;
				break;
			case DocumentTypeEnum.ksDocumentTextual:
				_activeDocumentType = DocType.Textual;
				break;
			case DocumentTypeEnum.ksDocumentUnknown:
				_activeDocumentType = DocType.Unknown;
				break;
			case DocumentTypeEnum.ksDocumentTechnologyAssembly:
				_activeDocumentType = DocType.Unknown;
				break;
			default:
				_activeDocumentType = DocType.Unknown;
				break;
		}
	}

	private ObservableCollection<ComponentModel> ConvertIPartListToNormalStringCollection(List<IPart7> innerList)
	{
		var result = new ObservableCollection<ComponentModel>();
		foreach (var p in innerList)
			result.Add(new ComponentModel(p));
		return result;
	}

}