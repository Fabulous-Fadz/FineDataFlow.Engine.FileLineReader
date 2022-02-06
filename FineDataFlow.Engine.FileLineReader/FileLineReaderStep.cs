using FineDataFlow.Engine.Inboxes;
using FineDataFlow.Engine.Outboxes;
using FineDataFlow.Engine.Vars;
using System;
using System.IO;

namespace FineDataFlow.Engine.FileLineReader
{
	public class FileLineReaderStep : Step
	{
		//[Inbox("SeedRowInbox")]
		public SeedRowInbox SeedRowInbox { get; set; }
		public SuccessRowOutbox SuccessRowOutbox { get; set; }

		public StringVar FilePath { get; set; }
		
		public BooleanVar IncludeEmptyLines { get; set; }
		public StringVar OutputLineVariable { get; set; }

		public BooleanVar TrimLeft { get; set; }
		public StringVar TrimLeftCharacters { get; set; }

		public BooleanVar TrimRight { get; set; }
		public StringVar TrimRightCharacters { get; set; }

		public BooleanVar AddFilePathVariableToOutput { get; set; }
		public StringVar OutputFilePathVariable { get; set; }

		public BooleanVar AddLineNumberVariableToOutput { get; set; }
		public StringVar OutputLineNumberVariable { get; set; }

		private string _filePath;
		private char[] _trimLeftCharacters;
		private char[] _trimRightCharacters;
		private string _outputFilePathVariable;
		private string _ouputLineNumberVariable;

		public override void Initialize()
		{
			_filePath = FilePath.Value;

			if (string.IsNullOrWhiteSpace(_filePath))
			{
				throw new InvalidOperationException($"{nameof(FilePath)} is required");
			}

			_filePath = Path.GetFullPath(FilePath.Value);

			if (!File.Exists(_filePath))
			{
				throw new InvalidOperationException($"File '{_filePath}' not found");
			}

			if (string.IsNullOrWhiteSpace(OutputLineVariable.Value))
			{
				throw new InvalidOperationException($"{nameof(OutputLineVariable)} is required");
			}

			if (AddFilePathVariableToOutput.Value)
			{
				if (string.IsNullOrWhiteSpace(OutputFilePathVariable.Value))
				{
					throw new InvalidOperationException($"{nameof(OutputFilePathVariable)} is required when {nameof(AddFilePathVariableToOutput)} is true");
				}
				
				_outputFilePathVariable = OutputFilePathVariable.Value.Trim();
			}

			if (AddLineNumberVariableToOutput.Value)
			{
				if (string.IsNullOrWhiteSpace(OutputLineNumberVariable.Value))
				{
					throw new InvalidOperationException($"{nameof(OutputLineNumberVariable)} is required when {nameof(AddLineNumberVariableToOutput)} is true");
				}

				_ouputLineNumberVariable = OutputFilePathVariable.Value.Trim();
			}

			if (TrimLeft.Value && !string.IsNullOrWhiteSpace(TrimLeftCharacters.Value))
			{
				_trimLeftCharacters = TrimLeftCharacters.Value.ToCharArray();
			}

			if (TrimRight.Value && !string.IsNullOrWhiteSpace(TrimRightCharacters.Value))
			{
				_trimRightCharacters = TrimRightCharacters.Value.ToCharArray();
			}

			SeedRowInbox.OnRow += SeedRowInbox_OnRow;
		}

		private void SeedRowInbox_OnRow(object sender, OnRowEventArgs e)
		{
			var lines = File.ReadAllLines(_filePath);
			
			for (long i = 0; i < lines.Length; i++)
			{
				var lineText = lines[i];
				
				if (!IncludeEmptyLines.Value && string.IsNullOrWhiteSpace(lineText))
				{
					continue;
				}

				if (TrimLeft.Value)
				{
					lineText = lineText.TrimStart();

					if (_trimLeftCharacters != null)
					{
						lineText = lineText.TrimStart(_trimLeftCharacters);
					}
				}

				if (TrimRight.Value)
				{
					lineText = lineText.TrimEnd();

					if (_trimLeftCharacters != null)
					{
						lineText = lineText.TrimEnd(_trimRightCharacters);
					}
				}

				var row = new Row();

				row.Data[OutputLineVariable.Value] = lineText;

				if (AddFilePathVariableToOutput.Value)
				{
					row.Data[_outputFilePathVariable] = _filePath;
				}

				if (AddLineNumberVariableToOutput.Value)
				{
					row.Data[_ouputLineNumberVariable] = i + 1;
				}

				SuccessRowOutbox.AddRow(row);
			}
		}
	}
}
