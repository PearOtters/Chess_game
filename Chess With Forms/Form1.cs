using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Media;
using System.IO;
using System.Runtime.CompilerServices;
using System.Data.SQLite;
using System.Net.NetworkInformation;
using System.Reflection;

namespace Chess_With_Forms
{
	public partial class Form1 : Form
	{
		private int y1, y2, x1, x2;
		private PictureBox[,] squareBoard;
		private System.Timers.Timer timer;
		private System.Timers.Timer timerB;
		private string[,] board;
		private readonly string[,] reverseBoard = new string[8, 8];
		private char col = 'w';
		private bool enabled = false, puttingPieces = false, tutorial = false;
		private int selected = 0;
		private int promoting = 0;
		private string pieceToPut = "_";
		private string demo = "_"; private int demoNum = 0;
		private bool timerTick = true;
		private bool rev = false;
		private bool enableRev = true;
		private bool tutBot = false;
		private readonly List<string[,]> moves = new List<string[,]>();
		private readonly List<string> replayMoves = new List<string>();
		private int movesNum = 0;
		private int TimeBetweenBotMove = 250;
		private readonly List<int[]> LastMoves = new List<int[]>();
		private SoundPlayer toc;
		private bool ignoreInvalid = false;
		private readonly Random rnd = new Random();
		private Button[] games;
		private int[] dragDropCoords = { 0, 0 };
		private readonly List<string> pastGameMoves = new List<string>();
		private int chessPiece = 0;
		private int[] boardCol = { 0, 112, 163 };
		private int[] boardCol2 = { 255, 225, 217, 209 };
        private readonly string filename = "Logins.txt";
		private string setupText = "";
		private bool pastGame = false; int pastGameMove = 0;
		private bool AIMediumLevel = false;
		private int depth = 2;
        int ID; string BoardCol; bool boardFlips = true, loggedIn = false; string name; bool soundEffects = true, promotionRange = true; 

        public Form1()
		{
			board = CreateEmptyBoard();
			InitializeComponent();
		}
		private void Setup()
		{
			toc = new SoundPlayer(@"G:\My Drive\NEA project\Toc\Toc2(LeRetour).wav");
            SetupTransparentText();
			XButton.Visible = false;
			squareBoard = new PictureBox[8, 8];
			int left, top = 2;
			bool colour = false;
			for (int y = 0; y < 8; y++)
			{
				left = 2;
				for (int x = 0; x < 8; x++)
				{
                    squareBoard[y, x] = new PictureBox
                    {
                        Location = new Point(left, top),
                        Size = new Size((Boardpanel.Width - 2) / 8, (Boardpanel.Height - 2) / 8),
						AllowDrop = true
                    };
                    if (colour)
					{
						squareBoard[y, x].BackColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
						colour = false;
					}
					else
					{
						squareBoard[y, x].BackColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
						colour = true;
					}
					left += (Boardpanel.Width - 2) / 8;
					Boardpanel.Controls.Add(squareBoard[y, x]);
					if (x == 7)
					{
						colour = !colour;
					}
				}
				top += (Boardpanel.Height - 2) / 8;
			}
			WhiteMoveCheckBox.Checked = true;
			SetupTimer();
			SetupPieces();
			MakeMove();
        }

		private struct AIStruct
		{
			public string[,] inBoard;
			public double adv;
			public List<int[]> inLastMoves;
        }

