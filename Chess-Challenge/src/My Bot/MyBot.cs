using ChessChallenge.API;
using System.Collections.Generic;
using static ChessChallenge.Application.ConsoleHelper;

public class MyBot : IChessBot
{

    public Move Think(Board board, Timer timer)
    {
        int depth = 4;
        (int eval, Move bestMove) = AlphaBetaEvaluation(board, depth, -10_000_000, 10_000_000, true);
        // Log($"eval: {eval}");
        return bestMove;
    }

    private (int eval, Move bestMove) AlphaBetaEvaluation(Board board, int depth, int alpha, int beta, bool canExtend) {
        

        // Base case: perform heuristic evaluation
        if (depth == 0 || board.IsDraw() || board.IsInCheckmate()) {
            return (Eval(board), Move.NullMove);
        }

        int bestEval = board.IsWhiteToMove ? -10_000_000 : 10_000_000;
        Move bestMove = Move.NullMove;
        
        Move[] moves = GetSortedMoves(board);
        
        // TODO: killer move heuristic
        // TODO: dynamic depth increase
        foreach (Move candidateMove in moves) {

            // Get evaluation of candidate move
            board.MakeMove(candidateMove);
            (int candidateEval, _) = AlphaBetaEvaluation(board, depth - 1, alpha, beta, false);
            board.UndoMove(candidateMove);

            // Update best evaluation
            if (board.IsWhiteToMove ^ candidateEval < bestEval) {
                bestEval = candidateEval;
                bestMove = candidateMove;
            }

            // Check for pruning
            if ((board.IsWhiteToMove && bestEval > beta) || (!board.IsWhiteToMove && bestEval < alpha)) {
                break;
            }

            // Update pruning variables
            if (board.IsWhiteToMove && bestEval > alpha) {
                alpha = bestEval;
            } else if (!board.IsWhiteToMove && bestEval < beta) {
                beta = bestEval;
            }
        }

        // For checkmates, we represent mate in N ply as 1,000,000 - N (and -1,000,000 + N for black)
        if (bestEval > 900_000) {
            bestEval--;
        } else if (bestEval < -900_000) {
            bestEval++;
        }
        return (bestEval, bestMove);

    }

    // Heuristic evaluation function
    private int Eval(Board board)
    {  
        // Check for end of game
        int checkmateEval = 1_000_000;
        if (board.IsInCheckmate()) {
            if (!board.IsWhiteToMove) {
                return checkmateEval;
            } else {
                return -1 * checkmateEval;
            }
        } else if (board.IsDraw()) {
            return 0;
        }

        int evaluation = 0;
        PieceList[] lists = board.GetAllPieceLists();
        foreach (PieceList list in lists) {
            // Material and positional evaluation
            for (int i = 0; i < list.Count; i++) {
                Piece piece = list.GetPiece(i);
                int file = piece.Square.File < 4 ? piece.Square.File : 7 - piece.Square.File;
                int rank = piece.IsWhite ? piece.Square.Rank : 7 - piece.Square.Rank;
                int pieceEval = 0;
                switch ((int)piece.PieceType) {
                    // TODO Pawns
                    case 1:
                        int[] PFileEval = {9, 9, 10, 13};
                        int[] PRankEval = {-10, -9, -13, -5, 1, 7, 40, -10};
                        pieceEval += 100 + PFileEval[file] + PRankEval[rank];
                        break;

                    // Knights
                    case 2:
                        int[] NFileEval = {-38, -14, -1, 2};
                        int[] NRankEval = {-25, -2, 11, 15, 14, 13, -1, -25};
                        pieceEval += 320 + NFileEval[file] + NRankEval[rank];
                        break;

                    // Bishops
                    case 3:
                        int[] BFileEval = {-13, -0, 1, 3};
                        int[] BRankEval = {-10, 1, 7, 5, 5, 3, 0, -10};
                        pieceEval += 330 + BFileEval[file] + BRankEval[rank];
                        break;

                    // Rooks
                    case 4:
                        // // Material Evaluation
                        // pieceEval += 500;
                        // // Bonus for 7th rank
                        // if (rank == 6) pieceEval += 10;
                        // // Penalty for edge file
                        // if (file == 1 && (rank != 0 || rank != 7)) pieceEval += -5;
                        // // Bonus for centralized on 1st rank
                        // if (rank == 0 && file == 3) pieceEval += 5;
                        int[] RFileEval = {-3, 1, 1, 1};
                        int[] RRankEval = {0, -2, -2, -2, -2, -2, 8, 0};
                        pieceEval += 500 + RFileEval[file] + RRankEval[rank];
                        break;

                    // Queens
                    case 5:
                        int[] QFileEval = {-11, -2, 1, 1};
                        int[] QRankEval = {-9, 1, 4, 5, 4, 3, 0, -9};
                        pieceEval += 900 + QFileEval[file] + QRankEval[rank];
                        break;

                    // TODO Kings
                    case 6:
                        break;
                }
                evaluation += pieceEval * (piece.IsWhite ? 1 : -1);
            }
        }

        return evaluation;
    }

    // // Yoinked from https://stackoverflow.com/a/12171691/21553030
    // private int countSetBits(ulong value) {
    //     int count = 0;
    //     while (value != 0) {
    //         count++;
    //         value &= value - 1;
    //     }
    //     return count;
    // }

    private Move[] GetSortedMoves(Board board) {
        // Sort by checks, then non-check captures
        // Use insertion sort because we're only ever going to deal
        // with relatively small n, and its code footprint is small
        int i = 0;
        Move[] legalMoves = board.GetLegalMoves();
        Move[] orderedMoves = new Move[legalMoves.Length];
        foreach(Move move in legalMoves) {
            if (MoveIsCheck(board, move)) {
                orderedMoves[i] = move;
                i++;
            }
        }
        foreach(Move move in legalMoves) {
            if (!MoveIsCheck(board, move) && move.IsCapture) {
                orderedMoves[i] = move;
                i++;
            }
        }
        foreach(Move move in legalMoves) {
            if (!MoveIsCheck(board, move) && !move.IsCapture) {
                orderedMoves[i] = move;
                i++;
            }
        }
        return orderedMoves;
    }

    private bool MoveIsCheck(Board board, Move move) {
        board.MakeMove(move);
        bool isCheck = board.IsInCheck();
        board.UndoMove(move);
        return isCheck;
    }
}