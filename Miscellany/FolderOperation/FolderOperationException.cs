using System;

namespace FolderOperation;

public class FolderOperationException : Exception {
	public FolderOperationException() : base() { }

	public FolderOperationException(string? message = null) : base(message) { }
}