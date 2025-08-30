using System.IO;
using KompasAPI7;
using ProductComposition.Models.Interfaces;

namespace ProductComposition.Models;

public class ComponentModel : IEntity
{
	public int Id { get; set; }
	public string? ComponentDesignation { get; init; }

	public string ComponentName
	{
		get
		{
			if (ComponentDesignation is null)
				return string.Empty;
			return ComponentDesignation.Length > 0 ? Path.GetFileName(ComponentDesignation) : string.Empty;
		}
	}
	
	public bool IsDetail { get; }
	
	public bool IsStandart { get; }
	
	public string Material { get; }
	
	public string Density { get; }
	
	public string? FullFileName { get; init; }
	public string ShortFileName
	{
		get
		{
			if (FullFileName is null)
				return string.Empty;
			return FullFileName.Length > 0 ? Path.GetFileName(FullFileName) : string.Empty;
		}
	}

	public string Extension
	{
		get
		{
			if (FullFileName is null)
				return string.Empty;
			return FullFileName.Length > 0 ? Path.GetExtension(FullFileName) : string.Empty;
		}
	}
	
	public ComponentModel(IPart7 part)
	{
		Id = 0;
		ComponentDesignation = part.ToString();
		FullFileName = part.FileName;
		IsDetail = part.Detail;
		IsStandart = part.Standard;
	}

	public ComponentModel(string componentDesignation)
	{
		Id = 0;
		ComponentDesignation = componentDesignation;
	}
	
	public ComponentModel(string componentDesignation, string fullFileName)
	{
		Id = 0;
		ComponentDesignation = componentDesignation;
		FullFileName = fullFileName;
	}

	
	
	public ComponentModel() : this(string.Empty)
	{
	}

	public override string ToString()
	{
		if (ComponentName != null) return ComponentName;
		return string.Empty;
	}
}