        private void MakeMove()
		{
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					squareBoard[y, x].DragDrop += new DragEventHandler(this.boardSquare_DragDrop);
					squareBoard[y, x].DragEnter += new DragEventHandler(this.boardSquare_DragEnter);
                    squareBoard[y, x].MouseDown += (sender1, click) =>
                    {
                        if (!puttingPieces && !tutorial)
                        {
                            PictureBox piece = sender1 as PictureBox;
                            bool isAvailable = false;
                            if (piece.Name.Contains('A'))
                            {
                                isAvailable = true;
                            }
                            if (click.Button == MouseButtons.Left)
                            {
                                if (piece.BackColor == Color.FromArgb(59, 217, 72))
                                {
                                    ResetColours(false);
                                    RefreshTransparentLetters();
                                }
                                else if (piece.BackColor == Color.DarkRed)
                                {
                                    ResetColours(false);
                                    squareBoard[piece.Top / 60, piece.Left / 60].BackColor = Color.Red;
                                    RefreshTransparentLetters();
                                }
                                else if (piece.Image != null && !isAvailable && piece.Name[0] == col && piece.BackColor != Color.FromArgb(59, 217, 72) && piece.BackColor != Color.Red)
                                {
                                    ResetColours(false);
                                    squareBoard[piece.Top / 60, piece.Left / 60].BackColor = Color.FromArgb(59, 217, 72);
                                    RefreshTransparentLetters();
                                    if (enabled)
                                    {
										foreach (int[] i in CheckAllAvailables(piece, col))
										{
											ShowAvailable(i);
										}
                                    }
                                }
                                else if (piece.Image != null && !isAvailable && piece.Name[0] == col && piece.BackColor != Color.FromArgb(59, 217, 72) && piece.BackColor == Color.Red)
                                {
                                    ResetColours(false);
                                    squareBoard[piece.Top / 60, piece.Left / 60].BackColor = Color.DarkRed;
                                    RefreshTransparentLetters();
                                    if (enabled)
                                    {
                                        foreach (int[] i in CheckAllAvailables(piece, col))
                                        {
                                            ShowAvailable(i);
                                        }
                                    }
                                }
                                else if (piece.Image != null && !isAvailable && piece.Name[0] != col)
                                {
                                    ResetColours(false);
                                    squareBoard[piece.Top / 60, piece.Left / 60].BackColor = Color.FromArgb(59, 217, 72);
                                    RefreshTransparentLetters();
                                }
                                else if (piece.Image == null && !isAvailable)
                                {
                                    ResetColours(false);
                                    RefreshTransparentLetters();
                                }
                                PieceMovement(piece, sender1);
                            }
                            else if (enabled && click.Button == MouseButtons.Right)
                            {
                                if (piece.BackColor == Color.Brown || piece.BackColor == Color.Maroon)
                                {
                                    if ((piece.Left / 60 + piece.Top / 60) % 2 == 0)
                                    {
                                        piece.BackColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
										CheckColour(true); CheckColour(false);
                                    }
                                    else
                                    {
                                        piece.BackColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
										CheckColour(true); CheckColour(false);
                                    }
                                    RefreshTransparentLetters();
                                }
                                else
                                {
                                    if ((piece.Left / 60 + piece.Top / 60) % 2 == 0)
                                    {
                                        piece.BackColor = Color.Brown;
                                    }
                                    else
                                    {
                                        piece.BackColor = Color.Maroon;
                                    }
                                    RefreshTransparentLetters();
                                }
                            }
							else if (enabled && click.Button == MouseButtons.Middle)
							{
								if (piece.Name.Contains(col))
								{
									if (piece.Image != null && !isAvailable)
									{
										if (piece.BackColor == Color.Red)
										{
											ResetColours(false);
											piece.BackColor = Color.DarkRed;
										}
										else
										{
											ResetColours(false);
											piece.BackColor = Color.FromArgb(59, 217, 72);
										}
                                        foreach (int[] i in CheckAllAvailables(piece, col))
                                        {
                                            ShowAvailable(i);
                                        }
                                        dragDropCoords = new int[2] { piece.Top / 60, piece.Left / 60 };
										piece.DoDragDrop(piece.Image, DragDropEffects.Copy);
                                    }
								}
								else
								{
									if (piece.Image != null && !isAvailable)
									{
                                        CheckColour(true); CheckColour(false);
                                        if (piece.BackColor == Color.Red)
										{
											ResetColours(false);
											piece.BackColor = Color.DarkRed;
										}
										else
										{
											ResetColours(false);
											piece.BackColor = Color.FromArgb(59, 217, 72);
										}
										piece.DoDragDrop(piece.Image, DragDropEffects.None);
									}
                                }
                                ResetColours(false);
                                RefreshTransparentLetters();
                            }
                        }
                    };
                }
			}
		}

		private void AIMovement(char inCol)
		{
			if (enabled)
			{
				bool colour = false;
				AutomaticFlipsCheckBox.Checked = false;
				List<AIStruct> AIMoves = new List<AIStruct>();
				char tempCol = inCol;
                if (inCol == 'w')
                {
                    inCol = 'b';
                    colour = true;
                }
                else
                {
                    inCol = 'w';
                }
                for (int y = 0; y < 8; y++)
				{
					for (int x = 0; x < 8; x++)
					{
                        if (board[y, x].Contains(tempCol))
						{
							PictureBox piece = squareBoard[y, x];
                            foreach (int[] i in CheckAllAvailables(piece, col))
							{
								
								AIStruct AIMove2 = MiniMax(piece, squareBoard[i[0], i[1]], depth, inCol);
								AIMoves.Add(AIMove2);
								SetupPieces();
								ResetColours(false);
							}
						}
                    }
				}
				double biggestAdv = -9999;
				foreach (AIStruct AIMove in AIMoves)
				{
					if (AIMove.adv > biggestAdv)
					{
						biggestAdv = AIMove.adv;
					}
				}
				List<AIStruct> bestAIMoves = new List<AIStruct>();
				foreach (AIStruct AIMove in AIMoves)
				{
					if (AIMove.adv == biggestAdv)
					{
						bestAIMoves.Add(AIMove);
					}
				}
				int num = rnd.Next(bestAIMoves.Count);
				board = CloneABoard(bestAIMoves[num].inBoard);
				RemoveLastMove();
				foreach (int[] i in bestAIMoves[num].inLastMoves)
				{
					LastMoves.Add(i);
				}
				ResetColours(true);
				SetupPieces();
				toc.Play();
				col = inCol;
				AddMove();
				CreateSetupText();
				replayMoves.Add(setupText);
				moves.Add(CloneBoard());
				movesNum++;
				if (CheckCanMove(colour))
				{
					int endGameStatus = 1;
					if (CheckCheck(colour))
					{
						enabled = false;
						timer.Stop(); timerB.Stop();
						CheckMateText.Visible = true;
						CheckMateText.Text = Environment.NewLine + Environment.NewLine;
                        if (col == 'w')
                        {
                            CheckMateText.Text += "Black wins";
                        }
                        else
                        {
                            CheckMateText.Text += "White wins";
                            endGameStatus = 0;
                        }
                        CheckMateText.Text += Environment.NewLine + "By Checkmate!";
						XButton.Visible = true;
					}
					else if (CheckNoPieces(inCol))
					{
						enabled = false; timer.Stop(); timerB.Stop();
						CheckMateText.Visible = true;
						CheckMateText.Text = Environment.NewLine + Environment.NewLine;
						if (col == 'w')
						{
							CheckMateText.Text += "Black wins";
						}
						else
						{
							CheckMateText.Text += "White wins";
                            endGameStatus = 0;
                        }
						CheckMateText.Text += Environment.NewLine + "By taking every piece!";
						XButton.Visible = true;
					}
					else
					{
						enabled = false; timer.Stop(); timerB.Stop();
						CheckMateText.Visible = true;
						CheckMateText.Text = Environment.NewLine + Environment.NewLine + "Stalemate!";
						XButton.Visible = true;
						endGameStatus = 2;
					}
					if (loggedIn)
					{
						SQLiteConnection conn = CreateConnection();
						int gameID = InsertNewGame(conn, ID, endGameStatus);
						foreach (string game in replayMoves)
						{
							InsertNewGameMove(conn, gameID, game);
						}
					}
					replayMoves.Clear();
				}
				CheckColour(false);
				int pieces = 0;
                for (int q = 0; q < 8; q++)
				{
					for (int w = 0; w < 8; w++)
					{
						if (board[q, w] != "_" && !board[q, w].Contains('K'))
						{
							pieces++;
						}
					}
				}
				if (pieces < 1)
				{
					enabled = false; timer.Stop(); timerB.Stop();
					CheckMateText.Visible = true;
					CheckMateText.Text = Environment.NewLine + Environment.NewLine + "Draw by insufficient material!";
					XButton.Visible = true;
					if (loggedIn)
					{
						SQLiteConnection conn = CreateConnection();
						int gameID = InsertNewGame(conn, ID, 3);
						foreach (string game in replayMoves)
						{
							InsertNewGameMove(conn, gameID, game);
						}
					}
				}
				ResetColours(false);
				BackMoveButton.Enabled = true;
			}
        }

		private AIStruct MiniMax(PictureBox piece1, PictureBox piece2, int inDepth, char inCol)
		{
			string[,] tempBoard = CloneABoard(board);
			AIStruct AIMove = new AIStruct();
            y1 = piece1.Top / 60; y2 = piece2.Top / 60; x1 = piece1.Left / 60; x2 = piece2.Left / 60;
			AIMove.inLastMoves = new List<int[]>()
			{
				new int[] { y1, x1 },
				new int[] { y2, x2 }
			};
            HasMoved(piece1);
            RemoveDoubleMoved(piece1);
            DoubleMoved(piece1, piece2);
            EnPassant(piece1, piece2);
            Castle(piece1, piece2);
            string name = piece1.Name;
            board[y1, x1] = "_";
            board[y2, x2] = name;
			MediumAIPromote();
            AIMove.adv = EvaluatePosAI(board);
			bool isMax = true;
			bool colour = true;
			int multiplier = -1;
			if (inCol == 'w')
			{
				isMax = false;
				multiplier = 1;
				colour = false;
			}
			char tempCol = inCol;
            if (inCol == 'w')
            {
                inCol = 'b';
            }
            else
            {
                inCol = 'w';
            }
            if (inDepth > 0)
            {
				inDepth--;
                List<AIStruct> AIMoves = new List<AIStruct>();
				int allAvailablesCount = 0;
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (board[y, x].Contains(tempCol))
                        {
                            PictureBox piece = squareBoard[y, x];
							List<int[]> allAvailables = CheckAllAvailables(piece, tempCol);
							allAvailablesCount += allAvailables.Count;
                            foreach (int[] i in allAvailables)
							{
                                AIStruct AIMove2 = MiniMax(piece, squareBoard[i[0], i[1]], inDepth, inCol);
                                AIMoves.Add(AIMove2); 
                                ResetColours(true);
                            }
                        }
                    }
                }
                if (allAvailablesCount < 1)
                {
					AIStruct AIMove2;
					if (CheckCanMove(colour))
					{
                        if (CheckCheck(colour))
                        {
                            AIMove2 = new AIStruct()
                            {
                                adv = multiplier * 9999,
                                inLastMoves = AIMove.inLastMoves,
                                inBoard = CloneABoard(board),
                            };
                        }
                        else
                        {
                            AIMove2 = new AIStruct()
                            {
                                adv = multiplier * 0,
                                inLastMoves = AIMove.inLastMoves,
                                inBoard = CloneABoard(board),
                            };
                        }
                        board = CloneABoard(tempBoard);
                        return AIMove2;
                    }

                }
                double miniMax;
				if (isMax)
				{
					miniMax = -9999;
					foreach (AIStruct AIMove2 in AIMoves)
					{
						if (AIMove2.adv > miniMax)
						{
							miniMax = AIMove2.adv;
						}
					}
				}
				else
				{
					miniMax = 9999;
					foreach (AIStruct AIMove2 in AIMoves)
					{
						if (AIMove2.adv < miniMax)
						{
							miniMax = AIMove2.adv;
						}
					}
				}
				AIMove.adv = miniMax;
            }
            AIMove.inBoard = CloneABoard(board);
            board = CloneABoard(tempBoard);
            return AIMove;
        }

        static string[,] CloneABoard(string[,] inBoard)
        {
            string[,] tempBoard = new string[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    tempBoard[i, j] = inBoard[i, j];
                }
            }
            return tempBoard;
        }

        private void MediumAIPromote()
		{
			for (int j = 0; j < 8; j++)
			{
				if (board[7, j].Contains('p'))
				{
					board[7, j] = board[7, j].Replace('p', 'Q');
				}
				if (board[0, j].Contains('p'))
				{
					board[0, j] = board[0, j].Replace('p', 'Q');
				}
			}
		}

		private double[,] ReverseNegateInts(double[,] inBoard)
		{
			double[,] tempBoard = new double[8, 8];
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					tempBoard[i , j] = -inBoard[7 -i , 7 -j];
				}
			}
			return tempBoard;
		}

		private double EvaluatePosAI(string[,] afterPos)
		{
            Dictionary<string, double> pieceValue = new Dictionary<string, double>
            {
                { "wp", -10 },
                { "wk", -30 },
                { "wB", -30 },
                { "wR", -50 },
                { "wQ", -90 },
                { "wK", -900 },
                { "bp", 10 },
                { "bk", 30 },
                { "bB", 30 },
                { "bR", 50 },
                { "bQ", 90 },
                { "bK", 900 }
            };
			
			double[,] positionPointsKing = new double[8, 8]
			{
				{ 2, 3, 1, 0, 0, 1, 3, 2 },
				{ 2, 2, 0, 0, 0, 0, 2, 2 },
				{ -1, -2, -2, -2, -2, -2, -2, -1 },
				{ -2, -3, -3, -4, -4, -3, -3, -2 },
                { -3, -4, -4, -5, -5, -4, -4, -3 },
                { -3, -4, -4, -5, -5, -4, -4, -3 },
                { -2, -3, -3, -4, -4, -3, -3, -2 },
                { -2, -3, -3, -4, -4, -3, -3, -2 },
            };
            double[,] positionPointsQueen = new double[8, 8]
            {
                { -2, -1, -1, -0.5, -0.5, -1, -1, -2 },
                { -1, 0, 0, 0, 0, 0.5, 0, -1 },
                { -1, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, -1 },
                { -0.5, 0, 0.5, 0.5, 0.5, 0.5, 0, 0 },
                { -0.5, 0, 0.5, 0.5, 0.5, 0.5, 0, -0.5 },
				{ -1, 0, 0.5, 0.5, 0.5, 0.5, 0, -1 },
                { -1, 0, 0, 0, 0, 0, 0, -1 },
                { -2, -1, -1, -0.5, -0.5, -1, -1, -2 },
            };
			double[,] positionPointsRook = new double[8, 8]
			{
				{ 0, 0, 0, 0.5, 0.5, 0, 0, 0 },
				{ -0.5, 0, 0, 0, 0, 0, 0, -0.5 },
				{ -0.5, 0, 0, 0, 0, 0, 0, -0.5 },
				{ -0.5, 0, 0, 0, 0, 0, 0, -0.5 },
				{ -0.5, 0, 0, 0, 0, 0, 0, -0.5 },
				{ -0.5, 0, 0, 0, 0, 0, 0, -0.5 },
				{ -0.5, 1, 1, 1, 1, 1, 1, -0.5 },
				{ 0, 0, 0, 0, 0, 0, 0, 0 }
			};
            double[,] positionPointsBishop = new double[8, 8]
            {
				{ -2, -1, -1, -1, -1, -1, -1, -2 },
				{ -1, 0.5, 0, 0, 0, 0, 0.5, -1 },
				{ -1, 1, 1, 1, 1, 1, 1, 1 },
				{ -1, 0, 1, 1, 1, 1, 0, -1 },
				{ -1, 0.5, 0.5, 1, 1, 0.5, 0.5, -1 },
				{ -1, 0, 0.5, 1, 1, 0.5, 0, -1 },
				{ -1, 0, 0, 0, 0, 0, 0, -1 },
				{ -2, -1, -1, -1, -1, -1, -1, -2 }
            };
			double[,] positionPointsKnight = new double[8, 8]
			{
				{ -5, -4, -3, -3, -3, -3, -4, -5 },
				{ -4, -2, 0, 0.5, 0.5, 0, -2, 4 },
				{ -3, 0.5, 1, 1.5, 1.5, 1, 0.5, -3 },
				{ -3, 0, 1.5, 2, 2, 1.5, 0, -3 },
				{ -3, 0.5, 1.5, 2, 2, 1.5, 0, -3 },
				{ -3, 0, 1, 1.5, 1.5, 1, 0, -3 },
				{ -4, -2, 0, 0, 0, 0, -2, -4 },
				{ -5, -4, -3, -3, -3, -3, -4, -5 },
            };
			double[,] positionPointsPawn = new double[8, 8]
			{
				{ 0, 0, 0, 0, 0, 0, 0, 0 },
				{ 0.5, 1, 1, -2, -2, 1, 1, 0.5 },
				{ 0.5, -0.5, -1, 0, 0, -1, -0.5, 0.5 },
				{ 0, 0, 0, 2, 2, 0, 0, 0 },
				{ 0.5, 0.5, 1, 2.5, 2.5, 1, 0.5, 0.5 },
				{ 1, 1, 2, 3, 3, 2, 1, 1 },
				{ 5, 5, 5, 5, 5, 5, 5, 5 },
				{ 0, 0, 0, 0, 0, 0, 0, 0 }
			};
            double num = 0;
			if (movesNum < 30)
			{
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 8; j++)
					{
						if (afterPos[i, j].Contains("wp"))
						{
							num += ReverseNegateInts(positionPointsPawn)[i, j];
						}
						else if (afterPos[i, j].Contains("bp"))
						{
							num += positionPointsPawn[i, j];
						}
						else if (afterPos[i, j].Contains("wk"))
						{
							num += ReverseNegateInts(positionPointsKnight)[i, j];
						}
						else if (afterPos[i, j].Contains("bk"))
						{
							num += positionPointsKnight[i, j];
						}
						else if (afterPos[i, j].Contains("wB"))
						{
							num += ReverseNegateInts(positionPointsBishop)[i, j];
						}
						else if (afterPos[i, j].Contains("bB"))
						{
							num += positionPointsBishop[i, j];
						}
						else if (afterPos[i, j].Contains("wR"))
						{
							num += ReverseNegateInts(positionPointsRook)[i, j];
						}
						else if (afterPos[i, j].Contains("wR"))
						{
							num += positionPointsRook[i, j];
						}
						else if (afterPos[i, j].Contains("wQ"))
						{
							num += ReverseNegateInts(positionPointsQueen)[i, j];
						}
						else if (afterPos[i, j].Contains("bQ"))
						{
							num += positionPointsQueen[i, j];
						}
						else if (afterPos[i, j].Contains("wK"))
						{
							num += ReverseNegateInts(positionPointsKing)[i, j];
						}
						else if (afterPos[i, j].Contains("bK"))
						{
							num += positionPointsKing[i, j];
						}
					}
				}
			}
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (afterPos[i, j].Length > 1)
                    {
                        string s = afterPos[i, j][0].ToString() + afterPos[i, j][1].ToString();
                        num += pieceValue[s];
                    }
                }
            }
            return num;
        }

		private void boardSquare_DragDrop(object sender, DragEventArgs e)
		{
			PictureBox piece = squareBoard[dragDropCoords[0], dragDropCoords[1]];
			PieceMovement(piece, sender);
		}

        private void boardSquare_DragEnter(object sender, DragEventArgs e)
        {
			if (e.Data.GetDataPresent(DataFormats.Bitmap))
			{
				e.Effect = DragDropEffects.Copy;
			}
        }

		private List<int[]> CheckAllAvailables(PictureBox piece, char inCol)
		{
            List<int[]> availables = new List<int[]>();
            if (piece.Name.Contains('R'))
            {
                foreach (int[] i in RookAvailable(piece))
                {
                    if (CheckAvailable(i, piece, inCol))
                    {
                        availables.Add(i);
                    }
                }
            }
            else if (piece.Name.Contains('p'))
            {
                foreach (int[] i in PawnAvailable(piece))
                {
                    if (CheckAvailable(i, piece, inCol))
                    {
                        availables.Add(i);
                    }
                }
            }
            else if (piece.Name.Contains('B'))
            {
                foreach (int[] i in BishopAvailable(piece))
                {
                    if (CheckAvailable(i, piece, inCol))
                    {
                        availables.Add(i);
                    }
                }
            }
            else if (piece.Name.Contains('Q'))
            {
                foreach (int[] i in QueenAvailable(piece))
                {
                    if (CheckAvailable(i, piece, inCol))
                    {
                        availables.Add(i);
                    }
                }
            }
            else if (piece.Name.Contains('k'))
            {
                foreach (int[] i in KnightAvailable(piece))
                {
                    if (CheckAvailable(i, piece, inCol))
                    {
                        availables.Add(i);
                    }
                }
            }
            else if (piece.Name.Contains('K'))
            {
                foreach (int[] i in KingAvailable(piece))
                {
                    if (CheckAvailable(i, piece, inCol))
                    {
                        availables.Add(i);
                    }
                }
            }
            return availables;
        }

		private void PieceMovement(PictureBox piece, object sender2)
		{
            for (int k = 0; k < 8; k++)
            {
                for (int l = 0; l < 8; l++)
                {
                    if (squareBoard[k, l].BackColor == Color.FromArgb(59, 217, 72) || squareBoard[k, l].BackColor == Color.DarkRed)
                    {
                        piece = squareBoard[k, l];
                    }
                }
            }
            PictureBox piece2 = sender2 as PictureBox;
            if (piece2.Name.Contains('A'))
            {
				if (movesNum > 0)
				{
                    ResignButton.Text = "Resign";
                }
                ResetColours(true);
                RemoveLastMove();
                y1 = piece.Top / 60; y2 = piece2.Top / 60; x1 = piece.Left / 60; x2 = piece2.Left / 60;
                LastMoves.Add(new int[] { y1, x1 });
                LastMoves.Add(new int[] { y2, x2 });
                HasMoved(piece);
                RemoveDoubleMoved(piece);
                DoubleMoved(piece, piece2);
                EnPassant(piece, piece2);
                Castle(piece, piece2);
                string name = piece.Name;
                board[y1, x1] = "_";
                board[y2, x2] = name;
                Promote(false);
                if (enabled)
                {
                    if (soundEffects)
                    {
                        toc.Play();
                    }
                    AddMove();
                    if ((col == 'w' && rev || col == 'b') && !rev && AutomaticFlipsCheckBox.Checked)
                    {
                        enableRev = false;
                    }
                    else if ((col == 'w' && !rev || col == 'b' && rev) && AutomaticFlipsCheckBox.Checked)
                    {
                        enableRev = true;
                    }
                    else
                    {
                        enableRev = false;
                    }
                    if (enableRev)
                    {
                        SetupPieces();
                        ResetColours(false);
                        Refresh();
                        System.Threading.Thread.Sleep(500);
                        ReverseBoard();
                    }
                    SetupPieces();
                    CreateSetupText();
                    replayMoves.Add(setupText);
                    moves.Add(CloneBoard());
                    movesNum++;
                    bool colour = false;
                    if (col == 'w')
                    {
                        timerB.Start(); timer.Stop();
                        col = 'b';
                        colour = true;
                    }
                    else
                    {
                        timerB.Stop(); timer.Start();
                        col = 'w';
                    }
                    if (CheckCanMove(colour))
                    {
                        int endGameStatus = 0;
                        if (CheckCheck(colour))
                        {
                            enabled = false; timer.Stop(); timerB.Stop();
                            CheckMateText.Visible = true;
                            CheckMateText.Text = Environment.NewLine + Environment.NewLine;
                            if (colour)
                            {
                                CheckMateText.Text += "White wins";
                            }
                            else
                            {
                                CheckMateText.Text += "Black wins";
                                endGameStatus = 1;
                            }
                            CheckMateText.Text += Environment.NewLine + "By Checkmate!";
                            XButton.Visible = true;
                        }
                        else if (CheckNoPieces('w'))
                        {
                            enabled = false; timer.Stop(); timerB.Stop();
                            CheckMateText.Visible = true;
                            CheckMateText.Text = Environment.NewLine + Environment.NewLine;
                            CheckMateText.Text += "Black wins";
                            endGameStatus = 1;
                            CheckMateText.Text += Environment.NewLine + "By taking every piece!";
                            XButton.Visible = true;
                        }
                        else if (CheckNoPieces('b'))
                        {
                            enabled = false; timer.Stop(); timerB.Stop();
                            CheckMateText.Visible = true;
                            CheckMateText.Text = Environment.NewLine + Environment.NewLine;
                            CheckMateText.Text += "White wins";
                            endGameStatus = 0;
                            CheckMateText.Text += Environment.NewLine + "By taking every piece!";
                            XButton.Visible = true;
                        }
                        else
                        {
                            enabled = false; timer.Stop(); timerB.Stop();
                            CheckMateText.Visible = true;
                            CheckMateText.Text = Environment.NewLine + Environment.NewLine + "Stalemate!";
                            XButton.Visible = true;
                            endGameStatus = 2;
                        }
                        if (loggedIn)
                        {
                            SQLiteConnection conn = CreateConnection();
                            int gameID = InsertNewGame(conn, ID, endGameStatus);
                            foreach (string game in replayMoves)
                            {
                                InsertNewGameMove(conn, gameID, game);
                            }
                        }
                        replayMoves.Clear();
                    }
                    CheckColour(colour);
                    int pieces = 0;
                    for (int q = 0; q < 8; q++)
                    {
                        for (int w = 0; w < 8; w++)
                        {
                            if (board[q, w] != "_" && !board[q, w].Contains('K'))
                            {
                                pieces++;
                            }
                        }
                    }
                    if (pieces < 1)
                    {
                        enabled = false; timer.Stop(); timerB.Stop();
                        CheckMateText.Visible = true;
                        CheckMateText.Text = Environment.NewLine + Environment.NewLine + "Draw by insufficient material!";
                        XButton.Visible = true;
                        if (loggedIn)
                        {
                            SQLiteConnection conn = CreateConnection();
                            int gameID = InsertNewGame(conn, ID, 3);
                            foreach (string game in replayMoves)
                            {
                                InsertNewGameMove(conn, gameID, game);
                            }
                        }
                    }
                    ResetColours(false);
                    BackMoveButton.Enabled = true;
                    if (tutBot)
                    {
                        System.Threading.Thread.Sleep(100);
                        Refresh();
                        TutBotMove();
                    }
					if (AIMediumLevel)
					{
						Refresh();
						AIMovement(col);
					}
                }
            }
        }

		private bool CheckNoPieces(char colChar)
		{
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (board[i, j].Contains(colChar))
					{
						return false;
					}
				}
			}
			return true;
		}

		private void ReverseBoard()
		{
			rev = !rev;
			foreach (int[] i in LastMoves)
            {
				i[0] = 7 - i[0];
                i[1] = 7 - i[1];
            }
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					reverseBoard[i, j] = board[7 - i, 7 - j];
				}
			}
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					board[i, j] = reverseBoard[i, j];
				}
			}
			SwitchTimers();
			ReverseTransparentLetters();
		}

		private void RefreshTransparentLetters()
		{
			transparentA.Refresh();
			transparentB.Refresh();
			transparentC.Refresh();
			transparentD.Refresh();
			transparentE.Refresh();
			transparentF.Refresh();
			transparentG.Refresh();
			transparentH.Refresh();
			transparent1.Refresh();
			transparent2.Refresh();
			transparent3.Refresh();
			transparent4.Refresh();
			transparent5.Refresh();
			transparent6.Refresh();
			transparent7.Refresh();
			transparent8.Refresh();
		}

		private void ReverseTransparentLetters()
		{
			(transparent1.ForeColor, transparent8.ForeColor) = (transparent8.ForeColor, transparent1.ForeColor);
			(transparent1.Location, transparent8.Location) = (transparent8.Location, transparent1.Location);
			(transparent2.ForeColor, transparent7.ForeColor) = (transparent7.ForeColor, transparent2.ForeColor);
			(transparent2.Location, transparent7.Location) = (transparent7.Location, transparent2.Location);
			(transparent3.ForeColor, transparent6.ForeColor) = (transparent3.ForeColor, transparent6.ForeColor);
			(transparent3.Location, transparent6.Location) = (transparent3.Location, transparent6.Location);
			(transparent4.ForeColor, transparent5.ForeColor) = (transparent5.ForeColor, transparent4.ForeColor);
			(transparent4.Location, transparent5.Location) = (transparent5.Location, transparent4.Location);
			(transparentA.ForeColor, transparentH.ForeColor) = (transparentH.ForeColor, transparentA.ForeColor);
			(transparentA.Location, transparentH.Location) = (transparentH.Location, transparentA.Location);
			(transparentB.ForeColor, transparentG.ForeColor) = (transparentG.ForeColor, transparentB.ForeColor);
			(transparentB.Location, transparentG.Location) = (transparentG.Location, transparentB.Location);
			(transparentC.ForeColor, transparentF.ForeColor) = (transparentF.ForeColor, transparentC.ForeColor);
			(transparentC.Location, transparentF.Location) = (transparentF.Location, transparentC.Location);
			(transparentD.ForeColor, transparentE.ForeColor) = (transparentE.ForeColor, transparentD.ForeColor);
			(transparentD.Location, transparentE.Location) = (transparentE.Location, transparentD.Location);
		}

		private void SetupTransparentText()
		{
			transparentA.ForeColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
			transparentB.ForeColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
			transparentC.ForeColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
			transparentD.ForeColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
			transparentE.ForeColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
			transparentF.ForeColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
			transparentG.ForeColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
			transparentH.ForeColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
			transparent1.ForeColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
			transparent2.ForeColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
			transparent3.ForeColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
			transparent4.ForeColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
			transparent5.ForeColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
			transparent6.ForeColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
			transparent7.ForeColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
			transparent8.ForeColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
		}

		private void SetupTimer()
		{
			if (timerTick)
			{
				timer = new System.Timers.Timer
				{
					Interval = 1000
				};
				timer.Elapsed += OnTimeEvent;
				timerB = new System.Timers.Timer
				{
					Interval = 1000
				};
				timerB.Elapsed += OnTimeEventB;
			}
		}

		private void SwitchTimers()
		{
			if (timerTick)
			{
				(InactiveTimer.Location, ActiveTimer.Location) = (ActiveTimer.Location, InactiveTimer.Location);
			}
		}

		private void OnTimeEvent(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (timerTick)
			{
				try
				{
					Invoke(new Action(() =>
					{
						int s, m;
						if (ActiveTimer.Text.Length == 5)
						{
							s = int.Parse(ActiveTimer.Text[3].ToString() + ActiveTimer.Text[4].ToString());
							m = int.Parse(ActiveTimer.Text[0].ToString() + ActiveTimer.Text[1].ToString());
						}
						else
						{
							s = int.Parse(ActiveTimer.Text[2].ToString() + ActiveTimer.Text[3].ToString());
							m = int.Parse(ActiveTimer.Text[0].ToString());
						}
						s--;
						if (s < 0)
						{
							s = 59;
							m--;
						}
						ActiveTimer.Text = String.Format("{0}:{1}", m.ToString().PadLeft(2, '0'), s.ToString().PadLeft(2, '0'));
						if (m == -1)
						{
							enabled = false;
							CheckMateText.Visible = true;
                            CheckMateText.Text = Environment.NewLine + Environment.NewLine;
                            CheckMateText.Text += "Black wins";
                            CheckMateText.Text += Environment.NewLine + "by Time fail!";
                            XButton.Visible = true;
							timer.Stop();
							ActiveTimer.Text = "00:00";
                            if (loggedIn)
                            {
                                SQLiteConnection conn = CreateConnection();
                                int gameID = InsertNewGame(conn, ID, 1);
                                foreach (string game in replayMoves)
                                {
                                    InsertNewGameMove(conn, gameID, game);
                                }
                            }
							replayMoves.Clear();
                        }
					}));
				}
				catch (Exception)
				{

				}
			}
		}

		private void OnTimeEventB(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (timerTick)
			{
				try
				{
					Invoke(new Action(() =>
					{
						int s, m;
						if (InactiveTimer.Text.Length == 5)
						{
							s = int.Parse(InactiveTimer.Text[3].ToString() + InactiveTimer.Text[4].ToString());
							m = int.Parse(InactiveTimer.Text[0].ToString() + InactiveTimer.Text[1].ToString());
						}
						else
						{
							s = int.Parse(InactiveTimer.Text[2].ToString() + InactiveTimer.Text[3].ToString());
							m = int.Parse(InactiveTimer.Text[0].ToString());
						}
						s--;
						if (s < 0)
						{
							s = 59;
							m--;
						}
						InactiveTimer.Text = String.Format("{0}:{1}", m.ToString().PadLeft(2, '0'), s.ToString().PadLeft(2, '0'));
						if (m == -1)
						{
							enabled = false;
							CheckMateText.Visible = true;
                            CheckMateText.Text = Environment.NewLine + Environment.NewLine;
                            CheckMateText.Text += "White wins";
                            CheckMateText.Text += Environment.NewLine + "by Time fail!";
                            XButton.Visible = true;
							timerB.Stop();
							InactiveTimer.Text = "00:00";
                            if (loggedIn)
                            {
                                SQLiteConnection conn = CreateConnection();
                                int gameID = InsertNewGame(conn, ID, 0);
                                foreach (string game in replayMoves)
                                {
                                    InsertNewGameMove(conn, gameID, game);
                                }
                            }
							replayMoves.Clear();
                        }
					}));
				}
				catch (Exception)
				{

				}
			}
		}

		private void SetupPieces()
		{
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					if (board[y, x].Length > 1)
					{
						if (board[y, x][0].ToString() + board[y, x][1].ToString() == "bR")
						{
                            if (chessPiece == 0)
                            {
                                squareBoard[y, x].Image = Properties.Resources.Black_Rook;
                            }
                            else if (chessPiece == 1)
							{
								squareBoard[y, x].Image = Properties.Resources.BRook1;
							}
                            else if (chessPiece == 2)
                            {
                                squareBoard[y, x].Image = Properties.Resources.AnarchyBRook;
                            }
                            else if (chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.Black_King;
                            }
                        }
						else if (board[y, x][0].ToString() + board[y, x][1].ToString() == "bk")
						{
                            if (chessPiece == 0)
                            {
                                squareBoard[y, x].Image = Properties.Resources.Black_Knight;
                            }
                            else if (chessPiece == 1)
                            {
                                squareBoard[y, x].Image = Properties.Resources.BKnight1;
                            }
                            else if (chessPiece == 2)
                            {
                                squareBoard[y, x].Image = Properties.Resources.AnarchyBKnight;
                            }
                            else if (chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.Black_King;
                            }
                        }
						else if (board[y, x][0].ToString() + board[y, x][1].ToString() == "bB")
						{
                            if (chessPiece == 0)
                            {
                                squareBoard[y, x].Image = Properties.Resources.Black_Bishop;
                            }
                            else if (chessPiece == 1)
                            {
                                squareBoard[y, x].Image = Properties.Resources.BBishop1;
                            }
                            else if (chessPiece == 2)
                            {
                                squareBoard[y, x].Image = Properties.Resources.AnarchyBBishop;
                            }
                            else if (chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.Black_King;
                            }
                        }
						else if (board[y, x][0].ToString() + board[y, x][1].ToString() == "bQ")
						{
                            if (chessPiece == 0)
                            {
                                squareBoard[y, x].Image = Properties.Resources.Black_Queen;
                            }
                            else if (chessPiece == 1)
                            {
                                squareBoard[y, x].Image = Properties.Resources.BQueen1;
                            }
                            else if (chessPiece == 2)
                            {
                                squareBoard[y, x].Image = Properties.Resources.AnarchyBQueen;
                            }
                            else if (chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.Black_King;
                            }
                        }
						else if (board[y, x][0].ToString() + board[y, x][1].ToString() == "bK")
						{
                            if (chessPiece == 0 || chessPiece == 2 || chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.Black_King;
                            }
                            else if (chessPiece == 1)
                            {
                                squareBoard[y, x].Image = Properties.Resources.BKing1;
                            }
                        }
						else if (board[y, x][0].ToString() + board[y, x][1].ToString() == "bp")
						{
                            if (chessPiece == 0)
                            {
                                squareBoard[y, x].Image = Properties.Resources.Black_Pawn;
                            }
                            else if (chessPiece == 1)
                            {
                                squareBoard[y, x].Image = Properties.Resources.BPawn1;
                            }
                            else if (chessPiece == 2)
                            {
                                squareBoard[y, x].Image = Properties.Resources.AnarchyBPawn;
                            }
                            else if (chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.Black_King;
                            }
                        }
						else if (board[y, x][0].ToString() + board[y, x][1].ToString() == "wR")
						{
                            if (chessPiece == 0)
                            {
                                squareBoard[y, x].Image = Properties.Resources.White_Rook;
                            }
                            else if (chessPiece == 1)
                            {
                                squareBoard[y, x].Image = Properties.Resources.WRook1;
                            }
                            else if (chessPiece == 2)
                            {
                                squareBoard[y, x].Image = Properties.Resources.AnarchyWRook;
                            }
                            else if (chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.White_King;
                            }
                        }
						else if (board[y, x][0].ToString() + board[y, x][1].ToString() == "wk")
						{
                            if (chessPiece == 0)
                            {
                                squareBoard[y, x].Image = Properties.Resources.White_Knight;
                            }
                            else if (chessPiece == 1)
                            {
                                squareBoard[y, x].Image = Properties.Resources.WKnight1;
                            }
                            else if (chessPiece == 2)
                            {
                                squareBoard[y, x].Image = Properties.Resources.AnarchyWKnight;
                            }
                            else if (chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.White_King;
                            }
                        }
						else if (board[y, x][0].ToString() + board[y, x][1].ToString() == "wB")
						{
                            if (chessPiece == 0)
                            {
                                squareBoard[y, x].Image = Properties.Resources.White_Bishop;
                            }
                            else if (chessPiece == 1)
                            {
                                squareBoard[y, x].Image = Properties.Resources.WBishop1;
                            }
                            else if (chessPiece == 2)
                            {
                                squareBoard[y, x].Image = Properties.Resources.AnarchyWBishop;
                            }
                            else if (chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.White_King;
                            }
                        }
						else if (board[y, x][0].ToString() + board[y, x][1].ToString() == "wQ")
						{
                            if (chessPiece == 0)
                            {
                                squareBoard[y, x].Image = Properties.Resources.White_Queen;
                            }
                            else if (chessPiece == 1)
                            {
                                squareBoard[y, x].Image = Properties.Resources.WQueen1;
                            }
                            else if (chessPiece == 2)
                            {
                                squareBoard[y, x].Image = Properties.Resources.AnarchyWQueen;
                            }
                            else if (chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.White_King;
                            }
                        }
						else if (board[y, x][0].ToString() + board[y, x][1].ToString() == "wK")
						{
                            if (chessPiece == 0 || chessPiece == 2 || chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.White_King;
                            }
                            else if (chessPiece == 1)
                            {
                                squareBoard[y, x].Image = Properties.Resources.WKing1;
                            }
                        }
						else if (board[y, x][0].ToString() + board[y, x][1].ToString() == "wp")
						{
                            if (chessPiece == 0)
                            {
                                squareBoard[y, x].Image = Properties.Resources.White_Pawn;
                            }
                            else if (chessPiece == 1)
                            {
                                squareBoard[y, x].Image = Properties.Resources.WPawn1;
                            }
                            else if (chessPiece == 2)
                            {
                                squareBoard[y, x].Image = Properties.Resources.AnarchyWPawn;
                            }
                            else if (chessPiece == 3)
                            {
                                squareBoard[y, x].Image = Properties.Resources.White_King;
                            }
                        }
						squareBoard[y, x].Name = board[y, x];
					}
					else
					{
						squareBoard[y, x].Image = null;
						squareBoard[y, x].Name = "";
					}
				}
			}
            int num = PieceAdv();
            string s = ", With advantage for black";
            if (num > 0)
            {
                s = ", With advantage for white";
            }
            else if (num == 0)
            {
                s = "";
            }
            PieceAdvText.Text = "Piece Value: " + Math.Abs(num).ToString() + s;
            RefreshTransparentLetters();
		}

		private void ShowAvailable(int[] i)
		{
			if (board[i[0], i[1]] == "_")
			{
				squareBoard[i[0], i[1]].Image = Properties.Resources.available_place_gray;
				squareBoard[i[0], i[1]].Name = "Av";
			}
			else
			{
				squareBoard[i[0], i[1]].BackgroundImage = Properties.Resources.Available_On_Pieces1;
				squareBoard[i[0], i[1]].Name += "A";
			}
		}

		private void Promote(bool toQueen)
		{
			ResetColours(false);
			for (int i = 0; i < 8; i++)
			{
				int k = 0;
				for (int j = 0; j < 2; j++)
				{
					if (j != 0)
					{
						k = 7;
					}
                    if (board[k, i].Contains("wp") && (!rev && k == 0 || rev && k == 7) || board[k, i].Contains("bp") && (!rev && k == 7 || rev && k == 0))
                    {
                        if (promotionRange && !toQueen)
                        {
                            Promotion.Visible = true;
                            listBox1.Visible = true;
                            enabled = false;
                        }
                        else
                        {
                            string pieceName = "";
                            foreach (char c in board[k, i])
                            {
                                if (c == 'M')
                                {

                                }
                                else if (c != 'p')
                                {
                                    pieceName += c.ToString();
                                }
                                else if (c == 'p')
                                {
                                    pieceName += "Q";
                                }
                            }
                            board[k, i] = pieceName;
                        }
                    }
                }
			}
		}

		private void PromoteBot()
		{
			for (int i = 0; i < 8; i++)
            {
                int k = 0;
				for (int j = 0; j < 2; j++)
				{
					if (j != 0)
					{
						k = 7;
					}
					char pieceToPromote;
                    int num = rnd.Next(0, 6);
                    if (num > 2)
                    {
                        num = 3;
                    }
                    if (board[k, i].Contains("wp") && (!rev && k == 0 || rev && k == 7) || board[k, i].Contains("bp") && (!rev && k == 7 || rev && k == 0))
					{
						if (num == 3)
						{
							pieceToPromote = 'Q';
						}
						else if (num == 2)
						{
							pieceToPromote = 'B';
						}
						else if (num == 1)
						{
							pieceToPromote = 'k';
						}
						else
						{
							pieceToPromote = 'R';
						}
						board[k, i] = board[k, i].Replace('p', pieceToPromote);
					}
				}
			}
		}

		private bool CheckAvailable(int[] i, PictureBox piece)
		{
			bool colour = true; bool isAvailable = true;
			if (col == 'w')
			{
				colour = false;
			}
			RemoveCheck();
			int y1 = piece.Top / 60, y2 = i[0], x1 = piece.Left / 60, x2 = i[1];
			string name = piece.Name;
			string temp1 = board[y1, x1], temp2 = board[y2, x2];
			board[y1, x1] = "_";
			board[y2, x2] = name;
			if (CheckCheck(colour))
			{
				isAvailable = false;
			}
			board[y1, x1] = temp1; board[y2, x2] = temp2;
			return isAvailable;
		}

        private bool CheckAvailable(int[] i, PictureBox piece, char inCol)
        {
            bool colour = true; bool isAvailable = true;
            if (inCol == 'w')
            {
                colour = false;
            }
            RemoveCheck();
            int y1 = piece.Top / 60, y2 = i[0], x1 = piece.Left / 60, x2 = i[1];
            string name = piece.Name;
            string temp1 = board[y1, x1], temp2 = board[y2, x2];
			string[,] tempBoard = CloneABoard(board);
            board[y1, x1] = "_";
            board[y2, x2] = name;
			EnPassant(piece, squareBoard[y2, x2]);
            if (CheckCheck(colour))
            {
                isAvailable = false;
            }
			board = CloneABoard(tempBoard);
            return isAvailable;
        }

        private void RemoveCheck()
		{
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (board[i, j].Contains('K') && board[i, j].Contains('C'))
					{
						string pieceName = "", name = board[i, j];
						for (int k = 0; k < name.Length; k++)
						{
							if (name[k] != 'C')
							{
								pieceName += name[k];
							}
						}
						board[i, j] = pieceName;
					}
				}
			}
		}

		private void GoThroughAvailable(int i, int j, bool colour)
		{
			char c;
			if (colour)
			{
				c = 'w';
			}
			else
			{
				c = 'b';
			}
			PictureBox piece = squareBoard[i, j];
			if (board[i, j].Contains('R') && board[i, j].Contains(c))
			{
				RookAvailable(piece);
			}
			else if (board[i, j].Contains('p') && board[i, j].Contains(c))
			{
				PawnAvailable(piece);
			}
			else if (board[i, j].Contains('B') && board[i, j].Contains(c))
			{
				BishopAvailable(piece);
			}
			else if (board[i, j].Contains('k') && board[i, j].Contains(c))
			{
				KnightAvailable(piece);
			}
			else if (board[i, j].Contains('Q') && board[i, j].Contains(c))
			{
				QueenAvailable(piece);
			}
			else if (board[i, j].Contains('K') && board[i, j].Contains(c))
			{
				KingAvailable(piece);
			}
		}

		private bool CheckCanMove(bool colour)
		{
			char col0 = 'w';
			if (colour)
			{
				col0 = 'b';
			}
			bool isCheckMate = true;
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					if (board[y, x][0] == col0)
					{
						if (board[y, x].Contains('p'))
						{
							foreach (int[] i in PawnAvailable(squareBoard[y, x]))
							{
								if (CheckAvailable(i, squareBoard[y, x]))
								{
									isCheckMate = false;
								}
							}
						}
						else if (board[y, x].Contains('R'))
						{
							foreach (int[] i in RookAvailable(squareBoard[y, x]))
							{
								if (CheckAvailable(i, squareBoard[y, x]))
								{
									isCheckMate = false;
								}
							}
						}
						else if (board[y, x].Contains('B'))
						{
							foreach (int[] i in BishopAvailable(squareBoard[y, x]))
							{
								if (CheckAvailable(i, squareBoard[y, x]))
								{
									isCheckMate = false;
								}
							}
						}
						else if (board[y, x].Contains('k'))
						{
							foreach (int[] i in KnightAvailable(squareBoard[y, x]))
							{
								if (CheckAvailable(i, squareBoard[y, x]))
								{
									isCheckMate = false;
								}
							}
						}
						else if (board[y, x].Contains('Q'))
						{
							foreach (int[] i in QueenAvailable(squareBoard[y, x]))
							{
								if (CheckAvailable(i, squareBoard[y, x]))
								{
									isCheckMate = false;
								}
							}
						}
						else if (board[y, x].Contains('K'))
						{
							foreach (int[] i in KingAvailable(squareBoard[y, x]))
							{
								if (CheckAvailable(i, squareBoard[y, x]))
								{
									isCheckMate = false;
								}
							}
						}
					}
				}
			}
			return isCheckMate;
		}

		private bool CheckCheck(bool colour)
		{
			char c;
			if (colour)
			{
				c = 'b';
			}
			else
			{
				c = 'w';
			}
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					GoThroughAvailable(i, j, colour);
				}
			}
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (board[i, j].Contains('K') && board[i, j].Contains('C') && board[i, j].Contains(c))
					{
						return true;
					}
				}
			}
			return false;
		}
		
		private void CheckColour(bool colour)
        {
			char c;
			if (colour)
			{
				c = 'b';
			}
			else
			{
				c = 'w';
			}
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					GoThroughAvailable(i, j, colour);
				}
			}
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (board[i, j].Contains('K') && board[i, j].Contains('C') && board[i, j].Contains(c))
					{
						squareBoard[i, j].BackColor = Color.Red;
					}
				}
			}
		}

		private void Castle(PictureBox piece, PictureBox piece2)
		{
			string pieceName;
			int y = piece.Top / 60, x = piece.Left / 60;
			int xx = piece2.Left / 60;/*
			if (piece2.Left / 60 - piece.Left / 60 == 2 && piece.Name[0] == 'w' && piece.Name.Contains('K'))
			{
				pieceName = board[7, 7] + "M";
				board[7, 7] = "_";
				board[7, 5] = pieceName;
				LastMoves.Add(new int[] { 7, 7 });
				LastMoves.Add(new int[] { 7, 5 });
			}
			else if (piece2.Left / 60 - piece.Left / 60 == -2 && piece.Name[0] == 'w' && piece.Name.Contains('K'))
			{
				pieceName = board[7, 0] + "M";
				board[7, 0] = "_";
				board[7, 3] = pieceName;
				LastMoves.Add(new int[] { 7, 0 });
				LastMoves.Add(new int[] { 7, 3 });
			}
			else if (piece2.Left / 60 - piece.Left / 60 == 2 && piece.Name[0] == 'b' && piece.Name.Contains('K'))
			{
				pieceName = board[7, 7] + "M";
				board[7, 7] = "_";
				board[7, 4] = pieceName;
				LastMoves.Add(new int[] { 7, 7 });
				LastMoves.Add(new int[] { 7, 4 });
			}
			else if (piece2.Left / 60 - piece.Left / 60 == -2 && piece.Name[0] == 'b' && piece.Name.Contains('K'))
			{
				pieceName = board[7, 0] + "M";
				board[7, 0] = "_";
				board[7, 2] = pieceName;
				LastMoves.Add(new int[] { 7, 0 });
				LastMoves.Add(new int[] { 7, 2 });
			}*/
			if (xx - x > 1 && piece.Name.Contains('K'))
            {
				pieceName = board[y, 7] + "M";
				board[y, 7] = "_";
				board[y, xx - 1] = pieceName;
				SetupPieces();
			}
			else if (xx - x < - 1 && piece.Name.Contains('K'))
            {
				pieceName = board[y, 0] + "M";
				board[y, 0] = "_";
				board[y, xx + 1] = pieceName;
				SetupPieces();
			}
		}

		private void EnPassant(PictureBox piece, PictureBox piece2)
		{
			if (board[piece2.Top / 60, piece2.Left / 60] == "_" && piece.Name.Contains('p'))
			{
				if (piece.Left / 60 - piece2.Left / 60 == -1 || piece.Left / 60 - piece2.Left / 60 == 1)
				{
					board[piece2.Top / 60 + 1, piece2.Left / 60] = "_";
				}
			}
		}

		private void DoubleMoved(PictureBox piece, PictureBox piece2)
		{
			if (piece.Name.Contains('p'))
			{
				if (piece.Top / 60 - piece2.Top / 60 == 2 || piece.Top / 60 - piece2.Top / 60 == -2)
				{
					piece.Name += "D";
					board[piece.Top / 60, piece.Left / 60] += "D";
				}
			}
		}

		private void RemoveDoubleMoved(PictureBox piece)
		{
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (squareBoard[7 - i, 7 - j] != piece)
					{
						string pieceName = "";
						if (squareBoard[7 - i, 7 - j].Name.Contains('D'))
						{
							foreach (char c in squareBoard[7 - i, 7 - j].Name)
							{
								if (c != 'D')
								{
									pieceName += c.ToString();
								}
							}
							squareBoard[7 - i, 7 - j].Name = pieceName;
							board[7 - i, 7 - j] = pieceName;
						}
					}
				}
			}
		}

		private void HasMoved(PictureBox piece)
		{
			if (!piece.Name.Contains('M'))
			{
				piece.Name += 'M';
			}
		}

		private void ResetColours(bool removeCheckCol)
		{
			bool colour = false;
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					if (colour)
					{
						if (squareBoard[y, x].BackColor != Color.FromArgb(246, 190, 0) && squareBoard[y, x].BackColor != Color.Red && squareBoard[y, x].BackColor != Color.DarkRed)
						{
							squareBoard[y, x].BackColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
						}
						colour = false;
					}
					else
					{
						if (squareBoard[y, x].BackColor != Color.Orange && squareBoard[y, x].BackColor != Color.Red && squareBoard[y, x].BackColor != Color.DarkRed)
						{
							squareBoard[y, x].BackColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
						}
						colour = true;
					}
					if (x == 7)
					{
						colour = !colour;
					}
					if (squareBoard[y, x].BackgroundImage != null)
					{
						squareBoard[y, x].BackgroundImage = null;
						List<char> list = new List<char>();
						for (int i = 0; i < squareBoard[y, x].Name.Length; i++)
						{
							if (squareBoard[y, x].Name[i] != 'A')
							{
								list.Add(squareBoard[y, x].Name[i]);
							}
							squareBoard[y, x].Name = list.ToString();
						}
						SetupPieces();
					}
					if (squareBoard[y, x].Name == "Av")
					{
						squareBoard[y, x].Image = null;
						squareBoard[y, x].Name = "";
					}
					else if (squareBoard[y, x].BackColor == Color.DarkRed)
					{
						squareBoard[y, x].BackColor = Color.Red;
					}
					if (removeCheckCol)
                    {
						if ((y + x) % 2 != 0 && squareBoard[y, x].BackColor == Color.Red)
                        {
							squareBoard[y, x].BackColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
						}
						else if ((y + x) % 2 == 0 && squareBoard[y, x].BackColor == Color.Red)
						{
							squareBoard[y, x].BackColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
						}
					}
				}
			}
			foreach (int[] i in LastMoves)
			{
				if ((i[0] + i[1]) % 2 != 0)
				{
					squareBoard[i[0], i[1]].BackColor = Color.Orange;
				}
				else
				{
					squareBoard[i[0], i[1]].BackColor = Color.FromArgb(246, 190, 0);
				}
			}
		}

		private void RemoveLastMove()
		{
			LastMoves.Clear();
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					if (squareBoard[y, x].BackColor == Color.FromArgb(246, 190, 0))
					{
						squareBoard[y, x].BackColor = Color.FromArgb(boardCol[0], boardCol[1], boardCol[2]);
					}
					else if (squareBoard[y, x].BackColor == Color.Orange)
					{
						squareBoard[y, x].BackColor = Color.FromArgb(boardCol2[0], boardCol2[1], boardCol2[2], boardCol2[3]);
					}
				}
			}
		}

		private List<int[]> PawnAvailable(PictureBox piece)
        {
			List<int[]> list = new List<int[]>();
            try
            {
				string pieceName = board[piece.Top / 60, piece.Left / 60];
				int y = piece.Top / 60, x = piece.Left / 60;
				int num = 1;
				if (pieceName.Contains('b') && rev || pieceName.Contains('w') && !rev)
				{
					num = -1;
				}
				if (board[y + 1 * num, x] == "_")
				{
					list.Add(new int[] { y + 1 * num, x });
					if (!pieceName.Contains('M'))
					{
						if (board[y + 2 * num, x] == "_")
						{
							list.Add(new int[] { y + 2 * num, x });
						}
					}
				}
				if (x < 7)
				{
					if (!board[y + 1 * num, x + 1].Contains('_') && !board[y + 1 * num, x + 1].Contains(pieceName[0]))
					{
						list.Add(new int[] { y + 1 * num, x + 1 });
						if (board[y + 1 * num, x + 1].Contains('K'))
						{
							board[y + 1 * num, x + 1] += "C";
						}
					}
					if (board[y, x + 1].Contains('D'))
					{
						list.Add(new int[] { y + 1 * num, x + 1 });
					}
				}
				if (x > 0)
				{
					if (!board[y + 1 * num, x - 1].Contains('_') && !board[y + 1 * num, x - 1].Contains(pieceName[0]))
					{
						if (board[y + 1 * num, x - 1].Contains('K'))
						{
							board[y + 1 * num, x - 1] += "C";
						}
						list.Add(new int[] { y + 1 * num, x - 1 });
					}
					if (board[y, x - 1].Contains('D'))
					{
						list.Add(new int[] { y + 1 * num, x - 1 });
					}
				}
			}
			catch (Exception) { }
			return list;
		} 

		private List<int[]> RookAvailable(PictureBox piece)
		{
			List<int[]> list = new List<int[]>();
            try
            {
				string pieceName = board[piece.Top / 60, piece.Left / 60];
				int x = piece.Left / 60, y = piece.Top / 60;
				for (int i = y - 1; i >= 0 / 60; i--)
				{
					if (board[i, x] != "_")
					{
						if (!board[i, x].Contains(pieceName[0]))
						{
							if (board[i, x].Contains('K'))
							{
								board[i, x] += "C";
							}
							list.Add(new int[] { i, x });
						}
						break;
					}
					list.Add(new int[] { i, x });
				}
				for (int i = y + 1; i < 8; i++)
				{
					if (board[i, x] != "_")
					{
						if (!board[i, x].Contains(pieceName[0]))
						{
							if (board[i, x].Contains('K'))
							{
								board[i, x] += "C";
							}
							list.Add(new int[] { i, x });
						}
						break;
					}
					list.Add(new int[] { i, x });
				}
				for (int i = x - 1; i >= 0; i--)
				{
					if (board[y, i] != "_")
					{
						if (!board[y, i].Contains(pieceName[0]))
						{
							if (board[y, i].Contains('K'))
							{
								board[y, i] += "C";
							}
							list.Add(new int[] { y, i });
						}
						break;
					}
					list.Add(new int[] { y, i });
				}
				for (int i = x + 1; i < 8; i++)
				{
					if (board[y, i] != "_")
					{
						if (!board[y, i].Contains(pieceName[0]))
						{
							if (board[y, i].Contains('K'))
							{
								board[y, i] += "C";
							}
							list.Add(new int[] { y, i });
						}
						break;
					}
					list.Add(new int[] { y, i });
				}
			}
			catch (Exception) { }
			return list;
		}

		private List<int[]> BishopAvailable(PictureBox piece)
		{
			List<int[]> list = new List<int[]>();
			try
			{
				string pieceName = board[piece.Top / 60, piece.Left / 60];
				int y = piece.Top / 60 + 1;
				for (int x = piece.Left / 60 + 1; x < 8; x++)
				{
					if (y > 7)
					{
						break;
					}
					if (board[y, x] != "_")
					{
						if (!board[y, x].Contains(pieceName[0]))
						{
							if (board[y, x].Contains('K'))
							{
								board[y, x] += "C";
							}
							list.Add(new int[] { y, x });
						}
						break;
					}
					list.Add(new int[] { y, x });
					y++;
				}
				y = piece.Top / 60 - 1;
				for (int x = piece.Left / 60 + 1; x < 8; x++)
				{
					if (y < 0)
					{
						break;
					}
					if (board[y, x] != "_")
					{
						if (!board[y, x].Contains(pieceName[0]))
						{
							if (board[y, x].Contains('K'))
							{
								board[y, x] += "C";
							}
							list.Add(new int[] { y, x });
						}
						break;
					}
					list.Add(new int[] { y, x });
					y--;
				}
				y = piece.Top / 60 + 1;
				for (int x = piece.Left / 60 - 1; x >= 0; x--)
				{
					if (y > 7)
					{
						break;
					}
					if (board[y, x] != "_")
					{
						if (!board[y, x].Contains(pieceName[0]))
						{
							if (board[y, x].Contains('K'))
							{
								board[y, x] += "C";
							}
							list.Add(new int[] { y, x });
						}
						break;
					}
					list.Add(new int[] { y, x });
					y++;
				}
				y = piece.Top / 60 - 1;
				for (int x = piece.Left / 60 - 1; x >= 0; x--)
				{
					if (y < 0)
					{
						break;
					}
					if (board[y, x] != "_")
					{
						if (!board[y, x].Contains(pieceName[0]))
						{
							if (board[y, x].Contains('K'))
							{
								board[y, x] += "C";
							}
							list.Add(new int[] { y, x });
						}
						break;
					}
					list.Add(new int[] { y, x });
					y--;
				}
			}
			catch (Exception) { }
			return list;
		}

		private List<int[]> QueenAvailable(PictureBox piece)
		{
			List<int[]> list = new List<int[]>();
			foreach (int[] i in RookAvailable(piece))
			{
				list.Add(i);
			}
			foreach (int[] i in BishopAvailable(piece))
			{
				list.Add(i);
			}
			return list;
		}

		private List<int[]> KnightAvailable(PictureBox piece)
		{
			List<int[]> list = new List<int[]>();
			try
			{
				string pieceName = board[piece.Top / 60, piece.Left / 60];
				int y = piece.Top / 60, x = piece.Left / 60;
				if (y - 2 >= 0 && x - 1 >= 0)
				{
					if (board[y - 2, x - 1] == "_")
					{
						list.Add(new int[] { y - 2, x - 1 });
					}
					else
					{
						if (!board[y - 2, x - 1].Contains(pieceName[0]))
						{
							if (board[y - 2, x - 1].Contains('K'))
							{
								board[y - 2, x - 1] += "C";
							}
							list.Add(new int[] { y - 2, x - 1 });
						}
					}
				}
				if (y - 2 >= 0 && x + 1 < 8)
				{
					if (board[y - 2, x + 1] == "_")
					{
						list.Add(new int[] { y - 2, x + 1 });
					}
					else
					{
						if (!board[y - 2, x + 1].Contains(pieceName[0]))
						{
							if (board[y - 2, x + 1].Contains('K'))
							{
								board[y - 2, x + 1] += "C";
							}
							list.Add(new int[] { y - 2, x + 1 });
						}
					}
				}
				if (y - 1 >= 0 && x - 2 >= 0)
				{
					if (board[y - 1, x - 2] == "_")
					{
						list.Add(new int[] { y - 1, x - 2 });
					}
					else
					{
						if (!board[y - 1, x - 2].Contains(pieceName[0]))
						{
							if (board[y - 1, x - 2].Contains('K'))
							{
								board[y - 1, x - 2] += "C";
							}
							list.Add(new int[] { y - 1, x - 2 });
						}
					}
				}
				if (y - 1 >= 0 && x + 2 < 8)
				{
					if (board[y - 1, x + 2] == "_")
					{
						list.Add(new int[] { y - 1, x + 2 });
					}
					else
					{
						if (!board[y - 1, x + 2].Contains(pieceName[0]))
						{
							if (board[y - 1, x + 2].Contains('K'))
							{
								board[y - 1, x + 2] += "C";
							}
							list.Add(new int[] { y - 1, x + 2 });
						}
					}
				}
				if (y + 2 < 8 && x - 1 >= 0)
				{
					if (board[y + 2, x - 1] == "_")
					{
						list.Add(new int[] { y + 2, x - 1 });
					}
					else
					{
						if (!board[y + 2, x - 1].Contains(pieceName[0]))
						{
							if (board[y + 2, x - 1].Contains('K'))
							{
								board[y + 2, x - 1] += "C";
							}
							list.Add(new int[] { y + 2, x - 1 });
						}
					}
				}
				if (y + 2 < 8 && x + 1 < 8)
				{
					if (board[y + 2, x + 1] == "_")
					{
						list.Add(new int[] { y + 2, x + 1 });
					}
					else
					{
						if (!board[y + 2, x + 1].Contains(pieceName[0]))
						{
							if (board[y + 2, x + 1].Contains('K'))
							{
								board[y + 2, x + 1] += "C";
							}
							list.Add(new int[] { y + 2, x + 1 });
						}
					}
				}
				if (y + 1 < 8 && x - 2 >= 0)
				{
					if (board[y + 1, x - 2] == "_")
					{
						list.Add(new int[] { y + 1, x - 2 });
					}
					else
					{
						if (!board[y + 1, x - 2].Contains(pieceName[0]))
						{
							if (board[y + 1, x - 2].Contains('K'))
							{
								board[y + 1, x - 2] += "C";
							}
							list.Add(new int[] { y + 1, x - 2 });
						}
					}
				}
				if (y + 1 < 8 && x + 2 < 8)
				{
					if (board[y + 1, x + 2] == "_")
					{
						list.Add(new int[] { y + 1, x + 2 });
					}
					else
					{
						if (!board[y + 1, x + 2].Contains(pieceName[0]))
						{
							if (board[y + 1, x + 2].Contains('K'))
							{
								board[y + 1, x + 2] += "C";
							}
							list.Add(new int[] { y + 1, x + 2 });
						}
					}
				}
			}
			catch (Exception) { }
			return list;
		}

		private List<int[]> KingAvailable(PictureBox piece)
		{
			List<int[]> list = new List<int[]>();
			try
			{
				string pieceName = board[piece.Top / 60, piece.Left / 60];
				int y = piece.Top / 60, x = piece.Left / 60;
				if (y + 1 < 8)
				{
					if (board[y + 1, x] == "_")
					{
						list.Add(new int[] { y + 1, x });
					}
					else
					{
						if (!board[y + 1, x].Contains(pieceName[0]))
						{
							if (board[y + 1, x].Contains('K'))
							{
								board[y + 1, x] += "C";
							}
							list.Add(new int[] { y + 1, x });
						}
					}
				}
				if (y + 1 < 8 && x + 1 < 8)
				{
					if (board[y + 1, x + 1] == "_")
					{
						list.Add(new int[] { y + 1, x + 1 });
					}
					else
					{
						if (!board[y + 1, x + 1].Contains(pieceName[0]))
						{
							if (board[y + 1, x + 1].Contains('K'))
							{
								board[y + 1, x + 1] += "C";
							}
							list.Add(new int[] { y + 1, x + 1 });
						}
					}
				}
				if (x + 1 < 8)
				{
					if (board[y, x + 1] == "_")
					{
						list.Add(new int[] { y, x + 1 });
					}
					else
					{
						if (!board[y, x + 1].Contains(pieceName[0]))
						{
							if (board[y, x + 1].Contains('K'))
							{
								board[y, x + 1] += "C";
							}
							list.Add(new int[] { y, x + 1 });
						}
					}
				}
				if (y - 1 >= 0 && x + 1 < 8)
				{
					if (board[y - 1, x + 1] == "_")
					{
						list.Add(new int[] { y - 1, x + 1 });
					}
					else
					{
						if (!board[y - 1, x + 1].Contains(pieceName[0]))
						{
							if (board[y - 1, x + 1].Contains('K'))
							{
								board[y - 1, x + 1] += "C";
							}
							list.Add(new int[] { y - 1, x + 1 });
						}
					}
				}
				if (y - 1 >= 0)
				{
					if (board[y - 1, x] == "_")
					{
						list.Add(new int[] { y - 1, x });
					}
					else
					{
						if (!board[y - 1, x].Contains(pieceName[0]))
						{
							if (board[y - 1, x].Contains('K'))
							{
								board[y - 1, x] += "C";
							}
							list.Add(new int[] { y - 1, x });
						}
					}
				}
				if (y + 1 < 8 && x - 1 >= 0)
				{
					if (board[y + 1, x - 1] == "_")
					{
						list.Add(new int[] { y + 1, x - 1 });
					}
					else
					{
						if (!board[y + 1, x - 1].Contains(pieceName[0]))
						{
							if (board[y + 1, x - 1].Contains('K'))
							{
								board[y + 1, x - 1] += "C";
							}
							list.Add(new int[] { y + 1, x - 1 });
						}
					}
				}
				if (x - 1 >= 0)
				{
					if (board[y, x - 1] == "_")
					{
						list.Add(new int[] { y, x - 1 });
					}
					else
					{
						if (!board[y, x - 1].Contains(pieceName[0]))
						{
							if (board[y, x - 1].Contains('K'))
							{
								board[y, x - 1] += "C";
							}
							list.Add(new int[] { y, x - 1 });
						}
					}
				}
				if (y - 1 >= 0 && x - 1 >= 0)
				{
					if (board[y - 1, x - 1] == "_")
					{
						list.Add(new int[] { y - 1, x - 1 });
					}
					else
					{
						if (!board[y - 1, x - 1].Contains(pieceName[0]))
						{
							if (board[y - 1, x - 1].Contains('K'))
							{
								board[y - 1, x - 1] += "C";
							}
							list.Add(new int[] { y - 1, x - 1 });
						}
					}
				}
				if (!pieceName.Contains('M') && !pieceName.Contains('C') && pieceName.Contains('w') && !rev || !pieceName.Contains('M') && !pieceName.Contains('C') && pieceName.Contains('b') && rev)
				{
					for (int i = 0; i < x; i++)
					{
						if (board[7, i].Length > 1)
						{
							if (board[7, i].Contains('R'))
							{
								if (!board[7, i].Contains('M'))
								{
									bool canCastle = true;
									for (int j = i + 1; j < x; j++)
									{
										if (board[7, j] != "_")
										{
											canCastle = false;
										}
									}
									if (canCastle)
									{
										if (CheckAvailable(new int[] { y, x - 1 }, piece))
										{
											list.Add(new int[] { y, x - 2 });
										}
									}
									break;
								}
							}
						}
					}
					for (int i = 7; i > x; i--)
					{
						if (board[7, i].Length > 1)
						{
							if (board[7, i].Contains('R'))
							{
								if (!board[7, i].Contains('M'))
								{
									bool canCastle = true;
									for (int j = x + 1; j < i; j++)
									{
										if (board[7, j] != "_")
										{
											canCastle = false;
										}
									}
									if (canCastle)
									{
										if (CheckAvailable(new int[] { y, x + 1 }, piece))
										{
											list.Add(new int[] { y, x + 2 });
										}
									}
									break;
								}
							}
						}
					}
				}
				else if (!pieceName.Contains('M') && !pieceName.Contains('C') && pieceName.Contains('w') && rev || !pieceName.Contains('M') && !pieceName.Contains('C') && pieceName.Contains('b') && !rev)
				{
					for (int i = 0; i < x; i++)
					{
						if (board[0, i].Length > 1)
						{
							if (board[0, i].Contains('R'))
							{
								if (!board[0, i].Contains('M'))
								{
									bool canCastle = true;
									for (int j = i + 1; j < x; j++)
									{
										if (board[0, j] != "_")
										{
											canCastle = false;
										}
									}
									if (canCastle)
									{
										if (CheckAvailable(new int[] { y, x - 1 }, piece))
										{
											list.Add(new int[] { y, x - 2 });
										}
									}
									break;
								}
							}
						}
					}
					for (int i = 7; i > x; i--)
					{
						if (board[0, i].Length > 1)
						{
							if (board[0, i].Contains('R'))
							{
								if (!board[0, i].Contains('M'))
								{
									bool canCastle = true;
									for (int j = x + 1; j < i; j++)
									{
										if (board[0, j] != "_")
										{
											canCastle = false;
										}
									}
									if (canCastle)
									{
										if (CheckAvailable(new int[] { y, x + 1 }, piece))
										{
											list.Add(new int[] { y, x + 2 });
										}
									}
									break;
								}
							}
						}
					}
				}
			}
			catch (Exception) { }
			return list;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			Setup();
		}

		static string[,] CreateBoard()
		{
			string[,] board =
			{
				{ "bR", "bk", "bB", "bQ", "bK", "bB", "bk", "bR"},
				{ "bp", "bp", "bp", "bp", "bp", "bp", "bp", "bp"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "wp", "wp", "wp", "wp", "wp", "wp", "wp", "wp"},
				{ "wR", "wk", "wB", "wQ", "wK", "wB", "wk", "wR"},
			};
			return board;
		}

		private void ConfirmYes_Click(object sender, EventArgs e)
		{
			RemoveLastMove(); ResetColours(true);
			ConfirmText.Visible = false; ConfirmYes.Visible = false; ConfirmNo.Visible = false; ConfirmCustom.Visible = false; enabled = true; puttingPieces = false;
			BlackKingButton.Visible = false; BlackQueenButton.Visible = false; BlackRookButton.Visible = false; EraseButton.Visible = false; tutorial = false; FlipBoardButton2.Visible = false;
			BlackBishopButton.Visible = false; BlackKnightButton.Visible = false; BlackPawnButton.Visible = false; DefaultButton.Visible = false; listBox1.Visible = false; AIDepth2.Checked = true;
			WhiteKingButton.Visible = false; WhiteQueenButton.Visible = false; WhiteRookButton.Visible = false; DemoButton.Visible = false; BackButton.Visible = false; BoardSetupText.Visible = false;
			WhiteBishopButton.Visible = false; WhiteKnightButton.Visible = false; WhitePawnButton.Visible = false; NextButton.Visible = false; Promotion.Visible = false; Chess960Button.Visible = false;
			WhiteMoveCheckBox.Visible = false; BlackMoveCheckBox.Visible = false; ResetButton.Visible = false; TutorialText.Visible = false; CheckMateText.Visible = false; RandomiseBoardButton.Visible = false;
			TutorialBotButton.Visible = false; BulletTimer.Visible = false; BlitzTimer.Visible = false; RapidTimer.Visible = false; ClassicTimer.Visible = false; TimerDuration.Visible = false;
			DropDownMenu.SelectedIndex = -1; ExitDemoButton.Visible = false; CasualTimer.Visible = false; ChessRulesLink.Visible = false; col = 'w'; XButton.Visible = false; BotCheckBox.Visible = false;
			ActiveTimer.Visible = true; InactiveTimer.Visible = true; WhiteMoveCheckBox.Checked = true; FlipBoardButton.Visible = false; tutBot = false; enableRev = true; BotCheckBox.Checked = false;
			timer.Stop(); timerB.Stop(); AutomaticFlipsCheckBox.Visible = false; ActiveTimer.Text = "10:00"; InactiveTimer.Text = "10:00"; AutomaticFlipsCheckBox.Checked = boardFlips; MovesCounter.Text = "Moves: 0";
			TimeBetweenMoves.Visible = false; Check000.Visible = false; Check025.Visible = false; Check050.Visible = false; Check100.Visible = false; BotVBotButton.Visible = false; MovesCounter.Visible = false;
			White00.Visible = false; White000.Visible = false; Black00.Visible = false; Black000.Visible = false; CanCastleText.Visible = false; BackMoveButton.Visible = false; NextMoveButton.Visible = false;
            White00.Checked = false; White000.Checked = false; Black00.Checked = false; Black000.Checked = false; moves.Clear(); movesNum = 0; BoardSetupText.Text = "8/8/8/8/8/8/8/8 w 0"; IgnoreInvalidPostition.Checked = false;
			LoginButton.Visible = false; LoginConfirmPass.Visible = false; LoginPassword.Visible = false; LoginUsername.Visible = false; CreateAccount.Visible = false; CreateAccount2.Visible = false; CreateAccountText.Visible = false;
			BackToLogin.Visible = false; SeePass.Visible = false; SeePassCon.Visible = false; SeePass.Image = Properties.Resources.Eye_Close; SeePassCon.Image = Properties.Resources.Eye_Close; AIDepthPanel.Visible = false;
            LoggedInText.Visible = false; LoggedInText.Text = "Logged in as: "; NotYouText.Visible = false; SignOutButton.Visible = false; ErrorMessage.Top = 532; BoardColourPanel.Visible = false;
            BoardColourText.Visible = false; BlueBoard.Visible = false; PurpleBoard.Visible = false; PinkBoard.Visible = false; GreenBoard.Visible = false; DarkGreenBoard.Visible = false; AIMediumLevel = false;
			PieceTypeText.Visible = false; PieceType0.Visible = false; PieceType1.Visible = false; PieceType2.Visible = false;  PieceTypePanel.Visible = false; pastGameMove = 0; HordeCustom.Visible = false;
			AutomaticFlipsText.Visible = false; FlipsPanel.Visible = false; FlipsTrue.Visible = false; FlipsFalse.Visible = false; PieceType3.Visible = false; pastGame = false; ignoreInvalid = false;
			EnableSoundFalse.Visible = false; EnableSoundTrue.Visible = false; EnableSoundText.Visible = false; EnableSoundPanel.Visible = false; LoggedInText2.Visible = false; IgnoreInvalidPostition.Visible = false;
			PieceAdvText.Text = "Piece Value: 0"; FlipBoardButton2.Enabled = true; ButtonPanel.Visible = false; Boardpanel.Visible = true; PieceAdvText.Visible = true; HordeButton.Visible = false;
			PromotionRangeOff.Visible = PromotionRangeOn.Visible = PromotionRangePanel.Visible = PromotionRangeText.Visible = ResignButton.Visible = false; ResignButton.Text = "Abort";
			board = CreateEmptyBoard(); SetupPieces();
            if (replayMoves.Count > 1 && loggedIn)
			{
				SQLiteConnection conn = CreateConnection();
				int gameID = InsertNewGame(conn, ID, 4);
				foreach (string game in replayMoves)
				{
                    InsertNewGameMove(conn, gameID, game);
				}
			}
			replayMoves.Clear();
            if (rev)
            {
				ReverseBoard();
            }
			if (selected == 0)
			{
				PassAndPlay();
			}
			else if (selected == 1)
			{
				GameModes();
			}
			else if (selected == 2)
			{
				Tutorial();
			}
			else if (selected == 3)
			{
				Custom();
			}
			else if (selected == 4)
			{
				Settings();
			}
			else if (selected == 5)
            {
				BotVSBot();
            }
			else if (selected == 6)
			{
				PastGames();
			}
			else if (selected == 7)
			{
				MediumLevelAI();
			}
		}

		private void MediumLevelAI()
		{
			AIDepthPanel.Visible = true;
			AIMediumLevel = true;
            board = CreateBoard();
            CreateSetupText();
            replayMoves.Add(setupText);
            moves.Add(CloneBoard());
            SetupPieces();
            ResetColours(false);
            RemoveLastMove();
        }

        private void PastGames()
        {
            PieceAdvText.Visible = false;
            InactiveTimer.Visible = false;
            ActiveTimer.Visible = false;
            LoggedInText2.Visible = true;
            Boardpanel.Visible = false;
            if (loggedIn)
            {
                LoggedInText2.Text = "Logged in as " + name;
                ButtonPanel.Visible = true;
                games = new Button[10];
                ButtonPanel.Controls.Clear();
                Refresh();
                int top = 2;
                SQLiteConnection conn = CreateConnection();
                int x = 0;
                SQLiteDataReader sqlite_datareader;
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = "SELECT * FROM games WHERE ID = " + ID + " ORDER BY TimeOfGame DESC";
                sqlite_datareader = sqlite_cmd.ExecuteReader();
                while (sqlite_datareader.Read() && x < 10)
                {
                    x++;
                    string endGameStatus = "";
                    if (sqlite_datareader.GetInt32(2) == 0)
                    {
                        endGameStatus = "Victory for White";
                    }
                    else if (sqlite_datareader.GetInt32(2) == 1)
                    {
                        endGameStatus = "Victory for Black";
                    }
                    else if (sqlite_datareader.GetInt32(2) == 2)
                    {
                        endGameStatus = "Stalemate";
                    }
                    else if (sqlite_datareader.GetInt32(2) == 3)
                    {
                        endGameStatus = "Draw";
                    }
                    else if (sqlite_datareader.GetInt32(2) == 4)
                    {
                        endGameStatus = "No outcome";
                    }
                    string myreader = "Outcome of game: " + endGameStatus + "  Number of moves: " + (sqlite_datareader.GetInt32(3) - 1).ToString() + "  Time of game: " +
                        sqlite_datareader.GetDateTime(4);
                    games[x - 1] = new Button()
                    {
                        Location = new Point(2, top),
                        Size = new Size((ButtonPanel.Width - 2), (ButtonPanel.Height - 2) / 10),
                        Tag = sqlite_datareader.GetInt32(0),
                        Text = myreader,
                        Visible = true
                    };
                    top += (Boardpanel.Height - 2) / 11;
                    ButtonPanel.Controls.Add(games[x - 1]);
                }
                sqlite_datareader.Close();
                PastMoves(conn);
            }
            else
            {
                LoggedInText2.Text = "Not Logged In";
            }
        }

        private void PastMoves(SQLiteConnection conn)
        {
            for (int i = 0; i < 10; i++)
            {
                if (games[i] != null)
                {
                    games[i].Click += (sender1, e) =>
                    {
                        Button game = sender1 as Button;
                        ButtonPanel.Visible = false;
                        ShowPastMoves(conn, int.Parse(game.Tag.ToString()));
                    };
                }
            }
        }

        private void ShowPastMoves(SQLiteConnection conn, int gameID)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT Board FROM gameInfo WHERE GameID = " + gameID + " ORDER BY MoveNum ASC";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            pastGameMoves.Clear();
            while (sqlite_datareader.Read())
            {
                pastGameMoves.Add(sqlite_datareader.GetString(0));
            }
            Boardpanel.Visible = true;
            BoardSetupText.Text = pastGameMoves[0];
            pastGame = true;
            enabled = false;
            BackMoveButton.Visible = NextMoveButton.Visible = MovesCounter.Visible = true;
            NextMoveButton.Enabled = true; LoggedInText2.Visible = false;
        }

        private void ConfirmCustom_Click(object sender, EventArgs e)
		{
			int DifColourKings = 0; bool w = false; bool b = false; int kingCount = 0;
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (board[i, j].Contains("wK") && w == false)
					{
						w = true;
						DifColourKings++;
					}
					else if (board[i, j].Contains("bK") && b == false)
					{
						b = true;
						DifColourKings++;
					}
				}
			}
			if (DifColourKings > 1)
			{
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 8; j++)
					{
						if (board[i, j].Contains('K'))
						{
							kingCount++;
						}
					}
				}
			}
			bool colour = false; bool available = false, NotInCheck = false, noPawnsInBackRank = false;
			if (kingCount == 2)
			{
				noPawnsInBackRank = true;
				for (int i = 0; i < 8; i++)
                {
					if (board[7, i].Contains('p'))
                    {
						noPawnsInBackRank = false;
                    }
					if (board[0, i].Contains('p'))
					{
						noPawnsInBackRank = false;
					}
				}
			}
			if (noPawnsInBackRank)
			{
				NotInCheck = true;
				if (BlackMoveCheckBox.Checked)
				{
					colour = true;
				}
				if (CheckCanMove(colour) || CheckCheck(!colour))
				{
					NotInCheck = false;
				}

			}
			if (NotInCheck)
            {
				int pieces = 0;
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 8; j++)
					{
						if (board[i, j] != "_")
						{
							pieces++;
						}
					}
				}
				if (pieces > 2)
				{
					available = true;
				}
			}
			if (available || ignoreInvalid)
			{
                if (!BotCheckBox.Checked)
                {
                    AutomaticFlipsCheckBox.Visible = true;
                    FlipBoardButton2.Visible = true;
                    if (rev)
                    {
                        ReverseBoard();
                    }
                }
                else
                {
                    AutomaticFlipsCheckBox.Checked = boardFlips;
                }
                colour = false;
                if (BlackMoveCheckBox.Checked)
                {
					CheckColour(true); CheckColour(false);
                    col = 'b';
					colour = true;
                    if (!rev)
                    {
                        SwitchTimers();
                        ReverseBoard();
                    }
                    SetupPieces();
                }
                else
                {
                    col = 'w';
                    if (rev)
                    {
                        SwitchTimers();
                        ReverseBoard();
                    }
                    SetupPieces();
                }
                Promote(true); SetupPieces();
				if (CheckCanMove(colour))
				{
					enabled = false; timer.Stop(); timerB.Stop();
					CheckMateText.Visible = true;
					CheckMateText.Text = Environment.NewLine + Environment.NewLine;
					CheckMateText.Text += "Cannot Move";
                    XButton.Visible = true;
                }
                ConfirmText.Visible = false; ConfirmYes.Visible = false; ConfirmNo.Visible = false; ConfirmCustom.Visible = false; enabled = true; puttingPieces = false;
				BlackKingButton.Visible = false; BlackQueenButton.Visible = false; BlackRookButton.Visible = false; EraseButton.Visible = false;
				BlackBishopButton.Visible = false; BlackKnightButton.Visible = false; BlackPawnButton.Visible = false; IgnoreInvalidPostition.Visible = false;
				WhiteKingButton.Visible = false; WhiteQueenButton.Visible = false; WhiteRookButton.Visible = false; HordeCustom.Visible = false;
				WhiteBishopButton.Visible = false; WhiteKnightButton.Visible = false; WhitePawnButton.Visible = false;
				WhiteMoveCheckBox.Visible = false; BlackMoveCheckBox.Visible = false; ResetButton.Visible = false; FlipBoardButton.Visible = false;
				BulletTimer.Visible = false; BlitzTimer.Visible = false; RapidTimer.Visible = false; ClassicTimer.Visible = false; TimerDuration.Visible = false;
				DefaultButton.Visible = false; CasualTimer.Visible = false; MovesCounter.Visible = true; BoardSetupText.Visible = false;
                White00.Visible = false; White000.Visible = false; Black00.Visible = false; Black000.Visible = false; CanCastleText.Visible = false;
                timerTick = true; BotCheckBox.Visible = false; BackMoveButton.Visible = true; NextMoveButton.Visible = ResignButton.Visible = true; RandomiseBoardButton.Visible = false;
				CreateSetupText();
                replayMoves.Add(setupText);
                moves.Add(CloneBoard());
                if (BotCheckBox.Checked)
                {
					if (rev)
					{
						ReverseBoard();
						SetupPieces();
					}
					MovesCounter.Visible = false;
					enabled = false;
                    AIDepthPanel.Visible = true;
                }
            }
			else
			{
				InvalidTimer.Start();
				InvalidText.Visible = true;
			}
		}

		private void ConfirmNo_Click(object sender, EventArgs e)
		{
			ConfirmText.Visible = false; ConfirmYes.Visible = false; ConfirmNo.Visible = false;
			DropDownMenu.SelectedIndex = -1;
		}

		private void DropDownMenu_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (DropDownMenu.SelectedIndex != -1)
			{
				ConfirmNo.Visible = true; ConfirmYes.Visible = true; ConfirmText.Visible = true;
				selected = DropDownMenu.SelectedIndex;
			}
		}

		private void Promotion_Click(object sender, EventArgs e)
		{
			if (listBox1.SelectedIndex > -1)
			{
				enabled = true;
				promoting = listBox1.SelectedIndex;
				Promotion.Enabled = true;
				listBox1.SelectedIndex = -1;
				listBox1.Visible = false;
				Promotion.Visible = false;
			}
			for (int j = 0; j < 2; j++)
			{
				int k = 0;
				if (j == 1)
                {
					k = 7;
                }
				for (int i = 0; i < 8; i++)
				{
                    if (board[k, i].Contains("wp") && (!rev && k == 0 || rev && k == 7) || board[k, i].Contains("bp") && (!rev && k == 7 || rev && k == 0))
                    {
						string pieceName = "";
						foreach (char c in board[k, i])
						{
							if (c == 'p')
							{
								if (promoting == 0)
								{
									pieceName += "Q";
								}
								else if (promoting == 1)
								{
									pieceName += "RM";
								}
								else if (promoting == 2)
								{
									pieceName += "k";
								}
								else if (promoting == 3)
								{
									pieceName += "B";
								}
							}
							else if (c != 'M')
							{
								pieceName += c.ToString();
							}

						}
						board[k, i] = pieceName;
						if (enabled)
						{
							if ((col == 'w' && rev || col == 'b') && !rev && AutomaticFlipsCheckBox.Checked)
							{
								enableRev = false;
							}
							else if ((col == 'w' && !rev || col == 'b' && rev) && AutomaticFlipsCheckBox.Checked)
							{
								enableRev = true;
							}
							else
							{
								enableRev = false;
							}
							if (enableRev)
							{
								SetupPieces();
								ResetColours(false);
								Refresh();
								System.Threading.Thread.Sleep(500);
								ReverseBoard();
							}
							SetupPieces();
							bool colour = false;
							if (col == 'w')
							{
								timerB.Start(); timer.Stop();
								col = 'b';
								colour = true;
							}
							else
							{
								timerB.Stop(); timer.Start();
								col = 'w';
							}
							CheckColour(colour);
							if (CheckCanMove(colour))
							{
								int endGameStatus = 0;
								if (CheckCheck(colour))
								{
									enabled = false; timer.Stop(); timerB.Stop();
									CheckMateText.Visible = true;
                                    CheckMateText.Text = Environment.NewLine + Environment.NewLine;
                                    if (colour)
                                    {
                                        CheckMateText.Text += "White wins";
                                    }
                                    else
                                    {
                                        CheckMateText.Text += "Black wins";
										endGameStatus = 1;
                                    }
                                    CheckMateText.Text += Environment.NewLine + "by Checkmate!";
                                    XButton.Visible = true;
								}
								else
								{
									enabled = false; timer.Stop(); timerB.Stop();
									CheckMateText.Visible = true;
									CheckMateText.Text = Environment.NewLine + Environment.NewLine + "Stalemate!";
									XButton.Visible = true;
									endGameStatus = 2;
								}
                                if (loggedIn)
                                {
                                    SQLiteConnection conn = CreateConnection();
                                    int gameID = InsertNewGame(conn, ID, endGameStatus);
                                    foreach (string game in replayMoves)
                                    {
                                        InsertNewGameMove(conn, gameID, game);
                                    }
                                }
								replayMoves.Clear();
                            }
                            if (soundEffects)
                            {
                                toc.Play();
                            }
                            ResetColours(false);
							if (tutBot)
							{
								TutBotMove();
							}
                            if (AIMediumLevel)
                            {
                                Refresh();
                                AIMovement(col);
                            }
                        }
					}
				}
			}
		}

		private void CustomiseBoard()
		{
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					squareBoard[y, x].Click += (sender1, e1) =>
					{
						PictureBox piece = sender1 as PictureBox;
						if (puttingPieces)
						{
							int inY = piece.Top / 60, inX = piece.Left / 60;
							board[inY, inX] = pieceToPut;
							if (pieceToPut.Contains("wp") && inY != 6)
							{
								board[inY, inX] += "M";
							}
							else if (pieceToPut.Contains("bp") && inY != 1)
							{
								board[inY, inX] += "M";
							}
							else if ((pieceToPut.Contains("wR") && inY != 7 || pieceToPut.Contains("wR") && inX != 0) && (pieceToPut.Contains("wR") && inY != 7 || pieceToPut.Contains("wR") && inX != 7))
							{
								board[inY, inX] += "M";
							}
							else if ((pieceToPut.Contains("bR") && inY != 0 || pieceToPut.Contains("bR") && inX != 0) && (pieceToPut.Contains("bR") && inY != 0 || pieceToPut.Contains("bR") && inX != 7))
							{
								board[inY, inX] += "M";
							}
							else if (pieceToPut.Contains("wK") && inY != 7 || pieceToPut.Contains("wK") && inX != 4)
							{
								board[inY, inX] += "M";
							}
							else if (pieceToPut.Contains("bK") && inY != 0 || pieceToPut.Contains("bK") && inX != 4)
							{
								board[inY, inX] += "M";
							}
							SetupPieces();
							CreateSetupText();
							BoardSetupText.Text = setupText;
                        }
                    };
				}
			}
		}

		private void TutorialClick()
		{
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					squareBoard[y, x].Click += (sender1, e1) =>
					{
						PictureBox piece = sender1 as PictureBox;
						int inY = piece.Top / 60, inX = piece.Left / 60;
						if (tutorial && board[inY, inX] != "_")
						{
							ChessRulesLink.Visible = false;
							DemoButton.Visible = true;
							TutorialBotButton.Visible = false;
							if (board[inY, inX].Contains('p'))
							{
								PawnClick(inY, inX);
							}
							else if (board[inY, inX].Contains('k'))
							{
								KnightClick(inY, inX);
							}
							else if (board[inY, inX].Contains('B'))
							{
								BishopClick(inY, inX);
							}
							else if (board[inY, inX].Contains('R'))
							{
								RookClick(inY, inX);
							}
							else if (board[inY, inX].Contains('Q'))
							{
								QueenClick(inY, inX);
							}
							else if (board[inY, inX].Contains('K'))
							{
								KingClick(inY, inX);
							}
						}
						else if (tutorial && board[inY, inX] == "_")
						{
							DemoButton.Visible = false;
							ResetColours(false);
							Tutorial();
						}
					};
				}
			}
		}

		static string[,] CreateEmptyBoard()
		{
			string[,] board =
			{
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
			};
			return board;
		}

		private void Custom()
		{
			ConfirmCustom.Visible = true; DefaultButton.Visible = true;
			BlackKingButton.Visible = true; BlackQueenButton.Visible = true; BlackRookButton.Visible = true; EraseButton.Visible = true;
			BlackBishopButton.Visible = true; BlackKnightButton.Visible = true; BlackPawnButton.Visible = true; IgnoreInvalidPostition.Visible = true;
			WhiteKingButton.Visible = true; WhiteQueenButton.Visible = true; WhiteRookButton.Visible = true; BoardSetupText.Visible = true;
			WhiteBishopButton.Visible = true; WhiteKnightButton.Visible = true; WhitePawnButton.Visible = true; RandomiseBoardButton.Visible = true;
			WhiteMoveCheckBox.Visible = true; BlackMoveCheckBox.Visible = true; ResetButton.Visible = true; puttingPieces = true;
			RapidTimer.Checked = true; TimerDuration.Visible = true; FlipBoardButton.Visible = true; BotCheckBox.Visible = true; HordeCustom.Visible = true;
			BulletTimer.Visible = true; BlitzTimer.Visible = true; RapidTimer.Visible = true; ClassicTimer.Visible = true; CasualTimer.Visible = true;
            White00.Visible = true; White000.Visible = true; Black00.Visible = true; Black000.Visible = true; CanCastleText.Visible = true;
            string[,] emptyBoard = CreateEmptyBoard();
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					board[i, j] = emptyBoard[i, j];
				}
			}
			SetupPieces();
			CustomiseBoard();
		}

		private void PassAndPlay()
		{
			ResignButton.Visible = true;
            BackMoveButton.Visible = true; NextMoveButton.Visible = true;
            FlipBoardButton2.Visible = true;
			AutomaticFlipsCheckBox.Visible = true;
			MovesCounter.Visible = true;
            board = CreateBoard();
            CreateSetupText();
            replayMoves.Add(setupText);
            moves.Add(CloneBoard());
            SetupPieces();
            ResetColours(false);
			RemoveLastMove();
		}

		private void Tutorial()
		{
			DemoButton.Enabled = true;
			board = CreateBoard();
			SetupPieces();
			TutorialBotButton.Visible = true;
			TutorialText.Visible = true;
			TutorialText.Font = new Font("Microsoft Sans Serif", 14);
			TutorialText.Text = "Welcome to the tutorial.";
			TutorialText.Text += Environment.NewLine + "This is the board with the base position. White makes his move first and then it is black's turn to move" +
			"The game lasts until a checkmate (click on the King for more information) or a stalemate (when a player cannot move anymore while not being in check, considered a draw)";
			TutorialText.Text += Environment.NewLine + "For more information click this link";
			TutorialText.Text += Environment.NewLine + "press any piece to learn more about it";
			TutorialText.Text += Environment.NewLine + "Or click on the button below to go against an easy difficulty bot to test your skills";
			tutorial = true;
			ChessRulesLink.Visible = true;
			ActiveTimer.Visible = false;
			InactiveTimer.Visible = false;
			timerTick = false;
			TutorialClick();
		}

		private void GameModes()
		{
			Chess960Button.Visible = true;
			HordeButton.Visible = true;
		}

		private void Settings()
		{
			BoardColourText.Visible = true; BlueBoard.Visible = true; PurpleBoard.Visible = true; PinkBoard.Visible = true; GreenBoard.Visible = true; DarkGreenBoard.Visible = true;
			BoardColourPanel.Visible = true; PieceTypeText.Visible = true; PieceType0.Visible = true; PieceType1.Visible = true; PieceType2.Visible = true; PieceTypePanel.Visible = true;
            AutomaticFlipsText.Visible = true; FlipsPanel.Visible = true; FlipsTrue.Visible = true; FlipsFalse.Visible = true; PieceType3.Visible = true;
			EnableSoundFalse.Visible = true; EnableSoundTrue.Visible = true; EnableSoundText.Visible = true; EnableSoundPanel.Visible = true;
			PromotionRangeOff.Visible = PromotionRangeOn.Visible = PromotionRangePanel.Visible = PromotionRangeText.Visible = true;
            LoginUsername.Text = "Login:";
            LoginPassword.Text = "Password:";
			LoginPassword.PasswordChar = '\0';
			LoginConfirmPass.Text = "Confirm password:";
			LoginConfirmPass.PasswordChar = '\0';
			if (!loggedIn)
			{
				LoginButton.Visible = true; LoginPassword.Visible = true; LoginUsername.Visible = true; CreateAccount.Visible = true; CreateAccountText.Visible = true;
				SeePass.Visible = true;
			}
			else
			{
                LoggedInText.Visible = true;
                NotYouText.Visible = true;
                SignOutButton.Visible = true;
				LoggedInText.Text += name;
            }
            board = CreateBoard();
            SetupPieces();
			enabled = false;
        }

		private void BotVSBot()
        {
			Check000.Checked = true;
			BotVBotButton.Visible = true;
			TimeBetweenMoves.Visible = true;
            White00.Checked = true; White000.Checked = true; Black00.Checked = true; Black000.Checked = true;
            Check000.Visible = true;
			Check025.Visible = true;
			Check050.Visible = true;
			Check100.Visible = true;
		}

		private void DemoKingBasePos2()
		{
			board = CreateEmptyBoard();
			board[0, 0] = "bR";
			board[0, 1] = "bB";
			board[0, 5] = "bR";
			board[0, 7] = "bK";
			board[1, 0] = "bp";
			board[1, 1] = "bp";
			board[1, 2] = "bk";
			board[1, 4] = "bp";
			board[2, 3] = "bp";
			board[2, 4] = "bp";
			board[4, 0] = "wQ";
			board[5, 3] = "wp";
			board[5, 6] = "wR";
			board[6, 1] = "wp";
			board[6, 2] = "wp";
			board[6, 4] = "wp";
			board[7, 2] = "wK";
			SetupPieces();
		}

		private void DemoKingBasePos1(int num)
        {
			if (num == 0)
			{
				board = new string[,]
				{
				{ "bR", "_", "_", "bQ", "bK", "_", "_", "bR"},
				{ "bp", "bp", "_", "bp", "bB", "bp", "_", "_"},
				{ "bk", "_", "_", "_", "bB", "_", "bp", "bp"},
				{ "_", "_", "_", "wk", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "wp", "_"},
				{ "_", "_", "wk", "_", "_", "wQ", "_", "_"},
				{ "wp", "wp", "wp", "_", "_", "wp", "wB", "wp"},
				{ "wR", "_", "_", "_", "wK", "_", "_", "wR"},
				};
			}
            else
            {
				board = new string[,]
				{
				{ "bR", "_", "_", "bQ", "bK", "_", "_", "bR"},
				{ "bp", "bp", "_", "bp", "_", "bp", "_", "_"},
				{ "bk", "_", "_", "_", "bB", "_", "bp", "bp"},
				{ "_", "_", "_", "wk", "_", "_", "_", "_"},
				{ "_", "_", "_", "_", "_", "_", "wp", "bB"},
				{ "_", "_", "wk", "_", "_", "wQ", "_", "_"},
				{ "wp", "wp", "wp", "_", "_", "wp", "wB", "wp"},
				{ "_", "_", "wK", "wR", "_", "_", "_", "wR"},
				};
			}
			SetupPieces();
        }

		private void DemoKing()
		{
			if (demo == "K")
			{
				if (demoNum == 0)
				{
					RemoveLastMove(); ResetColours(false);
					BackButton.Enabled = false;
					board = CreateEmptyBoard();
					board[6, 4] = "wK";
					board[1, 1] = "bK";
					board[3, 4] = "bp";
					SetupPieces();
					TutorialText.Text = "In this tutorial we will show the movement of the King.";
					TutorialText.Text += Environment.NewLine + "This first part of the tutorial is simply to show the basic movement of the King";
					TutorialText.Text += Environment.NewLine + "This second part of the tutorial will show you how to castle";
					TutorialText.Text += Environment.NewLine + "This third part of the tutorial will show a more advanced postion showing a winning endgame position";
					TutorialText.Text += Environment.NewLine + "Knowing the movement of other pieces is recommended for this tutorial";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 1)
				{
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[6, 4] = "wK";
					board[1, 1] = "bK";
					board[4, 4] = "bp";
					SetupPieces();
					squareBoard[3, 4].BackColor = Color.Orange;
					squareBoard[4, 4].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "The opponent pushes his pawn";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 2)
				{
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[6, 4] = "wK";
					board[1, 1] = "bK";
					board[4, 4] = "bp";
					SetupPieces();
					foreach(int[] i in KingAvailable(squareBoard[6, 4]))
					{
						if (CheckAvailable(i, squareBoard[6, 4]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[6, 4].BackColor = Color.FromArgb(59, 217, 72);
					squareBoard[3, 4].BackColor = Color.Orange;
					squareBoard[4, 4].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Your King can move anywhere one square around him but unlike other pieces he cannot move to where he could be put in danger for example " +
					"he cannot move to the diagonals of a pawn as shown here";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 3)
				{
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[5, 4] = "wK";
					board[1, 1] = "bK";
					board[4, 4] = "bp";
					SetupPieces();
					squareBoard[5, 4].BackColor = Color.Orange;
					squareBoard[6, 4].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "You want to move your King up so that it threatens the pawn and does not let it move up";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 4)
				{
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[5, 4] = "wK";
					board[2, 2] = "bK";
					board[4, 4] = "bp";
					SetupPieces();
					squareBoard[2, 2].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[1, 1].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "As he they cannot move their pawn the opponent moves his King to try to defend the pawn";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 5)
				{
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[5, 4] = "wK";
					board[2, 2] = "bK";
					board[4, 4] = "bp";
					SetupPieces();
					foreach (int[] i in KingAvailable(squareBoard[5, 4]))
					{
						if (CheckAvailable(i, squareBoard[5, 4]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[5, 4].BackColor = Color.FromArgb(59, 217, 72);
					squareBoard[2, 2].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[1, 1].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "The King can move anywhere, take the enemy pawn but cannot move where it can be taken by an oppposing piece";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 6)
				{
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[4, 4] = "wK";
					board[2, 2] = "bK";
					SetupPieces();
					squareBoard[5, 4].BackColor = Color.Orange;
					squareBoard[4, 4].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "You take the enemy pawn ending the game in a total draw as neither of you can take the other King as both of you have no pieces left";
					TutorialText.Text += Environment.NewLine + "This is the end of this part of tutorial";
					TutorialText.Text += Environment.NewLine + "Press Next to move to the next part";
				}
				//Second Part
				else if (demoNum == 7)
				{
					RemoveLastMove(); ResetColours(false);
					DemoKingBasePos1(0);
					SetupPieces();
					TutorialText.Text = "Moving on to the second part of the tutorial showing how to castle and why is castling is often a good move";
					TutorialText.Text += Environment.NewLine + "It is black to play";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 8)
				{
					RemoveLastMove(); ResetColours(false);
					board[1, 4] = "_";
					board[4, 7] = "bB";
					SetupPieces();
					squareBoard[1, 4].BackColor = Color.Orange;
					squareBoard[4, 7].BackColor = Color.Orange;
					TutorialText.Text = "One of your opponents best move in this position is to move their bishop to not allow for a trade while not weakening their position";
					TutorialText.Text += Environment.NewLine + "It is now your turn";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 9)
				{
					RemoveLastMove(); ResetColours(false);
					board[7, 2] = "_";
					board[7, 4] = "wK";
					board[7, 0] = "wR";
					board[7, 3] = "_";
					SetupPieces();
					foreach (int[] i in KingAvailable(squareBoard[7, 4]))
					{
						if (CheckAvailable(i, squareBoard[7, 4]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[7, 4].BackColor = Color.FromArgb(59, 217, 72);
					squareBoard[1, 4].BackColor = Color.Orange;
					squareBoard[4, 7].BackColor = Color.Orange;
					TutorialText.Text = "If your King has not moved and your Rooks have not yet moved you can do something called castling which allows your " +
                    "King to move 2 squares either side and the Rook to go over it";
					TutorialText.Text += Environment.NewLine + "The King can do this on either side as long as thier is no pieces in between or that the King is not in check" +
                    " and the squares the King and Rook moves to are not threatened";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 10)
				{
					RemoveLastMove(); ResetColours(false);
					board[7, 2] = "wK";
					board[7, 4] = "_";
					board[7, 0] = "_";
					board[7, 3] = "wR";
					DemoKingBasePos1(1);
					SetupPieces();
					squareBoard[7, 4].BackColor = Color.FromArgb(59, 217, 72);
					squareBoard[7, 2].BackColor = Color.Orange;
					squareBoard[7, 4].BackColor = Color.Orange;
					squareBoard[7, 0].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[7, 3].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Your best move is to do something called Queenside caslting which is to caslte on the side Queen started";
					TutorialText.Text += Environment.NewLine + "Doing this allows you to oppen up your Rook to be able to add another easy line of defence on your Knight while also protecting your King";
					TutorialText.Text += Environment.NewLine + "This is the end of this part of the tutorial";
					TutorialText.Text += Environment.NewLine + "Press Next to move to the last part of the tutorial";
				}
				//Third Part
				else if (demoNum == 11)
				{
					RemoveLastMove(); ResetColours(true);
					DemoKingBasePos2();
					TutorialText.Text = "Moving on to the third part of the tutorial we see from the beginning that black is winning in terms of piece value " +
					"as he has a pawn, Rook, Knight and Bishop for a Queen which has a higher value overall";
					TutorialText.Text += Environment.NewLine + "However he is not winning as this endgame is winning for white if they play the correct move even if black plays perfectly";
					TutorialText.Text += Environment.NewLine + "This is called a forced checkmate";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 12)
				{
					RemoveLastMove(); ResetColours(true);
					board[0, 5] = "_";
					board[7, 5] = "bR";
					SetupPieces();
					squareBoard[7, 2].BackColor = Color.Red;
					squareBoard[0, 5].BackColor = Color.Orange;
					squareBoard[7, 5].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "One of your opponents best moves is to move his Rook down to check your King as to lose slower and hope you make a mistake so they can take back the upper hand";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 13)
				{
					RemoveLastMove(); ResetColours(true);
					board[7, 2] = "wK";
					board[6, 3] = "_";
					SetupPieces();
					foreach (int[] i in KingAvailable(squareBoard[7, 2]))
					{
						if (CheckAvailable(i, squareBoard[7, 2]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[7, 2].BackColor = Color.DarkRed;
					squareBoard[0, 5].BackColor = Color.Orange;
					squareBoard[7, 5].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "You are in check meaning you have to put your King in a postition of safety this means either blocking the check or moving your King";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 14)
				{
					RemoveLastMove(); ResetColours(true);
					board[7, 2] = "_";
					board[6, 3] = "wK";
					board[7, 7] = "_";
					board[7, 5] = "bR";
					SetupPieces();
					squareBoard[7, 2].BackColor = Color.Orange;
					squareBoard[6, 3].BackColor = Color.Orange;
					TutorialText.Text = "Your only have one legal move however meaning you have to move your King in between your pawns";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 15)
				{
					RemoveLastMove(); ResetColours(false);
					board[7, 7] = "bR";
					board[7, 5] = "_";
					board[4, 6] = "_";
					board[4, 0] = "wQ";
					SetupPieces();
					squareBoard[7, 7].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[7, 5].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Your opponent now has to move his Rook far right as to not let you move your Queen to the right and win next move";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 16)
				{
					RemoveLastMove(); ResetColours(true);
					board[4, 6] = "wQ";
					board[4, 0] = "_";
					board[7, 3] = "_";
					board[7, 7] = "bR";
					SetupPieces();
					squareBoard[4, 6].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[4, 0].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Your best move is to now move your Queen in front of your Rook to threaten an unstopable mate.";
					TutorialText.Text += Environment.NewLine + "Another move would have been faster which is to postion on your opponents back rank (row where king started)" +
					" and would have been a checkmate but is protected by the opponent's Knight";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 17)
				{
					RemoveLastMove(); ResetColours(true);
					board[7, 3] = "bR";
					board[7, 7] = "_";
					SetupPieces();
					squareBoard[6, 3].BackColor = Color.Red;
					squareBoard[7, 3].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[7, 7].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Now the only way for the opponent to stop our mate in one is to put our King in check.";
					TutorialText.Text += Environment.NewLine + "The only available check for Black is to move the Rook down our back rank";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 18)
				{
					RemoveLastMove(); ResetColours(true);
					board[6, 3] = "wK";
					board[7, 3] = "bR";
					SetupPieces();
					foreach (int[] i in KingAvailable(squareBoard[6, 3]))
					{
						if (CheckAvailable(i, squareBoard[6, 3]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[6, 3].BackColor = Color.DarkRed;
					squareBoard[7, 3].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[7, 7].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "You now have 3 options either go diagonally up either side or take the enemy Rook";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 19)
				{
					RemoveLastMove(); ResetColours(true);
					board[6, 3] = "_";
					board[7, 3] = "wK";
					board[1, 2] = "bk";
					board[0, 4] = "_";
					SetupPieces();
					squareBoard[6, 3].BackColor = Color.Orange;
					squareBoard[7, 3].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "If you move top left you have a mate in 3 moves, if you move top right you have a slower mate in 4 moves however if you move your King down to take the enemy Rook " +
                    "you will get a mate in 1 move";
					TutorialText.Text += Environment.NewLine + "Therfore taking the enemy Rook is the best move";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 20)
				{
					RemoveLastMove(); ResetColours(true);
					board[1, 2] = "_";
					board[0, 4] = "bk";
					board[0, 6] = "_";
					board[4, 6] = "wQ";
					SetupPieces();
					squareBoard[1, 2].BackColor = Color.Orange;
					squareBoard[0, 4].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "The opponent has no moves to stop winning meaning on next turn you have a guaranteed mate in 1.";
					TutorialText.Text += Environment.NewLine + "This means they can do any move they want it won't matter";
					TutorialText.Text += Environment.NewLine + "In this case they just decided to move their Knight trying to stop one of the possible checkmates";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else
				{
					NextButton.Enabled = false;
					RemoveLastMove(); ResetColours(true);
					board[0, 6] = "wQ";
					board[4, 6] = "_";
					SetupPieces();
					squareBoard[0, 7].BackColor = Color.Red;
					squareBoard[0, 6].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[4, 6].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "There is now currently 5 different ways to end the game this is just showing one of them but it is definitely not the only one.";
					TutorialText.Text += Environment.NewLine + "Press Exit to leave";
				}
			}
		}

		private void KingClick(int y, int x)
		{
			ResetColours(false);
			squareBoard[y, x].BackColor = Color.FromArgb(59, 217, 72);
			TutorialText.Font = new Font("Microsoft Sans Serif", 12);
			TutorialText.Text = "This is a King";
			TutorialText.Text += Environment.NewLine + "The King can move 1 square in any direction";
			TutorialText.Text += Environment.NewLine + "If it moves where an opposite colour piece is it takes the piece";
			TutorialText.Text += Environment.NewLine + "It is the most important piece and has to be protected";
			TutorialText.Text += Environment.NewLine + "Your king cannot be put in a postion where it can be taken and if the opponent threatens your king it must be moved to safety";
			TutorialText.Text += Environment.NewLine + "If the King cannot move to safety within a turn then you are considered in checkmate and lose the game";
			TutorialText.Text += Environment.NewLine + "Press the demo button to see how the King moves and works";
			demo = "K";
		}

		private void DemoQueenBasePos()
		{
			board = CreateEmptyBoard();
			board[1, 0] = "bp";
			board[1, 1] = "bp";
			board[0, 3] = "bR";
			board[0, 7] = "bR";
			board[1, 5] = "bp";
			board[1, 7] = "bp";
			board[1, 6] = "bK";
			board[2, 2] = "bp";
			board[3, 2] = "bk";
			board[6, 3] = "wQ";
			board[6, 6] = "wp";
			board[6, 7] = "wp";
			board[7, 6] = "wK";
			SetupPieces();
		}

		private void DemoQueen()
		{
			if (demo == "Q")
			{
				if (demoNum == 0)
				{
					BackButton.Enabled = false;
					RemoveLastMove(); ResetColours(false);
					DemoQueenBasePos();
					squareBoard[0, 3].BackColor = Color.Orange;
					squareBoard[0, 2].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "In this tutorial we will show the movement of the Queen";
					TutorialText.Text += Environment.NewLine + "The opponent has decided to move his Rook to threaten our Queen";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 1)
				{
					RemoveLastMove(); ResetColours(true);
					DemoQueenBasePos();
					foreach (int[] i in QueenAvailable(squareBoard[6, 3]))
					{
						if (CheckAvailable(i, squareBoard[6, 3]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[6, 3].BackColor = Color.FromArgb(59, 217, 72);
					squareBoard[0, 3].BackColor = Color.Orange;
					squareBoard[0, 2].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "We are very behind in terms of piece value and we have to find moves that will remove us from this losing situation.";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 2)
				{
					RemoveLastMove(); ResetColours(false);
					board[6, 3] = "_";
					board[3, 6] = "wQ";
					board[1, 6] = "bK";
					board[0, 5] = "_";
					SetupPieces();
					squareBoard[1, 6].BackColor = Color.Red;
					squareBoard[6, 3].BackColor = Color.Orange;
					squareBoard[3, 6].BackColor = Color.Orange;
					TutorialText.Text = "We we cannot directly take the Rook yet as it is protected so we have to make a better move.";
					TutorialText.Text = "We move our Queen diagonnally to fork 3 pieces: the King, the Rook and the Knight.";
					TutorialText.Text += Environment.NewLine + "This forces the opponent into a lose lose situation";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 3)
				{
					RemoveLastMove(); ResetColours(true);
					board[1, 6] = "_";
					board[0, 5] = "bK";
					SetupPieces();
					squareBoard[1, 6].BackColor = Color.Orange;
					squareBoard[0, 5].BackColor = Color.Orange;
					TutorialText.Text = "This forces the king back as it is in check";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 4)
				{
					RemoveLastMove(); ResetColours(true);
					board[0, 3] = "bR";
					board[3, 6] = "wQ";
					SetupPieces();
					foreach (int[] i in QueenAvailable(squareBoard[3, 6]))
					{
						if (CheckAvailable(i, squareBoard[3, 6]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[3, 6].BackColor = Color.FromArgb(59, 217, 72);
					squareBoard[1, 6].BackColor = Color.Orange;
					squareBoard[0, 5].BackColor = Color.Orange;
					TutorialText.Text = "Now that the opponent King has moved back it leaves the enemy Rook and Knight defenceless";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 5)
				{
					RemoveLastMove(); ResetColours(true);
					board[0, 3] = "wQ";
					board[3, 6] = "_";
					board[1, 6] = "_";
					board[0, 5] = "bK";
					SetupPieces();
					squareBoard[0, 5].BackColor = Color.Red;
					squareBoard[3, 6].BackColor = Color.Orange;
					squareBoard[0, 3].BackColor = Color.Orange;
					TutorialText.Text = "We take the enemy Rook as it has a higher value";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 6)
				{
					RemoveLastMove(); ResetColours(true);
					board[1, 6] = "bK";
					board[0, 5] = "_";
					board[3, 6] = "_";
					board[0, 3] = "wQ";
					SetupPieces();
					squareBoard[1, 6].BackColor = Color.Orange;
					squareBoard[0, 5].BackColor = Color.Orange;
					TutorialText.Text = "The King is in check and has to move away from danger";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 7)
				{
					RemoveLastMove(); ResetColours(true);
					board[3, 6] = "wQ";
					board[0, 3] = "_";
					board[0, 5] = "_";
					board[1, 6] = "bK";
					SetupPieces();
					squareBoard[1, 6].BackColor = Color.Red;
					squareBoard[3, 6].BackColor = Color.Orange;
					squareBoard[0, 3  ].BackColor = Color.Orange;
					TutorialText.Text = "We move our Queen to fork the King once more";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 8)
				{
					RemoveLastMove(); ResetColours(true);
					board[0, 5] = "bK";
					board[1, 6] = "_";
					board[3, 6] = "wQ";
					board[3, 2] = "bk";
					SetupPieces();
					squareBoard[1, 6].BackColor = Color.Orange;
					squareBoard[0, 5].BackColor = Color.Orange;
					TutorialText.Text = "The opponent has only one legal move which is moving his King back";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 9)
				{
					RemoveLastMove(); ResetColours(true);
					board[3, 2] = "wQ";
					board[3, 6] = "_";
					board[0, 4] = "_";
					board[0, 5] = "bK";
					SetupPieces();
					squareBoard[0, 5].BackColor = Color.Red;
					squareBoard[3, 2].BackColor = Color.Orange;
					squareBoard[3, 6].BackColor = Color.Orange;
					TutorialText.Text = "We can now take the hanging Knight putting the King in check once more";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 10)
				{
					RemoveLastMove(); ResetColours(true);
					board[0, 4] = "bK";
					board[0, 5] = "_";
					board[3, 4] = "_";
					board[3, 2] = "wQ";
					SetupPieces();
					squareBoard[0, 4].BackColor = Color.Orange;
					squareBoard[0, 5].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "The opponent decides to move his King to the left";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 11)
				{
					RemoveLastMove(); ResetColours(true);
					board[3, 4] = "wQ";
					board[3, 2] = "_";
					board[0, 4] = "bK";
					board[1, 3] = "_";
					SetupPieces();
					squareBoard[0, 4].BackColor = Color.Red;
					squareBoard[3, 4].BackColor = Color.Orange;
					squareBoard[3, 2].BackColor = Color.Orange;
					TutorialText.Text = "The move from the opponent was a mistake as it allows us to put him in check and forking him one last time";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 12)
				{
					RemoveLastMove(); ResetColours(true);
					board[0, 4] = "_";
					board[1, 3] = "bK";
					board[0, 7] = "bR";
					board[3, 4] = "wQ";
					SetupPieces();
					squareBoard[0, 4].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[1, 3].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Again the opponent has no other choice than to move his King out of the way";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else
				{
					NextButton.Enabled = false;
					RemoveLastMove(); ResetColours(false);
					board[0, 7] = "wQ";
					board[3, 4] = "_";
					SetupPieces();
					squareBoard[0, 7].BackColor = Color.Orange;
					squareBoard[3, 4].BackColor = Color.Orange;
					TutorialText.Text = "We now can take his Rook for free leaving us at a winning postions from a very bad base postion.";
					TutorialText.Text += Environment.NewLine + "Now to win we only need to either slowly take his pawns or find a forced checkmate";
					TutorialText.Text += Environment.NewLine + "This is the end of the tutorial on the Queen";
					TutorialText.Text += Environment.NewLine + "Press Exit to leave";
				}
			}
		}

		private void QueenClick(int y, int x)
		{
			ResetColours(false);
			squareBoard[y, x].BackColor = Color.FromArgb(59, 217, 72);
			TutorialText.Font = new Font("Microsoft Sans Serif", 12);
			TutorialText.Text = "This is a Queen";
			TutorialText.Text += Environment.NewLine + "The Queen has the combined moves of the Rook and the Bishop meaning she can move either horizontally, vertically or diagonally any number of squares";
			TutorialText.Text += Environment.NewLine + "If it moves where an opposite colour piece is it takes the piece";
			TutorialText.Text += Environment.NewLine + "This is your most powerful piece and should not be sacrificed lightly";
			TutorialText.Text += Environment.NewLine + "Press the demo button to see how the Queen moves and works";
			demo = "Q";
		}

		private void DemoRookBasePos()
		{
			board = CreateEmptyBoard();
			board[0, 1] = "bR";
			board[0, 4] = "bK";
			board[0, 7] = "bQ";
			board[1, 0] = "bp";
			board[1, 4] = "bp";
			board[1, 5] = "bp";
			board[2, 3] = "bp";
			board[2, 6] = "bp";
			board[4, 4] = "wp";
			board[5, 1] = "wp";
			board[5, 3] = "wk";
			board[5, 5] = "wp";
			board[5, 6] = "wp";
			board[6, 2] = "wR";
			board[6, 4] = "wK";
			SetupPieces();
		}

		private void DemoRook()
		{
			if (demo == "R")
			{
				if (demoNum == 0)
				{
					BackButton.Enabled = false;
					RemoveLastMove(); ResetColours(false);
					DemoRookBasePos();
					TutorialText.Text = "In this tutorial we will show the movement of the Rook.";
					TutorialText.Text += Environment.NewLine + "Its the opponent's turn to play.";
					TutorialText.Text += Environment.NewLine + "They currently have a winning position with them having a pawn up and a Queen " +
					"for a R which is not very good";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 1)
				{
					RemoveLastMove(); ResetColours(false);
					board[5, 1] = "bR";
					board[0, 1] = "_";
					SetupPieces();
					squareBoard[0, 1].BackColor = Color.Orange;
					squareBoard[5, 1].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "The opponent decides to take your pawn with his Rook";
					TutorialText.Text += Environment.NewLine + "The opponent moves his Rook forward to take your pawn.";
					TutorialText.Text += Environment.NewLine + "It is not a good move as this allows you to take back the upper hand";
					TutorialText.Text += Environment.NewLine + "Press Next to see how";
				}
				else if (demoNum == 2)
				{
					RemoveLastMove(); ResetColours(false);
					board[0, 2] = "_";
					board[6, 2] = "wR";
					SetupPieces();
					foreach (int[] i in RookAvailable(squareBoard[6, 2]))
					{
						if (CheckAvailable(i, squareBoard[6, 2]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[6, 2].BackColor = Color.FromArgb(59, 217, 72);
					squareBoard[0, 1].BackColor = Color.Orange;
					squareBoard[5, 1].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "The opponent's Rook move allows access to the enemy back line";
					TutorialText.Text += Environment.NewLine + "It is not a good move as this allows you to take back the upper hand";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 3)
				{
					RemoveLastMove(); ResetColours(false);
					board[0, 2] = "wR";
					board[6, 2] = "_";
					board[1, 3] = "_";
					board[0, 4] = "bK";
					SetupPieces();
					squareBoard[0, 2].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[6, 2].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Your Rook now puts the King in check";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 4)
				{
					RemoveLastMove(); ResetColours(false);
					board[1, 3] = "bK";
					board[0, 4] = "_";
					board[0, 7] = "bQ";
					board[0, 2] = "wR";
					SetupPieces();
					squareBoard[1, 3].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[0, 4].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "With your Rook attacking the King the opponent has no other choice than to get away from check leaving a clean line to the enemy Queen";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else
				{
					NextButton.Enabled = false;
					RemoveLastMove(); ResetColours(false);
					board[0, 7] = "wR";
					board[0, 2] = "_";
					SetupPieces();
					squareBoard[0, 7].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[0, 2].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "You can now take the enemy Queen for free leaving you at a positive trade and a now winning position from a seamless losing postion";
					TutorialText.Text += Environment.NewLine + "This is the end of the tutorial on the Bishop";
					TutorialText.Text += Environment.NewLine + "Press Exit to leave";
				}
			}
		}

		private void RookClick(int y, int x)
		{
			ResetColours(false);
			squareBoard[y, x].BackColor = Color.FromArgb(59, 217, 72);
			TutorialText.Font = new Font("Microsoft Sans Serif", 12);
			TutorialText.Text = "This is a Rook";
			TutorialText.Text += Environment.NewLine + "The Rook can only move vertically or horizontally any number of squares but cannot move over other pieces";
			TutorialText.Text += Environment.NewLine + "If it moves where an opposite colour piece is it takes the piece";
			TutorialText.Text += Environment.NewLine + "Press the demo button to see how the Rook moves and works";
			demo = "R";
		}

		private void DemoBishop()
		{
			if (demo == "B")
			{
				if (demoNum == 0)
				{
					BackButton.Enabled = false;
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[7, 0] = "wB";
					board[5, 3] = "wk";
					board[2, 3] = "bR";
					board[2, 5] = "bK";
					board[3, 4] = "bk";
					SetupPieces();
					squareBoard[2, 1].BackColor = Color.Orange;
					squareBoard[2, 3].BackColor = Color.Orange;
					TutorialText.Text = "In this tutorial we will show the movement of the Bishop";
					TutorialText.Text += Environment.NewLine + "The opponent has decided to move his Rook to attack our Knight";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 1)
				{
					RemoveLastMove(); ResetColours(true);
					board = CreateEmptyBoard();
					board[7, 0] = "wB";
					board[5, 3] = "wk";
					board[2, 3] = "bR";
					board[2, 5] = "bK";
					board[3, 4] = "bk";
					SetupPieces();
					foreach (int[] i in BishopAvailable(squareBoard[7, 0]))
					{
						if (CheckAvailable(i, squareBoard[7, 0]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[7, 0].BackColor = Color.FromArgb(59, 217, 72);
					squareBoard[2, 1].BackColor = Color.Orange;
					squareBoard[2, 3].BackColor = Color.Orange;
					TutorialText.Text = "The opponent has made a fatal mistake by trying to defend their hanging Rook";
					TutorialText.Text += Environment.NewLine + "The Bishop can now take the enemy Knight";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 2)
				{
					RemoveLastMove(); ResetColours(true);
					board = CreateEmptyBoard();
					board[3, 4] = "wB";
					board[5, 3] = "wk";
					board[2, 3] = "bR";
					board[2, 5] = "bK";
					SetupPieces();
					squareBoard[2, 5].BackColor = Color.Red;
					squareBoard[7, 0].BackColor = Color.Orange;
					squareBoard[3, 4].BackColor = Color.Orange;
					TutorialText.Text = "The King cannot take the Bishop back as the Bishop is protected by our Knight.";
					TutorialText.Text += Environment.NewLine + "The Rook cannot take our Knight either as the King is in check therfore has to be moved away from danger";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 3)
				{
					RemoveLastMove(); ResetColours(true);
					board = CreateEmptyBoard();
					board[3, 4] = "wB";
					board[5, 3] = "wk";
					board[2, 3] = "bR";
					board[2, 4] = "bK";
					SetupPieces();
					squareBoard[2, 5].BackColor = Color.Orange;
					squareBoard[2, 4].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "The opponent is then forced into a losing situation and has to move away from danger";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 4)
				{
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[3, 4] = "wB";
					board[5, 3] = "wk";
					board[2, 3] = "bR";
					board[2, 4] = "bK";
					SetupPieces();
					foreach (int[] i in BishopAvailable(squareBoard[3, 4]))
					{
						if (CheckAvailable(i, squareBoard[3, 4]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[3, 4].BackColor = Color.FromArgb(59, 217, 72);
					TutorialText.Text = "Now our best move is to take the Rook";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else
				{
					NextButton.Enabled = false;
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[2, 3] = "wB";
					board[5, 3] = "wk";
					board[2, 4] = "bK";
					SetupPieces();
					squareBoard[2, 3].BackColor = Color.Orange;
					squareBoard[3, 4].BackColor = Color.Orange;
					TutorialText.Text = "After taking the Rook the opponent has lost a Rook and a Knight for a bishop when the king takes back.";
					TutorialText.Text += Environment.NewLine + "This means we have gotten a wining situtation from a losing postion as a Rook is considered stronger than a Bishop";
					TutorialText.Text += Environment.NewLine + "This is the end of the tutorial on the Bishop";
					TutorialText.Text += Environment.NewLine + "Press Exit to leave";
				}
			}
		}

		private void BishopClick(int y, int x)
		{
			ResetColours(false);
			squareBoard[y, x].BackColor = Color.FromArgb(59, 217, 72);
			TutorialText.Font = new Font("Microsoft Sans Serif", 12);
			TutorialText.Text = "This is a Bishop";
			TutorialText.Text += Environment.NewLine + "The Bishop can only move diagonally any number of square but cannot move over other pieces ";
			TutorialText.Text += Environment.NewLine + "will always end up on the same colour square as it started";
			TutorialText.Text += Environment.NewLine + "If it moves where an opposite colour piece is it takes the piece";
			TutorialText.Text += Environment.NewLine + "Press the demo button to see how the Bishop moves and works";
			demo = "B";
		}

		private void DemoKnight()
		{
			if (demo == "k")
			{
				if (demoNum == 0)
				{
					BackButton.Enabled = false;
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[6, 5] = "wk";
					board[2, 3] = "bQ";
					board[5, 2] = "bR";
					SetupPieces();
					foreach (int[] i in KnightAvailable(squareBoard[6, 5]))
					{
						if (CheckAvailable(i, squareBoard[6, 5]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[6, 5].BackColor = Color.FromArgb(59, 217, 72);
					TutorialText.Text = "This tutorial shows the movement of the Knight";
					TutorialText.Text += Environment.NewLine + "We want to move the Knight so that it threatens both the Rook and the enemy Queen";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 1)
				{
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[4, 4] = "wk";
					board[2, 3] = "bQ";
					board[5, 2] = "bR";
					SetupPieces();
					squareBoard[4, 4].BackColor = Color.Orange;
					squareBoard[6, 5].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "The best play is to move the Knight forward which threatens 2 pieces at once";
					TutorialText.Text += Environment.NewLine + "This is called a fork which ensures a lose-lose situation for the opponent" +
					"as they are forced to sacrifice one of their pieces either the Rook or the Queen which are both considered weaker than the Knight";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";

				}
				else if (demoNum == 2)
				{
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[4, 4] = "wk";
					board[2, 2] = "bQ";
					board[5, 2] = "bR";
					SetupPieces();
					squareBoard[2, 3].BackColor = Color.Orange;
					squareBoard[2, 2].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "The opponent decides to move their Queen protecting their Rook and sacrificing our Knight";
					TutorialText.Text += Environment.NewLine + "It is now our turn to play";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 3)
				{
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[4, 4] = "wk";
					board[2, 2] = "bQ";
					board[5, 2] = "bR";
					SetupPieces();
					squareBoard[4, 4].BackColor = Color.FromArgb(59, 217, 72);
					foreach (int[] i in KnightAvailable(squareBoard[4, 4]))
					{
						if (CheckAvailable(i, squareBoard[4, 4]))
						{
							ShowAvailable(i);
						}
					}
					squareBoard[2, 3].BackColor = Color.Orange;
					squareBoard[2, 2].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Now that the opponent has decided to save their most important piece, the Queen, and sacrificing their Rook instead";
					TutorialText.Text += Environment.NewLine + "Our best move is to now take the enemy Rook";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 4)
				{
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[5, 2] = "wk";
					board[2, 2] = "bQ";
					SetupPieces();
					squareBoard[5, 2].BackColor = Color.Orange;
					squareBoard[4, 4].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Now the opponent has no other choice than to take back";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else
				{
					NextButton.Enabled = false;
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[5, 2] = "bQ";
					SetupPieces();
					squareBoard[5, 2].BackColor = Color.Orange;
					squareBoard[2, 2].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "When the Queen takes back your Knight, it leaves us with a positive trade and in a real game could make the necessary difference to win a game";
					TutorialText.Text += Environment.NewLine + "This is the end of the tutorial on the Knight";
					TutorialText.Text += Environment.NewLine + "Press Exit to leave";
				}
			}
		}

		private void KnightClick(int y, int x)
		{
			ResetColours(false);
			squareBoard[y, x].BackColor = Color.FromArgb(59, 217, 72);
			TutorialText.Font = new Font("Microsoft Sans Serif", 12);
			TutorialText.Text = "This is a Knight";
			TutorialText.Text += Environment.NewLine + "The Knight is the only piece that can move over pieces";
			TutorialText.Text += Environment.NewLine + "The Knight moves 2 squares in any direction and then 1 square perpendicular to the direction of the first move";
			TutorialText.Text += Environment.NewLine + "If it moves where an opposite colour piece is it takes the piece";
			TutorialText.Text += Environment.NewLine + "Press the demo button to see how the Knight moves and works";
			demo = "k";
		}

		private void DemoPawn()
		{
			if (demo == "p")
			{
				if (demoNum == 0)
				{
					BackButton.Enabled = false;
					RemoveLastMove(); ResetColours(false);
					board = CreateEmptyBoard();
					board[6, 3] = "wp";
					board[1, 3] = "bp";
					SetupPieces();
					squareBoard[6, 3].BackColor = Color.FromArgb(59, 217, 72);
					foreach (int[] i in PawnAvailable(squareBoard[6, 3]))
					{
						if (CheckAvailable(i, squareBoard[6, 3]))
						{
							ShowAvailable(i);
						}
					}
					TutorialText.Text = "This tutorial is to show the movement of the pawn";
					TutorialText.Text += Environment.NewLine + "The pawn is in its starting position meaning it can move either 1 square up or 2 squares up";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 1)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[4, 3] = "wp";
					board[1, 3] = "bp";
					SetupPieces();
					squareBoard[4, 3].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[6, 3].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Now that you have made a move it is now the opponents turn to play";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 2)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[4, 3] = "wp";
					board[2, 3] = "bp";
					SetupPieces();
					squareBoard[1, 3].BackColor = Color.Orange;
					squareBoard[2, 3].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Now that opponent has made a move it is now your turn to play once again";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 3)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[4, 3] = "wp";
					board[2, 3] = "bp";
					SetupPieces();
					squareBoard[1, 3].BackColor = Color.Orange;
					squareBoard[2, 3].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[4, 3].BackColor = Color.FromArgb(59, 217, 72);
					squareBoard[3, 3].BackgroundImage = Properties.Resources.available_place_gray;
					TutorialText.Text = "Now the pawn has only one legal move and as it has already moved it cannot move 2 squares up again";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 4)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[3, 3] = "wp";
					board[2, 3] = "bp";
					SetupPieces();
					squareBoard[3, 3].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[4, 3].BackColor = Color.Orange;
					TutorialText.Text = "After the move is made it is now the opponents turn";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 5)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[3, 3] = "wp";
					board[2, 3] = "bp";
					SetupPieces();
					squareBoard[3, 3].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[4, 3].BackColor = Color.Orange;
					squareBoard[2, 3].BackColor = Color.FromArgb(59, 217, 72);
					TutorialText.Text = "The opponent cannot move his pawn as a pawn cannot move over another piece or take a piece moving up";
					TutorialText.Text += Environment.NewLine + "Press Next to move on to the next tutorial";
				}
				//Part 2
				else if (demoNum == 6)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[6, 3] = "wp";
					board[5, 2] = "bp";
					board[5, 4] = "bp";
					board[5, 3] = "bp";
					board[1, 3] = "bp";
					SetupPieces();
					squareBoard[6, 3].BackColor = Color.FromArgb(59, 217, 72);
					foreach (int[] i in PawnAvailable(squareBoard[6, 3]))
					{
						if (CheckAvailable(i, squareBoard[6, 3]))
						{
							ShowAvailable(i);
						}
					}
					TutorialText.Text = "We now move on to a next tutorial showing how to take pieces with the pawn";
					TutorialText.Text += Environment.NewLine + "The pawn can take both pieces on its diaginal left and right but cannot move forward as it is blocked by the enemy pawn";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 7)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[5, 2] = "wp";
					board[5, 4] = "bp";
					board[5, 3] = "bp";
					board[1, 3] = "bp";
					SetupPieces();
					squareBoard[5, 2].BackColor = Color.Orange;
					squareBoard[6, 3].BackColor = Color.Orange;
					TutorialText.Text = "The pawn has now taken the enemy pawn meaning it replaces its position and removes the last piece from the board";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 8)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[5, 2] = "wp";
					board[5, 4] = "bp";
					board[6, 3] = "bp";
					board[1, 3] = "bp";
					SetupPieces();
					squareBoard[5, 3].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[6, 3].BackColor = Color.Orange;
					TutorialText.Text = "The opponent pushes his pawn forward threatening promotion";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 9)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[4, 2] = "wp";
					board[5, 4] = "bp";
					board[6, 3] = "bp";
					board[1, 3] = "bp";
					SetupPieces();
					squareBoard[4, 2].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[5, 2].BackColor = Color.Orange;
					TutorialText.Text = "Your have to move up your pawn";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 10)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[4, 2] = "wp";
					board[5, 4] = "bp";
					board[7, 3] = "bQ";
					board[1, 3] = "bp";
					SetupPieces();
					squareBoard[7, 3].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[6, 3].BackColor = Color.Orange;
					TutorialText.Text = "Your opponent pushes his pawn forward and promoting it to a queen";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 11)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[3, 2] = "wp";
					board[5, 4] = "bp";
					board[7, 3] = "bQ";
					board[1, 3] = "bp";
					SetupPieces();
					squareBoard[4, 2].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[3, 2].BackColor = Color.Orange;
					TutorialText.Text = "You have no other option but to move your pawn up again";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 12)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[3, 2] = "wp";
					board[5, 4] = "bp";
					board[7, 3] = "bQ";
					board[3, 3] = "bp";
					SetupPieces();
					squareBoard[3, 3].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[1, 3].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "Your opponent decide to move his pawn up 2 squares from its starting postion";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else if (demoNum == 13)
				{
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[3, 2] = "wp";
					board[5, 4] = "bp";
					board[7, 3] = "bQ";
					board[3, 3] = "bp";
					SetupPieces();
					squareBoard[3, 2].BackColor = Color.FromArgb(59, 217, 72);
					squareBoard[2, 2].BackgroundImage = Properties.Resources.available_place_gray;
					squareBoard[2, 3].BackgroundImage = Properties.Resources.available_place_gray;
					squareBoard[3, 3].BackColor = Color.FromArgb(246, 190, 0);
					squareBoard[1, 3].BackColor = Color.FromArgb(246, 190, 0);
					TutorialText.Text = "You can now move your pawn diagonally even though there is no piece there this is due to the en passent rule meaning that if there is a pawn " +
					"that moves up 2 squares and end up next to an opposite color pawn that pawn can take it diagonally as if the other pawn had only moved up 1 square";
					TutorialText.Text += Environment.NewLine + "Press Next to continue";
				}
				else
				{
					NextButton.Enabled = false;
					ResetColours(false); RemoveLastMove();
					board = CreateEmptyBoard();
					board[2, 3] = "wp";
					board[5, 4] = "bp";
					board[7, 3] = "bQ";
					SetupPieces();
					squareBoard[2, 3].BackColor = Color.Orange;
					squareBoard[3, 2].BackColor = Color.Orange;
					TutorialText.Text = "The pawn no moves to the sqaure diagonally taking the pawn above it";
					TutorialText.Text += Environment.NewLine + "This is the end of the tutorial on the pawn";
					TutorialText.Text += Environment.NewLine + "Press Exit to leave";
				}
			}
		}

		private void PawnClick(int y, int x)
		{
			ResetColours(false);
			squareBoard[y, x].BackColor = Color.FromArgb(59, 217, 72);
			TutorialText.Font = new Font("Microsoft Sans Serif", 12);
			TutorialText.Text = "This is a Pawn";
			TutorialText.Text += Environment.NewLine + "The Pawn can move up to 2 squares upwards at its starting position and 1 square upwards after that ";
			TutorialText.Text += Environment.NewLine + "If their is a piece of opposite colour diagonally upwards to it then the Pawn can take that piece";
			TutorialText.Text += Environment.NewLine +
			"The Pawn cannot move horizontally or downwards and when it reaches the other side of the board it can promote meaning" +
			"it can turn into any piece of its colour apart from a King or a Pawn";
			TutorialText.Text += Environment.NewLine + "Press the demo button to see how the Pawn moves and works";
			demo = "p";
		}

		private void TutBotMove()
        {
			if (enabled)
			{
				bool hasMoved = false;
				int num1 = 0, num2 = 0, num3; int[] coords = new int[2];
				while (!hasMoved)
				{
					num1 = rnd.Next(0, 8); num2 = rnd.Next(0, 8);
					if (board[num1, num2].Contains(col))
					{
						if (board[num1, num2].Contains('R'))
						{
							if (RookAvailable(squareBoard[num1, num2]).Count > 0)
							{
								num3 = rnd.Next(0, RookAvailable(squareBoard[num1, num2]).Count);
								coords = RookAvailable(squareBoard[num1, num2])[num3];
								if (CheckAvailable(coords, squareBoard[num1, num2]))
								{
									hasMoved = true;
								}
							}
						}
						else if (board[num1, num2].Contains('p'))
						{
							if (PawnAvailable(squareBoard[num1, num2]).Count > 0)
							{
								num3 = rnd.Next(0, PawnAvailable(squareBoard[num1, num2]).Count);
								coords = PawnAvailable(squareBoard[num1, num2])[num3];
								if (CheckAvailable(coords, squareBoard[num1, num2]))
								{
									hasMoved = true;
								}
							}
						}
						else if (board[num1, num2].Contains('B'))
						{
							if (BishopAvailable(squareBoard[num1, num2]).Count > 0)
							{
								num3 = rnd.Next(0, BishopAvailable(squareBoard[num1, num2]).Count);
								coords = BishopAvailable(squareBoard[num1, num2])[num3];
								if (CheckAvailable(coords, squareBoard[num1, num2]))
								{
									hasMoved = true;
								}
							}
						}
						else if (board[num1, num2].Contains('Q'))
						{
							if (QueenAvailable(squareBoard[num1, num2]).Count > 0)
							{
								num3 = rnd.Next(0, QueenAvailable(squareBoard[num1, num2]).Count);
								coords = QueenAvailable(squareBoard[num1, num2])[num3];
								if (CheckAvailable(coords, squareBoard[num1, num2]))
								{
									hasMoved = true;
								}
							}
						}
						else if (board[num1, num2].Contains('k'))
						{
							if (KnightAvailable(squareBoard[num1, num2]).Count > 0)
							{
								num3 = rnd.Next(0, KnightAvailable(squareBoard[num1, num2]).Count);
								coords = KnightAvailable(squareBoard[num1, num2])[num3];
								if (CheckAvailable(coords, squareBoard[num1, num2]))
								{
									hasMoved = true;
								}
							}
						}
						else if (board[num1, num2].Contains('K'))
						{
							if (KingAvailable(squareBoard[num1, num2]).Count > 0)
							{
								num3 = rnd.Next(0, KingAvailable(squareBoard[num1, num2]).Count);
								coords = KingAvailable(squareBoard[num1, num2])[num3];
								if (CheckAvailable(coords, squareBoard[num1, num2]))
								{
									hasMoved = true;
								}
							}
						}
					}
				}
				PictureBox piece = squareBoard[num1, num2];
				PictureBox piece2 = squareBoard[coords[0], coords[1]];
				ResetColours(true);
				RemoveLastMove();
				y1 = piece.Top / 60; y2 = piece2.Top / 60; x1 = piece.Left / 60; x2 = piece2.Left / 60;
				LastMoves.Add(new int[] { y1, x1 });
				LastMoves.Add(new int[] { y2, x2 });
				HasMoved(piece);
				RemoveDoubleMoved(piece);
				DoubleMoved(piece, piece2);
				EnPassant(piece, piece2);
				Castle(piece, piece2);
				string name = piece.Name;
				board[y1, x1] = "_";
				board[y2, x2] = name;
				PromoteBot();
				SetupPieces();
				bool colour = false;
				if (col == 'w')
				{
					timerB.Start(); timer.Stop();
					col = 'b';
					colour = true;
				}
				else
				{
					timerB.Stop(); timer.Start();
					col = 'w';
				}
				CheckColour(colour);
				CreateSetupText();
                replayMoves.Add(setupText);
                moves.Add(CloneBoard());
				movesNum++;
				BackMoveButton.Enabled = true;
				if (CheckCanMove(colour))
				{
					int endGameStatus = 0;
					if (CheckCheck(colour))
					{
						enabled = false; timer.Stop(); timerB.Stop();
						CheckMateText.Visible = true;
						CheckMateText.Text = Environment.NewLine + Environment.NewLine;
                        if (colour)
                        {
                            CheckMateText.Text += "White wins";
                        }
                        else
                        {
                            CheckMateText.Text += "Black wins";
							endGameStatus = 1;
                        }
                        CheckMateText.Text += Environment.NewLine + "by Checkmate!";
						XButton.Visible = true;
					}
					else
					{
						enabled = false; timer.Stop(); timerB.Stop();
						CheckMateText.Visible = true;
						CheckMateText.Text = Environment.NewLine + Environment.NewLine + "Stalemate!";
						XButton.Visible = true;
						endGameStatus = 2;
					}
                    if (loggedIn)
                    {
                        SQLiteConnection conn = CreateConnection();
                        int gameID = InsertNewGame(conn, ID, endGameStatus);
                        foreach (string game in replayMoves)
                        {
                            InsertNewGameMove(conn, gameID, game);
                        }
                    }
                    replayMoves.Clear();
                }
				int pieces = 0;
				for (int q = 0; q < 8; q++)
				{
					for (int w = 0; w < 8; w++)
					{
						if (board[q, w] != "_")
						{
							pieces++;
						}
					}
				}
				if (pieces < 3)
				{
					enabled = false; timer.Stop(); timerB.Stop();
					CheckMateText.Visible = true;
					CheckMateText.Text = Environment.NewLine + Environment.NewLine + "Draw by insufficient material!";
					XButton.Visible = true;
				}
				AddMove();
				if (soundEffects)
                {
                    toc.Play();
                }
                ResetColours(false);
			}
		}

		private int PieceAdv()
		{
			Dictionary<string, int> pieceValue = new Dictionary<string, int>
			{
				{ "wp", 1 },
				{ "bp", -1 },
				{ "wk", 3 },
				{ "bk", -3 },
				{ "wB", 3 },
				{ "bB", -3 },
				{ "wR", 5 },
				{ "bR", -5 },
				{ "wQ", 9 },
				{ "bQ", -9 },
				{ "wK", 0 },
				{ "bK", 0 }
			};
			int num = 0;
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					if (board[i, j].Length > 1)
					{
						string s = board[i, j][0].ToString() + board[i, j][1].ToString();
						num += pieceValue[s];
					}
				}
			}
			return num;
		}

		private void NextButton_Click(object sender, EventArgs e)
		{
			BackButton.Enabled = true;
			demoNum++;
			if (demo == "p")
			{
				DemoPawn();
			}
			else if (demo == "k")
			{
				DemoKnight();
			}
			else if (demo == "B")
			{
				DemoBishop();
			}
			else if (demo == "R")
			{
				DemoRook();
			}
			else if (demo == "Q")
			{
				DemoQueen();
			}
			else if (demo == "K")
			{
				DemoKing();
			}
		}

		private void BackButton_Click(object sender, EventArgs e)
		{
			NextButton.Enabled = true;
			demoNum--;
			if (demo == "p")
			{
				DemoPawn();
			}
			else if (demo == "k")
			{
				DemoKnight();
			}
			else if (demo == "B")
			{
				DemoBishop();
			}
			else if (demo == "R")
			{
				DemoRook();
			}
			else if (demo == "Q")
			{
				DemoQueen();
			}
			else if (demo == "K")
			{
				DemoKing();
			}
		}

		private void BlackKingButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "bK";
		}

		private void BlackQueenButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "bQ";
		}

		private void BlackRookButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "bR";
		}

		private void BlackBishopButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "bB";
		}

		private void BlackKnightButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "bk";
		}

		private void BlackPawnButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "bp";
		}

		private void WhiteKingButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "wK";
		}

		private void WhiteQueenButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "wQ";
		}

		private void WhiteRookButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "wR";
		}

		private void BlackMoveCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (WhiteMoveCheckBox.Checked == false && BlackMoveCheckBox.Checked == false)
			{
				BlackMoveCheckBox.Checked = true;
				col = 'b';
			}
			if (BlackMoveCheckBox.Checked)
			{
				WhiteMoveCheckBox.Checked = false;
				col = 'b';
				CreateSetupText();
				BoardSetupText.Text = setupText;
			}
		}

		private void WhiteMoveCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (WhiteMoveCheckBox.Checked == false && BlackMoveCheckBox.Checked == false)
			{
				WhiteMoveCheckBox.Checked = true;
				col = 'w';
			}
			if (WhiteMoveCheckBox.Checked)
			{
				BlackMoveCheckBox.Checked = false;
				col = 'w';
				CreateSetupText();
				BoardSetupText.Text = setupText;
			}
		}

		private void ResetButton_Click(object sender, EventArgs e)
		{
            White00.Checked = false; White000.Checked = false; Black00.Checked = false; Black000.Checked = false;
            WhiteMoveCheckBox.Checked = true;
			if (rev)
            {
				SwitchTimers();
				ReverseBoard();
            }
			string[,] emptyBoard = CreateEmptyBoard();
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					board[i, j] = emptyBoard[i, j];
				}
			}
			SetupPieces();
			CreateSetupText();
			BoardSetupText.Text = setupText;
		}

		private void BulletTimer_CheckedChanged(object sender, EventArgs e)
		{
			if (BulletTimer.Checked)
			{
				BlitzTimer.Checked = false;
				RapidTimer.Checked = false;
				ClassicTimer.Checked = false;
				CasualTimer.Checked = false;
				ActiveTimer.Visible = true;
				InactiveTimer.Visible = true;
				ActiveTimer.Text = "01:00";
				InactiveTimer.Text = "01:00";
			}
			if (!BulletTimer.Checked && !BlitzTimer.Checked && !RapidTimer.Checked && !ClassicTimer.Checked && !CasualTimer.Checked)
			{
				BulletTimer.Checked = true;
			}
		}

		private void BlitzTimer_CheckedChanged(object sender, EventArgs e)
		{
			if (BlitzTimer.Checked)
			{
				BulletTimer.Checked = false;
				RapidTimer.Checked = false;
				ClassicTimer.Checked = false;
				CasualTimer.Checked = false;
				ActiveTimer.Visible = true;
				InactiveTimer.Visible = true;
				ActiveTimer.Text = "03:00";
				InactiveTimer.Text = "03:00";
				timerTick = true;
			}
			if (!BulletTimer.Checked && !BlitzTimer.Checked && !RapidTimer.Checked && !ClassicTimer.Checked && !CasualTimer.Checked)
			{
				BlitzTimer.Checked = true;
			}
		}

		private void RapidTimer_CheckedChanged(object sender, EventArgs e)
		{
			if (RapidTimer.Checked)
			{
				BulletTimer.Checked = false;
				BlitzTimer.Checked = false;
				ClassicTimer.Checked = false;
				CasualTimer.Checked = false;
				ActiveTimer.Visible = true;
				InactiveTimer.Visible = true;
				ActiveTimer.Text = "10:00";
				InactiveTimer.Text = "10:00";
				timerTick = true;
			}
			if (!BulletTimer.Checked && !BlitzTimer.Checked && !RapidTimer.Checked && !ClassicTimer.Checked && !CasualTimer.Checked)
			{
				RapidTimer.Checked = true;
			}
		}

		private void ClassicTimer_CheckedChanged(object sender, EventArgs e)
		{
			if (ClassicTimer.Checked)
			{
				BulletTimer.Checked = false;
				BlitzTimer.Checked = false;
				RapidTimer.Checked = false;
				CasualTimer.Checked = false;
				ActiveTimer.Visible = true;
				InactiveTimer.Visible = true;
				ActiveTimer.Text = "60:00";
				InactiveTimer.Text = "60:00";
				timerTick = true;
			}
			if (!BulletTimer.Checked && !BlitzTimer.Checked && !RapidTimer.Checked && !ClassicTimer.Checked && !CasualTimer.Checked)
			{
				ClassicTimer.Checked = true;
			}
		}

		private void CasualTimer_CheckedChanged(object sender, EventArgs e)
		{
			if (CasualTimer.Checked)
			{
				BulletTimer.Checked = false;
				BlitzTimer.Checked = false;
				RapidTimer.Checked = false;
				ClassicTimer.Checked = false;
				ActiveTimer.Visible = false;
				InactiveTimer.Visible = false;
                ActiveTimer.Text = "99:00";
                InactiveTimer.Text = "99:00";
                timerTick = false;
			}
			if (!BulletTimer.Checked && !BlitzTimer.Checked && !RapidTimer.Checked && !ClassicTimer.Checked && !CasualTimer.Checked)
			{
				CasualTimer.Checked = true;
			}
		}

		private void EraseButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "_";
		}

		private void DemoButton_Click(object sender, EventArgs e)
		{
			board = CreateEmptyBoard();
			tutorial = false; enabled = false;
			SetupPieces();
			ResetColours(false);
			DemoButton.Enabled = false; demoNum = 0;
			NextButton.Enabled = true; ExitDemoButton.Visible = true;
			NextButton.Visible = true; BackButton.Visible = true;
			if (demo == "p")
			{
				DemoPawn();
			}
			else if (demo == "k")
			{
				DemoKnight();
			}
			else if (demo == "B")
			{
				DemoBishop();
			}
			else if (demo == "R")
			{
				DemoRook();
			}
			else if (demo == "Q")
			{
				DemoQueen();
			}
			else if (demo == "K")
			{
				DemoKing();
			}
		}

		private void ExitDemoButton_Click(object sender, EventArgs e)
		{
			RemoveLastMove(); ResetColours(true);
			ExitDemoButton.Visible = false; DemoButton.Visible = false;
			NextButton.Visible = false; BackButton.Visible= false;
			Tutorial();
		}

		private void ChessRulesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("chrome.exe", "https://www.chess.com/learn-how-to-play-chess");
		}

		private void XButton_Click(object sender, EventArgs e)
		{
			CheckMateText.Visible = false;
			XButton.Visible = false;
		}

        private void FlipBoardButton_Click(object sender, EventArgs e)
        {
			SwitchTimers();
			ReverseBoard();
			SetupPieces();
			CreateSetupText();
			BoardSetupText.Text = setupText;
		}

		private void TutorialBotButton_Click(object sender, EventArgs e)
        {
			enabled = true;
			BackMoveButton.Visible = true; NextMoveButton.Visible = true;
            CreateSetupText();
            replayMoves.Add(setupText);
            moves.Add(CloneBoard());
            enableRev = false;
			TutorialText.Visible = false;
			tutorial = false;
			TutorialBotButton.Visible = false;
			ChessRulesLink.Visible = false;
			tutBot = true;
			AutomaticFlipsCheckBox.Checked = false;
			MovesCounter.Visible = true;
		}

		private string[,] CloneBoard()
		{
			string[,] boardClone = new string[8, 8];
			if (!rev)
			{ 
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 8; j++)
					{
						boardClone[i, j] = board[i, j];
					}
				}
			}
			else
			{
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 8; j++)
					{
						boardClone[i, j] = board[7 - i, 7 - j];
					}
				}
			}
            return boardClone;
		}

        private void FlipBoardButton2_Click(object sender, EventArgs e)
        {
			if (enabled)
			{
				ReverseBoard();
				SetupPieces();
				ResetColours(false);
			}
		}

        private void AutomaticFlipsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
			if (AutomaticFlipsCheckBox.Checked)
            {
				enableRev = true;
            }
            else
            {
				enableRev = false;
            }
        }

        private void BotCheckBox_CheckedChanged(object sender, EventArgs e)
        {
			AIMediumLevel = false;
            timerTick = true;
            ActiveTimer.Visible = true;
            InactiveTimer.Visible = true;
            if (BotCheckBox.Checked)
            {
				timerTick = false;
				ActiveTimer.Visible = false;
				InactiveTimer.Visible = false;
				AIMediumLevel = true;
            }
        }

        private void Check000_CheckedChanged(object sender, EventArgs e)
        {
			if (Check000.Checked)
            {
				TimeBetweenBotMove = 0;
				Check025.Checked = false;
				Check050.Checked = false;
				Check100.Checked = false;
			}
			if (!Check000.Checked && !Check025.Checked && !Check050.Checked && !Check100.Checked)
            {
				Check000.Checked = true;
            }
        }

        private void Check025_CheckedChanged(object sender, EventArgs e)
        {
			if (Check025.Checked)
			{
				TimeBetweenBotMove = 250;
				Check000.Checked = false;
				Check050.Checked = false;
				Check100.Checked = false;
			}
			if (!Check000.Checked && !Check025.Checked && !Check050.Checked && !Check100.Checked)
			{
				Check000.Checked = true;
			}
		}

		private void Check050_CheckedChanged(object sender, EventArgs e)
        {
			if (Check050.Checked)
			{
				TimeBetweenBotMove = 500;
				Check025.Checked = false;
				Check000.Checked = false;
				Check100.Checked = false;
			}
			if (!Check000.Checked && !Check025.Checked && !Check050.Checked && !Check100.Checked)
			{
				Check000.Checked = true;
			}
		}

		private void Check100_CheckedChanged(object sender, EventArgs e)
        {
			if (Check100.Checked)
			{
				TimeBetweenBotMove = 1000;
				Check025.Checked = false;
				Check050.Checked = false;
				Check000.Checked = false;
			}
			if (!Check000.Checked && !Check025.Checked && !Check050.Checked && !Check100.Checked)
			{
				Check000.Checked = true;
			}
		}

		private void AddMove()
        {
			string smoves = "";
			for (int i = 7; i < MovesCounter.Text.Length; i++)
			{
				smoves += MovesCounter.Text[i];
			}
			MovesCounter.Text = "Moves: ";
			int moves = int.Parse(smoves);
			moves++;
			MovesCounter.Text += moves.ToString();
		}

		private void SubtractMove()
		{
            string smoves = "";
            for (int i = 7; i < MovesCounter.Text.Length; i++)
            {
                smoves += MovesCounter.Text[i];
            }
            MovesCounter.Text = "Moves: ";
            int moves = int.Parse(smoves);
            moves--;
            MovesCounter.Text += moves.ToString();
        }

        private void BotVBotButton_Click(object sender, EventArgs e)
        {
            BackMoveButton.Visible = true; NextMoveButton.Visible = true;
            BotVBotButton.Visible = false;
			TimeBetweenMoves.Visible = false;
			Check000.Visible = false;
			Check025.Visible = false;
			Check050.Visible = false;
			Check100.Visible = false;
			timerTick = false;
			ActiveTimer.Visible = false;
			InactiveTimer.Visible = false;
			MovesCounter.Visible = true;
			board = CreateBoard();
            CreateSetupText();
            replayMoves.Add(setupText);
            moves.Add(CloneBoard());
            SetupPieces();
			Refresh();
			while (enabled)
			{
				TutBotMove();
				System.Threading.Thread.Sleep(TimeBetweenBotMove);
				Refresh();
			}
		}

		private void BoardSetup(string s, List<int> z, int i, int j)
        {
			string l = BoardSetupText.Text, m = "";
			for (int k = l.Length - 1; k > 0; k--)
            {
				if (l[k] != 'w' && l[k] != 'b')
                {
					m += l[k].ToString();
                }
                else
                {
					break;
                }
            }
			l = m;
			if (s[i] == 'R')
            {
				board[j, z[j]] = "wR";
				if (!rev && j == 7 && z[j] == 7 && !l.Contains('K') || rev && j == 0 && z[j] == 0 && !l.Contains('K'))
                {
					board[j, z[j]] += "M";
				}
				else if (!rev && j == 7 && z[j] == 0 && !l.Contains('Q') || rev && j == 0 && z[j] == 7 && !l.Contains('Q'))
                {
					board[j, z[j]] += "M";
				}
				else if (j != 7 & j != 0 || z[j] != 7 && z[j] != 0)
                {
					board[j, z[j]] += "M";
				}
			}
			else if (s[i] == 'r')
            {
				board[j, z[j]] = "bR";
				if (!rev && j == 0 && z[j] == 7 && !l.Contains('k') || rev && j == 7 && z[j] == 0 && !l.Contains('k'))
				{
					board[j, z[j]] += "M";
				}
				else if (!rev && j == 0 && z[j] == 0 && !l.Contains('q') || rev && j == 7 && z[j] == 7 && !l.Contains('q'))
				{
					board[j, z[j]] += "M";
				}
				else if (j != 7 & j != 0 || z[j] != 7 && z[j] != 0)
				{
					board[j, z[j]] += "M";
				}
			}
			else if (s[i] == 'Q')
			{
				board[j, z[j]] = "wQ";
			}
			else if (s[i] == 'q')
			{
				board[j, z[j]] = "bQ";
			}
			else if (s[i] == 'K')
			{
				board[j, z[j]] = "wK";
			}
			else if (s[i] == 'k')
			{
				board[j, z[j]] = "bK";
			}
			else if (s[i] == 'B')
			{
				board[j, z[j]] = "wB";
			}
			else if (s[i] == 'b')
			{
				board[j, z[j]] = "bB";
			}
			else if (s[i] == 'N')
			{
				board[j, z[j]] = "wk";
			}
			else if (s[i] == 'n')
			{
				board[j, z[j]] = "bk";
			}
			else if (s[i] == 'P')
			{
				board[j, z[j]] = "wp";
			}
			else if (s[i] == 'p')
			{
				board[j, z[j]] = "bp";
			}
			else
            {
				z[j]--;
            }
			z[j]++;
		}

		private void ReverseBoardSetup(int i, int j, ref string text)
        {
			if (board[i, j].Contains("wK"))
            {
				text += "K";
			}
			else if (board[i, j].Contains("bK"))
			{
                text += "k";
			}
			else if (board[i, j].Contains("wQ"))
			{
                text += "Q";
			}
			else if (board[i, j].Contains("bQ"))
			{
                text += "q";
			}
			if (board[i, j].Contains("wR"))
			{
                text += "R";
			}
			else if (board[i, j].Contains("bR"))
			{
                text += "r";
			}
			if (board[i, j].Contains("wB"))
			{
                text += "B";
			}
			else if (board[i, j].Contains("bB"))
			{
                text += "b";
			}
			if (board[i, j].Contains("wk"))
			{
                text += "N";
			}
			else if (board[i, j].Contains("bk"))
			{
                text += "n";
			}
			if (board[i, j].Contains("wp"))
			{
                text += "P";
			}
			else if (board[i, j].Contains("bp"))
			{
                text += "p";
			}
		}

        private void BoardSetupText_TextChanged(object sender, EventArgs e)
		{
			bool all8 = true;
			List<int> z = new List<int>();
			string text = BoardSetupText.Text;
			for (int j = 0; j < 8; j++)
			{
				z.Add(0);
				string s = "";
				for (int i = 0; i < text.Length; i++)
				{
					if (text[i] == '/' || text[i] == ' ')
					{
						break;
					}
					else
					{
						s += text[i];
					}
				}
				string t = "";
				for (int i = s.Length + 1; i < text.Length; i++)
				{
					t += text[i];
				}
                text = t;
                if (s.Length == 0 || text.Length == 0)
                {
					break;
                }
				for (int i = 0; i < s.Length; i++)
				{
					if (int.TryParse(s[i].ToString(), out int x))
					{
						if (x + z[j] < 9)
						{
							for (int k = z[j]; k < x + z[j]; k++)
							{
								board[j, k] = "_";
							}
							z[j] += x;
						}
                        else
                        {
							all8 = false;
                        }
					}
					else
					{
						if (z[j] < 8)
						{
							BoardSetup(s, z, i, j);
						}
                        else
                        {
							all8 = false;
                        }
					}
				}
			}
			if (z.Count == 8)
			{
				for (int i = 0; i < 8; i++)
				{
					if (z[i] != 8)
					{
						all8 = false;
					}
				}
			}
			else
            {
				all8 = false;
            }
			if (all8)
			{
				if (text.Length > 0)
				{
					if (text[0] == 'b')
					{
						BlackMoveCheckBox.Checked = true;
					}
					else if (text[0] == 'w')
					{
						WhiteMoveCheckBox.Checked = true;
					}
				}
                string t = "";
                for (int i = 2; i < text.Length; i++)
                {
                    t += text[i];
                }
                text = t;
				for (int i = 2; i < 6; i++)
				{
					for (int j = 0; j < 8; j++)
					{
						if (board[i, j].Contains('p') && !board[i, j].Contains('M'))
						{
							board[i, j] += "M";
						}
					}
				}
				for (int j = 0; j < 8; j++)
				{
					if (board[1, j].Contains("wp") && !board[1, j].Contains('M') && !rev)
					{
                        board[1, j] += "M";
                    }
                    else if (board[6, j].Contains("wp") && !board[6, j].Contains('M') && rev)
                    {
                        board[6, j] += "M";
                    }

                    if (board[1, j].Contains("bp") && !board[1, j].Contains('M') && rev)
                    {
                        board[1, j] += "M";
                    }
                    else if (board[6, j].Contains("bp") && !board[6, j].Contains('M') && !rev)
                    {
                        board[6, j] += "M";
                    }
                }
                if (text.Length > 0)
				{
					if (puttingPieces)
					{
						if (!White00.Checked && !rev && board[7, 7].Contains("wR"))
						{
							board[7, 7] += "M";
						}
						else if (!White00.Checked && rev && board[0, 0].Contains("wR"))
						{
							board[0, 0] += "M";
						}
						if (White00.Checked && !rev && board[7, 7].Contains("wR"))
						{
							board[7, 7] = "wR";
						}
						else if (White00.Checked && rev && board[0, 0].Contains("wR"))
						{
							board[0, 0] = "wR";
						}

						if (!White000.Checked && !rev && board[7, 0].Contains("wR"))
						{
							board[7, 0] += "M";
						}
						else if (!White000.Checked && rev && board[0, 7].Contains("wR"))
						{
							board[0, 7] += "M";
						}
						if (White000.Checked && !rev && board[7, 0].Contains("wR"))
						{
							board[7, 0] = "wR";
						}
						else if (White000.Checked && rev && board[0, 7].Contains("wR"))
						{
							board[0, 7] = "wR";
						}

						if (!Black00.Checked && !rev && board[0, 7].Contains("bR"))
						{
							board[0, 7] += "M";
						}
						else if (!Black00.Checked && rev && board[7, 0].Contains("bR"))
						{
							board[7, 0] += "M";
						}
						if (Black00.Checked && !rev && board[0, 7].Contains("bR"))
						{
							board[0, 7] = "bR";
						}
						else if (Black00.Checked && rev && board[7, 0].Contains("bR"))
						{
							board[7, 0] = "bR";
						}

						if (!Black000.Checked && !rev && board[0, 0].Contains("bR"))
						{
							board[0, 0] += "M";
						}
						else if (!Black000.Checked && rev && board[7, 7].Contains("bR"))
						{
							board[7, 7] += "M";
						}
						if (Black000.Checked && !rev && board[0, 0].Contains("bR"))
						{
							board[0, 0] = "bR";
						}
						else if (Black000.Checked && rev && board[7, 7].Contains("bR"))
						{
							board[7, 7] = "bR";
						}
					}
					
					{
                        if (!text.Contains('K') && !rev && board[7, 7].Contains("wR"))
                        {
                            board[7, 7] += "M";
                        }
                        else if (!text.Contains('K') && rev && board[0, 0].Contains("wR"))
                        {
                            board[0, 0] += "M";
                        }
						if (text.Contains('K') && !rev && board[7, 7].Contains("wR"))
                        {
                            board[7, 7] = "wR";
                        }
						else if (text.Contains('K') && rev && board[0, 0].Contains("wR"))
						{
							board[0, 0] = "wR";
						}


                        if (!text.Contains('Q') && !rev && board[7, 0].Contains("wR"))
                        {
                            board[7, 0] += "M";
                        }
                        else if (!text.Contains('Q') && rev && board[0, 7].Contains("wR"))
                        {
                            board[0, 7] += "M";
                        }

                        if (!text.Contains('k') && !rev && board[0, 7].Contains("wR"))
                        {
                            board[0, 7] += "M";
                        }
                        else if (!text.Contains('Q') && rev && board[7, 0].Contains("wR"))
                        {
                            board[7, 0] += "M";
                        }

                        if (!text.Contains('q') && !rev && board[0, 0].Contains("wR"))
                        {
                            board[0, 0] += "M";
                        }
                        else if (!text.Contains('Q') && rev && board[7, 7].Contains("wR"))
                        {
                            board[7, 7] += "M";
                        }
                    }
				}
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 8; j++)
					{
						if (!White00.Checked && !White000.Checked && board[i , j].Contains("wK") && !board[i, j].Contains("M"))
						{
							board[i, j] += "M";
						}
                        if (!Black00.Checked && !Black000.Checked && board[i, j].Contains("bK") && !board[i, j].Contains("M"))
                        {
                            board[i, j] += "M";
                        }
                    }
				}
                if (text.Contains("1"))
				{
					if (!rev)
					{
						ReverseBoard();
						SwitchTimers();
					}
                }
				else if (text.Contains("0"))
				{
					if (rev)
					{
						ReverseBoard();
						SwitchTimers();
                    }
                }
                SetupPieces();
            }
        }

        private void CreateSetupText()
		{
			BoardSetupText.Text = "";
			string text = "";
			for (int i = 0; i < 8; i++)
			{
				int emptySpaces = 0;
				for (int j = 0; j < 8; j++)
				{
					if (board[i, j] == "_")
					{
						emptySpaces++;
					}
					else
					{
						if (emptySpaces > 0)
						{
							text += emptySpaces.ToString();
							emptySpaces = 0;
						}
						ReverseBoardSetup(i, j, ref text);
					}
				}
				if (emptySpaces > 0)
				{
					text += emptySpaces.ToString();
				}
				if (i != 7)
				{
                    text += "/";
				}
				else
				{
                    text += " ";
				}
			}
            text += col.ToString();
            text += " ";
			if (!puttingPieces)
			{
				for (int i = 0; i < 2; i++)
				{
                    int k = 0;
                    if (i == 1)
					{
						k = 7;
					}
					for (int j = 0; j < 8; j++)
					{
						if (board[k, j].Contains("wK") && !board[k, j].Contains('M'))
						{
							if (board[k, 7].Contains("wR") && !rev && !board[k, 7].Contains("M"))
							{
                                text += "K";
							}
							else if (board[k, 0].Contains("wR") && rev && !board[k, 0].Contains("M"))
							{
                                text += "K";
                            }

							if (board[k, 0].Contains("wR") && !rev && !board[k, 0].Contains("M"))
							{
                                text += "Q";
                            }
							else if (board[k, 7].Contains("wR") && rev && !board[k, 7].Contains("M"))
							{
                                text += "Q";
                            }
                        }
						if (board[k, j].Contains("bK") && !board[k, j].Contains('M'))
						{
							if (board[k, 7].Contains("bR") && !rev && !board[k, 7].Contains("M"))
							{
                                text += "k";
                            }
							else if (board[k, 0].Contains("bR") && rev && !board[k, 0].Contains("M"))
							{
                                text += "k";
                            }

							if (board[k, 0].Contains("bR") && !rev && !board[k, 0].Contains("M"))
							{
								text += "q";
                            }
							else if (board[k, 7].Contains("bR") && rev && !board[k, 7].Contains("M"))
							{
								text += "q";
                            }
                        }
                    }
				}
            }
            if (puttingPieces)
			{
				if (White00.Checked)
				{
					text += "K";
				}
				if (White000.Checked)
				{
					text += "Q";
				}
				if (Black00.Checked)
				{
					text += "k";
				}
				if (Black000.Checked)
				{
					text += "q";
				}
			}
            if (text[text.Length - 1] != ' ')
			{
                text += " ";
            }
			if (rev)
            {
				text += "1";
			}
			else
            {
				text += "0";
			}
			setupText = text;
        }

		private void White00_CheckedChanged(object sender, EventArgs e)
		{
			CreateSetupText();
			BoardSetupText.Text = setupText;
		}

		private void White000_CheckedChanged(object sender, EventArgs e)
		{
            CreateSetupText();
            BoardSetupText.Text = setupText;
        }

		private void Black00_CheckedChanged(object sender, EventArgs e)
		{
            CreateSetupText();
            BoardSetupText.Text = setupText;
        }

		private void Black000_CheckedChanged(object sender, EventArgs e)
		{
            CreateSetupText();
            BoardSetupText.Text = setupText;
        }

        private void BackMoveButton_Click(object sender, EventArgs e)
        {
			if (pastGame)
			{
				pastGameMove--;
                SubtractMove();
                bool revIn = rev;
				BoardSetupText.Text = pastGameMoves[pastGameMove];
				rev = revIn;
                NextMoveButton.Enabled = true;
				SetupPieces();
				ResetColours(true);
				CheckColour(true); CheckColour(false);
                if (pastGameMove == 0)
                {
                    BackMoveButton.Enabled = false;
                }
            }
			else
			{
				movesNum--;
				SubtractMove();
				board = moves[movesNum];
				if (rev)
				{
					ReverseBoard();
				}
				FlipBoardButton2.Enabled = false;
				enabled = false;
				NextMoveButton.Enabled = true;
				SetupPieces();
				RemoveLastMove();
				ResetColours(true);
                CheckColour(true); CheckColour(false);
                if (movesNum == 0)
				{
					BackMoveButton.Enabled = false;
				}
			}
        }

        private void NextMoveButton_Click(object sender, EventArgs e)
        {
			if (pastGame)
			{
                pastGameMove++;
                AddMove();
                bool revIn = rev;
                BoardSetupText.Text = pastGameMoves[pastGameMove];
				rev = revIn;
                BackMoveButton.Enabled = true;
                SetupPieces();
                ResetColours(true);
                CheckColour(true); CheckColour(false);
                if (pastGameMove == pastGameMoves.Count - 1)
                {
                    NextMoveButton.Enabled = false;
                }
            }
			else
			{
				movesNum++;
				AddMove();
				board = moves[movesNum];
				if (rev)
				{
					ReverseBoard();
					rev = true;
				}
				BackMoveButton.Enabled = true;
				SetupPieces();
				RemoveLastMove();
				ResetColours(true);
                CheckColour(true); CheckColour(false);
                if (movesNum == moves.Count - 1)
				{
					FlipBoardButton2.Enabled = true;
					enabled = true;
					bool colour = false;
					if (col == 'b')
					{
						colour = true;
					}
					CheckCanMove(colour);
					NextMoveButton.Enabled = false;
				}
			}
        }

        private void RandomiseBoardButton_Click(object sender, EventArgs e)
        {
			List<string> pieces = new List<string>();
			for (int i = 0; i < 8; i++)
            {
				pieces.Add("wpM");
				pieces.Add("bpM");
			}
			pieces.Add("wR"); pieces.Add("bR");
			pieces.Add("wR"); pieces.Add("bR");
			pieces.Add("bK"); pieces.Add("wK");
			pieces.Add("bQ"); pieces.Add("wQ");
			pieces.Add("bB"); pieces.Add("wB");
			pieces.Add("bB"); pieces.Add("wB");
			pieces.Add("bk"); pieces.Add("wk");
			pieces.Add("bk"); pieces.Add("wk");
			bool available = false;
			while (!available)
			{
				board = CreateEmptyBoard();
				for (int i = 0; i < pieces.Count; i++)
				{
                    int num3 = 0, num4 = 8;
                    if (pieces[i].Contains('p'))
                    {
                        num3 = 1;
                        num4 = 7;
                    }
                    bool isAvailable = false;
					while (!isAvailable)
					{
						int num1 = rnd.Next(num3, num4);
						int num2 = rnd.Next(0, 8);
						if (board[num1, num2] == "_")
						{
							board[num1, num2] = pieces[i];
							isAvailable = true;
						}
					}
				}
				available = true;
				bool colour = true;
				if (col == 'w')
				{
					colour = false;
				}
                if (CheckCheck(colour) || CheckCheck(!colour))
				{
					available = false;
				}
			}

			SetupPieces();
			CreateSetupText();
            BoardSetupText.Text = setupText;
        }

		private void BlueBoard_CheckedChanged(object sender, EventArgs e)
		{
			if (BlueBoard.Checked)
			{
				boardCol = new int[] { 0, 112, 163 };
				boardCol2 = new int[] { 255, 225, 217, 209 };
				ResetColours(true);
				ReplaceInFile(1, "blue");
            }
        }

		private void PurpleBoard_CheckedChanged(object sender, EventArgs e)
		{
            if (PurpleBoard.Checked)
            {
				boardCol = new int[] { 106, 13, 173 };
				boardCol2 = new int[] { 255, 225, 217, 209 };
				ResetColours(true);
				ReplaceInFile(1, "purple");
            }
        }

        private void PinkBoard_CheckedChanged(object sender, EventArgs e)
        {
			if (PinkBoard.Checked)
			{
				boardCol = new int[] { 255, 192, 203 };
				boardCol2 = new int[] { 255, 255, 255, 255 };
				ResetColours(true);
				ReplaceInFile(1, "pink");
            }
        }

        private void GreenBoard_CheckedChanged(object sender, EventArgs e)
        {
			if (GreenBoard.Checked)
			{
				boardCol = new int[] { 80, 200, 120 };
				boardCol2 = new int[] { 255, 225, 217, 209 };
				ResetColours(true);
				ReplaceInFile(1, "green");
            }
        }

        private void DarkGreenBoard_CheckedChanged(object sender, EventArgs e)
        {
			if (DarkGreenBoard.Checked)
			{
				boardCol = new int[] { 0, 100, 0 };
				boardCol2 = new int[] { 255, 225, 217, 209 };
				ResetColours(true);
				ReplaceInFile(1, "dgreen");
			}
		}

        private void DefaultButton_Click(object sender, EventArgs e)
		{
			if (rev)
            {
				SwitchTimers();
				ReverseBoard();
            }
			White00.Checked = true;
            White000.Checked = true;
            Black00.Checked = true;
            Black000.Checked = true;
            board = CreateBoard();
			SetupPieces();
			CreateSetupText();
            BoardSetupText.Text = setupText;
        }

		private void InvalidTimer_Tick(object sender, EventArgs e)
		{
			InvalidTimer.Stop();
			InvalidText.Visible = false;
		}

		private void LoginPassword_Enter(object sender, EventArgs e)
		{
            if (LoginPassword.Text == "Password:")
            {
				SeePass.Image = Properties.Resources.Eye_Close;
                LoginPassword.Text = "";
                LoginPassword.PasswordChar = '*';
            }
        }

		private void LoginPassword_Leave(object sender, EventArgs e)
		{
            if (string.IsNullOrWhiteSpace(LoginPassword.Text))
            {
                LoginPassword.Text = "Password:";
                LoginPassword.PasswordChar = '\0';
            }
        }

		private void LoginConfirmPass_Enter(object sender, EventArgs e)
		{
            if (LoginConfirmPass.Text == "Confirm password:")
            {
				SeePassCon.Image = Properties.Resources.Eye_Close;
                LoginConfirmPass.Text = "";
                LoginConfirmPass.PasswordChar = '*';
            }
        }

		private void LoginConfirmPass_Leave(object sender, EventArgs e)
		{
            if (string.IsNullOrWhiteSpace(LoginConfirmPass.Text))
            {
                LoginConfirmPass.Text = "Confirm password:";
                LoginConfirmPass.PasswordChar = '\0';
            }
        }

		private void CreateAccount_Click(object sender, EventArgs e)
		{
			ErrorMessage.Top += 25;
			SeePassCon.Visible = true;
            CreateAccount.Visible = false;
			CreateAccount2.Visible = true;
			LoginButton.Visible = false;
			CreateAccountText.Visible = false;
			LoginConfirmPass.Visible = true;
			BackToLogin.Visible = true;
        }

        private void LoginButton_Click(object sender, EventArgs e)
		{
            bool isALogin;
            string login = LoginUsername.Text.ToLower();
            string password = LoginPassword.Text;
            if (LoginUsername.Text == "Login:")
            {
                login = "";
            }
            if (LoginPassword.Text == "Password:")
            {
                password = "";
            }
			
            (isALogin, name, ID, BoardCol, chessPiece, boardFlips, soundEffects, promotionRange) = Login(login, password);
            if (isALogin)
            {
				loggedIn = true;
				if (BoardCol == "blue")
				{
					BlueBoard.Checked = true;
				}
				else if (BoardCol == "purple")
				{
					PurpleBoard.Checked = true;
				}
				else if (BoardCol == "pink")
				{
					PinkBoard.Checked = true;
				}
				else if (BoardCol == "green")
				{
					GreenBoard.Checked = true;
				}
				else if (BoardCol == "dgreen")
				{
					DarkGreenBoard.Checked = true;
				}
				if (chessPiece == 0)
				{
					PieceType0.Checked = true;
				}
				else if (chessPiece == 1)
				{
					PieceType1.Checked = true;
				}
				else if (chessPiece == 2)
				{
					PieceType2.Checked = true;
				}
				else if (chessPiece == 3)
				{
					PieceType3.Checked = false;
				}
				if (boardFlips)
				{
					FlipsTrue.Checked = true;
				}
				else
				{
					FlipsFalse.Checked = true;
				}
				if (soundEffects)
				{
					EnableSoundTrue.Checked = true;
				}
				else
				{
					EnableSoundFalse.Checked = true;
				}
				if (promotionRange)
				{
					PromotionRangeOn.Checked = true;
				}
				else
				{
					PromotionRangeOff.Checked = true;
				}
                LoginUsername.Visible = false;
                LoginButton.Visible = false;
                CreateAccountText.Visible = false;
                CreateAccount.Visible = false;
                LoginPassword.Visible = false;
                SeePass.Visible = false;
                LoggedInText.Visible = true;
                LoggedInText.Text += name;
                NotYouText.Visible = true;
                SignOutButton.Visible = true;
            }
            else
            {
                ErrorMessage.Visible = true;
                ErrorMessageTimer.Start();
                ErrorMessage.Text = "Login or Password is incorrect";
            }
        }

        private string[] ReadFile()
        {
            string[] fileText = null;
            try
            {
                fileText = File.ReadAllLines(filename);
            }
            catch (FileNotFoundException)
            {
                using (FileStream fs = File.Create(filename))
                {

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.ToString());
            }
            return fileText;
        }

		private void ReplaceInFile(int toChange, string newString)
		{
			if (loggedIn)
			{
				string s = "id: " + ID.ToString();
				string[] fileText = ReadFile();
				for (int i = 0; i < fileText.Length; i++)
				{
					if (fileText[i] == s)
					{
						fileText[i + toChange] = newString;
					}
				}
				using (StreamWriter sw = new StreamWriter(filename))
				{
					foreach (string t in fileText)
					{
						sw.WriteLine(t);
					}
					sw.Close();
				}
			}
		}

        private (bool, string, int, string, int, bool, bool, bool) Login(string login, string password)
        {
			string inName = login;
            login = "log: " + login;
            password = "pass: " + password;
            string[] fileText = ReadFile();
            if (fileText != null)
            {
                for (int i = 0; i < fileText.Length; i++)
                {
                    if (fileText[i].ToLower() == login)
                    {
						if (fileText[i + 1] == password)
						{
							string s = "";
							for (int j = 4; j < fileText[i + 2].Length; j++)
							{
								s += fileText[i + 2][j];
							}
                            return (true, inName, int.Parse(s), fileText[i + 3], int.Parse(fileText[i + 4]), bool.Parse(fileText[i + 5]), bool.Parse(fileText[i + 6]), bool.Parse(fileText[i + 7]));
                        }
                    }
                }
            }
            return (false, "", 0, "", 0, false, false, false);
        }

        private bool CheckUsernameUnique(string login)
        {
            login = "log: " + login;
            string[] fileText = ReadFile();
			if (fileText != null)
			{
				foreach (string s in fileText)
				{
					if (s == login)
					{
						return false;
					}
				}
			}
            return true;
        }

		private int ReturnID()
        {
			string s; int num = 0;
			using (StreamReader sr = new StreamReader(filename))
            {
				while (!sr.EndOfStream)
                {
					string t = sr.ReadLine();
					if (t.Contains("id: "))
                    {
						s = "";
						for (int i = 4; i < t.Length; i++)
                        {
							s += t[i];
                        }
						if (num < int.Parse(s))
                        {
							num = int.Parse(s);
                        }
                    }
                }
				sr.Close();
            }
			return num;
        }

        private int CreateLogin(string login, string password)
        {
            string[] fileText = ReadFile();
			int id = ReturnID() + 1;
			string ID = "id: " + id;
            using (StreamWriter sw = new StreamWriter(filename))
            {
                foreach (string s in fileText)
                {
                    sw.WriteLine(s);
                }
                sw.WriteLine("log: " + login);
                sw.WriteLine("pass: " + password);
				sw.WriteLine(ID);
                sw.WriteLine("blue");
                sw.WriteLine("0");
                sw.WriteLine("true");
                sw.WriteLine("true");
                sw.WriteLine("true");
                sw.WriteLine("");
				sw.Close();
            }
			return id;
        }

        private void CreateAccount2_Click(object sender, EventArgs e)
		{
            string pass1 = LoginPassword.Text, pass2 = LoginConfirmPass.Text, login = LoginUsername.Text; bool isAvailable = false;
            if (LoginUsername.Text == "Login:")
            {
				login = "";
            }
			if (LoginPassword.Text == "Password:")
			{
				pass1 = "";
			}
			if (LoginConfirmPass.Text == "Confirm password:")
			{
				pass2 = "";
			}
            if (pass1 != pass2)
            {
                ErrorMessage.Text = "Passwords don't match";
            }
            else if (login.Length < 4)
            {
                ErrorMessage.Text = "Login is too short, must be longer than 3 letters";
            }
            else if (login.Length > 15)
            {
                ErrorMessage.Text = "Login is too long, must be shorter than 16 letters";
            }
            else if (pass1.Length < 4)
            {
                ErrorMessage.Text = "Password is too short, must be longer than 3 letters";
            }
            else if (pass1.Length > 15)
            {
                ErrorMessage.Text = "Password is too long, must be shorter than 16 letters";
            }
            else if (!CheckUsernameUnique(login))
            {
				ErrorMessage.Text = "This login is already in use try another one";
            }
			else if (LoginUsername.Text == "Login:")
			{
				ErrorMessage.Text = "Login is too short, must be longer than 3 letters";
            }
			else if (LoginPassword.Text == "Password:")
			{
                ErrorMessage.Text = "Password is too short, must be longer than 3 letters";
            }
            else
            {
                isAvailable = true;
            }
            if (isAvailable)
			{
				name = login;
				ErrorMessage.Top -= 25;
				loggedIn = true;
				BlueBoard.Checked = true;
				PieceType0.Checked = true;
				FlipsTrue.Checked = true;
				PromotionRangeOn.Checked = true;
				soundEffects = true;
                ID = CreateLogin(login, pass1);
                LoginUsername.Visible = false;
                CreateAccount.Visible = false;
                LoginPassword.Visible = false;
                LoginConfirmPass.Visible = false;
                CreateAccount2.Visible = false;
				SeePass.Visible = false;
                SeePassCon.Visible = false;
				BackToLogin.Visible = false;
				LoggedInText.Visible = true;
				LoggedInText.Text += name;
				NotYouText.Visible = true;
				SignOutButton.Visible = true;
				SQLiteConnection conn = CreateConnection();
				InsertNewUser(conn, ID);
            }
            else
			{
                ErrorMessage.Visible = true;
				ErrorMessageTimer.Start();
            }
        }

		private void ErrorMessageTimer_Tick(object sender, EventArgs e)
		{
			ErrorMessage.Visible = false;
			ErrorMessageTimer.Stop();
		}

		private void LoginPassword_TextChanged(object sender, EventArgs e)
		{
			SeePass.Image = Properties.Resources.Eye_Close;
            LoginPassword.PasswordChar = '*';
        }

		private void LoginConfirmPass_TextChanged(object sender, EventArgs e)
		{
			SeePassCon.Image = Properties.Resources.Eye_Close;
            LoginConfirmPass.PasswordChar = '*';
        }

		private void SeePass_Click(object sender, EventArgs e)
		{
            if (LoginPassword.PasswordChar == '\0' && LoginPassword.Text != "Password:")
            {
				SeePass.Image = Properties.Resources.Eye_Close;
                LoginPassword.PasswordChar = '*';
            }
            else
            {
				SeePass.Image = Properties.Resources.Eye_Open;
                LoginPassword.PasswordChar = '\0';
            }
        }

		private void SeePassCon_Click(object sender, EventArgs e)
		{
            if (LoginConfirmPass.PasswordChar == '\0' && LoginConfirmPass.Text != "Confirm password:")
            {
				SeePassCon.Image = Properties.Resources.Eye_Close;
                LoginConfirmPass.PasswordChar = '*';
            }
            else
            {
				SeePassCon.Image = Properties.Resources.Eye_Open;
                LoginConfirmPass.PasswordChar = '\0';
            }
        }

		private void SignOutButton_Click(object sender, EventArgs e)
		{
			loggedIn = false;
			PieceType0.Checked = true;
			FlipsTrue.Checked = true;
			EnableSoundTrue.Checked = true;
			BlueBoard.Checked = true;
			LoggedInText.Visible = false;
			NotYouText.Visible = false;
			SignOutButton.Visible = false;
			LoggedInText.Text = "Logged in as: ";
			Settings();
		}

		private void PieceType0_CheckedChanged(object sender, EventArgs e)
		{
			if (PieceType0.Checked)
			{
				chessPiece = 0;
				SetupPieces();
				ReplaceInFile(2, "0");
			}
        }

		private void PieceType1_CheckedChanged(object sender, EventArgs e)
		{
			if (PieceType1.Checked)
			{
				chessPiece = 1;
				SetupPieces();
				ReplaceInFile(2, "1");
			}
		}

		private void PieceType2_CheckedChanged(object sender, EventArgs e)
		{
			if (PieceType2.Checked)
			{
				chessPiece = 2;
				SetupPieces();
				ReplaceInFile(2, "2");
			}
        }

		private void FlipsTrue_CheckedChanged(object sender, EventArgs e)
		{
			if (FlipsTrue.Checked)
			{
				boardFlips = true;
				ReplaceInFile(3, "true");
			}
        }

		private void FlipsFalse_CheckedChanged(object sender, EventArgs e)
		{
			if (FlipsFalse.Checked)
			{
                boardFlips = false;
                ReplaceInFile(3, "false");
            }
		}

		private void EnableSoundTrue_CheckedChanged(object sender, EventArgs e)
		{
            if (EnableSoundTrue.Checked)
            {
                soundEffects = true;
                ReplaceInFile(4, "true");
            }
        }

		private void EnableSoundFalse_CheckedChanged(object sender, EventArgs e)
		{
            if (EnableSoundFalse.Checked)
            {
                soundEffects = false;
                ReplaceInFile(4, "false");
            }
        }

		private void PieceType3_CheckedChanged(object sender, EventArgs e)
		{
            if (PieceType3.Checked)
            {
                chessPiece = 3;
                SetupPieces();
                ReplaceInFile(2, "3");
            }
        }

        private void PromotionRangeOn_CheckedChanged(object sender, EventArgs e)
        {
            if (PromotionRangeOn.Checked)
            {
				promotionRange = true;
                ReplaceInFile(5, "true");
            }
        }

        private void PromotionRangeOff_CheckedChanged(object sender, EventArgs e)
        {
            if (PromotionRangeOff.Checked)
            {
                ReplaceInFile(5, "false");
				promotionRange = false;
            }
        }

        private void BackToLogin_Click(object sender, EventArgs e)
		{
            ErrorMessage.Top -= 25;
            SeePassCon.Visible = false;
            CreateAccount.Visible = true;
            CreateAccount2.Visible = false;
            LoginButton.Visible = true;
            CreateAccountText.Visible = true;
            LoginConfirmPass.Visible = false;
			BackToLogin.Visible = false;
        }

		private void LoginUsername_Enter(object sender, EventArgs e)
		{
            if (LoginUsername.Text == "Login:")
            {
                LoginUsername.Text = "";
            }
        }

		private void LoginUsername_Leave(object sender, EventArgs e)
		{
            if (string.IsNullOrWhiteSpace(LoginUsername.Text))
            {
                LoginUsername.Text = "Login:";
            }
        }

		private void WhiteBishopButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "wB";
		}

		private void WhiteKnightButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "wk";
		}

		private void WhitePawnButton_Click(object sender, EventArgs e)
		{
			puttingPieces = true;
			pieceToPut = "wp";
		}

        static void DeleteData(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "DROP TABLE IF EXISTS games";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "DROP TABLE IF EXISTS users";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "DROP TABLE IF EXISTS gameInfo";
            sqlite_cmd.ExecuteNonQuery();
        }

		private void ResignButton_Click(object sender, EventArgs e)
		{
			if (enabled)
			{
				enabled = false; timer.Stop(); timerB.Stop();
				CheckMateText.Visible = true;
				CheckMateText.Text = Environment.NewLine + Environment.NewLine;
				XButton.Visible = true;
				if (ResignButton.Text == "Resign")
				{
					int endGameStatus = 0;
					if (col == 'b')
					{
						CheckMateText.Text += "White wins";
					}
					else
					{
						CheckMateText.Text += "Black wins";
						endGameStatus = 1;
					}
					CheckMateText.Text += Environment.NewLine + "By Resignation!";
					if (loggedIn)
					{
						SQLiteConnection conn = CreateConnection();
						int gameID = InsertNewGame(conn, ID, endGameStatus);
						foreach (string game in replayMoves)
						{
							InsertNewGameMove(conn, gameID, game);
						}
					}
				}
				else if (ResignButton.Text == "Abort")
				{
					CheckMateText.Text += "Game aborted";
				}
				replayMoves.Clear();
			}
        }

		private void Chess960Button_Click(object sender, EventArgs e)
		{
            FlipBoardButton2.Visible = true;
            NextMoveButton.Visible = BackMoveButton.Visible = ResignButton.Visible = true;
            AutomaticFlipsCheckBox.Visible = true;
            Chess960Button.Visible = HordeButton.Visible = false;
            int k = 2, R = 2, K = 1, B = 2, Q = 1;
            List<string> strings = new List<string>();
            while (k > 0 || R > 0 || K > 0 || B > 0 || Q > 0)
            {
                int Randnum = rnd.Next(0, 5);
                if (Randnum == 0 && k > 0)
                {
                    k--; strings.Add("k");
                }
                else if (Randnum == 1 && R == 1 && K == 0 || Randnum == 1 && R == 2)
                {
                    R--; strings.Add("R");
                }
                else if (Randnum == 2 && K == 1 && R == 1)
                {
                    K--; strings.Add("KM");
                }
                else if (Randnum == 3 && B > 0)
                {
                    B--; strings.Add("B");
                }
                else if (Randnum == 4 && Q > 0)
                {
                    Q--; strings.Add("Q");
                }
            }
            board = new string[8, 8]
            {
                { "b" + strings[0], "b" + strings[1], "b" + strings[2], "b" + strings[3], "b" + strings[4], "b" + strings[5], "b" + strings[6], "b" + strings[7]},
                { "bp", "bp", "bp", "bp", "bp", "bp", "bp", "bp"},
                { "_", "_", "_", "_", "_", "_", "_", "_"},
                { "_", "_", "_", "_", "_", "_", "_", "_"},
                { "_", "_", "_", "_", "_", "_", "_", "_"},
                { "_", "_", "_", "_", "_", "_", "_", "_"},
                { "wp", "wp", "wp", "wp", "wp", "wp", "wp", "wp"},
                { "w" + strings[0], "w" + strings[1], "w" + strings[2], "w" + strings[3], "w" + strings[4], "w" + strings[5], "w" + strings[6], "w" + strings[7]}
            };
			SetupPieces();
			CreateSetupText();
        }

		private void HordeButton_Click(object sender, EventArgs e)
		{
            FlipBoardButton2.Visible = true;
            NextMoveButton.Visible = BackMoveButton.Visible = ResignButton.Visible = true;
            AutomaticFlipsCheckBox.Visible = true;
            Chess960Button.Visible = HordeButton.Visible = false;
            board = new string[8, 8]
            {
				{ "bR", "bk", "bB", "bQ", "bK", "bB", "bk", "bR"},
                { "bp", "bp", "bp", "bp", "bp", "bp", "bp", "bp"},
				{ "_", "_", "_", "_", "_", "_", "_", "_"},
				{ "_", "wp", "wp", "_", "_", "wp", "wp", "_"},
				{ "wpM", "wpM", "wpM", "wpM", "wpM", "wpM", "wpM", "wpM"},
				{ "wpM", "wpM", "wpM", "wpM", "wpM", "wpM", "wpM", "wpM"},
				{ "wp", "wp", "wp", "wp", "wp", "wp", "wp", "wp"},
				{ "wp", "wp", "wp", "wp", "wp", "wp", "wp", "wp"},
            };
			SetupPieces();
        }

		private void HordeCustom_Click(object sender, EventArgs e)
		{
			HordeButton_Click(sender, e);
            CreateSetupText();
            BoardSetupText.Text = setupText;
			IgnoreInvalidPostition.Checked = true;
        }

		private void IgnoreInvalidPostition_CheckedChanged(object sender, EventArgs e)
		{
			ignoreInvalid = IgnoreInvalidPostition.Checked;
		}

        private void AIButton_Click(object sender, EventArgs e)
        {
			timerTick = false; ActiveTimer.Text = "99:00"; InactiveTimer.Text = "99:00";
			ActiveTimer.Visible = false; InactiveTimer.Visible = false;
            ResignButton.Visible = true;
            BackMoveButton.Visible = true; NextMoveButton.Visible = true;
            MovesCounter.Visible = true;
			AutomaticFlipsCheckBox.Checked = false;
            AIDepthPanel.Visible = false;
			enabled = true;
        }

        private void AIDepth0_CheckedChanged(object sender, EventArgs e)
        {
			depth = 0;
        }

        private void AIDepth1_CheckedChanged(object sender, EventArgs e)
        {
			depth = 1;
        }

        private void AIDepth2_CheckedChanged(object sender, EventArgs e)
        {
			depth = 2;
        }

        private void AIDepth3_CheckedChanged(object sender, EventArgs e)
        {
			depth = 3;
        }

        static void InsertNewUser(SQLiteConnection conn, int ID)
        {
			try
			{
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = "INSERT INTO users (ID, GameNum) VALUES (" + ID + ", 0);";
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception)
			{
				CreateTables(conn);
				InsertNewUser(conn, ID);
			}
        }

		static int InsertNewGame(SQLiteConnection conn, int ID, int endGameStatus)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT GameID FROM games";
            int biggestID = 0;
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                if (sqlite_datareader.GetInt32(0) > biggestID)
                {
                    biggestID = sqlite_datareader.GetInt32(0);
                }
            }
            biggestID++;
            sqlite_datareader.Close();

            IncrementGameNum(conn, ID);

            sqlite_cmd.CommandText = "INSERT INTO games (GameID, ID, EndGameStatus, NumOfMoves, TimeOfGame) VALUES (" + biggestID + ", " + ID + ", " + endGameStatus + ", 0, " + DateTime.Now.ToString("yyyyMMddHHmmss") + ");";
            sqlite_cmd.ExecuteNonQuery();
			return biggestID;
        }

        static void IncrementGameNum(SQLiteConnection conn, int ID)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT GameNum FROM users WHERE ID = " + ID;
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            int gameNum = 0;
            while (sqlite_datareader.Read())
            {
                gameNum = sqlite_datareader.GetInt32(0);
            }
            gameNum++;
            sqlite_datareader.Close();

            sqlite_cmd.CommandText = "UPDATE users SET GameNum = " + gameNum + " WHERE ID = " + ID + ";";
            sqlite_cmd.ExecuteNonQuery();
        }

        static void InsertNewGameMove(SQLiteConnection conn, int GameID, string boardText)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT SpecGameID FROM gameInfo";
            int biggestID = 0;
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                if (sqlite_datareader.GetInt32(0) > biggestID)
                {
                    biggestID = sqlite_datareader.GetInt32(0);
                }
            }
            biggestID++;
            sqlite_datareader.Close();

            sqlite_cmd.CommandText = "SELECT SpecGameID FROM gameInfo WHERE GameID = " + GameID;
            int biggestMove = 0;
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                if (sqlite_datareader.GetInt32(0) > biggestMove)
                {
                    biggestMove = sqlite_datareader.GetInt32(0);
                }
            }
            biggestMove++;
            sqlite_datareader.Close();

            IncrementNumOfMoves(conn, GameID);

            sqlite_cmd.CommandText = "INSERT INTO gameInfo (SpecGameID, GameID, MoveNum, Board) VALUES (" + biggestID + ", " + GameID + ", " + biggestMove + ", '" + boardText + "');";
            sqlite_cmd.ExecuteNonQuery();
        }

        static void IncrementNumOfMoves(SQLiteConnection conn, int GameID)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT NumOfMoves FROM games WHERE GameID = " + GameID;
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            int numOfMoves = 0;
            while (sqlite_datareader.Read())
            {
                numOfMoves = sqlite_datareader.GetInt32(0);
            }
            numOfMoves++;
            sqlite_datareader.Close();

            sqlite_cmd.CommandText = "UPDATE games SET NumOfMoves = " + numOfMoves + " WHERE GameID = " + GameID + ";";
            sqlite_cmd.ExecuteNonQuery();
        }

        static SQLiteConnection CreateConnection()
        {

            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=chessmatchdata.db;Version=3;New=True;Compress=True;");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception)
            {

            }
            return sqlite_conn;
        }

        static void CreateTables(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd;
            try
            {
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = "CREATE TABLE users (ID INT(11) NOT NULL, GameNum INT(2) DEFAULT NULL, PRIMARY KEY (ID));";
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {

            }
            try
            {
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = "CREATE TABLE games (GameID INT(11) NOT NULL, ID INT(11) DEFAULT NULL, EndGameStatus INT(2), NumOfMoves INT(8), TimeOfGame DATETIME, PRIMARY KEY (GameID));";
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {

            }
            try
            {
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = "CREATE TABLE gameInfo (SpecGameID INT(11) NOT NULL, GameID INT(11) DEFAULT NULL, MoveNum INT(8), Board VARCHAR(32), PRIMARY KEY (SpecGameID));";
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {

            }
        }
    }


	// I got this code below from the internet to allow me to create transparent text which was used on the board to see the position of pieces

	public class TransparentLabel : Control
	{
		/// <summary>
		/// Creates a new <see cref="TransparentLabel"/> instance.
		/// </summary>
		public TransparentLabel()
		{
			TabStop = false;
		}

		/// <summary>
		/// Gets the creation parameters.
		/// </summary>
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x20;
				return cp;
			}
		}

		/// <summary>
		/// Paints the background.
		/// </summary>
		/// <param name="e">E.</param>
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			// do nothing
		}

		/// <summary>
		/// Paints the control.
		/// </summary>
		/// <param name="e">E.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			DrawText();
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if (m.Msg == 0x000F)
			{
				DrawText();
			}
		}

		private void DrawText()
		{
			using (Graphics graphics = CreateGraphics())
			using (SolidBrush brush = new SolidBrush(ForeColor))
			{
				SizeF size = graphics.MeasureString(Text, Font);

				// first figure out the top
				float top = 0;
				switch (textAlign)
				{
					case ContentAlignment.MiddleLeft:
					case ContentAlignment.MiddleCenter:
					case ContentAlignment.MiddleRight:
						top = (Height - size.Height) / 2;
						break;
					case ContentAlignment.BottomLeft:
					case ContentAlignment.BottomCenter:
					case ContentAlignment.BottomRight:
						top = Height - size.Height;
						break;
				}

				float left = -1;
				switch (textAlign)
				{
					case ContentAlignment.TopLeft:
					case ContentAlignment.MiddleLeft:
					case ContentAlignment.BottomLeft:
						if (RightToLeft == RightToLeft.Yes)
							left = Width - size.Width;
						else
							left = -1;
						break;
					case ContentAlignment.TopCenter:
					case ContentAlignment.MiddleCenter:
					case ContentAlignment.BottomCenter:
						left = (Width - size.Width) / 2;
						break;
					case ContentAlignment.TopRight:
					case ContentAlignment.MiddleRight:
					case ContentAlignment.BottomRight:
						if (RightToLeft == RightToLeft.Yes)
							left = -1;
						else
							left = Width - size.Width;
						break;
				}
				graphics.DrawString(Text, Font, brush, left, top);
			}
		}

		/// <summary>
		/// Gets or sets the text associated with this control.
		/// </summary>
		/// <returns>
		/// The text associated with this control.
		/// </returns>
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
				RecreateHandle();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether control's elements are aligned to support locales using right-to-left fonts.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// One of the <see cref="T:System.Windows.Forms.RightToLeft"/> values. The default is <see cref="F:System.Windows.Forms.RightToLeft.Inherit"/>.
		/// </returns>
		/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
		/// The assigned value is not one of the <see cref="T:System.Windows.Forms.RightToLeft"/> values.
		/// </exception>
		public override RightToLeft RightToLeft
		{
			get
			{
				return base.RightToLeft;
			}
			set
			{
				base.RightToLeft = value;
				RecreateHandle();
			}
		}

		/// <summary>
		/// Gets or sets the font of the text displayed by the control.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The <see cref="T:System.Drawing.Font"/> to apply to the text displayed by the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultFont"/> property.
		/// </returns>
		public override Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
				RecreateHandle();
			}
		}

		private ContentAlignment textAlign = ContentAlignment.TopLeft;
		/// <summary>
		/// Gets or sets the text alignment.
		/// </summary>
		public ContentAlignment TextAlign
		{
			get { return textAlign; }
			set
			{
				textAlign = value;
				RecreateHandle();
			}
		}
	}
}
