using System;

#nullable enable

namespace FolderWrapper;

public class FolderWrapperException : Exception {
	public FolderWrapperException() : base() { }

	public FolderWrapperException(string? message = null) : base(message) { }
}