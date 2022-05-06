using System;

namespace FolderWrapper;

public class FolderWrapperException : Exception {
	public FolderWrapperException() : base() { }

	public FolderWrapperException(string message = null) : base(message) { }
}