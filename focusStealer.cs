using System;
using System.Diagnostics;
using System.Linq;

// Cmd line reading
using System.Management;
using System.ComponentModel;
// using System.Collections.Generic;

namespace ProcessMonitor {
	class Program {
		static void Main(string[] args) {
			var lastPros = Process.GetProcesses().Select((x) => x.Id).ToList();
			var oldProcessList = Process.GetProcesses();
			var processlist = oldProcessList; // create pointer

			// create Pointers;
			// var pro = new List<int>();
			// var processlist = new List<int.Id>();

			var dot = 0;

			while (true) {
				if ( ++dot > 20 ) {
					Console.ForegroundColor = ConsoleColor.White;
					Console.Write( '.' ); // I'm WORKING here!
					dot = 0;
				}

				processlist = Process.GetProcesses();

				var currentPros = processlist.Select(x => x.Id).ToList();
				var diff = lastPros.Except(currentPros).ToList();

				var startingProcess = diff.Count == 0;

				if (startingProcess) {
					// pro = processlist.Where((x) => diff.Contains(x.Id)).ToList();
					diff = currentPros.Except(lastPros).ToList();
				}

				if ( diff.Count > 0 ) {
					var pro = processlist.ToList(); // how do I set types? :D

					if (startingProcess) {
						Console.ForegroundColor = ConsoleColor.Green;
						// pro = processlist.Where((x) => diff.Contains(x.Id)).ToList();
						// diff = currentPros.Except(lastPros).ToList();
						pro = processlist.Where((x) => diff.Contains(x.Id)).ToList();
					} else {
						Console.ForegroundColor = ConsoleColor.Red;
						pro = oldProcessList.Where(x => diff.Contains(x.Id)).ToList();
					}

					foreach (var pid in diff) {
						// 	var _theProcess = pro.Where((x) => x.Id == pid ).ToList()[0];

						// switch( _theProcess.ProcessName ) {
						// 	case 'php':
						// 	case 'conhost':
						// 		continue 2;
						// 	default:
						// }

						Console.Write(
							"\n[{0}] {1} - ",
							new object[] {
								Program.timeNow(),
								( startingProcess ? "starting" : "stopping" )
							}
						);
						Console.Write("PID {0,-6}", pid );
						try {
							var theProcess = pro.Where((x) => x.Id == pid ).ToList()[0];

							string fileName = "N/A";
							string fileCommand = "N/A";

							// Console.WriteLine(Program.GetAllProperties(theProcess));
							try {
								fileName = theProcess.MainModule.FileName;
								// If we fail to get the filename, we will definitely fail the command.
								// Some arguments are private -- so this might still fail.
								// In this Try command, we basically have a weakpoint by design.
								fileCommand = Program.getCommandLine( theProcess.MainModule, pid );
							} catch { }

							// var mainModule = theProcess.MainModule;
							Console.Write(
								"; PName {0,-20}; PPath {1} --\"{2}\"",
								new object[] {
									theProcess.ProcessName,
									fileName,
									fileCommand
								}
							);

							if ( startingProcess ) {
								// While we're at it... we watch YouTube a lot, and Edge sets it to a low priority.
								// This low priority causes 'normal' priority background processes to hamper video playback... notably.
								// Console.Write( theProcess.GetType(  ) );
								Program.fixEdgePriority( theProcess, pid );
							}
						} catch ( Exception e ) {
							Console.WriteLine(" Hit exception {0}", e );
							Console.Write( "Press Enter to continue or CTRL+C or fuckknowswhat to close window ..." );
							// Console.Read(); // Do we need this here?
						}
					}
					if (diff.Count > 0) {
						lastPros = currentPros;
						oldProcessList = processlist;
					}
				}

				System.Threading.Thread.Sleep(200);
			}
		}

		static string timeNow() {

			// SYSTEMTIME systime = new SYSTEMTIME();
			// GetSystemTime(ref systime);

			DateTime datetime = DateTime.Now;

			// systime.wYear = (ushort)datetime.Year;
			// systime.wMonth = (ushort)datetime.Month;
			// systime.wDayOfWeek = (ushort)datetime.DayOfWeek;
			// systime.wDay = (ushort)datetime.Day;
			// systime.wHour = (ushort)datetime.Hour;
			// systime.wMinute = (ushort)datetime.Minute;
			// systime.wSecond = (ushort)datetime.Second;

			return string.Format( "{0:00}:{1:00}:{2:00}", new object[] {
				(ushort)datetime.Hour,
				(ushort)datetime.Minute,
				(ushort)datetime.Second,
			} );
		}

		static string GetAllProperties(object obj){
			return string.Join(" ", obj.GetType()
				.GetProperties()
				.Select(prop => prop.GetValue(obj)));
		}

		static string getCommandLine( object MainModule, int pid ) {
			string cmdLine = null;
			using (var searcher = new ManagementObjectSearcher(
				string.Format( "SELECT CommandLine FROM Win32_Process WHERE ProcessId = {0}", pid )
			) ) {
				// By definition, the query returns at most 1 match, because the process
				// is looked up by ID (which is unique by definition).
				using (var matchEnum = searcher.Get().GetEnumerator()) {
					if (matchEnum.MoveNext()) { // Move to first item, from 0
						cmdLine = matchEnum.Current["CommandLine"].ToString();
					}
				}
			}
			return cmdLine;
		}

		static void fixEdgePriority( Process theProcess, int pid ) {
			if ( "msedge" != theProcess.ProcessName ) return;

			Console.Write( "\nFixing Priority for: " + pid + " To High: ... " );
			try {
				theProcess.PriorityClass = ProcessPriorityClass.High;
			} catch { }
			Console.Write( ProcessPriorityClass.High == theProcess.PriorityClass ? "succes!" : "failure :(..." );
				// Console.ReadLine();
		}
	}
}
