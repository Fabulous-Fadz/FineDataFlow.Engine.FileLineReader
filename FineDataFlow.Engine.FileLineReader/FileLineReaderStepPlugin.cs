using FineDataFlow.Engine.Abstractions;
using System;
using System.IO;

namespace FineDataFlow.Engine.FileLineReader
{
	[StepPlugin]
	public class FileLineReaderStepPlugin
	{
		// fields

		private string _fullFilePath;
		private char[] _trimLeftCharacters;
		private char[] _trimRightCharacters;
		private string _outputFilePathVariable;
		private string _ouputLineNumberVariable;

		// properties

		[SuccessOutbox]
		public Action<Row> Success;

		[Parameter]
		public string FilePath;

		[Parameter]
		public bool IncludeEmptyLines;

		[Parameter]
		public string OutputLineVariable;

		[Parameter]
		public bool TrimLeft;

		[Parameter]
		public string TrimLeftCharacters;

		[Parameter]
		public bool TrimRight;

		[Parameter]
		public string TrimRightCharacters;

		[Parameter]
		public bool AddFilePathVariableToOutput;

		[Parameter]
		public string OutputFilePathVariable;

		[Parameter]
		public bool AddLineNumberVariableToOutput;

		[Parameter]
		public string OutputLineNumberVariable;

		// methods

		[Initialize]
		public void Initialize()
		{
			if (string.IsNullOrWhiteSpace(FilePath))
			{
				throw new InvalidOperationException($"{nameof(FilePath)} is required");
			}

			_fullFilePath = Path.GetFullPath(FilePath);

			if (!File.Exists(_fullFilePath))
			{
				throw new InvalidOperationException($"File '{_fullFilePath}' not found");
			}

			if (string.IsNullOrWhiteSpace(OutputLineVariable))
			{
				throw new InvalidOperationException($"{nameof(OutputLineVariable)} is required");
			}

			if (AddFilePathVariableToOutput)
			{
				if (string.IsNullOrWhiteSpace(OutputFilePathVariable))
				{
					throw new InvalidOperationException($"{nameof(OutputFilePathVariable)} is required when {nameof(AddFilePathVariableToOutput)} is true");
				}
				
				_outputFilePathVariable = OutputFilePathVariable.Trim();
			}

			if (AddLineNumberVariableToOutput)
			{
				if (string.IsNullOrWhiteSpace(OutputLineNumberVariable))
				{
					throw new InvalidOperationException($"{nameof(OutputLineNumberVariable)} is required when {nameof(AddLineNumberVariableToOutput)} is true");
				}

				_ouputLineNumberVariable = OutputFilePathVariable.Trim();
			}

			if (TrimLeft && !string.IsNullOrWhiteSpace(TrimLeftCharacters))
			{
				_trimLeftCharacters = TrimLeftCharacters.ToCharArray();
			}

			if (TrimRight && !string.IsNullOrWhiteSpace(TrimRightCharacters))
			{
				_trimRightCharacters = TrimRightCharacters.ToCharArray();
			}
		}

		[SeedInbox]
		public async void ReadFileAsync()
		{
			var lines = await File.ReadAllLinesAsync(_fullFilePath);
			
			for (long i = 0; i < lines.Length; i++)
			{
				var lineText = lines[i];
				
				if (!IncludeEmptyLines && string.IsNullOrWhiteSpace(lineText))
				{
					continue;
				}

				if (TrimLeft)
				{
					lineText = lineText.TrimStart();

					if (_trimLeftCharacters != null)
					{
						lineText = lineText.TrimStart(_trimLeftCharacters);
					}
				}

				if (TrimRight)
				{
					lineText = lineText.TrimEnd();

					if (_trimLeftCharacters != null)
					{
						lineText = lineText.TrimEnd(_trimRightCharacters);
					}
				}

				var row = new Row();

				row[OutputLineVariable] = lineText;

				if (AddFilePathVariableToOutput)
				{
					row[_outputFilePathVariable] = FilePath;
				}

				if (AddLineNumberVariableToOutput)
				{
					row[_ouputLineNumberVariable] = i + 1;
				}

				Success(row);
			}
		}
	}
}
