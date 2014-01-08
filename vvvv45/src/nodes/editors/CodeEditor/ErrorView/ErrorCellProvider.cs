﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

using VVVV.Core;
using VVVV.Core.Model;
using VVVV.Core.View.Table;

namespace VVVV.HDE.CodeEditor.ErrorView
{
	/// <summary>
	/// Describes a compiler error.
	/// </summary>
	public class ErrorCellProvider : IEnumerable<ICell>
	{
		protected CompilerError FError;
		
		public ErrorCellProvider(CompilerError error)
		{
			FError = error;
		}
		
		public IEnumerator<ICell> GetEnumerator()
		{
		    yield return new Cell(FError.IsWarning ? "W" : "E", typeof(string));
			yield return new Cell(FError.Line, typeof(int));
			yield return new Cell(FError.ErrorText, typeof(string), true);
            yield return new Cell(FError.ErrorNumber, typeof(string));
			if (FError.FileName != null && FError.FileName.Length > 0)
			{
				yield return new Cell(Path.GetFileName(FError.FileName), typeof(string));
				yield return new Cell(Path.GetDirectoryName(FError.FileName), typeof(string));
			}
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
