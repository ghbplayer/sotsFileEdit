using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using sotsedit;

namespace s1edit
{
	class Processor
	{
		static public void process(Arguments arguments)
		{
			List<string> errors = new List<string>();
			string[] files = getListOfFiles(arguments.files);

			Modifier modifier = null;
			if (arguments.operation == Arguments.Operation.Change)
				try
				{
					modifier = new Modifier(arguments.details);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return;
				}

			string[] keys = arguments.key.Split('.');
			string lastKey = keys[keys.Length - 1];

			foreach (string file in files)
				try
				{
					bool change = false;
					using (MemoryStream dest = new MemoryStream())
					{
						using (StreamWriter output = new StreamWriter(dest))
						using (StreamReader input = new StreamReader(file, Encoding.ASCII))
						{
							int level = 0;
							int matchLevel = 0;
							string sectionHeader = String.Empty;
							for (string line = input.ReadLine(); line != null; line = input.ReadLine())
							{
								string trimmed = line.Trim();
								string comment = "";
								if (arguments.operation == Arguments.Operation.Change)
								{
									int commentStart = trimmed.IndexOf("//");
									if (commentStart != -1)
									{
										string nocom = trimmed.Remove(commentStart);
										nocom = nocom.Trim();
										comment = trimmed.Substring(nocom.Length, trimmed.Length- nocom.Length);
										trimmed = trimmed.Substring(0, nocom.Length);
									}
								}
								if (trimmed.StartsWith("{"))
								{
									if (trimmed != "{")
										throw new Exception("File contains a line with { and something else.");
									if (sectionHeader.Length == 0)
										throw new Exception("File contains { without section header.");
									if (level == matchLevel && keys.Length > level && sectionHeader.Equals(keys[level], StringComparison.InvariantCultureIgnoreCase))
									{
										++matchLevel;
										// Trigger insert
										if (matchLevel == keys.Length - 1 && arguments.operation == Arguments.Operation.Insert)
										{
											change = true;
											string newline = String.Empty.PadLeft(level + 1, '\t') + lastKey + " " + arguments.details;
											line += "\r\n" + newline;
										}
									}
									++level;
									sectionHeader = string.Empty;
								}
								else if (trimmed.StartsWith("}"))
								{
									if (trimmed != "}")
										throw new Exception("File contains a line with } and something else.");
									--level;
									if (matchLevel > level)
										matchLevel = level;
									sectionHeader = String.Empty;
								}
								else
								{
									if (trimmed.Any(c => Char.IsWhiteSpace(c)))
									{
										sectionHeader = string.Empty;
										if (arguments.operation != Arguments.Operation.Insert &&
											level == matchLevel &&
											matchLevel == keys.Length - 1)
										{
											int index = trimmed.IndexOfAny(new char[] { ' ', '\t' });
											if (index == lastKey.Length && trimmed.StartsWith(lastKey, StringComparison.InvariantCultureIgnoreCase))
											{
												if (arguments.operation == Arguments.Operation.Delete)
												{
													change = true;
													line = string.Empty;
												}
												else
												{	// Change
													change = true;
													string value = trimmed.Substring(index, trimmed.Length - index);
													modifier.apply(ref value);
													StringBuilder temp = new StringBuilder(string.Empty.PadLeft(level, '\t'));
													temp.Append(lastKey);
													temp.Append(" ");
													temp.Append(value);
													temp.Append(comment);
													temp.Append("\t// change: ");
													temp.Append(arguments.details);
													line = temp.ToString();
												}
											}
										}
									}
									else
										if (trimmed.Length > 0)
										sectionHeader = trimmed;
								}
								
								output.WriteLine(line);
							}
						}

						if (change)
							File.WriteAllBytes(file, dest.ToArray());
					}
				}
				catch (Exception e)
				{
					errors.Add(file + ": " + e.Message);
				}

			if (errors.Count != 0)
			{
				Console.WriteLine("Errors!");
				foreach (string error in errors)
					Console.WriteLine(error);
			}
		}

		static string[] getListOfFiles(string files)
		{
			if (!files.Contains('\\'))
				return Directory.GetFiles(".", files);
			int index = files.LastIndexOf('\\');
			string path = files.Substring(0, index);
			string name = files.Substring(index + 1);
			return Directory.GetFiles(path, name);
		}
	}
}
