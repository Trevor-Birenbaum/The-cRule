using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class TheCRuleScript : MonoBehaviour {

		public KMBombInfo bomb;
		// public KMAudio audio;

		public KMSelectable[] rowButtons;
		public KMSelectable[] colorButtons;
		public KMSelectable buttonS;
		public KMSelectable buttonR;

		public Material[] colors;
		public Material[] checkStates;

		// public AudioClip[] sounds;

		public Renderer[] rows = new Renderer[26]; // 8 7 6 5
			/*
			0 1 2 3 4 5 6 7
			 8 9 A B C D E
			  F G H I J K
				 L M N O P
			*/
		public TextMesh[] symbols = new TextMesh[26];
		private string[] symbolsList = new string[] {"♤♤", "♤♧", "♤♢", "♤♡", "♧♤", "♧♧", "♧♢", "♧♡", "♢♤", "♢♧", "♢♢", "♢♡", "♡♤", "♡♧", "♡♢", "♡♡"};

		private bool[,] bools = new bool[3,26]; // R G B
		private bool[,] boolsExSol = new bool[3,26];
		private string[] solLetters = new string[26];

		private int[] ruleNumbers = new int[3];
		private bool[,] ruleBinaries = new bool[3,4];

		int[] initOn = new int[10];

		private int cellPressIndex;
		private int colorPressIndex = -1;
		private bool moduleSolved = false;

		static int moduleIdCounter = 1;
		int moduleId;

		void Awake() {
				moduleId = moduleIdCounter++;

				foreach(KMSelectable element in rowButtons) {
						element.OnInteract += delegate() {
								cellPressIndex = Array.IndexOf(rowButtons, element);
								ExecuteClick(cellPressIndex);
								return false;
						};
				}

				foreach(KMSelectable color in colorButtons) {
						color.AddInteractionPunch(0.5f);
						color.OnInteract += delegate() {
								colorPressIndex = Array.IndexOf(colorButtons, color);
								return false;
						};
				}

				buttonS.OnInteract += delegate() {
						buttonS.AddInteractionPunch();
						StartCoroutine(Check());
						StopCoroutine(Check());
						return false;
				};

				buttonR.OnInteract += delegate() {
						buttonR.AddInteractionPunch();
						Reset();
						return false;
				};
		}

		void Start() {
				BlankModule();
				PickRuleNumbers();
				FindOneSolution();
				PickInitialColors();

				InitialLogging();
		}

		void PrintStates() {
				string printout = "";
				for(int i = 0; i < 8; i++) {
						if(bools[0,i] && bools[1,i] && bools[2,i]) {
								printout += "W ";
						} else if(bools[0,i] && bools[1,i] && !bools[2,i]) {
								printout += "Y ";
						} else if(bools[0,i] && !bools[1,i] && bools[2,i]) {
								printout += "M ";
						} else if(bools[0,i] && !bools[1,i] && !bools[2,i]) {
								printout += "R ";
						} else if(!bools[0,i] && bools[1,i] && bools[2,i]) {
								printout += "C ";
						} else if(!bools[0,i] && bools[1,i] && !bools[2,i]) {
								printout += "G ";
						} else if(!bools[0,i] && !bools[1,i] && bools[2,i]) {
								printout += "B ";
						} else if(!bools[0,i] && !bools[1,i] && !bools[2,i]) {
								printout += "K ";
						} else {
								printout += ". ";
						}
				}

				printout += "\n ";

				for(int i = 8; i < 15; i++) {
						if(bools[0,i] && bools[1,i] && bools[2,i]) {
								printout += "W ";
						} else if(bools[0,i] && bools[1,i] && !bools[2,i]) {
								printout += "Y ";
						} else if(bools[0,i] && !bools[1,i] && bools[2,i]) {
								printout += "M ";
						} else if(bools[0,i] && !bools[1,i] && !bools[2,i]) {
								printout += "R ";
						} else if(!bools[0,i] && bools[1,i] && bools[2,i]) {
								printout += "C ";
						} else if(!bools[0,i] && bools[1,i] && !bools[2,i]) {
								printout += "G ";
						} else if(!bools[0,i] && !bools[1,i] && bools[2,i]) {
								printout += "B ";
						} else if(!bools[0,i] && !bools[1,i] && !bools[2,i]) {
								printout += "K ";
						} else {
								printout += ". ";
						}
				}

				printout += "\n  ";

				for(int i = 15; i < 21; i++) {
						if(bools[0,i] && bools[1,i] && bools[2,i]) {
								printout += "W ";
						} else if(bools[0,i] && bools[1,i] && !bools[2,i]) {
								printout += "Y ";
						} else if(bools[0,i] && !bools[1,i] && bools[2,i]) {
								printout += "M ";
						} else if(bools[0,i] && !bools[1,i] && !bools[2,i]) {
								printout += "R ";
						} else if(!bools[0,i] && bools[1,i] && bools[2,i]) {
								printout += "C ";
						} else if(!bools[0,i] && bools[1,i] && !bools[2,i]) {
								printout += "G ";
						} else if(!bools[0,i] && !bools[1,i] && bools[2,i]) {
								printout += "B ";
						} else if(!bools[0,i] && !bools[1,i] && !bools[2,i]) {
								printout += "K ";
						} else {
								printout += ". ";
						}
				}

				printout += "\n   ";

				for(int i = 21; i < 26; i++) {
						if(Array.IndexOf(colors, rows[i].material) == 8) {
								printout += ". ";
						}
						else if(bools[0,i] && bools[1,i] && bools[2,i]) {
								printout += "W ";
						} else if(bools[0,i] && bools[1,i] && !bools[2,i]) {
								printout += "Y ";
						} else if(bools[0,i] && !bools[1,i] && bools[2,i]) {
								printout += "M ";
						} else if(bools[0,i] && !bools[1,i] && !bools[2,i]) {
								printout += "R ";
						} else if(!bools[0,i] && bools[1,i] && bools[2,i]) {
								printout += "C ";
						} else if(!bools[0,i] && bools[1,i] && !bools[2,i]) {
								printout += "G ";
						} else if(!bools[0,i] && !bools[1,i] && bools[2,i]) {
								printout += "B ";
						} else if(!bools[0,i] && !bools[1,i] && !bools[2,i]) {
								printout += "K ";
						}
				}

				Debug.LogFormat("[The cRule #{0}] Submitted the following states:\n" + printout, moduleId);
		}

		void BlankModule() {
				for(int i = 0; i < 26; i++) {
						rows[i].material = colors[8]; // gray
						symbols[i].text = "";
				}
		}

		void PickRuleNumbers() {
				ruleNumbers[0] = UnityEngine.Random.Range(1,15);
				do {
						ruleNumbers[1] = UnityEngine.Random.Range(1,15);
				} while(ruleNumbers[1] == ruleNumbers[0]);
				do {
						ruleNumbers[2] = UnityEngine.Random.Range(1,15);
				} while(ruleNumbers[2] == ruleNumbers[0] || ruleNumbers[2] == ruleNumbers[1]);

				for(int i = 0; i < 3; i++) {
						ruleBinaries[i,0] = (ruleNumbers[i] >= 8);
						ruleBinaries[i,1] = (ruleNumbers[i] % 8 >= 4);
						ruleBinaries[i,2] = (ruleNumbers[i] % 4 >= 2);
						ruleBinaries[i,3] = (ruleNumbers[i] % 2 == 1);
				}
		}

		void FindOneSolution() {
				for(int i = 0; i < 3; i++) {
						for(int j = 0; j < 8; j++) {
								boolsExSol[i,j] = (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5);
						}
				}

				int rGuarantee = UnityEngine.Random.Range(0,8);
				int gGuarantee = UnityEngine.Random.Range(0,8);
				int bGuarantee = UnityEngine.Random.Range(0,8);
				while(gGuarantee == rGuarantee) {
						gGuarantee = UnityEngine.Random.Range(0,8);
				}
				while(bGuarantee == rGuarantee || bGuarantee == gGuarantee) {
						bGuarantee = UnityEngine.Random.Range(0,8);
				}

				if(UnityEngine.Random.Range(0.0f, 1.0f) > 0.5) {
						boolsExSol[0,rGuarantee] = true;
						boolsExSol[1,rGuarantee] = false;
						boolsExSol[2,rGuarantee] = false;
				} else {
						boolsExSol[0,rGuarantee] = false;
						boolsExSol[1,rGuarantee] = true;
						boolsExSol[2,rGuarantee] = true;
				}
				if(UnityEngine.Random.Range(0.0f, 1.0f) > 0.5) {
						boolsExSol[0,gGuarantee] = false;
						boolsExSol[1,gGuarantee] = true;
						boolsExSol[2,gGuarantee] = false;
				} else {
						boolsExSol[0,gGuarantee] = true;
						boolsExSol[1,gGuarantee] = false;
						boolsExSol[2,gGuarantee] = true;
				}
				if(UnityEngine.Random.Range(0.0f, 1.0f) > 0.5) {
						boolsExSol[0,bGuarantee] = false;
						boolsExSol[1,bGuarantee] = false;
						boolsExSol[2,bGuarantee] = true;
				} else {
						boolsExSol[0,bGuarantee] = true;
						boolsExSol[1,bGuarantee] = true;
						boolsExSol[2,bGuarantee] = false;
				}

				for(int i = 0; i < 3; i++) {
						for(int j = 0; j < 7; j++) {
								if(ruleBinaries[i,0]) {
										if(boolsExSol[i,j] && boolsExSol[i,j+1]) {
												boolsExSol[i,j+8] = true;
										}
								}
								if(ruleBinaries[i,1]) {
										if(boolsExSol[i,j] && !boolsExSol[i,j+1]) {
												boolsExSol[i,j+8] = true;
										}
								}
								if(ruleBinaries[i,2]) {
										if(!boolsExSol[i,j] && boolsExSol[i,j+1]) {
												boolsExSol[i,j+8] = true;
										}
								}
								if(ruleBinaries[i,3]) {
										if(!boolsExSol[i,j] && !boolsExSol[i,j+1]) {
												boolsExSol[i,j+8] = true;
										}
								}
						}

						for(int j = 8; j < 14; j++) {
								if(ruleBinaries[i,0]) {
										if(boolsExSol[i,j] && boolsExSol[i,j+1]) {
												boolsExSol[i,j+7] = true;
										}
								}
								if(ruleBinaries[i,1]) {
										if(boolsExSol[i,j] && !boolsExSol[i,j+1]) {
												boolsExSol[i,j+7] = true;
										}
								}
								if(ruleBinaries[i,2]) {
										if(!boolsExSol[i,j] && boolsExSol[i,j+1]) {
												boolsExSol[i,j+7] = true;
										}
								}
								if(ruleBinaries[i,3]) {
										if(!boolsExSol[i,j] && !boolsExSol[i,j+1]) {
												boolsExSol[i,j+7] = true;
										}
								}
						}

						for(int j = 15; j < 20; j++) {
								if(ruleBinaries[i,0]) {
										if(boolsExSol[i,j] && boolsExSol[i,j+1]) {
												boolsExSol[i,j+6] = true;
										}
								}
								if(ruleBinaries[i,1]) {
										if(boolsExSol[i,j] && !boolsExSol[i,j+1]) {
												boolsExSol[i,j+6] = true;
										}
								}
								if(ruleBinaries[i,2]) {
										if(!boolsExSol[i,j] && boolsExSol[i,j+1]) {
												boolsExSol[i,j+6] = true;
										}
								}
								if(ruleBinaries[i,3]) {
										if(!boolsExSol[i,j] && !boolsExSol[i,j+1]) {
												boolsExSol[i,j+6] = true;
										}
								}
						}
				}

				for(int i = 0; i < 26; i++) {
						if(boolsExSol[0,i] && boolsExSol[1,i] && boolsExSol[2,i]) {
								solLetters[i] = "W";
						} else if(boolsExSol[0,i] && boolsExSol[1,i] && !boolsExSol[2,i]) {
								solLetters[i] = "Y";
						} else if(boolsExSol[0,i] && !boolsExSol[1,i] && boolsExSol[2,i]) {
								solLetters[i] = "M";
						} else if(boolsExSol[0,i] && !boolsExSol[1,i] && !boolsExSol[2,i]) {
								solLetters[i] = "R";
						} else if(!boolsExSol[0,i] && boolsExSol[1,i] && boolsExSol[2,i]) {
								solLetters[i] = "C";
						} else if(!boolsExSol[0,i] && boolsExSol[1,i] && !boolsExSol[2,i]) {
								solLetters[i] = "G";
						} else if(!boolsExSol[0,i] && !boolsExSol[1,i] && boolsExSol[2,i]) {
								solLetters[i] = "B";
						} else if(!boolsExSol[0,i] && !boolsExSol[1,i] && !boolsExSol[2,i]) {
								solLetters[i] = "K";
						}
				}
		}

		void TurnOn(int index) { // black, red, green, blue, white, cyan, magenta, yellow
				for(int i = 0; i < 3; i++) {
						bools[i,index] = boolsExSol[i, index];
				}

				if(!bools[0,index] && !bools[1,index] && !bools[2,index]) {
						rows[index].material = colors[0];
				} else if(bools[0,index] && !bools[1,index] && !bools[2,index]) {
						rows[index].material = colors[1];
				} else if(!bools[0,index] && bools[1,index] && !bools[2,index]) {
						rows[index].material = colors[2];
				} else if(!bools[0,index] && !bools[1,index] && bools[2,index]) {
						rows[index].material = colors[3];
				} else if(bools[0,index] && bools[1,index] && bools[2,index]) {
						rows[index].material = colors[4];
				} else if(!bools[0,index] && bools[1,index] && bools[2,index]) {
						rows[index].material = colors[5];
				} else if(bools[0,index] && !bools[1,index] && bools[2,index]) {
						rows[index].material = colors[6];
				} else if (bools[0,index] && bools[1,index] && !bools[2,index]) {
						rows[index].material = colors[7];
				}
		}

		void PickInitialColors() {
				int rInitOn = UnityEngine.Random.Range(0,26);
				for(int i = rInitOn; i < rInitOn + 26; i++) {
						int index = i % 26;
						if(solLetters[index].Equals("R") || solLetters[index].Equals("C")) {
								rInitOn = index;
								TurnOn(rInitOn);
								symbols[index].text = symbolsList[(ruleNumbers[0] - bomb.GetSerialNumberNumbers().Sum() + 160) % 16];
								break;
						}
				}

				int gInitOn = UnityEngine.Random.Range(0,26);
				for(int i = gInitOn; i < gInitOn + 26; i++) {
						int index = i % 26;
						if(solLetters[index].Equals("G") || solLetters[index].Equals("M")) {
								gInitOn = index;
								TurnOn(gInitOn);
								symbols[index].text = symbolsList[(ruleNumbers[1] - bomb.GetSerialNumberNumbers().Sum() + 160) % 16];
								break;
						}
				}

				int bInitOn = UnityEngine.Random.Range(0,26);
				for(int i = bInitOn; i < bInitOn + 26; i++) {
						int index = i % 26;
						if(solLetters[index].Equals("B") || solLetters[index].Equals("Y")) {
								bInitOn = index;
								TurnOn(bInitOn);
								symbols[index].text = symbolsList[(ruleNumbers[2] - bomb.GetSerialNumberNumbers().Sum() + 160) % 16];
								break;
						}
				}

				initOn[0] = rInitOn;
				initOn[1] = gInitOn;
				initOn[2] = bInitOn;

				for(int i = 3; i < 10; i++) {
						int temp;
						do {
								temp = UnityEngine.Random.Range(0,26);
						} while(initOn.Contains(temp));
						initOn[i] = temp;
				}

				for(int i = 3; i < 10; i++) {
						TurnOn(initOn[i]);
				}
		}

		void InitialLogging() {
				// Rule Number
				string[] logColor = new string[] {"Red ", "Green ", "Blue "};
				string[] logPair = new string[] {"(1,1) ", "(1,0) ", "(0,1) ", "(0,0) "};
				string value;
				for(int i = 0; i < 3; i++) {
						string RNPrint = "";
						RNPrint += logColor[i] + "Rule Number is " + ruleNumbers[i] + " (" + symbolsList[(ruleNumbers[i] - bomb.GetSerialNumberNumbers().Sum() + 160) % 16] + "); ";
						for(int j = 0; j < 4; j++) {
								if(ruleBinaries[i,j]) {
										value = "1";
								} else {
										value = "0";
								}
								RNPrint += logPair[j] + "--> " + value;
								if(j < 3) {
										RNPrint += ";";
								}
								RNPrint += " ";
						}
						Debug.LogFormat("[The cRule #{0}] " + RNPrint, moduleId);
				}

				// Initially filled in squares
				int redSym = initOn[0];
				int greenSym = initOn[1];
				int blueSym = initOn[2];

				Array.Sort(initOn);

				string initFillPrint = "Initially filled squares are ";
				for(int i = 0; i < 10; i++) {
						initFillPrint += ++initOn[i] + solLetters[initOn[i] - 1];
						if(i < 9) {
								initFillPrint += ",";
						}
						initFillPrint += " ";
				}
				Debug.LogFormat("[The cRule #{0}] " + initFillPrint, moduleId);
				Debug.LogFormat("[The cRule #{0}] Symbols: Red " + ++redSym + ", Green " + ++greenSym + ", Blue " + ++blueSym, moduleId);

				// Initial Solution
				string SolPrint = "";
				for(int i = 0; i < 26; i++) {
						SolPrint += solLetters[i] + " ";

						if(i == 7) {
								SolPrint += "\n ";
						}
						if(i == 14) {
								SolPrint += "\n  ";
						}
						if(i == 20) {
								SolPrint += "\n   ";
						}
				}
				Debug.LogFormat("[The cRule #{0}] One possible solution is:\n" + SolPrint, moduleId);


		}

		void ExecuteClick(int index) { // KRGBWCMY
				if(!initOn.Contains(index + 1) && colorPressIndex != -1) {
						bools[0,index] = (colorPressIndex == 1 || colorPressIndex == 4 || colorPressIndex == 6 || colorPressIndex == 7);
						bools[1,index] = (colorPressIndex == 2 || colorPressIndex == 4 || colorPressIndex == 5 || colorPressIndex == 7);
						bools[2,index] = (colorPressIndex == 3 || colorPressIndex == 4 || colorPressIndex == 5 || colorPressIndex == 6);
						rows[index].material = colors[colorPressIndex];
				} else {
						rowButtons[index].AddInteractionPunch();
				}
		}

		void Reset() {
				for(int i = 0; i < 26; i++) {
						if(!initOn.Contains(i + 1)) {
								// sound
								rows[i].material = colors[8];
						}
				}
				colorPressIndex = -1;
		}

		IEnumerator Check() { // change to IEnumerator
				bool correct = true;

				PrintStates();

				bool[,] boolsSol = new bool[3,26];
				for(int i = 0; i < 3; i++) {
						for(int j = 0; j < 8; j++) {
								boolsSol[i,j] = bools[i,j];
						}
				}

				for(int i = 0; i < 3; i++) {
						for(int j = 0; j < 7; j++) {
								if(ruleBinaries[i,0]) {
										if(boolsSol[i,j] && boolsSol[i,j+1]) {
												boolsSol[i,j+8] = true;
										}
								}
								if(ruleBinaries[i,1]) {
										if(boolsSol[i,j] && !boolsSol[i,j+1]) {
												boolsSol[i,j+8] = true;
										}
								}
								if(ruleBinaries[i,2]) {
										if(!boolsSol[i,j] && boolsSol[i,j+1]) {
												boolsSol[i,j+8] = true;
										}
								}
								if(ruleBinaries[i,3]) {
										if(!boolsSol[i,j] && !boolsSol[i,j+1]) {
												boolsSol[i,j+8] = true;
										}
								}
						}

						for(int j = 8; j < 14; j++) {
								if(ruleBinaries[i,0]) {
										if(boolsSol[i,j] && boolsSol[i,j+1]) {
												boolsSol[i,j+7] = true;
										}
								}
								if(ruleBinaries[i,1]) {
										if(boolsSol[i,j] && !boolsSol[i,j+1]) {
												boolsSol[i,j+7] = true;
										}
								}
								if(ruleBinaries[i,2]) {
										if(!boolsSol[i,j] && boolsSol[i,j+1]) {
												boolsSol[i,j+7] = true;
										}
								}
								if(ruleBinaries[i,3]) {
										if(!boolsSol[i,j] && !boolsSol[i,j+1]) {
												boolsSol[i,j+7] = true;
										}
								}
						}

						for(int j = 15; j < 20; j++) {
								if(ruleBinaries[i,0]) {
										if(boolsSol[i,j] && boolsSol[i,j+1]) {
												boolsSol[i,j+6] = true;
										}
								}
								if(ruleBinaries[i,1]) {
										if(boolsSol[i,j] && !boolsSol[i,j+1]) {
												boolsSol[i,j+6] = true;
										}
								}
								if(ruleBinaries[i,2]) {
										if(!boolsSol[i,j] && boolsSol[i,j+1]) {
												boolsSol[i,j+6] = true;
										}
								}
								if(ruleBinaries[i,3]) {
										if(!boolsSol[i,j] && !boolsSol[i,j+1]) {
												boolsSol[i,j+6] = true;
										}
								}
						}
				}

				for(int j = 0; j < 26; j++) {
						if(!initOn.Contains(j + 1)) {
								rows[j].material = checkStates[0];
								yield return new WaitForSeconds(0.1f);
						}
				}
				yield return new WaitForSeconds(1f);

				bool struck = false;
				for(int i = 0; i < 3; i++) {
						for(int j = 0; j < 26; j++) {
								if(bools[i,j] != boolsSol[i,j]) {
										correct = false;
										if(!struck) {
												GetComponent<KMBombModule>().HandleStrike();
												struck = true;
												Debug.LogFormat("[The cRule #{0}] Incorrect solution, strike given!", moduleId);
												Reset();
										}
								}
						}
				}

				if(correct) {
						for(int i = 0; i < 26; i++) {
								symbols[i].text = "";
						}
						for(int j = 0; j < 26; j++) {
								if(!bools[0,j] && !bools[1,j] && !bools[2,j]) {
										rows[j].material = colors[0];
								} else if(bools[0,j] && !bools[1,j] && !bools[2,j]) {
										rows[j].material = colors[1];
								} else if(!bools[0,j] && bools[1,j] && !bools[2,j]) {
										rows[j].material = colors[2];
								} else if(!bools[0,j] && !bools[1,j] && bools[2,j]) {
										rows[j].material = colors[3];
								} else if(bools[0,j] && bools[1,j] && bools[2,j]) {
										rows[j].material = colors[4];
								} else if(!bools[0,j] && bools[1,j] && bools[2,j]) {
										rows[j].material = colors[5];
								} else if(bools[0,j] && !bools[1,j] && bools[2,j]) {
										rows[j].material = colors[6];
								} else if (bools[0,j] && bools[1,j] && !bools[2,j]) {
										rows[j].material = colors[7];
								}
						}
						moduleSolved = true;
						GetComponent<KMBombModule>().HandlePass();
                        Debug.LogFormat("[The cRule #{0}] Module solved!", moduleId);
				}
		}

		//Souvenir: Which row(s) did not contain a color symbol?


		// TwitchPlays Code
		#pragma warning disable 414
    		private string TwitchHelpMessage = "Type '!{0} (c) # # #'' to color squares in reading order with the abbreviated color (c). Type '!{0} submit' to submit your answer, and type '!{0} reset' to reset. Colors and cells can be chained, such as in '!{0} r 4 12 c 5 k 16'. Colors are blac(k), (r)ed, (g)reen, (b)lue, (w)hite, (c)yan, (m)agenta, and (y)ellow.";
		#pragma warning restore 414

		IEnumerator ProcessTwitchCommand(string input) {
		yield return null;

				if (Regex.IsMatch(input, @"^\s*reset\s*$", RegexOptions.IgnoreCase)) {
						yield return null;
						buttonR.OnInteract();
						yield break;
				}

				if (Regex.IsMatch(input, @"^\s*submit\s*$", RegexOptions.IgnoreCase)) {
            yield return null;
            buttonS.OnInteract();
            yield break;
        }

				string[] parameters = input.Split(' ');
				var buttonsToPress = new List<KMSelectable>();
        foreach (string param in parameters) {
						if(param.EqualsIgnoreCase("k") || param.EqualsIgnoreCase("black")) {
								buttonsToPress.Add(colorButtons[0]);
						} else if(param.EqualsIgnoreCase("r") || param.EqualsIgnoreCase("red")) {
								buttonsToPress.Add(colorButtons[1]);
						} else if(param.EqualsIgnoreCase("g") || param.EqualsIgnoreCase("green")) {
								buttonsToPress.Add(colorButtons[2]);
						} else if(param.EqualsIgnoreCase("b") || param.EqualsIgnoreCase("blue")) {
								buttonsToPress.Add(colorButtons[3]);
						} else if(param.EqualsIgnoreCase("w") || param.EqualsIgnoreCase("white")) {
								buttonsToPress.Add(colorButtons[4]);
						} else if(param.EqualsIgnoreCase("c") || param.EqualsIgnoreCase("cyan")) {
								buttonsToPress.Add(colorButtons[5]);
						} else if(param.EqualsIgnoreCase("m") || param.EqualsIgnoreCase("magenta")) {
								buttonsToPress.Add(colorButtons[6]);
						} else if(param.EqualsIgnoreCase("y") || param.EqualsIgnoreCase("yellow")) {
								buttonsToPress.Add(colorButtons[7]);
						} else if(param.EqualsIgnoreCase("1")) {
								buttonsToPress.Add(rowButtons[0]);
						} else if(param.EqualsIgnoreCase("2")) {
								buttonsToPress.Add(rowButtons[1]);
						} else if(param.EqualsIgnoreCase("3")) {
								buttonsToPress.Add(rowButtons[2]);
						} else if(param.EqualsIgnoreCase("4")) {
								buttonsToPress.Add(rowButtons[3]);
						} else if(param.EqualsIgnoreCase("5")) {
								buttonsToPress.Add(rowButtons[4]);
						} else if(param.EqualsIgnoreCase("6")) {
								buttonsToPress.Add(rowButtons[5]);
						} else if(param.EqualsIgnoreCase("7")) {
								buttonsToPress.Add(rowButtons[6]);
						} else if(param.EqualsIgnoreCase("8")) {
								buttonsToPress.Add(rowButtons[7]);
						} else if(param.EqualsIgnoreCase("9")) {
								buttonsToPress.Add(rowButtons[8]);
						} else if(param.EqualsIgnoreCase("10")) {
								buttonsToPress.Add(rowButtons[9]);
						} else if(param.EqualsIgnoreCase("11")) {
								buttonsToPress.Add(rowButtons[10]);
						} else if(param.EqualsIgnoreCase("12")) {
								buttonsToPress.Add(rowButtons[11]);
						} else if(param.EqualsIgnoreCase("13")) {
								buttonsToPress.Add(rowButtons[12]);
						} else if(param.EqualsIgnoreCase("14")) {
								buttonsToPress.Add(rowButtons[13]);
						} else if(param.EqualsIgnoreCase("15")) {
								buttonsToPress.Add(rowButtons[14]);
						} else if(param.EqualsIgnoreCase("16")) {
								buttonsToPress.Add(rowButtons[15]);
						} else if(param.EqualsIgnoreCase("17")) {
								buttonsToPress.Add(rowButtons[16]);
						} else if(param.EqualsIgnoreCase("18")) {
								buttonsToPress.Add(rowButtons[17]);
						} else if(param.EqualsIgnoreCase("19")) {
								buttonsToPress.Add(rowButtons[18]);
						} else if(param.EqualsIgnoreCase("20")) {
								buttonsToPress.Add(rowButtons[19]);
						} else if(param.EqualsIgnoreCase("21")) {
								buttonsToPress.Add(rowButtons[20]);
						} else if(param.EqualsIgnoreCase("22")) {
								buttonsToPress.Add(rowButtons[21]);
						} else if(param.EqualsIgnoreCase("23")) {
								buttonsToPress.Add(rowButtons[22]);
						} else if(param.EqualsIgnoreCase("24")) {
								buttonsToPress.Add(rowButtons[23]);
						} else if(param.EqualsIgnoreCase("25")) {
								buttonsToPress.Add(rowButtons[24]);
						} else if(param.EqualsIgnoreCase("26")) {
								buttonsToPress.Add(rowButtons[25]);
						} else {
								yield break;
						}
				}

			foreach (KMSelectable b in buttonsToPress)
			{ 
				b.OnInteract();
			}
		}

	IEnumerator TwitchHandleForcedSolve()
	{
        for(int i = 0; i < solLetters.Length; i++)
        {
            yield return ProcessTwitchCommand(solLetters[i] + " " + (i + 1));
        }

        yield return ProcessTwitchCommand("submit");

        while (!moduleSolved)
		{
			yield return true;
		}
	}
}
