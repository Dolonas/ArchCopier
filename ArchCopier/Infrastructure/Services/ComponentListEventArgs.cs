using System;

namespace ArchCopier.Infrastructure.Services;

public class ComponentListEventArgs : EventArgs
{
	public bool IsFileListGot { get; }

	public ComponentListEventArgs(bool isFileListGot)
	{
		IsFileListGot = isFileListGot;
	}
}